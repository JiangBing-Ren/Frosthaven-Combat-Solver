using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CPhaseCheckForInitiativeAdjustments : CPhase
{
	private CActor actor;

	private bool tileHighlightsToggled;

	public CPhaseCheckForInitiativeAdjustments()
	{
		m_PhaseType = PhaseType.CheckForInitiativeAdjustments;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (GameState.InternalCurrentActor == null || GameState.InternalCurrentActor.IsDead)
		{
			GameState.NextPhase();
			return;
		}
		bool flag = false;
		actor = GameState.InternalCurrentActor;
		foreach (CActiveBonus item in GameState.InternalCurrentActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative))
		{
			if (item.Ability.ActiveBonusData.IsToggleBonus && item.RequirementsMet(actor))
			{
				flag = true;
				break;
			}
		}
		if (!GameState.ShowItemSelection(actor, actor, null, CItem.EItemTrigger.BeforeInitiativeCalc, onlyInActionPhase: false))
		{
			if (flag)
			{
				tileHighlightsToggled = true;
				ScenarioManager.ToggleTileHighlights(new List<Point> { actor.ArrayIndex }, enabled: true, new CVector4(0f, 0f, 1f, 1f));
				CCheckForInitiativeAdjustments_MessageData message = new CCheckForInitiativeAdjustments_MessageData(GameState.InternalCurrentActor);
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				GameState.NextPhase();
			}
		}
	}

	protected override void OnEndPhase()
	{
		if (tileHighlightsToggled && actor != null)
		{
			tileHighlightsToggled = false;
			ScenarioManager.ToggleTileHighlights(new List<Point> { actor.ArrayIndex }, enabled: false, new CVector4(0f, 0f, 1f, 1f));
		}
		base.OnEndPhase();
	}
}
