using System;
using TMPro;

namespace UnityEngine.UI;

[Serializable]
public struct TMPColorGradientState : IEquatable<TMPColorGradientState>
{
	[SerializeField]
	private TMP_ColorGradient m_Normal;

	[SerializeField]
	private TMP_ColorGradient m_Highlighted;

	[SerializeField]
	private TMP_ColorGradient m_Pressed;

	[SerializeField]
	private TMP_ColorGradient m_Selected;

	[SerializeField]
	private TMP_ColorGradient m_Disabled;

	public TMP_ColorGradient Normal
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

	public TMP_ColorGradient Highlighted
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

	public TMP_ColorGradient pressed
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

	public TMP_ColorGradient selected
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

	public TMP_ColorGradient disabled
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

	public bool Equals(TMPColorGradientState other)
	{
		if (Highlighted == other.Highlighted && pressed == other.pressed && selected == other.selected && disabled == other.disabled)
		{
			return Normal == other.Normal;
		}
		return false;
	}
}
