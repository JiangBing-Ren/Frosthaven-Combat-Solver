using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class ScenarioPossibleRoom : ISerializable
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EBiome
	{
		Inherit,
		Default,
		Crypt,
		Dungeon,
		Cave,
		Forest,
		City
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ESubBiome
	{
		Inherit,
		Default,
		Necropolis,
		Ruined,
		[Obsolete]
		Catacombs,
		Sewers,
		[Obsolete]
		Volcanic,
		[Obsolete]
		AncientCaverns,
		[Obsolete]
		DaggerForest,
		CorpseWood,
		Marsh,
		StoneRooms,
		[Obsolete]
		WoodenRooms,
		CityWalls,
		StoneWoodenRooms,
		[Obsolete]
		WoodenStoneRooms,
		Shack,
		Rot,
		SubBiome_01,
		SubBiome_02,
		SubBiome_03,
		SubBiome_04,
		SubBiome_05,
		DLC_SB_Ship,
		DLC_SB_Tunnels,
		DLC_SB_Platform,
		DLC_SB_RuinedSewer,
		DLC_SB_Void,
		DLC_SB_Arena,
		DLC_SB_Custom_01,
		DLC_SB_Custom_02,
		FH_MetallicRuins,
		FH_GenericRuins,
		FH_MetallicVents,
		FH_Dungeon,
		FH_Mausoleum,
		FH_Dwelling,
		FH_CivilCastle,
		FH_CoralCastle,
		FH_Plains,
		FH_Coastal,
		FH_WhiteFireWoods,
		FH_RadiantForest,
		FH_Mountains,
		FH_Cave,
		FH_Town,
		FH_ShipDeck,
		FH_ShipCabins,
		FH_ShipHull,
		FH_Vault,
		FH_TownInFlames,
		FH_MountainsLow,
		FH_Town_Low,
		FH_Town_High,
		FH_TownDocks,
		FH_CoralCastleSpecialDoors,
		FH_MetallicRuins_DoubleDoors,
		FH_Terraced_RadiantForest,
		FH_Cave_DoubleDoors,
		FH_Mountains_Skeleton,
		FH_Vault_SpecialDoors,
		FH_RadiantForest_SpecialDoors,
		FH_MetallicRuins_Spire,
		FH_Vault_ColoredDoors,
		FH_Dungeon_Elemental,
		FH_Coastal_SpecialDoors,
		FH_MetallicRuins_TheBigElevator,
		FH_ShipDeck_CustomFake,
		FH_WhiteFireWoods_EmptyWalls,
		FH_Dwelling_DoubleDoors,
		FH_CoralCastle_SpecialSpawners,
		FH_Dungeon_IntoTheBlack,
		FH_MetallicRuins_Prison,
		FH_Mountains_Iceberg,
		FH_CoastalBeach_Scenario091,
		FH_Cave_Scenario085,
		FH_Mountains_Features,
		FH_WhiteFireWoods_Crystal,
		FH_Vault_DoubleDoors,
		FH_Coastal_SpecialBays,
		FH_CoralCastle_Scenario076,
		FH_Mountains_Scenario006,
		FH_MetallicRuins_Scenario134,
		FH_MetallicRuins_3D,
		FH_Mountains_3D,
		FH_MetallicRuins_Scenario15,
		FH_Plains_Scenario130
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ETheme
	{
		Inherit,
		Default,
		Torture,
		Library,
		Treasure,
		Kitchen,
		AlchemyLab,
		Chapel,
		GeneralStore,
		KitchenStore,
		TombStore,
		TortureStore,
		SculptorStudio,
		BurialChamber,
		Mausoleum,
		Shrine,
		Temple,
		AncientLibrary,
		Study,
		[Obsolete]
		Well,
		[Obsolete]
		DruidTemple,
		[Obsolete]
		GuardRoom,
		[Obsolete]
		Prison,
		[Obsolete]
		CharnalHouse,
		[Obsolete]
		CessPool,
		[Obsolete]
		Cistern,
		Armoury,
		AncientTemple,
		DeepForestGlade,
		DruidsGrove,
		InoxBurialGround,
		InoxCamp,
		MagicGrotto,
		MushroomLand,
		StillWaters,
		WoodcuttersCottage,
		VermlingVillage,
		[Obsolete]
		FungalWasteland,
		[Obsolete]
		StagnantPits,
		[Obsolete]
		ForestGraveyard,
		[Obsolete]
		PlaguePits,
		[Obsolete]
		EvilDead,
		Volcanic,
		Ice,
		RockTemple,
		OldMine,
		[Obsolete]
		Storeroom,
		[Obsolete]
		BanquetHall,
		[Obsolete]
		Tavern,
		[Obsolete]
		Church,
		[Obsolete]
		DruidStore,
		[Obsolete]
		Bedroom,
		[Obsolete]
		Pantry,
		[Obsolete]
		Apothecary,
		[Obsolete]
		Hallway,
		[Obsolete]
		Gallery,
		[Obsolete]
		Bordello,
		[Obsolete]
		PsychoHut,
		[Obsolete]
		HermitHut,
		[Obsolete]
		HuntersCabin,
		[Obsolete]
		TownStreet,
		[Obsolete]
		TownWalls,
		Miscellaneous,
		Theme_01,
		Theme_02,
		Theme_03,
		Theme_04,
		Theme_05,
		DLC_TH_Vermling,
		DLC_TH_BlackSludge,
		DLC_TH_Abbatoir,
		DLC_TH_Gore,
		DLC_TH_Lab,
		DLC_TH_AbWarehouse,
		DLC_TH_TownGate,
		DLC_TH_Warehouse,
		DLC_TH_Custom_01,
		DLC_TH_Custom_02,
		DLC_TH_Custom_03,
		DLC_TH_Custom_04,
		FH_T_MR_Forge,
		FH_T_Library,
		FH_T_MR_Maintenance,
		FH_T_MR_ScrapPit,
		FH_T_Computers,
		FH_T_Prison,
		FH_T_RedCrystals,
		FH_T_Kitchen,
		FH_T_FieldRuins,
		FH_T_CrystalForest,
		FH_T_Shipwreck,
		FH_T_AlgoxSettlement,
		FH_T_AlgoxShrines,
		FH_T_IceCave,
		FH_T_EvilCave,
		FH_T_Underwater,
		FH_T_ReligiousHumans,
		FH_T_MR_Library,
		FH_T_Scenario23_Props,
		FH_T_Scenario02_Props,
		FH_T_Scenario30_Props,
		FH_T_Scenario19_Props,
		FH_T_Scenario14_Props,
		FH_T_Scenario77_Props,
		FH_T_Scenario10_18_Props,
		FH_T_Scenario45_Props,
		FH_T_Scenario05_Props,
		FH_T_Scenario50_Props,
		FH_T_Scenario09_Props,
		FH_T_Scenario22_Props,
		FH_T_Scenario65_66_67_68_Props,
		FH_T_Scenario54_Underwater_Props,
		FH_T_Scenario49_Props,
		FH_T_Scenario60_Props,
		FH_T_Scenario18_Props,
		FH_T_Scenario56_Props,
		FH_T_Scenario21_Props,
		FH_T_Scenario75_Props,
		FH_T_Scenario74_Props,
		FH_T_Scenario62_Props,
		FH_T_Scenario27_Props,
		FH_T_Scenario123_Props,
		FH_T_Scenario89_Props,
		FH_T_Scenario69_Props,
		FH_T_MR_Prison,
		FH_T_Scenario94_Props,
		FH_T_Scenario115_Props,
		FH_T_Scenario111_Props,
		FH_T_Scenario104_Warm,
		FH_T_Scenario104_Cold,
		FH_T_Scenario103_Props,
		FH_T_Scenario106_Props,
		FH_T_Scenario80_Props,
		FH_T_Scenario63_Props,
		FH_T_ShipStern_Bay,
		FH_T_ScenarioIntroAdventure04,
		FH_T_ScenarioIntroAdventure05,
		FH_T_ScenarioIntroAdventure07,
		FH_T_Scenario82_Props,
		FH_T_Scenario96_Props,
		FH_T_Scenario135_Props,
		FH_T_Scenario38_Props,
		FH_T_Scenario74_B_Props,
		FH_T_Scenario72_Props
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ESubTheme
	{
		Inherit,
		Default,
		Bandit,
		Undead,
		Demon,
		Cultist,
		Animal,
		FireDemon,
		IceDemon,
		TownMilitia,
		DLC_SubTH_BloodCult,
		DLC_SubTH_Custom_01,
		DLC_SubTH_Custom_02,
		FH_ST_Shrike,
		FH_ST_Harrower,
		FH_ST_Vermling,
		FH_ST_Automatons,
		FH_ST_Abael,
		FH_ST_Undead,
		FH_ST_Imperial,
		FH_ST_Lurker,
		FH_ST_FireDemon,
		FH_ST_IceDemon,
		FH_ST_Demon,
		FH_ST_Algox
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ETone
	{
		Inherit,
		Default,
		Dark,
		Natural,
		Spectral,
		Demonic,
		Toxic,
		Evil,
		Crystal,
		ForestDefault,
		ForestEerie,
		ForestMoonlight,
		ForestFairy,
		Bioluminescence,
		LavaLamp,
		Gaslight,
		Candlelight,
		Campaign_01,
		Campaign_02,
		Campaign_03,
		Campaign_04,
		Campaign_05,
		DLC_Tone_Custom_01,
		DLC_Tone_Custom_02,
		DLC_Tone_Custom_03,
		FH_Tone_MetallicVent,
		FH_Tone_MetallicRuinsDungeon,
		FH_Tone_Underwater,
		FH_Tone_IceCave,
		FH_Tone_CavesNatural,
		FH_Tone_Abandoned,
		FH_Tone_Castle,
		FH_Tone_CoralCastle,
		FH_Tone_Dwelling,
		FH_Tone_Dungeon,
		FH_Tone_ForestEerie,
		FH_Tone_ForestRadiant,
		FH_Tone_MountainSeasons,
		FH_Tone_PlainsSeasons,
		FH_Tone_Powered,
		FH_Tone_Town,
		FH_Tone_Vault,
		FH_Tone_MountainEternalWinter,
		FH_Tone_PlainsEternalWinter,
		FH_Tone_ForestSeasons,
		FH_Tone_ForestEternalSummer,
		FH_Tone_ForestEternalWinter,
		FH_Tone_CavesEvil,
		FH_Tone_Dark,
		FH_Tone_Demonic,
		FH_Tone_Earth,
		FH_Tone_Fire,
		FH_Tone_Iceberg,
		FH_Tone_Light,
		FH_Tone_Spectral,
		FH_Tone_Toxic,
		FH_Tone_Void,
		FH_Tone_Wind,
		FH_Tone_ShipExterior,
		FH_Tone_TownInFlames,
		FH_Tone_ShipInterior,
		FH_Tone_PolarWinter,
		FH_Tone_Abandoned_CUSTOM_FH16,
		FH_Tone_Abandoned_CUSTOM_FH19,
		FH_Tone_Coastal_Seasons,
		FH_Tone_VoidSwarm,
		FH_Tone_Scenario132,
		FH_Tone_Scenario066,
		FH_Tone_CUSTOM_RFS007,
		FH_Tone_CUSTOM_FH64
	}

	[KronosEnumPolice(0)]
	public enum EBiomeState
	{
		Inherit,
		Default,
		Natural,
		InteriorAbandoned,
		InteriorPower,
		Fire,
		Destroyed,
		Underwater,
		Iceberg,
		Light,
		Darkness,
		Wind,
		Earth,
		Corrupted,
		EternalSummer,
		EternalWinter
	}

	public static readonly EBiome[] Biomes = (EBiome[])Enum.GetValues(typeof(EBiome));

	public static readonly ESubBiome[] SubBiomes = (ESubBiome[])Enum.GetValues(typeof(ESubBiome));

	public static readonly ETheme[] Themes = (ETheme[])Enum.GetValues(typeof(ETheme));

	public static readonly ESubTheme[] SubThemes = (ESubTheme[])Enum.GetValues(typeof(ESubTheme));

	public static readonly ETone[] Tones = (ETone[])Enum.GetValues(typeof(ETone));

	public string Name { get; private set; }

	public string MonsterGroup { get; private set; }

	public EBiome Biome { get; private set; }

	public ESubBiome SubBiome { get; private set; }

	public ETheme Theme { get; private set; }

	public ESubTheme SubTheme { get; private set; }

	public ETone Tone { get; private set; }

	public EBiomeState BiomeState { get; private set; }

	public List<int> OneHexObstacles { get; private set; }

	public List<int> TwoHexObstacles { get; private set; }

	public List<int> ThreeHexObstacles { get; private set; }

	public List<int> Traps { get; private set; }

	public List<int> PressurePlates { get; private set; }

	public List<int> TerrainHotCoals { get; private set; }

	public List<int> TerrainWater { get; private set; }

	public List<int> TerrainThorns { get; private set; }

	public List<int> TerrainRubble { get; private set; }

	public List<int> GoldPiles { get; private set; }

	public List<int> TreasureChestChance { get; private set; }

	public List<string> ChestTreasureTables { get; private set; }

	public List<List<string>> GoalChests { get; private set; }

	public List<SpawnerData> SpawnerDatas { get; private set; }

	public List<Tuple<string, List<int>>> AllyMonsters { get; private set; }

	public List<Tuple<string, List<int>>> Enemy2Monsters { get; private set; }

	public List<Tuple<string, List<int>>> Objects { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("MonsterGroup", MonsterGroup);
		info.AddValue("Biome", Biome);
		info.AddValue("SubBiome", SubBiome);
		info.AddValue("Theme", Theme);
		info.AddValue("SubTheme", SubTheme);
		info.AddValue("Tone", Tone);
		info.AddValue("OneHexObstacles", OneHexObstacles);
		info.AddValue("TwoHexObstacles", TwoHexObstacles);
		info.AddValue("ThreeHexObstacles", ThreeHexObstacles);
		info.AddValue("Traps", Traps);
		info.AddValue("TerrainHotCoals", TerrainHotCoals);
		info.AddValue("TerrainWater", TerrainWater);
		info.AddValue("TerrainThorns", TerrainThorns);
		info.AddValue("TerrainRubble", TerrainRubble);
		info.AddValue("GoldPiles", GoldPiles);
		info.AddValue("TreasureChestChance", TreasureChestChance);
		info.AddValue("ChestTreasureTables", ChestTreasureTables);
		info.AddValue("GoalChests", GoalChests);
		info.AddValue("SpawnerDatas", SpawnerDatas);
		info.AddValue("AllyMonsters", AllyMonsters);
		info.AddValue("Enemy2Monsters", Enemy2Monsters);
		info.AddValue("Objects", Objects);
	}

	protected ScenarioPossibleRoom(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "MonsterGroup":
					MonsterGroup = info.GetString("MonsterGroup");
					break;
				case "Biome":
					Biome = (EBiome)info.GetValue("Biome", typeof(EBiome));
					break;
				case "SubBiome":
					SubBiome = (ESubBiome)info.GetValue("SubBiome", typeof(ESubBiome));
					break;
				case "Theme":
					Theme = (ETheme)info.GetValue("Theme", typeof(ETheme));
					break;
				case "SubTheme":
					SubTheme = (ESubTheme)info.GetValue("SubTheme", typeof(ESubTheme));
					break;
				case "Tone":
					Tone = (ETone)info.GetValue("Tone", typeof(ETone));
					break;
				case "OneHexObstacles":
					OneHexObstacles = (List<int>)info.GetValue("OneHexObstacles", typeof(List<int>));
					break;
				case "TwoHexObstacles":
					TwoHexObstacles = (List<int>)info.GetValue("TwoHexObstacles", typeof(List<int>));
					break;
				case "ThreeHexObstacles":
					ThreeHexObstacles = (List<int>)info.GetValue("ThreeHexObstacles", typeof(List<int>));
					break;
				case "Traps":
					Traps = (List<int>)info.GetValue("Traps", typeof(List<int>));
					break;
				case "TerrainHotCoals":
					TerrainHotCoals = (List<int>)info.GetValue("TerrainHotCoals", typeof(List<int>));
					break;
				case "TerrainWater":
					TerrainWater = (List<int>)info.GetValue("TerrainWater", typeof(List<int>));
					break;
				case "TerrainThorns":
					TerrainThorns = (List<int>)info.GetValue("TerrainThorns", typeof(List<int>));
					break;
				case "TerrainRubble":
					TerrainRubble = (List<int>)info.GetValue("TerrainRubble", typeof(List<int>));
					break;
				case "GoldPiles":
					GoldPiles = (List<int>)info.GetValue("GoldPiles", typeof(List<int>));
					break;
				case "TreasureChestChance":
					TreasureChestChance = (List<int>)info.GetValue("TreasureChestChance", typeof(List<int>));
					break;
				case "ChestTreasureTables":
					ChestTreasureTables = (List<string>)info.GetValue("ChestTreasureTables", typeof(List<string>));
					break;
				case "GoalChests":
					GoalChests = (List<List<string>>)info.GetValue("GoalChests", typeof(List<List<string>>));
					break;
				case "SpawnerDatas":
					SpawnerDatas = (List<SpawnerData>)info.GetValue("SpawnerDatas", typeof(List<SpawnerData>));
					break;
				case "AllyMonsters":
					try
					{
						AllyMonsters = (List<Tuple<string, List<int>>>)info.GetValue("AllyMonsters", typeof(List<Tuple<string, List<int>>>));
					}
					catch
					{
						AllyMonsters = new List<Tuple<string, List<int>>>();
					}
					break;
				case "Enemy2Monsters":
					Enemy2Monsters = (List<Tuple<string, List<int>>>)info.GetValue("Enemy2Monsters", typeof(List<Tuple<string, List<int>>>));
					break;
				case "NeutralMonsters":
					Enemy2Monsters = (List<Tuple<string, List<int>>>)info.GetValue("NeutralMonsters", typeof(List<Tuple<string, List<int>>>));
					break;
				case "Objects":
					Objects = (List<Tuple<string, List<int>>>)info.GetValue("Objects", typeof(List<Tuple<string, List<int>>>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize ScenarioPossibleRoom entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public ScenarioPossibleRoom(string name, string monsterGroup, EBiome biome, ESubBiome subBiome, ETheme theme, ESubTheme subTheme, ETone tone, List<int> oneHexObstacles, List<int> twoHexObstacles, List<int> threeHexObstacles, List<int> traps, List<int> pressurePlates, List<int> terrainHotCoals, List<int> terrainWater, List<int> terrainThorns, List<int> terrainRubble, List<int> goldPiles, List<int> treasureChestChance, List<string> chestTreasureTables, List<List<string>> goalChests, List<SpawnerData> spawnerDatas, List<Tuple<string, List<int>>> allyMonsters, List<Tuple<string, List<int>>> enemy2Monsters, List<Tuple<string, List<int>>> objects)
	{
		Name = name;
		MonsterGroup = monsterGroup;
		Biome = biome;
		SubBiome = subBiome;
		Theme = theme;
		SubTheme = subTheme;
		Tone = tone;
		OneHexObstacles = oneHexObstacles;
		TwoHexObstacles = twoHexObstacles;
		ThreeHexObstacles = threeHexObstacles;
		Traps = traps;
		PressurePlates = pressurePlates;
		TerrainHotCoals = terrainHotCoals;
		TerrainWater = terrainWater;
		TerrainThorns = terrainThorns;
		TerrainRubble = terrainRubble;
		GoldPiles = goldPiles;
		TreasureChestChance = treasureChestChance;
		ChestTreasureTables = chestTreasureTables;
		GoalChests = goalChests;
		SpawnerDatas = spawnerDatas;
		AllyMonsters = allyMonsters;
		Enemy2Monsters = enemy2Monsters;
		Objects = objects;
	}

	public bool Validate(string fileName)
	{
		bool result = true;
		if (string.IsNullOrEmpty(Name))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Name entry in ScenarioPossibleRoom file:\n" + fileName);
			result = false;
		}
		if (AllyMonsters != null && AllyMonsters.Count > 0)
		{
			foreach (Tuple<string, List<int>> allyMonster in AllyMonsters)
			{
				if (allyMonster.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in AllyMonster list for " + allyMonster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == allyMonster.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + allyMonster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		if (Enemy2Monsters != null && Enemy2Monsters.Count > 0)
		{
			foreach (Tuple<string, List<int>> enemy2Monster in Enemy2Monsters)
			{
				if (enemy2Monster.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in Enemy2Monsters list for " + enemy2Monster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == enemy2Monster.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + enemy2Monster.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		if (Objects != null && Objects.Count > 0)
		{
			foreach (Tuple<string, List<int>> objects in Objects)
			{
				if (objects.Item2.Count != 4)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Too many values in Objects list for " + objects.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
				if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == objects.Item1) == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster with matching ID for " + objects.Item1 + " in ScenarioPossibleRoom file:\n" + fileName);
					result = false;
				}
			}
		}
		return result;
	}

	public void SetBiome(EBiome biome)
	{
		Biome = biome;
	}

	public void SetSubBiome(ESubBiome subBiome)
	{
		SubBiome = subBiome;
	}

	public void SetTheme(ETheme theme)
	{
		Theme = theme;
	}

	public void SetSubTheme(ESubTheme subTheme)
	{
		SubTheme = subTheme;
	}

	public void SetTone(ETone tone)
	{
		Tone = tone;
	}
}
