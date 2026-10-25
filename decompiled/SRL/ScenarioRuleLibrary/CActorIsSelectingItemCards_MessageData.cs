using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsSelectingItemCards_MessageData : CMessageData
{
	public List<CActor> m_ActorsRefreshed;

	public CAbility m_ItemSelectionAbility;

	public CActorIsSelectingItemCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingItemCards, actorSpawningMessage, animOverload)
	{
	}
}
