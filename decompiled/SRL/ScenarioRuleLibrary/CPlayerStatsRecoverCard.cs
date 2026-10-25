using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsRecoverCard : CPlayerStatsAction, ISerializable
{
	public bool WasDiscarded;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("WasDiscarded", WasDiscarded);
	}

	public CPlayerStatsRecoverCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "WasDiscarded")
				{
					WasDiscarded = info.GetBoolean("WasDiscarded");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsRecoverCard entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsRecoverCard(bool wasDiscarded, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		WasDiscarded = wasDiscarded;
	}

	public static CPlayerStatsRecoverCard CreateRecoverCardStat(SEventRecoverCard sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsRecoverCard(sevent.WasDiscarded, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(mainActorClassID, mainActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
