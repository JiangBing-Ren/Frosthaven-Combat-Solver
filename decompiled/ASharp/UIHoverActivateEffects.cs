using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverActivateEffects : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum ActivateMode
	{
		EnableOnHover,
		DisableOnHover
	}

	[Tooltip("Select target to activate on mouse hover. Cannot select this object!")]
	public GameObject ActivatedTarget;

	public ActivateMode Mode;

	public bool ActivateOnSelected = true;

	public bool ActivateNonInteractable = true;

	private Selectable _selectable;

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return ActivateNonInteractable;
			}
			return true;
		}
	}

	private bool _hoveredActiveState => Mode == ActivateMode.EnableOnHover;

	private bool _nonHoverActiveState => Mode != ActivateMode.EnableOnHover;

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
		ActivatedTarget.SetActive(_nonHoverActiveState);
	}

	private void OnDisable()
	{
		ActivatedTarget.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			ActivatedTarget.SetActive(_hoveredActiveState);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!ActivateOnSelected || !(EventSystem.current?.currentSelectedGameObject == gameObject))
		{
			ActivatedTarget.SetActive(_nonHoverActiveState);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && ActivateOnSelected)
		{
			ActivatedTarget.SetActive(_hoveredActiveState);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (ActivateOnSelected)
		{
			ActivatedTarget.SetActive(_nonHoverActiveState);
		}
	}

	private void OnValidate()
	{
		if (ActivatedTarget == null)
		{
			Log.GUIError(name + " has UIHoverActivateEffects with no targets", this);
		}
		if (ActivatedTarget == gameObject)
		{
			Log.GUIError(name + " has UIHoverActivateEffects targeting itself. This is not allowed!", this);
		}
	}
}
