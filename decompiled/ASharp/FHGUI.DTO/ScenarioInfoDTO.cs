using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.DTO;

public class ScenarioInfoDTO
{
	public int Id;

	private Sprite m_Icon;

	public Color Color;

	public int Level;

	public string NameKey;

	public string DescriptionKey;

	public string ActiveEffectsLiteral;

	public string ObjectivesLiteral;

	public string LocationNameKey;

	public uint Difficulty;

	public bool IsCompleted;

	public List<SpriteWithTooltipDTO> RewardDtos;

	public List<SpriteWithTooltipDTO> AdditinalRewardDtos;

	public bool IsPlayable;

	public bool IsMandatory;

	public bool HasBoss;

	public bool HasAvailableChests;

	public bool HasAvailableRandomItems;

	public string IllustrationId;

	public string RegionId;

	public ScenarioChainType QuestChain;

	private IEnumerable<ScenarioDef.Monster> m_Monsters;

	private List<MonsterDataDto> m_MonsterDtos;

	public Sprite Icon
	{
		get
		{
			if (m_Icon != null)
			{
				return m_Icon;
			}
			m_Icon = UIInfoTools.Instance.GetQuestChainIcon(QuestChain);
			return m_Icon;
		}
		set
		{
			m_Icon = value;
		}
	}

	public Sprite Illustration => UIInfoTools.Instance.GetNarrativeIllustration(IllustrationId, throwError: false);

	public Sprite RegionIcon => UIInfoTools.Instance.GetLocationIcon(RegionId);

	public string NameLiteral => LocalizationManager.GetTranslation(NameKey);

	public string DescriptionLiteral => LocalizationManager.GetTranslation(DescriptionKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: false, useDefaultIfMissing: false, returnNullIfNotFound: true);

	public List<MonsterDataDto> MonsterDtos => GetOrBuildMonsterDTOs();

	public ScenarioInfoDTO()
	{
		m_MonsterDtos = new List<MonsterDataDto>();
		RewardDtos = new List<SpriteWithTooltipDTO>();
		AdditinalRewardDtos = new List<SpriteWithTooltipDTO>();
	}

	public ScenarioInfoDTO(ScenarioDef scenarioDef, int partySize, int level, bool isCompleted, bool isPlayable, bool isMandatory, ScenarioDef.LevelData levelData, IEnumerable<ScenarioDef.Monster> monsters = null)
		: this()
	{
		Id = (int)scenarioDef.ID.ID;
		QuestChain = scenarioDef.Chain;
		IllustrationId = scenarioDef.LevelID + "_Intro_1";
		RegionId = scenarioDef.Location.ToString();
		LocationNameKey = "GUI_Biome_" + scenarioDef.Location;
		Color = UIInfoTools.Instance.GetQuestChainColor(scenarioDef.Chain);
		Level = level;
		IsCompleted = isCompleted;
		NameKey = $"FS_{scenarioDef.ID}_NAME";
		DescriptionKey = $"FS_{scenarioDef.ID.ID:D3}_ShortDesc";
		IsPlayable = isPlayable;
		IsMandatory = isMandatory;
		Difficulty = scenarioDef.Difficulty;
		m_Monsters = monsters;
		if (!(levelData != null))
		{
			return;
		}
		if (m_Monsters == null)
		{
			m_Monsters = levelData.PartySettings.FirstOrDefault((ScenarioDef.PartySetting setting) => setting.PartyMembers == partySize)?.Monsters.OrderBy((ScenarioDef.Monster monster) => YMLDefRepository.Repo.DefRepository.Monsters[monster.ID].Name) ?? null;
		}
		if (levelData.Loot.Gold != 0)
		{
			SpriteWithTooltipDTO item = new SpriteWithTooltipDTO
			{
				Sprite = UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized,
				Title = LocalizationManager.GetTranslation("Gold"),
				TextValue = levelData.Loot.Gold.ToString()
			};
			RewardDtos.Add(item);
		}
		foreach (OutpostMaterialType item4 in OutpostResources.AllMaterialTypes())
		{
			int materialAmount = levelData.Loot.Resources.GetMaterialAmount(item4);
			if (materialAmount > 0)
			{
				SpriteWithTooltipDTO item2 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(item4.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetMaterialLocalizationKey(item4)),
					TextValue = materialAmount.ToString()
				};
				RewardDtos.Add(item2);
			}
		}
		foreach (OutpostHerbType allHerb in OutpostResources.GetAllHerbs())
		{
			int herbAmount = levelData.Loot.Resources.GetHerbAmount(allHerb);
			if (herbAmount > 0)
			{
				SpriteWithTooltipDTO item3 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(allHerb.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(allHerb)),
					TextValue = herbAmount.ToString()
				};
				RewardDtos.Add(item3);
			}
		}
		if (m_Monsters == null)
		{
			return;
		}
		foreach (ScenarioDef.Monster monster in m_Monsters)
		{
			if ((monster.IsBoss || monster.IsMiniBoss) && !monster.IsAlly)
			{
				HasBoss = true;
				break;
			}
		}
	}

	public ScenarioInfoDTO(CampaignController controller, ScenarioState scenarioState, int partySize, int level, bool isPlayable)
		: this(scenarioState.Def, partySize, level, scenarioState.Completed, isPlayable, scenarioState.Mandatory, controller.ScenarioController.GetLevelDataToShow(scenarioState), controller.ScenarioController.GetMonstersToShow(scenarioState, partySize))
	{
		ScenarioDef.LevelData levelDataToShow = controller.ScenarioController.GetLevelDataToShow(scenarioState);
		if (levelDataToShow != null)
		{
			if (!scenarioState.RandomItemLooted && levelDataToShow.Loot.RandomItemCount != 0 && controller.ValidRandomItemDeckIDs.Any())
			{
				HasAvailableRandomItems = true;
				SpriteWithTooltipDTO item = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetGenericIcon("RandomItem"),
					Title = LocalizationManager.GetTranslation("RANDOM_ITEM_TOOLTIP"),
					TextValue = string.Empty
				};
				RewardDtos.Add(item);
			}
			int num = levelDataToShow.Chests.SelectMany((ScenarioDef.Chest chest) => chest.TreasureTableIDs).Except(controller.State.LootedChestTreasureTableIDs).Count();
			if (num > 0)
			{
				HasAvailableChests = true;
				SpriteWithTooltipDTO item2 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetGenericIcon("TreasureChest"),
					Title = LocalizationManager.GetTranslation("TREASURE_CHEST_TOOLTIP"),
					TextValue = num.ToString()
				};
				RewardDtos.Add(item2);
			}
		}
		if (controller?.State?.ActiveScenario == null)
		{
			return;
		}
		foreach (string item5 in from x in controller.State.ActiveScenario.FavorsToUse
			where x.type == ScenarioFavorType.Discovery
			select x.param1)
		{
			if (Enum.TryParse<OutpostMaterialType>(item5, out var result))
			{
				SpriteWithTooltipDTO item3 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(result.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetMaterialLocalizationKey(result))
				};
				AdditinalRewardDtos.Add(item3);
			}
			if (Enum.TryParse<OutpostHerbType>(item5, out var result2))
			{
				SpriteWithTooltipDTO item4 = new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetResourceConfig(result2.ToString()).IconStylized,
					Title = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(result2))
				};
				AdditinalRewardDtos.Add(item4);
			}
		}
	}

	private List<MonsterDataDto> GetOrBuildMonsterDTOs()
	{
		if (m_Monsters == null)
		{
			return m_MonsterDtos;
		}
		if (m_MonsterDtos != null && m_MonsterDtos.Count > 0)
		{
			return m_MonsterDtos;
		}
		if (m_MonsterDtos == null)
		{
			m_MonsterDtos = new List<MonsterDataDto>();
		}
		foreach (ScenarioDef.Monster monster in m_Monsters.Distinct())
		{
			if (!m_MonsterDtos.Any((MonsterDataDto x) => x.Id.Equals(monster.ID.ID, StringComparison.InvariantCultureIgnoreCase)) && ((!monster.IsBoss && !monster.IsMiniBoss) || monster.IsAlly || IsCompleted))
			{
				MonsterDataDto item = MonsterDataDto.GenerateMonsterDataDto(monster);
				m_MonsterDtos.Add(item);
			}
		}
		m_MonsterDtos = m_MonsterDtos.OrderBy((MonsterDataDto monsterDataDto) => monsterDataDto.Id).ThenBy((MonsterDataDto x) => x.IsAlly ? 1 : (-1)).ThenBy((MonsterDataDto x) => (!x.IsBoss) ? 1 : (-1))
			.ThenBy((MonsterDataDto x) => (!x.IsMiniBoss) ? 1 : (-1))
			.ToList();
		return m_MonsterDtos;
	}
}
