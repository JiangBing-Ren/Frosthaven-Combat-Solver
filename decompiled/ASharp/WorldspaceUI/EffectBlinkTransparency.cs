using UnityEngine;

namespace WorldspaceUI;

public class EffectBlinkTransparency : IEffectBlink
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_OffAlpha;

	private bool isOff;

	protected override void OnEnable()
	{
		isOff = false;
		m_canvasGroup.alpha = 1f;
		base.OnEnable();
	}

	protected override void ProcessBlink()
	{
		isOff = !isOff;
		m_canvasGroup.alpha = (isOff ? m_OffAlpha : 1f);
	}
}
