using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class MonsterIDConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private string[] _monsterIDs;

	private bool CanShow(CMonsterClass monster)
	{
		if (monster == null)
		{
			return true;
		}
		return _monsterIDs.Contains(monster.ID);
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
