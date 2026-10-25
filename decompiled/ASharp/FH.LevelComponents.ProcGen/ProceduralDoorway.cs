using System.Collections.Generic;
using System.Linq;
using Apparance.Net;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
[ExecuteInEditMode]
public class ProceduralDoorway : ProceduralBase, IProceduralContent, IRoomVisibility
{
	private ParameterCollection Parameters;

	private HashSet<ProceduralMapTile> _connected = new HashSet<ProceduralMapTile>();

	private ProceduralTile m_proceduralTileCached;

	private ProceduralMapTile.Visibility? _cachedVisibility;

	private const string GENERATED_CONTENT_NAME = "Generated Content";

	private ProceduralTile _proceduralTileCached
	{
		get
		{
			if (!m_proceduralTileCached)
			{
				return m_proceduralTileCached = GetComponent<ProceduralTile>();
			}
			return m_proceduralTileCached;
		}
	}

	public void SetVisibility(RenderingLayer mergedLayer)
	{
		ApplyVisibility(mergedLayer);
	}

	public ProceduralMapTile.Visibility GetVisibility()
	{
		if (_cachedVisibility.HasValue)
		{
			return _cachedVisibility.Value;
		}
		ProceduralMapTile frontMapTile = GetFrontMapTile();
		bool flag = (bool)frontMapTile && frontMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed;
		ProceduralMapTile backMapTile = GetBackMapTile();
		bool flag2 = (bool)backMapTile && backMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed;
		_cachedVisibility = (((flag | flag2) || (!frontMapTile && !backMapTile)) ? ProceduralMapTile.Visibility.Revealed : ProceduralMapTile.Visibility.Unrevealed);
		return _cachedVisibility.Value;
	}

	private void LateUpdate()
	{
		_cachedVisibility = null;
	}

	private void ApplyVisibility(RenderingLayer mergedLayer)
	{
		ProceduralMapTile frontMapTile = GetFrontMapTile();
		bool flag = (bool)frontMapTile && frontMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed;
		ProceduralMapTile backMapTile = GetBackMapTile();
		bool flag2 = (bool)backMapTile && backMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed;
		bool enable = (flag | flag2) || (!frontMapTile && !backMapTile);
		ProceduralMapTile.EnableLightsAndEffects(base.gameObject, enable);
		GameObject gameObject = null;
		UnityGameEditorDoorProp unityGameEditorDoorProp = base.gameObject.FindInImmediateChildren<UnityGameEditorDoorProp>().FirstOrDefault();
		if (unityGameEditorDoorProp != null)
		{
			gameObject = unityGameEditorDoorProp.gameObject;
		}
		else
		{
			UnityGameEditorDoorProp component = base.gameObject.transform.parent.gameObject.GetComponent<UnityGameEditorDoorProp>();
			if (component != null)
			{
				gameObject = component.gameObject;
			}
		}
		if (gameObject != null)
		{
			ProceduralMapTile.EnableLightsAndEffects(gameObject, enable);
		}
		Transform transform = base.transform.Find("Generated Content");
		if ((bool)transform)
		{
			foreach (Transform item in transform)
			{
				DoorFrameVisibility component2 = item.GetComponent<DoorFrameVisibility>();
				if (component2 != null)
				{
					component2.UpdateVisibility(flag, flag2);
					break;
				}
			}
		}
		List<Renderer> list = GetComponentsInChildren<Renderer>().ToList();
		if ((bool)base.transform.parent && (bool)base.transform.parent.GetComponent<PropParent>())
		{
			Transform transform2 = base.transform.parent.Find("Generated Content");
			if ((bool)transform2)
			{
				list.AddRange(transform2.GetComponentsInChildren<Renderer>(includeInactive: true));
			}
		}
		foreach (Renderer item2 in list)
		{
			item2.SetLayer(mergedLayer);
		}
	}

	private void Start()
	{
		Rebuild();
		Apply(force_build: false);
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		int scenarioSeed = GetScenarioSeed();
		ProceduralStyle frontStyle = GetFrontStyle();
		int num = ((frontStyle != null) ? frontStyle.Seed : scenarioSeed);
		ParameterCollection style_struct = Parameters.WriteListBegin(3);
		if (FindParentStyle(isFront: true) != null)
		{
			frontStyle.WriteStyleParameters(style_struct, scenarioSeed, num, num);
		}
		Parameters.WriteListEnd();
		ProceduralStyle backStyle = GetBackStyle();
		int num2 = ((backStyle != null) ? backStyle.Seed : scenarioSeed);
		ParameterCollection style_struct2 = Parameters.WriteListBegin(4);
		if (FindParentStyle(isFront: false) != null)
		{
			backStyle.WriteStyleParameters(style_struct2, scenarioSeed, num2, num2);
		}
		Parameters.WriteListEnd();
		UnityEngine.Vector3 v = FindWallSize(isFront: true);
		Parameters.WriteVector3(Conversion.AVfromUV(v), 5);
		UnityEngine.Vector3 v2 = FindWallSize(isFront: false);
		Parameters.WriteVector3(Conversion.AVfromUV(v2), 6);
		Parameters.EndWrite();
	}

	private void Apply(bool force_build = true)
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		component.PartialParameterOverride = Parameters;
		component.IsPopulated = false;
		component.NotifyPropertyChanged();
		component.IsPopulated = true;
		component.NotifyPropertyChanged();
		apparanceDebug.RegisterEntity(component);
	}

	void IProceduralContent.RebuildContent()
	{
		Rebuild();
		Apply();
	}

	internal void CheckStyleChanges(bool invalidate_all)
	{
		GetFrontStyle()?.CheckChanges(invalidate_all);
		GetBackStyle()?.CheckChanges(invalidate_all);
	}

	private ProceduralStyle GetFrontStyle()
	{
		ProceduralStyle[] components = GetComponents<ProceduralStyle>();
		if (components.Length >= 1)
		{
			return components[0];
		}
		Log.Error("Doorway '" + name + "' (at " + transform.position.ToString() + ") has no ProceduralStyle components, doorways expect two.");
		return null;
	}

	private ProceduralStyle GetBackStyle()
	{
		ProceduralStyle[] components = GetComponents<ProceduralStyle>();
		if (components.Length >= 2)
		{
			return components[1];
		}
		Log.Error("Doorway '" + name + "' (at " + transform.position.ToString() + ") has only one ProceduralStyle component, doorways expect two.");
		return null;
	}

	private UnityEngine.Vector3 GetFrontPosition()
	{
		GameObject gameObject = base.gameObject.FindInImmediateChildren("outFront");
		if (gameObject != null)
		{
			return gameObject.transform.position;
		}
		return transform.position;
	}

	private UnityEngine.Vector3 GetBackPosition()
	{
		GameObject gameObject = base.gameObject.FindInImmediateChildren("outBack");
		if (gameObject != null)
		{
			return gameObject.transform.position;
		}
		return transform.position;
	}

	private ProceduralMapTile FindMapTileByPosition(UnityEngine.Vector3 position, Color diags_colour, out UnityEngine.Vector3 wall_space)
	{
		ProceduralTile proceduralTileCached = _proceduralTileCached;
		if (proceduralTileCached != null)
		{
			return proceduralTileCached.FindMapTileByPosition(position, out wall_space);
		}
		wall_space = UnityEngine.Vector3.zero;
		return null;
	}

	public ProceduralStyle FindParentStyle(bool isFront)
	{
		UnityEngine.Vector3 position = ((!isFront) ? GetBackPosition() : GetFrontPosition());
		ProceduralMapTile proceduralMapTile = FindMapTileByPosition(position, Color.black, out var _);
		if (!proceduralMapTile)
		{
			return null;
		}
		return proceduralMapTile.GetComponent<ProceduralStyle>();
	}

	public ProceduralMapTile GetFrontMapTile()
	{
		UnityEngine.Vector3 wall_space;
		return FindMapTileByPosition(GetFrontPosition(), Color.blue, out wall_space);
	}

	public ProceduralMapTile GetBackMapTile()
	{
		UnityEngine.Vector3 wall_space;
		return FindMapTileByPosition(GetBackPosition(), Color.blue, out wall_space);
	}

	private UnityEngine.Vector3 FindWallSize(bool isFront)
	{
		UnityEngine.Vector3 position = ((!isFront) ? GetBackPosition() : GetFrontPosition());
		FindMapTileByPosition(position, Color.black, out var wall_space);
		return wall_space;
	}
}
