using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EPOOutline;
using Gloomhaven;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;

public class WorldspaceUITools : MonoBehaviour
{
	private class OutlinableData
	{
		[Flags]
		public enum State
		{
			Disabled = 0,
			Enabled = 1,
			Hovered = 2,
			AbilityFocus = 4,
			Unrevealed = 8
		}

		public CActor Actor;

		public readonly bool IsActor;

		public readonly int DefaultLayer;

		public bool CanUseMaskedMode;

		public State CurrentState;

		public OutlinableData(Outlinable outlinable, CActor actor, bool canUseMaskedMode)
		{
			IsActor = actor != null;
			CanUseMaskedMode = canUseMaskedMode;
			Actor = actor;
			DefaultLayer = outlinable.OutlineLayer;
			CurrentState = (OutlinableIsEnabled(outlinable) ? State.Enabled : State.Disabled);
		}
	}

	private enum WUIStatus
	{
		Hidden,
		Showing,
		Visible,
		Hiding
	}

	[ConsoleVariable(Alias = "fade_health_bars")]
	public static bool ConsoleVar_FadeHealthBars = true;

	public static WorldspaceUITools Instance;

	private Dictionary<Outlinable, OutlinableData> _outlinables = new Dictionary<Outlinable, OutlinableData>();

	private Dictionary<CActor, HashSet<Outlinable>> _actorOutlinables = new Dictionary<CActor, HashSet<Outlinable>>();

	private bool m_OutlinesEnabled;

	public GameObject WorldspaceGUIPrefabLevel;

	public GameObject WorldspaceTileBehaviourUIPrefab;

	[SerializeField]
	private Canvas worldspaceCanvas;

	[SerializeField]
	private CanvasGroup worldspaceCanvasGroup;

	private Camera worldspaceCamera;

	private List<WorldspacePanelUIController> _activePanels = new List<WorldspacePanelUIController>();

	private Vector3 _lastCameraPosition = Vector3.negativeInfinity;

	private WUIStatus _currentUIStatus;

	private Tween _currentCanvasGroupAnim;

	private float _prevCameraFOV;

	private float _prevSqrdPositionDelta;

	private float _prevFOVDelta;

	private bool _enableWUIFading;

	public const int MASKED_MODE_LAYER_INDEX = 1;

	public const float MASKED_MODE_OUTLINE_OPACITY = 0.65f;

	private const float HIDE_TRANSITION_DURATION = 0.08f;

	private const float MOVEMENT_SENSITIVITY_SHOW = 0.5f;

	private const float MOVEMENT_SENSITIVITY_HIDE = 10f;

	private const float ZOOM_SENSITIVITY_SHOW = 1f;

	private const float ZOOM_SENSITIVITY_HIDE = 10f;

	private const float MOVE_THRESHOLD = 0.05f;

	private const float ZOOM_THRESHOLD = 1f;

	public Camera WorldspaceCamera => worldspaceCamera;

	public Canvas WorldspaceCanvas => worldspaceCanvas;

	public bool OutlinesEnabled => m_OutlinesEnabled;

	private void Awake()
	{
		Instance = this;
		m_OutlinesEnabled = false;
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Remove(GraphicsController.OnQualityLevelChanged, new Action(CheckFadingIsEnabled));
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Combine(GraphicsController.OnQualityLevelChanged, new Action(CheckFadingIsEnabled));
		CheckFadingIsEnabled();
	}

	private void CheckFadingIsEnabled()
	{
		_enableWUIFading = QualitySettings.names[QualitySettings.GetQualityLevel()] != "Fantastic";
	}

	private void OnDestroy()
	{
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Remove(GraphicsController.OnQualityLevelChanged, new Action(CheckFadingIsEnabled));
		Instance = null;
	}

	public void Init(Camera camera)
	{
		worldspaceCamera = camera;
	}

	public void AddOutlinableToList(Outlinable outlinable)
	{
		if (!_outlinables.ContainsKey(outlinable))
		{
			OutlinableData outlinableData = new OutlinableData(outlinable, null, canUseMaskedMode: false);
			_outlinables.Add(outlinable, outlinableData);
			ApplyStateToOutlinable(outlinable, outlinableData);
		}
	}

	public void AddOutlinableToList(Outlinable outlinable, CActor actor, bool canUseMaskedMode)
	{
		if (_outlinables.TryGetValue(outlinable, out var value))
		{
			OutlinableData outlinableData = value;
			if (outlinableData.Actor == null)
			{
				outlinableData.Actor = actor;
			}
			return;
		}
		bool flag = actor != null;
		value = new OutlinableData(outlinable, actor, canUseMaskedMode);
		_outlinables.Add(outlinable, value);
		if (flag)
		{
			if (_actorOutlinables.TryGetValue(actor, out var value2))
			{
				value2.Add(outlinable);
			}
			else
			{
				_actorOutlinables.Add(actor, new HashSet<Outlinable> { outlinable });
			}
		}
		ApplyStateToOutlinable(outlinable, value);
	}

	public void AddOutlinableToList(Outlinable outlinable, CActor actor)
	{
		AddOutlinableToList(outlinable, actor, actor != null);
	}

	public void RemoveOutlinableFromList(Outlinable outlinable)
	{
		if (!_outlinables.TryGetValue(outlinable, out var value))
		{
			return;
		}
		if (value.IsActor)
		{
			_actorOutlinables[value.Actor].Remove(outlinable);
			if (_actorOutlinables[value.Actor].Count == 0)
			{
				_actorOutlinables.Remove(value.Actor);
			}
		}
		value.CurrentState = OutlinableData.State.Disabled;
		ApplyStateToOutlinable(outlinable, value);
		_outlinables.Remove(outlinable);
	}

	public void RemoveOutlinableFromList(CActor actor)
	{
		if (!_actorOutlinables.TryGetValue(actor, out var value))
		{
			return;
		}
		foreach (Outlinable item in value)
		{
			if (_outlinables.TryGetValue(item, out var value2))
			{
				value2.CurrentState = OutlinableData.State.Disabled;
				ApplyStateToOutlinable(item, value2);
				_outlinables.Remove(item);
			}
		}
		_actorOutlinables[actor].Clear();
		_actorOutlinables.Remove(actor);
	}

	private void Update()
	{
		UpdateVisibilityStatus();
		if (_currentUIStatus != WUIStatus.Hidden)
		{
			OrderWorldspaceUIPanels();
		}
	}

	private void UpdateVisibilityStatus()
	{
		if (!_enableWUIFading || !ConsoleVar_FadeHealthBars || worldspaceCamera == null)
		{
			return;
		}
		bool flag = InputFHManager.Controls.Camera.DragMove.IsPressed();
		Vector3 position = worldspaceCamera.transform.position;
		float deltaTime = Time.deltaTime;
		if (deltaTime == 0f)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(_lastCameraPosition - position) / deltaTime;
		bool flag2 = num >= 0.0025000002f && num > _prevSqrdPositionDelta * 10f;
		bool flag3 = num < 0.0025000002f && num * 0.5f <= _prevSqrdPositionDelta;
		float num2 = Math.Abs(worldspaceCamera.fieldOfView - _prevCameraFOV) / deltaTime;
		bool flag4 = num2 >= 1f && num2 > _prevFOVDelta * 10f;
		bool flag5 = num2 < 1f && num2 * 1f <= _prevFOVDelta;
		WUIStatus currentUIStatus = _currentUIStatus;
		if (currentUIStatus == WUIStatus.Visible || currentUIStatus == WUIStatus.Showing)
		{
			if (flag4 | flag2)
			{
				_currentCanvasGroupAnim?.Kill();
				_currentCanvasGroupAnim = worldspaceCanvasGroup.DOFade(0f, 0.08f).SetEase(Ease.OutCubic).OnComplete(() =>
				{
					_currentUIStatus = WUIStatus.Hidden;
					SetPanelsActive(active: false);
				});
				_currentUIStatus = WUIStatus.Hiding;
			}
		}
		else
		{
			currentUIStatus = _currentUIStatus;
			if ((currentUIStatus == WUIStatus.Hidden || currentUIStatus == WUIStatus.Hiding) && (flag5 & flag3) && !flag)
			{
				SetPanelsActive(active: true);
				_currentCanvasGroupAnim?.Kill();
				_currentCanvasGroupAnim = worldspaceCanvasGroup.DOFade(1f, 0.08f).SetEase(Ease.InCubic).OnComplete(() =>
				{
					_currentUIStatus = WUIStatus.Visible;
				});
				_currentUIStatus = WUIStatus.Showing;
			}
		}
		_lastCameraPosition = position;
		_prevCameraFOV = worldspaceCamera.fieldOfView;
		_prevFOVDelta = num2;
		_prevSqrdPositionDelta = num;
	}

	private void OrderWorldspaceUIPanels()
	{
		if (worldspaceCamera == null)
		{
			return;
		}
		foreach (WorldspacePanelUIController item in _activePanels.OrderByDescending((WorldspacePanelUIController x) => Vector3.SqrMagnitude(x.TargetPosition - worldspaceCamera.transform.position)))
		{
			item.transform.SetAsLastSibling();
		}
	}

	private void SetPanelsActive(bool active)
	{
		worldspaceCanvas.enabled = active;
	}

	public void RegisterPanel(WorldspacePanelUIController panel)
	{
		if (!_activePanels.Contains(panel))
		{
			_activePanels.Add(panel);
		}
	}

	public void UnregisterPanel(WorldspacePanelUIController panel)
	{
		if (_activePanels.Contains(panel))
		{
			_activePanels.Remove(panel);
		}
	}

	public void ActivateAllOutlines(bool active)
	{
		m_OutlinesEnabled = active;
		foreach (KeyValuePair<Outlinable, OutlinableData> outlinable in _outlinables)
		{
			ApplyStateToOutlinable(outlinable.Key, outlinable.Value);
		}
	}

	public void EnableHoveredOutline(Outlinable hoveredOutlinable)
	{
		if (!SaveData.Instance.Global.DisableHoverOutlines && _outlinables.TryGetValue(hoveredOutlinable, out var value))
		{
			value.CurrentState |= OutlinableData.State.Hovered;
			ApplyStateToOutlinable(hoveredOutlinable, _outlinables[hoveredOutlinable]);
		}
	}

	public void DisableHoveredOutline(Outlinable hoveredOutlinable)
	{
		if (!SaveData.Instance.Global.DisableHoverOutlines && _outlinables.TryGetValue(hoveredOutlinable, out var value))
		{
			value.CurrentState &= ~OutlinableData.State.Hovered;
			ApplyStateToOutlinable(hoveredOutlinable, _outlinables[hoveredOutlinable]);
		}
	}

	public void EnableHoveredOutline(CActor actor)
	{
		if (SaveData.Instance.Global.DisableHoverOutlines || !_actorOutlinables.TryGetValue(actor, out var value))
		{
			return;
		}
		foreach (Outlinable item in value)
		{
			if (!_outlinables.TryGetValue(item, out var value2))
			{
				_actorOutlinables[actor].Remove(item);
				continue;
			}
			value2.CurrentState |= OutlinableData.State.Hovered;
			ApplyStateToOutlinable(item, value2);
		}
	}

	public void DisableHoveredOutline(CActor actor)
	{
		if (SaveData.Instance.Global.DisableHoverOutlines || !_actorOutlinables.TryGetValue(actor, out var value))
		{
			return;
		}
		foreach (Outlinable item in value)
		{
			if (!_outlinables.TryGetValue(item, out var value2))
			{
				_actorOutlinables[actor].Remove(item);
				continue;
			}
			value2.CurrentState &= ~OutlinableData.State.Hovered;
			ApplyStateToOutlinable(item, value2);
		}
	}

	public void DisableAllHoveredOutlines(bool ignorePointer = false)
	{
		if (SaveData.Instance.Global.DisableHoverOutlines || (!ignorePointer && UIManager.IsPointerOverUI))
		{
			return;
		}
		foreach (KeyValuePair<Outlinable, OutlinableData> outlinable in _outlinables)
		{
			if (!(outlinable.Key == null))
			{
				outlinable.Value.CurrentState &= ~OutlinableData.State.Hovered;
				ApplyStateToOutlinable(outlinable.Key, outlinable.Value);
			}
		}
	}

	public void EnableAbilityFocusOutline(Outlinable abilityFocusOutlinable)
	{
		if (!SaveData.Instance.Global.DisableAbilityFocusOutlines && _outlinables.TryGetValue(abilityFocusOutlinable, out var value))
		{
			value.CurrentState |= OutlinableData.State.AbilityFocus;
			ApplyStateToOutlinable(abilityFocusOutlinable, value);
		}
	}

	public void DisableAbilityFocusOutline(Outlinable abilityFocusOutlinable)
	{
		if (!SaveData.Instance.Global.DisableAbilityFocusOutlines && _outlinables.TryGetValue(abilityFocusOutlinable, out var value))
		{
			value.CurrentState &= ~OutlinableData.State.AbilityFocus;
			ApplyStateToOutlinable(abilityFocusOutlinable, value);
		}
	}

	public void SetOutlinableMaskedMode(Outlinable outlinable, bool maskedModeEnabled)
	{
		if (_outlinables.TryGetValue(outlinable, out var value))
		{
			value.CanUseMaskedMode = maskedModeEnabled;
			if (maskedModeEnabled)
			{
				SetOutlineToMaskedMode(outlinable);
			}
			else
			{
				SetOutlineToNormalMode(outlinable, value, m_OutlinesEnabled);
			}
		}
	}

	private void ApplyStateToOutlinable(Outlinable outlinable, OutlinableData data)
	{
		ApplyGlobalOutlineState(outlinable, data);
		bool flag = data.CurrentState != OutlinableData.State.Disabled && !data.CurrentState.HasFlag(OutlinableData.State.Unrevealed);
		if (data.CanUseMaskedMode)
		{
			if (flag)
			{
				SetOutlineToNormalMode(outlinable, data, outlineEnabled: true);
			}
			else
			{
				SetOutlineToMaskedMode(outlinable);
			}
		}
		else
		{
			SetOutlineToNormalMode(outlinable, data, flag);
		}
	}

	private void ApplyGlobalOutlineState(Outlinable outlinable, OutlinableData data)
	{
		if (!data.CurrentState.HasFlag(OutlinableData.State.Enabled) && m_OutlinesEnabled)
		{
			data.CurrentState |= OutlinableData.State.Enabled;
		}
		else if (data.CurrentState.HasFlag(OutlinableData.State.Enabled) && !m_OutlinesEnabled)
		{
			data.CurrentState &= ~OutlinableData.State.Enabled;
		}
	}

	public void SetRevealState(Outlinable outlinable, bool isRevealed)
	{
		if (!outlinable || !_outlinables.TryGetValue(outlinable, out var value))
		{
			Debug.LogWarning("Tried to set reveal state of prop that isn't registered.");
		}
		else if (!isRevealed && !value.CurrentState.HasFlag(OutlinableData.State.Unrevealed))
		{
			value.CurrentState |= OutlinableData.State.Unrevealed;
		}
		else if (isRevealed && value.CurrentState.HasFlag(OutlinableData.State.Unrevealed))
		{
			value.CurrentState &= ~OutlinableData.State.Unrevealed;
		}
	}

	private static void SetOutlineToMaskedMode(Outlinable outlinable)
	{
		outlinable.enabled = true;
		outlinable.RenderStyle = RenderStyle.FrontBack;
		outlinable.ComplexMaskingMode = ComplexMaskingMode.ObstaclesMode;
		outlinable.OutlineLayer = 1;
		outlinable.FrontParameters.Enabled = false;
		outlinable.BackParameters.Enabled = true;
		Color color = outlinable.OutlineParameters.Color;
		color.r = Mathf.Min(1f, color.r);
		color.g = Mathf.Min(1f, color.g);
		color.b = Mathf.Min(1f, color.b);
		color.a = 0.65f;
		outlinable.BackParameters.Color = color;
	}

	private static void SetOutlineToNormalMode(Outlinable outlinable, OutlinableData data, bool outlineEnabled)
	{
		outlinable.enabled = outlineEnabled;
		outlinable.RenderStyle = RenderStyle.Single;
		outlinable.ComplexMaskingMode = ComplexMaskingMode.None;
		outlinable.OutlineParameters.Enabled = outlineEnabled;
		if (data != null)
		{
			outlinable.OutlineLayer = data.DefaultLayer;
		}
	}

	private static bool OutlinableIsEnabled(Outlinable outlinable)
	{
		if (outlinable.RenderStyle == RenderStyle.Single)
		{
			return outlinable.OutlineParameters.Enabled;
		}
		if (!outlinable.FrontParameters.Enabled)
		{
			return outlinable.BackParameters.Enabled;
		}
		return true;
	}
}
