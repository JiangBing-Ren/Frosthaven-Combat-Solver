using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityCardDefaultAction : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	private Action _onPointerEnter;

	private Action _onPointerExit;

	private Action _onSelect;

	private Action _onDeselect;

	public void SetUIEvents(Action onPointerEnter, Action onPointerExit, Action onSelect, Action onDeselect)
	{
		_onPointerEnter = onPointerEnter;
		_onPointerExit = onPointerExit;
		_onSelect = onSelect;
		_onDeselect = onDeselect;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_onPointerEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onPointerExit?.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		_onSelect?.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_onDeselect?.Invoke();
	}
}
