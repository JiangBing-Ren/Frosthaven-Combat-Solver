namespace ScenarioRuleLibrary;

public class CActorWantsAnActionConfirmation_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorWantsAnActionConfirmation, actorSpawningMessage)
{
	public CAbility m_Ability;

	public bool m_Confirm = true;

	public bool m_CanUndo = true;

	public bool m_CanSkip = true;

	public bool m_ContinueAbility;

	public bool m_ShowHelpBox = true;

	public bool m_ShowShadowBonuses;

	public bool m_HideHand = true;

	public string m_CustomHelpBoxTextLocKey;

	public CActor m_Target;

	public CItem.EItemTrigger m_Trigger;

	public CVarsContext m_Context;
}
