using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CMonsterClass : CClass
{
	private List<CBaseCard> m_ActivatedCards;

	public static List<EMonsterBaseStats> RefreshRequiringStatIsXTypes = new List<EMonsterBaseStats>
	{
		EMonsterBaseStats.Shield,
		EMonsterBaseStats.Retaliate
	};

	protected List<CMonsterAbilityCard> m_AbilityCardsPool = new List<CMonsterAbilityCard>();

	protected List<CMonsterAbilityCard> m_AbilityCards = new List<CMonsterAbilityCard>();

	protected List<CMonsterAbilityCard> m_DiscardedAbilityCards = new List<CMonsterAbilityCard>();

	protected CMonsterAbilityCard m_RoundAbilityCard;

	protected CAction[] m_BaseActions = new CAction[8];

	protected BaseStats m_CurrentMonsterStat;

	protected int m_MonsterStatLevel;

	private bool m_Boss;

	private bool m_PredominantlyMelee;

	private bool m_AlwaysShowInnateImmunities;

	private MonsterYMLData m_MonstersYML;

	private CMonsterClass m_NonEliteVariant;

	private CMonsterClass m_EliteVariant;

	private CMonsterClass m_CardsPoolClass;

	private BaseStats m_OriginalMonsterStats;

	private int? m_FocusingInitiative;

	protected List<int> m_AvailableIDs;

	public CMonsterClass CardsPoolClass
	{
		get
		{
			if (!string.IsNullOrEmpty(m_MonstersYML.CardsPoolClass))
			{
				if (m_CardsPoolClass != null)
				{
					return m_CardsPoolClass;
				}
				m_CardsPoolClass = MonsterClassManager.Find(m_MonstersYML.CardsPoolClass);
				if (m_CardsPoolClass != null)
				{
					return m_CardsPoolClass;
				}
				Log.ErrorWithReport("Unknown monster class: " + m_MonstersYML.CardsPoolClass);
			}
			if (m_NonEliteVariant != null)
			{
				return m_NonEliteVariant.CardsPoolClass;
			}
			return this;
		}
	}

	public bool HasCards => (CardsPoolClass?.AbilityCardsPool?.Count).GetValueOrDefault() > 0;

	public List<CMonsterAbilityCard> AbilityCardsPool => m_AbilityCardsPool;

	public List<CMonsterAbilityCard> AbilityCards
	{
		get
		{
			return CardsPoolClass.m_AbilityCards;
		}
		private set
		{
			CardsPoolClass.m_AbilityCards = value;
		}
	}

	public List<CMonsterAbilityCard> DiscardedAbilityCards
	{
		get
		{
			return CardsPoolClass.m_DiscardedAbilityCards;
		}
		private set
		{
			CardsPoolClass.m_DiscardedAbilityCards = value;
		}
	}

	public CMonsterAbilityCard RoundAbilityCard
	{
		get
		{
			return CardsPoolClass.m_RoundAbilityCard;
		}
		set
		{
			CardsPoolClass.m_RoundAbilityCard = value;
		}
	}

	public CMonsterAbilityCard NextAbilityCard
	{
		get
		{
			if (AbilityCards.Count > 0)
			{
				return AbilityCards[0];
			}
			return null;
		}
	}

	public bool NextAbilityCardKnown => ActivatedCards.Any((CBaseCard x) => x.ActiveBonuses.Any((CActiveBonus y) => y.Ability.GetMiscVar("ShowNextCard").GetBool()));

	public List<CBaseCard> ActivatedCards => CardsPoolClass.m_ActivatedCards;

	public CAction BaseAction => m_BaseActions[m_MonsterStatLevel];

	public int Shield => m_CurrentMonsterStat.Shield;

	public int Retaliate => m_CurrentMonsterStat.Retaliate;

	public int RetaliateRange => m_CurrentMonsterStat.RetaliateRange;

	public int Target => m_CurrentMonsterStat.Target;

	public int Pierce => m_CurrentMonsterStat.Pierce;

	public int Push => m_CurrentMonsterStat.Push;

	public bool Advantage => m_CurrentMonsterStat.Advantage;

	public bool AttackersGainDisadv => m_CurrentMonsterStat.AttackersGainDisadvantage;

	public bool Boss => m_Boss;

	public bool PredominantlyMelee => m_PredominantlyMelee;

	public bool AlwaysShowInnateImmunities => m_AlwaysShowInnateImmunities;

	public int Range => m_CurrentMonsterStat.Range;

	public int Attack => m_CurrentMonsterStat.Attack;

	public int Move => m_CurrentMonsterStat.Move;

	public List<CCondition.ENegativeCondition> Conditions => m_CurrentMonsterStat.Conditions.ToList();

	public List<CAbility.EAbilityType> Immunities => m_CurrentMonsterStat.Immunities.ToList();

	public bool Flying => m_CurrentMonsterStat.Flying;

	public bool Invulnerable => m_CurrentMonsterStat.Invulnerable;

	public bool PierceInvulnerability => m_CurrentMonsterStat.PierceInvulnerablility;

	public bool Untargetable => m_CurrentMonsterStat.Untargetable;

	public bool DoesNotBlock => m_CurrentMonsterStat.DoesNotblock;

	public bool ImmuneToForcedMovement => m_CurrentMonsterStat.ImmuneToForcedMovement;

	public bool IsWaterMonster => m_CurrentMonsterStat.IsWaterMonster;

	public bool IgnoreActorCollision => m_CurrentMonsterStat.IgnoreActorCollision;

	public int StatLevel => m_MonsterStatLevel;

	public List<int> AvailableIDs => m_AvailableIDs;

	public BaseStats CurrentMonsterStat => m_CurrentMonsterStat;

	public BaseStats OriginalBaseStats => m_OriginalMonsterStats;

	[StateDump(Skip = true)]
	[Obsolete]
	public bool InitialState { get; set; }

	public CMonsterClass NonEliteVariant => m_NonEliteVariant;

	public CMonsterClass EliteVariant => m_EliteVariant;

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries => m_CurrentMonsterStat.StatIsBasedOnXEntries;

	public string MonsterClassIDToActImmediatelyBefore => m_MonstersYML.MonsterClassIDToActImmediatelyBefore;

	public MonsterYMLData MonsterYML => m_MonstersYML;

	public int? FocusingInitiative => m_FocusingInitiative;

	public bool CanActivatePressurePlates => m_CurrentMonsterStat.CanActivatePressurePlates;

	public List<AbilityData.StatIsBasedOnXData> AttackStatIsBasedOnXEntries => m_CurrentMonsterStat?.AttackStatIsBasedOnXEntries;

	public List<AbilityData.StatIsBasedOnXData> MoveStatIsBasedOnXEntries => m_CurrentMonsterStat?.MoveStatIsBasedOnXEntries;

	public List<AbilityData.StatIsBasedOnXData> ShieldStatIsBasedOnXEntries => m_CurrentMonsterStat?.ShieldStatIsBasedOnXEntries;

	public EMonsterType MonsterType => m_MonstersYML.MonsterType;

	public override int Health()
	{
		return GetHealth();
	}

	public CMonsterClass(MonsterYMLData monster, List<CMonsterAbilityCard> abilityCards)
		: base(monster.ID, monster.Models, monster.LocKey)
	{
		m_MonstersYML = monster;
		m_ActivatedCards = new List<CBaseCard>();
		for (int i = 0; i < 8; i++)
		{
			SetMonsterStatLevel(i);
			CAction cAction = new CAction(null, null, 0);
			try
			{
				cAction.AddAbility(CAbilityAttack.CreateDefaultAttack(m_CurrentMonsterStat.Attack, m_CurrentMonsterStat.Range, m_CurrentMonsterStat.Target, isMonster: true));
				cAction.AddAbility(CAbilityMove.CreateDefaultMove(m_CurrentMonsterStat.Move, isMonster: true));
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Unable to create base stats ability for monster " + monster.ID + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
			m_BaseActions[i] = cAction;
		}
		SetMonsterStatLevel(1);
		m_AbilityCardsPool = abilityCards;
		m_NonEliteVariant = MonsterClassManager.Find(monster.NonEliteVariant);
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.m_EliteVariant = this;
		}
		m_Boss = monster.MonsterType == EMonsterType.Boss;
		m_PredominantlyMelee = monster.PredominantlyMelee;
		m_AlwaysShowInnateImmunities = monster.AlwaysShowInnateImmunities;
		m_FocusingInitiative = monster.FocusingInitiative;
	}

	private int GetHealth()
	{
		return m_CurrentMonsterStat.Health;
	}

	public void SetBaseStatIsBasedOnXValueUsingCurrent(bool onlyRefreshRequiredTypes, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		SetBaseStatIsBasedOnXValue(ref m_CurrentMonsterStat, onlyRefreshRequiredTypes, additionalStatBasedOnXData);
	}

	public void SetBaseStatIsBasedOnXValue(ref BaseStats baseStats, bool onlyRefreshRequiredTypes, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		if ((StatIsBasedOnXEntries == null && additionalStatBasedOnXData == null) || ScenarioManager.CurrentScenarioState == null)
		{
			return;
		}
		List<AbilityData.StatIsBasedOnXData> list = new List<AbilityData.StatIsBasedOnXData>();
		if (StatIsBasedOnXEntries != null)
		{
			list.AddRange(StatIsBasedOnXEntries);
		}
		if (additionalStatBasedOnXData != null)
		{
			list.AddRange(additionalStatBasedOnXData);
		}
		if (onlyRefreshRequiredTypes)
		{
			list = list.Where((AbilityData.StatIsBasedOnXData x) => RefreshRequiringStatIsXTypes.Contains(x.BaseStatType)).ToList();
		}
		foreach (AbilityData.StatIsBasedOnXData item in list)
		{
			int baseStatIsBasedOnXValue = GetBaseStatIsBasedOnXValue(item);
			switch (item.BaseStatType)
			{
			case EMonsterBaseStats.Health:
				baseStats.Health = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Health : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Shield:
				baseStats.Shield = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Shield : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Retaliate:
				baseStats.Retaliate = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Retaliate : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.RetaliateRange:
				baseStats.RetaliateRange = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.RetaliateRange : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Pierce:
				baseStats.Pierce = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Pierce : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Push:
				baseStats.Push = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Push : 0) + baseStatIsBasedOnXValue);
				break;
			}
			if (item.AddTo)
			{
				item.AddedStat = baseStatIsBasedOnXValue;
			}
		}
	}

	public static int GetBaseStatIsBasedOnXValue(AbilityData.StatIsBasedOnXData statIsBasedOnXData, CAbility.EStatIsBasedOnXType basedOnOverride = CAbility.EStatIsBasedOnXType.None, float xVariableOverride = 1f, float yVariableOverride = 1f, int minScenarioLevel = 0)
	{
		int num = 0;
		float num2 = 0f;
		CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = ((basedOnOverride == CAbility.EStatIsBasedOnXType.None && statIsBasedOnXData != null) ? statIsBasedOnXData.BasedOn : basedOnOverride);
		float num3 = ((xVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.Multiplier : xVariableOverride);
		float num4 = ((yVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.SecondVariable : yVariableOverride);
		CAbilityFilterContainer cAbilityFilterContainer = statIsBasedOnXData?.Filter;
		int level = ScenarioManager.CurrentScenarioState.Level;
		if (statIsBasedOnXData != null && minScenarioLevel == 0)
		{
			minScenarioLevel = statIsBasedOnXData.MinLValue;
		}
		level = Math.Max(level, minScenarioLevel);
		switch (eStatIsBasedOnXType)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) * num3;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) + num3;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
			num2 = num3 + (float)(ScenarioManager.CurrentScenarioState.Players.Count * level);
			break;
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
			num2 = (num3 + (float)level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
			num2 = num4 + (num3 + (float)level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
			num2 = num3 + (float)(int)Math.Round(num4 * (float)level);
			break;
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level;
			break;
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
			num2 = num3 + (float)level;
			break;
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
			num2 = (float)level + num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
			num2 = num3 + (float)(int)Math.Ceiling((float)(level * ScenarioManager.CurrentScenarioState.Players.Count) / num4);
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level - num4;
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
			num2 = (num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level - num4) * (float)ScenarioManager.CurrentScenarioState.RoundNumber;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
			num2 = (num3 + (float)level) * ((float)ScenarioManager.CurrentScenarioState.Players.Count / num4);
			break;
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
			num2 = (float)level * num3 + num4;
			break;
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
			num2 = (float)level / num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX:
			num2 = (float)(level + ScenarioManager.CurrentScenarioState.Players.Count) * num3;
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count - num4;
			break;
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level * num4;
			break;
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
			num2 = (int)Math.Floor(((float)ScenarioManager.CurrentScenarioState.Players.Count + num3) / 2f * ((float)level + num4));
			break;
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		{
			int num9 = level;
			int count3 = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = (num3 + num4 * (float)count3) * (float)(2 + num9);
			break;
		}
		case CAbility.EStatIsBasedOnXType.FH22ShipFormulaCharactersLevel:
		{
			int num8 = level;
			int count2 = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = 2 * (2 + num8) * (3 * count2 - 1);
			break;
		}
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		{
			int num7 = level;
			int count = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = ((float)num7 * num3 + num4) * (float)count;
			break;
		}
		case CAbility.EStatIsBasedOnXType.ActorCount:
		{
			int num6 = 0;
			foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState2.IsRevealed)
				{
					CActor cActor2 = null;
					cActor2 = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState2.ActorGuid);
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 != null && cAbilityFilterContainer != null && cAbilityFilterContainer.IsValidTarget(cActor2, cActor2, isTargetedAbility: false, useTargetOriginalType: false, true))
					{
						num6++;
					}
				}
				else if (cAbilityFilterContainer != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState2 is EnemyState enemyState2) || enemyState2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden) && actorState2 != null && cAbilityFilterContainer.IsValidTarget_ActorState(actorState2, actorState2, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num6++;
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden && prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !prop.PropActorHasBeenAssigned)
					{
						CTile propTile = ScenarioManager.Tiles[prop.ArrayIndex.X, prop.ArrayIndex.Y];
						ObjectState objectState = prop.PropHealthDetails.CreateStateForPropWithHealth(propTile, prop.PropHealthDetails.GetPropStartingHealth());
						if (cAbilityFilterContainer.IsValidTarget_ActorState(objectState, objectState, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num6++;
						}
					}
				}
			}
			num2 = (float)num6 * num3;
			break;
		}
		case CAbility.EStatIsBasedOnXType.DeadActorCount:
		{
			int num5 = 0;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState is EnemyState enemyState && enemyState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				CActor cActor = null;
				cActor = ScenarioManager.Scenario.ExhaustedPlayers.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState.ActorGuid);
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadHeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadEnemies.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadAllyMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadNeutralMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor != null && cAbilityFilterContainer != null && cAbilityFilterContainer.IsValidTarget(cActor, cActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num5++;
				}
			}
			num2 = (float)num5 * num3;
			break;
		}
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX:
			num2 = num3 * ((float)ScenarioManager.CurrentScenarioState.Players.Count - num4);
			break;
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
			num2 = (num3 + (float)level) * num4 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.CharactersTimesXPlusLevelTimesY:
			num2 = ((float)level + (float)ScenarioManager.CurrentScenarioState.Players.Count * num3) * num4;
			break;
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
			num2 = ((float)level + num4) * num3;
			break;
		}
		switch (statIsBasedOnXData?.RoundingType ?? CAbility.EStatIsBasedOnXRoundingType.RoundOff)
		{
		case CAbility.EStatIsBasedOnXRoundingType.RoundOff:
			num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
			break;
		case CAbility.EStatIsBasedOnXRoundingType.ToFloor:
			num = (int)Math.Floor(num2);
			break;
		case CAbility.EStatIsBasedOnXRoundingType.ToCeil:
			num = (int)Math.Ceiling(num2);
			break;
		}
		if (statIsBasedOnXData != null)
		{
			num = Math.Max(statIsBasedOnXData.MinValue, num);
			num = Math.Min(statIsBasedOnXData.MaxValue, num);
		}
		return num;
	}

	private static void SetAddNumberOfPlayersTo(MonsterYMLData yml, ref BaseStats baseStats, int playercount = 0)
	{
		if (yml.AddNumberOfPlayersTo == null || yml.AddNumberOfPlayersTo.Count <= 0 || ScenarioManager.Scenario == null)
		{
			return;
		}
		int num = playercount;
		if (num == 0)
		{
			num = ScenarioManager.Scenario.AllPlayers.Count();
		}
		foreach (EMonsterBaseStats item in yml.AddNumberOfPlayersTo)
		{
			switch (item)
			{
			case EMonsterBaseStats.Attack:
				baseStats.Attack += num;
				break;
			case EMonsterBaseStats.Health:
				baseStats.Health += num;
				break;
			case EMonsterBaseStats.Move:
				baseStats.Move += num;
				break;
			case EMonsterBaseStats.Range:
				baseStats.Range += num;
				break;
			case EMonsterBaseStats.Target:
				baseStats.Target += num;
				break;
			default:
				Log.ErrorWithReport("Invalid property in AddNumberOfPlayersTo for monster " + yml.FileName);
				break;
			}
		}
	}

	public void SetMonsterStatLevel(int level, int playercount = 0, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		if (level >= 0)
		{
			m_MonsterStatLevel = level;
			m_CurrentMonsterStat = m_MonstersYML.MonsterBaseStats.Single((BaseStats x) => x.Level == level).Copy();
			m_OriginalMonsterStats = m_CurrentMonsterStat.Copy();
			SetBaseStatIsBasedOnXValue(ref m_CurrentMonsterStat, onlyRefreshRequiredTypes: false, additionalStatBasedOnXData);
		}
	}

	public BaseStats GetMonsterStatsData(int level)
	{
		BaseStats baseStats = m_MonstersYML.MonsterBaseStats.Single((BaseStats x) => x.Level == level).Copy();
		SetBaseStatIsBasedOnXValue(ref baseStats, onlyRefreshRequiredTypes: false);
		return baseStats;
	}

	public override CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		if (RoundAbilityCard != null)
		{
			if (RoundAbilityCard.Action.Abilities.Any((CAbility x) => x.HasID(ability.ID)))
			{
				return RoundAbilityCard;
			}
			if (RoundAbilityCard.Action.Augmentations != null && RoundAbilityCard.Action.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))))
			{
				return RoundAbilityCard;
			}
		}
		foreach (CBaseCard activatedCard in ActivatedCards)
		{
			if (activatedCard.ActiveBonuses.Any((CActiveBonus a) => a.Ability != null && a.Ability.HasID(ability.ID)))
			{
				return activatedCard;
			}
		}
		foreach (CAbilityCard temporaryCard in TemporaryCards)
		{
			if (temporaryCard.HasAbility(ability))
			{
				return temporaryCard;
			}
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CAbility item in scenarioModifier.AllListedTriggerAbilities(copyAbilities: false))
			{
				if (item != null && item.HasID(ability.ID))
				{
					return scenarioModifier;
				}
			}
		}
		if (CurrentMonsterStat.OnDeathAbilities != null && CurrentMonsterStat.OnDeathAbilities.Any((CAbility a) => a.HasID(ability.ID)))
		{
			return RoundAbilityCard;
		}
		return base.FindCardWithAbility(ability, actor);
	}

	public override CBaseCard FindCard(int id, string name)
	{
		CMonsterAbilityCard roundAbilityCard = RoundAbilityCard;
		if (roundAbilityCard != null && roundAbilityCard.ID == id && roundAbilityCard?.Name == name)
		{
			return roundAbilityCard;
		}
		foreach (CBaseCard activatedCard in ActivatedCards)
		{
			if (activatedCard != null && activatedCard.ID == id && activatedCard?.Name == name)
			{
				return activatedCard;
			}
		}
		return null;
	}

	public void SelectRoundAbilityCard()
	{
		if (PhaseManager.CurrentPhase == null)
		{
			return;
		}
		if (CardsPoolClass != this)
		{
			CardsPoolClass.SelectRoundAbilityCard();
			return;
		}
		if (m_AbilityCards.Count == 0 && m_RoundAbilityCard == null)
		{
			ResetAbilityDeck();
		}
		if (m_AbilityCards.Count > 0 && m_RoundAbilityCard == null)
		{
			SetRoundCard(m_AbilityCards[0]);
		}
	}

	public void SetRoundCard(CMonsterAbilityCard card)
	{
		m_RoundAbilityCard = card;
		m_RoundAbilityCard.ActionHasHappened = false;
		m_AbilityCards.Remove(m_RoundAbilityCard);
		if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == m_RoundAbilityCard.GetMonsterCardYML.ID))
		{
			Log.Warning("Duplicate ability " + m_RoundAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + ID);
		}
		m_AbilityCards.RemoveAll((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == m_RoundAbilityCard.GetMonsterCardYML.ID);
	}

	public void DiscardRoundAbilityCard()
	{
		if (CardsPoolClass != this)
		{
			CardsPoolClass.DiscardRoundAbilityCard();
		}
		else if (m_RoundAbilityCard != null)
		{
			m_DiscardedAbilityCards.Add(m_RoundAbilityCard);
			if (m_RoundAbilityCard.Shuffle)
			{
				ResetAbilityDeck();
			}
			m_RoundAbilityCard = null;
		}
	}

	public bool HasRemainingStandees()
	{
		return m_AvailableIDs.Count > 0;
	}

	public int GetNextID(int thisID = int.MaxValue)
	{
		if (!m_MonstersYML.IgnoreCardPoolStandeeLimit && CardsPoolClass != this)
		{
			return CardsPoolClass.GetNextID(thisID);
		}
		int num = -1;
		int num2 = 0;
		if (thisID != int.MaxValue)
		{
			num = m_AvailableIDs.FindIndex((int x) => x == thisID);
			num2 = thisID;
			if (num == -1 && m_AvailableIDs.Count > 0)
			{
				num = ScenarioManager.CurrentScenarioState.EnemyIDRNG.Next(m_AvailableIDs.Count);
				num2 = m_AvailableIDs[num];
			}
		}
		else
		{
			if (m_AvailableIDs.Count <= 0)
			{
				return -1;
			}
			num = ScenarioManager.CurrentScenarioState.EnemyIDRNG.Next(m_AvailableIDs.Count);
			num2 = m_AvailableIDs[num];
		}
		if (num != -1)
		{
			m_AvailableIDs.RemoveAt(num);
		}
		Log.SimpleInfo("Generating EnemyID: " + num2 + " for Class " + ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nIndex: " + num + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
		return num2;
	}

	public void RemoveAvailableID(int id)
	{
		CardsPoolClass.m_AvailableIDs.Remove(id);
		Log.SimpleInfo("Removed EnemyID: " + id + " for Class " + ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
	}

	public void RecycleID(int id)
	{
		if (!m_MonstersYML.IgnoreCardPoolStandeeLimit && CardsPoolClass != this)
		{
			CardsPoolClass.RecycleID(id);
			return;
		}
		if (!m_AvailableIDs.Contains(id) && id <= m_MonstersYML.StandeeLimit)
		{
			m_AvailableIDs.Add(id);
		}
		Log.SimpleInfo("Recycled EnemyID: " + id + " for Class " + ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
	}

	public List<CActiveBonus> FindAllActiveBonuses()
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses);
		}
		activeBonuses.AddRange(from w in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where !activeBonuses.Contains(w)
			select w);
		return activeBonuses;
	}

	public override IEnumerable<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		if (RoundAbilityCard != null)
		{
			if (actor == null)
			{
				foreach (CActiveBonus activeBonuse in RoundAbilityCard.ActiveBonuses)
				{
					yield return activeBonuse;
				}
			}
			else
			{
				foreach (CActiveBonus item in RoundAbilityCard.GetActiveBonusesForActor(actor))
				{
					yield return item;
				}
			}
		}
		lock (ActivatedCards)
		{
			foreach (CBaseCard activatedCard in ActivatedCards)
			{
				if (actor == null)
				{
					foreach (CActiveBonus activeBonuse2 in activatedCard.ActiveBonuses)
					{
						yield return activeBonuse2;
					}
					continue;
				}
				foreach (CActiveBonus item2 in activatedCard.GetActiveBonusesForActor(actor))
				{
					yield return item2;
				}
			}
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor, bool includeIfCasterOfAura = false)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(from w in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where w.Duration == durationType && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura
			select w);
		foreach (CActiveBonus item in from bonus in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where bonus.IsAura
			select bonus)
		{
			if (item.Duration == durationType && !activeBonuses.Contains(item) && (item.ValidActorsInRangeOfAura.Contains(actor) || (includeIfCasterOfAura && item.Caster == actor)))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Type() == type && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(from w in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where w.Type() == type && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura
			select w);
		foreach (CActiveBonus item in from bonus in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where bonus.IsAura
			select bonus)
		{
			if (item.Type() == type && !activeBonuses.Contains(item) && item.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Ability.ActiveBonusData.HasBehaviour(behaviour) && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(from w in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where w.Ability.ActiveBonusData.HasBehaviour(behaviour) && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura
			select w);
		foreach (CActiveBonus item in from bonus in ActivatedCards.SelectMany((CBaseCard s) => s.ActiveBonuses)
			where bonus.IsAura
			select bonus)
		{
			if (item.Ability.ActiveBonusData.HasBehaviour(behaviour) && !activeBonuses.Contains(item) && item.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonusAuras(CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (!activeBonuse.IsAura)
					{
						continue;
					}
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if ((actor as CHeroSummonActor)?.Summoner == activeBonuse.Actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else if (actor == activeBonuse.Actor && !list.Contains(activeBonuse))
					{
						list.Add(activeBonuse);
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonusAuras(actor);
			}
			throw ex;
		}
	}

	public bool HasActiveBonus(CActiveBonus activeBonus)
	{
		if (RoundAbilityCard != null && RoundAbilityCard.ActiveBonuses.Contains(activeBonus))
		{
			return true;
		}
		if (ActivatedCards.Any((CBaseCard a) => a.ActiveBonuses.Contains(activeBonus)))
		{
			return true;
		}
		return false;
	}

	public void ResetAbilityDeck()
	{
		if (CardsPoolClass != this)
		{
			CardsPoolClass.ResetAbilityDeck();
			return;
		}
		m_RoundAbilityCard = null;
		while (m_DiscardedAbilityCards.Count > 0)
		{
			CMonsterAbilityCard monsterAbilityCard = m_DiscardedAbilityCards[0];
			m_DiscardedAbilityCards.Remove(monsterAbilityCard);
			monsterAbilityCard.ActiveBonuses.Clear();
			if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == monsterAbilityCard.GetMonsterCardYML.ID))
			{
				Log.Warning("Duplicate ability " + monsterAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + ID);
			}
			else
			{
				m_AbilityCards.Add(monsterAbilityCard);
			}
		}
		if (AbilityCards.Count == 0)
		{
			m_AbilityCards.AddRange(AbilityCardsPool);
		}
		m_AbilityCards.Shuffle(ScenarioManager.CurrentScenarioState.EnemyAbilityCardRNG);
		Log.SimpleInfo("[Round " + ScenarioManager.CurrentScenarioState.RoundNumber + "] ENEMY CARD:" + ID + " ability deck shuffled:\nAbility Cards:\n" + string.Join("\n", m_AbilityCards.Select((CMonsterAbilityCard s) => s.Name)));
	}

	public override void Reset()
	{
		base.Reset();
		InitialState = true;
		foreach (CMonsterAbilityCard item in m_AbilityCardsPool)
		{
			item.Reset();
		}
		AbilityCards.Clear();
		DiscardedAbilityCards.Clear();
		ActivatedCards.Clear();
		TemporaryCards.Clear();
		RoundAbilityCard = null;
		ResetEnemyStandeeIDs();
	}

	public void ResetAbilityCardsToDefault()
	{
		if (CardsPoolClass != this)
		{
			CardsPoolClass.ResetAbilityCardsToDefault();
			return;
		}
		m_AbilityCards.Clear();
		m_AbilityCards.AddRange(AbilityCardsPool);
		m_DiscardedAbilityCards.Clear();
		InitialState = false;
	}

	public void ResetEnemyStandeeIDs()
	{
		if (m_AvailableIDs == null)
		{
			m_AvailableIDs = new List<int>();
		}
		else
		{
			m_AvailableIDs.Clear();
		}
		for (int i = 1; i <= m_MonstersYML.StandeeLimit; i++)
		{
			m_AvailableIDs.Add(i);
		}
	}

	public override int RemoveCurses(CActor actor)
	{
		return MonsterClassManager.RemoveAllCurseCards(actor as CEnemyActor);
	}

	public override void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
		if (actor is CEnemyActor enemyActor && ((MonsterClassManager.GetCurseCardCount(enemyActor) < 10) | canGoOverLimit))
		{
			Log.Info(ID + " has added a curse card to the Monsters attack modifier deck.");
			MonsterClassManager.AddCurseCard(enemyActor);
		}
		else
		{
			Log.Info("Curse card was not added as there are already 10 curse cards in the attack modifier deck for type: " + actor.OriginalType);
		}
	}

	public override void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
		int num = ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).AttackModifierCards.Where((AttackModifierYMLData x) => x.IsBless).Count()).Sum();
		num += MonsterClassManager.MonsterBlessCount;
		if (actor is CEnemyActor enemyActor && ((num < 10) | canGoOverLimit))
		{
			Log.Info(ID + " has added a bless card to the Monsters attack modifier deck.");
			MonsterClassManager.AddBlessCard(enemyActor);
		}
		else
		{
			Log.Info("Bless card was not added as there are already 10 bless cards in the attack modifier deck");
		}
	}

	public void LoadAvailableIDs(List<int> availableIDs)
	{
		if (availableIDs != null)
		{
			m_AvailableIDs = availableIDs.ToList();
			Log.SimpleInfo("Loaded EnemyIDs for Class " + ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
		}
	}

	public void LoadAbilityDeck(AbilityDeckState deck)
	{
		InitialState = deck.InitialState;
		RoundAbilityCard = null;
		if (deck.HandAbilityCardIDsAndInstanceIDs != null && deck.HandAbilityCardIDsAndInstanceIDs.Count > 0)
		{
			try
			{
				AbilityCards = deck.HandAbilityCardIDsAndInstanceIDs.Select((Tuple<int, int> s) => AbilityCardsPool.Single((CMonsterAbilityCard c) => c.ID == s.Item1)).ToList();
			}
			catch (Exception ex)
			{
				Log.Error("Failed to load in monster ability cards from save data for MonsterID: " + ID + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		else
		{
			AbilityCards.Clear();
		}
		if (deck.RoundAbilityCardIDsAndInstanceID != null && deck.RoundAbilityCardIDsAndInstanceID.Count > 0)
		{
			try
			{
				RoundAbilityCard = AbilityCardsPool.Single((CMonsterAbilityCard s) => s.ID == deck.RoundAbilityCardIDsAndInstanceID[0].Item1);
			}
			catch
			{
				throw new Exception("Failed to load in round cards from save data");
			}
		}
		if (deck.DiscardedAbilityCardIDs != null && deck.DiscardedAbilityCardIDs.Count > 0)
		{
			try
			{
				DiscardedAbilityCards = deck.DiscardedAbilityCardIDs.Select((int s) => AbilityCardsPool.Single((CMonsterAbilityCard c) => c.ID == s)).ToList();
				return;
			}
			catch (Exception ex2)
			{
				Log.ErrorWithReport("Failed to load in discarded cards from save data\n" + ex2.Message + "\n" + ex2.StackTrace);
				DiscardedAbilityCards = new List<CMonsterAbilityCard>();
				return;
			}
		}
		DiscardedAbilityCards.Clear();
	}

	public void LoadMonsterActiveCardsAndBonuses(AbilityDeckState deck)
	{
		List<CBaseCard> list = new List<CBaseCard>();
		List<int> list2 = new List<int>();
		if (deck.ActivatedAbilityCardIDs != null)
		{
			try
			{
				foreach (Tuple<int, CBaseCard.ECardType, string> activatedAbilityCard in deck.ActivatedAbilityCardIDs)
				{
					switch (activatedAbilityCard.Item2)
					{
					case CBaseCard.ECardType.CharacterAbility:
						list2.Add(activatedAbilityCard.Item1);
						break;
					case CBaseCard.ECardType.MonsterAbility:
					{
						CBaseCard cBaseCard = AbilityCardsPool.Single((CMonsterAbilityCard c) => c.ID == activatedAbilityCard.Item1);
						if (cBaseCard != null)
						{
							list.Add(cBaseCard);
							ActivatedCards.Add(cBaseCard);
							break;
						}
						throw new Exception("Ability card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.Item:
					case CBaseCard.ECardType.Pet:
					{
						CItem getItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData c) => c.ID == activatedAbilityCard.Item1).GetItem;
						if (getItem != null)
						{
							list.Add(getItem);
							ActivatedCards.Add(getItem);
							break;
						}
						throw new Exception("Item card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.AttackModifier:
					{
						AttackModifierCard card = ScenarioRuleClient.SRLYML.AttackModifiers.Single((AttackModifierYMLData c) => c.ID == activatedAbilityCard.Item1).Card;
						if (card != null)
						{
							list.Add(card);
							ActivatedCards.Add(card);
							break;
						}
						throw new Exception("Attack Modifier card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.Dummy:
					{
						CBaseCard item = ActivatedCards.SingleOrDefault((CBaseCard x) => x.ID == activatedAbilityCard.Item1 && x.CardType == CBaseCard.ECardType.Dummy) ?? CBaseCard.CreateDummy(activatedAbilityCard.Item3, activatedAbilityCard.Item1);
						list.Add(item);
						ActivatedCards.Add(item);
						break;
					}
					default:
						throw new Exception("Invalid Activated Card Type.  Unable to load activated cards");
					}
				}
				for (int num = ActivatedCards.Count - 1; num >= 0; num--)
				{
					CBaseCard activatedCard = ActivatedCards[num];
					if (!deck.ActiveBonuses.Any((ActiveBonusState a) => a.CardID == activatedCard.ID))
					{
						ActivatedCards.Remove(activatedCard);
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Failed to load in activated cards from save data\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		if (deck.ActiveBonuses == null || deck.ActiveBonuses.Count <= 0)
		{
			return;
		}
		foreach (ActiveBonusState activeBonusState in deck.ActiveBonuses)
		{
			if (list2.Contains(activeBonusState.CardID))
			{
				continue;
			}
			CActor cActor = ScenarioManager.FindActor(activeBonusState.ActorGuid);
			CActor caster = ScenarioManager.FindActor(activeBonusState.CasterGuid);
			if (cActor == null)
			{
				Log.Error("Unable to find actor attached to active bonus");
				continue;
			}
			int? num2 = null;
			if (activeBonusState.BespokeBehaviourStrength != 0)
			{
				num2 = activeBonusState.BespokeBehaviourStrength;
			}
			if (RoundAbilityCard != null && RoundAbilityCard.ID == activeBonusState.CardID && (!activeBonusState.NameIsValid() || activeBonusState.CardName == RoundAbilityCard.Name))
			{
				CAbility cAbility = RoundAbilityCard.FindAbilityOnCard(activeBonusState.AbilityName);
				if (cAbility == null)
				{
					Log.ErrorWithReport("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
					continue;
				}
				CMonsterAbilityCard roundAbilityCard = RoundAbilityCard;
				CAbility ability = CAbility.CopyAbility(cAbility, generateNewID: false);
				int? iD = activeBonusState.ID;
				int? bespokeStrength = num2;
				roundAbilityCard.AddActiveBonus(ability, cActor, caster, iD, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
				continue;
			}
			CBaseCard cBaseCard2 = list.SingleOrDefault((CBaseCard x) => x.ID == activeBonusState.CardID);
			if (cBaseCard2 != null)
			{
				CItem cItem = cBaseCard2 as CItem;
				CAbilityCard cAbilityCard = cBaseCard2 as CAbilityCard;
				CAbility cAbility2 = null;
				if (cBaseCard2.CardType == CBaseCard.ECardType.Dummy)
				{
					cAbility2 = CAbility.FromJson(activeBonusState.AbilityJson);
				}
				else if (cBaseCard2 is AttackModifierCard attackModifierCard)
				{
					cAbility2 = attackModifierCard.YMLData.Abilities.SingleOrDefault((CAbility x) => x.Name == activeBonusState.AbilityName);
					if (cAbility2 == null)
					{
						cAbility2 = attackModifierCard.YMLData.Abilities.SelectMany((CAbility x) => x.SubAbilities).SingleOrDefault((CAbility x) => x.Name == activeBonusState.AbilityName);
					}
				}
				else
				{
					cAbility2 = ((cAbilityCard != null) ? (cAbilityCard.FindAbilityOnCard(activeBonusState.AbilityID) ?? cAbilityCard.FindAbilityOnCard(activeBonusState.AbilityName)) : cItem?.FindAbilityOnItemCard(activeBonusState.AbilityName));
				}
				cBaseCard2.ActionHasHappened = activeBonusState.ActionHasHappened;
				if (cAbility2 == null)
				{
					Log.Error("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
					continue;
				}
				CAbility cAbility3 = null;
				cAbility3 = ((cBaseCard2.CardType != CBaseCard.ECardType.Dummy) ? CAbility.CopyAbility(cAbility2, generateNewID: false) : cAbility2);
				cAbility3.Name = activeBonusState.AbilityName;
				cAbility3.CachedAbilityBaseCard = cBaseCard2;
				if (activeBonusState.AbilityStrength.HasValue)
				{
					cAbility3.Strength = activeBonusState.AbilityStrength.Value;
				}
				if (activeBonusState.AuraRange.HasValue && cAbility3.ActiveBonusData.IsAura && cAbility3.ActiveBonusData.AuraTargeting == CAbility.EAbilityTargeting.Range)
				{
					cAbility3.Range = activeBonusState.AuraRange.Value;
				}
				CActiveBonus cActiveBonus = cBaseCard2.AddActiveBonus(cAbility3, cActor, caster, activeBonusState.ID, activeBonusState.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, num2, activeBonusState.ActiveBonusStartRound, activeBonusState.ToggledBonus, bonusCheck: false);
				if (activeBonusState.RestrictedActorGuids == null)
				{
					continue;
				}
				foreach (string restrictedActorGuid in activeBonusState.RestrictedActorGuids)
				{
					CActor cActor2 = ScenarioManager.FindActor(restrictedActorGuid);
					if (cActor2 != null)
					{
						cActiveBonus.RestrictActiveBonus(cActor2, manualRestrict: true);
					}
				}
				continue;
			}
			CItem cItem2 = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData s2) => s2.ID == activeBonusState.CardID && activeBonusState.CardName == s2.Name)?.GetItem;
			if (cItem2 != null)
			{
				CAbility cAbility4 = cItem2.YMLData.Data.Abilities.SingleOrDefault((CAbility s) => s.Name == activeBonusState.AbilityName);
				if (cAbility4 != null)
				{
					CAbility ability2 = CAbility.CopyAbility(cAbility4, generateNewID: false);
					int? iD2 = activeBonusState.ID;
					int? bespokeStrength = num2;
					cItem2.AddActiveBonus(ability2, cActor, caster, iD2, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
				}
				else
				{
					Log.ErrorWithReport("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
				}
				continue;
			}
			AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == activeBonusState.CardName);
			if (attackModifierYMLData != null)
			{
				foreach (CAbility item2 in attackModifierYMLData.Abilities.Where((CAbility w) => w.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA))
				{
					AttackModifierCard card2 = attackModifierYMLData.Card;
					int? iD3 = activeBonusState.ID;
					int? bespokeStrength = num2;
					card2.AddActiveBonus(item2, cActor, caster, iD3, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
				}
			}
			else
			{
				Log.Error("Unable to find card with ID " + activeBonusState.CardID + " for active bonus state");
			}
		}
	}

	public void ClearRoundAbilityCardForEditor()
	{
		if (RoundAbilityCard != null)
		{
			if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == RoundAbilityCard.GetMonsterCardYML.ID))
			{
				Log.Warning("Duplicate ability " + RoundAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + ID);
			}
			AbilityCards.Remove(RoundAbilityCard);
			AbilityCards.Insert(0, RoundAbilityCard);
			RoundAbilityCard = null;
		}
	}

	public int Initiative(CEnemyActor actor, bool includeOverrideCard)
	{
		int num = 0;
		bool flag = false;
		int initiativeBonus = actor.GetInitiativeBonus();
		if (MonsterClassIDToActImmediatelyBefore != null)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonsters.FirstOrDefault((CEnemyActor x) => x.MonsterClass.ID == MonsterClassIDToActImmediatelyBefore);
			if (cEnemyActor?.MonsterClass?.RoundAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				num = cEnemyActor.MonsterClass.RoundAbilityCard.Initiative;
				flag = true;
			}
		}
		CMonsterAbilityCard cMonsterAbilityCard = RoundAbilityCard;
		if (includeOverrideCard && actor.OverrideRoundAbilityCard != null)
		{
			cMonsterAbilityCard = actor.OverrideRoundAbilityCard;
		}
		if (!flag && cMonsterAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			num = cMonsterAbilityCard.Initiative;
			flag = true;
		}
		return num + initiativeBonus;
	}
}
