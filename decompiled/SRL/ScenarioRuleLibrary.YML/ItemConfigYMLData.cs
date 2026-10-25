using System;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ItemConfigYMLData
{
	public enum EItemAudioToggle
	{
		None,
		Potion,
		Shield,
		Body,
		Head,
		Boots,
		Trinkets,
		Weapon
	}

	public static readonly EItemAudioToggle[] ItemAudioToggles = (EItemAudioToggle[])Enum.GetValues(typeof(EItemAudioToggle));

	public string ItemName { get; set; }

	public string Icon { get; set; }

	public string Background { get; set; }

	public EItemAudioToggle Audio { get; set; }

	public string FileName { get; set; }

	public ItemConfigYMLData(string fileName)
	{
		FileName = fileName;
		ItemName = null;
		Icon = null;
		Background = null;
		Audio = EItemAudioToggle.None;
	}

	public bool Validate()
	{
		bool result = true;
		if (ItemName == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ItemName entry for item in file " + FileName);
			result = false;
		}
		if (Icon == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Icon entry for item in file " + FileName);
			result = false;
		}
		if (Background == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Background entry for item in file " + FileName);
			result = false;
		}
		if (Audio == EItemAudioToggle.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Audio entry for item in file " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(ItemConfigYMLData data)
	{
		if (data.ItemName != null)
		{
			ItemName = data.ItemName;
		}
		if (data.Icon != null)
		{
			Icon = data.Icon;
		}
		if (data.Background != null)
		{
			Background = data.Background;
		}
		if (data.Audio != EItemAudioToggle.None)
		{
			Audio = data.Audio;
		}
	}
}
