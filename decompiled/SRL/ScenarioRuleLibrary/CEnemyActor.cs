using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AStar;
using Libs.Utils;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CEnemyActor : CActor
{
	public static CAbility.EAbilityType[] damageAbilityTypes = new CAbility.EAbilityType[3]
	{
		CAbility.EAbilityType.Attack,
		CAbility.EAbilityType.Damage,
		CAbility.EAbilityType.Retaliate
	};

	public CMonsterAbilityCard OverrideRoundAbilityCard;

	private string m_SummonerGuid;

	public int StandeeID { get; set; }

	public new CMonsterClass MonsterClass => (CMonsterClass)m_Class;

	public override int Level => MonsterClass.StatLevel;

	public bool IsSummon { get; private set; }

	public int MonsterGroupType { get; set; }

	public bool ForcedUntargetable { get; set; }

	public bool IsImmuneToDamage { get; set; }

	public override int SelectedRoundCardsCount => 1;

	public override int MaxRoundCardsCount => 1;

	public override bool IgnoreIceSlideEffect
	{
		get
		{
			if (ScenarioScript.current != null)
			{
				return ScenarioScript.current.CanActorIgnoreIceSlideEffect(this);
			}
			return false;
		}
	}

	public new int SubInitiative { get; set; }

	public bool HasShiftedForm { get; set; }

	public override bool Invulnerable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count <= 0)
			{
				return MonsterClass.Invulnerable;
			}
			return true;
		}
	}

	public override bool PierceInvulnerability
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.PierceInvulnerability).Count <= 0)
			{
				return MonsterClass.PierceInvulnerability;
			}
			return true;
		}
	}

	public override bool Untargetable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Untargetable).Count <= 0 && !MonsterClass.Untargetable && !ForcedUntargetable)
			{
				return IsDowned;
			}
			return true;
		}
	}

	public override bool DoesNotBlock
	{
		get
		{
			if (!base.DoesNotBlock)
			{
				return MonsterClass.DoesNotBlock;
			}
			return true;
		}
	}

	public override bool IgnoreActorCollision
	{
		get
		{
			if (!MonsterClass.IgnoreActorCollision)
			{
				return ScenarioScript.current?.CanActorIgnoreCollisions(this) ?? false;
			}
			return true;
		}
	}

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
			List<CActiveBonus> source = CActiveBonus.FindApplicableActiveBonuses(this);
			foreach (CActiveBonus item2 in source.Where((CActiveBonus x) => x.Ability.AbilityType == CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item2.Ability is CAbilityImmunityTo cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (!list.Contains(immuneToAbilityType))
					{
						list.Add(immuneToAbilityType);
					}
				}
			}
			foreach (CAbility.EAbilityType immunity in MonsterClass.Immunities)
			{
				if (!list.Contains(immunity))
				{
					list.Add(immunity);
				}
			}
			if (IsImmuneToDamage)
			{
				CAbility.EAbilityType[] array = damageAbilityTypes;
				foreach (CAbility.EAbilityType item in array)
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			if (source.Any((CActiveBonus x) => x.Ability.AbilityType == CAbility.EAbilityType.PreventDamage && x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate)) && !list.Contains(CAbility.EAbilityType.Retaliate))
			{
				list.Add(CAbility.EAbilityType.Retaliate);
			}
			return list;
		}
	}

	public CEnemyActor Summoner => ScenarioManager.Scenario.AllMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == m_SummonerGuid);

	public CPlayerActor PlayerSummoner => ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == m_SummonerGuid);

	public int AttackStrengthForUI { get; private set; }

	public ItemCardYMLData PetCard
	{
		get
		{
			CMonsterClass monsterClass = MonsterClass;
			return ScenarioRuleClient.SRLYML.PetCards.Where((ItemCardYMLData x) => x.PetUnlockedBy != null).SingleOrDefault((ItemCardYMLData x) => x.PetUnlockedBy == monsterClass.ID || x.PetUnlockedBy == monsterClass.NonEliteVariant?.ID);
		}
	}

	public bool IsCapturable => PetCard != null;

	public override bool IsSummonedByPlayer => PlayerSummoner != null;

	public CEnemyActor(Point startPosition, CMonsterClass monsterClass, EType type, bool isSummon, string summonerGuid, string actorGuid, int currentHealth, int maxHealth, int level, int standeeID, bool initialMapLoad, int chosenModelIndex, string customLocKey = null)
		: base(type, monsterClass, startPosition, currentHealth, maxHealth, 0, actorGuid, chosenModelIndex)
	{
		if (customLocKey != null)
		{
			CustomLocKey = customLocKey;
		}
		IsSummon = isSummon;
		m_SummonerGuid = summonerGuid;
		StandeeID = standeeID;
		if (monsterClass.Advantage)
		{
			Tokens.AddPositiveToken(CCondition.EPositiveCondition.Advantage, int.MaxValue, EConditionDecTrigger.Never, this, this);
		}
		if ((monsterClass.NonEliteVariant != null) ? monsterClass.NonEliteVariant.InitialState : monsterClass.InitialState)
		{
			monsterClass.InitialState = false;
		}
		CalculateAttackStrengthForUI();
		CalculateShield();
		Log.Info("Created a new CEnemyActor with ActorID: " + ID);
	}

	public override void CleanUpBeforeSave()
	{
		base.CleanUpBeforeSave();
		AttackSummary = null;
	}

	public void ShiftEliteForm()
	{
		string text = Regex.Replace(MonsterClass.ID, "EliteID|ID", "");
		text = ((MonsterClass.NonEliteVariant == null) ? (text + "EliteID") : (text + "ID"));
		CMonsterClass cMonsterClass = MonsterClassManager.Find(text);
		m_Class = cMonsterClass;
		HasShiftedForm = true;
		CalculateAttackStrengthForUI();
	}

	public new bool IsElite()
	{
		return MonsterClass.NonEliteVariant != null;
	}

	public override CClass ActiveBonusClass()
	{
		if (MonsterClass.NonEliteVariant == null)
		{
			return MonsterClass;
		}
		return MonsterClass.NonEliteVariant;
	}

	public override int BaseShield()
	{
		MonsterClass.SetBaseStatIsBasedOnXValueUsingCurrent(onlyRefreshRequiredTypes: true);
		return MonsterClass.Shield;
	}

	public override int BaseRetaliate(int distanceToEnemy)
	{
		MonsterClass.SetBaseStatIsBasedOnXValueUsingCurrent(onlyRefreshRequiredTypes: true);
		if (distanceToEnemy > MonsterClass.RetaliateRange)
		{
			return 0;
		}
		return MonsterClass.Retaliate;
	}

	public override int BaseRetaliateRange()
	{
		return MonsterClass.RetaliateRange;
	}

	public override string ActorLocKey()
	{
		if (CustomLocKey != null && CustomLocKey != string.Empty)
		{
			return CustomLocKey;
		}
		return MonsterClass.LocKey;
	}

	public override string GetPrefabName()
	{
		if (string.IsNullOrEmpty(OverrideCharacterModel))
		{
			return m_Class.Models[ChosenModelIndex];
		}
		return OverrideCharacterModel;
	}

	public new CEnemyActor Clone()
	{
		CEnemyActor cEnemyActor = (CEnemyActor)MemberwiseClone();
		cEnemyActor.Inventory.InventoryActor = cEnemyActor;
		return cEnemyActor;
	}

	public override EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage, int targetNumber = 0)
	{
		if (MonsterClass.Advantage)
		{
			addAdvantage = true;
		}
		return Tokens.GetAdvantageStatus(addAdvantage, addDisadvantage, targetNumber);
	}

	public override int Initiative()
	{
		if (InitiativeLockedDuringRound.HasValue)
		{
			return InitiativeLockedDuringRound.Value;
		}
		if (IsMirrorActor(out var mirrorSource, useCacheOnly: true))
		{
			return mirrorSource.Initiative();
		}
		return MonsterClass.Initiative(this, includeOverrideCard: true);
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		CMonsterClass cMonsterClass = MonsterClass;
		if (IsTakingExtraTurn && TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.MonsterAction && !string.IsNullOrEmpty(TakingExtraTurnActionMonsterClassID))
		{
			CMonsterClass cMonsterClass2 = MonsterClassManager.Find(TakingExtraTurnActionMonsterClassID);
			if (cMonsterClass2 != null)
			{
				cMonsterClass = cMonsterClass2;
				if (cMonsterClass.RoundAbilityCard == null)
				{
					cMonsterClass.SelectRoundAbilityCard();
				}
			}
		}
		CMonsterAbilityCard cMonsterAbilityCard = cMonsterClass.RoundAbilityCard;
		if (OverrideRoundAbilityCard != null)
		{
			cMonsterAbilityCard = OverrideRoundAbilityCard;
		}
		cMonsterAbilityCard.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(cMonsterAbilityCard.Action.Infusions, cMonsterAbilityCard.Action.Augmentations, 0), cMonsterAbilityCard, this);
		foreach (CAbility ability in cMonsterAbilityCard.Action.Abilities)
		{
			CAbility item = CloneAbilityAndApplyBaseStats(ability);
			GameState.CurrentAction.Action.Abilities.Add(item);
		}
		CAbility cAbility = GameState.CurrentAction.Action.Abilities.Find((CAbility x) => x.AbilityType == CAbility.EAbilityType.Move);
		CAbility cAbility2 = GameState.CurrentAction.Action.Abilities.Find((CAbility x) => x.AbilityType == CAbility.EAbilityType.Attack);
		if (cAbility != null && cAbility2 != null)
		{
			cAbility.Range = cAbility2.Range;
		}
	}

	public void AbilitySelectionAddConsume(CActionAugmentation consume)
	{
		foreach (CActionAugmentationOp consumeOp in consume.AugmentationOps)
		{
			if (consumeOp.ParentAbilityName == string.Empty)
			{
				if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					CAbility item = CloneAbilityAndApplyBaseStats(consumeOp.Ability);
					GameState.CurrentAction.Action.Abilities.Add(item);
				}
				else
				{
					Log.ErrorWithReport("No parent specified for ability override");
				}
			}
			else
			{
				if (consumeOp.Type != CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					continue;
				}
				if (MonsterClass.RoundAbilityCard.Action.Abilities.SingleOrDefault((CAbility x) => x.Name == consumeOp.ParentAbilityName) != null)
				{
					CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
					CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
					if (eNegativeCondition == CCondition.ENegativeCondition.NA && ePositiveCondition == CCondition.EPositiveCondition.NA)
					{
						CAbility item2 = CloneAbilityAndApplyBaseStats(consumeOp.Ability);
						GameState.CurrentAction.Action.Abilities.Add(item2);
					}
				}
				else
				{
					Log.ErrorWithReport("Unable to find matching parent ability for consume.");
				}
			}
		}
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, Point? freeTileAwayFromCaster = null, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, MonsterClass.IsWaterMonster, Type == EType.HeroSummon || Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		CauseOfDeath = causeOfDeath;
		AttackSummary = attackSummary;
		m_CauseOfDeathAbility = causeOfDeathAbility;
		if (!m_OnDeathAbilityUsed && CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap && PhaseManager.Phase is CPhaseAction cPhaseAction && cPhaseAction != null && MonsterClass.RoundAbilityCard != null)
		{
			List<CAbility> list = new List<CAbility>();
			bool flag = false;
			if (ActorActionHasHappened)
			{
				foreach (CAbility item in MonsterClass.RoundAbilityCard.Action.Abilities.Where((CAbility x) => x.OnDeath))
				{
					CAbility cAbility = CloneAbilityAndApplyBaseStats(item);
					cAbility.ProcessIfDead = true;
					list.Add(cAbility);
					flag = true;
				}
			}
			if (MonsterClass.CurrentMonsterStat.OnDeathAbilities != null && MonsterClass.CurrentMonsterStat.OnDeathAbilities.Count > 0)
			{
				foreach (CAbility onDeathAbility in MonsterClass.CurrentMonsterStat.OnDeathAbilities)
				{
					CAbility cAbility2 = CAbility.CopyAbility(onDeathAbility, generateNewID: false);
					cAbility2.ProcessIfDead = true;
					cAbility2.OnDeath = true;
					list.Add(cAbility2);
				}
			}
			if (list.Count > 0)
			{
				if (!flag)
				{
					CActorDead_MessageData message = new CActorDead_MessageData(this)
					{
						m_Actor = this,
						m_OnDeathAbility = true
					};
					ScenarioRuleClient.MessageHandler(message);
					OnDeathAbilityActorDeadHandled = true;
				}
				m_AfterDeathListeners?.Invoke(this, null);
				TargetingActor = targetingActor;
				m_OnDeathAbilityUsed = true;
				GameState.OverrideCurrentActorForOneAction(this, null, killActorAfterAction: true, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				startedOnDeathAbility = true;
				return false;
			}
		}
		base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility, AttackSummary);
		if (startedOnDeathAbility)
		{
			TargetingActor = targetingActor;
			return false;
		}
		(Class as CMonsterClass).RecycleID(ID);
		return true;
	}

	public override void OnDeathAbilityComplete()
	{
		if (CauseOfDeath == ECauseOfDeath.None)
		{
			Log.ErrorWithReport("Cause of death was not set before calling OnDeathAbilityComplete()");
			CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		base.OnDeath(TargetingActor, CauseOfDeath, out var _, fromDeathAbilityComplete: true, m_CauseOfDeathAbility, AttackSummary);
		(Class as CMonsterClass).RecycleID(ID);
		GameState.KillActorInternal(TargetingActor, this, m_OnDeathAbilityUsed);
		TargetingActor = null;
		m_OnDeathAbilityUsed = false;
		m_CauseOfDeathAbility = null;
	}

	public static List<Point> FindEnemyPath(CActor enemyActor, EType actorType, Point startLocation, Point endLocation, bool jump, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, bool treatTrapsAsObstacles, bool isWaterMonster, out bool foundPath, bool logFailure = false, bool ignoreCannotStop = false, bool fly = false, bool ignoreBlocked = false)
	{
		HashSet<Point> hashSet = new HashSet<Point>();
		HashSet<Point> hashSet2 = new HashSet<Point>();
		if (!jump)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				if (allAliveActor.VisibleBlocksPathing && allAliveActor.IsTransientBlocker && !CActor.AreActorsAllied(actorType, allAliveActor.Type, null, allAliveActor))
				{
					hashSet.AddRange(from x in allAliveActor.GetTilesCanAttackFrom()
						select x.m_ArrayIndex);
				}
			}
			if (enemyActor.HasMirrorActor())
			{
				CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
				if (mirrorAbilityBehaviour != null && (enemyActor == mirrorAbilityBehaviour.ActiveBonus.Actor || enemyActor == mirrorAbilityBehaviour.MirrorTarget))
				{
					CTile[,] tiles = ScenarioManager.Tiles;
					foreach (CTile cTile in tiles)
					{
						if (cTile != null)
						{
							if (!mirrorAbilityBehaviour.IsMoveTileValidForAbilityMirrored(cTile.m_ArrayIndex, enemyActor))
							{
								hashSet.Add(cTile.m_ArrayIndex);
							}
							if (!ignoreCannotStop && !mirrorAbilityBehaviour.IsStopTileValidForAbilityMirrored(cTile.m_ArrayIndex, enemyActor))
							{
								hashSet2.Add(cTile.m_ArrayIndex);
							}
						}
					}
				}
			}
		}
		HashSet<Point> hashSet3 = new HashSet<Point>();
		List<CObjectObstacle> list;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		foreach (CObjectObstacle item in list)
		{
			foreach (TileIndex pathingBlocker in item.PathingBlockers)
			{
				if (item.IgnoresFlyAndJump)
				{
					hashSet3.Add(new Point(pathingBlocker));
				}
			}
		}
		if (treatTrapsAsObstacles)
		{
			CScenarioModifierMakeTrapsObstacles cScenarioModifierMakeTrapsObstacles = ScenarioManager.CurrentScenarioState.ScenarioModifiers.OfType<CScenarioModifierMakeTrapsObstacles>().First();
			foreach (CObjectTrap item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
			{
				if (cScenarioModifierMakeTrapsObstacles.ScenarioModifierFilter.IsValidProp(item2))
				{
					hashSet3.Add(new Point(item2.ArrayIndex));
				}
			}
		}
		if (isWaterMonster)
		{
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile2 in tiles)
			{
				if (cTile2 != null && !cTile2.IsWater && (ScenarioScript.current == null || !ScenarioScript.current.IsUnderwater(cTile2)))
				{
					hashSet3.Add(cTile2.m_ArrayIndex);
				}
			}
		}
		ScenarioScript current5 = ScenarioScript.current;
		if (current5 != null)
		{
			hashSet3.AddRange(current5.CustomFindEnemyPathTransientSuperBlockedToAdd());
		}
		return ScenarioManager.PathFinder.FindPath(enemyActor, startLocation, endLocation, jump | ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false, hashSet.ToList(), hashSet3.ToList(), hashSet2.ToList(), assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: false, ignoreCannotStop, fly);
	}

	public CAbility CloneAbilityAndApplyBaseStats(CAbility ability, bool generateNewID = false)
	{
		return CloneAbilityAndApplyBaseStats(this, ability, MonsterClass, generateNewID);
	}

	public static CAbility CloneAbilityAndApplyBaseStats(CActor TargetingActor, CAbility ability, CMonsterClass MonsterClass, bool generateNewID = false)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID);
		AbilityData.ActiveBonusData activeBonusData = ability.ActiveBonusData;
		if (activeBonusData != null)
		{
			_ = activeBonusData.Duration;
			if (true && ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				return cAbility;
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		int num = 0;
		if (ability.AddAttackBaseStat)
		{
			num = MonsterClass.Attack;
		}
		else
		{
			num = ((ability.AbilityType == CAbility.EAbilityType.Move) ? MonsterClass.Move : ((ability.AbilityType == CAbility.EAbilityType.Attack) ? MonsterClass.Attack : 0));
		}
		if (cAbility is CAbilityAttack && MonsterClass.AttackStatIsBasedOnXEntries != null)
		{
			cAbility.AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			flag |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
			flag2 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.AddTo);
			flag3 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Range);
			flag4 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Range && x.AddTo);
			flag5 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets);
			flag6 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets && x.AddTo);
		}
		else if (cAbility is CAbilityMove && MonsterClass.MoveStatIsBasedOnXEntries != null)
		{
			cAbility.AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			cAbility.SetStatBasedOnX(TargetingActor, MonsterClass.MoveStatIsBasedOnXEntries, cAbility.AbilityFilter);
			flag |= MonsterClass.MoveStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
		}
		if ((ability.StrengthIsBase || !flag) | flag2)
		{
			cAbility.Strength = (ability.StrengthIsBase ? ability.Strength : (num + ability.Strength));
		}
		if ((ability.RangeIsBase || !flag3) | flag4)
		{
			cAbility.Range = ((!ability.RangeIsBase) ? Math.Max(0, MonsterClass.Range + ability.Range) : ((ability.AreaEffect != null && ability.AreaEffect.Melee) ? 1 : ability.Range));
		}
		if ((ability.TargetIsBase || !flag5) | flag6)
		{
			cAbility.NumberTargets = (ability.TargetIsBase ? ability.NumberTargets : MonsterClass.Target);
		}
		if (!ability.RangeIsBase || ability.Range != 1)
		{
			AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.TreatAsMelee != true)
			{
				goto IL_0356;
			}
		}
		if (cAbility.MiscAbilityData == null)
		{
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
		}
		cAbility.MiscAbilityData.TreatAsMelee = true;
		goto IL_0356;
		IL_0356:
		if (cAbility is CAbilityAttack cAbilityAttack)
		{
			cAbilityAttack.Pierce += MonsterClass.Pierce;
		}
		if (ability.AbilityType == CAbility.EAbilityType.Move)
		{
			((CAbilityMove)cAbility).Fly = MonsterClass.Flying;
		}
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			if (MonsterClass.Push > 0)
			{
				CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.Push, cAbility.AbilityFilter, isMonster: true, cAbility.IsTargetedAbility, MonsterClass.Push, 99, cAbility.NumberTargets);
				cAbility2.IsInlineSubAbility = true;
				cAbility.SubAbilities.Add(cAbility2);
			}
			foreach (CCondition.ENegativeCondition condition in MonsterClass.Conditions)
			{
				if (condition == CCondition.ENegativeCondition.NA)
				{
					continue;
				}
				if (condition == CCondition.ENegativeCondition.Curse && cAbility.NegativeConditions.ContainsKey(condition))
				{
					if (cAbility.NegativeConditions[condition].Strength == 0)
					{
						cAbility.NegativeConditions[condition].Strength++;
					}
					cAbility.NegativeConditions[condition].Strength++;
					continue;
				}
				CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == condition.ToString());
				if (eAbilityType != CAbility.EAbilityType.None && !cAbility.NegativeConditions.ContainsKey(condition))
				{
					cAbility.NegativeConditions.Add(condition, CAbility.CreateAbility(eAbilityType, cAbility.AbilityFilter, isMonster: true, cAbility.IsTargetedAbility));
				}
			}
		}
		return cAbility;
	}

	public override void CalculateAttackStrengthForUI()
	{
		if (MonsterClass.AttackStatIsBasedOnXEntries != null && MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && (!x.AddTo || !CAbility.StatIsXTypesToIgnoreIfAddTo.Contains(x.BasedOn))))
		{
			CAbility cAbility = CAbilityAttack.CreateDefaultAttack(MonsterClass.Attack, 1, 1, isMonster: false);
			cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			AbilityData.StatIsBasedOnXData statIsBasedOnXData = MonsterClass.AttackStatIsBasedOnXEntries.Find((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
			int statIsBasedOnXValue = cAbility.GetStatIsBasedOnXValue(this, statIsBasedOnXData, CAbilityFilterContainer.CreateDefaultFilter());
			if (statIsBasedOnXData.AddTo)
			{
				AttackStrengthForUI = cAbility.Strength + statIsBasedOnXValue;
			}
			else
			{
				AttackStrengthForUI = statIsBasedOnXValue;
			}
		}
		else
		{
			AttackStrengthForUI = MonsterClass.Attack;
		}
	}

	public override void LoadFields(ActorState state)
	{
		if (state is EnemyState enemyState)
		{
			base.LoadFields(state);
			m_Class = MonsterClassManager.Find(enemyState.ClassID);
			Type = enemyState.Type;
			MonsterGroupType = enemyState.MonsterGroupType;
			HasShiftedForm = enemyState.HasShiftedForm;
			IsSummon = enemyState.IsSummon;
			m_SummonerGuid = enemyState.SummonerGuid;
		}
	}

	public override bool CanInfuse(ElementInfusionBoardManager.EElement element, bool endOfMonsterSet = false)
	{
		if (!endOfMonsterSet)
		{
			List<ElementInfusionBoardManager.EElement> enemyInfusedElements = ElementInfusionBoardManager.GetEnemyInfusedElements(this);
			if (enemyInfusedElements != null && enemyInfusedElements.Count > 0 && (enemyInfusedElements.Contains(element) || enemyInfusedElements.Contains(ElementInfusionBoardManager.EElement.Any)))
			{
				return false;
			}
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			foreach (CActiveBonus item in allAliveActor.FindApplicableActiveBonuses())
			{
				TValue miscVar = item.Ability.GetMiscVar("ForbiddenMonsterElement");
				if (!miscVar.isUnknown)
				{
					int num = miscVar.GetInt();
					if (element == (ElementInfusionBoardManager.EElement)num)
					{
						return false;
					}
				}
			}
		}
		return base.CanInfuse(element, endOfMonsterSet);
	}

	public override bool CanConsume(ElementInfusionBoardManager.EElement element, bool endOfMonsterSet = false)
	{
		if (!endOfMonsterSet)
		{
			List<ElementInfusionBoardManager.EElement> enemyConsumedElements = ElementInfusionBoardManager.GetEnemyConsumedElements(this);
			if (enemyConsumedElements != null && enemyConsumedElements.Contains(element))
			{
				return false;
			}
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			foreach (CActiveBonus item in allAliveActor.FindApplicableActiveBonuses())
			{
				TValue miscVar = item.Ability.GetMiscVar("ForbiddenMonsterElement");
				if (!miscVar.isUnknown)
				{
					string text = miscVar.GetString();
					if (element.ToString() == text)
					{
						return false;
					}
				}
			}
		}
		return base.CanConsume(element, endOfMonsterSet);
	}
}
