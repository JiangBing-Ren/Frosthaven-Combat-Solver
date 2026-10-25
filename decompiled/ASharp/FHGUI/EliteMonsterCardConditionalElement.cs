using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class EliteMonsterCardConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private bool _showWhenElite;

	private bool CanShow(CMonsterClass monster)
	{
		bool flag = monster?.NonEliteVariant != null;
		return _showWhenElite == flag;
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
		return CanShow(context.MonsterClass ?? context.FallbackMonsterClass);
	}
}
