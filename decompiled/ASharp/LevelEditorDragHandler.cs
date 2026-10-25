using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEditorDragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public static GameObject itemBeingDragged;

	private Vector3 m_StartPosition;

	public void OnBeginDrag(PointerEventData eventData)
	{
		itemBeingDragged = gameObject;
		m_StartPosition = transform.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		transform.position = InputFHManager.CursorPosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		itemBeingDragged = null;
		transform.position = m_StartPosition;
	}
}
