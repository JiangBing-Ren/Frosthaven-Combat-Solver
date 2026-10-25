using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
[StateDump(UseToString = true, NoMembers = true)]
public class CEnhancement : ISerializable
{
	public static readonly EEnhancement[] Enhancements = (EEnhancement[])Enum.GetValues(typeof(EEnhancement));

	public static readonly EEnhancementLine[] EnhancementLines = (EEnhancementLine[])Enum.GetValues(typeof(EEnhancementLine));

	[JsonProperty]
	public EEnhancement Enhancement { get; set; }

	[JsonProperty]
	public EEnhancementLine EnhancementLine { get; private set; }

	[JsonProperty]
	public int AbilityCardID { get; private set; }

	[JsonProperty]
	public string AbilityName { get; private set; }

	[JsonProperty]
	public int EnhancementSlot { get; private set; }

	[JsonProperty]
	[Obsolete]
	[StateDump(Skip = true)]
	public int PaidPrice { get; set; }

	[JsonProperty]
	public EEnhancementMarkType MarkType { get; private set; }

	public CAbilityCard AbilityCard
	{
		get
		{
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				CAbilityCard cAbilityCard = @class.FindCardWithID(AbilityCardID);
				if (cAbilityCard != null)
				{
					return cAbilityCard;
				}
			}
			return null;
		}
	}

	public CAbility Ability => AbilityCard.FindAbilityOnCard(AbilityName);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Enhancement", Enhancement);
		info.AddValue("EnhancementLine", EnhancementLine);
		info.AddValue("AbilityCardID", AbilityCardID);
		info.AddValue("AbilityName", AbilityName);
		info.AddValue("EnhancementSlot", EnhancementSlot);
		info.AddValue("PaidPrice", PaidPrice);
		info.AddValue("MarkType", MarkType);
	}

	private CEnhancement(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Enhancement":
					Enhancement = (EEnhancement)info.GetValue("Enhancement", typeof(EEnhancement));
					break;
				case "EnhancementLine":
					EnhancementLine = (EEnhancementLine)info.GetValue("EnhancementLine", typeof(EEnhancementLine));
					break;
				case "AbilityCardID":
					AbilityCardID = info.GetInt32("AbilityCardID");
					break;
				case "AbilityName":
					AbilityName = info.GetString("AbilityName");
					break;
				case "EnhancementSlot":
					EnhancementSlot = info.GetInt32("EnhancementSlot");
					break;
				case "PaidPrice":
					PaidPrice = info.GetInt32("PaidPrice");
					break;
				case "MarkType":
					MarkType = (EEnhancementMarkType)info.GetValue("MarkType", typeof(EEnhancementMarkType));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CEnhancement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		try
		{
			if (AbilityCardID == 235 && (AbilityName == "TSummon1" || AbilityName == "TSummon2"))
			{
				AbilityName = "TSummon";
			}
			if (AbilityCardID == 253 && (AbilityName == "TSummon1" || AbilityName == "TSummon2" || AbilityName == "TSummon3"))
			{
				AbilityName = "TSummon";
			}
		}
		catch (Exception ex2)
		{
			Log.ErrorWithReport("Exception while trying to deserialize CEnhancement and update multi summon card enhancement IDs\n" + ex2.Message + "\n" + ex2.StackTrace);
			throw ex2;
		}
	}

	[JsonConstructor]
	public CEnhancement(EEnhancement enhancement, EEnhancementLine enhancementLine, int abilityCardID, string abilityName, int enhancementSlot, EEnhancementMarkType markType = EEnhancementMarkType.DiamondPlus, int paidPrice = 0)
	{
		Enhancement = enhancement;
		EnhancementLine = enhancementLine;
		AbilityCardID = abilityCardID;
		AbilityName = abilityName;
		EnhancementSlot = enhancementSlot;
		PaidPrice = paidPrice;
		MarkType = markType;
	}

	public CEnhancement Copy()
	{
		return new CEnhancement(Enhancement, EnhancementLine, AbilityCardID, AbilityName, EnhancementSlot, MarkType, PaidPrice);
	}

	public void BuyEnhancement(EEnhancement enhancement, int paidPrice)
	{
		Enhancement = enhancement;
		PaidPrice = paidPrice;
		if (Enhancement != EEnhancement.NoEnhancement && PaidPrice == 0)
		{
			Log.Warning("Enhancement " + Enhancement.ToString() + " has Paid Price of zero.  Enhancement: " + ToString());
		}
		else if (Enhancement == EEnhancement.NoEnhancement && PaidPrice != 0)
		{
			Log.Warning("Empty enhancement slot has Paid Price of " + PaidPrice + ".  Enhancement: " + ToString());
		}
	}

	public static int BaseCost(EEnhancement enhancement, CAbility ability, bool classPersistent)
	{
		if (enhancement != EEnhancement.Area)
		{
			return ScenarioRuleClient.SRLYML.Enhancements.EnhancementBaseCosts(classPersistent)[enhancement];
		}
		return ScenarioRuleClient.SRLYML.Enhancements.EnhancementBaseCosts(classPersistent)[enhancement] / (ability.AreaEffect.Hexes.Count + ability.AbilityEnhancements.Where((CEnhancement w) => w.Enhancement == EEnhancement.Area).Count());
	}

	public static int PreviousEnhancementCost(CAbilityCard card, CAbility ability, bool classPersistent)
	{
		List<int> list = ScenarioRuleClient.SRLYML.Enhancements.NumberOfPreviousEnhancementsCost(classPersistent);
		int index = Math.Clamp((from s in card.GetActionAbilities(ability)
			select s.AbilityEnhancements.Count((CEnhancement w) => w.Enhancement != EEnhancement.NoEnhancement)).Sum(), 0, list.Count - 1);
		return list[index];
	}

	public static int AbilityCardLevelCost(CAbilityCard card, bool classPersistent)
	{
		List<int> list = ScenarioRuleClient.SRLYML.Enhancements.LevelOfAbilityCardCost(classPersistent);
		int index = Math.Clamp(card.Level - 1, 0, list.Count - 1);
		return list[index];
	}

	public static int MultiTargetCost(EEnhancement enhancement, CAbility ability, bool classPersistent)
	{
		bool flag = false;
		if ((ability.NumberTargets > 1 || ability.NumberTargets == -1) && ability.AbilityType != CAbility.EAbilityType.Summon)
		{
			flag = true;
		}
		else if (ability is CAbilityAttack cAbilityAttack && (cAbilityAttack.AllTargetsOnAttackPath || cAbilityAttack.AllTargetsOnMovePath || cAbilityAttack.AllTargetsOnMovePathSameStartAndEnd))
		{
			flag = true;
		}
		else if (ability is CAbilityMove cAbilityMove && (cAbilityMove.AllTargetsOnAttackPath || cAbilityMove.AllTargetsOnMovePath || cAbilityMove.AllTargetsOnMovePathSameStartAndEnd))
		{
			flag = true;
		}
		else if (ability.AreaEffect != null && enhancement != EEnhancement.Area)
		{
			flag = true;
		}
		else if (ability.MiscAbilityData != null && ability.MiscAbilityData.ConsiderMultiTargetForEnhancements.HasValue && ability.MiscAbilityData.ConsiderMultiTargetForEnhancements.Value)
		{
			flag = true;
		}
		if (!flag)
		{
			return 0;
		}
		return BaseCost(enhancement, ability, classPersistent) * (ScenarioRuleClient.SRLYML.Enhancements.MultiTargetMultiplier(classPersistent) - 1);
	}

	public static int TotalCost(EEnhancement enhancement, CAbilityCard card, CAbility ability, bool classPersistent)
	{
		if (card == null || ability == null)
		{
			Log.ErrorWithReport("Null Ability Card or Ability sent to TotalCost.");
			return 0;
		}
		return BaseCost(enhancement, ability, classPersistent) + MultiTargetCost(enhancement, ability, classPersistent) + PreviousEnhancementCost(card, ability, classPersistent) + AbilityCardLevelCost(card, classPersistent);
	}

	public bool Compare(object obj)
	{
		if (obj is CEnhancement cEnhancement)
		{
			if (Enhancement == cEnhancement.Enhancement && EnhancementLine == cEnhancement.EnhancementLine && AbilityCardID == cEnhancement.AbilityCardID && AbilityName == cEnhancement.AbilityName && EnhancementSlot == cEnhancement.EnhancementSlot)
			{
				return PaidPrice == cEnhancement.PaidPrice;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "Enhancement: " + Enhancement.ToString() + ", EnhancementLine: " + EnhancementLine.ToString() + ", AbilityCardID: " + AbilityCardID + ", AbilityName: " + AbilityName.ToString() + ", EnhancementSlot: " + EnhancementSlot;
	}
}
