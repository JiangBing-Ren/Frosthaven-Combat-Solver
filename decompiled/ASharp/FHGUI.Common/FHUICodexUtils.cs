using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.Common.DTO;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Common;

public class FHUICodexUtils
{
	private record InnateData
	{
		public string Type;

		public int Amount;

		public string DisplayValue;

		public string Description;

		public InnateData(string type, int amount, string displayValue, string description)
		{
			Type = type;
			Amount = amount;
			DisplayValue = displayValue;
			Description = description;
		}
	}

	private const int MaxMonsterLevel = 7;

	public static readonly List<CAbility.EAbilityType> InvalidImmunities = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.Sleep,
		CAbility.EAbilityType.Regenerate,
		CAbility.EAbilityType.Ward
	};

	private static CampaignSettings Settings { get; set; } = new CampaignSettings();

	public static int? GetMonsterOverrideLevel()
	{
		if (ScenarioManager.IsInTactical)
		{
			return ScenarioManager.CurrentScenarioState.Level;
		}
		if (Singleton<FHMapChoreographer>.IsInitialized && Singleton<FHMapChoreographer>.Instance?.GameState?.ActiveScenario != null)
		{
			return 0;
		}
		return null;
	}

	public static MonsterCardUIContext GetCardUIContext(int cardID, CMonsterClass monsterClass, int? overrideLevel = null)
	{
		return new MonsterCardUIContext
		{
			CardYML = UIGeneralInfoUtils.GetMonsterCardYML(cardID),
			EnemyForBaseStats = null,
			FallbackMonsterClass = monsterClass,
			ShowFaction = UICharacterUtils.HasFactions(),
			CardPile = UIGeneralInfoUtils.GetMonsterCardPile(cardID, monsterClass),
			OverrideLevel = overrideLevel
		};
	}

	public static TableData PopulateBossData(string monsterId, int highlightLevel, float rowHeight, float columnWidth, Color textNormal, Color textLevel, Color textHealth, FHUICodexTableCell cellTextField, bool includeImmunities = true)
	{
		return new TableData();
	}

	public static TableData PopulateMonsterData(string monsterId, int highlightLevel, float rowHeight, float columnWidth, Color textNormal, Color textLevel, Color textHealth, FHUICodexTableCell cellTextField, bool includeImmunities = true)
	{
		TableData tableData = new TableData();
		CMonsterClass cMonsterClass = MonsterClassManager.Find(monsterId);
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Level",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Health",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Damage",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Movement",
			Width = columnWidth
		});
		Dictionary<string, StatusEffectDTO> dictionary = new Dictionary<string, StatusEffectDTO>();
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i <= 7; i++)
		{
			BaseStats monsterStatsData = cMonsterClass.GetMonsterStatsData(i);
			if (includeImmunities)
			{
				foreach (StatusEffectDTO immunity in UICharacterUtils.GetImmunities(monsterStatsData.Immunities, cMonsterClass))
				{
					if (!dictionary.ContainsKey(immunity.Name))
					{
						dictionary.Add(immunity.Name, immunity);
					}
				}
			}
			foreach (InnateData monsterInnate in GetMonsterInnates(monsterStatsData, cMonsterClass.OriginalBaseStats))
			{
				hashSet.Add(monsterInnate.Type);
			}
		}
		List<CellData> list = new List<CellData>();
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Level"),
			Tooltip = LocalizationManager.GetTranslation("GUI_LEVEL")
		});
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Health"),
			Tooltip = LocalizationManager.GetTranslation("Health")
		});
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("MeleeAttack"),
			Tooltip = LocalizationManager.GetTranslation("Attack")
		});
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Move"),
			Tooltip = LocalizationManager.GetTranslation("Move")
		});
		foreach (string item3 in hashSet)
		{
			if (GetInsertColumnID(item3) != null)
			{
				continue;
			}
			Sprite sprite = null;
			if (UIInfoTools.Instance != null)
			{
				sprite = UIInfoTools.Instance.GetGenericIcon(GetInnateColumnID(item3));
				if ((object)sprite == null)
				{
					sprite = UIInfoTools.Instance.GetActiveAbilityIcon(GetInnateColumnID(item3));
				}
			}
			if (sprite == null)
			{
				list.Add(new CellData
				{
					TextValue = item3
				});
			}
			else
			{
				list.Add(new CellData
				{
					Icon = sprite,
					Tooltip = LocalizationManager.GetTranslation(GetInnateColumnLocKey(item3))
				});
			}
			tableData.Columns.Add(new ColumnDefinition
			{
				Id = item3,
				Width = columnWidth
			});
		}
		foreach (KeyValuePair<string, StatusEffectDTO> item4 in dictionary)
		{
			Sprite icon = item4.Value.Icon;
			if (icon == null)
			{
				list.Add(new CellData
				{
					TextValue = item4.Value.Name
				});
			}
			else
			{
				list.Add(new CellData
				{
					Icon = icon,
					Tooltip = item4.Value.Name
				});
			}
			tableData.Columns.Add(new ColumnDefinition
			{
				Id = item4.Key
			});
		}
		tableData.AddRow(list, rowHeight);
		for (int j = 0; j <= 7; j++)
		{
			BaseStats monsterStatsData2 = cMonsterClass.GetMonsterStatsData(j);
			BaseStatsDTO baseStatsDTO = new BaseStatsDTO(monsterStatsData2);
			List<InnateData> monsterInnates = GetMonsterInnates(monsterStatsData2, cMonsterClass.OriginalBaseStats);
			IEnumerable<StatusEffectDTO> immunities = UICharacterUtils.GetImmunities(monsterStatsData2.Immunities, cMonsterClass);
			List<CellData> list2 = new List<CellData>();
			CellData item = new CellData
			{
				TextValue = FHUILocFormatting.Color(j.ToString(), textLevel)
			};
			CellData item2 = new CellData
			{
				TextValue = FHUILocFormatting.Color(GetMonsterStat(EMonsterBaseStats.Health, monsterStatsData2, baseStatsDTO.Health).ToString(), textHealth)
			};
			CellData cellData = new CellData
			{
				TextValue = FHUILocFormatting.Color(GetMonsterStat(EMonsterBaseStats.Attack, monsterStatsData2, baseStatsDTO.Attack).ToString(), textNormal)
			};
			CellData cellData2 = new CellData
			{
				TextValue = FHUILocFormatting.Color(GetMonsterStat(EMonsterBaseStats.Move, monsterStatsData2, baseStatsDTO.Movement).ToString(), textNormal)
			};
			list2.Add(item);
			list2.Add(item2);
			list2.Add(cellData);
			list2.Add(cellData2);
			tableData.AddRow(list2, rowHeight, j == highlightLevel);
			foreach (InnateData item5 in monsterInnates)
			{
				string insertColumnID = GetInsertColumnID(item5.Type);
				if (insertColumnID != null)
				{
					if (insertColumnID == "Move")
					{
						cellData2.TextValue += item5.DisplayValue;
					}
					if (insertColumnID == "Attack")
					{
						cellData.TextValue += item5.DisplayValue;
					}
				}
			}
			if (tableData.Columns.Count <= 4)
			{
				continue;
			}
			for (int k = 4; k < tableData.Columns.Count; k++)
			{
				string ID = tableData.Columns[k].Id;
				CellData cellData3 = new CellData
				{
					TextValue = "-"
				};
				list2.Add(cellData3);
				if (includeImmunities)
				{
					StatusEffectDTO statusEffectDTO = immunities.FirstOrDefault((StatusEffectDTO effectDto) => effectDto.Name == ID);
					if (statusEffectDTO != null)
					{
						cellData3.Icon = statusEffectDTO.Icon;
						cellData3.TextValue = null;
						continue;
					}
				}
				InnateData innateData = monsterInnates.FirstOrDefault((InnateData pair) => pair.Type == ID);
				if (innateData != null)
				{
					Sprite sprite2 = ((UIInfoTools.Instance != null) ? UIInfoTools.Instance.GetActiveAbilityIcon(ID) : null);
					if (innateData.Amount != 0)
					{
						cellData3.TextValue = innateData.Amount.ToString();
					}
					else if (innateData.DisplayValue.IsNOTNullOrEmpty())
					{
						cellData3.TextValue = innateData.DisplayValue;
					}
					else if (sprite2 == null)
					{
						cellData3.TextValue = innateData.DisplayValue;
					}
					else
					{
						cellData3.Icon = sprite2;
					}
					cellData3.Tooltip = innateData.Description;
				}
			}
		}
		return tableData;
	}

	private static int GetMonsterStat(EMonsterBaseStats type, BaseStats stats, int defaultValue, int partySize = 2)
	{
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXEntry in stats.StatIsBasedOnXEntries)
		{
			if (statIsBasedOnXEntry.BaseStatType != type)
			{
				continue;
			}
			if (statIsBasedOnXEntry.BasedOn == CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount)
			{
				int num = (int)((float)Mathf.Max((ScenarioManager.CurrentScenarioState?.Players?.Count).GetValueOrDefault(), partySize) * statIsBasedOnXEntry.Multiplier);
				if (statIsBasedOnXEntry.AddTo)
				{
					return defaultValue + num;
				}
				return num;
			}
			if (statIsBasedOnXEntry.BasedOn == CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount)
			{
				int num2 = Mathf.Max((ScenarioManager.CurrentScenarioState?.Players?.Count).GetValueOrDefault(), partySize);
				if (statIsBasedOnXEntry.AddTo)
				{
					return defaultValue + num2;
				}
				return num2;
			}
		}
		return defaultValue;
	}

	private static string GetInsertColumnID(string id)
	{
		switch (id)
		{
		case "Flying":
			return "Move";
		case "Advantage":
		case "Disadvantage":
		case "Curse":
		case "Muddle":
		case "Poison":
		case "Brittle":
		case "Impair":
		case "Wound":
		case "Immobilize":
			return "Attack";
		default:
			return null;
		}
	}

	private static string GetInnateColumnID(string id)
	{
		if (id == "IsWaterMonster")
		{
			return "Move";
		}
		return id;
	}

	private static string GetInnateColumnLocKey(string id)
	{
		if (id == "IsWaterMonster")
		{
			return "monsterability_lightningeel";
		}
		return id;
	}

	private static List<InnateData> GetMonsterInnates(BaseStats currentStats, BaseStats originalStats, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		List<InnateData> list = new List<InnateData>();
		if (currentStats.Target > 1)
		{
			list.Add(new InnateData("Target", currentStats.Target, "<sprite name=Target>", ""));
		}
		if (currentStats.Pierce > 0)
		{
			list.Add(new InnateData("Pierce", currentStats.Pierce, "<sprite name=Pierce>", ""));
		}
		if (currentStats.Push > 0)
		{
			list.Add(new InnateData("Push", currentStats.Push, "<sprite name=Push>", ""));
		}
		if (currentStats.Flying)
		{
			list.Add(new InnateData("Flying", currentStats.Push, "<sprite name=Flying>", ""));
		}
		if (currentStats.Advantage)
		{
			list.Add(new InnateData("Advantage", currentStats.Push, "<sprite name=Advantage>", ""));
		}
		if (currentStats.AttackersGainDisadvantage)
		{
			list.Add(new InnateData("Disadvantage", currentStats.Push, "<sprite name=Disadvantage>", ""));
		}
		if (currentStats.IsWaterMonster)
		{
			list.Add(new InnateData("IsWaterMonster", 0, "", LocalizationManager.GetTranslation("monsterability_lightningeel")));
		}
		if (currentStats.Shield > 0 && (currentStats.ShieldStatIsBasedOnXEntries == null || !currentStats.ShieldStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Shield)))
		{
			list.Add(new InnateData("Shield", currentStats.Shield, "<sprite name=Shield>", ""));
		}
		if (currentStats.Retaliate > 0)
		{
			string text = "<sprite name=Retaliate> " + currentStats.Retaliate;
			text += ((currentStats.RetaliateRange > 1) ? (", <sprite name=Range> " + currentStats.RetaliateRange) : "");
			list.Add(new InnateData("Retaliate", (currentStats.RetaliateRange <= 1) ? currentStats.Retaliate : 0, text, ""));
		}
		if (currentStats.Conditions.Count > 0 || (additionalConditions != null && additionalConditions.Count > 0))
		{
			List<CCondition.ENegativeCondition> list2 = currentStats.Conditions.ToList();
			if (additionalConditions != null)
			{
				list2.AddRange(additionalConditions);
				list2 = list2.Distinct().ToList();
			}
			foreach (CCondition.ENegativeCondition item in list2)
			{
				list.Add(new InnateData(item.ToString(), 0, $"<size=-4><sprite name={item}></size>", ""));
			}
		}
		if (currentStats.AttackStatIsBasedOnXEntries != null || currentStats.ShieldStatIsBasedOnXEntries != null)
		{
			List<AbilityData.StatIsBasedOnXData> list3 = new List<AbilityData.StatIsBasedOnXData>();
			if (currentStats.AttackStatIsBasedOnXEntries != null)
			{
				list3.AddRange(currentStats.AttackStatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength));
			}
			if (currentStats.ShieldStatIsBasedOnXEntries != null)
			{
				list3.AddRange(currentStats.ShieldStatIsBasedOnXEntries);
			}
			foreach (AbilityData.StatIsBasedOnXData item2 in list3)
			{
				switch (item2.BasedOn)
				{
				case CAbility.EStatIsBasedOnXType.HexesMovedThisTurn:
					list.Add(new InnateData("HexesMoved", currentStats.Retaliate, "Boss_AddHexesMovedToAttackBuffText", ""));
					break;
				case CAbility.EStatIsBasedOnXType.ActorCount:
				case CAbility.EStatIsBasedOnXType.DeadActorCount:
				{
					string text2 = null;
					string className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterEnemyClasses?.FirstOrDefault();
					if (!className.IsNullOrEmpty())
					{
						MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == className);
						if (monsterYMLData != null)
						{
							text2 = monsterYMLData.LocKey;
						}
						else
						{
							Log.GUIError("Unable to find monster class " + className);
						}
					}
					else
					{
						className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterPlayerClasses?.FirstOrDefault();
						if (!className.IsNullOrEmpty())
						{
							CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == className);
							if (characterYMLData != null)
							{
								text2 = characterYMLData.LocKey;
							}
							else
							{
								Log.GUIError("Unable to find character class " + className);
							}
						}
						else
						{
							className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterHeroSummonClasses?.FirstOrDefault();
							if (!className.IsNullOrEmpty())
							{
								HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == className);
								if (heroSummonYMLData != null)
								{
									text2 = heroSummonYMLData.LocKey;
								}
								else
								{
									Log.GUIError("Unable to find Hero Summon class " + className);
								}
							}
							else
							{
								className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterObjectClasses?.FirstOrDefault();
								if (!className.IsNOTNullOrEmpty())
								{
									MonsterYMLData monsterYMLData2 = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == className);
									if (monsterYMLData2 != null)
									{
										text2 = monsterYMLData2.LocKey;
									}
								}
							}
						}
					}
					if (!text2.IsNOTNullOrEmpty())
					{
						break;
					}
					if (item2.BaseStatType == EMonsterBaseStats.Attack)
					{
						string text3 = "$Boss_BaseAttackStrengthIsEqualTo$ ";
						text3 += string.Format(LocalizationManager.GetTranslation("Boss_NumberOfXsPresent"), LocalizationManager.GetTranslation(text2));
						if (item2.Multiplier != 1f)
						{
							text3 = text3 + " $times$ " + item2.Multiplier;
						}
						list.Add(new InnateData("Retaliate", 0, text3, ""));
					}
					else if (item2.BaseStatType == EMonsterBaseStats.Shield)
					{
						string displayValue = string.Format(LocalizationManager.GetTranslation((item2.BasedOn == CAbility.EStatIsBasedOnXType.DeadActorCount) ? "INNATE_ShieldBasedOnNumberOfDeadXsPresent" : "INNATE_ShieldBasedOnNumberOfXsPresent"), (originalStats.Shield > 0) ? originalStats.Shield : 0, (item2.Multiplier >= 0f) ? " $plus$ " : " $minus$ ", LocalizationManager.GetTranslation(text2));
						list.Add(new InnateData("Shield", 0, displayValue, ""));
					}
					break;
				}
				}
			}
		}
		return list;
	}

	public static TableData PopulateBuildingCostData(uint buildingID, float rowHeight, float columnWidth, Color textNormal, Color textLevel)
	{
		TableData tableData = new TableData();
		BuildingTypeDef buildingTypeDef = YMLDefRepository.Repo?.DefRepository?.Buildings.Values.FirstOrDefault((BuildingTypeDef itemDef) => itemDef.ID.ID == buildingID);
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Level",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "TownProsperity",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Gold",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Lumber",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Metal",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Hide",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Morale",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Meral",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Hide",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Morale",
			Width = columnWidth
		});
		if (!buildingTypeDef.CannotBeDamaged)
		{
			tableData.Columns.Add(new ColumnDefinition
			{
				Id = "Repair",
				Width = columnWidth
			});
		}
		List<CellData> list = new List<CellData>();
		RowData rowData = tableData.AddRow(list, rowHeight);
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Level"),
			Tooltip = LocalizationManager.GetTranslation("GUI_LEVEL")
		});
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("NewBuilding"),
			TextValue = LocalizationManager.GetTranslation("GUI_Build")
		});
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		rowData.MergeCells(1, 5);
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Wrecks"),
			TextValue = LocalizationManager.GetTranslation("GUI_RebuildBuilding")
		});
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		rowData.MergeCells(7, 2);
		if (!buildingTypeDef.CannotBeDamaged)
		{
			list.Add(new CellData
			{
				Icon = UIInfoTools.Instance.GetGenericIcon("Repair")
			});
		}
		for (int num = 1; num <= buildingTypeDef.Levels; num++)
		{
			List<CellData> list2 = new List<CellData>();
			tableData.AddRow(list2, rowHeight);
			list2.Add(new CellData
			{
				TextValue = FHUILocFormatting.Color(num.ToString(), textLevel)
			});
			uint prosperityRequirement = buildingTypeDef.GetProsperityRequirement(num);
			if (prosperityRequirement != 0)
			{
				list2.Add(new CellData
				{
					TextValue = FHUILocFormatting.Color(prosperityRequirement.ToString(), textNormal),
					Icon = UIInfoTools.Instance.GetGenericIcon("TownProsperityLevel")
				});
			}
			else
			{
				list2.Add(CellData.Empty());
			}
			BuildingTypeDef.Cost constructCost = buildingTypeDef.GetConstructCost(num);
			list2.Add(ResourceToCellCellData("Gold", (int)constructCost.Gold, textNormal));
			list2.Add(ResourceToCellCellData("Lumber", constructCost.Resources.Lumber, textNormal));
			list2.Add(ResourceToCellCellData("Metal", constructCost.Resources.Metal, textNormal));
			list2.Add(ResourceToCellCellData("Hide", constructCost.Resources.Hide, textNormal));
			list2.Add(ResourceToCellCellData("Morale", (int)buildingTypeDef.GetUpgradeMoraleCost(num), textNormal));
			OutpostResources rebuildCost = buildingTypeDef.GetRebuildCost((uint)num);
			list2.Add(ResourceToCellCellData("Lumber", rebuildCost.Lumber, textNormal));
			list2.Add(ResourceToCellCellData("Metal", rebuildCost.Metal, textNormal));
			list2.Add(ResourceToCellCellData("Hide", rebuildCost.Hide, textNormal));
			if (!buildingTypeDef.CannotBeDamaged)
			{
				list2.Add(new CellData
				{
					TextValue = FHUILocFormatting.Color(buildingTypeDef.GetRepairMaterialCost((uint)num).ToString(), textNormal),
					Icon = UIInfoTools.Instance.GetGenericIcon("AnyMaterial")
				});
			}
		}
		return tableData;
	}

	private static CellData ResourceToCellCellData(string type, int amount, Color color)
	{
		if (amount == 0)
		{
			return CellData.Empty();
		}
		return new CellData
		{
			TextValue = FHUILocFormatting.Color($"<sprite name={type}>{amount}", color)
		};
	}

	public static TableData PopulateBuildingEffectsData(uint buildingID, float rowHeight, float columnWidth, Color textNormal, Color textLevel)
	{
		TableData tableData = new TableData();
		BuildingTypeDef buildingTypeDef = YMLDefRepository.Repo?.DefRepository?.Buildings.Values.FirstOrDefault((BuildingTypeDef itemDef) => itemDef.ID.ID == buildingID);
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(new BuildingTypeID(buildingID));
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Level",
			Width = columnWidth
		});
		for (int num = 0; num < buildingConfig.CodexEffects.Length; num++)
		{
			BuildingConfigUI.CodexTableEntry codexTableEntry = buildingConfig.CodexEffects[num];
			tableData.Columns.Add(new ColumnDefinition
			{
				Id = codexTableEntry.ID,
				Width = columnWidth
			});
		}
		List<CellData> list = new List<CellData>();
		tableData.AddRow(list, rowHeight);
		list.Add(new CellData
		{
			Icon = UIInfoTools.Instance.GetGenericIcon("Level"),
			Tooltip = LocalizationManager.GetTranslation("GUI_LEVEL")
		});
		for (int num2 = 0; num2 < buildingConfig.CodexEffects.Length; num2++)
		{
			BuildingConfigUI.CodexTableEntry codexTableEntry2 = buildingConfig.CodexEffects[num2];
			list.Add(new CellData
			{
				Icon = codexTableEntry2.Icon,
				TextValue = codexTableEntry2.GetHeaderLocalized(),
				Tooltip = codexTableEntry2.GetHeaderTooltipLocalized()
			});
		}
		for (int num3 = 1; num3 <= buildingTypeDef.Levels; num3++)
		{
			List<CellData> list2 = new List<CellData>();
			tableData.AddRow(list2, rowHeight);
			list2.Add(new CellData
			{
				TextValue = FHUILocFormatting.Color(num3.ToString(), textLevel)
			});
			for (int num4 = 0; num4 < buildingConfig.CodexEffects.Length; num4++)
			{
				BuildingConfigUI.CodexTableEntry.LevelData data = buildingConfig.CodexEffects[num4].GetData(num3 - 1);
				BuildingConfigUI.CodexTableEntry.CellItem cellItem = ((data != null) ? data.Effects[0] : null);
				string textStyle = buildingConfig.CodexEffects[num4].TextStyle;
				if (cellItem != null)
				{
					list2.Add(new CellData
					{
						TextValue = cellItem.GetTextLocalized(textStyle),
						Icon = cellItem.Icon,
						Tooltip = cellItem.GetTooltipLocalized(),
						Style = new CellStyle
						{
							MinFontSize = 12,
							MaxFontSize = 20
						}
					});
				}
				else
				{
					list2.Add(CellData.Empty());
				}
			}
		}
		return tableData;
	}

	public static TableData PopulateCharacterHealthData(CharacterClassDef classDef, float rowHeight, float columnWidth, Color textLevel, Color textHealth)
	{
		TableData tableData = new TableData();
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Type",
			Width = columnWidth
		});
		List<CellData> list = new List<CellData>
		{
			new CellData
			{
				Icon = UIInfoTools.Instance.GetGenericIcon("Level"),
				Tooltip = LocalizationManager.GetTranslation("GUI_LEVEL")
			}
		};
		List<CellData> list2 = new List<CellData>
		{
			new CellData
			{
				Icon = UIInfoTools.Instance.GetGenericIcon("Health"),
				Tooltip = LocalizationManager.GetTranslation("Health")
			}
		};
		tableData.AddRow(list, rowHeight);
		tableData.AddRow(list2, rowHeight);
		for (int i = 1; i < classDef.HealthTable.Length; i++)
		{
			tableData.Columns.Add(new ColumnDefinition
			{
				Id = "Level_" + i,
				Width = columnWidth
			});
			CellData item = new CellData
			{
				TextValue = FHUILocFormatting.Color(i.ToString(), textLevel)
			};
			list.Add(item);
			CellData item2 = new CellData
			{
				TextValue = FHUILocFormatting.Color(classDef.HealthTable[i].ToString(), textHealth)
			};
			list2.Add(item2);
		}
		return tableData;
	}

	public static TableData PopulateItemData(ItemDef def, float rowHeight, float columnWidth, Color text)
	{
		TableData tableData = new TableData
		{
			MaxWidth = 310f
		};
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Label",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Item1",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Item2",
			Width = columnWidth
		});
		tableData.Columns.Add(new ColumnDefinition
		{
			Id = "Item3",
			Width = columnWidth
		});
		List<CellData> list = new List<CellData>();
		list.Add(new CellData
		{
			TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_CODEX_ITEMSOURCE_LABEL"), "Codex-Label"),
			Style = new CellStyle
			{
				Alignment = TextAnchor.MiddleLeft,
				MaxFontSize = 20
			}
		});
		list.Add(GetSourceCellData(def));
		list.Add(CellData.Empty());
		list.Add(CellData.Empty());
		tableData.AddRow(list, rowHeight).MergeCells(1, 3);
		List<CellData> list2 = new List<CellData>();
		list2.Add(new CellData
		{
			TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_ITEMSLOT_LABEL"), "Codex-Label"),
			Style = new CellStyle
			{
				Alignment = TextAnchor.MiddleLeft,
				MaxFontSize = 20
			}
		});
		list2.Add(GetSlotTypeData(def));
		list2.Add(CellData.Empty());
		list2.Add(CellData.Empty());
		tableData.AddRow(list2, rowHeight).MergeCells(1, 3);
		if (def.CanBePurchased)
		{
			List<CellData> list3 = new List<CellData>();
			list3.Add(new CellData
			{
				TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST"), "Codex-Label"),
				Style = new CellStyle
				{
					Alignment = TextAnchor.MiddleLeft,
					MaxFontSize = 20
				}
			});
			list3.AddRange(GetItemCostCells(def).Take(3).ToList());
			while (list3.Count < tableData.Columns.Count)
			{
				list3.Add(CellData.Empty());
			}
			tableData.AddRow(list3, rowHeight);
		}
		if (def.CanBeCrafted || def.CanBeBrewed)
		{
			List<CellData> list4 = new List<CellData>();
			list4.Add(new CellData
			{
				TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST"), "Codex-Label"),
				Style = new CellStyle
				{
					Alignment = TextAnchor.MiddleLeft,
					MaxFontSize = 20
				}
			});
			list4.AddRange(GetItemCostCells(def).Take(3));
			while (list4.Count < tableData.Columns.Count)
			{
				list4.Add(CellData.Empty());
			}
			tableData.AddRow(list4, rowHeight);
		}
		if (def.CanBeCrafted && def.RequiredCraftingItemIDs.Count > 0)
		{
			List<CellData> list5 = GetItemRawCostCells(def).ToList();
			int num = (list5.Count + 2) / 3;
			for (int i = 0; i < num; i++)
			{
				List<CellData> list6 = new List<CellData>();
				if (i == 0)
				{
					list6.Add(new CellData
					{
						TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_CODEX_RAWCOST_LABEL"), "Codex-Label"),
						Style = new CellStyle
						{
							Alignment = TextAnchor.MiddleLeft,
							MaxFontSize = 20
						}
					});
				}
				else
				{
					list6.Add(CellData.Empty());
				}
				list6.AddRange(list5.Take(3));
				list5.RemoveRange(0, Mathf.Min(3, list5.Count));
				while (list6.Count < tableData.Columns.Count)
				{
					list6.Add(CellData.Empty());
				}
				tableData.AddRow(list6, rowHeight);
			}
		}
		List<CellData> list7 = new List<CellData>();
		list7.Add(new CellData
		{
			TextValue = FHUILocFormatting.Style(LocalizationManager.GetTranslation("GUI_SELL"), "Codex-Label"),
			Style = new CellStyle
			{
				Alignment = TextAnchor.MiddleLeft,
				MaxFontSize = 20
			}
		});
		list7.Add(GetSellData(def));
		list7.Add(CellData.Empty());
		list7.Add(CellData.Empty());
		tableData.AddRow(list7, rowHeight).MergeCells(1, 3);
		return tableData;
		static CellData GetSellData(ItemDef itemDef)
		{
			if (itemDef == null)
			{
				return CellData.Empty();
			}
			return new CellData
			{
				TextValue = $"<sprite name=Gold> {GetItemSellValue(itemDef)}"
			};
		}
		static CellData GetSlotTypeData(ItemDef itemDef)
		{
			if (itemDef == null)
			{
				return CellData.Empty();
			}
			string textValue = itemDef.Type switch
			{
				ItemEquipType.Head => LocalizationManager.GetTranslation("GUI_ITEM_SLOT_HEAD"), 
				ItemEquipType.Body => LocalizationManager.GetTranslation("GUI_ITEM_SLOT_BODY"), 
				ItemEquipType.Feet => LocalizationManager.GetTranslation("GUI_ITEM_SLOT_LEGS"), 
				ItemEquipType.OneHandWeapon => LocalizationManager.GetTranslation("GUI_ITEM_SLOT_ONEHAND"), 
				ItemEquipType.TwoHandWeapon => LocalizationManager.GetTranslation("GUI_INVENTORY_SLOT_TwoHandItem"), 
				ItemEquipType.Small => LocalizationManager.GetTranslation("GUI_ITEM_SLOT_SMALLITEM"), 
				_ => "-", 
			};
			return new CellData
			{
				Icon = ItemDTOUtilities.GetItemTypeImage(itemDef.Type),
				TextValue = textValue,
				Style = new CellStyle
				{
					Alignment = TextAnchor.MiddleLeft
				}
			};
		}
		static CellData GetSourceCellData(ItemDef itemDef)
		{
			if (itemDef == null)
			{
				return CellData.Empty();
			}
			if (itemDef.CanBeCrafted)
			{
				return new CellData
				{
					Icon = UIInfoTools.Instance.GetGenericIcon("FH_ICO_Craft"),
					TextValue = LocalizationManager.GetTranslation("GUI_CRAFTSMAN_NAME"),
					Style = new CellStyle
					{
						Alignment = TextAnchor.MiddleLeft
					}
				};
			}
			if (itemDef.CanBeBrewed)
			{
				return new CellData
				{
					Icon = UIInfoTools.Instance.GetGenericIcon("FH_ICONS_Alchemist"),
					TextValue = LocalizationManager.GetTranslation("BUILDING_Name_35"),
					Style = new CellStyle
					{
						Alignment = TextAnchor.MiddleLeft
					}
				};
			}
			if (itemDef.CanBePurchased)
			{
				return new CellData
				{
					Icon = UIInfoTools.Instance.GetGenericIcon("FH_ICO_Merc"),
					TextValue = LocalizationManager.GetTranslation("GUI_Merchant"),
					Style = new CellStyle
					{
						Alignment = TextAnchor.MiddleLeft
					}
				};
			}
			return CellData.Empty();
		}
	}

	private static IEnumerable<CellData> GetItemCostCells(ItemDef def)
	{
		if (def.GoldCost != 0)
		{
			yield return new CellData
			{
				TextValue = $"<sprite name=Gold> {def.GoldCost}"
			};
		}
		if (!def.CraftingCost.IsEmpty)
		{
			foreach (OutpostMaterialType item in OutpostResources.AllMaterialTypes())
			{
				if (def.CraftingCost.GetMaterialAmount(item) > 0)
				{
					yield return new CellData
					{
						TextValue = $"<sprite name={item.ToString()}> {def.CraftingCost.GetMaterialAmount(item)}"
					};
				}
			}
			foreach (OutpostHerbType item2 in OutpostResources.AllHerbTypes())
			{
				if (def.CraftingCost.GetHerbAmount(item2) > 0)
				{
					yield return new CellData
					{
						TextValue = $"<sprite name={item2.ToString()}> {def.CraftingCost.GetHerbAmount(item2)}"
					};
				}
			}
		}
		if (def.RequiredCraftingItemIDs.Count <= 0)
		{
			yield break;
		}
		foreach (ItemID requiredCraftingItemID in def.RequiredCraftingItemIDs)
		{
			yield return GetItemIconCells(requiredCraftingItemID);
		}
	}

	private static IEnumerable<CellData> GetItemRawCostCells(ItemDef def)
	{
		uint itemCraftingGoldCost = GetItemCraftingGoldCost(def);
		if (itemCraftingGoldCost != 0)
		{
			yield return new CellData
			{
				TextValue = $"<sprite name=Gold> {itemCraftingGoldCost}"
			};
		}
		OutpostResources resourceCost = GetItemCraftingResourceCost(def);
		if (resourceCost.Total == 0)
		{
			yield break;
		}
		foreach (OutpostMaterialType item in OutpostResources.AllMaterialTypes())
		{
			if (resourceCost.GetMaterialAmount(item) > 0)
			{
				yield return new CellData
				{
					TextValue = $"<sprite name={item.ToString()}> {resourceCost.GetMaterialAmount(item)}"
				};
			}
		}
		foreach (OutpostHerbType item2 in OutpostResources.AllHerbTypes())
		{
			if (resourceCost.GetHerbAmount(item2) > 0)
			{
				yield return new CellData
				{
					TextValue = $"<sprite name={item2.ToString()}> {resourceCost.GetHerbAmount(item2)}"
				};
			}
		}
	}

	private static CellData GetItemIconCells(ItemID id)
	{
		ItemDef def = YMLDefRepository.Repo?.DefRepository?.Items.Values.FirstOrDefault((ItemDef itemDef) => itemDef.ID == id);
		if (def == null)
		{
			return null;
		}
		return new CellData
		{
			Icon = UIInfoTools.Instance.GetItemConfig(def.Icon)?.miniIcon,
			Tooltip = LocalizationManager.GetTranslation(def.NameLocKey),
			OnClickAction = () =>
			{
				Singleton<FHUICodex>.Instance.Show(def.Name, fromOptionMenu: false, preserveOffset: true);
			}
		};
	}

	private static int GetItemSellValue(ItemDef item)
	{
		if (item.CanBePurchased)
		{
			return (int)item.GoldCost / 2;
		}
		if (item.CanBeCrafted)
		{
			return GetItemCraftingResourceCost(item).Total * (int)Settings.SellItemResourceValue;
		}
		if (item.CanBeBrewed)
		{
			return item.Brewing.HerbResources.Total * (int)Settings.SellItemResourceValue;
		}
		return 0;
	}

	private static OutpostResources GetItemCraftingResourceCost(ItemDef item)
	{
		OutpostResources craftingCost = item.CraftingCost;
		foreach (ItemDef item2 in GetMissingRequiredItemsToCraft(item))
		{
			craftingCost += GetItemCraftingResourceCost(item2);
			if (item2.Brewing != null)
			{
				craftingCost += item2.Brewing.HerbResources;
			}
		}
		return craftingCost;
	}

	private static uint GetItemCraftingGoldCost(ItemDef item)
	{
		uint num = item.GoldCost;
		foreach (ItemDef item2 in GetMissingRequiredItemsToCraft(item))
		{
			num += GetItemCraftingGoldCost(item2);
		}
		return num;
	}

	private static IEnumerable<ItemDef> GetMissingRequiredItemsToCraft(ItemDef item)
	{
		List<ItemDef> list = GetRequiredItemsToCraft(item).ToList();
		foreach (ItemDef item2 in list)
		{
			yield return item2;
		}
	}

	private static IEnumerable<ItemDef> GetRequiredItemsToCraft(ItemDef item)
	{
		return item.GetRequiredItemsForCrafting(YMLDefRepository.Repo?.DefRepository);
	}

	private static ItemDataDTO GetItem(int scenarioItemID)
	{
		return new ItemDataDTO(ScenarioRuleClient.SRLYML.ItemCards.First((ItemCardYMLData i) => i.ID == scenarioItemID));
	}

	public static FHItemTooltip.ItemTooltipParams GetItemTooltipParams(int scenarioItemID)
	{
		ItemDataDTO item = GetItem(scenarioItemID);
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = (item.FlipScenarioItemID.HasValue ? GetItem(item.FlipScenarioItemID.Value) : null),
			Data = default
		};
	}

	public static string CodexActorIDToUnlockTag(string actorID)
	{
		string text = "CODEX_Unlock_" + actorID;
		if (text.EndsWith("EliteID"))
		{
			text = text.ReplaceLastOccurrence("EliteID", "");
		}
		if (text.EndsWith("ID"))
		{
			text = text.ReplaceLastOccurrence("ID", "");
		}
		return text;
	}

	public static string CodexHeroIDToUnlockTag(CharacterClassID id)
	{
		return "CODEX_Unlock_" + id.ID;
	}

	public static string CodexBuildingIDToUnlockTag(BuildingTypeID id)
	{
		return $"CODEX_Unlock_Building_{id.ID}";
	}

	public static string CodexItemIDToUnlockTag(ItemID id)
	{
		return $"CODEX_Unlock_Item_{id.ID}";
	}
}
