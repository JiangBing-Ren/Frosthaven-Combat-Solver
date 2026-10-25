using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class ExtendedButton : Button
{
	public TextMeshProUGUI buttonText;

	public string textLanguageKey;

	public RectTransform targetRect;

	public bool scaleNonInteractable = true;

	public float highlightScaleFactor;

	public Transform[] ignoreScalingObjects;

	public bool animateScaling = true;

	public Vector3 hoverMovement = Vector3.zero;

	public float animationDuration = 0.3f;

	public bool autoDisplayTooltip = true;

	public UnityEvent onMouseEnter;

	public UnityEvent onMouseExit;

	public UnityEvent onSelected;

	public UnityEvent onDeselected;

	public bool restoreHighlightWhenReenable;

	public RectTransform overridedTargetRectScale;

	public RectTransform overridedTargetRectMovement;

	public AudioButtonProfile audioProfile;

	public bool useAudioController = true;

	public string mouseClickAudioItem;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string nonInteractableMouseDownAudioItem;

	public AudioClip mouseDownAudio;

	public AudioClip mouseUpAudio;

	public AudioClip mouseEnterAudio;

	public AudioClip mouseExitAudio;

	private CanvasGroup canvasGroup;

	private bool isFaded;

	private bool isHighlighted;

	private bool isMoved;

	private bool wasHighlightedOnDisable;

	public bool navigateOnlyInteractable;

	private Vector2 hoverStartPosition;

	public string TextLanguageKey
	{
		set
		{
			textLanguageKey = value;
			if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
			{
				buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
			}
		}
	}

	public bool IsFaded => isFaded;

	public RectTransform TargetRect
	{
		get
		{
			if (targetRect == null)
			{
				targetRect = GetComponent<RectTransform>();
			}
			return targetRect;
		}
	}

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (canvasGroup == null)
			{
				canvasGroup = GetComponent<CanvasGroup>();
			}
			return canvasGroup;
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (interactable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
		base.OnPointerClick(eventData);
		if (interactable)
		{
			PlaySound(null, (mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseClickAudioItem : mouseClickAudioItem);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnSubmit(eventData);
		if (interactable)
		{
			PlaySound(null, (mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseClickAudioItem : mouseClickAudioItem);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerDown(eventData);
		if (isHighlighted && interactable)
		{
			RectTransform rectTransform = ((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale);
			if (DOTween.IsTweening(rectTransform))
			{
				DOTween.Kill(rectTransform);
			}
			rectTransform.localScale = new Vector3((highlightScaleFactor + 1f) / 2f, (highlightScaleFactor + 1f) / 2f, 1f);
			PlaySound(mouseDownAudio, (mouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseDownAudioItem);
		}
		else if (!interactable)
		{
			PlaySound(null, (nonInteractableMouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.nonInteractableMouseDownAudioItem : nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (isHighlighted && interactable)
		{
			if (DOTween.IsTweening(gameObject))
			{
				DOTween.Kill(gameObject);
			}
			((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale).localScale = new Vector3(highlightScaleFactor, highlightScaleFactor, 1f);
			PlaySound(mouseUpAudio, (mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	private void PlaySound(AudioClip mouseAudio, string audioItem)
	{
		if (useAudioController)
		{
			if (!string.IsNullOrEmpty(audioItem) && AudioController.GetAudioItem(audioItem) != null)
			{
				AudioControllerUtils.PlaySound(audioItem);
			}
			else if (mouseAudio != null)
			{
				AudioControllerUtils.PlaySound(mouseAudio.name);
			}
		}
		else if ((bool)UIManager.Instance)
		{
			UIManager.Instance.PlayUISound(mouseAudio);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalize;
			I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalize;
		}
		if (restoreHighlightWhenReenable && wasHighlightedOnDisable && UIManager.GameObjectUnderPointer() == gameObject)
		{
			OnPointerEnter(new PointerEventData(EventSystem.current));
		}
		else if (currentSelectionState == SelectionState.Selected)
		{
			OnHighlight();
		}
		wasHighlightedOnDisable = false;
	}

	private void OnLocalize()
	{
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
		}
	}

	protected override void OnDisable()
	{
		bool flag = currentSelectionState == SelectionState.Selected;
		base.OnDisable();
		wasHighlightedOnDisable = isHighlighted;
		if (isHighlighted)
		{
			ToggleHighlight(active: false, playAnimation: false);
		}
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalize;
		}
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
			onDeselected.Invoke();
		}
		else if (flag)
		{
			if (interactable)
			{
				onMouseExit?.Invoke();
			}
			onDeselected.Invoke();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		onClick.RemoveAllListeners();
		onMouseEnter.RemoveAllListeners();
		onMouseExit.RemoveAllListeners();
		onSelected.RemoveAllListeners();
		onDeselected.RemoveAllListeners();
		buttonText = null;
		targetRect = null;
		canvasGroup = null;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		AutoTestController.LogButtonHover(gameObject, this);
		base.OnPointerEnter(eventData);
		OnHighlight();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		OnUnhighlight();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			OnHighlight();
			onSelected?.Invoke();
		}
	}

	public override Selectable FindSelectableOnUp()
	{
		if (navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		return base.FindSelectableOnUp();
	}

	public override Selectable FindSelectableOnLeft()
	{
		if (navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		return base.FindSelectableOnLeft();
	}

	public override Selectable FindSelectableOnRight()
	{
		if (navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		return base.FindSelectableOnRight();
	}

	public override Selectable FindSelectableOnDown()
	{
		if (navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		return base.FindSelectableOnDown();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnUnhighlight();
		onDeselected?.Invoke();
	}

	public void OnHighlight()
	{
		if (scaleNonInteractable || IsInteractable())
		{
			if (onMouseEnter != null)
			{
				onMouseEnter.Invoke();
			}
			PlaySound(mouseEnterAudio, (mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
			ToggleHighlight(active: true, animateScaling);
		}
	}

	public void OnUnhighlight()
	{
		wasHighlightedOnDisable = false;
		if (onMouseExit != null)
		{
			onMouseExit.Invoke();
		}
		PlaySound(mouseExitAudio, (mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		ToggleHighlight(active: false, animateScaling);
	}

	private void ToggleHighlight(bool active, bool playAnimation = true)
	{
		isHighlighted = active;
		if (active)
		{
			HighlightFinished(active: true);
		}
		RectTransform rectTransform = ((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale);
		Vector3 vector = (active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one);
		if (playAnimation)
		{
			if (DOTween.IsTweening(rectTransform))
			{
				DOTween.Kill(rectTransform);
			}
			TweenerCore<Vector3, Vector3, VectorOptions> t = rectTransform.DOScale(vector, animationDuration).SetEase(Ease.OutExpo).SetUpdate(isIndependentUpdate: true);
			if (!active)
			{
				t.OnComplete(() =>
				{
					HighlightFinished(active: false);
				});
			}
		}
		else
		{
			rectTransform.localScale = vector;
			if (!active)
			{
				HighlightFinished(active: false);
			}
		}
		if (!(hoverMovement == Vector3.zero) && active != isMoved)
		{
			RectTransform rectTransform2 = ((overridedTargetRectMovement == null) ? TargetRect : overridedTargetRectMovement);
			isMoved = active;
			if (active)
			{
				hoverStartPosition = rectTransform2.anchoredPosition;
			}
			Vector2 anchoredPosition = (active ? (hoverStartPosition + new Vector2(hoverMovement.x, hoverMovement.y)) : hoverStartPosition);
			rectTransform2.anchoredPosition = anchoredPosition;
		}
	}

	private void HighlightFinished(bool active)
	{
		if (ignoreScalingObjects == null || ignoreScalingObjects.Length == 0)
		{
			return;
		}
		Transform[] array = ignoreScalingObjects;
		foreach (Transform transform in array)
		{
			if (active)
			{
				UIManager.Instance.HighlightElement(transform.gameObject);
			}
			else
			{
				UIManager.Instance.UnhighlightElement(transform.gameObject);
			}
		}
	}

	public void SetFade(float fade)
	{
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = fade;
		}
	}

	public float GetFade()
	{
		if (!(CanvasGroup != null))
		{
			return 0f;
		}
		return CanvasGroup.alpha;
	}

	public void ClearSelectedState()
	{
		InstantClearState();
	}

	public void addToEnabledList(GameObject g)
	{
		g.SetActive(value: true);
	}
}
