using System.Collections.Generic;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ScenarioRoomEntry
{
	public string Name { get; private set; }

	public int ThreatLevel { get; private set; }

	public string ParentRoom { get; private set; }

	public bool BossInHere { get; private set; }

	public bool IsDungeonExitRoom { get; private set; }

	public bool IsAdditionalDungeonEntrance { get; private set; }

	public ScenarioMessage RoomRevealedMessage { get; private set; }

	public List<ScenarioPossibleRoom> PossibleRooms { get; private set; }

	public ScenarioRoomEntry(string name, int threatLevel, string parentRoom, bool bossInHere, ScenarioMessage roomRevealedMessage, List<ScenarioPossibleRoom> possibleRooms, bool isDungeonExitRoom, bool isAdditionalDungeonEntrance)
	{
		Name = name;
		ThreatLevel = threatLevel;
		ParentRoom = parentRoom;
		BossInHere = bossInHere;
		RoomRevealedMessage = roomRevealedMessage;
		PossibleRooms = possibleRooms;
		IsDungeonExitRoom = isDungeonExitRoom;
		IsAdditionalDungeonEntrance = isAdditionalDungeonEntrance;
	}

	public bool Validate(string fileName)
	{
		bool result = true;
		if (string.IsNullOrEmpty(Name))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Name entry in ScenarioRoomEntry file:\n" + fileName);
			result = false;
		}
		if (ThreatLevel == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing ThreatLevel entry in ScenarioRoomEntry file:\n" + fileName);
			result = false;
		}
		if (PossibleRooms == null || PossibleRooms.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing PossibleRooms entry in ScenarioRoomEntry file:\n" + fileName);
			result = false;
		}
		else
		{
			foreach (ScenarioPossibleRoom possibleRoom in PossibleRooms)
			{
				if (!possibleRoom.Validate(fileName))
				{
					result = false;
				}
			}
		}
		return result;
	}
}
