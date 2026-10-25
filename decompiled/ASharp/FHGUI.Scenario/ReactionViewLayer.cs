using FHGUI.Scenario.States;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario;

public class ReactionViewLayer : FHScenarioViewLayer<FHScenarioReactionViewState>
{
	public override bool ProcessMessage(CMessageData messageData)
	{
		GetTargetReactionParams(messageData, out var state, out var operationType);
		if (operationType == FHScenarioViewController.StateOperationType.None)
		{
			return false;
		}
		Set(state, operationType);
		return true;
	}

	private void GetTargetReactionParams(CMessageData messageData, out FHScenarioReactionViewState state, out FHScenarioViewController.StateOperationType operationType)
	{
		state = null;
		operationType = (FHScenarioViewController.IsBackendWaitingForReaction() ? FHScenarioViewController.StateOperationType.Show : FHScenarioViewController.StateOperationType.Clear);
		switch (messageData.m_Type)
		{
		case CMessageData.MessageType.PlayerSelectingToAvoidDamageOrNot:
		case CMessageData.MessageType.PlayerSelectingToToggleShields:
			state = new AvoidDamageViewState();
			break;
		case CMessageData.MessageType.PlayerToShortRest:
		case CMessageData.MessageType.PlayerFinishedSelectingToAvoidDamageOrNot:
			break;
		default:
			operationType = FHScenarioViewController.StateOperationType.None;
			break;
		}
	}
}
