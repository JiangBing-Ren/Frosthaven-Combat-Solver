using DG.Tweening;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

public class FHScenarioMarker : MonoBehaviour
{
	public enum HighlightType
	{
		None,
		Normal,
		Travel
	}

	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	public CanvasGroup _nameContainer;

	[SerializeField]
	private GameObject _highlightEffectNormal;

	[SerializeField]
	private GameObject _highlightEffectTravel;

	private Tween _runningAnimation;

	[field: SerializeField]
	public uint ScenarioID { get; private set; }

	public void Init(ScenarioState scenario)
	{
		_name.SetTextKey($"FS_{scenario.Def.ID}_NAME");
		_icon.sprite = UIInfoTools.Instance.GetQuestChainIcon(scenario.Def.Chain);
		_nameContainer.alpha = 0f;
		gameObject.SetActive(value: true);
		_highlightEffectNormal.gameObject.SetActive(value: false);
		_highlightEffectTravel.gameObject.SetActive(value: false);
	}

	public void Focus(HighlightType type)
	{
		_highlightEffectNormal.gameObject.SetActive(type == HighlightType.Normal);
		_highlightEffectTravel.gameObject.SetActive(type == HighlightType.Travel);
		_runningAnimation?.Kill();
		_runningAnimation = _nameContainer.DOFade(1f, 0.4f).SetEase(Ease.OutExpo);
	}

	public void Unfocus()
	{
		_highlightEffectNormal.gameObject.SetActive(value: false);
		_highlightEffectTravel.gameObject.SetActive(value: false);
		_runningAnimation?.Kill();
		_runningAnimation = _nameContainer.DOFade(0f, 0.2f).SetEase(Ease.OutExpo);
	}
}
