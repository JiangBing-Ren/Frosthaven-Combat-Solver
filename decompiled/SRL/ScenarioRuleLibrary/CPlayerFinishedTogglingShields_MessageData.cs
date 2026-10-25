using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayerFinishedTogglingShields_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public List<CItem> m_ToggledShields;

	public List<CActiveBonus> m_ToggledBonuses;

	public CPlayerFinishedTogglingShields_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerFinishedTogglingShields, actorSpawningMessage)
	{
	}
}
