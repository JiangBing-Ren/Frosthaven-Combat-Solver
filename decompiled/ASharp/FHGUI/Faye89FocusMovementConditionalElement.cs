using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class Faye89FocusMovementConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private bool _showWhenBossRevealed;

	protected bool CanShow(CMonsterClass monster)
	{
		EnemyState enemyState = ScenarioManager.CurrentScenarioState?.Monsters?.SingleOrDefault((EnemyState actor) => actor.IsBoss);
		return _showWhenBossRevealed == (enemyState?.IsRevealed ?? false);
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
