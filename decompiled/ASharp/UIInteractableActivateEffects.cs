using UnityEngine;
using UnityEngine.UI;

public class UIInteractableActivateEffects : MonoBehaviour
{
	public enum ActivateMode
	{
		EnableOnInteractable,
		DisableOnInteractable
	}

	[Tooltip("Select target to activate when selectable is interactable (or not). Cannot select this object!")]
	public GameObject ActivatedTarget;

	public ActivateMode Mode;

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

	private bool _interactableActiveState => Mode == ActivateMode.EnableOnInteractable;

	private bool _nonInteractableActiveState => Mode != ActivateMode.EnableOnInteractable;

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
		ActivatedTarget.SetActive(_nonInteractableActiveState);
	}

	private void OnDisable()
	{
		ActivatedTarget.SetActive(value: false);
	}

	private void Update()
	{
		if (_selectable == null)
		{
			enabled = false;
		}
		else
		{
			ActivatedTarget.SetActive(_isInteractable ? _interactableActiveState : _nonInteractableActiveState);
		}
	}

	private void OnValidate()
	{
		if (GetComponent<Selectable>() == null)
		{
			Log.GUIError(name + " has UIInteractableActivateEffects with no selectable.", this);
		}
		if (ActivatedTarget == null)
		{
			Log.GUIError(name + " has UIInteractableActivateEffects with no targets", this);
		}
		if (ActivatedTarget == gameObject)
		{
			Log.GUIError(name + " has UIInteractableActivateEffects targeting itself. This is not allowed!", this);
		}
	}
}
