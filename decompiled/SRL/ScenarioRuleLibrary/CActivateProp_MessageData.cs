using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActivateProp_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public bool m_InitialLoad;

	public bool m_CarriedByActor;

	public CActor m_CreditActor;

	public List<Reward> m_Rewards;

	public SEventObjectProp m_PropEvent;

	public CActivateProp_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActivateProp, actorSpawningMessage)
	{
	}
}
