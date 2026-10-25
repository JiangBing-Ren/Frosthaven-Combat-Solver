using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class MonsterConfigYMLData
{
	public string Avatar { get; set; }

	public string ID { get; set; }

	public string Portrait { get; set; }

	public string FileName { get; set; }

	public MonsterConfigYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		Avatar = null;
		Portrait = null;
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID entry for item in file " + FileName);
			result = false;
		}
		if (Avatar == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Avatar entry for item in file " + FileName);
			result = false;
		}
		if (Portrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Portrait entry for item in file " + FileName);
			result = false;
		}
		return result;
	}
}
