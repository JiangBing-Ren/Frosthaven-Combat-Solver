using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH127 : ScenarioScriptFrontend
{
	private class StairPropInfo
	{
		public GameObject propGo;

		public bool opened;

		public Point pos;

		public string nextRoom;
	}

	private bool m_Invalidate = true;

	private List<StairPropInfo> stairPropInfo;

	private ScenarioScript_FH127 _script127 => (ScenarioScript_FH127)_script;

	protected override void InitializeInternal()
	{
		stairPropInfo = new List<StairPropInfo>
		{
			new StairPropInfo
			{
				pos = new Point(5, 10),
				nextRoom = "Room_7"
			},
			new StairPropInfo
			{
				pos = new Point(13, 12),
				nextRoom = "Room_8"
			}
		};
		_script127.OnStairStateChanged = () =>
		{
			m_Invalidate = true;
		};
		_script127.OnStairStateChanged();
	}

	protected override void TickInternal()
	{
		if (!m_Invalidate)
		{
			return;
		}
		m_Invalidate = false;
		foreach (StairPropInfo item in stairPropInfo)
		{
			CTile tile = ScenarioManager.GetTile(item.pos);
			if (!tile.m_HexMap.Revealed)
			{
				continue;
			}
			CObjectProp cObjectProp = tile.FindPropOfTypes(EPropType.TerrainVisualEffect);
			if (item.propGo == null)
			{
				if (cObjectProp == null)
				{
					continue;
				}
				item.propGo = Choreographer.FindPropGameObject(cObjectProp);
				if (item.propGo == null)
				{
					continue;
				}
				item.propGo.AddComponent<UnityGameEditorDifficultTerrainProp>();
			}
			string nextRoomKey = item.nextRoom;
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.RoomName == nextRoomKey);
			CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
			cChangePropVisualState_MessageData.m_prop = cObjectProp;
			cChangePropVisualState_MessageData.m_delay = 0.001f;
			if (cMap.Revealed != item.opened)
			{
				item.opened = cMap.Revealed;
				cChangePropVisualState_MessageData.m_stateName = "State1";
			}
			else
			{
				cChangePropVisualState_MessageData.m_stateName = "State0";
			}
			Choreographer.s_Choreographer.Handle_ChangePropVisualState(cChangePropVisualState_MessageData);
		}
	}
}
