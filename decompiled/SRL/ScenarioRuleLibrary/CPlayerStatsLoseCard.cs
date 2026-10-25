using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsLoseCard : CPlayerStatsAction, ISerializable
{
	public int DamageAvoided { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DamageAvoided", DamageAvoided);
	}

	public CPlayerStatsLoseCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "DamageAvoided")
				{
					DamageAvoided = info.GetInt32("DamageAvoided");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsLoseCard entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsLoseCard(int damageAvoided, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		DamageAvoided = damageAvoided;
	}

	public static CPlayerStatsLoseCard CreateLoseCardStat(SEventLoseCard sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		int amount = sevent.Amount;
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		List<ElementInfusionBoardManager.EElement> elements = sevent.Elements;
		bool performedBySummons2 = performedBySummons;
		bool rolledIntoSummoner2 = rolledIntoSummoner;
		return new CPlayerStatsLoseCard(amount, baseData, new CPlayerStatsActionData(mainActorClassID, mainActorType, null, null, null, null, secondaryActorClassID, secondaryActorType, null, null, null, null, null, elements, int.MaxValue, CBaseCard.ECardType.None, sevent.AbilityType, "", int.MaxValue, performedBySummons2, rolledIntoSummoner2));
	}
}
