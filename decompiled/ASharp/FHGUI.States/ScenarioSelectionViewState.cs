using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ScenarioSelectionViewState : FHMapViewState
{
	private readonly List<ScenarioInfoDTO> m_AvailableScenarios = new List<ScenarioInfoDTO>(150);

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
	private StoryGoalsModule StoryGoalsModule => View.Modules.WorldMap.StoryGoalsModule;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	public bool IsScenarioSelected { get; private set; }

	public ScenarioSelectionViewState()
	{
		if (!View.IsReadyCheckRunning)
		{
			View.SetSelectedScenario(-1);
		}
	}

	public override void OnShow()
	{
		using (new ScenarioController.ScenarioCalculationScope())
		{
			View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
			bool flag = View.IsReadyCheckRunning && View.Player.Ready;
			ScenarioViewService service = View.GetService<ScenarioViewService>();
			HeaderService.SetupHeader(MainHeader, (!flag) ? HeaderViewService.HeaderFunctionality.All : HeaderViewService.HeaderFunctionality.None);
			QuickAccessBarService.Setup(MainViewMainCategories.WorldMap, MainViewSubCategories.WorldQuests, UIBuildingStateFilter.None, null, !flag);
			if (GameState.SelectedScenario != null && !GameState.ActiveScenario.HasEnteredScenario)
			{
				return;
			}
			int scenarioLevel = GameController.ScenarioController.GetScenarioLevel();
			int partySize = View.GameState.PartyCharacters.Count();
			m_AvailableScenarios.Clear();
			foreach (ScenarioState unlockedScenario in View.GameController.ScenarioController.GetUnlockedScenarios())
			{
				if (!unlockedScenario.Completed || !unlockedScenario.Def.CanNotBeReplayed)
				{
					m_AvailableScenarios.Add(new ScenarioInfoDTO(View.GameController, unlockedScenario, partySize, scenarioLevel, View.GameController.ScenarioController.IsScenarioPlayable(unlockedScenario.Def)));
				}
			}
			ScenarioDef selectedScenarioDef = View.SelectedScenarioDef;
			int selectedScenarioId = (int)((selectedScenarioDef != null) ? selectedScenarioDef.ID.ID : 0);
			ScenarioInfoDTO scenarioInfoDTO = m_AvailableScenarios.FirstOrDefault((ScenarioInfoDTO dto) => dto.Id == selectedScenarioId);
			bool flag2 = !View.GameController.CharacterController.IsValidParty(throwException: false);
			bool partyMemberIsReadyToRetire = View.GameState.PartyCharacters.Any((CharacterState x) => View.GameController.CharacterController.IsPersonalQuestCompleted(x));
			bool missingPersonalQuest = !View.GameController.ScenarioController.IsScenarioPlayableForPersonalQuest(selectedScenarioDef);
			if (scenarioInfoDTO != null && scenarioInfoDTO.IsMandatory)
			{
				partyMemberIsReadyToRetire = false;
				if (flag2)
				{
					Log.GUIWarning("Invalid Party Detected during Mandatory Quest");
				}
				flag2 = false;
			}
			ScenarioRequirementsDto requirementsDto = ((selectedScenarioDef != null) ? new ScenarioRequirementsDto(selectedScenarioDef, View.GameState, flag2, partyMemberIsReadyToRetire, missingPersonalQuest) : new ScenarioRequirementsDto());
			service.PrepareScenario(selectedScenarioDef, out var continueParams);
			bool hasStartQuestAuthority = !View.IsOnline || View.IsPartyLeader || View.IsReadyCheckRunning;
			ScenarioLauncher.Init(scenarioInfoDTO, requirementsDto, continueParams, hasStartQuestAuthority);
			ScenarioList.Init(new ScenarioListModule.Params
			{
				selectedScenario = scenarioInfoDTO,
				availableScenarios = m_AvailableScenarios,
				onScenarioClicked = OnScenarioSelected
			});
			View.OutpostSceneController.OpenWorldMap();
			if (StoryGoalsModule != null)
			{
				PuzzleState puzzleState = View.GameController.PuzzleController.IncompletePuzzles.FirstOrDefault();
				if (puzzleState != null && !ScenarioLauncher.isActiveAndEnabled)
				{
					StoryGoalsModule.gameObject.SetActive(value: true);
					StoryGoalsModule.Init(new StoryGoalsModule.Params
					{
						Puzzle = new PuzzleDTO(puzzleState.Def, View.GameController)
					});
				}
				else
				{
					StoryGoalsModule.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public override void OnHide()
	{
		if (!View.IsReadyCheckRunning)
		{
			View.SetSelectedScenario(-1);
		}
		View.OutpostSceneController.Reset();
	}

	public void OnScenarioSelected(ScenarioInfoDTO scenarioInfoDto)
	{
		if (!View.IsReadyCheckRunning)
		{
			IsScenarioSelected = scenarioInfoDto != null;
			View.SetSelectedScenario(scenarioInfoDto?.Id ?? (-1));
			if (scenarioInfoDto != null && scenarioInfoDto.Id >= 0)
			{
				View.OutpostSceneController.WorldMapScene.FocusOnScenario(scenarioInfoDto.Id);
			}
		}
	}
}
