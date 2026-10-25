using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FHRL;
using UnityEngine;
using UnityEngine.Rendering;

namespace FHGUI.Elements;

public class FHWorldMapScene : MonoBehaviour, ISceneRoot
{
	[SerializeField]
	private FHMapCameraController _cameraRig;

	[SerializeField]
	private Volume _postProcess;

	[SerializeField]
	private Ambience _ambience;

	[SerializeField]
	private Transform _questMarkerContainer;

	[SerializeField]
	private Transform[] _questObjectContainers;

	[SerializeField]
	private FHPartyTravel _PartyTravel;

	[SerializeField]
	private GameObject[] _FrostheavenWallLevels;

	private Dictionary<string, FHScenarioMarker> _markers = new Dictionary<string, FHScenarioMarker>();

	private List<FHScenarioObject> _objects = new List<FHScenarioObject>();

	private Dictionary<string, FHPOI> _pointsOfInterest = new Dictionary<string, FHPOI>();

	private int _prevUnlockedScenariosCount;

	private uint _prevWallLevel;

	public FHMapCameraController CameraRig => _cameraRig;

	public Transform Root => transform;

	public bool IsTravelling => _PartyTravel.IsTravelling;

	public void Init()
	{
		FHScenarioMarker[] componentsInChildren = _questMarkerContainer.GetComponentsInChildren<FHScenarioMarker>(includeInactive: true);
		Transform[] questObjectContainers = _questObjectContainers;
		foreach (Transform transform in questObjectContainers)
		{
			_objects.AddRange(transform.GetComponentsInChildren<FHScenarioObject>(includeInactive: true));
		}
		FHScenarioMarker[] array = componentsInChildren;
		foreach (FHScenarioMarker fHScenarioMarker in array)
		{
			_markers.TryAdd(fHScenarioMarker.ScenarioID.ToString(), fHScenarioMarker);
			fHScenarioMarker.gameObject.SetActive(value: false);
		}
		FHPOI[] componentsInChildren2 = Root.GetComponentsInChildren<FHPOI>(includeInactive: true);
		foreach (FHPOI fHPOI in componentsInChildren2)
		{
			_pointsOfInterest.TryAdd(fHPOI.Key, fHPOI);
			_objects.AddRange(fHPOI.GetComponentsInChildren<FHScenarioObject>(includeInactive: true));
		}
		_PartyTravel.Clear();
	}

	public void Show(object data = null)
	{
		gameObject.SetActive(value: true);
		_cameraRig.gameObject.SetActive(value: true);
		(List<ScenarioState>, uint) tuple = ((data is ITuple { Length: 2 } tuple2 && tuple2[0] is List<ScenarioState> && tuple2[1] is uint) ? (((List<ScenarioState>, uint))data) : (null, 0u));
		List<ScenarioState> item = tuple.Item1;
		_prevUnlockedScenariosCount = item?.Count ?? 0;
		_prevWallLevel = tuple.Item2;
		SetScenarioMarkers(item);
		if (_ambience != null)
		{
			_ambience.ApplyBlend(1f, 1f);
		}
		List<ScenarioID> scenarios = item?.Select((ScenarioState state) => state.ScenarioID).ToList() ?? new List<ScenarioID>();
		foreach (FHScenarioObject @object in _objects)
		{
			@object.Init(scenarios);
		}
		if (Singleton<FHMapChoreographer>.InstanceFast?.GameState != null)
		{
			string partyMarkerLocation = Singleton<FHMapChoreographer>.InstanceFast.GameState.PartyMarkerLocation;
			_PartyTravel.InitCache();
			Vector3 position = _PartyTravel.GetPosition(partyMarkerLocation);
			CameraRig.FocusOnTargetImmediately(position);
		}
		for (int num = 0; num < _FrostheavenWallLevels.Length; num++)
		{
			_FrostheavenWallLevels[num].SetActive(value: false);
		}
		_FrostheavenWallLevels[tuple.Item2].SetActive(value: true);
	}

	public void Hide()
	{
		StopFocusingOnScenario();
		_cameraRig.ResetFocusImmediately();
		gameObject.SetActive(value: false);
		_cameraRig.gameObject.SetActive(value: false);
	}

	public bool ShouldRefresh(object data = null)
	{
		(List<ScenarioState>, uint) tuple = ((data is ITuple { Length: 2 } tuple2 && tuple2[0] is List<ScenarioState> && tuple2[1] is uint) ? (((List<ScenarioState>, uint))data) : (null, 0u));
		if ((tuple.Item1?.Count ?? 0) == _prevUnlockedScenariosCount)
		{
			return tuple.Item2 != _prevWallLevel;
		}
		return true;
	}

	private void SetScenarioMarkers(List<ScenarioState> scenarios)
	{
		foreach (KeyValuePair<string, FHScenarioMarker> entry in _markers)
		{
			ScenarioState scenarioState = scenarios.SingleOrDefault((ScenarioState x) =>
			{
				uint iD = x.Def.ID.ID;
				return iD.ToString().Equals(entry.Key, StringComparison.InvariantCultureIgnoreCase);
			});
			if (scenarioState != null && (!scenarioState.Completed || !scenarioState.Def.CanNotBeReplayed))
			{
				entry.Value.Init(scenarioState);
			}
			else
			{
				entry.Value.gameObject.SetActive(value: false);
			}
		}
	}

	public void FocusOnScenario(int id)
	{
		_markers.TryGetValue(id.ToString(), out var value);
		if (value != null)
		{
			_cameraRig.FocusAndLockOntoTarget(value.transform, 1f);
		}
		foreach (FHScenarioMarker value2 in _markers.Values)
		{
			if (value2.ScenarioID == id)
			{
				value2.Focus((value2.ScenarioID == id) ? FHScenarioMarker.HighlightType.Normal : FHScenarioMarker.HighlightType.None);
			}
			else
			{
				value2.Unfocus();
			}
		}
	}

	public void StopFocusingOnScenario()
	{
		_cameraRig.StopRunningAutomation();
		foreach (FHScenarioMarker value in _markers.Values)
		{
			value.Unfocus();
		}
	}

	public FHScenarioMarker GetMarker(string id)
	{
		return _markers.GetValueOrDefault(id);
	}

	public FHPOI GetPointOfInterest(string id)
	{
		return _pointsOfInterest.GetValueOrDefault(id);
	}

	public void BeginPartyTravel(FHPartyTravel.TravelParams @params)
	{
		_PartyTravel.Init(@params);
	}

	public void StopPartyTravel()
	{
		_PartyTravel.StopPartyTravel();
		_PartyTravel.Clear();
	}

	public void ShowUnlockScenarios(int[] scenarios, Action OnTravelComplete)
	{
		_PartyTravel.ShowUnlockScenarios(scenarios, OnTravelComplete);
	}
}
