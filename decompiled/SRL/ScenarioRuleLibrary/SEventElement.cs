using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventElement : SEvent
{
	public ESESubTypeElement ElementSubType { get; private set; }

	public ElementInfusionBoardManager.EElement Element { get; private set; }

	public string ActorClassID { get; private set; }

	public CActor.EType ActorType { get; private set; }

	public bool ActorEnemySummon { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ElementSubType", ElementSubType);
		info.AddValue("Element", Element);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("ActorType", ActorType);
		info.AddValue("ActorEnemySummon", ActorEnemySummon);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public SEventElement(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ElementSubType":
					ElementSubType = (ESESubTypeElement)info.GetValue("ElementSubType", typeof(ESESubTypeElement));
					break;
				case "Element":
					Element = (ElementInfusionBoardManager.EElement)info.GetValue("Element", typeof(ElementInfusionBoardManager.EElement));
					break;
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "ActorType":
					ActorType = (CActor.EType)info.GetValue("ActorType", typeof(CActor.EType));
					break;
				case "ActorEnemySummon":
					ActorEnemySummon = info.GetBoolean("ActorEnemySummon");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActorClass":
				{
					string text = info.GetString("ActorClass").Replace(" ", string.Empty);
					ActorClassID = text + "ID";
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventElement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventElement(ESESubTypeElement elementSubType, ElementInfusionBoardManager.EElement element, string actorClassID, CActor.EType actorType, bool IsSummon, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, string text = "")
		: base(ESEType.Element, text)
	{
		ElementSubType = elementSubType;
		Element = element;
		ActorClassID = actorClassID;
		ActorType = actorType;
		ActorEnemySummon = IsSummon;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}
}
