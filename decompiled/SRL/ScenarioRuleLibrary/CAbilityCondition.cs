using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityCondition : CAbilityTargeting
{
	public static readonly EConditionDecTrigger[] ConditionDecTriggers = (EConditionDecTrigger[])Enum.GetValues(typeof(EConditionDecTrigger));

	public EAbilityType thisability;

	[JsonProperty]
	public int Duration { get; set; }

	[JsonProperty]
	public EConditionDecTrigger DecrementTrigger { get; set; }

	public bool IsMapCondition => GetMiscVar("IsMapCondition").GetBool();

	public CAbilityCondition(EAbilityType abilityType, int duration, EConditionDecTrigger decTrigger)
		: base(abilityType)
	{
		thisability = abilityType;
		Duration = duration;
		DecrementTrigger = decTrigger;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (NegativeAbilityIsMandatory && TargetingActor.Type == CActor.EType.Player && AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !IsPositive() && !TargetingActor.Immunities.Contains(AbilityType))
		{
			m_CanSkip = false;
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddCondition, CActiveBonus.EActiveBonusBehaviourType.MultiplyAddedConditions))
		{
			if (Strength == 0)
			{
				Strength = 1;
			}
			Strength += item.ReferenceStrength(this, TargetingActor);
		}
		SetHasStarted(val: true);
	}

	protected void LogIsApplying(List<CActor> actorsAppliedTo)
	{
		string secondaryActorClassID = "";
		CActor.EType secondaryActorType = CActor.EType.Unknown;
		bool secondaryActorIsSummon = false;
		CActor actor = null;
		if (ActorsTargeted.Count > 0)
		{
			actor = ActorsTargeted[ActorsTargeted.Count - 1];
			secondaryActorClassID = actor.Class.ID;
			secondaryActorType = actor.Type;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor != null)
				{
					secondaryActorIsSummon = cEnemyActor.IsSummon;
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(new SEventAbility.SEventAbilityBaseData(TargetingActor, ESESubTypeAbility.ActorIsApplying, this, defaultAction: false, 1, isBuff: false, secondaryActorClassID, secondaryActorType, secondaryActorIsSummon), DecrementTrigger, Duration));
	}

	protected void LogApplyToActor(CActor actor)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(new SEventAbility.SEventAbilityBaseData(TargetingActor, ESESubTypeAbility.ApplyToActor, this, actor), DecrementTrigger, Duration));
	}

	private bool CalcPrevented(List<CItem> usedItems, List<CActiveBonus> usedActiveBonuses)
	{
		if (!usedActiveBonuses.Any((CActiveBonus usedBonus) => usedBonus.Ability.ActiveBonusData.GetVar("PreventCondition").GetBool()))
		{
			return usedItems.Any((CItem item) => item.GetVar("PreventCondition").GetBool());
		}
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		bool flag = false;
		List<CItem> usedItems = new List<CItem>();
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (TargetingActor != actor && TargetingActor is CEnemyActor)
		{
			GameState.ShowItemSelection(actor, actor, this, IsPositive() ? CItem.EItemTrigger.OnPositiveConditionFromEnemy : CItem.EItemTrigger.OnNegativeConditionFromEnemy, onlyInActionPhase: true, killAfter: false, null, usedItems);
		}
		if (TargetingActor != null)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (!CalcPrevented(usedItems, list))
				{
					GameState.ShowItemSelection(playerActor, actor, this, IsPositive() ? CItem.EItemTrigger.OnPositiveConditionGlobal : CItem.EItemTrigger.OnNegativeConditionGlobal, onlyInActionPhase: true, killAfter: false, null, usedItems);
				}
			}
		}
		if (!IsPositive())
		{
			GameState.ShowPerkSelection(actor, actor, this, EPerkFlag.OnNegativeCondition);
			if (TargetingActor == actor)
			{
				GameState.ShowPerkSelection(actor, actor, this, EPerkFlag.OnNegativeConditionCastBySelf);
			}
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				if (!CalcPrevented(usedItems, list))
				{
					GameState.ShowToggleActiveBonusSelection(playerActor2, actor, this, EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.GenericOnNegativeCondition, !(PhaseManager.CurrentPhase is CPhaseShortRest), killAfter: false, null, list);
				}
			}
			list.Clear();
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericOnNegativeCondition, this, actor, actor, list);
			flag = CalcPrevented(usedItems, list);
			if (flag)
			{
				CPreventConditionCombatLog_MessageData message = new CPreventConditionCombatLog_MessageData(TargetingActor, AbilityType, actor);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		bool flag2 = false;
		if (flag && (DecrementTrigger == EConditionDecTrigger.WhenAnimatedClaymoreEnds || DecrementTrigger == EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds))
		{
			flag2 = true;
		}
		if (!flag | flag2)
		{
			List<CActiveBonus> source = (from x in actor.FindApplicableActiveBonuses(EAbilityType.ImmunityTo)
				where x.Ability is CAbilityImmunityTo cAbilityImmunityTo && cAbilityImmunityTo.ImmuneToAbilityTypes.Contains(AbilityType) && x.IsActiveBonusToggledAndNotRestricted(actor)
				select x).ToList();
			if (base.ApplyToActor(actor))
			{
				if (AbilityType != EAbilityType.Sleep && actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
				{
					actor.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep, EConditionDecTrigger.None, TargetingActor);
					CActorAwakened_MessageData message2 = new CActorAwakened_MessageData(actor);
					ScenarioRuleClient.MessageHandler(message2);
				}
				return true;
			}
			if (!source.Any((CActiveBonus x) => x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.ChooseCondition)))
			{
				CPreventConditionCombatLog_MessageData message3 = new CPreventConditionCombatLog_MessageData(TargetingActor, AbilityType, actor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			if (ActorsTargeted == null)
			{
				ActorsTargeted = new List<CActor>();
			}
			ActorsTargeted.Add(actor);
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		if (m_NegativeConditions.Count > 0)
		{
			ProcessNegativeStatusEffects(actor);
		}
		return false;
	}

	public bool AbilityTypeEqualsEnhancement(EEnhancement enhancementType)
	{
		return enhancementType switch
		{
			EEnhancement.Bless => AbilityType == EAbilityType.Bless, 
			EEnhancement.Curse => AbilityType == EAbilityType.Curse, 
			EEnhancement.Disarm => AbilityType == EAbilityType.Disarm, 
			EEnhancement.Immobilize => AbilityType == EAbilityType.Immobilize, 
			EEnhancement.Muddle => AbilityType == EAbilityType.Muddle, 
			EEnhancement.Poison => AbilityType == EAbilityType.Poison, 
			EEnhancement.Strengthen => AbilityType == EAbilityType.Strengthen, 
			EEnhancement.Regenerate => AbilityType == EAbilityType.Regenerate, 
			EEnhancement.Ward => AbilityType == EAbilityType.Ward, 
			EEnhancement.Wound => AbilityType == EAbilityType.Wound, 
			_ => false, 
		};
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type != CActor.EType.Player)
		{
			CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditionFromAbility(AbilityType);
			if (ePositiveCondition == CCondition.EPositiveCondition.NA)
			{
				base.SortValidActorsInRange(actorApplying, ref validActorsInRange);
			}
			else
			{
				SortValidActorsInRangeForCondition(actorApplying, ref validActorsInRange, ePositiveCondition, this);
			}
		}
	}

	public override bool ShouldSortValidActorsInRangeByInitiative()
	{
		return true;
	}

	public static void SortValidActorsInRangeForCondition(CActor actorApplying, ref List<CActor> validActorsInRange, CCondition.EPositiveCondition condition, CAbility ability)
	{
		List<CActor> list = validActorsInRange.Where((CActor x) => !CAbility.ImmuneToAbility(x, ability)).ToList();
		List<CActor> collection = validActorsInRange.Except(list).ToList();
		List<CActor> list2 = list.Where((CActor x) => !x.Tokens.HasKey(condition)).ToList();
		List<CActor> collection2 = list.Except(list2).ToList();
		validActorsInRange.Clear();
		validActorsInRange.AddRange(list2.OrderBy((CActor x) => x.GetDistanceTo(actorApplying)));
		validActorsInRange.AddRange(collection2);
		validActorsInRange.AddRange(collection);
	}
}
