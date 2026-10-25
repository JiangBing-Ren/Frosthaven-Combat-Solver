using System.Collections.Generic;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ScenarioAbilitiesYMLData
{
	public string ScenarioAbilityID { get; set; }

	public List<CAbility> ScenarioAbilities { get; set; }

	public string FileName { get; private set; }

	public ScenarioAbilitiesYMLData(string filename)
	{
		FileName = filename;
		ScenarioAbilityID = null;
		ScenarioAbilities = null;
	}

	public bool Validate()
	{
		if (ScenarioAbilityID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid ScenarioAbilityID specified for ScenarioAbility file " + FileName);
			return false;
		}
		if (ScenarioAbilities == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid ScenarioAbilities specified for ScenarioAbility file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(ScenarioAbilitiesYMLData newData)
	{
		if (newData.ScenarioAbilities != null)
		{
			ScenarioAbilities = newData.ScenarioAbilities;
		}
	}
}
