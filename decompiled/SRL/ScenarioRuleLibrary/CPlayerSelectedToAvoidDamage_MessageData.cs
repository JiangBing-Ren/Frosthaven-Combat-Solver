using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayerSelectedToAvoidDamage_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public GameState.EAvoidDamageOption m_AvoidDamageOption;

	public List<CAbilityCard> m_CardsBurnedToAvoidDamage;

	public List<CActiveBonus> m_UsedPreventDamageBonusesToAvoidDamage;

	public CPlayerSelectedToAvoidDamage_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectedToAvoidDamage, actorSpawningMessage)
	{
	}
}
