using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActorWantsAnAttackModifierConfirmation_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorWantsAnAttackModifierConfirmation, actorSpawningMessage)
{
	public CAbility m_Ability;

	public bool m_Confirm = true;

	public bool m_CanUndo = true;

	public bool m_CanSkip = true;

	public bool m_ContinueAbility;

	public bool m_ShowHelpBox = true;

	public AttackModifierCard m_Trigger;
}
