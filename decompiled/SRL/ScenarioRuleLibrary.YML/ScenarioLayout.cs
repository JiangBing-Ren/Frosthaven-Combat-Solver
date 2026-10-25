using System.Collections.Generic;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ScenarioLayout
{
	public string Name { get; private set; }

	public List<CObjective> WinningObjectives { get; private set; }

	public List<CObjective> LosingObjectives { get; private set; }

	public List<CScenarioModifier> ScenarioModifiers { get; private set; }

	public List<ScenarioRoomEntry> Rooms { get; private set; }

	public ScenarioLayout(string name, List<CObjective> winningObjectives, List<CObjective> losingObjectives, List<CScenarioModifier> scenarioModifiers, List<ScenarioRoomEntry> rooms)
	{
		Name = name;
		WinningObjectives = winningObjectives;
		LosingObjectives = losingObjectives;
		ScenarioModifiers = scenarioModifiers;
		Rooms = rooms;
	}

	public bool Validate(string fileName)
	{
		bool result = true;
		if (string.IsNullOrEmpty(Name))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Name entry in ScenarioLayout file:\n" + fileName);
			result = false;
		}
		if (LosingObjectives == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing or invalid entry for LosingObjectives in ScenarioLayout file:\n" + fileName);
			result = false;
		}
		if (ScenarioModifiers == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing or invalid entry for ScenarioModifiers in ScenarioLayout file:\n" + fileName);
			result = false;
		}
		if (Rooms == null || Rooms.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing or invalid entry for Rooms in ScenarioLayout file:\n" + fileName);
			result = false;
		}
		else
		{
			foreach (ScenarioRoomEntry room in Rooms)
			{
				if (!room.Validate(fileName))
				{
					result = false;
				}
			}
		}
		return result;
	}
}
