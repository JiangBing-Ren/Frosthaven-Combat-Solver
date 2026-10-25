using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class HeroRetirementView : FHMapViewState
{
	private readonly RosterViewService _rosterService;

	private CharacterState _characterState;

	private string _selectedAdditionalQuestID;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private CharacterRetireModule RetireModule => View.Modules.Characters.RetireModule;

	public HeroRetirementView(CharacterState character, string questId = null)
	{
		_characterState = character;
		_rosterService = View.GetService<RosterViewService>();
		_selectedAdditionalQuestID = questId;
	}

	public override void OnShow()
	{
		PartySlotDTO characterData = new PartySlotDTO(View.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(_characterState));
		PlayerID id = View.PlayerID;
		if (View.IsOnline)
		{
			if (!_rosterService.TryGetPlayerFromCharacterState(_characterState, out var player))
			{
				PopThisState();
				return;
			}
			id = player.ID;
		}
		List<CharacterPersonalQuestDTO> questOptions = (from def in MultiplayerGameController.GetPersonalQuests(id)
			select new CharacterPersonalQuestDTO(def, GameController.CharacterController.CampaignController)).ToList();
		CharacterRetireModule.Params retirementParams = new CharacterRetireModule.Params
		{
			CharacterData = characterData,
			CloseCallback = () =>
			{
				_rosterService.CancelRetire(_characterState.ID);
				PopThisState();
			},
			NextStepCallback = FinalizeQuestChoice,
			QuestOptions = questOptions,
			ChooseQuestCallback = SelectAdditionalQuest,
			SelectedAdditionalQuestID = _selectedAdditionalQuestID,
			HasInspiration = _rosterService.HasInspirationForAdditionalQuestUnlock(_characterState)
		};
		RetireModule.Init(retirementParams);
	}

	private void SelectAdditionalQuest(string questId)
	{
		_selectedAdditionalQuestID = ((_selectedAdditionalQuestID == questId) ? null : questId);
		View.RefreshView();
	}

	private void FinalizeQuestChoice()
	{
		PopThisState();
		View.PushState(new GenericModalViewState(LocalizationManager.GetTranslation("GUI_RETIRE_CONFIRMATION"), LocalizationManager.GetTranslation("GUI_RETIRE_ANNOUNCE_CAPTION"), (bool retireConfirmed) =>
		{
			if (retireConfirmed)
			{
				if (_rosterService.HasPendingRetireDistribution(_characterState))
				{
					View.PushState(new RetirementMaterialDistributionViewState(_characterState, _selectedAdditionalQuestID)
					{
						OnConfirm = (List<ResourceDistribution> distr) =>
						{
							_rosterService.FinalizeCharacterRetirement(_characterState.ID, _selectedAdditionalQuestID, distr);
						}
					}, FHMapViewLayer.LayerType.Modal);
				}
				else
				{
					_rosterService.FinalizeCharacterRetirement(_characterState.ID, _selectedAdditionalQuestID, _rosterService.GetRetireDistribution(_characterState));
				}
			}
			else
			{
				View.PushState(new HeroRetirementView(_characterState));
			}
		}));
	}
}
