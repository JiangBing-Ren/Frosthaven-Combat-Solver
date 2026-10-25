using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorWantsALootCardConfirmation_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorWantsALootCardConfirmation, actorSpawningMessage)
{
	public bool m_ShowHelpBox = true;

	public List<LootCard> m_LootCards;

	public string m_HelpBoxKey;
}
