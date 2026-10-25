using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsHand : CPlayerStatsBase, ISerializable
{
	public int HandSize { get; private set; }

	public int DiscardSize { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HandSize", HandSize);
		info.AddValue("DiscardSize", DiscardSize);
	}

	public CPlayerStatsHand(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "HandSize"))
				{
					if (name == "DiscardSize")
					{
						DiscardSize = info.GetInt32("DiscardSize");
					}
				}
				else
				{
					HandSize = info.GetInt32("HandSize");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsHand entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsHand(int handSize, int discardSize, CPlayerSatsBaseData baseData, CPlayerStatsAction.CPlayerStatsActionData actionData)
		: base(baseData)
	{
		HandSize = handSize;
		DiscardSize = discardSize;
	}

	public static CPlayerStatsHand CreateHandStat(SEventHand sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsHand(sevent.HandSize, sevent.DiscardSize, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsAction.CPlayerStatsActionData(mainActorClassID, mainActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
