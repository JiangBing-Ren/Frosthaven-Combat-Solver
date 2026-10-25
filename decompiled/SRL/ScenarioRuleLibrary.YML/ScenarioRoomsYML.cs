using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ScenarioRoomsYML
{
	public class RoguelikeRoom
	{
		public string RoomID { get; private set; }

		public List<EMapType> MapTiles { get; private set; }

		public List<int> OneHexObstacles { get; private set; }

		public List<int> TwoHexObstacles { get; private set; }

		public List<int> ThreeHexObstacles { get; private set; }

		public List<int> Traps { get; private set; }

		public List<int> TerrainHotCoals { get; private set; }

		public List<int> TerrainWater { get; private set; }

		public List<int> TerrainThorns { get; private set; }

		public List<int> TerrainRubble { get; private set; }

		public List<int> GoldPiles { get; private set; }

		public List<int> TreasureChestChance { get; private set; }

		public List<int> PressurePlates { get; private set; }

		public RoguelikeRoom(string roomID, List<EMapType> mapTiles, List<int> oneHexObstacles, List<int> twoHexObstacles, List<int> threeHexObstacles, List<int> traps, List<int> terrainHotCoals, List<int> terrainWater, List<int> terrainThorns, List<int> terrainRubble, List<int> goldPiles, List<int> treasureChestChance, List<int> pressurePlates)
		{
			RoomID = roomID;
			MapTiles = mapTiles;
			OneHexObstacles = oneHexObstacles;
			TwoHexObstacles = twoHexObstacles;
			ThreeHexObstacles = threeHexObstacles;
			Traps = traps;
			TerrainHotCoals = terrainHotCoals;
			TerrainWater = terrainWater;
			TerrainThorns = terrainThorns;
			TerrainRubble = terrainRubble;
			GoldPiles = goldPiles;
			TreasureChestChance = treasureChestChance;
			PressurePlates = pressurePlates;
		}

		public RoguelikeRoom(RoguelikeRoom sourceRoom, ScenarioPossibleRoom roomOverrides)
		{
			RoomID = sourceRoom.RoomID;
			MapTiles = sourceRoom.MapTiles;
			OneHexObstacles = roomOverrides.OneHexObstacles ?? sourceRoom.OneHexObstacles;
			TwoHexObstacles = roomOverrides.TwoHexObstacles ?? sourceRoom.TwoHexObstacles;
			ThreeHexObstacles = roomOverrides.ThreeHexObstacles ?? sourceRoom.ThreeHexObstacles;
			Traps = roomOverrides.Traps ?? sourceRoom.Traps;
			TerrainHotCoals = roomOverrides.TerrainHotCoals ?? sourceRoom.TerrainHotCoals;
			TerrainWater = roomOverrides.TerrainWater ?? sourceRoom.TerrainWater;
			TerrainThorns = roomOverrides.TerrainThorns ?? sourceRoom.TerrainThorns;
			TerrainRubble = roomOverrides.TerrainRubble ?? sourceRoom.TerrainRubble;
			GoldPiles = roomOverrides.GoldPiles ?? sourceRoom.GoldPiles;
			TreasureChestChance = roomOverrides.TreasureChestChance ?? sourceRoom.TreasureChestChance;
			PressurePlates = roomOverrides.PressurePlates ?? sourceRoom.PressurePlates;
		}
	}

	public List<RoguelikeRoom> Rooms { get; private set; }

	public bool IsLoaded { get; private set; }

	public string FileName { get; private set; }

	public ScenarioRoomsYML()
	{
		Rooms = new List<RoguelikeRoom>();
		IsLoaded = false;
	}

	public bool Validate()
	{
		bool result = true;
		if (Rooms.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Rooms were loaded " + FileName);
			result = false;
		}
		return result;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			FileName = fileName;
			IsLoaded = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					if (entry.Key.ToString() == "Parser")
					{
						continue;
					}
					if (entry.Value is Mapping)
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping))
						{
							List<EMapType> list = new List<EMapType>();
							List<int> oneHexObstacles = null;
							List<int> twoHexObstacles = null;
							List<int> threeHexObstacles = null;
							List<int> traps = null;
							List<int> terrainHotCoals = null;
							List<int> terrainWater = null;
							List<int> terrainThorns = null;
							List<int> terrainRubble = null;
							List<int> goldPiles = null;
							List<int> treasureChestChance = null;
							List<int> pressurePlates = null;
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								if (entry2 == null)
								{
									continue;
								}
								switch (entry2.Key.ToString())
								{
								case "MapTiles":
								{
									if (!YMLShared.GetStringList(entry2.Value, "MapTiles", FileName, out var values))
									{
										break;
									}
									foreach (string map in values)
									{
										EMapType eMapType = CMap.MapTypes.SingleOrDefault((EMapType s) => s.ToString() == map);
										if (eMapType != EMapType.None)
										{
											list.Add(eMapType);
											continue;
										}
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Map " + map + " in file " + fileName);
										flag = false;
									}
									break;
								}
								case "OneHexObstacles":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables3))
									{
										oneHexObstacles = spawnables3;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TwoHexObstacles":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables8))
									{
										twoHexObstacles = spawnables8;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "ThreeHexObstacles":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables11))
									{
										threeHexObstacles = spawnables11;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "Traps":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables5))
									{
										traps = spawnables5;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TerrainHotCoals":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables10))
									{
										terrainHotCoals = spawnables10;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TerrainWater":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables6))
									{
										terrainWater = spawnables6;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TerrainThorns":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables2))
									{
										terrainThorns = spawnables2;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TerrainRubble":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables9))
									{
										terrainRubble = spawnables9;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "GoldPiles":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables7))
									{
										goldPiles = spawnables7;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "TreasureChestChance":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables4))
									{
										treasureChestChance = spawnables4;
									}
									else
									{
										flag = false;
									}
									break;
								}
								case "PressurePlates":
								{
									if (ProcessSpawnables(entry2, FileName, out var spawnables))
									{
										pressurePlates = spawnables;
									}
									else
									{
										flag = false;
									}
									break;
								}
								default:
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry " + entry2.Key.ToString() + " in " + entry.Key.ToString() + ", File: " + FileName);
									break;
								}
							}
							if (list.Count == 0)
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing MapTiles entry in " + entry.Key.ToString() + ", File: " + FileName);
								flag = false;
							}
							if (!flag)
							{
								break;
							}
							Rooms.Add(new RoguelikeRoom(entry.Key.ToString(), list, oneHexObstacles, twoHexObstacles, threeHexObstacles, traps, terrainHotCoals, terrainWater, terrainThorns, terrainRubble, goldPiles, treasureChestChance, pressurePlates));
						}
						else
						{
							flag = false;
						}
						continue;
					}
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "Non mapping found, unable to parse yml. File:\n" + FileName);
					flag = false;
					break;
				}
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to parse yml. File:\n" + FileName);
			foreach (string item in yamlParser.Errors.Select((Pair<int, string> x) => x.Right))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, item);
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}

	public static bool ProcessSpawnables(MappingEntry entry, string fileName, out List<int> spawnables)
	{
		bool result = true;
		spawnables = new List<int>();
		if (YMLShared.GetSequence(entry.Value, entry.Key.ToString(), fileName, out var sequence))
		{
			foreach (DataItem entry2 in sequence.Entries)
			{
				if (entry2 is Scalar)
				{
					if (YMLShared.GetIntPropertyValue(entry2 as Scalar, entry.Key.ToString(), fileName, out var value))
					{
						if (value < 0)
						{
							value = 0;
						}
						if (value > 100)
						{
							value = 100;
						}
						spawnables.Add(value);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry under " + entry.Key.ToString() + ", must be a sequence of spawn chances out of 100. File:\n" + fileName);
					result = false;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}
}
