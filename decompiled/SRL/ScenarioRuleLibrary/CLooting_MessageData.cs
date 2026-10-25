using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CLooting_MessageData(string animOverload, CActor actorSpawningMessage) : CMessageData(MessageType.Looting, actorSpawningMessage, animOverload)
{
	public CAbilityLoot m_LootAbility;

	public CActor m_ActorLooting;

	public List<CObjectProp> m_PropsLooted;

	public int m_ExtraGoldFromAdditionalEffects;

	public bool m_FocusCamera;

	public bool m_OutOfPhase;

	public bool m_SyncMultiplayer = true;
}
