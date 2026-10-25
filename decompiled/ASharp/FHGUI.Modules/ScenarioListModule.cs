using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using Libs.GameConsole;
using UnityEngine;

namespace FHGUI.Modules;

public class ScenarioListModule : FHMapViewModule
{
	private enum ScenarioFilter
	{
		All = 1,
		TreasureChests = 2,
		RandomItems = 4
	}

	public record Params
	{
		public ScenarioInfoDTO selectedScenario;

		public List<ScenarioInfoDTO> availableScenarios;

		public Action<ScenarioInfoDTO> onScenarioClicked;
	}

	[ConsoleVariable(Alias = "scenario_list_order_by_id")]
	private static bool ScenarioListOrderByID_kvar;

	[Header("Components")]
	[SerializeField]
	private UIScenarioInfoSummary SelectedScenarioSummary;

	[SerializeField]
	private UIScenarioInfoSummary TemplateAvailableScenarioSummary;

	[SerializeField]
	private Transform ContainerAvailableScenarios;

	[SerializeField]
	private FHToggle ToggleAvailableScenarios;

	[SerializeField]
	private GameObject ArrowAvailableScenarios;

	[SerializeField]
	private Transform ContainerCompletedScenarios;

	[SerializeField]
	private FHToggle ToggleCompletedScenarios;

	[SerializeField]
	private GameObject ArrowCompletedScenarios;

	[SerializeField]
	private UIFilterGroup CategoryFilter;

	[SerializeField]
	private TextLocalizedListener LabelCategoryFilter;

	[Header("Settings")]
	[SerializeField]
	private Sprite FilterIconAllScenariosOn;

	[SerializeField]
	private Sprite FilterIconAllScenariosOff;

	[SerializeField]
	private Sprite FilterIconBlueprintsOn;

	[SerializeField]
	private Sprite FilterIconBlueprintsOff;

	[SerializeField]
	private Sprite FilterIconTreasureChestsOn;

	[SerializeField]
	private Sprite FilterIconTreasureChestsOff;

	private Quaternion arrowRotationOpen = Quaternion.Euler(new Vector3(0f, 0f, 0f));

	private Quaternion arrowRotationCollapsed = Quaternion.Euler(new Vector3(0f, 0f, 90f));

	private Params Data;

	private List<UIScenarioInfoSummary> _availableSummaries = new List<UIScenarioInfoSummary>();

	private List<UIScenarioInfoSummary> _completedSummaries = new List<UIScenarioInfoSummary>();

	private bool _initialized;

	protected override bool PlaySoundOnEnable => false;

	private ScenarioFilter ActiveFilter => (ScenarioFilter)(object)CategoryFilter.ActiveFilter;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_availableSummaries.Clear();
			_availableSummaries.AddRange(ContainerAvailableScenarios.GetComponentsInChildren<UIScenarioInfoSummary>());
			_completedSummaries.Clear();
			_completedSummaries.AddRange(ContainerCompletedScenarios.GetComponentsInChildren<UIScenarioInfoSummary>());
			UIFilterGroup categoryFilter = CategoryFilter;
			categoryFilter.OnFilter = (Action<Enum>)Delegate.Combine(categoryFilter.OnFilter, new Action<Enum>(SetFilter));
			CategoryFilter.SetFilters(GetFilters());
			CategoryFilter.UpdateState(ScenarioFilter.All);
			ToggleAvailableScenarios.onValueChanged.AddListener(HandleToggleAvailableScenarios);
			ToggleCompletedScenarios.onValueChanged.AddListener(HandleToggleCompletedScenarios);
		}
	}

	public void Init(Params @params)
	{
		Data = @params;
		InitCache();
		Rebuild();
	}

	private void Rebuild()
	{
		if (Data.selectedScenario != null)
		{
			SelectedScenarioSummary.gameObject.SetActive(value: true);
			SelectedScenarioSummary.Set(Data.selectedScenario, () =>
			{
				Data.onScenarioClicked(Data.selectedScenario);
			});
		}
		else
		{
			SelectedScenarioSummary.gameObject.SetActive(value: false);
			SelectedScenarioSummary.Clear();
		}
		if (ScenarioListOrderByID_kvar)
		{
			Data.availableScenarios = Data.availableScenarios.OrderBy((ScenarioInfoDTO s) => s.Id).ToList();
		}
		List<ScenarioInfoDTO> list = Data.availableScenarios.Where((ScenarioInfoDTO dto) => !dto.IsCompleted && MatchFilter(dto)).ToList();
		List<ScenarioInfoDTO> list2 = Data.availableScenarios.Where((ScenarioInfoDTO dto) => dto.IsCompleted && MatchFilter(dto)).ToList();
		PopulateContainer(list, _availableSummaries, TemplateAvailableScenarioSummary.gameObject, ContainerAvailableScenarios, Data.onScenarioClicked);
		PopulateContainer(list2, _completedSummaries, TemplateAvailableScenarioSummary.gameObject, ContainerCompletedScenarios, Data.onScenarioClicked);
		ContainerAvailableScenarios.gameObject.SetActive(list.Count > 0);
		ToggleAvailableScenarios.gameObject.SetActive(list.Count > 0);
		ContainerCompletedScenarios.gameObject.SetActive(list2.Count > 0);
		ToggleCompletedScenarios.gameObject.SetActive(list2.Count > 0);
		LabelCategoryFilter.SetTextKey(GetFilterLocKey(ActiveFilter));
		UpdateCategoriesVisability();
	}

	private void PopulateContainer(List<ScenarioInfoDTO> scenarios, List<UIScenarioInfoSummary> summaries, GameObject prototype, Transform container, Action<ScenarioInfoDTO> onScenarioClicked)
	{
		HelperTools.NormalizePool(ref summaries, prototype, container, scenarios.Count);
		for (int i = 0; i < scenarios.Count; i++)
		{
			ScenarioInfoDTO availableScenario = scenarios[i];
			summaries[i].Set(availableScenario, () =>
			{
				onScenarioClicked(availableScenario);
			});
		}
	}

	private void UpdateCategoriesVisability()
	{
		ContainerAvailableScenarios.gameObject.SetActive(ToggleAvailableScenarios.isOn);
		ContainerCompletedScenarios.gameObject.SetActive(ToggleCompletedScenarios.isOn);
		ArrowAvailableScenarios.transform.rotation = (ToggleAvailableScenarios.isOn ? arrowRotationOpen : arrowRotationCollapsed);
		ArrowCompletedScenarios.transform.rotation = (ToggleCompletedScenarios.isOn ? arrowRotationOpen : arrowRotationCollapsed);
	}

	private void SetFilter(Enum newFilter)
	{
		if (!object.Equals(CategoryFilter.ActiveFilter, newFilter))
		{
			CategoryFilter.UpdateState(newFilter);
			Rebuild();
		}
	}

	private bool MatchFilter(ScenarioInfoDTO scenarioInfoDto)
	{
		return ActiveFilter switch
		{
			ScenarioFilter.All => true, 
			ScenarioFilter.RandomItems => scenarioInfoDto.HasAvailableRandomItems, 
			ScenarioFilter.TreasureChests => scenarioInfoDto.HasAvailableChests, 
			_ => throw new Exception("Unhandled filter type " + ActiveFilter), 
		};
	}

	private void HandleToggleAvailableScenarios(bool on)
	{
		ToggleAvailableScenarios.OnDeselect(null);
		UpdateCategoriesVisability();
	}

	private void HandleToggleCompletedScenarios(bool on)
	{
		ToggleCompletedScenarios.OnDeselect(null);
		UpdateCategoriesVisability();
	}

	private string GetFilterLocKey(ScenarioFilter fillter)
	{
		return fillter switch
		{
			ScenarioFilter.All => "GUI_INVENTORY_SLOT_ALL", 
			ScenarioFilter.TreasureChests => "TREASURE_CHEST_TOOLTIP", 
			ScenarioFilter.RandomItems => "RANDOM_ITEM_TOOLTIP", 
			_ => "", 
		};
	}

	private UIFilterGroup.FilterParam[] GetFilters()
	{
		return new UIFilterGroup.FilterParam[3]
		{
			new UIFilterGroup.FilterParam
			{
				TextKey = "GUI_INVENTORY_SLOT_ALL",
				TooltipKey = "GUI_INVENTORY_SLOT_ALL",
				Icon = FilterIconAllScenariosOn,
				IconInactive = FilterIconAllScenariosOff,
				Filter = ScenarioFilter.All
			},
			new UIFilterGroup.FilterParam
			{
				TextKey = "RANDOM_ITEM_TOOLTIP",
				TooltipKey = "RANDOM_ITEM_TOOLTIP",
				Icon = FilterIconBlueprintsOn,
				IconInactive = FilterIconBlueprintsOff,
				Filter = ScenarioFilter.RandomItems
			},
			new UIFilterGroup.FilterParam
			{
				TextKey = "TREASURE_CHEST_TOOLTIP",
				TooltipKey = "TREASURE_CHEST_TOOLTIP",
				Icon = FilterIconTreasureChestsOn,
				IconInactive = FilterIconTreasureChestsOff,
				Filter = ScenarioFilter.TreasureChests
			}
		};
	}
}
