using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class CharacterResourceData
{
	public string ID { get; set; }

	public string Sprite { get; set; }

	public string LocKey { get; set; }

	public string ResourceModel { get; set; }

	public bool DropOnDeath { get; set; }

	public CObjectiveFilter CanLootFilter { get; set; }

	public int? MaxAmount { get; set; }

	public string FileName { get; private set; }

	public int NumTimesCanBeLooted { get; set; } = -1;

	public bool StayActiveAfterLooted { get; set; }

	public bool Hidden { get; set; }

	public CharacterResourceData(string fileName)
	{
		FileName = fileName;
		MaxAmount = null;
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID set for Character Resource Data.  File " + FileName);
			result = false;
		}
		if (Sprite == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Sprite set for Character Resource Data.  File " + FileName);
			result = false;
		}
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No LocKey set for Character Resource Data.  File " + FileName);
			result = false;
		}
		if (DropOnDeath && ResourceModel == string.Empty)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "To drop the resource on death a Resource Model must be set.  File " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(CharacterResourceData newData)
	{
		if (newData.ID != string.Empty)
		{
			ID = newData.ID;
		}
		if (newData.Sprite != string.Empty)
		{
			Sprite = newData.Sprite;
		}
		if (newData.LocKey != string.Empty)
		{
			LocKey = newData.LocKey;
		}
		if (newData.MaxAmount.HasValue)
		{
			MaxAmount = newData.MaxAmount;
		}
		NumTimesCanBeLooted = newData.NumTimesCanBeLooted;
		StayActiveAfterLooted = newData.StayActiveAfterLooted;
		Hidden = newData.Hidden;
	}
}
