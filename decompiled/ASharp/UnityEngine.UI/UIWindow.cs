using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("UI/UIWindow", 58)]
[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : MonoBehaviour, IEventSystemHandler, ISelectHandler, IPointerDownHandler, IExternallyStarted
{
	public enum Transition
	{
		Instant,
		Fade
	}

	public enum VisualState
	{
		Shown,
		Hidden
	}

	public enum FocusedAction
	{
		BringToFront,
		None,
		SetAsLastChildren
	}

	[Serializable]
	public class TransitionBeginEvent : UnityEvent<UIWindow, VisualState, bool>
	{
	}

	[Serializable]
	public class TransitionCompleteEvent : UnityEvent<UIWindow, VisualState>
	{
	}

	[Serializable]
	public class VisibilityEvent : UnityEvent
	{
	}

	protected static UIWindow m_FocusedWindow;

	[SerializeField]
	private bool m_BlocksRaycasts = true;

	[SerializeField]
	private VisualState m_StartingState = VisualState.Hidden;

	[SerializeField]
	private FocusedAction m_FocusedAction;

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private Ease m_TransitionDOEasing = Ease.InOutQuint;

	[SerializeField]
	private float m_TransitionDuration = 0.1f;

	[SerializeField]
	private string m_AudioItemShow;

	[SerializeField]
	private string m_AudioItemHide;

	[SerializeField]
	private bool m_DisabledClickToFocus;

	protected bool m_IsFocused;

	private VisualState m_CurrentVisualState = VisualState.Hidden;

	private CanvasGroup m_CanvasGroup;

	public TransitionBeginEvent onTransitionBegin = new TransitionBeginEvent();

	public TransitionCompleteEvent onTransitionComplete = new TransitionCompleteEvent();

	public VisibilityEvent onShown = new VisibilityEvent();

	public VisibilityEvent onHidden = new VisibilityEvent();

	public UnityEvent onFocused = new UnityEvent();

	private bool m_DefaultAudioItemsResolved;

	public static UIWindow FocusedWindow
	{
		get
		{
			return m_FocusedWindow;
		}
		private set
		{
			m_FocusedWindow = value;
		}
	}

	public bool IsPopUp { get; set; }

	public Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public Ease transitionEasing
	{
		get
		{
			return m_TransitionDOEasing;
		}
		set
		{
			m_TransitionDOEasing = value;
		}
	}

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
		}
	}

	public bool HasGoneToStartingState { get; private set; }

	public string AudioItemHide
	{
		get
		{
			EnsureDefaultAudioItems();
			return m_AudioItemHide;
		}
		set
		{
			EnsureDefaultAudioItems();
			m_AudioItemHide = value;
		}
	}

	public string AudioItemShow
	{
		get
		{
			EnsureDefaultAudioItems();
			return m_AudioItemShow;
		}
		set
		{
			EnsureDefaultAudioItems();
			m_AudioItemShow = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (m_CanvasGroup != null && m_CanvasGroup.alpha > 0f)
			{
				return gameObject.activeSelf;
			}
			return false;
		}
	}

	public bool IsOpen => m_CurrentVisualState == VisualState.Shown;

	public bool IsFocused => m_IsFocused;

	private void EnsureDefaultAudioItems()
	{
		if (m_DefaultAudioItemsResolved || !Application.isPlaying)
		{
			return;
		}
		AudioMapViewModuleProfile audioMapViewModuleProfile = ((UIInfoTools.Instance != null) ? UIInfoTools.Instance.defaultAudioMapViewModuleProfile : null);
		if (!(audioMapViewModuleProfile == null))
		{
			if (string.IsNullOrEmpty(m_AudioItemShow))
			{
				m_AudioItemShow = audioMapViewModuleProfile.OnShow;
			}
			if (string.IsNullOrEmpty(m_AudioItemHide))
			{
				m_AudioItemHide = audioMapViewModuleProfile.OnHide;
			}
			m_DefaultAudioItemsResolved = true;
		}
	}

	protected virtual void Awake()
	{
		m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
	}

	public void ExternalStart()
	{
		if (!HasGoneToStartingState)
		{
			if (m_CanvasGroup == null)
			{
				m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
			}
			if (Application.isPlaying)
			{
				EvaluateAndTransitionToVisualState(m_StartingState, instant: true);
			}
			HasGoneToStartingState = true;
		}
	}

	protected virtual void Start()
	{
		ExternalStart();
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		Focus();
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		if (!m_DisabledClickToFocus)
		{
			Focus();
		}
	}

	public virtual void Focus()
	{
		if (!m_IsFocused)
		{
			m_IsFocused = true;
			OnBeforeFocusWindow(this);
			if (m_FocusedAction == FocusedAction.BringToFront)
			{
				UIUtility.BringToFront(gameObject);
			}
			else if (m_FocusedAction == FocusedAction.SetAsLastChildren)
			{
				gameObject.transform.SetAsLastSibling();
			}
			onFocused.Invoke();
		}
	}

	public virtual void Toggle()
	{
		if (m_CurrentVisualState == VisualState.Shown)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public virtual void Show()
	{
		Show(instant: false);
	}

	public virtual void Show(bool instant)
	{
		Focus();
		if (!HasGoneToStartingState)
		{
			m_StartingState = VisualState.Shown;
		}
		else if (m_CurrentVisualState != VisualState.Shown)
		{
			EnsureDefaultAudioItems();
			if (!SceneController.Instance.IsLoading)
			{
				AudioControllerUtils.PlaySound(m_AudioItemShow);
			}
			EvaluateAndTransitionToVisualState(VisualState.Shown, instant);
			onShown.Invoke();
		}
	}

	public virtual void Hide()
	{
		Hide(instant: false);
	}

	public virtual void Hide(bool instant)
	{
		if (!HasGoneToStartingState)
		{
			m_StartingState = VisualState.Hidden;
		}
		else if (m_CurrentVisualState != VisualState.Hidden)
		{
			EnsureDefaultAudioItems();
			if (!SceneController.Instance.IsLoading)
			{
				AudioControllerUtils.PlaySound(m_AudioItemHide);
			}
			onHidden.Invoke();
			EvaluateAndTransitionToVisualState(VisualState.Hidden, instant);
		}
	}

	protected virtual void EvaluateAndTransitionToVisualState(VisualState state, bool instant)
	{
		if (!gameObject.activeSelf)
		{
			m_CanvasGroup.alpha = 0f;
		}
		if (state == VisualState.Shown || (state == VisualState.Hidden && gameObject.activeSelf))
		{
			gameObject.SetActive(value: true);
		}
		bool flag = enabled && (transform.parent == null || transform.parent.gameObject.activeInHierarchy);
		float num = ((state == VisualState.Shown) ? 1f : 0f);
		if (onTransitionBegin != null)
		{
			onTransitionBegin.Invoke(this, state, instant || m_Transition == Transition.Instant);
		}
		m_CurrentVisualState = state;
		if (((m_Transition == Transition.Fade) & flag) && !instant)
		{
			m_CanvasGroup.blocksRaycasts = m_BlocksRaycasts && IsOpen;
			m_CanvasGroup.DOKill();
			m_CanvasGroup.DOFade(num, m_TransitionDuration).SetEase(m_TransitionDOEasing).SetUpdate(isIndependentUpdate: true)
				.OnComplete(OnTweenFinished);
			return;
		}
		m_CanvasGroup.alpha = num;
		m_CanvasGroup.blocksRaycasts = m_BlocksRaycasts && IsOpen;
		gameObject.SetActive(IsOpen);
		m_CanvasGroup.alpha = 1f;
		if (onTransitionComplete != null)
		{
			onTransitionComplete.Invoke(this, state);
		}
	}

	protected virtual void OnTweenFinished()
	{
		m_CanvasGroup.blocksRaycasts = m_BlocksRaycasts && IsOpen;
		gameObject.SetActive(IsOpen);
		m_CanvasGroup.alpha = 1f;
		if (onTransitionComplete != null)
		{
			onTransitionComplete.Invoke(this, m_CurrentVisualState);
		}
	}

	public static List<UIWindow> GetWindows()
	{
		List<UIWindow> list = new List<UIWindow>();
		UIWindow[] array = Object.FindObjectsOfType<UIWindow>(includeInactive: true);
		foreach (UIWindow uIWindow in array)
		{
			if (uIWindow.gameObject.activeInHierarchy)
			{
				list.Add(uIWindow);
			}
		}
		return list;
	}

	protected static void OnBeforeFocusWindow(UIWindow window)
	{
		if (m_FocusedWindow != null)
		{
			m_FocusedWindow.m_IsFocused = false;
		}
		m_FocusedWindow = window;
	}

	public static void HidePopups()
	{
		foreach (UIWindow window in GetWindows())
		{
			if (window.IsOpen && window.IsPopUp)
			{
				window.Hide();
			}
		}
	}
}
