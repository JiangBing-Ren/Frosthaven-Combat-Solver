using System.Collections.Generic;
using UnityEngine;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIMainMenuOption))]
public abstract class MainOptionOpenSuboptions : MonoBehaviour
{
	private UIMainMenuOption m_Button;

	public UIMainMenuOption Button
	{
		get
		{
			if (m_Button == null)
			{
				m_Button = GetComponent<UIMainMenuOption>();
			}
			return m_Button;
		}
	}

	public abstract List<MenuSuboption> BuildOptions();

	protected virtual void Awake()
	{
		if (m_Button == null)
		{
			m_Button = GetComponent<UIMainMenuOption>();
		}
	}

	public void SetFocused(bool isFocused)
	{
		Button.SetFocused(isFocused);
	}

	public void Select()
	{
		Button.Select();
	}
}
