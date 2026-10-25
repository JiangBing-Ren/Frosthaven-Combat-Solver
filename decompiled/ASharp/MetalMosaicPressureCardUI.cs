using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class MetalMosaicPressureCardUI : MonoBehaviour
{
	[SerializeField]
	private Image[] _images;

	[SerializeField]
	private Image _pressureGauge;

	[SerializeField]
	private Color _lowPressureColor;

	[SerializeField]
	private Color _regularPressureColor;

	[SerializeField]
	private Color _highPressureColor;

	[SerializeField]
	private Color _overPressureColor;

	[SerializeField]
	private Sprite _lowPressureSprite;

	[SerializeField]
	private Sprite _regularPressureSprite;

	[SerializeField]
	private Sprite _highPressureSprite;

	[SerializeField]
	private Sprite _overPressureSprite;

	[HideInInspector]
	public CPlayerActor.EPressure pressure;
}
