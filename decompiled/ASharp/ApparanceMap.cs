using System.Collections.Generic;
using UnityEngine;

public class ApparanceMap : MonoBehaviour
{
	public string MapGuid { get; set; }

	public string RoomName { get; set; }

	public List<GameObject> Tiles { get; set; }

	public GameObject EntranceDoor { get; set; }

	public GameObject DungeonEntranceDoor { get; set; }

	public GameObject DungeonExitDoor { get; set; }

	public bool IsDungeonEntrance { get; set; }

	public bool IsDungeonExit { get; set; }
}
