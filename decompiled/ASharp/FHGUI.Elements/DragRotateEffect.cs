using UnityEngine;
using UnityEngine.EventSystems;

namespace FHGUI.Elements;

public class DragRotateEffect : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	[SerializeField]
	private float _speed = 10f;

	private bool _isRotating;

	private Vector2 _prevPos;

	public void OnBeginDrag(PointerEventData eventData)
	{
		_isRotating = true;
		_prevPos = InputFHManager.CursorPosition;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (_isRotating && Application.isFocused)
		{
			Vector2 vector = _prevPos - eventData.position;
			_prevPos = eventData.position;
			transform.Rotate(Vector3.up, vector.x * _speed * Time.deltaTime);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_isRotating = false;
	}
}
