using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsModifiers : CPlayerStatsAction, ISerializable
{
	public List<string> Modifier { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Modifier", Modifier);
	}

	public CPlayerStatsModifiers(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "Modifier")
				{
					Modifier = (List<string>)info.GetValue("Modifier", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsModifiers entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsModifiers(List<string> modifier, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		Modifier = modifier;
	}

	public static CPlayerStatsModifiers CreateAttackModifierStat(SEventAttackModifier sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsModifiers(sevent.UsedAttackModifierInfo.Select((SEventAttackModifier.ModifierInfo x) => x.ModifierWithNoLocalization).ToList(), new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData((mainActorClassID != null) ? mainActorClassID : sevent.MainActorClass, mainActorType, null, null, null, null, (secondaryActorClassID != null) ? secondaryActorClassID : sevent.SecondaryActorClass, secondaryActorType, sevent.SecondaryActorPositiveConditions, sevent.SecondaryActorNegativeConditions, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
