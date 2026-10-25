using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorRemovedCondition : SEventActor
{
	public CCondition.EPositiveCondition PositiveCondition { get; private set; }

	public CCondition.ENegativeCondition NegativeCondition { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PositiveCondition", PositiveCondition);
		info.AddValue("NegativeCondition", NegativeCondition);
	}

	public SEventActorRemovedCondition(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PositiveCondition"))
				{
					if (name == "NegativeCondition")
					{
						NegativeCondition = (CCondition.ENegativeCondition)info.GetValue("NegativeCondition", typeof(CCondition.ENegativeCondition));
					}
				}
				else
				{
					PositiveCondition = (CCondition.EPositiveCondition)info.GetValue("PositiveCondition", typeof(CCondition.EPositiveCondition));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorRemovedCondition entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorRemovedCondition(CCondition.EPositiveCondition positiveCondition, CCondition.ENegativeCondition negativeCondition, SEventActorBaseData baseData)
		: base(baseData)
	{
		PositiveCondition = positiveCondition;
		NegativeCondition = negativeCondition;
	}
}
