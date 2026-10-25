using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsActorActiveBonuses : CPlayerStatsBase, ISerializable
{
	public List<SEventActorActiveBonuses.ActiveBonusInfo> ActiveBonuses;

	public bool AtStartOfRest;

	public int CardsInDiscardPile;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActiveBonuses", ActiveBonuses);
		info.AddValue("AtStartOfRest", AtStartOfRest);
		info.AddValue("CardsInDiscardPile", CardsInDiscardPile);
	}

	public CPlayerStatsActorActiveBonuses(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "AtStartOfRest":
					AtStartOfRest = info.GetBoolean("AtStartOfRest");
					break;
				case "ActiveBonuses":
					ActiveBonuses = (List<SEventActorActiveBonuses.ActiveBonusInfo>)info.GetValue("ActiveBonuses", typeof(List<SEventActorActiveBonuses.ActiveBonusInfo>));
					break;
				case "CardsInDiscardPile":
					CardsInDiscardPile = info.GetInt32("CardsInDiscardPile");
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

	public CPlayerStatsActorActiveBonuses(List<SEventActorActiveBonuses.ActiveBonusInfo> activeBonuses, bool atStartOfRest, int cardsInDiscardPile, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		ActiveBonuses = activeBonuses.ToList();
		AtStartOfRest = atStartOfRest;
		CardsInDiscardPile = cardsInDiscardPile;
	}

	public static CPlayerStatsActorActiveBonuses CreateActorActiveBonusesStat(SEventActorActiveBonuses sevent, string advGuid, string sceGuid, string currentQuestType)
	{
		return new CPlayerStatsActorActiveBonuses(sevent.ActiveBonuses, sevent.AtStartOfRest, sevent.CardsInDiscardPile, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder));
	}
}
