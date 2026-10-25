using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FH.LevelComponents.ProcGen;
using FH.Tools;
using Libs.Utils;
using SETUtil;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Rendering;

namespace FH.SeasonTech;

[ExecuteAlways]
public class BiomeStateBasedToggle : MonoBehaviour
{
	[Header("Temperature")]
	[SerializeField]
	private List<GameObject> _temperatureToggle = new List<GameObject>();

	[SerializeField]
	private bool _toggleWhenAboveTemperature;

	[SerializeField]
	private List<GameObject> _temperatureToggleWhenAbove = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _temperatureToggleWhenBelow = new List<GameObject>();

	[SerializeField]
	private bool _scaleWithTemperature;

	[Tooltip("Toggles the default art off and uses only the visuals from this biome state")]
	[SerializeField]
	private bool _replaceDefaultVisuals;

	[HideInInspector]
	[SerializeField]
	private List<GameObject> _rainToggle = new List<GameObject>();

	[HideInInspector]
	[SerializeField]
	private List<GameObject> _snowToggle = new List<GameObject>();

	[HideInInspector]
	[SerializeField]
	private Volume _snowyVolume;

	[Header("Biome State")]
	[SerializeField]
	private ScenarioPossibleRoom.EBiomeState _biomeState = ScenarioPossibleRoom.EBiomeState.Default;

	[SerializeField]
	private List<GameObject> _biomeStateBasedToggle = new List<GameObject>();

	[Header("Technical")]
	[SerializeField]
	private List<BiomeStateBasedToggle> _references = new List<BiomeStateBasedToggle>();

	private Dictionary<Transform, Vector3> _originalScales = new Dictionary<Transform, Vector3>();

	private bool _isCurrentlyActiveAndRelevant;

	[NonSerialized]
	private WorldSeasonState _override;

	private float _temperatureThreshold => 0.5f;

	public bool HasOverride => _override != null;

	private void OnEnable()
	{
		Refresh();
	}

	[ContextMenu("Refresh")]
	public void Refresh()
	{
		UpdateReferences();
		Set(WorldSeasonState.LastSeasonState);
	}

	[ContextMenu("Debug Print Override State")]
	public void DebugPrintOverrideState()
	{
		Debug.Log(string.Format("Override: B:{0}, T:{1}. {2}:{3}", _override?.BiomeStateOverride, _override?.Temperature, "_isCurrentlyActiveAndRelevant", _isCurrentlyActiveAndRelevant));
	}

	private void UpdateReferences()
	{
		Transform parent = transform.parent;
		if ((bool)parent)
		{
			List<BiomeStateBasedToggle> list = new List<BiomeStateBasedToggle>();
			parent.gameObject.FindInImmediateChildren(ref list);
			SetReferences(list);
		}
	}

	public void SetOverrideWorldState(WorldSeasonState seasonState)
	{
		_override = seasonState;
	}

	public void Set(WorldSeasonState state)
	{
		_isCurrentlyActiveAndRelevant = false;
		if (_override != null)
		{
			state = _override;
		}
		ScenarioPossibleRoom.EBiomeState matchBiomeState = state.BiomeStateOverride;
		if (_biomeState == ScenarioPossibleRoom.EBiomeState.Inherit)
		{
			_biomeState = ScenarioPossibleRoom.EBiomeState.Default;
		}
		if (state.BiomeStateOverride == ScenarioPossibleRoom.EBiomeState.Inherit)
		{
			List<ProceduralStyle> list = new List<ProceduralStyle>(4);
			GeneralHelpersRuntime.GetProceduralStylesInParentsRecursive(transform, null, list);
			matchBiomeState = BiomeStateFromList(list);
		}
		bool stateToggle = ((_biomeState != ScenarioPossibleRoom.EBiomeState.Default) ? (matchBiomeState == _biomeState) : (_references.Count == 0 || (_references.Count > 0 && !_references.Any((BiomeStateBasedToggle a) => a._replaceDefaultVisuals && a._biomeState == matchBiomeState))));
		bool tempToggle = stateToggle && (_toggleWhenAboveTemperature ? (state.Temperature > _temperatureThreshold) : (state.Temperature < _temperatureThreshold));
		_temperatureToggle.ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, tempToggle, _toggleWhenAboveTemperature);
		});
		_temperatureToggleWhenAbove.ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, stateToggle && state.Temperature >= _temperatureThreshold, aboveTemperature: false);
		});
		_temperatureToggleWhenBelow.ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, stateToggle && state.Temperature < _temperatureThreshold, aboveTemperature: false);
		});
		bool rainToggle = (state.Rain & stateToggle) && state.Temperature > _temperatureThreshold;
		_rainToggle.Where((GameObject a) => a != null).ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, rainToggle, aboveTemperature: true);
		});
		bool snowToggle = (state.Rain & stateToggle) && state.Temperature <= _temperatureThreshold;
		_snowToggle.Where((GameObject a) => a != null).ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, snowToggle, aboveTemperature: false);
		});
		_biomeStateBasedToggle.Where((GameObject a) => a != null).ForEach((GameObject a) =>
		{
			ApplyForVisual(state, a, stateToggle, aboveTemperature: false, scaleWithTemperature: false);
		});
		if (_snowyVolume != null)
		{
			_snowyVolume.weight = Scale01FromState(state, aboveTemperature: false);
		}
	}

	public void SetBiomeStateAffiliation(ScenarioPossibleRoom.EBiomeState biomeState, IEnumerable<GameObject> gameObjects = null)
	{
		_biomeState = biomeState;
		if (_biomeState != ScenarioPossibleRoom.EBiomeState.Natural)
		{
			_biomeStateBasedToggle.AddRange(gameObjects);
		}
		else
		{
			_temperatureToggle.AddRange(gameObjects);
		}
		Refresh();
	}

	public void SetReferences(IList<BiomeStateBasedToggle> refs)
	{
		_references = refs.Where((BiomeStateBasedToggle a) => a != this && (_biomeState == ScenarioPossibleRoom.EBiomeState.Default || a._biomeState == ScenarioPossibleRoom.EBiomeState.Default)).ToList();
	}

	public void CleanUpReferences()
	{
		_references.RemoveAll((BiomeStateBasedToggle a) => a == null);
	}

	public BiomeStateBasedToggle[] GetReferences()
	{
		return _references.ToArray();
	}

	public void SetToggleAboveTemperature(bool value)
	{
		_toggleWhenAboveTemperature = value;
	}

	public void SetRepalceDefault(bool value)
	{
		_replaceDefaultVisuals = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ScenarioPossibleRoom.EBiomeState BiomeStateFromList(List<ProceduralStyle> outParentStyles)
	{
		ProceduralStyle proceduralStyle = outParentStyles.FirstOrDefault((ProceduralStyle a) => a.BiomeState != ScenarioPossibleRoom.EBiomeState.Inherit);
		if ((bool)proceduralStyle)
		{
			return proceduralStyle.GetBiomeState();
		}
		return ScenarioPossibleRoom.EBiomeState.Default;
	}

	private void OnValidate()
	{
		if (_biomeState == ScenarioPossibleRoom.EBiomeState.Inherit)
		{
			_biomeState = ScenarioPossibleRoom.EBiomeState.Default;
		}
		InitializeScales();
	}

	private void Start()
	{
		InitializeScales();
		_ = Application.isPlaying;
	}

	public bool IsBiomeStateConfigValid()
	{
		if (transform.childCount != 0)
		{
			if (_biomeStateBasedToggle.Count <= 0 && _temperatureToggle.Count <= 0 && _temperatureToggleWhenAbove.Count <= 0 && _temperatureToggleWhenBelow.Count <= 0 && _rainToggle.Count <= 0)
			{
				return _snowToggle.Count > 0;
			}
			return true;
		}
		return true;
	}

	private void InitializeScales()
	{
		ClearScales();
		RecordScales();
	}

	[ContextMenu("Clear Scales")]
	private void ClearScales()
	{
		foreach (KeyValuePair<Transform, Vector3> originalScale in _originalScales)
		{
			if ((bool)originalScale.Key)
			{
				originalScale.Key.localScale = originalScale.Value;
			}
		}
		_originalScales.Clear();
	}

	[ContextMenu("Record Scales")]
	private void RecordScales()
	{
		foreach (GameObject item in _rainToggle.Concat(_snowToggle).Concat(_biomeStateBasedToggle).Concat(_temperatureToggle))
		{
			if ((bool)item)
			{
				_originalScales[item.transform] = item.transform.localScale;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyForVisual(WorldSeasonState state, GameObject visual, bool stateToggle, bool aboveTemperature, bool scaleWithTemperature = true)
	{
		if (!visual)
		{
			Debug.LogError($"{name} has unlinked visuals (in {transform.parent})");
			return;
		}
		visual.SetActive(stateToggle);
		UpdateSpecialBehavior(state, visual, stateToggle, aboveTemperature, scaleWithTemperature);
		if (stateToggle)
		{
			_isCurrentlyActiveAndRelevant = true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateSpecialBehavior(WorldSeasonState state, GameObject go, bool toggle, bool aboveTemperature, bool scaleWithTemperatureParam)
	{
		if ((toggle && _scaleWithTemperature) & scaleWithTemperatureParam)
		{
			ApplyTemperatureScale(go.transform, Scale01FromState(state, aboveTemperature));
		}
		TriggerParticles(go);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void TriggerParticles(GameObject a)
	{
		if (!Application.isPlaying)
		{
			a.GetComponentsInChildren<ParticleSystem>(includeInactive: true).ForEach((ParticleSystem b) =>
			{
				b.Play(withChildren: true);
			});
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float Scale01FromState(WorldSeasonState state, bool aboveTemperature)
	{
		return MathUtil.Saturate(aboveTemperature ? ((state.Temperature - _temperatureThreshold) / (1.00001f - _temperatureThreshold)) : ((_temperatureThreshold - state.Temperature) / _temperatureThreshold));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ApplyTemperatureScale(Transform target, float scale01)
	{
		if (!_originalScales.ContainsKey(target))
		{
			_originalScales[target] = target.localScale;
		}
		target.localScale = _originalScales[target] * scale01;
	}

	public bool IsState(ScenarioPossibleRoom.EBiomeState state)
	{
		return _biomeState == state;
	}

	public bool IsCurrentlyActiveAndRelevant()
	{
		if (!_isCurrentlyActiveAndRelevant)
		{
			return _override?.BiomeStateOverride == _biomeState;
		}
		return true;
	}

	public static IEnumerable<GameObject> GetWinterAssetsFrom(IEnumerable<BiomeStateBasedToggle> states)
	{
		return states.Where((BiomeStateBasedToggle a) => a.IsState(ScenarioPossibleRoom.EBiomeState.Natural)).SelectMany((BiomeStateBasedToggle a) => a._temperatureToggleWhenBelow);
	}

	public static IEnumerable<GameObject> GetSummerAssetsFrom(IEnumerable<BiomeStateBasedToggle> states)
	{
		return states.Where((BiomeStateBasedToggle a) => a.IsState(ScenarioPossibleRoom.EBiomeState.Natural)).SelectMany((BiomeStateBasedToggle a) => a._temperatureToggleWhenAbove);
	}
}
