using FHGUI.Scenario.States;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario;

public class PhaseViewLayer : FHScenarioViewLayer<FHScenarioPhaseViewState>
{
	public CPhase.PhaseType CurrentPhase => State?.PhaseType ?? CPhase.PhaseType.None;

	public override bool ProcessMessage(CMessageData messageData)
	{
		GetTargetPhaseParams(messageData, out var phaseType, out var operationType);
		if (CurrentPhase == phaseType || operationType == FHScenarioViewController.StateOperationType.None)
		{
			return operationType != FHScenarioViewController.StateOperationType.None;
		}
		Set(GenerateTargetPhaseState(phaseType), operationType);
		return true;
	}

	private void GetTargetPhaseParams(CMessageData messageData, out CPhase.PhaseType phaseType, out FHScenarioViewController.StateOperationType operationType)
	{
		switch (messageData.m_Type)
		{
		case CMessageData.MessageType.PlayerToSelectAbilityCardsOrLongRest:
			phaseType = CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.ReplicateStartRoundCardState:
			phaseType = CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			operationType = FHScenarioViewController.StateOperationType.Update;
			break;
		case CMessageData.MessageType.PlayersHaveSelectedAbilityCardsOrLongRest:
			phaseType = CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			operationType = FHScenarioViewController.StateOperationType.Hide;
			break;
		case CMessageData.MessageType.MonsterClassesToSelectAbilityCards:
			phaseType = CPhase.PhaseType.MonsterClassesSelectAbilityCards;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.MonsterClassesHaveSelectedAbilityCards:
			phaseType = CPhase.PhaseType.MonsterClassesSelectAbilityCards;
			operationType = FHScenarioViewController.StateOperationType.Hide;
			break;
		case CMessageData.MessageType.ActionSelectionPhaseStart:
			phaseType = CPhase.PhaseType.ActionSelection;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.InfuserMechanicSelectionPhaseStart:
			phaseType = CPhase.PhaseType.InfuserMechanicSelection;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.ActorIsSelectingMoveTile:
		case CMessageData.MessageType.ActorIsSelectingAttackFocusTargets:
		case CMessageData.MessageType.ActorIsSelectingTargetingFocus:
		case CMessageData.MessageType.ActorIsSelectingTeleport2Tile:
		case CMessageData.MessageType.ActorIsSelectingPullTile:
		case CMessageData.MessageType.ActorIsSelectingPushTile:
		case CMessageData.MessageType.ActorIsSelectingItemCards:
		case CMessageData.MessageType.ActorIsSelectingDamageFocus:
		case CMessageData.MessageType.ActorIsSelectingActiveBonusBuffTarget:
		case CMessageData.MessageType.FinishedProcessingTileSelected:
		case CMessageData.MessageType.FinishedProcessingTileDeselected:
		case CMessageData.MessageType.FinishedProcessingActiveBonusToggle:
		case CMessageData.MessageType.FinishedProcessingClearTargets:
		case CMessageData.MessageType.FinishedProcessingUndo:
		case CMessageData.MessageType.FinishedProcessingItemToggle:
			phaseType = CPhase.PhaseType.ActionSelection;
			operationType = FHScenarioViewController.StateOperationType.Update;
			break;
		case CMessageData.MessageType.ActionSelection:
			phaseType = CPhase.PhaseType.ActionSelection;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.InfuserMechanicSelection:
			phaseType = CPhase.PhaseType.InfuserMechanicSelection;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.EndAction:
			phaseType = CPhase.PhaseType.Action;
			operationType = FHScenarioViewController.StateOperationType.Hide;
			break;
		case CMessageData.MessageType.StartTurn:
			phaseType = CPhase.PhaseType.StartTurn;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.NextRound:
			phaseType = CPhase.PhaseType.StartRoundEffects;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.PlayersExhausted:
			phaseType = CPhase.PhaseType.PlayerExhausted;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		case CMessageData.MessageType.PlayerToShortRest:
			phaseType = CPhase.PhaseType.ShortRest;
			operationType = FHScenarioViewController.StateOperationType.Show;
			break;
		default:
			operationType = FHScenarioViewController.StateOperationType.None;
			phaseType = PhaseManager.PhaseType;
			break;
		}
	}

	private FHScenarioPhaseViewState GenerateTargetPhaseState(CPhase.PhaseType targetPhaseType)
	{
		return targetPhaseType switch
		{
			CPhase.PhaseType.SelectAbilityCardsOrLongRest => (FHScenarioPhaseViewState)new SelectAbilityCardsViewState(), 
			CPhase.PhaseType.ActionSelection => new ActionSelectionViewState(), 
			CPhase.PhaseType.InfuserMechanicSelection => new ActionSelectionViewState(), 
			CPhase.PhaseType.MonsterClassesSelectAbilityCards => new CharactersSelectedCardsViewState(), 
			_ => null, 
		};
	}
}
