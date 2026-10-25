using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class ValrathMerchantRFS004FocusMovementConditionalElement : MonsterCardConditionalElement
{
	private enum WhenToShow
	{
		BeforeOpenDoor,
		BeforeReachMarko,
		AfterReachMarko
	}

	[SerializeField]
	private WhenToShow whenToShow;

	protected bool CanShow(CMonsterClass monster)
	{
		if (ScenarioScript.current is ScenarioScript_RFH004 scenarioScript_RFH)
		{
			if (!scenarioScript_RFH.door.DoorIsOpen)
			{
				return whenToShow == WhenToShow.BeforeOpenDoor;
			}
			if (!scenarioScript_RFH.ReachedMarko)
			{
				return whenToShow == WhenToShow.BeforeReachMarko;
			}
			return whenToShow == WhenToShow.AfterReachMarko;
		}
		return false;
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
