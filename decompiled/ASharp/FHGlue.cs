using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.BattleGoals;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public static class FHGlue
{
	public static byte[] BinarySerializeState(MultiplayerGameState state, DefRepository repo)
	{
		return new StateSerializer<MultiplayerGameState>(repo)
		{
			StateResolveAction = (MultiplayerGameState s) =>
			{
				s.CampaignState.ResolveDefs(repo);
			}
		}.BinarySerialize(state);
	}

	public static MultiplayerGameState BinaryDeserialzieState(byte[] data, DefRepository repo)
	{
		return new StateSerializer<MultiplayerGameState>(repo)
		{
			StateResolveAction = (MultiplayerGameState s) =>
			{
				s.CampaignState.ResolveDefs(repo);
			}
		}.BinaryDeserialzie(data);
	}

	public static void ApplyDifficulty(EAdventureDifficulty difficulty, CampaignState state)
	{
		state.Difficulty = difficulty.ToString();
	}

	public static MultiplayerGameState GameStateFromSave()
	{
		CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
		List<CharacterID> slotAssignmentId = SaveData.Instance.Global.CurrentAdventureData.FrosthavenSaveData?.SlotAssignments;
		if (slotAssignmentId == null)
		{
			return new MultiplayerGameState(frosthavenCampaign, null);
		}
		List<CharacterState> list = new List<CharacterState>();
		int i = 0;
		while (i < slotAssignmentId.Count)
		{
			if (slotAssignmentId[i] == null || !slotAssignmentId[i].IsValid)
			{
				list.Add(null);
			}
			else
			{
				list.Add(frosthavenCampaign.AllCharacters.FirstOrDefault((CharacterState p) => p.ID == slotAssignmentId[i]));
			}
			int num = i + 1;
			i = num;
		}
		return new MultiplayerGameState(frosthavenCampaign, list);
	}

	public static void SetCurrentScenario(CampaignController controller)
	{
		CampaignState state = controller.State;
		FHRL.ScenarioState selectedScenario = state.SelectedScenario;
		if (selectedScenario == null)
		{
			return;
		}
		CMapState mapState = AdventureState.MapState;
		int seed = controller.State.Random.Next();
		mapState.AlreadyRewardedChestTreasureTableIDs.Clear();
		mapState.AlreadyRewardedChestTreasureTableIDs.AddRange(controller.State.LootedChestTreasureTableIDs);
		mapState.CurrentMapScenarioState = new CMapScenarioState(selectedScenario.GetScenarionLevelID(controller), controller.ScenarioController.GetScenarioLevel(), seed);
		mapState.CurrentMapScenarioState.Init(selectedScenario.Def.Name);
		mapState.CurrentMapScenarioState.RegenerateMapScenario();
		mapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
		foreach (ActiveScenarioState.CharacterSettings css in state.ActiveScenario.CharacterScenarioSettings)
		{
			controller.CharacterController.TryGetCharacter(css.CharacterID, out var character);
			CMapCharacter mapCharacter = mapState.MapParty.SelectedCharacters.First((CMapCharacter c) => c.CharacterID == character.ClassID.ID);
			if (css.SelectedBattleGoal.HasValue)
			{
				CBattleGoalState battleGoal = new CBattleGoalState(MapRuleLibraryClient.MRLYML.BattleGoals.First((BattleGoalYMLData b) => b.ID == css.SelectedBattleGoal.Value.ID), mapCharacter);
				mapState.CurrentMapScenarioState.ChooseBattleGoal(character.ClassID.ID, battleGoal);
			}
			if (controller.HasUnfinishedPrologue())
			{
				continue;
			}
			CharacterState characterState = state.PartyCharacters.SingleOrDefault((CharacterState x) => x.ID == css.CharacterID);
			if (characterState == null)
			{
				continue;
			}
			List<CBattleGoalState> list = new List<CBattleGoalState>();
			foreach (MasteryDef mastery in Singleton<FHMapChoreographer>.InstanceFast.GameController.CharacterController.GetUncompletedMasteries(characterState))
			{
				CBattleGoalState item = new CBattleGoalState(MapRuleLibraryClient.MRLYML.Masteries.First((BattleGoalYMLData b) => b.ID == mastery.ID.ID), mapCharacter, IsMastery: true);
				list.Add(item);
			}
			mapState.CurrentMapScenarioState.SetMasteryGoals(characterState.ClassID.ID, list);
		}
		List<FHRL.ScenarioState> list2 = SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenCampaign?.UnlockedScenarios;
		if (list2 == null)
		{
			return;
		}
		List<FHScenarioSetup.CPEntry> list3 = new List<FHScenarioSetup.CPEntry>();
		foreach (FHRL.ScenarioState item2 in list2)
		{
			if (item2 != null)
			{
				list3.Add(new FHScenarioSetup.CPEntry
				{
					Id = item2.ScenarioID.ID,
					ConclusionParameters = ((item2.ConclusionParameters == null) ? new List<object>() : new List<object>(item2.ConclusionParameters)),
					ConclusionIndex = item2.ConclusionIndex
				});
			}
		}
		FHScenarioSetup fHScenarioSetup = new FHScenarioSetup
		{
			ScenarioID = state.ActiveScenario.Def.ID.ID,
			RandomItemLooted = selectedScenario.RandomItemLooted,
			IsWinter = !state.Outpost.Calendar.IsSummer,
			IsInPrologue = (state.IsInPrologue || state.SelectedScenario.Def.CanNotReturnToOutpost),
			IsInTutorial = (Singleton<FHMapChoreographer>.Instance?.IsTutorial ?? false),
			WasCompleted = state.SelectedScenario.Completed,
			ExtraGoldOnLootCard = (uint)state.ActiveScenario.Modifiers.Where((ScenarioModifier x) => x.Type == ScenarioModifierType.ExtraGoldOnLoot).Sum((ScenarioModifier x) => Convert.ToUInt32(x.Param1)),
			MustHaveUnusedSmallItem = state.ActiveScenario.Modifiers.Any((ScenarioModifier x) => x.Type == ScenarioModifierType.ExtraSmallItem),
			UnlockedClassIDs = state.UnlockedClassIDs.ConvertAll((CharacterClassID c) => c.ID),
			ConclusionParametersJSON = FHScenarioSetup.ConclusionParamsToJSON(list3),
			WaningToInertDisabled = state.NextScenarioModifiers.WaningToInertDisabled.Select((ScenarioElement x) => Enum.Parse<ElementInfusionBoardManager.EElement>(x.ToString())).ToList(),
			AlreadyLootedChests = state.LootedChestTreasureTableIDs.ToList()
		};
		mapState.MapParty.NextScenarioEffects.ShowMonsterCardsAtScenarioStart = state.NextScenarioModifiers.ShowMonsterCardsAtScenarioStart;
		mapState.MapParty.NextScenarioEffects.Ormi1 = state.NextScenarioModifiers.Ormi1;
		mapState.MapParty.NextScenarioEffects.Ormi2 = state.NextScenarioModifiers.Ormi2;
		mapState.MapParty.NextScenarioEffects.Damage = (int)state.NextScenarioModifiers.PartyDamage;
		mapState.MapParty.NextScenarioEffects.Discard = (int)state.NextScenarioModifiers.PartyDiscard;
		mapState.MapParty.NextScenarioEffects.RecoverTwoDiscardedCardsEndOfFirstRound = state.NextScenarioModifiers.PartyRecoverTwoDiscardedCardsEndOfFirstRound;
		mapState.MapParty.NextScenarioEffects.RecoverAllDiscardedCardsEndOfFirstRound = state.NextScenarioModifiers.PartyRecoverAllDiscardedCardsEndOfFirstRound;
		mapState.MapParty.NextScenarioEffects.ExtraCardFirstRound = state.NextScenarioModifiers.PartyExtraCardFirstRound;
		mapState.MapParty.NextScenarioEffects.DiscardCardFireLightOrDamage = (int)state.NextScenarioModifiers.PartyDiscardCardFireLightOrDamage;
		mapState.MapParty.NextScenarioEffects.MonsterAttackModsToSet.Clear();
		foreach (CharacterAttackModifierDef monsterAttackMod in state.ActiveScenario.MonsterAttackMods)
		{
			mapState.MapParty.NextScenarioEffects.MonsterAttackModsToSet.Add(monsterAttackMod.ID.ID);
		}
		foreach (ScenarioElement item3 in state.NextScenarioModifiers.ElementsToInfuse)
		{
			if (Enum.TryParse(typeof(ElementInfusionBoardManager.EElement), item3.ToString(), out var result))
			{
				mapState.MapParty.NextScenarioEffects.Infusions.Add((ElementInfusionBoardManager.EElement)result);
			}
		}
		List<ItemID> list4 = controller.ValidRandomItemDeckIDs.ToList();
		state.ActiveScenario.NextRandomItemID = ((list4.Count > 0) ? new ItemID?(list4[0]) : ((ItemID?)null));
		state.ActiveScenario.NextRandomScenarioID = null;
		List<ScenarioDef> list5 = controller.ScenarioController.GetRemainingRandomSideScenarios().ToList();
		if (list5.Count > 0)
		{
			state.Random.Shuffle(list5);
			state.ActiveScenario.NextRandomScenarioID = list5[0].ID;
		}
		List<ItemID> list6 = controller.ValidRandomItemBlueprintDeckIDs.ToList();
		state.ActiveScenario.NextRandomItemBlueprintID = ((list6.Count > 0) ? new ItemID?(list6[0]) : ((ItemID?)null));
		fHScenarioSetup.NextRandomItemID = state.ActiveScenario.NextRandomItem?.ScenarioItemID ?? 0;
		fHScenarioSetup.NextRandomScenarioID = state.ActiveScenario.NextRandomScenarioID?.ID ?? 0;
		fHScenarioSetup.NextRandomItemBlueprintID = (uint)(state.ActiveScenario.NextRandomItemBlueprint?.ScenarioItemID ?? 0);
		ScenarioLoot scenarioLootWithFavorsAfterBeingApplied = controller.ScenarioController.GetScenarioLootWithFavorsAfterBeingApplied();
		mapState.CurrentMapScenarioState.InitialState.LootTable[1] = (byte)scenarioLootWithFavorsAfterBeingApplied.Gold;
		mapState.CurrentMapScenarioState.InitialState.LootTable[2] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Lumber;
		mapState.CurrentMapScenarioState.InitialState.LootTable[4] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Hide;
		mapState.CurrentMapScenarioState.InitialState.LootTable[3] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Metal;
		mapState.CurrentMapScenarioState.InitialState.LootTable[5] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Arrowvine;
		mapState.CurrentMapScenarioState.InitialState.LootTable[6] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Axenut;
		mapState.CurrentMapScenarioState.InitialState.LootTable[7] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Corpsecap;
		mapState.CurrentMapScenarioState.InitialState.LootTable[8] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Flamefruit;
		mapState.CurrentMapScenarioState.InitialState.LootTable[9] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Rockroot;
		mapState.CurrentMapScenarioState.InitialState.LootTable[10] = (byte)scenarioLootWithFavorsAfterBeingApplied.Resources.Snowthistle;
		foreach (IGrouping<object, ScenarioModifier> item4 in from x in state.ActiveScenario.Modifiers
			where x.Type == ScenarioModifierType.RemoveCardFromMonster
			group x by x.Param1)
		{
			MonsterDef monster = controller.DefRepository.Monsters[new MonsterID((string)item4.Key)];
			CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monster.ID.ID);
			IEnumerable<MonsterCardID> cardsToRemove = item4.Select((ScenarioModifier x) => new MonsterCardID(uint.Parse((string)x.Param2)));
			RemoveCardsFromMonsters(cMonsterClass, cardsToRemove);
			if (cMonsterClass.NonEliteVariant != null)
			{
				RemoveCardsFromMonsters(cMonsterClass.NonEliteVariant, cardsToRemove);
			}
			if (cMonsterClass.EliteVariant != null)
			{
				RemoveCardsFromMonsters(cMonsterClass.EliteVariant, cardsToRemove);
			}
		}
		mapState.CurrentMapScenarioState.InitialState.Pets.AddRange(state.ActiveScenario.PetIDs.Select((PetID x) => new CItem((int)x.ID, CBaseCard.ECardType.Pet, x.ID)));
		foreach (ChallengeID selectedChallengeID in state.ActiveScenario.SelectedChallengeIDs)
		{
			mapState.CurrentMapScenarioState.InitialState.AddChallenge(selectedChallengeID.ID);
		}
		int scenarioLevel = controller.ScenarioController.GetScenarioLevel();
		GameMode.ScenarioLevelSetting scenarioLevelSetting = controller.ScenarioController.GetScenarioLevelSetting(scenarioLevel);
		GameMode.ScenarioLevelSetting scenarioLevelSetting2 = controller.ScenarioController.GetScenarioLevelSetting(scenarioLevel + 1);
		fHScenarioSetup.GoldConversion = scenarioLevelSetting.GoldConversion;
		fHScenarioSetup.GoldConversionNextLevel = scenarioLevelSetting2.GoldConversion;
		fHScenarioSetup.TrapDamage = scenarioLevelSetting.TrapDamage;
		fHScenarioSetup.HazardousTerrainDamage = scenarioLevelSetting.HazardousTerrainDamage;
		fHScenarioSetup.HasBeenRestarted = state.ActiveScenario.HasBeenRestarted;
		fHScenarioSetup.CanNotBeLost = state.ActiveScenario.Def.CanNotBeLost;
		FHRL.ScenarioState scenarioState = state.UnlockedScenarios.SingleOrDefault((FHRL.ScenarioState x) => x.Def == state.ActiveScenario.Def);
		if (scenarioState != null)
		{
			fHScenarioSetup.IsAlternativeLevel = scenarioState.IsUsingAlternativeLevel(controller);
		}
		mapState.EnterScenario(fHScenarioSetup);
	}

	private static void RemoveCardsFromMonsters(CMonsterClass monsterClass, IEnumerable<MonsterCardID> cardsToRemove)
	{
		List<CMonsterAbilityCard> list = MonsterClassManager.LoadMonsterCards(monsterClass.MonsterYML);
		foreach (MonsterCardID cardID in cardsToRemove)
		{
			CMonsterAbilityCard item = list.SingleOrDefault((CMonsterAbilityCard x) => x.ID == cardID.ID);
			list.Remove(item);
		}
		monsterClass.AbilityCardsPool.Clear();
		monsterClass.AbilityCardsPool.AddRange(list);
	}

	public static void UpdateSave()
	{
		foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
		{
			AdventureState.MapState.MapParty.RemoveCharacterFromCharactersList(checkCharacter);
			AdventureState.MapState.MapParty.RemoveSelectedCharacter(checkCharacter);
		}
		foreach (NetworkControllable allControllable in ControllableRegistry.AllControllables)
		{
			if (allControllable.Slot.Index >= 0)
			{
				CMapCharacter cMapCharacter = (allControllable.ControllableObject as FHControllableCharacter)?.GHCharacter;
				if (cMapCharacter != null)
				{
					AdventureState.MapState.MapParty.AddCharacterToCharactersList(cMapCharacter);
					AdventureState.MapState.MapParty.AddSelectedCharacterToSlot(cMapCharacter, allControllable.Slot.Index);
				}
			}
		}
	}

	public static CEnhancement CreateCEnhancement(CardEnhacementSlotQuery query, ClassCardState state)
	{
		EEnhancement enhancement = ((state != null) ? GetEEnhnancement(state.GetEnhancedSlot(query)?.Enhancement) : EEnhancement.NoEnhancement);
		EEnhancementLine enhancementLine = (EEnhancementLine)query.SlotDef.EnhancementLine;
		AbilityCardYMLData abilityCardYMLData = ScenarioRuleClient.SRLYML.AbilityCards.First((AbilityCardYMLData c) => c.ID == query.CardDef.ID.ID);
		return new CEnhancement(enhancement, enhancementLine, abilityCardYMLData.ID, query.SlotDef.AbilityName, (int)query.SlotDef.SlotIndex, (EEnhancementMarkType)query.SlotDef.Mark);
	}

	public static EEnhancement GetEEnhnancement(CardEnhancementTypeDef def)
	{
		if (def == null)
		{
			return EEnhancement.NoEnhancement;
		}
		return def.Effect switch
		{
			CardEnhancementEffectType.AddOne => def.ForEffect switch
			{
				CardEffectType.Damage => EEnhancement.PlusDamage, 
				CardEffectType.Attack => EEnhancement.PlusAttack, 
				CardEffectType.Heal => EEnhancement.PlusHeal, 
				CardEffectType.Move => EEnhancement.PlusMove, 
				CardEffectType.Pierce => EEnhancement.PlusPierce, 
				CardEffectType.Pull => EEnhancement.PlusPull, 
				CardEffectType.Push => EEnhancement.PlusPush, 
				CardEffectType.Range => EEnhancement.PlusRange, 
				CardEffectType.Retailate => EEnhancement.PlusRetaliate, 
				CardEffectType.Shield => EEnhancement.PlusShield, 
				CardEffectType.Target => EEnhancement.PlusTarget, 
				CardEffectType.SummonAttack => EEnhancement.SummonAttack, 
				CardEffectType.SummonHP => EEnhancement.SummonHP, 
				CardEffectType.SummonMove => EEnhancement.SummonMove, 
				CardEffectType.SummonRange => EEnhancement.SummonRange, 
				CardEffectType.Teleport => EEnhancement.PlusTeleport, 
				_ => EEnhancement.NoEnhancement, 
			}, 
			CardEnhancementEffectType.Ward => EEnhancement.Ward, 
			CardEnhancementEffectType.Regenerate => EEnhancement.Regenerate, 
			CardEnhancementEffectType.Strengthen => EEnhancement.Strengthen, 
			CardEnhancementEffectType.Bless => EEnhancement.Bless, 
			CardEnhancementEffectType.Wound => EEnhancement.Wound, 
			CardEnhancementEffectType.Poison => EEnhancement.Poison, 
			CardEnhancementEffectType.Immobilize => EEnhancement.Immobilize, 
			CardEnhancementEffectType.Muddle => EEnhancement.Muddle, 
			CardEnhancementEffectType.Curse => EEnhancement.Curse, 
			CardEnhancementEffectType.ElementAir => EEnhancement.Air, 
			CardEnhancementEffectType.ElementDark => EEnhancement.Dark, 
			CardEnhancementEffectType.ElementEarth => EEnhancement.Earth, 
			CardEnhancementEffectType.ElementFire => EEnhancement.Fire, 
			CardEnhancementEffectType.ElementIce => EEnhancement.Ice, 
			CardEnhancementEffectType.ElementLight => EEnhancement.Light, 
			CardEnhancementEffectType.ElementWild => EEnhancement.AnyElement, 
			CardEnhancementEffectType.Jump => EEnhancement.Jump, 
			CardEnhancementEffectType.AoEHex => EEnhancement.Area, 
			_ => EEnhancement.NoEnhancement, 
		};
	}

	public static EEnhancementLine GetEEnhancementLine(CardEnhacementSlotQuery query)
	{
		return EEnhancementLine.Mainline;
	}

	public static CardEffectType GetEnhancementCardEffectType(CEnhancement e)
	{
		if (e.EnhancementLine == EEnhancementLine.Mainline)
		{
			switch (e.Ability.AbilityType)
			{
			case CAbility.EAbilityType.Move:
				return CardEffectType.Move;
			case CAbility.EAbilityType.Damage:
				return CardEffectType.Damage;
			case CAbility.EAbilityType.Attack:
				return CardEffectType.Attack;
			case CAbility.EAbilityType.Heal:
				return CardEffectType.Heal;
			case CAbility.EAbilityType.Shield:
				return CardEffectType.Shield;
			case CAbility.EAbilityType.Retaliate:
				return CardEffectType.Retailate;
			case CAbility.EAbilityType.Pull:
				return CardEffectType.Pull;
			case CAbility.EAbilityType.Push:
				return CardEffectType.Push;
			case CAbility.EAbilityType.Teleport:
			case CAbility.EAbilityType.Teleport2:
				return CardEffectType.Teleport;
			case CAbility.EAbilityType.Trap:
			{
				CAbilityTrap cAbilityTrap = e.Ability as CAbilityTrap;
				if (cAbilityTrap.TrapData.Damage > 0)
				{
					return CardEffectType.Damage;
				}
				if (cAbilityTrap.TrapData.Heal > 0)
				{
					return CardEffectType.Heal;
				}
				break;
			}
			}
		}
		switch (e.EnhancementLine)
		{
		case EEnhancementLine.Pull:
			return CardEffectType.Pull;
		case EEnhancementLine.Push:
			return CardEffectType.Push;
		case EEnhancementLine.Range:
		case EEnhancementLine.RetaliateRange:
			return CardEffectType.Range;
		case EEnhancementLine.SummonAttack:
			return CardEffectType.SummonAttack;
		case EEnhancementLine.SummonHealth:
			return CardEffectType.SummonHP;
		case EEnhancementLine.SummonMove:
			return CardEffectType.SummonMove;
		case EEnhancementLine.SummonRange:
			return CardEffectType.SummonRange;
		case EEnhancementLine.Targets:
			return CardEffectType.Target;
		case EEnhancementLine.Pierce:
			return CardEffectType.Pierce;
		default:
			return CardEffectType.Other;
		}
	}

	public static CMapCharacter CreateGHCharacter(CharacterState character, CampaignState state, CampaignController controller)
	{
		CMapCharacter cMapCharacter = new CMapCharacter(character.ClassID.ID, character.Name, 0);
		cMapCharacter.XPTable.Clear();
		cMapCharacter.XPTable.AddRange(state.GameMode.CharacterExperienceTable.Select((uint i) => (int)i));
		foreach (PerkDef perk in character.Perks)
		{
			cMapCharacter.Perks.Add(new CharacterPerk(perk.ID.ID, character.ClassID.ID)
			{
				IsActive = true
			});
		}
		cMapCharacter.ModifyGold((int)character.Gold, useGoldModifier: false);
		cMapCharacter.GainEXP((int)character.Experience, 1f);
		for (int num = 1; num < character.Level; num++)
		{
			cMapCharacter.LevelUp(adjustPerkPointsToo: false);
		}
		foreach (ItemDef equippedItem in controller.CharacterController.GetEquippedItems(character))
		{
			CItem addItem = new CItem(equippedItem.ScenarioItemID, $"{cMapCharacter.CharacterID}-{equippedItem.ScenarioItemID}", 0u);
			cMapCharacter.AddItemToEquippedItems(addItem);
		}
		CharacterController characterController = controller.CharacterController;
		cMapCharacter.ManuallySetSmallItemOverride = (int)(characterController.GetMaxUnlockedSmallItems(character) - characterController.GetBaseMaxUnlockedSmallItems(character));
		cMapCharacter.OwnedAbilityCardIDs.Clear();
		cMapCharacter.HandAbilityCardIDs.Clear();
		List<CEnhancement> list = new List<CEnhancement>();
		foreach (ClassCardDef cardDef in character.OwnedCards)
		{
			cMapCharacter.OwnedAbilityCardIDs.AddNotContained((int)cardDef.ID.ID);
			if (character.SelectedCardIDs.Contains(cardDef.ID))
			{
				cMapCharacter.HandAbilityCardIDs.AddNotContained((int)cardDef.ID.ID);
			}
			foreach (ClassCardDef.EnhacementSlot enhancementSlot in cardDef.EnhancementSlots)
			{
				ClassCardState state2 = state.ClassCardModifications.FirstOrDefault((ClassCardState m) => m.CardDef == cardDef);
				CEnhancement item = CreateCEnhancement(new CardEnhacementSlotQuery(cardDef, enhancementSlot), state2);
				list.Add(item);
			}
		}
		cMapCharacter.CompletedMasteries = character.CompletedMasteryIDs.Count();
		cMapCharacter.HandAbilityCardIDs.Sort();
		cMapCharacter.Enhancements.Clear();
		cMapCharacter.Enhancements.AddRange(list);
		foreach (CharacterScenarioModifier scenarioModifier in character.ScenarioModifiers)
		{
			switch (scenarioModifier.Type)
			{
			case CharacterScenarioModifierType.PoisonNextAttack:
				if (!cMapCharacter.NegativeConditionsNextAttack.Any((NegativeConditionPair x) => x.NegativeCondition == CCondition.ENegativeCondition.Poison))
				{
					cMapCharacter.NegativeConditionsNextAttack.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Poison, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Turns, 1));
				}
				break;
			case CharacterScenarioModifierType.Damage:
				cMapCharacter.DamageOnScenarioStart += scenarioModifier.Strenght;
				break;
			case CharacterScenarioModifierType.IncreaseMaxHealth:
				cMapCharacter.IncreasedHealth += scenarioModifier.Strenght;
				break;
			case CharacterScenarioModifierType.Bless:
				cMapCharacter.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Bless, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.None, (CActor)null));
				break;
			case CharacterScenarioModifierType.Ward:
				cMapCharacter.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Ward, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.None, (CActor)null));
				break;
			case CharacterScenarioModifierType.Curse:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Curse, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.None, 1));
				break;
			case CharacterScenarioModifierType.Disarm:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Disarm, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Muddle:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Muddle, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Wound:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Wound, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Brittle:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Brittle, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Stun:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Stun, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Poison:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Poison, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Impair:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Impair, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.Regenerate:
				cMapCharacter.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Regenerate, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.WhenDamaged, (CActor)null));
				break;
			case CharacterScenarioModifierType.Strengthen:
				cMapCharacter.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Strengthen, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, (CActor)null));
				break;
			case CharacterScenarioModifierType.LootCard:
				cMapCharacter.LootCardsAtScenarioStart++;
				break;
			case CharacterScenarioModifierType.LootCardAdvantage:
				cMapCharacter.LootCardAdvantage = true;
				break;
			case CharacterScenarioModifierType.DiscardSmallItem:
				cMapCharacter.DiscardSmallItemCount++;
				break;
			case CharacterScenarioModifierType.Invisible:
				cMapCharacter.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Invisible, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, (CActor)null));
				break;
			case CharacterScenarioModifierType.Immobilize:
				cMapCharacter.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Immobilize, RewardCondition.EConditionMapDuration.NextScenario, 1, EConditionDecTrigger.Turns, 1));
				break;
			case CharacterScenarioModifierType.DiscardCard:
				cMapCharacter.DiscardCardsCount += (int)scenarioModifier.Strenght;
				break;
			case CharacterScenarioModifierType.RecoverAllDiscardedCardsEndOfSecondRound:
				cMapCharacter.RecoverCardsEndOfSecondRound = int.MaxValue;
				break;
			case CharacterScenarioModifierType.LootExtraIfEveryoneLootedSameAmount:
				cMapCharacter.LootExtraIfEveryoneLootedSameAmount = true;
				break;
			case CharacterScenarioModifierType.LootExtraIfNobodyElseLooted:
				cMapCharacter.LootExtraIfNobodyElseLooted = true;
				break;
			}
		}
		return cMapCharacter;
	}

	public static ItemDef GetScenarioItemDef(CItem item, DefRepository repo)
	{
		if (repo.Items.Any((KeyValuePair<ItemID, ItemDef> itemEntry) => itemEntry.Value.ScenarioItemID == item.ID))
		{
			return repo.Items.SingleOrDefault((KeyValuePair<ItemID, ItemDef> itemEntry) => itemEntry.Value.ScenarioItemID == item.ID).Value;
		}
		return null;
	}
}
