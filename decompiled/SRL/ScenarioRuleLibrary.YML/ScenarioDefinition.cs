using System.Collections.Generic;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ScenarioDefinition
{
	public string ID { get; private set; }

	public string Description { get; private set; }

	public ApparanceStyle BaseStyle { get; private set; }

	public string BossID { get; private set; }

	public int BossCount { get; private set; }

	public string MonsterFamily { get; private set; }

	public List<string> ScenarioMeshes { get; private set; }

	public List<string> RewardTreasureTables { get; private set; }

	public List<string> ChestTreasureTables { get; private set; }

	public ScenarioMessage ScenarioStartMessage { get; private set; }

	public ScenarioMessage ScenarioCompleteMessage { get; private set; }

	public ScenarioLayout ScenarioLayout { get; private set; }

	public string FileName { get; private set; }

	public string FileContents { get; private set; }

	public ScenarioDefinition(string id, string description, ApparanceStyle baseStyle, string bossID, int bossCount, string monsterFamily, List<string> scenarioMeshes, List<string> rewardTreasureTables, List<string> chestTreasureTables, ScenarioMessage startMessage, ScenarioMessage completeMessage, ScenarioLayout scenarioLayout, string filename, string fileContents)
	{
		ID = id;
		Description = description;
		BaseStyle = baseStyle;
		BossID = bossID;
		BossCount = bossCount;
		MonsterFamily = monsterFamily;
		ScenarioMeshes = scenarioMeshes;
		RewardTreasureTables = rewardTreasureTables;
		ChestTreasureTables = chestTreasureTables;
		ScenarioStartMessage = startMessage;
		ScenarioCompleteMessage = completeMessage;
		ScenarioLayout = scenarioLayout;
		FileName = filename;
		FileContents = fileContents;
	}

	public bool Validate()
	{
		bool result = true;
		if (BaseStyle.Biome == ScenarioPossibleRoom.EBiome.Inherit)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing BaseBiome entry in scenario file:\n" + FileName);
			result = false;
		}
		if (MonsterFamily == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing MonsterFamily entry in scenario file:\n" + FileName);
			result = false;
		}
		if (ScenarioLayout == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing/Invalid ScenarioLayout entry in scenario file:\n" + FileName);
			result = false;
		}
		else if (!ScenarioLayout.Validate(FileName))
		{
			result = false;
		}
		return result;
	}
}
