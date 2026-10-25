using System;
using System.Collections.Generic;
using FH.Tools;

namespace ScenarioRuleLibrary;

public class CPhase
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum PhaseType
	{
		PlayerExhausted,
		SelectAbilityCardsOrLongRest,
		MonsterClassesSelectAbilityCards,
		StartTurn,
		ActionSelection,
		Action,
		EndTurn,
		EndRound,
		Count,
		None,
		Autosave,
		StartRoundEffects,
		CheckForInitiativeAdjustments,
		CheckForForgoActionActiveBonuses,
		StartScenarioEffects,
		EndTurnLoot,
		AfterEndTurnLoot,
		ShortRest,
		InfuserMechanicSelection
	}

	protected PhaseType m_PhaseType;

	[StateDump(Skip = true)]
	protected bool _phaseEnded;

	public PhaseType Type => m_PhaseType;

	public void Update()
	{
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{
	}

	public void EndPhase()
	{
		if (!_phaseEnded)
		{
			_phaseEnded = true;
			SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseEnd, "", doNotSerialize: true));
			OnEndPhase();
		}
	}

	protected virtual void OnEndPhase()
	{
	}

	public void NextStep(bool passing = false)
	{
		Log.SimpleInfo("Current Phase of Type: " + Type.ToString() + " processing Next Step");
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseNextStep, "", doNotSerialize: true));
		OnNextStep(passing);
	}

	protected virtual void OnNextStep(bool passing = false)
	{
	}

	public void TileSelected(CTile tile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseTileSelected));
		OnTileSelected(tile, optionalTileList);
	}

	protected virtual void OnTileSelected(CTile tile, List<CTile> optionalTileList)
	{
		ScenarioRuleClient.MessageHandler(new CTileSelectionFinished_MessageData());
	}

	public void TileDeselected(CTile tile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseTileDeselected));
		OnTileDeselected(tile, optionalTileList);
	}

	protected virtual void OnTileDeselected(CTile tile, List<CTile> optionalTileList)
	{
	}

	public void ApplySingleTarget(CActor actor)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseApplySingleTarget));
		OnApplySingleTarget(actor);
	}

	protected virtual void OnApplySingleTarget(CActor actor)
	{
	}

	public void StepComplete(bool passingStep = false)
	{
		Log.SimpleInfo("Current Phase of Type: " + Type.ToString() + " processing Step Complete");
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(Type, ESESubTypePhase.PhaseStepComplete));
		OnStepComplete(passingStep);
	}

	protected virtual void OnStepComplete(bool passingStep = false)
	{
	}
}
