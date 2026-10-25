using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP.Extensions;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Scenario.DTO;
using FHGUI.Services;
using FHRL;
using GLOOM;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using SharedLibrary.Achievments;
using UnityEngine;

namespace FHGUI.Scenario.Services;

public class ScenarioMechanicViewService : FHScenarioViewService
{
	[ConsoleVariable(Alias = "scenario_mechanics_force_show_articles", Persistent = false)]
	private static bool _forceShowArticles;

	public ScenarioMechanicViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public ScenarioMechanicsInfoModule.Params GetData(ScenarioMechanicsConfig config)
	{
		ScenarioRuleLibrary.ScenarioState scenarioState = ScenarioManager.CurrentScenarioState;
		List<ScenarioMechanicDTO> list = new List<ScenarioMechanicDTO>();
		string scenarioId = scenarioState.ID.RemoveChar('\'');
		foreach (ScenarioMechanicsConfig.Entry item in config.Entries.Where((ScenarioMechanicsConfig.Entry entry) => !entry.Hidden && entry.ID.Contains(scenarioId)))
		{
			if (CheckActive(item))
			{
				list.Add(new ScenarioMechanicDTO
				{
					ID = item.ID,
					Content = item.Content,
					StyleCategory = item.StyleCategory,
					SimpleContentKey = item.SimpleContentKey,
					Seen = HasShownMechanics(item.ID)
				});
			}
		}
		return new ScenarioMechanicsInfoModule.Params
		{
			LevelID = scenarioId,
			ScenarioNameKey = ScenarioResultViewService.GetCurrentScenarioNameKey(),
			WinObjectives = scenarioState.WinObjectives,
			LoseObjectives = scenarioState.LoseObjectives,
			ScenarioRules = list,
			Challenges = ScenarioResultViewService.GetChallenges(EResult.None),
			Favors = ScenarioResultViewService.GetFavors(),
			Loot = GetCurrentLoot(),
			CustomData = GetCustomScenarioData()
		};
		bool CheckActive(ScenarioMechanicsConfig.Entry entry)
		{
			if (_forceShowArticles)
			{
				return true;
			}
			if (!entry.ID.Contains(scenarioId, StringComparison.InvariantCulture))
			{
				return false;
			}
			if (entry.Trigger.Contains("RoomRevealed_All"))
			{
				return ScenarioManager.CurrentScenarioState.Maps.All((CMap map) => map.Revealed);
			}
			if (entry.Trigger.Contains("RoomRevealed_Any"))
			{
				return ScenarioManager.CurrentScenarioState.Maps.Count((CMap map) => map.Revealed) > 1;
			}
			if (entry.Trigger.Contains("RoomRevealed"))
			{
				CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => entry.Trigger.Contains(x.MapGuid));
				if (cMap?.MapTiles != null)
				{
					return cMap.Revealed;
				}
				return false;
			}
			if (entry.Trigger.Contains("ModifierActivated"))
			{
				string value = entry.Trigger.Replace("ModifierActivated_", "");
				foreach (CScenarioModifier scenarioModifier in scenarioState.ScenarioModifiers)
				{
					if (!scenarioModifier.EventIdentifier.IsNullOrEmpty() && scenarioModifier.EventIdentifier.Contains(value) && !scenarioModifier.Deactivated && !scenarioModifier.IsHidden)
					{
						return true;
					}
				}
				return false;
			}
			if (entry.Trigger.Contains("ModifierShown"))
			{
				string value2 = entry.Trigger.Replace("ModifierShown_", "");
				foreach (CScenarioModifier scenarioModifier2 in scenarioState.ScenarioModifiers)
				{
					if (!scenarioModifier2.EventIdentifier.IsNullOrEmpty() && scenarioModifier2.EventIdentifier.Contains(value2) && !scenarioModifier2.IsHidden)
					{
						return true;
					}
				}
				return false;
			}
			if (entry.Trigger.Contains("ModifierDeactivated"))
			{
				string value3 = entry.Trigger.Replace("ModifierDeactivated_", "");
				foreach (CScenarioModifier scenarioModifier3 in scenarioState.ScenarioModifiers)
				{
					if (!scenarioModifier3.EventIdentifier.IsNullOrEmpty() && scenarioModifier3.EventIdentifier.Contains(value3) && (scenarioModifier3.Deactivated || scenarioModifier3.IsHidden))
					{
						return true;
					}
				}
				return false;
			}
			if (entry.Trigger.Contains("ObjectiveCompleted"))
			{
				string value4 = entry.Trigger.Replace("ObjectiveCompleted_", "");
				foreach (CObjective allObjective in scenarioState.AllObjectives)
				{
					if (allObjective.EventIdentifier.Contains(value4) && allObjective.IsComplete)
					{
						return true;
					}
				}
			}
			if (entry.Trigger.Contains("ObjectiveActivated"))
			{
				string value5 = entry.Trigger.Replace("ObjectiveActivated_", "");
				foreach (CObjective allObjective2 in scenarioState.AllObjectives)
				{
					if (allObjective2.EventIdentifier.Contains(value5) && allObjective2.IsActive && !allObjective2.IsComplete)
					{
						return true;
					}
				}
			}
			if (entry.Trigger.Contains("RoundStarted"))
			{
				int num = entry.Trigger.Replace("RoundStarted_", "").ToInt32(int.MaxValue);
				return scenarioState.RoundNumber >= num;
			}
			if (entry.Trigger.Contains("RoundRange"))
			{
				(int, int) tuple = ExtractRange(entry.Trigger);
				if (scenarioState.RoundNumber >= tuple.Item1)
				{
					return scenarioState.RoundNumber <= tuple.Item2;
				}
				return false;
			}
			if (entry.Trigger.Contains("PartySizeRange"))
			{
				(int, int) tuple2 = ExtractRange(entry.Trigger);
				int count = ScenarioManager.CurrentScenarioState.Players.Count;
				if (count >= tuple2.Item1)
				{
					return count <= tuple2.Item2;
				}
				return false;
			}
			if (entry.Trigger.Contains("ScenarioLevel"))
			{
				(int, int) tuple3 = ExtractRange(entry.Trigger);
				if (ScenarioManager.CurrentScenarioState.Level >= tuple3.Item1)
				{
					return ScenarioManager.CurrentScenarioState.Level <= tuple3.Item2;
				}
				return false;
			}
			if (entry.Trigger.Contains("RoomLocked"))
			{
				CMap cMap2 = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => entry.Trigger.Contains(x.MapGuid));
				if (cMap2 != null)
				{
					return IsLockedRoom(cMap2);
				}
				return false;
			}
			if (entry.Trigger.Contains("RoomUnlocked"))
			{
				CMap cMap3 = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => entry.Trigger.Contains(x.MapGuid));
				if (cMap3 != null)
				{
					return !IsLockedRoom(cMap3);
				}
				return false;
			}
			if (entry.Trigger.Contains("HasAnyClosedRoom"))
			{
				return !ScenarioManager.CurrentScenarioState.Maps.All((CMap map) => map.Revealed);
			}
			if (entry.Trigger.Contains("RoomNotRevealed"))
			{
				return ScenarioManager.CurrentScenarioState.Maps.Any((CMap map) => entry.Trigger.Contains(map.MapGuid) && !map.Revealed);
			}
			if (entry.Trigger.Contains("RoomNotUnlocked"))
			{
				return ScenarioManager.CurrentScenarioState.Maps.Any((CMap map) => entry.Trigger.Contains(map.MapGuid) && IsLockedRoom(map));
			}
			if (entry.Trigger.Contains("Quest_Start"))
			{
				return true;
			}
			if (entry.Trigger.Contains("ActorHasBuff_FH066_MoveBuff"))
			{
				return true;
			}
			if (entry.Trigger.Contains("HeroHasBuff"))
			{
				string buffName = entry.Trigger.Replace("HeroHasBuff_", "");
				return ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor actor) => actor.ActiveBonuses.Any((CActiveBonus x) => x.Ability.Name == buffName));
			}
			if (entry.Trigger.Contains("CampaignSectionTriggered"))
			{
				string sectionID = entry.Trigger.Replace("CampaignSectionTriggered_", "");
				return ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioRuleLibrary.ScenarioState.TriggeredLevelMessageInfo info) => info.MessageName == sectionID);
			}
			if (entry.Trigger.Contains("CampaignSectionNotTriggered"))
			{
				string sectionID2 = entry.Trigger.Replace("CampaignSectionNotTriggered_", "");
				return !ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioRuleLibrary.ScenarioState.TriggeredLevelMessageInfo info) => info.MessageName == sectionID2);
			}
			if (entry.Trigger.Contains("HasActiveMonster"))
			{
				string monsterID = entry.Trigger.Replace("HasActiveMonster_", "");
				return ScenarioManager.Scenario.AllMonsters.Any((CEnemyActor x) => !x.IsDead && x.Class.ID.StartsWith(monsterID));
			}
			return false;
		}
	}

	private static bool IsLockedRoom(CMap room)
	{
		bool result = false;
		foreach (CMapTile mapTile in room.MapTiles)
		{
			List<CObjectProp> list = ScenarioManager.GetTile(mapTile.ArrayIndex).m_Props.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num] is CObjectDoor cObjectDoor)
				{
					result = true;
					if (!cObjectDoor.DoorIsLocked)
					{
						return false;
					}
				}
			}
		}
		return result;
	}

	private List<SpriteWithTooltipDTO> GetCurrentLoot()
	{
		List<SpriteWithTooltipDTO> list = new List<SpriteWithTooltipDTO>();
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return list;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		OutpostResources outpostResources = default;
		OutpostResources outpostResources2 = default;
		ScenarioDef.LevelData levelData = YMLDefRepository.Repo.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) => pair.Value.LevelID == ScenarioManager.CurrentScenarioState.ID || pair.Value.AlternativeLevelID == ScenarioManager.CurrentScenarioState.ID).Value?.Levels.FirstOrDefault((ScenarioDef.LevelData data) => data.ID == ScenarioManager.CurrentScenarioState.ID);
		if (levelData != null)
		{
			num2 = (int)levelData.Loot.Gold;
			num4 = levelData.Chests.Count((ScenarioDef.Chest chest) => !chest.TreasureTableIDs.Any((string id) => ScenarioManager.CurrentScenarioState.ScenarioSetup.AlreadyLootedChests.Contains(id)));
			num6 = (int)((!ScenarioManager.CurrentScenarioState.ScenarioSetup.RandomItemLooted) ? levelData.Loot.RandomItemCount : 0);
			outpostResources2 += levelData.Loot.Resources;
		}
		foreach (byte item6 in ScenarioManager.CurrentScenarioState.LootDeck)
		{
			LootCard lootCard = CLootDeck.AllCards[item6];
			if (lootCard.Type == LootCard.EType.None)
			{
				break;
			}
			switch (lootCard.Type)
			{
			case LootCard.EType.RandomItem:
			{
				FHScenarioSetup scenarioSetup = ScenarioManager.CurrentScenarioState.ScenarioSetup;
				if (!scenarioSetup.RandomItemLooted && scenarioSetup.NextRandomItemID == 0)
				{
					num++;
				}
				else
				{
					num5++;
				}
				break;
			}
			case LootCard.EType.Money:
				num++;
				break;
			case LootCard.EType.Lumber:
				outpostResources.AddMaterial(OutpostMaterialType.Lumber);
				break;
			case LootCard.EType.Metal:
				outpostResources.AddMaterial(OutpostMaterialType.Metal);
				break;
			case LootCard.EType.Hide:
				outpostResources.AddMaterial(OutpostMaterialType.Hide);
				break;
			case LootCard.EType.Arrowvine:
				outpostResources.AddHerb(OutpostHerbType.Arrowvine);
				break;
			case LootCard.EType.Axenut:
				outpostResources.AddHerb(OutpostHerbType.Axenut);
				break;
			case LootCard.EType.Corpsecap:
				outpostResources.AddHerb(OutpostHerbType.Corpsecap);
				break;
			case LootCard.EType.Flamefruit:
				outpostResources.AddHerb(OutpostHerbType.Flamefruit);
				break;
			case LootCard.EType.Rockroot:
				outpostResources.AddHerb(OutpostHerbType.Rockroot);
				break;
			case LootCard.EType.Snowthistle:
				outpostResources.AddHerb(OutpostHerbType.Snowthistle);
				break;
			}
		}
		if (num > 0 || num2 > 0)
		{
			SpriteWithTooltipDTO item = new SpriteWithTooltipDTO
			{
				Sprite = UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized,
				Title = LocalizationManager.GetTranslation("Gold"),
				TextValue = FormatCountValue(num, num2),
				AdditionalData = IsDepleted(num, num2)
			};
			list.Add(item);
		}
		foreach (OutpostMaterialType item7 in OutpostResources.AllMaterialTypes())
		{
			int materialAmount = outpostResources.GetMaterialAmount(item7);
			int materialAmount2 = outpostResources2.GetMaterialAmount(item7);
			if (materialAmount > 0 || materialAmount2 > 0)
			{
				SpriteWithTooltipDTO item2 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(item7.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetMaterialLocalizationKey(item7)),
					TextValue = FormatCountValue(materialAmount, materialAmount2),
					AdditionalData = IsDepleted(materialAmount, materialAmount2)
				};
				list.Add(item2);
			}
		}
		foreach (OutpostHerbType allHerb in OutpostResources.GetAllHerbs())
		{
			int herbAmount = outpostResources.GetHerbAmount(allHerb);
			int herbAmount2 = outpostResources2.GetHerbAmount(allHerb);
			if (herbAmount > 0 || herbAmount2 > 0)
			{
				SpriteWithTooltipDTO item3 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(allHerb.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(allHerb)),
					TextValue = FormatCountValue(herbAmount, herbAmount2),
					AdditionalData = IsDepleted(herbAmount, herbAmount2)
				};
				list.Add(item3);
			}
		}
		for (int num7 = 0; num7 < ScenarioManager.Tiles.GetLength(0); num7++)
		{
			for (int num8 = 0; num8 < ScenarioManager.Tiles.GetLength(1); num8++)
			{
				CTile cTile = ScenarioManager.Tiles[num7, num8];
				if (cTile == null)
				{
					continue;
				}
				foreach (CObjectProp item8 in cTile.m_Props.Where((CObjectProp p) => p.IsLootable))
				{
					if (item8.ObjectType == ScenarioManager.ObjectImportType.Chest)
					{
						num3++;
					}
				}
			}
		}
		if (num5 > 0 || num6 > 0)
		{
			SpriteWithTooltipDTO item4 = new SpriteWithTooltipDTO
			{
				Sprite = UIInfoTools.Instance.GetGenericIcon("RandomItem"),
				Title = LocalizationManager.GetTranslation("RANDOM_ITEM_TOOLTIP"),
				TextValue = FormatCountValue(num5, num6),
				AdditionalData = IsDepleted(num5, num6)
			};
			list.Add(item4);
		}
		if (num3 > 0 || num4 > 0)
		{
			SpriteWithTooltipDTO item5 = new SpriteWithTooltipDTO
			{
				Sprite = UIInfoTools.Instance.GetGenericIcon("TreasureChest"),
				Title = LocalizationManager.GetTranslation("TREASURE_CHEST_TOOLTIP"),
				TextValue = FormatCountValue(num3, num4),
				AdditionalData = IsDepleted(num3, num4)
			};
			list.Add(item5);
		}
		return list;
		static string FormatCountValue(int num9, int initial)
		{
			if (num9 == 0 && initial > 0)
			{
				return FHUILocFormatting.Color(num9.ToString(), Color.gray);
			}
			return num9.ToString();
		}
		static bool IsDepleted(int num9, int initial)
		{
			if (num9 == 0)
			{
				return initial > 0;
			}
			return false;
		}
	}

	private object GetCustomScenarioData()
	{
		if (ScenarioScript.current is ScenarioScript_FH024)
		{
			ScenarioScript_FH024 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH024;
			List<(string, bool)> list = new List<(string, bool)>();
			int num = scenarioScript_FH.Deck.Count + scenarioScript_FH.DeckSpend.Count;
			for (int i = 1; i < num + 1; i++)
			{
				list.Add((scenarioScript_FH.GetFxName(i), !scenarioScript_FH.DeckSpend.Contains(i)));
			}
			return list;
		}
		return null;
	}

	public bool HasShownMechanics(string key)
	{
		return GlobalStatRepository.Instance.ShownScenarioMechanics.HasValue(key);
	}

	public void SetMechanicsAsShown(string key)
	{
		GlobalStatRepository.Instance.ShownScenarioMechanics.NewValue(key);
	}

	public bool Validate(ScenarioMechanicsConfig config, string eventKey, object[] parameters, out bool isNew)
	{
		isNew = false;
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState == null)
		{
			return false;
		}
		string levelID = currentScenarioState.ID.RemoveChar('\'');
		switch (eventKey)
		{
		case "ScenarioLoaded":
		{
			bool result10 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry entry2 in config.GetRelevant(levelID))
				{
					if (entry2.Trigger.Contains("Quest_Start"))
					{
						isNew |= !HasShownMechanics(entry2.ID);
						result10 = true;
					}
					else if (entry2.Trigger.Contains("HasAnyClosedRoom") && !ScenarioManager.CurrentScenarioState.Maps.All((CMap map) => map.Revealed))
					{
						isNew |= !HasShownMechanics(entry2.ID);
						result10 = true;
					}
					else if (entry2.Trigger.Contains("RoomNotRevealed") && ScenarioManager.CurrentScenarioState.Maps.Any((CMap map) => entry2.Trigger.Contains(map.MapGuid) && !map.Revealed))
					{
						isNew |= !HasShownMechanics(entry2.ID);
						result10 = true;
					}
					else if (entry2.Trigger.Contains("RoomNotUnlocked") && ScenarioManager.CurrentScenarioState.Maps.Any((CMap map) => entry2.Trigger.Contains(map.MapGuid) && IsLockedRoom(map)))
					{
						isNew |= !HasShownMechanics(entry2.ID);
						result10 = true;
					}
					else if (entry2.Trigger.Contains("ScenarioLevel"))
					{
						(int, int) tuple2 = ExtractRange(entry2.Trigger);
						if (ScenarioManager.CurrentScenarioState.Level >= tuple2.Item1 && ScenarioManager.CurrentScenarioState.Level <= tuple2.Item2)
						{
							isNew |= !HasShownMechanics(entry2.ID);
							result10 = true;
						}
					}
					else if (entry2.Trigger.Contains("CampaignSectionNotTriggered"))
					{
						string sectionID2 = entry2.Trigger.Replace("CampaignSectionNotTriggered_", "");
						if (!ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioRuleLibrary.ScenarioState.TriggeredLevelMessageInfo info) => info.MessageName == sectionID2))
						{
							result10 = true;
							isNew |= !HasShownMechanics(entry2.ID);
						}
					}
					else if (entry2.Trigger.Contains("PartySizeRange"))
					{
						(int, int) tuple3 = ExtractRange(entry2.Trigger);
						int count = ScenarioManager.CurrentScenarioState.Players.Count;
						if (count >= tuple3.Item1 && count <= tuple3.Item2)
						{
							result10 = true;
							isNew |= !HasShownMechanics(entry2.ID);
						}
					}
				}
				return result10;
			}
		}
		case "UpdateScenarioModifierDescriptions":
		case "ScenarioModifierHiddenStateUpdated":
		{
			bool result9 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item in config.GetRelevant(levelID))
				{
					if (item.Trigger.Contains("ModifierActivated"))
					{
						string value2 = item.Trigger.Replace("ModifierActivated_", "");
						foreach (CScenarioModifier scenarioModifier in currentScenarioState.ScenarioModifiers)
						{
							if (!scenarioModifier.EventIdentifier.IsNullOrEmpty() && scenarioModifier.EventIdentifier.Contains(value2) && !scenarioModifier.Deactivated && !scenarioModifier.IsHidden)
							{
								result9 = true;
								isNew |= !HasShownMechanics(item.ID);
							}
						}
					}
					if (!item.Trigger.Contains("ModifierShown"))
					{
						continue;
					}
					string value3 = item.Trigger.Replace("ModifierShown_", "");
					foreach (CScenarioModifier scenarioModifier2 in currentScenarioState.ScenarioModifiers)
					{
						if (!scenarioModifier2.EventIdentifier.IsNullOrEmpty() && scenarioModifier2.EventIdentifier.Contains(value3) && !scenarioModifier2.IsHidden)
						{
							result9 = true;
							isNew |= !HasShownMechanics(item.ID);
						}
					}
				}
				return result9;
			}
		}
		case "RoomRevealed":
		{
			bool result8 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item2 in config.GetRelevant(levelID))
				{
					if ((item2.Trigger.Contains("RoomRevealed_All") && ScenarioManager.CurrentScenarioState.Maps.All((CMap map) => map.Revealed)) || (item2.Trigger.Contains("RoomRevealed_Any") && ScenarioManager.CurrentScenarioState.Maps.Count((CMap map) => map.Revealed) > 1) || (item2.Trigger.Contains("RoomRevealed") && item2.Trigger.Contains((string)parameters[0])))
					{
						result8 = true;
						isNew |= !HasShownMechanics(item2.ID);
					}
				}
				return result8;
			}
		}
		case "ObjectiveCompleted":
		{
			bool result7 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item3 in config.GetRelevant(levelID))
				{
					if (item3.Trigger.Contains("ObjectiveCompleted") && item3.Trigger.Contains((string)parameters[0]))
					{
						result7 = true;
						isNew |= !HasShownMechanics(item3.ID);
					}
				}
				return result7;
			}
		}
		case "ObjectiveActivated":
		{
			bool result6 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item4 in config.GetRelevant(levelID))
				{
					if (item4.Trigger.Contains("ObjectiveActivated") && item4.Trigger.Contains((string)parameters[0]))
					{
						result6 = true;
						isNew |= !HasShownMechanics(item4.ID);
					}
				}
				return result6;
			}
		}
		case "RoundStarted":
		{
			bool result5 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item5 in config.GetRelevant(levelID))
				{
					if (item5.Trigger.Contains("RoundStarted") && item5.Trigger.Replace("RoundStarted_", "").ToInt32(int.MaxValue) == (int)parameters[0])
					{
						result5 = true;
						isNew |= !HasShownMechanics(item5.ID);
					}
					if (item5.Trigger.Contains("RoundRange"))
					{
						(int, int) tuple = ExtractRange(item5.Trigger);
						if ((int)parameters[0] >= tuple.Item1 && (int)parameters[0] <= tuple.Item2)
						{
							result5 = true;
							isNew |= !HasShownMechanics(item5.ID);
						}
					}
				}
				return result5;
			}
		}
		case "DoorUnlocked":
		{
			bool result4 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry entry in config.GetRelevant(levelID))
				{
					if (entry.Trigger.Contains("RoomUnlocked") && GetRelevantUnlockedRoom((string)parameters[0]).Any((string s) => entry.Trigger.Contains(s)))
					{
						result4 = true;
						isNew |= !HasShownMechanics(entry.ID);
					}
				}
				return result4;
			}
		}
		case "ActiveBonusesChanged":
		{
			bool result3 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item6 in config.GetRelevant(levelID))
				{
					if (item6.Trigger.Contains("HeroHasBuff"))
					{
						string buffName = item6.Trigger.Replace("HeroHasBuff_", "");
						if (ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor actor) => actor.ActiveBonuses.Any((CActiveBonus x) => x.Ability.Name == buffName)))
						{
							result3 = true;
							isNew |= !HasShownMechanics(item6.ID);
						}
					}
				}
				return result3;
			}
		}
		case "CampaignSectionTriggered":
		{
			bool result2 = false;
			{
				foreach (ScenarioMechanicsConfig.Entry item7 in config.GetRelevant(levelID))
				{
					if (item7.Trigger.Contains("CampaignSectionTriggered"))
					{
						string sectionID = item7.Trigger.Replace("CampaignSectionTriggered_", "");
						if (ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioRuleLibrary.ScenarioState.TriggeredLevelMessageInfo info) => info.MessageName == sectionID))
						{
							result2 = true;
							isNew |= !HasShownMechanics(item7.ID);
						}
					}
				}
				return result2;
			}
		}
		case "ActorSpawned":
		{
			bool result = false;
			CActor cActor = parameters[0] as CActor;
			{
				foreach (ScenarioMechanicsConfig.Entry item8 in config.GetRelevant(levelID))
				{
					if (item8.Trigger.Contains("HasActiveMonster"))
					{
						string value = item8.Trigger.Replace("HasActiveMonster_", "");
						if (cActor.Class.ID.StartsWith(value))
						{
							result = true;
							isNew |= !HasShownMechanics(item8.ID);
						}
					}
				}
				return result;
			}
		}
		default:
			return false;
		}
	}

	private static (int min, int max) ExtractRange(ReadOnlySpan<char> s)
	{
		int num = s.IndexOf('-');
		if (num == -1)
		{
			return (min: 0, max: 0);
		}
		ReadOnlySpan<char> readOnlySpan = s.Slice(0, num);
		ReadOnlySpan<char> s2 = s.Slice(num + 1);
		int i;
		for (i = 0; i < readOnlySpan.Length && !char.IsDigit(readOnlySpan[i]) && readOnlySpan[i] != '-'; i++)
		{
		}
		int num2 = int.Parse(readOnlySpan.Slice(i));
		int num3 = int.Parse(s2);
		if (num2 >= num3)
		{
			return (min: num3, max: num2);
		}
		return (min: num2, max: num3);
	}

	private static IEnumerable<string> GetRelevantUnlockedRoom(string doorID)
	{
		List<CMap> maps = ScenarioManager.CurrentScenarioState.Maps;
		foreach (CMap room in maps)
		{
			foreach (CMapTile mapTile in room.MapTiles)
			{
				CTile tile = ScenarioManager.GetTile(mapTile.ArrayIndex);
				List<CObjectProp> currentTileProps = tile.m_Props.ToList();
				for (int i = currentTileProps.Count - 1; i >= 0; i--)
				{
					if (currentTileProps[i] is CObjectDoor { DoorIsLocked: false })
					{
						yield return room.MapGuid;
					}
				}
			}
		}
	}

	public static bool HasSpecialRulesData()
	{
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState == null)
		{
			return false;
		}
		ScenarioMechanicsConfig scenarioMechanicsConfig = Singleton<FHScenarioViewController>.InstanceFast?.ScenarioMechanics?.MechanicsData;
		if (scenarioMechanicsConfig != null)
		{
			return scenarioMechanicsConfig.GetRelevant(currentScenarioState.ID).Any((ScenarioMechanicsConfig.Entry entry) => !entry.Hidden && entry.StyleCategory == ScenarioMechanicsConfig.Entry.Category.Rule);
		}
		return false;
	}

	public static LocalizationParam[] GetRulesLocalizationParams()
	{
		List<LocalizationParam> list = new List<LocalizationParam>();
		list.Add(new LocalizationParam("trap", ScenarioManager.TrapDamage.ToString()));
		list.Add(new LocalizationParam("hazardous", ScenarioManager.HazardousTerrainDamage.ToString()));
		if (ScenarioScript.current == null)
		{
			return list.ToArray();
		}
		foreach (var item in ScenarioScript.current.GetSpecialRulesParamsString())
		{
			list.Add(new LocalizationParam(item.Item1, item.Item2));
		}
		return list.ToArray();
	}
}
