using System;
using Apparance.Net;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Serialization;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
public class ProceduralStyle : MonoBehaviour
{
	[Tooltip("Specify seed for testing in editor only, overidden during gameplay and generation process")]
	[FormerlySerializedAs("Seed")]
	[SerializeField]
	private int _seed;

	[SerializeField]
	private bool saveThisSeed;

	[Header("Visual Style")]
	[Tooltip("Main visual biome setting")]
	public ScenarioPossibleRoom.EBiome Biome;

	[Tooltip("Additional biome qualifier")]
	public ScenarioPossibleRoom.ESubBiome SubBiome;

	[Tooltip("Main visual theme setting")]
	public ScenarioPossibleRoom.ETheme Theme;

	[Tooltip("Additional theme qualifier")]
	public ScenarioPossibleRoom.ESubTheme SubTheme;

	[Tooltip("Main visual tone setting")]
	public ScenarioPossibleRoom.ETone Tone;

	[Tooltip("Weather system")]
	public ScenarioPossibleRoom.EBiomeState BiomeState;

	private int m_OverrideSiblingIndex;

	[Tooltip("Objects of similar visual position, arrangement, or setup can be grouped to help generation system make aesthetic choices")]
	public int SymmetryGroup;

	[Header("Experimental")]
	[Tooltip("Enable regular re-generation of procedural elements with progressively randomised style")]
	public bool AnimateStyle;

	[Tooltip("Rate at which style changes and procedural elements are rebuilt")]
	public float AnimateStylePeriod = 1f;

	private bool PrevAnimateStyle;

	private DateTime AnimateStyleTimer;

	private bool m_Invalidate;

	[NonSerialized]
	private GloomUtility.ValueChangeTracker m_ValueTracking;

	public int Seed
	{
		get
		{
			return _seed;
		}
		set
		{
			if (!saveThisSeed)
			{
				_seed = value;
			}
		}
	}

	public int OverrideSiblingIndex => m_OverrideSiblingIndex;

	public bool HasBeenManuallyInitialised { get; private set; }

	private GloomUtility.ValueChangeTracker ValueTracker
	{
		get
		{
			if (m_ValueTracking == null)
			{
				m_ValueTracking = new GloomUtility.ValueChangeTracker();
			}
			return m_ValueTracking;
		}
	}

	private void Start()
	{
	}

	public ScenarioPossibleRoom.EBiome GetBiome()
	{
		if (Biome != ScenarioPossibleRoom.EBiome.Inherit)
		{
			return Biome;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetBiome();
		}
		return ScenarioPossibleRoom.EBiome.Default;
	}

	public ScenarioPossibleRoom.ESubBiome GetSubBiome()
	{
		if (SubBiome != ScenarioPossibleRoom.ESubBiome.Inherit)
		{
			return SubBiome;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetSubBiome();
		}
		return ScenarioPossibleRoom.ESubBiome.Default;
	}

	public ScenarioPossibleRoom.ETheme GetTheme()
	{
		if (Theme != ScenarioPossibleRoom.ETheme.Inherit)
		{
			return Theme;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetTheme();
		}
		return ScenarioPossibleRoom.ETheme.Default;
	}

	public ScenarioPossibleRoom.ESubTheme GetSubTheme()
	{
		if (SubTheme != ScenarioPossibleRoom.ESubTheme.Inherit)
		{
			return SubTheme;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetSubTheme();
		}
		return ScenarioPossibleRoom.ESubTheme.Default;
	}

	public ScenarioPossibleRoom.ETone GetTone()
	{
		if (Tone != ScenarioPossibleRoom.ETone.Inherit)
		{
			return Tone;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetTone();
		}
		return ScenarioPossibleRoom.ETone.Default;
	}

	public ScenarioPossibleRoom.EBiomeState GetBiomeState()
	{
		if (BiomeState != ScenarioPossibleRoom.EBiomeState.Inherit)
		{
			return BiomeState;
		}
		ProceduralStyle proceduralStyle = FindParentStyle();
		if (proceduralStyle != null)
		{
			return proceduralStyle.GetBiomeState();
		}
		return ScenarioPossibleRoom.EBiomeState.Default;
	}

	public void WriteStyleParameters(ParameterCollection style_struct, int scenario_seed = 0, int room_seed = 0, int object_seed = 0)
	{
		if (transform.parent == null)
		{
			Log.Warning("WriteStyleParameters called on object with no parent (ignore when prefab editing)");
			return;
		}
		FetchObjectHierarchyValues(out var objectIndex, out var objectCount);
		if (HasBeenManuallyInitialised && m_OverrideSiblingIndex != -1)
		{
			objectIndex = m_OverrideSiblingIndex;
		}
		if (!HasBeenManuallyInitialised && object_seed == 0)
		{
			object_seed = objectIndex;
		}
		style_struct.WriteInteger((int)GetBiome());
		style_struct.WriteInteger((int)GetSubBiome());
		style_struct.WriteInteger((int)GetTheme());
		style_struct.WriteInteger((int)GetSubTheme());
		style_struct.WriteInteger((int)GetTone());
		style_struct.WriteInteger(scenario_seed);
		style_struct.WriteInteger(room_seed);
		style_struct.WriteInteger(object_seed);
		style_struct.WriteInteger(objectIndex);
		style_struct.WriteInteger(objectCount);
		style_struct.WriteInteger(SymmetryGroup);
		style_struct.WriteBool(bool_value: false);
	}

	public void FetchObjectHierarchyValues(out int objectIndex, out int objectCount)
	{
		objectIndex = -1;
		objectCount = 0;
		ProceduralStyle component = GetComponent<ProceduralStyle>();
		if (transform?.parent == null)
		{
			objectIndex = 0;
			objectCount = 1;
			return;
		}
		foreach (ProceduralStyle item in transform.parent.gameObject.FindInImmediateChildren<ProceduralStyle>())
		{
			if (item == component)
			{
				objectIndex = objectCount;
			}
			objectCount++;
		}
	}

	public void Invalidate()
	{
		m_Invalidate = true;
	}

	private void LateUpdate()
	{
		if (m_Invalidate)
		{
			m_Invalidate = false;
			CheckChanges(invalidate_all: true);
		}
	}

	internal void CheckChanges(bool invalidate_all = false)
	{
		ProceduralScenario component = GetComponent<ProceduralScenario>();
		if (component != null && ValueTracker.CheckValue("Seed", Seed))
		{
			component.NotifySeedChange();
			invalidate_all = true;
		}
		bool flag = false;
		if (invalidate_all)
		{
			Rebuild();
		}
		else
		{
			if (CheckMajorChanges())
			{
				Rebuild();
				invalidate_all = true;
			}
			if (CheckPotentialChanges())
			{
				Rebuild();
				flag = true;
			}
			if (CheckMinorChanges())
			{
				Rebuild();
			}
		}
		if (invalidate_all | flag)
		{
			PropagateChanges(invalidate_all);
		}
	}

	private bool CheckMajorChanges()
	{
		return ValueTracker.CheckValue("Seed", Seed);
	}

	private bool CheckPotentialChanges()
	{
		if (!ValueTracker.CheckValue("Biome", (int)GetBiome()) && !ValueTracker.CheckValue("SubBiome", (int)GetSubBiome()) && !ValueTracker.CheckValue("Theme", (int)GetTheme()) && !ValueTracker.CheckValue("SubTheme", (int)GetSubTheme()) && !ValueTracker.CheckValue("Tone", (int)GetTone()))
		{
			return ValueTracker.CheckValue("BiomeState", (int)GetBiomeState());
		}
		return true;
	}

	private bool CheckMinorChanges()
	{
		return ValueTracker.CheckValue("SymmetryGroup", SymmetryGroup);
	}

	internal void Rebuild()
	{
		GetComponent<IProceduralContent>()?.RebuildContent();
	}

	internal void PropagateChanges(bool invalidate_all)
	{
		foreach (ProceduralStyle item in GloomUtility.FindAllDescendentComponents<ProceduralStyle>(this, inside_matching: false))
		{
			item.CheckChanges(invalidate_all);
		}
		ProceduralMapTile component = GetComponent<ProceduralMapTile>();
		if (!(component != null))
		{
			return;
		}
		ProceduralScenario scenario = component.GetScenario();
		if (!(scenario != null))
		{
			return;
		}
		int num = 0;
		foreach (GameObject doorway in scenario.Doorways)
		{
			ProceduralDoorway component2 = doorway.GetComponent<ProceduralDoorway>();
			if (component2 != null)
			{
				component2.CheckStyleChanges(invalidate_all);
				num++;
			}
		}
	}

	public void ForceValidate()
	{
		OnValidate();
	}

	private void OnValidate()
	{
		if (gameObject.scene.name == null || !gameObject.activeInHierarchy)
		{
			return;
		}
		if (GetComponent<ProceduralScenario>() != null)
		{
			if (Biome == ScenarioPossibleRoom.EBiome.Inherit)
			{
				Biome = ScenarioPossibleRoom.EBiome.Default;
			}
			if (SubBiome == ScenarioPossibleRoom.ESubBiome.Inherit)
			{
				SubBiome = ScenarioPossibleRoom.ESubBiome.Default;
			}
			if (Theme == ScenarioPossibleRoom.ETheme.Inherit)
			{
				Theme = ScenarioPossibleRoom.ETheme.Default;
			}
			if (SubTheme == ScenarioPossibleRoom.ESubTheme.Inherit)
			{
				SubTheme = ScenarioPossibleRoom.ESubTheme.Default;
			}
			if (Tone == ScenarioPossibleRoom.ETone.Inherit)
			{
				Tone = ScenarioPossibleRoom.ETone.Default;
			}
			if (BiomeState == ScenarioPossibleRoom.EBiomeState.Inherit)
			{
				BiomeState = ScenarioPossibleRoom.EBiomeState.Default;
			}
		}
		CheckChanges();
	}

	private ProceduralStyle FindParentStyle()
	{
		ProceduralDoorway component = GetComponent<ProceduralDoorway>();
		if (component != null)
		{
			bool isFront = GetComponent<ProceduralStyle>() == this;
			return component.FindParentStyle(isFront);
		}
		ProceduralBase component2 = GetComponent<ProceduralBase>();
		if (component2 != null)
		{
			return component2.GetParentStyle();
		}
		return null;
	}

	public void InitialiseWithOverride(CApparanceOverrideDetails overrideDetails)
	{
		Seed = overrideDetails.OverrideSeed;
		Biome = overrideDetails.OverrideBiome;
		SubBiome = overrideDetails.OverrideSubBiome;
		Theme = overrideDetails.OverrideTheme;
		SubTheme = overrideDetails.OverrideSubTheme;
		Tone = overrideDetails.OverrideTone;
		BiomeState = overrideDetails.OverrideBiomeState;
		m_OverrideSiblingIndex = overrideDetails.OverrideSiblingIndex;
		HasBeenManuallyInitialised = true;
		Invalidate();
	}
}
