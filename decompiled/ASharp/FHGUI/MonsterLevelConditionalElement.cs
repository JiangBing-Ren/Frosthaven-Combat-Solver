using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class MonsterLevelConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private int _level;

	[SerializeField]
	private ComparisonOp _comparison = ComparisonOp.Equal;

	private bool CanShow(int level)
	{
		return MonsterCardConditionalElement.Compare(level, _level, _comparison);
	}

	private bool CanShow(CEnemyActor enemy)
	{
		return CanShow((enemy?.MonsterClass?.StatLevel).GetValueOrDefault());
	}

	protected override bool CanShow(MonsterCardUIContext context)
	{
		if (context.EnemyForBaseStats != null)
		{
			return CanShow(context.EnemyForBaseStats);
		}
		if (context.OverrideLevel.HasValue)
		{
			return CanShow(context.OverrideLevel.Value);
		}
		return CanShow(context.MonsterClass?.StatLevel ?? context.FallbackMonsterClass.StatLevel);
	}
}
