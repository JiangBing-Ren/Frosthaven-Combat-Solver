using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActorIsHealing_MessageData : CMessageData
{
	public Dictionary<CActor, int> m_ActorsHealedAndHealStrength;

	public CAbilityHeal m_HealAbility;

	public AbilityData.EActiveBonusFlag m_bonusesFlags;

	public CActorIsHealing_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsHealing, actorSpawningMessage, animOverload)
	{
	}
}
