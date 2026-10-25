using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CShowChooseAbilityUI_MessageData : CMessageData
{
	public CAbility m_ChooseAbility;

	public List<CAbility> m_AbilitiesToChooseFrom;

	public List<CAbility> m_AbilitiesAlreadyChosen;

	public CBaseCard m_BaseCard;

	public CShowChooseAbilityUI_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowChooseAbilityUI, actorSpawningMessage)
	{
	}
}
