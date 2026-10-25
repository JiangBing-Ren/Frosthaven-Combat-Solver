using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonsterSpawnRates
{
	public string SpawnRatesID { get; private set; }

	public int Min { get; private set; }

	public int Max { get; set; }

	public int[] EliteChance { get; set; }

	public MonsterSpawnRates(string spawnRatesID, int min, int max, int[] eliteChance)
	{
		SpawnRatesID = spawnRatesID;
		Min = min;
		Max = max;
		EliteChance = eliteChance;
	}

	public MonsterSpawnRates Copy()
	{
		return new MonsterSpawnRates(SpawnRatesID, Min, Max, EliteChance);
	}

	public static List<MonsterSpawnRates> Process(MappingEntry entry, string fileName)
	{
		List<MonsterSpawnRates> list = new List<MonsterSpawnRates>();
		if (YMLShared.GetMapping(entry, fileName, out var mapping))
		{
			foreach (MappingEntry spawnablesEntry in mapping.Entries)
			{
				if (spawnablesEntry == null || !YMLShared.GetSequence(spawnablesEntry.Value, spawnablesEntry.Key.ToString(), fileName, out var sequence))
				{
					continue;
				}
				MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData m) => m.ID == spawnablesEntry.Key.ToString());
				bool flag = true;
				if (monsterYMLData == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {spawnablesEntry.Key.ToString()} in {fileName}");
					flag = false;
				}
				if (sequence.Entries.Count == 2 || ((sequence.Entries.Count == 6) & flag))
				{
					int[] array = new int[0];
					int min = 0;
					int max = 0;
					if (YMLShared.ParseIntValue((sequence.Entries[0] as Scalar).Text, "Min", fileName, out var value))
					{
						min = value;
					}
					if (YMLShared.ParseIntValue((sequence.Entries[1] as Scalar).Text, "Max", fileName, out var value2))
					{
						max = value2;
					}
					if (sequence.Entries.Count == 6)
					{
						array = new int[5];
						if (YMLShared.ParseIntValue((sequence.Entries[2] as Scalar).Text, "EliteChance", fileName, out var value3))
						{
							array[1] = value3;
						}
						if (YMLShared.ParseIntValue((sequence.Entries[3] as Scalar).Text, "EliteChance", fileName, out var value4))
						{
							array[2] = value4;
						}
						if (YMLShared.ParseIntValue((sequence.Entries[4] as Scalar).Text, "EliteChance", fileName, out var value5))
						{
							array[3] = value5;
						}
						if (YMLShared.ParseIntValue((sequence.Entries[5] as Scalar).Text, "EliteChance", fileName, out var value6))
						{
							array[4] = value6;
						}
					}
					list.Add(new MonsterSpawnRates(spawnablesEntry.Key.ToString(), min, max, array));
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry under " + entry.Key?.ToString() + ". File:\n" + fileName);
				}
			}
			if (list.Sum((MonsterSpawnRates x) => x.Min) > 100)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid monster group - Minimum threat percentages exceed 100%");
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid monster group - no valid mapping found");
		}
		return list;
	}
}
