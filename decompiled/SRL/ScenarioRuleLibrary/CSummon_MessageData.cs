using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSummon_MessageData : CMessageData
{
	public CActor m_ActorSummoning;

	public CAbilitySummon m_SummonAbility;

	public List<CActor> m_SummonedActors;

	public List<CTile> m_SummonTiles;

	public bool m_SkipChoreographerWait;

	public CSummon_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Summon, actorSpawningMessage, animOverload)
	{
	}
}
