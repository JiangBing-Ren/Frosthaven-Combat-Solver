using FHGUI.Common.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIBaseStatsView : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _health;

	[SerializeField]
	private TMP_Text _movement;

	[SerializeField]
	private TMP_Text _attack;

	[SerializeField]
	private TMP_Text _range;

	[SerializeField]
	private RectTransform _rangeContainer;

	public void Init(BaseStatsDTO stats)
	{
		_health.text = stats.Health.ToString();
		_attack.text = ((stats.Attack > 0 || stats.HasAttackStatIsBasedOnX) ? stats.Attack.ToString() : "-");
		_movement.text = ((stats.Movement > 0 || stats.HasMovementStatIsBasedOnX) ? stats.Movement.ToString() : "-");
		_range.text = stats.Range.ToStringWithInfinity();
		_rangeContainer.gameObject.SetActive(stats.ShowRange);
		_movement.color = new Color(_movement.color.r, _movement.color.g, _movement.color.b, (stats.Movement == 0) ? 0.5f : 1f);
		_attack.color = new Color(_attack.color.r, _attack.color.g, _attack.color.b, (stats.Attack == 0) ? 0.5f : 1f);
		_range.color = new Color(_range.color.r, _range.color.g, _range.color.b, (stats.Range <= 1) ? 0.5f : 1f);
	}
}
