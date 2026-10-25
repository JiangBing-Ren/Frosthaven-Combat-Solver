using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class PlayerCountConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private int _targetPlayerCount;

	[SerializeField]
	private ComparisonOp _comparison;

	protected override bool CanShow(MonsterCardUIContext context)
	{
		return MonsterCardConditionalElement.Compare((ScenarioManager.Scenario?.InitialPlayers?.Count).GetValueOrDefault(), _targetPlayerCount, _comparison);
	}
}
