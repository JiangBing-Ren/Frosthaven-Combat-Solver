using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH098 : ScenarioScriptFrontend
{
	private static List<string> rooms = new List<string> { "4bd5a8c4-aa09-422c-9bbd-601aab85623e", "316f6d48-faaa-4744-bb75-0112751f60a4", "08d024e5-aa5e-4b42-840f-e1794cb4c25f" };

	private bool _roomsDirty = true;

	private ScenarioScript_FH098 _script098 => (ScenarioScript_FH098)_script;

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		_script098.OnOpeningRoom = OnOpeningRoom;
	}

	private void OnOpeningRoom()
	{
		_roomsDirty = true;
	}

	protected override void TickInternal()
	{
		CalcRoomLights();
	}

	private void CalcRoomLights()
	{
		if (!_roomsDirty)
		{
			return;
		}
		_roomsDirty = false;
		for (int i = 0; i < rooms.Count; i++)
		{
			GameObject map = EnvironmentLoader.Instance.GetMap(rooms[i]);
			bool flag = _script098.CollapsingRooms.Contains((byte)i);
			foreach (GameObject item in map.FindAllInChildren("PCG_STE_ABD_Red_Lightshafts", includeInactive: true))
			{
				item.SetActive(!flag);
			}
		}
	}
}
