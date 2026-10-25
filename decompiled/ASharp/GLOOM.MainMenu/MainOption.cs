using UnityEngine;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIMainMenuOption))]
public class MainOption : MonoBehaviour
{
	public UIMainMenuOption Button { get; private set; }

	protected virtual void Awake()
	{
		Button = GetComponent<UIMainMenuOption>();
	}

	public virtual void Select()
	{
	}

	public virtual void Deselect()
	{
		Button.Deselect();
	}

	public void SetFocused(bool isFocused)
	{
		Button.SetFocused(isFocused);
	}
}
