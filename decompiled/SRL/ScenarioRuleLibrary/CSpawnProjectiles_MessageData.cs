using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSpawnProjectiles_MessageData : CMessageData
{
	public string projectileSettingsName;

	public List<CObjectProp> m_FromProps;

	public CActor m_TargetActor;

	public CSpawnProjectiles_MessageData()
		: base(MessageType.SpawnProjectiles, null)
	{
	}
}
