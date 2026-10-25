using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAction : SEvent
{
	[Serializable]
	public class SEventActionAugmentationData : ISerializable
	{
		public List<ElementInfusionBoardManager.EElement> ElementsConsumed { get; private set; }

		public List<SEventActionAugmentationOpData> AugmentationOpDatas { get; private set; }

		public int XP { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ElementsConsumed", ElementsConsumed);
			info.AddValue("AugmentationOpDatas", AugmentationOpDatas);
			info.AddValue("XP", XP);
		}

		public SEventActionAugmentationData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "ElementsConsumed":
						ElementsConsumed = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsConsumed", typeof(List<ElementInfusionBoardManager.EElement>));
						break;
					case "AugmentationOpDatas":
						AugmentationOpDatas = (List<SEventActionAugmentationOpData>)info.GetValue("AugmentationOpDatas", typeof(List<SEventActionAugmentationOpData>));
						break;
					case "XP":
						XP = info.GetInt32("XP");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventActionAugmentationData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
			if (ElementsConsumed == null)
			{
				ElementsConsumed = new List<ElementInfusionBoardManager.EElement>();
			}
			if (AugmentationOpDatas == null)
			{
				AugmentationOpDatas = new List<SEventActionAugmentationOpData>();
			}
		}

		public SEventActionAugmentationData(List<ElementInfusionBoardManager.EElement> elementsConsumed, List<CActionAugmentationOp> augmentationsOps, int xp)
		{
			ElementsConsumed = elementsConsumed;
			XP = xp;
			AugmentationOpDatas = new List<SEventActionAugmentationOpData>();
			foreach (CActionAugmentationOp augmentationsOp in augmentationsOps)
			{
				if (augmentationsOp.AbilityOverride == null || !augmentationsOp.AbilityOverride.GetVar("DontLog").GetBool())
				{
					AugmentationOpDatas.Add(new SEventActionAugmentationOpData(augmentationsOp.Type, augmentationsOp.AbilityOverride, augmentationsOp.Ability));
				}
			}
		}
	}

	[Serializable]
	public class SEventActionAugmentationOpData : ISerializable
	{
		public CActionAugmentationOp.EActionAugmentationType Type { get; private set; }

		public SEventAbilityOverrideData AbilityOverrideData { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Type", Type);
			info.AddValue("AbilityOverrideData", AbilityOverrideData);
		}

		public SEventActionAugmentationOpData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					string name = item.Name;
					if (!(name == "Type"))
					{
						if (name == "AbilityOverrideData")
						{
							AbilityOverrideData = (SEventAbilityOverrideData)info.GetValue("AbilityOverrideData", typeof(SEventAbilityOverrideData));
						}
					}
					else
					{
						Type = (CActionAugmentationOp.EActionAugmentationType)info.GetValue("Type", typeof(CActionAugmentationOp.EActionAugmentationType));
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventActionAugmentationOpData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventActionAugmentationOpData(CActionAugmentationOp.EActionAugmentationType type, CAbilityOverride abilityOverride, CAbility ability)
		{
			Type = type;
			if (abilityOverride != null && type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
			{
				AbilityOverrideData = new SEventAbilityOverrideData(abilityOverride);
			}
			else if (ability != null && type == CActionAugmentationOp.EActionAugmentationType.Ability)
			{
				AbilityOverrideData = new SEventAbilityOverrideData(ability);
			}
		}
	}

	[Serializable]
	public class SEventAbilityOverrideData : ISerializable
	{
		public Guid? ID;

		public CAbility.EAbilityType? AbilityType { get; private set; }

		public int? Strength { get; private set; }

		public int? Range { get; private set; }

		public int? NumberOfTargets { get; private set; }

		public bool? Jump { get; private set; }

		public bool? Fly { get; private set; }

		public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

		public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

		public bool? AllTargetsOnMovePath { get; private set; }

		public bool? AllTargetsOnAttackPath { get; private set; }

		public int? Pierce { get; private set; }

		public int? RetaliateRange { get; private set; }

		public CAbilityPush.EAdditionalPushEffect? AdditionalPushEffect { get; private set; }

		public int? AdditionalPushEffectDamage { get; private set; }

		public int? AdditionalPushEffectXP { get; private set; }

		public int? ConditionDuration { get; private set; }

		public EConditionDecTrigger? ConditionDecTrigger { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ID", ID);
			info.AddValue("AbilityType", AbilityType);
			info.AddValue("Strength", Strength);
			info.AddValue("Range", Range);
			info.AddValue("NumberOfTargets", NumberOfTargets);
			info.AddValue("Jump", Jump);
			info.AddValue("Fly", Fly);
			info.AddValue("PositiveConditions", PositiveConditions);
			info.AddValue("NegativeConditions", NegativeConditions);
			info.AddValue("AllTargetsOnMovePath", AllTargetsOnMovePath);
			info.AddValue("AllTargetsOnAttackPath", AllTargetsOnAttackPath);
			info.AddValue("Pierce", Pierce);
			info.AddValue("RetaliateRange", RetaliateRange);
			info.AddValue("AdditionalPushEffect", AdditionalPushEffect);
			info.AddValue("AdditionalPushEffectDamage", AdditionalPushEffectDamage);
			info.AddValue("AdditionalPushEffectXP", AdditionalPushEffectXP);
			info.AddValue("ConditionDuration", ConditionDuration);
			info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		}

		public SEventAbilityOverrideData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "ID":
						ID = (Guid?)info.GetValue("ID", typeof(Guid?));
						break;
					case "AbilityType":
						AbilityType = (CAbility.EAbilityType?)info.GetValue("AbilityType", typeof(CAbility.EAbilityType?));
						break;
					case "Strength":
						Strength = (int?)info.GetValue("Strength", typeof(int?));
						break;
					case "Range":
						Range = (int?)info.GetValue("Range", typeof(int?));
						break;
					case "NumberOfTargets":
						NumberOfTargets = (int?)info.GetValue("NumberOfTargets", typeof(int?));
						break;
					case "Jump":
						Jump = (bool?)info.GetValue("Jump", typeof(bool?));
						break;
					case "Fly":
						Fly = (bool?)info.GetValue("Fly", typeof(bool?));
						break;
					case "PositiveConditions":
						PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
						break;
					case "NegativeConditions":
						NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
						break;
					case "AllTargetsOnMovePath":
						AllTargetsOnMovePath = (bool?)info.GetValue("AllTargetsOnMovePath", typeof(bool?));
						break;
					case "AllTargetsOnAttackPath":
						AllTargetsOnAttackPath = (bool?)info.GetValue("AllTargetsOnAttackPath", typeof(bool?));
						break;
					case "Pierce":
						Pierce = (int?)info.GetValue("Pierce", typeof(int?));
						break;
					case "RetaliateRange":
						RetaliateRange = (int?)info.GetValue("RetaliateRange", typeof(int?));
						break;
					case "AdditionalPushEffect":
						AdditionalPushEffect = (CAbilityPush.EAdditionalPushEffect?)info.GetValue("AdditionalPushEffect", typeof(CAbilityPush.EAdditionalPushEffect?));
						break;
					case "AdditionalPushEffectDamage":
						AdditionalPushEffectDamage = (int?)info.GetValue("AdditionalPushEffectDamage", typeof(int?));
						break;
					case "AdditionalPushEffectXP":
						AdditionalPushEffectXP = (int?)info.GetValue("AdditionalPushEffectXP", typeof(int?));
						break;
					case "ConditionDuration":
						ConditionDuration = (int?)info.GetValue("ConditionDuration", typeof(int?));
						break;
					case "ConditionDecTrigger":
						ConditionDecTrigger = (EConditionDecTrigger?)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger?));
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityOverrideData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
			if (PositiveConditions == null)
			{
				PositiveConditions = new List<CCondition.EPositiveCondition>();
			}
			if (NegativeConditions == null)
			{
				NegativeConditions = new List<CCondition.ENegativeCondition>();
			}
		}

		public SEventAbilityOverrideData(CAbilityOverride abilityOverride)
		{
			ID = abilityOverride.ID;
			AbilityType = abilityOverride.AbilityType;
			Strength = abilityOverride.Strength;
			Range = abilityOverride.Range;
			NumberOfTargets = abilityOverride.NumberOfTargets;
			Jump = abilityOverride.Jump;
			Fly = abilityOverride.Fly;
			PositiveConditions = abilityOverride.PositiveConditions;
			NegativeConditions = abilityOverride.NegativeConditions;
			AllTargetsOnMovePath = abilityOverride.AllTargetsOnMovePath;
			AllTargetsOnAttackPath = abilityOverride.AllTargetsOnAttackPath;
			Pierce = abilityOverride.Pierce;
			RetaliateRange = abilityOverride.RetaliateRange;
			AdditionalPushEffect = abilityOverride.AdditionalPushEffect;
			AdditionalPushEffectDamage = abilityOverride.AdditionalPushEffectDamage;
			AdditionalPushEffectXP = abilityOverride.AdditionalPushEffectXP;
			ConditionDuration = abilityOverride.ConditionDuration;
			ConditionDecTrigger = abilityOverride.ConditionDecTrigger;
		}

		public SEventAbilityOverrideData(CAbility ability)
		{
			ID = null;
			AbilityType = ability.AbilityType;
			Strength = ability.Strength;
			Range = ability.Range;
			AllTargetsOnMovePath = ability.AllTargetsOnMovePath;
			AllTargetsOnAttackPath = ability.AllTargetsOnAttackPath;
			PositiveConditions = ability.PositiveConditions.Keys.ToList();
			NegativeConditions = ability.NegativeConditions.Keys.ToList();
			Jump = ((ability is CAbilityMove cAbilityMove) ? new bool?(cAbilityMove.Jump) : ((bool?)null));
			Fly = ((ability is CAbilityMove cAbilityMove2) ? new bool?(cAbilityMove2.Fly) : ((bool?)null));
			Pierce = ((ability is CAbilityAttack cAbilityAttack) ? new int?(cAbilityAttack.Pierce) : ((int?)null));
			NumberOfTargets = null;
			RetaliateRange = null;
			AdditionalPushEffect = null;
			AdditionalPushEffectDamage = null;
			AdditionalPushEffectXP = null;
			ConditionDuration = null;
			ConditionDecTrigger = null;
		}
	}

	public Guid AbilityGUID;

	public CActor.EType? MainActorType;

	public ESESubTypeAction SubTypeAction { get; private set; }

	public string CurrentPhaseAbilityName { get; private set; }

	public string DefaultAbility { get; private set; }

	public List<SEventActionAugmentationData> ActionAugmentationDatas { get; private set; }

	public string CardName { get; private set; }

	public bool TopAction { get; private set; }

	public bool DefaultAction { get; private set; }

	public CBaseCard.ECardType CardType { get; private set; }

	public string MainActorClassID { get; private set; }

	public bool Ended { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubTypeAction", SubTypeAction);
		info.AddValue("CurrentPhaseAbilityName", CurrentPhaseAbilityName);
		info.AddValue("DefaultAbility", DefaultAbility);
		info.AddValue("ActionAugmentationDatas", ActionAugmentationDatas);
		info.AddValue("AbilityGUID", AbilityGUID);
		info.AddValue("MainActorClassID", MainActorClassID);
		info.AddValue("MainActorType", MainActorType);
		info.AddValue("CardType", CardType);
		info.AddValue("Ended", Ended);
	}

	public SEventAction(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "MainActorClassID":
					MainActorClassID = info.GetString("MainActorClassID");
					break;
				case "MainActorType":
					MainActorType = (CActor.EType?)info.GetValue("MainActorType", typeof(CActor.EType?));
					break;
				case "CardType":
					CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
					break;
				case "CardName":
					CardName = info.GetString("CardName");
					break;
				case "TopAction":
					TopAction = info.GetBoolean("TopAction");
					break;
				case "Ended":
					Ended = info.GetBoolean("Ended");
					break;
				case "DefaultAction":
					DefaultAction = info.GetBoolean("DefaultAction");
					break;
				case "AbilityGUID":
					AbilityGUID = (Guid)info.GetValue("AbilityGUID", typeof(Guid));
					break;
				case "SubTypeAction":
					SubTypeAction = (ESESubTypeAction)info.GetValue("SubTypeAction", typeof(ESESubTypeAction));
					break;
				case "CurrentPhaseAbilityName":
					CurrentPhaseAbilityName = info.GetString("CurrentPhaseAbilityName");
					break;
				case "DefaultAbility":
					DefaultAbility = info.GetString("DefaultAbility");
					break;
				case "ActionAugmentationDatas":
					ActionAugmentationDatas = (List<SEventActionAugmentationData>)info.GetValue("ActionAugmentationDatas", typeof(List<SEventActionAugmentationData>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAction entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (ActionAugmentationDatas == null)
		{
			ActionAugmentationDatas = new List<SEventActionAugmentationData>();
		}
	}

	public SEventAction(ESESubTypeAction subTypeAction, string currentPhaseAbilityName, List<CActionAugmentation> actionAugmentations, Guid abilityGuid, CActor actor, CAbility ability, bool ended = false)
		: this(subTypeAction, currentPhaseAbilityName, actionAugmentations, abilityGuid, actor, ability?.GetAbilityBaseCard()?.Name, ability?.GetAbilityBaseCard() is CAbilityCard cAbilityCard && cAbilityCard.GetTopActionAbilities().Any((CAbility x) => x.Name.Equals(ability?.Name, StringComparison.InvariantCultureIgnoreCase)), ability?.Name?.Contains("Default", StringComparison.InvariantCultureIgnoreCase) == true, actor?.Class?.ID, actor?.OriginalType, (ability?.GetAbilityBaseCard()?.CardType).GetValueOrDefault(), ended)
	{
	}

	private SEventAction(ESESubTypeAction subTypeAction, string currentPhaseAbilityName, List<CActionAugmentation> actionAugmentations, Guid abilityGuid, CActor actor, string cardName, bool topAction, bool defaultAction, string mainActorClassID, CActor.EType? mainActorType, CBaseCard.ECardType cardType, bool ended, string text = "", string defaultAbility = "")
		: base(ESEType.Action, text)
	{
		SubTypeAction = subTypeAction;
		CurrentPhaseAbilityName = currentPhaseAbilityName;
		DefaultAbility = defaultAbility;
		AbilityGUID = abilityGuid;
		CardName = cardName;
		TopAction = topAction;
		DefaultAction = defaultAction;
		MainActorClassID = mainActorClassID;
		MainActorType = mainActorType;
		CardType = cardType;
		Ended = ended;
		ActionAugmentationDatas = new List<SEventActionAugmentationData>();
		foreach (CActionAugmentation actionAugmentation in actionAugmentations)
		{
			List<ElementInfusionBoardManager.EElement> list = actionAugmentation.Elements.ToList();
			if (actor != null)
			{
				list.RemoveAll((ElementInfusionBoardManager.EElement x) => !actor.CanConsume(x));
			}
			ActionAugmentationDatas.Add(new SEventActionAugmentationData(list, actionAugmentation.AugmentationOps, actionAugmentation.XP));
		}
	}
}
