using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Services;

public class CardViewService : FHMapViewService
{
	public CardViewService(FHMapView view)
		: base(view)
	{
	}

	public ClassCardDef GetCardDef(CardDTO cardDto)
	{
		return GetCardDef(cardDto.Id);
	}

	public ClassCardDef GetCardDef(int id)
	{
		return DefRepo.ClassCards[new ClassCardID((uint)id)];
	}

	public IEnumerable<CardDTO> GetLevelUpCards(CharacterDataDTO characterDataDto)
	{
		bool hasCharacter = TryGetCardUnlocks(characterDataDto.CharacterID, out var characterState, out var cardUnlocks, out var _);
		IOrderedEnumerable<ClassCardDef> orderedEnumerable = from classCardDef in characterDataDto.Class.GetAllCards(Choreographer.DefRepo)
			where classCardDef.Level > 1
			orderby classCardDef.ID
			select classCardDef;
		foreach (ClassCardDef cardDef in orderedEnumerable)
		{
			bool flag = characterDataDto.Cards.Any((CardDTO c) => c.Id == cardDef.ID.ID);
			bool flag2 = hasCharacter && GameController.CharacterController.CanUnlockCard(characterState, cardDef);
			bool flag3 = cardUnlocks < 1 || !flag2;
			ClassCardDef cardDef2 = cardDef;
			bool isInHand = flag;
			bool isDisabled = flag3;
			yield return new CardDTO(cardDef2, isInHand, characterDataDto.CharacterID, UIPlayerCharacterUtils.GetEnhancementsForCard(characterDataDto.Class, (int)cardDef.ID.ID), isSelectable: true, isDisabled);
		}
	}

	public bool TryGetCardUnlocks(CharacterID characterID, out CharacterState characterState, out int cardUnlocks, out bool fullyUnlocked)
	{
		bool flag = GameController.CharacterController.TryGetCharacter(characterID, out characterState);
		cardUnlocks = (int)(flag ? characterState.CardUnlocks : 0);
		fullyUnlocked = flag && characterState.PassedCards.Count() >= GameController.CharacterController.GetMaxLevel() - 1;
		return flag;
	}

	public void FillHandCards(CharacterDataDTO characterDataDto, HandCardListModule handCardListModule, UICardInteractionEvents interactionEvents, List<CardDTO> ownedCards = null)
	{
		if (ownedCards == null)
		{
			ownedCards = characterDataDto.Cards;
		}
		List<CardDTO> cards = ownedCards.Where((CardDTO dto) => dto.IsInHand).ToList();
		bool isDualDeck = ownedCards.Any((CardDTO c) => c.IsAltColor);
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(characterDataDto.Class));
		Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(characterDataDto.Class));
		handCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = cards,
			InteractionEvents = interactionEvents,
			MinCount = (int)characterDataDto.Class.MaxCards
		}, characterColor, characterAltColor, isDualDeck);
	}

	public IEnumerable<UIPartySlotHints.HintedActionParams> GetHandCardInteractionHints(CardDTO cardDto)
	{
		if (!cardDto.IsInHand)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD"),
				ColorSchemeID = "Add"
			};
		}
		else
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_REMOVE"),
				ColorSchemeID = "Remove"
			};
		}
		yield return new UIPartySlotHints.HintedActionParams
		{
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.MiddleClick, "GUI_INSPECT"),
			ColorSchemeID = "Inspect"
		};
	}

	public void UpdateCardEnhancements(CharacterClassDef classDef)
	{
		List<CEnhancement> list = new List<CEnhancement>();
		foreach (ClassCardDef cardDef in DefRepo.ClassCards.Values.Where((ClassCardDef s) => s.ClassID == classDef.ID))
		{
			CAbilityCard cAbilityCard = null;
			foreach (ClassCardDef.EnhacementSlot enhancementSlot in cardDef.EnhancementSlots)
			{
				ClassCardState state = _view.GameController.State.ClassCardModifications.FirstOrDefault((ClassCardState m) => m.CardDef == cardDef);
				CEnhancement cEnhancement = FHGlue.CreateCEnhancement(new CardEnhacementSlotQuery(cardDef, enhancementSlot), state);
				if (cAbilityCard == null)
				{
					cAbilityCard = cEnhancement.AbilityCard;
				}
				list.Add(cEnhancement);
			}
		}
		UpdateEnhancementsForClass(list, classDef.ID.ID);
	}

	public void UpdateCardEnhancements(CharacterClassDef classDef, ClassCardDef cardDef)
	{
		List<CEnhancement> list = new List<CEnhancement>();
		foreach (ClassCardDef.EnhacementSlot enhancementSlot in cardDef.EnhancementSlots)
		{
			ClassCardState state = _view.GameController.State.ClassCardModifications.FirstOrDefault((ClassCardState m) => m.CardDef == cardDef);
			CEnhancement cEnhancement = FHGlue.CreateCEnhancement(new CardEnhacementSlotQuery(cardDef, enhancementSlot), state);
			if (cEnhancement.Enhancement != EEnhancement.NoEnhancement)
			{
				list.Add(cEnhancement);
			}
		}
		UpdateEnhancementsOnCard(list, (int)cardDef.ID.ID);
	}

	private void UpdateEnhancementsOnCard(IReadOnlyList<CEnhancement> enhancements, int cardId, CCharacterClass characterClass = null)
	{
		if (characterClass == null)
		{
			characterClass = CharacterClassManager.Classes.Find((CCharacterClass s) => s.AbilityCardsPool.Any((CAbilityCard c) => c.ID == cardId));
		}
		CAbilityCard card = characterClass.AbilityCardsPool.Single((CAbilityCard c) => c.ID == cardId);
		UpdateEnhancementsOnCard(enhancements, card);
	}

	private void UpdateEnhancementsOnCard(IReadOnlyList<CEnhancement> enhancements, CAbilityCard card)
	{
		foreach (CEnhancement enhancement in enhancements)
		{
			if (enhancement.AbilityCardID != card.ID)
			{
				continue;
			}
			foreach (CEnhancement allEnhancement in card.GetAllEnhancements())
			{
				if (!(allEnhancement.AbilityName != enhancement.AbilityName) && allEnhancement.EnhancementLine == enhancement.EnhancementLine && allEnhancement.EnhancementSlot == enhancement.EnhancementSlot)
				{
					allEnhancement.Enhancement = enhancement.Enhancement;
					allEnhancement.PaidPrice = enhancement.PaidPrice;
				}
			}
		}
	}

	private void UpdateEnhancementsForClass(IReadOnlyList<CEnhancement> enhancements, string classID)
	{
		foreach (CAbilityCard item in CharacterClassManager.Classes.Find((CCharacterClass s) => s.ID == classID).AbilityCardsPool)
		{
			UpdateEnhancementsOnCard(enhancements, item);
		}
	}
}
