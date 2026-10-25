using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsActionEnded : CPlayerStatsBase, ISerializable
{
	public string CardName { get; private set; }

	public bool TopAction { get; private set; }

	public bool DefaultAction { get; private set; }

	public bool BottomAction => !TopAction;

	public CBaseCard.ECardType CardType { get; private set; }

	public bool Ended { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CardName", CardName);
		info.AddValue("TopAction", TopAction);
		info.AddValue("DefaultAction", DefaultAction);
		info.AddValue("CardType", CardType);
		info.AddValue("Ended", Ended);
	}

	public CPlayerStatsActionEnded(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CardName":
					CardName = info.GetString("CardName");
					break;
				case "TopAction":
					TopAction = info.GetBoolean("TopAction");
					break;
				case "DefaultAction":
					DefaultAction = info.GetBoolean("DefaultAction");
					break;
				case "CardType":
					CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
					break;
				case "Ended":
					Ended = info.GetBoolean("Ended");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsHeal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsActionEnded(string cardName, bool topAction, bool defaultAction, CBaseCard.ECardType cardType, bool ended, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		CardName = cardName;
		TopAction = topAction;
		DefaultAction = defaultAction;
		CardType = cardType;
		Ended = ended;
	}

	public static CPlayerStatsActionEnded CreateActionEndedStat(SEventAction sevent, string advGuid, string sceGuid, string currentQuestType)
	{
		return new CPlayerStatsActionEnded(sevent.CardName, sevent.TopAction, sevent.DefaultAction, sevent.CardType, sevent.Ended, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder));
	}
}
