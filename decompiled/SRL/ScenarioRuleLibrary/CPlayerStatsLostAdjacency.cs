using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsLostAdjacency : CPlayerStatsAction, ISerializable
{
	public string CharacterID { get; private set; }

	public bool WasForced { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("WasForced", WasForced);
	}

	public CPlayerStatsLostAdjacency(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CharacterID"))
				{
					if (name == "WasForced")
					{
						WasForced = info.GetBoolean("WasForced");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsLostAdjacency entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsLostAdjacency(string characterID, bool wasForced, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		CharacterID = characterID;
		WasForced = wasForced;
	}

	public static CPlayerStatsLostAdjacency CreateLostAdjacencyStat(SEventLostAdjacency sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsLostAdjacency(sevent.CharacterID, sevent.WasForced, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(mainActorClassID, mainActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
