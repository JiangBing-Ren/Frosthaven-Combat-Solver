using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using Libs.Threading;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Locations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.YML;

public class YMLDefRepository
{
	private readonly string[] VerticalChunkcExcludeClasses = new string[0];

	private static YMLDefRepository _repo;

	private YMLLoading _debugLoading;

	public static YMLDefRepository Repo => _repo ?? (_repo = new YMLDefRepository());

	public GenericYML<GameMode> GameModes { get; private set; } = new GenericYML<GameMode>();

	public GenericYML<ScenarioDef> FHScenarios { get; private set; } = new GenericYML<ScenarioDef>();

	public GenericYML<ItemDef> FHItems { get; private set; } = new GenericYML<ItemDef>();

	public GenericYML<CampaignSectionDef> CampaignSections { get; private set; } = new GenericYML<CampaignSectionDef>();

	public GenericYML<CardEnhancementTypeDef> FHEnhancements { get; private set; } = new GenericYML<CardEnhancementTypeDef>();

	public DefRepository DefRepository { get; private set; }

	private bool ShouldRemoveDebugYML
	{
		get
		{
			if (Application.platform == RuntimePlatform.PS5 || Application.platform == RuntimePlatform.PS5)
			{
				return !Application.isEditor;
			}
			return false;
		}
	}

	public static void Clear()
	{
		_repo = new YMLDefRepository();
	}

	private static void FillScenarioDef(ScenarioDef def)
	{
		FillScenarioDef(def, def.LevelID);
		if (!string.IsNullOrEmpty(def.AlternativeLevelID))
		{
			FillScenarioDef(def, def.AlternativeLevelID);
		}
	}

	private static void FillScenarioDef(ScenarioDef def, string levelID)
	{
		CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(levelID);
		if (customLevel == null)
		{
			return;
		}
		ScenarioDef.LevelData levelData = new ScenarioDef.LevelData
		{
			ID = levelID
		};
		def.Levels.Add(levelData);
		foreach (CObjectChest item in from t in customLevel.ScenarioState.ChestProps.OfType<CObjectChest>()
			where t.Type != CObjectChest.EType.None || (t.ChestTreasureTablesID != null && t.ChestTreasureTablesID.Any())
			select t)
		{
			ScenarioDef.Chest chest = new ScenarioDef.Chest();
			if (item.Type != CObjectChest.EType.None)
			{
				chest.TreasureTableIDs.Add(item.Type.ToString());
			}
			else
			{
				chest.TreasureTableIDs.AddRange(item.ChestTreasureTablesID);
			}
			levelData.Chests.Add(chest);
		}
		if (def.IncludesChests != null)
		{
			foreach (string includesChest in def.IncludesChests)
			{
				ScenarioDef.Chest chest2 = new ScenarioDef.Chest();
				chest2.TreasureTableIDs.Add(includesChest.ToString());
				levelData.Chests.Add(chest2);
			}
		}
		for (int num = 2; num <= 4; num++)
		{
			levelData.PartySettings.Add(new ScenarioDef.PartySetting
			{
				PartyMembers = (uint)num
			});
		}
		levelData.Loot = default;
		levelData.Loot.Resources = default;
		levelData.Loot.Gold = customLevel.ScenarioState.LootTable[1];
		levelData.Loot.Resources.AddMaterial(OutpostMaterialType.Lumber, customLevel.ScenarioState.LootTable[2]);
		levelData.Loot.Resources.AddMaterial(OutpostMaterialType.Metal, customLevel.ScenarioState.LootTable[3]);
		levelData.Loot.Resources.AddMaterial(OutpostMaterialType.Hide, customLevel.ScenarioState.LootTable[4]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Arrowvine, customLevel.ScenarioState.LootTable[5]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Axenut, customLevel.ScenarioState.LootTable[6]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Corpsecap, customLevel.ScenarioState.LootTable[7]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Flamefruit, customLevel.ScenarioState.LootTable[8]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Rockroot, customLevel.ScenarioState.LootTable[9]);
		levelData.Loot.Resources.AddHerb(OutpostHerbType.Snowthistle, customLevel.ScenarioState.LootTable[10]);
		levelData.Loot.RandomItemCount = customLevel.ScenarioState.LootTable[11];
		List<EnemyState> list = customLevel.ScenarioState.Monsters.Concat(customLevel.ScenarioState.Enemy2Monsters).Concat(customLevel.ScenarioState.AllyMonsters).ToList();
		string text = "FH";
		if (def.Name.Contains("RFS_"))
		{
			text = "RFH";
		}
		string arg = text;
		uint iD = def.ID.ID;
		if (ScenarioScript.TryCreate(string.Format("ScenarioScript_{0}{1}", arg, iD.ToString("D3")), out var result))
		{
			list.AddRange(result.GetMonstersThatMightSpawn(customLevel.ScenarioState));
		}
		List<EnemyState> list2 = new List<EnemyState>();
		foreach (EnemyState monster in list)
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Classes.FirstOrDefault((CMonsterClass s) => s.ID == monster.ClassID);
			if (cMonsterClass == null)
			{
				continue;
			}
			foreach (CAbilitySummon item2 in from x in cMonsterClass.AbilityCardsPool.SelectMany((CMonsterAbilityCard x) => x.Action.Abilities)
				where x is CAbilitySummon
				select x as CAbilitySummon)
			{
				if (item2.SummonIDs.Count != 4)
				{
					continue;
				}
				for (int num2 = 1; num2 <= 4; num2++)
				{
					string summonID = item2.SummonIDs[num2 - 1];
					CMonsterClass summonClass = MonsterClassManager.Classes.FirstOrDefault((CMonsterClass s) => s.ID == summonID);
					if (summonClass != null)
					{
						EnemyState enemyState = list.Concat(list2).FirstOrDefault((EnemyState x) => x.ClassID == summonID || x.ClassID == summonClass?.NonEliteVariant?.ID);
						if (enemyState == null)
						{
							enemyState = new EnemyState(summonID, 0, null, CActor.EType.Enemy);
							enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
							enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Hidden;
							list2.Add(enemyState);
						}
						ScenarioManager.EPerPartySizeConfig value = ((summonClass.NonEliteVariant != null) ? ScenarioManager.EPerPartySizeConfig.ToElite : ScenarioManager.EPerPartySizeConfig.Normal);
						if (enemyState.ConfigPerPartySize[num2] != ScenarioManager.EPerPartySizeConfig.ToElite)
						{
							enemyState.ConfigPerPartySize[num2] = value;
						}
					}
				}
			}
		}
		list.AddRange(list2);
		List<CMap> list3 = new List<CMap>();
		foreach (TileIndex tile in customLevel.StartingTileIndexes)
		{
			CMap cMap = customLevel.ScenarioState.Maps.FirstOrDefault((CMap map) => map.MapTiles.Any((CMapTile x) => x.ArrayIndex.ToPoint() == tile.ToPoint()));
			if (cMap != null && !list3.Contains(cMap))
			{
				list3.Add(cMap);
			}
		}
		foreach (EnemyState monster2 in list)
		{
			foreach (ScenarioDef.PartySetting partySetting in levelData.PartySettings)
			{
				if (!monster2.ConfigPerPartySize.TryGetValue((int)partySetting.PartyMembers, out var value2))
				{
					Log.Error($"In scenario '{def.LevelID}' monster '{monster2.ID}' does not have config for party size {partySetting.PartyMembers}!");
				}
				else
				{
					if (value2 == ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						continue;
					}
					bool flag = !monster2.IsElite && value2 == ScenarioManager.EPerPartySizeConfig.ToElite;
					CMonsterClass cMonsterClass2 = null;
					cMonsterClass2 = ((!flag) ? MonsterClassManager.Classes.FirstOrDefault((CMonsterClass s) => s.ID == monster2.ClassID) : MonsterClassManager.Classes.FirstOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == monster2.ClassID));
					if (cMonsterClass2 == null)
					{
						Log.Error($"In scenario '{def.LevelID}' monster '{monster2.ClassID}, elite:{flag}' does not exist in MonsterClassManager!");
					}
					else
					{
						if (cMonsterClass2.MonsterYML.TreatAsNonMonster)
						{
							continue;
						}
						CMap cMap2 = ((monster2.Location == null) ? null : customLevel?.ScenarioState?.Maps?.FirstOrDefault((CMap map) => map.MapTiles.Any((CMapTile x) => x.ArrayIndex.ToPoint() == monster2.Location.ToPoint())));
						partySetting.Monsters.Add(new ScenarioDef.Monster
						{
							ID = new MonsterID(cMonsterClass2.ID),
							IsElite = (monster2.IsElite || value2 == ScenarioManager.EPerPartySizeConfig.ToElite),
							IsBoss = monster2.IsBoss,
							IsMiniBoss = cMonsterClass2.MonsterYML.MiniBoss,
							IsAlly = (monster2.Type == CActor.EType.Ally || monster2.Type == CActor.EType.Neutral),
							IsSecondFaction = (monster2.Type == CActor.EType.Enemy2),
							Model = cMonsterClass2.DefaultModel,
							NonEliteID = ((cMonsterClass2.NonEliteVariant != null) ? new MonsterID(cMonsterClass2.NonEliteVariant.ID) : default(MonsterID)),
							IsInFirstRoom = (cMap2 != null && list3.Contains(cMap2))
						});
					}
				}
			}
		}
		foreach (CObjective allObjective in customLevel.ScenarioState.AllObjectives)
		{
			levelData.Objectives.Add(LocalizationObjectiveConveter.ToBriefingScenarioObjective(allObjective));
		}
	}

	public void GenerateDefRepository()
	{
		DateTime now = DateTime.Now;
		if (DefRepository != null)
		{
			throw new Exception("DefRepository already generated!");
		}
		DefRepository defRepository = Repo.ToRuleLibraryDefRepo();
		DefRepository = defRepository;
		ApplyVerticalChunkPatches();
		Log.YMLInfo("Finished generating def repository. Duration: " + (DateTime.Now - now).TotalSeconds);
	}

	private void ApplyVerticalChunkPatches()
	{
		DefRepository.CharacterClasses.RemoveWhere((KeyValuePair<CharacterClassID, CharacterClassDef> c) => VerticalChunkcExcludeClasses.Contains(c.Key.ID));
		foreach (ScenarioDef value in DefRepository.Scenarios.Values)
		{
			foreach (ScenarioDef.Conclusion conclusion in value.Conclusions)
			{
				conclusion.Effects.RemoveAll((CampaignEffect c) => c.GetStringParams().Any((string s) => VerticalChunkcExcludeClasses.Contains(s)));
			}
		}
	}

	public void DebugUnload()
	{
		ScenarioRuleClient.UnloadData();
		Clear();
		_debugLoading.Close();
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.None;
	}

	public void DebugGenerateDefRepository()
	{
		if (DefRepository != null)
		{
			throw new Exception("DefRepository already generated!");
		}
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		_debugLoading = new YMLLoading();
		_debugLoading.Init();
		_debugLoading.LoadAll();
		ScenarioRuleClient.LoadData();
		DefRepository defRepository = Repo.ToRuleLibraryDefRepo();
		DefRepository = defRepository;
		ApplyVerticalChunkPatches();
	}

	public IEnumerator GenerateDefRepositoryCrt()
	{
		if (DefRepository != null)
		{
			throw new Exception("DefRepository already generated!");
		}
		if (Application.isPlaying)
		{
			IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-YMLDefRepository => GenerateDefRepository()", () =>
			{
				GenerateDefRepository();
			});
			yield return threadHandle.WaitForCompletionCrt();
		}
		else
		{
			GenerateDefRepository();
		}
	}

	private DefRepository ToRuleLibraryDefRepo()
	{
		JobSystem.ExecuteComplexJobs(FHScenarios.Items.ToList(), FillScenarioDef);
		DefRepository repo = new DefRepository();
		foreach (ScenarioDef item4 in FHScenarios.Items)
		{
			if (!(item4.ID == new ScenarioID(666u)) || !ShouldRemoveDebugYML)
			{
				repo.Scenarios[item4.ID] = item4;
			}
		}
		foreach (ItemDef def in FHItems.Items)
		{
			ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData itemCardYMLData2) => itemCardYMLData2.ID == def.ScenarioItemID);
			if (itemCardYMLData != null)
			{
				def.DescriptionLocKey = (itemCardYMLData.Name.IsNullOrEmpty() ? string.Empty : (itemCardYMLData.Name.Substring(0, itemCardYMLData.Name.Length - 1) + "2"));
				def.Icon = itemCardYMLData.Art;
				def.NameLocKey = itemCardYMLData.Name;
				if (int.TryParse(itemCardYMLData.FlipItemID, out var result))
				{
					def.FlipScenarioItemID = result;
				}
				def.IsFlipItemSideFront = itemCardYMLData.FlipItemSide == CItem.EFlipItemSide.Front;
			}
			repo.Items[def.ID] = def;
		}
		foreach (ItemCardYMLData petCard in ScenarioRuleClient.SRLYML.PetCards)
		{
			PetDef petDef = new PetDef
			{
				ID = new PetID((uint)petCard.ID),
				Name = petCard.Name,
				ArtKey = petCard.Art,
				Description = petCard.Description,
				RoundDuration = (petCard.Data.Abilities != null && petCard.Data.Abilities.Any((CAbility x) =>
				{
					AbilityData.ActiveBonusData activeBonusData = x.ActiveBonusData;
					return activeBonusData != null && activeBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round;
				}))
			};
			repo.Pets[petDef.ID] = petDef;
		}
		foreach (GameMode item5 in Repo.GameModes.Items)
		{
			repo.GameModes[item5.Name] = item5;
		}
		OutpostEventDef.LoadFromAssembly(repo);
		BuildingTypeDef.LoadFromAssembly(repo);
		PersonalQuestDef.LoadFromAssembly(repo);
		TownGuardPerkDef.LoadFromAssembly(repo);
		TownGuardAttackModifierDef.LoadFromAssembly(repo);
		PuzzleDef.LoadFromAssembly(repo);
		CampaignStickers.LoadFromAssembly(repo);
		LoadChallengesFromAssembly(repo);
		foreach (CampaignSectionDef item6 in CampaignSections.Items)
		{
			repo.CampaignSections[item6.ID] = item6;
		}
		foreach (CardEnhancementTypeDef item7 in FHEnhancements.Items)
		{
			repo.CardEnhancementTypes[item7.ID] = item7;
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			CharacterAttackModifierDef characterAttackModifierDef = new CharacterAttackModifierDef
			{
				ID = new CharacterAttackModifierID(attackModifier.Name),
				MathModifier = attackModifier.MathModifier,
				Basic = attackModifier.Name.Contains("Basic"),
				Rolling = attackModifier.Rolling,
				Shuffle = attackModifier.Shuffle,
				Persistent = attackModifier.IsPersistent,
				Damage = attackModifier.Card.Damage,
				Heal = attackModifier.Card.Heal,
				HealAmount = attackModifier.Card.HealAmount,
				AddTarget = attackModifier.AddTarget,
				HealAlly = attackModifier.Card.HealAlly,
				Push3 = attackModifier.Card.Push3,
				Push2 = attackModifier.Card.Push2,
				Push = attackModifier.Card.Push,
				Brittle = attackModifier.Card.Brittle,
				Consume = attackModifier.Card.Consume,
				TimeToken = attackModifier.Card.TimeToken,
				Infuse = attackModifier.Card.Infuse,
				Kill = attackModifier.Card.Kill,
				Pierce = attackModifier.Card.Pierce,
				Pull = attackModifier.Card.Pull,
				Shield = attackModifier.Card.Shield,
				IsBless = attackModifier.IsBless,
				IsCurse = attackModifier.IsCurse,
				InfuseElements = attackModifier.Card.InfuseElements.Select(CrossReference.EElementToCardEnhancementType).ToList(),
				ConsumeElements = attackModifier.Card.ConsumeElements.Select(CrossReference.EElementToCardEnhancementType).ToList(),
				NegativeConditions = attackModifier.Card.NegativeConditions.Select((CCondition.ENegativeCondition condition) => condition.ToString()).ToList(),
				PositiveConditions = attackModifier.Card.PositiveConditions.Select((CCondition.EPositiveCondition condition) => condition.ToString()).ToList(),
				CreateIce = attackModifier.Card.CreateIce,
				Strengthen = attackModifier.Card.Strengthen,
				Move = attackModifier.Card.Move,
				ModeTransfer = attackModifier.Card.ModeTransfer,
				MainAbilityStrength = attackModifier.Card.MainAbilityStrength,
				MoveActiveBonusTracker = attackModifier.Abilities.Any((CAbility x) => x.AbilityType == CAbility.EAbilityType.MoveActiveBonusTracker),
				Choose = attackModifier.Abilities.Any((CAbility x) => x.AbilityType == CAbility.EAbilityType.Choose),
				CustomTargeting = attackModifier.Abilities.Any((CAbility x) => x.Targeting == CAbility.EAbilityTargeting.Custom)
			};
			repo.AttackModifiers[characterAttackModifierDef.ID] = characterAttackModifierDef;
		}
		foreach (PerksYMLData perk in ScenarioRuleClient.SRLYML.Perks)
		{
			bool tactical = perk.Flags != ScenarioRuleLibrary.YML.EPerkFlag.None || (perk.Abilities != null && perk.Abilities.Count > 0) || (perk.Overrides != null && perk.Overrides.Count > 0) || perk.CompareAbility != null;
			PerkDef perkDef = new PerkDef
			{
				ID = new PerkID(perk.ID),
				Name = perk.Name,
				Description = perk.Description,
				Flags = (FHRL.EPerkFlag)perk.Flags,
				Available = perk.Available,
				Cost = perk.Cost,
				ModifiersToAdd = (from data in perk.CardsToAdd
					where repo.AttackModifiers.Any((KeyValuePair<CharacterAttackModifierID, CharacterAttackModifierDef> mod) => mod.Key.ID == data.Name)
					select new CharacterAttackModifierID(data.Name)).ToList(),
				ModifiersToRemove = (from data in perk.CardsToRemove
					where repo.AttackModifiers.Any((KeyValuePair<CharacterAttackModifierID, CharacterAttackModifierDef> mod) => mod.Key.ID == data.Name)
					select new CharacterAttackModifierID(data.Name)).ToList(),
				Tactical = tactical
			};
			repo.Perks[perkDef.ID] = perkDef;
		}
		foreach (KeyValuePair<AttackModifierYMLData, int> item8 in ScenarioRuleClient.SRLYML.AttackModifierDecks.Where((AttackModifierDeckYMLData x) => x.IsMonsterDeck && !x.IsReducedRandomnessDeck).FirstOrDefault().AttackModifierDeck)
		{
			CharacterAttackModifierID characterAttackModifierID = new CharacterAttackModifierID(item8.Key.Name);
			_ = repo.AttackModifiers[characterAttackModifierID];
			repo.DefaultMonsterAttackModifiers.Add(new GameMode.AttackMod
			{
				AttackModID = characterAttackModifierID,
				Quantity = (uint)item8.Value
			});
		}
		foreach (AbilityCardYMLData abilityCard in ScenarioRuleClient.SRLYML.AbilityCards)
		{
			if (abilityCard.CharacterID == "All")
			{
				continue;
			}
			ClassCardDef classCardDef = new ClassCardDef
			{
				ID = new ClassCardID((uint)abilityCard.ID),
				NameLocKey = abilityCard.Name,
				Initiative = (uint)abilityCard.Initiative,
				ClassID = new CharacterClassID(abilityCard.CharacterID),
				Level = ((abilityCard.Level == "X") ? 1u : uint.Parse(abilityCard.Level)),
				IsSpare = (abilityCard.Level == "X"),
				IsGeminateMeleeCard = (abilityCard.RequiredForm == CPlayerActor.EForm.Melee),
				IsModeCard = (abilityCard.TopActionCardData.ModeAbilities.Any() || abilityCard.BottomActionCardData.ModeAbilities.Any())
			};
			List<CAction> list = new List<CAction>();
			if (abilityCard.TopActionCardData?.Abilities != null)
			{
				list.Add(abilityCard.TopActionCardData);
			}
			if (abilityCard.BottomActionCardData?.Abilities != null)
			{
				list.Add(abilityCard.BottomActionCardData);
			}
			foreach (CAction item9 in list)
			{
				IEnumerable<CAbility> actionAbilities = CAbilityCard.GetActionAbilities(item9);
				bool flag = abilityCard.BottomActionCardData == item9;
				List<CAbility> source = item9.Abilities.Where((CAbility a) => a.AreaEffect != null).ToList();
				int effectHexes = 0;
				if (source.Any())
				{
					effectHexes = source.Max((CAbility a) => a.AreaEffect.EnabledHexes.Count((CAreaEffect.CAreaEffectHex x) => x.Flags.HasFlag(CAreaEffect.EHexFlag.Enemy)));
				}
				ClassCardDef.Ability ability = new ClassCardDef.Ability
				{
					EffectHexes = effectHexes,
					IsMove = item9.Abilities.Any((CAbility a) => a.AbilityType == CAbility.EAbilityType.Move),
					IsLostAfterUse = ((flag ? abilityCard.BottomDiscardType : abilityCard.TopDiscardType) == DiscardType.Lost),
					TotalMoveStrength = item9.Abilities.Where((CAbility x) => x.AbilityType == CAbility.EAbilityType.Move).Sum((CAbility a) => a.Strength),
					IsTopAction = !flag,
					HasPersistentBonus = item9.Abilities.Any((CAbility a) => a.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent || a.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon),
					ActionInfusion = (item9.Infusions?.Select((ElementInfusionBoardManager.EElement x) => Enum.Parse<ScenarioElement>(x.ToString()))?.ToList() ?? new List<ScenarioElement>())
				};
				foreach (CAbility ab in actionAbilities)
				{
					for (int num = 0; num < ab.AvailableEnhancementSlots.Count; num++)
					{
						CEnhancement cEnhancement = ab.AvailableEnhancementSlots[num];
						bool isMultiTarget = ab.NumberTargets > 1 || ab.NumberTargets == -1 || ab.Targeting == CAbility.EAbilityTargeting.All || ab.Targeting == CAbility.EAbilityTargeting.Room || ab.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms || ab.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets) || (ab.AreaEffect != null && ab.AreaEffect.EnabledHexes.Count((CAreaEffect.CAreaEffectHex x) => x.Flags.HasFlag(CAreaEffect.EHexFlag.Enemy)) > 1);
						if (actionAbilities.Any((CAbility x) => x is CAbilityControlActor cAbilityControlActor && cAbilityControlActor.ControlActorData.ControlAbilities.Any((CAbility y) => y.ID == ab.ID) && cAbilityControlActor.NumberTargets != 1))
						{
							isMultiTarget = true;
						}
						List<CardEnhancementEffectType> list2 = new List<CardEnhancementEffectType>();
						foreach (CCondition.EPositiveCondition allPositiveCondition in ab.GetAllPositiveConditions())
						{
							if (Enum.TryParse<CardEnhancementEffectType>(allPositiveCondition.ToString(), out var result2))
							{
								list2.Add(result2);
							}
						}
						foreach (CCondition.ENegativeCondition allNegativeCondition in ab.GetAllNegativeConditions())
						{
							if (Enum.TryParse<CardEnhancementEffectType>(allNegativeCondition.ToString(), out var result3))
							{
								list2.Add(result3);
							}
						}
						ClassCardDef.EnhacementSlot item = new ClassCardDef.EnhacementSlot
						{
							Effect = FHGlue.GetEnhancementCardEffectType(ab.AvailableEnhancementSlots[num]),
							Mark = (CardEnhancementMarkType)ab.AvailableEnhancementSlots[num].MarkType,
							AbilityName = ab.Name,
							AbilityType = ab.AbilityType.ToString(),
							SlotIndex = (uint)cEnhancement.EnhancementSlot,
							EnhancementLine = (uint)cEnhancement.EnhancementLine,
							IsMultiTarget = isMultiTarget,
							BuiltInConditions = list2,
							IsJump = (ab is CAbilityMove cAbilityMove && cAbilityMove.Jump),
							IsPersistent = (ab.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent || ab.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
						};
						ability.EnhancementSlots.Add(item);
					}
				}
				classCardDef.Actions.Add(ability);
			}
			repo.ClassCards[classCardDef.ID] = classCardDef;
		}
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			CharacterClassDef def2 = new CharacterClassDef
			{
				ID = new CharacterClassID(@class.ID),
				Name = @class.CharacterID,
				ClassName = @class.CharacterYML.LocKey,
				MaxCards = (uint)@class.CharacterYML.NumberAbilityCardsInBattle,
				Description = @class.CharacterYML.Description,
				OnlyValidInLevels = ((@class.CharacterYML.OnlyValidInLevels == null) ? null : @class.CharacterYML.OnlyValidInLevels.ToList())
			};
			List<PerksYMLData> perkIds = ScenarioRuleClient.SRLYML.Perks.Where((PerksYMLData p) => p.CharacterID == def2.ID.ID).ToList();
			List<PerkDef> source2 = repo.Perks.Values.Where((PerkDef p) => perkIds.Any((PerksYMLData py) => py.ID == p.ID.ID)).ToList();
			def2.Perks.AddRange(source2.Select((PerkDef p) => p.ID));
			repo.CharacterClasses[def2.ID] = def2;
			if (!string.IsNullOrEmpty(@class.CharacterYML.Race))
			{
				if (Enum.TryParse<CharacterRace>(@class.CharacterYML.Race, ignoreCase: true, out var result4))
				{
					def2.Race = result4;
				}
				else
				{
					Log.Error("Invalid character race '" + @class.CharacterYML.Race + "' for '" + @class.ID + "'");
				}
			}
			def2.MaxCards = (uint)@class.CharacterYML.NumberAbilityCardsInBattle;
			def2.HealthTable = @class.CharacterYML.HealthTable.Select((int i) => (uint)i).ToArray();
			def2.Complexity = (uint)@class.CharacterYML.Complexity;
			if (@class.CharacterYML.HeroStats != null)
			{
				def2.Melee = (uint)@class.CharacterYML.HeroStats.Melee;
				def2.Ranged = (uint)@class.CharacterYML.HeroStats.Ranged;
				def2.Mobility = (uint)@class.CharacterYML.HeroStats.Mobility;
				def2.Support = (uint)@class.CharacterYML.HeroStats.Support;
				def2.Defense = (uint)@class.CharacterYML.HeroStats.Defense;
				def2.Control = (uint)@class.CharacterYML.HeroStats.Control;
			}
			def2.Strengths = @class.CharacterYML.Strengths;
			def2.Weaknesses = @class.CharacterYML.Weaknesses;
			def2.MasteryIDs = ((@class.CharacterYML.MasteryIDs == null) ? null : @class.CharacterYML.MasteryIDs.Select((string x) => new MasteryID(x)).ToArray());
			def2.RetirementCampaignSection = ((@class.CharacterYML == null) ? null : @class.CharacterYML.RetireCampaignSection);
			string[] elementalAffinities;
			if (@class.CharacterYML.ElementalAffinities != null)
			{
				elementalAffinities = @class.CharacterYML.ElementalAffinities;
				foreach (string text in elementalAffinities)
				{
					if (!string.IsNullOrEmpty(text) && Enum.TryParse<ElementInfusionBoardManager.EElement>(text, ignoreCase: true, out var result5))
					{
						CardEnhancementEffectType item2 = CrossReference.EElementToCardEnhancementType(result5);
						def2.Affinities.Add(item2);
						continue;
					}
					Log.Error("Invalid character elemental affinity '" + text + "' for '" + @class.ID + "'");
				}
			}
			if (@class.CharacterYML.Traits == null)
			{
				continue;
			}
			elementalAffinities = @class.CharacterYML.Traits;
			foreach (string text2 in elementalAffinities)
			{
				if (!string.IsNullOrEmpty(text2) && Enum.TryParse<CharacterTraitType>(text2, ignoreCase: true, out var result6))
				{
					def2.Traits.Add(result6);
					continue;
				}
				Log.Error("Invalid character trait '" + text2 + "' for '" + @class.ID + "'");
			}
		}
		foreach (BattleGoalYMLData battleGoal in MapRuleLibraryClient.MRLYML.BattleGoals)
		{
			BattleGoalDef battleGoalDef = new BattleGoalDef
			{
				ID = new BattleGoalID(battleGoal.ID),
				Name = battleGoal.LocalisedName,
				Description = battleGoal.LocalisedDescription,
				PerkChecksReward = (uint)battleGoal.PerkChecks.GetValueOrDefault(),
				PerkChecksRewardFullyParty = (uint)battleGoal.PerkChecksFullParty.GetValueOrDefault()
			};
			repo.BattleGoals[battleGoalDef.ID] = battleGoalDef;
		}
		foreach (BattleGoalYMLData mastery in MapRuleLibraryClient.MRLYML.Masteries)
		{
			MasteryDef masteryDef = new MasteryDef
			{
				ID = new MasteryID(mastery.ID),
				Name = mastery.LocalisedName,
				Description = mastery.LocalisedDescription,
				PerkMarksReward = (uint)(mastery.PerkPoints ?? 1)
			};
			repo.MasteryDefs[masteryDef.ID] = masteryDef;
		}
		foreach (CMonsterClass class2 in MonsterClassManager.Classes)
		{
			MonsterDef monsterDef = new MonsterDef
			{
				ID = new MonsterID(class2.ID),
				Name = class2.LocKey
			};
			repo.Monsters[monsterDef.ID] = monsterDef;
			foreach (CMonsterAbilityCard item10 in class2.AbilityCardsPool)
			{
				MonsterCardID monsterCardID = new MonsterCardID((uint)item10.ID);
				if (!repo.MonsterCards.TryGetValue(monsterCardID, out var value))
				{
					value = new MonsterCardDef
					{
						ID = monsterCardID,
						Name = item10.Name,
						MonsterIDs = new List<MonsterID>(),
						AbilityTypes = item10.Action.Abilities.Select((CAbility x) => x.AbilityType.ToString()).ToList()
					};
					repo.MonsterCards[value.ID] = value;
				}
				value.MonsterIDs.Add(monsterDef.ID);
			}
		}
		foreach (object value2 in Enum.GetValues(typeof(EAdventureDifficulty)))
		{
			EAdventureDifficulty item3 = (EAdventureDifficulty)value2;
			foreach (DifficultyYMLData item11 in MapRuleLibraryClient.MRLYML.Difficulty)
			{
				DifficultyID difficultyID = new DifficultyID($"{value2}_{item11.GameMode}");
				DifficultyDef difficultyDef = new DifficultyDef
				{
					ID = difficultyID
				};
				foreach (CAdventureDifficulty difficultySetting in item11.DifficultySettings)
				{
					if (difficultySetting.ActiveOn.Contains(item3))
					{
						if (difficultySetting.HasHeroHealthModifier)
						{
							difficultyDef.Effects.Add(new DifficultyEffect
							{
								Type = DifficultyEffectType.HeroHealth,
								Strength = difficultySetting.HeroHealthModifier,
								PositiveEffect = difficultySetting.PositiveEffect
							});
						}
						else if (difficultySetting.HasSummonHealthModifier)
						{
							difficultyDef.Effects.Add(new DifficultyEffect
							{
								Type = DifficultyEffectType.SummonHealth,
								Strength = difficultySetting.SummonHealthModifier,
								PositiveEffect = difficultySetting.PositiveEffect
							});
						}
						else if (difficultySetting.HasBlessCards)
						{
							difficultyDef.Effects.Add(new DifficultyEffect
							{
								Type = DifficultyEffectType.BlessCards,
								Strength = difficultySetting.BlessCards,
								PositiveEffect = difficultySetting.PositiveEffect
							});
						}
						else if (difficultySetting.HasScenarioLevelModifier)
						{
							difficultyDef.Effects.Add(new DifficultyEffect
							{
								Type = DifficultyEffectType.ScenarioLevelModifier,
								Strength = difficultySetting.ScenarioLevelModifier,
								PositiveEffect = difficultySetting.PositiveEffect
							});
						}
					}
				}
				repo.Difficulties[difficultyID] = difficultyDef;
			}
		}
		return repo;
	}

	public static void LoadChallengesFromAssembly(DefRepository repo)
	{
		foreach (Type item in typeof(CChallenge).Assembly.ExportedTypes.Where((Type t) => typeof(CChallenge).IsAssignableFrom(t) && !t.IsAbstract).ToList())
		{
			CChallenge cChallenge = (CChallenge)Activator.CreateInstance(item);
			CChallenge.Challenges[cChallenge.ID] = cChallenge;
			ChallengeDef challengeDef = new ChallengeDef
			{
				ID = new ChallengeID(cChallenge.ID),
				NameLocKey = cChallenge.NameLocKey,
				DescriptionLocKey = cChallenge.DescriptionLocKey,
				IllustrationKey = cChallenge.Name
			};
			repo.Challenges[challengeDef.ID] = challengeDef;
		}
	}

	private IEnumerable<FHGameException> ValidateGlue(DefRepository repo)
	{
		foreach (ScenarioDef def in repo.Scenarios.Values)
		{
			if (ScenarioRuleClient.SRLYML.GetCustomLevel(def.LevelID) == null)
			{
				yield return new FHGameException($"Missing level '{def.LevelID}' in ScenarioDef '{def.ID}'");
			}
			if (def.HasAlternativeLevelID && ScenarioRuleClient.SRLYML.GetCustomLevel(def.AlternativeLevelID) == null)
			{
				yield return new FHGameException($"Missing level '{def.HasAlternativeLevelID}' in ScenarioDef '{def.ID}'");
			}
		}
	}
}
