using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using I2.Loc;
using ScenarioRuleLibrary;
using UnityEngine.Pool;

namespace FHGUI.Testing;

public static class TestingCharacterCreationFlowUtils
{
	public static void PopulateCharacterDetails(DefRepository defRepo, CharacterDataDTO characterDataDto, CharacterPreviewModule previewModule, CharacterGoalsModule goalsModule, HeroClassStatsModule statsModule, CharacterBiographyModule bioModule, CharacterPersonalQuestModule personalQuestModule, bool isTempCharacter, bool showName = true)
	{
		ClassInfoDTO classInfoDTO = ((characterDataDto.Class != null) ? new ClassInfoDTO(characterDataDto.Level, characterDataDto.Class, null) : new ClassInfoDTO());
		previewModule?.Init(characterDataDto, null, showValues: false, showName);
		goalsModule?.Init(classInfoDTO.Masteries);
		bioModule?.Init(classInfoDTO);
		if ((object)personalQuestModule != null)
		{
			CharacterPersonalQuestDTO personalQuest = ((characterDataDto.PersonalQuestDef != null) ? new CharacterPersonalQuestDTO(characterDataDto.PersonalQuestDef) : new CharacterPersonalQuestDTO(null));
			personalQuestModule.Init(new CharacterPersonalQuestModule.Params
			{
				PersonalQuest = personalQuest
			});
		}
		statsModule?.Init(classInfoDTO, characterDataDto, isTempCharacter ? "GUI_CHARACTER_PREVIEW_CARDS" : "GUI_CHARACTER_EDIT_HAND", isTempCharacter, showExp: true);
	}

	public static void PopulateCharacterDetails(DefRepository defRepo, CharacterState characterState, CharacterPreviewModule previewModule, CharacterGoalsModule goalsModule, HeroClassStatsModule statsModule, CharacterBiographyModule bioModule, CharacterPersonalQuestModule personalQuestModule)
	{
		CharacterDataDTO characterDataDto = BuildCharacterDTO(defRepo, characterState);
		PopulateCharacterDetails(defRepo, characterDataDto, previewModule, goalsModule, statsModule, bioModule, personalQuestModule, characterState.PartyState == CharacterPartyStateType.CreationProcess);
	}

	public static CharacterDataDTO BuildCharacterDTO(DefRepository defRepo, CharacterState characterState)
	{
		CharacterDataDTO characterDataDTO = new CharacterDataDTO();
		characterDataDTO.Name = characterState.Name;
		characterDataDTO.Level = characterState.Level;
		characterDataDTO.IsMaxLevel = false;
		characterDataDTO.Experience = new RangedValueDTO(0, 999);
		characterDataDTO.Gold = characterState.Gold;
		characterDataDTO.Resources = characterState.Resources;
		characterDataDTO.Class = characterState.ClassDef;
		characterDataDTO.SetPersonalQuest(new CharacterPersonalQuestDTO(characterState.PersonalQuestDef));
		characterDataDTO.PersonalQuestVisible = true;
		PopulateStartingDeck(defRepo, characterDataDTO);
		return characterDataDTO;
	}

	public static CharacterDataDTO BuildCharacterDTO(DefRepository defRepo, CharacterClassDef classDef)
	{
		CharacterDataDTO characterDataDTO = new CharacterDataDTO();
		characterDataDTO.Name = string.Empty;
		characterDataDTO.Level = 1u;
		characterDataDTO.IsMaxLevel = false;
		characterDataDTO.Experience = new RangedValueDTO(0, 999);
		characterDataDTO.Gold = 0u;
		characterDataDTO.Class = classDef;
		characterDataDTO.Name = GetClassNameLocalized(classDef);
		PopulateStartingDeck(defRepo, characterDataDTO);
		return characterDataDTO;
	}

	public static void PopulateCharacterDTO(DefRepository defRepo, CharacterDataDTO dto, CharacterClassDef classDef)
	{
		if (dto == null)
		{
			dto = new CharacterDataDTO();
		}
		dto.Name = string.Empty;
		dto.Level = 1u;
		dto.IsMaxLevel = false;
		dto.Experience = new RangedValueDTO(0, 999);
		dto.Gold = 0u;
		dto.Class = classDef;
		dto.Name = GetClassNameLocalized(classDef);
		PopulateStartingDeck(defRepo, dto);
	}

	private static string GetClassNameLocalized(CharacterClassDef def)
	{
		string text = CrossReference.ClassIdToECharacterModel(def.ID.ID).ToString();
		string text2 = LocalizationManager.GetTranslation("FH/" + text);
		if (string.IsNullOrWhiteSpace(text2))
		{
			text2 = text;
		}
		return text2;
	}

	public static PartySlotDTO PopulateSlotData(int slotId, CharacterState characterState, MultiplayerGameState.Player player, bool isLeader, bool isAvailable, bool isSelected)
	{
		if (characterState == null)
		{
			return new PartySlotDTO
			{
				SlotId = slotId,
				MultiplayerUserData = null
			};
		}
		string iD = characterState.ClassID.ID;
		ECharacter character = CrossReference.ClassIdToECharacterModel(iD);
		return new PartySlotDTO
		{
			SlotId = slotId,
			Character = character,
			ClassID = iD,
			Portrait = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
			Illustration = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
			Color = UIInfoTools.Instance.GetCharacterColor(character),
			Name = characterState.Name,
			Gold = characterState.Gold,
			Level = characterState.Level,
			ShowValues = true,
			HasAvailableCardUnlocks = (isAvailable && characterState.CardUnlocks != 0),
			HasAvailablePerkMarks = (isAvailable && characterState.PerkMarks != 0),
			IsAvailable = isAvailable,
			IsSelected = isSelected,
			MultiplayerUserData = null,
			IsReady = true
		};
	}

	public static List<PartySlotDTO> PopulatePartySlotDTOs(TestingCharacterCreationFlowViewData controller, TestingCharacterCreationFlowViewData.PartySlotDesc[] partySlotData)
	{
		List<PartySlotDTO> list = new List<PartySlotDTO>(partySlotData.Length);
		for (uint num = 0u; num < partySlotData.Length; num++)
		{
			TestingCharacterCreationFlowViewData.PartySlotDesc partySlotDesc = partySlotData[num];
			NetworkPlayer playerAssignedToSlot = PlayerRegistry.GetPlayerAssignedToSlot(partySlotDesc.ID);
			MultiplayerGameState.Player player = ((playerAssignedToSlot != null) ? controller.GetPlayer(playerAssignedToSlot.PlayerID) : null);
			CharacterState character = partySlotDesc.Character;
			bool isLeader = true;
			bool isAvailable = true;
			bool isSelected = true;
			PartySlotDTO item = PopulateSlotData((int)partySlotDesc.ID.ID, character, player, isLeader, isAvailable, isSelected);
			list.Add(item);
		}
		return list;
	}

	public static List<PartySlotDTO> GetAvailableClassesData(TestingCharacterCreationFlowViewData controller, CharacterClassDef selectedCharacterDef)
	{
		ECharacter selectedCharacterType = ((selectedCharacterDef != null) ? CrossReference.ClassIdToECharacterModel(selectedCharacterDef.ID.ID) : ECharacter.None);
		return controller.UnlockedClasses.Select((CharacterClassDef classDef, int index) =>
		{
			string iD = classDef.ID.ID;
			ECharacter eCharacter = CrossReference.ClassIdToECharacterModel(iD);
			return PartyViewService.FromClassType(index + 1, iD, eCharacter, selectedCharacterType == eCharacter, controller.CanCreateCharacterFromClass(classDef));
		}).ToList();
	}

	public static List<CharacterClassID> GetUnlockedClassesIds()
	{
		return new List<CharacterClassID>
		{
			new CharacterClassID("BannerSpearID"),
			new CharacterClassID("BoneshaperID"),
			new CharacterClassID("BlinkbladeID"),
			new CharacterClassID("GeminateID")
		};
	}

	public static List<ItemID> GetPurchasableItemsIds()
	{
		return new List<ItemID>
		{
			new ItemID(11u),
			new ItemID(120u),
			new ItemID(121u),
			new ItemID(122u),
			new ItemID(123u),
			new ItemID(124u),
			new ItemID(125u),
			new ItemID(126u),
			new ItemID(127u),
			new ItemID(128u)
		};
	}

	public static void AddRandomQuestOptions(DefRepository defRepo, MultiplayerGameState.Player player)
	{
		FHRandom fHRandom = new FHRandom();
		List<PersonalQuestID> list = defRepo.PersonalQuests.Keys.ToList();
		fHRandom.Shuffle(list);
		player.DrawnPersonalQuests.Clear();
		player.DrawnPersonalQuests.AddRange(list.Take(2));
	}

	public static void PopulateStartingDeck(DefRepository DefRepo, CharacterDataDTO _characterData)
	{
		_characterData.Cards.Clear();
		IEnumerable<ClassCardDef> enumerable = from def in _characterData.Class.GetAllCards(DefRepo)
			where def.Level == 1
			select def;
		int maxCards = (int)_characterData.Class.MaxCards;
		foreach (ClassCardDef item in enumerable)
		{
			_characterData.Cards.Add(new CardDTO(item, maxCards-- > 0));
		}
	}

	public static uint[] GetHighestCharacterPerLevelHealthTable(DefRepository defs)
	{
		List<uint> list = CollectionPool<List<uint>, uint>.Get();
		foreach (KeyValuePair<CharacterClassID, CharacterClassDef> characterClass in defs.CharacterClasses)
		{
			uint[] healthTable = characterClass.Value.HealthTable;
			for (int i = 0; i < healthTable.Length; i++)
			{
				if (list.Count <= i)
				{
					list.Add(healthTable[i]);
				}
				else if (list[i] < healthTable[i])
				{
					list[i] = healthTable[i];
				}
			}
		}
		uint[] result = list.ToArray();
		CollectionPool<List<uint>, uint>.Release(list);
		return result;
	}
}
