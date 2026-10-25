using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAttackModifier : SEvent
{
	[Serializable]
	public class OverrideBuffData : ISerializable
	{
		public enum EOverrideBuffType
		{
			Conditional,
			Item,
			ActiveBonus
		}

		public EOverrideBuffType OverrideBuffType { get; set; }

		public string OverrideBuffName { get; set; }

		public int OverrideStrength { get; set; }

		public List<CCondition.ENegativeCondition> OverrideNegativeConditions { get; private set; }

		public List<CCondition.EPositiveCondition> OverridePositiveConditions { get; private set; }

		public bool IsScalar { get; set; }

		public bool LogFullName { get; set; }

		public string OverrideSourceActorClassID { get; set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("OverrideBuffType", OverrideBuffType);
			info.AddValue("OverrideBuffName", OverrideBuffName);
			info.AddValue("OverrideStrength", OverrideStrength);
			info.AddValue("OverrideNegativeConditions", OverrideNegativeConditions);
			info.AddValue("OverridePositiveConditions", OverridePositiveConditions);
			info.AddValue("OverrideSourceActorClassID", OverrideSourceActorClassID);
			info.AddValue("IsScalar", IsScalar);
		}

		public OverrideBuffData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "IsScalar":
						IsScalar = info.GetBoolean("IsScalar");
						break;
					case "OverrideBuffType":
						OverrideBuffType = (EOverrideBuffType)info.GetValue("OverrideBuffType", typeof(EOverrideBuffType));
						break;
					case "OverrideBuffName":
						OverrideBuffName = info.GetString("OverrideBuffName");
						break;
					case "OverrideStrength":
						OverrideStrength = info.GetInt32("OverrideStrength");
						break;
					case "OverrideNegativeConditions":
						OverrideNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("OverrideNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
						break;
					case "OverridePositiveConditions":
						OverridePositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("OverridePositiveConditions", typeof(List<CCondition.EPositiveCondition>));
						break;
					case "OverrideSourceActorClassID":
						OverrideSourceActorClassID = info.GetString("OverrideSourceActorClassID");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize OverrideBuffData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public OverrideBuffData(EOverrideBuffType overrideBuffType, string overrideBuffName, int overrideStrength, List<CCondition.ENegativeCondition> overrideNegativeConditions, List<CCondition.EPositiveCondition> overridePositiveConditions, string overrideSourceActorClassID = null, bool logFullName = true, bool isScalar = false)
		{
			OverrideBuffType = overrideBuffType;
			OverrideBuffName = overrideBuffName;
			OverrideStrength = overrideStrength;
			OverrideNegativeConditions = overrideNegativeConditions;
			OverridePositiveConditions = overridePositiveConditions;
			OverrideSourceActorClassID = overrideSourceActorClassID;
			LogFullName = logFullName;
			IsScalar = isScalar;
		}
	}

	[Serializable]
	public struct ModifierInfo
	{
		public string ModifierWithCustomLocalization;

		public string ModifierWithNoLocalization;

		public string ModifierValue;

		public string ModifierChangedToValue;
	}

	public int PreModifierAttackStrength { get; private set; }

	public int FinalAttackStrength { get; set; }

	public int BonusStrengthFromAttackModifiers { get; set; }

	public int BonusStrengthOutsideOfModifiers { get; set; }

	public int AttackAbilityAttackStrength { get; set; }

	public List<OverrideBuffData> OverrideBuffs { get; set; }

	public EAdvantageStatuses AdvantageStatus { get; private set; }

	public List<ModifierInfo> UsedAttackModifierInfo { get; private set; }

	public List<ModifierInfo> UnusedAttackModifierInfo { get; private set; }

	public int TargetActorID { get; set; }

	public int TargetActorShield { get; set; }

	public int DrawingActorPierce { get; set; }

	public int HealFromModifiers { get; private set; }

	public int ShieldFromModifiers { get; private set; }

	public int PierceFromModifers { get; private set; }

	public int FinalPierce { get; set; }

	public int ExploitedVulnerability { get; set; }

	public int ExploitedVulnerabilityStrength { get; set; }

	public bool TargetActorWasPoisoned { get; set; }

	public bool TargetActorWasInvulnerable { get; set; }

	public bool TargetActorHasWard { get; set; }

	public bool TargetActorHasBrittle { get; set; }

	public List<CCondition.ENegativeCondition> NegativeConditionsFromModifiers { get; private set; }

	public List<CCondition.EPositiveCondition> PositiveConditionsFromModifiers { get; private set; }

	public List<ElementInfusionBoardManager.EElement> ElementsInfusedFromModifiers { get; private set; }

	public string MainActorClass { get; private set; }

	public CActor.EType MainActorType { get; private set; }

	public bool MainActorIsEnemySummon { get; private set; }

	public List<PositiveConditionPair> MainActorPositiveConditions { get; private set; }

	public List<NegativeConditionPair> MainActorNegativeConditions { get; private set; }

	public string SecondaryActorClass { get; private set; }

	public CActor.EType? SecondaryActorType { get; private set; }

	public bool SecondaryActorIsSummon { get; private set; }

	public List<PositiveConditionPair> SecondaryActorPositiveConditions { get; private set; }

	public List<NegativeConditionPair> SecondaryActorNegativeConditions { get; private set; }

	public int AttackIndex { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PreModifierAttackStrength", PreModifierAttackStrength);
		info.AddValue("FinalAttackStrength", FinalAttackStrength);
		info.AddValue("BonusStrengthFromAttackModifiers", BonusStrengthFromAttackModifiers);
		info.AddValue("BonusStrengthOutsideOfModifiers", BonusStrengthOutsideOfModifiers);
		info.AddValue("AttackAbilityAttackStrength", AttackAbilityAttackStrength);
		info.AddValue("OverrideBuffs", OverrideBuffs);
		info.AddValue("AdvantageStatus", AdvantageStatus);
		info.AddValue("UsedAttackModifierInfo", UsedAttackModifierInfo);
		info.AddValue("UnusedAttackModifierInfo", UnusedAttackModifierInfo);
		info.AddValue("TargetActorID", TargetActorID);
		info.AddValue("TargetActorShield", TargetActorShield);
		info.AddValue("DrawingActorPierce", DrawingActorPierce);
		info.AddValue("HealFromModifiers", HealFromModifiers);
		info.AddValue("ShieldFromModifiers", ShieldFromModifiers);
		info.AddValue("PierceFromModifiers", PierceFromModifers);
		info.AddValue("FinalPierce", FinalPierce);
		info.AddValue("TargetActorWasPoisoned", TargetActorWasPoisoned);
		info.AddValue("ExploitedVulnerability", ExploitedVulnerability);
		info.AddValue("ExploitedVulnerabilityStrength", ExploitedVulnerabilityStrength);
		info.AddValue("TargetActorWasInvulnerable", TargetActorWasInvulnerable);
		info.AddValue("TargetActorHasWard", TargetActorHasWard);
		info.AddValue("TargetActorHasBrittle", TargetActorHasBrittle);
		info.AddValue("NegativeConditionsFromModifiers", NegativeConditionsFromModifiers);
		info.AddValue("PositiveConditionsFromModifiers", PositiveConditionsFromModifiers);
		info.AddValue("ElementsInfusedFromModifiers", ElementsInfusedFromModifiers);
		info.AddValue("MainActorClass", MainActorClass);
		info.AddValue("MainActorType", MainActorType);
		info.AddValue("MainActorIsEnemySummon", MainActorIsEnemySummon);
		info.AddValue("MainActorPositiveConditions", MainActorPositiveConditions);
		info.AddValue("MainActorNegativeConditions", MainActorNegativeConditions);
		info.AddValue("SecondaryActor", SecondaryActorClass);
		info.AddValue("SecondaryActorType", SecondaryActorType);
		info.AddValue("SecondaryActorIsSummon", SecondaryActorIsSummon);
		info.AddValue("SecondaryActorPositiveConditions", SecondaryActorPositiveConditions);
		info.AddValue("SecondaryActorNegativeConditions", SecondaryActorNegativeConditions);
		info.AddValue("AttackIndex", AttackIndex);
	}

	public SEventAttackModifier(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		List<string> list = null;
		List<string> list2 = null;
		List<string> list3 = null;
		List<string> list4 = null;
		List<string> list5 = null;
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PreModifierAttackStrength":
					PreModifierAttackStrength = info.GetInt32("PreModifierAttackStrength");
					break;
				case "FinalAttackStrength":
					FinalAttackStrength = info.GetInt32("FinalAttackStrength");
					break;
				case "BonusStrengthFromAttackModifiers":
					BonusStrengthFromAttackModifiers = info.GetInt32("BonusStrengthFromAttackModifiers");
					break;
				case "BonusStrengthOutsideOfModifiers":
					BonusStrengthOutsideOfModifiers = info.GetInt32("BonusStrengthOutsideOfModifiers");
					break;
				case "AttackAbilityAttackStrength":
					AttackAbilityAttackStrength = info.GetInt32("AttackAbilityAttackStrength");
					break;
				case "OverrideBuffs":
					OverrideBuffs = (List<OverrideBuffData>)info.GetValue("OverrideBuffs", typeof(List<OverrideBuffData>));
					break;
				case "AdvantageStatus":
					AdvantageStatus = (EAdvantageStatuses)info.GetValue("AdvantageStatus", typeof(EAdvantageStatuses));
					break;
				case "UsedAttackModifierInfo":
					UsedAttackModifierInfo = (List<ModifierInfo>)info.GetValue("UsedAttackModifierInfo", typeof(List<ModifierInfo>));
					break;
				case "UnusedAttackModifierInfo":
					UnusedAttackModifierInfo = (List<ModifierInfo>)info.GetValue("UnusedAttackModifierInfo", typeof(List<ModifierInfo>));
					break;
				case "UsedAttackModifierStrings":
					list = (List<string>)info.GetValue("UsedAttackModifierStrings", typeof(List<string>));
					break;
				case "MathModifiers":
					list5 = (List<string>)info.GetValue("MathModifiers", typeof(List<string>));
					break;
				case "UnusedAttackModifierStringsModified":
					list2 = (List<string>)info.GetValue("UnusedAttackModifierStringsModified", typeof(List<string>));
					break;
				case "UsedAttackModifierStringsModified":
					list3 = (List<string>)info.GetValue("UsedAttackModifierStringsModified", typeof(List<string>));
					break;
				case "UnusedAttackModifierStrings":
					list4 = (List<string>)info.GetValue("UnusedAttackModifierStrings", typeof(List<string>));
					break;
				case "TargetActorID":
					TargetActorID = info.GetInt32("TargetActorID");
					break;
				case "TargetActorShield":
					TargetActorShield = info.GetInt32("TargetActorShield");
					break;
				case "DrawingActorPierce":
					DrawingActorPierce = info.GetInt32("DrawingActorPierce");
					break;
				case "HealFromModifiers":
					HealFromModifiers = info.GetInt32("HealFromModifiers");
					break;
				case "ShieldFromModifiers":
					ShieldFromModifiers = info.GetInt32("ShieldFromModifiers");
					break;
				case "PierceFromModifers":
					PierceFromModifers = info.GetInt32("PierceFromModifers");
					break;
				case "FinalPierce":
					FinalPierce = info.GetInt32("FinalPierce");
					break;
				case "TargetActorWasPoisoned":
					TargetActorWasPoisoned = info.GetBoolean("TargetActorWasPoisoned");
					break;
				case "ExploitedVulnerability":
					ExploitedVulnerability = info.GetInt32("ExploitedVulnerability");
					break;
				case "ExploitedVulnerabilityStrength":
					ExploitedVulnerabilityStrength = info.GetInt32("ExploitedVulnerabilityStrength");
					break;
				case "TargetActorWasInvulnerable":
					TargetActorWasInvulnerable = info.GetBoolean("TargetActorWasInvulnerable");
					break;
				case "TargetActorHasWard":
					TargetActorHasWard = info.GetBoolean("TargetActorHasWard");
					break;
				case "TargetActorHasBrittle":
					TargetActorHasBrittle = info.GetBoolean("TargetActorHasBrittle");
					break;
				case "NegativeConditionsFromModifiers":
					NegativeConditionsFromModifiers = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditionsFromModifiers", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "PositiveConditionsFromModifiers":
					PositiveConditionsFromModifiers = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditionsFromModifiers", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "ElementsInfusedFromModifiers":
					ElementsInfusedFromModifiers = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsInfusedFromModifiers", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "MainActorClass":
					MainActorClass = info.GetString("MainActorClass");
					break;
				case "MainActorType":
					MainActorType = (CActor.EType)info.GetValue("MainActorType", typeof(CActor.EType));
					break;
				case "MainActorIsEnemySummon":
					MainActorIsEnemySummon = info.GetBoolean("MainActorIsEnemySummon");
					break;
				case "MainActorPositiveConditions":
					MainActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("MainActorPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "MainActorNegativeConditions":
					MainActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("MainActorNegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "SecondaryActorClass":
					SecondaryActorClass = info.GetString("SecondaryActorClass");
					break;
				case "SecondaryActorType":
					SecondaryActorType = (CActor.EType?)info.GetValue("SecondaryActorType", typeof(CActor.EType?));
					break;
				case "SecondaryActorIsSummon":
					SecondaryActorIsSummon = info.GetBoolean("SecondaryActorIsSummon");
					break;
				case "SecondaryActorPositiveConditions":
					SecondaryActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("SecondaryActorPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "SecondaryActorNegativeConditions":
					SecondaryActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("SecondaryActorNegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "AttackIndex":
					AttackIndex = info.GetInt32("AttackIndex");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAttackModifier entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (list5 != null && list != null && list3 != null && list4 != null && list2 != null)
		{
			UsedAttackModifierInfo = new List<ModifierInfo>();
			UnusedAttackModifierInfo = new List<ModifierInfo>();
			for (int i = 0; i < list.Count; i++)
			{
				UsedAttackModifierInfo.Add(new ModifierInfo
				{
					ModifierWithCustomLocalization = list[i],
					ModifierChangedToValue = list3[i],
					ModifierValue = list5[i],
					ModifierWithNoLocalization = list[i]
				});
			}
			for (int j = 0; j < list4.Count; j++)
			{
				UsedAttackModifierInfo.Add(new ModifierInfo
				{
					ModifierWithCustomLocalization = list4[j],
					ModifierChangedToValue = list2[j],
					ModifierValue = "",
					ModifierWithNoLocalization = list4[j]
				});
			}
		}
	}

	public SEventAttackModifier(int initialAttackStrength, EAdvantageStatuses advantageStatuses, List<ModifierInfo> attackModifierStrings, List<ModifierInfo> unusedAttackModifiersStrings, int targetActorId, int finalAttackStrength, int bonusStrengthFromAttackModifiers, int bonusStrengthOutsideOfModifiers, int healFromModifiers, int shieldFromModifiers, int pierceFromModifiers, List<CCondition.ENegativeCondition> negativeConditionsFromModifiers, List<CCondition.EPositiveCondition> positiveConditionsFromModifiers, List<ElementInfusionBoardManager.EElement> elementsInfusedFromModifiers, string mainActorClass, CActor.EType mainActorType, bool mainActorIsEnemySummon, List<PositiveConditionPair> mainActorPositiveConditions, List<NegativeConditionPair> mainActorNegativeConditions, string secondaryActorClass, CActor.EType secondaryActorType, bool secondaryActorIsSummon, List<PositiveConditionPair> secondaryActorPositiveConditions, List<NegativeConditionPair> secondaryActorNegativeConditions, string text = "", int attackIndex = 0)
		: base(ESEType.AttackModifier, text)
	{
		PreModifierAttackStrength = initialAttackStrength;
		AdvantageStatus = advantageStatuses;
		UsedAttackModifierInfo = attackModifierStrings;
		UnusedAttackModifierInfo = unusedAttackModifiersStrings;
		TargetActorID = targetActorId;
		FinalAttackStrength = finalAttackStrength;
		BonusStrengthFromAttackModifiers = bonusStrengthFromAttackModifiers;
		BonusStrengthOutsideOfModifiers = bonusStrengthOutsideOfModifiers;
		HealFromModifiers = healFromModifiers;
		ShieldFromModifiers = shieldFromModifiers;
		PierceFromModifers = pierceFromModifiers;
		NegativeConditionsFromModifiers = negativeConditionsFromModifiers;
		PositiveConditionsFromModifiers = positiveConditionsFromModifiers;
		ElementsInfusedFromModifiers = elementsInfusedFromModifiers;
		MainActorClass = mainActorClass;
		MainActorType = mainActorType;
		MainActorIsEnemySummon = mainActorIsEnemySummon;
		MainActorPositiveConditions = mainActorPositiveConditions;
		MainActorNegativeConditions = mainActorNegativeConditions;
		SecondaryActorClass = secondaryActorClass;
		SecondaryActorType = secondaryActorType;
		SecondaryActorIsSummon = secondaryActorIsSummon;
		SecondaryActorPositiveConditions = secondaryActorPositiveConditions;
		SecondaryActorNegativeConditions = secondaryActorNegativeConditions;
		AttackIndex = attackIndex;
	}

	public SEventAttackModifier(string text = "")
		: base(ESEType.AttackModifier, text)
	{
	}
}
