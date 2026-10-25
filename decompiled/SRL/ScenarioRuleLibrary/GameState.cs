using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using Libs.GameConsole;
using Libs.Threading;
using MathParserTK;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Achievments;

namespace ScenarioRuleLibrary;

public class GameState
{
	public enum EActionSequenceType
	{
		FirstAction,
		SecondAction,
		ThirdAction,
		Complete
	}

	[Flags]
	public enum EActionFlag
	{
		None = 0,
		TopPlayed = 1,
		BottomPlayed = 2,
		TopForgone = 4,
		BottomForgone = 8
	}

	public enum EActionInitiator
	{
		None,
		AbilityCard,
		ItemCard,
		ActiveBonus,
		ScenarioModifier,
		CompanionSummon,
		ActionsTriggeredOutsideActionPhase,
		OverrideTurnAction
	}

	public enum EAvoidDamageOption
	{
		None,
		Lose1HandCard,
		Lose2DiscardCards,
		UsePreventDamageActiveBonus
	}

	public class ActorInitiativeComparer : IComparer<CActor>
	{
		public bool CheckForAdjustment { get; set; }

		public ActorInitiativeComparer(bool checkforadjustment = false)
		{
			CheckForAdjustment = checkforadjustment;
		}

		public int CompareWithClass(CActor x, CActor y, CClass xClass, CClass yClass)
		{
			if (x == null || y == null)
			{
				return 0;
			}
			if (x.GetMirrorAbilitiesActor() == y)
			{
				return 1;
			}
			bool flag = ShouldReverseInitiativeOrder();
			int num = ((xClass is CMonsterClass cMonsterClass) ? cMonsterClass.Initiative(x as CEnemyActor, includeOverrideCard: false) : x.Initiative());
			int num2 = ((yClass is CMonsterClass cMonsterClass2) ? cMonsterClass2.Initiative(y as CEnemyActor, includeOverrideCard: false) : y.Initiative());
			if (CheckForAdjustment)
			{
				List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(x, CAbility.EAbilityType.AdjustInitiative);
				if (list.Count > 0)
				{
					foreach (CActiveBonus item in list)
					{
						if (item.TryGetBehaviour(out CAdjustInitiativeActiveBonus_FocusInitiative behaviour))
						{
							num = behaviour.SetFocusInitiative();
						}
					}
				}
				list = CActiveBonus.FindApplicableActiveBonuses(y, CAbility.EAbilityType.AdjustInitiative);
				if (list.Count > 0)
				{
					foreach (CActiveBonus item2 in list)
					{
						if (item2.TryGetBehaviour(out CAdjustInitiativeActiveBonus_FocusInitiative behaviour2))
						{
							num2 = behaviour2.SetFocusInitiative();
						}
					}
				}
			}
			if (num != num2)
			{
				if (flag)
				{
					return num2.CompareTo(num);
				}
				return num.CompareTo(num2);
			}
			if (x.Type == CActor.EType.Player && y.Type == CActor.EType.Player && x.SubInitiative() != y.SubInitiative())
			{
				if (flag)
				{
					return y.SubInitiative().CompareTo(x.SubInitiative());
				}
				return x.SubInitiative().CompareTo(y.SubInitiative());
			}
			if (x is CHeroSummonActor cHeroSummonActor && y is CPlayerActor cPlayerActor)
			{
				if (cPlayerActor == cHeroSummonActor.Summoner)
				{
					if (CActiveBonus.FindApplicableActiveBonuses(cPlayerActor, CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.FocusInitiativeBeforeSummons).Any())
					{
						return 1;
					}
					return -1;
				}
				return cHeroSummonActor.Summoner.SubInitiative().CompareTo(cPlayerActor.SubInitiative());
			}
			if (y is CHeroSummonActor cHeroSummonActor2 && x is CPlayerActor cPlayerActor2)
			{
				if (cPlayerActor2 == cHeroSummonActor2.Summoner)
				{
					if (CActiveBonus.FindApplicableActiveBonuses(cPlayerActor2, CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.FocusInitiativeBeforeSummons).Any())
					{
						return -1;
					}
					return 1;
				}
				return cPlayerActor2.SubInitiative().CompareTo(cHeroSummonActor2.Summoner.SubInitiative());
			}
			if (x is CHeroSummonActor cHeroSummonActor3 && y is CHeroSummonActor cHeroSummonActor4 && cHeroSummonActor3.Summoner == cHeroSummonActor4.Summoner)
			{
				if (cHeroSummonActor3.StandeeID <= cHeroSummonActor4.StandeeID)
				{
					return -1;
				}
				return 1;
			}
			if (x.Type == CActor.EType.Player && y.IsMonsterType)
			{
				return -1;
			}
			if (x.IsMonsterType && y.Type == CActor.EType.Player)
			{
				return 1;
			}
			if (x.Class is CMonsterClass cMonsterClass3 && y.Class is CMonsterClass cMonsterClass4)
			{
				bool boss = cMonsterClass3.Boss;
				bool boss2 = cMonsterClass4.Boss;
				if (boss != boss2)
				{
					if (boss && !boss2)
					{
						return -1;
					}
					return 1;
				}
				if (cMonsterClass3.MonsterClassIDToActImmediatelyBefore == cMonsterClass4.ID)
				{
					return -1;
				}
				if (cMonsterClass3.ID == cMonsterClass4.MonsterClassIDToActImmediatelyBefore)
				{
					return 1;
				}
				int monsterGroupType = ((CEnemyActor)x).MonsterGroupType;
				int num3 = ((CEnemyActor)y).MonsterGroupType.CompareTo(monsterGroupType);
				if (num3 != 0)
				{
					return num3;
				}
				if (cMonsterClass3.ID == cMonsterClass4.ID)
				{
					int subInitiative = ((CEnemyActor)x).SubInitiative;
					num3 = ((CEnemyActor)y).SubInitiative.CompareTo(subInitiative);
					if (num3 != 0)
					{
						return num3;
					}
				}
				num3 = string.Compare(x.Class.ID, y.Class.ID);
				if (num3 != 0)
				{
					return num3;
				}
				bool flag2 = cMonsterClass3.NonEliteVariant == null;
				bool flag3 = cMonsterClass4.NonEliteVariant == null;
				if (flag2 != flag3)
				{
					if (flag2 && !flag3)
					{
						return 1;
					}
					return -1;
				}
				num3 = string.Compare(x.Type.ToString(), y.Type.ToString());
				if (num3 != 0)
				{
					return num3;
				}
			}
			int iD = x.ID;
			if (x is CHeroSummonActor cHeroSummonActor5)
			{
				iD = cHeroSummonActor5.Summoner.ID;
			}
			int iD2 = y.ID;
			if (y is CHeroSummonActor cHeroSummonActor6)
			{
				iD2 = cHeroSummonActor6.Summoner.ID;
			}
			return iD.CompareTo(iD2);
		}

		public int Compare(CActor x, CActor y, CClass xClass, CClass yClass)
		{
			int result = CompareWithClass(x, y, xClass, yClass);
			int num = TurnOrderSoFar.IndexOf((x, xClass));
			int num2 = TurnOrderSoFar.IndexOf((y, yClass));
			if (num != -1 && num2 == -1)
			{
				return -1;
			}
			if (num2 != -1 && num == -1)
			{
				return 1;
			}
			if (num != -1 && num2 != -1)
			{
				int num3 = num.CompareTo(num2);
				if (num3 != 0)
				{
					return num3;
				}
			}
			return result;
		}

		public int Compare(CActor x, CActor y)
		{
			CClass xClass = x?.Class;
			if (x is CEnemyActor { OverrideRoundAbilityCard: not null } cEnemyActor)
			{
				xClass = MonsterClassManager.Find(cEnemyActor.OverrideRoundAbilityCard.ClassID);
			}
			CClass yClass = y?.Class;
			if (y is CEnemyActor { OverrideRoundAbilityCard: not null } cEnemyActor2)
			{
				yClass = MonsterClassManager.Find(cEnemyActor2.OverrideRoundAbilityCard.ClassID);
			}
			return Compare(x, y, xClass, yClass);
		}
	}

	public class CRangeSortedActor
	{
		public CActor m_Actor;

		public int m_Range;

		public bool m_Self;

		public CRangeSortedActor(CActor actor, int range, bool self = false)
		{
			m_Actor = actor;
			m_Range = range;
			m_Self = self;
		}
	}

	public class CurrentActorAction
	{
		public CAction Action { get; private set; }

		public CBaseCard BaseCard { get; private set; }

		public CActor ActionInitiator { get; private set; }

		public CurrentActorAction(CAction action, CBaseCard baseCard, CActor initiator = null)
		{
			Action = action;
			BaseCard = baseCard;
			ActionInitiator = initiator ?? CurrentActor;
		}
	}

	public class OverrideActorForActionStackData
	{
		public CActor PreviousActor;

		public bool OverrideSwitchedType;

		public CActor.EType OriginalType;

		public bool OverrideKillActor;

		public int PreOverrideAugmentSlots;

		public List<CAugment> AddedAugments;

		public CActor ControllingActor;

		public bool UseControllingActorModifierDeck;

		public bool UseControllingActorPlayerControl;

		public CActor ActorBeingControlled;

		public bool TreatAsNonControl;

		public OverrideActorForActionStackData(CActor previousActor, bool overrideSwitchedType, CActor.EType originalType, bool overrideKillActor, int preOverrideAugmentSlots, CActor actorBeingControlled, List<CAugment> addedAugments = null, CActor controllingActor = null, bool useControllingActorModifierDeck = false, bool useControllingActorPlayerControl = false, bool treatAsNonControl = false)
		{
			ActorBeingControlled = actorBeingControlled;
			PreviousActor = previousActor;
			OverrideSwitchedType = overrideSwitchedType;
			OriginalType = originalType;
			OverrideKillActor = overrideKillActor;
			PreOverrideAugmentSlots = preOverrideAugmentSlots;
			AddedAugments = addedAugments;
			ControllingActor = controllingActor;
			UseControllingActorModifierDeck = useControllingActorModifierDeck;
			UseControllingActorPlayerControl = useControllingActorPlayerControl;
			TreatAsNonControl = treatAsNonControl;
		}
	}

	public class QueueOverrideActorForTurnData
	{
		public CActor.EType OverrideActorType;

		public List<CAbility> OverrideActionAbilities;

		public CBaseCard OverrideActionBaseCard;

		public CActor ControllingActor;

		public bool UseControllingActorPlayerControl;

		public QueueOverrideActorForTurnData(CActor.EType overrideActorType, List<CAbility> overrideActionAbilities, CBaseCard overrideActionBaseCard, CActor controllingActor = null, bool useControllingActorPlayerControl = false)
		{
			OverrideActorType = overrideActorType;
			OverrideActionAbilities = overrideActionAbilities;
			OverrideActionBaseCard = overrideActionBaseCard;
			ControllingActor = controllingActor;
			UseControllingActorPlayerControl = useControllingActorPlayerControl;
		}
	}

	public class OverrideActorForTurnData
	{
		public CActor OverridenActor;

		public bool OverrideSwitchedType;

		public CActor.EType OriginalType;

		public CActor ControllingActor;

		public OverrideActorForTurnData(CActor overridenActor, bool overrideSwitchedType, CActor.EType originalType, CActor controllingActor)
		{
			OverridenActor = overridenActor;
			OverrideSwitchedType = overrideSwitchedType;
			OriginalType = originalType;
			ControllingActor = controllingActor;
		}
	}

	public class DamageData
	{
		public CActor ActorDamaged;

		public int DamageSourceStrength;

		public int DamageSourceStrengthBeforeWardBrittle;

		public int Shield;

		public int PreDamageHealth;

		public CActor DamageSourceActor;

		public CAbility DamageSourceAbility;

		public CAbility.EAbilityType DamageSourceAbilityType;

		public CActor.ECauseOfDamage CauseOfDamage;

		public bool IsTrapDamage;

		public bool IsTerrainDamage;

		public bool CheckIfPlayerCanAvoidDamageByBurningCards;

		public bool CannotPreventDamageWithActiveBonus;

		public bool TriggerOnDamageListeners;

		public int DamageAvoided;

		public int DamageShielded;

		public int DamageShieldedByItems;

		public bool HadRegeneration;

		public List<PositiveConditionPair> PositiveConditionsBeforeDamage { get; private set; }

		public bool IsDirectDamage
		{
			get
			{
				if (!IsTrapDamage && !IsTerrainDamage)
				{
					return DamageSourceAbilityType != CAbility.EAbilityType.Attack;
				}
				return true;
			}
		}

		public DamageData(CActor actorDamaged, int damageSourceStrength, int shield, int preDamageHealth, CActor damageSourceActor, CAbility damageSourceAbility, CAbility.EAbilityType damageSourceAbilityType, CActor.ECauseOfDamage causeOfDamage, bool isTrapDamage, bool isTerrainDamage, bool checkIfPlayerCanAvoidDamageByBurningCards, bool cannotPreventDamageWithActiveBonus, bool triggerOnDamageListeners, bool hadRegeneration, List<PositiveConditionPair> positiveConditionsBeforeDamage, int damageSourceStrengthBeforeWardBrittle)
		{
			ActorDamaged = actorDamaged;
			DamageSourceStrength = damageSourceStrength;
			Shield = shield;
			PreDamageHealth = preDamageHealth;
			DamageSourceActor = damageSourceActor;
			DamageSourceAbility = damageSourceAbility;
			DamageSourceAbilityType = damageSourceAbilityType;
			CauseOfDamage = causeOfDamage;
			IsTrapDamage = isTrapDamage;
			IsTerrainDamage = isTerrainDamage;
			CheckIfPlayerCanAvoidDamageByBurningCards = checkIfPlayerCanAvoidDamageByBurningCards;
			CannotPreventDamageWithActiveBonus = cannotPreventDamageWithActiveBonus;
			TriggerOnDamageListeners = triggerOnDamageListeners;
			HadRegeneration = hadRegeneration;
			PositiveConditionsBeforeDamage = positiveConditionsBeforeDamage;
			DamageSourceStrengthBeforeWardBrittle = damageSourceStrengthBeforeWardBrittle;
		}
	}

	public class DummyReplicateDamageState
	{
	}

	[ConsoleVariable(Alias = "skip_monster_turns", Persistent = false)]
	public static bool s_SkipMonsterTurns = false;

	[ConsoleVariable(Alias = "auto_restore_player_cards", Persistent = false)]
	public static bool s_AutoRestorePlayerCards = false;

	public static bool s_AutoSelectPlayerCards = false;

	[ConsoleVariable(Alias = "auto_take_damage", Persistent = false)]
	public static bool s_AutoTakeDamage = false;

	public static float s_MulPlayerHealth = 1f;

	[ConsoleVariable(Alias = "default_move_strength", Persistent = false)]
	public static int s_DefaultMoveStrength = 2;

	[ConsoleVariable(Alias = "desync_var", Persistent = false)]
	public static int s_DesyncVar = 0;

	[ConsoleVariable(Alias = "delayed_short_rest_in_single_player", Persistent = false)]
	public static bool s_DelayedShortRestInSinglePlayer = false;

	[ConsoleVariable(Alias = "enable_auto_end_selection_after_short_rest", Persistent = true)]
	public static bool s_EnableAutoEndSelectionAfterShortRest = true;

	public static List<(CActor, CClass)> TurnOrderSoFar = new List<(CActor, CClass)>();

	private static EActionInitiator _CurrentActionInitiator;

	public static bool ProtectLastDummyCardID;

	[StateDump(Skip = true)]
	public static MathParser s_MathParser = new MathParser();

	public static ActorInitiativeComparer s_ActorInitiativeComparer = new ActorInitiativeComparer();

	public static ActorInitiativeComparer s_ActorAdjustedInitiativeComparer = new ActorInitiativeComparer(checkforadjustment: true);

	public static volatile bool s_ThreadAboutToAbort;

	private static Dictionary<string, List<Tuple<int, EActionFlag>>[]> s_PlayedCards = new Dictionary<string, List<Tuple<int, EActionFlag>>[]>();

	private static List<CActor> s_InitiativeSortedActors = new List<CActor>();

	private static CActor s_LastActor;

	public static CPlayerActor s_ShortRestingPlayer;

	public static int s_ShortRestCardToBurnID;

	private static CActor s_TurnActor;

	private static EActionFlag[] s_ActionFlagsByActionSequenceType = new EActionFlag[Enum.GetNames(typeof(EActionSequenceType)).Length];

	private static List<CActionAugmentation> s_CurrentActionValidAugmentations = new List<CActionAugmentation>();

	private static List<CActionAugmentation> s_CurrentActionSelectedAugmentations = new List<CActionAugmentation>();

	private static int s_RoundCount;

	private static List<KeyValuePair<CEnemyActor, CActor>> s_EnemyKillTracker;

	private static int s_DamageInflictedThisTurn;

	private static int s_DamagePreventedThisTurn;

	private static List<CActor> s_ActorsMovedThisTurn;

	private static List<CActor> s_ActorsKilledThisTurn;

	private static List<CActor> s_ActorsKilledThisRound;

	private static int s_TrophyTokensGainedThisRound;

	private static int s_TargetsDamagedInPrevAttackThisTurn;

	private static Guid s_TargetsDamagedInPrevAttackThisTurnAbilityID;

	private static int s_TargetsActuallyDamagedInPrevAttackThisTurn;

	private static Guid s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID;

	private static int s_TargetsDamagedInPrevDamageAbilityThisTurn;

	private static Guid s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID;

	private static List<TileIndex> s_HexesMovedThisTurn = new List<TileIndex>();

	private static int s_ObstaclesDestroyedThisTurn;

	private static int s_HazardousTerrainTilesMovedOverThisTurn;

	private static int s_DifficultTerrainTilesMovedOverThisTurn;

	private static volatile EAvoidDamageOption s_PlayerSelectedToAvoidDamage;

	private static volatile List<CAbilityCard> s_CardsBurnedToAvoidDamage;

	private static volatile List<CActiveBonus> s_UsedPreventDamageBonusesToAvoidDamage;

	private static volatile bool s_RecievedPlayerActorToAvoidDamageResponse;

	private static volatile CPlayerActor s_PlayerActorToAvoidDamage;

	private static volatile bool s_RecievedSelectedToAvoidDamageResponse;

	[StateDump(Skip = true)]
	private static volatile bool s_RecievedContinueAbilityResponse;

	[StateDump(Skip = true)]
	private static volatile bool s_ReceivedMultiplayerFinishWaitingResponse;

	private static volatile List<CActiveBonus> s_ToggledPreventBonuses;

	private static volatile List<CActiveBonus> s_ToggledShieldBonuses;

	private static volatile List<CItem> s_ToggledShieldItems;

	private static int s_RecievedDamagedReducedByShieldItems;

	private static int s_RecievedDamagePreventedByActiveBonuses;

	private static Dictionary<CActor, QueueOverrideActorForTurnData> s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();

	private static Stack<OverrideActorForActionStackData> s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();

	private static Stack<OverrideActorForActionStackData> s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();

	private static int s_CheckAdjustInitiativeIndex = 0;

	private static List<CAbility> s_PerksSelectedToUseAfterUnfreezingThread = new List<CAbility>();

	private static List<AttackModifierYMLData> s_AttackModifierCardsSelectedToUseAfterUnfreezingThread = new List<AttackModifierYMLData>();

	private static List<LootCard> s_LootCardsSelectedAfterUnfreezingThread = new List<LootCard>();

	[StateDump(Skip = true)]
	private static int s_TotalShieldDuringLastAttack;

	private static bool s_GenericConfirmButtonToggled = false;

	private static bool s_WaitingForGenericConfirmation = false;

	private const int c_InitiativeBitShift = 24;

	private const int c_PlayerEnemyBitShift = 16;

	private const int c_EliteBitShift = 8;

	public static Func<CPlayerActor> s_getHostCharacter;

	public static Func<CPlayerActor> s_getHostCharacterIncludingDead;

	public static Stack<CAbility> OutOfPhaseAbilityStack = new Stack<CAbility>();

	public static Dictionary<CAbilityCard, CBaseCard.ECardPile> __OutOfPhaseCardFromPiles = new Dictionary<CAbilityCard, CBaseCard.ECardPile>();

	public int actorID;

	public string actorGUID;

	public int damageSourceID;

	public string damageSourceGUID;

	public int PlayerActorToAvoidDamageID;

	public List<int> CardsBurnedToAvoidDamageIDs;

	public List<int> UsedPreventDamageBonusesToAvoidDamageIDs;

	public List<int> ToggledPreventBonusesIDs;

	public List<int> ToggledShieldBonusesIDs;

	public List<int> ToggledShieldItemIDs;

	public const string ItemSelectionSleepKey = "ItemSelection";

	public const string ChooseWhoWillTakeDamageSleepKey = "SelectingToNegateDamageForAlly";

	public const string WaitingForMultiplayerSyncCheckKey = "SyncWaitForConfirmation";

	private const string WaitingForCampaignSectionKey = "WaitingForCampaignSection_{0}";

	[StateDump(Skip = true)]
	public static bool ChooseAdvantageModifiers { get; set; }

	public static Dictionary<string, int> EnemyKillStatsByClass
	{
		get
		{
			if (s_EnemyKillTracker != null)
			{
				return s_EnemyKillTracker?.GroupBy((KeyValuePair<CEnemyActor, CActor> item) => item.Key.MonsterClass.ID).ToDictionary((IGrouping<string, KeyValuePair<CEnemyActor, CActor>> g) => g.Key, (IGrouping<string, KeyValuePair<CEnemyActor, CActor>> g) => g.Count());
			}
			return new Dictionary<string, int>();
		}
	}

	public static Dictionary<string, int> EnemyEncounteredByClass
	{
		get
		{
			Dictionary<string, int> enemyKillStatsByClass = EnemyKillStatsByClass;
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				string iD = enemy.MonsterClass.ID;
				if (enemyKillStatsByClass.ContainsKey(iD))
				{
					enemyKillStatsByClass[iD]++;
				}
				else
				{
					enemyKillStatsByClass.Add(iD, 1);
				}
			}
			return enemyKillStatsByClass;
		}
	}

	public static int LastSpawnerStartRound { get; set; }

	public static bool IsInLevelEditor { get; set; }

	public static CActor InternalCurrentActor => s_CurrentActor;

	public static CActor TurnActor => s_TurnActor;

	public static List<CActor> InitiativeSortedActors => s_InitiativeSortedActors;

	public static CurrentActorAction CurrentAction { get; set; }

	public static List<CActiveBonus> PendingOnLongRestBonuses { get; set; }

	public static List<CActiveBonus> PendingOnShortRestBonuses { get; set; }

	public static bool HasShortRestWithPreRestAbilities { get; set; }

	public static List<(CAbility ability, CActor actor)> PendingOnRestAbilities { get; set; }

	public static bool HasPendingRestAbilities
	{
		get
		{
			if ((PendingOnLongRestBonuses == null || PendingOnLongRestBonuses.Count <= 0) && (PendingOnShortRestBonuses == null || PendingOnShortRestBonuses.Count <= 0))
			{
				if (PendingOnRestAbilities != null)
				{
					return PendingOnRestAbilities.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	public static List<CActiveBonus> PendingActiveBonuses { get; set; }

	public static List<CActiveBonus> PendingStartEndTurnAbilityBonusTriggers { get; set; }

	public static List<Tuple<CAbility, CActor>> PendingScenarioModifierAbilities { get; set; }

	public static List<CPlayerActor> PendingCompanionSummonActors { get; set; }

	public static bool SkipNextPhase { get; set; }

	public static bool EndRoundReached { get; set; }

	public static Stack<EActionFlag> ExtraTurnActionSelectionFlagStack { get; set; }

	public static Stack<EActionFlag> OutOfPhaseActionSelectionFlagStack { get; set; }

	public static CAbilityExtraTurn CurrentExtraTurnAbility { get; set; }

	public static EActionInitiator CurrentActionInitiator
	{
		get
		{
			return _CurrentActionInitiator;
		}
		set
		{
			Log.Info($"CurrentActionInitiator: {_CurrentActionInitiator} -> {value}");
			_CurrentActionInitiator = value;
		}
	}

	public static List<CActionAugmentation> CurrentActionValidAugmentations => s_CurrentActionValidAugmentations;

	public static List<CActionAugmentation> CurrentActionSelectedAugmentations => s_CurrentActionSelectedAugmentations;

	public static bool OutputAdvantageInfo { get; set; }

	public static int RoundCount => s_RoundCount;

	public static Stack<DamageData> DamageDataStack { get; private set; }

	public static DamageData CurrentDamageData
	{
		get
		{
			if (DamageDataStack == null || DamageDataStack.Count <= 0)
			{
				return null;
			}
			return DamageDataStack.Peek();
		}
	}

	public static CAttackSummary.TargetSummary LastTargetSummary { get; set; }

	public static CAbilityCard RoundAbilityCardSelected { get; set; }

	public static CAbilityCard ExtraRoundAbilityCardselected { get; set; }

	public static CBaseCard.ActionType RoundAbilityCardActionType { get; private set; }

	public static CBaseCard.ActionType LastAbilityCardActionType { get; private set; } = CBaseCard.ActionType.NA;

	public static EAvoidDamageOption PlayerSelectedToAvoidDamage => s_PlayerSelectedToAvoidDamage;

	public static List<CAbilityCard> CardsBurnedToAvoidDamage => s_CardsBurnedToAvoidDamage;

	public static int DamageInflictedThisTurn => s_DamageInflictedThisTurn;

	public static List<CActor> ActorsKilledThisTurn => s_ActorsKilledThisTurn;

	public static List<CActor> ActorsKilledThisRound => s_ActorsKilledThisRound;

	public static int TrophyTokensGainedThisRound => s_TrophyTokensGainedThisRound;

	public static int TargetsDamagedInPrevAttackThisTurn => s_TargetsDamagedInPrevAttackThisTurn;

	public static int TargetsActuallyDamagedInPrevAttackThisTurn => s_TargetsActuallyDamagedInPrevAttackThisTurn;

	public static int TargetsDamagedInPrevDamageAbilityThisTurn => s_TargetsDamagedInPrevDamageAbilityThisTurn;

	public static List<CActor> ActorsMovedThisTurn => s_ActorsMovedThisTurn;

	public static List<TileIndex> HexesMovedThisTurn => s_HexesMovedThisTurn;

	public static int ObstaclesDestroyedThisTurn => s_ObstaclesDestroyedThisTurn;

	public static bool WaitingForMercenarySpecialMechanicSlotChoice { get; set; }

	public static bool OverridingCurrentActor { get; set; }

	public static bool PlayedInfusion { get; set; }

	public static CPhaseInfuserMechanicSelection PendingInfusionPhase { get; set; }

	public static Stack<OverrideActorForActionStackData> OverridenActionActorStack => s_OverridenActionActorStack;

	public static Dictionary<CActor, QueueOverrideActorForTurnData> ActorsToOverrideTurns => s_ActorsToOverrideTurns;

	public static OverrideActorForTurnData CurrentOverridenActorForTurnData { get; private set; }

	public static Tuple<CActor, int> RedirectedDamageToActor { get; set; }

	public static Stack<CAbilityCard> CachedExtraTurnInitiatorCardStack { get; private set; }

	public static CObjectObstacle LastDestroyedObstacle { get; set; }

	public static List<CPlayerActor> PreInitiativeAdjustedPlayerActors { get; set; } = new List<CPlayerActor>();

	public static int RecievedDamagedReducedByShieldItems => s_RecievedDamagedReducedByShieldItems;

	public static List<CAbility> PerksSelectedToUseAfterUnfreezingThread => s_PerksSelectedToUseAfterUnfreezingThread;

	public static List<AttackModifierYMLData> CurrentlyChoosingModifiers { get; set; } = new List<AttackModifierYMLData>();

	public static CAbilityAttack CurrentAttackAbility
	{
		get
		{
			CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
			HashSet<CAbility> hashSet = new HashSet<CAbility>();
			while (cAbility != null && !(cAbility is CAbilityAttack) && !hashSet.Contains(cAbility))
			{
				hashSet.Add(cAbility);
				if (cAbility.ParentAbility == null)
				{
					if (cAbility.TargetingActor?.MindControlAbility == null || cAbility == cAbility.TargetingActor.MindControlAbility)
					{
						break;
					}
					cAbility = cAbility.TargetingActor.MindControlAbility;
				}
				else
				{
					cAbility = cAbility.ParentAbility;
				}
			}
			if (cAbility != null && cAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
			{
				return cAbility as CAbilityAttack;
			}
			return null;
		}
	}

	public static bool IsSelectingAdvantageModifier
	{
		get
		{
			if (ThreadSleepReason != null)
			{
				return ThreadSleepReason.Equals("AttackModifierAdvantage", StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}
	}

	public static List<AttackModifierYMLData> AttackModifierCardsSelectedToUseAfterUnfreezingThread => s_AttackModifierCardsSelectedToUseAfterUnfreezingThread;

	public static List<LootCard> LootCardsSelectedAfterUnfreezingThread => s_LootCardsSelectedAfterUnfreezingThread;

	[StateDump(Skip = true)]
	public static int TotalShieldDuringLastAttack
	{
		get
		{
			return s_TotalShieldDuringLastAttack;
		}
		set
		{
			s_TotalShieldDuringLastAttack = value;
		}
	}

	public static bool GenericConfirmButtonToggled
	{
		get
		{
			return s_GenericConfirmButtonToggled;
		}
		set
		{
			s_GenericConfirmButtonToggled = value;
		}
	}

	public static bool WaitingForGenericConfirmation
	{
		get
		{
			return s_WaitingForGenericConfirmation;
		}
		set
		{
			s_WaitingForGenericConfirmation = value;
		}
	}

	public static int LastDummyCardID { get; set; }

	[StateDump(Skip = true)]
	public static bool ThreadIsSleeping { get; set; }

	public static string ThreadSleepReason { get; set; }

	public static bool ShuffleAttackModsEnabledForPlayers { get; set; }

	public static bool ShuffleAbilityDecksEnabledForMonsters { get; set; }

	public static bool ShuffleAttackModsEnabledForMonsters { get; set; }

	public static bool RandomiseOnLoad { get; set; }

	public static bool WaitingForPlayerActorToContinueAbility
	{
		get
		{
			if (ThreadIsSleeping)
			{
				return !s_RecievedContinueAbilityResponse;
			}
			return false;
		}
	}

	public static bool WaitingForPlayerActorToAvoidDamageResponse => !s_RecievedPlayerActorToAvoidDamageResponse;

	public static bool WaitingForPlayerToSelectDamageResponse => !s_RecievedSelectedToAvoidDamageResponse;

	public static CActor CurrentActor
	{
		get
		{
			return s_CurrentActor;
		}
		set
		{
			s_CurrentActor = value;
		}
	}

	private static CActor s_CurrentActor { get; set; }

	public static CPlayerActor HostCharacter => s_getHostCharacter?.Invoke();

	public static CPlayerActor HostCharacterIncludingDead => s_getHostCharacterIncludingDead?.Invoke();

	public static bool IsLoadingClassActiveBonuses { get; private set; }

	public static CAbility CurrentOutOfPhaseCardSelectionAbility
	{
		get
		{
			if (OutOfPhaseAbilityStack.Count == 0)
			{
				return null;
			}
			return OutOfPhaseAbilityStack.Peek();
		}
		set
		{
			if (value == null)
			{
				if (OutOfPhaseAbilityStack.Count > 0)
				{
					OutOfPhaseAbilityStack.Pop();
				}
			}
			else if (!OutOfPhaseAbilityStack.Contains(value))
			{
				OutOfPhaseAbilityStack.Push(value);
			}
		}
	}

	public static CAbilitySelectAndPlayCard CurrentSelectAndPlayCardAbility => CurrentOutOfPhaseCardSelectionAbility as CAbilitySelectAndPlayCard;

	public static CAbilitySelectAndMoveCard CurrentSelectAndMoveCardsAbility => CurrentOutOfPhaseCardSelectionAbility as CAbilitySelectAndMoveCard;

	public static CAbilitySelectCard CurrentSelectCardsAbility => CurrentOutOfPhaseCardSelectionAbility as CAbilitySelectCard;

	public static bool IsPerformingCardOutOfPhase => CurrentOutOfPhaseCardSelectionAbility != null;

	public static bool IsChoosingToBurnCardForAlly
	{
		get
		{
			if (ThreadIsSleeping)
			{
				return ThreadSleepReason == "SelectingToNegateDamageForAlly";
			}
			return false;
		}
	}

	public static bool IsActionFlagSet(EActionFlag flag)
	{
		if (OutOfPhaseActionSelectionFlagStack.Count > 0)
		{
			EActionFlag eActionFlag = OutOfPhaseActionSelectionFlagStack.Peek();
			return eActionFlag.HasFlag(flag);
		}
		if (ExtraTurnActionSelectionFlagStack.Count > 0)
		{
			EActionFlag eActionFlag2 = ExtraTurnActionSelectionFlagStack.Peek();
			return eActionFlag2.HasFlag(flag);
		}
		for (int i = 0; i < 3; i++)
		{
			if (s_ActionFlagsByActionSequenceType[i].HasFlag(flag))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetPlayedActionsCount()
	{
		if (OutOfPhaseActionSelectionFlagStack.Count > 0)
		{
			int num = 0;
			EActionFlag eActionFlag = OutOfPhaseActionSelectionFlagStack.Peek();
			if (eActionFlag.HasFlag(EActionFlag.TopPlayed) || eActionFlag.HasFlag(EActionFlag.TopForgone))
			{
				num++;
			}
			if (eActionFlag.HasFlag(EActionFlag.BottomPlayed) || eActionFlag.HasFlag(EActionFlag.BottomForgone))
			{
				num++;
			}
			return num;
		}
		if (ExtraTurnActionSelectionFlagStack.Count > 0)
		{
			int num2 = 0;
			EActionFlag eActionFlag2 = ExtraTurnActionSelectionFlagStack.Peek();
			if (eActionFlag2.HasFlag(EActionFlag.TopPlayed) || eActionFlag2.HasFlag(EActionFlag.TopForgone))
			{
				num2++;
			}
			if (eActionFlag2.HasFlag(EActionFlag.BottomPlayed) || eActionFlag2.HasFlag(EActionFlag.BottomForgone))
			{
				num2++;
			}
			return num2;
		}
		return GetPlayedRoundActionsCount();
	}

	public static int GetPlayedRoundActionsCount()
	{
		return GetPlayedActionsCount(s_ActionFlagsByActionSequenceType);
	}

	public static int GetMaxRoundActionsCount()
	{
		return s_ActionFlagsByActionSequenceType.Length;
	}

	private static int GetPlayedActionsCount(IList<EActionFlag> actionFlagsByActionSequenceType)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			if (actionFlagsByActionSequenceType[i] != EActionFlag.None)
			{
				num++;
			}
		}
		return num;
	}

	public static EActionSequenceType GetCurrentActionSequenceType(int roundCardsCount)
	{
		if (CurrentOutOfPhaseCardSelectionAbility != null)
		{
			return GetCurrentActionSequenceTypeFromFlags(OutOfPhaseActionSelectionFlagStack.Peek());
		}
		if (ExtraTurnActionSelectionFlagStack.Count <= 0)
		{
			return GetCurrentActionSequenceType(s_ActionFlagsByActionSequenceType, roundCardsCount);
		}
		return GetCurrentActionSequenceTypeFromFlags(ExtraTurnActionSelectionFlagStack.Peek());
	}

	private static EActionSequenceType GetCurrentActionSequenceType(IList<EActionFlag> actionFlagsByActionSequenceType, int roundCardsCount)
	{
		int playedActionsCount = GetPlayedActionsCount(actionFlagsByActionSequenceType);
		if (roundCardsCount == 2 && playedActionsCount >= 0 && playedActionsCount <= 1)
		{
			return (EActionSequenceType)playedActionsCount;
		}
		if (roundCardsCount > 2 && playedActionsCount >= 0 && playedActionsCount <= 2)
		{
			return (EActionSequenceType)playedActionsCount;
		}
		return EActionSequenceType.Complete;
	}

	private static EActionSequenceType GetCurrentActionSequenceTypeFromFlags(EActionFlag flags)
	{
		if (flags.HasFlag(EActionFlag.TopPlayed) && flags.HasFlag(EActionFlag.BottomPlayed))
		{
			return EActionSequenceType.Complete;
		}
		if (flags.HasFlag(EActionFlag.TopPlayed) || flags.HasFlag(EActionFlag.BottomPlayed))
		{
			return EActionSequenceType.SecondAction;
		}
		return EActionSequenceType.FirstAction;
	}

	private static void ResetActionFlagsByActionSequenceType(IList<EActionFlag> actionFlagsByActionSequenceType)
	{
		for (int i = 0; i < actionFlagsByActionSequenceType.Count; i++)
		{
			actionFlagsByActionSequenceType[i] = EActionFlag.None;
		}
	}

	public static int UpdatedInitiative(CActor actor, int initiative)
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.AdjustInitiative);
		if (list.Count > 0)
		{
			foreach (CActiveBonus item in list)
			{
				if (item.TryGetBehaviour(out CAdjustInitiativeActiveBonus_FocusInitiative behaviour))
				{
					initiative = behaviour.SetFocusInitiative();
				}
			}
		}
		return initiative;
	}

	public static int UpdatedInitiativeWithSummons(CActor actor, int initiative)
	{
		CActor actor2 = actor;
		if (actor is CHeroSummonActor { Summoner: not null } cHeroSummonActor)
		{
			actor2 = cHeroSummonActor.Summoner;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor2, CAbility.EAbilityType.AdjustInitiative);
		if (list.Count > 0)
		{
			foreach (CActiveBonus item in list)
			{
				if (item.TryGetBehaviour(out CAdjustInitiativeActiveBonus_FocusInitiative behaviour))
				{
					initiative = behaviour.SetFocusInitiative();
				}
			}
		}
		return initiative;
	}

	public static IEnumerable<CAttackSummary.TargetSummary> CurrentAttackSummaries()
	{
		return CurrentAttackAbility?.CurrentAndPreviousTargetSummaries();
	}

	public static bool IsSelectingCardForMode()
	{
		CAbilityModeTransfer modeAbility;
		return IsSelectingCardForMode(out modeAbility);
	}

	public static bool IsSelectingCardForMode(out CAbilityModeTransfer modeAbility)
	{
		modeAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityModeTransfer;
		if (modeAbility != null)
		{
			if (modeAbility.m_State != CAbilityModeTransfer.EModeTransferState.SelectModeCardToActivate)
			{
				return modeAbility.m_State == CAbilityModeTransfer.EModeTransferState.SelectModeCardToSummonOrDeactivate;
			}
			return true;
		}
		return false;
	}

	public static CBaseCard.ECardPile RecoverCardPile(CActor actor)
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioScripts != null)
		{
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				CBaseCard.ECardPile eCardPile = scenarioScript.OverrideRecoverCardPile(actor);
				if (eCardPile != CBaseCard.ECardPile.None)
				{
					return eCardPile;
				}
			}
		}
		return CBaseCard.ECardPile.Hand;
	}

	public static void AddPlayedCard(int cardId, EActionFlag flags, CAbilityExtraTurn.EExtraTurnType extraTurnType, string actorGuid)
	{
		if (!s_PlayedCards.ContainsKey(actorGuid))
		{
			s_PlayedCards[actorGuid] = new List<Tuple<int, EActionFlag>>[Enum.GetNames(typeof(CAbilityExtraTurn.EExtraTurnType)).Length];
		}
		List<Tuple<int, EActionFlag>>[] array = s_PlayedCards[actorGuid];
		if (array[(int)extraTurnType] == null)
		{
			array[(int)extraTurnType] = new List<Tuple<int, EActionFlag>>();
		}
		array[(int)extraTurnType].Add(new Tuple<int, EActionFlag>(cardId, flags));
	}

	public static List<Tuple<int, EActionFlag>> GetPlayedCards(CAbilityExtraTurn.EExtraTurnType extraTurnType, string actorGuid)
	{
		if (!s_PlayedCards.ContainsKey(actorGuid))
		{
			s_PlayedCards[actorGuid] = new List<Tuple<int, EActionFlag>>[Enum.GetNames(typeof(CAbilityExtraTurn.EExtraTurnType)).Length];
		}
		List<Tuple<int, EActionFlag>>[] array = s_PlayedCards[actorGuid];
		if (array[(int)extraTurnType] == null)
		{
			array[(int)extraTurnType] = new List<Tuple<int, EActionFlag>>();
		}
		return array[(int)extraTurnType];
	}

	public static void ClearPlayedCards(CAbilityExtraTurn.EExtraTurnType extraTurnType, string actorGuid)
	{
		if (s_PlayedCards.TryGetValue(actorGuid, out var value))
		{
			value[(int)extraTurnType]?.Clear();
		}
	}

	public static void ClearPlayedCards(string actorGuid)
	{
		if (s_PlayedCards.TryGetValue(actorGuid, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				value[i]?.Clear();
			}
		}
	}

	public static void ClearPlayedCards()
	{
		foreach (string key in s_PlayedCards.Keys)
		{
			List<Tuple<int, EActionFlag>>[] array = s_PlayedCards[key];
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Clear();
			}
		}
	}

	public static void CleanUpBeforeSave()
	{
		LastTargetSummary = null;
		s_PlayerActorToAvoidDamage = null;
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_CardsBurnedToAvoidDamage?.Clear();
		s_UsedPreventDamageBonusesToAvoidDamage?.Clear();
		s_RecievedDamagedReducedByShieldItems = 0;
		s_RecievedDamagePreventedByActiveBonuses = 0;
		s_ToggledPreventBonuses?.Clear();
		s_ToggledShieldBonuses?.Clear();
		s_ToggledShieldItems?.Clear();
		s_TargetsDamagedInPrevAttackThisTurn = 0;
		s_TargetsDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID = Guid.Empty;
		LastDestroyedObstacle = null;
		List<CActor> list = ScenarioManager.Scenario?.AllActors;
		if (list != null)
		{
			foreach (CActor item in list)
			{
				item.CleanUpBeforeSave();
			}
		}
		SortLists();
	}

	public static void OnAfterLoad()
	{
		SortLists();
	}

	public static CPlayerActor ResolvePlayerAfterLoad(CPlayerActor oldActor)
	{
		if (oldActor == null)
		{
			return null;
		}
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.Single((CPlayerActor x) => x.ActorGuid == oldActor.ActorGuid);
		if (cPlayerActor != oldActor)
		{
			Log.Info($"ResolvePlayerAfterLoad({oldActor}) -> {cPlayerActor}");
		}
		return cPlayerActor;
	}

	public static CActor ResolveActorAfterLoad(CActor oldActor)
	{
		if (oldActor == null)
		{
			return null;
		}
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == oldActor.ActorGuid);
		if (cActor != oldActor)
		{
			Log.Info($"ResolvePlayerAfterLoad({oldActor}) -> {cActor}");
		}
		return cActor;
	}

	private static void SortLists()
	{
		ScenarioManager.Scenario.SortLists();
		ScenarioManager.CurrentScenarioState.Props.Sort(ComparePropsByGUID);
	}

	public static int ComparePropsByGUID(CObjectProp a, CObjectProp b)
	{
		return string.Compare(a.PropGuid, b.PropGuid, StringComparison.Ordinal);
	}

	public static int CompareActorsByGUID(CActor a, CActor b)
	{
		return string.Compare(a.ActorGuid, b.ActorGuid, StringComparison.Ordinal);
	}

	public static int CompareAbilityCardsByID(CAbilityCard a, CAbilityCard b)
	{
		return a.ID.CompareTo(b.ID);
	}

	public static int CompareBaseCardsByID(CBaseCard a, CBaseCard b)
	{
		return a.ID.CompareTo(b.ID);
	}

	public static void StartLoadingClassActiveBonuses()
	{
		IsLoadingClassActiveBonuses = true;
	}

	public static void EndLoadingClassActiveBonuses()
	{
		IsLoadingClassActiveBonuses = false;
	}

	public static void Reset()
	{
		ScenarioManager.Reset();
		CActorStatic.Reset();
		CActor.Reset();
		CGenericActiveBonus.Reset();
		PendingInfusionPhase = null;
		IsLoadingClassActiveBonuses = false;
		HasShortRestWithPreRestAbilities = false;
		CurrentAction = null;
		PendingOnLongRestBonuses = null;
		PendingOnShortRestBonuses = null;
		PendingOnRestAbilities = null;
		PendingActiveBonuses = null;
		PendingStartEndTurnAbilityBonusTriggers = null;
		PendingScenarioModifierAbilities = null;
		PendingCompanionSummonActors = null;
		SkipNextPhase = false;
		CurrentActionInitiator = EActionInitiator.None;
		OutputAdvantageInfo = false;
		DamageDataStack = null;
		LastTargetSummary = null;
		RoundAbilityCardSelected = null;
		RoundAbilityCardActionType = CBaseCard.ActionType.TopAction;
		LastAbilityCardActionType = CBaseCard.ActionType.NA;
		WaitingForMercenarySpecialMechanicSlotChoice = false;
		OverridingCurrentActor = false;
		RedirectedDamageToActor = null;
		s_MathParser = new MathParser();
		s_ActorInitiativeComparer = new ActorInitiativeComparer();
		s_ActorAdjustedInitiativeComparer = new ActorInitiativeComparer(checkforadjustment: true);
		s_InitiativeSortedActors = new List<CActor>();
		PreInitiativeAdjustedPlayerActors = new List<CPlayerActor>();
		s_LastActor = null;
		s_CurrentActor = null;
		s_ShortRestingPlayer = null;
		s_ShortRestCardToBurnID = 0;
		s_TurnActor = null;
		TurnOrderSoFar = new List<(CActor, CClass)>();
		EndRoundReached = false;
		ClearPlayedCards();
		ResetActionFlagsByActionSequenceType(s_ActionFlagsByActionSequenceType);
		ExtraTurnActionSelectionFlagStack = new Stack<EActionFlag>();
		OutOfPhaseActionSelectionFlagStack = new Stack<EActionFlag>();
		CachedExtraTurnInitiatorCardStack = new Stack<CAbilityCard>();
		CurrentExtraTurnAbility = null;
		s_CurrentActionValidAugmentations = new List<CActionAugmentation>();
		s_CurrentActionSelectedAugmentations = new List<CActionAugmentation>();
		s_RoundCount = 0;
		s_EnemyKillTracker = new List<KeyValuePair<CEnemyActor, CActor>>();
		s_DamageInflictedThisTurn = 0;
		s_DamagePreventedThisTurn = 0;
		s_ActorsMovedThisTurn = new List<CActor>();
		s_ActorsKilledThisTurn = new List<CActor>();
		s_ActorsKilledThisRound = new List<CActor>();
		s_TrophyTokensGainedThisRound = 0;
		s_TargetsDamagedInPrevAttackThisTurn = 0;
		s_TargetsDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID = Guid.Empty;
		s_HexesMovedThisTurn = new List<TileIndex>();
		s_ObstaclesDestroyedThisTurn = 0;
		s_HazardousTerrainTilesMovedOverThisTurn = 0;
		s_DifficultTerrainTilesMovedOverThisTurn = 0;
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_CardsBurnedToAvoidDamage = new List<CAbilityCard>();
		s_UsedPreventDamageBonusesToAvoidDamage = new List<CActiveBonus>();
		s_ToggledPreventBonuses = new List<CActiveBonus>();
		s_ToggledShieldBonuses = new List<CActiveBonus>();
		s_ToggledShieldItems = new List<CItem>();
		s_RecievedPlayerActorToAvoidDamageResponse = true;
		s_PlayerActorToAvoidDamage = null;
		s_RecievedSelectedToAvoidDamageResponse = true;
		s_ThreadAboutToAbort = false;
		s_RecievedDamagedReducedByShieldItems = 0;
		s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();
		s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();
		s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();
		s_CheckAdjustInitiativeIndex = 0;
		s_PerksSelectedToUseAfterUnfreezingThread = new List<CAbility>();
		s_AttackModifierCardsSelectedToUseAfterUnfreezingThread = new List<AttackModifierYMLData>();
		CurrentlyChoosingModifiers = new List<AttackModifierYMLData>();
		s_TotalShieldDuringLastAttack = 0;
		s_GenericConfirmButtonToggled = false;
		s_WaitingForGenericConfirmation = false;
		s_RecievedContinueAbilityResponse = false;
		s_ReceivedMultiplayerFinishWaitingResponse = false;
		LastDestroyedObstacle = null;
		CurrentOutOfPhaseCardSelectionAbility = null;
		__OutOfPhaseCardFromPiles.Clear();
		ThreadCalculationScope<CActiveBonus.ActiveBonusThreadData>.ToggleAllScopes(enabled: true);
		Log.GameplayInfo("GAME STATE RESET!");
	}

	public static void Start()
	{
		s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();
		s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();
		s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();
		PendingOnLongRestBonuses = new List<CActiveBonus>();
		PendingOnShortRestBonuses = new List<CActiveBonus>();
		PendingOnRestAbilities = new List<(CAbility, CActor)>();
		PendingActiveBonuses = new List<CActiveBonus>();
		PendingScenarioModifierAbilities = new List<Tuple<CAbility, CActor>>();
		PendingStartEndTurnAbilityBonusTriggers = new List<CActiveBonus>();
		PendingCompanionSummonActors = new List<CPlayerActor>();
		DamageDataStack = new Stack<DamageData>();
		if (s_RoundCount == 0)
		{
			s_RoundCount = 1;
		}
		CheckObjectives();
		CNextRound_MessageData message = new CNextRound_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void Stop()
	{
		PhaseManager.Stop();
	}

	public static void PlayerSelectedAbilityCardAction(CAbilityCard roundAbilityCard, CBaseCard.ActionType actionType)
	{
		_ = (CPlayerActor)s_CurrentActor;
		CurrentActionInitiator = EActionInitiator.AbilityCard;
		CAction cAction = roundAbilityCard.GetActionForType(actionType).Copy();
		roundAbilityCard.ActionHasHappened = false;
		CurrentAction = new CurrentActorAction(cAction, roundAbilityCard);
		roundAbilityCard.SetSelectedAction(cAction);
		RoundAbilityCardSelected = roundAbilityCard;
		RoundAbilityCardActionType = actionType;
		for (int num = s_CurrentActionSelectedAugmentations.Count - 1; num >= 0; num--)
		{
			CActionAugmentation cActionAugmentation = s_CurrentActionSelectedAugmentations[num];
			if (cActionAugmentation.ActionID != CurrentAction.Action.ID)
			{
				foreach (ElementInfusionBoardManager.EElement element in cActionAugmentation.Elements)
				{
					SelectActionAugmentation(cActionAugmentation, element, remove: true);
				}
			}
		}
	}

	public static void ChangeModifierValues(CActor actor, List<AttackModifierYMLData> modifiers, CAbilityAttack attackAbility, CActor currentTarget, EAdvantageStatuses advantageStatus)
	{
		for (int i = 0; i < modifiers.Count; i++)
		{
			AttackModifierYMLData attackModifierYMLData = modifiers[i];
			AttackModifierYMLData attackModifierYMLData2 = (modifiers[i] = attackModifierYMLData.Copy(createCard: true));
			if (ShowAlternateModifierSelection(actor, attackModifierYMLData.Card, attackAbility, onlyInActionPhase: false))
			{
				AttackModifierYMLData attackModifierYMLData4 = (modifiers[i] = attackModifierYMLData2.Copy(createCard: true));
				attackModifierYMLData4.OriginalModifier = attackModifierYMLData2.MathModifier;
				attackModifierYMLData4.MathModifier = attackModifierYMLData2.AlternateMathModifier;
				attackModifierYMLData4.ModifiedFromModifier = attackModifierYMLData2.Card;
				CChangeModifier_MessageData message = new CChangeModifier_MessageData(InternalCurrentActor)
				{
					m_OriginalModifier = attackModifierYMLData4.OriginalModifier
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (attackAbility.MiscAbilityData?.ReplaceModifiers != null && attackAbility.MiscAbilityData.ReplaceWithModifiers != null)
		{
			for (int j = 0; j < modifiers.Count; j++)
			{
				AttackModifierYMLData attackModifierYMLData6 = modifiers[j];
				if (attackAbility.MiscAbilityData.ReplaceModifiers.Contains(attackModifierYMLData6.MathModifier))
				{
					int num = attackAbility.MiscAbilityData.ReplaceModifiers.IndexOf(attackModifierYMLData6.MathModifier);
					if (num >= attackAbility.MiscAbilityData.ReplaceWithModifiers.Count)
					{
						num = 0;
					}
					string replaceWithModifierName = attackAbility.MiscAbilityData.ReplaceWithModifiers[num];
					AttackModifierYMLData attackModifierYMLData7 = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == replaceWithModifierName);
					char operatorSymbol;
					int mathVal;
					if (attackModifierYMLData7 != null)
					{
						AttackModifierYMLData attackModifierYMLData8 = (modifiers[j] = attackModifierYMLData7.Copy(createCard: true));
						attackModifierYMLData8.OriginalModifier = attackModifierYMLData6.MathModifier;
						attackModifierYMLData8.MathModifier = attackModifierYMLData7.MathModifier;
						attackModifierYMLData8.ModifiedFromModifier = attackModifierYMLData7.Card;
						CChangeModifier_MessageData message2 = new CChangeModifier_MessageData(InternalCurrentActor)
						{
							m_OriginalModifier = attackModifierYMLData8.OriginalModifier
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
					else if (AttackModifierYMLData.TryParseMathModifier(replaceWithModifierName, out operatorSymbol, out mathVal))
					{
						AttackModifierYMLData attackModifierYMLData10 = (modifiers[j] = attackModifierYMLData6.Copy(createCard: true));
						attackModifierYMLData10.OriginalModifier = attackModifierYMLData6.MathModifier;
						attackModifierYMLData10.MathModifier = replaceWithModifierName;
						attackModifierYMLData10.ModifiedFromModifier = attackModifierYMLData6.Card;
						CChangeModifier_MessageData message3 = new CChangeModifier_MessageData(InternalCurrentActor)
						{
							m_OriginalModifier = attackModifierYMLData10.OriginalModifier
						};
						ScenarioRuleClient.MessageHandler(message3);
					}
				}
			}
		}
		currentTarget.m_OnDrawModifierListeners?.Invoke(advantageStatus, ref modifiers, s_CurrentActor, currentTarget);
		if (s_CurrentActor is CHeroSummonActor { Summoner: var summoner })
		{
			summoner?.m_OnDrawModifierListeners?.Invoke(advantageStatus, ref modifiers, s_CurrentActor, currentTarget);
		}
		else
		{
			s_CurrentActor.m_OnDrawModifierListeners?.Invoke(advantageStatus, ref modifiers, s_CurrentActor, currentTarget);
		}
		if (modifiers == null || modifiers.Count == 0)
		{
			Log.ErrorWithReport("Attack modifier cards used is empty!");
		}
	}

	public static int DrawAndApplyAttackModifierCards(CAbilityAttack attackAbility, CAbilityAttack originalAttackAbiliy, CActor actor, CAttackSummary.TargetSummary summary, int attackStrength, EAdvantageStatuses advantageStatus, out List<AttackModifierYMLData> outAttackModifierCards, out List<AttackModifierYMLData> outNotUsedAttackModifierCards, out int outPierce, out SEventAttackModifier outAttackModifierEvent, out int outAddTargetsDrawn, int attackIndex, out List<AttackModifierYMLData> outDrawnAttackModifierCardsTotal)
	{
		CActor currentTarget = summary.ActorToAttack;
		summary.State = CAttackSummary.TargetSummary.SummaryState.DrawingModifiers;
		outAttackModifierCards = new List<AttackModifierYMLData>();
		outNotUsedAttackModifierCards = new List<AttackModifierYMLData>();
		outAttackModifierEvent = new SEventAttackModifier();
		outDrawnAttackModifierCardsTotal = new List<AttackModifierYMLData>();
		outPierce = 0;
		outAddTargetsDrawn = 0;
		CActor cActor = actor;
		if (OverridingCurrentActor)
		{
			OverrideActorForActionStackData overrideActorForActionStackData = OverridenActionActorStack.Peek();
			if (overrideActorForActionStackData != null && overrideActorForActionStackData.ControllingActor != null && overrideActorForActionStackData.UseControllingActorModifierDeck)
			{
				cActor = overrideActorForActionStackData.ControllingActor;
			}
		}
		string overrideMathModifier = null;
		if (summary.NoDrawing())
		{
			overrideMathModifier = "+0";
			summary.FinishDrawingModifiers();
		}
		else if (CAbilityAttack.s_AttackModifierCardOverride == null)
		{
			List<AttackModifierYMLData> collection;
			if (cActor.Class is CCharacterClass || cActor.Class is CHeroSummonClass || (cActor.Class is CMonsterClass && cActor is CEnemyActor { IsSummon: not false, PlayerSummoner: not null }))
			{
				CHeroSummonActor cHeroSummonActor = ((cActor is CHeroSummonActor) ? (cActor as CHeroSummonActor) : null);
				CEnemyActor cEnemyActor2 = ((cActor is CEnemyActor) ? (cActor as CEnemyActor) : null);
				CPlayerActor cPlayerActor = null;
				if (cHeroSummonActor != null)
				{
					cPlayerActor = cHeroSummonActor.Summoner;
				}
				else
				{
					cPlayerActor = ((cEnemyActor2 == null) ? ((CPlayerActor)cActor) : cEnemyActor2.PlayerSummoner);
				}
				CCharacterClass characterClass = cPlayerActor.CharacterClass;
				if (characterClass.AttackModifierCards.Count == 0)
				{
					characterClass.CheckAttackModifierCardShuffle(force: true);
				}
				if (!string.IsNullOrWhiteSpace(CAbilityAttack.ConsoleVar_DebugAttackModifierCard))
				{
					AttackModifierYMLData attackModifierYMLData = characterClass.AttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					if (attackModifierYMLData == null)
					{
						attackModifierYMLData = characterClass.DiscardedAttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData != null)
					{
						collection = new List<AttackModifierYMLData> { attackModifierYMLData };
					}
					else
					{
						Log.Error("Invalid debug_attack_modifier_card = '" + CAbilityAttack.ConsoleVar_DebugAttackModifierCard + "'");
						collection = characterClass.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out outNotUsedAttackModifierCards, attackAbility, currentTarget, out outDrawnAttackModifierCardsTotal, summary);
					}
				}
				else
				{
					collection = characterClass.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out outNotUsedAttackModifierCards, attackAbility, currentTarget, out outDrawnAttackModifierCardsTotal, summary);
				}
			}
			else
			{
				if (!(cActor.Class is CMonsterClass))
				{
					Log.ErrorWithReport("Invalid Actor Type");
					return 0;
				}
				CEnemyActor cEnemyActor3 = (CEnemyActor)cActor;
				if (MonsterClassManager.IsDeckEmpty(cEnemyActor3))
				{
					MonsterClassManager.CheckAttackModifierCardShuffle(cEnemyActor3);
				}
				if (!string.IsNullOrWhiteSpace(CAbilityAttack.ConsoleVar_DebugAttackModifierCard))
				{
					AttackModifierYMLData attackModifierYMLData2 = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.Enemy2MonsterAttackModifierDeck.AttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.Enemy2MonsterAttackModifierDeck.DiscardedAttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.AlliedMonsterAttackModifierDeck.AttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.AlliedMonsterAttackModifierDeck.DiscardedAttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.NeutralMonsterAttackModifierDeck.AttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 == null)
					{
						attackModifierYMLData2 = MonsterClassManager.NeutralMonsterAttackModifierDeck.DiscardedAttackModifierCards.FirstOrDefault((AttackModifierYMLData c) => c.Name == CAbilityAttack.ConsoleVar_DebugAttackModifierCard);
					}
					if (attackModifierYMLData2 != null)
					{
						collection = new List<AttackModifierYMLData> { attackModifierYMLData2 };
					}
					else
					{
						Log.Error("Invalid debug_attack_modifier_card = '" + CAbilityAttack.ConsoleVar_DebugAttackModifierCard + "'");
						collection = MonsterClassManager.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out outNotUsedAttackModifierCards, attackAbility, currentTarget, out outDrawnAttackModifierCardsTotal, summary);
					}
				}
				else
				{
					collection = MonsterClassManager.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out outNotUsedAttackModifierCards, attackAbility, currentTarget, out outDrawnAttackModifierCardsTotal, summary);
				}
			}
			outAttackModifierCards.AddRange(collection);
			if (outAttackModifierCards == null || outAttackModifierCards.Count == 0)
			{
				Log.ErrorWithReport("Attack modifier cards used is empty!");
			}
		}
		else
		{
			overrideMathModifier = CAbilityAttack.s_AttackModifierCardOverride ?? "";
			CAbilityAttack.s_AttackModifierCardOverride = null;
		}
		summary.OverrideMathModifier = overrideMathModifier;
		int strength = attackAbility.Strength;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		List<CItem.EItemSlotState> list = new List<CItem.EItemSlotState>();
		List<CCondition.EPositiveCondition> list2 = new List<CCondition.EPositiveCondition>();
		List<CCondition.ENegativeCondition> list3 = new List<CCondition.ENegativeCondition>();
		List<ElementInfusionBoardManager.EElement> list4 = new List<ElementInfusionBoardManager.EElement>();
		foreach (AttackModifierYMLData outAttackModifierCard in outAttackModifierCards)
		{
			outAddTargetsDrawn += (outAttackModifierCard.AddTarget ? 1 : 0);
			num += (outAttackModifierCard.Card.HealAlly ? 1 : 0);
			num2 += outAttackModifierCard.Card.HealAmount;
			num3 += outAttackModifierCard.Card.DamageAmount;
			foreach (CAbility ability2 in outAttackModifierCard.Abilities)
			{
				if (ability2 is CAbilityPush cAbilityPush)
				{
					num4 += cAbilityPush.Strength;
				}
				else if (ability2 is CAbilityPull cAbilityPull)
				{
					num5 += cAbilityPull.Strength;
				}
				else if (ability2 is CAbilityShield cAbilityShield)
				{
					num6 += cAbilityShield.Strength;
				}
				else if (ability2 is CAbilityRefreshItemCards cAbilityRefreshItemCards)
				{
					num7 += cAbilityRefreshItemCards.Strength;
					foreach (CItem.EItemSlotState item in cAbilityRefreshItemCards.SlotStatesToRefresh)
					{
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
				else
				{
					if (!(ability2 is CAbilityInfuse cAbilityInfuse))
					{
						continue;
					}
					foreach (ElementInfusionBoardManager.EElement item2 in cAbilityInfuse.ElementsToInfuse)
					{
						if (!list4.Contains(item2))
						{
							list4.Add(item2);
						}
					}
				}
			}
			foreach (CAbilityOverride @override in outAttackModifierCard.Overrides)
			{
				if (@override.Pierce.HasValue)
				{
					outPierce += @override.Pierce.Value;
				}
				if (@override.PositiveConditions != null)
				{
					foreach (CCondition.EPositiveCondition positiveCondition in @override.PositiveConditions)
					{
						if (!list2.Contains(positiveCondition) || positiveCondition == CCondition.EPositiveCondition.Bless)
						{
							list2.Add(positiveCondition);
						}
					}
				}
				if (@override.NegativeConditions == null)
				{
					continue;
				}
				foreach (CCondition.ENegativeCondition negativeCondition in @override.NegativeConditions)
				{
					if (!list3.Contains(negativeCondition) || negativeCondition == CCondition.ENegativeCondition.Curse)
					{
						list3.Add(negativeCondition);
					}
				}
			}
		}
		summary.StrengthFromModifierOverrides = 0;
		foreach (AttackModifierYMLData outAttackModifierCard2 in outAttackModifierCards)
		{
			if (outAttackModifierCard2.MathModifier != outAttackModifierCard2.AlternateMathModifier && !string.IsNullOrEmpty(outAttackModifierCard2.AlternateMathModifier))
			{
				continue;
			}
			CActor targetingActor = actor;
			if ((outAttackModifierCard2.ControlTargetingActor || outAttackModifierCard2.TryToPerformAsSummoner) && actor is CHeroSummonActor { Summoner: not null } cHeroSummonActor2)
			{
				targetingActor = cHeroSummonActor2.Summoner;
			}
			else if (outAttackModifierCard2.TryToPerformAsSummoner && actor is CEnemyActor { PlayerSummoner: not null } cEnemyActor4)
			{
				targetingActor = cEnemyActor4.PlayerSummoner;
			}
			foreach (CAbility ability3 in outAttackModifierCard2.Abilities)
			{
				CAbility ability = CAbility.CopyAbility(ability3, generateNewID: false, fullCopy: true);
				attackAbility.ModifierAbilities.Add(new CAbilityAttack.AttackModifierAbilityInfo
				{
					Ability = ability,
					TargetingActor = targetingActor,
					ControlTargetingActor = outAttackModifierCard2.ControlTargetingActor,
					Card = outAttackModifierCard2
				});
			}
			ApplyModifierToAttack(outAttackModifierCard2, summary);
		}
		attackAbility.CachedModifierAbilities = attackAbility.ModifierAbilities?.ToList();
		int bonusStrengthFromAttackModifiers = attackAbility.Strength - strength;
		int bonusStrengthOutsideOfModifiers = 0;
		foreach (AttackModifierYMLData outAttackModifierCard3 in outAttackModifierCards)
		{
			if (outAttackModifierCard3.MathModifier.Contains("*") || outAttackModifierCard3.MathModifier.Contains("/"))
			{
				summary.PreShieldScalarModifiers.Add(outAttackModifierCard3.MathModifier);
			}
			else
			{
				summary.NonScalarMathModifiers.Add(outAttackModifierCard3.MathModifier);
			}
		}
		int num8 = summary.CalcExpressionString(calcPostShieldScalars: false, calcShield: false);
		List<SEventAttackModifier.ModifierInfo> list5 = new List<SEventAttackModifier.ModifierInfo>();
		foreach (AttackModifierYMLData outAttackModifierCard4 in outAttackModifierCards)
		{
			list5.Add(new SEventAttackModifier.ModifierInfo
			{
				ModifierWithCustomLocalization = ((outAttackModifierCard4.ModifiedFromModifier != null) ? outAttackModifierCard4.ModifiedFromModifier.AttackModifierLogString() : outAttackModifierCard4.Card.AttackModifierLogString(outAttackModifierCard4.OriginalModifier)),
				ModifierWithNoLocalization = ((outAttackModifierCard4.ModifiedFromModifier != null) ? outAttackModifierCard4.ModifiedFromModifier.AttackModifierLogString(null, skipLoc: true) : outAttackModifierCard4.Card.AttackModifierLogString(outAttackModifierCard4.OriginalModifier, skipLoc: true)),
				ModifierValue = outAttackModifierCard4.MathModifier,
				ModifierChangedToValue = ((outAttackModifierCard4.ModifiedFromModifier != null || (outAttackModifierCard4.MathModifier != outAttackModifierCard4.OriginalModifier && outAttackModifierCard4.OriginalModifier != null)) ? outAttackModifierCard4.Card.AttackModifierLogString(outAttackModifierCard4.MathModifier, skipLoc: true) : "")
			});
		}
		List<SEventAttackModifier.ModifierInfo> list6 = new List<SEventAttackModifier.ModifierInfo>();
		foreach (AttackModifierYMLData outNotUsedAttackModifierCard in outNotUsedAttackModifierCards)
		{
			list6.Add(new SEventAttackModifier.ModifierInfo
			{
				ModifierWithCustomLocalization = ((outNotUsedAttackModifierCard.ModifiedFromModifier != null) ? outNotUsedAttackModifierCard.ModifiedFromModifier.AttackModifierLogString() : outNotUsedAttackModifierCard.Card.AttackModifierLogString(outNotUsedAttackModifierCard.OriginalModifier)),
				ModifierWithNoLocalization = ((outNotUsedAttackModifierCard.ModifiedFromModifier != null) ? outNotUsedAttackModifierCard.ModifiedFromModifier.AttackModifierLogString(null, skipLoc: true) : outNotUsedAttackModifierCard.Card.AttackModifierLogString(outNotUsedAttackModifierCard.OriginalModifier, skipLoc: true)),
				ModifierValue = outNotUsedAttackModifierCard.MathModifier,
				ModifierChangedToValue = ((outNotUsedAttackModifierCard.ModifiedFromModifier != null || (outNotUsedAttackModifierCard.MathModifier != outNotUsedAttackModifierCard.OriginalModifier && outNotUsedAttackModifierCard.OriginalModifier != null)) ? outNotUsedAttackModifierCard.Card.AttackModifierLogString(outNotUsedAttackModifierCard.MathModifier, skipLoc: true) : "")
			});
		}
		bool mainActorIsEnemySummon = false;
		if (actor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor5 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
			if (cEnemyActor5 != null)
			{
				mainActorIsEnemySummon = cEnemyActor5.IsSummon;
			}
		}
		bool secondaryActorIsSummon = false;
		if (currentTarget.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor6 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == currentTarget.ActorGuid);
			if (cEnemyActor6 != null)
			{
				secondaryActorIsSummon = cEnemyActor6.IsSummon;
			}
		}
		outAttackModifierEvent = new SEventAttackModifier(attackStrength, advantageStatus, list5, list6, currentTarget.ID, num8, bonusStrengthFromAttackModifiers, bonusStrengthOutsideOfModifiers, num2, num6, outPierce, list3, list2, list4, actor.Class.ID, actor.Type, mainActorIsEnemySummon, actor.Tokens.CheckPositiveTokens.ToList(), actor.Tokens.CheckNegativeTokens.ToList(), currentTarget.Class.ID, currentTarget.Type, secondaryActorIsSummon, currentTarget.Tokens.CheckPositiveTokens.ToList(), currentTarget.Tokens.CheckNegativeTokens.ToList(), "", attackIndex);
		return num8;
	}

	public static void ApplyModifierToAttack(AttackModifierYMLData attackModifierCard, CAttackSummary.TargetSummary summary)
	{
		CAbility attackAbilityWithOverrides = summary.AttackAbilityWithOverrides;
		CActor actorToAttack = summary.ActorToAttack;
		int strength = attackAbilityWithOverrides.Strength;
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = attackAbilityWithOverrides.StatIsBasedOnXEntries.ToList();
		foreach (CAbilityOverride @override in attackModifierCard.Overrides)
		{
			attackAbilityWithOverrides.OverrideAbilityValues(@override, perform: false, null, null, actorToAttack, fromModifier: true);
		}
		int num = attackAbilityWithOverrides.Strength - strength;
		if (num != 0)
		{
			attackModifierCard.OriginalModifier = attackModifierCard.MathModifier;
			int num2 = Convert.ToInt32(s_MathParser.Parse(attackModifierCard.MathModifier, isRadians: false)) + num;
			attackModifierCard.MathModifier = string.Format("{0}{1}", (num2 >= 0) ? "+" : "-", num2);
		}
		attackAbilityWithOverrides.StatIsBasedOnXEntries = statIsBasedOnXEntries;
		attackAbilityWithOverrides.Strength = strength;
	}

	public static DummyReplicateDamageState ActorBeenDamaged(CActor actor, int strength, bool checkIfPlayerCanAvoidDamage = false, CActor damageSource = null, CAbility damageAbility = null, CAbility.EAbilityType damageAbilityType = CAbility.EAbilityType.None, int shield = 0, bool isTrapDamage = false, bool isTerrainDamage = false, bool cannotPreventDamageWithActiveBonus = false, bool pierceInvulnerable = false, int delay = 0, bool triggerOnDamageListeners = true, int minHealth = 0, bool skipHitAnimation = false)
	{
		bool triggeredPreventDamage;
		DamageData preventedDamageData;
		return ActorBeenDamaged(actor, strength, out triggeredPreventDamage, out preventedDamageData, checkIfPlayerCanAvoidDamage, damageSource, damageAbility, damageAbilityType, shield, isTrapDamage, isTerrainDamage, cannotPreventDamageWithActiveBonus, pierceInvulnerable, delay, triggerOnDamageListeners, minHealth, skipHitAnimation);
	}

	public static int CalcWardBrittle(int strength, CAttackSummary.TargetSummary targetSummary, bool apply = false, bool skipHitAnimation = true)
	{
		CActor actorToAttack = targetSummary.ActorToAttack;
		if (strength > 0)
		{
			if (targetSummary.Brittle)
			{
				strength *= 2;
				if (apply)
				{
					actorToAttack.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Brittle, EConditionDecTrigger.None, targetSummary.OriginalAbility?.TargetingActor);
					CBrittleRemoved_MessageData cBrittleRemoved_MessageData = new CBrittleRemoved_MessageData(actorToAttack, actorToAttack.Health);
					cBrittleRemoved_MessageData.m_skipHitAnimation = skipHitAnimation;
					ScenarioRuleClient.MessageHandler(cBrittleRemoved_MessageData);
				}
			}
			if (targetSummary.Ward)
			{
				int num = strength;
				strength /= 2;
				if ((num > 0) & apply)
				{
					actorToAttack.RemovePositiveConditionToken(CCondition.EPositiveCondition.Ward);
					CWardTriggered_MessageData message = new CWardTriggered_MessageData("", null)
					{
						m_WardedActor = actorToAttack,
						m_ActorWasAsleep = actorToAttack.Tokens.HasKey(CCondition.ENegativeCondition.Sleep)
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		else if (apply)
		{
			targetSummary.Brittle = false;
			targetSummary.Ward = false;
		}
		return strength;
	}

	public static DummyReplicateDamageState ActorBeenDamaged(CActor actor, int strength, out bool triggeredPreventDamage, out DamageData preventedDamageData, bool checkIfPlayerCanAvoidDamage = false, CActor damageSource = null, CAbility damageAbility = null, CAbility.EAbilityType damageAbilityType = CAbility.EAbilityType.None, int shield = 0, bool isTrapDamage = false, bool isTerrainDamage = false, bool cannotPreventDamageWithActiveBonus = false, bool pierceInvulnerable = false, int delay = 0, bool triggerOnDamageListeners = true, int minHealth = 0, bool skipHitAnimation = false)
	{
		triggeredPreventDamage = false;
		preventedDamageData = null;
		if (delay > 0)
		{
			ThreadIsSleeping = true;
			Thread.Sleep(delay);
			ThreadIsSleeping = false;
		}
		if (damageSource == null)
		{
			damageSource = actor;
		}
		if (damageAbility is CAbilityAttack cAbilityAttack)
		{
			LastTargetSummary = cAbilityAttack.CurrentAttackingTargetSummary;
			UpdateTargetsDamagedInPrevAttackThisTurn(cAbilityAttack);
		}
		else if (damageAbility is CAbilityDamage cAbilityDamage)
		{
			LastTargetSummary = cAbilityDamage.CurrentDamagingTargetSummary;
			UpdateTargetsDamagedInPrevDamageAbilityThisTurn(cAbilityDamage);
		}
		else
		{
			LastTargetSummary = new CAttackSummary.TargetSummary(strength, actor, damageSource);
		}
		if (strength < 0)
		{
			CLogDamageCalculation_MessageData message = new CLogDamageCalculation_MessageData(LastTargetSummary.ActorToAttack)
			{
				m_Ability = LastTargetSummary.OriginalAbility,
				m_ActorBeingAttacked = LastTargetSummary.ActorToAttack,
				m_AttackingActor = LastTargetSummary.DamageSourceActor,
				m_DamageEvent = (LastTargetSummary.OriginalAbility as CAbilityDamage)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
				m_AttackEvent = (LastTargetSummary.OriginalAbility as CAbilityAttack)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
				m_ModifierEvent = LastTargetSummary.AttackModifierEvent
			};
			ScenarioRuleClient.MessageHandler(message);
			return null;
		}
		if (!pierceInvulnerable && (damageSource == null || !damageSource.PierceInvulnerability) && actor.Invulnerable)
		{
			return null;
		}
		List<CActiveBonus> list = (from bonus in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate)
			where bonus.RequirementsMet(damageSource)
			select bonus).ToList();
		if (damageAbilityType == CAbility.EAbilityType.Retaliate && list.Any())
		{
			list[0].TriggerPreventDamage(strength, damageSource, actor, damageAbility);
			return null;
		}
		List<CActiveBonus> list2 = (from bonus in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.TransferDamage)
			where bonus.RequirementsMet(damageSource)
			select bonus).ToList();
		if (list2.Any() && list2[0].TriggerTransferDamage(strength, damageSource, actor, damageAbility))
		{
			return null;
		}
		List<PositiveConditionPair> checkPositiveTokens = actor.Tokens.CheckPositiveTokens;
		CActor.ECauseOfDamage eCauseOfDamage = CActor.ECauseOfDamage.None;
		eCauseOfDamage = (isTrapDamage ? CActor.ECauseOfDamage.Trap : (isTerrainDamage ? CActor.ECauseOfDamage.HazardousTerrain : (damageAbilityType switch
		{
			CAbility.EAbilityType.Attack => (actor != damageSource) ? ((actor.Type != damageSource.Type) ? CActor.ECauseOfDamage.EnemyAttack : CActor.ECauseOfDamage.Ally) : CActor.ECauseOfDamage.Self, 
			CAbility.EAbilityType.Wound => CActor.ECauseOfDamage.Wound, 
			_ => (actor != damageSource) ? ((actor.Type != damageSource?.Type) ? CActor.ECauseOfDamage.EnemyDamage : CActor.ECauseOfDamage.Ally) : CActor.ECauseOfDamage.Self, 
		})));
		strength = LastTargetSummary.CalcExpressionString(calcPostShieldScalars: true, calcShield: true);
		if (damageAbility != null)
		{
			damageAbility.DamageInflictedByAbility += strength;
			damageAbility.DamageInflictedByAbilityOnLastTarget = strength;
		}
		if (eCauseOfDamage == CActor.ECauseOfDamage.Self && actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Any((CActiveBonus it) => (it.Ability as CAbilityImmunityTo).ImmuneToSelfDamage))
		{
			strength = 0;
		}
		if (minHealth != 0)
		{
			strength = Math.Min(strength, actor.Health - minHealth);
		}
		int health = actor.Health;
		actor.Damaged(strength, damageAbilityType == CAbility.EAbilityType.Attack, damageSource, damageAbility, skipHitAnimation);
		if (strength > 0 && eCauseOfDamage != CActor.ECauseOfDamage.Self && eCauseOfDamage != CActor.ECauseOfDamage.None && actor is CEnemyActor)
		{
			GlobalStatRepository.Instance.MaximumDamageDealt.NewValue(strength);
			ScenarioStatRepository.Instance.TotalDamageDealt.Increment(strength);
			if (damageSource is CPlayerActor cPlayerActor)
			{
				CampaignStatRepository.Instance.DamageDealtByCharacters.Increment(cPlayerActor.CharacterName, strength);
			}
		}
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		if (actorState != null)
		{
			foreach (CObjective item in ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective o) => o.ObjectiveType == EObjectiveType.DealXDamage))
			{
				(item as CObjective_DealXDamage).SetActorDamaged(actorState, strength);
			}
			foreach (CObjective item2 in ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective o) => o.ObjectiveType == EObjectiveType.DamageQuota))
			{
				(item2 as CObjective_DamageQuota).SetActorDamaged(actorState, damageSource, strength);
			}
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		bool hadRegeneration = actor.Tokens.HasKey(CCondition.EPositiveCondition.Regenerate);
		DamageDataStack.Push(new DamageData(actor, strength, shield, health, damageSource, damageAbility, damageAbilityType, eCauseOfDamage, isTrapDamage, isTerrainDamage, checkIfPlayerCanAvoidDamage, cannotPreventDamageWithActiveBonus, triggerOnDamageListeners, hadRegeneration, checkPositiveTokens, LastTargetSummary.CalcExpressionString(calcPostShieldScalars: false, calcShield: true)));
		CActiveBonus.RefreshAllAuraActiveBonuses();
		ContinueActorDamagedAfterSelectingPlayerActorToBurnCards(out triggeredPreventDamage, out preventedDamageData, cannotPreventDamageWithActiveBonus, skipHitAnimation);
		return null;
	}

	private static void ChooseWhoWillTakeDamage(CActor actor, bool cannotPreventDamageWithActiveBonus, int strength)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (!cannotPreventDamageWithActiveBonus)
		{
			list = (from x in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.PreventDamage)
				where !x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate)
				select x).ToList();
		}
		bool flag = list.Any((CActiveBonus x) => x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards) && x.RequirementsMet(actor));
		bool flag2 = ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor it) => it.CharacterClass.HandAbilityCards.Count > 0 || it.CharacterClass.DiscardedAbilityCards.Count > 1);
		s_PlayerActorToAvoidDamage = null;
		if (actor is CPlayerActor cPlayerActor)
		{
			s_PlayerActorToAvoidDamage = cPlayerActor;
		}
		else
		{
			if ((!(actor is CPlayerActor) & flag & flag2) && strength > 0)
			{
				s_RecievedPlayerActorToAvoidDamageResponse = false;
				CChoosePlayerActorToBurnCardToPreventDamage_MessageData message = new CChoosePlayerActorToBurnCardToPreventDamage_MessageData(LastTargetSummary);
				ScenarioRuleClient.MessageHandler(message);
				if (!ScenarioRuleClient.IsWorkerThread)
				{
					throw new InvalidOperationException($"Trying to apply damage on the Main thread will cause endless freeze. ABORT! {actor}");
				}
				bool flag3 = false;
				ThreadIsSleeping = true;
				ThreadSleepReason = "SelectingToNegateDamageForAlly";
				while (!s_RecievedPlayerActorToAvoidDamageResponse && !s_ThreadAboutToAbort)
				{
					if (!flag3)
					{
						flag3 = true;
						if (ScenarioRuleClient.IsWorkerThread)
						{
							Log.Trace("Sleeping SRL Thread");
						}
					}
					Thread.Sleep(100);
				}
				ThreadIsSleeping = false;
				ThreadSleepReason = null;
				if (s_ThreadAboutToAbort)
				{
					s_RecievedPlayerActorToAvoidDamageResponse = true;
					s_PlayerActorToAvoidDamage = null;
					s_ThreadAboutToAbort = false;
					return;
				}
			}
			if (s_PlayerActorToAvoidDamage == null && actor is CHeroSummonActor cHeroSummonActor && list.Count > 0)
			{
				s_PlayerActorToAvoidDamage = cHeroSummonActor.Summoner;
			}
		}
		CPlayerFinishedBurningCardForAlly_MessageData message2 = new CPlayerFinishedBurningCardForAlly_MessageData(CurrentDamageData.ActorDamaged)
		{
			m_ActorBeingAttacked = CurrentDamageData.ActorDamaged
		};
		ScenarioRuleClient.MessageHandler(message2);
	}

	public static void ContinueActorDamagedAfterSelectingPlayerActorToBurnCards(out bool triggeredPreventDamage, out DamageData preventedDamageData, bool cannotPreventDamageWithActiveBonus, bool skipHitAnimation = false)
	{
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_CardsBurnedToAvoidDamage.Clear();
		s_UsedPreventDamageBonusesToAvoidDamage?.Clear();
		s_ToggledPreventBonuses.Clear();
		s_ToggledShieldBonuses.Clear();
		s_ToggledShieldItems.Clear();
		triggeredPreventDamage = false;
		preventedDamageData = null;
		bool flag = CurrentDamageData.DamageSourceStrengthBeforeWardBrittle > 0;
		bool flag2 = CurrentDamageData.ActorDamaged.Health < CurrentDamageData.PreDamageHealth;
		bool flag3 = CurrentDamageData.DamageSourceAbilityType == CAbility.EAbilityType.Retaliate;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(CurrentDamageData.ActorDamaged, CAbility.EAbilityType.Shield);
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(list.FindAll((CActiveBonus it) => it is CShieldActiveBonus && it.Ability.ActiveBonusData.IsToggleBonus && (!it.Ability.IsItemAbility || !it.Ability.ActiveBonusData.AuraFilter.HasTargetTypeFilters() || it.Ability.ActiveBonusData.AuraFilter.IsValidTarget(CurrentDamageData.ActorDamaged, it.Actor, isTargetedAbility: false, useTargetOriginalType: false, false)) && !it.IsRestricted(CurrentDamageData.ActorDamaged) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(CurrentDamageData.DamageSourceAbilityType)) && it.Ability.ActiveBonusData.Consuming.Count <= 0));
		bool flag4 = list2.Count > 0 && !CurrentDamageData.CannotPreventDamageWithActiveBonus;
		List<CActiveBonus> source = list.FindAll((CActiveBonus x) => x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.ShieldIncomingAttacks));
		if (CurrentDamageData.DamageSourceAbility != null && CurrentDamageData.DamageSourceAbility.GetAbilityBaseCard() != null)
		{
			_ = CurrentDamageData.DamageSourceAbility.GetAbilityBaseCard() is CScenarioModifierTriggerAbility;
		}
		else
			_ = 0;
		CAttackSummary.TargetSummary lastTargetSummary = LastTargetSummary;
		bool flag5 = false;
		if (flag)
		{
			bool flag6 = false;
			CPlayerActor playerActorToUseItems = CurrentDamageData.ActorDamaged as CPlayerActor;
			if (playerActorToUseItems?.Inventory != null)
			{
				flag6 = playerActorToUseItems.Inventory.AllItemsAndPets.Any((CItem item) => (item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttacked) || item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttackedIgnoringDamageTaken)) && playerActorToUseItems.Inventory.IsItemUsable(item) && item.YMLData.Data.CompareAbility != null && CurrentDamageData.DamageSourceAbility != null && item.YMLData.Data.CompareAbility.CompareAbility(CurrentDamageData.DamageSourceAbility, CurrentDamageData.ActorDamaged));
			}
			if (!CurrentDamageData.IsDirectDamage && (flag4 | flag6))
			{
				flag5 = true;
				s_RecievedSelectedToAvoidDamageResponse = false;
				CPlayerSelectingToToggleShields_MessageData message = new CPlayerSelectingToToggleShields_MessageData(CurrentDamageData.ActorDamaged)
				{
					m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
					m_ActorToShowCardsFor = playerActorToUseItems,
					m_ModifiedStrength = CurrentDamageData.DamageSourceStrength,
					m_TargetSummary = lastTargetSummary,
					m_DamagingAbility = CurrentDamageData.DamageSourceAbility,
					m_DamageType = CurrentDamageData.DamageSourceAbilityType
				};
				ScenarioRuleClient.MessageHandler(message);
				CLogDamageCalculation_MessageData message2 = new CLogDamageCalculation_MessageData(CurrentDamageData.ActorDamaged)
				{
					m_Ability = CurrentDamageData.DamageSourceAbility,
					m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
					m_AttackingActor = CurrentDamageData.DamageSourceActor,
					m_DamageEvent = (CurrentDamageData.DamageSourceAbility as CAbilityDamage)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
					m_AttackEvent = (CurrentDamageData.DamageSourceAbility as CAbilityAttack)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
					m_ModifierEvent = lastTargetSummary.AttackModifierEvent,
					m_OverrideLogKey = "COMBAT_LOG_DAMAGE_WHILE_SELECTING_SHIELDS"
				};
				ScenarioRuleClient.MessageHandler(message2);
				bool flag7 = false;
				ThreadIsSleeping = true;
				while (!s_RecievedSelectedToAvoidDamageResponse && !s_ThreadAboutToAbort)
				{
					if (!flag7)
					{
						flag7 = true;
						if (ScenarioRuleClient.IsWorkerThread)
						{
							Log.Trace("Sleeping SRL Thread");
						}
					}
					Thread.Sleep(100);
				}
				ThreadIsSleeping = false;
				if (s_ThreadAboutToAbort)
				{
					s_RecievedSelectedToAvoidDamageResponse = true;
					s_ThreadAboutToAbort = false;
					Log.SimpleInfo("Abort First ContinueActorDamagedAfterSelectingPlayerActorToBurnCards");
					return;
				}
			}
			HandleFinishedTogglingShield(lastTargetSummary, apply: true, skipHitAnimation);
			flag = CurrentDamageData.DamageSourceStrength > 0;
			CPlayerFinishedTogglingShields_MessageData message3 = new CPlayerFinishedTogglingShields_MessageData(CurrentDamageData.ActorDamaged)
			{
				m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
				m_ToggledShields = s_ToggledShieldItems.ToList(),
				m_ToggledBonuses = s_ToggledShieldBonuses.ToList()
			};
			ScenarioRuleClient.MessageHandler(message3);
			foreach (CActiveBonus item in s_ToggledShieldBonuses.ToList())
			{
				if (item.IsActiveBonusToggledAndNotRestricted(CurrentDamageData.ActorDamaged))
				{
					preventedDamageData = CurrentDamageData;
				}
				item.TriggerPreventDamage(s_RecievedDamagePreventedByActiveBonuses, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
			}
		}
		CLogDamageCalculation_MessageData message4 = new CLogDamageCalculation_MessageData(CurrentDamageData.ActorDamaged)
		{
			m_Ability = CurrentDamageData.DamageSourceAbility,
			m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
			m_AttackingActor = CurrentDamageData.DamageSourceActor,
			m_DamageEvent = (CurrentDamageData.DamageSourceAbility as CAbilityDamage)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
			m_AttackEvent = (CurrentDamageData.DamageSourceAbility as CAbilityAttack)?.GetLogEvent(ESESubTypeAbility.AbilityPerform),
			m_ModifierEvent = lastTargetSummary.AttackModifierEvent
		};
		ScenarioRuleClient.MessageHandler(message4);
		ChooseWhoWillTakeDamage(CurrentDamageData.ActorDamaged, cannotPreventDamageWithActiveBonus, CurrentDamageData.DamageSourceStrengthBeforeWardBrittle);
		CPlayerActor cPlayerActor = s_PlayerActorToAvoidDamage;
		bool flag8 = s_PlayerSelectedToAvoidDamage == EAvoidDamageOption.Lose1HandCard || s_PlayerSelectedToAvoidDamage == EAvoidDamageOption.Lose2DiscardCards;
		bool flag9 = cPlayerActor?.IsDead ?? false;
		bool flag10 = cPlayerActor != null && cPlayerActor.CharacterClass.HandAbilityCards.Count > 0;
		bool flag11 = cPlayerActor != null && cPlayerActor.CharacterClass.DiscardedAbilityCards.Count > 1;
		if (s_AutoTakeDamage && (cPlayerActor == null || cPlayerActor == CurrentDamageData.ActorDamaged))
		{
			flag10 = false;
			flag11 = false;
		}
		List<CActiveBonus> list3 = CActiveBonus.FindApplicableActiveBonuses(CurrentDamageData.ActorDamaged, CAbility.EAbilityType.PreventDamage);
		List<CActiveBonus> list4 = new List<CActiveBonus>();
		if (!flag3)
		{
			list3.RemoveAll((CActiveBonus x) => x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventBySettingRetaliateToValue));
		}
		list4.AddRange(list3.FindAll((CActiveBonus it) => it is CPreventDamageActiveBonus cPreventDamageActiveBonus && it.RequirementsMet(CurrentDamageData.ActorDamaged) && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(CurrentDamageData.ActorDamaged) && (!cPreventDamageActiveBonus.PreventOnlyIfLethal || (cPreventDamageActiveBonus.PreventOnlyIfLethal && CurrentDamageData.DamageSourceStrength >= CurrentDamageData.PreDamageHealth)) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(CurrentDamageData.DamageSourceAbilityType)) && it.Ability.ActiveBonusData.Consuming.Count <= 0));
		bool flag12 = list4.Count > 0 && !CurrentDamageData.CannotPreventDamageWithActiveBonus;
		if (!flag3)
		{
			list3.RemoveAll((CActiveBonus x) => x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventBySettingRetaliateToValue));
		}
		List<CActiveBonus> list5 = list3.FindAll((CActiveBonus x) => x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards));
		List<CActiveBonus> list6 = list3.Where((CActiveBonus x) => x.IsAura && x.Caster != CurrentDamageData.ActorDamaged && CActor.AreActorsAllied(CurrentDamageData.ActorDamaged.Type, CActor.EType.Player, CurrentDamageData.ActorDamaged, x.Caster)).ToList();
		List<CActiveBonus> list7 = list6.Where((CActiveBonus x) => x.Ability.ActiveBonusData.ToggleIsOptional).ToList();
		bool flag13 = (cPlayerActor != null && cPlayerActor.OriginalType == CActor.EType.Player) || CurrentDamageData.ActorDamaged.OriginalType == CActor.EType.Player || list7.Count > 0 || (CActor.AreActorsAllied(CurrentDamageData.ActorDamaged.OriginalType, CActor.EType.Player, CurrentDamageData.ActorDamaged, null) && (list5.Count() > 0 || (source.Count() > 0 && !CurrentDamageData.IsDirectDamage)));
		if (cPlayerActor == null && list6.Count > 0 && CurrentDamageData.ActorDamaged is CEnemyActor && list7.Count > 0)
		{
			cPlayerActor = list7.Select((CActiveBonus x) => x.Caster as CPlayerActor).FirstOrDefault((CPlayerActor x) => x != null);
		}
		if (flag8 || ((((flag10 | flag11 | flag12) || ScenarioScript_FH100.IsActive) & flag & flag2) && !flag9 && (flag13 || ((CurrentDamageData.ActorDamaged is CHeroSummonActor) & flag12)) && CurrentDamageData.CheckIfPlayerCanAvoidDamageByBurningCards))
		{
			if (!flag8)
			{
				CurrentDamageData.ActorDamaged.Inventory.HighlightUsableItems((CAbility)null, CurrentDamageData.ActorDamaged, new CItem.EItemTrigger[2]
				{
					CItem.EItemTrigger.OnAttacked,
					CItem.EItemTrigger.OnAttackedIgnoringDamageTaken
				});
				bool allowBurnCards = CurrentDamageData.CheckIfPlayerCanAvoidDamageByBurningCards && (CurrentDamageData.ActorDamaged is CPlayerActor || list5.Count() > 0 || cPlayerActor?.GetMirrorHealthActor() == CurrentDamageData.ActorDamaged);
				s_RecievedSelectedToAvoidDamageResponse = false;
				CPlayerSelectingToAvoidDamageOrNot_MessageData message5 = new CPlayerSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged)
				{
					m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
					m_ActorToShowCardsFor = cPlayerActor,
					m_ModifiedStrength = CurrentDamageData.DamageSourceStrength,
					m_IsDirectDamage = CurrentDamageData.IsDirectDamage,
					m_TargetSummary = LastTargetSummary,
					m_DamagingAbility = CurrentDamageData.DamageSourceAbility,
					m_AllowBurnCards = allowBurnCards,
					m_DamageType = CurrentDamageData.DamageSourceAbilityType
				};
				ScenarioRuleClient.MessageHandler(message5);
				if (!ScenarioRuleClient.IsWorkerThread)
				{
					throw new InvalidOperationException($"Trying to burn cards on the Main thread will cause endless freeze. ABORT! {CurrentDamageData.ActorDamaged}");
				}
				bool flag14 = false;
				ThreadIsSleeping = true;
				while (!s_RecievedSelectedToAvoidDamageResponse && !s_ThreadAboutToAbort)
				{
					if (!flag14)
					{
						flag14 = true;
						if (ScenarioRuleClient.IsWorkerThread)
						{
							Log.Trace("Sleeping SRL Thread");
						}
					}
					Thread.Sleep(100);
				}
				ThreadIsSleeping = false;
				if (s_ThreadAboutToAbort)
				{
					s_RecievedSelectedToAvoidDamageResponse = true;
					s_ThreadAboutToAbort = false;
					Log.SimpleInfo("Abort First ContinueActorDamagedAfterSelectingPlayerActorToBurnCards");
					return;
				}
			}
			bool flag15 = false;
			if (s_PlayerSelectedToAvoidDamage != EAvoidDamageOption.None)
			{
				if (CurrentDamageData.ActorDamaged != cPlayerActor)
				{
					foreach (CActiveBonus item2 in list5)
					{
						item2.ToggleActiveBonus(null, CurrentDamageData.ActorDamaged);
						item2.TriggerPreventDamage(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
					}
				}
				if (s_PlayerSelectedToAvoidDamage != EAvoidDamageOption.UsePreventDamageActiveBonus)
				{
					flag15 = true;
				}
				Log.SimpleInfo(CurrentDamageData.ActorDamaged.Class.ID + " chose to avoid " + CurrentDamageData.DamageSourceStrength + " damage");
				CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
				CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
				if (s_PlayerSelectedToAvoidDamage == EAvoidDamageOption.UsePreventDamageActiveBonus)
				{
					foreach (CActiveBonus item3 in s_UsedPreventDamageBonusesToAvoidDamage.ToList())
					{
						if (item3.IsActiveBonusToggledAndNotRestricted(CurrentDamageData.ActorDamaged))
						{
							triggeredPreventDamage = true;
							preventedDamageData = CurrentDamageData;
						}
						item3.TriggerPreventDamage(CurrentDamageData.DamageAvoided, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
					}
				}
				CPlayerSelectedToAvoidDamage_MessageData cPlayerSelectedToAvoidDamage_MessageData = new CPlayerSelectedToAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerSelectedToAvoidDamage_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				cPlayerSelectedToAvoidDamage_MessageData.m_AvoidDamageOption = s_PlayerSelectedToAvoidDamage;
				cPlayerSelectedToAvoidDamage_MessageData.m_CardsBurnedToAvoidDamage = s_CardsBurnedToAvoidDamage.ToList();
				cPlayerSelectedToAvoidDamage_MessageData.m_UsedPreventDamageBonusesToAvoidDamage = s_UsedPreventDamageBonusesToAvoidDamage?.ToList();
				ScenarioRuleClient.MessageHandler(cPlayerSelectedToAvoidDamage_MessageData);
				ScenarioScript.current?.TickAvoidedDamage(CurrentDamageData.ActorDamaged, s_PlayerSelectedToAvoidDamage, s_CardsBurnedToAvoidDamage.ToList(), s_UsedPreventDamageBonusesToAvoidDamage?.ToList());
			}
			else
			{
				foreach (CActiveBonus item4 in s_ToggledPreventBonuses.ToList())
				{
					if (item4.IsActiveBonusToggledAndNotRestricted(CurrentDamageData.ActorDamaged))
					{
						triggeredPreventDamage = true;
						preventedDamageData = CurrentDamageData;
					}
					item4.TriggerPreventDamage(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
				}
				if (CurrentDamageData.PreDamageHealth - CurrentDamageData.ActorDamaged.Health > 0 && CurrentDamageData.TriggerOnDamageListeners)
				{
					CurrentDamageData.ActorDamaged.m_OnTakenDamageListeners?.Invoke(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceAbility, s_RecievedDamagedReducedByShieldItems, CurrentDamageData.PreDamageHealth - CurrentDamageData.ActorDamaged.Health, CurrentDamageData.DamageSourceActor);
				}
				if (ActorHealthCheck(CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged))
				{
					CPlayerSelectedToNotAvoidDamage_MessageData cPlayerSelectedToNotAvoidDamage_MessageData = new CPlayerSelectedToNotAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
					cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
					cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorOriginalHealth = CurrentDamageData.PreDamageHealth;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedToNotAvoidDamage_MessageData);
				}
			}
			if (RedirectedDamageToActor == null)
			{
				CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData = new CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				ScenarioRuleClient.MessageHandler(cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData);
				if (flag15 && PhaseManager.Phase is CPhaseAction cPhaseAction)
				{
					foreach (CItem item5 in CurrentDamageData.ActorDamaged.Inventory.AllItemsAndPets.Where((CItem item) => item.YMLData.Trigger == CItem.EItemTrigger.OnPreventDamageByLosingCards))
					{
						if (item5.YMLData?.Data?.ItemRequirements != null && !item5.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility, null, item5))
						{
							continue;
						}
						CurrentDamageData.ActorDamaged.Inventory.SelectItem(item5);
						CurrentDamageData.ActorDamaged.Inventory.HandleUsedItems();
						if (item5?.YMLData?.Data?.Abilities != null && item5.YMLData.Data.Abilities.Count > 0)
						{
							bool interruptRegardlessOfStrength = false;
							if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityAttack { State: CAbilityAttack.EAttackState.ActorBeenAttacked })
							{
								interruptRegardlessOfStrength = true;
							}
							cPhaseAction.StackNextAbilities(item5.YMLData.Data.Abilities, CurrentDamageData.ActorDamaged, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength);
						}
					}
				}
			}
		}
		else if (CurrentDamageData.ActorDamaged is CPlayerActor && CurrentDamageData.CheckIfPlayerCanAvoidDamageByBurningCards)
		{
			if (!flag5)
			{
				ShowItemSelection(CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceActor, CurrentDamageData.DamageSourceAbility, CItem.EItemTrigger.OnAttackedIgnoringDamageTaken, onlyInActionPhase: false);
			}
			if (ActorHealthCheck(CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged))
			{
				CPlayerSelectedToNotAvoidDamage_MessageData cPlayerSelectedToNotAvoidDamage_MessageData2 = new CPlayerSelectedToNotAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerSelectedToNotAvoidDamage_MessageData2.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				cPlayerSelectedToNotAvoidDamage_MessageData2.m_ActorOriginalHealth = CurrentDamageData.PreDamageHealth;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedToNotAvoidDamage_MessageData2);
			}
			CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2 = new CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged);
			cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
			ScenarioRuleClient.MessageHandler(cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2);
		}
		else if (!(CurrentDamageData.ActorDamaged is CPlayerActor))
		{
			bool flag16 = CurrentDamageData.DamageSourceAbility != null && CEnemyActor.damageAbilityTypes.Contains(CurrentDamageData.DamageSourceAbility.AbilityType);
			if (flag16 && CAbility.ImmuneToAbility(CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility))
			{
				goto IL_108c;
			}
			if (!flag16)
			{
				List<CAbility.EAbilityType> immunities = CurrentDamageData.ActorDamaged.Immunities;
				if (immunities != null && immunities.Contains(CurrentDamageData.DamageSourceAbilityType))
				{
					goto IL_108c;
				}
			}
			if (flag12 && CurrentDamageData.DamageSourceStrength > 0)
			{
				CActiveBonus cActiveBonus = list4[0];
				if (!cActiveBonus.IsRestricted(CurrentDamageData.ActorDamaged))
				{
					triggeredPreventDamage = true;
					cActiveBonus.ToggleActiveBonus(null, CurrentDamageData.ActorDamaged);
					cActiveBonus.TriggerPreventDamage(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
					preventedDamageData = CurrentDamageData;
					if (!cActiveBonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventPartialDamage))
					{
						Log.SimpleInfo(CurrentDamageData.ActorDamaged.Class.ID + " prevented " + CurrentDamageData.DamageSourceStrength + " damage");
						CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
						CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
					}
				}
			}
			else if (CurrentDamageData.ActorDamaged.Immunities.Contains(CAbility.EAbilityType.Damage) && (CurrentDamageData.CauseOfDamage == CActor.ECauseOfDamage.HazardousTerrain || CurrentDamageData.CauseOfDamage == CActor.ECauseOfDamage.Trap || CurrentDamageData.DamageSourceAbilityType == CAbility.EAbilityType.Retaliate))
			{
				triggeredPreventDamage = true;
				preventedDamageData = CurrentDamageData;
				CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
				CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
				CPreventDamageTriggered_MessageData message6 = new CPreventDamageTriggered_MessageData("", CurrentDamageData.ActorDamaged)
				{
					m_PreventDamageBaseCard = null,
					m_OverridePreventedDamage = CurrentDamageData.DamageAvoided
				};
				ScenarioRuleClient.MessageHandler(message6);
			}
			goto IL_130b;
		}
		goto IL_1385;
		IL_1385:
		if (s_ThreadAboutToAbort)
		{
			s_ThreadAboutToAbort = false;
			Log.SimpleInfo("Abort Second ContinueActorDamagedAfterSelectingPlayerActorToBurnCards");
		}
		else
		{
			FinishActorDamaged();
		}
		return;
		IL_108c:
		Log.SimpleInfo(CurrentDamageData.ActorDamaged.Class.ID + " prevented " + CurrentDamageData.DamageSourceStrength + " damage");
		CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
		CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
		triggeredPreventDamage = true;
		CPreventDamageTriggered_MessageData cPreventDamageTriggered_MessageData = new CPreventDamageTriggered_MessageData(null, CurrentDamageData.ActorDamaged);
		cPreventDamageTriggered_MessageData.m_PreventDamageBaseCard = null;
		cPreventDamageTriggered_MessageData.m_DamageAbility = CurrentDamageData.DamageSourceAbility;
		cPreventDamageTriggered_MessageData.m_OverridePreventedDamage = CurrentDamageData.DamageSourceStrength;
		ScenarioRuleClient.MessageHandler(cPreventDamageTriggered_MessageData);
		goto IL_130b;
		IL_130b:
		if (CurrentDamageData.ActorDamaged.m_OnTakenDamageListeners != null && list4.Count <= 0 && CurrentDamageData.TriggerOnDamageListeners)
		{
			CurrentDamageData.ActorDamaged.m_OnTakenDamageListeners?.Invoke(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceAbility, 0, CurrentDamageData.PreDamageHealth - CurrentDamageData.ActorDamaged.Health, CurrentDamageData.DamageSourceActor);
		}
		goto IL_1385;
	}

	public static void FinishActorDamaged()
	{
		if (CurrentDamageData.DamageSourceStrength > 0 && s_PlayerSelectedToAvoidDamage == EAvoidDamageOption.None)
		{
			CurrentDamageData.ActorDamaged.m_OnDamagedListeners?.Invoke(CurrentDamageData);
		}
		int num = Math.Max(0, Math.Min(CurrentDamageData.PreDamageHealth, CurrentDamageData.DamageSourceStrength - CurrentDamageData.DamageAvoided));
		UpdateDamageInflictedThisTurn(num, CurrentDamageData.DamageSourceAbilityType);
		UpdateDamagePreventedThisTurn(CurrentDamageData.DamageShielded);
		Log.SimpleInfo(CurrentDamageData.ActorDamaged.Class.ID + " takes " + num + " damage and is now at " + CurrentDamageData.ActorDamaged.Health + " health");
		if (CurrentDamageData.DamageSourceAbility != null)
		{
			CurrentDamageData.DamageSourceActor.FindCardWithAbility(CurrentDamageData.DamageSourceAbility);
		}
		if (CurrentDamageData.HadRegeneration && num > 0)
		{
			CurrentDamageData.ActorDamaged.RemovePositiveConditionToken(CCondition.EPositiveCondition.Regenerate, EConditionDecTrigger.WhenDamaged);
			CRegenerateRemoved_MessageData message = new CRegenerateRemoved_MessageData(CurrentDamageData.ActorDamaged);
			ScenarioRuleClient.MessageHandler(message);
		}
		bool isMelee = false;
		if (CurrentDamageData.DamageSourceAbility is CAbilityAttack cAbilityAttack && cAbilityAttack != null && cAbilityAttack.IsMeleeAttack)
		{
			isMelee = true;
		}
		if (CurrentDamageData.ActorDamaged.OriginalType == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == CurrentDamageData.ActorDamaged.ActorGuid)?.IsSummon;
			if (LastTargetSummary != null)
			{
				CurrentDamageData.DamageShielded = LastTargetSummary.ShieldMinusPierce;
			}
		}
		CActor damageSourceActor = CurrentDamageData.DamageSourceActor;
		if (damageSourceActor != null && damageSourceActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == CurrentDamageData.DamageSourceActor.ActorGuid)?.IsSummon;
		}
		int num2 = 0;
		if (LastTargetSummary != null && LastTargetSummary.Poison)
		{
			num2 = 1;
			foreach (AttackModifierYMLData usedAttackMod in LastTargetSummary.UsedAttackMods)
			{
				if (usedAttackMod.MathModifier.Contains("*2"))
				{
					num2 *= 2;
				}
				if (LastTargetSummary.UsedAttackMods[0].MathModifier.Contains("*0"))
				{
					num2 = 0;
				}
			}
		}
		if (LastTargetSummary != null)
		{
			LastTargetSummary.ActualDamageTaken = num;
		}
		if (CurrentDamageData.DamageSourceAbility is CAbilityAttack abilityAttack && num > 0)
		{
			UpdateTargetsActuallyDamagedInPrevAttackThisTurn(abilityAttack);
		}
		if (CurrentDamageData.DamageSourceAbility != null)
		{
			CurrentDamageData.DamageSourceAbility.DamageActuallyTakenByAbility = num;
			if (CurrentDamageData.ActorDamaged.Health < 0)
			{
				CurrentDamageData.DamageSourceAbility.ExcessDamageInflictedOnLastTargetKilled = Math.Abs(CurrentDamageData.ActorDamaged.Health);
			}
			else
			{
				CurrentDamageData.DamageSourceAbility.ExcessDamageInflictedOnLastTargetKilled = 0;
			}
		}
		DamageData damageData = DamageDataStack.Pop();
		_ = damageData.DamageSourceActor?.Class.ID;
		_ = damageData.DamageSourceActor?.OriginalType;
		if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility } && currentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && cAbilityCard != null && cAbilityCard.ActiveBonuses?.Count > 0 && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
		{
			_ = cAbilityCard.ClassID;
		}
		if (PhaseManager.CurrentPhase is CPhaseAction { PreviousPhaseAbilities: var previousPhaseAbilities } cPhaseAction2 && (previousPhaseAbilities == null || previousPhaseAbilities.Count <= 0 || !cPhaseAction2.PreviousPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.TargetingActor?.Class?.ID == "SandDevilID")))
		{
			List<CPhaseAction.CPhaseAbility> currentPhaseAbilities = cPhaseAction2.CurrentPhaseAbilities;
			if (currentPhaseAbilities != null && currentPhaseAbilities.Count > 0)
			{
				cPhaseAction2.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.TargetingActor?.Class?.ID == "SandDevilID");
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorDamaged(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorDamaged, damageData.ActorDamaged, damageData.DamageSourceActor, damageData.DamageSourceAbility), num, damageData.CauseOfDamage, damageData.DamageAvoided, damageData.DamageShielded, damageData.DamageShieldedByItems, isMelee, num2, damageData.Shield, damageData.PositiveConditionsBeforeDamage, damageData.PreDamageHealth));
		if (RedirectedDamageToActor != null)
		{
			CActor item = RedirectedDamageToActor.Item1;
			int item2 = RedirectedDamageToActor.Item2;
			RedirectedDamageToActor = null;
			ActorBeenDamaged(item, item2, damageData.CheckIfPlayerCanAvoidDamageByBurningCards, damageData.DamageSourceActor, null, CAbility.EAbilityType.Damage, damageData.Shield, damageData.IsTrapDamage, damageData.IsTerrainDamage, damageData.CannotPreventDamageWithActiveBonus);
		}
	}

	public static void ShowActionConfirmationPanel(CActor actor, CActor target, CAbility ability, string WaitReason, bool onlyInActionPhase = true)
	{
		if (PhaseManager.Phase is CPhaseAction || !onlyInActionPhase)
		{
			s_RecievedContinueAbilityResponse = false;
			CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = new CActorWantsAnActionConfirmation_MessageData(actor);
			cActorWantsAnActionConfirmation_MessageData.m_Ability = ability;
			cActorWantsAnActionConfirmation_MessageData.m_ShowHelpBox = true;
			cActorWantsAnActionConfirmation_MessageData.m_Confirm = true;
			cActorWantsAnActionConfirmation_MessageData.m_CanUndo = false;
			cActorWantsAnActionConfirmation_MessageData.m_CanSkip = false;
			cActorWantsAnActionConfirmation_MessageData.m_Target = target;
			if (WaitReason.Equals("AttackAbilityEnded", StringComparison.InvariantCultureIgnoreCase))
			{
				cActorWantsAnActionConfirmation_MessageData.m_HideHand = false;
				cActorWantsAnActionConfirmation_MessageData.m_CustomHelpBoxTextLocKey = "GUI_TOOLTIP_ATTACK_ABILITY_FINISHED";
			}
			if (WaitReason.Equals("ShadowMarkTarget", StringComparison.InvariantCultureIgnoreCase))
			{
				cActorWantsAnActionConfirmation_MessageData.m_ShowShadowBonuses = true;
			}
			cActorWantsAnActionConfirmation_MessageData.m_ContinueAbility = true;
			ScenarioRuleClient.MessageHandler(cActorWantsAnActionConfirmation_MessageData);
			WaitForConfirmation();
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
			}
		}
	}

	public static bool HasPreShortRestItemsValid(CActor caster)
	{
		return caster.Inventory.AllItemsAndPets.Any((CItem item) => item.YMLData.Trigger == CItem.EItemTrigger.BeforeShortRest && (item.SlotState == CItem.EItemSlotState.Equipped || item.SlotState == CItem.EItemSlotState.Useable));
	}

	public static void WaitForConfirmation(string reason = null, bool waitForMultiplayer = false)
	{
		ThreadSleepReason = reason;
		ThreadIsSleeping = true;
		s_RecievedContinueAbilityResponse = false;
		bool flag = false;
		while (!s_RecievedContinueAbilityResponse && !s_ThreadAboutToAbort)
		{
			if (!flag)
			{
				flag = true;
				if (ScenarioRuleClient.IsWorkerThread)
				{
					Log.Trace("Sleeping SRL Thread");
				}
			}
			Thread.Sleep(100);
		}
		ThreadIsSleeping = false;
		ThreadSleepReason = null;
		if (waitForMultiplayer)
		{
			WaitMultiplayerSync();
		}
	}

	public static void WaitMultiplayerSync()
	{
		ThreadSleepReason = "SyncWaitForConfirmation";
		ThreadIsSleeping = true;
		s_ReceivedMultiplayerFinishWaitingResponse = false;
		bool flag = false;
		CWaitForMultiplayerSyncPoint_MessageData message = new CWaitForMultiplayerSyncPoint_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		while (!s_ReceivedMultiplayerFinishWaitingResponse && !s_ThreadAboutToAbort)
		{
			if (!flag)
			{
				flag = true;
				if (ScenarioRuleClient.IsWorkerThread)
				{
					Log.Trace("Sleeping SRL Thread to wait for other players");
				}
			}
			Thread.Sleep(100);
		}
		ThreadIsSleeping = false;
		ThreadSleepReason = null;
	}

	public static void FinishedMultiplayerSyncWait()
	{
		if (ThreadIsSleeping && ThreadSleepReason == "SyncWaitForConfirmation")
		{
			s_ReceivedMultiplayerFinishWaitingResponse = true;
		}
	}

	public static bool ShowItemSelection(CActor caster, CActor actorAbilityAppliedTo, CAbility ability, CItem.EItemTrigger trigger, bool onlyInActionPhase = true, bool killAfter = false, CVars Vars = null, List<CItem> usedItems = null)
	{
		if (PhaseManager.CurrentPhase == null)
		{
			return false;
		}
		if (caster is CPlayerActor { CanUseItems: false })
		{
			return false;
		}
		List<CItem> list = null;
		bool result = false;
		IEnumerable<CItem> enumerable = caster.Inventory.AllItemsAndPets.Where((CItem cItem) => cItem.YMLData.Trigger == trigger && (cItem.SlotState == CItem.EItemSlotState.Equipped || cItem.SlotState == CItem.EItemSlotState.Useable));
		Dictionary<CAbility, CVarsContext> contextsToRestore;
		if ((PhaseManager.Phase is CPhaseAction || !onlyInActionPhase) && enumerable.Any())
		{
			if (Vars == null)
			{
				Vars = new CVars();
			}
			Vars.Set("AppliedCondition", new TValue(ability));
			Vars.Set("AppliedActor", new TValue(actorAbilityAppliedTo));
			Vars.Set("ItemOwner", new TValue(caster));
			Vars.Set("ShieldDuringAttack", TotalShieldDuringLastAttack);
			CVarsContext cVarsContext = new CVarsContext(Vars);
			contextsToRestore = new Dictionary<CAbility, CVarsContext>();
			if (ability != null)
			{
				contextsToRestore[ability] = ability.VarsContext?.parentContext;
				ability.AddParentContext(cVarsContext);
				caster.Inventory.HighlightUsableItems(ability, caster, trigger);
			}
			else
			{
				caster.Inventory.HighlightUsableItems(cVarsContext, caster, trigger);
			}
			if (!caster.Inventory.AllItemsAndPets.Any((CItem cItem) => cItem.SlotState == CItem.EItemSlotState.Useable))
			{
				RestoreContext();
				return false;
			}
			if ((trigger == CItem.EItemTrigger.OnActorMoved || trigger == CItem.EItemTrigger.OnAutomaticActivateProp) && ability is CAbilityMove cAbilityMove)
			{
				cAbilityMove.LocoPause(cAbilityMove.TargetingActor, pause: false);
				cAbilityMove.LocoStop();
			}
			List<Point> list2 = new List<Point>();
			CActor cActor = Vars.Get("ActorToAttack").GetObject<CActor>();
			if (cActor != null)
			{
				list2.Add(cActor.ArrayIndex);
			}
			if (trigger == CItem.EItemTrigger.OnAttackTargetOneByOne || trigger == CItem.EItemTrigger.AfterAttackOneByOne || trigger == CItem.EItemTrigger.BeforeStartOfTurn || trigger == CItem.EItemTrigger.AtStartOfTurn)
			{
				list2.Add(actorAbilityAppliedTo.ArrayIndex);
			}
			if (list2.Count > 0)
			{
				ScenarioManager.ToggleTileHighlights(list2, enabled: true);
			}
			List<Point> tileIndices = new List<Point> { caster.ArrayIndex };
			CVector4 color = new CVector4(0f, 0f, 1f, 1f);
			ScenarioManager.ToggleTileHighlights(tileIndices, enabled: true, color);
			s_RecievedContinueAbilityResponse = false;
			CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = new CActorWantsAnActionConfirmation_MessageData(caster);
			cActorWantsAnActionConfirmation_MessageData.m_Ability = ability;
			cActorWantsAnActionConfirmation_MessageData.m_ShowHelpBox = trigger == CItem.EItemTrigger.OnNegativeConditionFromEnemy || trigger == CItem.EItemTrigger.OnNegativeConditionGlobal || trigger == CItem.EItemTrigger.OnPositiveConditionFromEnemy || trigger == CItem.EItemTrigger.OnPositiveConditionGlobal;
			cActorWantsAnActionConfirmation_MessageData.m_Confirm = true;
			cActorWantsAnActionConfirmation_MessageData.m_CanUndo = false;
			cActorWantsAnActionConfirmation_MessageData.m_CanSkip = false;
			cActorWantsAnActionConfirmation_MessageData.m_ContinueAbility = true;
			cActorWantsAnActionConfirmation_MessageData.m_Trigger = trigger;
			cActorWantsAnActionConfirmation_MessageData.m_Context = cVarsContext;
			cActorWantsAnActionConfirmation_MessageData.m_Target = actorAbilityAppliedTo;
			cActorWantsAnActionConfirmation_MessageData.m_CustomHelpBoxTextLocKey = ((trigger == CItem.EItemTrigger.OnAttackedBeforeAttackModifiers) ? "GUI_CHOOSE_ABILITY_TARGET_Item" : null);
			ScenarioRuleClient.MessageHandler(cActorWantsAnActionConfirmation_MessageData);
			foreach (CItem item2 in enumerable)
			{
				if (item2.YMLData.Data.Abilities == null)
				{
					continue;
				}
				foreach (CAbility ability2 in item2.YMLData.Data.Abilities)
				{
					contextsToRestore[ability2] = ability2.VarsContext.parentContext;
					ability2.VarsContext.parentContext = cVarsContext;
				}
			}
			WaitForConfirmation("ItemSelection");
			RestoreContext();
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
				if (list2 != null)
				{
					ScenarioManager.ToggleTileHighlights(list2, enabled: false);
				}
				ScenarioManager.ToggleTileHighlights(tileIndices, enabled: false, color);
				return false;
			}
			list = caster.Inventory.AllItemsAndPets.Where((CItem cItem) => cItem.YMLData.Trigger == trigger && cItem.SlotState == CItem.EItemSlotState.Selected).ToList();
			CAbilityAttack cAbilityAttack = Vars.Get("ParentAttack").GetObject<CAbilityAttack>();
			CActor cActor2 = Vars.Get("ActorToAttack").GetObject<CActor>();
			if (cAbilityAttack == null)
			{
				cAbilityAttack = ability as CAbilityAttack;
			}
			bool flag = false;
			CActor currentActor = CurrentActor;
			foreach (CItem item3 in list)
			{
				usedItems?.Add(item3);
				if (item3.YMLData.Data.Vars != null && item3.YMLData.Data.Vars.Count > 0)
				{
					result = true;
				}
				if (item3.YMLData.Data.Overrides != null)
				{
					if (cAbilityAttack != null && cActor2 != null)
					{
						cAbilityAttack.ApplySingleTargetForSpecificItem(cActor2, item3);
						result = true;
					}
					else
					{
						foreach (CAbilityOverride @override in item3.YMLData.Data.Overrides)
						{
							if (cAbilityAttack != null && @override.Vars != null && @override.GetVar("ApplyToMainAttack").GetBool())
							{
								cAbilityAttack.OverrideAbilityValues(@override, perform: false, item3);
							}
							else
							{
								ability.OverrideAbilityValues(@override, perform: false, item3);
							}
							flag = true;
							result = true;
						}
					}
				}
				if (item3.YMLData.Data.RetaliateValue > 0)
				{
					result = true;
					CAbilityRetaliate.SingleShotRetaliate(item3.YMLData.Data.RetaliateValue, item3.YMLData.Data.RetaliateRange, actorAbilityAppliedTo, caster, ability);
				}
				if (item3.YMLData.Data.ShieldValue > 0 && trigger == CItem.EItemTrigger.OnAttackedBeforeAttackModifiers)
				{
					string shieldKey = $"Shield_{item3.YMLData.ID}";
					CAbility cAbility = CAbilityShield.CreateDefaultShield(item3.YMLData.Data.ShieldValue, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), shieldKey);
					cAbility.ActiveBonusData.AbilityData = cAbility;
					cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Turn;
					cAbility.ActiveBonusData.Hidden = true;
					cAbility.Start(caster, caster);
					cAbility.ApplyToActor(caster);
					CActiveBonus shieldBonus = caster.ActiveBonuses.SingleOrDefault((CActiveBonus x) => x.Ability.Name == shieldKey);
					if (shieldBonus != null)
					{
						flag = true;
						CPhaseAction.CPhaseAbility cPhaseAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility;
						if (cPhaseAbility != null)
						{
							cPhaseAbility.m_PostEndAction = (Action)Delegate.Combine(cPhaseAbility.m_PostEndAction, (Action)(() =>
							{
								shieldBonus.Finish();
							}));
						}
					}
				}
				if (item3.YMLData.Data.Abilities == null)
				{
					continue;
				}
				List<CAbility> list3 = new List<CAbility>();
				foreach (CAbility ability3 in item3.YMLData.Data.Abilities)
				{
					result = true;
					CAbility cAbility2 = CAbility.CopyAbility(ability3, generateNewID: false);
					cAbility2.AddParentContext(cVarsContext);
					if (trigger == CItem.EItemTrigger.BeforeShortRest)
					{
						PendingOnRestAbilities.Add((cAbility2, caster));
					}
					else
					{
						list3.Add(cAbility2);
					}
				}
				if (trigger == CItem.EItemTrigger.BeforeInitiativeCalc || trigger == CItem.EItemTrigger.AfterMonsterCardsRevealed)
				{
					if (s_CurrentActor != caster)
					{
						OverrideCurrentActorForOneAction(caster, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
					}
					PhaseManager.StartAbilities(list3, list3[0].GetAbilityBaseCard(), fullCopy: false, null, EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
					continue;
				}
				if (trigger == CItem.EItemTrigger.AfterShortRest)
				{
					foreach (CAbility ability4 in item3.YMLData.Data.Abilities)
					{
						CAbility item = CAbility.CopyAbility(ability4, generateNewID: false);
						PendingOnRestAbilities.Add((item, caster));
					}
					continue;
				}
				if (trigger == CItem.EItemTrigger.AtStartOfTurn)
				{
					foreach (CAbility item4 in list3)
					{
						PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(item4, caster));
					}
					continue;
				}
				if (trigger == CItem.EItemTrigger.OnSummonKilled || trigger == CItem.EItemTrigger.OnActorMoved || trigger == CItem.EItemTrigger.OnAnyActorDeath || trigger == CItem.EItemTrigger.OnAttackedIgnoringDamageTaken || trigger == CItem.EItemTrigger.OnPositiveConditionGlobal || trigger == CItem.EItemTrigger.OnAutomaticActivateProp)
				{
					if (PhaseManager.CurrentPhaseAction != null)
					{
						if (s_CurrentActor != caster && list3.Any((CAbility x) => x is CAbilityAttack))
						{
							OverrideCurrentActorForOneAction(caster);
						}
						PhaseManager.CurrentPhaseAction.StackNextAbilities(list3, caster, killAfter, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
					}
					continue;
				}
				if (trigger == CItem.EItemTrigger.AfterAttackedIgnoringDamageTaken || trigger == CItem.EItemTrigger.OnAttackTargetOneByOne)
				{
					if (PhaseManager.CurrentPhaseAction != null)
					{
						PhaseManager.CurrentPhaseAction.StackNextAbilities(list3, caster, killAfter, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: true);
					}
					continue;
				}
				if (trigger == CItem.EItemTrigger.OnAbilityApplied || trigger == CItem.EItemTrigger.OnAbilityEnded)
				{
					if (PhaseManager.CurrentPhaseAction != null)
					{
						PhaseManager.CurrentPhaseAction.StackNextAbilities(list3, caster, killAfter);
					}
					continue;
				}
				foreach (CAbility item5 in list3)
				{
					item5.Start(caster, caster);
					if (item5.Targeting == CAbility.EAbilityTargeting.Custom && item5.ValidActorsInRange != null && item5.ValidActorsInRange.Count > 0)
					{
						item5.ApplyToActor(item5.ValidActorsInRange[0]);
					}
					else
					{
						item5.ApplyToActor(caster);
					}
				}
			}
			caster.Inventory.HandleUsedItems();
			if (list2 != null)
			{
				ScenarioManager.ToggleTileHighlights(list2, enabled: false);
			}
			ScenarioManager.ToggleTileHighlights(tileIndices, enabled: false, color);
			if (flag && cAbilityAttack?.AttackSummary != null)
			{
				CAttackSummary.TargetSummary targetSummary = cAbilityAttack.AttackSummary.FindTarget(caster);
				if (targetSummary != null)
				{
					targetSummary.CalcOverallAdvantageDisadvantage();
					targetSummary.CacheFinalAttackStrength(initial: true, beforeModifiers: true);
				}
			}
		}
		foreach (CItem allItemsAndPet in caster.Inventory.AllItemsAndPets)
		{
			if (allItemsAndPet.YMLData.Trigger == trigger && allItemsAndPet.SlotState == CItem.EItemSlotState.Useable && (list == null || !list.Contains(allItemsAndPet)))
			{
				allItemsAndPet.SlotState = CItem.EItemSlotState.Equipped;
			}
		}
		return result;
		void RestoreContext()
		{
			foreach (KeyValuePair<CAbility, CVarsContext> item6 in contextsToRestore)
			{
				item6.Key.VarsContext.parentContext = item6.Value;
			}
			contextsToRestore.Clear();
		}
	}

	public static bool ShowToggleActiveBonusSelection(CPlayerActor actorToCheck, CActor actorAbilityAppliedTo, CAbility ability, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool onlyInActionPhase = true, bool killAfter = false, CVars Vars = null, List<CActiveBonus> usedBonuses = null)
	{
		if (actorToCheck == null)
		{
			return false;
		}
		List<CActiveBonus> usableActiveBonuses = (from x in actorAbilityAppliedTo.FindApplicableActiveBonuses()
			where x.Ability.ActiveBonusData.IsToggleBonus && x.Caster == actorToCheck && ((behaviourType != CActiveBonus.EActiveBonusBehaviourType.None && x.HasBehaviour(behaviourType)) || (abilityType != CAbility.EAbilityType.None && x.Ability.AbilityType == abilityType))
			select x).ToList();
		return ShowToggleActiveBonusSelection(actorToCheck, actorAbilityAppliedTo, ability, usableActiveBonuses, behaviourType, onlyInActionPhase, killAfter, Vars, usedBonuses);
	}

	public static bool ShowToggleActiveBonusSelection(CPlayerActor actorToCheck, CActor actorAbilityAppliedTo, CAbility ability, List<CActiveBonus> usableActiveBonuses, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool onlyInActionPhase = true, bool killAfter = false, CVars Vars = null, List<CActiveBonus> usedBonuses = null)
	{
		if (actorToCheck == null)
		{
			return false;
		}
		bool result = false;
		if ((PhaseManager.Phase is CPhaseAction || !onlyInActionPhase) && usableActiveBonuses.Count > 0)
		{
			foreach (CActiveBonus item in usableActiveBonuses.ToList())
			{
				if (behaviourType != CActiveBonus.EActiveBonusBehaviourType.None)
				{
					CBespokeBehaviour behaviour = item.GetBehaviour(behaviourType);
					if (!item.RequirementsMet(actorToCheck, ability) || item.CreateTriggerContext(behaviour, null, ability, actorToCheck, actorAbilityAppliedTo, true) == null)
					{
						usableActiveBonuses.Remove(item);
					}
				}
			}
			if (usableActiveBonuses.Count == 0)
			{
				return false;
			}
			foreach (CActiveBonus usableActiveBonuse in usableActiveBonuses)
			{
				if (usableActiveBonuse.ToggledBonus)
				{
					usableActiveBonuse.ToggleLocked = false;
					usableActiveBonuse.ToggleActiveBonus(null, actorToCheck);
				}
			}
			List<Point> list = new List<Point>();
			if (actorAbilityAppliedTo != null)
			{
				list.Add(actorAbilityAppliedTo.ArrayIndex);
			}
			if (list.Count > 0)
			{
				ScenarioManager.ToggleTileHighlights(list, enabled: true);
			}
			s_RecievedContinueAbilityResponse = false;
			CActorWantsAnActiveBonusConfirmation_MessageData cActorWantsAnActiveBonusConfirmation_MessageData = new CActorWantsAnActiveBonusConfirmation_MessageData(actorToCheck);
			cActorWantsAnActiveBonusConfirmation_MessageData.m_Ability = ability;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_ShowHelpBox = true;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_Confirm = true;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_CanUndo = false;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_CanSkip = false;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_ContinueAbility = true;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_UsableBonuses = usableActiveBonuses;
			cActorWantsAnActiveBonusConfirmation_MessageData.m_Target = actorAbilityAppliedTo;
			switch (behaviourType)
			{
			case CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement:
				cActorWantsAnActiveBonusConfirmation_MessageData.m_HelpTextKeyOverride = "FH/GUI_SELECT_ACTIVE_BONUS_ON_FINISH_MOVE_HELPBOX";
				break;
			case CActiveBonus.EActiveBonusBehaviourType.GenericHealAbilityFinished:
				cActorWantsAnActiveBonusConfirmation_MessageData.m_HelpTextKeyOverride = "FH/GUI_SELECT_ACTIVE_BONUS_ON_FINISH_HEAL_HELPBOX";
				break;
			case CActiveBonus.EActiveBonusBehaviourType.StartRoundIfExhausted:
				foreach (CActiveBonus usableActiveBonuse2 in usableActiveBonuses)
				{
					string text = usableActiveBonuse2.Ability.ActiveBonusData.GetMiscVar("OverrideToggleHelpTextKey").GetString();
					if (!string.IsNullOrEmpty(text))
					{
						cActorWantsAnActiveBonusConfirmation_MessageData.m_HelpTextKeyOverride = text;
						break;
					}
				}
				break;
			}
			ScenarioRuleClient.MessageHandler(cActorWantsAnActiveBonusConfirmation_MessageData);
			WaitForConfirmation("ActiveBonusToggle");
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
				if (list != null)
				{
					ScenarioManager.ToggleTileHighlights(list, enabled: false);
				}
				return false;
			}
			List<CActiveBonus> list2 = usableActiveBonuses.Where((CActiveBonus x) => x.IsActiveBonusToggledAndNotRestricted(actorToCheck)).ToList();
			usedBonuses?.AddRange(list2);
			if (list2.Count > 0)
			{
				result = true;
			}
			if (list != null)
			{
				ScenarioManager.ToggleTileHighlights(list, enabled: false);
			}
		}
		return result;
	}

	public static bool ShowAlternateModifierSelection(CActor caster, AttackModifierCard card, CAbility ability, bool onlyInActionPhase = true)
	{
		if ((PhaseManager.Phase is CPhaseAction || !onlyInActionPhase) && card != null && card.YMLData?.AlternateMathModifier != null)
		{
			if (card.YMLData.AlternateFilter != null && !card.YMLData.AlternateFilter.IsValidTarget(caster, caster, isTargetedAbility: false, useTargetOriginalType: true, true, skipUntargetableCheck: true))
			{
				return false;
			}
			s_RecievedContinueAbilityResponse = false;
			CacheCurrentModifierChoice(card.YMLData);
			CActorWantsAnAttackModifierConfirmation_MessageData cActorWantsAnAttackModifierConfirmation_MessageData = new CActorWantsAnAttackModifierConfirmation_MessageData(caster);
			cActorWantsAnAttackModifierConfirmation_MessageData.m_Ability = ability;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_ShowHelpBox = true;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_Confirm = true;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_CanUndo = false;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_CanSkip = false;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_ContinueAbility = true;
			cActorWantsAnAttackModifierConfirmation_MessageData.m_Trigger = card;
			ScenarioRuleClient.MessageHandler(cActorWantsAnAttackModifierConfirmation_MessageData);
			WaitForConfirmation();
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
				return false;
			}
			bool result = AttackModifierCardsSelectedToUseAfterUnfreezingThread.Contains(card.YMLData);
			AttackModifierCardsSelectedToUseAfterUnfreezingThread.Clear();
			CacheCurrentModifierChoice(null);
			return result;
		}
		return false;
	}

	public static bool ShowGenericConfirmButton(CActor caster, string helpBoxLocKey, string confirmButtonLocKey, string titleTextLocKey = null, string cancelButtonLocKey = null, bool showHeader = true, object[] helpParams = null)
	{
		s_RecievedContinueAbilityResponse = false;
		WaitingForGenericConfirmation = true;
		CActorWantsGenericConfirmation_MessageData cActorWantsGenericConfirmation_MessageData = new CActorWantsGenericConfirmation_MessageData(caster);
		cActorWantsGenericConfirmation_MessageData.m_CancelButtonLocKey = cancelButtonLocKey;
		cActorWantsGenericConfirmation_MessageData.m_TitleTextLocKey = titleTextLocKey;
		cActorWantsGenericConfirmation_MessageData.m_HelpBoxTextLocKey = helpBoxLocKey;
		cActorWantsGenericConfirmation_MessageData.m_ConfirmButtonLocKey = confirmButtonLocKey;
		cActorWantsGenericConfirmation_MessageData.m_ShowHeader = showHeader;
		cActorWantsGenericConfirmation_MessageData.m_HelpParams = helpParams;
		ScenarioRuleClient.MessageHandler(cActorWantsGenericConfirmation_MessageData);
		WaitForConfirmation();
		WaitingForGenericConfirmation = false;
		if (s_ThreadAboutToAbort)
		{
			s_RecievedContinueAbilityResponse = true;
			s_ThreadAboutToAbort = false;
			return false;
		}
		bool genericConfirmButtonToggled = GenericConfirmButtonToggled;
		GenericConfirmButtonToggled = false;
		return genericConfirmButtonToggled;
	}

	public static bool ShowLootCardSelection(CActor caster, List<LootCard> cards, out LootCard selectedLootCard, string helpBoxKey = null)
	{
		CCharacterClass obj = caster.Class as CCharacterClass;
		selectedLootCard = default;
		if (obj == null)
		{
			return false;
		}
		if (cards.Count > 0)
		{
			s_RecievedContinueAbilityResponse = false;
			CActorWantsALootCardConfirmation_MessageData cActorWantsALootCardConfirmation_MessageData = new CActorWantsALootCardConfirmation_MessageData(caster);
			cActorWantsALootCardConfirmation_MessageData.m_ShowHelpBox = true;
			cActorWantsALootCardConfirmation_MessageData.m_LootCards = cards;
			cActorWantsALootCardConfirmation_MessageData.m_HelpBoxKey = helpBoxKey;
			ScenarioRuleClient.MessageHandler(cActorWantsALootCardConfirmation_MessageData);
			WaitForConfirmation();
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
				return false;
			}
			selectedLootCard = LootCardsSelectedAfterUnfreezingThread[0];
			LootCardsSelectedAfterUnfreezingThread.Clear();
			return true;
		}
		return false;
	}

	public static void ChooseAttackModifier(AttackModifierYMLData cardData)
	{
		AttackModifierCardsSelectedToUseAfterUnfreezingThread.Clear();
		if (cardData != null)
		{
			AttackModifierCardsSelectedToUseAfterUnfreezingThread.Add(cardData);
		}
	}

	public static void CacheCurrentModifiersChoice(List<AttackModifierYMLData> modifiers)
	{
		CurrentlyChoosingModifiers.Clear();
		if (modifiers != null)
		{
			CurrentlyChoosingModifiers.AddRange(modifiers);
		}
	}

	public static void CacheCurrentModifierChoice(AttackModifierYMLData modifier)
	{
		CacheCurrentModifiersChoice((modifier != null) ? new List<AttackModifierYMLData> { modifier } : null);
	}

	public static bool ShowChooseAttackModifier(CActor caster, List<AttackModifierYMLData> modifiers, AttackModifierYMLData autoSelect, out AttackModifierYMLData selectedLootCard, string helpBoxKey = null)
	{
		CCharacterClass cCharacterClass = caster.Class as CCharacterClass;
		if (cCharacterClass == null && caster is CHeroSummonActor { Summoner: var summoner })
		{
			cCharacterClass = summoner?.CharacterClass;
		}
		selectedLootCard = null;
		if (cCharacterClass == null)
		{
			return false;
		}
		if (modifiers.Count > 0)
		{
			s_RecievedContinueAbilityResponse = false;
			CacheCurrentModifiersChoice(modifiers);
			CShowChooseAttackModifierAdvantage_MessageData cShowChooseAttackModifierAdvantage_MessageData = new CShowChooseAttackModifierAdvantage_MessageData(caster);
			cShowChooseAttackModifierAdvantage_MessageData.m_AttackModifiers = modifiers;
			cShowChooseAttackModifierAdvantage_MessageData.m_AutoSelect = autoSelect;
			ScenarioRuleClient.MessageHandler(cShowChooseAttackModifierAdvantage_MessageData);
			WaitForConfirmation("AttackModifierAdvantage");
			if (s_ThreadAboutToAbort)
			{
				s_RecievedContinueAbilityResponse = true;
				s_ThreadAboutToAbort = false;
				return false;
			}
			selectedLootCard = AttackModifierCardsSelectedToUseAfterUnfreezingThread[0];
			AttackModifierCardsSelectedToUseAfterUnfreezingThread.Clear();
			CacheCurrentModifiersChoice(null);
			return true;
		}
		return false;
	}

	public static void ShowPerkSelection(CActor caster, CActor actorConditionAppliedTo, CAbility ability, EPerkFlag trigger, bool onlyInActionPhase = true)
	{
		if (!(caster.Class is CCharacterClass cCharacterClass))
		{
			return;
		}
		List<PerksYMLData> list = cCharacterClass.Perks.Where((PerksYMLData perksYMLData) => (perksYMLData.Flags & trigger) != 0).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			PerksYMLData perk = list[num];
			CAbility cAbility = perk.Abilities.First();
			List<CActiveBonus> list2 = (from bonus in caster.FindApplicableActiveBonuses(cAbility.AbilityType, cAbility.ActiveBonusData.Behaviour)
				where bonus.Ability.PerkID == perk.ID
				select bonus).ToList();
			if (list2 != null && list2.Count > 0 && list2[0].IsRestricted(caster))
			{
				list.RemoveAt(num);
			}
		}
		if ((!(PhaseManager.Phase is CPhaseAction) && onlyInActionPhase) || list.Count <= 0)
		{
			return;
		}
		s_RecievedContinueAbilityResponse = false;
		CActorWantsAPerkConfirmation_MessageData cActorWantsAPerkConfirmation_MessageData = new CActorWantsAPerkConfirmation_MessageData(caster);
		cActorWantsAPerkConfirmation_MessageData.m_Ability = ability;
		cActorWantsAPerkConfirmation_MessageData.m_ShowHelpBox = true;
		cActorWantsAPerkConfirmation_MessageData.m_Confirm = true;
		cActorWantsAPerkConfirmation_MessageData.m_CanUndo = false;
		cActorWantsAPerkConfirmation_MessageData.m_CanSkip = false;
		cActorWantsAPerkConfirmation_MessageData.m_ContinueAbility = true;
		cActorWantsAPerkConfirmation_MessageData.m_Trigger = trigger;
		ScenarioRuleClient.MessageHandler(cActorWantsAPerkConfirmation_MessageData);
		WaitForConfirmation();
		if (s_ThreadAboutToAbort)
		{
			s_RecievedContinueAbilityResponse = true;
			s_ThreadAboutToAbort = false;
			return;
		}
		foreach (CAbility item in PerksSelectedToUseAfterUnfreezingThread)
		{
			item.Start(caster, caster);
			item.ApplyToActor(caster);
		}
		PerksSelectedToUseAfterUnfreezingThread.Clear();
	}

	public static CActionAugmentation SelectActionAugmentation(CActionAugmentation augmentation, ElementInfusionBoardManager.EElement element, bool remove, int elementIndex = 0, bool noElement = false)
	{
		if (augmentation == null || (!noElement && (elementIndex < 0 || elementIndex >= augmentation.Elements.Count)))
		{
			return null;
		}
		if (remove)
		{
			if (s_CurrentActionSelectedAugmentations.Contains(augmentation))
			{
				s_CurrentActionSelectedAugmentations.Remove(augmentation);
			}
		}
		else if (!noElement && element != augmentation.Elements[elementIndex] && augmentation.Elements[elementIndex] == ElementInfusionBoardManager.EElement.Any)
		{
			CActionAugmentation cActionAugmentation = augmentation.Copy();
			cActionAugmentation.Elements[elementIndex] = element;
			s_CurrentActionSelectedAugmentations.Remove(augmentation);
			s_CurrentActionSelectedAugmentations.Add(cActionAugmentation);
			augmentation = cActionAugmentation;
		}
		else if (!s_CurrentActionSelectedAugmentations.Contains(augmentation))
		{
			s_CurrentActionSelectedAugmentations.Add(augmentation);
		}
		return augmentation;
	}

	public static void Lose1HandCardToAvoidAttack(CPlayerActor playerActor, CAbilityCard abilityCard)
	{
		if (!CAbilityPreventLoseCard.TryPreventLose(playerActor, CActiveBonus.EActiveBonusBehaviourType.PreventLoseCardToPreventDamage))
		{
			s_CardsBurnedToAvoidDamage.Add(abilityCard);
			playerActor.CharacterClass.MoveAbilityCard(abilityCard, playerActor.CharacterClass.HandAbilityCards, playerActor.CharacterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
		}
	}

	public static void Lose2DiscardCardsToAvoidAttack(CPlayerActor playerActor, CAbilityCard abilityCard1, CAbilityCard abilityCard2)
	{
		if (!CAbilityPreventLoseCard.TryPreventLose(playerActor, CActiveBonus.EActiveBonusBehaviourType.PreventLoseCardToPreventDamage))
		{
			s_CardsBurnedToAvoidDamage.Add(abilityCard1);
			playerActor.CharacterClass.MoveAbilityCard(abilityCard1, playerActor.CharacterClass.DiscardedAbilityCards, playerActor.CharacterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		}
		if (!CAbilityPreventLoseCard.TryPreventLose(playerActor, CActiveBonus.EActiveBonusBehaviourType.PreventLoseCardToPreventDamage))
		{
			s_CardsBurnedToAvoidDamage.Add(abilityCard2);
			playerActor.CharacterClass.MoveAbilityCard(abilityCard2, playerActor.CharacterClass.DiscardedAbilityCards, playerActor.CharacterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		}
	}

	public static void UsePreventDamageBonusToAvoidAttack(List<CActiveBonus> bonuses)
	{
		s_UsedPreventDamageBonusesToAvoidDamage.AddRange(bonuses);
	}

	public static void PlayerAvoidingDamage(EAvoidDamageOption avoidDamageOption, CPlayerActor playerToBurnCard = null)
	{
		s_PlayerSelectedToAvoidDamage = avoidDamageOption;
		if (playerToBurnCard != null && IsChoosingToBurnCardForAlly)
		{
			SelectedPlayerToAvoidDamage(playerToBurnCard);
		}
		else
		{
			s_RecievedSelectedToAvoidDamageResponse = true;
		}
	}

	public static List<(int damagePrevented, CActiveBonus bonus, CItem item)> CalcShieldPierceResult(List<CActiveBonus> bonuses, List<CItem> toggledShieldItems, CAttackSummary.TargetSummary summary)
	{
		List<(int, CActiveBonus, CItem)> list = new List<(int, CActiveBonus, CItem)>();
		int num = summary.CalcExpressionString(calcPostShieldScalars: false, calcShield: false);
		CActor actorBeingAttacked = summary.ActorToAttack;
		int pierce = summary.Pierce;
		int num2 = num;
		int num3 = 0;
		int num4 = pierce;
		List<CActiveBonus> list2 = bonuses.Where((CActiveBonus x) => x.Ability.AbilityType == CAbility.EAbilityType.Shield && x.IsActiveBonusToggledAndNotRestricted(actorBeingAttacked)).ToList();
		list?.Clear();
		if (toggledShieldItems.Count > 0)
		{
			foreach (CItem toggledShieldItem in toggledShieldItems)
			{
				if (toggledShieldItem.YMLData.Data.Abilities != null && toggledShieldItem.YMLData.Data.Abilities.Count > 0)
				{
					foreach (CAbility ability in toggledShieldItem.YMLData.Data.Abilities)
					{
						list2.RemoveAll((CActiveBonus x) => x.Ability.ID == ability.ID);
					}
				}
				if (toggledShieldItem.YMLData.Data.ShieldValue > 0)
				{
					int num5 = Math.Min(num2, Math.Max(0, toggledShieldItem.YMLData.Data.ShieldValue - num4));
					num3 += num5;
					num4 = Math.Max(0, num4 - toggledShieldItem.YMLData.Data.ShieldValue);
					num2 -= num5;
					list.Add((num5, null, toggledShieldItem));
				}
			}
		}
		s_RecievedDamagedReducedByShieldItems = num3;
		foreach (CActiveBonus item in list2)
		{
			CAbility ability2 = item.Ability;
			int num6 = item.ReferenceStrength(ability2, actorBeingAttacked);
			int num7 = Math.Min(num2, Math.Max(0, num6 - num4));
			num3 += num7;
			num4 = Math.Max(0, num4 - num6);
			num2 -= num7;
			list.Add((num7, item, null));
		}
		s_RecievedDamagePreventedByActiveBonuses = num3 - s_RecievedDamagedReducedByShieldItems;
		return list;
	}

	public static void PlayerFinishedTogglingShields(CAttackSummary.TargetSummary summary, bool apply = true)
	{
		IEnumerable<CActiveBonus> source = from x in summary.ActorToShowOptionsFor.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield).Concat(summary.ActorToAttack.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield)).Distinct()
			where x.IsActiveBonusToggledAndNotRestricted(summary.ActorToShowOptionsFor)
			select x;
		List<CItem> selectedItems = summary.ActorToShowOptionsFor.Inventory.SelectedItems;
		s_ToggledShieldBonuses = source.ToList();
		s_ToggledShieldItems = selectedItems.ToList();
		if (apply)
		{
			s_RecievedSelectedToAvoidDamageResponse = true;
		}
	}

	public static void HandleFinishedTogglingShield(CAttackSummary.TargetSummary summary, bool apply = true, bool skipHitAnimation = false)
	{
		List<CActiveBonus> first = s_ToggledShieldBonuses;
		List<CItem> toggledShieldItems = s_ToggledShieldItems;
		List<CActiveBonus> passiveShieldBonuses = summary.PassiveShieldBonuses;
		CalcShieldPierceResult(first.Concat(passiveShieldBonuses).Distinct().ToList(), toggledShieldItems, summary);
		summary.CacheFinalAttackStrength(initial: false, beforeModifiers: false);
		if (apply)
		{
			summary.ActorToShowOptionsFor.Inventory.HandleUsedItems();
		}
		int num = summary.CalcExpressionString(calcPostShieldScalars: false, calcShield: true);
		if (apply)
		{
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericAfterConfirmShields, summary.OriginalAbility, summary.OriginalAbility?.TargetingActor, summary.ActorToAttack, ("TargetSummary", new TValue(summary)));
		}
		int finalAttackStrength = summary.FinalAttackStrength;
		int num2 = CurrentDamageData.DamageSourceStrengthBeforeWardBrittle - num;
		int num3 = CurrentDamageData.DamageSourceStrength - finalAttackStrength;
		if (num2 > 0 && apply)
		{
			Log.SimpleInfo(CurrentDamageData.ActorDamaged.Class.ID + " shielded " + s_RecievedDamagedReducedByShieldItems + " damage");
			CurrentDamageData.DamageSourceStrengthBeforeWardBrittle = num;
			CurrentDamageData.DamageSourceStrength = finalAttackStrength;
			if (num3 > 0)
			{
				CurrentDamageData.ActorDamaged.Healed(num3, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
			}
		}
		if (summary.AttackModifierEvent != null)
		{
			summary.AttackModifierEvent.FinalAttackStrength = summary.FinalAttackStrength;
		}
		CalcWardBrittle(CurrentDamageData.DamageSourceStrengthBeforeWardBrittle, summary, apply);
		if (summary.AttackModifierEvent != null)
		{
			summary.AttackModifierEvent.TargetActorShield = summary.Shield;
			summary.AttackModifierEvent.TargetActorHasBrittle = summary.Brittle;
			summary.AttackModifierEvent.TargetActorHasWard = summary.Ward;
			if (apply)
			{
				SEventLogMessageHandler.AddEventLogMessage(summary.AttackModifierEvent);
			}
		}
	}

	public static void PlayerNotAvoidingDamage(List<CActiveBonus> toggledPreventBonuses)
	{
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_ToggledPreventBonuses = toggledPreventBonuses.ToList();
		s_RecievedSelectedToAvoidDamageResponse = true;
	}

	public static void SelectedPlayerToAvoidDamage(CPlayerActor playerActor)
	{
		s_RecievedPlayerActorToAvoidDamageResponse = true;
		s_PlayerActorToAvoidDamage = playerActor;
	}

	public static void SelectedContinueAbility()
	{
		s_RecievedContinueAbilityResponse = true;
	}

	public static bool ResolveKillCredit(ref CActor killer, CActor victim, CActor.ECauseOfDeath causeOfDeath, ref CAbility causeOfDeathAbility)
	{
		if (killer != null && killer != victim)
		{
			return false;
		}
		if (causeOfDeath != CActor.ECauseOfDeath.Trap && causeOfDeath != CActor.ECauseOfDeath.HazardousTerrain)
		{
			return false;
		}
		CAbility cAbility = (PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability;
		if (!(cAbility is CAbilityPush cAbilityPush))
		{
			if (!(cAbility is CAbilityPull cAbilityPull))
			{
				if (!(cAbility is CAbilityTeleport2 cAbilityTeleport))
				{
					return false;
				}
				if (cAbilityTeleport.ActorToTeleport != victim)
				{
					return false;
				}
			}
			else if (cAbilityPull.CurrentTarget != victim)
			{
				return false;
			}
		}
		else if (cAbilityPush.CurrentTarget != victim)
		{
			return false;
		}
		killer = cAbility.TargetingActor;
		causeOfDeathAbility = cAbility;
		return true;
	}

	public static bool KillActor(CActor targetingActor, CActor actor, CActor.ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, CAbility causeOfDeathAbility = null, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null, bool instantlyDeletePrefab = false, bool dontLog = false)
	{
		if (actor.CanBeDowned)
		{
			actor.SetDowned(downed: true);
			startedOnDeathAbility = false;
			return false;
		}
		ResolveKillCredit(ref targetingActor, actor, causeOfDeath, ref causeOfDeathAbility);
		if (actor is CEnemyActor cEnemyActor)
		{
			if (!(targetingActor is CEnemyActor))
			{
				GlobalStatRepository.Instance.MonstersKilled.Increment();
			}
			cEnemyActor.Type = cEnemyActor.OriginalType;
		}
		if (actor.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete: false, causeOfDeathAbility, attackSummary))
		{
			KillActorInternal(targetingActor, actor, onDeathAbility: false, actorWasAsleep, instantlyDeletePrefab, dontLog);
			ScenarioManager.CheckAchievements();
			return true;
		}
		return false;
	}

	public static void TeleportActor(CActor actor, CTile destTile, bool tryReveal = false, CAbility ability = null, bool invokeOnMovedListeners = false, bool useAnim = true, bool useFade = false, bool fromDoorOpen = false)
	{
		CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(actor, useAnim, useFade)
		{
			m_EndLocation = destTile.m_ArrayIndex,
			m_StartLocation = actor.ArrayIndex,
			m_ActorTeleported = actor,
			m_TeleportAbility = null
		};
		CAbilityMove cAbilityMove = ability as CAbilityMove;
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			if (cAbilityMove == null)
			{
				CAbilityMove cAbilityMove2 = cPhaseAction.CurrentPhaseAbility?.m_Ability as CAbilityMove;
				if (cAbilityMove2?.CurrentMovingActor == actor)
				{
					cAbilityMove = cAbilityMove2;
				}
			}
			if (cAbilityMove != null)
			{
				if (cAbilityMove.RemainingMovesAfterLimit > 0)
				{
					if (cAbilityMove.TargetingActor != actor)
					{
						PhaseManager.NextStep();
					}
					if (fromDoorOpen)
					{
						cAbilityMove.RemainingMoves--;
					}
					cAbilityMove.ForceInterrupt(fromTeleport: true);
				}
				else
				{
					cAbilityMove.WasInterrupted = true;
					cAbilityMove.WasForcedInterrupted = true;
				}
			}
		}
		bool invokeOnMovedListeners2 = actor.invokeOnMovedListeners;
		if (!invokeOnMovedListeners)
		{
			actor.invokeOnMovedListeners = false;
		}
		CTile tile = actor.Tile;
		int num = Point.HexDist(actor.ArrayIndex, destTile.m_ArrayIndex);
		actor.ArrayIndex = destTile.m_ArrayIndex;
		if (ability != null)
		{
			CAbilityTeleport2.ActivatePropsOnTeleport(actor, destTile, tile, ability);
		}
		actor.invokeOnMovedListeners = invokeOnMovedListeners2;
		ScenarioRuleClient.MessageHandler(message);
		if (tryReveal)
		{
			CMap[] array = new CMap[2] { destTile.m_HexMap, destTile.m_Hex2Map };
			bool flag = false;
			CMap[] array2 = array;
			foreach (CMap cMap in array2)
			{
				if (cMap != null && !cMap.Revealed)
				{
					cMap.Reveal(initial: false);
					CRevealRooms_MessageData message2 = new CRevealRooms_MessageData();
					ScenarioRuleClient.MessageHandler(message2);
					flag = true;
				}
			}
			if (flag)
			{
				SortIntoInitiativeAndIDOrder();
			}
		}
		if (invokeOnMovedListeners)
		{
			CAbility cAbility = CAbilityTeleport2.CreateDefaultTeleport2(num, num, 1, actor.Type != CActor.EType.Player);
			cAbility.TargetingActor = actor;
			actor.m_OnMovedListeners?.Invoke(cAbility, actor, new List<CActor>(), newActorCarried: false, num, finalMovement: true, destTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), destTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), num, actor.PrevArrayIndex);
		}
	}

	public static bool PhaseOutActor(CActor actor, bool instant = true)
	{
		actor.CauseOfDeath = CActor.ECauseOfDeath.ActorRemovedFromMap;
		actor.PhasedOut = true;
		CPhaseOut_MessageData message = new CPhaseOut_MessageData
		{
			m_Actor = actor,
			m_Tile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y],
			m_PhaseOutInstantly = instant
		};
		ScenarioRuleClient.MessageHandler(message);
		return true;
	}

	public static bool PhaseInActor(CActor actor)
	{
		if (!actor.PhasedOut)
		{
			return false;
		}
		actor.CauseOfDeath = CActor.ECauseOfDeath.StillAlive;
		actor.PhasedOut = false;
		if (actor is CPlayerActor cPlayerActor && !actor.IsDeadForObjectives)
		{
			if (!ScenarioManager.Scenario.PlayerActors.Contains(cPlayerActor))
			{
				ScenarioManager.Scenario.PlayerActors.Add(cPlayerActor);
				ScenarioManager.Scenario.ExhaustedPlayers.Remove(cPlayerActor);
			}
			if (cPlayerActor.Form == CPlayerActor.EForm.Choose)
			{
				cPlayerActor.PendingForm = ((cPlayerActor.PrevForm != CPlayerActor.EForm.None) ? cPlayerActor.PrevForm : CPlayerActor.EForm.Melee);
			}
		}
		if (actor is CHeroSummonActor && !ScenarioManager.Scenario.HeroSummons.Contains((CHeroSummonActor)actor) && !actor.IsDead)
		{
			ScenarioManager.Scenario.HeroSummons.Add((CHeroSummonActor)actor);
			ScenarioManager.Scenario.DeadHeroSummons.Remove((CHeroSummonActor)actor);
		}
		CPhaseIn_MessageData message = new CPhaseIn_MessageData
		{
			m_Actor = actor,
			m_Tile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]
		};
		ScenarioRuleClient.MessageHandler(message);
		return true;
	}

	public static bool KillSummonOnActorDeath(CActor targetingActor, CHeroSummonActor heroSummonActor)
	{
		if (heroSummonActor.ContinueAfterSummonerDeath)
		{
			return false;
		}
		if (heroSummonActor.OnDeathFromSummonerDeath(targetingActor))
		{
			KillActorInternal(targetingActor, heroSummonActor);
			return true;
		}
		return false;
	}

	public static void KillActorInternal(CActor targetingActor, CActor actor, bool onDeathAbility = false, bool actorWasAsleep = false, bool instantlyDeletePrefab = false, bool dontLog = false)
	{
		if (!ScenarioManager.Scenario.HasActor(actor))
		{
			return;
		}
		actor.PhasedOut = false;
		actor.IsDowned = false;
		CClass.CancelAllActiveBonusesOnDeath(actor);
		if (actor.Class is CMonsterClass)
		{
			s_EnemyKillTracker.Add(new KeyValuePair<CEnemyActor, CActor>((CEnemyActor)actor, targetingActor));
			ScenarioManager.Scenario.RemoveMonster((CEnemyActor)actor);
		}
		else if (actor.Class is CCharacterClass)
		{
			ScenarioManager.Scenario.RemovePlayer((CPlayerActor)actor);
		}
		else if (actor.Class is CHeroSummonClass)
		{
			ScenarioManager.Scenario.RemoveHeroSummon((CHeroSummonActor)actor);
		}
		if (actor.CarriedQuestItems.Count > 0)
		{
			for (int num = actor.CarriedQuestItems.Count - 1; num >= 0; num--)
			{
				CObjectProp questItemProp = actor.CarriedQuestItems[num];
				actor.DropQuestItem(questItemProp);
			}
			actor.CarriedQuestItems.Clear();
		}
		if (!(actor is CEnemyActor) || actor is CEnemyActor { OnDeathAbilityActorDeadHandled: false })
		{
			CActorDead_MessageData message = new CActorDead_MessageData(actor)
			{
				m_Actor = actor,
				m_OnDeathAbility = onDeathAbility,
				m_ActorWasAsleep = actorWasAsleep,
				m_InstantlyDeletePrefab = instantlyDeletePrefab,
				m_DontLog = dontLog
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		actor.m_AfterDeathListeners?.Invoke(actor, null);
		for (int num2 = ScenarioManager.Scenario.HeroSummons.Count - 1; num2 >= 0; num2--)
		{
			CHeroSummonActor cHeroSummonActor = ScenarioManager.Scenario.HeroSummons[num2];
			if (cHeroSummonActor.Summoner == actor)
			{
				KillSummonOnActorDeath(targetingActor, cHeroSummonActor);
			}
		}
		CActiveBonus.RefreshAllAuraActiveBonuses();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	public static bool ActorHealthCheck(CActor targetingActor, CActor actor, bool isTrap = false, bool isTerrain = false, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null)
	{
		bool onDeathAbility;
		return ActorHealthCheck(targetingActor, actor, out onDeathAbility, isTrap, isTerrain, actorWasAsleep, attackSummary);
	}

	public static bool ActorHealthCheck(CActor targetingActor, CActor actor, out bool onDeathAbility, bool isTrap = false, bool isTerrain = false, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null)
	{
		onDeathAbility = false;
		if (actor.Health <= 0 && !actor.IsDead)
		{
			CActor.ECauseOfDeath causeOfDeath;
			if (isTrap)
			{
				causeOfDeath = CActor.ECauseOfDeath.Trap;
			}
			else
			{
				causeOfDeath = (isTerrain ? CActor.ECauseOfDeath.HazardousTerrain : CActor.ECauseOfDeath.Damage);
			}
			onDeathAbility = !KillActor(targetingActor, actor, causeOfDeath, out var _, actor.LastAbilityDamagedBy, actorWasAsleep, attackSummary) && !actor.IsDowned;
			return actor.IsDowned;
		}
		if (!actor.IsDead && PhaseManager.PreviousPhase is CPhaseShortRest && CheckExhaustAfterShortRest(actor as CPlayerActor, isShortRestingPlayer: false))
		{
			return false;
		}
		return !actor.IsDead;
	}

	public static void Undo()
	{
		s_CurrentActionValidAugmentations.Clear();
		s_CurrentActionSelectedAugmentations.Clear();
		s_CurrentActor.Inventory.ClearSelectedItems();
		if (CurrentActionInitiator == EActionInitiator.ActionsTriggeredOutsideActionPhase)
		{
			CurrentActionInitiator = EActionInitiator.None;
		}
	}

	public static void SortIntoInitiativeAndIDOrder()
	{
		List<CEnemyActor> allAliveMonsters = ScenarioManager.Scenario.AllAliveMonsters;
		if (allAliveMonsters != null && allAliveMonsters.Count > 0)
		{
			allAliveMonsters.Sort((CEnemyActor x, CEnemyActor y) => x.Class.ID.CompareTo(y.Class.ID));
			string classid = allAliveMonsters[0].Class.ID;
			int num = 0;
			for (int num2 = 0; num2 < allAliveMonsters.Count; num2++)
			{
				CEnemyActor cEnemyActor = allAliveMonsters[num2];
				if (!string.Equals(cEnemyActor.Class.ID, classid))
				{
					foreach (CEnemyActor item in allAliveMonsters.Where((CEnemyActor m) => m.Class.ID == classid))
					{
						item.MonsterGroupType = num;
					}
					num = 0;
					classid = cEnemyActor.Class.ID;
				}
				if (cEnemyActor.Type == CActor.EType.Ally)
				{
					num |= 8;
				}
				if (cEnemyActor.Type == CActor.EType.Enemy)
				{
					num |= 4;
				}
				if (cEnemyActor.Type == CActor.EType.Enemy2)
				{
					num |= 2;
				}
				if (cEnemyActor.Type == CActor.EType.Neutral)
				{
					num |= 1;
				}
			}
			foreach (CEnemyActor item2 in allAliveMonsters.Where((CEnemyActor m) => m.Class.ID == classid))
			{
				item2.MonsterGroupType = num;
			}
		}
		s_InitiativeSortedActors.Clear();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			s_InitiativeSortedActors.Add(playerActor);
		}
		foreach (CHeroSummonActor item3 in ScenarioManager.Scenario.HeroSummons.Where((CHeroSummonActor x) => x.Summoner == null || x.Summoner.IsDead))
		{
			s_InitiativeSortedActors.Add(item3);
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (allAliveMonster.Class is CMonsterClass { RoundAbilityCard: not null })
			{
				s_InitiativeSortedActors.Add(allAliveMonster);
			}
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (@object.Class is CMonsterClass { RoundAbilityCard: not null })
			{
				s_InitiativeSortedActors.Add(@object);
			}
		}
		s_InitiativeSortedActors.RemoveAll((CActor x) => x.IsDead);
		s_InitiativeSortedActors.Sort((CActor x, CActor y) => s_ActorInitiativeComparer.Compare(x, y));
		foreach (CHeroSummonActor item4 in from x in ScenarioManager.Scenario.HeroSummons
			where x.Summoner != null && !x.Summoner.IsDead
			orderby x.Summoner.CharacterClass.ActivatedCards.IndexOf(x.FindSummonCard()), x.ID
			select x)
		{
			int index = s_InitiativeSortedActors.IndexOf(item4.Summoner);
			s_InitiativeSortedActors.Insert(index, item4);
		}
	}

	public static bool ShouldReverseInitiativeOrder()
	{
		return ScenarioManager.CurrentScenarioState.Challenges.Any((CChallenge x) => x.GetTriggers().Contains(CChallenge.Trigger.ReverseInitiative));
	}

	public static void AddActorsIntoInitiativeOrderAfterCurrentActor(List<CActor> insertActors)
	{
		int num = s_InitiativeSortedActors.IndexOf(s_CurrentActor) + 1;
		foreach (CActor insertActor in insertActors)
		{
			s_InitiativeSortedActors.Insert(num, insertActor);
			num++;
		}
	}

	public static void AddActorIntoInitiativeOrder(CActor insertActor)
	{
		s_InitiativeSortedActors.Add(insertActor);
		s_InitiativeSortedActors.Sort(s_ActorInitiativeComparer);
	}

	public static bool EndTurnCheckNextActorAndMoveToNextPhase()
	{
		if (PhaseManager.PreviousPhase != null)
		{
			PhaseManager.EndActiveBonusOrScenarioModifierAbility();
			return false;
		}
		Log.Info("GameState EndTurnCheckNextActorAndMoveToNextPhase");
		bool flag = false;
		if (s_CurrentActor.IsTakingExtraTurn)
		{
			flag = true;
			ExtraTurnActionSelectionFlagStack.Pop();
		}
		else
		{
			ResetActionFlagsByActionSequenceType(s_ActionFlagsByActionSequenceType);
			LastAbilityCardActionType = CBaseCard.ActionType.NA;
		}
		CurrentActionInitiator = EActionInitiator.None;
		s_LastActor = s_CurrentActor;
		if (s_LastActor.Class is CCharacterClass cCharacterClass && !flag)
		{
			cCharacterClass.HasLongRested = false;
			cCharacterClass.HasShortRested = false;
			cCharacterClass.HasImprovedShortRested = false;
		}
		if (s_LastActor != null && s_LastActor.IsDead && PhaseManager.CurrentPhaseAction?.ElementsToInfuse != null && PhaseManager.CurrentPhaseAction.ElementsToInfuse.Count > 0)
		{
			ElementInfusionBoardManager.Infuse(PhaseManager.CurrentPhaseAction.ElementsToInfuse, s_LastActor);
		}
		CheckForFinishedActiveBonuses();
		if (s_CurrentActor != s_LastActor && OverridingCurrentActor)
		{
			return false;
		}
		if (s_CurrentActor.Type == CActor.EType.Player)
		{
			s_CurrentActor.Inventory.HighlightUsableItems(null, default(CItem.EItemTrigger));
		}
		if (!s_CurrentActor.IsTakingExtraTurn)
		{
			CActor cActor = s_CurrentActor;
			bool playedThisRound = (s_CurrentActor.PlayedAtLeastOnceThisRound = true);
			cActor.PlayedThisRound = playedThisRound;
		}
		EndOverrideActorForOneTurn();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		s_CurrentActionValidAugmentations.Clear();
		s_CurrentActionSelectedAugmentations.Clear();
		if (EndRoundReached)
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.EndRound);
		}
		else
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurn);
		}
		return true;
	}

	public static void MoveToNextActor()
	{
		ResetActionFlagsByActionSequenceType(s_ActionFlagsByActionSequenceType);
		RoundAbilityCardSelected = null;
		LastAbilityCardActionType = CBaseCard.ActionType.NA;
		CActor cActor = s_TurnActor;
		int num = -1;
		CActor nextActor = null;
		for (int i = 0; i < s_InitiativeSortedActors.Count; i++)
		{
			CActor cActor2 = s_InitiativeSortedActors[i];
			if (ScenarioManager.Scenario.HasActor(cActor2) && (!cActor2.Deactivated || !cActor2.SkipTurnWhenDeactivated) && (!cActor2.PlayedThisRound || cActor2.HasPendingExtraTurn))
			{
				if (s_SkipMonsterTurns && !(cActor2 is CPlayerActor))
				{
					bool playedThisRound = (cActor2.PlayedAtLeastOnceThisRound = true);
					cActor2.PlayedThisRound = playedThisRound;
					cActor2.MarkWontPlayThisRound(cActor2.PlayedThisRound);
				}
				else if (cActor2.GetMirrorAbilitiesActor() == null)
				{
					num = i;
					nextActor = cActor2;
					break;
				}
			}
		}
		ElementInfusionBoardManager.CheckMonsterSetFinished(cActor, nextActor);
		if (num == -1)
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.EndRound);
			return;
		}
		s_CurrentActor = s_InitiativeSortedActors[num];
		s_TurnActor = s_CurrentActor;
		CActor turnActor = TurnActor;
		CEnemyActor enemy = turnActor as CEnemyActor;
		if (enemy != null && enemy.OverrideRoundAbilityCard != null)
		{
			TurnOrderSoFar.Add((enemy, MonsterClassManager.MonsterAndObjectClasses.FirstOrDefault((CMonsterClass x) => x.ID == enemy.OverrideRoundAbilityCard.ClassID)));
		}
		else
		{
			TurnOrderSoFar.Add((TurnActor, TurnActor.Class));
		}
		ClearPlayedCards(s_TurnActor.ActorGuid);
		if (s_CurrentActor.HasPendingExtraTurn && s_CurrentActor == cActor)
		{
			s_CurrentActor.TakingExtraTurnOfTypeStack.Push(s_CurrentActor.PendingExtraTurnOfTypeStack.Pop());
			s_CurrentActor.TakingExtraTurnActionMonsterClassIDStack.Push(s_CurrentActor.PendingExtraTurnActionMonsterClassIDStack.Pop());
			EActionFlag item = EActionFlag.None;
			ExtraTurnActionSelectionFlagStack.Push(item);
			if (s_CurrentActor is CPlayerActor cPlayerActor)
			{
				if (s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
				{
					CAbilityExtraTurn.PendingExtraTurnData pendingExtraTurnData = cPlayerActor.CharacterClass.PendingExtraTurnCardsData[0];
					cPlayerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(pendingExtraTurnData.PendingExtraTurnCards.First((CAbilityCard x) => x.Initiative == pendingExtraTurnData.LeadingInitiative));
					List<CAbilityCard> pendingExtraTurnCards = pendingExtraTurnData.PendingExtraTurnCards;
					for (int num2 = pendingExtraTurnCards.Count - 1; num2 >= 0; num2--)
					{
						CAbilityCard cAbilityCard = pendingExtraTurnCards[num2];
						if (pendingExtraTurnCards.Contains(cAbilityCard))
						{
							cPlayerActor.CharacterClass.MoveAbilityCard(cAbilityCard, pendingExtraTurnCards, cPlayerActor.CharacterClass.ExtraTurnCards, "PendingExtraTurnCards", "ExtraTurnCards");
						}
						else
						{
							Log.ErrorWithReport("Card to use for Extra Turn could not be found in PendingExtraTurnCards List");
						}
					}
					cPlayerActor.CharacterClass.PendingExtraTurnCardsData.RemoveAt(0);
				}
				cPlayerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Push(cPlayerActor.CharacterClass.ExtraTurnCards.ToList());
			}
		}
		if (s_CurrentActor is CPlayerActor cPlayerActor2)
		{
			if (cPlayerActor2.SkipTopCardAction)
			{
				cPlayerActor2.SkipTopCardAction = false;
				SetActionFlag(EActionFlag.TopForgone, cPlayerActor2.SelectedRoundCardsCount);
			}
			if (cPlayerActor2.SkipBottomCardAction)
			{
				cPlayerActor2.SkipBottomCardAction = false;
				SetActionFlag(EActionFlag.BottomForgone, cPlayerActor2.SelectedRoundCardsCount);
			}
			ShowItemSelection(cPlayerActor2, cPlayerActor2, null, CItem.EItemTrigger.BeforeStartOfTurn, onlyInActionPhase: false);
		}
		if (s_CurrentActor is CHeroSummonActor cHeroSummonActor)
		{
			ShowItemSelection(cHeroSummonActor.Summoner, cHeroSummonActor, null, CItem.EItemTrigger.BeforeStartOfTurn, onlyInActionPhase: false);
			if (cHeroSummonActor.SummonData.PlayerControlled || cHeroSummonActor.FindApplicableActiveBonuses(CAbility.EAbilityType.ControlSummonTurn).Any())
			{
				QueueOverrideActorForOneTurn(cHeroSummonActor, CActor.EType.Player, null, null, cHeroSummonActor.Summoner);
			}
		}
		foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
		{
			s_InitiativeSortedActor.AbilityTypesPerformedThisTurn.Clear();
		}
		s_DamageInflictedThisTurn = 0;
		s_DamagePreventedThisTurn = 0;
		s_ActorsMovedThisTurn.Clear();
		s_ActorsKilledThisTurn.Clear();
		s_TargetsDamagedInPrevAttackThisTurn = 0;
		s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		s_HexesMovedThisTurn.Clear();
		s_ObstaclesDestroyedThisTurn = 0;
		s_HazardousTerrainTilesMovedOverThisTurn = 0;
		s_DifficultTerrainTilesMovedOverThisTurn = 0;
		PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForForgoActionActiveBonuses);
	}

	private static void ProgressActionSelection(CAbilityCard roundAbilityCard, int roundCardsCount)
	{
		bool flag = roundAbilityCard.SelectedAction.ID == roundAbilityCard.TopAction.ID || roundAbilityCard.SelectedAction.ID == roundAbilityCard.DefaultAttackAction.ID;
		if (GetCurrentActionSequenceType(roundCardsCount) >= EActionSequenceType.Complete)
		{
			return;
		}
		if (IsPerformingCardOutOfPhase)
		{
			SetActionFlag(flag ? EActionFlag.TopPlayed : EActionFlag.BottomPlayed, roundCardsCount);
		}
		else if (s_CurrentActor.IsTakingExtraTurn)
		{
			if (ExtraTurnActionSelectionFlagStack.Count > 0)
			{
				EActionFlag eActionFlag = ExtraTurnActionSelectionFlagStack.Pop();
				if (s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn || s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleActionBeforeCardSelection || s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleActionAfterExtraTurnPerform)
				{
					eActionFlag |= EActionFlag.TopPlayed | EActionFlag.BottomPlayed;
				}
				else
				{
					eActionFlag = ((!flag) ? (eActionFlag | EActionFlag.BottomPlayed) : (eActionFlag | EActionFlag.TopPlayed));
				}
				ExtraTurnActionSelectionFlagStack.Push(eActionFlag);
			}
		}
		else
		{
			SetActionFlag(flag ? EActionFlag.TopPlayed : EActionFlag.BottomPlayed, roundCardsCount);
			LastAbilityCardActionType = RoundAbilityCardActionType;
		}
	}

	public static void SetActionFlag(EActionFlag flag, int roundCardsCount)
	{
		if (CurrentOutOfPhaseCardSelectionAbility != null)
		{
			EActionFlag eActionFlag = OutOfPhaseActionSelectionFlagStack.Pop();
			eActionFlag |= flag;
			OutOfPhaseActionSelectionFlagStack.Push(eActionFlag);
		}
		else if (s_CurrentActor.IsTakingExtraTurn)
		{
			EActionFlag eActionFlag2 = ExtraTurnActionSelectionFlagStack.Pop();
			eActionFlag2 |= flag;
			ExtraTurnActionSelectionFlagStack.Push(eActionFlag2);
		}
		else
		{
			SetActionFlag(flag, s_ActionFlagsByActionSequenceType, roundCardsCount);
		}
	}

	public static int GetRemainingActions(CPlayerActor player, bool top)
	{
		List<Tuple<int, EActionFlag>> playedCards = GetPlayedCards(CAbilityExtraTurn.EExtraTurnType.None, player.ActorGuid);
		int num = playedCards.Count((Tuple<int, EActionFlag> x) => x.Item2 == EActionFlag.TopPlayed || x.Item2 == EActionFlag.TopForgone);
		int num2 = playedCards.Count((Tuple<int, EActionFlag> x) => x.Item2 == EActionFlag.BottomPlayed || x.Item2 == EActionFlag.BottomForgone);
		int maxCardsClampedToRound = player.MaxCardsClampedToRound;
		int num3;
		int val;
		if (top)
		{
			num3 = num;
			val = num2;
		}
		else
		{
			num3 = num2;
			val = num;
		}
		return maxCardsClampedToRound - num3 - Math.Max(val, 1);
	}

	public static void SetActionFlagRound(EActionFlag flag, int roundCardsCount)
	{
		SetActionFlag(flag, s_ActionFlagsByActionSequenceType, roundCardsCount);
	}

	public static EActionFlag GetActionFlagRound(int roundCardsCount)
	{
		return s_ActionFlagsByActionSequenceType[roundCardsCount];
	}

	private static void SetActionFlag(EActionFlag flag, IList<EActionFlag> actionFlagsByActionSequenceType, int roundCardsCount)
	{
		EActionSequenceType currentActionSequenceType = GetCurrentActionSequenceType(actionFlagsByActionSequenceType, roundCardsCount);
		if (currentActionSequenceType < EActionSequenceType.Complete)
		{
			actionFlagsByActionSequenceType[(int)currentActionSequenceType] |= flag;
		}
	}

	public static void LogEndTurn(CActor actor)
	{
		if (actor is CPlayerActor cPlayerActor)
		{
			ReportEndTurn(cPlayerActor);
			CPlayerActor.LogHeroSummonMoveChanged(cPlayerActor);
			CPlayerActor.LogActiveBonusTrackers(cPlayerActor);
		}
		else if (actor is CHeroSummonActor heroSummon)
		{
			CPlayerActor.LogHeroSummonMoveChanged(heroSummon);
		}
	}

	public static void NextPhase()
	{
		Log.GameplayInfo($"GameState Next Phase called, current phase: {PhaseManager.PhaseType}");
		switch (PhaseManager.PhaseType)
		{
		case CPhase.PhaseType.StartScenarioEffects:
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.StartRoundEffects);
			}
			break;
		case CPhase.PhaseType.StartRoundEffects:
			PlayedInfusion = false;
			foreach (CActiveBonus item in CharacterClassManager.FindAllActiveBonuses())
			{
				item.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerRound);
			}
			if (LastSpawnerStartRound != ScenarioManager.CurrentScenarioState.RoundNumber)
			{
				LastSpawnerStartRound = ScenarioManager.CurrentScenarioState.RoundNumber;
				foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
				{
					if (spawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						spawner.OnStartRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
					}
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventRound(ScenarioManager.Scenario.AllEnemyMonsters.Count + ScenarioManager.Scenario.AllEnemy2Monsters.Count));
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				heroSummon.CachedSummonCard = null;
			}
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.PlayerExhausted);
			}
			break;
		case CPhase.PhaseType.PlayerExhausted:
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.Autosave);
			}
			break;
		case CPhase.PhaseType.Autosave:
		{
			bool flag6 = true;
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				flag6 &= allPlayer.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap;
				if (allPlayer.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
				{
					s_CurrentActor = allPlayer;
					return;
				}
			}
			if (flag6)
			{
				MonsterClassManager.SelectRoundAbilityCards();
				PhaseManager.SetNextPhase(CPhase.PhaseType.MonsterClassesSelectAbilityCards);
				ScenarioRuleClient.Pass();
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			}
			break;
		}
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			CurrentActionInitiator = EActionInitiator.None;
			PhaseManager.SetNextPhase(CPhase.PhaseType.ShortRest);
			break;
		case CPhase.PhaseType.ShortRest:
		{
			CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(null);
			ScenarioRuleClient.MessageHandler(message);
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (playerActor.CharacterClass.RoundAbilityCards.Count < 2 && !playerActor.IsLongRestSelected && !playerActor.IsDead)
				{
					PhaseManager.SetNextPhase(CPhase.PhaseType.SelectAbilityCardsOrLongRest);
					return;
				}
			}
			MonsterClassManager.SelectRoundAbilityCards();
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor2.CharacterClass.ImprovedShortRest = false;
				playerActor2.CharacterClass.ImprovedShortRestDrawCount = 0u;
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.MonsterClassesSelectAbilityCards);
			break;
		}
		case CPhase.PhaseType.MonsterClassesSelectAbilityCards:
			ClearPlayedCards();
			if (!(PhaseManager.CurrentPhase is CPhaseSelectMonsterClassAbilityCards cPhaseSelectMonsterClassAbilityCards) || !cPhaseSelectMonsterClassAbilityCards.PromptBeforeEnd())
			{
				ResetActionFlagsByActionSequenceType(s_ActionFlagsByActionSequenceType);
				RoundAbilityCardSelected = null;
				LastAbilityCardActionType = CBaseCard.ActionType.NA;
				SortIntoInitiativeAndIDOrder();
				s_DamageInflictedThisTurn = 0;
				s_DamagePreventedThisTurn = 0;
				s_ActorsMovedThisTurn.Clear();
				s_ActorsKilledThisTurn.Clear();
				s_ActorsKilledThisRound.Clear();
				s_TrophyTokensGainedThisRound = 0;
				s_TargetsDamagedInPrevAttackThisTurn = 0;
				s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
				s_HexesMovedThisTurn.Clear();
				s_ObstaclesDestroyedThisTurn = 0;
				s_HazardousTerrainTilesMovedOverThisTurn = 0;
				s_DifficultTerrainTilesMovedOverThisTurn = 0;
				s_CheckAdjustInitiativeIndex = 0;
				if (PreInitiativeAdjustedPlayerActors.Count > 0)
				{
					s_CurrentActor = PreInitiativeAdjustedPlayerActors[s_CheckAdjustInitiativeIndex++];
				}
				PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForInitiativeAdjustments);
			}
			break;
		case CPhase.PhaseType.CheckForInitiativeAdjustments:
		{
			CEndInitiativeAdjustments_MessageData message3 = new CEndInitiativeAdjustments_MessageData(InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message3);
			if (s_CheckAdjustInitiativeIndex < PreInitiativeAdjustedPlayerActors.Count)
			{
				s_CurrentActor = PreInitiativeAdjustedPlayerActors[s_CheckAdjustInitiativeIndex++];
				PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForInitiativeAdjustments);
				break;
			}
			PreInitiativeAdjustedPlayerActors.Clear();
			SortIntoInitiativeAndIDOrder();
			foreach (CPlayerActor playerActor3 in ScenarioManager.Scenario.PlayerActors)
			{
				foreach (CAdjustInitiativeActiveBonus item2 in playerActor3.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative))
				{
					item2.InitiativeAdjusted();
				}
				if (playerActor3.Agility != CPlayerActor.EAgility.None)
				{
					SEventLogMessageHandler.AddEventLogMessage(new SEventActorAgility(playerActor3.Agility, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorAgility, playerActor3)));
				}
			}
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				allActor.InitiativeLockedDuringRound = allActor.Initiative();
			}
			MoveToNextActor();
			break;
		}
		case CPhase.PhaseType.CheckForForgoActionActiveBonuses:
			if (s_CurrentActor is CHeroSummonActor { IsCompanionSummon: not false, Summoner: var summoner })
			{
				List<CActiveBonus> list = summoner.FindApplicableActiveBonuses(CAbility.EAbilityType.ForgoActionsForCompanion);
				if (list.Count > 0 && !summoner.CharacterClass.LongRest)
				{
					foreach (CForgoActionsForCompanionActiveBonus item3 in list)
					{
						item3.ResetToggles();
					}
					CShowForgoActiveBonusBar_MessageData cShowForgoActiveBonusBar_MessageData = new CShowForgoActiveBonusBar_MessageData(summoner);
					cShowForgoActiveBonusBar_MessageData.m_Ability = list[0].Ability;
					ScenarioRuleClient.MessageHandler(cShowForgoActiveBonusBar_MessageData);
				}
				else
				{
					PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
				}
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
			}
			break;
		case CPhase.PhaseType.StartTurn:
		{
			OverrideActorForOneTurn(s_CurrentActor);
			CActor cActor2 = s_CurrentActor;
			bool actorWasAsleep2 = cActor2.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			if (cActor2 is CPlayerActor cPlayerActor7 && !cActor2.IsDead && !cActor2.IsTakingExtraTurn && !cActor2.TurnStarted)
			{
				if (cPlayerActor7.UsesTimeTokens())
				{
					int maxValue = Math.Max(2, cPlayerActor7.GetTimeTokens());
					if (cPlayerActor7.Agility == CPlayerActor.EAgility.Fast)
					{
						cPlayerActor7.SetTimeTokens(cPlayerActor7.GetTimeTokens() - 1, maxValue);
					}
					else if (cPlayerActor7.Agility == CPlayerActor.EAgility.Slow)
					{
						cPlayerActor7.SetTimeTokens(cPlayerActor7.GetTimeTokens() + 1, maxValue);
					}
				}
				else if (cPlayerActor7.Class.ID == "GeminateID")
				{
					if (cPlayerActor7.PendingForm == CPlayerActor.EForm.None && cPlayerActor7.Form == CPlayerActor.EForm.Choose)
					{
						cPlayerActor7.PendingForm = cPlayerActor7.PrevForm;
					}
					if (cPlayerActor7.Form == CPlayerActor.EForm.None || (cPlayerActor7.Form == CPlayerActor.EForm.Choose && cPlayerActor7.PendingForm != CPlayerActor.EForm.None))
					{
						cPlayerActor7.Form = cPlayerActor7.PendingForm;
						cPlayerActor7.PendingForm = CPlayerActor.EForm.None;
					}
					if (cPlayerActor7.Form == CPlayerActor.EForm.Choose)
					{
						Log.ErrorWithReport($"Invalid character form: {cPlayerActor7.Form}");
					}
				}
			}
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
			foreach (CSpawner spawner2 in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner2.OnStartTurn(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			cActor2.StartTurn();
			if (s_CurrentActor == cActor2)
			{
				if (PendingScenarioModifierAbilities.Count > 0)
				{
					PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
					break;
				}
				if (!ActorHealthCheck(cActor2, cActor2, isTrap: false, isTerrain: false, actorWasAsleep2))
				{
					if (ScenarioManager.Scenario.PlayerActors.Count > 0 && s_CurrentActor == cActor2 && PhaseManager.PhaseType == CPhase.PhaseType.StartTurn)
					{
						EndTurnCheckNextActorAndMoveToNextPhase();
					}
					break;
				}
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.StartTurn)
			{
				if (cActor2.Class is CCharacterClass cCharacterClass)
				{
					cCharacterClass.SetupRoundAbilityCardsSelectedInCardSelection();
				}
				PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			}
			break;
		}
		case CPhase.PhaseType.ActionSelection:
			s_CurrentActionValidAugmentations.Clear();
			s_CurrentActionSelectedAugmentations.Clear();
			if (CurrentAction == null)
			{
				if (!(s_CurrentActor is CPlayerActor cPlayerActor2) || !cPlayerActor2.CharacterClass.LongRest || cPlayerActor2.CharacterClass.HasLongRested)
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
				break;
			}
			if ((CurrentAction.Action.Augmentations != null && CurrentAction.Action.Augmentations.Count > 0 && !s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun)) || s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(s_CurrentActor);
				List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>();
				if (s_CurrentActor.IsOriginalMonsterType)
				{
					list2 = ElementInfusionBoardManager.GetEnemyConsumedElements(s_CurrentActor);
				}
				foreach (CActionAugmentation augmentation in CurrentAction.Action.Augmentations)
				{
					bool flag = false;
					if (s_CurrentActor.IsOriginalMonsterType && list2 != null && list2.Count > 0)
					{
						foreach (ElementInfusionBoardManager.EElement item4 in augmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag = list2.Contains(item4);
							if (!flag)
							{
								break;
							}
						}
						if (augmentation.Elements.Contains(ElementInfusionBoardManager.EElement.Any))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						List<ElementInfusionBoardManager.EElement> list3 = availableElements;
						bool flag2 = true;
						foreach (ElementInfusionBoardManager.EElement item5 in augmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag2 = list3.Remove(item5);
							if (!flag2)
							{
								break;
							}
						}
						if (augmentation.Elements.Count((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any) > list3.Count)
						{
							flag2 = false;
						}
						if (flag2)
						{
							s_CurrentActionValidAugmentations.Add(augmentation);
							if (s_CurrentActor.IsOriginalMonsterType)
							{
								(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation);
							}
						}
					}
					else
					{
						s_CurrentActionValidAugmentations.Add(augmentation);
						if (s_CurrentActor.IsOriginalMonsterType)
						{
							(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation);
						}
					}
				}
			}
			if (s_CurrentActionValidAugmentations.Count > 0 && s_CurrentActor.IsOriginalMonsterType)
			{
				s_CurrentActionSelectedAugmentations.AddRange(s_CurrentActionValidAugmentations);
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.Action);
			break;
		case CPhase.PhaseType.InfuserMechanicSelection:
			s_CurrentActionValidAugmentations.Clear();
			s_CurrentActionSelectedAugmentations.Clear();
			if (CurrentAction == null)
			{
				if (!(s_CurrentActor is CPlayerActor cPlayerActor4) || !cPlayerActor4.CharacterClass.LongRest || cPlayerActor4.CharacterClass.HasLongRested)
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
				break;
			}
			if ((CurrentAction.Action.Augmentations != null && CurrentAction.Action.Augmentations.Count > 0 && !s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun)) || s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				List<ElementInfusionBoardManager.EElement> availableElements2 = ElementInfusionBoardManager.GetAvailableElements(s_CurrentActor);
				List<ElementInfusionBoardManager.EElement> list4 = new List<ElementInfusionBoardManager.EElement>();
				if (s_CurrentActor.IsOriginalMonsterType)
				{
					list4 = ElementInfusionBoardManager.GetEnemyConsumedElements(s_CurrentActor);
				}
				foreach (CActionAugmentation augmentation2 in CurrentAction.Action.Augmentations)
				{
					bool flag7 = false;
					if (s_CurrentActor.IsOriginalMonsterType && list4 != null && list4.Count > 0)
					{
						foreach (ElementInfusionBoardManager.EElement item6 in augmentation2.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag7 = list4.Contains(item6);
							if (!flag7)
							{
								break;
							}
						}
						if (augmentation2.Elements.Contains(ElementInfusionBoardManager.EElement.Any))
						{
							flag7 = true;
						}
					}
					if (!flag7)
					{
						List<ElementInfusionBoardManager.EElement> list5 = availableElements2;
						bool flag8 = true;
						foreach (ElementInfusionBoardManager.EElement item7 in augmentation2.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag8 = list5.Remove(item7);
							if (!flag8)
							{
								break;
							}
						}
						if (augmentation2.Elements.Count((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any) > list5.Count)
						{
							flag8 = false;
						}
						if (flag8)
						{
							s_CurrentActionValidAugmentations.Add(augmentation2);
							if (s_CurrentActor.IsOriginalMonsterType)
							{
								(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation2);
							}
						}
					}
					else
					{
						s_CurrentActionValidAugmentations.Add(augmentation2);
						if (s_CurrentActor.IsOriginalMonsterType)
						{
							(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation2);
						}
					}
				}
			}
			if (s_CurrentActionValidAugmentations.Count > 0 && s_CurrentActor.IsOriginalMonsterType)
			{
				s_CurrentActionSelectedAugmentations.AddRange(s_CurrentActionValidAugmentations);
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.Action);
			break;
		case CPhase.PhaseType.Action:
		{
			s_CurrentActionValidAugmentations.Clear();
			s_CurrentActionSelectedAugmentations.Clear();
			s_RecievedDamagedReducedByShieldItems = 0;
			s_TotalShieldDuringLastAttack = 0;
			if (OverridingCurrentActor)
			{
				EndOverrideCurrentActorForOneAction();
			}
			else if (CurrentOutOfPhaseCardSelectionAbility != null)
			{
				CurrentActor = CurrentOutOfPhaseCardSelectionAbility.TargetingActor;
			}
			else if (TurnActor != null && (CurrentActionInitiator == EActionInitiator.AbilityCard || CurrentActionInitiator == EActionInitiator.None))
			{
				CurrentActor = TurnActor;
			}
			if (s_CurrentActor.Class is CMonsterClass || s_CurrentActor.Class is CHeroSummonClass || !ScenarioManager.Scenario.HasActor(s_CurrentActor))
			{
				if (s_CurrentActor.Class is CCharacterClass)
				{
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
					{
						ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
					}
					else
					{
						EndTurnCheckNextActorAndMoveToNextPhase();
					}
				}
				else
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
				break;
			}
			bool flag3 = false;
			if (RoundAbilityCardSelected?.SelectedAction != null)
			{
				CAbilityCard roundAbilityCardSelected = RoundAbilityCardSelected;
				if (CurrentActionInitiator == EActionInitiator.AbilityCard && !PlayedInfusion)
				{
					ProgressActionSelection(RoundAbilityCardSelected, s_CurrentActor.SelectedRoundCardsCount);
				}
				if (s_CurrentActor != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor is CPlayerActor cPlayerActor3)
				{
					bool flag4 = cPlayerActor3.GetVar("LostAllCards").GetBool();
					bool flag5 = !flag4;
					if (IsPerformingCardOutOfPhase && CurrentSelectAndPlayCardAbility != null)
					{
						if (!PlayedInfusion)
						{
							if (flag5)
							{
								cPlayerActor3.CharacterClass.DiscardOutOfPhasenAbilityCard(RoundAbilityCardSelected);
							}
							flag3 = CurrentSelectAndPlayCardAbility.IncNumCardsPlayed();
						}
					}
					else if (s_CurrentActor.IsTakingExtraTurn)
					{
						if (flag5)
						{
							cPlayerActor3.CharacterClass.DiscardExtraTurnAbilityCard(RoundAbilityCardSelected);
						}
					}
					else
					{
						if (RoundAbilityCardSelected != null && RoundAbilityCardSelected.CurrentCardPile == CBaseCard.ECardPile.Round && flag5)
						{
							cPlayerActor3.CharacterClass.DiscardRoundAbilityCard(RoundAbilityCardSelected);
						}
						if (ExtraRoundAbilityCardselected != null)
						{
							if (flag5)
							{
								cPlayerActor3.CharacterClass.DiscardRoundAbilityCard(ExtraRoundAbilityCardselected);
							}
							ExtraRoundAbilityCardselected = null;
						}
					}
					if (flag4)
					{
						cPlayerActor3.SetTempVar("LostAllCards", false);
					}
					CClass.MarkAbilityActiveBonusesFinished(s_CurrentActor);
				}
				if (roundAbilityCardSelected == RoundAbilityCardSelected && !flag3 && !PlayedInfusion)
				{
					RoundAbilityCardSelected = null;
				}
			}
			CPlayerActor.FinishChangePressure(endTurn: false);
			if (flag3)
			{
				break;
			}
			if (PendingScenarioModifierAbilities.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
				break;
			}
			CurrentActor = TurnActor;
			if (PendingInfusionPhase != null || PlayedInfusion)
			{
				PlayedInfusion = false;
				PhaseManager.SetNextPhase(CPhase.PhaseType.InfuserMechanicSelection);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			}
			break;
		}
		case CPhase.PhaseType.EndTurnLoot:
			PhaseManager.SetNextPhase(CPhase.PhaseType.AfterEndTurnLoot);
			break;
		case CPhase.PhaseType.AfterEndTurnLoot:
			if (PendingScenarioModifierAbilities.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
				break;
			}
			if (CurrentActionInitiator != EActionInitiator.None)
			{
				PhaseManager.EndActiveBonusOrScenarioModifierAbility();
				break;
			}
			if (s_CurrentActor is CPlayerActor cPlayerActor && cPlayerActor.Class.ID == "GeminateID" && cPlayerActor.PendingForm != CPlayerActor.EForm.None)
			{
				if (!cPlayerActor.IgnoreSwitchForm)
				{
					cPlayerActor.Form = cPlayerActor.PendingForm;
					cPlayerActor.PendingForm = CPlayerActor.EForm.None;
				}
				else
				{
					cPlayerActor.IgnoreSwitchForm = false;
				}
			}
			MoveToNextActor();
			break;
		case CPhase.PhaseType.EndTurn:
			foreach (CPlayerActor playerActor4 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor4.ClearCharacterSpecialMechanicsCache(clearAugments: false, clearSongs: false, clearDooms: true);
			}
			if ((PhaseManager.CurrentPhase as CPhaseEndTurn).m_SentEndTurnMessage)
			{
				if (s_CurrentActor.IsTakingExtraTurn)
				{
					if (s_CurrentActor is CPlayerActor cPlayerActor5)
					{
						CAbilityExtraTurn.EExtraTurnType takingExtraTurnOfType = s_CurrentActor.TakingExtraTurnOfType;
						EndExtraTurn(force_cancel: false);
						MarkEndTurnActiveBonusesAsFinished();
						cPlayerActor5.CharacterClass.DiscardRoundAbilityCards(extraTurn: true);
						if (takingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || takingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
						{
							PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurnLoot);
						}
					}
					else
					{
						s_CurrentActor.TakingExtraTurnOfTypeStack.Pop();
						s_CurrentActor.TakingExtraTurnActionMonsterClassIDStack.Pop();
						PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurnLoot);
					}
				}
				else
				{
					LogEndTurn(s_CurrentActor);
					MarkEndTurnActiveBonusesAsFinished();
					if (s_CurrentActor is CPlayerActor cPlayerActor6)
					{
						cPlayerActor6.CharacterClass.DiscardRoundAbilityCards();
						if (cPlayerActor6.UsesResonanceTokens() && cPlayerActor6.Class.ID == "ShattersongID")
						{
							cPlayerActor6.SetResonanceTokens(cPlayerActor6.GetResonanceTokens() + 1);
						}
					}
					CActor cActor = s_CurrentActor;
					if (!cActor.HasBaneThisTurn && cActor.Tokens.HasKey(CCondition.ENegativeCondition.Bane) && !cActor.IsDead && !cActor.IsTakingExtraTurn)
					{
						NegativeConditionPair negativeConditionPair = cActor.Tokens.CheckNegativeTokens.LastOrDefault((NegativeConditionPair x) => x.NegativeCondition == CCondition.ENegativeCondition.Bane);
						cActor.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Bane);
						int health = cActor.Health;
						bool actorWasAsleep = cActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
						if (!(cActor is CEnemyActor { IsImmuneToDamage: not false }))
						{
							CAbility cAbility = CAbilityDamage.CreateDefaultDamage(negativeConditionPair.Strength, 1, 1, isMonster: false, "", isMultipass: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), "GameStateBaneAbility");
							cAbility.MiscAbilityData = new AbilityData.MiscAbilityData
							{
								AutotriggerAbility = true
							};
							PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, cActor));
						}
						CBaneTriggered_MessageData message2 = new CBaneTriggered_MessageData(cActor)
						{
							m_BanedActor = cActor,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
					cActor.HasBaneThisTurn = false;
					foreach (CSpawner spawner3 in ScenarioManager.CurrentScenarioState.Spawners)
					{
						if (spawner3.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
						{
							spawner3.OnEndTurn(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
						}
					}
					cActor.EndTurn();
					CPlayerActor.FinishChangePressure(endTurn: true);
				}
			}
			if (PendingScenarioModifierAbilities.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurnLoot);
			}
			break;
		case CPhase.PhaseType.EndRound:
		{
			CActor.EType lastActorType = ((s_CurrentActor != null) ? s_CurrentActor.Type : ((s_LastActor != null) ? s_LastActor.Type : CActor.EType.Player));
			s_LastActor = null;
			s_CurrentActor = null;
			s_TurnActor = null;
			EndRoundReached = false;
			foreach (CActor key in s_ActorsToOverrideTurns.Keys)
			{
				key.EndMindControl();
			}
			s_ActorsToOverrideTurns.Clear();
			bool strongToWaningEnabled = true;
			foreach (CActor allAliveActor2 in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor2.LastAbilityPerformed = null;
				if (allAliveActor2.FindApplicableActiveBonuses().Any((CActiveBonus bonus) => bonus.Ability.GetMiscVar("ElementsDontWane").GetBool()))
				{
					strongToWaningEnabled = false;
				}
			}
			foreach (CPlayerActor playerActor5 in ScenarioManager.Scenario.PlayerActors)
			{
				CClass.MarkRoundActiveBonusesFinished(playerActor5);
			}
			foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
			{
				CClass.MarkRoundActiveBonusesFinished(exhaustedPlayer);
			}
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				CClass.MarkRoundActiveBonusesFinished(enemy);
			}
			foreach (CEnemyActor deadEnemy in ScenarioManager.Scenario.DeadEnemies)
			{
				CClass.MarkRoundActiveBonusesFinished(deadEnemy);
			}
			foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
			{
				CClass.MarkRoundActiveBonusesFinished(heroSummon2);
			}
			foreach (CHeroSummonActor deadHeroSummon in ScenarioManager.Scenario.DeadHeroSummons)
			{
				CClass.MarkRoundActiveBonusesFinished(deadHeroSummon);
			}
			foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
			{
				CClass.MarkRoundActiveBonusesFinished(enemy2Monster);
			}
			foreach (CEnemyActor deadEnemy2Monster in ScenarioManager.Scenario.DeadEnemy2Monsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadEnemy2Monster);
			}
			foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(allyMonster);
			}
			foreach (CEnemyActor deadAllyMonster in ScenarioManager.Scenario.DeadAllyMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadAllyMonster);
			}
			foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(neutralMonster);
			}
			foreach (CEnemyActor deadNeutralMonster in ScenarioManager.Scenario.DeadNeutralMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadNeutralMonster);
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				CClass.MarkRoundActiveBonusesFinished(@object);
			}
			foreach (CObjectActor deadObject in ScenarioManager.Scenario.DeadObjects)
			{
				CClass.MarkRoundActiveBonusesFinished(deadObject);
			}
			foreach (CPlayerActor playerActor6 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor6.CharacterClass.CheckForFinishedActiveBonuses(playerActor6);
			}
			foreach (CActor allAliveActor3 in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor3.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
			foreach (CPlayerActor playerActor7 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor7.CharacterClass.CheckAttackModifierCardShuffle();
			}
			foreach (CPlayerActor playerActor8 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor8.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Clear();
				playerActor8.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Clear();
			}
			MonsterClassManager.DiscardRoundAbilityCards();
			MonsterClassManager.CheckAllAttackModifierDecksForShuffle();
			foreach (CSpawner spawner4 in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner4.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner4.OnEndRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			ElementInfusionBoardManager.EndRound(lastActorType, strongToWaningEnabled);
			ScenarioManager.CurrentScenarioState.RoundNumber++;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			NextRound();
			break;
		}
		case CPhase.PhaseType.Count:
		case CPhase.PhaseType.None:
			break;
		}
	}

	private static void MarkEndTurnActiveBonusesAsFinished()
	{
		if (InternalCurrentActor != null && !InternalCurrentActor.HasPendingExtraTurn)
		{
			InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Turns);
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CClass.MarkTurnActiveBonusesFinished(playerActor);
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			CClass.MarkTurnActiveBonusesFinished(exhaustedPlayer);
		}
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			CClass.MarkTurnActiveBonusesFinished(enemy);
		}
		foreach (CEnemyActor deadEnemy in ScenarioManager.Scenario.DeadEnemies)
		{
			CClass.MarkTurnActiveBonusesFinished(deadEnemy);
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			CClass.MarkTurnActiveBonusesFinished(heroSummon);
		}
		foreach (CHeroSummonActor deadHeroSummon in ScenarioManager.Scenario.DeadHeroSummons)
		{
			CClass.MarkTurnActiveBonusesFinished(deadHeroSummon);
		}
		foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
		{
			CClass.MarkTurnActiveBonusesFinished(enemy2Monster);
		}
		foreach (CEnemyActor deadEnemy2Monster in ScenarioManager.Scenario.DeadEnemy2Monsters)
		{
			CClass.MarkTurnActiveBonusesFinished(deadEnemy2Monster);
		}
		foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
		{
			CClass.MarkTurnActiveBonusesFinished(allyMonster);
		}
		foreach (CEnemyActor deadAllyMonster in ScenarioManager.Scenario.DeadAllyMonsters)
		{
			CClass.MarkTurnActiveBonusesFinished(deadAllyMonster);
		}
		foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
		{
			CClass.MarkTurnActiveBonusesFinished(neutralMonster);
		}
		foreach (CEnemyActor deadNeutralMonster in ScenarioManager.Scenario.DeadNeutralMonsters)
		{
			CClass.MarkTurnActiveBonusesFinished(deadNeutralMonster);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			CClass.MarkTurnActiveBonusesFinished(@object);
		}
		foreach (CObjectActor deadObject in ScenarioManager.Scenario.DeadObjects)
		{
			CClass.MarkTurnActiveBonusesFinished(deadObject);
		}
	}

	public static void ReportEndTurn(CPlayerActor player)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorEndTurn(player.ArrayIndex.X, player.ArrayIndex.Y, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorEndTurn, player)));
	}

	public static void ReportStartTurn(CActor actor)
	{
		if (actor is CPlayerActor cPlayerActor && ScenarioManager.CurrentScenarioState?.Stats?.m_PlayerStats != null)
		{
			if (ScenarioManager.CurrentScenarioState.Stats.m_PlayerStats[cPlayerActor.Class.ID].StartTurn.Any((CPlayerStatsStartTurn x) => x.Round == ScenarioManager.CurrentScenarioState.RoundNumber))
			{
				return;
			}
			CPlayerActor.LogHeroSummonMoveChanged(cPlayerActor);
		}
		else
		{
			CHeroSummonActor heroSummon = actor as CHeroSummonActor;
			if (heroSummon != null && ScenarioManager.CurrentScenarioState?.Stats?.m_HeroSummonStats != null)
			{
				if (ScenarioManager.CurrentScenarioState.Stats.m_HeroSummonStats.StartTurn.Any((CPlayerStatsStartTurn x) => x.Round == ScenarioManager.CurrentScenarioState.RoundNumber && x.MainActorGUID == heroSummon.ActorGuid))
				{
					return;
				}
				CPlayerActor.LogHeroSummonMoveChanged(heroSummon);
			}
			else
			{
				CEnemyActor enemy = actor as CEnemyActor;
				if (enemy != null && ScenarioManager.CurrentScenarioState?.Stats?.m_MonsterStats != null && ScenarioManager.CurrentScenarioState.Stats.m_MonsterStats.StartTurn.Any((CPlayerStatsStartTurn x) => x.Round == ScenarioManager.CurrentScenarioState.RoundNumber && x.MainActorGUID == enemy.ActorGuid))
				{
					return;
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorStartTurn(actor.ArrayIndex.X, actor.ArrayIndex.Y, (actor?.CharacterClass != null) ? actor.CharacterClass.RoundAbilityCards.Select((CAbilityCard card) => card.Initiative).ToList() : null, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorStartTurn, actor)));
	}

	public static void GetAdjacency(CActor player, out bool wall, out int obstacles, out int allies, out int enemies, out int players)
	{
		wall = false;
		obstacles = (allies = (enemies = (players = 0)));
		if (player != null)
		{
			wall = CAbilityFilter.WallIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y]);
			obstacles = CAbilityFilter.ObstacleIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y]);
			allies = CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y], player, CActor.EType.Ally);
			enemies = CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y], player, CActor.EType.Enemy);
			players = CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y], player, CActor.EType.Player);
		}
	}

	public static List<CActor> GetAdjacentEnemies(CActor player, Point arrayIndex)
	{
		if (player != null)
		{
			CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], player, CActor.EType.Enemy, out var adjacent);
			return adjacent;
		}
		return new List<CActor>();
	}

	public static List<CActor> GetAdjacentEnemies(CActor player, CActor appliedActor)
	{
		if (player != null)
		{
			List<CActor> list = new List<CActor>();
			foreach (CTile item in appliedActor.GetTilesCanAttackFrom())
			{
				CAbilityFilter.ActorIsAdjacent(item, player, CActor.EType.Enemy, out var adjacent);
				list.AddRange(adjacent);
			}
			return list.Distinct().ToList();
		}
		return new List<CActor>();
	}

	public static List<CTile> GetAdjacentLootTiles(CActor player, Point arrayIndex)
	{
		if (player != null)
		{
			List<CTile> list = new List<CTile>();
			GetAdjacentTiles(arrayIndex, list, clear: false);
			return list.FindAll((CTile t) => CAbilityFilter.IsValidTile(t, CAbilityFilter.EFilterTile.Loot));
		}
		return new List<CTile>();
	}

	public static bool LostAdjacency(CActor actor, Point firstPoint, Point secondPoint, CAbility ability = null, bool wasForced = false)
	{
		if (actor != null && actor is CPlayerActor && firstPoint != secondPoint)
		{
			bool flag = GetAdjacentEnemies(actor, firstPoint).Count > 0;
			if (flag)
			{
				if (ability != null)
				{
					wasForced = ability.TargetingActor != actor || actor.MindControlAbility != null;
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventLostAdjacency(actor.Class.ID, wasForced));
			}
			return flag;
		}
		return false;
	}

	public static bool LostAdjacency(CActor actor, CTile firstTile, CTile secondTile, CAbility ability = null, bool wasForced = false)
	{
		if (actor != null && firstTile != null && secondTile != null)
		{
			return LostAdjacency(actor, new Point(firstTile), new Point(secondTile), ability, wasForced);
		}
		return false;
	}

	public static void CheckForFinishedActiveBonuses(bool important = false)
	{
		if (!ScenarioRuleClient.IsWorkerThread)
		{
			ScenarioRuleClient.CheckFinishedBonuses(null, important);
			return;
		}
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			allPlayer.CharacterClass.CheckForFinishedActiveBonuses(allPlayer, important);
		}
	}

	public static void CheckAllPressurePlates()
	{
		List<Tuple<CObjectPressurePlate, CActor>> list = new List<Tuple<CObjectPressurePlate, CActor>>();
		List<CObjectPressurePlate> list2 = new List<CObjectPressurePlate>();
		foreach (CObjectPressurePlate item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectPressurePlate>().ToList())
		{
			if (item.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				CTile cTile = ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y];
				CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
				if ((cActor?.Class is CCharacterClass) | (cActor?.MonsterClass != null && cActor.MonsterClass.CanActivatePressurePlates))
				{
					list.Add(new Tuple<CObjectPressurePlate, CActor>(item, cActor));
				}
				else
				{
					list2.Add(item);
				}
			}
		}
		foreach (Tuple<CObjectPressurePlate, CActor> item2 in list)
		{
			item2.Item1.Activate(item2.Item2);
		}
		foreach (CObjectPressurePlate item3 in list2)
		{
			item3.Deactivate();
		}
	}

	public static void RecoverDiscardedCards(CAbilityCard card, CPlayerActor playerActor)
	{
		RecoverDiscardedCards(new List<CAbilityCard> { card }, playerActor);
	}

	public static void RecoverDiscardedCards(List<CAbilityCard> cards, CPlayerActor playerActor)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		CBaseCard.ECardPile pile = RecoverCardPile(playerActor);
		List<CAbilityCard> cardPileList = characterClass.GetCardPileList(pile);
		string cardPileListKey = CCharacterClass.GetCardPileListKey(pile);
		foreach (CAbilityCard item in cards.ToList())
		{
			playerActor.CharacterClass.MoveAbilityCard(item, characterClass.DiscardedAbilityCards, cardPileList, "DiscardedAbilityCards", cardPileListKey);
		}
	}

	public static void RecoverLostCards(CAbilityCard card, CPlayerActor playerActor)
	{
		RecoverLostCards(new List<CAbilityCard> { card }, playerActor);
	}

	public static void RecoverLostCards(List<CAbilityCard> cards, CPlayerActor playerActor)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		CBaseCard.ECardPile pile = RecoverCardPile(playerActor);
		List<CAbilityCard> cardPileList = characterClass.GetCardPileList(pile);
		string cardPileListKey = CCharacterClass.GetCardPileListKey(pile);
		foreach (CAbilityCard item in cards.ToList())
		{
			playerActor.CharacterClass.MoveAbilityCard(item, characterClass.LostAbilityCards, cardPileList, "LostAbilityCards", cardPileListKey);
		}
	}

	public static void PlayerLongRested(CAbilityCard abilityCard, string fastReloadID, CPlayerActor playerActor = null, bool improvedShortRestUsed = false, bool fromStateUpdate = false)
	{
		if (playerActor == null)
		{
			playerActor = (CPlayerActor)s_CurrentActor;
		}
		CCharacterClass characterClass = playerActor.CharacterClass;
		characterClass.HasStartedLongRest = false;
		characterClass.HasQueuedPreRestBonuses = false;
		int valueOrDefault = (playerActor?.CharacterClass?.HandAbilityCards?.Count).GetValueOrDefault();
		int valueOrDefault2 = (playerActor?.CharacterClass?.DiscardedAbilityCards?.Count).GetValueOrDefault();
		if (!improvedShortRestUsed)
		{
			s_ActionFlagsByActionSequenceType[0] = EActionFlag.TopPlayed | EActionFlag.BottomPlayed;
			s_ActionFlagsByActionSequenceType[1] = EActionFlag.TopPlayed | EActionFlag.BottomPlayed;
			if (playerActor.MaxRoundCardsCount > 2)
			{
				s_ActionFlagsByActionSequenceType[2] = EActionFlag.TopPlayed | EActionFlag.BottomPlayed;
			}
			characterClass.HasLongRested = true;
		}
		else
		{
			characterClass.ShortRest = false;
			characterClass.HasShortRested = true;
			characterClass.HasImprovedShortRested = true;
			characterClass.LongRest = false;
		}
		bool improvedShortRest = playerActor.CharacterClass.ImprovedShortRest;
		if (improvedShortRest)
		{
			CPlayerActor mainActor = playerActor;
			int cardsInHand = valueOrDefault;
			int cardsInDiscard = valueOrDefault2;
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(mainActor, CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, null, null, hasHappened: true, CAbility.EAbilityFlag.None, default, DiscardType.None, null, null, isBuff: false, cardsInHand, cardsInDiscard)));
		}
		else
		{
			CPlayerActor mainActor2 = playerActor;
			int cardsInDiscard = valueOrDefault;
			int cardsInHand = valueOrDefault2;
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(mainActor2, CAbility.EAbilityType.LongRest, ESESubTypeAbility.AbilityEnded, null, null, null, hasHappened: true, CAbility.EAbilityFlag.None, default, DiscardType.None, null, null, isBuff: false, cardsInDiscard, cardsInHand)));
		}
		int health = playerActor.Health;
		if (abilityCard != null)
		{
			playerActor.CharacterClass.MoveAbilityCard(abilityCard, characterClass.DiscardedAbilityCards, characterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		}
		RecoverDiscardedCards(characterClass.DiscardedAbilityCards, playerActor);
		if (improvedShortRest)
		{
			foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterShortRest))
			{
				item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
		}
		if (!improvedShortRest)
		{
			playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
		}
		else
		{
			playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.AfterShortRest);
		}
		playerActor.CharacterClass.LongRest = false;
		playerActor.CharacterClass.ImprovedShortRest = false;
		playerActor.CharacterClass.ImprovedShortRestDrawCount = 0u;
		if (improvedShortRest)
		{
			playerActor.CharacterClass.ShortRestCardBurned = abilityCard.CardInstanceID;
			if (!fromStateUpdate)
			{
				CReplicateStartRoundCardState_MessageData message = new CReplicateStartRoundCardState_MessageData(playerActor, 51, playerActor.CharacterClass.GetCurrentCardState(fastReloadID));
				ScenarioRuleClient.MessageHandler(message);
			}
			if (playerActor.Class.ID == "GeminateID")
			{
				if (playerActor.Form != CPlayerActor.EForm.Choose)
				{
					playerActor.PendingForm = playerActor.Form;
				}
				playerActor.Form = CPlayerActor.EForm.Choose;
			}
			PhaseManager.StepComplete();
		}
		else
		{
			CAbility cAbility = CAbilityHeal.CreateDefaultHeal(2, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData
			{
				AutotriggerAbility = true,
				IgnoreStun = true
			};
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
			playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Spent);
			playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Locked, CItem.EItemSlot.None, CItem.EUsageType.Spent);
			playerActor.ActivatePassiveItems(firstLoad: true);
			playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.AtEndOfTurn, CItem.EItemTrigger.DuringOwnTurn);
			ScenarioRuleClient.MessageHandler(new CPlayerLongRested_MessageData(health, abilityCard, playerActor));
			PendingOnLongRestBonuses.Clear();
			PendingOnLongRestBonuses.AddRange(from x in playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnLongRest)
				where !x.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PreRestBonus) && x.RequirementsMet(playerActor)
				select x);
			PendingOnLongRestBonuses.AddRange(from x in playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnActionEndedAndLongRest)
				where !x.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PreRestBonus) && x.RequirementsMet(playerActor)
				select x);
			if (playerActor.Class.ID == "GeminateID")
			{
				playerActor.PendingForm = CPlayerActor.EForm.Choose;
			}
			List<CAbility> list = (from x in playerActor.CharacterClass.Perks.Where((PerksYMLData p) => (p.Flags & EPerkFlag.ExecuteAbilitiesAfterLongRest) != 0).SelectMany((PerksYMLData p) => p.Abilities)
				select CAbility.CopyAbility(x, generateNewID: true) into ability
				where ability.MeetsStartAbilityRequirements(playerActor)
				select ability).ToList();
			if (list.Count > 0)
			{
				((CPhaseAction)PhaseManager.CurrentPhase).StackNextAbilities(list, playerActor, killAfter: false, stackToNextCurrent: true);
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				scenarioScript.OnLongRestComplete(playerActor);
			}
		}
		CChallenge.CheckTrigger(CChallenge.Trigger.OnRest, new CChallenge.TriggerParams
		{
			actor = playerActor
		});
	}

	public static bool TriggerAnyOnRestAbilities()
	{
		bool result = false;
		if (PendingOnRestAbilities.Count > 0)
		{
			List<(CAbility, CActor)> list = new List<(CAbility, CActor)>(PendingOnRestAbilities);
			if (PhaseManager.CurrentPhaseAction == null)
			{
				(CAbility, CActor) tuple = list[0];
				list.RemoveAt(0);
				OverrideCurrentActorForOneAction(tuple.Item2, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				PhaseManager.StartAbility(tuple.Item1, tuple.Item1.GetAbilityBaseCard());
			}
			foreach (var item in list)
			{
				PhaseManager.CurrentPhaseAction.InjectAbility(item.Item1, item.Item2, item.Item1.GetAbilityBaseCard());
			}
			PendingOnRestAbilities.Clear();
			result = true;
		}
		return result;
	}

	public static bool TriggerAnyOnLongRestAddActiveBonuses()
	{
		bool flag = false;
		if (PendingOnLongRestBonuses.Count > 0)
		{
			foreach (CActiveBonus pendingOnLongRestBonuse in PendingOnLongRestBonuses)
			{
				if (pendingOnLongRestBonuse.TryGetBehaviour(out CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest behaviour))
				{
					flag |= behaviour.TriggerAbility();
				}
				if (pendingOnLongRestBonuse.TryGetBehaviour(out CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEndedAndLongRest behaviour2))
				{
					flag |= behaviour2.TriggerAbility();
				}
			}
			PendingOnLongRestBonuses.Clear();
		}
		return flag | TriggerAnyOnRestAbilities();
	}

	public static bool CheckExhaustAfterShortRest(CPlayerActor playerActor, bool isShortRestingPlayer)
	{
		if (playerActor == null)
		{
			return false;
		}
		if (playerActor.IsDead)
		{
			PendingOnShortRestBonuses.RemoveAll((CActiveBonus x) => x.Actor == playerActor && !x.Ability.ProcessIfDead);
			PendingOnRestAbilities.RemoveAll(((CAbility ability, CActor actor) x) => x.actor == playerActor && !x.ability.ProcessIfDead);
			return true;
		}
		if (playerActor.IsExhausted())
		{
			bool actorWasAsleep = playerActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			CClass.CancelAllActiveBonusesOnDeath(playerActor);
			KillActor(playerActor, playerActor, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
			CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(playerActor);
			cPlayersExhausted_MessageData.m_Players = new List<CPlayerActor> { playerActor };
			ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
			return true;
		}
		if (isShortRestingPlayer)
		{
			QueuePostShortRestBonuses(playerActor);
			List<CAbility> list = playerActor.CharacterClass.Perks.Where((PerksYMLData p) => (p.Flags & EPerkFlag.ExecuteAbilitiesAfterShortRest) != 0).SelectMany((PerksYMLData p) => p.Abilities).ToList();
			if (list.Count > 0)
			{
				foreach (CAbility item in list)
				{
					CAbility cAbility = CAbility.CopyAbility(item, generateNewID: false);
					cAbility.IsScenarioModifierAbility = true;
					PendingOnRestAbilities.Add((cAbility, playerActor));
				}
			}
		}
		return false;
	}

	public static void QueuePreRestBonuses(CPlayerActor playerActor)
	{
		playerActor.CharacterClass.HasQueuedPreRestBonuses = true;
		PendingOnShortRestBonuses.Clear();
		PendingOnLongRestBonuses.Clear();
		if (s_ShortRestingPlayer != null)
		{
			List<CActiveBonus> collection = (from x in playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnShortRest)
				where x.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PreRestBonus) && x.RequirementsMet(playerActor)
				select x).ToList();
			PendingOnShortRestBonuses.AddRange(collection);
		}
		else
		{
			List<CActiveBonus> collection2 = (from x in playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnLongRest)
				where x.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PreRestBonus) && x.RequirementsMet(playerActor)
				select x).ToList();
			PendingOnLongRestBonuses.AddRange(collection2);
		}
		playerActor.TideActiveBonusesCountBeforeRest = CustomAbility.GetTideActiveBonuses(playerActor).Count;
	}

	public static void QueuePostShortRestBonuses(CPlayerActor playerActor)
	{
		PendingOnShortRestBonuses.Clear();
		PendingOnLongRestBonuses.Clear();
		List<CActiveBonus> collection = (from x in playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnShortRest)
			where !x.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PreRestBonus) && x.RequirementsMet(playerActor)
			select x).ToList();
		PendingOnShortRestBonuses.AddRange(collection);
	}

	public static void PlayerShortRested(CPlayerActor playerActor, CAbilityCard discardedAbilityCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate = false)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		if (characterClass.HasShortRested & fromStateUpdate)
		{
			Log.SimpleInfo("Returning early from short rest triggered by a state update as " + playerActor.ActorLocKey() + " has already short rested");
			return;
		}
		int valueOrDefault = (playerActor?.CharacterClass?.HandAbilityCards?.Count).GetValueOrDefault();
		int valueOrDefault2 = (playerActor?.CharacterClass?.DiscardedAbilityCards?.Count).GetValueOrDefault();
		characterClass.HasQueuedPreRestBonuses = false;
		characterClass.ShortRest = false;
		characterClass.HasShortRested = true;
		characterClass.ShortRestCardBurned = discardedAbilityCard?.CardInstanceID ?? 0;
		characterClass.ShortRestCardRedrawn = loseHealth;
		characterClass.LongRest = false;
		if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle && !ScenarioManager.CurrentScenarioState.FreeShortRest)
		{
			playerActor.ApplyCondition(playerActor, CCondition.ENegativeCondition.Muddle, 0, EConditionDecTrigger.Turns);
		}
		if (discardedAbilityCard != null)
		{
			playerActor.CharacterClass.MoveAbilityCard(discardedAbilityCard, characterClass.DiscardedAbilityCards, characterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		}
		RecoverDiscardedCards(characterClass.DiscardedAbilityCards, playerActor);
		playerActor.CharacterClass.ExcludedFromShortRestAbilityCards.Clear();
		playerActor.CharacterClass.ImprovedShortRestCanExcludeCards = false;
		foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterShortRest))
		{
			item.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, playerActor);
		}
		if (updateScenarioRNG)
		{
			ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
			if (loseHealth)
			{
				ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
			}
		}
		if (loseHealth)
		{
			List<PositiveConditionPair> checkPositiveTokens = playerActor.Tokens.CheckPositiveTokens;
			int health = playerActor.Health;
			ActorBeenDamaged(playerActor, 1, !fromStateUpdate, playerActor, null, CAbility.EAbilityType.ShortRest);
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorDamaged(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorDamaged, playerActor), health - playerActor.Health, CActor.ECauseOfDamage.Self, 0, 0, 0, isMelee: false, 0, 0, checkPositiveTokens, health));
		}
		CChallenge.CheckTrigger(CChallenge.Trigger.OnRest, new CChallenge.TriggerParams
		{
			actor = playerActor
		});
		ShowItemSelection(playerActor, playerActor, null, CItem.EItemTrigger.AfterShortRest, onlyInActionPhase: false);
		CPlayerShortRested_MessageData message = new CPlayerShortRested_MessageData(playerActor, discardedAbilityCard, playerActor);
		ScenarioRuleClient.MessageHandler(message);
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript.OnShortRestComplete(playerActor);
		}
		int cardsInHand = valueOrDefault;
		int cardsInDiscard = valueOrDefault2;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(playerActor, CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, null, null, hasHappened: true, CAbility.EAbilityFlag.None, default, DiscardType.None, null, null, isBuff: false, cardsInHand, cardsInDiscard)));
		CheckExhaustAfterShortRest(playerActor, isShortRestingPlayer: true);
		if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
		{
			PhaseManager.StartActiveBonusOrScenarioModifierAbility();
		}
	}

	public static bool TriggerAnyOnShortRestAddActiveBonuses(CPlayerActor playerActor)
	{
		bool flag = false;
		if (PendingOnShortRestBonuses.Count > 0)
		{
			foreach (CActiveBonus item in PendingOnShortRestBonuses.ToList())
			{
				if (item.TryGetBehaviour(out CDuringActionAbilityActiveBonus_TriggerAbilityOnShortRest behaviour))
				{
					flag |= behaviour.TriggerAbility(playerActor);
				}
			}
		}
		return flag | TriggerAnyOnRestAbilities();
	}

	public static void PlayerSelectedCardsToRecover(CPlayerActor playerActor, List<CAbilityCard> recoveredAbilityCards, CBaseCard.ECardPile recoveredCardPile)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard recoveredAbilityCard in recoveredAbilityCards)
		{
			if (characterClass.DiscardedAbilityCards.Contains(recoveredAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(recoveredAbilityCard, characterClass.DiscardedAbilityCards, characterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
			}
			else if (characterClass.LostAbilityCards.Contains(recoveredAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(recoveredAbilityCard, characterClass.LostAbilityCards, characterClass.HandAbilityCards, "LostAbilityCards", "HandAbilityCards");
			}
			else
			{
				Log.ErrorWithReport("Card to recover could not be found in DiscardedAbilityCards or LostAbilityCards Lists");
			}
		}
		if (recoveredCardPile != CBaseCard.ECardPile.None)
		{
			characterClass.LastRecoveredAbilityCards = recoveredAbilityCards;
			CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
			cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
			cMoveSelectedCards.m_MoveFromPile = recoveredCardPile;
			cMoveSelectedCards.m_quantity = recoveredAbilityCards.Count;
			cMoveSelectedCards.m_Cards = recoveredAbilityCards;
			ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
		}
	}

	public static void PlayerSelectedCardsToLose(CPlayerActor playerActor, List<CAbilityCard> losingAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard losingAbilityCard in losingAbilityCards)
		{
			List<CAbilityCard> toAbilityCardList = characterClass.LostAbilityCards;
			string toCardPileName = "LostAbilityCards";
			if (losingAbilityCard.GetAbilityCardYML.TopIsPermLost || losingAbilityCard.GetAbilityCardYML.BottomIsPermLost)
			{
				toAbilityCardList = characterClass.PermanentlyLostAbilityCards;
				toCardPileName = "PermanentlyLostAbilityCards";
			}
			if (characterClass.HandAbilityCards.Contains(losingAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(losingAbilityCard, characterClass.HandAbilityCards, toAbilityCardList, "HandAbilityCards", toCardPileName);
			}
			else if (characterClass.DiscardedAbilityCards.Contains(losingAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(losingAbilityCard, characterClass.DiscardedAbilityCards, toAbilityCardList, "DiscardedAbilityCards", toCardPileName);
			}
			else
			{
				Log.ErrorWithReport("Card to lose could not be found in HandAbilityCards or DiscardedAbilityCards Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.Hand;
		cMoveSelectedCards.m_MoveToPile = CBaseCard.ECardPile.Lost;
		cMoveSelectedCards.m_quantity = losingAbilityCards.Count;
		cMoveSelectedCards.m_Cards = losingAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedCardsToDiscard(CPlayerActor playerActor, List<CAbilityCard> losingAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard losingAbilityCard in losingAbilityCards)
		{
			if (characterClass.HandAbilityCards.Contains(losingAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(losingAbilityCard, characterClass.HandAbilityCards, characterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
			}
			else
			{
				Log.ErrorWithReport("Card to lose could not be found in HandAbilityCards Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.Hand;
		cMoveSelectedCards.m_MoveToPile = CBaseCard.ECardPile.Discarded;
		cMoveSelectedCards.m_quantity = losingAbilityCards.Count;
		cMoveSelectedCards.m_Cards = losingAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedCardsToMoveActiveBonusTracker(CPlayerActor playerActor, List<CAbilityCard> cards, int moveStrength, CAbilityMoveActiveBonusTracker moveTrackerAbility)
	{
		foreach (CActiveBonus activeBonusesWithMovableTracker in CAbilityMoveActiveBonusTracker.GetActiveBonusesWithMovableTrackers(playerActor, moveStrength))
		{
			foreach (CAbilityCard card in cards)
			{
				if (activeBonusesWithMovableTracker.BaseCard != card)
				{
					continue;
				}
				if (moveStrength > 0)
				{
					int num = 0;
					int trackerIndex = activeBonusesWithMovableTracker.TrackerIndex;
					int num2 = activeBonusesWithMovableTracker.TrackerIndex + moveStrength;
					if (num2 > activeBonusesWithMovableTracker.TrackerMaxIndex)
					{
						num2 = activeBonusesWithMovableTracker.TrackerMaxIndex + 1;
					}
					List<int> tracker = activeBonusesWithMovableTracker.Tracker;
					for (int i = trackerIndex; i < num2; i++)
					{
						num += tracker[i];
					}
					playerActor.GainXP(num, activeBonusesWithMovableTracker.Ability.XPSourceKey);
				}
				activeBonusesWithMovableTracker.TrackerIndex += moveStrength;
				if (activeBonusesWithMovableTracker.Remaining <= 0)
				{
					activeBonusesWithMovableTracker.Finish();
				}
			}
		}
		CMoveActiveBonusTrackers cMoveActiveBonusTrackers = new CMoveActiveBonusTrackers(playerActor);
		cMoveActiveBonusTrackers.m_Actor = playerActor;
		cMoveActiveBonusTrackers.m_Cards = cards;
		cMoveActiveBonusTrackers.m_Ability = moveTrackerAbility;
		ScenarioRuleClient.MessageHandler(cMoveActiveBonusTrackers);
	}

	public static void PlayerSelectedCardsToIncreaseLimit(CPlayerActor playerActor, List<CAbilityCard> increasingLimitAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard increasingLimitAbilityCard in increasingLimitAbilityCards)
		{
			if (characterClass.AbilityCardsPool.Contains(increasingLimitAbilityCard))
			{
				playerActor.CharacterClass.SelectedAbilityCards.Add(increasingLimitAbilityCard);
				playerActor.CharacterClass.HandAbilityCards.Add(increasingLimitAbilityCard);
			}
			else
			{
				Log.ErrorWithReport("Card to increase limit by could not be found in AbilityCardsPool Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.None;
		cMoveSelectedCards.m_quantity = increasingLimitAbilityCards.Count;
		cMoveSelectedCards.m_Cards = increasingLimitAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedExtraTurnCards(CPlayerActor playerActor, List<CAbilityCard> extraTurnCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		List<CAbilityCard> list = null;
		string text = null;
		if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
		{
			list = new List<CAbilityCard>();
			text = "PendingExtraTurnCards";
			characterClass.PendingExtraTurnCardsData.Add(new CAbilityExtraTurn.PendingExtraTurnData(characterClass.ExtraTurnInitiativeAbilityCard.Initiative, list));
			characterClass.PendingExtraTurnCardsData = characterClass.PendingExtraTurnCardsData.OrderBy((CAbilityExtraTurn.PendingExtraTurnData x) => x.LeadingInitiative).ToList();
		}
		else
		{
			list = characterClass.ExtraTurnCards;
			text = "ExtraTurnCards";
		}
		list.Clear();
		foreach (CAbilityCard extraTurnCard in extraTurnCards)
		{
			if (characterClass.AbilityCardsPool.Contains(extraTurnCard) || characterClass.GivenCards.Contains(extraTurnCard))
			{
				Log.SimpleInfo(playerActor.Class.ID + " chose " + extraTurnCard.Name + " (Intiative " + extraTurnCard.Initiative + ") for extra turn");
				playerActor.CharacterClass.MoveAbilityCard(extraTurnCard, characterClass.HandAbilityCards, list, "HandAbilityCards", text);
			}
			else
			{
				Log.ErrorWithReport("Card to use for Extra Turn could not be found in AbilityCardsPool Lists");
			}
		}
		if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
		{
			if (!playerActor.HasPendingExtraTurn && !playerActor.IsTakingExtraTurn)
			{
				s_InitiativeSortedActors.Remove(playerActor);
				bool flag = false;
				foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
				{
					if (s_ActorInitiativeComparer.Compare(playerActor, s_InitiativeSortedActor) < 0)
					{
						int index = s_InitiativeSortedActors.IndexOf(s_InitiativeSortedActor);
						s_InitiativeSortedActors.Insert(index, playerActor);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					s_InitiativeSortedActors.Add(playerActor);
				}
			}
			playerActor.PendingExtraTurnOfTypeStack.Push(playerActor.SelectingCardsForExtraTurnOfType);
			playerActor.PendingExtraTurnActionMonsterClassIDStack.Push(string.Empty);
			playerActor.SelectingCardsForExtraTurnOfType = CAbilityExtraTurn.EExtraTurnType.None;
			PlayerFinishedMovingCards(playerActor);
		}
		else
		{
			StartActorExtraTurnImmediately(playerActor);
			CExtraTurnCardsSelected message = new CExtraTurnCardsSelected(playerActor);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void PlayerFinishedMovingCards(CPlayerActor playerActor)
	{
		CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(null)
		{
			WaitActor = playerActor,
			WaitTickFrame = 10000,
			ClearEvents = false
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void NextRound()
	{
		s_RoundCount++;
		CheckObjectives();
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			allActor.NextRound();
		}
		CNextRound_MessageData message = new CNextRound_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
		PhaseManager.SetNextPhase(CPhase.PhaseType.StartRoundEffects);
	}

	public static List<CActor> ActorListOnly(List<CRangeSortedActor> rangedSortedActors, bool sortForAttack = false, bool shortestOnly = false, bool skipUntargetableCheck = false)
	{
		rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Actor.SubInitiative().CompareTo(y.m_Actor.SubInitiative()));
		if (sortForAttack)
		{
			rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => UpdatedInitiativeWithSummons(x.m_Actor, x.m_Actor.Initiative()).CompareTo(UpdatedInitiativeWithSummons(y.m_Actor, y.m_Actor.Initiative())));
		}
		else
		{
			rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Actor.Initiative().CompareTo(y.m_Actor.Initiative()));
		}
		rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Range.CompareTo(y.m_Range));
		if (shortestOnly)
		{
			rangedSortedActors.RemoveAll((CRangeSortedActor x) => x.m_Range > rangedSortedActors[0].m_Range);
		}
		rangedSortedActors.RemoveAll((CRangeSortedActor x) => x.m_Actor.Untargetable && !skipUntargetableCheck && !x.m_Self);
		return rangedSortedActors.Select((CRangeSortedActor s) => s.m_Actor).Distinct().ToList();
	}

	public static List<CActor> GetActorsInRange(Point origin, int range)
	{
		return GetActorsOnTiles(GetTilesInRange(origin, range));
	}

	public static List<CActor> GetAllAlliesInRange(CActor actor, Point origin, int range, bool includeObjects = false)
	{
		return (from x in GetActorsOnTiles(GetTilesInRange(origin, range))
			where CustomAbility.IsAlly(actor, x, checkOriginalType: false, includeObjects)
			select x).ToList();
	}

	public static List<CActor> GetAllEnemiesInRange(CActor actor, Point origin, int range)
	{
		return (from x in GetActorsOnTiles(GetTilesInRange(origin, range))
			where CustomAbility.IsEnemy(actor, x)
			select x).ToList();
	}

	public static List<CActor> GetActorsInRange(CActor targetingActor, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, CTile lastSelectedTile = null, bool? canTargetInvisible = false, bool originalTargetType = false, bool skipUntargetableCheck = false, bool ignoreLOS = false, bool useRealPositions = true, bool freeIce = true)
	{
		Point arrayIndex = targetingActor.ArrayIndex;
		bool isTargetedAbility2 = isTargetedAbility;
		bool originalTargetType2 = originalTargetType;
		return GetActorsInRange(arrayIndex, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, areaEffectTiles, isTargetedAbility2, lastSelectedTile, canTargetInvisible, skipUntargetableCheck, originalTargetType2, ignoreLOS, useRealPositions, freeIce);
	}

	public static List<CActor> GetActorsInRange(Point arrayIndex, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, CTile lastSelectedTile = null, bool? canTargetInvisible = false, bool skipUntargetableCheck = false, bool originalTargetType = false, bool ignoreLOS = false, bool useRealPositions = true, bool freeIce = true)
	{
		List<CRangeSortedActor> list = new List<CRangeSortedActor>();
		if (abilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !abilityFilter.HasNonTargetTypeFilters())
		{
			if (actorsToIgnore == null || !actorsToIgnore.Contains(filterActor))
			{
				list.Add(new CRangeSortedActor(filterActor, 0, self: true));
			}
			return ActorListOnly(list);
		}
		if (areaEffect != null)
		{
			if (areaEffectTiles == null)
			{
				Log.ErrorWithReport("AreaEffect sent without tile set to GetActorsInRange.");
				return ActorListOnly(list);
			}
			foreach (CTile areaEffectTile in areaEffectTiles)
			{
				CActor actorOnTile = GetActorOnTile(areaEffectTile, filterActor, abilityFilter, actorsToIgnore, isTargetedAbility, canTargetInvisible, originalTargetType: false, skipUntargetableCheck: false, useRealPositions);
				if (actorOnTile != null && !actorOnTile.IsDead)
				{
					List<Point> list2 = ScenarioManager.PathFinder.FindPath(filterActor, arrayIndex, areaEffectTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
					if (foundPath && (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], areaEffectTile)))
					{
						list.Add(new CRangeSortedActor(actorOnTile, list2.Count));
					}
				}
			}
			return ActorListOnly(list);
		}
		Point startLocation = lastSelectedTile?.m_ArrayIndex ?? arrayIndex;
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(startLocation.X, startLocation.Y, ScenarioManager.EAdjacentPosition.ECenter);
		List<CTile> list3 = new List<CTile>();
		for (int i = startLocation.Y - range; i <= startLocation.Y + range; i++)
		{
			for (int j = startLocation.X - range; j <= startLocation.X + range; j++)
			{
				CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile2 != null)
				{
					CObjectDoor cObjectDoor = (CObjectDoor)adjacentTile2.FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectDoor != null && cObjectDoor.Activated)
					{
						list3.Add(adjacentTile2);
					}
				}
			}
		}
		for (int k = startLocation.Y - range; k <= startLocation.Y + range; k++)
		{
			for (int l = startLocation.X - range; l <= startLocation.X + range; l++)
			{
				CTile adjacentTile3 = ScenarioManager.GetAdjacentTile(l, k, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile3 == null)
				{
					continue;
				}
				List<CActor> actorsOnTile = GetActorsOnTile(adjacentTile3, filterActor, abilityFilter, actorsToIgnore, isTargetedAbility, canTargetInvisible, originalTargetType, skipUntargetableCheck, useRealPositions);
				if (actorsOnTile == null || actorsOnTile.Count == 0)
				{
					continue;
				}
				foreach (CActor item in actorsOnTile)
				{
					if (item == null || item.IsDead)
					{
						continue;
					}
					CTile targetTile = ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y];
					CTile cTile = null;
					int num = 0;
					bool foundPath2;
					if (startLocation.Equals(item.ArrayIndex))
					{
						foundPath2 = true;
					}
					else
					{
						if (!ignoreLOS && !CActor.HaveLOS(ScenarioManager.Tiles[startLocation.X, startLocation.Y], targetTile))
						{
							continue;
						}
						if (adjacentTile3.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile3.m_HexMap != adjacentTile.m_HexMap)
						{
							bool flag = false;
							foreach (CTile item2 in list3)
							{
								if ((adjacentTile3.m_HexMap == item2.m_HexMap && adjacentTile.m_HexMap == item2.m_Hex2Map) || (adjacentTile3.m_HexMap == item2.m_Hex2Map && adjacentTile.m_HexMap == item2.m_HexMap))
								{
									if (cTile == null)
									{
										cTile = item2;
									}
									else
									{
										flag = true;
									}
								}
							}
							if (flag)
							{
								cTile = null;
							}
						}
						List<Point> list4 = null;
						if (cTile == null)
						{
							list4 = ScenarioManager.PathFinder.FindPath(null, startLocation, adjacentTile3.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath2, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
						}
						else
						{
							list4 = ScenarioManager.PathFinder.FindPath(null, startLocation, cTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath2, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
							if (foundPath2)
							{
								list4.AddRange(ScenarioManager.PathFinder.FindPath(null, cTile.m_ArrayIndex, item.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath2, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true));
							}
						}
						foreach (Point item3 in list4)
						{
							if (item3 == item.ArrayIndex || !freeIce || !ScenarioManager.PathFinder.GetNode(item3).IsIceTerrain)
							{
								num++;
							}
						}
					}
					if (foundPath2 && num <= range)
					{
						list.Add(new CRangeSortedActor(item, num));
					}
				}
			}
		}
		return ActorListOnly(list, sortForAttack: false, shortestOnly: false, skipUntargetableCheck);
	}

	public static bool? CanTargetInvisible(CActor actor, bool? canTargetInvisible)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			bool? flag = canTargetInvisible;
			if (flag != true)
			{
				List<CActiveBonus> list = (from w in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.Attack)
					where w != null && w.Ability?.MiscAbilityData?.CanTargetInvisible == true
					select w).ToList();
				if (list.Count > 0)
				{
					CAbility ability = list[0].Ability;
					if (ability == null || ability.MiscAbilityData?.NotApplyEnemy != true)
					{
						flag = true;
					}
				}
			}
			return flag;
		}
	}

	public static CActor GetActorOnTile(CTile tile, CActor filterActor, CAbilityFilterContainer abilityFilter, List<CActor> actorsToIgnore, bool isTargetedAbility, bool? canTargetInvisible = false, bool originalTargetType = false, bool skipUntargetableCheck = false, bool useRealPositions = true)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (tile == null)
			{
				return null;
			}
			List<CActor> list = ScenarioManager.Scenario.FindActorsAt(tile.m_ArrayIndex, null, includeInitial: false, isRetry: false, useRealPositions);
			if (list.Count == 0)
			{
				return null;
			}
			foreach (CActor item in list)
			{
				if (abilityFilter.IsValidTarget(item, filterActor, isTargetedAbility, originalTargetType, canTargetInvisible, skipUntargetableCheck))
				{
					if (actorsToIgnore != null && actorsToIgnore.Contains(item))
					{
						return null;
					}
					return item;
				}
			}
			return null;
		}
	}

	public static List<CActor> GetActorsOnTile(CTile tile, CActor filterActor, CAbilityFilterContainer abilityFilter, List<CActor> actorsToIgnore, bool isTargetedAbility, bool? canTargetInvisible = false, bool originalTargetType = false, bool skipUntargetableCheck = false, bool useRealPositions = true)
	{
		List<CActor> list = ScenarioManager.Scenario.FindActorsAt(tile.m_ArrayIndex, null, includeInitial: false, isRetry: false, useRealPositions);
		if (list == null)
		{
			return null;
		}
		if (list.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!abilityFilter.IsValidTarget(list[num], filterActor, isTargetedAbility, originalTargetType, canTargetInvisible, skipUntargetableCheck) || (actorsToIgnore != null && actorsToIgnore.Contains(list[num])))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static List<CActor> GetActorsOnTiles(List<CTile> tiles)
	{
		List<CActor> list = new List<CActor>();
		foreach (CTile tile in tiles)
		{
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(tile.m_ArrayIndex))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void GetAdjacentTiles(Point origin, List<CTile> tiles, bool clear = true)
	{
		if (clear)
		{
			tiles.Clear();
		}
		for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.ELeft; eAdjacentPosition <= ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition++)
		{
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(origin.X, origin.Y, eAdjacentPosition);
			if (adjacentTile != null)
			{
				tiles.Add(adjacentTile);
			}
		}
	}

	public static List<CTile> GetTilesInRange(Point origin, int range)
	{
		List<CTile> list = new List<CTile>();
		int num = Math.Max(1, range);
		int num2 = ((origin.Y - num >= 0) ? (origin.Y - num) : 0);
		int num3 = ((origin.Y + num < ScenarioManager.Height) ? (origin.Y + num) : ScenarioManager.Height);
		int num4 = ((origin.X - num >= 0) ? (origin.X - num) : 0);
		int num5 = ((origin.X + num < ScenarioManager.Width) ? (origin.X + num) : ScenarioManager.Width);
		for (int i = num2; i <= num3; i++)
		{
			for (int j = num4; j <= num5; j++)
			{
				if (i >= 0 && j >= 0 && i < ScenarioManager.Height && j < ScenarioManager.Width && Point.AxialDistance(b: Point.OddRToAxial(new Point(j, i)), a: Point.OddRToAxial(origin)) <= range)
				{
					CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
					if (adjacentTile != null)
					{
						list.Add(adjacentTile);
					}
				}
			}
		}
		return list;
	}

	public static void GetTilesInRange(Point origin, int range, List<Point> points, bool clear = true)
	{
		if (clear)
		{
			points.Clear();
		}
		int num = Math.Max(1, range);
		int num2 = ((origin.Y - num >= 0) ? (origin.Y - num) : 0);
		int num3 = ((origin.Y + num < ScenarioManager.Height) ? (origin.Y + num) : ScenarioManager.Height);
		int num4 = ((origin.X - num >= 0) ? (origin.X - num) : 0);
		int num5 = ((origin.X + num < ScenarioManager.Width) ? (origin.X + num) : ScenarioManager.Width);
		for (int i = num2; i <= num3; i++)
		{
			for (int j = num4; j <= num5; j++)
			{
				if (i >= 0 && j >= 0 && i < ScenarioManager.Height && j < ScenarioManager.Width)
				{
					Point point = new Point(j, i);
					if (Point.AxialDistance(Point.OddRToAxial(origin), Point.OddRToAxial(point)) <= range)
					{
						points.Add(point);
					}
				}
			}
		}
	}

	public static List<CTile> GetTilesInRange(CActor targetActor, int range, CAbility.EAbilityTargeting targetingType, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, bool allowClosedDoorTiles = false, bool includeTargetPosition = false, bool skipPathingCheck = false, bool includeStartingTile = false)
	{
		return GetTilesInRange(targetActor.ArrayIndex, range, targetingType, emptyTilesOnly, ignoreBlocked, innerRange, ignorePathLength, ignoreBlockedWithActor, ignoreLOS, emptyOpenDoorTiles, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, allowClosedDoorTiles, includeTargetPosition, noActorsAllowed: false, skipPathingCheck, includeStartingTile);
	}

	public static List<CTile> GetTilesInRange(CActor actor, int range)
	{
		List<CTile> list = new List<CTile> { actor.Tile };
		List<Point> list2 = new List<Point>();
		if (actor is CObjectActor { AttachedProp: not null } cObjectActor)
		{
			list = (from x in cObjectActor.AttachedProp.GetTiles()
				select ScenarioManager.GetTile(x)).ToList();
		}
		foreach (CTile item in list)
		{
			GetTilesInRange(item.m_ArrayIndex, range, list2, clear: false);
		}
		return (from x in list2
			select ScenarioManager.GetTile(x) into x
			where x != null
			select x).Distinct().ToList();
	}

	public static List<CTile> GetTilesInRange(Point arrayIndex, int range, CAbility.EAbilityTargeting targetingType, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, bool allowClosedDoorTiles = false, bool includeTargetPosition = false, bool noActorsAllowed = false, bool skipPathingCheck = false, bool includeStartingTile = false, bool checkTeleport = false)
	{
		List<CTile> list = new List<CTile>();
		List<CTile> list2 = new List<CTile>();
		List<CTile> list3 = new List<CTile>();
		switch (targetingType)
		{
		case CAbility.EAbilityTargeting.Range:
		{
			int num = Math.Max(1, range);
			int num2 = ((arrayIndex.Y - num >= 0) ? (arrayIndex.Y - num) : 0);
			int num3 = ((arrayIndex.Y + num < ScenarioManager.Height) ? (arrayIndex.Y + num) : ScenarioManager.Height);
			int num4 = ((arrayIndex.X - num >= 0) ? (arrayIndex.X - num) : 0);
			int num5 = ((arrayIndex.X + num < ScenarioManager.Width) ? (arrayIndex.X + num) : ScenarioManager.Width);
			for (int k = num2; k <= num3; k++)
			{
				for (int l = num4; l <= num5; l++)
				{
					if (k < 0 || l < 0 || k >= ScenarioManager.Height || l >= ScenarioManager.Width)
					{
						continue;
					}
					CTile adjacentTile = ScenarioManager.GetAdjacentTile(l, k, ScenarioManager.EAdjacentPosition.ECenter);
					if (adjacentTile != null && (innerRange == null || !innerRange.Contains(adjacentTile)))
					{
						CObjectDoor cObjectDoor = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
						if (cObjectDoor != null && cObjectDoor.Activated)
						{
							list3.Add(adjacentTile);
						}
						list2.Add(adjacentTile);
					}
				}
			}
			break;
		}
		case CAbility.EAbilityTargeting.Room:
		{
			CTile cTile2 = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
			if (cTile2.m_HexMap != null && cTile2.m_Hex2Map != null)
			{
				break;
			}
			foreach (CMapTile mapTile in cTile2.m_HexMap.MapTiles)
			{
				CTile item = ScenarioManager.Tiles[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y];
				list2.Add(item);
			}
			break;
		}
		case CAbility.EAbilityTargeting.All:
		{
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile3 in tiles)
			{
				if (cTile3 != null)
				{
					list2.Add(cTile3);
				}
			}
			break;
		}
		case CAbility.EAbilityTargeting.AllConnectedRooms:
		{
			CTile originTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile != null && ScenarioManager.RoomsBetweenTilesRevealed(originTile, cTile, checkTeleport))
				{
					list2.Add(cTile);
				}
			}
			break;
		}
		}
		foreach (CTile item2 in list2)
		{
			if (item2 == null || (!includeStartingTile && item2.m_ArrayIndex == arrayIndex))
			{
				continue;
			}
			bool foundPath = false;
			List<Point> list4 = null;
			CTile cTile4 = null;
			int num6 = 0;
			CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
			if (!skipPathingCheck)
			{
				if (item2.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile2.FindProp(ScenarioManager.ObjectImportType.Door) == null && item2.m_HexMap != adjacentTile2.m_HexMap)
				{
					bool flag = false;
					foreach (CTile item3 in list3)
					{
						if ((item2.m_HexMap == item3.m_HexMap && adjacentTile2.m_HexMap == item3.m_Hex2Map) || (item2.m_HexMap == item3.m_Hex2Map && adjacentTile2.m_HexMap == item3.m_HexMap))
						{
							if (cTile4 == null)
							{
								cTile4 = item3;
							}
							else
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						cTile4 = null;
					}
					if (cTile4 != null)
					{
						list4 = ScenarioManager.PathFinder.FindPath(null, arrayIndex, cTile4.m_ArrayIndex, ignoreBlocked | ignoreBlockedWithActor, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false);
						if (foundPath)
						{
							list4.AddRange(ScenarioManager.PathFinder.FindPath(null, cTile4.m_ArrayIndex, item2.m_ArrayIndex, ignoreBlocked | ignoreBlockedWithActor, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false));
						}
					}
				}
				if (cTile4 == null)
				{
					list4 = ScenarioManager.PathFinder.FindPath(null, arrayIndex, item2.m_ArrayIndex, ignoreBlocked | ignoreBlockedWithActor, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
				}
				if (includeTargetPosition && !foundPath && item2.m_ArrayIndex == arrayIndex)
				{
					foundPath = true;
					list4.Add(arrayIndex);
				}
				num6 = CAbilityMove.CalculateMoveCost(null, arrayIndex, list4, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, null);
			}
			else
			{
				foundPath = true;
			}
			if (emptyTilesOnly)
			{
				bool flag2 = false;
				bool flag3 = true;
				bool flag4 = item2.m_Props.FindAll((CObjectProp x) => x.PropType != EPropType.None && x.ObjectType != ScenarioManager.ObjectImportType.Shadow && x.ObjectType != ScenarioManager.ObjectImportType.VFXProp && x.ObjectType != ScenarioManager.ObjectImportType.TerrainLampVisualEffect && x.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && x.ObjectType != ScenarioManager.ObjectImportType.Spawner && (x.ObjectType != ScenarioManager.ObjectImportType.Trap || (x.ObjectType == ScenarioManager.ObjectImportType.Trap && !x.Activated))).ToList().Count == 0;
				bool flag5 = ((item2.m_Props.Count > 0) & ignoreDifficultTerrain) && (item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater);
				bool flag6 = item2.m_Props.Count > 0 && (item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainLampVisualEffect);
				bool flag7 = item2.m_Props.Count > 0 && item2.m_Props.Any((CObjectProp x) => x.IsLootable);
				bool flag8 = item2.FindProp(ScenarioManager.ObjectImportType.Door) != null;
				bool isBridgeOpen = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].IsBridgeOpen;
				bool flag9 = flag8 && (isBridgeOpen | allowClosedDoorTiles);
				bool walkable = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Walkable;
				bool blocked = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked;
				bool flag10 = (num6 <= range) | ignorePathLength;
				foreach (CActor item4 in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex))
				{
					if (!(item4 is CHeroSummonActor cHeroSummonActor) || !cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
					{
						flag3 = false;
					}
				}
				if (emptyOpenDoorTiles)
				{
					if ((((flag3 && (flag4 | flag5 | flag6 | flag7 | flag9)) & walkable) && !blocked) & flag10 & foundPath)
					{
						flag2 = true;
					}
				}
				else if ((((flag3 && (flag4 || (ignoreDifficultTerrain && item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.MoneyToken || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Chest || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.GoalChest || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Shadow || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.VFXProp || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Resource || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.PressurePlate || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Portal || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Spawner || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainLampVisualEffect)) & walkable) && !blocked) & foundPath & flag10)
				{
					flag2 = true;
				}
				if (flag2 && ((allowClosedDoorTiles & ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
				{
					list.Add(item2);
				}
			}
			else if (ignoreBlockedWithActor)
			{
				if (foundPath && ((num6 <= range) | ignorePathLength) && (ignoreBlocked || !ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked || (ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked && ScenarioManager.Scenario.FindActorAt(item2.m_ArrayIndex) != null)) && (((allowClosedDoorTiles || item2.m_HexMap.Revealed) & ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
				{
					list.Add(item2);
				}
			}
			else if (!emptyTilesOnly && !allowClosedDoorTiles)
			{
				CObjectDoor cObjectDoor2 = item2.FindDoor();
				if (!noActorsAllowed && cObjectDoor2?.RuntimeAttachedActor != null)
				{
					cObjectDoor2 = null;
				}
				if ((cObjectDoor2 != null && !ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].IsBridgeOpen) || (!foundPath && (!includeStartingTile || !(item2.m_ArrayIndex == arrayIndex))) || !((num6 <= range) | ignorePathLength))
				{
					continue;
				}
				if (noActorsAllowed)
				{
					bool flag11 = true;
					foreach (CActor item5 in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex))
					{
						if (!(item5 is CHeroSummonActor cHeroSummonActor2) || !cHeroSummonActor2.HeroSummonClass.SummonYML.TreatAsTrap)
						{
							flag11 = false;
						}
					}
					if (flag11 && (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
					{
						list.Add(item2);
					}
				}
				else if (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2))
				{
					list.Add(item2);
				}
			}
			else if (foundPath && ((num6 <= range) | ignorePathLength) && (((allowClosedDoorTiles || item2.m_HexMap.Revealed) & ignoreLOS) || ((item2.FindDoor()?.DoorIsClosed ?? false) & allowClosedDoorTiles) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
			{
				list.Add(item2);
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetTilesAtRange(Point arrayIndex, int atRange, int range, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, bool allowClosedDoorTiles = false, bool includeTargetPosition = false)
	{
		List<CTile> list = new List<CTile>();
		int num = Math.Max(1, atRange);
		for (int i = arrayIndex.Y - num; i <= arrayIndex.Y + num; i += num)
		{
			for (int j = arrayIndex.X - num; j <= arrayIndex.X + num; j += num)
			{
				if (i < 0 || j < 0 || i >= ScenarioManager.Height || j >= ScenarioManager.Width)
				{
					continue;
				}
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile == null || (innerRange != null && innerRange.Contains(adjacentTile)))
				{
					continue;
				}
				List<Point> list2 = ScenarioManager.PathFinder.FindPath(null, arrayIndex, adjacentTile.m_ArrayIndex, ignoreBlocked | ignoreBlockedWithActor, ignoreMoveCost: true, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out var foundPath, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
				if (includeTargetPosition && !foundPath && adjacentTile.m_ArrayIndex == arrayIndex)
				{
					foundPath = true;
					list2.Add(arrayIndex);
				}
				int num2 = CAbilityMove.CalculateMoveCost(null, arrayIndex, list2, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, null);
				if (emptyTilesOnly)
				{
					bool flag = false;
					if (emptyOpenDoorTiles)
					{
						if (((ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && (adjacentTile.m_Props.Count == 0 || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainLampVisualEffect || adjacentTile.m_Props[0].IsLootable || (adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) != null && (ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].IsBridgeOpen | allowClosedDoorTiles))) && ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked) & foundPath) && ((num2 <= range) | ignorePathLength))
						{
							flag = true;
						}
					}
					else if (((ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && (adjacentTile.m_Props.Count == 0 || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.MoneyToken || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Chest || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.GoalChest || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Shadow || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.VFXProp || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Resource || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.PressurePlate || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Portal || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Spawner || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainLampVisualEffect) && ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked) & foundPath) && ((num2 <= range) | ignorePathLength))
					{
						flag = true;
					}
					if (flag && ((allowClosedDoorTiles & ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
					{
						list.Add(adjacentTile);
					}
				}
				else if (ignoreBlockedWithActor)
				{
					if (foundPath && ((num2 <= range) | ignorePathLength) && (ignoreBlocked || !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked || (ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked && ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) != null)) && ((allowClosedDoorTiles & ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
					{
						list.Add(adjacentTile);
					}
				}
				else if (foundPath && ((num2 <= range) | ignorePathLength) && (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
				{
					list.Add(adjacentTile);
				}
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetNearestEmptyTiles(CTile startTile, int minRange = 0, int maxRange = int.MaxValue)
	{
		List<CTile> list = new List<CTile> { startTile };
		if (minRange <= 0 && startTile.IsWalkable && !startTile.IsBlocked && startTile.IsEmpty)
		{
			return list;
		}
		List<CTile> list2 = new List<CTile>();
		List<CTile> list3 = new List<CTile>();
		List<CTile> list4 = new List<CTile>();
		int num = 0;
		while (true)
		{
			list2.Clear();
			list3.Clear();
			num++;
			if (num > maxRange)
			{
				return list2;
			}
			foreach (CTile item in list)
			{
				GetAdjacentTiles(item.m_ArrayIndex, list4);
				foreach (CTile item2 in list4)
				{
					if (Point.HexDist(startTile.m_ArrayIndex, item2.m_ArrayIndex) != num || list3.Contains(item2))
					{
						continue;
					}
					list3.Add(item2);
					if (num >= minRange)
					{
						CNode cNode = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
						if (cNode.Walkable && !cNode.Blocked && item2.IsEmpty)
						{
							list2.Add(item2);
						}
					}
				}
			}
			if (list2.Count > 0)
			{
				break;
			}
			list.Clear();
			List<CTile> list5 = list;
			list = list3;
			list3 = list5;
		}
		return list2;
	}

	public static void UpdateDamageInflictedThisTurn(int damageInflictedThisTurn, CAbility.EAbilityType damageSourceAbilityType)
	{
		if (!OverridingCurrentActor)
		{
			s_DamageInflictedThisTurn += damageInflictedThisTurn;
		}
		CPhaseAction.UpdateDamageInflictedThisAction(damageInflictedThisTurn, damageSourceAbilityType);
	}

	public static void UpdateDamagePreventedThisTurn(int damagePreventedThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_DamagePreventedThisTurn += damagePreventedThisTurn;
		}
		CPhaseAction.UpdateDamagePreventedThisAction(damagePreventedThisTurn);
	}

	public static void UpdateActorsKilledThisRoundAndTurn(CActor actorKilled)
	{
		s_ActorsKilledThisRound.Add(actorKilled);
		s_ActorsKilledThisTurn.Add(actorKilled);
	}

	public static void AddTrophyTokensGainedThisRound(int trophies)
	{
		if (trophies > 0)
		{
			s_TrophyTokensGainedThisRound += trophies;
		}
	}

	public static void UpdateTargetsDamagedInPrevAttackThisTurn(CAbilityAttack abilityAttack)
	{
		if (s_TargetsDamagedInPrevAttackThisTurnAbilityID != abilityAttack.ID)
		{
			s_TargetsDamagedInPrevAttackThisTurnAbilityID = abilityAttack.ID;
			s_TargetsDamagedInPrevAttackThisTurn = 0;
		}
		s_TargetsDamagedInPrevAttackThisTurn++;
		CPhaseAction.UpdateTargetsDamagedInPrevAttackThisAction(abilityAttack);
	}

	public static void UpdateTargetsActuallyDamagedInPrevAttackThisTurn(CAbilityAttack abilityAttack)
	{
		if (s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID != abilityAttack.ID)
		{
			s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID = abilityAttack.ID;
			s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		}
		s_TargetsActuallyDamagedInPrevAttackThisTurn++;
		CPhaseAction.UpdateTargetsActuallyDamagedInPrevAttackThisAction(abilityAttack);
	}

	public static void UpdateTargetsDamagedInPrevDamageAbilityThisTurn(CAbilityDamage abilityDamage)
	{
		if (s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID != abilityDamage.ID)
		{
			s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID = abilityDamage.ID;
			s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		}
		s_TargetsDamagedInPrevDamageAbilityThisTurn++;
		CPhaseAction.UpdateTargetsDamagedInPrevDamageAbilityThisAction(abilityDamage);
	}

	public static void UpdateHexesMovedThisTurn(CActor actorMoved, List<Point> hexesMoved)
	{
		if (actorMoved == null)
		{
			return;
		}
		if (hexesMoved.Count > 0)
		{
			s_ActorsMovedThisTurn.Add(actorMoved);
		}
		if (!OverridingCurrentActor)
		{
			s_HexesMovedThisTurn.AddRange(hexesMoved.Select((Point p) => new TileIndex(p)));
		}
	}

	public static void UpdateObstaclesDestroyedThisTurn(int obstaclesDestroyedThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_ObstaclesDestroyedThisTurn += obstaclesDestroyedThisTurn;
		}
		CPhaseAction.UpdateObstaclesDestroyedThisAction(obstaclesDestroyedThisTurn);
	}

	public static void UpdateHazardousTerrainTilesMovedOverThisTurn(int hazardousTerrainTilesMovedOverThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_HazardousTerrainTilesMovedOverThisTurn += hazardousTerrainTilesMovedOverThisTurn;
		}
	}

	public static void UpdateDifficultTerrainTilesMovedOverThisTurn(int difficultTerrainTilesMovedOverThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_DifficultTerrainTilesMovedOverThisTurn += difficultTerrainTilesMovedOverThisTurn;
		}
	}

	public static void OverrideCurrentActorForOneAction(CActor actor, CActor.EType? switchType = null, bool killActorAfterAction = false, List<CAugment> addedAugments = null, CActor controllingActor = null, bool useControllingActorModifierDeck = false, bool useControllingActorPlayerControl = false, bool immediatelySetCurrentActor = false, bool treatAsNonControl = false)
	{
		CActor.EType type = actor.Type;
		if (switchType.HasValue && !treatAsNonControl)
		{
			actor.Type = switchType.Value;
		}
		s_OverridenActionActorStack.Push(new OverrideActorForActionStackData(s_CurrentActor, switchType.HasValue, type, killActorAfterAction, actor.AugmentSlots, actor, addedAugments, controllingActor, useControllingActorModifierDeck, useControllingActorPlayerControl, treatAsNonControl));
		if (addedAugments != null && addedAugments.Count > 0)
		{
			actor.OverrideAugmentSlots(actor.Augments.Count + addedAugments.Count);
			foreach (CAugment addedAugment in addedAugments)
			{
				actor.Class.AddAugmentBaseCardToTemporaryCards(addedAugment.RegisteredBaseCard as CAbilityCard);
				actor.Augments.Add(addedAugment);
			}
		}
		if (immediatelySetCurrentActor || s_CurrentActor == null || PhaseManager.CurrentPhaseAction == null)
		{
			s_CurrentActor = actor;
		}
		OverridingCurrentActor = true;
		ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
	}

	public static void EndOverrideCurrentActorForOneAction(bool forceUseOnDeathAbilityComplete = false)
	{
		if (IsPerformingCardOutOfPhase)
		{
			if (OverridingCurrentActor)
			{
				s_CurrentActor = CurrentOutOfPhaseCardSelectionAbility?.TargetingActor;
			}
			return;
		}
		InternalCurrentActor.Inventory.HandleUsedItems();
		lock (s_OverridenActionActorStack)
		{
			OverrideActorForActionStackData overrideActorForActionStackData = s_OverridenActionActorStack.Pop();
			CActor actorBeingControlled = overrideActorForActionStackData.ActorBeingControlled;
			actorBeingControlled.EndMindControl();
			if (overrideActorForActionStackData.OverrideSwitchedType)
			{
				actorBeingControlled.Type = overrideActorForActionStackData.OriginalType;
			}
			if (overrideActorForActionStackData.AddedAugments != null && overrideActorForActionStackData.AddedAugments.Count > 0)
			{
				actorBeingControlled.OverrideAugmentSlots(null);
				for (int num = overrideActorForActionStackData.AddedAugments.Count - 1; num >= 0; num--)
				{
					CAugment item = overrideActorForActionStackData.AddedAugments[num];
					actorBeingControlled.Augments.Remove(item);
				}
				actorBeingControlled.Class.ClearTemporaryCards();
			}
			if (overrideActorForActionStackData.PreviousActor != null)
			{
				s_CurrentActor = overrideActorForActionStackData.PreviousActor;
			}
			OverridingCurrentActor = s_OverridenActionActorStack.Count > 0;
			ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
			if (forceUseOnDeathAbilityComplete || overrideActorForActionStackData.OverrideKillActor)
			{
				actorBeingControlled.OnDeathAbilityComplete();
			}
		}
	}

	public static CActor GetControllingActor(CActor actor)
	{
		if (actor == null)
		{
			return null;
		}
		lock (s_OverridenActionActorStack)
		{
			if (OverridingCurrentActor && actor == s_CurrentActor && s_OverridenActionActorStack.Count > 0)
			{
				OverrideActorForActionStackData overrideActorForActionStackData = s_OverridenActionActorStack.Peek();
				bool flag = overrideActorForActionStackData.ControllingActor != null;
				if ((actor is CPlayerActor || actor is CHeroSummonActor { Summoner: not null }) && !overrideActorForActionStackData.UseControllingActorPlayerControl)
				{
					flag = false;
				}
				if (overrideActorForActionStackData.TreatAsNonControl)
				{
					return null;
				}
				if (flag)
				{
					return overrideActorForActionStackData.ControllingActor;
				}
				return actor;
			}
		}
		if (s_ActorsToOverrideTurns.TryGetValue(actor, out var value))
		{
			bool flag2 = value.ControllingActor != null;
			if ((actor is CPlayerActor || actor is CHeroSummonActor { Summoner: not null }) && !value.UseControllingActorPlayerControl)
			{
				flag2 = false;
			}
			if (flag2)
			{
				return value.ControllingActor;
			}
			return actor;
		}
		if (actor is CHeroSummonActor cHeroSummonActor3)
		{
			return cHeroSummonActor3.Summoner;
		}
		return null;
	}

	public static void QueueOverrideActorForOneTurn(CActor actor, CActor.EType switchType, List<CAbility> controlAbilities = null, CBaseCard baseCard = null, CActor controllingActor = null, bool useControllingActorPlayerControl = false)
	{
		s_ActorsToOverrideTurns[actor] = new QueueOverrideActorForTurnData(switchType, controlAbilities, baseCard, controllingActor, useControllingActorPlayerControl);
	}

	public static void OverrideActorForOneTurn(CActor currentActor)
	{
		if (s_ActorsToOverrideTurns.TryGetValue(currentActor, out var value))
		{
			CActor.EType originalType = currentActor.OriginalType;
			currentActor.Type = value.OverrideActorType;
			if (value.OverrideActionAbilities != null && value.OverrideActionAbilities.Count > 0 && value.OverrideActionBaseCard != null)
			{
				SetActionFlag(EActionFlag.TopPlayed | EActionFlag.BottomPlayed, s_ActionFlagsByActionSequenceType, currentActor.MaxRoundCardsCount);
			}
			CurrentOverridenActorForTurnData = new OverrideActorForTurnData(currentActor, currentActor.Type == originalType, originalType, null);
			CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(currentActor)
			{
				m_ControlActorAbility = null,
				m_ControlledActor = currentActor
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void OverrideCurrentActor(CActor currentActor)
	{
		s_CurrentActor = currentActor;
	}

	public static void EndOverrideActorForOneTurn()
	{
		if (CurrentOverridenActorForTurnData != null)
		{
			CurrentOverridenActorForTurnData.OverridenActor.EndMindControl();
			CurrentOverridenActorForTurnData.OverridenActor.Type = CurrentOverridenActorForTurnData.OriginalType;
			s_ActorsToOverrideTurns.Remove(CurrentOverridenActorForTurnData.OverridenActor);
			CurrentOverridenActorForTurnData = null;
		}
	}

	public static void CacheExtraTurnInitiatorCard(CAbilityCard extraTurnInitiatorCard)
	{
		if (s_CurrentActor != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			CachedExtraTurnInitiatorCardStack.Push(extraTurnInitiatorCard);
		}
	}

	public static void StartActorExtraTurnImmediately(CPlayerActor playerActor)
	{
		playerActor.TakingExtraTurnOfTypeStack.Push(playerActor.SelectingCardsForExtraTurnOfType);
		playerActor.SelectingCardsForExtraTurnOfType = CAbilityExtraTurn.EExtraTurnType.None;
		playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Push(playerActor.CharacterClass.ExtraTurnCards.ToList());
		if (CurrentActionInitiator == EActionInitiator.AbilityCard)
		{
			ProgressActionSelection(RoundAbilityCardSelected, 2);
		}
		EActionFlag eActionFlag = EActionFlag.None;
		if (playerActor.SkipTopCardAction)
		{
			playerActor.SkipTopCardAction = false;
			eActionFlag |= EActionFlag.TopPlayed;
		}
		if (playerActor.SkipBottomCardAction)
		{
			playerActor.SkipBottomCardAction = false;
			eActionFlag |= EActionFlag.BottomPlayed;
		}
		ExtraTurnActionSelectionFlagStack.Push(eActionFlag);
		s_ExtraTurnActorStack.Push(new OverrideActorForActionStackData(s_CurrentActor, overrideSwitchedType: false, s_CurrentActor.Type, overrideKillActor: false, s_CurrentActor.AugmentSlots, playerActor));
		s_CurrentActor = playerActor;
		s_TurnActor = playerActor;
		if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActions || playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			playerActor.AbilityTypesPerformedThisTurn.Clear();
			s_DamageInflictedThisTurn = 0;
			s_DamagePreventedThisTurn = 0;
			s_ActorsKilledThisTurn.Clear();
			s_ActorsMovedThisTurn.Clear();
			s_TargetsDamagedInPrevAttackThisTurn = 0;
			s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
			s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
			s_HexesMovedThisTurn.Clear();
			s_ObstaclesDestroyedThisTurn = 0;
			s_HazardousTerrainTilesMovedOverThisTurn = 0;
			s_DifficultTerrainTilesMovedOverThisTurn = 0;
		}
		if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleActionAfterExtraTurnPerform)
		{
			PhaseManager.CurrentPhase.NextStep();
		}
		else
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
		}
	}

	public static void CancelExtraTurn()
	{
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			allPlayer.SelectingCardsForExtraTurnOfType = CAbilityExtraTurn.EExtraTurnType.None;
		}
	}

	public static void EndExtraTurn(bool force_cancel)
	{
		CActor cActor = s_CurrentActor;
		CCharacterClass cCharacterClass = null;
		if (cActor.Class is CCharacterClass cCharacterClass2)
		{
			if (CurrentExtraTurnAbility != null && !CurrentExtraTurnAbility.IsPending() && (force_cancel || cCharacterClass2.ExtraTurnCardsSelectedInCardSelectionStack.Count == 0))
			{
				CEndedExtraTurn cEndedExtraTurn = new CEndedExtraTurn(s_CurrentActor);
				cEndedExtraTurn.m_ActorTurnEnded = cActor;
				ScenarioRuleClient.MessageHandler(cEndedExtraTurn);
				return;
			}
			cCharacterClass = cCharacterClass2;
			cCharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Pop();
		}
		CAbilityExtraTurn.EExtraTurnType eExtraTurnType = cActor.TakingExtraTurnOfTypeStack.Pop();
		if (cActor.PendingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (cCharacterClass != null)
			{
				CAbilityExtraTurn.PendingExtraTurnData pendingExtraTurnData = cCharacterClass.PendingExtraTurnCardsData[0];
				cCharacterClass.SetExtraTurnInitiativeAbilityCard(pendingExtraTurnData.PendingExtraTurnCards.First((CAbilityCard x) => x.Initiative == pendingExtraTurnData.LeadingInitiative));
			}
			s_InitiativeSortedActors.Remove(cActor);
			bool flag = false;
			foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
			{
				if (s_ActorInitiativeComparer.Compare(cActor, s_InitiativeSortedActor) < 0)
				{
					int index = s_InitiativeSortedActors.IndexOf(s_InitiativeSortedActor);
					s_InitiativeSortedActors.Insert(index, cActor);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				s_InitiativeSortedActors.Add(cActor);
			}
		}
		else if (cActor.PendingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
		{
			if (cCharacterClass != null)
			{
				CAbilityExtraTurn.PendingExtraTurnData pendingExtraTurnData2 = cCharacterClass.PendingExtraTurnCardsData[0];
				cCharacterClass.SetExtraTurnInitiativeAbilityCard(pendingExtraTurnData2.PendingExtraTurnCards.First());
			}
		}
		else
		{
			cCharacterClass?.SetExtraTurnInitiativeAbilityCard(null);
		}
		if (s_ExtraTurnActorStack.Count > 0)
		{
			s_CurrentActor = s_ExtraTurnActorStack.Pop().PreviousActor;
		}
		if (s_CurrentActor != null && CachedExtraTurnInitiatorCardStack.Count > 0 && CachedExtraTurnInitiatorCardStack.Peek() != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			CachedExtraTurnInitiatorCardStack.Peek().ActionHasHappened = true;
		}
		switch (eExtraTurnType)
		{
		case CAbilityExtraTurn.EExtraTurnType.SingleActionBeforeCardSelection:
		{
			CEndedExtraTurn cEndedExtraTurn3 = new CEndedExtraTurn(s_CurrentActor);
			cEndedExtraTurn3.m_ActorTurnEnded = cActor;
			ScenarioRuleClient.MessageHandler(cEndedExtraTurn3);
			s_CurrentActor = null;
			PhaseManager.SetNextPhase(CPhase.PhaseType.StartRoundEffects);
			break;
		}
		default:
		{
			CEndedExtraTurn cEndedExtraTurn4 = new CEndedExtraTurn(s_CurrentActor);
			cEndedExtraTurn4.m_ActorTurnEnded = cActor;
			ScenarioRuleClient.MessageHandler(cEndedExtraTurn4);
			PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			break;
		}
		case CAbilityExtraTurn.EExtraTurnType.BothActionsLater:
		case CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn:
			if (eExtraTurnType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
			{
				CEndedExtraTurn cEndedExtraTurn2 = new CEndedExtraTurn(s_CurrentActor);
				cEndedExtraTurn2.m_ActorTurnEnded = cActor;
				ScenarioRuleClient.MessageHandler(cEndedExtraTurn2);
			}
			break;
		}
	}

	public static void FinishEndExtraTurn()
	{
		if (CachedExtraTurnInitiatorCardStack.Count == 0)
		{
			return;
		}
		CAbilityCard cAbilityCard = CachedExtraTurnInitiatorCardStack.Pop();
		if (cAbilityCard != RoundAbilityCardSelected && cAbilityCard != null && cAbilityCard.CurrentCardPile != CBaseCard.ECardPile.Discarded && cAbilityCard.CurrentCardPile != CBaseCard.ECardPile.Lost && s_CurrentActor != null && cAbilityCard != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			if (s_CurrentActor.IsTakingExtraTurn)
			{
				((CPlayerActor)s_CurrentActor).CharacterClass.DiscardExtraTurnAbilityCard(cAbilityCard);
			}
			else
			{
				((CPlayerActor)s_CurrentActor).CharacterClass.DiscardRoundAbilityCard(cAbilityCard);
			}
			CClass.MarkAbilityActiveBonusesFinished(s_CurrentActor);
		}
	}

	public static void SetCurrentActorForCompanionSummon(CPlayerActor playerActor)
	{
		s_CurrentActor = playerActor;
		ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
	}

	public static void CheckObjectives()
	{
		foreach (CObjective item in ScenarioManager.CurrentScenarioState.WinObjectives.Where((CObjective o) => !o.IsActive))
		{
			item.CheckActivationRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		foreach (CObjective item2 in ScenarioManager.CurrentScenarioState.LoseObjectives.Where((CObjective o) => !o.IsActive))
		{
			item2.CheckActivationRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		CCheckCompleteObjectives_MessageData message = new CCheckCompleteObjectives_MessageData();
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void MoveActorStraightLine(CActor actor, Point targetPoint, bool shouldWait = false, bool slideWithoutAnim = false, float slideSpeed = 1f, bool notifyChoreographer = true)
	{
		CTile cTile = ScenarioManager.Tiles[targetPoint.X, targetPoint.Y];
		if (actor is CObjectActor { AttachedProp: not null, AttachedProp: var attachedProp } cObjectActor)
		{
			if (!cObjectActor.CalcNeighbourTileAndRotation(targetPoint, out var newPos, out var newAngle))
			{
				return;
			}
			targetPoint = newPos;
			cTile = ScenarioManager.Tiles[targetPoint.X, targetPoint.Y];
			CVector3 rotation = new CVector3(0f, newAngle, 0f);
			attachedProp.PropTile.DelProp(cObjectActor.AttachedProp);
			cTile.AddProp(cObjectActor.AttachedProp);
			CObjectObstacle cObjectObstacle = attachedProp as CObjectObstacle;
			if (cObjectObstacle != null)
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = false;
				}
			}
			if (cTile.m_HexMap.MapGuid != attachedProp.StartingMapGuid)
			{
				attachedProp.StartingMapGuid = cTile.m_HexMap.MapGuid;
			}
			attachedProp.SetLocation(new TileIndex(targetPoint), null, rotation);
			actor.ArrayIndex = targetPoint;
			if (cObjectObstacle != null)
			{
				lock (cObjectObstacle.PathingBlockers)
				{
					foreach (TileIndex pathingBlocker2 in cObjectObstacle.PathingBlockers)
					{
						ScenarioManager.PathFinder.Nodes[pathingBlocker2.X, pathingBlocker2.Y].Blocked = true;
					}
				}
			}
		}
		else
		{
			actor.ArrayIndex = targetPoint;
		}
		if (notifyChoreographer)
		{
			CActorHasMoved_MessageData cActorHasMoved_MessageData = new CActorHasMoved_MessageData(actor);
			cActorHasMoved_MessageData.m_Ability = null;
			cActorHasMoved_MessageData.m_Waypoints = new List<CTile> { actor.Tile, cTile };
			cActorHasMoved_MessageData.m_MovingActor = actor;
			cActorHasMoved_MessageData.m_ActorsToCarry = new List<CActor>();
			cActorHasMoved_MessageData.m_Jump = false;
			cActorHasMoved_MessageData.m_ForceMoveViaLoco = true;
			cActorHasMoved_MessageData.m_ShouldWait = shouldWait;
			cActorHasMoved_MessageData.m_Slide = slideWithoutAnim;
			cActorHasMoved_MessageData.m_SlideSpeed = slideSpeed;
			cActorHasMoved_MessageData.m_DontTeleport = true;
			ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData);
		}
	}

	public static bool IsTileInBounds(Point tile)
	{
		if (tile.Y >= 0 && tile.X >= 0 && tile.Y < ScenarioManager.Height)
		{
			return tile.X < ScenarioManager.Width;
		}
		return false;
	}

	public static bool IsWaitingForCampaignSection(string id)
	{
		if (ThreadIsSleeping && ThreadSleepReason != null)
		{
			return ThreadSleepReason.StartsWith($"WaitingForCampaignSection_{id}");
		}
		return false;
	}

	public static void TriggerCampaignSection(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventCampaignSectionTriggered(id, shouldPause: false), processImmediately: true);
			if (ScenarioRuleClient.IsWorkerThread)
			{
				WaitForConfirmation($"WaitingForCampaignSection_{id}", waitForMultiplayer: true);
			}
			else
			{
				Log.Warning("Trying to freeze game thread for message " + id + " from main thread");
			}
		}
	}

	public static void OnLevelMessageWasDismissed(CLevelMessage message)
	{
		if (IsWaitingForCampaignSection(message.MessageName))
		{
			SelectedContinueAbility();
		}
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return;
		}
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript.OnLevelMessageWasDismissed(message);
		}
	}

	public static CTile FindClosestEmptyTile(CTile originTile, int maxDepth = 100)
	{
		if (!originTile.Actors.Any() && !originTile.IsBlocked && originTile.IsWalkable)
		{
			return originTile;
		}
		List<CTile> searchedTiles = new List<CTile> { originTile };
		return FindClosestEmptyTileRecursive(originTile, ref searchedTiles, 0, maxDepth);
	}

	private static CTile FindClosestEmptyTileRecursive(CTile currentTile, ref List<CTile> searchedTiles, int currentDepth, int maxDepth)
	{
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable || searchedTiles.Contains(allAdjacentTile))
			{
				continue;
			}
			if (allAdjacentTile.Actors.Any() || allAdjacentTile.IsBlocked || !allAdjacentTile.IsWalkable)
			{
				searchedTiles.Add(allAdjacentTile);
				if (currentDepth < maxDepth)
				{
					CTile cTile = FindClosestEmptyTileRecursive(allAdjacentTile, ref searchedTiles, currentDepth + 1, maxDepth);
					if (cTile != null)
					{
						return cTile;
					}
				}
				continue;
			}
			return allAdjacentTile;
		}
		return null;
	}

	public static CTile FindClosestEmptyFeaturelessTile(CTile originTile, int maxDepth = 100, List<CTile> toIgnore = null)
	{
		if (CAbilityFilter.IsTileEmpty(originTile))
		{
			return originTile;
		}
		List<CTile> list = new List<CTile>();
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && CAbilityFilter.IsTileEmpty(cTile) && (toIgnore == null || !toIgnore.Contains(cTile)))
			{
				list.Add(cTile);
			}
		}
		return list.OrderBy((CTile x) => Point.HexDist(x.m_ArrayIndex, originTile.m_ArrayIndex)).FirstOrDefault();
	}

	public static bool WillMovingOnTileEndMovement(CTile tile, CActor actor, CAbility ability)
	{
		CObjectTrap cObjectTrap = tile.FindTrapProp();
		if (cObjectTrap != null && ability != null)
		{
			if (!actor.FlyingAfterConditions && ability.GetMiscVar("StopOnFirstTrap").GetBool())
			{
				return true;
			}
			foreach (CActiveBonus item in ability.TargetingActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.GenericTrapActivated))
			{
				CAbility addAbility = (item.Ability as CAbilityAddActiveBonus).AddAbility;
				if (!(addAbility is CAbilityNull) && item.CreateTriggerContext(item.BespokeBehaviour, addAbility, CExpression.Globals.CurrentAbility, actor, actor, true, ("trap", new TValue(cObjectTrap))) != null)
				{
					return true;
				}
			}
		}
		if (ScenarioManager.CurrentScenarioState?.ScenarioScripts != null)
		{
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.WillMovingOnTileEndMovement(tile, actor, ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static GameStateState Save()
	{
		GameStateState gameStateState = new GameStateState();
		gameStateState.PathfinderIceTerrain = ScenarioManager.PathFinder?.HasIceTerrain ?? false;
		gameStateState.LastSpawnerStartRound = LastSpawnerStartRound;
		gameStateState.ObstaclesDestroyedThisTurn = s_ObstaclesDestroyedThisTurn;
		gameStateState.RoundCount = s_RoundCount;
		gameStateState.CurrentActorGUID = SaveActorAsGUID(s_CurrentActor);
		gameStateState.ShortRestingPlayerGUID = SaveActorAsGUID(s_ShortRestingPlayer);
		gameStateState.ShortRestCardToBurnID = s_ShortRestCardToBurnID;
		gameStateState.TurnActorGUID = SaveActorAsGUID(s_TurnActor);
		gameStateState.LastActorGUID = SaveActorAsGUID(s_LastActor);
		gameStateState.SkipMonsterTurns = s_SkipMonsterTurns;
		gameStateState.AutoRestorePlayerCards = s_AutoRestorePlayerCards;
		gameStateState.AutoSelectPlayerCards = s_AutoSelectPlayerCards;
		gameStateState.AutoTakeDamage = s_AutoTakeDamage;
		gameStateState.MulPlayerHealth = s_MulPlayerHealth;
		gameStateState.InitiativeSortedActors = SaveActorsAsGUIDs(s_InitiativeSortedActors);
		gameStateState.TurnOrderSoFar = TurnOrderSoFar.Select(((CActor, CClass) x) => (ActorGuid: x.Item1.ActorGuid, ID: x.Item2.ID)).ToList();
		gameStateState.PreInitiativeAdjustedPlayerActors = SaveActorsAsGUIDs(PreInitiativeAdjustedPlayerActors);
		gameStateState.CheckAdjustInitiativeIndex = s_CheckAdjustInitiativeIndex;
		gameStateState.ActionFlagsByActionSequenceType = s_ActionFlagsByActionSequenceType.ToArray();
		gameStateState.PlayedCards = CopyPlayedCards(s_PlayedCards);
		gameStateState.CurrentActionInitiator = CurrentActionInitiator;
		gameStateState.RoundAbilityCardActionType = RoundAbilityCardActionType;
		gameStateState.RoundAbilityCardSelected = ((RoundAbilityCardSelected != null) ? RoundAbilityCardSelected.ID : 0);
		gameStateState.LastAbilityCardActionType = LastAbilityCardActionType;
		gameStateState.DamageInflictedThisTurn = s_DamageInflictedThisTurn;
		gameStateState.DamagePreventedThisTurn = s_DamagePreventedThisTurn;
		gameStateState.ActorsMovedThisTurn = SaveActorsAsGUIDs(s_ActorsMovedThisTurn);
		gameStateState.HexesMovedThisTurn = s_HexesMovedThisTurn.ToList();
		gameStateState.ActorsKilledThisTurn = SaveActorsAsGUIDs(s_ActorsKilledThisTurn);
		gameStateState.ActorsKilledThisRound = SaveActorsAsGUIDs(s_ActorsKilledThisRound);
		gameStateState.TrophyTokensGainedThisRound = s_TrophyTokensGainedThisRound;
		gameStateState.LastDummyCardID = LastDummyCardID;
		gameStateState.MonsterConsumes = ElementInfusionBoardManager.s_EnemyConsumedElements.Select((KeyValuePair<string, List<ElementInfusionBoardManager.EElement>> x) => (Key: x.Key, Value: x.Value)).ToList();
		gameStateState.MonsterInfusions = ElementInfusionBoardManager.s_EnemyInfusedElements.Select((KeyValuePair<string, List<ElementInfusionBoardManager.EElement>> x) => (Key: x.Key, Value: x.Value)).ToList();
		gameStateState.EnemyKillTracker.Clear();
		foreach (KeyValuePair<CEnemyActor, CActor> item3 in s_EnemyKillTracker)
		{
			string item = SaveActorAsGUID(item3.Key);
			string item2 = SaveActorAsGUID(item3.Value);
			gameStateState.EnemyKillTracker.Add((item, item2));
		}
		return gameStateState;
	}

	private static Dictionary<string, List<Tuple<int, EActionFlag>>[]> CopyPlayedCards(Dictionary<string, List<Tuple<int, EActionFlag>>[]> source)
	{
		Dictionary<string, List<Tuple<int, EActionFlag>>[]> dictionary = new Dictionary<string, List<Tuple<int, EActionFlag>>[]>();
		foreach (string key in source.Keys)
		{
			List<Tuple<int, EActionFlag>>[] array = source[key];
			List<Tuple<int, EActionFlag>>[] array2 = new List<Tuple<int, EActionFlag>>[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					array2[i] = array[i].ToList();
				}
			}
			dictionary.Add(key, array2);
		}
		return dictionary;
	}

	private static void ValidateActor(CActor actor)
	{
		if (actor == null)
		{
			return;
		}
		bool flag = true;
		CPlayerActor player = actor as CPlayerActor;
		if (player != null)
		{
			flag = ScenarioManager.CurrentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ActorGuid == player.ActorGuid) != null;
		}
		else
		{
			CEnemyActor enemy = actor as CEnemyActor;
			if (enemy != null)
			{
				flag = ScenarioManager.CurrentScenarioState.AllEnemyStates.SingleOrDefault((EnemyState x) => x.ActorGuid == enemy.ActorGuid) != null;
			}
			else
			{
				CHeroSummonActor heroSummon = actor as CHeroSummonActor;
				flag = ((heroSummon == null) ? (ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid) != null) : (ScenarioManager.CurrentScenarioState.HeroSummons.SingleOrDefault((HeroSummonState x) => x.ActorGuid == heroSummon.ActorGuid) != null));
			}
		}
		if (flag)
		{
			return;
		}
		throw new Exception($"Trying to save {actor} with guid that isn't present in the current scenario state");
	}

	public static void Load(GameStateState state)
	{
		if (state == null)
		{
			state = new GameStateState();
		}
		if (ScenarioManager.PathFinder != null)
		{
			ScenarioManager.PathFinder.HasIceTerrain = state.PathfinderIceTerrain;
		}
		LastSpawnerStartRound = state.LastSpawnerStartRound;
		s_ObstaclesDestroyedThisTurn = state.ObstaclesDestroyedThisTurn;
		s_RoundCount = state.RoundCount;
		s_CurrentActor = LoadActorFromGUID<CActor>(state.CurrentActorGUID);
		s_ShortRestingPlayer = LoadActorFromGUID<CPlayerActor>(state.ShortRestingPlayerGUID);
		s_ShortRestCardToBurnID = state.ShortRestCardToBurnID;
		s_TurnActor = LoadActorFromGUID<CActor>(state.TurnActorGUID);
		s_LastActor = LoadActorFromGUID<CActor>(state.LastActorGUID);
		if (!ScenarioManager.CurrentScenarioState.IsFirstLoad)
		{
			s_SkipMonsterTurns = state.SkipMonsterTurns;
			s_AutoRestorePlayerCards = state.AutoRestorePlayerCards;
			s_AutoSelectPlayerCards = state.AutoSelectPlayerCards;
			s_AutoTakeDamage = state.AutoTakeDamage;
			s_MulPlayerHealth = ((state.MulPlayerHealth <= 0f) ? 1f : state.MulPlayerHealth);
		}
		else
		{
			MulPlayersHealth(s_MulPlayerHealth);
		}
		s_InitiativeSortedActors = LoadActorsFromGUIDs<CActor>(state.InitiativeSortedActors);
		TurnOrderSoFar = new List<(CActor, CClass)>();
		if (state.TurnOrderSoFar == null)
		{
			if (!string.IsNullOrEmpty(state.TurnActorGUID))
			{
				List<CActor> list = ScenarioManager.Scenario.AllAliveActorsThreadSafe.Where((CActor x) => x.PlayedThisRound).ToList();
				list.Sort(s_ActorInitiativeComparer);
				TurnOrderSoFar.AddRange(list.Select((CActor x) => (x: x, Class: x.Class)));
			}
		}
		else
		{
			foreach (var item4 in state.TurnOrderSoFar)
			{
				CActor item = LoadActorFromGUID<CActor>(item4.Item1);
				CClass cClass = CharacterClassManager.Find(item4.Item2);
				if (cClass == null)
				{
					cClass = MonsterClassManager.Find(item4.Item2);
				}
				if (cClass == null)
				{
					cClass = CharacterClassManager.FindHeroSummonClass(item4.Item2);
				}
				TurnOrderSoFar.Add((item, cClass));
			}
		}
		PreInitiativeAdjustedPlayerActors = LoadActorsFromGUIDs<CPlayerActor>(state.PreInitiativeAdjustedPlayerActors);
		s_CheckAdjustInitiativeIndex = state.CheckAdjustInitiativeIndex;
		s_ActionFlagsByActionSequenceType = state.ActionFlagsByActionSequenceType.ToArray();
		s_PlayedCards = CopyPlayedCards(state.PlayedCards);
		CurrentActionInitiator = state.CurrentActionInitiator;
		RoundAbilityCardActionType = state.RoundAbilityCardActionType;
		RoundAbilityCardSelected = ((state.RoundAbilityCardSelected <= 0) ? null : (s_CurrentActor as CPlayerActor)?.CharacterClass.AbilityCardsPool.Single((CAbilityCard x) => x.ID == state.RoundAbilityCardSelected));
		LastAbilityCardActionType = state.LastAbilityCardActionType;
		s_DamageInflictedThisTurn = state.DamageInflictedThisTurn;
		s_DamagePreventedThisTurn = state.DamagePreventedThisTurn;
		s_ActorsMovedThisTurn = LoadActorsFromGUIDs<CActor>(state.ActorsMovedThisTurn);
		s_HexesMovedThisTurn = state.HexesMovedThisTurn?.ToList() ?? new List<TileIndex>();
		s_ActorsKilledThisTurn = LoadActorsFromGUIDs<CActor>(state.ActorsKilledThisTurn);
		s_ActorsKilledThisRound = LoadActorsFromGUIDs<CActor>(state.ActorsKilledThisRound);
		s_TrophyTokensGainedThisRound = state.TrophyTokensGainedThisRound;
		LastDummyCardID = state.LastDummyCardID;
		if (s_EnemyKillTracker == null)
		{
			s_EnemyKillTracker = new List<KeyValuePair<CEnemyActor, CActor>>();
		}
		else
		{
			s_EnemyKillTracker.Clear();
		}
		if (state.EnemyKillTracker != null && state.EnemyKillTracker.Count > 0)
		{
			foreach (var item5 in state.EnemyKillTracker)
			{
				string item2 = item5.victimGUID;
				string item3 = item5.killerGUID;
				CEnemyActor key = LoadActorFromGUID<CEnemyActor>(item2);
				CActor value = LoadActorFromGUID<CActor>(item3);
				s_EnemyKillTracker.Add(new KeyValuePair<CEnemyActor, CActor>(key, value));
			}
		}
		ElementInfusionBoardManager.s_EnemyConsumedElements = new Dictionary<string, List<ElementInfusionBoardManager.EElement>>();
		foreach (var monsterConsume in state.MonsterConsumes)
		{
			ElementInfusionBoardManager.s_EnemyConsumedElements[monsterConsume.Item1] = monsterConsume.Item2;
		}
		ElementInfusionBoardManager.s_EnemyInfusedElements = new Dictionary<string, List<ElementInfusionBoardManager.EElement>>();
		foreach (var monsterInfusion in state.MonsterInfusions)
		{
			ElementInfusionBoardManager.s_EnemyInfusedElements[monsterInfusion.Item1] = monsterInfusion.Item2;
		}
	}

	public static void MulPlayersHealth(float mul)
	{
		if (mul <= 0f || mul == 1f)
		{
			return;
		}
		List<CPlayerActor> list = ScenarioManager.Scenario?.PlayerActors;
		if (list == null)
		{
			return;
		}
		foreach (CPlayerActor item in list)
		{
			item.MaxHealth = (int)Math.Ceiling((float)item.MaxHealth * mul);
			item.Health = (int)Math.Ceiling((float)item.Health * mul);
		}
	}

	public static string SaveActorAsGUID(CActor actor)
	{
		ValidateActor(actor);
		return actor?.ActorGuid;
	}

	public static T LoadActorFromGUID<T>(string guid) where T : CActor
	{
		if (string.IsNullOrEmpty(guid))
		{
			return null;
		}
		return ScenarioManager.Scenario.AllActors.Single((CActor x) => x.ActorGuid == guid) as T;
	}

	public static List<string> SaveActorsAsGUIDs<T>(List<T> actors) where T : CActor
	{
		if (actors != null)
		{
			foreach (T actor in actors)
			{
				ValidateActor(actor);
			}
		}
		return actors?.Select((T x) => x.ActorGuid).ToList();
	}

	public static List<T> LoadActorsFromGUIDs<T>(List<string> guids, bool noNull = true) where T : CActor
	{
		if (guids == null)
		{
			if (!noNull)
			{
				return null;
			}
			return new List<T>();
		}
		return guids.Select((string x) => LoadActorFromGUID<T>(x)).ToList();
	}
}
