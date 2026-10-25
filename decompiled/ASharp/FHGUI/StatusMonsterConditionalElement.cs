using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class StatusMonsterConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private CCondition.ENegativeCondition _condition;

	private bool CanShow(CMonsterClass monster, int? overrideLevel = null)
	{
		return ((!overrideLevel.HasValue) ? monster?.Conditions?.Contains(_condition) : monster?.GetMonsterStatsData(overrideLevel.Value).Conditions.Contains(_condition)) == true;
	}

	private bool CanShow(CEnemyActor enemy)
	{
		return CanShow(enemy?.MonsterClass);
	}

	protected override bool CanShow(MonsterCardUIContext context)
	{
		if (context.EnemyForBaseStats != null)
		{
			return CanShow(context.EnemyForBaseStats);
		}
		return CanShow(context.MonsterClass ?? context.FallbackMonsterClass, context.OverrideLevel);
	}
}
