using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH113 : ScenarioScript
{
	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		CMap cMap = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_4_C");
		CMap cMap2 = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_13_C");
		if ((_tickRoomRevealed == cMap || _tickRoomRevealed == cMap2) && cMap.Revealed && cMap2.Revealed)
		{
			CObjectDoor cObjectDoor = ScenarioManager.GetTile(9, 10)?.FindDoor();
			if (cObjectDoor != null && cObjectDoor.DoorIsClosed)
			{
				cObjectDoor.ForceActivate(null);
			}
		}
	}
}
