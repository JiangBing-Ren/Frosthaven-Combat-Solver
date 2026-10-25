using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CShowChooseAttackModifierAdvantage_MessageData : CMessageData
{
	public List<AttackModifierYMLData> m_AttackModifiers;

	public AttackModifierYMLData m_AutoSelect;

	public bool m_ForceSelect;

	public CShowChooseAttackModifierAdvantage_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowChooseAttackModifierAdvantage, actorSpawningMessage, string.Empty)
	{
	}
}
