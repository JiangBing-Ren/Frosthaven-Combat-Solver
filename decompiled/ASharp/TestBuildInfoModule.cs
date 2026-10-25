using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Elements.Buildings;
using FHGUI.Modules;
using FHGUI.Testing;
using FHRL;
using FHRL.Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestBuildInfoModule : MonoBehaviour
{
	[Serializable]
	public class TestDonationData
	{
		public OutpostResources DonatedResources;

		public int DonatedGold;

		public int DonatedMorale;
	}

	[Serializable]
	public class TestBuildingData
	{
		public BuildingId BuildingId;

		public bool IsWrecked;

		public bool IsConstructed;

		public bool IsPartyLeader;

		public BuildingConstructionType ConstructionType;

		public uint Level;

		public bool HasDonation;
	}

	[Serializable]
	public class TestPartyData
	{
		public OutpostResources TotalPartyResources;

		public uint Gold;
	}

	public BuildingInfoModule Info;

	public TMP_Dropdown BuildingTypes;

	public Button TestButton;

	public OutpostBuildingDataSO Data;

	public BuildingId BuildingId;

	[Header("Params")]
	public InputField BuildingLevel;

	public Toggle IsPartyLeader;

	public TMP_Dropdown ConstructionType;

	public Toggle IsConstructed;

	[Header("Donations")]
	public InputField DonatedGold;

	public InputField DonatedMetal;

	public InputField DonatedLumber;

	public InputField DonatedHide;

	private DefRepository _defRepository;

	private BuildingStateDTO _building;

	private BuildingState _buildingState;

	public UITestPreviewObjectHelper PreviewObject;

	public TestDonationData DonationData;

	public TestBuildingData BuildingData;

	public TestPartyData PartyData;

	private void Awake()
	{
		_defRepository = new DefRepository();
		BuildingTypeDef.LoadFromAssembly(_defRepository);
		BuildingId[] buildingIds = (BuildingId[])typeof(BuildingId).GetEnumValues();
		List<string> options = buildingIds.Select((BuildingId e) => e.ToString()).ToList();
		BuildingTypes.ClearOptions();
		BuildingTypes.AddOptions(options);
		TestButton.onClick.AddListener(Test);
		BuildingTypes.onValueChanged.AddListener((int arg0) =>
		{
			BuildingId = buildingIds[arg0];
		});
	}

	private void Test()
	{
		BuildingTypeDef value = _defRepository.Buildings.FirstOrDefault((KeyValuePair<BuildingTypeID, BuildingTypeDef> b) => b.Key.ID == (uint)BuildingId).Value;
		_buildingState = GetBuildingState(value);
		_building = GetBuildingDTO(_buildingState);
		Info.ShowInfo(new BuildingInfoModule.BuildingInfoParams
		{
			Building = _building,
			InfoParams = GetBuildingInfoParams(value),
			StateParams = GetBuildingStateParams(_buildingState, _building)
		});
	}

	private BuildingStateDTO GetBuildingDTO(BuildingState state)
	{
		BuildingTypeDef def = state.Def;
		return new BuildingStateDTO
		{
			Def = def,
			Icon = Data.GetBuildingIcon(def.ID),
			IconTintable = Data.GetBuildingIconTimetable(def.ID),
			Level = state.Level,
			NameLocKey = Data.GetBuildingName(def.ID),
			DescriptionLocKey = Data.GetBuildingDescription(def.ID),
			ConstructionState = BuildingData.ConstructionType,
			CanUpgrade = false,
			AvailableProsperity = 10u,
			IsConstructed = (_buildingState.donationDistribution != null || (bool)IsConstructed),
			ConstructionCost = new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = def.BuildGoldCost,
					Resources = def.BuildResourceCost,
					Morale = def.BuildMoraleCost
				},
				RequiredProsperityLevel = def.GetProsperityRequirement((int)state.Level)
			},
			UpgradeCost = new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = def.GetUpgradeGoldCost((int)state.Level),
					Resources = def.GetUpgradeResourceCost((int)state.Level),
					Morale = def.GetUpgradeMoraleCost((int)state.Level)
				},
				RequiredProsperityLevel = def.GetProsperityRequirement((int)state.Level)
			},
			RebuildCost = new BuildingStateDTO.Cost
			{
				Spendable = new BuildingTypeDef.Cost
				{
					Gold = 0u,
					Resources = def.GetRebuildCost(state.Level),
					Morale = 0u
				},
				RequiredProsperityLevel = def.GetProsperityRequirement((int)state.Level)
			},
			HasStartedProject = (state.donationDistribution != null),
			IsDonationComplete = GetIsDonationComplete(state),
			CanFinalizeDonation = true,
			IsWrecked = state.Wrecked
		};
	}

	private BuildingState GetBuildingState(BuildingTypeDef def)
	{
		return new BuildingState(def, _defRepository, BuildingData.Level, BuildingData.IsWrecked)
		{
			ConstructionState = BuildingData.ConstructionType,
			Level = BuildingData.Level,
			donationDistribution = (BuildingData.HasDonation ? new BuildingMaterialDistribution
			{
				Morale = (uint)DonationData.DonatedMorale,
				GoldCost = DonationData.DonatedGold,
				MaterialCost = DonationData.DonatedResources,
				ConstructionPerTurn = 2u,
				ExtraConstructionPerTurn = 1u
			} : null)
		};
	}

	private IBuildingInfoParams GetBuildingInfoParams(BuildingTypeDef building)
	{
		if (!(building is Garden_BuildingTypeDef))
		{
			if (!(building is Barrack_BuildingTypeDef))
			{
				if (!(building is Stables_BuildingTypeDef))
				{
					if (building is Workshop_BuildingTypeDef)
					{
						return new WorkshopBuildingParams
						{
							HasBoat = true,
							HasClimbingGear = false,
							HasSled = true
						};
					}
					if (!(building is Boat_BuildingTypeDef) && !(building is ClimbingGear_BuildingTypeDef) && !(building is Sled_BuildingTypeDef))
					{
						if (building is Walls_BuildingTypeDef)
						{
							return default(Walls_UIBuildInfoFunctionality.WallsData);
						}
						if (IsSingleButtonFunctionalityBuilding(building))
						{
							return new SingleActionBuildingParams
							{
								Action = () =>
								{
									Debug.Log("Showing building screen");
								},
								IsInteractable = true,
								ActionLocKey = "GUI_UseBuilding",
								ActionHintLocKey = "GUI_open_building_message"
							};
						}
						return new ResourceBuildingData
						{
							BuyAction = new UIBuildInfoResourceActionElement.ResourceBuildingActionData
							{
								LeftSideElement = new ResourceElement.ResourceActionElementData
								{
									Amount = 2,
									IsGold = true
								},
								RightSideElement = new ResourceElement.ResourceActionElementData
								{
									Amount = 1,
									Material = OutpostMaterialType.Hide,
									IsAvailable = true
								},
								Action = new BuildingFunctionalityAction
								{
									ActionText = string.Format("<sprite name=Gold>{0} <sprite name=ArrowShort> <sprite name={2}>{1}", 2, 1, FHUILocKeys.GetMaterialLocalizationKey(OutpostMaterialType.Hide)),
									Charges = 1,
									IsInteractable = true,
									MaxCharges = 2,
									OnClick = (object _) =>
									{
										Debug.Log("Buying resources");
									},
									TooltipText = ""
								}
							}
						};
					}
					return new WorkshopUpgradeBuildingParams
					{
						Icon = UIInfoTools.Instance.GetBuildingConfig(building).Icon,
						IsWorkshopWrecked = false
					};
				}
				return new Stables_UIBuildInfoFunctionality.StablesStateParams
				{
					Pets = new List<PetDTO>(),
					MaxPetCapacity = 12,
					PetCapacity = 4
				};
			}
			return new BarracksStateParams
			{
				AvailableSoldersToTrain = 2u,
				SoldierCount = 2u,
				MaxSoldierCount = 3u,
				CanTrainSoldier = true,
				IsPartyLeader = BuildingData.IsPartyLeader,
				TrainAction = new BuildingFunctionalityAction
				{
					Charges = 0,
					MaxCharges = 0,
					ActionText = $"<sprite name=Gold>{1} <sprite name={FHUILocKeys.GetMaterialTMPIcon(OutpostMaterialType.Metal)}> {2} <sprite name=ArrowShort> <sprite name=Soldiers>{1}",
					IsInteractable = true,
					OnClick = (object _) =>
					{
						Debug.Log("Training soldiers!");
					},
					TooltipText = ""
				}
			};
		}
		GardenStateParams gardenStateParams = new GardenStateParams();
		gardenStateParams.Plots = new GardenStateParams.GardenPlotData[3]
		{
			new GardenStateParams.GardenPlotData
			{
				Herb = OutpostHerbType.Arrowvine,
				Unlocked = true,
				UnlockLevel = 1u
			},
			new GardenStateParams.GardenPlotData
			{
				Herb = OutpostHerbType.Axenut,
				Unlocked = true,
				UnlockLevel = 2u
			},
			new GardenStateParams.GardenPlotData
			{
				Herb = OutpostHerbType.None,
				Unlocked = false,
				UnlockLevel = 3u
			}
		};
		gardenStateParams.PlantState = GardenState.PlantStateType.Collect;
		gardenStateParams.IsPartyLeader = BuildingData.IsPartyLeader;
		gardenStateParams.Plant = (OutpostHerbType[] herbs) =>
		{
			Debug.Log("Planting");
		};
		gardenStateParams.Resources = default;
		return gardenStateParams;
	}

	private OutpostStateDTO GetOutpostStateDTO()
	{
		return new OutpostStateDTO
		{
			Gold = 0u,
			Resources = default,
			Inspiration = 0u,
			Morale = 0,
			Prosperity = 0u,
			TownLevel = 0u
		};
	}

	public IBuildingInfoStateParams GetBuildingStateParams(BuildingState state, BuildingStateDTO building)
	{
		if (state.Wrecked || state.ConstructionState == BuildingConstructionType.Rebuilding)
		{
			return new UIBuildingInfoRebuild.RebuildBuildingParams
			{
				OutpostState = GetOutpostStateDTO(),
				IsPartyLeader = BuildingData.IsPartyLeader,
				DonationData = new BuildDonationActionData(StartRebuildProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonation),
				DonatedResources = GetDonatedResources(state),
				DonatedGold = GetDonatedGold(state),
				PartyResources = PartyData.TotalPartyResources,
				CanStartProject = true
			};
		}
		if (!building.IsConstructed || state.ConstructionState == BuildingConstructionType.NewBuilding)
		{
			return new UIBuildingInfoConstruct.ConstructBuildingParams
			{
				OutpostState = GetOutpostStateDTO(),
				IsPartyLeader = BuildingData.IsPartyLeader,
				DonationData = new BuildDonationActionData(StartConstructionProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonation),
				DonatedResources = GetDonatedResources(state),
				DonatedGold = GetDonatedGold(state),
				PartyResources = PartyData.TotalPartyResources,
				PartyGold = PartyData.Gold,
				CanStartProject = true
			};
		}
		if (building.HasMoreUpgrades || building.ConstructionState == BuildingConstructionType.Upgrading)
		{
			return new UIBuildingInfoUpgrade.UpgradeBuildingParams
			{
				OutpostState = GetOutpostStateDTO(),
				IsPartyLeader = BuildingData.IsPartyLeader,
				DonationData = new BuildDonationActionData(StartUpgradeProject, CancelBuildingProject, ShowDonationScreen, FinalizeDonation),
				DonatedResources = GetDonatedResources(state),
				PartyResources = PartyData.TotalPartyResources,
				DonatedGold = GetDonatedGold(state),
				CanStartProject = true
			};
		}
		return null;
	}

	private void Show()
	{
		_building = GetBuildingDTO(_buildingState);
		Info.ShowInfo(new BuildingInfoModule.BuildingInfoParams
		{
			Building = _building,
			InfoParams = null,
			StateParams = GetBuildingStateParams(_buildingState, _building)
		});
	}

	public void StartConstructionProject(BuildingTypeID id)
	{
		_buildingState.donationDistribution = new BuildingMaterialDistribution
		{
			Morale = (uint)DonationData.DonatedMorale,
			GoldCost = DonationData.DonatedGold,
			MaterialCost = DonationData.DonatedResources,
			ConstructionPerTurn = 2u,
			ExtraConstructionPerTurn = 1u
		};
		_buildingState.Level = 1u;
		BuildingData.ConstructionType = BuildingConstructionType.NewBuilding;
		_building = GetBuildingDTO(_buildingState);
		Show();
		Debug.Log("StartConstructionProject");
	}

	private void StartUpgradeProject(BuildingTypeID id)
	{
		_buildingState.donationDistribution = new BuildingMaterialDistribution
		{
			Morale = (uint)DonationData.DonatedMorale,
			GoldCost = DonationData.DonatedGold,
			MaterialCost = DonationData.DonatedResources,
			ConstructionPerTurn = 2u,
			ExtraConstructionPerTurn = 1u
		};
		BuildingData.ConstructionType = BuildingConstructionType.Upgrading;
		_building = GetBuildingDTO(_buildingState);
		Show();
		Debug.Log("StartUpgradeProject");
	}

	private void StartRebuildProject(BuildingTypeID id)
	{
		_buildingState.donationDistribution = new BuildingMaterialDistribution
		{
			Morale = (uint)DonationData.DonatedMorale,
			GoldCost = DonationData.DonatedGold,
			MaterialCost = DonationData.DonatedResources,
			ConstructionPerTurn = 2u,
			ExtraConstructionPerTurn = 1u
		};
		BuildingData.ConstructionType = BuildingConstructionType.Rebuilding;
		_building = GetBuildingDTO(_buildingState);
		Show();
		Debug.Log("StartRebuildProject");
	}

	private bool IsWaitingForDonation(BuildingState state)
	{
		return state.IsWaitingForDonations;
	}

	private uint? GetDonatedGold(BuildingState state)
	{
		if (!state.IsWaitingForDonations)
		{
			return null;
		}
		return (uint)state.donationDistribution.TotalGoldDonated;
	}

	private OutpostResources? GetDonatedResources(BuildingState state)
	{
		return state.donationDistribution?.TotalMaterialsDonated;
	}

	private void CancelBuildingProject(BuildingTypeID id)
	{
		_buildingState.donationDistribution = null;
		Show();
		Debug.Log("CancelBuildingProject");
	}

	private void ShowDonationScreen(BuildingTypeID id)
	{
		_buildingState.donationDistribution = null;
		_buildingState.ConstructionState = BuildingConstructionType.None;
		_buildingState.Level = 1u;
		_building = GetBuildingDTO(_buildingState);
		Debug.Log("ShowDonationScreen");
		Show();
	}

	private void FinalizeDonation(BuildingTypeID id)
	{
		Debug.Log("FinalizeDonation");
		Show();
	}

	private bool IsSingleButtonFunctionalityBuilding(BuildingTypeDef building)
	{
		if (building is Alchemist_BuildingTypeDef || building is Craftsman_BuildingTypeDef || building is TradingPost_BuildingTypeDef || building is Enhancer_BuildingTypeDef)
		{
			return true;
		}
		return false;
	}

	private bool GetIsDonationComplete(BuildingState building)
	{
		if (!building.IsWaitingForDonations)
		{
			return false;
		}
		OutpostResources totalMaterialsDonated = building.donationDistribution.TotalMaterialsDonated;
		OutpostResources materialCost = building.donationDistribution.MaterialCost;
		if (materialCost.TotalMaterials - totalMaterialsDonated.TotalMaterials <= 0)
		{
			return building.donationDistribution.TotalGoldDonated >= building.donationDistribution.GoldCost;
		}
		return false;
	}
}
