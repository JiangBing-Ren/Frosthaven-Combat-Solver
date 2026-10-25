using System;
using UnityEngine;

[Serializable]
public class BlockExtended<T>
{
	[SerializeField]
	private T m_Normal;

	[SerializeField]
	private T m_Highlighted;

	[SerializeField]
	private T m_Pressed;

	[SerializeField]
	private T m_Selected;

	[SerializeField]
	private T m_Active;

	[SerializeField]
	private T m_ActiveHighlighted;

	[SerializeField]
	private T m_ActivePressed;

	[SerializeField]
	private T m_ActiveSelected;

	[SerializeField]
	private T m_Disabled;

	public T normal
	{
		get
		{
			return m_Normal;
		}
		set
		{
			m_Normal = value;
		}
	}

	public T highlighted
	{
		get
		{
			return m_Highlighted;
		}
		set
		{
			m_Highlighted = value;
		}
	}

	public T pressed
	{
		get
		{
			return m_Pressed;
		}
		set
		{
			m_Pressed = value;
		}
	}

	public T selected
	{
		get
		{
			return m_Selected;
		}
		set
		{
			m_Selected = value;
		}
	}

	public T disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled = value;
		}
	}

	public T active
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_Active = value;
		}
	}

	public T activeHighlighted
	{
		get
		{
			return m_ActiveHighlighted;
		}
		set
		{
			m_ActiveHighlighted = value;
		}
	}

	public T activePressed
	{
		get
		{
			return m_ActivePressed;
		}
		set
		{
			m_ActivePressed = value;
		}
	}

	public T activeSelected
	{
		get
		{
			return m_ActiveSelected;
		}
		set
		{
			m_ActiveSelected = value;
		}
	}
}
