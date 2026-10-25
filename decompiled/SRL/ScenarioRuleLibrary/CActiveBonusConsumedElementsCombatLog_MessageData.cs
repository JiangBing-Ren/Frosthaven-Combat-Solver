using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActiveBonusConsumedElementsCombatLog_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public List<ElementInfusionBoardManager.EElement> m_ElementsConsumed;

	public CActiveBonusConsumedElementsCombatLog_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusConsumedElementsCombatLog, actorSpawningMessage)
	{
	}
}
