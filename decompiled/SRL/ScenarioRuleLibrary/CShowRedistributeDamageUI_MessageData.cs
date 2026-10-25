using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CShowRedistributeDamageUI_MessageData : CMessageData
{
	public List<CActor> m_ActorsToRedistributeBetween;

	public CAbilityRedistributeDamage m_RedistributeDamageAbility;

	public CShowRedistributeDamageUI_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowRedistributeDamageUI, actorSpawningMessage)
	{
	}
}
