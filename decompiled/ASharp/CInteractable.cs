using UnityEngine;

public class CInteractable : MonoBehaviour, IHoverable
{
	public enum EInputType
	{
		None,
		Primary,
		Alternative
	}

	public virtual void ShowNormalInterface(bool disabled, EInputType inputType)
	{
	}

	public virtual void OnCursorEnter()
	{
	}

	public virtual void OnCursorExit()
	{
	}
}
