using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class MapConfigYMLData
{
	public string NorthWest { get; set; }

	public string NorthEast { get; set; }

	public string SouthWest { get; set; }

	public string SouthEast { get; set; }

	public string FileName { get; set; }

	public MapConfigYMLData(string fileName)
	{
		FileName = fileName;
		NorthWest = null;
		NorthEast = null;
		SouthWest = null;
		SouthEast = null;
	}

	public bool Validate()
	{
		bool result = true;
		if (NorthWest == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No NorthWest entry for item in file " + FileName);
			result = false;
		}
		if (NorthEast == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No NorthEast entry for item in file " + FileName);
			result = false;
		}
		if (SouthWest == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No SouthWest entry for item in file " + FileName);
			result = false;
		}
		if (SouthEast == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No SouthEast entry for item in file " + FileName);
			result = false;
		}
		return result;
	}
}
