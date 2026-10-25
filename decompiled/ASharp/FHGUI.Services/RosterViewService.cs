using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.States;
using FHRL;

namespace FHGUI.Services;

public class RosterViewService : FHMapViewService
{
	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly PartyViewService _partyViewService;

	public bool HasRetiredCharacters => _view.GameState.RetiredCharacters.Any();

	public RosterViewService(FHMapView view, CharacterDetailsViewService characterDetailsService, PartyViewService partyViewService)
		: base(view)
	{
		_characterDetailsService = characterDetailsService;
		_partyViewService = partyViewService;
	}

	public int NumRetiredCharacters()
	{
		return _view.GameState.RetiredCharacters.Count;
	}

	public bool TryGetPlayerFromCharacterState(CharacterState characterState, out MultiplayerGameState.Player player)
	{
		player = null;
		if (characterState.PartyState == CharacterPartyStateType.Retired || characterState.PartyState == CharacterPartyStateType.Removed)
		{
			return false;
		}
		foreach (MultiplayerGameState.Character characterControl in _view.MultiplayerGameState.CharacterControls)
		{
			if (!(characterControl.CharacterID != characterState.ID))
			{
				player = MultiplayerGameController.GetSlotOwner(characterControl.SlotID);
				if (player != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<PartySlotDTO> GetActiveRosterSlots(bool includeActive, bool includeRetired, bool addEmptySlot = true)
	{
		List<PartySlotDTO> list = new List<PartySlotDTO>(_view.GameState.AllCharacters.Count + 1);
		if (includeActive)
		{
			foreach (CharacterState allCharacter in _view.GameState.AllCharacters)
			{
				PartySlotDTO partySlotDTO = new PartySlotDTO(_characterDetailsService.PopulateCharacterDTO(allCharacter));
				if (_view.IsOnline && TryGetPlayerFromCharacterState(allCharacter, out var player))
				{
					partySlotDTO.MultiplayerUserData = _view.GetPlayerData(player);
				}
				partySlotDTO.CanRetire = _view.GameController.CharacterController.IsPersonalQuestCompleted(allCharacter);
				list.Add(partySlotDTO);
			}
		}
		if (includeRetired)
		{
			foreach (CharacterState retiredCharacter in _view.GameState.RetiredCharacters)
			{
				PartySlotDTO partySlotDTO2 = new PartySlotDTO(_characterDetailsService.PopulateCharacterDTO(retiredCharacter));
				partySlotDTO2.Retired = true;
				list.Add(partySlotDTO2);
			}
		}
		if (addEmptySlot)
		{
			list.Add(PartySlotDTO.Empty());
		}
		return list;
	}

	public IEnumerable<UIPartySlotHints.HintedActionParams> GetInteractions(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty && HasAuthorityToCreateNewCharacter())
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_CREATE_CHARACTER_NAME_CONFIRM"),
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					StartNewCharacter(partySlotDto.SlotId);
				},
				ColorSchemeID = "Create"
			};
		}
		if (!partySlotDto.Retired && !partySlotDto.InCreation && HasAuthorityToJoinParty(partySlotDto) && CanJoinParty(partySlotDto) && _partyViewService.TryGetLocalPlayerFreePartySlot(out var slot))
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_JOIN_PARTY"),
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					AddCharacterToParty(partySlotDto, slot.ID.ID);
				},
				ColorSchemeID = "Join"
			};
		}
		if (!partySlotDto.CanRetire && !partySlotDto.InCreation && !partySlotDto.Retired && HasAuthorityToLeaveParty(partySlotDto))
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_LEAVE"),
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					LeaveParty(partySlotDto);
				},
				ColorSchemeID = "Leave"
			};
		}
		if (partySlotDto.CanRetire && HasAuthorityToRetireCharacter(partySlotDto))
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_RETIRE"),
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					if (GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out var character))
					{
						StartCharacterRetirement(character.ID);
					}
				},
				ColorSchemeID = "Retire"
			};
		}
		if (!partySlotDto.IsEmpty)
		{
			bool isLocalPlayerOwned = _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID == new PartySlotID(partySlotDto.SlotId));
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, isLocalPlayerOwned ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					ViewCharacter(partySlotDto, !isLocalPlayerOwned);
				},
				ColorSchemeID = "Edit"
			};
		}
	}

	public UIPartySlotHints.HintedActionParams GetLevelUPInteraction(PartySlotDTO partySlotDto)
	{
		if (!partySlotDto.Retired && !partySlotDto.InCreation && !partySlotDto.IsEmpty && HasAuthorityToLevelUpCharacterFromProsperity(partySlotDto))
		{
			return new UIPartySlotHints.HintedActionParams
			{
				ActionCallback = () =>
				{
					LevelUpCharacter(partySlotDto);
				}
			};
		}
		return null;
	}

	public UIPartySlotHints.HintedActionParams GetDeleteInteraction(PartySlotDTO partySlotDto)
	{
		if (!partySlotDto.Retired && !partySlotDto.InCreation && !partySlotDto.IsEmpty && !partySlotDto.CanRetire && HasAuthorityToDeleteCharacter(partySlotDto))
		{
			return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_DELETE_CHARACTER",
				ActionCallback = () =>
				{
					DeleteCharacter(partySlotDto);
				}
			};
		}
		return null;
	}

	public bool HasAuthorityToCreateNewCharacter()
	{
		return true;
	}

	private bool HasAuthorityToRetireCharacter(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			return false;
		}
		if (partySlotDto.InParty && _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID == new PartySlotID(partySlotDto.SlotId)))
		{
			return true;
		}
		return false;
	}

	private bool CanJoinParty(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			return false;
		}
		if (partySlotDto.IsEmpty || partySlotDto.InParty)
		{
			return false;
		}
		CharacterClassDef value = DefRepo.CharacterClasses.FirstOrDefault((KeyValuePair<CharacterClassID, CharacterClassDef> pair) => pair.Key.ID == partySlotDto.ClassID).Value;
		return !GameController.CharacterController.HasCharacterFromClassInParty(value);
	}

	private bool HasAuthorityToJoinParty(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			return false;
		}
		if (partySlotDto.IsEmpty || partySlotDto.InParty)
		{
			return false;
		}
		if (!_view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.Character == null))
		{
			return false;
		}
		return true;
	}

	private bool HasAuthorityToLeaveParty(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty || !partySlotDto.InParty)
		{
			return false;
		}
		if (!_view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID == new PartySlotID(partySlotDto.SlotId)))
		{
			return false;
		}
		return true;
	}

	private bool HasAuthorityToDeleteCharacter(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			return false;
		}
		bool flag = _view.MultiplayerGameState.PartyLeaderID == _view.PlayerID;
		if (!partySlotDto.InParty && !flag)
		{
			return false;
		}
		if (partySlotDto.InParty && !_view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID == new PartySlotID(partySlotDto.SlotId)))
		{
			return false;
		}
		return true;
	}

	private bool HasAuthorityToLevelUpCharacterFromProsperity(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			return false;
		}
		if (!partySlotDto.InParty && _view.IsPartyLeader)
		{
			return true;
		}
		if (_view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID == new PartySlotID(partySlotDto.SlotId)))
		{
			return true;
		}
		return false;
	}

	private void ViewCharacter(PartySlotDTO partySlotDto, bool readOnly)
	{
		CharacterClassDef classDef = DefRepo.CharacterClasses.FirstOrDefault((KeyValuePair<CharacterClassID, CharacterClassDef> pair) => pair.Key.ID == partySlotDto.ClassID).Value;
		if (classDef == null)
		{
			return;
		}
		CharacterState character;
		if (partySlotDto.Retired)
		{
			CharacterState characterState = _view.GameState.RetiredCharacters.FirstOrDefault((CharacterState c) => c.ClassDef == classDef && c.Name == partySlotDto.Name);
			_view.PushState(new EditCharacterViewState(_characterDetailsService.PopulateCharacterDTO(characterState), readOnly: true, dontRefresh: true));
		}
		else if (!partySlotDto.InParty)
		{
			CharacterState characterState2 = _view.GameState.AllCharacters.FirstOrDefault((CharacterState c) => c.ClassDef == classDef && c.Name == partySlotDto.Name);
			_view.PushState(new EditCharacterViewState(_characterDetailsService.PopulateCharacterDTO(characterState2), readOnly: true));
		}
		else if (GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out character))
		{
			_view.SetSelectedCharacter(character, partySlotDto.SlotId);
			if (readOnly)
			{
				_view.PushState(new EditCharacterViewState(_characterDetailsService.PopulateCharacterDTO(character), readOnly));
			}
			else
			{
				_view.PushState(new EditCharacterViewState());
			}
		}
	}

	private void AddCharacterToParty(PartySlotDTO partySlotDto, uint slot)
	{
		GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out var character);
		_partyViewService.AddCharacterToParty(character, slot);
	}

	private void LeaveParty(PartySlotDTO partySlotDto)
	{
		GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out var character);
		_partyViewService.RemoveCharacterFromParty(character);
	}

	private void DeleteCharacter(PartySlotDTO partySlotDto)
	{
		GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out var character);
		_partyViewService.DeleteCharacter(character);
	}

	private void LevelUpCharacter(PartySlotDTO partySlotDto)
	{
		if (GameController.CharacterController.TryGetCharacter(partySlotDto.CharacterID, out var character))
		{
			_partyViewService.LeveUpCharacter(character);
		}
	}

	private void StartNewCharacter(int slotId)
	{
		_view.PushState(new NewCharacterViewState(slotId, resetDetails: true));
	}

	public void StartCharacterRetirement(CharacterID characterID)
	{
		Choreographer.ExecuteCommand(FHRPC.Campaign.Character.StartRetire, characterID);
		if (MultiplayerGameController.CanDrawPesonalQuests(_view.PlayerID))
		{
			Choreographer.ExecuteCommand("DrawPersonalQuests", _view.PlayerID);
		}
	}

	public void FinalizeCharacterRetirement(CharacterID characterID, string additionalQuestDefId, List<ResourceDistribution> distribution)
	{
		PersonalQuestID key = new PersonalQuestID(additionalQuestDefId);
		DefRepo.PersonalQuests.TryGetValue(key, out var value);
		Choreographer.ExecuteCommand(FHRPC.Campaign.Character.FinishRetire, characterID, value, distribution);
		Choreographer.ExecuteCommand("ClearPersonalQuests", _view.PlayerID);
	}

	public void CancelRetire(CharacterID characterID)
	{
		if (GameController.CharacterController.TryGetCharacter(characterID, out var character) && character.PartyState == CharacterPartyStateType.RetireProcess)
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Character.CancelRetire, characterID);
		}
	}

	public bool HasPendingRetireDistribution(CharacterState characterState)
	{
		if (!characterState.Resources.HasMaterials)
		{
			return false;
		}
		int num = 0;
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			if (partyCharacter != characterState && partyCharacter.PartyState != CharacterPartyStateType.RetireProcess)
			{
				num++;
			}
		}
		if (num > 1)
		{
			return true;
		}
		return false;
	}

	public IEnumerable<OutpostEventDTO.Effect> GetRetireRewards(CharacterState characterState)
	{
		if (characterState == null || string.IsNullOrEmpty(characterState.ClassDef.RetirementCampaignSection))
		{
			yield break;
		}
		CampaignSectionID campaignSectionID = new CampaignSectionID(characterState.ClassDef.RetirementCampaignSection);
		if (!DefRepo.CampaignSections.ContainsKey(campaignSectionID))
		{
			yield break;
		}
		CampaignSectionDef campaignSectionDef = DefRepo.CampaignSections[campaignSectionID];
		if (!campaignSectionDef.Repeatable && _view.GameState.ActivatedCampaignSectionIDs.Contains(campaignSectionID))
		{
			yield break;
		}
		OutpostEventsService service = _view.GetService<OutpostEventsService>();
		List<CampaignEffect> list = service.CombineCampaignEffects(campaignSectionDef.Effects);
		foreach (CampaignEffect item in list)
		{
			OutpostEventDTO.Effect formattedEffect = service.GetFormattedEffect(item);
			if (!formattedEffect.Hidden)
			{
				yield return formattedEffect;
			}
		}
	}

	public List<ResourceDistribution> GetRetireDistribution(CharacterState characterState)
	{
		List<ResourceDistribution> list = new List<ResourceDistribution>();
		if (characterState == null || characterState.Resources.Total == 0)
		{
			return list;
		}
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			ResourceDistribution item = new ResourceDistribution
			{
				CharacterID = partyCharacter.ID
			};
			list.Add(item);
		}
		if (list.Count == 0)
		{
			return list;
		}
		List<OutpostMaterialType> list2 = characterState.Resources.ToMaterialList();
		int num = 0;
		foreach (OutpostMaterialType item2 in list2)
		{
			list[num % list.Count].Resources.AddMaterial(item2);
			num++;
		}
		return list;
	}

	public bool HasInspirationForAdditionalQuestUnlock(CharacterState characterState)
	{
		return characterState.InspirationAllocatedForRetirement;
	}
}
