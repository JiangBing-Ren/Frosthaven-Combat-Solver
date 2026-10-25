using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class DistributionModalModule : FHMapViewModule
{
	public FHButton DoneButton;

	public FHButton CancelButton;

	public Transform ItemsRoot;

	public CharacterDistributionController DistributionControllerPrefab;

	public TMP_Text Title;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer m_ReadyUsersListContainer;

	public Action OnDone;

	public Action OnCancel;

	public Action<GoldDistribution> OnGoldDistributionChanged;

	public Action<ResourceDistribution> OnResourceDistributionChanged;

	public Action<CharacterItemDistribution> OnItemDistributionChanged;

	public Action<BuildingTypeDistribution> OnBuildingDistributionChanged;

	public Action<PetDistribution> OnPetDistributionChanged;

	private List<CharacterDistributionController> _distributionControllers = new List<CharacterDistributionController>();

	public void Awake()
	{
		DoneButton.onClick.AddListener(() =>
		{
			OnDone?.Invoke();
		});
		CancelButton.onClick.AddListener(() =>
		{
			OnCancel?.Invoke();
		});
	}

	public void Init_BuildingDistribution(string title, BuildingMaterialDistribution distr, uint goldCost, OutpostResourceChange materialCost, CampaignState state, PlayerDTO player)
	{
		Title.text = title;
		int num = materialCost.Total - distr.TotalMaterialsAndInspirationDonated;
		int num2 = (int)goldCost - distr.TotalGoldDonated;
		bool flag = num == 0 && num2 == 0;
		CancelButton.gameObject.SetActive(value: true);
		if (materialCost.Total > 0)
		{
			Title.text += " for ";
			OutpostResourceChange outpostResourceChange = materialCost;
			if (outpostResourceChange.AnyMaterial > 0)
			{
				Title.text += $"{outpostResourceChange.AnyMaterial} Materials, ";
			}
			foreach (IGrouping<OutpostMaterialType, OutpostMaterialType> item in from m in outpostResourceChange.Resources.ToMaterialList()
				group m by m)
			{
				Title.text += $"{item.Count()} {item.Key}, ";
			}
		}
		if (goldCost != 0)
		{
			Title.text += $"{goldCost} Gold";
		}
		int num3 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(distr.materialDistributions.Count))
		{
			ResourceDistribution resourceDistribution = distr.materialDistributions[num3];
			GoldDistribution characterGoldDistribution = distr.goldDistributions[num3];
			resourceDistribution.GetCharacter(state);
			characterDistributionController.Init_BuildingMaterialDistribution(distr, characterGoldDistribution, resourceDistribution, state, goldCost, materialCost, enabled: true, flag, player.IsPartyLeader);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_BuildingDistribution(title, distr, goldCost, materialCost, state, player);
			};
			num3++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		if (flag)
		{
			DoneButton.GetComponentInChildren<TMP_Text>().text = "DONE";
		}
		else
		{
			DoneButton.GetComponentInChildren<TMP_Text>().text = $"{num} MATERIALS REMAIN, {num2} GOLD REMAINS";
		}
	}

	public void Init_LoseAnySnowthistle(List<ResourceDistribution> resourceDistribution, CampaignState state, PlayerDTO player)
	{
		Title.text = "Lose Any Snowthistle";
		CancelButton.gameObject.SetActive(value: false);
		bool flag = resourceDistribution.Any((ResourceDistribution d) => d.Resources.Snowthistle > 0);
		int num = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistribution.Count()))
		{
			CharacterState character = resourceDistribution[num].GetCharacter(state);
			ResourceDistribution distr = resourceDistribution[num];
			OutpostResources outpostResources = character?.Resources ?? state.Outpost.Resources;
			ResourceMod item = new ResourceMod
			{
				Name = "Snowthistle",
				Value = outpostResources.Snowthistle,
				Mod = distr.Resources.Snowthistle,
				MaxMod = outpostResources.Snowthistle,
				Change = (int mod) =>
				{
					distr.Resources.Snowthistle += mod;
					OnResourceDistributionChanged?.Invoke(distr);
				}
			};
			bool flag2 = player.OwnedCharacter.Contains(character);
			CharacterDistributionController.ResourceSourceDTO resourceSourceDTO = null;
			if (character != null)
			{
				resourceSourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
			}
			else
			{
				resourceSourceDTO = new CharacterDistributionController.ResourceSourceDTO(state.Outpost);
				flag2 = player.IsPartyLeader;
			}
			characterDistributionController.Init(resourceSourceDTO, new List<ResourceMod> { item }, isGain: false, flag2, done: false);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_LoseAnySnowthistle(resourceDistribution, state, player);
			};
			num++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = (flag ? "DONE" : "SELECT AT LEAST 1 SNOWTHISTLE");
	}

	public void Init_Event_EverybodyChooseLoseUpToMaterials(DistributionChangeDTO change, List<ResourceDistribution> resourceDistr, CampaignState state, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		bool flag = change.ChangeType == DistributionChangeType.Gain;
		Title.text = (flag ? "Gain " : "Lose ");
		OutpostResourceChange resources = change.Resources;
		if (resources.AnyMaterial > 0)
		{
			Title.text += $"{resources.AnyMaterial} Materials, ";
		}
		if (resources.AnyHerb > 0)
		{
			Title.text += $"{resources.AnyHerb} Herbs, ";
		}
		foreach (IGrouping<OutpostMaterialType, OutpostMaterialType> item in from m in resources.Resources.ToMaterialList()
			group m by m)
		{
			Title.text += $"{item.Count()} {item.Key}, ";
		}
		foreach (IGrouping<OutpostHerbType, OutpostHerbType> item2 in from m in resources.Resources.ToHerbList()
			group m by m)
		{
			Title.text += $"{item2.Count()} {item2.Key}, ";
		}
		int num = resourceDistr.Sum((ResourceDistribution x) => x.Resources.Total);
		bool done = change.Resources.Total - num == 0;
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistr.Count()))
		{
			CharacterState character = resourceDistr[num2].GetCharacter(state);
			bool flag2 = false;
			CharacterDistributionController.ResourceSourceDTO resourceDTO;
			if (character != null)
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
				flag2 = player.OwnedCharacter.Contains(character);
			}
			else
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(state.Outpost);
				flag2 = player.IsPartyLeader;
			}
			characterDistributionController.Init(resourceDistr, resourceDTO, change.Resources, change.ChangeType, flag2, done);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_Event_EverybodyChooseLoseUpToMaterials(change, resourceDistr, state, player);
			};
			num2++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = "DONE";
	}

	public void Init_TradeLumberForMetal(List<ResourceDistribution> resourceDistribution, CampaignState state, PlayerDTO player)
	{
		Title.text = "Trade Lumber for Metal";
		CancelButton.gameObject.SetActive(value: false);
		int num = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistribution.Count()))
		{
			CharacterState character = resourceDistribution[num].GetCharacter(state);
			ResourceDistribution distr = resourceDistribution[num];
			OutpostResources outpostResources = character?.Resources ?? state.Outpost.Resources;
			ResourceMod item = new ResourceMod
			{
				Name = "Lumber",
				Value = outpostResources.Lumber,
				Mod = distr.Resources.Lumber,
				MaxMod = outpostResources.Lumber,
				Change = (int mod) =>
				{
					distr.Resources.Lumber += mod;
					OnResourceDistributionChanged?.Invoke(distr);
				}
			};
			bool flag = player.OwnedCharacter.Contains(character);
			CharacterDistributionController.ResourceSourceDTO resourceSourceDTO = null;
			if (character != null)
			{
				resourceSourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
			}
			else
			{
				resourceSourceDTO = new CharacterDistributionController.ResourceSourceDTO(state.Outpost);
				flag = player.IsPartyLeader;
			}
			characterDistributionController.Init(resourceSourceDTO, new List<ResourceMod> { item }, isGain: false, flag, done: false);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_TradeLumberForMetal(resourceDistribution, state, player);
			};
			num++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = "DONE";
	}

	public void Init_EverybodyChooseLoseGold(List<GoldDistribution> goldDistr, int gold, CampaignState state, PlayerDTO player)
	{
		Title.text = $"Choose to Lose {gold} Gold";
		CancelButton.gameObject.SetActive(value: false);
		int num = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(goldDistr.Count()))
		{
			CharacterState character = goldDistr[num].GetCharacter(state);
			GoldDistribution distr = goldDistr[num];
			ResourceMod item = new ResourceMod
			{
				Name = "Gold",
				Value = (int)character.Gold,
				Mod = distr.Gold,
				MaxMod = int.MaxValue,
				Change = (int mod) =>
				{
					distr.Gold += mod;
					OnGoldDistributionChanged?.Invoke(distr);
				}
			};
			characterDistributionController.Init(enabled: player.OwnedCharacter.Contains(character), done: distr.Gold > 0, source: new CharacterDistributionController.ResourceSourceDTO(character), bindings: new List<ResourceMod> { item }, isGain: false);
			characterDistributionController.ModValue = gold;
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_EverybodyChooseLoseGold(goldDistr, gold, state, player);
			};
			num++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = "DONE";
	}

	public void Init_EverybodyChooseLoseUpToGold(List<GoldDistribution> goldDistr, int gold, CampaignState state, PlayerDTO player)
	{
		Title.text = $"Choose to Lose up to {gold} Gold";
		CancelButton.gameObject.SetActive(value: false);
		int num = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(goldDistr.Count()))
		{
			CharacterState character = goldDistr[num].GetCharacter(state);
			GoldDistribution distr = goldDistr[num];
			ResourceMod item = new ResourceMod
			{
				Name = "Gold",
				Value = (int)character.Gold,
				Mod = distr.Gold,
				MaxMod = gold,
				Change = (int mod) =>
				{
					distr.Gold += mod;
					OnGoldDistributionChanged?.Invoke(distr);
				}
			};
			characterDistributionController.Init(enabled: player.OwnedCharacter.Contains(character), done: distr.Gold >= gold, source: new CharacterDistributionController.ResourceSourceDTO(character), bindings: new List<ResourceMod> { item }, isGain: false);
			characterDistributionController.ModValue = 1;
			characterDistributionController.OnGoldDistributionChanged = (GoldDistribution c) =>
			{
				Init_EverybodyChooseLoseUpToGold(goldDistr, gold, state, player);
			};
			num++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = "DONE";
	}

	public void Init(DistributionChangeDTO change, List<CharacterItemDistribution> itemDistribution, CampaignController controller, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		bool flag = false;
		int num;
		if (change.Items != null)
		{
			num = change.Items.Count((ItemDef c) => !itemDistribution.Any((CharacterItemDistribution i) => i.Item == c));
			if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
			{
				Title.text = $"Buy items at a discount of {controller.State.BuyItemGoldDiscount} gold";
				flag = true;
			}
			else
			{
				if (change.ChangeType == DistributionChangeType.GainOnePerParty)
				{
					Title.text = "Choose 1 item for the party to gain";
					num = 1 - Math.Min(1, itemDistribution.Where((CharacterItemDistribution i) => i.Item != null).Count());
				}
				else
				{
					Title.text = "Gain " + string.Join(", ", change.Items.Select((ItemDef s) => s.Name));
				}
				flag = num == 0;
			}
		}
		else
		{
			Title.text = $"Lose {change.LoseItem} Items";
			num = change.LoseItem - itemDistribution.Count((CharacterItemDistribution i) => i.Item != null);
			flag = num == 0;
		}
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(itemDistribution.Count()))
		{
			CharacterState character = itemDistribution[num2].GetCharacter(controller.State);
			bool flag2 = player.OwnedCharacter.Contains(character);
			if (change.Items != null)
			{
				if (change.ChangeType == DistributionChangeType.BuyAtDiscountOptional)
				{
					List<ItemDef> items = change.Items.Where((ItemDef item) => controller.CharacterController.CanHaveItem(character, item) && controller.TradingController.IsItemAvailableForBuying(item, ignoreTradingRequirements: true)).ToList();
					characterDistributionController.Init(itemDistribution, character, items, controller.TradingController, flag2, done: false);
				}
				else
				{
					List<ItemDef> items2 = change.Items.Where((ItemDef item) => controller.CharacterController.CanHaveItem(character, item)).ToList();
					characterDistributionController.Init(itemDistribution, character, items2, flag2, flag);
				}
			}
			else
			{
				characterDistributionController.Init(itemDistribution, character, change.ItemFilter, controller, flag2, flag);
			}
			characterDistributionController.OnItemDistributionChanged = (CharacterItemDistribution c) =>
			{
				OnItemDistributionChanged?.Invoke(c);
			};
			num2++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag) ? $"{num} ITEM REMAINS" : "DONE");
	}

	public void Init(DistributionChangeDTO change, List<GoldDistribution> goldDistribution, CampaignState state, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		bool flag = change.ChangeType == DistributionChangeType.Gain;
		Title.text = (flag ? "Gain " : "Lose ") + change.Gold + " Gold";
		int num = change.Gold - goldDistribution.Sum((GoldDistribution g) => g.Gold);
		bool flag2 = num == 0;
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(goldDistribution.Count))
		{
			CharacterState character = goldDistribution[num2].GetCharacter(state);
			if (character != null)
			{
				bool flag3 = player.OwnedCharacter.Contains(character);
				characterDistributionController.Init(goldDistribution, new CharacterDistributionController.GoldSourceDTO(character), change.Gold, flag, flag3, flag2);
			}
			else
			{
				bool isPartyLeader = player.IsPartyLeader;
				characterDistributionController.Init(goldDistribution, new CharacterDistributionController.GoldSourceDTO(state), change.Gold, flag, isPartyLeader, flag2);
			}
			characterDistributionController.OnGoldDistributionChanged = (GoldDistribution c) =>
			{
				OnGoldDistributionChanged?.Invoke(c);
			};
			num2++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag2 && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag2) ? $"{num} GOLD REMAINS" : "DONE");
	}

	public void Init(DistributionChangeDTO change, List<ResourceDistribution> resourceDistr, CampaignState state, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		bool flag = change.ChangeType == DistributionChangeType.Gain;
		Title.text = (flag ? "Gain " : "Lose ");
		OutpostResourceChange resources = change.Resources;
		if (resources.AnyMaterial > 0)
		{
			Title.text += $"{resources.AnyMaterial} Materials, ";
		}
		if (resources.AnyHerb > 0)
		{
			Title.text += $"{resources.AnyHerb} Herbs, ";
		}
		foreach (IGrouping<OutpostMaterialType, OutpostMaterialType> item in from m in resources.Resources.ToMaterialList()
			group m by m)
		{
			Title.text += $"{item.Count()} {item.Key}, ";
		}
		foreach (IGrouping<OutpostHerbType, OutpostHerbType> item2 in from m in resources.Resources.ToHerbList()
			group m by m)
		{
			Title.text += $"{item2.Count()} {item2.Key}, ";
		}
		OutpostResources resources2 = change.Resources.Resources;
		int num = change.Resources.Total;
		if (change.ChangeType == DistributionChangeType.LoseEach)
		{
			int num2 = num;
			foreach (ResourceDistribution item3 in resourceDistr)
			{
				OutpostResources resources3 = item3.Resources;
				if (resources3.Total < num2)
				{
					num2 = resources3.Total;
				}
			}
			num -= num2;
		}
		else
		{
			foreach (ResourceDistribution item4 in resourceDistr)
			{
				resources2 -= item4.Resources;
				num -= item4.Resources.Total;
			}
		}
		bool flag2 = num == 0;
		int num3 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistr.Count))
		{
			CharacterState character = resourceDistr[num3].GetCharacter(state);
			bool flag3 = false;
			CharacterDistributionController.ResourceSourceDTO resourceDTO;
			if (character != null)
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
				flag3 = player.OwnedCharacter.Contains(character);
			}
			else
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(state.Outpost);
				flag3 = player.IsPartyLeader;
			}
			characterDistributionController.Init(resourceDistr, resourceDTO, change.Resources, change.ChangeType, flag3, flag2);
			characterDistributionController.OnResourceDistributionChanged = (ResourceDistribution c) =>
			{
				OnResourceDistributionChanged?.Invoke(c);
			};
			num3++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag2 && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag2) ? $"{num} RESOURCES REMAINS" : "DONE");
	}

	public void Init_EventGainAnyMaterialEach(DistributionChangeDTO change, List<ResourceDistribution> resourceDistr, CampaignState state, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		Title.text = "Gain ";
		OutpostResourceChange resources = change.Resources;
		if (resources.AnyMaterial > 0)
		{
			Title.text += $"{resources.AnyMaterial} Materials, ";
		}
		if (resources.AnyHerb > 0)
		{
			Title.text += $"{resources.AnyHerb} Herbs, ";
		}
		foreach (IGrouping<OutpostMaterialType, OutpostMaterialType> item in from m in resources.Resources.ToMaterialList()
			group m by m)
		{
			Title.text += $"{item.Count()} {item.Key}, ";
		}
		foreach (IGrouping<OutpostHerbType, OutpostHerbType> item2 in from m in resources.Resources.ToHerbList()
			group m by m)
		{
			Title.text += $"{item2.Count()} {item2.Key}, ";
		}
		_ = change.Resources;
		int total = change.Resources.Total;
		int num = total;
		foreach (ResourceDistribution item3 in resourceDistr)
		{
			OutpostResources resources2 = item3.Resources;
			if (resources2.Total < num)
			{
				num = resources2.Total;
			}
		}
		total -= num;
		bool flag = total == 0;
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistr.Count()))
		{
			CharacterState character = resourceDistr[num2].GetCharacter(state);
			bool flag2 = false;
			CharacterDistributionController.ResourceSourceDTO resourceDTO;
			if (character != null)
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
				flag2 = player.OwnedCharacter.Contains(character);
			}
			else
			{
				resourceDTO = new CharacterDistributionController.ResourceSourceDTO(state.Outpost);
				flag2 = player.IsPartyLeader;
			}
			bool done = characterDistributionController.ResourceDistr != null && change.Resources.Total - characterDistributionController.ResourceDistr.Resources.Total <= 0;
			characterDistributionController.Init(resourceDistr, resourceDTO, change.Resources, change.ChangeType, flag2, done);
			characterDistributionController.OnResourceDistributionChanged = (ResourceDistribution c) =>
			{
				OnResourceDistributionChanged?.Invoke(c);
			};
			num2++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag) ? $"{total} RESOURCES REMAINS" : "DONE");
	}

	public void Init_EventWR24OptionA(DistributionChangeDTO change, List<CharacterItemDistribution> itemDistribution, CampaignController controller, PlayerDTO player)
	{
		CancelButton.gameObject.SetActive(value: false);
		bool flag = false;
		Title.text = $"Lose {change.LoseItem} Items";
		int num = 0;
		foreach (CharacterItemDistribution item in itemDistribution)
		{
			if (item.Item == null)
			{
				num++;
			}
		}
		flag = num == 0;
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(itemDistribution.Count()))
		{
			CharacterState character = itemDistribution[num2].GetCharacter(controller.State);
			bool flag2 = player.OwnedCharacter.Contains(character);
			characterDistributionController.Init_EventWR24OptionA(itemDistribution, character, controller, flag2, flag);
			characterDistributionController.OnItemDistributionChanged = (CharacterItemDistribution c) =>
			{
				OnItemDistributionChanged?.Invoke(c);
			};
			num2++;
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag) ? $"{num} ITEM REMAINS" : "DONE");
	}

	public void Init_Event_LosePet(PetDistribution petDistribution, PetController petController, PlayerDTO player)
	{
		int petsMustLose = petController.CampaignController.OutpostEventController.GetPetsMustLose();
		int num = petsMustLose - petDistribution.PetIDs.Count;
		bool flag = false;
		Title.text = $"Select {petsMustLose} Pets to lose";
		flag = num == 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(1))
		{
			characterDistributionController.Init_Event_LosePet(petDistribution, petController, enabled: true, flag);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_Event_LosePet(petDistribution, petController, player);
			};
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag) ? $"{num} PETS REMAIN" : "DONE");
	}

	public void Init_Event_SelectBuildingsToProtect(BuildingTypeDistribution buildingDistribution, int buildingsToSelect, OutpostState outpostState, PlayerDTO player)
	{
		int num = buildingsToSelect - buildingDistribution.BuildingTypeIDs.Count;
		bool flag = false;
		Title.text = $"Select {buildingsToSelect} Buildings to protect";
		flag = num == 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(1))
		{
			characterDistributionController.Init_Event_SelectBuildingsToProtect(buildingDistribution, outpostState, enabled: true, flag);
			characterDistributionController.OnDistributionChanged = () =>
			{
				Init_Event_SelectBuildingsToProtect(buildingDistribution, buildingsToSelect, outpostState, player);
			};
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = flag && !player.IsReady;
		DoneButton.GetComponentInChildren<TMP_Text>().text = ((!flag) ? $"{num} BUILDINGS REMAIN" : "DONE");
	}

	public void Init_HeroRetirement(DistributionChangeDTO change, List<ResourceDistribution> resourceDistribution, CampaignState state, PlayerDTO player)
	{
		Title.text = LocalizationManager.GetTranslation("GUI_RETIRE_DISTRIBUTE_RESOURCES_DESCRIPTION");
		Title.text += "\n";
		OutpostResourceChange resources = change.Resources;
		foreach (IGrouping<OutpostMaterialType, OutpostMaterialType> item in from m in resources.Resources.ToMaterialList()
			group m by m)
		{
			Title.text += $"{item.Count()} {item.Key}, ";
		}
		OutpostResources resources2 = change.Resources.Resources;
		int num = change.Resources.Total;
		foreach (ResourceDistribution item2 in resourceDistribution)
		{
			resources2 -= item2.Resources;
			num -= item2.Resources.Total;
		}
		bool interactable = num == 0;
		int num2 = 0;
		foreach (CharacterDistributionController characterDistributionController in GetCharacterDistributionControllers(resourceDistribution.Count()))
		{
			CharacterState character = resourceDistribution[num2].GetCharacter(state);
			if (character != null)
			{
				CharacterDistributionController.ResourceSourceDTO resourceDTO = new CharacterDistributionController.ResourceSourceDTO(character);
				bool done = characterDistributionController.ResourceDistr != null && change.Resources.Total - characterDistributionController.ResourceDistr.Resources.Total <= 0;
				characterDistributionController.Init(resourceDistribution, resourceDTO, change.Resources, change.ChangeType, enabled: true, done);
				characterDistributionController.OnResourceDistributionChanged = (ResourceDistribution c) =>
				{
					OnResourceDistributionChanged?.Invoke(c);
				};
				num2++;
			}
		}
		m_ReadyUsersListContainer.Init(player.ReadyUsers);
		DoneButton.interactable = interactable;
		CancelButton.gameObject.SetActive(value: true);
		DoneButton.gameObject.SetActive(value: true);
		CancelButton.SetTextKey("GUI_BACK");
		DoneButton.SetTextKey("GUI_CONFIRM");
	}

	private IEnumerable<CharacterDistributionController> GetCharacterDistributionControllers(int count)
	{
		HelperTools.NormalizePool(ref _distributionControllers, DistributionControllerPrefab.gameObject, ItemsRoot, count);
		for (int i = 0; i < count; i++)
		{
			yield return _distributionControllers[i];
		}
	}
}
