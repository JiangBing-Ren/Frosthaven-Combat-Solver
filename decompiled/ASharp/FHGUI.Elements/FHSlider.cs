using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHSlider : Slider, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
{
	public Action OnBeginDragEvent;

	public Action OnEndDragEvent;

	public void OnBeginDrag(PointerEventData eventData)
	{
		OnBeginDragEvent?.Invoke();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		OnEndDragEvent?.Invoke();
	}
}
