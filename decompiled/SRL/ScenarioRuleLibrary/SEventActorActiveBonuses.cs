using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorActiveBonuses : SEventActor
{
	[Serializable]
	public struct ActiveBonusInfo
	{
		public List<string> MiscVars;

		public AbilityData.EActiveBonusFlag Flags;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MiscVars", MiscVars);
			info.AddValue("Flags", Flags);
		}

		public ActiveBonusInfo(CActiveBonus bonus)
		{
			Flags = (bonus?.Ability?.ActiveBonusData?.Flags).GetValueOrDefault();
			if (bonus?.Ability?.ActiveBonusData?.MiscVars == null)
			{
				MiscVars = new List<string>();
				return;
			}
			MiscVars = bonus.Ability.ActiveBonusData.MiscVars.Select(((string, TValue) miscVar) => miscVar.Item1).ToList();
		}

		public ActiveBonusInfo(SerializationInfo info, StreamingContext context)
		{
			MiscVars = null;
			Flags = AbilityData.EActiveBonusFlag.None;
			foreach (SerializationEntry item in info)
			{
				try
				{
					string name = item.Name;
					if (!(name == "MiscVars"))
					{
						if (name == "Flags")
						{
							Flags = (AbilityData.EActiveBonusFlag)info.GetValue("Flags", typeof(AbilityData.EActiveBonusFlag));
						}
					}
					else
					{
						MiscVars = (List<string>)info.GetValue("MiscVars", typeof(List<string>));
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize ActiveBonusInfo entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}
	}

	public List<ActiveBonusInfo> ActiveBonuses;

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

	public SEventActorActiveBonuses(SerializationInfo info, StreamingContext context)
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
					ActiveBonuses = (List<ActiveBonusInfo>)info.GetValue("ActiveBonuses", typeof(List<ActiveBonusInfo>));
					break;
				case "CardsInDiscardPile":
					CardsInDiscardPile = info.GetInt32("CardsInDiscardPile");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorUsedItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorActiveBonuses(CActor actor, bool atStartOfRest, SEventActorBaseData baseData, List<CActiveBonus> cachedActiveBonuses)
		: base(baseData)
	{
		AtStartOfRest = atStartOfRest;
		ActiveBonuses = (cachedActiveBonuses ?? actor.FindApplicableActiveBonuses()).Select((CActiveBonus bonus) => new ActiveBonusInfo(bonus)).ToList();
		CardsInDiscardPile = (actor.CharacterClass?.DiscardedAbilityCards?.Count).GetValueOrDefault();
	}
}
