using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackedButton : Button
{
	[SerializeField]
	private PointerEventData.InputButton buttonKey;

	[SerializeField]
	private AudioButtonProfile audioProfile;

	[SerializeField]
	private string mouseClickAudioItem;

	[SerializeField]
	private string mouseEnterAudioItem;

	[SerializeField]
	private bool playAudioWhenNoInteractable;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		onClick.RemoveAllListeners();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (interactable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
		if (buttonKey != PointerEventData.InputButton.Left && buttonKey == eventData.button)
		{
			if (!IsActive() || !IsInteractable())
			{
				return;
			}
			onClick.Invoke();
		}
		else if (buttonKey == PointerEventData.InputButton.Left && buttonKey == eventData.button)
		{
			base.OnPointerClick(eventData);
		}
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseClickAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnSubmit(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseClickAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
		AutoTestController.LogButtonHover(gameObject, this);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Singleton<InputFHManager>.Instance.UsingGamepad && (IsInteractable() || playAudioWhenNoInteractable))
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
	}

	public void ClearSelectedState()
	{
		InstantClearState();
	}
}
