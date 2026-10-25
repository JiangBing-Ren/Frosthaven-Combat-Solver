using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActivateOrDeactivateSpawner_MessageData : CMessageData
{
	public CActor m_ActorDeactivatingSpawner;

	public List<CTile> m_Tiles;

	public CAbility m_ActivateOrDeactivateSpawnerAbility;

	public CActivateOrDeactivateSpawner_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActivateOrDeactivateSpawner, actorSpawningMessage, animOverload)
	{
	}
}
