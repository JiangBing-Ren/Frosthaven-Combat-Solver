using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsForcedActorsIntoTraps : CPlayerStatsAction, ISerializable
{
	public int AbilityCardID { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AbilityCardID", AbilityCardID);
	}

	public CPlayerStatsForcedActorsIntoTraps(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "AbilityCardID")
				{
					AbilityCardID = info.GetInt32("AbilityCardID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsTrap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsForcedActorsIntoTraps(int AbilityCardID, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		this.AbilityCardID = AbilityCardID;
	}

	public static CPlayerStatsForcedActorsIntoTraps CreateForcedIntoTrapStat(SEventActorForcedIntoTrap sevent, string advGuid, string sceGuid, string currentQuestType, string ActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsForcedActorsIntoTraps(sevent.AbilityCardID, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(mainActorClassID, ActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
