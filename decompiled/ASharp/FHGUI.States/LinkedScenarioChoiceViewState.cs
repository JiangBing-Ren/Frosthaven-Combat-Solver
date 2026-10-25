using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class LinkedScenarioChoiceViewState : FHMapViewState
{
	private readonly ScenarioViewService _scenarioService;

	public override StateCategory Category => StateCategory.WorldMap;

	[Preserve]
	private MainHeaderModule MainHeader => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private WorldMapModule WorldMap => View.Modules.WorldMap.WorldMap;

	[Preserve]
	private ScenarioListModule ScenarioList => View.Modules.WorldMap.ScenarioList;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncher => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private TextModule TextHint => View.Modules.WorldMap.TextHint;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	public LinkedScenarioChoiceViewState()
	{
		_scenarioService = View.GetService<ScenarioViewService>();
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		HeaderService.SetupHeader(MainHeader, HeaderViewService.HeaderFunctionality.None);
		QuickAccessBarService.Setup(MainViewMainCategories.WorldMap, MainViewSubCategories.WorldQuests, UIBuildingStateFilter.None, null, isInteractable: false);
		TextHint.SetText("GUI_OptionalScenarioChoice_Hint");
		if (GameState.SelectedScenario != null && !GameState.ActiveScenario.HasEnteredScenario)
		{
			if (!View.IsActionQueuedAfterUnresolvedActions(_scenarioService.SetupSelectedScenario))
			{
				View.ExecAfterNoUnresolvedActions(_scenarioService.SetupSelectedScenario);
			}
			return;
		}
		int level = GameController.ScenarioController.GetScenarioLevel();
		int partySize = View.GameState.PartyCharacters.Count();
		List<ScenarioInfoDTO> list = (from scenarioState in View.GameController.ScenarioController.GetLinkedScenarios()
			select new ScenarioInfoDTO(View.GameController, scenarioState, partySize, level, View.GameController.ScenarioController.IsScenarioPlayable(scenarioState))).ToList();
		list.Add(_scenarioService.ReturnToOutpostDTO);
		ScenarioDef selectedScenarioDef = View.SelectedScenarioDef;
		int id = _scenarioService.ReturnToOutpostDTO.Id;
		int selectedScenarioId = ((selectedScenarioDef != null) ? ((int)selectedScenarioDef.ID.ID) : id);
		bool flag = selectedScenarioId == id;
		ScenarioInfoDTO scenarioInfoDTO = list.FirstOrDefault((ScenarioInfoDTO dto) => dto.Id == selectedScenarioId);
		ScenarioRequirementsDto requirementsDto = ((selectedScenarioDef != null) ? new ScenarioRequirementsDto(selectedScenarioDef, View.GameState, invalidParty: false, partyMemberIsReadyToRetire: false, missingPersonalQuest: false) : new ScenarioRequirementsDto());
		ButtonsBarParam continueParams;
		if (flag)
		{
			_scenarioService.PrepareReturnToOutpost(out continueParams);
		}
		else
		{
			_scenarioService.PrepareScenario(selectedScenarioDef, out continueParams);
		}
		bool hasStartQuestAuthority = !flag || !View.IsOnline || View.IsPartyLeader || View.IsReadyCheckRunning;
		ScenarioLauncher.Init(scenarioInfoDTO, requirementsDto, continueParams, hasStartQuestAuthority);
		ScenarioList.Init(new ScenarioListModule.Params
		{
			selectedScenario = scenarioInfoDTO,
			availableScenarios = list,
			onScenarioClicked = OnScenarioSelected
		});
		View.OutpostSceneController.OpenWorldMap();
	}

	public override void OnHide()
	{
		View.OutpostSceneController.Reset();
	}

	private void OnScenarioSelected(ScenarioInfoDTO scenarioInfoDto)
	{
		if (!View.IsReadyCheckRunning)
		{
			View.SetSelectedScenario(scenarioInfoDto.Id);
			View.OutpostSceneController.WorldMapScene.FocusOnScenario(scenarioInfoDto.Id);
			OnShow();
		}
	}
}
