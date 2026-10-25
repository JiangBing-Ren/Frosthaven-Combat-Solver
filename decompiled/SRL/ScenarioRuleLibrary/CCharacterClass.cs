using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CCharacterClass : CClass
{
	public static readonly ECharacter[] CharacterNames = (ECharacter[])Enum.GetValues(typeof(ECharacter));

	[StateDump(Skip = true)]
	public List<CAbilityCard> LastRecoveredAbilityCards;

	private List<CAbilityCard> m_SelectedAbilityCardsOutOfPhase = new List<CAbilityCard>();

	private List<CAbilityCard> m_AbilityCardsPool = new List<CAbilityCard>();

	private List<CAbilityCard> m_RoundAbilityCardsSelectedInCardSelection = new List<CAbilityCard>();

	private List<CAbilityCard> m_RoundAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_HandAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_DiscardedAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_ExcludedFromShortRestAbilityCards = new List<CAbilityCard>();

	private List<CBaseCard> m_ActivatedCards = new List<CBaseCard>();

	private List<CAbilityCard> m_LostAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_PermanentlyLostAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_TakenCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_OwnedUnusedCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_ExtraTurnCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_SupplyCards = new List<CAbilityCard>();

	private List<AttackModifierYMLData> m_AttackModifierCardsPool = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_AttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_DiscardedAttackModifierCards = new List<AttackModifierYMLData>();

	public bool m_ShuffleAttackModifierCards;

	private bool m_LongRest;

	private bool m_HasLongRested;

	private bool m_ShortRest;

	private bool m_HasShortRested;

	private bool m_HasImprovedShortRested;

	private bool m_ImprovedShortRest;

	private uint m_ImprovedShortRestDrawCount;

	private bool _mImprovedShortRestCanExcludeCards;

	[StateDump(Skip = true)]
	private bool m_ExcludedCardsFromShortRest;

	private CAbilityCard m_InitiativeAbilityCard;

	private CAbilityCard m_SubInitiativeAbilityCard;

	private CAbilityCard m_ExtraTurnInitiativeAbilityCard;

	private int m_NumberAbilityCardsInBattle;

	private List<CAbilityCard> m_SelectedAbilityCards;

	private List<CAbilityCard> m_OwnedAbilityCards;

	private int m_Health;

	private int m_Level;

	private CharacterYMLData m_CharacterYML;

	private List<PerksYMLData> m_Perks = new List<PerksYMLData>();

	private HeroSummonYMLData m_CompanionSummonData;

	private int m_TimeTokens;

	private int m_TrophyTokens;

	private int m_ResonanceTokens;

	private static object s_CheckForFinishedActiveBonusesLock = new object();

	public List<CAbilityCard> SelectedAbilityCardsOutOfPhase => m_SelectedAbilityCardsOutOfPhase;

	public List<CAbilityCard> AbilityCardsPool => m_AbilityCardsPool;

	public List<CAbilityCard> SelectedAbilityCards => m_SelectedAbilityCards;

	public List<CAbilityCard> OwnedAbilityCards => m_OwnedAbilityCards;

	public List<CAbilityCard> UnselectedAbilityCards => (from x in m_AbilityCardsPool.Except(SelectedAbilityCards)
		where !x.SupplyCard
		select x).ToList();

	public List<CAbilityCard> SupplyCards => m_SupplyCards;

	public List<CAbilityCard> GivenCards => ActivatedAbilityCards.Where((CAbilityCard x) => x.ClassID != ID).ToList();

	public List<CAbilityCard> RoundAbilityCardsSelectedInCardSelection => m_RoundAbilityCardsSelectedInCardSelection;

	public List<CAbilityCard> RoundAbilityCards => m_RoundAbilityCards;

	public List<CAbilityCard> HandAbilityCards => m_HandAbilityCards;

	public List<CAbilityCard> DiscardedAbilityCards => m_DiscardedAbilityCards;

	public List<CAbilityCard> ExcludedFromShortRestAbilityCards => m_ExcludedFromShortRestAbilityCards;

	public List<CAbilityCard> PermanentlyLostAbilityCards => m_PermanentlyLostAbilityCards;

	public List<CAbilityCard> TakenCards => m_TakenCards;

	public List<CAbilityCard> OwnedUnusedCards => m_OwnedUnusedCards;

	public List<CBaseCard> ActivatedCards => m_ActivatedCards;

	public List<CAbilityCard> ActivatedAbilityCards => (from s in m_ActivatedCards
		where s is CAbilityCard
		select s as CAbilityCard).ToList();

	public List<CAbilityCard> LostAbilityCards => m_LostAbilityCards;

	public List<CAbilityCard> ExtraTurnCards => m_ExtraTurnCards;

	public List<CAbilityCard> NextPendingExtraTurnCards
	{
		get
		{
			if (PendingExtraTurnCardsData.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return PendingExtraTurnCardsData[0].PendingExtraTurnCards;
		}
	}

	public List<CAbilityCard> AllPendingExtraTurnCards
	{
		get
		{
			if (PendingExtraTurnCardsData.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return PendingExtraTurnCardsData.SelectMany((CAbilityExtraTurn.PendingExtraTurnData x) => x.PendingExtraTurnCards).ToList();
		}
	}

	public List<CAbilityExtraTurn.PendingExtraTurnData> PendingExtraTurnCardsData { get; set; }

	public List<CAbilityCard> ExtraTurnCardsSelectedInCardSelection
	{
		get
		{
			if (ExtraTurnCardsSelectedInCardSelectionStack.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return ExtraTurnCardsSelectedInCardSelectionStack.Peek();
		}
	}

	public Stack<List<CAbilityCard>> ExtraTurnCardsSelectedInCardSelectionStack { get; set; }

	public List<AttackModifierYMLData> AttackModifierCards => m_AttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCardsPool => m_AttackModifierCardsPool;

	public List<AttackModifierYMLData> DiscardedAttackModifierCards => m_DiscardedAttackModifierCards;

	[StateDump(Skip = true)]
	public bool HasStartedLongRest { get; set; }

	public bool LongRest
	{
		get
		{
			return m_LongRest;
		}
		set
		{
			m_LongRest = value;
		}
	}

	public bool HasLongRested
	{
		get
		{
			return m_HasLongRested;
		}
		set
		{
			m_HasLongRested = value;
		}
	}

	public bool ShortRest
	{
		get
		{
			return m_ShortRest;
		}
		set
		{
			m_ShortRest = value;
		}
	}

	public bool HasShortRested
	{
		get
		{
			return m_HasShortRested;
		}
		set
		{
			m_HasShortRested = value;
		}
	}

	public bool HasImprovedShortRested
	{
		get
		{
			return m_HasImprovedShortRested;
		}
		set
		{
			m_HasImprovedShortRested = value;
		}
	}

	public bool HasQueuedPreRestBonuses { get; set; }

	public bool ShortRestCardRedrawn { get; set; }

	public int ShortRestCardBurned { get; set; }

	public bool ImprovedShortRest
	{
		get
		{
			return m_ImprovedShortRest;
		}
		set
		{
			m_ImprovedShortRest = value;
		}
	}

	public uint ImprovedShortRestDrawCount
	{
		get
		{
			return m_ImprovedShortRestDrawCount;
		}
		set
		{
			m_ImprovedShortRestDrawCount = value;
		}
	}

	public bool ImprovedShortRestCanExcludeCards
	{
		get
		{
			return _mImprovedShortRestCanExcludeCards;
		}
		set
		{
			_mImprovedShortRestCanExcludeCards = value;
		}
	}

	[StateDump(Skip = true)]
	public bool ExcludedCardsFromShortRest
	{
		get
		{
			return m_ExcludedCardsFromShortRest;
		}
		set
		{
			m_ExcludedCardsFromShortRest = value;
		}
	}

	public CAbilityCard InitiativeAbilityCard => m_InitiativeAbilityCard;

	public CAbilityCard SubInitiativeAbilityCard => m_SubInitiativeAbilityCard;

	public CAbilityCard ExtraTurnInitiativeAbilityCard => m_ExtraTurnInitiativeAbilityCard;

	public int NumberAbilityCardsInBattle => m_NumberAbilityCardsInBattle;

	public bool InitialState { get; set; }

	public string CharacterID => m_CharacterYML.ID;

	public ECharacter CharacterModel => m_CharacterYML.Model;

	public HeroSummonYMLData CompanionSummonData => m_CompanionSummonData;

	public int ModelInstanceID { get; private set; }

	public int[] HealthTable => m_CharacterYML.HealthTable;

	public override List<PerksYMLData> Perks => m_Perks;

	public CharacterYMLData CharacterYML => m_CharacterYML;

	public int TimeTokens
	{
		get
		{
			return m_TimeTokens;
		}
		private set
		{
			m_TimeTokens = value;
		}
	}

	public int TrophyTokens
	{
		get
		{
			return m_TrophyTokens;
		}
		private set
		{
			m_TrophyTokens = value;
		}
	}

	public int ResonanceTokens
	{
		get
		{
			return m_ResonanceTokens;
		}
		private set
		{
			m_ResonanceTokens = value;
		}
	}

	public override int Health()
	{
		return m_Health;
	}

	public CCharacterClass(CharacterYMLData character, List<CAbilityCard> abilityCards, int level)
		: base(character.ID, character.Model.ToString(), character.LocKey, character.OverrideAbilityCardArt)
	{
		CCharacterClass cCharacterClass = this;
		m_CharacterYML = character;
		m_Level = level;
		m_Health = character.HealthTable[level];
		m_NumberAbilityCardsInBattle = character.NumberAbilityCardsInBattle;
		m_AbilityCardsPool = abilityCards.Where((CAbilityCard x) => !x.SupplyCard).ToList();
		m_SupplyCards = abilityCards.Where((CAbilityCard x) => x.SupplyCard).ToList();
		m_AttackModifierCardsPool = character.AttackModifierDeck.GetAllAttackModifiers;
		TimeTokens = character.TimeTokens;
		TrophyTokens = character.TrophyTokens;
		ResonanceTokens = character.ResonanceTokens;
		if (character.CompanionSummonID != null)
		{
			m_CompanionSummonData = ScenarioRuleClient.SRLYML.HeroSummons.Single((HeroSummonYMLData s) => s.ID == character.CompanionSummonID);
		}
		ExtraTurnCardsSelectedInCardSelectionStack = new Stack<List<CAbilityCard>>();
		PendingExtraTurnCardsData = new List<CAbilityExtraTurn.PendingExtraTurnData>();
		Reset();
		ModelInstanceID = (int)(CharacterModel + CharacterClassManager.Classes.Where((CCharacterClass w) => w.CharacterModel == cCharacterClass.CharacterModel).Count() * 1000);
	}

	public override void SortLists()
	{
		m_HandAbilityCards?.Sort(GameState.CompareAbilityCardsByID);
		m_DiscardedAbilityCards?.Sort(GameState.CompareAbilityCardsByID);
		m_ExcludedFromShortRestAbilityCards?.Sort(GameState.CompareAbilityCardsByID);
		m_LostAbilityCards?.Sort(GameState.CompareAbilityCardsByID);
		m_PermanentlyLostAbilityCards?.Sort(GameState.CompareAbilityCardsByID);
		m_SupplyCards?.Sort(GameState.CompareAbilityCardsByID);
	}

	public void SetupRoundAbilityCardsSelectedInCardSelection()
	{
		m_RoundAbilityCardsSelectedInCardSelection.Clear();
		m_RoundAbilityCardsSelectedInCardSelection.AddRange(m_RoundAbilityCards);
	}

	public void MoveAbilityCard(CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromCardPileName, string toCardPileName, bool sendNetworkSelectedCardsWhenDone = false)
	{
		if (!string.IsNullOrEmpty(toCardPileName))
		{
			CBaseCard.ECardPile result;
			if (toCardPileName == "SelectedAbilityCardsOutOfPhase")
			{
				abilityCard.CurrentCardPile = CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase;
			}
			else if (Enum.TryParse<CBaseCard.ECardPile>(toCardPileName.Replace("AbilityCards", ""), ignoreCase: true, out result))
			{
				abilityCard.CurrentCardPile = result;
			}
		}
		else
		{
			abilityCard.CurrentCardPile = CBaseCard.ECardPile.None;
		}
		string[] array = Environment.StackTrace.Split(new string[1] { Environment.NewLine }, 3, StringSplitOptions.None);
		string text = ((array.Length > 1) ? ("\n" + array[1] + ((array.Length > 2) ? ("\n" + array[2]) : "")) : "");
		Log.Info("Moving ability card: " + abilityCard.Name + " from pile: " + fromCardPileName + " to: " + toCardPileName + text);
		bool flag = true;
		CActor actor = GetActor();
		if (actor != null && actor.HasMirrorActor())
		{
			flag = false;
		}
		if (fromAbilityCardList != null)
		{
			if (fromAbilityCardList.Contains(abilityCard))
			{
				fromAbilityCardList.Remove(abilityCard);
			}
			else if (flag)
			{
				Log.ErrorWithReport("Error: Tried to move Ability Card " + abilityCard.Name + " from a list that did not contain it");
			}
		}
		if (toAbilityCardList != null)
		{
			if (!toAbilityCardList.Contains(abilityCard))
			{
				toAbilityCardList.Add(abilityCard);
			}
			else if (flag)
			{
				Log.ErrorWithReport("Error: Tried to move Ability Card " + abilityCard.Name + " into a list that already contained it");
			}
		}
		if (flag)
		{
			CheckCardPilesForDuplicates(abilityCard);
		}
		int count = HandAbilityCards.Count;
		int discardSize = ActivatedAbilityCards.Count((CAbilityCard x) => x != null && x.SelectedAction?.CardPile == CBaseCard.ECardPile.Discarded) + DiscardedAbilityCards.Count;
		SEventLogMessageHandler.AddEventLogMessage(new SEventHand(CharacterID, count, discardSize));
		if ((toCardPileName == "LostAbilityCards" || toCardPileName == "PermanentlyLostAbilityCards") && !(fromCardPileName == "ActiveBonusCards") && (!(PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityModeTransfer { LastSummonKilled: var lastSummonKilled } cAbilityModeTransfer) || (lastSummonKilled?.CachedSummonCard?.ID != abilityCard?.ID && (cAbilityModeTransfer.LastDeactivatedModeCard?.ID != abilityCard?.ID || cAbilityModeTransfer.ModeUsage == CAbilityModeTransfer.EModeUse.DeactivateMode))))
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventLoseCard(CharacterID, GameState.CurrentDamageData?.DamageSourceStrength ?? 0, (GameState.CurrentDamageData?.DamageSourceAbility?.AbilityType).GetValueOrDefault(), abilityCard.ID, toCardPileName));
		}
		if (toCardPileName == "HandAbilityCards")
		{
			if (fromCardPileName == "DiscardedAbilityCards")
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventRecoverCard(CharacterID, wasDiscarded: true));
			}
			else if (fromCardPileName == "LostAbilityCards")
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventRecoverCard(CharacterID, wasDiscarded: false));
			}
		}
		CPlayerFinishedMovingAbilityCard message = new CPlayerFinishedMovingAbilityCard(GetActor())
		{
			m_SendNetworkSelectedRoundCards = sendNetworkSelectedCardsWhenDone
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	private void CheckCardPilesForDuplicates(CAbilityCard abilityCard)
	{
		int num = 0;
		string text = "";
		if (RoundAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "ROUND";
		}
		if (HandAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "HAND, ";
		}
		if (ActivatedAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "ACTIVATED, ";
		}
		if (DiscardedAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "DISCARD, ";
		}
		if (LostAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "LOST, ";
		}
		if (PermanentlyLostAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "PERMALOST, ";
		}
		if (TakenCards.Contains(abilityCard))
		{
			num++;
			text += "TAKEN, ";
		}
		if (OwnedUnusedCards.Contains(abilityCard))
		{
			num++;
			text += "OWNEDUNUSED, ";
		}
		if (num > 1)
		{
			Log.ErrorWithReport("Duplicate card: " + abilityCard.Name + " found in piles: " + text + " after moving cards");
		}
	}

	public void ActivateCard(CBaseCard cardToActivate)
	{
		if (CardActivated(cardToActivate))
		{
			return;
		}
		if (cardToActivate == null)
		{
			Log.ErrorWithReport("Attempting to add a null card to the Activated pile.");
			return;
		}
		if (m_ActivatedCards.Any((CBaseCard x) => cardToActivate.ID == x.ID))
		{
			Log.ErrorWithReport($"Attempting to add a card with id {cardToActivate.ID} when an activated card with that id is already present");
			return;
		}
		m_ActivatedCards.Add(cardToActivate);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public bool CardActivated(CBaseCard cardToActivate)
	{
		return m_ActivatedCards.Contains(cardToActivate);
	}

	public void SetInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		if (m_InitiativeAbilityCard != abilityCard)
		{
			Log.Info($"Changing initiative ability card of {ID} from '{m_InitiativeAbilityCard}' to '{abilityCard}'");
		}
		m_InitiativeAbilityCard = abilityCard;
	}

	public void SetSubInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		if (m_SubInitiativeAbilityCard != abilityCard)
		{
			Log.Info($"Changing sub-initiative ability card of {ID} from '{m_SubInitiativeAbilityCard}' to '{abilityCard}'");
		}
		m_SubInitiativeAbilityCard = abilityCard;
	}

	public void SetExtraTurnInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		if (m_ExtraTurnInitiativeAbilityCard != abilityCard)
		{
			Log.Info($"Changing extra turn initiative ability card of {ID} from '{m_ExtraTurnInitiativeAbilityCard}' to '{abilityCard}'");
		}
		m_ExtraTurnInitiativeAbilityCard = abilityCard;
	}

	public static CBaseCard.ECardPile GetCardPile(string name)
	{
		return ((CBaseCard.ECardPile[])Enum.GetValues(typeof(CBaseCard.ECardPile))).SingleOrDefault((CBaseCard.ECardPile x) => x.ToString() == name);
	}

	public List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed, CAbilityAttack attackAbility, CActor currentTarget, out List<AttackModifierYMLData> cardsDrawnTotal, CAttackSummary.TargetSummary targetSummary)
	{
		List<AttackModifierYMLData> result = CActor.DrawAttackModifierCards(actor, attackStrength, advStatus, m_AttackModifierCards, m_DiscardedAttackModifierCards, out var shuffle, out notUsed, attackAbility, currentTarget, out cardsDrawnTotal, targetSummary);
		m_ShuffleAttackModifierCards |= shuffle;
		return result;
	}

	private void MoveAbilityCardToPile(CAbilityCard abilityCard, List<CAbilityCard> pileToMoveFrom, string pileToMoveFromName)
	{
		if (!string.IsNullOrEmpty(abilityCard?.ClassID))
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == abilityCard.ClassID);
			if (cCharacterClass != null && cCharacterClass != this)
			{
				cCharacterClass.MoveAbilityCardToPile(abilityCard, pileToMoveFrom, pileToMoveFromName);
				return;
			}
		}
		if (abilityCard.SupplyCard)
		{
			pileToMoveFrom?.Remove(abilityCard);
			CharacterClassManager.AllAbilityCardInstances.Remove(abilityCard);
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass == this);
			if (cPlayerActor != null)
			{
				CSupplyCardUsed_MessageData cSupplyCardUsed_MessageData = new CSupplyCardUsed_MessageData(cPlayerActor);
				cSupplyCardUsed_MessageData.m_ActorUsedCard = cPlayerActor;
				cSupplyCardUsed_MessageData.m_SupplyCardUsed = abilityCard;
				ScenarioRuleClient.MessageHandler(cSupplyCardUsed_MessageData);
			}
			return;
		}
		if (abilityCard.SelectedAction != null)
		{
			abilityCard.LastPlayedActionType = abilityCard.GetActionType(abilityCard.SelectedAction);
		}
		CBaseCard.ECardPile eCardPile = CBaseCard.ECardPile.Discarded;
		CBaseCard.ECardPile eCardPile2 = CBaseCard.ECardPile.None;
		if (abilityCard.SelectedAction != null)
		{
			eCardPile = ((!abilityCard.ActionHasHappened) ? CBaseCard.ECardPile.Discarded : abilityCard.SelectedAction.CardPile);
		}
		if (abilityCard.GetAllAbilities().Any((CAbility x) => x.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.Tide) && x.ActiveBonusData.GetMiscVar("MoveCardToLostPile").GetBool()))
		{
			eCardPile = CBaseCard.ECardPile.Lost;
		}
		if (abilityCard.ActiveBonuses.Any((CActiveBonus bonus) => bonus.Duration != CActiveBonus.EActiveBonusDurationType.NA))
		{
			eCardPile = CBaseCard.ECardPile.Activated;
		}
		if (ScenarioScript.current != null)
		{
			CActor actor = GameState.InternalCurrentActor ?? ScenarioManager.Scenario.AllActors.First((CActor x) => x.CharacterClass == this);
			eCardPile = ScenarioScript.current.OverrideMoveAbilityCardToPile(actor, eCardPile, abilityCard, pileToMoveFromName);
		}
		if (abilityCard.PendingCardPile != CBaseCard.ECardPile.None)
		{
			eCardPile = abilityCard.PendingCardPile;
		}
		if (abilityCard.CurrentCardPile == eCardPile)
		{
			return;
		}
		if (abilityCard.CurrentCardPile == CBaseCard.ECardPile.Activated && eCardPile != CBaseCard.ECardPile.Activated)
		{
			CRefreshActiveBonusUI_MessageData message = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
			{
				m_Actor = GameState.InternalCurrentActor
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		abilityCard.CurrentCardPile = eCardPile;
		switch (eCardPile)
		{
		case CBaseCard.ECardPile.Activated:
		{
			if (pileToMoveFrom.Contains(abilityCard))
			{
				pileToMoveFrom?.Remove(abilityCard);
				if (!m_ActivatedCards.Contains(abilityCard) && abilityCard.CurrentCardPile != CBaseCard.ECardPile.Activated)
				{
					if (abilityCard == null)
					{
						Log.ErrorWithReport("Attempting to add a null card to the Activated pile.");
						return;
					}
					m_ActivatedCards.Add(abilityCard);
					foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
					{
						allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
					}
				}
			}
			CRefreshActiveBonusUI_MessageData message2 = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
			{
				m_Actor = GameState.InternalCurrentActor
			};
			ScenarioRuleClient.MessageHandler(message2);
			CBaseCard.ECardPile? eCardPile3 = abilityCard.SelectedAction?.CardPile;
			eCardPile2 = ((eCardPile3 == CBaseCard.ECardPile.Discarded) ? CBaseCard.ECardPile.Discarded : CBaseCard.ECardPile.Lost);
			if (eCardPile2 == CBaseCard.ECardPile.Lost)
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventLoseCard(CharacterID, GameState.CurrentDamageData?.DamageSourceStrength ?? 0, (GameState.CurrentDamageData?.DamageSourceAbility?.AbilityType).GetValueOrDefault(), abilityCard.ID, (eCardPile3 == CBaseCard.ECardPile.PermanentlyLost) ? "PermanentlyLostAbilityCards" : "LostAbilityCards"));
			}
			break;
		}
		case CBaseCard.ECardPile.Lost:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, LostAbilityCards, pileToMoveFromName, "LostAbilityCards");
			break;
		case CBaseCard.ECardPile.Discarded:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, DiscardedAbilityCards, pileToMoveFromName, "DiscardedAbilityCards");
			break;
		case CBaseCard.ECardPile.PermanentlyLost:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, PermanentlyLostAbilityCards, pileToMoveFromName, "PermanentlyLostAbilityCards");
			break;
		case CBaseCard.ECardPile.Taken:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, TakenCards, pileToMoveFromName, "TakenAbilityCards");
			break;
		case CBaseCard.ECardPile.OwnedUnused:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, OwnedUnusedCards, pileToMoveFromName, "OwnedUnusedAbilityCards");
			break;
		}
		if (pileToMoveFrom != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventDiscardCard(ID, abilityCard.ID, (eCardPile2 == CBaseCard.ECardPile.None) ? eCardPile : eCardPile2));
		}
	}

	public List<CAbilityCard> GetCardPileList(CBaseCard.ECardPile pile)
	{
		return pile switch
		{
			CBaseCard.ECardPile.Lost => LostAbilityCards, 
			CBaseCard.ECardPile.PermanentlyLost => PermanentlyLostAbilityCards, 
			CBaseCard.ECardPile.Taken => TakenCards, 
			CBaseCard.ECardPile.OwnedUnused => OwnedUnusedCards, 
			CBaseCard.ECardPile.Discarded => DiscardedAbilityCards, 
			CBaseCard.ECardPile.Activated => ActivatedAbilityCards, 
			CBaseCard.ECardPile.Hand => HandAbilityCards, 
			CBaseCard.ECardPile.Round => RoundAbilityCards, 
			CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase => SelectedAbilityCardsOutOfPhase, 
			_ => null, 
		};
	}

	public static string GetCardPileListKey(CBaseCard.ECardPile pile)
	{
		return pile.ToString() + "AbilityCards";
	}

	public void DiscardRoundAbilityCards(bool extraTurn = false)
	{
		if (extraTurn)
		{
			for (int num = ExtraTurnCards.Count - 1; num >= 0; num--)
			{
				MoveAbilityCardToPile(ExtraTurnCards[num], ExtraTurnCards, "ExtraTurnCards");
			}
		}
		else
		{
			for (int num2 = RoundAbilityCards.Count - 1; num2 >= 0; num2--)
			{
				MoveAbilityCardToPile(RoundAbilityCards[num2], RoundAbilityCards, "RoundAbilityCards");
			}
		}
	}

	public void ClearRoundAbilityCards()
	{
		foreach (CAbilityCard item in m_RoundAbilityCards.ToList())
		{
			MoveAbilityCard(item, m_RoundAbilityCards, m_HandAbilityCards, "RoundAbilityCards", "HandAbilityCards");
		}
	}

	public void DiscardRoundAbilityCard(CAbilityCard roundAbilityCard)
	{
		MoveAbilityCardToPile(roundAbilityCard, RoundAbilityCards, "RoundAbilityCards");
	}

	public void DiscardExtraTurnAbilityCard(CAbilityCard extraTurnAbilityCard)
	{
		MoveAbilityCardToPile(extraTurnAbilityCard, ExtraTurnCards, "ExtraTurnCards");
	}

	public void DiscardOutOfPhasenAbilityCard(CAbilityCard extraTurnAbilityCard)
	{
		MoveAbilityCardToPile(extraTurnAbilityCard, SelectedAbilityCardsOutOfPhase, "SelectedAbilityCardsOutOfPhase");
	}

	public void RestoreCachedAugmentOrSongAbilityCard(CAbilityCard roundAbilityCard)
	{
		roundAbilityCard.ActionHasHappened = true;
		if (DiscardedAbilityCards.Contains(roundAbilityCard))
		{
			DiscardedAbilityCards.Remove(roundAbilityCard);
			roundAbilityCard.SetSelectedAction(roundAbilityCard.TopAction);
			roundAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Activated;
			if (!m_ActivatedCards.Contains(roundAbilityCard))
			{
				if (roundAbilityCard == null)
				{
					Log.ErrorWithReport("Attempting to add a null card to the Activated pile.");
					return;
				}
				m_ActivatedCards.Add(roundAbilityCard);
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
				}
			}
			CheckCardPilesForDuplicates(roundAbilityCard);
		}
		CRefreshActiveBonusUI_MessageData message = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
		{
			m_Actor = GameState.InternalCurrentActor
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public void CheckAttackModifierCardShuffle(bool force = false)
	{
		if (m_ShuffleAttackModifierCards | force)
		{
			if (GameState.ShuffleAttackModsEnabledForPlayers)
			{
				CActor.ShuffleAttackModifierCards(m_AttackModifierCards, m_DiscardedAttackModifierCards);
			}
			m_ShuffleAttackModifierCards = false;
		}
	}

	public override CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.HasAbility(ability))
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.HasAbility(ability))
			{
				return item;
			}
		}
		IEnumerable<CItem> enumerable = GetActor()?.Inventory?.AllItemsAndPets;
		if (enumerable != null)
		{
			foreach (CItem item2 in enumerable)
			{
				if (item2.YMLData.Data.Abilities != null && item2.FindAbilityOnItemCard(null, ability.ID) != null)
				{
					return item2;
				}
			}
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			if (itemCard.Data.Abilities != null && itemCard.Data.Abilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return itemCard.GetItem;
			}
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			if (attackModifier.Abilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return attackModifier.Card;
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
			foreach (CAbility item3 in scenarioModifier.AllListedTriggerAbilities(copyAbilities: false))
			{
				if (item3 != null && item3.HasID(ability.ID))
				{
					return scenarioModifier;
				}
			}
		}
		if (ability.VarsContext.GetVar("AttackSummaryModInfo").GetObject<CAbilityAttack.AttackModifierAbilityInfo>() == null)
		{
			CActiveBonus cActiveBonus = ability.VarsContext.GetVar("ActiveBonus").GetObject<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Ability == ability)
					{
						return activatedCard;
					}
					if (cActiveBonus == activeBonuse)
					{
						return activatedCard;
					}
				}
			}
		}
		return base.FindCardWithAbility(ability, actor);
	}

	public CAbilityCard FindCardWithID(int id)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ID == id)
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.ID == id)
			{
				return item;
			}
		}
		return null;
	}

	public override CBaseCard FindCard(int id, string name)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ID == id && roundAbilityCard.Name == name)
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.ID == id && item.Name == name)
			{
				return item;
			}
		}
		foreach (CBaseCard activatedCard in m_ActivatedCards)
		{
			if (activatedCard is CItem cItem && cItem.ID == id && cItem.Name == name)
			{
				return cItem;
			}
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			if (attackModifier.Name == name)
			{
				return attackModifier.Card;
			}
		}
		return null;
	}

	public List<CActiveBonus> FindActiveBonusAuras(CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
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

	public List<CActiveBonus> FindCasterActiveBonuses(CActor caster)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (!list.Contains(activeBonuse) && caster == activeBonuse.Caster)
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
				return FindCasterActiveBonuses(caster);
			}
			throw ex;
		}
	}

	public override IEnumerable<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		lock (m_ActivatedCards)
		{
			foreach (CBaseCard activatedCard in ActivatedCards)
			{
				if (actor == null)
				{
					lock (activatedCard.ActiveBonuses)
					{
						foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
						{
							yield return activeBonuse;
						}
					}
					continue;
				}
				foreach (CActiveBonus item in activatedCard.GetActiveBonusesForActor(actor))
				{
					yield return item;
				}
			}
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			if (m_ActivatedCards.Any((CBaseCard a) => a == null))
			{
				Log.Warning("m_ActivatedCards contains null entries!");
				m_ActivatedCards.RemoveAll((CBaseCard r) => r == null);
			}
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				if (activatedCard.ActiveBonuses.Any((CActiveBonus a) => a == null))
				{
					Log.Warning("Ability Card " + activatedCard.Name + " contains null entries in the ActiveBonuses list!");
					activatedCard.ActiveBonuses.RemoveAll((CActiveBonus r) => r == null);
				}
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Type() == type && (activeBonuse.Actor as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else
					{
						if (activeBonuse.Type() != type || list.Contains(activeBonuse))
						{
							continue;
						}
						if (activeBonuse.IsAura)
						{
							if (activeBonuse.ValidActorsInRangeOfAura.Contains(actor))
							{
								list.Add(activeBonuse);
							}
						}
						else if (actor == activeBonuse.Actor)
						{
							list.Add(activeBonuse);
						}
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(type, actor);
			}
			throw ex;
		}
		catch (NullReferenceException ex2)
		{
			string text = "Null Objects: ";
			if (actor == null)
			{
				text += "actor ";
			}
			if (m_ActivatedCards == null)
			{
				text += "m_ActivatedCards ";
			}
			Log.ErrorWithReport(ex2.Message + "\n" + text + "\n" + ex2.StackTrace);
			throw ex2;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Ability.ActiveBonusData.HasBehaviour(behaviour) && (activeBonuse.Actor as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse) && activeBonuse.IsSong)
						{
							list.Add(activeBonuse);
						}
					}
					else
					{
						if (!activeBonuse.Ability.ActiveBonusData.HasBehaviour(behaviour) || list.Contains(activeBonuse))
						{
							continue;
						}
						if (activeBonuse.IsAura)
						{
							if (activeBonuse.ValidActorsInRangeOfAura.Contains(actor))
							{
								list.Add(activeBonuse);
							}
						}
						else if (actor == activeBonuse.Actor)
						{
							list.Add(activeBonuse);
						}
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(behaviour, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindCasterActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Ability.ActiveBonusData.HasBehaviour(behaviour) && (activeBonuse.Caster as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse) && activeBonuse.IsSong)
						{
							list.Add(activeBonuse);
						}
					}
					else if (activeBonuse.Ability.ActiveBonusData.HasBehaviour(behaviour) && !list.Contains(activeBonuse) && actor == activeBonuse.Caster)
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
				return FindCasterActiveBonuses(behaviour, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			if (durationType == CActiveBonus.EActiveBonusDurationType.Summon)
			{
				foreach (CBaseCard activatedCard in m_ActivatedCards)
				{
					activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && (x.Actor as CHeroSummonActor)?.Summoner == actor)
						where !activeBonuses.Contains(w)
						select w);
				}
			}
			else
			{
				foreach (CBaseCard activatedCard2 in m_ActivatedCards)
				{
					activeBonuses.AddRange(from w in activatedCard2.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && x.Actor == actor)
						where !activeBonuses.Contains(w)
						select w);
				}
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(durationType, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindAllSongActiveBonuses(CActor actor, CActor soothsinger)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.IsSong && x.Ability.Song.Filter.IsValidTarget(actor, soothsinger, isTargetedAbility: false, actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false))
					where !activeBonuses.Contains(w)
					select w);
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindAllSongActiveBonuses(actor, soothsinger);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindAllDoomActiveBonuses(CActor actor = null)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.IsDoom && (actor == null || x.Actor == actor))
					where !activeBonuses.Contains(w)
					select w);
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindAllDoomActiveBonuses(actor);
			}
			throw ex;
		}
	}

	public bool HasActiveBonus(CActiveBonus activeBonus)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ActiveBonuses.Contains(activeBonus))
			{
				return true;
			}
		}
		foreach (CBaseCard activatedCard in m_ActivatedCards)
		{
			if (activatedCard.ActiveBonuses.Contains(activeBonus))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckForFinishedActiveBonuses(CActor actor, bool important = false, bool allowFromMainThread = false)
	{
		if (!ScenarioRuleClient.IsWorkerThread && !allowFromMainThread)
		{
			bool flag = ScenarioRuleClient.CheckFinishedBonuses(actor, important);
			if (!GameState.ThreadIsSleeping | flag)
			{
				return false;
			}
		}
		lock (s_CheckForFinishedActiveBonusesLock)
		{
			bool result = false;
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
			{
				foreach (CActiveBonus item2 in new List<CActiveBonus>(roundAbilityCard.ActiveBonuses))
				{
					if (item2.Finished() || item2.Finishing() || item2.Duration == CActiveBonus.EActiveBonusDurationType.NA)
					{
						roundAbilityCard.ActiveBonuses.Remove(item2);
					}
					if (roundAbilityCard.ActiveBonuses.Count != 0)
					{
						continue;
					}
					foreach (CCharacterClass item3 in ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor x) => x.CharacterClass))
					{
						item3.m_ActivatedCards.Remove(roundAbilityCard);
					}
					foreach (CMonsterClass item4 in ScenarioManager.Scenario.AllMonsters.Select((CEnemyActor x) => x.MonsterClass))
					{
						item4.ActivatedCards.Remove(roundAbilityCard);
					}
					if ((PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == null || PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability.GetAbilityBaseCard() != roundAbilityCard) && GameState.RoundAbilityCardSelected != roundAbilityCard && !list.Contains(roundAbilityCard))
					{
						list.Add(roundAbilityCard);
					}
				}
			}
			foreach (CAbilityCard item5 in list)
			{
				MoveAbilityCardToPile(item5, RoundAbilityCards, "RoundAbilityCards");
				result = true;
			}
			list.Clear();
			foreach (CBaseCard item6 in m_ActivatedCards.ToList())
			{
				foreach (CActiveBonus item7 in new List<CActiveBonus>(item6.ActiveBonuses))
				{
					if (item7.Finished() || item7.Finishing() || item7.Duration == CActiveBonus.EActiveBonusDurationType.NA)
					{
						item6.ActiveBonuses.Remove(item7);
					}
					if (item6.ActiveBonuses.Count != 0)
					{
						continue;
					}
					foreach (CCharacterClass item8 in ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor x) => x.CharacterClass))
					{
						item8.m_ActivatedCards.Remove(item6);
					}
					foreach (CMonsterClass item9 in ScenarioManager.Scenario.AllMonsters.Select((CEnemyActor x) => x.MonsterClass))
					{
						item9.ActivatedCards.Remove(item6);
					}
					if (item6.CardType == CBaseCard.ECardType.CharacterAbility && !list.Contains(item6))
					{
						list.Add(item6 as CAbilityCard);
					}
					CItem item = item6 as CItem;
					if (item != null && item.SlotState != CItem.EItemSlotState.TempLocked && item.SlotState != CItem.EItemSlotState.Consumed && item.SlotState != CItem.EItemSlotState.Spent)
					{
						CItem cItem = actor.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == item.ID);
						if (cItem != null)
						{
							cItem.SlotState = CItem.EItemSlotState.Active;
							actor.Inventory.HandleUsedItem(cItem, item7);
						}
					}
				}
			}
			foreach (CAbilityCard item10 in list)
			{
				MoveAbilityCardToPile(item10, null, "ActiveBonusCards");
				result = true;
			}
			return result;
		}
	}

	public void ResetCards()
	{
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			item.Reset();
		}
	}

	public void ResetCardAbilitiesAndEnhancementsOnly()
	{
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			item.ResetAbilitiesAndEnhancementsOnly();
		}
	}

	public static string AbilityCardsToString(List<CAbilityCard> list)
	{
		if (list == null)
		{
			return "null";
		}
		string text = $"[{list.Count}] {{";
		for (int i = 0; i < list.Count; i++)
		{
			CAbilityCard cAbilityCard = list[i];
			if (i > 0)
			{
				text += ", ";
			}
			text += cAbilityCard.ID;
		}
		return text + " }";
	}

	public override void Reset()
	{
		base.Reset();
		if (AbilityDeckState.DebugCardID > 0 && m_HandAbilityCards.Any((CAbilityCard c) => c.ID == AbilityDeckState.DebugCardID))
		{
			Log.Info("Cleared hand AbilityDeck on Reset: " + AbilityCardsToString(m_HandAbilityCards));
		}
		InitialState = true;
		m_HandAbilityCards.Clear();
		m_DiscardedAbilityCards.Clear();
		m_ExcludedFromShortRestAbilityCards.Clear();
		m_LostAbilityCards.Clear();
		m_PermanentlyLostAbilityCards.Clear();
		m_TakenCards.Clear();
		m_OwnedUnusedCards.Clear();
		m_ActivatedCards.Clear();
		m_RoundAbilityCardsSelectedInCardSelection.Clear();
		m_RoundAbilityCards.Clear();
		m_Perks.Clear();
		m_AttackModifierCards.Clear();
		m_DiscardedAttackModifierCards.Clear();
		ExtraTurnCards.Clear();
		ExtraTurnCardsSelectedInCardSelectionStack.Clear();
		m_SelectedAbilityCardsOutOfPhase.Clear();
		PendingExtraTurnCardsData.Clear();
		TemporaryCards.Clear();
		ResetCards();
		m_ShuffleAttackModifierCards = false;
		m_HasLongRested = false;
		m_HasShortRested = false;
		m_HasImprovedShortRested = false;
		m_ShortRest = false;
		m_LongRest = false;
		m_ImprovedShortRest = false;
		_mImprovedShortRestCanExcludeCards = false;
		m_ExcludedCardsFromShortRest = false;
		ShortRestCardRedrawn = false;
		ShortRestCardBurned = 0;
		m_InitiativeAbilityCard = null;
		m_SubInitiativeAbilityCard = null;
		m_ExtraTurnInitiativeAbilityCard = null;
		if (m_SelectedAbilityCards == null)
		{
			m_OwnedAbilityCards = new List<CAbilityCard>();
			int num = NumberAbilityCardsInBattle;
			if (m_NumberAbilityCardsInBattle > m_AbilityCardsPool.Count)
			{
				Log.ErrorWithReport("Number of cards taken in to battle by " + ID + " (" + NumberAbilityCardsInBattle + ") is greater than the number of loaded ability cards available (" + m_AbilityCardsPool.Count + ")");
				num = m_AbilityCardsPool.Count;
			}
			for (int num2 = 0; num2 < num; num2++)
			{
				m_AbilityCardsPool[num2].CurrentCardPile = CBaseCard.ECardPile.Hand;
				m_HandAbilityCards.Add(m_AbilityCardsPool[num2]);
				m_OwnedAbilityCards.Add(m_AbilityCardsPool[num2]);
			}
			m_OwnedAbilityCards.Sort((CAbilityCard a, CAbilityCard b) => a.ID.CompareTo(b.ID));
			if (AbilityDeckState.DebugCardID > 0 && m_HandAbilityCards.Any((CAbilityCard c) => c.ID == AbilityDeckState.DebugCardID))
			{
				Log.Info("Recreated hand AbilityDeck on Reset from pool: " + AbilityCardsToString(m_HandAbilityCards));
			}
		}
		else
		{
			foreach (CAbilityCard ownedAbilityCard in OwnedAbilityCards)
			{
				ownedAbilityCard.CurrentCardPile = CBaseCard.ECardPile.OwnedUnused;
				ownedAbilityCard.PendingCardPile = CBaseCard.ECardPile.None;
			}
			int num3 = 0;
			foreach (CAbilityCard selectedAbilityCard in m_SelectedAbilityCards)
			{
				selectedAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Hand;
				m_HandAbilityCards.Add(selectedAbilityCard);
				if (++num3 == m_NumberAbilityCardsInBattle)
				{
					break;
				}
			}
			m_OwnedUnusedCards.AddRange(m_OwnedAbilityCards.Where((CAbilityCard c) => !m_SelectedAbilityCards.Contains(c)));
			if (AbilityDeckState.DebugCardID > 0 && m_HandAbilityCards.Any((CAbilityCard c) => c.ID == AbilityDeckState.DebugCardID))
			{
				Log.Info($"Recreated hand AbilityDeck on Reset from {m_SelectedAbilityCards.Count} selected: {AbilityCardsToString(m_HandAbilityCards)}");
			}
		}
		m_AttackModifierCardsPool = m_CharacterYML.AttackModifierDeck.GetAllAttackModifiers;
		ApplyPerksToList(m_Perks, ref m_AttackModifierCardsPool);
	}

	public void ResetAttackModifierDeck(bool onlyShuffleExisting = false)
	{
		if (onlyShuffleExisting && m_AttackModifierCards.Count > 0)
		{
			m_AttackModifierCards.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
			return;
		}
		m_AttackModifierCards.Clear();
		m_DiscardedAttackModifierCards.Clear();
		if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.ScenarioRNGNotNull)
		{
			foreach (AttackModifierYMLData item in m_AttackModifierCardsPool)
			{
				m_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, m_AttackModifierCards.Count), item);
			}
			return;
		}
		m_AttackModifierCards = m_AttackModifierCardsPool.ToList();
	}

	public void SetHand(List<CAbilityCard> selectedAbilityCards, List<CAbilityCard> ownedAbilityCards)
	{
		m_SelectedAbilityCards = selectedAbilityCards;
		m_OwnedAbilityCards = ownedAbilityCards;
		m_OwnedAbilityCards.Sort((CAbilityCard a, CAbilityCard b) => a.ID.CompareTo(b.ID));
		Reset();
	}

	public override void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
		if ((ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).m_AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count()).Sum() < 10) | canGoOverLimit)
		{
			Log.Info(ID + " has added a curse card to their attack modifier deck.");
			CClass.AddCurseCardToDeck(m_AttackModifierCards);
		}
		else
		{
			Log.Info("Curse card was not added as there are already 10 curse cards in the attack modifier deck");
		}
	}

	public override int RemoveCurses(CActor actor)
	{
		List<AttackModifierYMLData> list = m_AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).ToList();
		foreach (AttackModifierYMLData item in list)
		{
			m_AttackModifierCards.Remove(item);
		}
		return list.Count;
	}

	public override void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
		if ((ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).m_AttackModifierCards.Where((AttackModifierYMLData x) => x.IsBless).Count()).Sum() + MonsterClassManager.MonsterBlessCount < 10) | canGoOverLimit)
		{
			Log.Info(ID + " has added a bless card to their attack modifier deck.");
			CClass.AddBlessCardToDeck(m_AttackModifierCards);
		}
		else
		{
			Log.Info("Bless card was not added as there are already 10 bless cards in the attack modifier deck");
		}
	}

	public override void RemoveBlesses(CActor actor)
	{
		m_AttackModifierCards.RemoveAll((AttackModifierYMLData x) => x.IsBless);
	}

	public void AddPerk(PerksYMLData perk)
	{
		m_Perks.Add(perk);
	}

	public void ApplyPerk(PerksYMLData perk)
	{
		m_Perks.Add(perk);
		ApplyPerksToList(new List<PerksYMLData> { perk }, ref m_AttackModifierCardsPool);
		ResetAttackModifierDeck();
	}

	public static void ApplyPerksToList(List<PerksYMLData> applyThesePerks, ref List<AttackModifierYMLData> applyToThisList)
	{
		foreach (PerksYMLData applyThesePerk in applyThesePerks)
		{
			foreach (AttackModifierYMLData item2 in applyThesePerk.CardsToAdd)
			{
				applyToThisList.Add(item2.Copy());
			}
			foreach (AttackModifierYMLData am in applyThesePerk.CardsToRemove)
			{
				if (applyToThisList.Any((AttackModifierYMLData a) => a.Name == am.Name))
				{
					AttackModifierYMLData item = applyToThisList.First((AttackModifierYMLData f) => f.Name == am.Name);
					applyToThisList.Remove(item);
				}
			}
		}
	}

	public void ApplyItemModifiersToList(List<CItem> items)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (PerksYMLData perk in m_Perks)
		{
			if ((perk.Flags & EPerkFlag.IgnoreNegativeItemEffects) != 0)
			{
				flag = true;
			}
			else if ((perk.Flags & EPerkFlag.IgnoreMinusOneItemEffects) != 0)
			{
				flag2 = true;
			}
		}
		foreach (CItem item in items)
		{
			if (item.YMLData.Data.RemoveModifiers.Count > 0)
			{
				foreach (KeyValuePair<AttackModifierYMLData, int> modifierEntry in item.YMLData.Data.RemoveModifiers)
				{
					for (int i = 0; i < modifierEntry.Value; i++)
					{
						AttackModifierYMLData attackModifierYMLData = m_AttackModifierCardsPool.FirstOrDefault((AttackModifierYMLData s) => s.Name == modifierEntry.Key.Name);
						if (attackModifierYMLData != null)
						{
							m_AttackModifierCardsPool.Remove(attackModifierYMLData);
						}
					}
				}
			}
			if (item.YMLData.Data.AdditionalModifiers.Count <= 0)
			{
				continue;
			}
			foreach (KeyValuePair<AttackModifierYMLData, int> additionalModifier in item.YMLData.Data.AdditionalModifiers)
			{
				AttackModifierYMLData key = additionalModifier.Key;
				int num = (int)Math.Round(GameState.s_MathParser.Parse(key.MathModifier));
				if (!((num <= 0) & flag) && !((num == -1) & flag2))
				{
					int value = additionalModifier.Value;
					for (int num2 = 0; num2 < value; num2++)
					{
						m_AttackModifierCardsPool.Add(key.Copy());
					}
				}
			}
		}
		ResetAttackModifierDeck();
	}

	public void ApplyEventModifiersToList(Dictionary<string, int> modifiers)
	{
		foreach (KeyValuePair<string, int> modifierEntry in modifiers)
		{
			for (int i = 0; i < modifierEntry.Value; i++)
			{
				AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == modifierEntry.Key);
				if (attackModifierYMLData != null)
				{
					m_AttackModifierCardsPool.Add(attackModifierYMLData.Copy());
				}
			}
		}
		ResetAttackModifierDeck();
	}

	public void ApplyEnhancements(List<CEnhancement> enhancements)
	{
		foreach (CEnhancement enhancement in enhancements)
		{
			CAbilityCard cAbilityCard = AbilityCardsPool.SingleOrDefault((CAbilityCard s) => s.ID == enhancement.AbilityCardID);
			if (cAbilityCard != null)
			{
				CAbility cAbility = cAbilityCard.FindAbilityOnCard(enhancement.AbilityName);
				if (cAbility != null)
				{
					cAbility.EnhanceAbility(enhancement);
				}
				else
				{
					Log.Error("Unable to find ability with Name " + enhancement.AbilityName + " on card " + cAbilityCard.Name);
				}
			}
			else
			{
				Log.Error("Unable to find card with ID " + enhancement.AbilityCardID + " on character " + ID);
			}
		}
	}

	public void LoadAbilityDeck(AbilityDeckState deck)
	{
		InitialState = deck.InitialState;
		deck.LogIfContainsDebugCardID("LoadAbilityDeck");
		if (deck.SelectedAbilityCardIDsAndInstanceIDs != null)
		{
			try
			{
				m_SelectedAbilityCards.Clear();
				foreach (Tuple<int, int> selectedAbilityCardTuple in deck.SelectedAbilityCardIDsAndInstanceIDs)
				{
					CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == selectedAbilityCardTuple.Item1);
					if (cAbilityCard.SupplyCard)
					{
						CAbilityCard cAbilityCard2 = cAbilityCard.Copy(selectedAbilityCardTuple.Item2);
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard2);
						cAbilityCard = cAbilityCard2;
					}
					m_SelectedAbilityCards.Add(cAbilityCard);
				}
			}
			catch
			{
				throw new Exception("Failed to load in SelectedAbilityCards from save data");
			}
		}
		if (deck.OwnedAbilityCardIDs != null)
		{
			try
			{
				m_OwnedAbilityCards.Clear();
				foreach (int ownedAbilityCardID in deck.OwnedAbilityCardIDs)
				{
					CAbilityCard item = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == ownedAbilityCardID);
					m_OwnedAbilityCards.Add(item);
				}
				m_OwnedAbilityCards.Sort((CAbilityCard a, CAbilityCard b) => a.ID.CompareTo(b.ID));
			}
			catch
			{
				throw new Exception("Failed to load in SelectedAbilityCards from save data");
			}
		}
		if (deck.HandAbilityCardIDsAndInstanceIDs != null)
		{
			try
			{
				m_HandAbilityCards.Clear();
				foreach (Tuple<int, int> handAbilityCardTuple in deck.HandAbilityCardIDsAndInstanceIDs)
				{
					CAbilityCard cAbilityCard3 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == handAbilityCardTuple.Item1);
					if (cAbilityCard3.SupplyCard)
					{
						CAbilityCard cAbilityCard4 = cAbilityCard3.Copy(handAbilityCardTuple.Item2);
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard4);
						cAbilityCard3 = cAbilityCard4;
					}
					m_HandAbilityCards.Add(cAbilityCard3);
					cAbilityCard3.CurrentCardPile = CBaseCard.ECardPile.Hand;
				}
			}
			catch
			{
				throw new Exception("Failed to load in HandAbilityCards from save data");
			}
		}
		if (deck.RoundAbilityCardIDsAndInstanceID != null)
		{
			try
			{
				m_RoundAbilityCards.Clear();
				foreach (Tuple<int, int> item3 in deck.RoundAbilityCardIDsAndInstanceID)
				{
					int roundAbilityCardID = item3.Item1;
					CAbilityCard cAbilityCard5 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == roundAbilityCardID);
					if (cAbilityCard5.SupplyCard)
					{
						CAbilityCard cAbilityCard6 = cAbilityCard5.Copy();
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard6);
						cAbilityCard5 = cAbilityCard6;
					}
					m_RoundAbilityCards.Add(cAbilityCard5);
					cAbilityCard5.CurrentCardPile = CBaseCard.ECardPile.Round;
				}
			}
			catch
			{
				throw new Exception("Failed to load in RoundAbilityCards from save data");
			}
		}
		if (deck.RoundCardsSelectedInCardSelectionIDsAndInstanceID != null)
		{
			try
			{
				m_RoundAbilityCardsSelectedInCardSelection.Clear();
				foreach (Tuple<int, int> item4 in deck.RoundCardsSelectedInCardSelectionIDsAndInstanceID)
				{
					int roundAbilityCardID2 = item4.Item1;
					CAbilityCard cAbilityCard7 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == roundAbilityCardID2);
					if (cAbilityCard7.SupplyCard)
					{
						CAbilityCard cAbilityCard8 = cAbilityCard7.Copy();
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard8);
						cAbilityCard7 = cAbilityCard8;
					}
					m_RoundAbilityCardsSelectedInCardSelection.Add(cAbilityCard7);
					cAbilityCard7.CurrentCardPile = CBaseCard.ECardPile.Round;
				}
			}
			catch
			{
				throw new Exception("Failed to load in RoundAbilityCards from save data");
			}
		}
		if (deck.DiscardedAbilityCardIDs != null)
		{
			try
			{
				m_DiscardedAbilityCards = deck.DiscardedAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
				foreach (CAbilityCard discardedAbilityCard in m_DiscardedAbilityCards)
				{
					discardedAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Discarded;
				}
			}
			catch
			{
				throw new Exception("Failed to load in DiscardedAbilityCards from save data");
			}
		}
		if (deck.LostAbilityCardIDs != null)
		{
			try
			{
				m_LostAbilityCards = deck.LostAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
				foreach (CAbilityCard lostAbilityCard in m_LostAbilityCards)
				{
					lostAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Lost;
				}
			}
			catch
			{
				throw new Exception("Failed to load in LostAbilityCards from save data");
			}
		}
		if (deck.PermaLostAbilityCardIDs != null)
		{
			try
			{
				m_PermanentlyLostAbilityCards = deck.PermaLostAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
				foreach (CAbilityCard permanentlyLostAbilityCard in m_PermanentlyLostAbilityCards)
				{
					permanentlyLostAbilityCard.CurrentCardPile = CBaseCard.ECardPile.PermanentlyLost;
				}
			}
			catch
			{
				throw new Exception("Failed to load in PermaLostAbilityCards from save data");
			}
		}
		if (deck.TakenCardIDs != null)
		{
			try
			{
				m_TakenCards = deck.TakenCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
				foreach (CAbilityCard takenCard in TakenCards)
				{
					takenCard.CurrentCardPile = CBaseCard.ECardPile.Taken;
				}
			}
			catch
			{
				throw new Exception("Failed to load in TakenCards from save data");
			}
		}
		if (deck.OwnedUnusedCardIDs != null)
		{
			try
			{
				m_OwnedUnusedCards = deck.OwnedUnusedCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
				foreach (CAbilityCard ownedUnusedCard in OwnedUnusedCards)
				{
					ownedUnusedCard.CurrentCardPile = CBaseCard.ECardPile.OwnedUnused;
				}
			}
			catch
			{
				throw new Exception("Failed to load in OwnedUnusedCards from save data");
			}
		}
		List<CAbilityCard> list = new List<CAbilityCard>();
		if (deck.ActivatedAbilityCardIDs != null)
		{
			try
			{
				foreach (Tuple<int, CBaseCard.ECardType, string> activatedAbilityCard in deck.ActivatedAbilityCardIDs)
				{
					switch (activatedAbilityCard.Item2)
					{
					case CBaseCard.ECardType.CharacterAbility:
					{
						CAbilityCard cAbilityCard9 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == activatedAbilityCard.Item1);
						if (cAbilityCard9 != null)
						{
							if (!m_ActivatedCards.Contains(cAbilityCard9))
							{
								m_ActivatedCards.Add(cAbilityCard9);
								cAbilityCard9.CurrentCardPile = CBaseCard.ECardPile.Activated;
							}
							else
							{
								list.Add(cAbilityCard9);
							}
							break;
						}
						throw new Exception("Ability card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.Item:
					case CBaseCard.ECardType.Pet:
					{
						CItem cItem = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor x) => x.Class == this)?.Inventory?.AllItemsAndPets?.FirstOrDefault((CItem x) => x.ID == activatedAbilityCard.Item1);
						if (cItem != null)
						{
							m_ActivatedCards.Add(cItem);
							break;
						}
						throw new Exception("Item card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.AttackModifier:
					{
						AttackModifierCard card = ScenarioRuleClient.SRLYML.AttackModifiers.Single((AttackModifierYMLData c) => c.ID == activatedAbilityCard.Item1).Card;
						if (card != null)
						{
							m_ActivatedCards.Add(card);
							break;
						}
						throw new Exception("Attack Modifier card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.Dummy:
					{
						CBaseCard item2 = m_ActivatedCards.SingleOrDefault((CBaseCard x) => x.ID == activatedAbilityCard.Item1 && x.CardType == CBaseCard.ECardType.Dummy) ?? CBaseCard.CreateDummy(activatedAbilityCard.Item3, activatedAbilityCard.Item1);
						m_ActivatedCards.Add(item2);
						break;
					}
					default:
						throw new Exception("Invalid Activated Card Type.  Unable to load activated cards");
					}
				}
				for (int num = m_ActivatedCards.Count - 1; num >= 0; num--)
				{
					CBaseCard activatedCard = m_ActivatedCards[num];
					if (!deck.ActiveBonuses.Any((ActiveBonusState a) => a.CardID == activatedCard.ID))
					{
						m_ActivatedCards.Remove(activatedCard);
						if (activatedCard is CAbilityCard cAbilityCard10)
						{
							m_DiscardedAbilityCards.Add(cAbilityCard10);
							cAbilityCard10.CurrentCardPile = CBaseCard.ECardPile.Discarded;
						}
					}
				}
			}
			catch
			{
				throw new Exception("Failed to load in ActivatedAbilityCards from save data");
			}
		}
		if (deck.InitiativeAbilityCardID.HasValue)
		{
			try
			{
				CAbilityCard initiativeAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard s) => s.ID == deck.InitiativeAbilityCardID.Value);
				m_InitiativeAbilityCard = initiativeAbilityCard;
			}
			catch
			{
				throw new Exception("Failed to load in InitiativeAbilityCard from save data");
			}
		}
		if (deck.SubInitiativeAbilityCardID.HasValue)
		{
			try
			{
				CAbilityCard subInitiativeAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard s) => s.ID == deck.SubInitiativeAbilityCardID.Value);
				m_SubInitiativeAbilityCard = subInitiativeAbilityCard;
			}
			catch
			{
				throw new Exception("Failed to load in InitiativeAbilityCard from save data");
			}
		}
		if (deck.ExtraTurnInitiativeAbilityCardID.HasValue)
		{
			try
			{
				CAbilityCard extraTurnInitiativeAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard s) => s.ID == deck.ExtraTurnInitiativeAbilityCardID.Value);
				m_ExtraTurnInitiativeAbilityCard = extraTurnInitiativeAbilityCard;
			}
			catch
			{
				throw new Exception("Failed to load in InitiativeAbilityCard from save data");
			}
		}
		if (deck.Enhancements != null && deck.Enhancements.Count > 0)
		{
			ApplyEnhancements(deck.Enhancements);
		}
		if (deck.AbilityCardLastPerformedActions != null && deck.AbilityCardLastPerformedActions.Count > 0)
		{
			foreach (var entry in deck.AbilityCardLastPerformedActions)
			{
				CAbilityCard cAbilityCard11 = AbilityCardsPool.FirstOrDefault((CAbilityCard x) => x.ID == entry.Item1);
				if (cAbilityCard11 != null)
				{
					cAbilityCard11.LastPlayedActionType = entry.Item2;
				}
			}
		}
		if (deck.ActiveBonuses == null || deck.ActiveBonuses.Count <= 0)
		{
			return;
		}
		for (int num2 = deck.ActiveBonuses.Count - 1; num2 >= 0; num2--)
		{
			ActiveBonusState checkActiveBonusState = deck.ActiveBonuses[num2];
			CBaseCard cBaseCard = m_ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == checkActiveBonusState.CardID && (s.Name == checkActiveBonusState.CardName || s.Name.Contains(checkActiveBonusState.CardName.Replace(" ", string.Empty).Replace("'", string.Empty))));
			if (cBaseCard == null || (cBaseCard.CardType != CBaseCard.ECardType.AttackModifier && cBaseCard.CardType != CBaseCard.ECardType.Item))
			{
				for (int num3 = 0; num3 < deck.ActiveBonuses.Count; num3++)
				{
					if (num2 != num3)
					{
						ActiveBonusState state = deck.ActiveBonuses[num3];
						if (ActiveBonusState.Compare(checkActiveBonusState, state, string.Empty, string.Empty, isMPCompare: false).Count <= 0)
						{
							Log.Error("Duplicate Active Bonus State removed from Character Class " + ID);
							deck.ActiveBonuses.Remove(checkActiveBonusState);
							break;
						}
					}
				}
			}
		}
		List<CAbility> list2 = new List<CAbility>();
		foreach (ActiveBonusState activeBonusState in deck.ActiveBonuses)
		{
			CBaseCard cBaseCard2 = m_ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == activeBonusState.CardID && (s.Name == activeBonusState.CardName || s.Name.Contains(activeBonusState.CardName.Replace(" ", string.Empty).Replace("'", string.Empty))));
			if (cBaseCard2 == null)
			{
				Log.Error("Unable to find card with ID " + activeBonusState.CardID + " for active bonus state");
			}
			else
			{
				if (list.Contains(cBaseCard2))
				{
					continue;
				}
				CItem cItem2 = cBaseCard2 as CItem;
				CAbilityCard cAbilityCard12 = cBaseCard2 as CAbilityCard;
				CAbility cAbility = null;
				if (cBaseCard2.CardType == CBaseCard.ECardType.Dummy)
				{
					foreach (CActiveBonus activeBonuse in cBaseCard2.ActiveBonuses)
					{
						if (activeBonuse.Ability.ID == activeBonusState.AbilityID)
						{
							cAbility = activeBonuse.Ability;
							break;
						}
					}
					if (cAbility == null)
					{
						cAbility = CAbility.FromJson(activeBonusState.AbilityJson);
					}
				}
				else if (cBaseCard2 is AttackModifierCard attackModifierCard)
				{
					cAbility = attackModifierCard.YMLData.Abilities.SingleOrDefault((CAbility x) => x.Name == activeBonusState.AbilityName);
					if (cAbility == null)
					{
						cAbility = attackModifierCard.YMLData.Abilities.SelectMany((CAbility x) => x.SubAbilities).SingleOrDefault((CAbility x) => x.Name == activeBonusState.AbilityName);
					}
				}
				else
				{
					cAbility = ((cAbilityCard12 != null) ? (cAbilityCard12.FindAbilityOnCard(activeBonusState.AbilityID) ?? cAbilityCard12.FindAbilityOnCard(activeBonusState.AbilityName)) : cItem2?.FindAbilityOnItemCard(activeBonusState.AbilityName));
				}
				cBaseCard2.ActionHasHappened = activeBonusState.ActionHasHappened;
				if (cAbility == null)
				{
					Log.Error("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
					continue;
				}
				CActor cActor = ScenarioManager.FindActor(activeBonusState.ActorGuid);
				if (cActor == null)
				{
					Log.Error("Unable to find actor attached to active bonus");
					continue;
				}
				CActor cActor2 = null;
				if (activeBonusState.CasterGuid != null)
				{
					cActor2 = ScenarioManager.FindActor(activeBonusState.CasterGuid);
				}
				else if (cAbilityCard12 != null)
				{
					cActor2 = ScenarioManager.FindActorWithAbilityCard(cAbilityCard12);
				}
				else if (cItem2 != null)
				{
					cActor2 = ScenarioManager.FindActorWithActivatedItemFromID(cItem2.ID, this);
				}
				if (cActor2 == null)
				{
					Log.Error("Unable to find caster attached to active bonus");
					continue;
				}
				CAbility cAbility2 = null;
				if (cBaseCard2.CardType == CBaseCard.ECardType.Dummy)
				{
					cAbility2 = cAbility;
				}
				else
				{
					cAbility2 = CAbility.CopyAbility(cAbility, generateNewID: false);
					cAbility2.TargetingActor = cActor;
				}
				if (!list2.Contains(cAbility))
				{
					cAbility2.ApplyEnhancement(cAbility2.AbilityEnhancements);
					list2.Add(cAbility);
				}
				int? bespokeStrength = null;
				if (activeBonusState.BespokeBehaviourStrength != 0)
				{
					bespokeStrength = activeBonusState.BespokeBehaviourStrength;
				}
				cAbility2.Name = activeBonusState.AbilityName;
				cAbility2.CachedAbilityBaseCard = cBaseCard2;
				if (activeBonusState.AbilityStrength.HasValue)
				{
					cAbility2.Strength = activeBonusState.AbilityStrength.Value;
				}
				if (activeBonusState.AuraRange.HasValue && cAbility2.ActiveBonusData.IsAura && cAbility2.ActiveBonusData.AuraTargeting == CAbility.EAbilityTargeting.Range)
				{
					cAbility2.Range = activeBonusState.AuraRange.Value;
				}
				CActiveBonus cActiveBonus = null;
				if (activeBonusState.IsDoom)
				{
					CAbilityAddDoom cAbilityAddDoom = (CAbilityAddDoom)cAbilityCard12.GetAllAbilities().FirstOrDefault((CAbility a) => a.AbilityType == CAbility.EAbilityType.AddDoom);
					cActor2.AddDoom(cAbilityAddDoom.Doom, cActor, addDoomActiveBonuses: false);
					cActiveBonus = cAbilityCard12.AddActiveBonus(cAbility, cActor, cActor2, activeBonusState.ID, activeBonusState.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: true, bespokeStrength, activeBonusState.ActiveBonusStartRound, activeBonusState.ToggledBonus);
					cAbilityCard12.SetSelectedAction(activeBonusState.IsTopAction ? cAbilityCard12.TopAction : cAbilityCard12.BottomAction);
				}
				else if (cAbility2 is CAbilityAttack { Augment: not null } || cAbility2 is CAbilityAddActiveBonus { Augment: not null })
				{
					cActor.AddAugmentOrSong(cAbility2, cActor2, activeBonusState.ID, activeBonusState.ActiveBonusStartRound);
					cAbilityCard12.SetSelectedAction(activeBonusState.IsTopAction ? cAbilityCard12.TopAction : cAbilityCard12.BottomAction);
				}
				else if (cAbility2.Song != null)
				{
					cActor.AddAugmentOrSong(cAbility2, cActor2, activeBonusState.ID, activeBonusState.ActiveBonusStartRound);
					cAbilityCard12.SetSelectedAction(activeBonusState.IsTopAction ? cAbilityCard12.TopAction : cAbilityCard12.BottomAction);
				}
				else if (cItem2 != null)
				{
					CActiveBonus cActiveBonus2 = cItem2.AddActiveBonus(cAbility2, cActor, cActor2, activeBonusState.ID, activeBonusState.Remaining, isAugment: false, isSong: false, loadingItemBonus: true, isDoom: false, bespokeStrength, activeBonusState.ActiveBonusStartRound, activeBonusState.ToggledBonus);
					cActor.Inventory.HandleActiveItemTriggered(cItem2, ignoreConsume: true);
					if (activeBonusState.RestrictedActorGuids != null)
					{
						foreach (string restrictedActorGuid in activeBonusState.RestrictedActorGuids)
						{
							CActor cActor3 = ScenarioManager.FindActor(restrictedActorGuid);
							if (cActor3 != null)
							{
								cActiveBonus2.RestrictActiveBonus(cActor3, manualRestrict: true);
							}
						}
					}
					cActiveBonus = cActiveBonus2;
				}
				else
				{
					cAbilityCard12?.SetSelectedAction(activeBonusState.IsTopAction ? cAbilityCard12.TopAction : cAbilityCard12.BottomAction);
					CActiveBonus cActiveBonus3 = cBaseCard2.AddActiveBonus(cAbility2, cActor, cActor2, activeBonusState.ID, activeBonusState.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength, activeBonusState.ActiveBonusStartRound, activeBonusState.ToggledBonus);
					if (activeBonusState.RestrictedActorGuids != null)
					{
						foreach (string restrictedActorGuid2 in activeBonusState.RestrictedActorGuids)
						{
							CActor cActor4 = ScenarioManager.FindActor(restrictedActorGuid2);
							if (cActor4 != null)
							{
								cActiveBonus3.RestrictActiveBonus(cActor4, manualRestrict: true);
							}
						}
					}
					cActiveBonus = cActiveBonus3;
				}
				cActiveBonus?.Load(activeBonusState);
			}
		}
	}

	public void LoadAttackModifierDeck(AttackModifierDeckState deck)
	{
		if (deck.AttackModifierCardsPool != null && deck.AttackModifierCardsPool.Count > 0)
		{
			m_AttackModifierCardsPool.Clear();
			CClass.AddModifierCards(deck.AttackModifierCardsPool, ref m_AttackModifierCardsPool, allowShuffle: false);
		}
		if (deck.AttackModifierCardsAvailable != null)
		{
			m_AttackModifierCards.Clear();
			CClass.AddModifierCards(deck.AttackModifierCardsAvailable, ref m_AttackModifierCards, allowShuffle: false);
		}
		if (deck.AttackModifierCardsDiscarded != null)
		{
			m_DiscardedAttackModifierCards.Clear();
			CClass.AddModifierCards(deck.AttackModifierCardsDiscarded, ref m_DiscardedAttackModifierCards, allowShuffle: false);
		}
		m_ShuffleAttackModifierCards = deck.ShuffleAttackModifierCards;
	}

	public void AddAdditionalModifierCards(List<string> cardNames, bool toDiscard = false)
	{
		if (toDiscard)
		{
			CClass.AddModifierCards(cardNames, ref m_DiscardedAttackModifierCards);
		}
		else
		{
			CClass.AddModifierCards(cardNames, ref m_AttackModifierCards);
		}
	}

	public void RemoveModifierCards(List<string> cardNames)
	{
		CClass.RemoveModifierCards(cardNames, ref m_AttackModifierCards);
	}

	public CActor GetActor()
	{
		return ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.CharacterClass.ID == ID);
	}

	public CharacterResourceData GetCharacterResourceData(string resourceID)
	{
		return CharacterYML.CharacterResourceDatas.SingleOrDefault((CharacterResourceData x) => x.ID == resourceID);
	}

	public StartRoundCardState GetCurrentCardState(string fastReloadID)
	{
		List<CAbilityCard> roundAbilityCards;
		lock (RoundAbilityCards)
		{
			roundAbilityCards = RoundAbilityCards.ToList();
		}
		List<CAbilityCard> handAbilityCards;
		lock (HandAbilityCards)
		{
			handAbilityCards = HandAbilityCards.ToList();
		}
		List<CAbilityCard> discardedAbilityCards;
		lock (DiscardedAbilityCards)
		{
			discardedAbilityCards = DiscardedAbilityCards.ToList();
		}
		List<CAbilityCard> lostAbilityCards;
		lock (LostAbilityCards)
		{
			lostAbilityCards = LostAbilityCards.ToList();
		}
		List<CAbilityCard> permaLostAbilityCards;
		lock (PermanentlyLostAbilityCards)
		{
			permaLostAbilityCards = PermanentlyLostAbilityCards.ToList();
		}
		List<CAbilityCard> takenAbilityCards;
		lock (TakenCards)
		{
			takenAbilityCards = TakenCards.ToList();
		}
		List<CAbilityCard> ownedUnusedAbilityCards;
		lock (OwnedUnusedCards)
		{
			ownedUnusedAbilityCards = OwnedUnusedCards.ToList();
		}
		List<CBaseCard> activatedCards;
		lock (ActivatedCards)
		{
			activatedCards = ActivatedCards.ToList();
		}
		List<CAbilityCard> excludedFromShortRestAbilityCardIDs;
		lock (ExcludedFromShortRestAbilityCards)
		{
			excludedFromShortRestAbilityCardIDs = ExcludedFromShortRestAbilityCards.ToList();
		}
		return new StartRoundCardState(roundAbilityCards, handAbilityCards, discardedAbilityCards, lostAbilityCards, permaLostAbilityCards, takenAbilityCards, ownedUnusedAbilityCards, activatedCards, excludedFromShortRestAbilityCardIDs, InitiativeAbilityCard, SubInitiativeAbilityCard, HasShortRested, HasImprovedShortRested, ShortRestCardBurned, ImprovedShortRest, ShortRestCardRedrawn, ExcludedCardsFromShortRest, LongRest, ShortRest, fastReloadID);
	}

	public void ProxySetStartRoundDeckState(StartRoundCardState startRoundCardState)
	{
		m_HasShortRested = startRoundCardState.HasShortRested;
		m_HasImprovedShortRested = startRoundCardState.HasImprovedShortRested;
		ImprovedShortRest = startRoundCardState.ImprovedShortRest;
		ImprovedShortRestDrawCount = 0u;
		ExcludedCardsFromShortRest = startRoundCardState.ExcludedCardsFromShortRest;
		LongRest = startRoundCardState.LongRestSelected;
		ShortRest = startRoundCardState.ShortRestSelected;
		for (int num = ActivatedCards.Count - 1; num >= 0; num--)
		{
			CBaseCard cBaseCard = ActivatedCards[num];
			if (!startRoundCardState.ActivatedCardIDs.Contains(cBaseCard.ID))
			{
				List<CActiveBonus> list = cBaseCard.ActiveBonuses.ToList();
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					CClass.CancelActiveBonus(list[num2]);
				}
				cBaseCard.ActiveBonuses.Clear();
				m_ActivatedCards.Remove(cBaseCard);
			}
		}
		if (AbilityDeckState.DebugCardID > 0 && m_HandAbilityCards.Any((CAbilityCard c) => c.ID == AbilityDeckState.DebugCardID))
		{
			Log.Info("Cleared hand AbilityDeck on ProxySetStartRoundDeckState: " + AbilityCardsToString(m_HandAbilityCards));
		}
		m_RoundAbilityCards.Clear();
		m_HandAbilityCards.Clear();
		m_DiscardedAbilityCards.Clear();
		m_LostAbilityCards.Clear();
		m_PermanentlyLostAbilityCards.Clear();
		m_TakenCards.Clear();
		m_OwnedUnusedCards.Clear();
		m_ExcludedFromShortRestAbilityCards.Clear();
		foreach (int roundCardInstanceID in startRoundCardState.RoundAbilityCardInstanceIDs)
		{
			CAbilityCard item = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == roundCardInstanceID);
			m_RoundAbilityCards.Add(item);
		}
		foreach (int handAbilityCardInstanceID in startRoundCardState.HandAbilityCardInstanceIDs)
		{
			CAbilityCard item2 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == handAbilityCardInstanceID);
			m_HandAbilityCards.Add(item2);
		}
		foreach (int discardedCardInstanceID in startRoundCardState.DiscardedAbilityCardInstanceIDs)
		{
			CAbilityCard item3 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == discardedCardInstanceID);
			m_DiscardedAbilityCards.Add(item3);
		}
		foreach (int lostCardInstanceID in startRoundCardState.LostAbilityCardInstanceIDs)
		{
			CAbilityCard item4 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == lostCardInstanceID);
			m_LostAbilityCards.Add(item4);
		}
		foreach (int permaLostCardInstanceID in startRoundCardState.PermaLostAbilityCardInstanceIDs)
		{
			CAbilityCard item5 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == permaLostCardInstanceID);
			m_PermanentlyLostAbilityCards.Add(item5);
		}
		foreach (int takenCardInstanceID in startRoundCardState.TakenAbilityCardInstanceIDs)
		{
			CAbilityCard item6 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == takenCardInstanceID);
			m_TakenCards.Add(item6);
		}
		foreach (int ownedUnusedCardInstanceID in startRoundCardState.OwnedUnusedAbilityCardInstanceIDs)
		{
			CAbilityCard item7 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == ownedUnusedCardInstanceID);
			m_OwnedUnusedCards.Add(item7);
		}
		foreach (int excludedFromShortRestAbilityCardID in startRoundCardState.ExcludedFromShortRestAbilityCardIDs)
		{
			CAbilityCard item8 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == excludedFromShortRestAbilityCardID);
			m_ExcludedFromShortRestAbilityCards.Add(item8);
		}
		if (AbilityDeckState.DebugCardID > 0 && m_HandAbilityCards.Any((CAbilityCard c) => c.ID == AbilityDeckState.DebugCardID))
		{
			Log.Info("Recreated hand AbilityDeck on ProxySetStartRoundDeckState: " + AbilityCardsToString(m_HandAbilityCards));
		}
		m_InitiativeAbilityCard = CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard c) => c.CardInstanceID == startRoundCardState.InitiativeCardID);
		m_SubInitiativeAbilityCard = CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard c) => c.CardInstanceID == startRoundCardState.SubInitiativeCardID);
		CPlayerActor cPlayerActor = (CPlayerActor)GetActor();
		if (cPlayerActor.IsExhausted())
		{
			bool actorWasAsleep = cPlayerActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			GameState.KillActor(cPlayerActor, cPlayerActor, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
			CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(cPlayerActor);
			cPlayersExhausted_MessageData.m_Players = new List<CPlayerActor> { cPlayerActor };
			ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
		}
	}
}
