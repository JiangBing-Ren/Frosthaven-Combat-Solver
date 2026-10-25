using System.Collections;
using FHGUI.Modules;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FHGUI.Common.Elements;

public class FHInteractableElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	private UISimpleInteractionEvents _interactionEvents = new UISimpleInteractionEvents();

	private Coroutine _onClickCrt;

	public void Init(UISimpleInteractionEvents interactionEvents)
	{
		_interactionEvents = interactionEvents;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_interactionEvents?.OnHover?.Invoke(this, isFlagRaised: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_interactionEvents?.OnHover?.Invoke(this, isFlagRaised: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_interactionEvents?.OnClick?.Invoke(this);
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (_interactionEvents?.OnDoubleClick == null)
			{
				OnClick();
			}
			else if (eventData.clickCount == 1)
			{
				_onClickCrt = StartCoroutine(OnClick_Crt());
			}
			else
			{
				OnDoubleClick();
			}
		}
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			OnRightClick();
		}
	}

	private void CancelClick()
	{
		if (_onClickCrt != null)
		{
			StopCoroutine(_onClickCrt);
			_onClickCrt = null;
		}
	}

	private IEnumerator OnClick_Crt()
	{
		yield return new WaitForSeconds(0.2f);
		OnClick();
	}

	public void OnClick()
	{
		_interactionEvents?.OnClick?.Invoke(this);
	}

	public void OnDoubleClick()
	{
		CancelClick();
		_interactionEvents?.OnDoubleClick?.Invoke(this);
	}

	public void OnRightClick()
	{
		_interactionEvents?.OnRightClick?.Invoke(this);
	}
}
