using System.Collections.Generic;
using FHGUI.DTO;
using FHRL;

namespace FHGUI.Services;

public class EnhancerViewService : FHMapViewService
{
	public bool IsEnhancerEnabled => _view.GameController.EnhancementController.Enabled;

	public EnhancerViewService(FHMapView view)
		: base(view)
	{
	}

	public IEnumerable<CardEnhancementTypeDef> GetEnhancementsForSlot(ClassCardDef card, ClassCardDef.EnhacementSlot slot)
	{
		CardEnhacementSlotQuery slot2 = new CardEnhacementSlotQuery(card, slot);
		return _view.GameController.EnhancementController.GetEnhancementsForSlot(slot2);
	}

	public bool CanEnhance(CharacterState character, CardEnhacementSlotQuery query, CardEnhancementTypeDef enhancement)
	{
		return _view.GameController.EnhancementController.CanEnhance(character, query, enhancement);
	}

	public void Enhance(CharacterState character, CardDTO abilityCard, CardEnhancementSlotDTO slot, CardEnhancementDTO enhancement)
	{
		ClassCardDef classCardDef = DefRepo.ClassCards[new ClassCardID((uint)abilityCard.Id)];
		foreach (ClassCardDef.EnhacementSlot enhancementSlot in classCardDef.EnhancementSlots)
		{
			if (enhancementSlot.Effect != slot.Effect || enhancementSlot.SlotIndex != slot.SlotIndex || enhancementSlot.AbilityName != slot.AbilityName)
			{
				continue;
			}
			CardEnhacementSlotQuery slot2 = new CardEnhacementSlotQuery(classCardDef, enhancementSlot);
			foreach (CardEnhancementTypeDef item in _view.GameController.EnhancementController.GetEnhancementsForSlot(slot2))
			{
				if (!(item.ID.ID != enhancement.ID))
				{
					Enhance(character, slot2, item);
					return;
				}
			}
		}
	}

	public void Enhance(CharacterState character, CardEnhacementSlotQuery slot, CardEnhancementTypeDef enhancement)
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Enchancement.Enhance, character.ID, slot, enhancement);
	}

	public List<CardEnhancementSlotDTO> GetCardEnhancements(CharacterState character, CardDTO cardDTO)
	{
		List<CardEnhancementSlotDTO> list = new List<CardEnhancementSlotDTO>();
		ClassCardDef classCardDef = DefRepo.ClassCards[new ClassCardID((uint)cardDTO.Id)];
		foreach (ClassCardDef.EnhacementSlot enhancementSlot in classCardDef.EnhancementSlots)
		{
			CardEnhancementSlotDTO cardEnhancementSlotDTO = new CardEnhancementSlotDTO(enhancementSlot);
			CardEnhacementSlotQuery slot = new CardEnhacementSlotQuery(classCardDef, enhancementSlot);
			IEnumerable<CardEnhancementTypeDef> enhancementsForSlot = _view.GameController.EnhancementController.GetEnhancementsForSlot(slot);
			CardEnhancementTypeDef existingEnhancementForSlot = _view.GameController.EnhancementController.GetExistingEnhancementForSlot(slot);
			cardEnhancementSlotDTO.InUse = existingEnhancementForSlot != null;
			list.Add(cardEnhancementSlotDTO);
			foreach (CardEnhancementTypeDef item in enhancementsForSlot)
			{
				if (cardEnhancementSlotDTO.Enhancements == null)
				{
					cardEnhancementSlotDTO.Enhancements = new List<CardEnhancementDTO>();
				}
				CardEnhancementDTO cardEnhancementDTO = new CardEnhancementDTO(item);
				int num = (int)(cardEnhancementDTO.CurrentGoldCost = (uint)_view.GameController.EnhancementController.GetEnhacementGoldCost(slot, item));
				cardEnhancementDTO.Affordable = character.Gold >= num;
				cardEnhancementDTO.Sloted = existingEnhancementForSlot == item;
				cardEnhancementSlotDTO.Enhancements.Add(cardEnhancementDTO);
			}
			if (cardEnhancementSlotDTO.Enhancements == null || cardEnhancementSlotDTO.Enhancements.Count == 0)
			{
				Log.GUIWarning($"EnhancementSlots {enhancementSlot} for ability {classCardDef} is missing any eligible Enhancements");
			}
		}
		return list;
	}

	public bool HasEnhancementSlots(CardDTO cardDTO)
	{
		ClassCardDef classCardDef = DefRepo.ClassCards[new ClassCardID((uint)cardDTO.Id)];
		foreach (ClassCardDef.EnhacementSlot enhancementSlot in classCardDef.EnhancementSlots)
		{
			CardEnhacementSlotQuery slot = new CardEnhacementSlotQuery(classCardDef, enhancementSlot);
			if (_view.GameController.EnhancementController.GetEnhancementsForSlot(slot) != null)
			{
				return true;
			}
			if (_view.GameController.EnhancementController.GetExistingEnhancementForSlot(slot) != null)
			{
				return true;
			}
		}
		return false;
	}
}
