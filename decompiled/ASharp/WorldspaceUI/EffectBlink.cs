using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class EffectBlink : IEffectBlink
{
	[SerializeField]
	private Image m_EffectImage;

	[SerializeField]
	private Sprite[] m_EffectSprites;

	private int m_SpriteIndex;

	protected override void OnDisable()
	{
		base.OnDisable();
		m_EffectImage.sprite = m_EffectSprites[0];
	}

	protected override void ProcessBlink()
	{
		m_SpriteIndex = ((++m_SpriteIndex < m_EffectSprites.Length) ? m_SpriteIndex : 0);
		m_EffectImage.sprite = m_EffectSprites[m_SpriteIndex];
	}
}
