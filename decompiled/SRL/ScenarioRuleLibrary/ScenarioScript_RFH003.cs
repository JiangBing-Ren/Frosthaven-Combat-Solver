using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH003 : ScenarioScript
{
	public int _tutStep;

	public CObjective_Scripted SelectCardsObjective;

	public CObjective_ReachRound CompleteFirstRoundObjective;

	public CObjective_ReachRound CompleteSecondRoundObjective;

	protected override bool Initialize()
	{
		SelectCardsObjective = GetObjective<CObjective_Scripted>("SelectCards");
		SelectCardsObjective.Total = 2;
		SelectCardsObjective.HideOnComplete = true;
		CompleteFirstRoundObjective = GetObjective<CObjective_ReachRound>("CompleteFirstRound");
		CompleteSecondRoundObjective = GetObjective<CObjective_ReachRound>("CompleteSecondRound");
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		return _tutStep.ToString();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			_tutStep = int.Parse(storage);
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (_tickActor is CEnemyActor focusActor)
			{
				CPlayerActor focusTarget = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.Class.ID == "DrifterID");
				CustomAbility.SetFocusOverride(focusActor, focusTarget);
			}
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			if (_tickActor is CPlayerActor cPlayerActor && cPlayerActor.Class.ID == "DrifterID" && ScenarioManager.CurrentScenarioState.RoundNumber == 1)
			{
				cPlayerActor.StartLooting();
				cPlayerActor.TryLootItem(1031);
				cPlayerActor.Tile.FindPropOfTypes(EPropType.LootToken)?.Activate(cPlayerActor);
				cPlayerActor.ShowLastLootReward("", "Custom", null, focusCamera: false, outOfPhase: true);
			}
			break;
		}
	}
}
