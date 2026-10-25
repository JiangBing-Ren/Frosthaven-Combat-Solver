using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverFadeEffects : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum FadeMode
	{
		FadeIn,
		FadeOut
	}

	[Tooltip("Select target CanvasGroup to fade. Cannot select this object!")]
	public CanvasGroup TargetCanvas;

	public FadeMode Mode;

	public float Duration = 0.3f;

	public bool FadeOnSelected = true;

	public bool FadeNonInteractable = true;

	private Selectable _selectable;

	private Tween _tween;

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return FadeNonInteractable;
			}
			return true;
		}
	}

	private float _hoveredFadeAlpha
	{
		get
		{
			if (Mode != FadeMode.FadeIn)
			{
				return 0f;
			}
			return 1f;
		}
	}

	private float _nonHoverFadeAlpha
	{
		get
		{
			if (Mode != FadeMode.FadeIn)
			{
				return 1f;
			}
			return 0f;
		}
	}

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
		TargetCanvas.gameObject.SetActive(Mode == FadeMode.FadeOut);
		TargetCanvas.alpha = _nonHoverFadeAlpha;
	}

	private void OnDisable()
	{
		if (_tween != null && _tween.IsActive())
		{
			_tween.Kill();
		}
	}

	private void FadeTo(float alpha)
	{
		TargetCanvas.gameObject.SetActive(value: true);
		if (_tween != null && _tween.IsActive())
		{
			_tween.Kill();
		}
		_tween = TargetCanvas.DOFade(alpha, Duration).SetEase(Ease.OutExpo).SetUpdate(isIndependentUpdate: true)
			.OnComplete(OnFadeComplete);
	}

	private void OnFadeComplete()
	{
		if (TargetCanvas.alpha == 0f)
		{
			TargetCanvas.gameObject.SetActive(value: false);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			FadeTo(_hoveredFadeAlpha);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((!FadeOnSelected || !(EventSystem.current?.currentSelectedGameObject == gameObject)) && TargetCanvas.gameObject.activeSelf)
		{
			FadeTo(_nonHoverFadeAlpha);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && FadeOnSelected)
		{
			FadeTo(_hoveredFadeAlpha);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (FadeOnSelected)
		{
			FadeTo(_nonHoverFadeAlpha);
		}
	}

	private void OnValidate()
	{
		if (TargetCanvas == null)
		{
			Log.GUIError(name + " has UIHoverFadeEffects with no targets", this);
		}
		if ((bool)TargetCanvas && TargetCanvas.gameObject == gameObject)
		{
			Log.GUIError(name + " has UIHoverFadeEffects targeting itself. This is not allowed!", this);
		}
	}
}
