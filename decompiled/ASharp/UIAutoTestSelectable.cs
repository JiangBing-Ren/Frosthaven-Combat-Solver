using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAutoTestSelectable : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public string AutoTestSelectableID;

	private Selectable _selectable;

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null))
			{
				return _selectable.IsInteractable();
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

	private void OnValidate()
	{
		if (string.IsNullOrWhiteSpace(AutoTestSelectableID))
		{
			Log.GUIError(name + " has empty AutoTestSelectableID!", this);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
	}
}
