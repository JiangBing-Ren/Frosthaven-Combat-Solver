using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.Services;

public class ResourceAllocationService : FHMapViewService
{
	private readonly CharacterDetailsViewService _characterDetailsService;

	public OutpostResources GetTownResources => GameController.State.Outpost.Resources;

	private ItemViewService _itemService => _view.GetService<ItemViewService>();

	public ResourceAllocationService(FHMapView view, CharacterDetailsViewService characterDetailsService)
		: base(view)
	{
		_characterDetailsService = characterDetailsService;
	}

	public void OnResourceDistributionChanged(ResourceDistribution distribution)
	{
		if (distribution.CharacterID == null)
		{
			_view.Choreographer.ExecuteCommand("UpdateFrosthaveResourceDistribution", distribution.Resources);
		}
		else
		{
			_view.Choreographer.ExecuteCommand("UpdateResourceDistribution", distribution.CharacterID, distribution.Resources);
		}
	}

	public void OnBuildingDistributionChanged(BuildingTypeDistribution distribution)
	{
		_view.Choreographer.ExecuteCommand("UpdateFrosthaveBuildingDistribution", distribution.BuildingTypeIDs);
	}

	public void OnPetDistributionChanged(PetDistribution distribution)
	{
		_view.Choreographer.ExecuteCommand("UpdateFrosthavenPetDistribution", distribution.PetIDs);
	}

	public void OnGoldDistributionChanged(GoldDistribution distribution)
	{
		if (distribution.CharacterID == null)
		{
			_view.Choreographer.ExecuteCommand("UpdateFrosthavenGoldDistribution", distribution.Gold);
		}
		else
		{
			_view.Choreographer.ExecuteCommand("UpdateGoldDistribution", distribution.CharacterID, distribution.Gold);
		}
	}

	public void OnItemDistributionChanged(CharacterItemDistribution distribution)
	{
		_view.Choreographer.ExecuteCommand("UpdateItemsDistribution", distribution.CharacterID, distribution.Item);
	}

	public DistributionsDTO GetBuildingRepairDistribution(BuildingState buildingState, out OutpostResourceChange change)
	{
		if (buildingState.Damaged)
		{
			change = new OutpostResourceChange
			{
				AnyMaterial = (int)GameController.BuildingsController.GetRepairMaterialCost(buildingState)
			};
		}
		else
		{
			change = default;
		}
		return new DistributionsDTO
		{
			BuildingMaterial = _view.GameState.GenerateDefaultBuildingMaterialDistribution(0, change.Resources, _view.IsPartyLeader)
		};
	}

	public DistributionsDTO PopulateDistributionLegacy(DistributionModalModule DistributionModalModule, DistributionChangeDTO change, PlayerDTO playerDTO)
	{
		DistributionsDTO distributions = GetDistributions(change);
		if (change.Gold != 0)
		{
			DistributionModalModule.Init(change, distributions.Gold, _view.GameState, playerDTO);
		}
		if (change.Resources.AnyMaterial > 0 || change.Resources.Resources.HasMaterials)
		{
			DistributionModalModule.Init(change, distributions.Resources, _view.GameState, playerDTO);
		}
		else if (change.Resources.AnyHerb > 0)
		{
			DistributionModalModule.Init(change, distributions.Resources, _view.GameState, playerDTO);
		}
		if (change.Items != null || change.LoseItem > 0)
		{
			DistributionModalModule.Init(change, distributions.Items, GameController, playerDTO);
		}
		return distributions;
	}

	public DistributionsDTO GetDistributions(DistributionChangeDTO change)
	{
		DistributionsDTO distributionsDTO = new DistributionsDTO();
		if (change.Gold != 0)
		{
			distributionsDTO.Gold = _view.MultiplayerGameController.GenerateGoldDistribution();
		}
		if (change.Resources.AnyMaterial > 0 || change.Resources.Resources.HasMaterials)
		{
			if (distributionsDTO.Resources == null)
			{
				distributionsDTO.Resources = _view.MultiplayerGameController.GenerateMaterialDistribution(change.Resources);
			}
			else
			{
				distributionsDTO.Resources.AddRange(_view.MultiplayerGameController.GenerateMaterialDistribution(change.Resources));
			}
		}
		if (change.Resources.AnyHerb > 0)
		{
			if (distributionsDTO.Resources == null)
			{
				distributionsDTO.Resources = _view.MultiplayerGameController.GenerateHerbDistribution(change.Resources);
			}
			else
			{
				distributionsDTO.Resources.AddRange(_view.MultiplayerGameController.GenerateHerbDistribution(change.Resources));
			}
		}
		if (change.Items != null || change.LoseItem > 0)
		{
			distributionsDTO.Items = _view.MultiplayerGameController.GenerateItemDistribution();
		}
		return distributionsDTO;
	}

	public void PopulateEventDistributionLegacy(DistributionModalModule DistributionModalModule, PlayerDTO playerDTO, CampaignUnresolvedAction _unresolvedAction, DistributionsDTO _distributions, CampaignState GameState)
	{
		switch (_unresolvedAction.Type)
		{
		case CampaignInteractionType.Event_LosePet:
			_distributions.PetDistribution = _view.MultiplayerGameController.GeneratePetDistribution();
			DistributionModalModule.Init_Event_LosePet(_distributions.PetDistribution, _view.GameController.PetController, playerDTO);
			break;
		case CampaignInteractionType.Event_SelectBuildingsToProtect:
			_distributions.BuildingType = _view.MultiplayerGameController.GenerateBuildingTypeDistribution();
			DistributionModalModule.Init_Event_SelectBuildingsToProtect(_distributions.BuildingType, _unresolvedAction.GetInt(0), GameState.Outpost, playerDTO);
			break;
		case CampaignInteractionType.Event_GainAnyMaterialsEach:
		{
			OutpostResourceChange materialsFor_GainAnyMaterialsEach = _view.GameController.OutpostEventController.GetMaterialsFor_GainAnyMaterialsEach();
			_distributions.Resources = _view.MultiplayerGameController.GenerateMaterialDistribution(materialsFor_GainAnyMaterialsEach);
			DistributionModalModule.Init_EventGainAnyMaterialEach(new DistributionChangeDTO
			{
				Resources = materialsFor_GainAnyMaterialsEach,
				ChangeType = DistributionChangeType.GainEach
			}, _distributions.Resources, GameState, playerDTO);
			break;
		}
		case CampaignInteractionType.Event_LoseAnySnowthistle:
			_distributions.Resources = _view.MultiplayerGameController.GenerateResourceDistribution(includeFrosthaven: true, includeCharacters: false);
			DistributionModalModule.Init_LoseAnySnowthistle(_distributions.Resources, GameState, playerDTO);
			break;
		case CampaignInteractionType.Event_TradeLumberForMetal:
			_distributions.Resources = _view.MultiplayerGameController.GenerateResourceDistribution(includeFrosthaven: false, includeCharacters: true);
			DistributionModalModule.Init_TradeLumberForMetal(_distributions.Resources, GameState, playerDTO);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseGold:
			_distributions.Gold = _view.MultiplayerGameController.GenerateGoldDistribution();
			DistributionModalModule.Init_EverybodyChooseLoseGold(_distributions.Gold, GameController.OutpostEventController.Event_EverybodyChooseLoseGold_Amount, GameState, playerDTO);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToGold:
			_distributions.Gold = _view.MultiplayerGameController.GenerateGoldDistribution();
			DistributionModalModule.Init_EverybodyChooseLoseUpToGold(_distributions.Gold, _unresolvedAction.GetInt(0), GameState, playerDTO);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToMaterials:
		{
			OutpostResourceChange materialFor_LoseUpToMaterials = _view.GameController.OutpostEventController.GetMaterialFor_LoseUpToMaterials();
			_distributions.Resources = _view.MultiplayerGameController.GenerateMaterialDistribution(materialFor_LoseUpToMaterials);
			DistributionChangeDTO change = new DistributionChangeDTO
			{
				Resources = materialFor_LoseUpToMaterials,
				ChangeType = DistributionChangeType.Lose
			};
			DistributionModalModule.Init_Event_EverybodyChooseLoseUpToMaterials(change, _distributions.Resources, GameState, playerDTO);
			break;
		}
		case CampaignInteractionType.Event_WR24OptionA:
			_distributions.Items = _view.MultiplayerGameController.GenerateItemDistribution_WR24();
			DistributionModalModule.Init_EventWR24OptionA(new DistributionChangeDTO
			{
				ChangeType = DistributionChangeType.Event_WR24OptionA_Lose,
				LoseItem = 1
			}, _distributions.Items, _view.GameController, playerDTO);
			break;
		default:
			throw new Exception($"Invalid action to handle distribution for:{_unresolvedAction.Type}");
		}
	}

	public void FinalizeDistribution(DistributionChangeDTO _change, DistributionsDTO _distributions)
	{
		if (_distributions.Gold != null)
		{
			if (_change.ChangeType == DistributionChangeType.Gain)
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveGainGold, _distributions.Gold);
			}
			else
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveLoseGold, _distributions.Gold);
			}
		}
		if (_distributions.Resources != null)
		{
			if (_change.ChangeType == DistributionChangeType.Gain)
			{
				if (_change.Resources.AnyMaterial > 0 || _change.Resources.Resources.HasMaterials)
				{
					_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveGainMaterials, _distributions.Resources);
				}
				else if (_change.Resources.AnyHerb > 0)
				{
					_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveGainAnyHerbs, _distributions.Resources);
				}
			}
			else if (_change.ChangeType == DistributionChangeType.LoseEach)
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_LoseMaterialsEach, _distributions.Resources);
			}
			else if (_change.Resources.AnyMaterial > 0 || _change.Resources.Resources.HasMaterials)
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveLoseMaterials, _distributions.Resources);
			}
			else if (_change.Resources.AnyHerb > 0)
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveLoseAnyHerbs, _distributions.Resources);
			}
		}
		if (_distributions.Items == null)
		{
			return;
		}
		if (_change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveBuyItemsAtDiscount, _distributions.Items);
			return;
		}
		if (_change.ChangeType == DistributionChangeType.Gain)
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveGainItems, _distributions.Items);
			return;
		}
		if (_change.ChangeType == DistributionChangeType.GainOnePerParty)
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveGainOneItemForParty, _distributions.Items);
			return;
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.ResolveLoseItem, _distributions.Items.First((CharacterItemDistribution d) => d.Item != null).CharacterID, _distributions.Items.First((CharacterItemDistribution d) => d.Item != null).Item);
	}

	public void FinalizeEventDistributionLegacy(CampaignUnresolvedAction unresolvedAction, DistributionsDTO distributions)
	{
		switch (unresolvedAction.Type)
		{
		case CampaignInteractionType.Event_LosePet:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_LosePet, distributions.PetDistribution);
			break;
		case CampaignInteractionType.Event_SelectBuildingsToProtect:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_SelectBuildingsToProtect, distributions.BuildingType);
			break;
		case CampaignInteractionType.Event_LoseAnySnowthistle:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_LoseAnySnowthistle, distributions.Resources);
			break;
		case CampaignInteractionType.Event_TradeLumberForMetal:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_TradeLumberForMetal, distributions.Resources);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseGold:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_EverybodyChooseLoseGold, (from s in distributions.Gold
				where s.Gold > 0
				select s.CharacterID).ToList());
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToGold:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_EverybodyChooseLoseUpToGold, distributions.Gold);
			break;
		case CampaignInteractionType.Event_WR24OptionA:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_WR24OptionA, distributions.Items);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToMaterials:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_EverybodyChooseLoseUpToResources, distributions.Resources);
			break;
		case CampaignInteractionType.Event_GainAnyMaterialsEach:
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveEvent_GainAnyMaterialsEach, distributions.Resources);
			break;
		case CampaignInteractionType.Event_BuyItemsDiscountOptional:
		case CampaignInteractionType.Event_GainOneItemForParty:
		case CampaignInteractionType.Event_LoseMaterialsEach:
			break;
		}
	}

	public PartySlotDTO GetPartySlotDTO(CharacterID characterID)
	{
		PartyViewService service = _view.GetService<PartyViewService>();
		MultiplayerGameController.PartySlotDesc partySlotDesc = _view.MultiplayerGameController.GetSlots().FirstOrDefault((MultiplayerGameController.PartySlotDesc desc) => desc.Character?.ID == characterID);
		if (partySlotDesc.IsValid())
		{
			return service.GetSlotData((int)partySlotDesc.ID.ID, partySlotDesc.Character, isSelected: false);
		}
		CharacterState characterState = _view.GameState.AllCharacters.FirstOrDefault((CharacterState state) => state.ID == characterID);
		if (characterState == null)
		{
			return null;
		}
		return new PartySlotDTO(_characterDetailsService.PopulateCharacterDTO(characterState))
		{
			CanRetire = _view.GameController.CharacterController.IsPersonalQuestCompleted(characterState),
			Owned = _view.PlayerCharacters.Any((CharacterState state) => state.ID == characterID)
		};
	}

	public OutpostResources GetCharacterResources(CharacterID characterID)
	{
		return _view.GameState.AllCharacters.FirstOrDefault((CharacterState state) => state.ID == characterID)?.Resources ?? default(OutpostResources);
	}

	public void Populate_DefaultGoldDistribution(ref DistributionModule.Params data, DistributionChangeDTO change, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		bool flag = change.ChangeType == DistributionChangeType.Gain;
		int num = distributions.Gold.Sum((GoldDistribution goldDistribution) => goldDistribution.Gold);
		long num2 = change.Gold - num;
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
		{
			HeaderData = new UIDistributionRowHeader.Params
			{
				Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
				Text = $"{num2}/{change.Gold}"
			},
			CellData = new List<UIDistributionCell.Params>()
		};
		data.Table.Rows.Add(distributionRowData);
		foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
		{
			GoldDistribution heroGoldDistribution = distributions.Gold.FirstOrDefault((GoldDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
			int num3 = heroGoldDistribution?.Gold ?? 0;
			uint gold = c.Character.Gold;
			int maxDelta = (int)(flag ? ((num2 > 0) ? (num3 + num2) : num3) : ((int)Mathf.Min(gold, num3 + num2)));
			UIDistributionCell.Params item2 = new UIDistributionCell.Params
			{
				CurrentDelta = num3,
				MaxDelta = maxDelta,
				Enabled = HasDistributionAuthority(c.Character),
				IsGain = flag,
				OnChange = (int delta) =>
				{
					heroGoldDistribution.Gold += delta;
					OnGoldDistributionChanged(heroGoldDistribution);
				}
			};
			distributionRowData.CellData.Add(item2);
		}
		bool isDone = num2 == 0;
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = FHUILocFormatting.PopulateParams(flag ? "GUI_RESOURCE_ALLOCATION_GAIN_GOLD" : "GUI_RESOURCE_ALLOCATION_LOSE_GOLD", change.Gold);
	}

	public void Populate_DefaultMaterialDistribution(ref DistributionModule.Params data, DistributionChangeDTO change, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		if (change.Resources.AnyMaterial != 0 || change.Resources.Resources.TotalMaterials > 0)
		{
			foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
			{
				DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
				{
					Character = GetPartySlotDTO(partyCharacter.ID),
					CharacterResources = GetCharacterResources(partyCharacter.ID)
				};
				data.Table.Columns.Add(item);
			}
		}
		if (change.Resources.AnyHerb != 0 || change.Resources.Resources.TotalHerbs > 0)
		{
			DistributionTableDTO.ColumnData item2 = new DistributionTableDTO.ColumnData
			{
				TownResources = GetTownResources
			};
			data.Table.Columns.Add(item2);
		}
		bool flag = change.ChangeType == DistributionChangeType.Gain;
		OutpostResources outpostResources = default;
		int num = 0;
		OutpostResources outpostResources2 = default;
		foreach (ResourceDistribution resource in distributions.Resources)
		{
			outpostResources += resource.Resources;
		}
		foreach (OutpostMaterialType item5 in OutpostResources.AllMaterialTypes())
		{
			outpostResources2.AddMaterial(item5, Math.Max(0, outpostResources.GetMaterialAmount(item5) - change.Resources.Resources.GetMaterialAmount(item5)));
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int num2 = change.Resources.Resources.GetMaterialAmount(materialType);
			if (change.Resources.AnyMaterial != 0)
			{
				num2 += change.Resources.AnyMaterial;
			}
			if (num2 <= 0)
			{
				continue;
			}
			int num3 = ((change.ChangeType == DistributionChangeType.LoseEach) ? (num2 * data.Table.Columns.Count) : num2);
			int num4 = num3 - outpostResources2.Total + outpostResources2.GetMaterialAmount(materialType) - outpostResources.GetMaterialAmount(materialType);
			num += num4;
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = $"{num4}/{num3}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
			{
				ResourceDistribution heroDistribution = distributions.Resources.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
				int num5 = ((heroDistribution != null) ? heroDistribution.Resources.GetMaterialAmount(materialType) : 0);
				int a = c.CharacterResources.GetMaterialAmount(materialType) - num5;
				int num6 = ((change.ChangeType == DistributionChangeType.LoseEach) ? (num2 - heroDistribution.Resources.TotalMaterials) : num4);
				int maxDelta = (flag ? (num5 + num6) : (num5 + Mathf.Min(a, num6)));
				UIDistributionCell.Params item3 = new UIDistributionCell.Params
				{
					CurrentDelta = num5,
					MaxDelta = maxDelta,
					Enabled = HasDistributionAuthority(c.Character),
					IsGain = flag,
					OnChange = (int delta) =>
					{
						heroDistribution.Resources.AddMaterial(materialType, delta);
						OnResourceDistributionChanged(heroDistribution);
					}
				};
				distributionRowData.CellData.Add(item3);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		foreach (OutpostHerbType herbType in OutpostResources.AllHerbTypes())
		{
			int num7 = change.Resources.Resources.GetHerbAmount(herbType);
			if (change.Resources.AnyHerb != 0)
			{
				num7 = change.Resources.AnyHerb;
			}
			if (num7 <= 0)
			{
				continue;
			}
			int num8 = ((change.Resources.AnyHerb != 0) ? (num7 - outpostResources.TotalHerbs) : (num7 - num7 - outpostResources.GetHerbAmount(herbType)));
			num += num8;
			bool flag2 = change.Resources.AnyHerb != 0;
			if (!flag && data.Table.Columns.All((DistributionTableDTO.ColumnData columnData) => columnData.TownResources.GetHerbAmount(herbType) == 0))
			{
				continue;
			}
			DistributionTableDTO.DistributionRowData distributionRowData2 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(herbType).Icon,
					Text = (flag2 ? num8.ToString() : num7.ToString())
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData column in data.Table.Columns)
			{
				UIDistributionCell.Params item4;
				if (!column.IsTown)
				{
					item4 = UIDistributionCell.Params.Empty();
				}
				else
				{
					ResourceDistribution townResources = distributions.Resources.FirstOrDefault((ResourceDistribution distribution) => distribution.IsFrosthavenSupply);
					int num9 = ((townResources != null) ? townResources.Resources.GetHerbAmount(herbType) : 0);
					int a2 = column.TownResources.GetHerbAmount(herbType) - num9;
					int maxDelta2 = (flag ? (num9 + num8) : (num9 + Mathf.Min(a2, num8)));
					item4 = new UIDistributionCell.Params
					{
						CurrentDelta = num9,
						MaxDelta = maxDelta2,
						Enabled = _view.IsPartyLeader,
						OnChange = (int delta) =>
						{
							townResources.Resources.AddHerb(herbType, delta);
							OnResourceDistributionChanged(townResources);
						}
					};
				}
				distributionRowData2.CellData.Add(item4);
			}
			data.Table.Rows.Add(distributionRowData2);
		}
		bool isDone = num == 0;
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		string text = string.Empty;
		if (change.ChangeType == DistributionChangeType.LoseEach)
		{
			text = "GUI_RESOURCE_ALLOCATION_GENERIC_EACH_HERO_LOSE";
		}
		else if (change.ChangeType == DistributionChangeType.GainEach)
		{
			text = "GUI_RESOURCE_ALLOCATION_GENERIC_EACH_HERO_GAIN";
		}
		else if (flag)
		{
			text = "GUI_RESOURCE_ALLOCATION_GENERIC_COLLECTIVELY_GAIN";
		}
		else if (!flag)
		{
			text = "GUI_RESOURCE_ALLOCATION_GENERIC_COLLECTIVELY_LOSE";
		}
		string text2 = string.Join(", ", GetMaterialParams());
		data.DescriptionLiteral = (text.IsNullOrEmpty() ? text2 : FHUILocFormatting.PopulateParams(text, text2));
		IEnumerable<string> GetMaterialParams()
		{
			if (change.Resources.AnyMaterial > 0)
			{
				yield return FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_GENERIC_MATERIALS", change.Resources.AnyMaterial);
			}
			if (change.Resources.AnyHerb > 0)
			{
				yield return FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_GENERIC_HERBS", change.Resources.AnyHerb);
			}
			if (LocalizeResources(change.Resources.Resources, out var result))
			{
				yield return result;
			}
		}
	}

	public static bool LocalizeResources(OutpostResources resources, out string result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool first = true;
		if (resources.Metal > 0)
		{
			AppendResource(resources.Metal, "Metal", null);
		}
		if (resources.Lumber > 0)
		{
			AppendResource(resources.Lumber, "Lumber", null);
		}
		if (resources.Hide > 0)
		{
			AppendResource(resources.Hide, "Hide", null);
		}
		if (resources.Arrowvine > 0)
		{
			AppendResource(resources.Arrowvine, "Arrowvine", null);
		}
		if (resources.Axenut > 0)
		{
			AppendResource(resources.Axenut, "Axenut", null);
		}
		if (resources.Corpsecap > 0)
		{
			AppendResource(resources.Corpsecap, "Corpsecap", null);
		}
		if (resources.Flamefruit > 0)
		{
			AppendResource(resources.Flamefruit, "Flamefruit", null);
		}
		if (resources.Rockroot > 0)
		{
			AppendResource(resources.Rockroot, "Rockroot", null);
		}
		if (resources.Snowthistle > 0)
		{
			AppendResource(resources.Snowthistle, "Snowthistle", null);
		}
		if (stringBuilder.Length == 0)
		{
			result = null;
			return false;
		}
		result = stringBuilder.ToString();
		return true;
		void AppendResource(int count, string sprite, string nameKey)
		{
			if (!first)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(count);
			if (!string.IsNullOrEmpty(sprite))
			{
				stringBuilder.Append("<size=-2><sprite name=" + sprite + "></size>");
			}
			if (!string.IsNullOrEmpty(nameKey))
			{
				stringBuilder.Append(LocalizationManager.GetTranslation(nameKey));
			}
			first = false;
		}
	}

	private FHItemTooltip.ItemTooltipParams ItemTooltipFunc(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Cost = new FHItemTooltip.ItemTooltipCost
				{
					CostLiteral = ((item.Cost != 0) ? FHUILocFormatting.Gold((int)item.Cost) : string.Empty)
				}
			}
		};
	}

	public void Populate_DefaultItemDistribution(ref DistributionModule.Params data, DistributionChangeDTO change, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		if (change.Items != null)
		{
			foreach (ItemDef itemDef in change.Items.Distinct())
			{
				if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional && !GameController.TradingController.IsItemAvailableForBuying(itemDef, ignoreTradingRequirements: true))
				{
					continue;
				}
				ItemDataDTO itemDataDTO = new ItemDataDTO(itemDef);
				if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
				{
					itemDataDTO.Cost = (uint)GameController.TradingController.GetDiscountedCost(itemDef, GameController.State.BuyItemGoldDiscount);
				}
				DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
				{
					HeaderData = new UIDistributionRowHeader.Params
					{
						Icon = UIInfoTools.Instance.GetItemConfig(itemDef.Icon).miniIcon,
						Item = itemDataDTO,
						TooltipParamsFunc = ItemTooltipFunc
					},
					CellData = new List<UIDistributionCell.Params>()
				};
				foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
				{
					CharacterItemDistribution heroDistribution = distributions.Items.FirstOrDefault((CharacterItemDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
					CharacterState character = _view.GameState.PartyCharacters.First((CharacterState state) => state.ID == c.Character.CharacterID);
					bool flag = _view.GameController.CharacterController.GetOwnedItems(character).Contains(itemDef);
					int num = change.Items.Count((ItemDef def) => def == itemDef) - distributions.Items.Count((CharacterItemDistribution i) => i.Item == itemDef);
					Sprite sprite = ((heroDistribution?.Item != null && heroDistribution.Item == itemDef && !flag) ? UIInfoTools.Instance.GetItemConfig(heroDistribution.Item.Icon).miniIcon : null);
					bool flag2 = true;
					if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
					{
						CharacterState character2 = _view.GameState.PartyCharacters.FirstOrDefault((CharacterState state) => state.ID == c.Character.CharacterID);
						flag2 = GameController.TradingController.CanBuyItem(character2, itemDef, travellingMerchant: false);
					}
					bool flag3 = true;
					if (change.ChangeType == DistributionChangeType.GainOnePerParty)
					{
						flag3 = distributions.Items.Count((CharacterItemDistribution i) => i.Item != null) == 0 || distributions.Items.Any((CharacterItemDistribution i) => i.Item == itemDef);
					}
					UIDistributionCell.Params item2 = new UIDistributionCell.Params
					{
						Icon = sprite,
						CurrentDelta = ((heroDistribution?.Item != null && heroDistribution.Item == itemDef) ? 1 : 0),
						MaxDelta = ((heroDistribution?.Item != itemDef && num > 0) ? 1 : 0),
						Enabled = (HasDistributionAuthority(c.Character) & flag2 & flag3),
						HideValueString = true,
						Owned = flag,
						OnChange = (int delta) =>
						{
							heroDistribution.Item = ((delta == 1) ? itemDef : null);
							OnItemDistributionChanged(heroDistribution);
						},
						Item = ((sprite != null) ? itemDataDTO : null),
						TooltipParamsFunc = ItemTooltipFunc
					};
					distributionRowData.CellData.Add(item2);
				}
				data.Table.Rows.Add(distributionRowData);
			}
		}
		if (change.LoseItem > 0)
		{
			IEnumerable<ItemDef> enumerable = (from itemDef3 in _view.GameState.PartyCharacters.SelectMany((CharacterState character4) => _view.GameController.CharacterController.GetOwnedItems(character4))
				where change.ItemFilter == ItemFilterFlags.None || itemDef3.HasFilterFlag(change.ItemFilter, GameController)
				select itemDef3).Distinct();
			int num2 = change.LoseItem - distributions.Items.Count((CharacterItemDistribution i) => i.Item != null);
			foreach (ItemDef itemDef2 in enumerable)
			{
				DistributionTableDTO.DistributionRowData distributionRowData2 = new DistributionTableDTO.DistributionRowData
				{
					HeaderData = new UIDistributionRowHeader.Params
					{
						Icon = UIInfoTools.Instance.GetItemConfig(itemDef2.Icon).miniIcon,
						Item = new ItemDataDTO(itemDef2),
						TooltipParamsFunc = ItemTooltipFunc
					},
					CellData = new List<UIDistributionCell.Params>()
				};
				foreach (DistributionTableDTO.ColumnData c2 in data.Table.Columns)
				{
					CharacterItemDistribution heroDistribution2 = distributions.Items.FirstOrDefault((CharacterItemDistribution distribution) => distribution.CharacterID == c2.Character.CharacterID);
					CharacterState character3 = _view.GameState.PartyCharacters.First((CharacterState state) => state.ID == c2.Character.CharacterID);
					bool flag4 = _view.GameController.CharacterController.GetOwnedItems(character3).Contains(itemDef2);
					bool flag5 = distributions.Items.Any((CharacterItemDistribution cd) => cd.Item == itemDef2 && cd != heroDistribution2);
					bool flag6 = heroDistribution2?.Item != null && heroDistribution2?.Item == itemDef2;
					Sprite sprite2 = (flag4 ? UIInfoTools.Instance.GetItemConfig(itemDef2.Icon).miniIcon : null);
					UIDistributionCell.Params item3 = new UIDistributionCell.Params
					{
						Icon = sprite2,
						CurrentDelta = (flag6 ? 1 : 0),
						MaxDelta = ((flag4 && num2 > 0 && !flag5 && !flag6) ? 1 : 0),
						NotEligible = ((!flag6 & flag4) && num2 == 0),
						IsPicked = flag6,
						Enabled = HasDistributionAuthority(c2.Character),
						HideValueString = true,
						SuppressButtons = true,
						OnChange = (int delta) =>
						{
							heroDistribution2.Item = ((delta < 0) ? null : itemDef2);
							OnItemDistributionChanged(heroDistribution2);
						},
						Item = ((sprite2 != null) ? new ItemDataDTO(itemDef2) : null),
						TooltipParamsFunc = ItemTooltipFunc
					};
					distributionRowData2.CellData.Add(item3);
				}
				data.Table.Rows.Add(distributionRowData2);
			}
		}
		bool isDone = false;
		string descriptionLiteral;
		if (change.Items != null)
		{
			int num3 = change.Items.Count((ItemDef itemDef3) => distributions.Items.All((CharacterItemDistribution i) => i.Item != itemDef3));
			if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
			{
				descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_BUY_AT_DISCOUNT", _view.GameState.BuyItemGoldDiscount);
				isDone = true;
			}
			else
			{
				if (change.ChangeType == DistributionChangeType.GainOnePerParty)
				{
					descriptionLiteral = LocalizationManager.GetTranslation("GUI_RESOURCE_ALLOCATION_DISTRIBUTE_ONE_ITEM");
					num3 = 1 - Math.Min(1, distributions.Items.Count((CharacterItemDistribution i) => i.Item != null));
				}
				else if (change.Items.Count == 1)
				{
					descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_GAIN_ITEM", LocalizationManager.GetTranslation(change.Items.FirstOrDefault().NameLocKey));
				}
				else
				{
					string text = string.Join(", ", change.Items.Select((ItemDef i) => LocalizationManager.GetTranslation(i.NameLocKey)));
					descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_GAIN_ITEMS", text);
				}
				isDone = num3 == 0;
			}
		}
		else
		{
			descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_LOSE_NUMBER_OF_ITEMS", change.LoseItem);
			int num3 = change.LoseItem - distributions.Items.Count((CharacterItemDistribution i) => i.Item != null);
			isDone = num3 == 0;
		}
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = descriptionLiteral;
	}

	public void Populate_BuildingDistribution(ref DistributionModule.Params data, BuildingMaterialDistribution distributions, uint goldCost, OutpostResourceChange materialCost, Action OnDistributionModified)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		if (_view.GameState.Inspiration != 0 && !data.Table.Columns.Any((DistributionTableDTO.ColumnData columnData) => columnData.IsTown))
		{
			data.Table.Columns.Add(new DistributionTableDTO.ColumnData
			{
				TownResources = GetTownResources
			});
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		int num = materialCost.Total - distributions.TotalMaterialsAndInspirationDonated;
		int num2 = (int)goldCost - distributions.TotalGoldDonated;
		bool done = num == 0 && num2 == 0;
		int num3 = distributions.goldDistributions.Sum((GoldDistribution goldDistribution) => goldDistribution.Gold);
		OutpostResourceChange outpostResourceChange = materialCost;
		if (outpostResourceChange.Resources.HasNegativeValue)
		{
			outpostResourceChange.AnyMaterial += outpostResourceChange.Resources.TotalMaterials;
			outpostResourceChange.Resources = default;
		}
		OutpostResources outpostResources = default;
		foreach (ResourceDistribution materialDistribution in distributions.materialDistributions)
		{
			outpostResources += materialDistribution.Resources;
		}
		if (goldCost != 0)
		{
			long num4 = goldCost - num3;
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
					Text = num4.ToString()
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
			{
				int num5 = distributions.goldDistributions.FirstOrDefault((GoldDistribution distribution) => distribution.CharacterID == c.Character.CharacterID)?.Gold ?? 0;
				uint gold = c.Character.Gold;
				float num6 = ((num4 > 0) ? Mathf.Min(num4 + num5, Mathf.Max(num5, (int)Mathf.Min(num5 + gold, distributions.GoldCost))) : ((float)num5));
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					CurrentDelta = num5,
					MaxDelta = (int)num6,
					Enabled = HasDistributionAuthority(c.Character),
					OnChange = (int delta) =>
					{
					}
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int num7 = materialCost.Resources.GetMaterialAmount(materialType);
			if (materialCost.AnyMaterial != 0)
			{
				num7 = materialCost.AnyMaterial;
			}
			if (num7 <= 0)
			{
				continue;
			}
			long num8 = num7 - (outpostResources.TotalMaterials + distributions.Inspiration);
			DistributionTableDTO.DistributionRowData distributionRowData2 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = $"{num8}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c2 in data.Table.Columns)
			{
				UIDistributionCell.Params item3;
				if (c2.IsTown)
				{
					item3 = UIDistributionCell.Params.Empty();
				}
				else
				{
					int num9 = distributions.materialDistributions.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c2.Character.CharacterID)?.Resources.GetMaterialAmount(materialType) ?? 0;
					int num10 = c2.CharacterResources.GetMaterialAmount(materialType) - num9;
					float num11 = ((num8 > 0) ? Mathf.Min(num8 + num9, Mathf.Max(num9, Mathf.Min(num9 + num10, num7))) : ((float)num9));
					item3 = new UIDistributionCell.Params
					{
						CurrentDelta = num9,
						MaxDelta = (int)num11,
						Enabled = true,
						OnChange = (int delta) =>
						{
							ResourceDistribution resourceDistribution = distributions.materialDistributions.SingleOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c2.Character.CharacterID);
							if (resourceDistribution != null)
							{
								resourceDistribution.Resources.AddMaterial(materialType, delta);
								OnDistributionModified?.Invoke();
							}
						}
					};
				}
				distributionRowData2.CellData.Add(item3);
			}
			data.Table.Rows.Add(distributionRowData2);
		}
		if (_view.GameState.Inspiration != 0)
		{
			int anyMaterial = materialCost.AnyMaterial;
			long num12 = anyMaterial - (outpostResources.TotalMaterials + distributions.Inspiration);
			DistributionTableDTO.DistributionRowData distributionRowData3 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Inspiration").Icon,
					Text = $"{num12}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData column in data.Table.Columns)
			{
				UIDistributionCell.Params item4;
				if (column.IsTown)
				{
					int num13 = (int)(_view.GameState.Inspiration - distributions.Inspiration);
					int inspiration = (int)distributions.Inspiration;
					float num14 = ((num12 > 0) ? Mathf.Min(num12 + inspiration, Mathf.Max(inspiration, Mathf.Min(inspiration + num13, anyMaterial))) : ((float)inspiration));
					item4 = new UIDistributionCell.Params
					{
						CurrentDelta = inspiration,
						MaxDelta = (int)num14,
						Enabled = true,
						OnChange = (int delta) =>
						{
							distributions.Inspiration += (uint)delta;
							OnDistributionModified?.Invoke();
						}
					};
				}
				else
				{
					item4 = UIDistributionCell.Params.Empty();
				}
				distributionRowData3.CellData.Add(item4);
			}
			data.Table.Rows.Add(distributionRowData3);
		}
		data.IsDone = () => done;
		data.IsValid = () => done;
		data.DescriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_BUILDING_REPAIR", materialCost.AnyMaterial);
	}

	public void Populate_EventDistribution(ref DistributionModule.Params data, CampaignUnresolvedAction unresolvedAction, DistributionsDTO distributions, bool playerIsReady)
	{
		switch (unresolvedAction.Type)
		{
		case CampaignInteractionType.Event_LosePet:
			distributions.PetDistribution = _view.MultiplayerGameController.GeneratePetDistribution();
			PopulateEvent_LosePet(ref data, distributions, playerIsReady);
			break;
		case CampaignInteractionType.Event_SelectBuildingsToProtect:
			distributions.BuildingType = _view.MultiplayerGameController.GenerateBuildingTypeDistribution();
			PopulateEvent_SelectBuildingsToProtect(ref data, distributions, unresolvedAction.GetInt(0), playerIsReady);
			break;
		case CampaignInteractionType.Event_GainAnyMaterialsEach:
		{
			OutpostResourceChange materialsFor_GainAnyMaterialsEach = _view.GameController.OutpostEventController.GetMaterialsFor_GainAnyMaterialsEach();
			distributions.Resources = _view.MultiplayerGameController.GenerateMaterialDistribution(materialsFor_GainAnyMaterialsEach);
			PopulateEvent_GainAnyMaterialsEach(ref data, materialsFor_GainAnyMaterialsEach, distributions, playerIsReady);
			break;
		}
		case CampaignInteractionType.Event_LoseAnySnowthistle:
			distributions.Resources = _view.MultiplayerGameController.GenerateResourceDistribution(includeFrosthaven: true, includeCharacters: false);
			PopulateEvent_LoseAnySnowthistle(ref data, distributions.Resources, playerIsReady);
			break;
		case CampaignInteractionType.Event_TradeLumberForMetal:
			distributions.Resources = _view.MultiplayerGameController.GenerateResourceDistribution(includeFrosthaven: false, includeCharacters: true);
			PopulateEvent_TradeLumberForMetal(ref data, distributions.Resources, playerIsReady);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseGold:
			distributions.Gold = _view.MultiplayerGameController.GenerateGoldDistribution();
			PopulateEvent_EverybodyChooseLoseGold(ref data, distributions.Gold, GameController.OutpostEventController.Event_EverybodyChooseLoseGold_Amount, playerIsReady);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToGold:
			distributions.Gold = _view.MultiplayerGameController.GenerateGoldDistribution();
			PopulateEvent_EverybodyChooseLoseUpToGold(ref data, distributions.Gold, unresolvedAction.GetInt(0), playerIsReady);
			break;
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToMaterials:
		{
			OutpostResourceChange materialFor_LoseUpToMaterials = _view.GameController.OutpostEventController.GetMaterialFor_LoseUpToMaterials();
			distributions.Resources = _view.MultiplayerGameController.GenerateMaterialDistribution(materialFor_LoseUpToMaterials);
			PopulateEvent_EverybodyChooseLoseUpToMaterials(ref data, new DistributionChangeDTO
			{
				Resources = materialFor_LoseUpToMaterials,
				ChangeType = DistributionChangeType.Lose
			}, distributions, playerIsReady);
			break;
		}
		case CampaignInteractionType.Event_WR24OptionA:
			distributions.Items = _view.MultiplayerGameController.GenerateItemDistribution_WR24();
			PopulateEvent_WR24OptionA(ref data, new DistributionChangeDTO
			{
				ChangeType = DistributionChangeType.Event_WR24OptionA_Lose,
				LoseItem = 1
			}, distributions.Items, playerIsReady);
			break;
		default:
			throw new Exception($"Invalid action to handle distribution for:{unresolvedAction.Type}");
		}
	}

	public void Populate_BuildingDonation(ref DistributionModule.Params data, BuildingState _buildingState)
	{
		BuildingViewService buildingService = _view.GetService<BuildingViewService>();
		BuildingMaterialDistribution donationDistribution = _buildingState.donationDistribution;
		OutpostResources materialCost = donationDistribution.MaterialCost;
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		OutpostResources outpostResources = default;
		foreach (ResourceDistribution materialDistribution in donationDistribution.materialDistributions)
		{
			outpostResources += materialDistribution.Resources;
		}
		int num = donationDistribution.goldDistributions.Sum((GoldDistribution goldDistribution) => goldDistribution.Gold);
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		if (donationDistribution.GoldCost != 0)
		{
			int num2 = donationDistribution.GoldCost - num;
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
					Text = $"{donationDistribution.GoldCost - num2}/{donationDistribution.GoldCost}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
			{
				int num3 = donationDistribution.goldDistributions.FirstOrDefault((GoldDistribution distribution) => distribution.CharacterID == c.Character.CharacterID)?.Gold ?? 0;
				uint gold = c.Character.Gold;
				int maxDelta = ((num2 > 0) ? Mathf.Min(num2 + num3, Mathf.Max(num3, (int)Mathf.Min(num3 + gold, donationDistribution.GoldCost))) : num3);
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					CurrentDelta = num3,
					MaxDelta = maxDelta,
					Enabled = c.Character.Owned,
					OnChange = (int delta) =>
					{
						if (delta > 0)
						{
							buildingService.DonateGold(_buildingState.ID, c.Character.CharacterID, delta);
						}
						else
						{
							buildingService.RefundGold(_buildingState.ID, c.Character.CharacterID, -delta);
						}
					}
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int materialAmount = materialCost.GetMaterialAmount(materialType);
			if (materialAmount <= 0)
			{
				continue;
			}
			int num4 = materialAmount - outpostResources.GetMaterialAmount(materialType);
			DistributionTableDTO.DistributionRowData distributionRowData2 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = $"{materialAmount - num4}/{materialAmount}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c2 in data.Table.Columns)
			{
				int num5 = donationDistribution.materialDistributions.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c2.Character.CharacterID)?.Resources.GetMaterialAmount(materialType) ?? 0;
				int materialAmount2 = c2.CharacterResources.GetMaterialAmount(materialType);
				int maxDelta2 = ((num4 > 0) ? Mathf.Min(num4 + num5, Mathf.Max(num5, Mathf.Min(num5 + materialAmount2, materialAmount))) : num5);
				UIDistributionCell.Params item3 = new UIDistributionCell.Params
				{
					CurrentDelta = num5,
					MaxDelta = maxDelta2,
					Enabled = c2.Character.Owned,
					OnChange = (int delta) =>
					{
						if (delta > 0)
						{
							buildingService.DonateMaterials(_buildingState.ID, c2.Character.CharacterID, new OutpostResources(materialType, delta));
						}
						else
						{
							buildingService.RefundMaterials(_buildingState.ID, c2.Character.CharacterID, new OutpostResources(materialType, -delta));
						}
					}
				};
				distributionRowData2.CellData.Add(item3);
			}
			data.Table.Rows.Add(distributionRowData2);
		}
	}

	public bool HasDistributionAuthority(PartySlotDTO partySlotDto)
	{
		return _view.IsPartyLeader;
	}

	public bool HasDistributionAuthority(PlayerDTO playerDto)
	{
		return _view.IsPartyLeader;
	}

	private void PopulateEvent_LosePet(ref DistributionModule.Params data, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
		{
			TownResources = GetTownResources
		};
		data.Table.Columns.Add(item);
		PetController petController = GameController.PetController;
		int petsMustLose = petController.CampaignController.OutpostEventController.GetPetsMustLose();
		int num = petsMustLose - distributions.PetDistribution.PetIDs.Count;
		bool isDone = num == 0;
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		foreach (PetDef petDef in petController.GetOwnedPets())
		{
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetItemConfig(petDef.ArtKey).miniIcon,
					PrefabTooltipParams = GetPetTooltipParams(petDef.ID.ToString(), string.Empty)
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData column in data.Table.Columns)
			{
				_ = column;
				PetDistribution petDistribution = distributions.PetDistribution;
				bool flag = petDistribution.PetIDs.Contains(petDef.ID);
				Sprite icon = (flag ? UIInfoTools.Instance.GetItemConfig(petDef.ArtKey).miniIcon : null);
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					Icon = icon,
					CurrentDelta = (flag ? 1 : 0),
					MaxDelta = ((num > 0 && !flag) ? 1 : 0),
					NotEligible = (!flag && num == 0),
					IsPicked = flag,
					Enabled = _view.IsPartyLeader,
					HideValueString = true,
					SuppressButtons = true,
					OnChange = (int delta) =>
					{
						if (delta < 0)
						{
							petDistribution.PetIDs.Remove(petDef.ID);
						}
						else
						{
							petDistribution.PetIDs.Add(petDef.ID);
						}
						OnPetDistributionChanged(petDistribution);
					},
					PrefabTooltipParams = (flag ? GetPetTooltipParams(petDef.ID.ToString(), string.Empty) : null)
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_REMOVE_PET", petsMustLose);
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = descriptionLiteral;
	}

	public UIPrefabTooltipTarget.TooltipInitParams GetPetTooltipParams(string petId, string actionHintLiteral)
	{
		if (string.IsNullOrEmpty(petId))
		{
			return null;
		}
		PetDef petDef = _view.GameController.DefRepository.Pets.First((KeyValuePair<PetID, PetDef> pair) =>
		{
			uint iD = pair.Value.ID.ID;
			return iD.ToString() == petId;
		}).Value;
		return new UIPrefabTooltipTarget.TooltipInitParams
		{
			Prefab = null,
			OnObjectCreatedCallback = (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = UIInfoTools.Instance.GetItemMiniSprite(petDef.ArtKey),
					CaptionLiteral = LocalizationManager.GetTranslation(petDef.Name),
					DescriptionLiteral = LocalizationManager.GetTranslation(petDef.Description),
					Hints = actionHintLiteral
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			}
		};
	}

	private void PopulateEvent_SelectBuildingsToProtect(ref DistributionModule.Params data, DistributionsDTO distributions, int buildingsToSelect, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
		{
			TownResources = GetTownResources
		};
		data.Table.Columns.Add(item);
		int num = buildingsToSelect - distributions.BuildingType.BuildingTypeIDs.Count;
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		foreach (BuildingState buildingState in GameController.State.Outpost.Buildings.Where((BuildingState x) => x.CanBeDamaged))
		{
			BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(buildingState.Def.ID);
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = buildingConfig.Icon,
					TooltipLiteral = LocalizationManager.GetTranslation(buildingConfig.GetName())
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData column in data.Table.Columns)
			{
				_ = column;
				bool flag = distributions.BuildingType.BuildingTypeIDs.Contains(buildingState.Def.ID);
				Sprite sprite = (flag ? UIInfoTools.Instance.GetBuildingConfig(buildingState.Def.ID).Icon : null);
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					Icon = sprite,
					CurrentDelta = (flag ? 1 : 0),
					MaxDelta = ((num > 0 && !flag) ? 1 : 0),
					NotEligible = (!flag && num == 0),
					IsPicked = flag,
					Enabled = _view.IsPartyLeader,
					HideValueString = true,
					SuppressButtons = true,
					OnChange = (int delta) =>
					{
						if (delta < 0)
						{
							distributions.BuildingType.BuildingTypeIDs.Remove(buildingState.Def.ID);
						}
						else
						{
							distributions.BuildingType.BuildingTypeIDs.Add(buildingState.Def.ID);
						}
						OnBuildingDistributionChanged(distributions.BuildingType);
					},
					TooltipLiteral = ((sprite != null) ? LocalizationManager.GetTranslation(buildingConfig.GetName()) : string.Empty)
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		bool isDone = num == 0;
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_SELECT_BUILDINGS_TO_PROTECT", buildingsToSelect);
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = descriptionLiteral;
	}

	private void PopulateEvent_GainAnyMaterialsEach(ref DistributionModule.Params data, OutpostResourceChange change, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int anyMaterial = change.AnyMaterial;
			if (anyMaterial <= 0)
			{
				continue;
			}
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = $"{anyMaterial}"
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
			{
				ResourceDistribution heroDistribution = distributions.Resources.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
				int num = ((heroDistribution != null) ? heroDistribution.Resources.GetMaterialAmount(materialType) : 0);
				int num2 = anyMaterial - ((heroDistribution != null) ? heroDistribution.Resources.TotalMaterials : 0);
				int maxDelta = num + num2;
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					CurrentDelta = num,
					MaxDelta = maxDelta,
					Enabled = HasDistributionAuthority(c.Character),
					IsGain = true,
					OnChange = (int delta) =>
					{
						heroDistribution.Resources.AddMaterial(materialType, delta);
						OnResourceDistributionChanged(heroDistribution);
					}
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		bool isDone = distributions.Resources.All((ResourceDistribution d) => d.Resources.TotalMaterials >= change.AnyMaterial);
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_EACH_HERO_GAIN_MATERIAL", change.AnyMaterial);
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = descriptionLiteral;
	}

	private void PopulateEvent_LoseAnySnowthistle(ref DistributionModule.Params data, List<ResourceDistribution> resourceDistribution, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
		{
			TownResources = GetTownResources
		};
		data.Table.Columns.Add(item);
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		OutpostHerbType herbType = OutpostHerbType.Snowthistle;
		DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
		{
			HeaderData = new UIDistributionRowHeader.Params
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(herbType).Icon,
				Text = "-"
			},
			CellData = new List<UIDistributionCell.Params>()
		};
		foreach (DistributionTableDTO.ColumnData column in data.Table.Columns)
		{
			ResourceDistribution townResources = resourceDistribution.FirstOrDefault((ResourceDistribution distribution) => distribution.IsFrosthavenSupply);
			int num = ((townResources != null) ? townResources.Resources.GetHerbAmount(herbType) : 0);
			int num2 = column.TownResources.GetHerbAmount(herbType) - num;
			int maxDelta = num + num2;
			UIDistributionCell.Params item2 = new UIDistributionCell.Params
			{
				CurrentDelta = num,
				MaxDelta = maxDelta,
				Enabled = _view.IsPartyLeader,
				OnChange = (int delta) =>
				{
					townResources.Resources.AddHerb(herbType, delta);
					OnResourceDistributionChanged(townResources);
				}
			};
			distributionRowData.CellData.Add(item2);
		}
		data.Table.Rows.Add(distributionRowData);
		string translation = LocalizationManager.GetTranslation("GUI_RESOURCE_ALLOCATION_TRADE_ANY_AMOUNT_OF_SNOWTHISTLE");
		bool done = resourceDistribution.Any((ResourceDistribution d) => d.Resources.Snowthistle > 0);
		data.IsDone = () => !playerIsReady & done;
		data.IsValid = () => done;
		data.DescriptionLiteral = translation;
	}

	private void PopulateEvent_TradeLumberForMetal(ref DistributionModule.Params data, List<ResourceDistribution> resourceDistribution, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		OutpostMaterialType materialType = OutpostMaterialType.Lumber;
		DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
		{
			HeaderData = new UIDistributionRowHeader.Params
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
				Text = "-"
			},
			CellData = new List<UIDistributionCell.Params>()
		};
		foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
		{
			ResourceDistribution heroDistribution = resourceDistribution.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
			int num = ((heroDistribution != null) ? heroDistribution.Resources.GetMaterialAmount(materialType) : 0);
			int num2 = c.CharacterResources.GetMaterialAmount(materialType) - num;
			int maxDelta = num + num2;
			UIDistributionCell.Params item2 = new UIDistributionCell.Params
			{
				CurrentDelta = num,
				MaxDelta = maxDelta,
				Enabled = HasDistributionAuthority(c.Character),
				OnChange = (int delta) =>
				{
					heroDistribution.Resources.AddMaterial(materialType, delta);
					OnResourceDistributionChanged(heroDistribution);
				}
			};
			distributionRowData.CellData.Add(item2);
		}
		data.Table.Rows.Add(distributionRowData);
		string translation = LocalizationManager.GetTranslation("GUI_RESOURCE_ALLOCATION_TRADE_LUMBER_FOR_METAL");
		data.IsDone = () => !playerIsReady;
		data.IsValid = () => true;
		data.DescriptionLiteral = translation;
	}

	private void PopulateEvent_EverybodyChooseLoseGold(ref DistributionModule.Params data, List<GoldDistribution> goldDistributions, int loseGoldAmount, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
		{
			HeaderData = new UIDistributionRowHeader.Params
			{
				Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
				Text = "-"
			},
			CellData = new List<UIDistributionCell.Params>()
		};
		data.Table.Rows.Add(distributionRowData);
		foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
		{
			GoldDistribution heroGoldDistribution = goldDistributions.FirstOrDefault((GoldDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
			int currentDelta = heroGoldDistribution?.Gold ?? 0;
			int maxDelta = ((c.Character.Gold >= loseGoldAmount) ? loseGoldAmount : 0);
			UIDistributionCell.Params item2 = new UIDistributionCell.Params
			{
				CurrentDelta = currentDelta,
				MaxDelta = maxDelta,
				Enabled = HasDistributionAuthority(c.Character),
				IsGain = false,
				OnChange = (int delta) =>
				{
					heroGoldDistribution.Gold += delta * loseGoldAmount;
					OnGoldDistributionChanged(heroGoldDistribution);
				}
			};
			distributionRowData.CellData.Add(item2);
		}
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_EACH_HERO_CHOOSE_TO_LOSE_GOLD", loseGoldAmount);
		data.IsDone = () => !playerIsReady;
		data.IsValid = () => true;
		data.DescriptionLiteral = descriptionLiteral;
	}

	private void PopulateEvent_EverybodyChooseLoseUpToGold(ref DistributionModule.Params data, List<GoldDistribution> goldDistributions, int maxGoldAmount, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
		{
			HeaderData = new UIDistributionRowHeader.Params
			{
				Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
				Text = "-"
			},
			CellData = new List<UIDistributionCell.Params>()
		};
		data.Table.Rows.Add(distributionRowData);
		foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
		{
			GoldDistribution heroGoldDistribution = goldDistributions.FirstOrDefault((GoldDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
			int currentDelta = heroGoldDistribution?.Gold ?? 0;
			int maxDelta = (int)Mathf.Min(c.Character.Gold, maxGoldAmount);
			UIDistributionCell.Params item2 = new UIDistributionCell.Params
			{
				CurrentDelta = currentDelta,
				MaxDelta = maxDelta,
				Enabled = HasDistributionAuthority(c.Character),
				IsGain = false,
				OnChange = (int delta) =>
				{
					heroGoldDistribution.Gold += delta;
					OnGoldDistributionChanged(heroGoldDistribution);
				}
			};
			distributionRowData.CellData.Add(item2);
		}
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_EACH_HERO_INVEST_GOLD", maxGoldAmount);
		data.IsDone = () => !playerIsReady;
		data.IsValid = () => true;
		data.DescriptionLiteral = descriptionLiteral;
	}

	private void PopulateEvent_EverybodyChooseLoseUpToMaterials(ref DistributionModule.Params data, DistributionChangeDTO change, DistributionsDTO distributions, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		if (change.Resources.AnyMaterial != 0 || change.Resources.Resources.TotalMaterials > 0)
		{
			foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
			{
				DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
				{
					Character = GetPartySlotDTO(partyCharacter.ID),
					CharacterResources = GetCharacterResources(partyCharacter.ID)
				};
				data.Table.Columns.Add(item);
			}
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int anyMaterial = change.Resources.AnyMaterial;
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = string.Empty
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
			{
				ResourceDistribution heroDistribution = distributions.Resources.FirstOrDefault((ResourceDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
				int num = ((heroDistribution != null) ? heroDistribution.Resources.GetMaterialAmount(materialType) : 0);
				int b = anyMaterial - ((heroDistribution != null) ? heroDistribution.Resources.TotalMaterials : 0);
				int a = c.CharacterResources.GetMaterialAmount(materialType) - num;
				int maxDelta = num + Mathf.Min(a, b);
				UIDistributionCell.Params item2 = new UIDistributionCell.Params
				{
					CurrentDelta = num,
					MaxDelta = maxDelta,
					Enabled = HasDistributionAuthority(c.Character),
					IsGain = false,
					OnChange = (int delta) =>
					{
						heroDistribution.Resources.AddMaterial(materialType, delta);
						OnResourceDistributionChanged(heroDistribution);
					}
				};
				distributionRowData.CellData.Add(item2);
			}
			data.Table.Rows.Add(distributionRowData);
		}
		string descriptionLiteral = FHUILocFormatting.PopulateParams("GUI_RESOURCE_ALLOCATION_EACH_HERO_INVEST_ANY_MATERIAL", change.Resources.AnyMaterial);
		data.IsDone = () => !playerIsReady;
		data.IsValid = () => true;
		data.DescriptionLiteral = descriptionLiteral;
	}

	private void PopulateEvent_WR24OptionA(ref DistributionModule.Params data, DistributionChangeDTO change, List<CharacterItemDistribution> itemDistribution, bool playerIsReady)
	{
		data.Table = new DistributionTableDTO();
		data.Table.Columns = new List<DistributionTableDTO.ColumnData>();
		foreach (CharacterState partyCharacter in _view.GameState.PartyCharacters)
		{
			DistributionTableDTO.ColumnData item = new DistributionTableDTO.ColumnData
			{
				Character = GetPartySlotDTO(partyCharacter.ID),
				CharacterResources = GetCharacterResources(partyCharacter.ID)
			};
			data.Table.Columns.Add(item);
		}
		data.Table.Rows = new List<DistributionTableDTO.DistributionRowData>();
		if (change.LoseItem > 0)
		{
			foreach (ItemDef itemDef in _view.GameState.PartyCharacters.SelectMany((CharacterState character2) => OutpostEventController.GetValidItemsForEvent_WR24OptionA(character2, _view.GameController)).Distinct())
			{
				DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
				{
					HeaderData = new UIDistributionRowHeader.Params
					{
						Icon = UIInfoTools.Instance.GetItemConfig(itemDef.Icon).miniIcon,
						Item = new ItemDataDTO(itemDef),
						TooltipParamsFunc = ItemTooltipFunc
					},
					CellData = new List<UIDistributionCell.Params>()
				};
				foreach (DistributionTableDTO.ColumnData c in data.Table.Columns)
				{
					CharacterItemDistribution heroDistribution = itemDistribution.FirstOrDefault((CharacterItemDistribution distribution) => distribution.CharacterID == c.Character.CharacterID);
					CharacterState character = _view.GameState.PartyCharacters.First((CharacterState state) => state.ID == c.Character.CharacterID);
					_view.GameController.CharacterController.GetOwnedItems(character).Contains(itemDef);
					bool flag = OutpostEventController.GetValidItemsForEvent_WR24OptionA(character, _view.GameController).Contains(itemDef);
					bool flag2 = heroDistribution?.Item != null && heroDistribution?.Item == itemDef;
					Sprite sprite = (flag ? UIInfoTools.Instance.GetItemConfig(itemDef.Icon).miniIcon : null);
					UIDistributionCell.Params item2 = new UIDistributionCell.Params
					{
						Icon = sprite,
						CurrentDelta = (flag2 ? 1 : 0),
						MaxDelta = ((flag && !flag2) ? 1 : 0),
						NotEligible = (!flag2 & flag),
						IsPicked = flag2,
						Enabled = HasDistributionAuthority(c.Character),
						HideValueString = true,
						SuppressButtons = true,
						OnChange = (int delta) =>
						{
							heroDistribution.Item = ((delta < 0) ? null : itemDef);
							OnItemDistributionChanged(heroDistribution);
						},
						Item = ((sprite != null) ? new ItemDataDTO(itemDef) : null),
						TooltipParamsFunc = ItemTooltipFunc
					};
					distributionRowData.CellData.Add(item2);
				}
				data.Table.Rows.Add(distributionRowData);
			}
		}
		bool isDone = !itemDistribution.Any((CharacterItemDistribution distribution) => distribution.Item == null);
		string translation = LocalizationManager.GetTranslation("GUI_RESOURCE_ALLOCATION_EACH_HERO_CHOOSE_ONE_ITEM_DISCARD");
		data.IsDone = () => isDone && !playerIsReady;
		data.IsValid = () => isDone;
		data.DescriptionLiteral = translation;
	}

	public DistributionTableDTO GetDefaultResourceTable(DistributionChangeDTO change, DistributionsDTO distributions, Action OnChangeCallback)
	{
		return new DistributionTableDTO
		{
			Rows = BuildDefaultTableRowData(change, distributions, OnChangeCallback),
			Columns = GetDefaultColumnData(change, distributions, this)
		};
	}

	private List<DistributionTableDTO.DistributionRowData> BuildDefaultTableRowData(DistributionChangeDTO change, DistributionsDTO distributions, Action OnChange)
	{
		List<DistributionTableDTO.DistributionRowData> list = new List<DistributionTableDTO.DistributionRowData>();
		if (change.Gold != 0)
		{
			DistributionTableDTO.DistributionRowData distributionRowData = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
					Text = change.Gold.ToString()
				},
				CellData = new List<UIDistributionCell.Params>(distributions.Gold.Count)
			};
			foreach (GoldDistribution goldDist in distributions.Gold)
			{
				UIDistributionCell.Params item = new UIDistributionCell.Params
				{
					CurrentDelta = goldDist.Gold,
					MaxDelta = change.Gold,
					Enabled = true,
					OnChange = (int delta) =>
					{
						goldDist.Gold += delta;
					}
				};
				distributionRowData.CellData.Add(item);
			}
			list.Add(distributionRowData);
		}
		OutpostResources outpostResources = default;
		if (distributions.Resources != null)
		{
			foreach (ResourceDistribution resource4 in distributions.Resources)
			{
				outpostResources += resource4.Resources;
			}
		}
		foreach (OutpostMaterialType materialType in OutpostResources.AllMaterialTypes())
		{
			int materialAmount = change.Resources.Resources.GetMaterialAmount(materialType);
			if (materialAmount <= 0 && change.Resources.AnyMaterial == 0)
			{
				continue;
			}
			int num = materialAmount - outpostResources.GetMaterialAmount(materialType);
			DistributionTableDTO.DistributionRowData distributionRowData2 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(materialType).Icon,
					Text = materialAmount.ToString()
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			if (distributions.Resources != null)
			{
				foreach (ResourceDistribution resource in distributions.Resources)
				{
					int materialAmount2 = resource.Resources.GetMaterialAmount(materialType);
					int materialAmount3 = change.Resources.Resources.GetMaterialAmount(materialType);
					int maxDelta = ((num > 0) ? Mathf.Max(materialAmount2, Mathf.Min(materialAmount2 + materialAmount3, materialAmount)) : materialAmount2);
					UIDistributionCell.Params item2 = new UIDistributionCell.Params
					{
						CurrentDelta = resource.Resources.GetMaterialAmount(materialType),
						MaxDelta = maxDelta,
						Enabled = true,
						OnChange = (int delta) =>
						{
							resource.Resources.AddMaterial(materialType, delta);
							OnChange?.Invoke();
						}
					};
					distributionRowData2.CellData.Add(item2);
				}
			}
			if (distributions.BuildingMaterial?.materialDistributions != null)
			{
				foreach (ResourceDistribution resource2 in distributions.BuildingMaterial.materialDistributions)
				{
					UIDistributionCell.Params item3 = new UIDistributionCell.Params
					{
						CurrentDelta = resource2.Resources.GetMaterialAmount(materialType),
						MaxDelta = change.Resources.Resources.GetMaterialAmount(materialType),
						Enabled = true,
						OnChange = (int delta) =>
						{
							resource2.Resources.AddMaterial(materialType, delta);
							OnChange?.Invoke();
						}
					};
					distributionRowData2.CellData.Add(item3);
				}
			}
			list.Add(distributionRowData2);
		}
		foreach (OutpostHerbType herbType in OutpostResources.AllHerbTypes())
		{
			int herbAmount = change.Resources.Resources.GetHerbAmount(herbType);
			if (herbAmount <= 0 && change.Resources.AnyHerb == 0)
			{
				continue;
			}
			DistributionTableDTO.DistributionRowData distributionRowData3 = new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(herbType).Icon,
					Text = herbAmount.ToString()
				},
				CellData = new List<UIDistributionCell.Params>()
			};
			foreach (ResourceDistribution resource3 in distributions.Resources)
			{
				UIDistributionCell.Params item4 = new UIDistributionCell.Params
				{
					CurrentDelta = resource3.Resources.GetHerbAmount(herbType),
					MaxDelta = change.Resources.Resources.GetHerbAmount(herbType),
					Enabled = true,
					OnChange = (int delta) =>
					{
						resource3.Resources.AddHerb(herbType, delta);
						OnChange?.Invoke();
					}
				};
				distributionRowData3.CellData.Add(item4);
			}
			list.Add(distributionRowData3);
		}
		if (distributions?.BuildingMaterial != null && distributions.BuildingMaterial.Inspiration != 0)
		{
			new DistributionTableDTO.DistributionRowData
			{
				HeaderData = new UIDistributionRowHeader.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Inspiration").Icon,
					Text = distributions.BuildingMaterial.Inspiration.ToString()
				},
				CellData = new List<UIDistributionCell.Params>()
			};
		}
		return list;
	}

	private List<DistributionTableDTO.ColumnData> GetDefaultColumnData(DistributionChangeDTO change, DistributionsDTO distributions, ResourceAllocationService service)
	{
		List<DistributionTableDTO.ColumnData> list = new List<DistributionTableDTO.ColumnData>();
		if (distributions.Resources != null)
		{
			foreach (ResourceDistribution resource in distributions.Resources)
			{
				if ((!(resource.CharacterID != null) || !list.Any((DistributionTableDTO.ColumnData data) => data.Character.CharacterID == resource.CharacterID)) && (!(resource.CharacterID == null) || !list.Any((DistributionTableDTO.ColumnData data) => data.Character == null)))
				{
					DistributionTableDTO.ColumnData columnData = new DistributionTableDTO.ColumnData();
					if (resource.CharacterID != null)
					{
						columnData.Character = service.GetPartySlotDTO(resource.CharacterID);
						columnData.CharacterResources = service.GetCharacterResources(resource.CharacterID);
					}
					else
					{
						columnData.TownResources = service.GetTownResources;
					}
					list.Add(columnData);
				}
			}
		}
		if (distributions.Gold != null)
		{
			foreach (GoldDistribution resource2 in distributions.Gold)
			{
				if (!(resource2.CharacterID != null) || !list.Any((DistributionTableDTO.ColumnData data) => data.Character.CharacterID == resource2.CharacterID))
				{
					DistributionTableDTO.ColumnData columnData2 = new DistributionTableDTO.ColumnData();
					if (resource2.CharacterID != null)
					{
						columnData2.Character = service.GetPartySlotDTO(resource2.CharacterID);
						columnData2.CharacterResources = service.GetCharacterResources(resource2.CharacterID);
					}
					list.Add(columnData2);
				}
			}
		}
		if (change.Items != null)
		{
			foreach (CharacterState partyMemeber in Singleton<FHMapChoreographer>.InstanceFast.View.GameState.PartyCharacters)
			{
				if (!list.Any((DistributionTableDTO.ColumnData data) => data.Character.CharacterID == partyMemeber.ID))
				{
					DistributionTableDTO.ColumnData columnData3 = new DistributionTableDTO.ColumnData();
					columnData3.Character = service.GetPartySlotDTO(partyMemeber.ID);
					columnData3.CharacterResources = service.GetCharacterResources(partyMemeber.ID);
					list.Add(columnData3);
				}
			}
		}
		if (distributions?.BuildingMaterial != null && (distributions == null || distributions.BuildingMaterial.Inspiration != 0) && !list.Any((DistributionTableDTO.ColumnData data) => data.Character == null))
		{
			DistributionTableDTO.ColumnData columnData4 = new DistributionTableDTO.ColumnData();
			columnData4.TownResources = service.GetTownResources;
			list.Add(columnData4);
		}
		return list;
	}
}
