using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.DTO;
using FHRL;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Common;

public static class UIPlayerCharacterUtils
{
	public static (int, int) GetXp(CMapCharacter charData, CPlayerActor cPlayer = null)
	{
		int item = 80;
		int item2 = 0;
		cPlayer = cPlayer ?? charData.GetActor();
		if (charData != null)
		{
			GetExpRanges(charData, out var prevLevelThreshold, out var nextLevelThreshold);
			item = nextLevelThreshold - prevLevelThreshold;
			item2 = charData.EXP + cPlayer.XP - prevLevelThreshold;
		}
		return (item2, item);
	}

	public static void GetExpRanges(CMapCharacter charData, out int prevLevelThreshold, out int nextLevelThreshold)
	{
		int num = charData.Level;
		do
		{
			prevLevelThreshold = ((num > 0) ? charData.GetXPThreshold(num - 1) : 0);
			nextLevelThreshold = ((num >= 0) ? charData.GetXPThreshold(num) : 0);
			num--;
		}
		while (num >= -1 && (prevLevelThreshold == int.MaxValue || nextLevelThreshold == int.MaxValue));
	}

	public static CMapCharacter GetCharacter(CPlayerActor actor)
	{
		return AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == actor.Class.ID);
	}

	public static Sprite GetPassivePerkIcon(CharacterPerk perk)
	{
		if ((perk.Perk.Flags & ScenarioRuleLibrary.YML.EPerkFlag.IgnoreNegativeItemEffects) != 0)
		{
			return UIInfoTools.Instance.IgnoreNegativeItemEffectsIcon;
		}
		if ((perk.Perk.Flags & ScenarioRuleLibrary.YML.EPerkFlag.IgnoreNegativeScenarioEffects) != 0)
		{
			return UIInfoTools.Instance.IgnoreNegativeScenarioEffectsIcon;
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(perk.CharacterID);
	}

	public static Sprite GetPerkIcon(PerksYMLData perk, string classID)
	{
		if ((perk.Flags & ScenarioRuleLibrary.YML.EPerkFlag.IgnoreNegativeItemEffects) != 0)
		{
			return UIInfoTools.Instance.IgnoreNegativeItemEffectsIcon;
		}
		if ((perk.Flags & ScenarioRuleLibrary.YML.EPerkFlag.IgnoreNegativeScenarioEffects) != 0)
		{
			return UIInfoTools.Instance.IgnoreNegativeScenarioEffectsIcon;
		}
		return UIInfoTools.Instance.GetPerkIcon(perk.ID);
	}

	public static UIMultiplayerUser.Data? GetPlayerData(CPlayerActor actor)
	{
		return GetPlayerData(actor.Class.ID);
	}

	public static UIMultiplayerUser.Data? GetPlayerData(string classID)
	{
		if (!FFSNetwork.IsOnline)
		{
			return null;
		}
		return GetPlayerData(ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(classID))?.Controller);
	}

	public static UIMultiplayerUser.Data? GetPlayerData(int slotIndex)
	{
		if (!FFSNetwork.IsOnline)
		{
			return null;
		}
		return GetPlayerData(PlayerRegistry.GetPlayerAssignedToSlot(PartySlotID.FromIndex(slotIndex)));
	}

	public static UIMultiplayerUser.Data? GetPlayerData(NetworkPlayer player)
	{
		if (!FFSNetwork.IsOnline)
		{
			return null;
		}
		if (player == null)
		{
			return null;
		}
		return new UIMultiplayerUser.Data
		{
			Id = player.PlayerID.ID,
			Username = player.Username,
			Avatar = player.Avatar,
			IsLeader = !player.IsClient
		};
	}

	public static IEnumerable<HeroSummonGeneralDTO> GetSummons(CPlayerActor actor)
	{
		return from summon in actor.GetAllHeroSummons()
			select new HeroSummonGeneralDTO(summon, getStatuses: false);
	}

	public static List<Tuple<string, string, Sprite>> GetPassiveAttributes(CPlayerActor actor)
	{
		List<Tuple<string, string, Sprite>> list = new List<Tuple<string, string, Sprite>>();
		if (actor.CarriedQuestItems.Count == 0)
		{
			return list;
		}
		foreach (CObjectProp carriedQuestItem in actor.CarriedQuestItems)
		{
			if (carriedQuestItem.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				list.Add(new Tuple<string, string, Sprite>(FHUILocFormatting.GetScenarioQuestItemName(carriedQuestItem), FHUILocFormatting.GetScenarioQuestItemDesc(carriedQuestItem), UIInfoTools.Instance.CarryableQuestItemIconSprite));
			}
		}
		return list;
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CPlayerActor actor)
	{
		CMapCharacter character = GetCharacter(actor);
		if (character != null)
		{
			return GetEnhancements(character);
		}
		return Array.Empty<CEnhancement>();
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CPlayerActor actor, int cardID)
	{
		CMapCharacter character = GetCharacter(actor);
		if (character != null)
		{
			return GetEnhancements(character, cardID);
		}
		return Array.Empty<CEnhancement>();
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CMapCharacter character)
	{
		return character.Enhancements.ToArray();
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CMapCharacter character, int cardID)
	{
		return from e in GetEnhancements(character)
			where e.AbilityCardID == cardID
			select e;
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CharacterState character)
	{
		return GetEnhancements(character.ClassDef);
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CharacterID characterID)
	{
		FHMapChoreographer instanceFast = Singleton<FHMapChoreographer>.InstanceFast;
		if ((object)instanceFast != null && instanceFast.GameController.CharacterController.TryGetCharacter(characterID, out var character))
		{
			return GetEnhancements(character);
		}
		return Enumerable.Empty<CEnhancement>();
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CharacterDataDTO character)
	{
		return GetEnhancements(character.Class);
	}

	public static IEnumerable<CEnhancement> GetEnhancements(CharacterClassDef characterDef)
	{
		if (Singleton<FHMapChoreographer>.InstanceFast == null)
		{
			yield break;
		}
		CampaignController gameController = Singleton<FHMapChoreographer>.InstanceFast.GameController;
		DefRepository defRepo = gameController.DefRepository;
		IEnumerable<ClassCardDef> enumerable = defRepo.ClassCards.Values.Where((ClassCardDef s) => s.ClassID == characterDef.ID);
		foreach (ClassCardDef item in enumerable)
		{
			CEnhancement[] array = GetEnhancementsForCard((int)item.ID.ID, defRepo).ToArray();
			CEnhancement[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				yield return array2[num];
			}
		}
	}

	public static IEnumerable<CEnhancement> GetEnhancementsForCard(int abilityCardID, DefRepository defRepo = null)
	{
		if (defRepo == null)
		{
			defRepo = Singleton<FHMapChoreographer>.InstanceFast?.DefRepo;
		}
		if (defRepo == null)
		{
			yield break;
		}
		CampaignState state = Singleton<FHMapChoreographer>.InstanceFast.GameState;
		ClassCardDef cardDef = defRepo.ClassCards.Values.FirstOrDefault((ClassCardDef c) => c.ID.ID == abilityCardID);
		if (cardDef == null)
		{
			yield break;
		}
		CAbilityCard card = null;
		foreach (ClassCardDef.EnhacementSlot enhancementSlot in cardDef.EnhancementSlots)
		{
			ClassCardState state2 = state.ClassCardModifications.FirstOrDefault((ClassCardState m) => m.CardDef == cardDef);
			CEnhancement cEnhancement = FHGlue.CreateCEnhancement(new CardEnhacementSlotQuery(cardDef, enhancementSlot), state2);
			if (card == null)
			{
				card = cEnhancement.AbilityCard;
			}
			if (cEnhancement.Enhancement != EEnhancement.NoEnhancement)
			{
				yield return cEnhancement;
			}
		}
	}

	public static IEnumerable<CEnhancement> GetEnhancementsForCard(CharacterClassDef def, int abilityCardID)
	{
		return GetEnhancementsForCard(abilityCardID);
	}

	public static IEnumerable<CEnhancement> GetEnhancementsForCard(CMapCharacter character, int abilityCardID)
	{
		return character.Enhancements.Where((CEnhancement e) => e.AbilityCardID == abilityCardID);
	}

	public static IEnumerable<CEnhancement> GetEnhancementsForCard(CPlayerActor actor, int abilityCardID)
	{
		return GetEnhancementsForCard(GetCharacter(actor), abilityCardID);
	}

	public static List<CharacterPerk> GetActivePassivePerks(CPlayerActor actor)
	{
		if (AdventureState.MapState?.MapParty == null)
		{
			return new List<CharacterPerk>();
		}
		return GetActivePassivePerks(GetCharacter(actor));
	}

	public static List<CharacterPerk> GetActivePassivePerks(CMapCharacter character)
	{
		if (character != null)
		{
			return character.GetActivePassivePerks();
		}
		return new List<CharacterPerk>();
	}

	public static GoalDTO GetBattleGoal(CPlayerActor actor)
	{
		CBattleGoalState cBattleGoalState = AdventureState.MapState?.CurrentMapScenarioState?.GetChosenBattleGoal(actor.Class.ID);
		if (cBattleGoalState != null)
		{
			return new GoalDTO(cBattleGoalState, AdventureState.MapState.CurrentMapScenarioState.CurrentState.Players.Count);
		}
		return null;
	}

	public static GoalDTO GetPersonalQuest(CPlayerActor actor)
	{
		return GetPersonalQuest(GetCharacter(actor));
	}

	public static GoalDTO GetPersonalQuest(CMapCharacter character)
	{
		if (character?.PersonalQuest == null)
		{
			return null;
		}
		return new GoalDTO(character.PersonalQuest);
	}

	public static IEnumerable<GoalDTO> GetMasteries(CPlayerActor actor)
	{
		return AdventureState.MapState?.CurrentMapScenarioState?.MasteryBattleGoalStates.Where((Tuple<string, CBattleGoalState> c) => c.Item1 == actor.CharacterClass.ID)?.Select((Tuple<string, CBattleGoalState> c) => new GoalDTO(c.Item2, AdventureState.MapState.CurrentMapScenarioState.CurrentState.Players.Count)) ?? Enumerable.Empty<GoalDTO>();
	}
}
