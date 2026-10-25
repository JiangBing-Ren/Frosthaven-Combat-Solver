using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHGUI.Common;
using FHGUI.DTO;
using FHRL;

namespace FHGUI.Services;

public class ImportPartyService(FHMapView view) : FHMapViewService(view)
{
	private List<PartyAdventureData> _failedSaves = new List<PartyAdventureData>();

	public async Task<List<ImportPartyDTO>> GetAvailableParties()
	{
		List<ImportPartyDTO> result = new List<ImportPartyDTO>();
		List<PartyAdventureData> list = await SaveData.Instance.Global.LoadSavesFromDisk();
		if (list == null || list.Count == 0)
		{
			return result;
		}
		foreach (PartyAdventureData item2 in list)
		{
			if (item2 == SaveData.Instance.Global.CurrentAdventureData || _failedSaves.Contains(item2) || item2.IsSaving)
			{
				continue;
			}
			if (item2.FrosthavenCampaign == null)
			{
				_failedSaves.Add(item2);
				continue;
			}
			List<CharacterState> list2 = item2.FrosthavenCampaign?.PartyCharacters.ToList();
			if (list2 == null || list2.Count == 0)
			{
				continue;
			}
			ImportPartyDTO importPartyDTO = new ImportPartyDTO(item2);
			foreach (CharacterState item3 in list2)
			{
				ImportPartyDTO importPartyDTO2 = importPartyDTO;
				if (importPartyDTO2.Members == null)
				{
					importPartyDTO2.Members = new List<CharacterDataDTO>();
				}
				CharacterDataDTO item = BuildCharacterFromIncompleteState(DefRepo, item3);
				importPartyDTO.Members.Add(item);
			}
			result.Add(importPartyDTO);
		}
		return result;
	}

	private CharacterDataDTO BuildCharacterFromIncompleteState(DefRepository defRepo, CharacterState characterState)
	{
		CharacterDataDTO characterDataDTO = new CharacterDataDTO();
		CrossReference.GetExperienceProgress(characterState, out var currentProgress, out var maxProgress);
		characterDataDTO.CharacterID = characterState.ID;
		characterDataDTO.Name = characterState.Name;
		characterDataDTO.Level = characterState.Level;
		characterDataDTO.IsMaxLevel = CrossReference.IsMaxLevel(characterState);
		characterDataDTO.Experience = new RangedValueDTO((int)currentProgress, (int)maxProgress);
		characterDataDTO.Gold = characterState.Gold;
		characterDataDTO.Resources = characterState.Resources;
		characterDataDTO.Class = defRepo.CharacterClasses[characterState.ClassID];
		characterDataDTO.Health = new RangedValueDTO((int)characterDataDTO.Class.HealthTable[1], (int)characterDataDTO.Class.HealthTable[^1]);
		characterDataDTO.Items = GetEquippedItems(DefRepo, characterState);
		characterDataDTO.MaxSmallItems = GetMaxSmallItems(characterState);
		characterDataDTO.UnlockedSmallItemCount = GetMaxUnlockedSmallItems(characterState);
		characterDataDTO.MaxOneHandSlots = GetMaxOneHandSlots(characterState);
		characterDataDTO.Cards.Clear();
		foreach (ClassCardDef allCard in characterDataDTO.Class.GetAllCards(defRepo))
		{
			characterDataDTO.Cards.Add(new CardDTO(allCard, characterState.SelectedCardIDs.Contains(allCard.ID), characterState.ID, UIPlayerCharacterUtils.GetEnhancementsForCard(characterState.ClassDef, (int)allCard.ID.ID)));
		}
		EnsureCards(characterDataDTO.Class);
		if (characterState.PersonalQuest != null)
		{
			characterDataDTO.SetPersonalQuest(_view.GetService<CharacterDetailsViewService>().GetPersonalQuest(characterState));
		}
		characterDataDTO.Masteries = characterDataDTO.Class.MasteryIDs?.Select((MasteryID md) => new GoalDTO(defRepo.MasteryDefs.Values.SingleOrDefault((MasteryDef x) => x.ID == md), characterState != null && GetCompletedMasteries(defRepo, characterState).Any((MasteryDef x) => x.ID == md))).ToList();
		return characterDataDTO;
	}

	private void EnsureCards(CharacterClassDef CharacterClass)
	{
	}

	public IEnumerable<MasteryDef> GetCompletedMasteries(DefRepository defRepo, CharacterState character)
	{
		foreach (MasteryDef item in DefRepo.MasteryDefs.FromKeys(character.CompletedMasteryIDs))
		{
			yield return item;
		}
	}

	public IEnumerable<ItemDataDTO> GetEquippedItems(DefRepository defRepo, CharacterState character)
	{
		return from item in defRepo.Items.FromKeys(character.EquipedItemIDs)
			select new ItemDataDTO(item, -1, showCost: false, isNew: false, canBuy: false, isEquipped: true, 0, character.Gold >= item.GoldCost, isForbidden: false);
	}

	public uint GetBaseMaxUnlockedSmallItems(CharacterState character)
	{
		return _view.GameController.CharacterController.GetBaseMaxUnlockedSmallItems(character);
	}

	public uint GetMaxUnlockedSmallItems(CharacterState character)
	{
		uint baseMaxUnlockedSmallItems = GetBaseMaxUnlockedSmallItems(character);
		IEnumerable<ItemDef> source = DefRepo.Items.FromKeys(character.EquipedItemIDs);
		return baseMaxUnlockedSmallItems + (uint)source.Sum((ItemDef x) => (from y in x.Effects
			where y.Type == CampaignEffectType.IncreaseMaxSmallItem
			select y.GetInt(0)).Sum());
	}

	public uint GetMaxSmallItems(CharacterState character, bool includeScenarioModifiers = true)
	{
		return 5u;
	}

	public uint GetMaxOneHandSlots(CharacterState character)
	{
		return 1u;
	}

	public void ImportParty(string partyId)
	{
		PartyAdventureData partyAdventureData = null;
		foreach (CharacterState item in SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.AllCharacters.ToList())
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.CancelCreateNewCharacter, item.ID);
		}
		uint num = 1u;
		foreach (CharacterState partyCharacter in partyAdventureData.FrosthavenCampaign.PartyCharacters)
		{
			PartySlotID partySlotID = new PartySlotID(num++);
			_view.Choreographer.ExecuteCommand("CreateCopyOfCharacter", partySlotID, partyCharacter);
		}
	}
}
