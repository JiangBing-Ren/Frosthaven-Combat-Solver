using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierApplyConditionToActor : CScenarioModifier
{
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	public EConditionDecTrigger ConditionDecrementTrigger { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("ConditionDecrementTrigger", ConditionDecrementTrigger);
	}

	public CScenarioModifierApplyConditionToActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PositiveConditions":
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "ConditionDecrementTrigger":
					ConditionDecrementTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierApplyConditionToActor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierApplyConditionToActor(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.Turns, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ApplyConditionToActor, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		ConditionDecrementTrigger = conditionDecrementTrigger;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			PerformScenarioModifier(roundCount, allActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || (!IsPositive && IgnoreNegativeScenarioEffects(currentActor)) || (ApplyOnceTotal && HasBeenAppliedOnce) || AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = true;
		if (ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null)
			{
				if (!ScenarioModifierFilter.IsValidTarget(actorState))
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		if (ApplyToEachActorOnce)
		{
			AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
		HasBeenAppliedOnce = true;
		foreach (CCondition.EPositiveCondition posCon in PositiveConditions)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCon.ToString()), new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 0, 1, 1, 0, CAbility.EAbilityFlag.None, 1, ConditionDecrementTrigger);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.SetTreatAsScenarioEffect(val: true);
			cAbility.SetTreatAsScenarioRule(TreatAsScenarioRule);
			cAbility.TreatAsEventEffect = TreatAsEventEffect;
			if (ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario)
			{
				if (cAbility.MiscAbilityData == null)
				{
					cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				cAbility.SkipAnim = true;
				cAbility.SetMiscVar("IsMapCondition", true);
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, currentActor));
			}
			else
			{
				cAbility.Start(currentActor, currentActor);
				bool flag2 = false;
				if ((!(cAbility is CAbilityBless cAbilityBless)) ? ((CAbilityTargeting)cAbility).ApplyToActor(currentActor) : cAbilityBless.ApplyToActor(currentActor, canGoOverBlessLimit: true))
				{
					CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(currentActor, posCon, CCondition.ENegativeCondition.NA, currentActor);
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		foreach (CCondition.ENegativeCondition negCon in NegativeConditions)
		{
			CAbility cAbility2 = CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCon.ToString()), new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 0, 1, 1, 0, CAbility.EAbilityFlag.None, 1, ConditionDecrementTrigger);
			cAbility2.IsScenarioModifierAbility = true;
			cAbility2.SetTreatAsScenarioEffect(val: true);
			cAbility2.SetTreatAsScenarioRule(TreatAsScenarioRule);
			cAbility2.TreatAsEventEffect = TreatAsEventEffect;
			if (ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario)
			{
				if (cAbility2.MiscAbilityData == null)
				{
					cAbility2.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				cAbility2.MiscAbilityData.AutotriggerAbility = true;
				cAbility2.SkipAnim = true;
				cAbility2.SetMiscVar("IsMapCondition", true);
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility2, currentActor));
			}
			else
			{
				cAbility2.Start(currentActor, currentActor);
				bool flag3 = false;
				if ((!(cAbility2 is CAbilityCurse cAbilityCurse)) ? ((CAbilityTargeting)cAbility2).ApplyToActor(currentActor) : cAbilityCurse.ApplyToActor(currentActor, canGoOverCurseLimit: true))
				{
					CConditionCombatLog_MessageData message2 = new CConditionCombatLog_MessageData(currentActor, CCondition.EPositiveCondition.NA, negCon, currentActor);
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
		}
	}
}
