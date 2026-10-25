using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAudioEffects : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public AudioButtonProfile AudioProfile;

	public bool PlayWhenNoInteractable;

	[Header("Override Audio Profile")]
	public string PointerClickAudioItem;

	public string PointerDownAudioItem;

	public string PointerUpAudioItem;

	public string PointerEnterAudioItem;

	public string PointerExitAudioItem;

	public Func<bool> CanPlayClickSound;

	private Selectable _selectable;

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return PlayWhenNoInteractable;
			}
			return true;
		}
	}

	private bool _isClickable
	{
		get
		{
			if (CanPlayClickSound != null)
			{
				return CanPlayClickSound();
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_isClickable)
		{
			if (_isInteractable)
			{
				AudioControllerUtils.PlaySound((PointerDownAudioItem.IsNullOrEmpty() && AudioProfile != null) ? AudioProfile.mouseDownAudioItem : PointerDownAudioItem);
			}
			else if (AudioProfile != null)
			{
				AudioControllerUtils.PlaySound(AudioProfile.nonInteractableMouseDownAudioItem);
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_isClickable && _isInteractable)
		{
			AudioControllerUtils.PlaySound((PointerUpAudioItem.IsNullOrEmpty() && AudioProfile != null) ? AudioProfile.mouseUpAudioItem : PointerUpAudioItem);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			AudioControllerUtils.PlaySound((PointerEnterAudioItem.IsNullOrEmpty() && AudioProfile != null) ? AudioProfile.mouseEnterAudioItem : PointerEnterAudioItem);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			AudioControllerUtils.PlaySound((PointerExitAudioItem.IsNullOrEmpty() && AudioProfile != null) ? AudioProfile.mouseExitAudioItem : PointerExitAudioItem);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_isInteractable && _isClickable)
		{
			AudioControllerUtils.PlaySound((PointerClickAudioItem.IsNullOrEmpty() && AudioProfile != null) ? AudioProfile.mouseClickAudioItem : PointerClickAudioItem);
		}
	}
}
