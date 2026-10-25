using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsAgility : CPlayerStatsBase, ISerializable
{
	public CPlayerActor.EAgility Agility;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Agility", Agility);
	}

	public CPlayerStatsAgility(SerializationInfo info, StreamingContext context)
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
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsHeal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsAgility(CPlayerActor.EAgility agility, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		Agility = agility;
	}
}
