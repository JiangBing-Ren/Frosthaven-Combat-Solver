using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

public class RemoveYMLData
{
	public List<string> FilesToRemove { get; set; }

	public string FileName { get; set; }

	public RemoveYMLData(string filename)
	{
		FileName = filename;
		FilesToRemove = new List<string>();
	}

	public bool Validate()
	{
		return true;
	}

	public void UpdateData(RemoveYMLData newData)
	{
		if (newData.FilesToRemove != null)
		{
			FilesToRemove = newData.FilesToRemove;
		}
	}
}
