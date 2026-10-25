using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorSelectingObjectPosition_MessageData : CMessageData
{
	public List<CTile> m_TilesSelected;

	public CActorSelectingObjectPosition_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorSelectObjectPosition, actorSpawningMessage)
	{
	}
}
