using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.DTO;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingCharacterCreationFlowViewData : MonoBehaviour
{
	private struct StateStackOperation
	{
		public TestState State;

		public bool IsPush => State != null;

		public bool IsPop => !IsPush;
	}

	public struct PartySlotDesc
	{
		public PartySlotID ID;

		public PlayerID PlayerID;

		public CharacterState Character;

		public bool IsValid()
		{
			return ID.IsValid;
		}
	}

	[SerializeField]
	private GameObject m_UIRoot;

	public readonly List<CharacterState> AllCharacters = new List<CharacterState>();

	public readonly List<CharacterClassID> UnlockedClassIDs = TestingCharacterCreationFlowUtils.GetUnlockedClassesIds();

	public readonly List<ItemID> PurchasableItemIDs = TestingCharacterCreationFlowUtils.GetPurchasableItemsIds();

	public PartySlotDesc[] PartySlotData = new PartySlotDesc[4];

	public List<MultiplayerGameState.Player> Players = new List<MultiplayerGameState.Player>();

	private Stack<TestState> m_StateStack = new Stack<TestState>();

	private Queue<StateStackOperation> m_StateStackOperationQueue = new Queue<StateStackOperation>();

	private bool m_RefreshView;

	private bool m_Initialzied;

	public DefRepository DefRepo { get; private set; }

	public List<FHMapViewModule> AllModules { get; private set; }

	public TestState CurrentView
	{
		get
		{
			if (!m_StateStack.TryPeek(out var result))
			{
				return null;
			}
			return result;
		}
	}

	public IEnumerable<CharacterClassDef> UnlockedClasses => DefRepo.CharacterClasses.FromKeys(UnlockedClassIDs);

	public IEnumerable<ItemDef> PurchasableItems => DefRepo.Items.FromKeys(PurchasableItemIDs);

	public CharacterState SelectedCharacter { get; private set; }

	public int SelectedPartySlotId { get; private set; }

	private void Init()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			DefRepo = YMLDefRepository.Repo.DefRepository;
			SetupPartyData();
			AllModules = m_UIRoot.GetComponentsInChildren<FHMapViewModule>(includeInactive: true).ToList();
		}
	}

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
		SetState(new PartyCreationState(this));
	}

	private void Update()
	{
		if (m_StateStackOperationQueue.Any())
		{
			if (CurrentView != null)
			{
				CurrentView.OnHide();
			}
			Queue<StateStackOperation> queue = new Queue<StateStackOperation>(m_StateStackOperationQueue);
			m_StateStackOperationQueue.Clear();
			StateStackOperation result;
			while (queue.TryDequeue(out result))
			{
				if (result.IsPop)
				{
					m_StateStack.TryPop(out var _);
				}
				else
				{
					m_StateStack.Push(result.State);
				}
			}
			if (CurrentView != null)
			{
				List<IUIViewModule> nextModules = CurrentView.Modules.ToList();
				AllModules.ForEach((FHMapViewModule m) =>
				{
					m.gameObject.SetActive(nextModules.Contains(m));
				});
				m_RefreshView = true;
			}
			else
			{
				AllModules.ForEach((FHMapViewModule m) =>
				{
					m.gameObject.SetActive(value: false);
				});
			}
		}
		if (!m_StateStack.Any())
		{
			m_StateStack.Push(new PartyCreationState(this));
			CurrentView?.OnShow();
			m_RefreshView = false;
		}
		if (m_RefreshView)
		{
			CurrentView?.OnShow();
			m_RefreshView = false;
		}
	}

	public void RefreshView()
	{
		m_RefreshView = true;
	}

	public void PushState(TestState state)
	{
		m_StateStackOperationQueue.Enqueue(new StateStackOperation
		{
			State = state
		});
	}

	public void PopState()
	{
		m_StateStackOperationQueue.Enqueue(default);
	}

	public void SetState(TestState state)
	{
		m_StateStackOperationQueue.Clear();
		m_StateStackOperationQueue.Enqueue(new StateStackOperation
		{
			State = state
		});
	}

	public T GetModule<T>() where T : IUIViewModule
	{
		return AllModules.OfType<T>().First();
	}

	public List<PartySlotDTO> GetPartySlotDTOs()
	{
		return TestingCharacterCreationFlowUtils.PopulatePartySlotDTOs(this, PartySlotData);
	}

	private void SetupPartyData()
	{
		MultiplayerGameState.Player player = new MultiplayerGameState.Player(new PlayerID(1));
		TestingCharacterCreationFlowUtils.AddRandomQuestOptions(DefRepo, player);
		Players.Add(player);
		for (uint num = 0u; num < PartySlotData.Length; num++)
		{
			PartySlotDesc partySlotDesc = new PartySlotDesc
			{
				ID = new PartySlotID(num + 1),
				PlayerID = player.ID
			};
			PartySlotData[num] = partySlotDesc;
		}
	}

	public bool CanCreateCharacterFromClass(CharacterClassDef def)
	{
		return true;
	}

	public CharacterClassDef ECharacterModelToClassDef(ECharacter model)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass s) => s.CharacterModel == model);
		if (cCharacterClass == null)
		{
			Log.Error(string.Format("{0} is not a recognized {1}!", model, "CCharacterClass"));
			return null;
		}
		CharacterClassID characterClassID = new CharacterClassID(cCharacterClass.CharacterID);
		if (!DefRepo.CharacterClasses.TryGetValue(characterClassID, out var value))
		{
			Log.Error(string.Format("{0} with {1} {2}!", "CharacterClassDef", "CharacterClassID", characterClassID));
			return null;
		}
		return value;
	}

	public void SetSelectedCharacter(CharacterState character, int slotIndex)
	{
		if (slotIndex < 0 && character != null)
		{
			PartySlotDTO partySlotDTO = GetPartySlotDTOs().FirstOrDefault((PartySlotDTO s) => s.ClassID == character.ClassID.ID);
			if (partySlotDTO.ClassID == character.ClassID.ID)
			{
				slotIndex = partySlotDTO.SlotId;
			}
		}
		SelectedCharacter = character;
		SelectedPartySlotId = slotIndex;
		RefreshView();
	}

	public bool IsCharacterNameValid(string characterName)
	{
		if (characterName.Length < 2 || characterName.Length > 18)
		{
			return false;
		}
		return !string.IsNullOrWhiteSpace(characterName);
	}

	public bool TryGetCharacter(CharacterClassDef def, out CharacterState character)
	{
		character = AllCharacters.FirstOrDefault((CharacterState c) => c.ClassDef == def);
		return character != null;
	}

	public bool CanCreateCharacter(CharacterClassDef def, string name, bool throwsException = false)
	{
		if (!CanCreateCharacterFromClass(def))
		{
			return false;
		}
		return IsCharacterNameValid(name);
	}

	public CharacterState CreateNewCharacter(CharacterClassDef def, string playerName, int slotId)
	{
		if (def == null)
		{
			throw new ArgumentNullException("CharacterClassDef cannot be null");
		}
		if (!CanCreateCharacter(def, playerName))
		{
			throw new Exception("Character cannot be created");
		}
		CharacterState characterState = new CharacterState(def, DefRepo, CharacterID.NewID())
		{
			Name = (string.IsNullOrEmpty(playerName) ? def.Name : playerName),
			PartyState = CharacterPartyStateType.CreationProcess,
			Gold = 30u
		};
		characterState.PersonalQuest = null;
		int num = slotId - 1;
		PartySlotDesc partySlotDesc = PartySlotData[num];
		partySlotDesc.Character = characterState;
		PartySlotData[num] = partySlotDesc;
		AllCharacters.Add(characterState);
		return characterState;
	}

	public void CancelCreateNewCharacter(CharacterClassDef classDef)
	{
		CharacterState characterState = AllCharacters.Find((CharacterState c) => c.ClassDef == classDef);
		if (characterState == null)
		{
			return;
		}
		AllCharacters.Remove(characterState);
		for (int num = 0; num < PartySlotData.Length; num++)
		{
			PartySlotDesc partySlotDesc = PartySlotData[num];
			if (partySlotDesc.Character != null && !(partySlotDesc.Character.ClassDef != classDef))
			{
				partySlotDesc.Character = null;
				PartySlotData[num] = partySlotDesc;
				break;
			}
		}
	}

	public MultiplayerGameState.Player GetPlayer(CharacterState character)
	{
		PartySlotDesc slot = PartySlotData.FirstOrDefault((PartySlotDesc p) => p.Character == character);
		if (!slot.IsValid())
		{
			return null;
		}
		return Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == slot.PlayerID);
	}

	public MultiplayerGameState.Player GetPlayer(PlayerID playerID)
	{
		return Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == playerID);
	}

	public IEnumerable<ItemDef> GetPurchasableItemDefs()
	{
		DefRepo.Items.FromKeys(PurchasableItemIDs);
		return PurchasableItems.Where((ItemDef item) => item.CanBePurchased);
	}

	public IEnumerable<ItemDataDTO> GetPurchasableItems(CharacterState character)
	{
		return FromItemDef(GetPurchasableItemDefs(), character, showCost: true);
	}

	private List<ItemDataDTO> FromItemDef(IEnumerable<ItemDef> items, CharacterState characterState, bool showCost = false)
	{
		return (from i in items
			group i by i.ID).Select((IGrouping<ItemID, ItemDef> i) =>
		{
			ItemDef itemDef = items.First((ItemDef ii) => ii.ID == i.Key);
			int num = i.Count();
			return new ItemDataDTO(itemDef, num, showCost, isNew: false, itemDef.GoldCost <= characterState.Gold && !characterState.OwnedItems.Contains(itemDef), characterState.EquipedItemIDs.Contains(itemDef.ID), 0, itemDef.GoldCost <= characterState.Gold, characterState.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == itemDef.ID))
			{
				Quantity = (uint)num
			};
		}).ToList();
	}

	public Dictionary<ItemEquipType, IEnumerable<ItemDataDTO>> GetPurchasableItemsBySlotType(CharacterState character)
	{
		IEnumerable<ItemDef> purchasableItemDefs = GetPurchasableItemDefs();
		DefRepository defRepo = DefRepo;
		return (from i in purchasableItemDefs
			orderby i.ID
			group i by i.Type).ToDictionary((IGrouping<ItemEquipType, ItemDef> g) => g.Key, (IGrouping<ItemEquipType, ItemDef> g) => (from item in g
			group item by item.ID).Select((IGrouping<ItemID, ItemDef> i) =>
		{
			ItemDef itemDef = defRepo.Items[i.Key];
			int num = i.Count();
			return new ItemDataDTO(itemDef, num, showCost: true, isNew: false, itemDef.GoldCost <= character.Gold && !character.OwnedItems.Contains(itemDef), character.EquipedItemIDs.Contains(itemDef.ID), 0, itemDef.GoldCost <= character.Gold, character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == itemDef.ID))
			{
				Quantity = (uint)num
			};
		}));
	}

	public IEnumerable<ItemDataDTO> GetOwnedItems(CharacterState character, bool includeSellCost = false)
	{
		return character.OwnedItems.Select((ItemDef item) => new ItemDataDTO(item, -1, includeSellCost, isNew: false, canBuy: false, IsItemEquipped(item.ID, character), includeSellCost ? 42 : 0, item.GoldCost <= character.Gold, character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == item.ID)));
	}

	public IEnumerable<ItemDataDTO> GetEquippedItems(CharacterState character, bool includeSellCost = false)
	{
		return character.EquipedItems.Select((ItemDef item) => new ItemDataDTO(item, -1, includeSellCost, isNew: false, canBuy: false, isEquipped: true, includeSellCost ? 42 : 0, item.GoldCost <= character.Gold, character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == item.ID)));
	}

	public uint GetMaxUnlockedSmallItems(CharacterState character)
	{
		return character.Level / 2 + character.Level % 2;
	}

	public uint GetMaxSmallItems(CharacterState character, bool includeScenarioModifiers = true)
	{
		return 5u;
	}

	public ItemDataDTO GetFlipItem(uint itemID)
	{
		ItemDef def = DefRepo.Items[new ItemID(itemID)];
		ItemDef itemDef = (def.FlipScenarioItemID.HasValue ? DefRepo.Items.Values.FirstOrDefault((ItemDef i) => i.ScenarioItemID == def.FlipScenarioItemID.Value) : null);
		if (itemDef != null)
		{
			return new ItemDataDTO(itemDef);
		}
		return null;
	}

	public uint GetMaxOneHandSlots(CharacterState character)
	{
		return 1u;
	}

	public bool IsItemEquipped(ItemID itemID, CharacterState character)
	{
		return character.EquipedItemIDs.Contains(itemID);
	}

	public PersonalQuestDef GetQuestDef(PersonalQuestID id)
	{
		return DefRepo.PersonalQuests.FirstOrDefault((KeyValuePair<PersonalQuestID, PersonalQuestDef> q) => q.Key == id).Value;
	}

	public int GetEmptyPartySlotId()
	{
		for (int i = 0; i < PartySlotData.Length; i++)
		{
			if (PartySlotData[i].Character == null)
			{
				return i + 1;
			}
		}
		return 0;
	}

	public int GetPartySlotIndex(CharacterState character)
	{
		for (int i = 0; i < PartySlotData.Length; i++)
		{
			if (PartySlotData[i].Character == character)
			{
				return i;
			}
		}
		return -1;
	}

	public CharacterState EnsureCharacter()
	{
		CharacterState selectedCharacter = SelectedCharacter;
		if (selectedCharacter != null)
		{
			return selectedCharacter;
		}
		if (AllCharacters.Count > 0)
		{
			return AllCharacters[0];
		}
		CharacterClassDef def = UnlockedClasses.FirstOrDefault((CharacterClassDef def2) => CanCreateCharacterFromClass(def2));
		return CreateNewCharacter(def, "Generic Character", GetEmptyPartySlotId());
	}

	public CharacterState GetCharacterAtSlot(int slotSlotId)
	{
		for (int i = 0; i < PartySlotData.Length; i++)
		{
			if (PartySlotData[i].ID.ID == slotSlotId)
			{
				return PartySlotData[i].Character;
			}
		}
		return null;
	}

	public void RemoveCharacterAtSlot(int slotSlotId)
	{
		int num = slotSlotId - 1;
		PartySlotDesc partySlotDesc = PartySlotData[num];
		partySlotDesc.Character = null;
		PartySlotData[num] = partySlotDesc;
	}
}
