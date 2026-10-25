using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class ExtendedToggle : Toggle
{
	public RectTransform targetRect;

	public float highlightScaleFactor = 1.1f;

	public bool scaleWhenSelected;

	public Transform[] ignoreScalingObjects;

	public bool animateScaling;

	public float animationDuration = 0.3f;

	public UnityEvent onMouseEnter;

	public UnityEvent onMouseExit;

	public UnityEvent onSelected;

	public UnityEvent onDeselected;

	public bool triggerNonInteractableHover;

	public bool useAudioController = true;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string nonInteractableMouseDownAudioItem;

	public AudioClip mouseDownAudio;

	public AudioClip mouseUpAudio;

	public AudioClip mouseEnterAudio;

	public AudioClip mouseExitAudio;

	public AudioButtonProfile audioProfile;

	public ButtonSwitch buttonSwitch;

	private bool isHighlighted;

	private bool isMouseOver;

	private Tween hoverAnimationTween;

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

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Log.GUIInfo("Toggle press for toggle " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (interactable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
		base.OnPointerClick(eventData);
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Log.GUIInfo("Toggle press for toggle " + gameObject.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			base.OnSubmit(eventData);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Log.GUIInfo("Toggle press for toggle " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerDown(eventData);
		if (isHighlighted && interactable)
		{
			if (DOTween.IsTweening(gameObject))
			{
				DOTween.Kill(gameObject);
			}
			TargetRect.localScale = new Vector3((highlightScaleFactor + 1f) / 2f, (highlightScaleFactor + 1f) / 2f, 1f);
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
			TargetRect.localScale = new Vector3(highlightScaleFactor, highlightScaleFactor, 1f);
			PlaySound(mouseUpAudio, (mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		onValueChanged.AddListener(OnValueChanged);
	}

	public new void SetIsOnWithoutNotify(bool value)
	{
		base.SetIsOnWithoutNotify(value);
		if (buttonSwitch != null)
		{
			buttonSwitch.Refresh(value);
		}
	}

	private void OnValueChanged(bool active)
	{
		if (!active && scaleWhenSelected && !isMouseOver)
		{
			ToggleHighlight(active: false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ToggleHighlight(active: false, playAnimation: false);
	}

	protected override void OnDisable()
	{
		bool flag = currentSelectionState == SelectionState.Selected;
		base.OnDisable();
		ToggleHighlight(active: false, playAnimation: false);
		if (flag)
		{
			onDeselected.Invoke();
		}
		if (isMouseOver && (interactable || triggerNonInteractableHover))
		{
			onMouseExit?.Invoke();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		onValueChanged.RemoveAllListeners();
		onMouseEnter.RemoveAllListeners();
		onMouseExit.RemoveAllListeners();
		onSelected.RemoveAllListeners();
		onDeselected.RemoveAllListeners();
		targetRect = null;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		isMouseOver = true;
		OnHighlight();
		AutoTestController.LogButtonHover(gameObject, this);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		isMouseOver = false;
		OnUnhighlight();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			OnHighlight();
			onSelected.Invoke();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnUnhighlight();
		onDeselected.Invoke();
	}

	private void OnHighlight()
	{
		if (interactable || triggerNonInteractableHover)
		{
			onMouseEnter?.Invoke();
		}
		PlaySound(mouseEnterAudio, (mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		if (!isOn || !scaleWhenSelected)
		{
			ToggleHighlight(active: true, animateScaling);
		}
	}

	private void OnUnhighlight()
	{
		if (interactable || triggerNonInteractableHover)
		{
			onMouseExit?.Invoke();
		}
		PlaySound(mouseExitAudio, (mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		if (!isOn || !scaleWhenSelected)
		{
			ToggleHighlight(active: false, animateScaling);
		}
	}

	private void ToggleHighlight(bool active, bool playAnimation = true)
	{
		isHighlighted = active;
		if (active)
		{
			HighlightFinished(active: true);
		}
		if (playAnimation)
		{
			if (DOTween.IsTweening(TargetRect))
			{
				DOTween.Kill(TargetRect);
			}
			hoverAnimationTween = TargetRect.DOScale(active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one, animationDuration).SetEase(Ease.OutExpo).SetUpdate(isIndependentUpdate: true);
			if (!active)
			{
				hoverAnimationTween.OnComplete(() =>
				{
					HighlightFinished(active: false);
				});
			}
		}
		else
		{
			TargetRect.localScale = (active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one);
			if (!active)
			{
				HighlightFinished(active: false);
			}
		}
	}

	private void HighlightFinished(bool active)
	{
		if (ignoreScalingObjects != null && ignoreScalingObjects.Length != 0)
		{
			Transform[] array = ignoreScalingObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetParent(active ? transform.parent : transform, worldPositionStays: true);
			}
		}
	}
}
