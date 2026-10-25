using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

[ExecuteAlways]
public class FHTabButtonChildElement : MonoBehaviour
{
	[SerializeField]
	public FHTabButton _parent;

	private IFHSelectable _selectableParent;

	[SerializeField]
	private Selectable.Transition m_Transition = Selectable.Transition.ColorTint;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private ColorBlock m_ActiveColors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private SpriteState m_ActiveSpriteState;

	[SerializeField]
	private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

	[SerializeField]
	private AnimationTriggers m_ActiveAnimationTriggers = new AnimationTriggers();

	[SerializeField]
	private TMPColorGradientState m_ColorGradient;

	[SerializeField]
	private TMPColorGradientState m_ActiveColorGradient;

	[SerializeField]
	private Graphic m_TargetGraphic;

	private IFHSelectable.SelectionState _currentState;

	private bool _focused;

	public Image image
	{
		get
		{
			return m_TargetGraphic as Image;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public TMP_Text Text
	{
		get
		{
			return m_TargetGraphic as TMP_Text;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	private void Awake()
	{
		m_TargetGraphic = GetComponent<Graphic>();
		if (_parent != null && _selectableParent == null)
		{
			FindParent();
		}
	}

	private void OnEnable()
	{
		if (_selectableParent != null)
		{
			DoStateTransition(GetParentState(), IsFocused(), instant: true);
			WireParentEvents();
		}
	}

	private void OnDisable()
	{
		if (_selectableParent != null)
		{
			_selectableParent.OnSelectionStateChanged -= OnParentStateChanged;
		}
	}

	private void Reset()
	{
		if (_parent == null)
		{
			_parent = GetComponentInParent<FHTabButton>(includeInactive: true);
		}
		FindParent();
		DoStateTransition(IFHSelectable.SelectionState.Normal, focused: false, instant: true);
	}

	private void FindParent()
	{
		if (_selectableParent == null)
		{
			_selectableParent = _parent;
			if (_selectableParent != null)
			{
				WireParentEvents();
			}
		}
	}

	private void WireParentEvents()
	{
		_selectableParent.OnSelectionStateChanged -= OnParentStateChanged;
		_selectableParent.OnSelectionStateChanged += OnParentStateChanged;
	}

	private void OnParentStateChanged(IFHSelectable.SelectionState state)
	{
		DoStateTransition(state, IsFocused(), instant: true);
	}

	public void RefreshState()
	{
		DoStateTransition(GetParentState(), IsFocused(), instant: true);
	}

	private void DoStateTransition(IFHSelectable.SelectionState state, bool focused, bool instant)
	{
		if (gameObject.activeInHierarchy && _selectableParent != null)
		{
			_currentState = state;
			_focused = focused;
			Color color;
			Sprite newSprite;
			string triggername;
			TMP_ColorGradient newPreset;
			switch (state)
			{
			case IFHSelectable.SelectionState.Normal:
				color = (_focused ? m_ActiveColors.normalColor : m_Colors.normalColor);
				newSprite = (_focused ? null : null);
				triggername = (_focused ? m_ActiveAnimationTriggers.normalTrigger : m_AnimationTriggers.normalTrigger);
				newPreset = (_focused ? m_ActiveColorGradient.Normal : m_ColorGradient.Normal);
				break;
			case IFHSelectable.SelectionState.Highlighted:
				color = (_focused ? m_ActiveColors.highlightedColor : m_Colors.highlightedColor);
				newSprite = (_focused ? m_ActiveSpriteState.highlightedSprite : m_SpriteState.highlightedSprite);
				triggername = (_focused ? m_ActiveAnimationTriggers.highlightedTrigger : m_AnimationTriggers.highlightedTrigger);
				newPreset = (_focused ? m_ActiveColorGradient.Highlighted : m_ColorGradient.Highlighted);
				break;
			case IFHSelectable.SelectionState.Pressed:
				color = (_focused ? m_ActiveColors.pressedColor : m_Colors.pressedColor);
				newSprite = (_focused ? m_ActiveSpriteState.pressedSprite : m_SpriteState.pressedSprite);
				triggername = (_focused ? m_ActiveAnimationTriggers.pressedTrigger : m_AnimationTriggers.pressedTrigger);
				newPreset = (_focused ? m_ActiveColorGradient.pressed : m_ColorGradient.pressed);
				break;
			case IFHSelectable.SelectionState.Selected:
				color = (_focused ? m_ActiveColors.selectedColor : m_Colors.selectedColor);
				newSprite = (_focused ? m_ActiveSpriteState.selectedSprite : m_SpriteState.selectedSprite);
				triggername = (_focused ? m_ActiveAnimationTriggers.selectedTrigger : m_AnimationTriggers.selectedTrigger);
				newPreset = (_focused ? m_ActiveColorGradient.selected : m_ColorGradient.selected);
				break;
			case IFHSelectable.SelectionState.Disabled:
				color = (_focused ? m_ActiveColors.disabledColor : m_Colors.disabledColor);
				newSprite = (_focused ? m_ActiveSpriteState.disabledSprite : m_SpriteState.disabledSprite);
				triggername = (_focused ? m_ActiveAnimationTriggers.disabledTrigger : m_AnimationTriggers.disabledTrigger);
				newPreset = (_focused ? m_ActiveColorGradient.disabled : m_ColorGradient.disabled);
				break;
			default:
				color = Color.black;
				newSprite = null;
				triggername = string.Empty;
				newPreset = null;
				break;
			}
			switch (m_Transition)
			{
			case Selectable.Transition.ColorTint:
				StartColorTween(color * m_Colors.colorMultiplier, instant);
				break;
			case Selectable.Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Selectable.Transition.Animation:
				TriggerAnimation(triggername);
				break;
			}
			DoColorGradientSwap(newPreset);
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(image == null))
		{
			image.overrideSprite = newSprite;
		}
	}

	private void DoColorGradientSwap(TMP_ColorGradient newPreset)
	{
		if (!(Text == null) && Text.enableVertexGradient)
		{
			Text.colorGradientPreset = newPreset;
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void TriggerAnimation(string triggername)
	{
	}

	private bool IsFocused()
	{
		if (_parent != null)
		{
			return _parent.IsFocused;
		}
		return false;
	}

	private IFHSelectable.SelectionState GetParentState()
	{
		if (!_selectableParent.IsInteractable())
		{
			return IFHSelectable.SelectionState.Disabled;
		}
		if (_selectableParent.IsSelected())
		{
			return IFHSelectable.SelectionState.Selected;
		}
		if (_selectableParent.IsHighlighted())
		{
			return IFHSelectable.SelectionState.Highlighted;
		}
		return IFHSelectable.SelectionState.Normal;
	}
}
