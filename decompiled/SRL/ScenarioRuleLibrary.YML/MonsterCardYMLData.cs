using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class MonsterCardYMLData
{
	public int ID { get; set; }

	public int Initiative { get; set; }

	public bool? ShuffleNullable { get; set; }

	public CAction CardAction { get; set; }

	public string FileName { get; private set; }

	public string Name { get; set; }

	public bool Shuffle
	{
		get
		{
			if (!ShuffleNullable.HasValue)
			{
				return false;
			}
			return ShuffleNullable.Value;
		}
		set
		{
			ShuffleNullable = value;
		}
	}

	public MonsterCardYMLData(string fileName)
	{
		FileName = fileName;
		ID = int.MaxValue;
		Initiative = int.MaxValue;
		ShuffleNullable = null;
		CardAction = null;
	}

	public bool Validate()
	{
		if (ID == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified in " + FileName);
			return false;
		}
		if (Initiative == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Initiative specified in " + FileName);
			return false;
		}
		if (!ShuffleNullable.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Shuffle specified in " + FileName);
			return false;
		}
		if (CardAction == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CardAction specified in " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(MonsterCardYMLData data)
	{
		if (data.Initiative != int.MaxValue)
		{
			Initiative = data.Initiative;
		}
		if (data.ShuffleNullable.HasValue)
		{
			ShuffleNullable = data.ShuffleNullable;
		}
		if (data.CardAction != null)
		{
			CardAction = data.CardAction;
		}
	}
}
