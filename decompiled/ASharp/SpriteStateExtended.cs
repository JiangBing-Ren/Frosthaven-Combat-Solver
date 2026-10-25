using System;
using UnityEngine;

[Serializable]
public struct SpriteStateExtended
{
	[SerializeField]
	private Sprite m_HighlightedSprite;

	[SerializeField]
	private Sprite m_PressedSprite;

	[SerializeField]
	private Sprite m_SelectedSprite;

	[SerializeField]
	private Sprite m_ActiveSprite;

	[SerializeField]
	private Sprite m_ActiveHighlightedSprite;

	[SerializeField]
	private Sprite m_ActivePressedSprite;

	[SerializeField]
	private Sprite m_ActiveSelectedSprite;

	[SerializeField]
	private Sprite m_DisabledSprite;

	public Sprite highlightedSprite
	{
		get
		{
			return m_HighlightedSprite;
		}
		set
		{
			m_HighlightedSprite = value;
		}
	}

	public Sprite pressedSprite
	{
		get
		{
			return m_PressedSprite;
		}
		set
		{
			m_PressedSprite = value;
		}
	}

	public Sprite selectedSprite
	{
		get
		{
			return m_SelectedSprite;
		}
		set
		{
			m_SelectedSprite = value;
		}
	}

	public Sprite activeSprite
	{
		get
		{
			return m_ActiveSprite;
		}
		set
		{
			m_ActiveSprite = value;
		}
	}

	public Sprite activeHighlightedSprite
	{
		get
		{
			return m_ActiveHighlightedSprite;
		}
		set
		{
			m_ActiveHighlightedSprite = value;
		}
	}

	public Sprite activePressedSprite
	{
		get
		{
			return m_ActivePressedSprite;
		}
		set
		{
			m_ActivePressedSprite = value;
		}
	}

	public Sprite activeSelectedSprite
	{
		get
		{
			return m_ActiveSelectedSprite;
		}
		set
		{
			m_ActiveSelectedSprite = value;
		}
	}

	public Sprite disabledSprite
	{
		get
		{
			return m_DisabledSprite;
		}
		set
		{
			m_DisabledSprite = value;
		}
	}
}
