using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH064 : ScenarioScriptFrontend
{
	private static string centralRoomGuid = "26829844-7105-40cb-9b36-2f0fc6dbb34d";

	private static List<GameObject> _outherRooms;

	private static GameObject _centralRoom;

	private static GameObject _centralRoomAmbience;

	private static GameObject _centralRoomBossLightingVFX;

	private ScenarioScript_FH064 _script064 => (ScenarioScript_FH064)_script;

	private static List<GameObject> outherRooms
	{
		get
		{
			if (_outherRooms == null || _outherRooms.Count == 0 || _outherRooms[0] == null)
			{
				_outherRooms = (from obj in EnvironmentLoader.Instance.GetMaps()
					where obj != centralRoom
					select obj).ToList();
			}
			return _outherRooms;
		}
	}

	private static GameObject centralRoom
	{
		get
		{
			if (_centralRoom == null)
			{
				_centralRoom = EnvironmentLoader.Instance.GetMap(centralRoomGuid);
			}
			return _centralRoom;
		}
	}

	private static GameObject centralRoomAmbience
	{
		get
		{
			if (_centralRoomAmbience == null && centralRoom != null)
			{
				_centralRoomAmbience = centralRoom.FindInChildren("PCG_FH_PlainsSeasons_ScenarioAmbience");
			}
			return _centralRoomAmbience;
		}
	}

	private static GameObject centralRoomBossLightingVFX
	{
		get
		{
			if (_centralRoomBossLightingVFX == null)
			{
				GameObject propPrefab = GlobalSettings.GetPropPrefab("S64BossLigtningVFX");
				GameObject gameObject = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[9, 11].m_GameObject;
				_centralRoomBossLightingVFX = ObjectPool.Spawn(propPrefab, gameObject.transform, Vector3.zero, Quaternion.identity);
			}
			return _centralRoomBossLightingVFX;
		}
	}

	protected override void TickInternal()
	{
		CalcVisuals();
	}

	private void CalcVisuals(bool reset = false)
	{
		CalcRoomsVisibility(reset);
		CalcLighting(reset);
	}

	private void CalcRoomsVisibility(bool reset = false)
	{
		bool active = !_script064.SecondPhase;
		if (reset)
		{
			active = true;
		}
		if (outherRooms == null)
		{
			return;
		}
		foreach (GameObject outherRoom in outherRooms)
		{
			if (outherRoom != null)
			{
				outherRoom.SetActive(active);
			}
		}
	}

	private void CalcLighting(bool reset = false)
	{
		bool flag = _script064.SecondPhase;
		if (reset)
		{
			flag = false;
		}
		if (centralRoomAmbience != null)
		{
			centralRoomAmbience.SetActive(!flag);
		}
		if (centralRoomBossLightingVFX != null)
		{
			centralRoomBossLightingVFX.SetActive(flag);
		}
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		CalcVisuals(reset: true);
	}

	protected override void DeinitializeInternal()
	{
		base.DeinitializeInternal();
		_outherRooms = null;
		_centralRoom = null;
		_centralRoomAmbience = null;
		_centralRoomBossLightingVFX = null;
	}

	protected override void OnFastLoadInternal()
	{
		CalcVisuals(reset: true);
		_outherRooms = null;
		_centralRoom = null;
		_centralRoomAmbience = null;
		ObjectPool.Recycle(_centralRoomBossLightingVFX);
		_centralRoomBossLightingVFX = null;
	}
}
