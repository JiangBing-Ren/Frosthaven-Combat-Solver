using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayersExhausted_MessageData : CMessageData
{
	public List<CPlayerActor> m_Players;

	public CPlayersExhausted_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayersExhausted, actorSpawningMessage)
	{
	}
}
