using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINonInteractableEvents : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Button.ButtonClickedEvent NonInteractableClick = new Button.ButtonClickedEvent();

	private Selectable _selectable;

	private void Awake()
	{
		_selectable = GetComponent<Selectable>();
		if (_selectable == null)
		{
			Debug.LogError("\"" + name + "\" doesn't have selectable to listen for.", this);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && (bool)_selectable && !_selectable.IsInteractable())
		{
			NonInteractableClick.Invoke();
		}
	}
}
