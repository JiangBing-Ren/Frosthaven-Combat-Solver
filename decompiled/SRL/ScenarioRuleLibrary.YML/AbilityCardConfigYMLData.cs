using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class AbilityCardConfigYMLData
{
	public const string Default = "Default";

	public string ID { get; set; }

	public string DefaultCharacter { get; set; }

	public string TitleSprite { get; set; }

	public string TopActionRegular { get; set; }

	public string TopActionHighlight { get; set; }

	public string TopActionSelected { get; set; }

	public string TopActionDisabled { get; set; }

	public string BottomActionRegular { get; set; }

	public string BottomActionHighlight { get; set; }

	public string BottomActionSelected { get; set; }

	public string BottomActionDisabled { get; set; }

	public string DefaultTopActionRegular { get; set; }

	public string DefaultTopActionHighlight { get; set; }

	public string DefaultTopActionDisabled { get; set; }

	public string DefaultBottomActionRegular { get; set; }

	public string DefaultBottomActionHighlight { get; set; }

	public string DefaultBottomActionDisabled { get; set; }

	public string PreviewRegular { get; set; }

	public string PreviewHighlight { get; set; }

	public string PreviewSelected { get; set; }

	public string PreviewSelectedHighlight { get; set; }

	public string PreviewActive { get; set; }

	public string PreviewActiveHighlight { get; set; }

	public string FileName { get; set; }

	public string InitiativeColor { get; set; }

	public string ButtonHolder { get; set; }

	public string LongRestRegular { get; set; }

	public string LongRestHighlight { get; set; }

	public string LongRestSelected { get; set; }

	public string LongRestDisabled { get; set; }

	public string PreviewRegularTextColor { get; set; }

	public string PreviewActiveTextColor { get; set; }

	public string PreviewSelectedTextColor { get; set; }

	public string PreviewDiscardedTextColor { get; set; }

	public string PreviewLostTextColor { get; set; }

	public string PreviewPermalostTextColor { get; set; }

	public float PreviewDiscardedTextOpacity { get; set; }

	public float PreviewLostTextOpacity { get; set; }

	public string PreviewDiscarded { get; set; }

	public string PreviewLost { get; set; }

	public string PreviewPermalost { get; set; }

	public string PreviewLongRest { get; set; }

	public string PreviewLongRestHighlight { get; set; }

	public string PreviewLongRestSelected { get; set; }

	public string PreviewLongRestSelectedHighlight { get; set; }

	public string PreviewLongRestDiscarded { get; set; }

	public string PreviewLongRestLost { get; set; }

	public AbilityCardConfigYMLData(string fileName)
	{
		FileName = FileName;
		ID = null;
		DefaultCharacter = null;
		TitleSprite = null;
		TopActionRegular = null;
		TopActionSelected = null;
		TopActionHighlight = null;
		TopActionDisabled = null;
		BottomActionRegular = null;
		BottomActionSelected = null;
		BottomActionHighlight = null;
		BottomActionDisabled = null;
		DefaultTopActionRegular = null;
		DefaultTopActionHighlight = null;
		DefaultTopActionDisabled = null;
		DefaultBottomActionRegular = null;
		DefaultBottomActionHighlight = null;
		DefaultBottomActionDisabled = null;
		PreviewRegular = null;
		PreviewHighlight = null;
		PreviewSelected = null;
		PreviewSelectedHighlight = null;
		PreviewActive = null;
		PreviewActiveHighlight = null;
		LongRestRegular = "Default";
		LongRestHighlight = "Default";
		LongRestSelected = "Default";
		LongRestDisabled = "Default";
		InitiativeColor = "F3DDABFF";
		PreviewRegularTextColor = "F3DDABFF";
		PreviewActiveTextColor = "FFFFFFFF";
		PreviewSelectedTextColor = "FFFFFFFF";
		PreviewDiscardedTextColor = "595959FF";
		PreviewLostTextColor = "8F3A2CFF";
		PreviewPermalostTextColor = "8F3A2C33";
		PreviewDiscardedTextOpacity = 1f;
		PreviewLostTextOpacity = 0.75f;
		PreviewDiscarded = "Default";
		PreviewLost = "Default";
		PreviewPermalost = "Default";
		PreviewLongRest = "Default";
		PreviewLongRestHighlight = "Default";
		PreviewLongRestDiscarded = "Default";
		PreviewLongRestSelected = "Default";
		PreviewLongRestSelectedHighlight = "Default";
		PreviewLongRestLost = "Default";
		ButtonHolder = "Default";
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID entry for item in file " + FileName);
			result = false;
		}
		if (DefaultCharacter == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Character entry for item in file " + FileName);
			result = false;
		}
		if (TitleSprite == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TitleSprite entry for item in file " + FileName);
			result = false;
		}
		if (TopActionRegular == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TopActionRegular entry for item in file " + FileName);
			result = false;
		}
		if (TopActionSelected == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TopActionSelected entry for item in file " + FileName);
			result = false;
		}
		if (TopActionHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TopActionHighlight entry for item in file " + FileName);
			result = false;
		}
		if (TopActionDisabled == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No TopActionDisabled entry for item in file " + FileName);
			result = false;
		}
		if (BottomActionRegular == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No BottomActionRegular entry for item in file " + FileName);
			result = false;
		}
		if (BottomActionSelected == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No BottomActionSelected entry for item in file " + FileName);
			result = false;
		}
		if (BottomActionHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No BottomActionHighlight entry for item in file " + FileName);
			result = false;
		}
		if (BottomActionDisabled == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No BottomActionDisabled entry for item in file " + FileName);
			result = false;
		}
		if (DefaultTopActionRegular == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultTopActionRegular entry for item in file " + FileName);
			result = false;
		}
		if (DefaultTopActionHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultTopActionHighlight entry for item in file " + FileName);
			result = false;
		}
		if (DefaultTopActionDisabled == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultTopActionDisabled entry for item in file " + FileName);
			result = false;
		}
		if (DefaultBottomActionRegular == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultBottomActionRegular entry for item in file " + FileName);
			result = false;
		}
		if (DefaultBottomActionHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultBottomActionHighlight entry for item in file " + FileName);
			result = false;
		}
		if (DefaultBottomActionDisabled == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No DefaultBottomActionDisabled entry for item in file " + FileName);
			result = false;
		}
		if (PreviewRegular == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewRegular entry for item in file " + FileName);
			result = false;
		}
		if (PreviewHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewHighlight entry for item in file " + FileName);
			result = false;
		}
		if (PreviewSelected == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewSelected entry for item in file " + FileName);
			result = false;
		}
		if (PreviewSelectedHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewSelectedHighlight entry for item in file " + FileName);
			result = false;
		}
		if (PreviewActive == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewActive entry for item in file " + FileName);
			result = false;
		}
		if (PreviewActiveHighlight == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PreviewActiveHighlight entry for item in file " + FileName);
			result = false;
		}
		return result;
	}
}
