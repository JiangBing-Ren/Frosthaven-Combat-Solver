using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class RenameCharacterViewState : FHMapViewState
{
	private readonly CharacterID _characterID;

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly PartyViewService _partyService;

	private readonly bool _creationProcess;

	private readonly bool _unassignedCharacter;

	private string _characterName;

	private FHTabButton _activeTab;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	private HeroClassStatsModule HeroClassStatsModule => View.Modules.Characters.HeroClassStats;

	[Preserve]
	private CharacterBiographyModule CharacterBioModule => View.Modules.Characters.CharacterBio;

	[Preserve]
	private CharacterRenameModule CharacterRenameModule => View.Modules.Characters.CharacterRename;

	[Preserve]
	private HandCardListModule HandCardListModule => View.Modules.Characters.AbilitiesHandCard;

	public RenameCharacterViewState(CharacterState character, string characterName, bool creationProcess, bool unassignedCharacter)
		: this(character.ID, characterName, creationProcess, unassignedCharacter)
	{
	}

	public RenameCharacterViewState(CharacterID characterID, string characterName, bool creationProcess, bool unassignedCharacter)
	{
		_characterID = characterID;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_partyService = View.GetService<PartyViewService>();
		_creationProcess = creationProcess;
		_unassignedCharacter = unassignedCharacter;
		_characterName = characterName;
	}

	public override void OnShow()
	{
		if (!_unassignedCharacter && !_partyService.LocalPlayerOwnsCharacter(_characterID))
		{
			View.ResetView();
			return;
		}
		if ((_unassignedCharacter && !_partyService.HasCharacterInRoster(_characterID)) || (!_unassignedCharacter && !_partyService.LocalPlayerOwnsCharacter(_characterID)))
		{
			PopThisState();
			return;
		}
		SavePayload();
		bool flag = IsNameValid(_characterName);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(OnBack), _creationProcess ? CharacterDetailsScreenModule.Button_Continue(OnContinue, flag) : CharacterDetailsScreenModule.Button_Rename(OnContinue, flag));
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_CHARACTER_INVALID_NAME");
		}
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			PopThisState();
			return;
		}
		CharacterDataDTO characterDataDTO = _characterDetailsService.PopulateCharacterDTO(character);
		_characterDetailsService.PopulateCharacterDetails(characterDataDTO, character.PartyState == CharacterPartyStateType.CreationProcess, CharacterPreviewModule, HeroClassStatsModule, CharacterBioModule, HandCardListModule);
		View.GameController.CharacterController.GetCharacterNameSizeRange(out var _, out var max);
		CharacterRenameModule.Show(_characterName, max, OnNameChanged, OnNameSubmit);
		if (_creationProcess)
		{
			bool excludePersonalQuest = !_characterDetailsService.HasAvailablePersonalQuest(View.PlayerID);
			CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(excludePersonalQuest), 1);
		}
		CharacterDetailsScreenModule.SetCharacterClass(characterDataDTO.Class);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Rename);
		View.OutpostSceneController.OpenCharacterScreen(characterDataDTO);
		LoadPayload();
	}

	public override void OnHide()
	{
		ClearPayload();
		CharacterDetailsScreenModule.ClearAll();
	}

	protected override void SavePayload()
	{
		_activeTab = CharacterDetailsScreenModule.InfoCategoriesActiveTab;
	}

	protected override void LoadPayload()
	{
		CharacterDetailsScreenModule.TrySetInfoCategoriesActiveTab(_activeTab);
	}

	protected override void ClearPayload()
	{
		_activeTab = null;
	}

	private void OnNameChanged(string newName)
	{
		_characterName = newName;
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character) && !(character.Name == newName))
		{
			OnShow();
		}
	}

	private void OnNameSubmit(string value)
	{
		OnNameChanged(value);
		if (IsNameValid(_characterName))
		{
			OnContinue();
		}
	}

	private bool IsNameValid(string nameCheck)
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return false;
		}
		if (nameCheck == character.Name)
		{
			return true;
		}
		return View.GameController.CharacterController.IsCharacterNameValid(nameCheck);
	}

	private void OnBack()
	{
		if (_creationProcess)
		{
			if (MultiplayerGameController.GetPersonalQuests(View.PlayerID).Any())
			{
				Choreographer.ExecuteCommand("ClearPersonalQuests", View.PlayerID);
			}
			View.GetService<PartyViewService>().CancelCreateNewCharacter(_characterID);
		}
		PopThisState();
	}

	private void OnContinue()
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return;
		}
		if (character.Name != _characterName)
		{
			FHMultiplayerClient.Promise promise = Choreographer.ExecuteCommand(FHRPC.Campaign.Character.RenameCharacter, character.ID, _characterName);
			if (_creationProcess)
			{
				promise.OnFailed(PopThisState);
			}
		}
		if (_creationProcess)
		{
			if (_characterDetailsService.HasAvailablePersonalQuest(View.PlayerID))
			{
				_characterDetailsService.ToPersonalQuestSelectionState(character, _creationProcess, _unassignedCharacter);
			}
			else
			{
				View.PushState(new SpendStartingGoldViewState(character, _creationProcess, _unassignedCharacter));
			}
		}
		else
		{
			PopThisState();
		}
	}
}
