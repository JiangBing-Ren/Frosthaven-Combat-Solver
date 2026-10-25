using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CHeroSummonActor : CActor
{
	private string m_SummonerGuid;

	[StateDump(Skip = true)]
	private CPlayerActor m_SummonerCached;

	public int StandeeID { get; set; }

	public CHeroSummonClass HeroSummonClass => (CHeroSummonClass)m_Class;

	public HeroSummonYMLData SummonData { get; private set; }

	public CBaseAbilityCard BaseCard { get; private set; }

	public int SummonedOrderIndex { get; set; }

	public bool? ReturnToSummoner { get; set; }

	public override int SelectedRoundCardsCount => 1;

	public override int MaxRoundCardsCount => 1;

	[StateDump(Skip = true)]
	public CBaseCard CachedSummonCard { get; set; }

	public bool ContinueAfterSummonerDeath
	{
		get
		{
			if (Summoner == null)
			{
				return false;
			}
			return Summoner.SummonsContinueAfterDeath;
		}
	}

	public override bool Invulnerable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count <= 0)
			{
				return SummonData.Invulnerable;
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
				return SummonData.PierceInvulnerability;
			}
			return true;
		}
	}

	public override bool Untargetable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Untargetable).Count <= 0 && !SummonData.Untargetable)
			{
				return IsDowned;
			}
			return true;
		}
	}

	public override bool DoesNotBlock => SummonData.DoesNotBlock;

	public override bool IgnoreActorCollision => SummonData.IgnoreActorCollision;

	public bool PlayerControlled
	{
		get
		{
			if (!SummonData.PlayerControlled)
			{
				return FindApplicableActiveBonuses(CAbility.EAbilityType.ControlSummonTurn).Any();
			}
			return true;
		}
	}

	public bool IsPassive
	{
		get
		{
			if (SummonData.Move == 0 && SummonData.Attack + GetEnhancementAttackStrength() == 0)
			{
				if (SummonData.SpecialAbilities != null)
				{
					return SummonData.SpecialAbilities.Count == 0;
				}
				return true;
			}
			return false;
		}
	}

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item.Ability is CAbilityImmunityTo cAbilityImmunityTo))
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
			return list;
		}
	}

	public CPlayerActor Summoner
	{
		get
		{
			if (m_SummonerCached != null)
			{
				return m_SummonerCached;
			}
			m_SummonerCached = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == m_SummonerGuid);
			return m_SummonerCached;
		}
	}

	public override CPlayerActor PlayerActor => Summoner;

	public bool IsCompanionSummon
	{
		get
		{
			if (Summoner.CharacterClass.CompanionSummonData == null)
			{
				return false;
			}
			return SummonData == Summoner.CharacterClass.CompanionSummonData;
		}
	}

	public int AttackStrength
	{
		get
		{
			if (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength))
			{
				CAbility cAbility = CAbilityAttack.CreateDefaultAttack(SummonData.GetAttackFromActor(this), 1, 1, isMonster: false);
				cAbility.Start(this, this);
				cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
				cAbility.SetStatBasedOnX(this, SummonData.AttackStatIsBasedOnXEntries, cAbility.AbilityFilter);
				return cAbility.Strength;
			}
			return SummonData.GetAttackFromActor(this);
		}
	}

	public bool IsBanner
	{
		get
		{
			string iD = SummonData.ID;
			int num;
			switch (iD)
			{
			default:
				num = ((!(iD == "BannerOfValorID")) ? 1 : 0);
				break;
			case "BannerOfStrengthID":
			case "BannerOfCourageID":
			case "BannerOfDoomID":
			case "BannerOfHopeID":
				num = 0;
				break;
			}
			return num == 0;
		}
	}

	public override bool IsSummonedByPlayer => Summoner != null;

	public CHeroSummonActor(Point startPosition, string summonerGuid, int summonerLevel, int standeeID, CHeroSummonClass heroSummonClass, HeroSummonYMLData summonData, string actorGuid, int chosenModelIndex, float baseHealthModifier = 1f, int enhancementHealth = 0)
		: base(EType.HeroSummon, heroSummonClass, startPosition, summonData.GetHealth(summonerLevel, baseHealthModifier), summonData.GetHealth(summonerLevel, baseHealthModifier), summonerLevel, actorGuid, chosenModelIndex)
	{
		StandeeID = standeeID;
		m_SummonerGuid = summonerGuid;
		SummonData = summonData;
		BaseCard = new CBaseAbilityCard(0, summonData.ID.GetHashCode(), heroSummonClass.ID, CBaseCard.ECardType.HeroSummonAbility, summonData.ID);
		m_Health += enhancementHealth;
		MaxHealth += enhancementHealth;
		Log.Info("Created a new CHeroSummonActor with BaseCardID: " + BaseCard.ID);
	}

	public override string GetPrefabName()
	{
		return SummonData.Model.ToString();
	}

	public override int Initiative()
	{
		if (InitiativeLockedDuringRound.HasValue)
		{
			return InitiativeLockedDuringRound.Value;
		}
		if (Summoner == null || Summoner.IsDead)
		{
			return 99;
		}
		return Summoner.Initiative();
	}

	public override string ActorLocKey()
	{
		return HeroSummonClass.LocKey;
	}

	public override int BaseShield()
	{
		return SummonData.Shield;
	}

	public override int BaseRetaliate(int distanceToEnemy)
	{
		if (distanceToEnemy > SummonData.RetaliateRange)
		{
			return 0;
		}
		return SummonData.Retaliate;
	}

	public override int BaseRetaliateRange()
	{
		return SummonData.RetaliateRange;
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		BaseCard.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(null, null, 0), BaseCard, this);
		int num = SummonData.GetMoveFromActor(this) + GetEnhancementMoveStrength();
		if (num > 0 || SummonData.MoveOverride != null || (SummonData.MoveStatIsBasedOnXEntries != null && SummonData.MoveStatIsBasedOnXEntries.Count > 0))
		{
			CAbility cAbility = CAbilityMove.CreateDefaultMove(num, isMonster: false, SummonData.Range + GetEnhancementRangeStrength(), jump: false, SummonData.Flying, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: true, SummonData.AIFocusOverrideDetails);
			if (SummonData.MoveOverride != null)
			{
				cAbility.OverrideAbilityValues(SummonData.MoveOverride, perform: false);
			}
			GameState.CurrentAction.Action.AddAbility(cAbility);
		}
		if (SummonData.GetAttackFromActor(this) + GetEnhancementAttackStrength() > 0 || SummonData.AttackOverride != null || (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Count > 0))
		{
			CAbility ability = CAbilityAttack.CreateDefaultAttack(0, 0, 1, isMonster: false, SummonData.AttackAnimOverload, isMultipass: true, null, null, isDefault: true, SummonData.AIFocusOverrideDetails);
			ability = ApplyBaseStats(ability);
			if (SummonData.AttackOverride != null)
			{
				ability.OverrideAbilityValues(SummonData.AttackOverride, perform: false);
			}
			GameState.CurrentAction.Action.AddAbility(ability);
		}
		if (SummonData.SpecialAbilities == null)
		{
			return;
		}
		foreach (CAbility specialAbility in SummonData.SpecialAbilities)
		{
			GameState.CurrentAction.Action.AddAbility(CAbility.CopyAbility(specialAbility, generateNewID: false));
		}
	}

	public CAbilitySummon GetSummonAbility()
	{
		if (Summoner?.CharacterClass?.ActivatedAbilityCards == null)
		{
			return null;
		}
		foreach (CAbilityCard activatedAbilityCard in Summoner.CharacterClass.ActivatedAbilityCards)
		{
			CAbility cAbility = activatedAbilityCard.ActiveBonuses.Find((CActiveBonus x) => x is CSummonActiveBonus cSummonActiveBonus && cSummonActiveBonus.Actor == this)?.Ability;
			if (cAbility != null)
			{
				return (CAbilitySummon)cAbility;
			}
		}
		return null;
	}

	public int GetEnhancementAttackStrength()
	{
		if (GetSummonAbility()?.AbilityEnhancements.Find((CEnhancement e) => e.Enhancement == EEnhancement.SummonAttack) != null)
		{
			return 1;
		}
		return 0;
	}

	public int GetEnhancementRangeStrength()
	{
		if (GetSummonAbility()?.AbilityEnhancements.Find((CEnhancement e) => e.Enhancement == EEnhancement.SummonRange) != null)
		{
			return 1;
		}
		return 0;
	}

	public int GetEnhancementHealthStrength()
	{
		CAbilitySummon summonAbility = GetSummonAbility();
		if (summonAbility?.AbilityEnhancements != null && summonAbility.AbilityEnhancements.Find((CEnhancement e) => e.Enhancement == EEnhancement.SummonHP) != null)
		{
			return 1;
		}
		return 0;
	}

	public int GetEnhancementMoveStrength()
	{
		if (GetSummonAbility()?.AbilityEnhancements.Find((CEnhancement e) => e.Enhancement == EEnhancement.SummonMove) != null)
		{
			return 1;
		}
		return 0;
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, Point? freeTileAwayFromCaster = null, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster: false, Type == EType.HeroSummon || Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public CAbility ApplyBaseStats(CAbility ability, bool copyCurrentOverrides = false, bool overrideAttackZero = false, bool generateNewID = false)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID, fullCopy: false, copyCurrentOverrides);
		if (!ability.StrengthIsBase)
		{
			if (cAbility is CAbilityAttack cAbilityAttack)
			{
				int num = SummonData.GetAttackFromActor(this) + GetEnhancementAttackStrength();
				if (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Count > 0)
				{
					cAbilityAttack.Strength += num;
					cAbilityAttack.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
					cAbilityAttack.SetStatBasedOnX(this, SummonData.AttackStatIsBasedOnXEntries, cAbility.AbilityFilter);
				}
				else
				{
					if (num == 0 && !overrideAttackZero)
					{
						return null;
					}
					cAbilityAttack.Strength += num;
				}
			}
			else if (cAbility is CAbilityMove cAbilityMove)
			{
				cAbilityMove.Strength += SummonData.GetMoveFromActor(this) + GetEnhancementMoveStrength();
			}
		}
		if (cAbility is CAbilityAttack cAbilityAttack2)
		{
			cAbilityAttack2.Pierce += SummonData.GetPierceFromActor(this);
		}
		if (ability.AbilityType == CAbility.EAbilityType.Move)
		{
			((CAbilityMove)cAbility).Fly = SummonData.Flying;
		}
		if (!ability.RangeIsBase)
		{
			cAbility.Range += SummonData.Range + GetEnhancementRangeStrength();
		}
		return cAbility;
	}

	public void OnSummoned()
	{
		if (SummonData.OnSummonAbilities == null || SummonData.OnSummonAbilities.Count <= 0 || !(PhaseManager.Phase is CPhaseAction cPhaseAction) || cPhaseAction == null)
		{
			return;
		}
		foreach (CAbility onSummonAbility in SummonData.OnSummonAbilities)
		{
			onSummonAbility.CachedAbilityBaseCard = null;
		}
		cPhaseAction.StackNextAbilities(SummonData.OnSummonAbilities, this);
	}

	public void Cheat_ReapplyOnSummonBonuses()
	{
		if (IsDead || SummonData.OnSummonAbilities == null || SummonData.OnSummonAbilities.Count <= 0)
		{
			return;
		}
		foreach (CAbility item in SummonData.OnSummonAbilities.Where((CAbility x) => x.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && x.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA))
		{
			item.CachedAbilityBaseCard = null;
			CAbility cAbility = CAbility.CopyAbility(item, generateNewID: false);
			cAbility.Start(this, this);
			cAbility.ApplyToActor(this);
		}
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		CauseOfDeath = causeOfDeath;
		m_CauseOfDeathAbility = causeOfDeathAbility;
		CPlayerActor.LogHeroSummonMoveChanged(this);
		if (!m_OnDeathAbilityUsed && CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap && PhaseManager.Phase is CPhaseAction cPhaseAction && cPhaseAction != null)
		{
			List<CAbility> list = new List<CAbility>();
			if (SummonData.AdjacentAttackOnDeath > 0)
			{
				CAbility cAbility = CAbilityAttack.CreateDefaultAttack(SummonData.AdjacentAttackOnDeath, 1, -1, isMonster: false, SummonData.AttackOnDeathAnimOverload, isMultipass: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.None, CAbilityFilter.EFilterAllyType.Ally, null, null, null, null, null, null, null, null, null, null, null, null, null, new CEqualityFilter(">", 0, valueIsPercentage: false)));
				cAbility.AbilityFilter.IsValidTarget(this, this, isTargetedAbility: true, useTargetOriginalType: false, false);
				if (cAbility.AbilityFilter.LastCheckedCasterAdjacentEnemies.Count > 0)
				{
					cAbility.ProcessIfDead = true;
					list.Add(cAbility);
				}
			}
			if (SummonData.OnDeathAbilities != null && SummonData.OnDeathAbilities.Count > 0)
			{
				foreach (CAbility onDeathAbility in SummonData.OnDeathAbilities)
				{
					CAbility cAbility2 = CAbility.CopyAbility(onDeathAbility, generateNewID: false);
					cAbility2.ProcessIfDead = true;
					list.Add(cAbility2);
				}
			}
			if (list.Count > 0)
			{
				TargetingActor = targetingActor;
				m_OnDeathAbilityUsed = true;
				GameState.OverrideCurrentActorForOneAction(this, OriginalType, killActorAfterAction: true, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				startedOnDeathAbility = true;
				return false;
			}
		}
		base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility);
		if (startedOnDeathAbility)
		{
			TargetingActor = targetingActor;
			return false;
		}
		Summoner.RecycleSummonId(HeroSummonClass, ID);
		return true;
	}

	public bool OnDeathFromSummonerDeath(CActor targetingActor)
	{
		base.OnDeath(targetingActor, (Summoner.CauseOfDeath == ECauseOfDeath.ActorRemovedFromMap) ? ECauseOfDeath.ActorRemovedFromMap : ECauseOfDeath.SummonerDied, out var _);
		Summoner.RecycleSummonId(HeroSummonClass, ID);
		return true;
	}

	public override void OnDeathAbilityComplete()
	{
		if (CauseOfDeath == ECauseOfDeath.None)
		{
			Log.ErrorWithReport("Cause of death was not set before calling OnDeathAbilityComplete()");
			CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		base.OnDeath(TargetingActor, CauseOfDeath, out var _, fromDeathAbilityComplete: true, m_CauseOfDeathAbility);
		Summoner.RecycleSummonId(HeroSummonClass, ID);
		GameState.KillActorInternal(TargetingActor, this, m_OnDeathAbilityUsed);
		TargetingActor = null;
		m_OnDeathAbilityUsed = false;
		m_CauseOfDeathAbility = null;
	}

	public new CHeroSummonActor Clone()
	{
		CHeroSummonActor cHeroSummonActor = (CHeroSummonActor)MemberwiseClone();
		cHeroSummonActor.Inventory.InventoryActor = cHeroSummonActor;
		return cHeroSummonActor;
	}

	public override void LoadFields(ActorState state)
	{
		if (state is HeroSummonState heroSummonState)
		{
			base.LoadFields(state);
			m_SummonerGuid = heroSummonState.Summoner;
			m_SummonerCached = null;
			SummonData = heroSummonState.HeroSummonData;
			SummonedOrderIndex = heroSummonState.SummonedOrderIndex;
			CachedSummonCard = FindSummonCard();
		}
	}

	public CAbilityCard FindSummonCard()
	{
		foreach (CAbilityCard activatedAbilityCard in Summoner.CharacterClass.ActivatedAbilityCards)
		{
			if (activatedAbilityCard.ActiveBonuses.Any((CActiveBonus x) => x is CSummonActiveBonus cSummonActiveBonus && cSummonActiveBonus.Actor == this))
			{
				return activatedAbilityCard;
			}
		}
		return null;
	}
}
