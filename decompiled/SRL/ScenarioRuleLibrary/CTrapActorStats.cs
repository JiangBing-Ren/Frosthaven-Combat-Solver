using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CTrapActorStats : CPlayerStatsBase, ISerializable
{
	public int HealAmount { get; private set; }

	public string ObjectGuid { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HealAmount", HealAmount);
		info.AddValue("ObjectGuid", ObjectGuid);
	}

	public CTrapActorStats(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "HealAmount"))
				{
					if (name == "ObjectGuid")
					{
						ObjectGuid = info.GetString("ObjectGuid");
					}
				}
				else
				{
					HealAmount = info.GetInt32("HealAmount");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsTrapWithHealXExists entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CTrapActorStats(int healAmount, string objectGuid, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		HealAmount = healAmount;
		ObjectGuid = objectGuid;
	}

	public static CTrapActorStats CreateTrapStat(SEventTrapActor sevent, string advGuid, string sceGuid, string currentQuestType)
	{
		return new CTrapActorStats(sevent.HealAmount, sevent.ObjectGuid, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder));
	}
}
