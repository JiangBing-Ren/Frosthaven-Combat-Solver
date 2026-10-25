using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH063 : ScenarioScript
{
	private CActor _crain;

	private CObjective_Scripted _crainDiedObjective;

	private CScenarioModifier _buffAttackRule;

	private CScenarioModifier _debuffAttackRule;

	private List<CScenarioModifier> _specialRules = new List<CScenarioModifier>();

	private readonly string tuningDeviceID = "TuningDevice";

	private readonly string phaseVfxPropID = "S63EnvVFX";

	private CObjectProp _phaseVfxProp;

	protected override bool Initialize()
	{
		_crain = ScenarioManager.CurrentScenarioState.AllyMonsters[0].Actor;
		_specialRules.Add(GetScenarioModifier("SpecialRule1"));
		_specialRules.Add(GetScenarioModifier("SpecialRule2"));
		_specialRules.Add(GetScenarioModifier("SpecialRule3"));
		_crainDiedObjective = GetObjective("CrainDied") as CObjective_Scripted;
		_crainDiedObjective.ScriptedIsComplete = false;
		_buffAttackRule = GetScenarioModifier("BuffAttack");
		_debuffAttackRule = GetScenarioModifier("DebuffAttack");
		TryPlaceTuningDevice();
		UpdateTuningDeviceVFX();
		return true;
	}

	private void TryPlaceTuningDevice()
	{
		if (!_crain.Tile.m_Props.Any((CObjectProp p) => p.PrefabName == tuningDeviceID))
		{
			CObjectProp prop = new CObjectProp(tuningDeviceID, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(_crain.ArrayIndex), null, null, null, _crain.Tile.m_HexMap.MapGuid);
			_crain.Tile.SpawnProp(prop);
		}
	}

	private void UpdateTuningDeviceVFX()
	{
		if (_phaseVfxProp == null)
		{
			_phaseVfxProp = _crain.Tile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == phaseVfxPropID);
			if (_phaseVfxProp == null)
			{
				if (ScenarioScript.RoundNumber < _specialRules[0].ActivationRound)
				{
					return;
				}
				_phaseVfxProp = SpawnVFXProp(_crain.Tile, phaseVfxPropID);
			}
		}
		string stateName;
		if (ScenarioScript.RoundNumber >= _specialRules[2].ActivationRound)
		{
			stateName = "StateRound10";
		}
		else
		{
			stateName = ((ScenarioScript.RoundNumber < _specialRules[1].ActivationRound) ? "StateRound4" : "StateRound8");
		}
		ScenarioRuleClient.MessageHandler(new CChangePropVisualState_MessageData
		{
			m_prop = _phaseVfxProp,
			m_stateName = stateName
		});
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor == _crain)
		{
			_crainDiedObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound || !OncePerRound())
		{
			return;
		}
		foreach (CScenarioModifier specialRule in _specialRules)
		{
			if (ScenarioScript.RoundNumber >= specialRule.ActivationRound)
			{
				specialRule.SetHiddenState(shouldSetHidden: false);
			}
		}
		UpdateTuningDeviceVFX();
		foreach (EnemyState monster in ScenarioManager.CurrentScenarioState.Monsters)
		{
			Debuff(monster.Actor);
		}
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			Buff(player.Actor);
		}
		foreach (HeroSummonState heroSummon in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			Buff(heroSummon.Actor);
		}
		if (ScenarioScript.RoundNumber == 5)
		{
			GameState.TriggerCampaignSection("FS_063_The_Savvas_Seal_End_Round_4");
		}
	}

	public override void OnHeroSummonFinished(CHeroSummonActor actor)
	{
		Buff(actor);
	}

	public override bool IsActorInteracting(CActor actor)
	{
		return actor == _crain;
	}

	private void Debuff(CActor actor)
	{
		if (ScenarioScript.RoundNumber >= _specialRules[0].ActivationRound)
		{
			actor.Tokens.AddNegativeToken(CCondition.ENegativeCondition.Wound, int.MaxValue, EConditionDecTrigger._Wound_Unremovable, actor);
		}
		if (ScenarioScript.RoundNumber >= _specialRules[1].ActivationRound)
		{
			actor.Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disadvantage, int.MaxValue, EConditionDecTrigger.Never, actor);
		}
		if (ScenarioScript.RoundNumber >= _specialRules[2].ActivationRound)
		{
			_debuffAttackRule.PerformScenarioModifier(ScenarioScript.RoundNumber, actor, 2, forceActivate: true);
		}
	}

	private void Buff(CActor actor)
	{
		if (ScenarioScript.RoundNumber >= _specialRules[2].ActivationRound)
		{
			_buffAttackRule.PerformScenarioModifier(ScenarioScript.RoundNumber, actor, 2, forceActivate: true);
		}
	}

	private void SetCrainFocusOverride(CActor actor)
	{
		EnemyState enemyState = ScenarioManager.CurrentScenarioState.AllEnemyStates.FirstOrDefault((EnemyState a) => a.ActorGuid == actor.ActorGuid);
		CAIFocusOverrideDetails cAIFocusOverrideDetails = enemyState.AIFocusOverride;
		if (cAIFocusOverrideDetails == null || cAIFocusOverrideDetails.OverrideType != CAIFocusOverrideDetails.EOverrideType.OverrideFocus || cAIFocusOverrideDetails.OverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.Actor)
		{
			cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
			cAIFocusOverrideDetails.IsDisabled = false;
			enemyState.SetAIFocusOverride(cAIFocusOverrideDetails);
		}
		cAIFocusOverrideDetails.ForcedUseTileIndex = null;
		cAIFocusOverrideDetails.TargetGUID = _crain.ActorGuid;
	}

	private void ClearCrainFocusOverride(CActor actor)
	{
		CAIFocusOverrideDetails.ClearAIFocusOverride(actor);
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		if (actor.IsEnemyMonsterType)
		{
			Debuff(actor);
		}
	}

	protected override void TickAbilityStartedInternal()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			base.TickAbilityStartedInternal();
			CAbility tickAbility = _tickAbility;
			CActor targetingActor = tickAbility.TargetingActor;
			if (!(targetingActor is CEnemyActor) || targetingActor.Type != CActor.EType.Enemy || targetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm) || targetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				return;
			}
			CEnemyActor cEnemyActor = targetingActor as CEnemyActor;
			if (!(tickAbility.GetAbilityBaseCard() is CMonsterAbilityCard cMonsterAbilityCard))
			{
				return;
			}
			ClearCrainFocusOverride(targetingActor);
			int num = -1;
			CAbilityMove cAbilityMove = null;
			CAbilityAttack cAbilityAttack = null;
			for (int i = 0; i < cMonsterAbilityCard.Action.Abilities.Count; i++)
			{
				if (cMonsterAbilityCard.Action.Abilities[i].ID == tickAbility.ID)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				return;
			}
			for (int j = num; j < cMonsterAbilityCard.Action.Abilities.Count; j++)
			{
				CAbility cAbility = cMonsterAbilityCard.Action.Abilities[j];
				if (cAbility is CAbilityAttack cAbilityAttack2)
				{
					cAbilityAttack = cAbilityAttack2;
					break;
				}
				if (cAbility is CAbilityMove cAbilityMove2 && !targetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize))
				{
					if (cAbilityMove != null)
					{
						break;
					}
					cAbilityMove = cAbilityMove2;
				}
			}
			if (cAbilityAttack == null)
			{
				return;
			}
			List<Point> list = new List<Point> { targetingActor.ArrayIndex };
			if (cAbilityMove != null)
			{
				CAbilityMove cAbilityMove3 = cEnemyActor.CloneAbilityAndApplyBaseStats(cAbilityMove) as CAbilityMove;
				cAbilityMove3.AbilityStartListenersInvoked = true;
				cAbilityMove3.Start(targetingActor, targetingActor);
				CAIMoveNew cAIMoveNew = new CAIMoveNew(targetingActor, cAbilityMove3.Strength, cAbilityMove3.Jump, targetingActor.FlyingAfterConditions, cAbilityMove3.IgnoreDifficultTerrain, ignoreHazardousTerrain: true, cAbilityMove3.WaterTilesThatCostZero, isWaterMonster: false, isPlayer: false, cAbilityMove3.Range, allowMove: false, null, moveTest: false);
				cAIMoveNew.Process();
				list.AddRange(cAIMoveNew.ReachablePoints);
			}
			CAbilityAttack cAbilityAttack3 = cEnemyActor.CloneAbilityAndApplyBaseStats(cAbilityAttack) as CAbilityAttack;
			cAbilityAttack3.AbilityStartListenersInvoked = true;
			cAbilityAttack3.Start(targetingActor, targetingActor);
			foreach (Point item in list)
			{
				targetingActor.OverrideArrayIndex = item;
				CAIMoveNew cAIMoveNew2 = new CAIMoveNew(targetingActor, 0, jump: false, fly: false, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, isWaterMonster: false, isPlayer: false, cAbilityAttack3.Range, allowMove: false, cAbilityAttack3, moveTest: false);
				cAIMoveNew2.Process();
				targetingActor.OverrideArrayIndex = default;
				if (cAIMoveNew2.ActorsCanAttackWithThisCalc().FirstOrDefault((CActor x) => x == _crain) != null)
				{
					SetCrainFocusOverride(targetingActor);
					break;
				}
			}
			targetingActor.AIMoveFocusActors?.Clear();
			targetingActor.AIMoveFocusWaypoints?.Clear();
			targetingActor.AIMoveFocusPath = new List<Point>();
		}
	}
}
