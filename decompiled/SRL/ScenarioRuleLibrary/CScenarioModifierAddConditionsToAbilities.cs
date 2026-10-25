using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierAddConditionsToAbilities : CScenarioModifier
{
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public CScenarioModifierAddConditionsToAbilities(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PositiveConditions"))
				{
					if (name == "NegativeConditions")
					{
						NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					}
				}
				else
				{
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierAddConditionsToAbilities entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierAddConditionsToAbilities(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.AddConditionsToAbilities, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}

	public bool ShouldAddConditions(CActor currentActor, CAbility.EAbilityType currentAbilityType)
	{
		if (AfterAbilityTypes != null && AfterAbilityTypes.Count > 0 && !AfterAbilityTypes.Contains(currentAbilityType))
		{
			return false;
		}
		if (ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null)
			{
				if (ScenarioModifierFilter.IsValidTarget(actorState))
				{
					return ScenarioModifierFilter.IsValidAbilityType(currentAbilityType);
				}
				return false;
			}
			return false;
		}
		return true;
	}
}
