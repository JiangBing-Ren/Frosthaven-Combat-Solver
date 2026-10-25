using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDiscardCard : CPlayerStatsAction, ISerializable
{
	public int DiscardedCardID { get; private set; }

	public CBaseCard.ECardPile Pile { get; private set; }

	public DateTime TimeStamp { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DiscardedCardID", DiscardedCardID);
		info.AddValue("Pile", Pile);
		info.AddValue("TimeStamp", TimeStamp);
	}

	public CPlayerStatsDiscardCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "DiscardedCardID":
					DiscardedCardID = info.GetInt32("DiscardedCardID");
					break;
				case "Pile":
					Pile = (CBaseCard.ECardPile)info.GetValue("Pile", typeof(CBaseCard.ECardPile));
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsDiscardCard entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDiscardCard(int card, CBaseCard.ECardPile pile, DateTime timeStamp, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		DiscardedCardID = card;
		Pile = pile;
		TimeStamp = timeStamp;
	}

	public static CPlayerStatsDiscardCard CreateDiscardCardStat(SEventDiscardCard sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsDiscardCard(sevent.CardID, sevent.Pile, sevent.TimeStamp, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(mainActorClassID, mainActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
