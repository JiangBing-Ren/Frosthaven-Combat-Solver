using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CShowNextMonsterClassAbilityCards_MessageData : CMessageData
{
	public List<CMonsterClass> m_MonsterClasses;

	public bool m_OfferPutBottomOfDeck;

	public string m_AltHelpKey;

	public CShowNextMonsterClassAbilityCards_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowNextMonsterClassAbilityCards, actorSpawningMessage)
	{
	}
}
