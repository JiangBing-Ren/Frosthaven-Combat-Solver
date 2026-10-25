using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHRL;
using GLOOM;

namespace FHGUI.Services;

public class PerksViewService : FHMapViewService
{
	public PerksViewService(FHMapView view)
		: base(view)
	{
	}

	public List<AttackModifierDTO> GetActiveAttackModifiers(CharacterDataDTO characterDto)
	{
		CharacterState characterState = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		return GetActiveAttackModifiers(characterState);
	}

	public List<AttackModifierDTO> GetActiveAttackModifiers(CharacterState characterState)
	{
		List<AttackModifierDTO> list = new List<AttackModifierDTO>();
		Dictionary<CharacterAttackModifierDef, int> dictionary = new Dictionary<CharacterAttackModifierDef, int>();
		foreach (CharacterAttackModifierDef attackModifier in characterState.AttackModifiers)
		{
			if (!dictionary.TryAdd(attackModifier, 1))
			{
				dictionary[attackModifier]++;
			}
		}
		foreach (KeyValuePair<CharacterAttackModifierDef, int> item in dictionary)
		{
			list.Add(AttackModifierDTOFromDef(item.Key, item.Value));
		}
		return list;
	}

	public AttackModifierDTO AttackModifierDTOFromDef(CharacterAttackModifierDef def, int quantity)
	{
		return new AttackModifierDTO(def, quantity);
	}

	public List<PerkDTO> GetAvailablePerks(CharacterDataDTO characterDto, bool includeCompleted = true)
	{
		CharacterState character = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		return GetAvailablePerks(character);
	}

	public List<PerkDTO> GetAvailablePerks(CharacterState character, bool includeCompleted = true, bool stacked = true)
	{
		List<PerkDTO> list = new List<PerkDTO>();
		foreach (PerkDef item in from def in _view.GameController.CharacterController.GetClassPerks(character.ClassDef)
			orderby def.ID
			select def)
		{
			int perkInstancesCount = _view.GameController.CharacterController.GetPerkInstancesCount(item, character);
			int num = item.Available - perkInstancesCount;
			int num2 = ((stacked || (includeCompleted && num == 0)) ? 1 : num);
			if (num2 > 0)
			{
				for (int num3 = 0; num3 < num2; num3++)
				{
					PerkDTO perkDTO = PerkDTOFromDef(item, character, perkInstancesCount);
					perkDTO.CostKey = "GUI_PERK_COST_SHORT";
					list.Add(perkDTO);
				}
			}
		}
		return list;
	}

	public List<PerkDTO> GetTacticalPerks(CharacterDataDTO characterDto)
	{
		CharacterState character = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		return GetTacticalPerks(character);
	}

	public List<PerkDTO> GetTacticalPerks(CharacterState character)
	{
		List<PerkDTO> list = new List<PerkDTO>();
		List<PerkDef> list2 = _view.GameController.CharacterController.GetLearnedPerks(character).ToList();
		foreach (PerkDef def in list2)
		{
			if (def.Tactical)
			{
				int unlockedCount = list2.Count((PerkDef cDef) => cDef.ID == def.ID);
				list.Add(PerkDTOFromDef(def, character, unlockedCount));
			}
		}
		return list;
	}

	public List<PerkDTO> GetLearnedPerks(CharacterDataDTO characterDto)
	{
		CharacterState character = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		return GetLearnedPerks(character);
	}

	public List<PerkDTO> GetLearnedPerks(CharacterState character)
	{
		List<PerkDTO> list = new List<PerkDTO>();
		List<PerkDef> list2 = _view.GameController.CharacterController.GetLearnedPerks(character).ToList();
		foreach (PerkDef def in list2)
		{
			int unlockedCount = list2.Count((PerkDef cDef) => cDef.ID == def.ID);
			list.Add(PerkDTOFromDef(def, character, unlockedCount));
		}
		return list;
	}

	public List<AttackModifierDTO> GetPerkAttackModifiers(List<string> ids)
	{
		if (ids == null)
		{
			return null;
		}
		DefRepository defRepo = Choreographer.DefRepo;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (string id in ids)
		{
			if (!dictionary.TryAdd(id, 1))
			{
				dictionary[id]++;
			}
		}
		List<AttackModifierDTO> list = new List<AttackModifierDTO>();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			CharacterAttackModifierDef def = defRepo.AttackModifiers[new CharacterAttackModifierID(item.Key)];
			list.Add(AttackModifierDTOFromDef(def, item.Value));
		}
		return list;
	}

	public IEnumerable<string> GetUnlockHeroPerksHintsLocalized(CharacterState character, PerkDTO perkDTO)
	{
		bool allPerksUnlocked = !HasUnlockablePerks(character);
		if (allPerksUnlocked)
		{
			yield return LocalizationManager.GetTranslation("GUI_PERKS_ALL_HEROPERKS_UNLOCKED");
		}
		if (!perkDTO.IsComplete && !perkDTO.Affordable)
		{
			yield return LocalizationManager.GetTranslation("GUI_PERKS_INSUFFICIENT_PERKPOINTS");
		}
		if (perkDTO.IsComplete && !allPerksUnlocked)
		{
			yield return LocalizationManager.GetTranslation("GUI_PERKS_ALREADY_OWNED");
		}
	}

	public bool HasUnlockablePerks(CharacterState character)
	{
		if (character == null)
		{
			return false;
		}
		foreach (PerkDef classPerk in _view.GameController.CharacterController.GetClassPerks(character.ClassDef))
		{
			if (_view.GameController.CharacterController.GetAvailablePerkInstancesCount(classPerk, character) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<AttackModifierDTO> GetActiveTownAttackModifiers()
	{
		Dictionary<TownGuardAttackModifierDef, int> dictionary = new Dictionary<TownGuardAttackModifierDef, int>();
		List<TownGuardAttackModifierDef> list = _view.GameState.Outpost.TownGuardDeck.OrderBy((TownGuardAttackModifierDef def) => def.ID).ToList();
		list.Sort(Compare);
		foreach (TownGuardAttackModifierDef item in list)
		{
			if (!dictionary.TryAdd(item, 1))
			{
				dictionary[item]++;
			}
		}
		List<AttackModifierDTO> list2 = new List<AttackModifierDTO>();
		foreach (KeyValuePair<TownGuardAttackModifierDef, int> item2 in dictionary)
		{
			list2.Add(AttackModifierDTOFromDef(item2.Key, item2.Value));
		}
		return list2;
		static int Compare(TownGuardAttackModifierDef x, TownGuardAttackModifierDef y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return 1;
			}
			if (y == null)
			{
				return -1;
			}
			if (x.Type == y.Type && x.Type == TownGuardModType.Modifier)
			{
				int value = x.Value;
				int value2 = y.Value;
				if (x.IsMultiplyMathModifier)
				{
					if (y.IsMultiplyMathModifier)
					{
						return value.CompareTo(value2);
					}
					if (value > 0)
					{
						return 1;
					}
					return -1;
				}
				if (y.IsMultiplyMathModifier)
				{
					if (value2 > 0)
					{
						return 1;
					}
					return -1;
				}
				return value.CompareTo(value2);
			}
			if (x.Type == y.Type)
			{
				return 0;
			}
			if (x.Type == TownGuardModType.Failure)
			{
				return -1;
			}
			if (y.Type == TownGuardModType.Success)
			{
				return -1;
			}
			return 0;
		}
	}

	public List<PerkDTO> GetAvailableTownPerks()
	{
		List<PerkDTO> list = new List<PerkDTO>();
		bool canAffordNewTownGuardPerks = _view.GameController.OutpostController.CanAffordNewTownGuardPerks;
		foreach (TownGuardPerkDef allTownGuardPerk in _view.GameController.OutpostController.GetAllTownGuardPerks())
		{
			int townGuardPerkMaxCount = _view.GameController.OutpostController.GetTownGuardPerkMaxCount(allTownGuardPerk);
			int availableTownGuardPerkCount = _view.GameController.OutpostController.GetAvailableTownGuardPerkCount(allTownGuardPerk);
			int unlockedCount = townGuardPerkMaxCount - availableTownGuardPerkCount;
			PerkDTO item = PerkDTOFromDef(allTownGuardPerk, canAffordNewTownGuardPerks, townGuardPerkMaxCount, unlockedCount);
			list.Add(item);
		}
		return list;
	}

	public uint GetTownGuardPerksPoints()
	{
		return _view.GameController.OutpostController.GetTownGuardPerksPoints();
	}

	public uint GetTownGuardPerkChecks()
	{
		return _view.GameController.OutpostController.GetTownGuardPerkChecks();
	}

	public List<AttackModifierDTO> GetTownGuardAttackModifiers(List<string> ids)
	{
		if (ids == null)
		{
			return null;
		}
		DefRepository defRepo = Choreographer.DefRepo;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (string id in ids)
		{
			if (!dictionary.TryAdd(id, 1))
			{
				dictionary[id]++;
			}
		}
		List<AttackModifierDTO> list = new List<AttackModifierDTO>();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			TownGuardAttackModifierDef def = defRepo.TownGuardAttackModifierDefs[new TownGuardAttackModifierID(item.Key)];
			list.Add(AttackModifierDTOFromDef(def, item.Value));
		}
		return list;
	}

	public IEnumerable<string> GetUnlockTownGuardPerksHintsLocalized(PerkDTO perkDTO)
	{
		bool flag = true;
		foreach (TownGuardPerkDef allTownGuardPerk in _view.GameController.OutpostController.GetAllTownGuardPerks())
		{
			if (_view.GameController.OutpostController.GetAvailableTownGuardPerkCount(allTownGuardPerk) > 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			yield return LocalizationManager.GetTranslation("GUI_TOWNGUARD_ALL_PERKS_UNLOCKED");
		}
		if (!perkDTO.IsComplete && !perkDTO.Affordable)
		{
			yield return LocalizationManager.GetTranslation("GUI_PERKS_INSUFFICIENT_PERKPOINTS");
		}
		if (perkDTO.IsComplete)
		{
			yield return LocalizationManager.GetTranslation("GUI_PERKS_ALREADY_OWNED");
		}
		if (!_view.IsPartyLeader)
		{
			yield return LocalizationManager.GetTranslation("GUI_TOWNGUARD_LEADERONLY_UNLOCK");
		}
	}

	public AttackModifierDTO AttackModifierDTOFromDef(TownGuardAttackModifierDef def, int quantity, int consumedQuantity = 0)
	{
		return new AttackModifierDTO(def, quantity, consumedQuantity);
	}

	public PerkDTO PerkDTOFromDef(PerkDef def, CharacterState character, int unlockedCount)
	{
		PerkDTO perkDTO = new PerkDTO(def);
		perkDTO.HasAbility = def.Tactical;
		perkDTO.Affordable = character.PerkMarks >= perkDTO.Cost;
		perkDTO.UnlockedCount = unlockedCount;
		if (perkDTO.Icon != null)
		{
			return perkDTO;
		}
		List<string> ids = (perkDTO.ModifiersToAdd.Any() ? perkDTO.ModifiersToAdd : perkDTO.ModifiersToRemove);
		List<AttackModifierDTO> perkAttackModifiers = GetPerkAttackModifiers(ids);
		if (perkAttackModifiers != null && perkAttackModifiers.Any())
		{
			perkDTO.DisplayModifier = perkAttackModifiers[0];
		}
		return perkDTO;
	}

	public PerkDTO PerkDTOFromDef(TownGuardPerkDef def, bool affordable, int maxCount, int unlockedCount)
	{
		return new PerkDTO(def)
		{
			Affordable = affordable,
			MaxCount = maxCount,
			UnlockedCount = unlockedCount,
			CostKey = "GUI_TOWNGUARD_PERK_COST_SHORT"
		};
	}

	public bool CanLearnPerk(CharacterDataDTO characterDto, PerkDTO perkDto)
	{
		CharacterState character = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		PerkDef value = _view.GameController.DefRepository.Perks.Single((KeyValuePair<PerkID, PerkDef> p) => p.Key.ID == perkDto.ID).Value;
		return _view.GameController.CharacterController.CanLearnPerk(character, value);
	}

	public bool CanLearnTownGuardPerk(PerkDTO perkDto)
	{
		if (!uint.TryParse(perkDto.ID, out var result))
		{
			Log.GUIWarning("Unknown TownGuard Perk Id " + perkDto.ID);
			return false;
		}
		return _view.GameController.OutpostController.CanLearnTownGuardPerk(DefRepo.TownGuardPerkDefs[new TownGuardPerkID(result)]);
	}

	public void LearnCharacterPerk(CharacterDataDTO characterDto, PerkDTO perkDto)
	{
		CharacterState character = _view.GameController.State.AllCharacters.Single((CharacterState s) => s.ID == characterDto.CharacterID);
		PerkDef value = _view.GameController.DefRepository.Perks.Single((KeyValuePair<PerkID, PerkDef> p) => p.Key.ID == perkDto.ID).Value;
		LearnCharacterPerk(character, value);
		_view.RefreshView();
	}

	public void LearnCharacterPerk(CharacterState character, PerkDef perk)
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.LearnPerk, character.ID, perk);
	}

	public void LearnTownGuardPerk(PerkDTO perkDto)
	{
		if (!uint.TryParse(perkDto.ID, out var result))
		{
			Log.GUIWarning("Unknown TownGuard Perk Id " + perkDto.ID);
			return;
		}
		TownGuardPerkDef def = DefRepo.TownGuardPerkDefs[new TownGuardPerkID(result)];
		LearnTownGuardPerk(def);
	}

	public void LearnTownGuardPerk(TownGuardPerkDef def)
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Outpost.AddTownGuardPerk, def, false);
	}
}
