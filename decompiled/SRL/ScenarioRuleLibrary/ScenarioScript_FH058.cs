using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH058 : ScenarioScript
{
	private static int ORPHAN_DAMAGE_RANGE = 2;

	private static Point hulkingPillarPos = new Point(6, 5);

	private EnemyState _orphan;

	private CObjective_Scripted _looseObjective;

	private ObjectState _hulkingPillar;

	private CObjectDoor _door1;

	private CScenarioModifier[] _specialRules;

	private CObjectProp _orphanTargetProp;

	private bool _showNarrativeEvent;

	private int HealAndDamageValue => (int)Math.Ceiling((float)(ScenarioManager.CurrentScenarioState.Level + 2) / 2f);

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Props;
		List<ObjectState> objects = ScenarioManager.CurrentScenarioState.Objects;
		_door1 = GetProp(9, 5, "ThinDoor") as CObjectDoor;
		_hulkingPillar = objects.Find((ObjectState o) => o.ClassID == "HulkingPillarID");
		_orphan = ScenarioManager.CurrentScenarioState.Monsters.Find((EnemyState m) => m.ClassID == "TheOrphanID");
		_orphanTargetProp = GetProp(17, 6);
		_looseObjective = (CObjective_Scripted)ScenarioManager.CurrentScenarioState.LoseObjectives[0];
		_looseObjective.CustomLocKeyArgs = new List<object> { _orphan.Actor.OriginalMaxHealth };
		_specialRules = new CScenarioModifier[5]
		{
			GetScenarioModifier("specialRule1"),
			GetScenarioModifier("specialRule2"),
			GetScenarioModifier("specialRule3"),
			GetScenarioModifier("specialRule4"),
			GetScenarioModifier("specialRule5")
		};
		_specialRules[2].CustomLocKeyArgs = new List<object> { HealAndDamageValue };
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		RefreshOrphanImmunity();
		UpdateRulesAndObjectivesVisibility();
		CAIFocusOverrideDetails aIFocusOverride = _orphan.AIFocusOverride;
		aIFocusOverride.OverrideType = CAIFocusOverrideDetails.EOverrideType.None;
		aIFocusOverride.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.None;
		aIFocusOverride.TargetGUID = null;
		aIFocusOverride.FocusBenign = false;
		_orphan.SetAIFocusOverride(aIFocusOverride);
	}

	private void RefreshOrphanImmunity()
	{
		if (!(_orphan.Actor is CEnemyActor cEnemyActor))
		{
			return;
		}
		cEnemyActor.IsImmuneToDamage = _door1.DoorIsLocked;
		List<CActiveBonus> list = (from x in cEnemyActor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo)
			where x.Ability.Name.Equals("OrphanMoveImmunity", StringComparison.InvariantCultureIgnoreCase)
			select x).ToList();
		if (_door1.DoorIsLocked)
		{
			if (!list.Any())
			{
				CAbility cAbility = CAbilityImmunityTo.CreateDefaultImmunityTo(new List<CAbility.EAbilityType> { CAbility.EAbilityType.Move }, new List<CAbility.EAttackType>(), immuneToSelfDamage: false);
				cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				cAbility.ActiveBonusData.Hidden = true;
				cAbility.Name = "OrphanMoveImmunity";
				cAbility.Start(cEnemyActor, cEnemyActor);
				cAbility.ApplyToActor(cEnemyActor);
			}
			return;
		}
		foreach (CActiveBonus item in list)
		{
			item.Finish();
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickPhaseStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 100);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseStartTurn()
	{
		CheckLooseObjective();
		CheckDoorsRulesAndObjectives();
	}

	protected void TickPhaseEndTurn()
	{
		if (_tickActor == _orphan.Actor && !_door1.DoorIsLocked)
		{
			HealSelfAndDamageMonsters(HealAndDamageValue, HealAndDamageValue, ORPHAN_DAMAGE_RANGE);
		}
		CheckLooseObjective();
		CheckDoorsRulesAndObjectives();
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility.TargetingActor == _orphan.Actor)
		{
			CAIFocusOverrideDetails aIFocusOverride = _orphan.AIFocusOverride;
			if (_tickAbility is CAbilityMove)
			{
				aIFocusOverride.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
				aIFocusOverride.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop;
				aIFocusOverride.TargetGUID = _orphanTargetProp.PropGuid;
				aIFocusOverride.FocusBenign = true;
			}
			else
			{
				aIFocusOverride.OverrideType = CAIFocusOverrideDetails.EOverrideType.None;
				aIFocusOverride.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.None;
				aIFocusOverride.TargetGUID = null;
				aIFocusOverride.FocusBenign = false;
			}
			_orphan.SetAIFocusOverride(aIFocusOverride);
		}
		base.TickAbilityStartedInternal();
	}

	protected override void TickAbilityEndedInternal()
	{
		CheckLooseObjective();
		CheckDoorsRulesAndObjectives();
	}

	protected override void TickRepositionInternal()
	{
		CheckLooseObjective();
		CheckDoorsRulesAndObjectives();
	}

	protected override void TickDeathInternal()
	{
		if (_hulkingPillar.Actor == _tickActor)
		{
			_showNarrativeEvent = true;
			CheckLooseObjective();
			CheckDoorsRulesAndObjectives();
		}
	}

	private void HealSelfAndDamageMonsters(int healAmount, int damageAmount, int range)
	{
		Point arrayIndex = _orphan.Actor.ArrayIndex;
		int num = 0;
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (ScenarioManager.GetTileDistance(arrayIndex.X, arrayIndex.Y, allAliveMonster.ArrayIndex.X, allAliveMonster.ArrayIndex.Y) <= range && allAliveMonster != _orphan.Actor)
			{
				DealScenarioDamage(allAliveMonster, damageAmount, isTrapDamage: false, isTerrainDamage: false, _orphan.Actor.MonsterClass.LocKey, null, null, null, showInflictMessage: false);
				num++;
			}
		}
		if (num > 0)
		{
			HealActor(_orphan.Actor, num * healAmount);
		}
	}

	private void CheckDoorsRulesAndObjectives()
	{
		if (_hulkingPillar.Actor.IsDead && _door1.DoorIsLocked)
		{
			_door1.SetExtraLockState(lockedStateToSet: false);
			RefreshOrphanImmunity();
			DealScenarioDamage(_orphan.Actor, (ScenarioManager.CurrentScenarioState.Level + 3) * ScenarioScript.PartySize);
			ScenarioManager.PathFinder.GetNode(hulkingPillarPos).Blocked = false;
			UpdateRulesAndObjectivesVisibility();
		}
	}

	private void CheckLooseObjective()
	{
		if (_looseObjective.IsActive && !_looseObjective.IsComplete)
		{
			_looseObjective.ScriptedIsComplete = _orphan.Actor.Health >= _orphan.Actor.OriginalMaxHealth;
			_looseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			CheckObjectivesComplete();
		}
	}

	private void HealActor(CActor actor, int strength)
	{
		CAbilityHeal obj = CAbility.CreateAbility(CAbility.EAbilityType.Heal, CAbilityFilterContainer.CreateDefaultFilter(), actor.Type == CActor.EType.Enemy, isTargetedAbility: true, strength) as CAbilityHeal;
		obj.TargetingActor = actor;
		obj.ApplyToActor(actor);
	}

	private void UpdateRulesAndObjectivesVisibility()
	{
		bool flag = !_door1.DoorIsLocked;
		_specialRules[0].SetHiddenState(flag);
		_specialRules[1].SetHiddenState(flag);
		_specialRules[2].SetHiddenState(!flag);
		_specialRules[3].SetHiddenState(!flag);
		_specialRules[4].SetHiddenState(!flag);
		_looseObjective.SetActivation(flag);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (_showNarrativeEvent && message.MessageName == "FS_058_Orphan's_Core_Special_1")
		{
			_showNarrativeEvent = false;
			return true;
		}
		return base.AllowTriggerMessageInternal(ev, message);
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("boss_health_leach", HealAndDamageValue.ToString());
		yield return ("boss_damage_on_orb_destroyed", ((ScenarioManager.CurrentScenarioState.Level + 3) * ScenarioScript.PartySize).ToString());
	}
}
