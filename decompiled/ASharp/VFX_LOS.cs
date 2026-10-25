using System;
using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using SETUtil.Extend;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

public class VFX_LOS : MonoBehaviour, ICelContent
{
	[SerializeField]
	private Renderer[] _maskedRenderers;

	private Cel _LOS_Map;

	private Vector3 _requestedPosition;

	private float _requestedRadius = 3f;

	public const string ENABLE_PARAM = "_VFXLOS_Enabled";

	private const string MASK_PARAM = "_VFXLOS_Mask";

	private const string AREA_SIZE_PARAM = "_VFXLOS_AreaSize";

	private const string AREA_PIVOT_PARAM = "_VFXLOS_AreaPivot";

	private List<(Renderer renderer, int submeshCount)> Contents = new List<(Renderer, int)>();

	private bool _currentlyCalculatingLOS;

	private bool IsInitialized => _LOS_Map != null;

	private int HexLayer => LayerMask.NameToLayer("Hex");

	public Renderer[] MaskedRenderers => _maskedRenderers;

	private void Init()
	{
		if (_maskedRenderers == null || _maskedRenderers.Length < 1)
		{
			return;
		}
		VFX_LOS_SettingsDef vFX_LOS_SettingsDef = Resources.Load<VFX_LOS_SettingsDef>("Settings/VFX_LOS_SettingsDef");
		Bounds visualsBounds = ProceduralScenario.GetVisualsBounds(from x in EnvironmentLoader.Instance.GetMaps()
			select x.GetComponent<ProceduralMapTile>());
		_LOS_Map = MultiplaneSystem.GetInstance().NewCel(new CelParams
		{
			LevelBounds = visualsBounds,
			Resolution = Vector2Int.one * vFX_LOS_SettingsDef.MaskResolution,
			TextureFormat = RenderTextureFormat.R8,
			DepthBufferPrecision = vFX_LOS_SettingsDef.MaskDepthBufferPrecision,
			ReplacementMaterial = vFX_LOS_SettingsDef.MultiplaneRenderingMaterial,
			PostProcessing = vFX_LOS_SettingsDef.MultiplaneRenderingPostProcessing,
			Content = this,
			MinClipHeight = -1f,
			CullingMask = HexLayer
		});
		Vector2 vector = Vector2.one * _LOS_Map.CelParams.OrthographicAreaSize;
		Vector2 vector2 = new Vector2(visualsBounds.center.x, visualsBounds.center.z);
		Renderer[] maskedRenderers = _maskedRenderers;
		for (int num = 0; num < maskedRenderers.Length; num++)
		{
			Material[] clones = maskedRenderers[num].GetOrAdd<MaterialModification>().GetClones();
			foreach (Material obj in clones)
			{
				obj.SetFloat("_VFXLOS_Enabled", 1f);
				obj.SetTexture("_VFXLOS_Mask", _LOS_Map.RenderTextureTarget);
				obj.SetVector("_VFXLOS_AreaSize", vector);
				obj.SetVector("_VFXLOS_AreaPivot", vector2);
			}
		}
	}

	private List<TileBehaviour> GetAllTilesInRange(Vector3 center, float radius, out TileBehaviour closest)
	{
		Collider[] array = Physics.OverlapSphere(center, radius);
		List<TileBehaviour> list = new List<TileBehaviour>(array.Length);
		closest = null;
		float num = float.PositiveInfinity;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].TryGetComponent<TileBehaviour>(out var component))
			{
				list.Add(component);
				float num2 = Vector3.SqrMagnitude(component.transform.position - center);
				if (num2 <= num)
				{
					num = num2;
					closest = component;
				}
			}
		}
		return list;
	}

	public IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents()
	{
		return Contents;
	}

	public void RequestLOS(Vector3 position, float radius)
	{
		_requestedPosition = position;
		_requestedRadius = radius;
		if (_currentlyCalculatingLOS)
		{
			return;
		}
		if (!IsInitialized)
		{
			Init();
		}
		_currentlyCalculatingLOS = true;
		List<TileBehaviour> tiles = GetAllTilesInRange(position, radius, out var startTile);
		this.Synchronize(() =>
		{
			List<TileBehaviour> list = new List<TileBehaviour>();
			try
			{
				foreach (TileBehaviour item in tiles)
				{
					if (CActor.HaveLOS(startTile.m_ClientTile.m_Tile, item.m_ClientTile.m_Tile))
					{
						list.Add(item);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return list;
		}, (List<TileBehaviour> visibleTiles) =>
		{
			if ((bool)this)
			{
				Contents = new List<(Renderer, int)>(visibleTiles.Count);
				foreach (TileBehaviour visibleTile in visibleTiles)
				{
					if (visibleTile.TryGetComponent<MeshRenderer>(out var component) && visibleTile.TryGetComponent<MeshFilter>(out var component2))
					{
						Contents.Add((component, component2.sharedMesh.subMeshCount));
					}
				}
				_LOS_Map.RequestRefresh();
				_currentlyCalculatingLOS = false;
			}
		});
	}

	public void RefreshLOS()
	{
		RequestLOS(_requestedPosition, _requestedRadius);
	}

	[ContextMenu("Find child renderers")]
	private void FindChildRenderers()
	{
		_maskedRenderers = GetComponentsInChildren<Renderer>();
	}

	private void OnDestroy()
	{
		_LOS_Map?.Dispose();
	}
}
