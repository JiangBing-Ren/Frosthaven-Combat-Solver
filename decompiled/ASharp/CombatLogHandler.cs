using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class CombatLogHandler : Singleton<CombatLogHandler>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private class LogInfo
	{
		public string text;

		public List<CombatLogFilter> filters;

		public CombatLogText row;

		public Sprite background;

		public CombatLogText prefab;

		public LogInfo(string text, CombatLogText prefab, Sprite background = null, List<CombatLogFilter> filters = null, CombatLogText row = null)
		{
			this.text = text;
			this.filters = filters;
			this.row = row;
			this.background = background;
			this.prefab = prefab;
		}
	}

	private const float ExpandedHeightPercent = 1f;

	private const float LayoutTolerance = 0.01f;

	private const float MouseWheelDeltaThreshold = 0.01f;

	private const int MouseWheelStep = 1;

	[SerializeField]
	private UIWindow window;

	[Header("Texts")]
	[SerializeField]
	private CombatLogText combatLogTextHighlighted;

	[SerializeField]
	private CombatLogText combatLogText;

	[SerializeField]
	private Sprite enemyHighlightImage;

	[SerializeField]
	private Sprite playerHighlightImage;

	[SerializeField]
	private Sprite summonHighlightImage;

	[SerializeField]
	private Sprite roundHighlightImage;

	[Header("Structure")]
	[SerializeField]
	private RectTransform logViewport;

	[SerializeField]
	private RectTransform logContent;

	[SerializeField]
	private Slider scrollSlider;

	[SerializeField]
	private RectTransform combatLogWindow;

	[SerializeField]
	[Min(1f)]
	private int visibleEntryCount = 8;

	[Header("Minimize/Expand")]
	[SerializeField]
	private float minimizeToPercent = 0.5f;

	[SerializeField]
	private float expandAnimationTime = 0.5f;

	private readonly List<LogInfo> logs = new List<LogInfo>();

	private readonly List<LogInfo> filteredLogs = new List<LogInfo>();

	private int visibleStartIndex;

	private bool expanded;

	private bool hovered;

	private bool isSliderInteracting;

	private bool isUpdatingScrollSlider;

	private Tween minAnimation;

	protected override void AwakeInit()
	{
		ResolveRuntimeReferences();
		ValidateReferences();
		ConfigureScrollSlider();
		combatLogWindow.anchorMax = new Vector2(1f, minimizeToPercent);
		expanded = false;
		window.onShown.AddListener(() =>
		{
			UpdateLogs(moveToLatest: true);
		});
		if (SaveData.Instance.Global.DisabledCombatLog)
		{
			window.Hide();
		}
		else
		{
			window.Show();
		}
		if (SaveData.Instance != null)
		{
			SaveData.Instance.GlobalDataChanged -= OnGlobalDataChanged;
			SaveData.Instance.GlobalDataChanged += OnGlobalDataChanged;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (SaveData.Instance != null)
		{
			SaveData.Instance.GlobalDataChanged -= OnGlobalDataChanged;
		}
	}

	private void Update()
	{
		UpdateSliderInteractionState();
		if (hovered && window.IsOpen && !isSliderInteracting)
		{
			float y = Input.mouseScrollDelta.y;
			if (!(Mathf.Abs(y) <= 0.01f))
			{
				int num = ((!(y > 0f)) ? 1 : (-1));
				SetVisibleStartIndex(visibleStartIndex + num);
			}
		}
	}

	private void OnEnable()
	{
		AttachScrollSliderListener();
		if (Singleton<CombatLogSettings>.Instance != null)
		{
			Singleton<CombatLogSettings>.Instance.OnCombatLogFiltersChanged.AddListener(HandleCombatLogFiltersChanged);
			Singleton<CombatLogSettings>.Instance.OnEnableCombatLog.AddListener(window.Show);
			Singleton<CombatLogSettings>.Instance.OnDisableCombatLog.AddListener(window.Hide);
		}
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			if (scrollSlider != null)
			{
				scrollSlider.onValueChanged.RemoveListener(HandleOnScrollSliderChanged);
			}
			isSliderInteracting = false;
			RecycleAllSpawnedLogs();
			CancelAnimation();
			if (Singleton<CombatLogSettings>.Instance != null)
			{
				Singleton<CombatLogSettings>.Instance.OnCombatLogFiltersChanged.RemoveListener(HandleCombatLogFiltersChanged);
				Singleton<CombatLogSettings>.Instance.OnEnableCombatLog.RemoveListener(window.Show);
				Singleton<CombatLogSettings>.Instance.OnDisableCombatLog.RemoveListener(window.Hide);
			}
		}
	}

	private bool IsFiltered(List<CombatLogFilter> filters)
	{
		return filters?.Exists((CombatLogFilter filter) => !SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter)) ?? true;
	}

	private void HandleCombatLogFiltersChanged()
	{
		UpdateLogs();
	}

	private void UpdateLogs(bool moveToLatest = false)
	{
		BuildFilteredLogs();
		int count = filteredLogs.Count;
		if (moveToLatest)
		{
			visibleStartIndex = GetLatestVisibleStartIndex(count);
		}
		else
		{
			visibleStartIndex = GetClampedVisibleStartIndex(count);
		}
		if (!window.IsOpen)
		{
			RecycleAllSpawnedLogs();
			UpdateExpansionFromContent();
			UpdateScrollSliderState(count);
			return;
		}
		int visibleEndIndex = GetVisibleEndIndex(count, visibleStartIndex);
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			LogInfo logInfo = filteredLogs[i];
			if (i >= visibleStartIndex && i < visibleEndIndex)
			{
				if (logInfo.row == null)
				{
					logInfo.row = CreateLog(logInfo.prefab, logInfo.text, logInfo.background);
				}
				else
				{
					logInfo.row.gameObject.SetActive(value: true);
				}
				logInfo.row.transform.SetSiblingIndex(num);
				num++;
			}
			else if (logInfo.row != null)
			{
				RecycleLogObject(logInfo);
			}
		}
		UpdateExpansionFromContent();
		UpdateScrollSliderState(count);
	}

	public void AddLog(string text, CombatLogFilter filter)
	{
		AddLog(text, new List<CombatLogFilter> { filter });
	}

	public void AddLog(string text, List<CombatLogFilter> filters = null)
	{
		AddLog(combatLogText, text, filters, null);
	}

	public void AddHighlightedLog(string text, CActor.EType? type, List<CombatLogFilter> filters = null)
	{
		Sprite background;
		if (type == CActor.EType.Player)
		{
			text = $"<color=#a7cc7b>{text}</color>";
			background = playerHighlightImage;
		}
		else if (type == CActor.EType.Enemy)
		{
			text = $"<color=#f16e6e>{text}</color>";
			background = enemyHighlightImage;
		}
		else if (type == CActor.EType.HeroSummon)
		{
			text = $"<color=#d4d979>{text}</color>";
			background = summonHighlightImage;
		}
		else
		{
			background = roundHighlightImage;
		}
		AddLog(combatLogTextHighlighted, text, filters, background);
	}

	private void AddLog(CombatLogText log, string text, List<CombatLogFilter> filters, Sprite background)
	{
		if (log == null)
		{
			throw new ArgumentNullException("log");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		bool moveToLatest = IsShowingLatestEntries();
		logs.Add(new LogInfo(text, log, background, filters));
		if (window.IsOpen)
		{
			UpdateLogs(moveToLatest);
		}
		else
		{
			UpdateScrollSliderState(CountFilteredLogs());
		}
		text = Regex.Replace(text, "<sprite name=([^>]+)>", "[$1]");
		Log.SimpleInfo(text);
	}

	private CombatLogText CreateLog(CombatLogText log, string text, Sprite background)
	{
		CombatLogText combatLogText = ObjectPool.Spawn(log, logContent);
		Log.GUIInfo("COMBAT LOG DEBUGGING: Spawned log with text: " + text + "and Prefab Name: " + combatLogText.gameObject.name);
		combatLogText.Init(text, background);
		return combatLogText;
	}

	[ContextMenu("Clear Log")]
	public void ClearLog()
	{
		RecycleAllSpawnedLogs();
		logs.Clear();
		filteredLogs.Clear();
		visibleStartIndex = 0;
		UpdateScrollSliderState(0);
		CancelAnimation();
		combatLogWindow.anchorMax = new Vector2(1f, minimizeToPercent);
		expanded = false;
	}

	private void ResolveRuntimeReferences()
	{
		if (logViewport == null)
		{
			Transform transform = base.transform.Find("Scroll View/Viewport");
			if (transform != null)
			{
				logViewport = transform as RectTransform;
			}
		}
		if (logContent == null)
		{
			if (logViewport != null)
			{
				Transform transform2 = logViewport.Find("Content");
				if (transform2 != null)
				{
					logContent = transform2 as RectTransform;
				}
			}
			else
			{
				Transform transform3 = base.transform.Find("Scroll View/Viewport/Content");
				if (transform3 != null)
				{
					logContent = transform3 as RectTransform;
				}
			}
		}
		if (scrollSlider == null)
		{
			Transform transform4 = base.transform.Find("Scroll View");
			if (transform4 != null)
			{
				scrollSlider = transform4.GetComponentInChildren<Slider>(includeInactive: true);
			}
			if (scrollSlider == null)
			{
				scrollSlider = GetComponentInChildren<Slider>(includeInactive: true);
			}
		}
	}

	private void ValidateReferences()
	{
		if (window == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a UIWindow reference.");
		}
		if (logContent == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a log content transform.");
		}
		if (logViewport == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a log viewport transform.");
		}
		if (scrollSlider == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a scroll slider.");
		}
		if (combatLogWindow == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a combat log window transform.");
		}
		if (combatLogText == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a combat log text prefab.");
		}
		if (combatLogTextHighlighted == null)
		{
			throw new InvalidOperationException("CombatLogHandler requires a highlighted combat log text prefab.");
		}
		if (visibleEntryCount < 1)
		{
			throw new InvalidOperationException("CombatLogHandler requires at least one visible entry.");
		}
		if (minimizeToPercent <= 0f || minimizeToPercent > 1f)
		{
			throw new InvalidOperationException("CombatLogHandler minimize height must be in range (0, 1].");
		}
		if (expandAnimationTime < 0f)
		{
			throw new InvalidOperationException("CombatLogHandler expand animation time cannot be negative.");
		}
	}

	private void ConfigureScrollSlider()
	{
		scrollSlider.wholeNumbers = true;
		AttachScrollSliderListener();
	}

	private void AttachScrollSliderListener()
	{
		if (!(scrollSlider == null))
		{
			scrollSlider.onValueChanged.RemoveListener(HandleOnScrollSliderChanged);
			scrollSlider.onValueChanged.AddListener(HandleOnScrollSliderChanged);
		}
	}

	private void BuildFilteredLogs()
	{
		filteredLogs.Clear();
		for (int i = 0; i < logs.Count; i++)
		{
			LogInfo logInfo = logs[i];
			if (IsFiltered(logInfo.filters))
			{
				filteredLogs.Add(logInfo);
			}
			else if (logInfo.row != null)
			{
				RecycleLogObject(logInfo);
			}
		}
	}

	private void RecycleAllSpawnedLogs()
	{
		for (int i = 0; i < logs.Count; i++)
		{
			RecycleLogObject(logs[i]);
		}
	}

	private void RecycleLogObject(LogInfo logInfo)
	{
		if (!(logInfo.row == null))
		{
			if (ObjectPool.instance != null)
			{
				ObjectPool.Recycle(logInfo.row.gameObject, logInfo.prefab.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(logInfo.row.gameObject);
			}
			logInfo.row = null;
		}
	}

	private void UpdateScrollSliderState(int filteredCount)
	{
		if (!(scrollSlider == null))
		{
			int clampedVisibleStartIndex = GetClampedVisibleStartIndex(filteredCount);
			if (visibleStartIndex != clampedVisibleStartIndex)
			{
				visibleStartIndex = clampedVisibleStartIndex;
			}
			int latestVisibleStartIndex = GetLatestVisibleStartIndex(filteredCount);
			bool flag = latestVisibleStartIndex > 0;
			scrollSlider.gameObject.SetActive(flag);
			scrollSlider.interactable = flag;
			scrollSlider.minValue = 0f;
			scrollSlider.maxValue = latestVisibleStartIndex;
			if (!flag)
			{
				SetScrollSliderValue(0);
			}
			else
			{
				SetScrollSliderValue(clampedVisibleStartIndex);
			}
		}
	}

	private void HandleOnScrollSliderChanged(float sliderValue)
	{
		if (!isUpdatingScrollSlider)
		{
			int filteredCount = CountFilteredLogs();
			int latestVisibleStartIndex = GetLatestVisibleStartIndex(filteredCount);
			if (latestVisibleStartIndex != 0)
			{
				int value = Mathf.RoundToInt(sliderValue);
				value = Mathf.Clamp(value, 0, latestVisibleStartIndex);
				SetVisibleStartIndex(value);
			}
		}
	}

	private void SetScrollSliderValue(int value)
	{
		if (!(scrollSlider == null))
		{
			value = Mathf.Clamp(value, (int)scrollSlider.minValue, (int)scrollSlider.maxValue);
			isUpdatingScrollSlider = true;
			scrollSlider.value = value;
			isUpdatingScrollSlider = false;
		}
	}

	private void SetVisibleStartIndex(int requestedStartIndex)
	{
		int filteredCount = CountFilteredLogs();
		int latestVisibleStartIndex = GetLatestVisibleStartIndex(filteredCount);
		int num = Mathf.Clamp(requestedStartIndex, 0, latestVisibleStartIndex);
		if (num != visibleStartIndex)
		{
			visibleStartIndex = num;
			UpdateLogs();
		}
	}

	private void UpdateSliderInteractionState()
	{
		if (!window.IsOpen)
		{
			isSliderInteracting = false;
		}
		else if (!isSliderInteracting)
		{
			if (Input.GetMouseButtonDown(0) && IsPointerOverSlider())
			{
				isSliderInteracting = true;
				ExpandLog();
			}
		}
		else if (!Input.GetMouseButton(0))
		{
			isSliderInteracting = false;
			if (!hovered)
			{
				MinimizeLog();
			}
		}
	}

	private bool IsPointerOverSlider()
	{
		if (scrollSlider == null)
		{
			return false;
		}
		RectTransform rectTransform = scrollSlider.transform as RectTransform;
		if (rectTransform == null)
		{
			return false;
		}
		return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
	}

	private bool IsShowingLatestEntries()
	{
		int num = CountFilteredLogs();
		int clampedVisibleStartIndex = GetClampedVisibleStartIndex(num);
		return GetVisibleEndIndex(num, clampedVisibleStartIndex) >= num;
	}

	private int CountFilteredLogs()
	{
		int num = 0;
		for (int i = 0; i < logs.Count; i++)
		{
			if (IsFiltered(logs[i].filters))
			{
				num++;
			}
		}
		return num;
	}

	private int GetLatestVisibleStartIndex(int filteredCount)
	{
		if (filteredCount < 0)
		{
			throw new ArgumentOutOfRangeException("filteredCount", "Filtered log count cannot be negative.");
		}
		return Mathf.Max(0, filteredCount - visibleEntryCount);
	}

	private int GetClampedVisibleStartIndex(int filteredCount)
	{
		return Mathf.Clamp(visibleStartIndex, 0, GetLatestVisibleStartIndex(filteredCount));
	}

	private int GetVisibleEndIndex(int filteredCount, int visibleStart)
	{
		if (visibleStart < 0)
		{
			throw new ArgumentOutOfRangeException("visibleStart", "Visible start index cannot be negative.");
		}
		int latestVisibleStartIndex = GetLatestVisibleStartIndex(filteredCount);
		if (visibleStart > latestVisibleStartIndex)
		{
			throw new ArgumentOutOfRangeException("visibleStart", "Visible start index cannot exceed latest start index.");
		}
		return Mathf.Min(filteredCount, visibleStart + visibleEntryCount);
	}

	public void ExpandLog()
	{
		if (!expanded && NeedsExpandedView())
		{
			expanded = true;
			CancelAnimation();
			minAnimation = combatLogWindow.DOAnchorMax(new Vector2(1f, 1f), expandAnimationTime).OnComplete(() =>
			{
				minAnimation = null;
			});
		}
	}

	public void MinimizeLog()
	{
		if (expanded)
		{
			expanded = false;
			CancelAnimation();
			minAnimation = combatLogWindow.DOAnchorMax(new Vector2(1f, minimizeToPercent), expandAnimationTime).OnComplete(() =>
			{
				minAnimation = null;
			});
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
		ExpandLog();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
		if (!isSliderInteracting)
		{
			MinimizeLog();
		}
	}

	private void CancelAnimation()
	{
		if (minAnimation != null)
		{
			minAnimation.Kill();
		}
		minAnimation = null;
	}

	private void UpdateExpansionFromContent()
	{
		if (hovered || isSliderInteracting)
		{
			ExpandLog();
		}
		else if (expanded && !NeedsExpandedView())
		{
			MinimizeLog();
		}
	}

	private bool NeedsExpandedView()
	{
		return logContent.rect.height - logViewport.rect.height > 0.01f;
	}

	private void OnGlobalDataChanged()
	{
		if (SaveData.Instance.Global.DisabledCombatLog)
		{
			window.Hide();
		}
		else
		{
			window.Show();
		}
	}
}
