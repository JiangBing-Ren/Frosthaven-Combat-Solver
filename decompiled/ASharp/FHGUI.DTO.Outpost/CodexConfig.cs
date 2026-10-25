using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

[CreateAssetMenu(fileName = "CodexDataSO", menuName = "Defs/Codex")]
public class CodexConfig : ScriptableObject
{
	[Serializable]
	public class CodexEntry
	{
		public string ID;

		public GameObject EntryContent;

		public string Category;

		[Tooltip("Entry name and Primary search keyword. (Mandatory)")]
		public string LocalizationKey;

		[Tooltip("Localization keys for additional search keywords (Optional)")]
		public List<string> keywords;

		[Tooltip("Unlock tag id (Optional)")]
		public string unlockTag;

		public override string ToString()
		{
			return "[CodexEntry, " + ID + ", " + Category + " ]";
		}
	}

	[Serializable]
	public class CodexCategory
	{
		public string ID;

		public string LocalizationKey;

		public Sprite Icon;
	}

	public CodexCategory[] Categories;

	public CodexEntry[] Entries;

	public GameObject SimpleEntryTemplate;

	public Sprite DefaultIllustration;

	private List<CodexEntry> m_AllEntries;

	public GameObject MonsterContentPrototype;

	public GameObject BossContentPrototype;

	public GameObject BuildingContentPrototype;

	public GameObject ItemContentPrototype;

	private static readonly string[] m_MonsterEntryIdList = new string[48]
	{
		"AbaelHerderID", "AbaelScoutID", "AlgoxArcherID", "AlgoxGuardID", "AlgoxIcespeakerID", "AlgoxPriestID", "AlgoxScoutID", "AlgoxSnowspeakerID", "AncientArtilleryID", "BlackImpID",
		"BurrowingBladeID", "ChaosDemonID", "CityGuardID", "DeepTerrorID", "EarthDemonID", "FlameDemonID", "FlamingBladespinnerID", "ForestImpID", "FrostDemonID", "FrozenCorpseID",
		"HarrowerInfesterID", "HoundID", "IceWraithID", "LightningEelID", "LivingBonesID", "LivingDoomID", "LivingSpiritID", "LurkerClawcrusherID", "LurkerMindsnipperID", "LurkerSoldierID",
		"LurkerWavethrowerID", "NightDemonID", "OozeID", "PiranhaPigID", "PolarBearID", "RendingDrakeID", "RoboticBoltshooterID", "RuinedMachineID", "SavvasIcestormID", "SavvasLavaflowID",
		"ShrikeFiendID", "SnowImpID", "SpittingDrakeID", "SteelAutomatonID", "SunDemonID", "VermlingPriestID", "VermlingScoutID", "WindDemonID"
	};

	private static readonly string[] m_BossEntryIdList = new string[34]
	{
		"AlgoxFrozenFistID", "AlgoxSnowDancerID", "AlgoxStormcallerID", "AugmentedMindthiefID", "BelaraID", "BlacksmithID", "ChaosLieutenantID", "CityGuardSwordOfJusticeID", "ElderOozeID", "FishKingID",
		"FishKingLeviathanID", "FractureOfTheDeepID", "GiantPiranhaPigID", "HarbingerOfShadowID", "HatefulCorpseID", "LadyInWhiteID", "LordOfChaosID", "PowerCoreID", "PrinceOfFrostID", "ProgramDirectorID",
		"ReluctantGhostAID", "RenderID", "RimeheartID", "SeekerOfTheAbyssID", "TheCollectorID", "TheCoralCorpseID", "TheMawID", "TheRelicID", "TheOrphanID", "TheShimmerID",
		"TheTormentorID", "VanjalID", "VermlingWarchiefID", "VestigeOfTheImprisonedGodID"
	};

	public List<CodexEntry> AllEntries()
	{
		if (m_AllEntries != null && m_AllEntries.Count > 0)
		{
			return m_AllEntries;
		}
		m_AllEntries = new List<CodexEntry>();
		m_AllEntries.AddRange(Entries);
		if (ReleaseStage.IsFullRelease())
		{
			m_AllEntries.AddRange(from entry in Monsters()
				orderby LocalizationManager.GetTranslation(entry.LocalizationKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true)
				select entry);
			m_AllEntries.AddRange(from entry in Bosses()
				orderby LocalizationManager.GetTranslation(entry.LocalizationKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true)
				select entry);
			m_AllEntries.AddRange(from entry in Buildings()
				orderby LocalizationManager.GetTranslation(entry.LocalizationKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true)
				select entry);
			m_AllEntries.AddRange(from entry in Items()
				orderby LocalizationManager.GetTranslation(entry.LocalizationKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true)
				select entry);
		}
		return m_AllEntries;
	}

	public IEnumerable<CodexEntry> Monsters()
	{
		Dictionary<MonsterID, MonsterDef>.ValueCollection valueCollection = YMLDefRepository.Repo?.DefRepository?.Monsters.Values;
		foreach (MonsterDef item in valueCollection)
		{
			if (m_MonsterEntryIdList.Contains(item.ID.ID) && !item.ID.ID.Contains("EliteID"))
			{
				string text = item.ID.ID.ReplaceLastOccurrence("ID", "");
				CMonsterClass cMonsterClass = MonsterClassManager.Find(item.ID.ID);
				if (cMonsterClass != null && MonsterClassManager.Find(text + "EliteID") != null && !cMonsterClass.Boss && LocalizationManager.TryGetTranslation(text, out var _))
				{
					yield return new CodexEntry
					{
						ID = text,
						EntryContent = MonsterContentPrototype,
						Category = "MONSTERS",
						LocalizationKey = text,
						unlockTag = FHUICodexUtils.CodexActorIDToUnlockTag(text)
					};
				}
			}
		}
	}

	public IEnumerable<CodexEntry> Bosses()
	{
		Dictionary<MonsterID, MonsterDef>.ValueCollection valueCollection = YMLDefRepository.Repo?.DefRepository?.Monsters.Values;
		foreach (MonsterDef item in valueCollection)
		{
			if (m_BossEntryIdList.Contains(item.ID.ID))
			{
				string text = item.ID.ID.ReplaceLastOccurrence("ID", "");
				CMonsterClass cMonsterClass = MonsterClassManager.Find(item.ID.ID);
				if (cMonsterClass != null && LocalizationManager.TryGetTranslation(item.Name, out var _) && cMonsterClass.AbilityCardsPool.Any())
				{
					yield return new CodexEntry
					{
						ID = text,
						EntryContent = BossContentPrototype,
						Category = "BOSSES",
						LocalizationKey = item.Name,
						unlockTag = FHUICodexUtils.CodexActorIDToUnlockTag(text)
					};
				}
			}
		}
	}

	public IEnumerable<CodexEntry> Buildings()
	{
		Dictionary<BuildingTypeID, BuildingTypeDef>.KeyCollection keyCollection = YMLDefRepository.Repo?.DefRepository?.Buildings.Keys;
		if (keyCollection == null)
		{
			yield break;
		}
		foreach (BuildingTypeID item in keyCollection)
		{
			BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(item);
			if (!buildingConfig.HiddenInCodex)
			{
				yield return new CodexEntry
				{
					ID = item.ToString(),
					EntryContent = BuildingContentPrototype,
					Category = "BUILDINGS",
					LocalizationKey = buildingConfig.GetCodexName(),
					unlockTag = FHUICodexUtils.CodexBuildingIDToUnlockTag(item)
				};
			}
		}
	}

	public IEnumerable<CodexEntry> Items()
	{
		Dictionary<ItemID, ItemDef>.ValueCollection valueCollection = YMLDefRepository.Repo?.DefRepository?.Items.Values;
		if (valueCollection == null)
		{
			yield break;
		}
		foreach (ItemDef item in valueCollection)
		{
			yield return new CodexEntry
			{
				ID = item.Icon,
				EntryContent = ItemContentPrototype,
				Category = "ITEMS",
				LocalizationKey = item.NameLocKey,
				unlockTag = FHUICodexUtils.CodexItemIDToUnlockTag(item.ID)
			};
		}
	}
}
