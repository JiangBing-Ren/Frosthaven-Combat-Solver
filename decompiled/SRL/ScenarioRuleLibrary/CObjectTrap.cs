using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectTrap : CObjectProp, ISerializable
{
	public bool Damage { get; set; }

	public bool HasCustomDamageSet { get; private set; }

	public int DamageValue { get; private set; }

	public int HealValue { get; set; }

	public List<CCondition.ENegativeCondition> Conditions { get; set; }

	public List<CCondition.EPositiveCondition> PositiveConditions { get; set; }

	public int AdjacentRange { get; set; }

	public int AdjacentDamageValue { get; set; }

	public List<CCondition.ENegativeCondition> AdjacentConditions { get; set; }

	public CAbilityFilterContainer AdjacentFilter { get; set; }

	public int TriggeredXP { get; set; }

	public bool TargetAdjacentActors => AdjacentRange > 0;

	public bool ForceNextActivation { get; set; }

	public bool SkipThisActivation { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Conditions", Conditions);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("Damage", Damage);
		info.AddValue("HasCustomDamageSet", HasCustomDamageSet);
		info.AddValue("DamageValue", DamageValue);
		info.AddValue("HealValue", HealValue);
		info.AddValue("AdjacentRange", AdjacentRange);
		info.AddValue("AdjacentDamageValue", AdjacentDamageValue);
		info.AddValue("AdjacentConditions", AdjacentConditions);
		info.AddValue("AdjacentFilter", AdjacentFilter);
		info.AddValue("TriggeredXP", TriggeredXP);
	}

	public CObjectTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Conditions":
					Conditions = (List<CCondition.ENegativeCondition>)info.GetValue("Conditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "Damage":
					Damage = info.GetBoolean("Damage");
					break;
				case "HasCustomDamageSet":
					HasCustomDamageSet = info.GetBoolean("HasCustomDamageSet");
					break;
				case "DamageValue":
					DamageValue = info.GetInt32("DamageValue");
					break;
				case "HealValue":
					HealValue = info.GetInt32("HealValue");
					break;
				case "AdjacentRange":
					AdjacentRange = info.GetInt32("AdjacentRange");
					break;
				case "AdjacentDamageValue":
					AdjacentDamageValue = info.GetInt32("AdjacentDamageValue");
					break;
				case "AdjacentConditions":
					AdjacentConditions = (List<CCondition.ENegativeCondition>)info.GetValue("AdjacentConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "AdjacentFilter":
					AdjacentFilter = (CAbilityFilterContainer)info.GetValue("AdjacentFilter", typeof(CAbilityFilterContainer));
					break;
				case "TriggeredXP":
					TriggeredXP = info.GetInt32("TriggeredXP");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectTrap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public void LogActor()
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventTrapActor(HealValue, PropGuid));
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue, int adjacentRange, int adjacentDamageValue, List<CCondition.ENegativeCondition> adjacentConditions, CAbilityFilterContainer adjacentFilter, int triggeredXp)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Damage = damage;
		HasCustomDamageSet = damageValue > 0 || (adjacentRange > 0 && adjacentDamageValue > 0);
		DamageValue = damageValue;
		Conditions = conditions?.ToList();
		AdjacentRange = adjacentRange;
		AdjacentDamageValue = adjacentDamageValue;
		AdjacentConditions = adjacentConditions;
		AdjacentFilter = adjacentFilter;
		TriggeredXP = triggeredXp;
		if (AdjacentFilter == null)
		{
			AdjacentFilter = CAbilityFilterContainer.CreateDefaultFilter();
		}
		UpdateVisualState();
		LogActor();
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, AbilityData.TrapData trapData)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Damage = trapData.Damage > 0 || (trapData.AdjacentRange > 0 && trapData.AdjacentDamage > 0);
		HasCustomDamageSet = trapData.Damage > 0 || (trapData.AdjacentRange > 0 && trapData.AdjacentDamage > 0) || trapData.Heal > 0 || (trapData.PositiveConditions != null && trapData.PositiveConditions.Count > 0);
		DamageValue = trapData.Damage;
		HealValue = trapData.Heal;
		Conditions = trapData.Conditions?.ToList();
		PositiveConditions = trapData.PositiveConditions?.ToList();
		AdjacentRange = trapData.AdjacentRange;
		AdjacentDamageValue = trapData.AdjacentDamage;
		AdjacentConditions = trapData.AdjacentConditions;
		AdjacentFilter = trapData.AdjacentFilter;
		TriggeredXP = trapData.TriggeredXP;
		UpdateVisualState();
		LogActor();
	}

	public CObjectTrap(string name, ScenarioManager.ObjectImportType type, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue)
		: base(name, type, null, null, null, null, mapGuid)
	{
		Damage = damage;
		DamageValue = damageValue;
		Conditions = conditions?.ToList();
		TriggeredXP = 0;
		UpdateVisualState();
		LogActor();
	}

	public CObjectTrap(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<CCondition.ENegativeCondition> conditions, bool damage, int damageValue)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		Damage = damage;
		DamageValue = damageValue;
		Conditions = conditions?.ToList();
		TriggeredXP = 0;
		UpdateVisualState();
		LogActor();
	}

	public void UpdateVisualState()
	{
		CUpdateTrapVisualState_MessageData message = new CUpdateTrapVisualState_MessageData
		{
			m_trap = this,
			m_delay = 0.001f
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public int GetTrapDamage()
	{
		if (Owner == null && !HasCustomDamageSet)
		{
			return ScenarioManager.TrapDamage;
		}
		return DamageValue;
	}

	public void ApplyDamage(int trapDamage, CActor actor, bool actorIsJumping = false, SEventObjectPropTrap trapEvent = null, List<Action> effectQueue = null)
	{
		if (trapDamage <= 0 || !GameState.ActorHealthCheck(actor, actor))
		{
			return;
		}
		List<CAbility.EAbilityType> immunities = actor.Immunities;
		if (immunities != null && immunities.Contains(CAbility.EAbilityType.Damage))
		{
			return;
		}
		int health = actor.Health;
		bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		bool triggeredPreventDamage = false;
		if (trapEvent != null && effectQueue != null)
		{
			int num = trapDamage;
			if (actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
			{
				num *= 2;
			}
			if (actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
			{
				num /= 2;
			}
			trapEvent.Damage = num;
			trapEvent.EffectTextInOrderOfApplication.Add($"<sprite name=Damage>{num}");
			effectQueue.Add(() =>
			{
				ApplyDamage(trapDamage, actor, actorIsJumping);
			});
		}
		else
		{
			CActor trapCredit = GetTrapCredit(actor);
			GameState.ActorBeenDamaged(actor, trapDamage, out triggeredPreventDamage, out var _, checkIfPlayerCanAvoidDamage: true, trapCredit, null, CAbility.EAbilityType.Trap, 0, isTrapDamage: true, isTerrainDamage: false, cannotPreventDamageWithActiveBonus: false, pierceInvulnerable: false, 0, triggerOnDamageListeners: true, 0, actorIsJumping);
			if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None || !Damage) && GameState.ActorHealthCheck(actor, actor, isTrap: true, isTerrain: false, actorWasAsleep) && !triggeredPreventDamage)
			{
				CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
				{
					m_ActorBeingDamaged = actor,
					m_DamageAbility = null,
					m_ActorOriginalHealth = health,
					m_ActorWasAsleep = actorWasAsleep,
					m_skipHitAnimation = actorIsJumping
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public void ApplyHeal(CActor actor, SEventObjectPropTrap trapEvent = null, List<Action> effectQueue = null)
	{
		if (HealValue <= 0 || !GameState.ActorHealthCheck(actor, actor))
		{
			return;
		}
		if (trapEvent != null && effectQueue != null)
		{
			trapEvent.EffectTextInOrderOfApplication.Add($"<sprite name=Heal>{HealValue}");
			effectQueue.Add(() =>
			{
				ApplyHeal(actor);
			});
			return;
		}
		CAbilityHeal obj = CAbilityHeal.CreateDefaultHeal(HealValue, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self)) as CAbilityHeal;
		obj.SkipAnim = true;
		obj.MiscAbilityData = new AbilityData.MiscAbilityData
		{
			AutotriggerAbility = true
		};
		obj.Start(actor, actor);
		obj.ActorIsApplying(actor, new List<CActor> { actor });
		obj.ApplyToActor(actor);
	}

	public void ApplyNegativeConditions(CActor actor, List<CCondition.ENegativeCondition> Conditions, SEventObjectPropTrap trapEvent = null, List<Action> effectQueue = null)
	{
		if (Conditions == null || Conditions.Count == 0 || !GameState.ActorHealthCheck(actor, actor))
		{
			return;
		}
		CActor cActor = ((Owner == null) ? actor : Owner);
		foreach (CCondition.ENegativeCondition condition in Conditions)
		{
			CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition.ToString());
			if (condition != CCondition.ENegativeCondition.NA && eAbilityType != CAbility.EAbilityType.None)
			{
				if (trapEvent != null && effectQueue != null)
				{
					trapEvent.EffectTextInOrderOfApplication.Add($"<sprite name={condition}>");
					effectQueue.Add(() =>
					{
						ApplyNegativeConditions(actor, new List<CCondition.ENegativeCondition> { condition });
					});
					continue;
				}
				CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
				cAbility.Start(cActor, cActor);
				if (((CAbilityTargeting)cAbility).ApplyToActor(actor))
				{
					actor.m_OnConditionApplyToActorListeners?.Invoke(cAbility, actor);
				}
				if (eAbilityType == CAbility.EAbilityType.Stun || eAbilityType == CAbility.EAbilityType.Immobilize || eAbilityType == CAbility.EAbilityType.Sleep)
				{
					CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				Log.ErrorWithReport("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
			}
		}
	}

	public void ApplyPositiveConditions(CActor actor, List<CCondition.EPositiveCondition> PositiveConditions, SEventObjectPropTrap trapEvent = null, List<Action> effectQueue = null)
	{
		if (PositiveConditions == null || PositiveConditions.Count == 0 || !GameState.ActorHealthCheck(actor, actor))
		{
			return;
		}
		CActor cActor = ((Owner == null) ? actor : Owner);
		foreach (CCondition.EPositiveCondition condition in PositiveConditions)
		{
			CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition.ToString());
			if (condition != CCondition.EPositiveCondition.NA && eAbilityType != CAbility.EAbilityType.None)
			{
				if (trapEvent != null && effectQueue != null)
				{
					trapEvent.EffectTextInOrderOfApplication.Add($"<sprite name={condition}>");
					effectQueue.Add(() =>
					{
						ApplyPositiveConditions(actor, new List<CCondition.EPositiveCondition> { condition });
					});
					continue;
				}
				CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
				cAbility.Start(cActor, cActor);
				if (((CAbilityTargeting)cAbility).ApplyToActor(actor))
				{
					actor.m_OnConditionApplyToActorListeners?.Invoke(cAbility, actor);
				}
			}
			else
			{
				Log.ErrorWithReport("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
			}
		}
	}

	public bool ForceActivate(CActor actor, CActor creditActor = null)
	{
		ForceNextActivation = true;
		return Activate(actor, creditActor);
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		bool forceNextActivation = ForceNextActivation;
		ForceNextActivation = false;
		if (!forceNextActivation && Activated)
		{
			return false;
		}
		ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
		int num = 0;
		if (Damage)
		{
			num = GetTrapDamage();
		}
		string text = "";
		string overrideInternalActor = "";
		CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
		if (cPhaseAction != null && cPhaseAction.CurrentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
		{
			text = cAbilityCard.ClassID;
			overrideInternalActor = cAbilityCard.ClassID;
		}
		SEventObjectPropTrap sEventObjectPropTrap = null;
		List<Action> list = new List<Action>();
		bool flag = false;
		if (actor != null)
		{
			if (string.IsNullOrEmpty(text))
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					text = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor { Summoner: var summoner })
				{
					text = summoner?.CharacterClass.ID;
				}
				if ((actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2) && GameState.TurnActor != null && GameState.TurnActor.Type == CActor.EType.Player)
				{
					text = GameState.TurnActor.Class.ID;
					overrideInternalActor = GameState.TurnActor.Class.ID;
				}
			}
			sEventObjectPropTrap = new SEventObjectPropTrap(num, text, "Activate", ESESubTypeObjectProp.Activated, ObjectType, PropGuid, PrefabName, m_OwnerGuid, "", overrideInternalActor, ArrayIndex.ToPoint());
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (!forceNextActivation && !WillActivate(actor, actorIsJumping: false, fly))
			{
				return false;
			}
			if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && !ScenarioRuleClient.IsWorkerThread)
			{
				ScenarioRuleClient.AddSRLQueueMessage(new CSRLTrapMessage(this, actor), processImmediately: false);
				return false;
			}
			if (!ScenarioManager.Scenario.HasActor(actor))
			{
				ApplyAdjacentEffects(actor, alreadyAppliedToActor: true);
				return false;
			}
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericTrapActivating, CExpression.Globals.CurrentAbility, actor, actor, ("trap", new TValue(this)));
			if (SkipThisActivation)
			{
				SkipThisActivation = false;
				return false;
			}
			SEventLogMessageHandler.AddEventLogMessage(sEventObjectPropTrap);
			if (Owner != null && TriggeredXP > 0 && !CActor.AreActorsAllied(actor.Type, Owner.Type, actor, Owner))
			{
				Owner.GainXP(TriggeredXP);
			}
			flag = cPhaseAction == null || (!(cPhaseAction.CurrentPhaseAbility?.m_Ability is CAbilityAttack) && !(cPhaseAction.CurrentPhaseAbility?.m_Ability is CAbilityDamage)) || TileIndex.Compare(ArrayIndex, actor.ArrayIndex);
			if (flag)
			{
				if (actor.IsEnemyMonsterType)
				{
					ApplyDamage(num, actor, actorIsJumping, sEventObjectPropTrap, list);
					ApplyHeal(actor, sEventObjectPropTrap, list);
					ApplyPositiveConditions(actor, PositiveConditions, sEventObjectPropTrap, list);
					ApplyNegativeConditions(actor, Conditions, sEventObjectPropTrap, list);
				}
				else
				{
					if (actor.WillActorDie(num, 0, null, includePlayers: true))
					{
						ApplyHeal(actor, sEventObjectPropTrap, list);
						ApplyDamage(num, actor, actorIsJumping, sEventObjectPropTrap, list);
					}
					else
					{
						ApplyDamage(num, actor, actorIsJumping, sEventObjectPropTrap, list);
						ApplyHeal(actor, sEventObjectPropTrap, list);
					}
					ApplyNegativeConditions(actor, Conditions, sEventObjectPropTrap, list);
					ApplyPositiveConditions(actor, PositiveConditions, sEventObjectPropTrap, list);
				}
			}
		}
		base.Activate(actor, creditActor, actorIsJumping, actorIsFlying);
		foreach (Action item in list)
		{
			item();
		}
		if (RuntimeAttachedActor == null)
		{
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericTrapActivated, CExpression.Globals.CurrentAbility, actor, actor, ("trap", new TValue(this)));
		}
		ApplyAdjacentEffects(actor, flag);
		if (RuntimeAttachedActor != null && !RuntimeAttachedActor.IsDead)
		{
			CObjectActor runtimeAttachedActor = RuntimeAttachedActor;
			runtimeAttachedActor.ClearAttachedProp();
			ClearAttachedRuntimeActor();
			GameState.KillActor(runtimeAttachedActor, runtimeAttachedActor, CActor.ECauseOfDeath.Suicide, out var _, null, actorWasAsleep: false, null, instantlyDeletePrefab: false, dontLog: true);
		}
		return false;
	}

	private void ApplyAdjacentEffects(CActor actor, bool alreadyAppliedToActor)
	{
		if (!TargetAdjacentActors)
		{
			return;
		}
		CActor filterActor = ((Owner == null) ? actor : Owner);
		List<CActor> actorsToIgnore = (alreadyAppliedToActor ? new List<CActor> { actor } : new List<CActor>());
		foreach (CActor item in GameState.GetActorsInRange(new Point(ArrayIndex.X, ArrayIndex.Y), filterActor, AdjacentRange, actorsToIgnore, AdjacentFilter, null, null, isTargetedAbility: false, null, true))
		{
			if ((item != RuntimeAttachedActor || RuntimeAttachedActor == null) && !item.IsDead)
			{
				ApplyNegativeConditions(item, AdjacentConditions);
				int trapDamage = ((AdjacentDamageValue > 0) ? AdjacentDamageValue : ScenarioManager.TrapDamage);
				ApplyDamage(trapDamage, item);
			}
		}
	}

	public CActor GetTrapCredit(CActor actor)
	{
		CActor cActor = Owner;
		if (cActor == null || ((actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2) && GameState.InternalCurrentActor.Type == CActor.EType.Player))
		{
			cActor = GameState.InternalCurrentActor;
		}
		return cActor;
	}

	public static bool IgnoreFlyAndJump(CActor actor)
	{
		List<CActiveBonus> list = actor?.FindApplicableActiveBonuses();
		if (list == null)
		{
			return false;
		}
		foreach (CActiveBonus item in list)
		{
			if (item.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.GenericTrapsIgnoreFlyAndJump))
			{
				return true;
			}
		}
		return false;
	}

	public override bool WillActivate(CActor actor, bool actorIsJumping, bool actorIsFlying)
	{
		if (!actorIsJumping && !actorIsFlying)
		{
			return true;
		}
		if (SpecificPropType == ESpecificPropType.DetonationCharge)
		{
			return true;
		}
		if (IgnoreFlyAndJump(actor))
		{
			return true;
		}
		return false;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		int num = 0;
		if (Damage)
		{
			num = ((Owner != null || HasCustomDamageSet) ? DamageValue : ScenarioManager.TrapDamage);
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (fly)
			{
				num = 0;
			}
		}
		return num > 0;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		int incomingDamage = 0;
		if (Damage)
		{
			incomingDamage = ((Owner != null || HasCustomDamageSet) ? DamageValue : ScenarioManager.TrapDamage);
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var _, ref strength);
			if (fly)
			{
				incomingDamage = 0;
			}
		}
		return actor.WillActorDie(incomingDamage);
	}

	public void SetCustomDamageValue(int damageValueToSet)
	{
		HasCustomDamageSet = true;
		DamageValue = damageValueToSet;
		UpdateVisualState();
	}

	public void SetHealValue(int healValue)
	{
		HealValue = healValue;
		UpdateVisualState();
	}

	public void ClearCustomDamage()
	{
		HasCustomDamageSet = false;
	}

	public static List<Tuple<int, string>> Compare(CObjectTrap trap1, CObjectTrap trap2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(trap1, trap2, isMPCompare));
			if (trap1.Damage != trap2.Damage)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2101, "CObjectTrap Damage does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"Damage",
						trap1.Damage.ToString(),
						trap2.Damage.ToString()
					}
				});
			}
			if (trap1.HasCustomDamageSet != trap2.HasCustomDamageSet)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2102, "CObjectTrap HasCustomDamageSet does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"HasCustomDamageSet",
						trap1.HasCustomDamageSet.ToString(),
						trap2.HasCustomDamageSet.ToString()
					}
				});
			}
			if (trap1.DamageValue != trap2.DamageValue)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2103, "CObjectTrap DamageValue does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"DamageValue",
						trap1.DamageValue.ToString(),
						trap2.DamageValue.ToString()
					}
				});
			}
			if (trap1.TriggeredXP != trap2.TriggeredXP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2104, "CObjectTrap TriggeredXP does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"TriggeredXP",
						trap1.TriggeredXP.ToString(),
						trap2.TriggeredXP.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(trap1.Conditions, trap2.Conditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2105, "CObjectTrap Conditions null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
					new string[3]
					{
						"ObjectType",
						trap1.ObjectType.ToString(),
						trap2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
					new string[3]
					{
						"Conditions",
						(trap1.Conditions == null) ? "is null" : "is not null",
						(trap2.Conditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (trap1.Conditions.Count != trap2.Conditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2106, "CObjectTrap total Conditions Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
						new string[3]
						{
							"ObjectType",
							trap1.ObjectType.ToString(),
							trap2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
						new string[3]
						{
							"Conditions Count",
							trap1.Conditions.Count.ToString(),
							trap2.Conditions.Count.ToString()
						}
					});
					break;
				}
				foreach (CCondition.ENegativeCondition condition in trap1.Conditions)
				{
					if (!trap2.Conditions.Contains(condition))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2107, "CObjectTrap Conditions does not match.", new List<string[]>
						{
							new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
							new string[3]
							{
								"ObjectType",
								trap1.ObjectType.ToString(),
								trap2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
							new string[3]
							{
								"NegativeCondition",
								condition.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (CCondition.ENegativeCondition condition2 in trap2.Conditions)
				{
					if (!trap1.Conditions.Contains(condition2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2107, "CObjectTrap Conditions does not match.", new List<string[]>
						{
							new string[3] { "Prop GUID", trap1.PropGuid, trap2.PropGuid },
							new string[3]
							{
								"ObjectType",
								trap1.ObjectType.ToString(),
								trap2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", trap1.PrefabName, trap2.PrefabName },
							new string[3]
							{
								"NegativeCondition",
								"Missing",
								condition2.ToString()
							}
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2199, "Exception during CObjectTrap compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
