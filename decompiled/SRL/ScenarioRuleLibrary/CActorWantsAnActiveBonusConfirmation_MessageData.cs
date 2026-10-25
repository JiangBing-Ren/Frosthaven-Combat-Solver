using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorWantsAnActiveBonusConfirmation_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorWantsAnActiveBonusConfirmation, actorSpawningMessage)
{
	public CAbility m_Ability;

	public bool m_Confirm = true;

	public bool m_CanUndo = true;

	public bool m_CanSkip = true;

	public bool m_ContinueAbility;

	public bool m_ShowHelpBox = true;

	public bool m_ShowShadowBonuses;

	public CActor m_Target;

	public List<CActiveBonus> m_UsableBonuses;

	public string m_HelpTextKeyOverride;
}
