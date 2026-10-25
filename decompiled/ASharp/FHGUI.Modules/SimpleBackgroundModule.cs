using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class SimpleBackgroundModule : FHMapViewModule
{
	[SerializeField]
	private Image m_DefaultBackground;

	[SerializeField]
	private Image m_OverrideBackground;

	[SerializeField]
	private Image m_SolidBackground;

	protected override bool PlaySoundOnEnable => false;

	public void SetEnabled(bool isEnabled, Sprite overrideSprite = null)
	{
		m_DefaultBackground.enabled = isEnabled && overrideSprite == null;
		m_OverrideBackground.enabled = isEnabled && overrideSprite != null;
		m_SolidBackground.enabled = isEnabled;
		if (isEnabled && overrideSprite != null)
		{
			m_OverrideBackground.sprite = overrideSprite;
		}
	}
}
