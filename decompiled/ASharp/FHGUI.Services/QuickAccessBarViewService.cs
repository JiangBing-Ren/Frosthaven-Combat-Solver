using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using FHRL.Buildings;

namespace FHGUI.Services;

public class QuickAccessBarViewService : FHMapViewService
{
	private class OutpostNewAlert
	{
		public bool HasNewItemsForCrafting;

		public bool HasNewPotionsForBrewing;

		public bool HasNewItemsInStore;

		public bool HasNewTownGuardPerk;

		public bool HasNewCards;

		public bool HasNewQuest;
	}

	private BuildingViewService _buildingService;

	private ItemViewService _merchantService;

	private CharacterDetailsViewService _characterService;

	private OutpostQuickAccessBarModule _quickAccessBar;

	private OutpostNewAlert _newAlerts = new OutpostNewAlert();

	public MainViewSubCategories CurrentTab { get; private set; }

	public MainViewMainCategories Context => QuickAccessBarUtils.GetContext(CurrentTab);

	public QuickAccessBarViewService(FHMapView view, BuildingViewService buildingService, CharacterDetailsViewService characterService)
		: base(view)
	{
		_buildingService = buildingService;
		_characterService = characterService;
		_merchantService = view.GetService<ItemViewService>();
		_quickAccessBar = _view.AllModules.OfType<OutpostQuickAccessBarModule>().First();
		_view.BuildingTooltipActionEventChannel.OnSelectBuilding.AddListener(OnSelectBuilding);
		_view.BuildingTooltipActionEventChannel.OnDeselectBuilding.AddListener(OnDeselectBuilding);
		FHMapView view2 = _view;
		view2.OnStateEnter = (Action<FHMapViewState>)Delegate.Combine(view2.OnStateEnter, new Action<FHMapViewState>(OnStateEnter));
		CurrentTab = MainViewSubCategories.None;
	}

	private void OnStateEnter(FHMapViewState state)
	{
		if (state is CharacterPerksViewState)
		{
			return;
		}
		if (!(state is CraftingViewState))
		{
			if (state is EnhancerViewState || state is HandManagementViewState)
			{
				return;
			}
			if (!(state is ShopViewState))
			{
				if (state is UnlockCardsViewState)
				{
					_newAlerts.HasNewCards = false;
				}
			}
			else
			{
				_newAlerts.HasNewItemsInStore = false;
			}
		}
		else
		{
			_newAlerts.HasNewItemsForCrafting = false;
		}
	}

	public void ProcessCampaignEvents(IEnumerable<CampaignEvent> events)
	{
		if (events != null)
		{
			_newAlerts.HasNewItemsForCrafting = events.Any((CampaignEvent ev) => ev is CampaignEvent.NewItemAvailabeForCrafting);
			_newAlerts.HasNewPotionsForBrewing = events.Any((CampaignEvent ev) => ev is CampaignEvent.NewBrewableRecipieDiscovered);
			_newAlerts.HasNewItemsInStore = events.Any((CampaignEvent ev) => ev is CampaignEvent.NewItemAvailabeForPurchase);
			_newAlerts.HasNewTownGuardPerk = events.Any((CampaignEvent ev) => ev is CampaignEvent.TownGuardPerkNewPerk);
			_newAlerts.HasNewCards = events.Any((CampaignEvent ev) => ev is CampaignEvent.CharacterCardUnlocksChanged);
			_newAlerts.HasNewQuest = events.Any((CampaignEvent ev) => ev is CampaignEvent.ScenarioUnlocked);
		}
	}

	private void OnDeselectBuilding()
	{
		if (_quickAccessBar.BuildMenu.isActiveAndEnabled && _quickAccessBar.BuildMenu.Selected != null)
		{
			_quickAccessBar.BuildMenu.SetSelectedState(_quickAccessBar.BuildMenu.Selected.ID, selected: false);
		}
	}

	private void OnSelectBuilding(BuildingTypeID id)
	{
		if (_quickAccessBar.BuildMenu.isActiveAndEnabled)
		{
			UIConstructBuildingListElement selected = _quickAccessBar.BuildMenu.Selected;
			if ((object)selected == null || selected.ID != id)
			{
				_quickAccessBar.BuildMenu.SetSelectedState(id, selected: true);
			}
		}
	}

	public void Setup(MainViewMainCategories context = MainViewMainCategories.None, MainViewSubCategories subcategory = MainViewSubCategories.None, UIBuildingStateFilter buildingFilter = UIBuildingStateFilter.None, Action<UIBuildingStateFilter> onBuildingFilterClicked = null, bool isInteractable = true, MainViewSubCategories[] allowedInteractions = null, ConfirmActionDTO onConfirmActionClick = null)
	{
		CurrentTab = subcategory;
		QuickAccessActionsDTO actionsDto = new QuickAccessActionsDTO
		{
			OnBuildingSelected = (BuildingTypeID id, bool selected) =>
			{
				if (onConfirmActionClick != null)
				{
					onConfirmActionClick.Execute(() =>
					{
						OnBuildingSelect(id, selected);
					}, null);
				}
				else
				{
					OnBuildingSelect(id, selected);
				}
			},
			OnBuildButtonClicked = () =>
			{
				if (onConfirmActionClick != null)
				{
					onConfirmActionClick.Execute(ToggleBuildMenu, null);
				}
				else
				{
					ToggleBuildMenu();
				}
			},
			OnOutpostButtonClicked = ((onConfirmActionClick != null) ? ((Action)(() =>
			{
				onConfirmActionClick.Execute(GoToOutpost, null);
			})) : new Action(GoToOutpost)),
			OnWorldMapButtonClicked = ((onConfirmActionClick != null) ? ((Action)(() =>
			{
				onConfirmActionClick.Execute(GoToWorldMap, null);
			})) : new Action(GoToWorldMap)),
			OnBuildingFilterClicked = ((onConfirmActionClick != null) ? ((Action<UIBuildingStateFilter>)((UIBuildingStateFilter f) =>
			{
				onConfirmActionClick.Execute(() =>
				{
					onBuildingFilterClicked?.Invoke(f);
				}, null);
			})) : onBuildingFilterClicked)
		};
		_quickAccessBar.Init(GetQuickAccessBarData(context, subcategory, buildingFilter, onConfirmActionClick), _buildingService.GetAllBuildings(), actionsDto, !isInteractable, allowedInteractions, ToggleBuildMenu);
		OutpostConstructBuildingModule buildMenu = _quickAccessBar.BuildMenu;
		buildMenu.OnFilterClicked = (Action<UIBuildingStateFilter>)Delegate.Remove(buildMenu.OnFilterClicked, new Action<UIBuildingStateFilter>(OnFilterClicked));
		OutpostConstructBuildingModule buildMenu2 = _quickAccessBar.BuildMenu;
		buildMenu2.OnFilterClicked = (Action<UIBuildingStateFilter>)Delegate.Combine(buildMenu2.OnFilterClicked, new Action<UIBuildingStateFilter>(OnFilterClicked));
	}

	public void OnFilterClicked(UIBuildingStateFilter obj)
	{
		_view.BuildingTooltipActionEventChannel.DeselectBuilding();
	}

	private void ToggleBuildMenu()
	{
		if (!_quickAccessBar.BuildMenu.isActiveAndEnabled && _view.BuildingTooltipActionEventChannel.SelectedBuilding.HasValue)
		{
			_view.BuildingTooltipActionEventChannel.DeselectBuilding();
		}
	}

	private void OnBuildingSelect(BuildingTypeID id, bool selected)
	{
		if (selected)
		{
			_view.BuildingTooltipActionEventChannel.SelectBuilding(id);
		}
		else
		{
			_view.BuildingTooltipActionEventChannel.DeselectBuilding();
		}
	}

	private void GoToOutpost()
	{
		_view.ReplaceState(new OutpostVillageMapState());
	}

	private void GoToWorldMap()
	{
		_view.ReplaceState(new ScenarioSelectionViewState());
	}

	private QuickAccessBarDTO GetQuickAccessBarData(MainViewMainCategories context, MainViewSubCategories subcategory = MainViewSubCategories.None, UIBuildingStateFilter buildingFilter = UIBuildingStateFilter.None, ConfirmActionDTO onConfirmAction = null)
	{
		return new QuickAccessBarDTO
		{
			Context = context,
			SelectedCategory = ValidateSubcategory(subcategory, context),
			AvailableTabs = GetAvailableButtons(context, onConfirmAction),
			BuildingFilter = buildingFilter,
			HasBoat = _view.GameState.CanPlayBoatScenarios,
			HasSled = _view.GameState.CanPlaySledScenarios,
			HasClimbingGear = _view.GameState.CanPlayClimbingGearScenarios,
			HasActiveScenario = (_view.SelectedScenarioDef != null && _view.IsReadyCheckRunning)
		};
	}

	private IEnumerable<(MainViewSubCategories, QuickAccessTabDTO)> GetAvailableButtons(MainViewMainCategories context, ConfirmActionDTO onNonInteractableClick = null)
	{
		switch (context)
		{
		case MainViewMainCategories.Outpost:
			if (_buildingService.HasBuilding<Craftsman_BuildingTypeDef>() || _buildingService.HasBuilding<CraftingTable_BuildingTypeDef>())
			{
				yield return (MainViewSubCategories.Craftsman, InsertAction(GetTabData(MainViewSubCategories.Craftsman), onNonInteractableClick));
			}
			if (_merchantService.MerchantEnabled)
			{
				yield return (MainViewSubCategories.Merchant, InsertAction(GetTabData(MainViewSubCategories.Merchant), onNonInteractableClick));
			}
			if (_merchantService.IsTravellingMerchantEnabled)
			{
				yield return (MainViewSubCategories.TravellingMerchant, InsertAction(GetTabData(MainViewSubCategories.TravellingMerchant), onNonInteractableClick));
			}
			if (_buildingService.HasBuilding<Alchemist_BuildingTypeDef>(includeInConstruction: false))
			{
				yield return (MainViewSubCategories.Alchemy, InsertAction(GetTabData(MainViewSubCategories.Alchemy), onNonInteractableClick));
			}
			if (_buildingService.HasBuilding<Enhancer_BuildingTypeDef>(includeInConstruction: false))
			{
				yield return (MainViewSubCategories.Enhancer, InsertAction(GetTabData(MainViewSubCategories.Enhancer), onNonInteractableClick));
			}
			if (_buildingService.HasBuilding<TownHall_BuildingTypeDef>(includeInConstruction: false))
			{
				yield return (MainViewSubCategories.Defense, InsertAction(GetTabData(MainViewSubCategories.Defense), onNonInteractableClick));
			}
			break;
		case MainViewMainCategories.Characters:
			yield return (MainViewSubCategories.CharacterManagement, InsertAction(GetTabData(MainViewSubCategories.CharacterManagement), onNonInteractableClick));
			yield return (MainViewSubCategories.HandManagement, InsertAction(GetTabData(MainViewSubCategories.HandManagement), onNonInteractableClick));
			yield return (MainViewSubCategories.NewCards, InsertAction(GetTabData(MainViewSubCategories.NewCards), onNonInteractableClick));
			yield return (MainViewSubCategories.Perks, InsertAction(GetTabData(MainViewSubCategories.Perks), onNonInteractableClick));
			break;
		case MainViewMainCategories.WorldMap:
			yield return (MainViewSubCategories.WorldQuests, InsertAction(GetTabData(MainViewSubCategories.WorldQuests), onNonInteractableClick));
			if (_view.GetService<PuzzleBookViewService>().IsPuzzleBookUnlocked)
			{
				yield return (MainViewSubCategories.PuzzleBook, InsertAction(GetTabData(MainViewSubCategories.PuzzleBook), onNonInteractableClick));
			}
			if (_buildingService.HasBuilding<TownHall_BuildingTypeDef>(includeInConstruction: false))
			{
				yield return (MainViewSubCategories.JobPostings, InsertAction(GetTabData(MainViewSubCategories.JobPostings), onNonInteractableClick));
			}
			break;
		case MainViewMainCategories.Roster:
			yield return (MainViewSubCategories.ActiveCharacters, InsertAction(GetTabData(MainViewSubCategories.ActiveCharacters), onNonInteractableClick));
			yield return (MainViewSubCategories.RetiredCharacters, InsertAction(GetTabData(MainViewSubCategories.RetiredCharacters), onNonInteractableClick));
			break;
		default:
			_ = context;
			break;
		}
		static QuickAccessTabDTO InsertAction(QuickAccessTabDTO dto, ConfirmActionDTO validateAction)
		{
			if (validateAction != null)
			{
				Action action = dto.OnClick;
				dto.OnClick = () =>
				{
					validateAction.Execute(action, null);
				};
			}
			return dto;
		}
	}

	private void GoToMainCategory(MainViewSubCategories subCategory)
	{
		MainViewMainCategories context = QuickAccessBarUtils.GetContext(subCategory);
		MainCategoryChangeRequestHandler(context);
	}

	private QuickAccessTabDTO GetTabData(MainViewSubCategories subCategory)
	{
		return subCategory switch
		{
			MainViewSubCategories.None => default, 
			MainViewSubCategories.Craftsman => (_buildingService.HasBuilding<Craftsman_BuildingTypeDef>() || _buildingService.HasBuilding<CraftingTable_BuildingTypeDef>()) ? new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Craftsman);
			}, _newAlerts.HasNewItemsForCrafting, IsSubCategoryInteractable(subCategory)) : default(QuickAccessTabDTO), 
			MainViewSubCategories.Merchant => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Merchant);
			}, _newAlerts.HasNewItemsInStore, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.TravellingMerchant => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.TravellingMerchant);
			}, _newAlerts.HasNewItemsInStore, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.Alchemy => _buildingService.HasBuilding<Alchemist_BuildingTypeDef>() ? new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Alchemy);
			}, _newAlerts.HasNewPotionsForBrewing, IsSubCategoryInteractable(subCategory)) : default(QuickAccessTabDTO), 
			MainViewSubCategories.Enhancer => _buildingService.HasBuilding<Enhancer_BuildingTypeDef>() ? new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Enhancer);
			}, hasNew: false, IsSubCategoryInteractable(subCategory)) : default(QuickAccessTabDTO), 
			MainViewSubCategories.Defense => _buildingService.HasBuilding<TownHall_BuildingTypeDef>() ? new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Defense);
			}, _view.GameState.Outpost.TownGuardPerksPoints != 0, IsSubCategoryInteractable(subCategory)) : default(QuickAccessTabDTO), 
			MainViewSubCategories.CharacterManagement => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.CharacterManagement);
			}, hasNew: false, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.HandManagement => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.HandManagement);
			}, _view.SelectedCharacter != null && _view.SelectedCharacter.SelectedCardIDs.Count != _view.SelectedCharacter.ClassDef.MaxCards, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.NewCards => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.NewCards);
			}, _view.SelectedCharacter != null && _view.SelectedCharacter.CardUnlocks != 0, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.Perks => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.Perks);
			}, _view.SelectedCharacter != null && _view.SelectedCharacter.PerkMarks != 0 && _view.GetService<PerksViewService>().HasUnlockablePerks(_view.SelectedCharacter), IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.WorldQuests => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.WorldQuests);
			}, hasNew: false, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.PuzzleBook => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.PuzzleBook);
			}, _view.GameState.UnlockedPuzzles.Any((PuzzleState puzzle) => !puzzle.IsCompleted), IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.ActiveCharacters => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.ActiveCharacters);
			}, hasNew: false, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.RetiredCharacters => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.RetiredCharacters);
			}, hasNew: false, IsSubCategoryInteractable(subCategory)), 
			MainViewSubCategories.JobPostings => new QuickAccessTabDTO(() =>
			{
				SubCategoryChangeRequestHandler(MainViewSubCategories.JobPostings);
			}, _view.GameState.Outpost.JobPostings.Any((CampaignSectionID job) => !_view.GameController.OutpostController.IsJobPostingCompleted(job) && !_view.GameController.OutpostController.IsJobPostingAccepted(job)), IsSubCategoryInteractable(subCategory)), 
			_ => throw new ArgumentOutOfRangeException("subCategory", subCategory, null), 
		};
	}

	private MainViewSubCategories ValidateSubcategory(MainViewSubCategories subcategory, MainViewMainCategories context)
	{
		if (subcategory == MainViewSubCategories.None || QuickAccessBarUtils.GetContext(subcategory) != context)
		{
			return QuickAccessBarUtils.GetDefaultSubcategory(context);
		}
		return subcategory;
	}

	private bool IsSubCategoryInteractable(MainViewSubCategories subCategory)
	{
		return subCategory switch
		{
			MainViewSubCategories.Craftsman => _view.SelectedCharacter != null && _view.GetService<CraftingViewService>().IsCraftingEnabled, 
			MainViewSubCategories.None => true, 
			MainViewSubCategories.Merchant => _view.SelectedCharacter != null && _view.GetService<ItemViewService>().MerchantEnabled, 
			MainViewSubCategories.TravellingMerchant => _view.SelectedCharacter != null && _view.GetService<ItemViewService>().IsTravellingMerchantEnabled, 
			MainViewSubCategories.Alchemy => _view.SelectedCharacter != null && _view.GetService<AlchemistViewService>().IsBrewingEnabled, 
			MainViewSubCategories.Enhancer => _view.SelectedCharacter != null && _view.GetService<EnhancerViewService>().IsEnhancerEnabled, 
			MainViewSubCategories.Defense => true, 
			MainViewSubCategories.CharacterManagement => true, 
			MainViewSubCategories.HandManagement => _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter), 
			MainViewSubCategories.NewCards => _view.GameController.CharacterController.CanLevelUp, 
			MainViewSubCategories.Perks => _view.SelectedCharacter != null && _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter), 
			MainViewSubCategories.WorldQuests => true, 
			MainViewSubCategories.PuzzleBook => _view.GetService<PuzzleBookViewService>().HasPuzzles, 
			MainViewSubCategories.ActiveCharacters => true, 
			MainViewSubCategories.RetiredCharacters => _view.GetService<RosterViewService>().HasRetiredCharacters, 
			MainViewSubCategories.JobPostings => _view.Choreographer.GameState.Outpost.JobPostings.Any(), 
			_ => throw new ArgumentOutOfRangeException("subCategory", subCategory, null), 
		};
	}

	public void MainCategoryChangeRequestHandler(MainViewMainCategories category, bool force = false)
	{
		if (force || Context != category)
		{
			CurrentTab = QuickAccessBarUtils.GetDefaultSubcategory(category);
			FHMapViewState state = category switch
			{
				MainViewMainCategories.None => (FHMapViewState)new OutpostVillageMapState(), 
				MainViewMainCategories.Outpost => new OutpostVillageMapState(), 
				MainViewMainCategories.Characters => new EditCharacterViewState(), 
				MainViewMainCategories.WorldMap => new ScenarioSelectionViewState(), 
				MainViewMainCategories.Roster => new ManageRosterViewState(), 
				_ => throw new ArgumentOutOfRangeException("category", category, $"Invalid category {category} passed to QuickAccessBar"), 
			};
			_view.ReplaceState(state);
		}
	}

	public void CloseCurrentTab()
	{
		MainViewMainCategories context = Context;
		if (context == MainViewMainCategories.None || CurrentTab == MainViewSubCategories.None)
		{
			return;
		}
		if (_view.StateStackLength > 1)
		{
			_view.PopState(FHMapViewLayer.LayerType.Base);
			return;
		}
		switch (context)
		{
		case MainViewMainCategories.Outpost:
		case MainViewMainCategories.Characters:
		case MainViewMainCategories.Roster:
			MainCategoryChangeRequestHandler(MainViewMainCategories.Outpost, force: true);
			break;
		case MainViewMainCategories.WorldMap:
			MainCategoryChangeRequestHandler(MainViewMainCategories.WorldMap, force: true);
			break;
		}
	}

	public void SubCategoryChangeRequestHandler(MainViewSubCategories category, bool force = false)
	{
		if (CurrentTab == category && !force)
		{
			CloseCurrentTab();
			return;
		}
		CurrentTab = category;
		CharacterDataDTO characterDataDto = ((_view.SelectedCharacter != null) ? _characterService.PopulateCharacterDTO(_view.SelectedCharacter) : null);
		bool readOnly = !_view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter);
		FHMapViewState state = category switch
		{
			MainViewSubCategories.Alchemy => (FHMapViewState)new AlchemistViewState(), 
			MainViewSubCategories.Merchant => new ShopViewState(), 
			MainViewSubCategories.TravellingMerchant => new TravellingMerchantViewState(), 
			MainViewSubCategories.Craftsman => new CraftingViewState(), 
			MainViewSubCategories.Enhancer => new EnhancerViewState(), 
			MainViewSubCategories.Defense => new TownPerksViewState(), 
			MainViewSubCategories.CharacterManagement => new EditCharacterViewState(), 
			MainViewSubCategories.HandManagement => new HandManagementViewState(characterDataDto, readOnly), 
			MainViewSubCategories.NewCards => new UnlockCardsViewState(characterDataDto, readOnly), 
			MainViewSubCategories.Perks => new CharacterPerksViewState(), 
			MainViewSubCategories.WorldQuests => new ScenarioSelectionViewState(), 
			MainViewSubCategories.PuzzleBook => new PuzzleBookViewState(), 
			MainViewSubCategories.JobPostings => new JobPostingsViewState(), 
			MainViewSubCategories.ActiveCharacters => new ManageRosterViewState(), 
			MainViewSubCategories.RetiredCharacters => new ManageRosterViewState(showRetired: true), 
			_ => new MainViewState(category), 
		};
		_view.ReplaceState(state);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		_view.BuildingTooltipActionEventChannel.OnSelectBuilding.RemoveListener(OnSelectBuilding);
		_view.BuildingTooltipActionEventChannel.OnDeselectBuilding.RemoveListener(OnDeselectBuilding);
	}
}
