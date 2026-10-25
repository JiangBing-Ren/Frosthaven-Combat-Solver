using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class CharacterConfigYMLData
{
	public string ID { get; set; }

	public ECharacter Model { get; set; }

	public string Icon { get; set; }

	public string IconHighlight { get; set; }

	public string IconGold { get; set; }

	public string CardConfig { get; set; }

	public string NewAdventurePortrait { get; set; }

	public string NewAdventurePortraitHighlight { get; set; }

	public string CampaignRewardIcon { get; set; }

	public string Color { get; set; }

	public string AltColor { get; set; }

	public string MapMarker { get; set; }

	public string ScenarioPortrait { get; set; }

	public string ScenarioPreviewPortrait { get; set; }

	public string InitiativeBackground { get; set; }

	public string TabIcon { get; set; }

	public string TabIconSelected { get; set; }

	public string ActiveAbilityIcon { get; set; }

	public string AssemblyPortrait { get; set; }

	public string DistributionPortrait { get; set; }

	public string Avatar { get; set; }

	public string FileName { get; set; }

	public CharacterConfigYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		Model = ECharacter.None;
		Icon = null;
		IconHighlight = null;
		IconGold = null;
		NewAdventurePortrait = null;
		NewAdventurePortraitHighlight = null;
		CampaignRewardIcon = null;
		MapMarker = null;
		ScenarioPortrait = null;
		ScenarioPreviewPortrait = null;
		InitiativeBackground = null;
		TabIcon = null;
		TabIconSelected = null;
		ActiveAbilityIcon = null;
		AssemblyPortrait = null;
		DistributionPortrait = null;
		Color = null;
		AltColor = null;
		CardConfig = null;
		Avatar = null;
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID entry for item in file " + FileName);
			result = false;
		}
		if (Model == ECharacter.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Model entry for item in file " + FileName);
			result = false;
		}
		if (Icon == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Icon entry for item in file " + FileName);
			result = false;
		}
		if (IconHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No IconHighlight entry for item in file " + FileName);
			result = false;
		}
		if (IconGold == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No IconGold entry for item in file " + FileName);
			result = false;
		}
		if (NewAdventurePortrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No NewAdventurePortrait entry for item in file " + FileName);
			result = false;
		}
		if (NewAdventurePortraitHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No NewAdventurePortriatHighlight entry for item in file " + FileName);
			result = false;
		}
		if (CampaignRewardIcon == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CampaignRewardIcon entry for item in file " + FileName);
			result = false;
		}
		if (MapMarker == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MapMarker entry for item in file " + FileName);
			result = false;
		}
		if (ScenarioPortrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ScenarioPortrait entry for item in file " + FileName);
			result = false;
		}
		if (ScenarioPreviewPortrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ScenarioPreviewPortrait entry for item in file " + FileName);
			result = false;
		}
		if (InitiativeBackground == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No InitiativeBackground entry for item in file " + FileName);
			result = false;
		}
		if (TabIcon == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TabIcon entry for item in file " + FileName);
			result = false;
		}
		if (TabIconSelected == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TabIconSelected entry for item in file " + FileName);
			result = false;
		}
		if (ActiveAbilityIcon == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ActiveAbilityIcon entry for item in file " + FileName);
			result = false;
		}
		if (AssemblyPortrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No AssemblyPortrait entry for item in file " + FileName);
			result = false;
		}
		if (DistributionPortrait == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DistributionPortrait entry for item in file " + FileName);
			result = false;
		}
		if (Avatar == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Avatar entry for item in file " + FileName);
			result = false;
		}
		return result;
	}
}
