using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Elements.Buildings;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using FHRL.Buildings;
using GLOOM;

namespace FHGUI.Services;

public class BuildingViewService : FHMapViewService
{
	private GardenController _gardenController => _view.Choreographer.GameController.GardenController;

	private GardenState _gardenState => _view.Choreographer.GameState.Outpost.Garden;

	private PetController _petController => _view.Choreographer.GameController.PetController;

	public bool TavernFestivalsEnabled => CampaignController.OutpostController.TavernFestivalsEnabled;

	public Tavern_BuildingTypeDef.FestivalSize CurrentFestivalSize => CampaignController.State.Outpost.CurrentTavernFestivalLevel;

	public Tavern_BuildingTypeDef.FestivalSize MaxFestivalSize
	{
		get
		{
			BuildingState buildingStateByID = GetBuildingStateByID(BuildingId.Tavern);
			if (buildingStateByID == null)
			{
				return Tavern_BuildingTypeDef.FestivalSize.None;
			}
			return (buildingStateByID.Def as Tavern_BuildingTypeDef).GetMaxFestivalSizePerLevel(buildingStateByID.Level);
		}
	}

	private BuildingsController BuildingsController => GameController.BuildingsController;

	private CharacterController CharacterController => GameController.CharacterController;

	private CampaignController CampaignController => GameController.OutpostController.CampaignController;

	private TradingController TradingController => GameController.TradingController;

	private OutpostState OutpostState => Choreographer.GameState.Outpost;

	private CampaignState GameState => Choreographer.GameState;

	private OutpostController OutpostController => Choreographer.GameController.OutpostController;

	public bool CanConstruct
	{
		get
		{
			if (BuildingsController.GetConstructionsRemaining() == 0)
			{
				return BuildingsController.GetExtraConstructionsRemaining() != 0;
			}
			return true;
		}
	}

	public bool CanConstructExtraBuilding => BuildingsController.GetExtraConstructionsRemaining() != 0;

	public uint TotalConstructionsRemaining => BuildingsController.GetConstructionsRemaining() + BuildingsController.GetExtraConstructionsRemaining();

	public bool HasBuildingsToConstruct => GetAvailableToConstruct().Any();

	public bool HasBuildingsToUpgrade => GetAvailableToUpgrade().Any();

	public bool HasWreckedBuildings => OutpostState.Buildings.Any((BuildingState b) => b.Wrecked);

	public bool HasConstructionProject => OutpostState.Buildings.Any((BuildingState b) => b.donationDistribution != null);

	public IEnumerable<OutpostHerbType> GetGardenPlots()
	{
		if (_gardenState == null)
		{
			yield break;
		}
		foreach (OutpostHerbType plot in _gardenState.Plots)
		{
			yield return plot;
		}
	}

	public IEnumerable<GardenStateParams.GardenPlotData> GetAllPossibleGardenPlots()
	{
		BuildingTypeDef def = GetBuildingDef(BuildingId.Garden);
		OutpostHerbType[] currentPlots = GetGardenPlots().ToArray();
		int totalPlots = 0;
		for (uint level = 1u; level <= def.Levels; level++)
		{
			IEnumerable<CampaignEffect> onConstructEffectsOfType = GetOnConstructEffectsOfType(def, CampaignEffectType.AddGardenPlots, level);
			int plots = onConstructEffectsOfType.Sum((CampaignEffect e) => e.GetInt(0));
			for (int i = 0; i < plots; i++)
			{
				int num = totalPlots + i;
				OutpostHerbType outpostHerbType = currentPlots.ElementAtOrDefault(num);
				bool flag = num < currentPlots.Length;
				yield return new GardenStateParams.GardenPlotData
				{
					Herb = (flag ? outpostHerbType : OutpostHerbType.None),
					Unlocked = flag,
					UnlockLevel = level
				};
			}
			totalPlots += plots;
		}
	}

	public GardenState.PlantStateType GetPlantState()
	{
		return _gardenState?.PlantState ?? GardenState.PlantStateType.None;
	}

	public bool PlantHerbs(OutpostHerbType[] herbs)
	{
		if (!_gardenController.CanUseGarden || !_gardenController.IsInPlantMode)
		{
			return false;
		}
		if (!_view.IsPartyLeader)
		{
			return false;
		}
		Choreographer.ExecuteCommand(FHRPC.Campaign.Garden.PlantHerbs, herbs);
		return true;
	}

	private IEnumerable<PetDef> GetAllPetsState()
	{
		return _petController.GetOwnedPets();
	}

	public PetDTO GetPet(PetDef state)
	{
		return new PetDTO(state);
	}

	public IEnumerable<PetDTO> GetAllPets()
	{
		return GetAllPetsState().Select(GetPet);
	}

	public bool CanTrainSoldiers()
	{
		return OutpostController.CanTrainSoldiers(1u, _view.SelectedCharacter);
	}

	public int GetMaxTrainSoldiers()
	{
		return (int)OutpostController.MaxTrainSoldiersCount;
	}

	public int GetTrainSoldiersLeft()
	{
		return (int)OutpostController.CanTrainSoldiersCount;
	}

	public bool TrainSoldier(OutpostMaterialType material)
	{
		if (material == OutpostMaterialType.None)
		{
			return false;
		}
		CharacterState selectedCharacter = _view.SelectedCharacter;
		OutpostResources outpostResources = new OutpostResources(material, (int)OutpostController.TrainSoldierMaterialCost);
		if (!selectedCharacter.Resources.Has(outpostResources))
		{
			return false;
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.TrainSoldiers, selectedCharacter.ID, 1, outpostResources);
		return true;
	}

	public void PickTrainSoldierMaterialType(OutpostMaterialType material)
	{
		if (!_view.TryGetPersistantModulePayload((IUIViewModule)_view.Modules.OutpostMap.BuildingInfoModule, out BuildingInfoModule.PersistentData value))
		{
			_view.SetPersistantModulePayload(_view.Modules.OutpostMap.BuildingInfoModule, new BuildingInfoModule.PersistentData
			{
				PickedSoldierTrainResource = material,
				PickedInnTradeResource = OutpostMaterialType.None
			});
		}
		if (value != null)
		{
			value.PickedSoldierTrainResource = material;
		}
		_view.RefreshView();
	}

	public OutpostMaterialType GetTrainSoldierMaterialType(CharacterState character)
	{
		if (character == null)
		{
			return OutpostMaterialType.None;
		}
		_view.TryGetPersistantModulePayload(_view.Modules.OutpostMap.BuildingInfoModule, out var value);
		if (!(value is BuildingInfoModule.PersistentData persistentData))
		{
			return OutpostMaterialType.None;
		}
		if (character.Resources.HasMaterial(persistentData.PickedSoldierTrainResource, (int)OutpostController.TrainSoldierMaterialCost))
		{
			return persistentData.PickedSoldierTrainResource;
		}
		return OutpostMaterialType.None;
	}

	public IReadOnlyList<JobPostingDTO> GetAllJobs()
	{
		return OutpostState.JobPostings.Select(GetJobPosting).ToList().AsReadOnly();
	}

	public JobPostingDTO GetJobPosting(CampaignSectionID campaignSectionID)
	{
		ScenarioDef jobPostingScenarioDef = OutpostController.GetJobPostingScenarioDef(campaignSectionID);
		string text = "Event_" + campaignSectionID.ID + "_Text";
		string description = _view.GetService<OutpostEventsService>().LocalizeMultiKeySectionDescriptions(text);
		return new JobPostingDTO
		{
			ID = campaignSectionID,
			Name = LocalizationManager.GetTranslation($"FS_{jobPostingScenarioDef.ID}_NAME"),
			Key = text,
			Description = description,
			State = GetJobState(campaignSectionID)
		};
	}

	public JobPostingDTO.JobState GetJobState(CampaignSectionID campaignSectionID)
	{
		if (OutpostController.IsJobPostingCompleted(campaignSectionID))
		{
			return JobPostingDTO.JobState.Completed;
		}
		if (OutpostController.IsJobPostingAccepted(campaignSectionID))
		{
			return JobPostingDTO.JobState.Accepted;
		}
		return JobPostingDTO.JobState.None;
	}

	public bool CanAcceptJobPosting(CampaignSectionID campaignSectionID)
	{
		if (OutpostState.JobPostings.Contains(campaignSectionID))
		{
			return OutpostController.IsValidJobPostingSectionToAccept(campaignSectionID);
		}
		return false;
	}

	public bool AcceptJobPosting(CampaignSectionID campaignSectionID)
	{
		if (!_view.IsPartyLeader)
		{
			return false;
		}
		if (!CanAcceptJobPosting(campaignSectionID))
		{
			return false;
		}
		try
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.AcceptJobPosting, campaignSectionID);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public bool CanHoldAnyFestival()
	{
		if (!TavernFestivalsEnabled)
		{
			return false;
		}
		if (CurrentFestivalSize != Tavern_BuildingTypeDef.FestivalSize.None)
		{
			return false;
		}
		return true;
	}

	public bool CanHoldFestival(Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		if (!TavernFestivalsEnabled)
		{
			return false;
		}
		if (festivalSize == Tavern_BuildingTypeDef.FestivalSize.None)
		{
			return false;
		}
		if (CurrentFestivalSize != Tavern_BuildingTypeDef.FestivalSize.None)
		{
			return false;
		}
		if (MaxFestivalSize < festivalSize)
		{
			return false;
		}
		return true;
	}

	public bool CanCharacterAffordFestival(CharacterState character, Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		int tavernFestivalGoldCost = GetTavernFestivalGoldCost(festivalSize);
		return character.Gold >= tavernFestivalGoldCost;
	}

	private void HoldFestival(Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		if (CanHoldFestival(festivalSize) && CanCharacterAffordFestival(_view.SelectedCharacter, festivalSize))
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.HoldFestival, _view.SelectedCharacter.ID, festivalSize);
		}
	}

	public Tavern_UIBuildInfoFunctionality.TavernFestivalData GetFestivalData(Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		Tavern_BuildingTypeDef.FestivalSize maxFestivalSize = MaxFestivalSize;
		return new Tavern_UIBuildInfoFunctionality.TavernFestivalData
		{
			Gold = GetTavernFestivalGoldCost(festivalSize),
			Morale = GetTavernFestivaMoraleReward(festivalSize),
			FestivalSize = festivalSize,
			CurrentState = ((maxFestivalSize < festivalSize) ? Tavern_UIBuildInfoFunctionality.TavernFestivalData.State.Locked : ((CurrentFestivalSize == festivalSize) ? Tavern_UIBuildInfoFunctionality.TavernFestivalData.State.Active : Tavern_UIBuildInfoFunctionality.TavernFestivalData.State.None)),
			CanAfford = CanCharacterAffordFestival(_view.SelectedCharacter, festivalSize),
			CanHoldFestival = CanHoldFestival(festivalSize)
		};
	}

	public int GetTavernFestivalGoldCost(Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		return (DefRepo.Buildings.Values.FirstOrDefault((BuildingTypeDef x) => x is Tavern_BuildingTypeDef) as Tavern_BuildingTypeDef).GetGoldCostPerFestival(festivalSize);
	}

	public int GetTavernFestivaMoraleReward(Tavern_BuildingTypeDef.FestivalSize festivalSize)
	{
		return (DefRepo.Buildings.Values.FirstOrDefault((BuildingTypeDef x) => x is Tavern_BuildingTypeDef) as Tavern_BuildingTypeDef).GetMoralePerFestival(festivalSize);
	}

	public OutpostMaterialType GetInnTradeMaterialType(CharacterState character)
	{
		if (character == null)
		{
			return OutpostMaterialType.None;
		}
		_view.TryGetPersistantModulePayload(_view.Modules.OutpostMap.BuildingInfoModule, out var value);
		if (!(value is BuildingInfoModule.PersistentData persistentData) || !_view.GetService<BuildingViewService>().CanBuyMaterialFromInn(character, persistentData.PickedInnTradeResource))
		{
			return OutpostMaterialType.None;
		}
		return persistentData.PickedInnTradeResource;
	}

	public void PickInnTradeMaterialType(OutpostMaterialType material)
	{
		if (!_view.TryGetPersistantModulePayload((IUIViewModule)_view.Modules.OutpostMap.BuildingInfoModule, out BuildingInfoModule.PersistentData value))
		{
			_view.SetPersistantModulePayload(_view.Modules.OutpostMap.BuildingInfoModule, new BuildingInfoModule.PersistentData
			{
				PickedInnTradeResource = material,
				PickedSoldierTrainResource = OutpostMaterialType.None
			});
		}
		if (value != null)
		{
			value.PickedInnTradeResource = material;
		}
		_view.RefreshView();
	}

	public UIJewellerRow.JewellerRowData? GetJewelerData()
	{
		BuildingState buildingStateByID = GetBuildingStateByID(BuildingId.Jeweler);
		if (buildingStateByID == null)
		{
			return GetJewelerDataForLevel(1u);
		}
		return GetJewelerDataForLevel((buildingStateByID.Level > 1 && buildingStateByID.UnderConstruction) ? (buildingStateByID.Level - 1) : buildingStateByID.Level, buildingStateByID);
	}

	public UIJewellerRow.JewellerRowData? GetJewelerDataForLevel(uint level, BuildingState jewelerState = null)
	{
		if (!(DefRepo.Buildings.Values.FirstOrDefault((BuildingTypeDef x) => x is Jeweler_BuildingTypeDef) is Jeweler_BuildingTypeDef jeweler_BuildingTypeDef))
		{
			return null;
		}
		CampaignEffect campaignEffect = jeweler_BuildingTypeDef.GetActiveEffects(level, wrecked: false, OutpostPhase.Downtime, GameController).FirstOrDefault();
		if (campaignEffect.Type == CampaignEffectType.None)
		{
			return null;
		}
		uint num = ((jewelerState != null && jewelerState.UnderConstruction) ? (jewelerState.Level - 1) : (jewelerState?.Level ?? 0));
		bool flag = level > num;
		return new UIJewellerRow.JewellerRowData
		{
			PercentageDiscount = campaignEffect.GetUInt(0),
			TooltipText = "",
			UnlockLevel = level,
			JewelerState = (flag ? UIJewellerRow.JewellerRowData.State.Locked : ((jewelerState == null || !jewelerState.WreckedOrRebuilding) ? UIJewellerRow.JewellerRowData.State.Active : UIJewellerRow.JewellerRowData.State.None))
		};
	}

	public IEnumerable<UIHallOfRevelryRow.HallOfRevelryData> GetHallOfRevelryData()
	{
		HallOfRevelry_BuildingTypeDef hall = DefRepo.Buildings.Values.FirstOrDefault((BuildingTypeDef x) => x is HallOfRevelry_BuildingTypeDef) as HallOfRevelry_BuildingTypeDef;
		uint currentLvl = GetBuildingStateByID(BuildingId.HallOfRevelry)?.Level ?? 0;
		for (uint level = 1u; level <= hall.Levels; level++)
		{
			bool locked = level > currentLvl;
			IEnumerable<ScenarioFavorType> favors = hall.GetFavors(level);
			foreach (ScenarioFavorType item in favors)
			{
				FavorConfigUI favorConfig = UIInfoTools.Instance.GetFavorConfig(item);
				string tooltipText = (locked ? string.Format(LocalizationManager.GetTranslation("GUI_REQUIRES_LEVEL"), level) : (LocalizationManager.GetTranslation(FHUILocKeys.GetFavorName(item)) + "\n" + LocalizationManager.GetTranslation(FHUILocKeys.GetFavorDescription(item))));
				yield return new UIHallOfRevelryRow.HallOfRevelryData
				{
					TooltipText = tooltipText,
					RequiredLevel = level,
					Icon = favorConfig.Icon
				};
			}
		}
	}

	public int GetLibraryDiscount(uint level)
	{
		Library_BuildingTypeDef library_BuildingTypeDef = (Library_BuildingTypeDef)GetBuildingDef(BuildingId.Library);
		int num = 0;
		if (level <= 1)
		{
			int num2 = GetActiveEffectOfType(library_BuildingTypeDef, CampaignEffectType.CanRespecCharacters).GetInt(0);
			num = GetActiveEffectOfType(library_BuildingTypeDef, CampaignEffectType.CanRespecCharacters, 2u).GetInt(0);
			return num2 - num;
		}
		if (level > library_BuildingTypeDef.Levels)
		{
			return 0;
		}
		int num3 = GetActiveEffectOfType(library_BuildingTypeDef, CampaignEffectType.CanRespecCharacters).GetInt(0);
		num = GetActiveEffectOfType(library_BuildingTypeDef, CampaignEffectType.CanRespecCharacters, level).GetInt(0);
		return num3 - num;
	}

	public Library_UIBuildingInfoFunctionality.LibraryStateParams GetLibraryStateParams()
	{
		Library_BuildingTypeDef def = (Library_BuildingTypeDef)GetBuildingDef(BuildingId.Library);
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		uint level = (flag ? ((buildingStateByID.ConstructionState == BuildingConstructionType.Upgrading) ? (buildingStateByID.Level - 1) : buildingStateByID.Level) : 0u);
		return new Library_UIBuildingInfoFunctionality.LibraryStateParams
		{
			Action = () =>
			{
				ShowBuildingScreen(def);
			},
			IsInteractable = flag,
			Discount = (flag ? GetLibraryDiscount(level) : GetLibraryDiscount(1u)),
			Locked = !flag
		};
	}

	public BuildingViewService(FHMapView view)
		: base(view)
	{
	}

	public IEnumerable<BuildingStateDTO> GetConstructedBuildings()
	{
		return OutpostState.Buildings.Where((BuildingState b) => b.ConstructionState != BuildingConstructionType.NewBuilding).Select(GetBuilding);
	}

	public IEnumerable<BuildingStateDTO> GetBuildings()
	{
		return OutpostState.Buildings.Select(GetBuilding);
	}

	public IEnumerable<BuildingTypeDef> GetAvailableToConstruct()
	{
		return CampaignController.OutpostController.ValidForBuildingDefs;
	}

	public IEnumerable<BuildingState> GetAvailableToUpgrade()
	{
		return OutpostState.Buildings.Where((BuildingState b) => BuildingsController.CanUpgrade(b));
	}

	public IEnumerable<BuildingState> GetWreckedBuildings()
	{
		return OutpostState.Buildings.Where((BuildingState b) => b.Wrecked);
	}

	public IEnumerable<BuildingStateDTO> GetBuildingsByState(BuildingConstructionType state)
	{
		return OutpostState.Buildings.Where((BuildingState b) => b.ConstructionState == state).Select(GetBuilding);
	}

	public IEnumerable<BuildingStateDTO> GetBuildingsInState(params BuildingConstructionType[] states)
	{
		return from b in OutpostState.Buildings
			where states.Contains(b.ConstructionState)
			select GetBuilding(b);
	}

	public IEnumerable<BuildingStateDTO> GetBuildingsNotInState(params BuildingConstructionType[] states)
	{
		return from b in OutpostState.Buildings
			where !states.Contains(b.ConstructionState)
			select GetBuilding(b);
	}

	public IEnumerable<BuildingStateDTO> GetAllBuildings()
	{
		IEnumerable<BuildingStateDTO> second = OutpostState.Buildings.Select(GetBuilding);
		return (from b in GetAvailableToConstruct()
			select GetNewBuildingByDef(b)).Concat(second).ToList();
	}

	public IEnumerable<BuildingStateDTO> GetAvailableBuildingsForConstruction()
	{
		DefRepository defRepo = Choreographer.DefRepo;
		IEnumerable<BuildingStateDTO> constructed = GetConstructedBuildings();
		return from b in defRepo.Buildings
			where !constructed.Any((BuildingStateDTO state) => state.Def.ID == b.Key) && b.Value.BuildProsperityRequirement <= OutpostState.ProsperityLevel
			select GetNewBuildingByDef(b.Value);
	}

	public IEnumerable<BuildingState> GetBuildingsInState(BuildingConstructionType state)
	{
		return OutpostState.Buildings.Where((BuildingState b) => b.ConstructionState == state);
	}

	public BuildingStateDTO GetBuilding(BuildingState state)
	{
		BuildingTypeDef def = state.Def;
		uint num = ((state.ConstructionState == BuildingConstructionType.Upgrading || state.ConstructionState == BuildingConstructionType.NewBuilding) ? (state.Level - 1) : state.Level);
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(state.Def);
		return new BuildingStateDTO
		{
			Def = def,
			Icon = buildingConfig.Icon,
			IconTintable = buildingConfig.TimetableIcon,
			Level = num,
			NameLocKey = buildingConfig.GetName(num),
			DescriptionLocKey = buildingConfig.GetEffect(num),
			ConstructionState = state.ConstructionState,
			IsDamaged = (state.DamagedState == BuildingDamagedState.Damaged),
			IsRepaired = (state.DamagedState == BuildingDamagedState.Repaired),
			CanBuild = false,
			CanUpgrade = (state.ConstructionState == BuildingConstructionType.None && BuildingsController.IsConstructionAllowed && BuildingsController.CanUpgrade(state)),
			CanRebuild = state.Wrecked,
			IsWrecked = state.Wrecked,
			IsConstructed = (state.ConstructionState != BuildingConstructionType.NewBuilding),
			ConstructionCost = GetConstructCost(state),
			UpgradeCost = GetUpgradeCost(state),
			RebuildCost = GetRebuildCost(state),
			IsDonationComplete = GetIsDonationComplete(state),
			CanFinalizeDonation = BuildingsController.CanFinalizeDonation(state),
			AvailableMorale = _view.GameState.Outpost.Morale,
			AvailableProsperity = _view.GameState.Outpost.ProsperityLevel,
			HasStartedProject = IsWaitingForDonation(state),
			IsExtraConstructionProject = IsExtraConstruction(state),
			FinalizeDonationTT = ((!CanConstruct) ? string.Format(LocalizationManager.GetTranslation("GUI_SECOND_BUILD_TOOLTIP"), BuildingsController.CampaignController.Settings.ConstructPerTurn + BuildingsController.CampaignController.Settings.ExtraConstructPerTurn) : string.Empty),
			ConstructEffects = ((num == 0) ? def.GetOnConstructEffects(1u, CampaignController).ToArray() : def.GetOnConstructEffects(num, CampaignController).ToArray()),
			ActiveEffects = def.GetActiveEffects(num, wrecked: false, OutpostPhase.Downtime, CampaignController)?.ToArray(),
			ApplyEffects = def.GetApplyEffects(num, wrecked: false, OutpostPhase.Downtime, CampaignController)?.ToArray(),
			UpgradeEffects = def.GetActiveEffects(num + 1, wrecked: false, OutpostPhase.Downtime, CampaignController).ToArray(),
			UpgradeConstructEffects = def.GetOnConstructEffects(num + 1, CampaignController).ToArray()
		};
	}

	public BuildingStateDTO GetBuildingById(BuildingTypeID id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (buildingStateByID != null)
		{
			return GetBuilding(buildingStateByID);
		}
		return GetNewBuildingByDef(GetBuildingDef(id));
	}

	public BuildingStateDTO GetNewBuildingByDef(BuildingTypeDef def)
	{
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(def);
		uint level = 0u;
		string failReason;
		return new BuildingStateDTO
		{
			Def = def,
			Icon = buildingConfig.Icon,
			IconTintable = buildingConfig.TimetableIcon,
			Level = level,
			NameLocKey = buildingConfig.GetName(),
			DescriptionLocKey = buildingConfig.GetConstructEffect(),
			ConstructionState = BuildingConstructionType.None,
			IsDamaged = false,
			CanBuild = (BuildingsController.IsConstructionAllowed && BuildingsController.CanBuild(def)),
			MissingBuildRequirement = (def.CanBeConstructed(CampaignController, out failReason) ? null : failReason),
			CanUpgrade = false,
			CanRebuild = false,
			IsWrecked = false,
			ConstructionCost = GetConstructCost(def),
			UpgradeCost = null,
			HasStartedProject = false,
			IsDonationComplete = false,
			AvailableProsperity = _view.GameState.Outpost.ProsperityLevel,
			AvailableMorale = _view.GameState.Outpost.Morale,
			IsConstructed = false,
			FinalizeDonationTT = ((!CanConstruct) ? string.Format(LocalizationManager.GetTranslation("GUI_SECOND_BUILD_TOOLTIP"), BuildingsController.CampaignController.Settings.ConstructPerTurn + BuildingsController.CampaignController.Settings.ExtraConstructPerTurn) : string.Empty),
			ConstructEffects = def.GetOnConstructEffects(1u, CampaignController)?.ToArray(),
			ActiveEffects = def.GetActiveEffects(level, wrecked: false, OutpostPhase.Downtime, CampaignController)?.ToArray(),
			ApplyEffects = def.GetApplyEffects(level, wrecked: false, OutpostPhase.Downtime, CampaignController)?.ToArray(),
			UpgradeEffects = ((def.Levels > 1) ? def.GetActiveEffects(2u, wrecked: false, OutpostPhase.Downtime, CampaignController).ToArray() : null),
			UpgradeConstructEffects = ((def.Levels > 1) ? def.GetOnConstructEffects(2u, CampaignController).ToArray() : null)
		};
	}

	public BuildingTypeDef GetBuildingDef(BuildingId id)
	{
		return DefRepo.Buildings[new BuildingTypeID((uint)id)];
	}

	public BuildingTypeDef GetBuildingDef(BuildingTypeID id)
	{
		return DefRepo.Buildings[id];
	}

	public BuildingState GetBuildingStateByID(BuildingTypeID id)
	{
		return OutpostState.Buildings.FirstOrDefault((BuildingState b) => b.Def.ID == id);
	}

	public BuildingState GetBuildingStateByID(BuildingId id)
	{
		return OutpostState.Buildings.FirstOrDefault((BuildingState b) => b.Def.ID.IsEqual(id));
	}

	public BuildingState GetBuildingByType<T>() where T : BuildingTypeDef
	{
		return OutpostState.Buildings.FirstOrDefault((BuildingState b) => b.Def is T);
	}

	public uint GetBuildingLevelById(BuildingTypeID id)
	{
		return GetBuildingStateByID(id)?.Level ?? 0;
	}

	public bool IsBuildingWrecked(BuildingTypeID id)
	{
		return GetBuildingStateByID(id)?.WreckedOrRebuilding ?? false;
	}

	private bool IsBuildingConstructed(BuildingTypeID id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		return IsBuildingConstructed(buildingStateByID);
	}

	private bool IsBuildingConstructed(BuildingState state)
	{
		if (state != null)
		{
			return state.ConstructionState != BuildingConstructionType.NewBuilding;
		}
		return false;
	}

	public bool IsBuildingUnlocked(BuildingTypeID id)
	{
		return CampaignController.OutpostController.ValidForBuildingIDs.Contains(id);
	}

	public OutpostStateDTO GetOutpostStateDTO()
	{
		return new OutpostStateDTO(OutpostState, GameState, GameState.Inspiration);
	}

	public bool HasBuilding<T>(bool includeInConstruction = true) where T : BuildingTypeDef
	{
		return OutpostState.Buildings.Any((BuildingState b) => b.Def is T && b.Level != 0 && (includeInConstruction || b.ConstructionState != BuildingConstructionType.NewBuilding));
	}

	public bool HasMoreUpgrades<T>() where T : BuildingTypeDef
	{
		return OutpostState.Buildings.Any((BuildingState b) => b.Def is T && HasMoreUpgrades(b));
	}

	public bool HasMoreUpgrades(BuildingState state)
	{
		uint num = ((state.ConstructionState != BuildingConstructionType.None) ? (state.Level - 1) : state.Level);
		if (state.Def.Levels > num)
		{
			return !state.Def.CannotBuyUpgrades;
		}
		return false;
	}

	public BuildingStateDTO.Cost? GetUpgradeCost(BuildingState state)
	{
		if (!HasMoreUpgrades(state))
		{
			return null;
		}
		int level = (int)((state.ConstructionState == BuildingConstructionType.Upgrading || state.ConstructionState == BuildingConstructionType.NewBuilding) ? state.Level : (state.Level + 1));
		if (state.donationDistribution != null)
		{
			return new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = (uint)state.donationDistribution.GoldCost,
					Resources = state.donationDistribution.MaterialCost,
					Morale = state.donationDistribution.Morale
				},
				RequiredProsperityLevel = GetProsperityRequirement(state.Def, level)
			};
		}
		if (IsCurrentConstructionProject(state))
		{
			return new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = BuildingsController.GetUpgradeGoldCost(state.Def, level),
					Resources = BuildingsController.GetUpgradeResourceCostWithReduction(state.Def, level),
					Morale = BuildingsController.GetUpgradeMoraleCost(state, level)
				},
				RawResourcesCost = BuildingsController.GetUpgradeResourceCost(state, level),
				RequiredProsperityLevel = BuildingsController.GetUpgradeProsperityRequirement(state.Def, level)
			};
		}
		return new BuildingStateDTO.Cost
		{
			Spendable = new BuildingTypeDef.Cost
			{
				Gold = BuildingsController.GetUpgradeGoldCost(state),
				Resources = BuildingsController.GetUpgradeResourceCostWithReduction(state),
				Morale = BuildingsController.GetUpgradeMoraleCost(state)
			},
			RawResourcesCost = BuildingsController.GetUpgradeResourceCost(state),
			RequiredProsperityLevel = GetUpgradeProsperityRequirement(state)
		};
	}

	public BuildingStateDTO.Cost? GetConstructCost(BuildingState state)
	{
		if (state.donationDistribution != null)
		{
			return new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = (uint)state.donationDistribution.GoldCost,
					Resources = state.donationDistribution.MaterialCost,
					Morale = state.donationDistribution.Morale
				},
				RequiredProsperityLevel = GetProsperityRequirement(state.Def, 1)
			};
		}
		return GetConstructCost(state.Def);
	}

	public BuildingStateDTO.Cost? GetConstructCost(BuildingTypeDef def)
	{
		return new BuildingStateDTO.Cost
		{
			Spendable = new BuildingTypeDef.Cost
			{
				Gold = BuildingsController.GetBuildGoldCost(def),
				Resources = BuildingsController.GetBuildResourceCostWithReduction(def),
				Morale = BuildingsController.GetConstructMoraleCost(def)
			},
			RawResourcesCost = BuildingsController.GetBuildResourceCost(def),
			RequiredProsperityLevel = GetProsperityRequirement(def, 1)
		};
	}

	private uint GetProjectedExtraMoralePenalty(int newConstructionsCount = 1)
	{
		if (BuildingsController.GetConstructionsRemaining() - newConstructionsCount > 0)
		{
			return 0u;
		}
		return BuildingsController.GetConstructionExtraMoraleCost();
	}

	public uint GetProsperityRequirement(BuildingTypeDef def, int level)
	{
		return def.GetProsperityRequirement(level);
	}

	public uint GetUpgradeProsperityRequirement(BuildingState building)
	{
		return BuildingsController.GetUpgradeProsperityRequirement(building);
	}

	public BuildingStateDTO.Cost? GetRebuildCost(BuildingState state)
	{
		if (!state.WreckedOrRebuilding)
		{
			return null;
		}
		return new BuildingStateDTO.Cost
		{
			Spendable = new BuildingTypeDef.Cost
			{
				Resources = ((state.donationDistribution != null) ? state.donationDistribution.MaterialCost : BuildingsController.GetRebuildResourceCostWithReduction(state))
			},
			RawResourcesCost = BuildingsController.GetRebuildResourceCost(state)
		};
	}

	public bool CanUpgrade(BuildingState state)
	{
		return BuildingsController.CanUpgrade(state, state.Level + 1);
	}

	public IBuildingInfoParams GetBuildingInfoParams(BuildingStateDTO building)
	{
		BuildingTypeDef def = building.Def;
		Alchemist_BuildingTypeDef alchemist_BuildingTypeDef = def as Alchemist_BuildingTypeDef;
		if (alchemist_BuildingTypeDef == null)
		{
			Enhancer_BuildingTypeDef enhancer_BuildingTypeDef = def as Enhancer_BuildingTypeDef;
			if (enhancer_BuildingTypeDef == null)
			{
				if (!(def is Library_BuildingTypeDef))
				{
					if (!(def is Garden_BuildingTypeDef))
					{
						if (!(def is Barrack_BuildingTypeDef))
						{
							if (!(def is Stables_BuildingTypeDef))
							{
								if (!(def is Workshop_BuildingTypeDef))
								{
									if (!(def is Tavern_BuildingTypeDef))
									{
										if (!(def is Inn_BuildingTypeDef))
										{
											if (!(def is TempleOfTheGreatOak_BuildingTypeDef templeOfTheGreatOak_BuildingTypeDef))
											{
												TradingPost_BuildingTypeDef tradingPost_BuildingTypeDef = def as TradingPost_BuildingTypeDef;
												if (tradingPost_BuildingTypeDef == null)
												{
													Craftsman_BuildingTypeDef craftsman_BuildingTypeDef = def as Craftsman_BuildingTypeDef;
													if (craftsman_BuildingTypeDef == null)
													{
														if (!(def is Jeweler_BuildingTypeDef))
														{
															if (!(def is HallOfRevelry_BuildingTypeDef))
															{
																if (def is TownHall_BuildingTypeDef)
																{
																	return new TownHall_UIBuildInfoFunctionality.TownHallStateParams
																	{
																		AllChallengesCount = CampaignController.DefRepository.Challenges.Count,
																		CompletedChallengesCount = GameState.CompletedChallengeIDs.Count
																	};
																}
																if (def is Boat_BuildingTypeDef || def is ClimbingGear_BuildingTypeDef || def is Sled_BuildingTypeDef)
																{
																	return new WorkshopUpgradeBuildingParams
																	{
																		Icon = UIInfoTools.Instance.GetBuildingConfig(def).Icon,
																		IsWorkshopWrecked = GetBuildingByType<Workshop_BuildingTypeDef>().WreckedOrRebuilding
																	};
																}
																BuildingTypeDef def2 = def;
																if (!IsResourceBuilding(def2))
																{
																	if (def is Walls_BuildingTypeDef)
																	{
																		return default(Walls_UIBuildInfoFunctionality.WallsData);
																	}
																	if (IsSingleButtonFunctionalityBuilding(building))
																	{
																		return new SingleActionBuildingParams
																		{
																			Action = () =>
																			{
																				ShowBuildingScreen(building.Def);
																			},
																			IsInteractable = (building.IsConstructed && !building.IsWrecked && !building.IsRebuilding && _view.SelectedCharacter != null),
																			ActionLocKey = "GUI_UseBuilding",
																			ActionHintLocKey = "GUI_open_building_message"
																		};
																	}
																	return default(Info_UIBuildInfoFunctionality.InfoParams);
																}
																return new ResourceBuildingData
																{
																	BuyAction = GetResourceBuildingBuyActionData(def2),
																	SellAction = GetResourceBuildingSellActionData(def2),
																	SellAtHighPriceAction = GetResourceBuildingSellAtHighPriceActionData(def2)
																};
															}
															return new HallOfRevelry_UIBuildInfoFunctionality.HallOfRevelryStateParams
															{
																Rows = GetHallOfRevelryData().ToArray()
															};
														}
														return new Jeweller_UIBuildInfoFunctionality.JewellerStateParams
														{
															JewellerData = GetJewelerData()
														};
													}
													return new SingleActionBuildingParams
													{
														IsInteractable = (IsBuildingConstructed(craftsman_BuildingTypeDef.ID) && !IsBuildingWrecked(craftsman_BuildingTypeDef.ID)),
														ActionLocKey = GetBuildingName(craftsman_BuildingTypeDef),
														Action = () =>
														{
															ShowBuildingScreen(craftsman_BuildingTypeDef);
														}
													};
												}
												return new SingleActionBuildingParams
												{
													IsInteractable = true,
													ActionLocKey = GetBuildingName(tradingPost_BuildingTypeDef),
													Action = () =>
													{
														ShowBuildingScreen(tradingPost_BuildingTypeDef);
													}
												};
											}
											return new SingleActionBuildingParams
											{
												IsInteractable = (_view.SelectedCharacter != null && IsBuildingConstructed(templeOfTheGreatOak_BuildingTypeDef.ID) && !IsBuildingWrecked(templeOfTheGreatOak_BuildingTypeDef.ID) && GameController.OutpostController.CanBlessCharacter(_view.SelectedCharacter)),
												ActionLocKey = "GUI_CONTRIBUTE",
												ActionHintLocKey = (building.IsWrecked ? string.Empty : ((_view.SelectedCharacter != null && _view.SelectedCharacter.TempleBlessed) ? "GUI_TEMPLE_CONTRIBUTE_LIMIT_REACHED" : "GUI_TEMPLE_CONTRIBUTE_DESCRIPTION")),
												Action = () =>
												{
													Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.BlessCharacter, _view.SelectedCharacter.ID);
												}
											};
										}
										return new Inn_UIBuildInfoFunctionality.InnStateParams
										{
											GoldCost = (uint)_view.GameController.TradingController.GetMaterialBuyGold(OutpostMaterialType.Lumber),
											CurrentGold = (_view.SelectedCharacter?.Gold ?? 0),
											MaterialAmount = 1u,
											PickedMaterialType = GetInnTradeMaterialType(_view.SelectedCharacter),
											OnPickMaterialType = PickInnTradeMaterialType,
											Functionality = GetInnAction(building.Def),
											CanBuy = (OutpostMaterialType type) => CanBuyMaterialFromInn(_view.SelectedCharacter, type)
										};
									}
									return new Tavern_UIBuildInfoFunctionality.TavernStateParams
									{
										Festivals = new Tavern_BuildingTypeDef.FestivalSize[3]
										{
											Tavern_BuildingTypeDef.FestivalSize.Small,
											Tavern_BuildingTypeDef.FestivalSize.Medium,
											Tavern_BuildingTypeDef.FestivalSize.Large
										}.Select(GetFestivalData).ToArray(),
										HoldFestival = new BuildingFunctionalityAction
										{
											ActionText = "GUI_TAVERN_CELEBRATE",
											Charges = ((CurrentFestivalSize == Tavern_BuildingTypeDef.FestivalSize.None) ? 1 : 0),
											IsInteractable = CanHoldAnyFestival(),
											Localize = true,
											MaxCharges = 1,
											OnClick = (object param) =>
											{
												HoldFestival((Tavern_BuildingTypeDef.FestivalSize)param);
											},
											TooltipText = GetTooltip(building)
										},
										CurrentFestival = CurrentFestivalSize
									};
								}
								return new WorkshopBuildingParams
								{
									HasBoat = HasBuilding<Boat_BuildingTypeDef>(includeInConstruction: false),
									HasClimbingGear = HasBuilding<ClimbingGear_BuildingTypeDef>(includeInConstruction: false),
									HasSled = HasBuilding<Sled_BuildingTypeDef>(includeInConstruction: false)
								};
							}
							return new Stables_UIBuildInfoFunctionality.StablesStateParams
							{
								Pets = GetAllPets().ToList(),
								PetCapacity = _petController.GetPetCapacity(),
								MaxPetCapacity = _petController.GetPetMaxCapacity(),
								GetSlotUnlockAtLevel = GetSlotUnlockAtLevel
							};
						}
						return new BarracksStateParams
						{
							AvailableSoldersToTrain = _view.GameController.OutpostController.CanTrainSoldiersCount,
							SoldierCount = _view.GameController.OutpostController.Soldiers,
							MaxSoldierCount = _view.GameController.OutpostController.SoldierCapacity,
							CanTrainSoldier = (_view.SelectedCharacter != null && _view.GameController.OutpostController.CanTrainSoldiers(1u, _view.SelectedCharacter)),
							CurrentDefense = _view.GameController.OutpostController.CampaignController.AttackController.SoldierDefenseModifier,
							IsPartyLeader = _view.IsPartyLeader,
							TrainAction = GetBarracksBuildingActions().First(),
							GoldCost = _view.GameController.OutpostController.TrainSoldierGoldCost,
							SoldierPerBuy = 1u,
							CurrentGold = (_view.SelectedCharacter?.Gold ?? 0),
							CurrentResources = (_view.SelectedCharacter?.Resources ?? default(OutpostResources)),
							MaterialCost = _view.GameController.OutpostController.TrainSoldierMaterialCost,
							PickedMaterialType = GetTrainSoldierMaterialType(_view.SelectedCharacter),
							OnPickMaterialType = PickTrainSoldierMaterialType,
							Hint = ((building.IsConstructed && !building.IsWrecked && !building.IsRebuilding) ? "GUI_SELECT_RESOURCE" : null)
						};
					}
					return new GardenStateParams
					{
						Plots = GetAllPossibleGardenPlots().ToArray(),
						PlantState = GetPlantState(),
						IsPartyLeader = _view.IsPartyLeader,
						Plant = (OutpostHerbType[] herbs) =>
						{
							PlantHerbs(herbs);
						},
						Resources = OutpostState.Resources
					};
				}
				return GetLibraryStateParams();
			}
			return new SingleActionBuildingParams
			{
				Action = () =>
				{
					ShowBuildingScreen(enhancer_BuildingTypeDef);
				},
				ActionLocKey = GetBuildingName(enhancer_BuildingTypeDef),
				IsInteractable = (IsBuildingConstructed(enhancer_BuildingTypeDef.ID) && !IsBuildingWrecked(enhancer_BuildingTypeDef.ID))
			};
		}
		return new SingleActionBuildingParams
		{
			Action = () =>
			{
				ShowBuildingScreen(alchemist_BuildingTypeDef);
			},
			ActionLocKey = GetBuildingName(alchemist_BuildingTypeDef),
			IsInteractable = (IsBuildingConstructed(alchemist_BuildingTypeDef.ID) && !IsBuildingWrecked(alchemist_BuildingTypeDef.ID))
		};
	}

	private string GetTooltip(BuildingStateDTO state)
	{
		BuildingTypeDef def = state.Def;
		if (!(def is Garden_BuildingTypeDef))
		{
			if (!(def is HallOfRevelry_BuildingTypeDef))
			{
				return string.Empty;
			}
			if (CurrentFestivalSize != Tavern_BuildingTypeDef.FestivalSize.None)
			{
				return LocalizationManager.GetTranslation("GUI_TavernActiveFestivalWarning");
			}
		}
		return string.Empty;
	}

	public string GetBuildingName(BuildingTypeDef def)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		return UIInfoTools.Instance.GetBuildingConfig(def).GetName(buildingStateByID?.Level ?? 1);
	}

	public uint GetSlotUnlockAtLevel(int slotIndex)
	{
		BuildingTypeDef buildingTypeDef = DefRepo.Buildings.Values.FirstOrDefault((BuildingTypeDef x) => x is Stables_BuildingTypeDef);
		for (uint num = 0u; num <= buildingTypeDef.Levels; num++)
		{
			int num2 = (from x in buildingTypeDef.GetActiveEffects(num, wrecked: false, OutpostPhase.Downtime, CampaignController)
				where x.Type == CampaignEffectType.PetCapacity
				select x).Sum((CampaignEffect x) => x.GetInt(0));
			if (slotIndex < num2)
			{
				return num;
			}
		}
		return 0u;
	}

	private bool IsResourceBuilding(BuildingTypeDef def)
	{
		if (def is HuntingLodge_BuildingTypeDef || def is HideDepot_BuildingTypeDef || def is LoggingCamp_BuildingTypeDef || def is LumberDepot_BuildingTypeDef || def is MetalDepot_BuildingTypeDef || def is MiningCamp_BuildingTypeDef)
		{
			return true;
		}
		return false;
	}

	private IEnumerable<BuildingFunctionalityAction> GetBuildingActions(BuildingStateDTO building)
	{
		return GetBuildingActions(GetBuildingStateByID(building.Def.ID));
	}

	private IEnumerable<BuildingFunctionalityAction> GetBuildingActions(BuildingState building)
	{
		if (building.Def is Barrack_BuildingTypeDef)
		{
			return GetBarracksBuildingActions();
		}
		if (IsResourceBuilding(building.Def))
		{
			return GetResourceBuildingActions(building.Def);
		}
		return null;
	}

	private IEnumerable<BuildingFunctionalityAction> GetResourceBuildingActions(BuildingTypeDef def)
	{
		OutpostMaterialType material = GetResourceByResourceBuilding(def);
		BuildingFunctionalityAction? resourceBuildingBuyAction = GetResourceBuildingBuyAction(def, material);
		if (resourceBuildingBuyAction.HasValue)
		{
			yield return resourceBuildingBuyAction.Value;
		}
		BuildingFunctionalityAction? resourceBuildingSellAtHighPriceAction = GetResourceBuildingSellAtHighPriceAction(def, material);
		if (resourceBuildingSellAtHighPriceAction.HasValue)
		{
			yield return resourceBuildingSellAtHighPriceAction.Value;
		}
		BuildingFunctionalityAction? resourceSellAtDefaultPriceAction = GetResourceSellAtDefaultPriceAction(def, material);
		if (resourceSellAtDefaultPriceAction.HasValue)
		{
			yield return resourceSellAtDefaultPriceAction.Value;
		}
	}

	private BuildingFunctionalityAction GetInnAction(BuildingTypeDef def)
	{
		BuildingFunctionalityAction result = new BuildingFunctionalityAction
		{
			ActionText = LocalizationManager.GetTranslation("GUI_BUY"),
			Charges = 0,
			IsInteractable = false,
			MaxCharges = 0,
			OnClick = null,
			TooltipText = ""
		};
		if (_view.SelectedCharacter == null)
		{
			return result;
		}
		BuildingState state = GetBuildingStateByID(def.ID);
		if (state == null || state.ConstructionState == BuildingConstructionType.NewBuilding)
		{
			return result;
		}
		CampaignEffect campaignEffect = GetApplyEffectsOfType(def.ID.ToBuildingID(), CampaignEffectType.BuyMaterial).FirstOrDefault();
		if (campaignEffect.Type == CampaignEffectType.None)
		{
			return result;
		}
		int num = campaignEffect.GetInt(0);
		int num2 = OutpostResources.AllMaterialTypes().Count() * (int)state.Level - state.BuyResources.Total;
		int num3 = num - num2;
		OutpostMaterialType outpostMaterialType = OutpostMaterialType.Hide;
		int materialAmount = TradingController.GetMaterialsCanBuy(state.Def).GetMaterialAmount(outpostMaterialType);
		bool flag = !state.WreckedOrRebuilding && materialAmount > 0 && num3 > 0;
		bool flag2 = num3 == 0 || materialAmount == 0;
		bool flag3 = _view.SelectedCharacter.Gold >= TradingController.GetMaterialBuyGold(outpostMaterialType);
		string text = string.Empty;
		if (flag2)
		{
			text = LocalizationManager.GetTranslation("GUI_PURCHASE_LIMIT_REACHED");
		}
		else if (!flag3)
		{
			text = LocalizationManager.GetTranslation("GUI_INSUFFICIENT_RESOURCES");
		}
		return new BuildingFunctionalityAction
		{
			ActionText = LocalizationManager.GetTranslation("GUI_BUY"),
			Charges = num3,
			IsInteractable = ((flag && TradingController.IsMaterialBuyingEnabled(outpostMaterialType, state.Def)) & flag3),
			MaxCharges = num,
			OnClick = (flag ? ((BuildingFunctionalityActionHandler)((object _) =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.BuyMaterials, _view.SelectedCharacter.ID, new OutpostResources(GetInnTradeMaterialType(_view.SelectedCharacter)), state.Def);
			})) : null),
			TooltipText = (flag2 ? LocalizationManager.GetTranslation("GUI_weekly_limit_reached") : text)
		};
	}

	private IEnumerable<BuildingFunctionalityAction> GetBarracksBuildingActions()
	{
		_ = _view.Choreographer.GameController.OutpostController;
		int maxTrainSoldiers = GetMaxTrainSoldiers();
		int trainSoldiersLeft = GetTrainSoldiersLeft();
		_ = string.Empty;
		yield return new BuildingFunctionalityAction
		{
			Charges = trainSoldiersLeft,
			MaxCharges = maxTrainSoldiers,
			ActionText = LocalizationManager.GetTranslation("GUI_TRAIN_SOLDIER"),
			IsInteractable = (_view.SelectedCharacter != null && CanTrainSoldiers() && GetTrainSoldierMaterialType(_view.SelectedCharacter) != OutpostMaterialType.None),
			OnClick = (object _) =>
			{
				TrainSoldier(GetTrainSoldierMaterialType(_view.SelectedCharacter));
			},
			TooltipText = ""
		};
	}

	private OutpostMaterialType GetResourceByResourceBuilding(BuildingTypeDef def)
	{
		if (!(def is HideDepot_BuildingTypeDef))
		{
			if (!(def is HuntingLodge_BuildingTypeDef))
			{
				if (!(def is LoggingCamp_BuildingTypeDef))
				{
					if (!(def is LumberDepot_BuildingTypeDef))
					{
						if (!(def is MetalDepot_BuildingTypeDef))
						{
							if (def is MiningCamp_BuildingTypeDef)
							{
								return OutpostMaterialType.Metal;
							}
							throw new ArgumentOutOfRangeException("def is not a resource building! Cannot find related resource type!");
						}
						return OutpostMaterialType.Metal;
					}
					return OutpostMaterialType.Lumber;
				}
				return OutpostMaterialType.Lumber;
			}
			return OutpostMaterialType.Hide;
		}
		return OutpostMaterialType.Hide;
	}

	private CampaignEffectType MaterialToEffectType(OutpostMaterialType material)
	{
		return material switch
		{
			OutpostMaterialType.Lumber => CampaignEffectType.BuyLumber, 
			OutpostMaterialType.Metal => CampaignEffectType.BuyMetal, 
			OutpostMaterialType.Hide => CampaignEffectType.BuyHide, 
			_ => throw new ArgumentOutOfRangeException($"{material} is not valid material type!"), 
		};
	}

	private CampaignEffectType SellMaterialToEffectType(OutpostMaterialType material)
	{
		return material switch
		{
			OutpostMaterialType.Lumber => CampaignEffectType.SellLumber, 
			OutpostMaterialType.Metal => CampaignEffectType.SellMetal, 
			OutpostMaterialType.Hide => CampaignEffectType.SellHide, 
			_ => throw new ArgumentOutOfRangeException($"{material} is not valid material type!"), 
		};
	}

	private CampaignEffectType SellMaterialAtHighPriceToEffectType(OutpostMaterialType material)
	{
		return material switch
		{
			OutpostMaterialType.Lumber => CampaignEffectType.SellLumberForHighPrice, 
			OutpostMaterialType.Metal => CampaignEffectType.SellMetalForHighPrice, 
			OutpostMaterialType.Hide => CampaignEffectType.SellHideForHighPrice, 
			_ => throw new ArgumentOutOfRangeException($"{material} is not valid material type!"), 
		};
	}

	private BuildingFunctionalityAction? GetResourceBuildingBuyAction(BuildingTypeDef def, OutpostMaterialType material)
	{
		if (_view.SelectedCharacter == null)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		CampaignEffectType campaignEffectType = MaterialToEffectType(material);
		CampaignEffect[] source = GetApplyEffectsOfType(def.ID.ToBuildingID(), campaignEffectType).ToArray();
		if (!source.Any())
		{
			return null;
		}
		IEnumerable<CampaignEffect> source2 = GameState.NextScenarioModifiers.EffectsActiveUntilNextWeek.Where((CampaignEffect x) => x.Type == campaignEffectType && x.GetInt(1) == def.ID.ID);
		int num = source.First().GetInt(0) + source2.Sum((CampaignEffect x) => x.GetInt(0));
		int num2 = (flag ? TradingController.GetMaterialsCanBuy(def).GetMaterialAmount(material) : TradingController.GetMaterialsCanBuyFromDef(def, 1u).GetMaterialAmount(material));
		bool flag2 = _view.SelectedCharacter.Gold >= TradingController.GetMaterialBuyGold(material);
		bool flag3 = num2 > 0 && num > 0;
		bool num3 = num > 0 && num2 == 0;
		bool flag4 = flag && !buildingStateByID.WreckedOrRebuilding && buildingStateByID.ConstructionState != BuildingConstructionType.NewBuilding;
		string translation = LocalizationManager.GetTranslation("GUI_BUY");
		if (num3)
		{
			translation = LocalizationManager.GetTranslation("GUI_weekly_limit_reached");
		}
		else if (!flag2)
		{
			translation = LocalizationManager.GetTranslation("GUI_INSUFFICIENT_RESOURCES");
		}
		return new BuildingFunctionalityAction
		{
			ActionText = string.Format("<sprite name=Gold>{0} <sprite name=ArrowShort> <sprite name={2}>{1}", TradingController.GetMaterialBuyGold(material), 1, FHUILocKeys.GetMaterialIconLocalizationKey(material)),
			Charges = num2,
			IsInteractable = ((flag3 && TradingController.IsMaterialBuyingEnabled(material, def)) & flag2 & flag4),
			MaxCharges = num,
			OnClick = (flag3 ? ((BuildingFunctionalityActionHandler)((object _) =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.BuyMaterials, _view.SelectedCharacter.ID, new OutpostResources(material), def);
			})) : null),
			TooltipText = translation
		};
	}

	public BuildingFunctionalityAction? GetResourceSellAtDefaultPriceAction(BuildingTypeDef def, OutpostMaterialType material)
	{
		if (_view.SelectedCharacter == null)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		CampaignEffectType effectType = SellMaterialToEffectType(material);
		if (!GetApplyEffectsOfType(def.ID.ToBuildingID(), effectType).ToArray().Any())
		{
			return null;
		}
		int sellAmount = 1;
		bool flag2 = flag && TradingController.CanSellMaterialForDefaultPrice(material, def);
		bool flag3 = _view.SelectedCharacter.Resources.HasMaterial(material, sellAmount);
		bool flag4 = flag && !buildingStateByID.WreckedOrRebuilding && buildingStateByID.ConstructionState != BuildingConstructionType.NewBuilding;
		string tooltipText = ((!flag3) ? LocalizationManager.GetTranslation("GUI_INSUFFICIENT_RESOURCES") : LocalizationManager.GetTranslation("GUI_SELL"));
		return new BuildingFunctionalityAction
		{
			ActionText = string.Format("<sprite name={2}>{0} <sprite name=ArrowShort> <sprite name=Gold>{1}", sellAmount, flag ? TradingController.GetMaterialSellGold(material, def) : TradingController.GetDefMaterialSellGold(material, def, 1u), FHUILocKeys.GetMaterialIconLocalizationKey(material)),
			Charges = 0,
			IsInteractable = ((flag2 && TradingController.IsMaterialSellingEnabled(material) && _view.SelectedCharacter.Resources.HasMaterial(material, sellAmount)) & flag4),
			MaxCharges = 0,
			OnClick = (object _) =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.SellMaterials, _view.SelectedCharacter.ID, new OutpostResources(material, sellAmount), def);
			},
			TooltipText = tooltipText
		};
	}

	public BuildingFunctionalityAction? GetResourceBuildingSellAtHighPriceAction(BuildingTypeDef def, OutpostMaterialType material)
	{
		if (_view.SelectedCharacter == null)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		int num = (flag ? TradingController.GetMaxMaterialsCanSellForHigherPrice(def).GetMaterialAmount(material) : TradingController.GetMaxMaterialsCanSellForHigherPriceFromDef(def, 0u).GetMaterialAmount(material));
		int sellAmount = 1;
		CampaignEffectType effectType = SellMaterialAtHighPriceToEffectType(material);
		if (!GetApplyEffectsOfType(def.ID.ToBuildingID(), effectType).ToArray().Any())
		{
			return null;
		}
		int num2 = buildingStateByID?.SellResourcesForHigherPrices.GetMaterialAmount(material) ?? 0;
		int num3 = (flag ? TradingController.GetMaterialsCanSellForHigherPrice(_view.SelectedCharacter, def).GetMaterialAmount(material) : 0);
		bool flag2 = num > 0 && num2 == 0;
		bool flag3 = _view.SelectedCharacter.Resources.HasMaterial(material, sellAmount);
		bool flag4 = flag && !buildingStateByID.WreckedOrRebuilding && buildingStateByID.ConstructionState != BuildingConstructionType.NewBuilding && TradingController.IsMaterialSellingEnabled(material);
		string translation = LocalizationManager.GetTranslation("GUI_SELL");
		if (flag2)
		{
			translation = LocalizationManager.GetTranslation("GUI_weekly_limit_reached");
		}
		else if (!flag3)
		{
			translation = LocalizationManager.GetTranslation("GUI_INSUFFICIENT_RESOURCES");
		}
		return new BuildingFunctionalityAction
		{
			ActionText = string.Format("<sprite name={2}>{0} <sprite name=ArrowShort> <sprite name=Gold>{1}", sellAmount, TradingController.CampaignController.Settings.SellMaterialGoldHighPrice, FHUILocKeys.GetMaterialIconLocalizationKey(material)),
			Charges = num2,
			IsInteractable = ((num > 0 && num3 > 0 && TradingController.IsMaterialSellingEnabled(material)) & flag3 & flag4),
			MaxCharges = num,
			OnClick = ((num3 > 0) ? ((BuildingFunctionalityActionHandler)((object _) =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.SellMaterials, _view.SelectedCharacter.ID, new OutpostResources(material, sellAmount), def);
			})) : null),
			TooltipText = translation
		};
	}

	private UIBuildInfoResourceActionElement.ResourceBuildingActionData? GetResourceBuildingBuyActionData(BuildingTypeDef def)
	{
		OutpostMaterialType resourceByResourceBuilding = GetResourceByResourceBuilding(def);
		BuildingFunctionalityAction? resourceBuildingBuyAction = GetResourceBuildingBuyAction(def, resourceByResourceBuilding);
		if (!resourceBuildingBuyAction.HasValue)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		int materialBuyGold = TradingController.GetMaterialBuyGold(resourceByResourceBuilding);
		return new UIBuildInfoResourceActionElement.ResourceBuildingActionData
		{
			Action = resourceBuildingBuyAction.Value,
			NameLocKey = "GUI_BUY",
			DescriptionLocKey = "",
			LeftSideElement = new ResourceElement.ResourceActionElementData
			{
				IsGold = true,
				Amount = materialBuyGold,
				IsAvailable = (materialBuyGold <= _view.SelectedCharacter?.Gold)
			},
			RightSideElement = new ResourceElement.ResourceActionElementData
			{
				Material = resourceByResourceBuilding,
				Amount = 1,
				IsAvailable = true
			},
			Locked = !flag,
			LockedLevel = 1u
		};
	}

	private UIBuildInfoResourceActionElement.ResourceBuildingActionData? GetResourceBuildingSellActionData(BuildingTypeDef def)
	{
		OutpostMaterialType resourceByResourceBuilding = GetResourceByResourceBuilding(def);
		BuildingFunctionalityAction? resourceSellAtDefaultPriceAction = GetResourceSellAtDefaultPriceAction(def, resourceByResourceBuilding);
		if (!resourceSellAtDefaultPriceAction.HasValue)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		int amount = 1;
		int amount2 = (flag ? TradingController.GetMaterialSellGold(resourceByResourceBuilding, def) : TradingController.GetDefMaterialSellGold(resourceByResourceBuilding, def, 1u));
		return new UIBuildInfoResourceActionElement.ResourceBuildingActionData
		{
			Action = resourceSellAtDefaultPriceAction.Value,
			NameLocKey = "GUI_SELL",
			DescriptionLocKey = "",
			LeftSideElement = new ResourceElement.ResourceActionElementData
			{
				Amount = amount,
				Material = resourceByResourceBuilding,
				IsAvailable = _view.SelectedCharacter.Resources.HasMaterial(resourceByResourceBuilding, amount)
			},
			RightSideElement = new ResourceElement.ResourceActionElementData
			{
				Amount = amount2,
				IsGold = true,
				IsAvailable = true
			},
			Locked = !flag,
			LockedLevel = 1u
		};
	}

	private UIBuildInfoResourceActionElement.ResourceBuildingActionData? GetResourceBuildingSellAtHighPriceActionData(BuildingTypeDef def)
	{
		OutpostMaterialType resourceByResourceBuilding = GetResourceByResourceBuilding(def);
		BuildingFunctionalityAction? resourceBuildingSellAtHighPriceAction = GetResourceBuildingSellAtHighPriceAction(def, resourceByResourceBuilding);
		if (!resourceBuildingSellAtHighPriceAction.HasValue)
		{
			return null;
		}
		BuildingState buildingStateByID = GetBuildingStateByID(def.ID);
		bool flag = IsBuildingConstructed(buildingStateByID);
		int amount = 1;
		int goldForHighPriceMaterial = OutpostController.CampaignController.TradingController.GetGoldForHighPriceMaterial(resourceByResourceBuilding);
		return new UIBuildInfoResourceActionElement.ResourceBuildingActionData
		{
			Action = resourceBuildingSellAtHighPriceAction.Value,
			NameLocKey = "GUI_SELL",
			DescriptionLocKey = "",
			LeftSideElement = new ResourceElement.ResourceActionElementData
			{
				Amount = amount,
				Material = resourceByResourceBuilding,
				IsAvailable = _view.SelectedCharacter.Resources.HasMaterial(resourceByResourceBuilding, amount)
			},
			RightSideElement = new ResourceElement.ResourceActionElementData
			{
				Amount = goldForHighPriceMaterial,
				IsGold = true,
				IsAvailable = true
			},
			Locked = !flag,
			LockedLevel = 1u
		};
	}

	public bool CanBuyMaterialFromInn(CharacterState character, OutpostMaterialType material)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(BuildingId.Inn);
		if (buildingStateByID == null || buildingStateByID.WreckedOrRebuilding || buildingStateByID.ConstructionState == BuildingConstructionType.NewBuilding)
		{
			return false;
		}
		if (character != null && material != OutpostMaterialType.None && material != OutpostMaterialType.All)
		{
			_ = 5;
		}
		if (GetApplyEffectsOfType(buildingStateByID.Def.ID.ToBuildingID(), CampaignEffectType.BuyMaterial).FirstOrDefault().Type == CampaignEffectType.None)
		{
			return false;
		}
		if (!TradingController.CanCharacterBuyMaterial(character, material, buildingStateByID.Def, out var _))
		{
			return false;
		}
		int level = (int)buildingStateByID.Level;
		return TradingController.GetMaterialsCanBuy(buildingStateByID.Def).Total - level * 2 > 0;
	}

	private BuildingFunctionalityAction? GetInnBuyAction(BuildingTypeDef def, OutpostMaterialType material)
	{
		if (_view.SelectedCharacter == null)
		{
			return null;
		}
		BuildingState state = GetBuildingStateByID(def.ID);
		if (state == null || state.WreckedOrRebuilding || state.ConstructionState == BuildingConstructionType.NewBuilding)
		{
			return null;
		}
		CampaignEffect campaignEffect = GetApplyEffectsOfType(def.ID.ToBuildingID(), CampaignEffectType.BuyMaterial).FirstOrDefault();
		if (campaignEffect.Type == CampaignEffectType.None)
		{
			return null;
		}
		int num = campaignEffect.GetInt(0);
		int num2 = OutpostResources.AllMaterialTypes().Count() - state.BuyResources.Total;
		int num3 = num - num2;
		int materialAmount = TradingController.GetMaterialsCanBuy(state.Def).GetMaterialAmount(material);
		bool flag = !state.WreckedOrRebuilding && materialAmount > 0 && num > 0;
		bool flag2 = num3 == 0 && materialAmount == 0;
		return new BuildingFunctionalityAction
		{
			ActionText = string.Format("<sprite name=Gold>{0} <sprite name=ArrowShort> <sprite name={2}>{1}", TradingController.GetMaterialBuyGold(material), 1, FHUILocKeys.GetMaterialIconLocalizationKey(material)),
			Charges = num3,
			IsInteractable = (flag && TradingController.IsMaterialBuyingEnabled(material, state.Def) && _view.SelectedCharacter.Gold >= TradingController.GetMaterialBuyGold(material)),
			MaxCharges = num,
			OnClick = (flag ? ((BuildingFunctionalityActionHandler)((object _) =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.BuyMaterials, _view.SelectedCharacter.ID, new OutpostResources(material), state.Def);
			})) : null),
			TooltipText = (flag2 ? LocalizationManager.GetTranslation("GUI_weekly_limit_reached") : "")
		};
	}

	public IEnumerable<BuildingState> GetCurrentBuildingProjects()
	{
		return OutpostState.Buildings.Where((BuildingState b) => b.donationDistribution != null);
	}

	public bool IsCurrentConstructionProject(BuildingTypeID id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		return IsCurrentConstructionProject(buildingStateByID);
	}

	public bool IsCurrentConstructionProject(BuildingState building)
	{
		if (building == null)
		{
			return false;
		}
		return building.ConstructionState != BuildingConstructionType.None;
	}

	public bool IsWaitingForDonation(BuildingTypeID id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		return IsWaitingForDonation(buildingStateByID);
	}

	public bool IsFreeDonation(BuildingTypeID id)
	{
		return GetBuildingStateByID(id)?.donationDistribution.MaterialCost.IsEmpty ?? false;
	}

	public bool IsWaitingForDonation(BuildingState building)
	{
		return building?.IsWaitingForDonations ?? false;
	}

	public bool GetIsDonationComplete(BuildingState building)
	{
		if (building.IsWaitingForDonations)
		{
			return BuildingsController.GetIsDonationComplete(building);
		}
		return false;
	}

	public bool IsExtraConstruction(BuildingState building)
	{
		if ((building == null || !building.WreckedOrRebuilding) && BuildingsController.IsConstructionAllowed && BuildingsController.GetConstructionsRemaining() == 0)
		{
			return BuildingsController.GetExtraConstructionsRemaining() >= 1;
		}
		return false;
	}

	public IBuildingInfoStateParams GetBuildingStateParams(BuildingStateDTO building)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(building.Def.ID);
		if (buildingStateByID == null || !building.IsConstructed || building.ConstructionState == BuildingConstructionType.NewBuilding)
		{
			string startProjectTooltip = string.Empty;
			string finalizeProjectTooltip = string.Empty;
			if (!_view.IsPartyLeader)
			{
				startProjectTooltip = (finalizeProjectTooltip = LocalizationManager.GetTranslation("GUI_building_party_leader_warning"));
			}
			if (building.ConstructionCost?.RequiredProsperityLevel > OutpostState.ProsperityLevel)
			{
				startProjectTooltip = string.Format(LocalizationManager.GetTranslation("GUI_prosperity_requirement"), building.ConstructionCost.Value.RequiredProsperityLevel);
			}
			if (OutpostState.ConstructTurnRemains == 0)
			{
				finalizeProjectTooltip = LocalizationManager.GetTranslation("GUI_building_limit_reached");
			}
			return new UIBuildingInfoConstruct.ConstructBuildingParams
			{
				OutpostState = GetOutpostStateDTO(),
				IsPartyLeader = _view.IsPartyLeader,
				DonationData = new BuildDonationActionData(StartConstructionProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonations),
				DonatedResources = GetDonatedResources(buildingStateByID),
				DonatedGold = GetDonatedGold(buildingStateByID),
				PartyResources = _view.GameState.PartyCharacters.Aggregate(default, (OutpostResources resources, CharacterState characterState) => resources += characterState.Resources),
				PartyGold = (uint)_view.GameState.PartyCharacters.Sum((CharacterState cc) => cc.Gold),
				CanStartProject = (IsBuildingUnlocked(building.Def.ID) && BuildingsController.IsConstructionAllowed && !IsWaitingForDonation(buildingStateByID)),
				StartProjectTooltip = startProjectTooltip,
				FinalizeProjectTooltip = finalizeProjectTooltip
			};
		}
		if (building.IsWrecked || building.ConstructionState == BuildingConstructionType.Rebuilding)
		{
			string startProjectTooltip2 = string.Empty;
			if (!_view.IsPartyLeader)
			{
				startProjectTooltip2 = LocalizationManager.GetTranslation("GUI_building_party_leader_warning");
			}
			return new UIBuildingInfoRebuild.RebuildBuildingParams
			{
				OutpostState = GetOutpostStateDTO(),
				IsPartyLeader = _view.IsPartyLeader,
				DonationData = new BuildDonationActionData(StartRebuildProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonations),
				DonatedResources = GetDonatedResources(buildingStateByID),
				DonatedGold = GetDonatedGold(buildingStateByID),
				PartyResources = _view.GameState.PartyCharacters.Aggregate(default, (OutpostResources resources, CharacterState characterState) => resources += characterState.Resources),
				PartyGold = (uint)_view.GameState.PartyCharacters.Sum((CharacterState cc) => cc.Gold),
				CanStartProject = (BuildingsController.IsConstructionAllowed && !IsWaitingForDonation(buildingStateByID)),
				StartProjectTooltip = startProjectTooltip2
			};
		}
		string startProjectTooltip3 = string.Empty;
		string finalizeProjectTooltip2 = string.Empty;
		if (!_view.IsPartyLeader)
		{
			startProjectTooltip3 = (finalizeProjectTooltip2 = LocalizationManager.GetTranslation("GUI_building_party_leader_warning"));
		}
		if (building.UpgradeCost?.RequiredProsperityLevel > OutpostState.ProsperityLevel)
		{
			startProjectTooltip3 = string.Format(LocalizationManager.GetTranslation("GUI_prosperity_requirement"), building.UpgradeCost.Value.RequiredProsperityLevel);
		}
		if (OutpostState.ConstructTurnRemains == 0)
		{
			finalizeProjectTooltip2 = LocalizationManager.GetTranslation("GUI_building_limit_reached");
		}
		return new UIBuildingInfoUpgrade.UpgradeBuildingParams
		{
			DonationData = new BuildDonationActionData(StartUpgradeProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonations),
			OutpostState = GetOutpostStateDTO(),
			IsPartyLeader = _view.IsPartyLeader,
			DonatedResources = GetDonatedResources(buildingStateByID),
			DonatedGold = GetDonatedGold(buildingStateByID),
			PartyResources = _view.GameState.PartyCharacters.Aggregate(default, (OutpostResources resources, CharacterState characterState) => resources += characterState.Resources),
			PartyGold = (uint)_view.GameState.PartyCharacters.Sum((CharacterState cc) => cc.Gold),
			CanStartProject = (BuildingsController.IsConstructionAllowed && !IsWaitingForDonation(buildingStateByID)),
			StartProjectTooltip = startProjectTooltip3,
			FinalizeProjectTooltip = finalizeProjectTooltip2
		};
	}

	private bool IsSingleButtonFunctionalityBuilding(BuildingStateDTO building)
	{
		BuildingTypeDef def = building.Def;
		if (def is CraftingTable_BuildingTypeDef || def is Craftsman_BuildingTypeDef || def is TradingPost_BuildingTypeDef || def is Enhancer_BuildingTypeDef || def is Alchemist_BuildingTypeDef)
		{
			return true;
		}
		return false;
	}

	private void ShowBuildingScreen(BuildingTypeID id)
	{
		FHMapViewState buildingScreen = GetBuildingScreen(id);
		if (buildingScreen != null)
		{
			_view.PushState(buildingScreen);
		}
	}

	private void ShowBuildingScreen(BuildingTypeDef def)
	{
		ShowBuildingScreen(def.ID);
	}

	public void ShowDonationScreen(BuildingTypeID id)
	{
		if (!IsWaitingForDonation(id))
		{
			HideDonationModal();
		}
		else if (_view.CurrentModalState is BuildingDonationDistributionViewState state)
		{
			_view.TryPopState(state);
		}
		else
		{
			_view.PushState(new BuildingDonationDistributionViewState(GetBuildingStateByID(id).Def));
		}
	}

	private FHMapViewState GetBuildingScreen(BuildingTypeDef def)
	{
		if (!(def is Craftsman_BuildingTypeDef))
		{
			if (!(def is CraftingTable_BuildingTypeDef))
			{
				if (!(def is Alchemist_BuildingTypeDef))
				{
					if (!(def is Enhancer_BuildingTypeDef))
					{
						if (!(def is TradingPost_BuildingTypeDef))
						{
							if (!(def is MiningCamp_BuildingTypeDef))
							{
								if (!(def is LoggingCamp_BuildingTypeDef))
								{
									if (!(def is HuntingLodge_BuildingTypeDef))
									{
										if (!(def is MetalDepot_BuildingTypeDef))
										{
											if (!(def is LumberDepot_BuildingTypeDef))
											{
												if (!(def is HideDepot_BuildingTypeDef))
												{
													if (!(def is Library_BuildingTypeDef))
													{
														if (def is TempleOfTheGreatOak_BuildingTypeDef)
														{
															return GetTempleBuildingViewState();
														}
														return null;
													}
													return GetLibraryViewState();
												}
												return new ShopViewState();
											}
											return new ShopViewState();
										}
										return new ShopViewState();
									}
									return new ShopViewState();
								}
								return new ShopViewState();
							}
							return new ShopViewState();
						}
						return new ShopViewState();
					}
					return new EnhancerViewState();
				}
				return new AlchemistViewState();
			}
			return new CraftingViewState();
		}
		return new CraftingViewState();
	}

	private FHMapViewState GetBuildingScreen(BuildingTypeID id)
	{
		return GetBuildingScreen(DefRepo.Buildings[id]);
	}

	private FHMapViewState GetLibraryViewState()
	{
		if (_view?.SelectedCharacter == null)
		{
			return null;
		}
		bool readOnly = !_view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter);
		return new UnlockCardsViewState(_view.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(_view.SelectedCharacter), readOnly);
	}

	private FHMapViewState GetTempleBuildingViewState()
	{
		CharacterState characterState = _view?.SelectedCharacter;
		if (characterState == null)
		{
			return null;
		}
		if (!GameController.OutpostController.CanBlessCharacter(characterState))
		{
			return null;
		}
		List<GenericListModalViewState.ListItem> items = new List<GenericListModalViewState.ListItem>
		{
			new GenericListModalViewState.ListItem
			{
				Enabled = GameController.OutpostController.CanBlessCharacter(characterState),
				Text = "Bless " + characterState.Name,
				Item = characterState.ID
			}
		};
		return new GenericListModalViewState("Select character", items, (GenericListModalViewState.ListItem result) =>
		{
			if (result != null)
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.BlessCharacter, result.Item);
			}
		});
	}

	private BuildingDonationDTO? GetBuildingDonationData(BuildingTypeID id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (buildingStateByID == null || !buildingStateByID.IsWaitingForDonations)
		{
			return null;
		}
		return new BuildingDonationDTO
		{
			Character = _view.SelectedCharacter.ID,
			Gold = _view.SelectedCharacter.Gold,
			Inspiration = _view.GameState.Inspiration,
			CurrentResources = _view.SelectedCharacter.Resources,
			Distribution = buildingStateByID.donationDistribution,
			IsPartyLeader = _view.IsPartyLeader,
			IsDone = BuildingsController.GetIsDonationComplete(buildingStateByID)
		};
	}

	public bool DonateMaterials(BuildingTypeID id, CharacterID characterID, OutpostResources resources)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (!CharacterController.TryGetCharacter(characterID, out var character))
		{
			return false;
		}
		if (!BuildingsController.CanDonateMaterials(resources + buildingStateByID.donationDistribution.TotalMaterialsDonated, buildingStateByID))
		{
			return false;
		}
		OutpostResources outpostResources = BuildingsController.PrepAddMaterialsForDonation(buildingStateByID, characterID, resources);
		Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.DonateMaterialsToBuilding, buildingStateByID.Def, outpostResources, character.ID);
		return true;
	}

	public bool DonateGold(BuildingTypeID id, CharacterID characterID, int gold)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (!CharacterController.TryGetCharacter(characterID, out var character))
		{
			return false;
		}
		if (!BuildingsController.CanDonateGold(gold + buildingStateByID.donationDistribution.TotalGoldDonated, buildingStateByID))
		{
			return false;
		}
		int num = BuildingsController.PrepAddGoldForDonation(buildingStateByID, characterID, gold);
		Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.DonateGoldToBuilding, buildingStateByID.Def, num, character.ID);
		return true;
	}

	public bool RefundGold(BuildingTypeID id, CharacterID characterID, int delta)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		int num = BuildingsController.PrepDelGoldForDonation(buildingStateByID, characterID, delta);
		Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.RefundGoldFromBuilding, buildingStateByID.Def, num, characterID);
		return true;
	}

	public bool RefundMaterials(BuildingTypeID id, CharacterID characterID, OutpostResources delta)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		OutpostResources outpostResources = BuildingsController.PrepDelMaterialsForDonation(buildingStateByID, characterID, delta);
		Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.RefundMaterialsFromBuilding, buildingStateByID.Def, outpostResources, characterID);
		return true;
	}

	public int GetDonatedResource(BuildingTypeID id, CharacterID characterID, OutpostMaterialType resource)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		CharacterState character = _view.GameState.AllCharacters.FirstOrDefault((CharacterState characterState) => characterState.ID == characterID);
		switch (resource)
		{
		case OutpostMaterialType.Lumber:
			return BuildingsController.GetDonatedMaterialsSoFar(buildingStateByID, character).Lumber;
		case OutpostMaterialType.Metal:
			return BuildingsController.GetDonatedMaterialsSoFar(buildingStateByID, character).Metal;
		case OutpostMaterialType.Hide:
			return BuildingsController.GetDonatedMaterialsSoFar(buildingStateByID, character).Hide;
		case OutpostMaterialType.None:
		case OutpostMaterialType.All:
		case OutpostMaterialType.Any:
			return 0;
		default:
			throw new ArgumentOutOfRangeException("resource", resource, null);
		}
	}

	private void StartConstructionProject(BuildingTypeID id)
	{
		BuildingTypeDef buildingTypeDef = DefRepo.Buildings[id];
		if (_view.IsPartyLeader && BuildingsController.CanBuild(buildingTypeDef))
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.BuildBuilding, buildingTypeDef, null);
		}
	}

	private void StartUpgradeProject(BuildingTypeID id)
	{
		if (_view.IsPartyLeader)
		{
			BuildingState buildingStateByID = GetBuildingStateByID(id);
			if (BuildingsController.CanUpgrade(buildingStateByID))
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.UpgradeBuilding, buildingStateByID.Def, null);
			}
		}
	}

	private void StartRebuildProject(BuildingTypeID id)
	{
		if (_view.IsPartyLeader)
		{
			BuildingState buildingStateByID = GetBuildingStateByID(id);
			if (buildingStateByID.Wrecked && OutpostState.Phase == OutpostPhase.Downtime)
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.Rebuild, buildingStateByID.Def, null);
			}
		}
	}

	private void FinalizeDonations(BuildingTypeID id)
	{
		if (!_view.IsPartyLeader)
		{
			return;
		}
		BuildingState state = GetBuildingStateByID(id);
		if (state == null || !BuildingsController.GetIsDonationComplete(state) || !BuildingsController.CanFinalizeDonation(state))
		{
			return;
		}
		uint constructionsRemaining = BuildingsController.GetConstructionsRemaining();
		uint extraConstructionsRemaining = BuildingsController.GetExtraConstructionsRemaining();
		if (constructionsRemaining == 0 && extraConstructionsRemaining == 0 && !state.WreckedOrRebuilding)
		{
			return;
		}
		if (constructionsRemaining == 0 && !state.WreckedOrRebuilding)
		{
			uint constructionExtraMoraleCost = BuildingsController.GetConstructionExtraMoraleCost();
			GenericModalViewState state2 = new GenericModalViewState(string.Format(LocalizationManager.GetTranslation("GUI_SECOND_BUILD_CONFIRMATION"), constructionExtraMoraleCost), LocalizationManager.GetTranslation("GUI_SECOND_BUILD_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRM"), (bool confirm) =>
			{
				if (confirm)
				{
					Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.FinalizeBuildingDonations, state.Def);
					AudioControllerUtils.PlaySound(UIInfoTools.Instance?.startBuildingAudioItem);
				}
			});
			_view.PushState(state2);
		}
		else
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.FinalizeBuildingDonations, state.Def);
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.startBuildingAudioItem);
		}
	}

	private void CancelBuildingProject(BuildingTypeID id)
	{
		if (_view.IsPartyLeader)
		{
			BuildingState buildingStateByID = GetBuildingStateByID(id);
			if (IsWaitingForDonation(id))
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.CancelBuildingConstruction, buildingStateByID.Def);
			}
		}
	}

	public OutpostResources? GetDonatedResources(BuildingState state)
	{
		if (!IsCurrentConstructionProject(state))
		{
			return null;
		}
		return BuildingsController.GetDonatedMaterialsSoFar(state);
	}

	public uint? GetDonatedGold(BuildingState state)
	{
		if (!IsCurrentConstructionProject(state))
		{
			return null;
		}
		return (uint)BuildingsController.GetDonatedGoldSoFar(state);
	}

	public IEnumerable<CampaignEffect> GetActiveEffects(BuildingId id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (buildingStateByID == null)
		{
			return GetActiveEffects(GetBuildingDef(id.ToBuildingTypeID()));
		}
		return buildingStateByID.Def.GetActiveEffects(buildingStateByID.Level, buildingStateByID.WreckedOrRebuilding, OutpostState.Phase, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetActiveEffects(BuildingTypeDef def, uint level = 1u)
	{
		return def.GetActiveEffects(level, wrecked: false, OutpostState.Phase, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetActiveEffectsOfType(BuildingId id, CampaignEffectType effectType)
	{
		return from effect in GetActiveEffects(id)
			where effect.Type == effectType
			select effect;
	}

	public IEnumerable<CampaignEffect> GetActiveEffectsOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return from effect in GetActiveEffects(def, level)
			where effect.Type == effectType
			select effect;
	}

	public CampaignEffect GetActiveEffectOfType(BuildingId id, CampaignEffectType effectType)
	{
		return GetApplyEffects(id).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public CampaignEffect GetActiveEffectOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return GetActiveEffects(def, level).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public IEnumerable<CampaignEffect> GetApplyEffects(BuildingId id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (buildingStateByID == null)
		{
			return GetApplyEffects(GetBuildingDef(id.ToBuildingTypeID()));
		}
		uint level = ((buildingStateByID.ConstructionState == BuildingConstructionType.Upgrading) ? (buildingStateByID.Level - 1) : buildingStateByID.Level);
		return buildingStateByID.Def.GetApplyEffects(level, buildingStateByID.WreckedOrRebuilding, OutpostState.Phase, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetApplyEffects(BuildingTypeDef def, uint level = 1u)
	{
		return def.GetApplyEffects(level, wrecked: false, OutpostState.Phase, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetApplyEffectsOfType(BuildingId id, CampaignEffectType effectType)
	{
		return from effect in GetApplyEffects(id)
			where effect.Type == effectType
			select effect;
	}

	public CampaignEffect GetApplyEffectOfType(BuildingId id, CampaignEffectType effectType)
	{
		return GetApplyEffects(id).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public IEnumerable<CampaignEffect> GetApplyEffectsOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return from effect in GetApplyEffects(def, level)
			where effect.Type == effectType
			select effect;
	}

	public CampaignEffect GetApplyEffectOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return GetApplyEffects(def, level).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public IEnumerable<CampaignEffect> GetOnConstructEffects(BuildingId id)
	{
		BuildingState buildingStateByID = GetBuildingStateByID(id);
		if (buildingStateByID == null)
		{
			return GetOnConstructEffects(GetBuildingDef(id.ToBuildingTypeID()));
		}
		uint level = ((buildingStateByID.ConstructionState == BuildingConstructionType.Upgrading) ? (buildingStateByID.Level - 1) : buildingStateByID.Level);
		return buildingStateByID.Def.GetOnConstructEffects(level, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetOnConstructEffects(BuildingTypeDef def, uint level = 1u)
	{
		return def.GetOnConstructEffects(level, CampaignController);
	}

	public IEnumerable<CampaignEffect> GetOnConstructEffectsOfType(BuildingId id, CampaignEffectType effectType)
	{
		return from effect in GetOnConstructEffects(id)
			where effect.Type == effectType
			select effect;
	}

	public IEnumerable<CampaignEffect> GetOnConstructEffectsOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return from effect in GetOnConstructEffects(def, level)
			where effect.Type == effectType
			select effect;
	}

	public CampaignEffect GetOnConstructEffectOfType(BuildingId id, CampaignEffectType effectType)
	{
		return GetOnConstructEffects(id).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public CampaignEffect GetOnConstructEffectOfType(BuildingTypeDef def, CampaignEffectType effectType, uint level = 1u)
	{
		return GetOnConstructEffects(def, level).FirstOrDefault((CampaignEffect effect) => effect.Type == effectType);
	}

	public bool HasEmptyGardenPlots()
	{
		if (GetBuildingByType<Garden_BuildingTypeDef>() == null)
		{
			return false;
		}
		if (OutpostState?.Garden == null)
		{
			return false;
		}
		return OutpostState.Garden.Plots.Any((OutpostHerbType type) => type == OutpostHerbType.None);
	}

	public int GetMaxPets()
	{
		BuildingState buildingByType = GetBuildingByType<Stables_BuildingTypeDef>();
		return buildingByType?.Def.GetActiveEffects(buildingByType.Level, wrecked: false, OutpostPhase.Downtime, GameController).Where((CampaignEffect x) => x.Type == CampaignEffectType.PetCapacity).Sum((CampaignEffect x) => x.GetInt(0)) ?? 0;
	}

	public void HideDonationModal()
	{
		if (_view.CurrentModalState is BuildingDonationDistributionViewState state)
		{
			_view.TryPopState(state);
		}
	}
}
