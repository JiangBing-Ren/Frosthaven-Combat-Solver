using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectRefreshOrConsumeItems_MessageData : CMessageData
{
	public CActor m_ActorBeenRefreshed;

	public CAbility m_Ability;

	public List<CItem.EItemSlot> m_SlotsToChooseFrom;

	public List<CItem.EItemSlotState> m_SlotStatesToChooseFrom;

	public List<CItem> m_SpecificItems;

	public CSelectRefreshOrConsumeItems_MessageData(CActor actorSpawningMessage, CActor targetActor)
		: base(MessageType.SelectRefreshOrConsumeItems, actorSpawningMessage)
	{
		m_ActorBeenRefreshed = targetActor;
	}
}
