using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CStackRewardsToShow_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public List<Reward> m_Rewards;

	public string m_SourceType;

	public CStackRewardsToShow_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StackRewardsToShow, actorSpawningMessage)
	{
	}
}
