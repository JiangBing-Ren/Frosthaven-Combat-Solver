using System;
using System.Collections.Generic;
using FH.Tools;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityExtraTurn : CAbilityTargeting
{
	[KronosEnumPolice(0)]
	public enum EExtraTurnType
	{
		None,
		TopAction,
		BottomAction,
		BothActions,
		BothActionsLater,
		MonsterAction,
		CurrentAction,
		SingleCardAnyActionAtTheEndOfTurn,
		SingleActionBeforeCardSelection,
		SingleActionAfterExtraTurnPerform
	}

	public class PendingExtraTurnData
	{
		public int LeadingInitiative;

		public List<CAbilityCard> PendingExtraTurnCards;

		public PendingExtraTurnData(int leadingInitiative, List<CAbilityCard> pendingExtraTurnCards)
		{
			LeadingInitiative = leadingInitiative;
			PendingExtraTurnCards = pendingExtraTurnCards;
		}
	}

	public static readonly EExtraTurnType[] ExtraTurnTypes = (EExtraTurnType[])Enum.GetValues(typeof(EExtraTurnType));

	[JsonProperty]
	public EExtraTurnType ExtraTurnType { get; set; }

	[JsonProperty]
	public string ActionMonsterClassID { get; set; }

	[JsonProperty]
	public List<EAbilityType> AbilityTypeFilter { get; set; }

	[JsonProperty]
	public int MaxAbilityLevel { get; set; }

	public CAbilityExtraTurn(EExtraTurnType extraTurnType, List<EAbilityType> abilityTypeFilter, int maxAbilityLevel, string actionMonsterClassID)
		: base(EAbilityType.ExtraTurn)
	{
		ExtraTurnType = extraTurnType;
		AbilityTypeFilter = abilityTypeFilter;
		MaxAbilityLevel = maxAbilityLevel;
		ActionMonsterClassID = actionMonsterClassID;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		for (int num = m_ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor = m_ValidActorsInRange[num];
			if (cActor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType == EExtraTurnType.BothActions || ExtraTurnType == EExtraTurnType.BothActionsLater)
				{
					if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 2)
					{
						m_ValidActorsInRange.Remove(cActor);
					}
				}
				else if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 1)
				{
					m_ValidActorsInRange.Remove(cActor);
				}
			}
		}
		SetHasStarted(val: true);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			TargetingActor.Inventory.HandleUsedItems();
			if (actor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType != EExtraTurnType.MonsterAction)
				{
					cPlayerActor.CharacterClass.ResetInitiativeBonus(cPlayerActor);
					AbilityHasHappened = true;
					cPlayerActor.SelectingCardsForExtraTurnOfType = ExtraTurnType;
					if (ExtraTurnType == EExtraTurnType.CurrentAction)
					{
						if (GameState.RoundAbilityCardSelected.LastSelectedAction.ID == GameState.RoundAbilityCardSelected.TopAction.ID || GameState.RoundAbilityCardSelected.LastSelectedAction.ID == GameState.RoundAbilityCardSelected.DefaultAttackAction.ID)
						{
							ExtraTurnType = EExtraTurnType.TopAction;
						}
						else
						{
							ExtraTurnType = EExtraTurnType.BottomAction;
						}
					}
					if (ExtraTurnType == EExtraTurnType.BottomAction)
					{
						cPlayerActor.SkipTopCardAction = true;
					}
					else if (ExtraTurnType == EExtraTurnType.TopAction)
					{
						cPlayerActor.SkipBottomCardAction = true;
					}
					CAbilityCard extraTurnInitiatorCard = null;
					if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
					{
						extraTurnInitiatorCard = cAbilityCard;
					}
					GameState.CacheExtraTurnInitiatorCard(extraTurnInitiatorCard);
					CSelectExtraTurnCards_MessageData cSelectExtraTurnCards_MessageData = new CSelectExtraTurnCards_MessageData(AnimOverload, TargetingActor);
					cSelectExtraTurnCards_MessageData.m_ActorTakingExtraTurn = actor;
					cSelectExtraTurnCards_MessageData.m_ExtraTurnAbility = this;
					ScenarioRuleClient.MessageHandler(cSelectExtraTurnCards_MessageData);
				}
				else
				{
					Log.ErrorWithReport("Unsupported ExtraTurnType for PlayerActor");
				}
			}
			else if (ExtraTurnType == EExtraTurnType.MonsterAction)
			{
				actor.PendingExtraTurnOfTypeStack.Push(ExtraTurnType);
				actor.PendingExtraTurnActionMonsterClassIDStack.Push(ActionMonsterClassID);
			}
			else
			{
				Log.ErrorWithReport("Unsupported ExtraTurnType for MonsterActor");
			}
		}
		return true;
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		base.TileDeselected(selectedTile, optionalTileList);
		for (int num = m_ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor = m_ValidActorsInRange[num];
			if (cActor is CPlayerActor cPlayerActor)
			{
				if (ExtraTurnType == EExtraTurnType.BothActions || ExtraTurnType == EExtraTurnType.BothActionsLater)
				{
					if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 2)
					{
						m_ValidActorsInRange.Remove(cActor);
					}
				}
				else if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 1)
				{
					m_ValidActorsInRange.Remove(cActor);
				}
			}
		}
	}

	public bool IsValidAbility(CAbilityCard card)
	{
		if (card.Level > MaxAbilityLevel)
		{
			return false;
		}
		if (IsValidAction(card.TopAction))
		{
			return true;
		}
		if (IsValidAction(card.BottomAction))
		{
			return true;
		}
		return false;
	}

	public bool IsPending()
	{
		return TargetingActor.PendingExtraTurnOfType != EExtraTurnType.None;
	}

	public bool IsValidAction(CAction action)
	{
		if (IsPending())
		{
			return true;
		}
		if (AbilityTypeFilter.Count == 0)
		{
			return true;
		}
		foreach (CAbility ability in action.Abilities)
		{
			if (AbilityTypeFilter.Contains(ability.AbilityType))
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
