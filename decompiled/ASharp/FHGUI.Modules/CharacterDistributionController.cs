using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class CharacterDistributionController : MonoBehaviour
{
	public class ResourceSourceDTO
	{
		public string Name;

		public OutpostResources Resources;

		public bool ShowIfEmpty;

		public CharacterID CharacterID;

		public ResourceSourceDTO(CharacterState character)
		{
			CharacterID = character.ID;
			Name = character.Name;
			Resources = character.Resources;
		}

		public ResourceSourceDTO(OutpostState outpost)
		{
			Name = "Frosthaven";
			Resources = outpost.Resources;
		}
	}

	public class GoldSourceDTO
	{
		public string Name;

		public int Gold;

		public bool ShowIfEmpty;

		public CharacterID CharacterID;

		public GoldSourceDTO(CharacterState character)
		{
			CharacterID = character.ID;
			Name = character.Name;
			Gold = (int)character.Gold;
		}

		public GoldSourceDTO(CampaignState gameState)
		{
			Name = "Frosthaven";
			Gold = (int)gameState.TotalPartyGold;
		}
	}

	public TMP_Text CharacterNameLabel;

	private GoldDistribution _goldDistr;

	private ResourceDistribution _resourceDistr;

	private CharacterItemDistribution _itemDistribution;

	private BuildingTypeDistribution _buildingTypeDistribution;

	private PetDistribution _petDistribution;

	public Action<GoldDistribution> OnGoldDistributionChanged;

	public Action<ResourceDistribution> OnResourceDistributionChanged;

	public Action<CharacterItemDistribution> OnItemDistributionChanged;

	public Action<BuildingTypeDistribution> OnBuildingDistributionChanged;

	public Action<PetDistribution> OnPetDistributionChanged;

	public Action OnDistributionChanged;

	public DistributionController DistributionControllerPrefab;

	private List<DistributionController> _distributionControllers = new List<DistributionController>();

	public int ModValue = 1;

	public ResourceDistribution ResourceDistr => _resourceDistr;

	private void Awake()
	{
		DistributionController[] componentsInChildren = GetComponentsInChildren<DistributionController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	public void Init(List<CharacterItemDistribution> itemDistributions, CharacterState character, ItemFilterFlags itemFilter, CampaignController controller, bool enabled, bool done)
	{
		ModValue = 1;
		_itemDistribution = itemDistributions.First((CharacterItemDistribution d) => d.CharacterID == character.ID);
		itemDistributions.Any((CharacterItemDistribution i) => i.Item != null && (itemFilter == ItemFilterFlags.None || i.Item.HasFilterFlag(itemFilter, controller)));
		bool isGain = false;
		CharacterNameLabel.text = character.Name;
		List<ResourceMod> resourceMods = (from item in controller.CharacterController.GetOwnedItems(character)
			where itemFilter == ItemFilterFlags.None || item.HasFilterFlag(itemFilter, controller)
			select new ResourceMod
			{
				Name = item.Name,
				Value = 1,
				Mod = ((_itemDistribution.Item == item) ? 1 : 0),
				MaxMod = ((!itemDistributions.Any((CharacterItemDistribution i) => i.Item == item && i != _itemDistribution)) ? 1 : 0),
				Change = (int mod) =>
				{
					if (mod < 0)
					{
						_itemDistribution.Item = null;
					}
					else
					{
						_itemDistribution.Item = item;
					}
					OnItemDistributionChanged?.Invoke(_itemDistribution);
				}
			}).ToList();
		SetBindings(resourceMods, isGain, enabled, done);
	}

	public void Init_Event_SelectBuildingsToProtect(BuildingTypeDistribution buildingDistribution, OutpostState outpost, bool enabled, bool done)
	{
		ModValue = 1;
		_buildingTypeDistribution = buildingDistribution;
		bool isGain = true;
		CharacterNameLabel.text = "Frosthaven";
		List<ResourceMod> resourceMods = (from item in outpost.Buildings
			where item.CanBeDamaged
			select new ResourceMod
			{
				Name = item.Def.Name,
				Value = 0,
				Mod = (_buildingTypeDistribution.BuildingTypeIDs.Contains(item.Def.ID) ? 1 : 0),
				MaxMod = 1,
				Change = (int mod) =>
				{
					if (mod < 0)
					{
						_buildingTypeDistribution.BuildingTypeIDs.Remove(item.Def.ID);
					}
					else
					{
						_buildingTypeDistribution.BuildingTypeIDs.Add(item.Def.ID);
					}
					OnBuildingDistributionChanged?.Invoke(_buildingTypeDistribution);
				}
			}).ToList();
		SetBindings(resourceMods, isGain, enabled, done);
	}

	public void Init_Event_LosePet(PetDistribution petDistribution, PetController petController, bool enabled, bool done)
	{
		ModValue = 1;
		_petDistribution = petDistribution;
		bool isGain = false;
		CharacterNameLabel.text = "Frosthaven";
		List<ResourceMod> resourceMods = (from item in petController.GetOwnedPets()
			select new ResourceMod
			{
				Name = LocalizationManager.GetTranslation(item.Name),
				Value = 1,
				Mod = (_petDistribution.PetIDs.Contains(item.ID) ? 1 : 0),
				MaxMod = 1,
				Change = (int mod) =>
				{
					if (mod < 0)
					{
						_petDistribution.PetIDs.Remove(item.ID);
					}
					else
					{
						_petDistribution.PetIDs.Add(item.ID);
					}
					OnPetDistributionChanged?.Invoke(_petDistribution);
				}
			}).ToList();
		SetBindings(resourceMods, isGain, enabled, done);
	}

	public void Init_EventWR24OptionA(List<CharacterItemDistribution> itemDistributions, CharacterState character, CampaignController controller, bool enabled, bool done)
	{
		ModValue = 1;
		_itemDistribution = itemDistributions.First((CharacterItemDistribution d) => d.CharacterID == character.ID);
		bool isGain = false;
		CharacterNameLabel.text = character.Name;
		List<ResourceMod> resourceMods = (from item in OutpostEventController.GetValidItemsForEvent_WR24OptionA(character, controller)
			select new ResourceMod
			{
				Name = item.Name,
				Value = 1,
				Mod = ((_itemDistribution.Item == item) ? 1 : 0),
				MaxMod = 1,
				Change = (int mod) =>
				{
					if (mod < 0)
					{
						_itemDistribution.Item = null;
					}
					else
					{
						_itemDistribution.Item = item;
					}
					OnItemDistributionChanged?.Invoke(_itemDistribution);
				}
			}).ToList();
		SetBindings(resourceMods, isGain, enabled, done);
	}

	public void Init(List<CharacterItemDistribution> itemDistributions, CharacterState character, List<ItemDef> items, TradingController tradingController, bool enabled, bool done)
	{
		ModValue = 1;
		_itemDistribution = itemDistributions.First((CharacterItemDistribution d) => d.CharacterID == character.ID);
		bool isGain = true;
		CharacterNameLabel.text = character.Name;
		List<ResourceMod> buyableItemBindings = GetBuyableItemBindings(items, itemDistributions, tradingController, character);
		SetBindings(buyableItemBindings, isGain, enabled, done);
	}

	public void Init(List<CharacterItemDistribution> itemDistributions, CharacterState character, List<ItemDef> items, bool enabled, bool done)
	{
		ModValue = 1;
		_itemDistribution = itemDistributions.First((CharacterItemDistribution d) => d.CharacterID == character.ID);
		bool isGain = true;
		CharacterNameLabel.text = character.Name;
		List<ResourceMod> resourceMods = items.Select((ItemDef item) => new ResourceMod
		{
			Name = item.Name,
			Value = 0,
			Mod = ((_itemDistribution.Item == item) ? 1 : 0),
			MaxMod = ((!itemDistributions.Any((CharacterItemDistribution i) => i.Item == item && i != _itemDistribution)) ? 1 : 0),
			Change = (int mod) =>
			{
				if (mod < 0)
				{
					_itemDistribution.Item = null;
				}
				else
				{
					_itemDistribution.Item = item;
				}
				OnItemDistributionChanged?.Invoke(_itemDistribution);
			}
		}).ToList();
		SetBindings(resourceMods, isGain, enabled, done);
	}

	public void Init(List<GoldDistribution> goldDistributions, GoldSourceDTO source, int total, bool isGain, bool enabled, bool done)
	{
		ModValue = 1;
		_goldDistr = goldDistributions.First((GoldDistribution c) => c.CharacterID == source.CharacterID);
		int maxMod = total - goldDistributions.Sum((GoldDistribution g) => g.Gold) + _goldDistr.Gold;
		CharacterNameLabel.text = source.Name;
		foreach (DistributionController distributionController in GetDistributionControllers(1))
		{
			SetResource(new ResourceMod
			{
				Name = "Gold",
				Value = source.Gold,
				Mod = _goldDistr.Gold,
				MaxMod = maxMod,
				Change = (int mod) =>
				{
					_goldDistr.Gold += mod;
					OnGoldDistributionChanged?.Invoke(_goldDistr);
				}
			}, distributionController, isGain, enabled, done);
		}
	}

	public void Init(ResourceSourceDTO source, List<ResourceMod> bindings, bool isGain, bool enabled, bool done)
	{
		ModValue = 1;
		CharacterNameLabel.text = source.Name;
		SetBindings(bindings, isGain, enabled, done);
	}

	public void Init_BuildingMaterialDistribution(BuildingMaterialDistribution buildingMaterialDistribution, GoldDistribution characterGoldDistribution, ResourceDistribution characterMaterialDistribution, CampaignState state, uint goldCost, OutpostResourceChange materialCost, bool enabled, bool done, bool enableInspiration)
	{
		ModValue = 1;
		_resourceDistr = characterMaterialDistribution;
		_goldDistr = characterGoldDistribution;
		CharacterState character = characterMaterialDistribution.GetCharacter(state);
		CharacterNameLabel.text = character.Name;
		OutpostResourceChange outpostResourceChange = materialCost;
		if (outpostResourceChange.Resources.HasNegativeValue)
		{
			outpostResourceChange.AnyMaterial += outpostResourceChange.Resources.TotalMaterials;
			outpostResourceChange.Resources = default;
		}
		OutpostResources outpostResources = default;
		foreach (ResourceDistribution item in buildingMaterialDistribution.materialDistributions.Where((ResourceDistribution x) => x.CharacterID != character.ID))
		{
			outpostResources += item.Resources;
		}
		OutpostResources outpostResources2 = materialCost.Resources - outpostResources;
		List<ResourceMod> list = new List<ResourceMod>();
		if (goldCost != 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Gold",
				Value = (int)character.Gold,
				Mod = _goldDistr.Gold,
				MaxMod = (int)goldCost,
				Change = (int mod) =>
				{
					_goldDistr.Gold += mod;
				}
			});
		}
		if (materialCost.AnyMaterial > 0 || materialCost.Resources.Lumber > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Lumber",
				Value = character.Resources.Lumber,
				Mod = _resourceDistr.Resources.Lumber,
				MaxMod = outpostResourceChange.AnyMaterial + Math.Min(outpostResources2.Lumber, outpostResourceChange.Resources.Lumber),
				Change = (int mod) =>
				{
					_resourceDistr.Resources.Lumber += mod;
				}
			});
		}
		if (materialCost.AnyMaterial > 0 || materialCost.Resources.Metal > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Metal",
				Value = character.Resources.Metal,
				Mod = _resourceDistr.Resources.Metal,
				MaxMod = outpostResourceChange.AnyMaterial + Math.Min(outpostResources2.Metal, outpostResourceChange.Resources.Metal),
				Change = (int mod) =>
				{
					_resourceDistr.Resources.Metal += mod;
				}
			});
		}
		if (materialCost.AnyMaterial > 0 || materialCost.Resources.Hide > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Hide",
				Value = character.Resources.Hide,
				Mod = _resourceDistr.Resources.Hide,
				MaxMod = outpostResourceChange.AnyMaterial + Math.Min(outpostResources2.Hide, outpostResourceChange.Resources.Hide),
				Change = (int mod) =>
				{
					_resourceDistr.Resources.Hide += mod;
				}
			});
		}
		if (state.Inspiration != 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Inspiration",
				Value = (int)state.Inspiration,
				Mod = (int)buildingMaterialDistribution.Inspiration,
				MaxMod = outpostResourceChange.Total,
				Change = (int mod) =>
				{
					buildingMaterialDistribution.Inspiration += (uint)mod;
				}
			});
		}
		SetBindings(list, isGain: false, enabled, done);
	}

	private List<ResourceMod> GetBuyableItemBindings(List<ItemDef> items, List<CharacterItemDistribution> itemDistributions, TradingController tradingController, CharacterState character)
	{
		return (from item in items.Distinct()
			select new ResourceMod
			{
				Name = item.Name,
				Value = 0,
				Mod = ((_itemDistribution.Item == item) ? 1 : 0),
				MaxMod = ((itemDistributions.Where((CharacterItemDistribution x) => x.Item == item).Count() != items.Where((ItemDef x) => x == item).Count() && !itemDistributions.Any((CharacterItemDistribution i) => !tradingController.CanBuyItem(character, item, travellingMerchant: false))) ? 1 : 0),
				Change = (int mod) =>
				{
					if (mod < 0)
					{
						_itemDistribution.Item = null;
					}
					else
					{
						_itemDistribution.Item = item;
					}
					OnItemDistributionChanged?.Invoke(_itemDistribution);
				},
				Price = tradingController.GetDiscountedCost(item, tradingController.CampaignController.State.BuyItemGoldDiscount)
			}).ToList();
	}

	private List<ResourceMod> GetResourceDistributionBindings(List<ResourceDistribution> resourceDistributions, ResourceSourceDTO resourceDTO, OutpostResourceChange change, DistributionChangeType changeType, TradingController tradingController)
	{
		ResourceDistribution resourceDistribution = _resourceDistr;
		OutpostResourceChange outpostResourceChange = change;
		if (changeType == DistributionChangeType.LoseEach || changeType == DistributionChangeType.GainEach)
		{
			outpostResourceChange.Resources -= _resourceDistr.Resources;
		}
		else
		{
			foreach (ResourceDistribution resourceDistribution2 in resourceDistributions)
			{
				if (resourceDistribution2 != resourceDistribution)
				{
					outpostResourceChange.Resources -= resourceDistribution2.Resources;
				}
			}
		}
		if (outpostResourceChange.Resources.HasNegativeValue)
		{
			outpostResourceChange.AnyHerb += outpostResourceChange.Resources.TotalHerbs;
			outpostResourceChange.AnyMaterial += outpostResourceChange.Resources.TotalMaterials;
			outpostResourceChange.Resources = default;
		}
		if (tradingController != null)
		{
			resourceDistribution.GetCharacter(tradingController.CampaignController.State);
		}
		List<ResourceMod> list = new List<ResourceMod>();
		int price = -1;
		if (resourceDTO.ShowIfEmpty || change.AnyMaterial > 0 || change.Resources.Lumber > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Lumber",
				Value = resourceDTO.Resources.Lumber,
				Mod = resourceDistribution.Resources.Lumber,
				MaxMod = outpostResourceChange.AnyMaterial + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Lumber : outpostResourceChange.Resources.Lumber),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Lumber += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyMaterial > 0 || change.Resources.Metal > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Metal",
				Value = resourceDTO.Resources.Metal,
				Mod = resourceDistribution.Resources.Metal,
				MaxMod = outpostResourceChange.AnyMaterial + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Metal : outpostResourceChange.Resources.Metal),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Metal += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyMaterial > 0 || change.Resources.Hide > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Hide",
				Value = resourceDTO.Resources.Hide,
				Mod = resourceDistribution.Resources.Hide,
				MaxMod = outpostResourceChange.AnyMaterial + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Hide : outpostResourceChange.Resources.Hide),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Hide += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Arrowvine > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Arrowvine",
				Value = resourceDTO.Resources.Arrowvine,
				Mod = resourceDistribution.Resources.Arrowvine,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Arrowvine : outpostResourceChange.Resources.Arrowvine),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Arrowvine += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Axenut > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Axenut",
				Value = resourceDTO.Resources.Axenut,
				Mod = resourceDistribution.Resources.Axenut,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Axenut : outpostResourceChange.Resources.Axenut),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Axenut += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Corpsecap > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Corpsecap",
				Value = resourceDTO.Resources.Corpsecap,
				Mod = resourceDistribution.Resources.Corpsecap,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Corpsecap : outpostResourceChange.Resources.Corpsecap),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Corpsecap += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Flamefruit > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Flamefruit",
				Value = resourceDTO.Resources.Flamefruit,
				Mod = resourceDistribution.Resources.Flamefruit,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Flamefruit : outpostResourceChange.Resources.Flamefruit),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Flamefruit += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Rockroot > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Rockroot",
				Value = resourceDTO.Resources.Rockroot,
				Mod = resourceDistribution.Resources.Rockroot,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Rockroot : outpostResourceChange.Resources.Rockroot),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Rockroot += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		if (resourceDTO.ShowIfEmpty || change.AnyHerb > 0 || change.Resources.Snowthistle > 0)
		{
			list.Add(new ResourceMod
			{
				Name = "Snowthistle",
				Value = resourceDTO.Resources.Snowthistle,
				Mod = resourceDistribution.Resources.Snowthistle,
				MaxMod = outpostResourceChange.AnyHerb + ((changeType == DistributionChangeType.GainEach) ? _resourceDistr.Resources.Snowthistle : outpostResourceChange.Resources.Snowthistle),
				Change = (int mod) =>
				{
					resourceDistribution.Resources.Snowthistle += mod;
					OnResourceDistributionChanged?.Invoke(resourceDistribution);
				},
				Price = price
			});
		}
		return list;
	}

	public void Init(List<ResourceDistribution> resourceDistributions, ResourceSourceDTO resourceDTO, OutpostResourceChange resourceChange, DistributionChangeType changeType, bool enabled, bool done)
	{
		ModValue = 1;
		_resourceDistr = resourceDistributions.First((ResourceDistribution d) => d.CharacterID == resourceDTO.CharacterID);
		CharacterNameLabel.text = resourceDTO.Name;
		List<ResourceMod> resourceDistributionBindings = GetResourceDistributionBindings(resourceDistributions, resourceDTO, resourceChange, changeType, null);
		SetBindings(resourceDistributionBindings, changeType == DistributionChangeType.Gain || changeType == DistributionChangeType.GainEach, enabled, done);
	}

	public void AddBinding(ResourceMod mod, bool isGain, bool enabled, bool done)
	{
		SetResource(mod, GetNextController(), isGain, enabled, done);
	}

	private void SetResource(ResourceMod resource, DistributionController distrController, bool isGain, bool enabled, bool done)
	{
		distrController.Init(resource.Name, resource.Value, resource.Mod, resource.MaxMod, isGain, resource.Price);
		distrController.SetEnabled(enabled);
		distrController.OnDistributionChanged = (int mod) =>
		{
			resource.Mod += mod * ModValue;
			resource.Change?.Invoke(mod * ModValue);
			distrController.Init(resource.Name, resource.Value, resource.Mod, resource.MaxMod, isGain, resource.Price);
			OnDistributionChanged?.Invoke();
		};
		distrController.Refresh(done);
	}

	private void SetBindings(List<ResourceMod> resourceMods, bool isGain, bool enabled, bool done)
	{
		int num = 0;
		foreach (DistributionController distributionController in GetDistributionControllers(resourceMods.Count))
		{
			ResourceMod resource = resourceMods[num];
			SetResource(resource, distributionController, isGain, enabled, done);
			num++;
		}
	}

	public void ResetControllers()
	{
		for (int i = 0; i < _distributionControllers.Count; i++)
		{
			_distributionControllers[i].gameObject.SetActive(value: false);
		}
	}

	private IEnumerable<DistributionController> GetDistributionControllers(int count)
	{
		HelperTools.NormalizePool(ref _distributionControllers, DistributionControllerPrefab.gameObject, transform, count);
		for (int i = 0; i < count; i++)
		{
			yield return _distributionControllers[i];
		}
	}

	private DistributionController GetNextController()
	{
		return GetDistributionControllers(_distributionControllers.Count((DistributionController x) => x.gameObject.activeInHierarchy) + 1).Last();
	}
}
