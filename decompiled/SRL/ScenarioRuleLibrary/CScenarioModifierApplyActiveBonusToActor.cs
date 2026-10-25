using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierApplyActiveBonusToActor : CScenarioModifier
{
	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierApplyActiveBonusToActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "TileSpecificAbilityIds")
				{
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierApplyActiveBonusToActor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierApplyActiveBonusToActor(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, Dictionary<TileIndex, List<string>> abilityIdsByLocation, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ApplyActiveBonusToActor, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TileSpecificAbilityIds = abilityIdsByLocation?.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value) ?? null;
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

	public bool GetScenarioScriptAtRuntime(out ScenarioScript script)
	{
		if (GetScenarioScriptName(out var name))
		{
			return ScenarioManager.CurrentScenarioState.FindScenarioScript(name, out script);
		}
		script = null;
		return false;
	}

	public bool TryCreateScenarioScript(out ScenarioScript ss)
	{
		ss = null;
		if (GetScenarioScriptName(out var name))
		{
			ss = ScenarioScript.Create(name);
		}
		return ss != null;
	}

	public bool GetScenarioScriptName(out string name)
	{
		name = null;
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return false;
		}
		List<CAbility> list = TriggerAbilities(null, ScenarioManager.CurrentScenarioState.Players.Count, copyAbilities: false);
		if (list != null)
		{
			foreach (CAbility item in list)
			{
				if (item is CAbilityNull || item is CAbilityMove)
				{
					return ScenarioScript.GetName(item.ActiveBonusData, out name);
				}
			}
		}
		return false;
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
		List<CAbility> list = TriggerAbilities(currentActor, ScenarioManager.CurrentScenarioState.Players.Count, copyAbilities: false);
		if (list == null)
		{
			return;
		}
		foreach (CAbility item in list)
		{
			if (item is CAbilityNull)
			{
				foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
				{
					scenarioScript.TickPhase(currentActor, this);
				}
				continue;
			}
			CAbility cAbility = CAbility.CopyAbility(item, generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.SetMiscVar("TreatAsEventEffect", TreatAsEventEffect);
			cAbility.SetMiscVar("TreatAsScenarioRule", TreatAsScenarioRule);
			cAbility.Start(currentActor, currentActor);
			if (!cAbility.AbilityHasBeenCancelled)
			{
				cAbility.ApplyToActor(currentActor);
			}
		}
		HasBeenAppliedOnce = true;
		if (ApplyToEachActorOnce)
		{
			AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
	}

	public override CAbility TriggerAbility(CActor actorPerformingAbility = null, int partySize = 2, bool copyAbility = true)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				CAbility cAbility = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities[0];
				if (copyAbility)
				{
					cAbility = CAbility.CopyAbility(cAbility, generateNewID: false);
				}
				return cAbility;
			}
		}
		return base.TriggerAbility(actorPerformingAbility, partySize, copyAbility);
	}

	public override List<CAbility> TriggerAbilities(CActor actorPerformingAbility = null, int partySize = 2, bool copyAbilities = true)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				List<CAbility> list = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities.ToList();
				if (copyAbilities)
				{
					list = list.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList();
				}
				return list;
			}
		}
		return base.TriggerAbilities(actorPerformingAbility, partySize, copyAbilities);
	}

	public override List<CAbility> AllListedTriggerAbilities(bool copyAbilities = true)
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(ScenarioAbilityID))
		{
			list.Add(ScenarioAbilityID);
		}
		if (TileSpecificAbilityIds != null)
		{
			foreach (TileIndex key in TileSpecificAbilityIds.Keys)
			{
				foreach (string item in TileSpecificAbilityIds[key])
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		List<CAbility> list2 = new List<CAbility>();
		foreach (string abilityID in list)
		{
			ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityID);
			if (scenarioAbilitiesYMLData == null)
			{
				continue;
			}
			foreach (CAbility scenarioAbility in scenarioAbilitiesYMLData.ScenarioAbilities)
			{
				CAbility cAbility = scenarioAbility;
				if (copyAbilities)
				{
					cAbility = CAbility.CopyAbility(cAbility, generateNewID: false);
				}
				list2.Add(cAbility);
				if (cAbility is CAbilityAddActiveBonus { AddAbility: { } cAbility2 })
				{
					if (copyAbilities)
					{
						cAbility2 = CAbility.CopyAbility(cAbility2, generateNewID: false);
					}
					list2.Add(cAbility2);
				}
			}
		}
		return list2 ?? new List<CAbility>();
	}
}
