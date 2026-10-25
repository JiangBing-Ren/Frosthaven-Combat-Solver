using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Button Extended", 58)]
public class UIButtonExtended : Button
{
	public enum VisualState
	{
		Normal,
		Highlighted,
		Pressed,
		Disabled
	}

	[Serializable]
	public class StateChangeEvent : UnityEvent<VisualState, bool>
	{
	}

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public AudioButtonProfile audioProfile;

	public bool playAudioWhenNoInteractable;

	public StateChangeEvent onStateChange = new StateChangeEvent();

	protected override void OnDisable()
	{
		base.OnDisable();
		if (onStateChange != null)
		{
			onStateChange.Invoke(VisualState.Disabled, arg1: true);
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (onStateChange != null)
		{
			onStateChange.Invoke((VisualState)state, instant);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (interactable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
		base.OnPointerClick(eventData);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseDownAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		}
	}

	public void ClearSelectedState()
	{
		InstantClearState();
	}
}
