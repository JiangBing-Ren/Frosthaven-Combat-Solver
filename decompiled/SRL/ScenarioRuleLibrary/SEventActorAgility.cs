using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorAgility : SEventActor
{
	public CPlayerActor.EAgility Agility;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Agility", Agility);
	}

	public SEventActorAgility(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "Agility")
				{
					Agility = (CPlayerActor.EAgility)info.GetValue("Agility", typeof(CPlayerActor.EAgility));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorUsedItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorAgility(CPlayerActor.EAgility agility, SEventActorBaseData baseData)
		: base(baseData)
	{
		Agility = agility;
	}
}
