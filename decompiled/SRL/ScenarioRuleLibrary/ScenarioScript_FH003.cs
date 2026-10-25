namespace ScenarioRuleLibrary;

public class ScenarioScript_FH003 : ScenarioScript
{
	private CScenarioModifier _specialRule1;

	private CObjectDoor _door2;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 26);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected override bool Initialize()
	{
		_specialRule1 = GetScenarioModifier("specialRule1");
		_door2 = GetProp(6, 5, "ThickDoor") as CObjectDoor;
		return true;
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRuleVisibility();
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateSpecialRuleVisibility();
	}

	private void UpdateSpecialRuleVisibility()
	{
		if (_door2.DoorIsOpen && _specialRule1.IsHidden)
		{
			_specialRule1.SetHiddenState(shouldSetHidden: false);
		}
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		string prefabName = actor.GetPrefabName();
		if (prefabName == "AlgoxPriest")
		{
			return "AlgoxPriestSnowspeaker";
		}
		if (prefabName == "AlgoxPriest Elite")
		{
			return "AlgoxPriestSnowspeaker Elite";
		}
		return null;
	}
}
