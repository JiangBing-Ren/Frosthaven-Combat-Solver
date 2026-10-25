using System;
using UnityEngine;

[Serializable]
public class AnimationTriggersExtended
{
	[SerializeField]
	private string m_NormalTrigger = "Normal";

	[SerializeField]
	private string m_HighlightedTrigger = "Highlighted";

	[SerializeField]
	private string m_PressedTrigger = "Pressed";

	[SerializeField]
	private string m_SelectedTrigger = "Highlighted";

	[SerializeField]
	private string m_ActiveTrigger = "Active";

	[SerializeField]
	private string m_ActiveHighlightedTrigger = "ActiveHighlighted";

	[SerializeField]
	private string m_ActivePressedTrigger = "ActivePressed";

	[SerializeField]
	private string m_ActiveSelectedTrigger = "ActiveHighlighted";

	[SerializeField]
	private string m_DisabledTrigger = "Disabled";

	public string normalTrigger
	{
		get
		{
			return m_NormalTrigger;
		}
		set
		{
			m_NormalTrigger = value;
		}
	}

	public string highlightedTrigger
	{
		get
		{
			return m_HighlightedTrigger;
		}
		set
		{
			m_HighlightedTrigger = value;
		}
	}

	public string pressedTrigger
	{
		get
		{
			return m_PressedTrigger;
		}
		set
		{
			m_PressedTrigger = value;
		}
	}

	public string selectedTrigger
	{
		get
		{
			return m_SelectedTrigger;
		}
		set
		{
			m_SelectedTrigger = value;
		}
	}

	public string activeTrigger
	{
		get
		{
			return m_ActiveTrigger;
		}
		set
		{
			m_ActiveTrigger = value;
		}
	}

	public string activeHighlightedTrigger
	{
		get
		{
			return m_ActiveHighlightedTrigger;
		}
		set
		{
			m_ActiveHighlightedTrigger = value;
		}
	}

	public string activePressedTrigger
	{
		get
		{
			return m_ActivePressedTrigger;
		}
		set
		{
			m_ActivePressedTrigger = value;
		}
	}

	public string activeSelectedTrigger
	{
		get
		{
			return m_ActiveSelectedTrigger;
		}
		set
		{
			m_ActiveSelectedTrigger = value;
		}
	}

	public string disabledTrigger
	{
		get
		{
			return m_DisabledTrigger;
		}
		set
		{
			m_DisabledTrigger = value;
		}
	}
}
