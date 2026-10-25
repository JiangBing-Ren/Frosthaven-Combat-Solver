namespace UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class UIBlackOverlay : Singleton<UIBlackOverlay>
{
	private Image image;

	private Canvas defaultCanvas;

	private CanvasGroup canvasGroup;

	private UIWindow.VisualState state;

	private int windowCount;

	protected override void AwakeInit()
	{
		image = GetComponent<Image>();
		defaultCanvas = GetComponentInParent<Canvas>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = false;
		HideInstant();
	}

	public bool IsActive()
	{
		if (enabled)
		{
			return gameObject.activeInHierarchy;
		}
		return false;
	}

	public bool IsVisible()
	{
		return state == UIWindow.VisualState.Shown;
	}

	public void HideInstant()
	{
		Hide(0f);
	}

	public void ShowControlled(UIWindow window, float alphaWhenVisible = 1f, float transitionDuration = -1f)
	{
		DoTimedTransition(window, UIWindow.VisualState.Shown, alphaWhenVisible, transitionDuration);
	}

	public void HideControlled(UIWindow window, float transitionDuration = -1f)
	{
		DoTimedTransition(window, UIWindow.VisualState.Hidden, 1f, transitionDuration);
	}

	public void DoTimedTransition(UIWindow window, UIWindow.VisualState wantedState, bool instant = false)
	{
		DoTimedTransition(window, wantedState, 1f, instant ? 0f : (-1f));
	}

	private void DoTimedTransition(UIWindow window, UIWindow.VisualState wantedState, float alphaWhenVisible, float transitionDuration)
	{
		if (!IsActive() || window == null || (wantedState == UIWindow.VisualState.Hidden && !IsVisible()))
		{
			return;
		}
		float duration = ((transitionDuration < 0f) ? window.transitionDuration : transitionDuration);
		if (wantedState == UIWindow.VisualState.Shown)
		{
			windowCount++;
			if (IsVisible())
			{
				UIUtility.BringToFront(window.gameObject, (Canvas)null, false);
				return;
			}
			state = wantedState;
			UIUtility.BringToFront(gameObject, UIUtility.FindInParents<Canvas>(window.gameObject), worldPositionStays: false);
			UIUtility.BringToFront(window.gameObject, (Canvas)null, false);
			image.CrossFadeAlpha(Mathf.Clamp(alphaWhenVisible, 0f, 1f), duration, ignoreTimeScale: true);
			window.onHidden.RemoveListener(ParentToDefaultCanvas);
			window.onHidden.AddListener(ParentToDefaultCanvas);
			canvasGroup.blocksRaycasts = true;
		}
		else
		{
			Hide(duration);
		}
	}

	private void Hide(float duration)
	{
		Log.GUIInfo("About to hide the overlay. Current window count: " + windowCount);
		windowCount--;
		if (windowCount < 0)
		{
			windowCount = 0;
		}
		if (windowCount <= 0 && state != UIWindow.VisualState.Hidden)
		{
			state = UIWindow.VisualState.Hidden;
			if (duration <= 0f)
			{
				image.CrossFadeAlpha(0f, 0f, ignoreTimeScale: true);
				image.canvasRenderer.SetAlpha(0f);
			}
			else
			{
				image.CrossFadeAlpha(0f, duration, ignoreTimeScale: true);
			}
			canvasGroup.blocksRaycasts = false;
		}
	}

	public void ParentToDefaultCanvas()
	{
		transform.SetParent(defaultCanvas.transform, worldPositionStays: false);
	}
}
