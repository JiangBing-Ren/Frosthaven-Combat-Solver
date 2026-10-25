using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.TopDownProjection;
using FH.PostProcessing;
using SETUtil;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class SurroundingWaterOverride : MonoBehaviour, ISurroundingOverride
{
	[HideInInspector]
	public Material[] WaterMaterialPerQualitySetting;

	public Material WaterMaterialFallback;

	public Material OceanFloorMaterial;

	[Header("Padding")]
	public float PaddingSize = 100f;

	public int PaddingSubdivisions = 5;

	[Header("Scene Capture")]
	public Material MultiplaneRenderingMaterial;

	public Material MultiplaneRenderingPostProcessing;

	public int Iterations = 1;

	public float Radius = 1f;

	public int ShoreMaskResolution = 1024;

	public float CelDepth = 0.1f;

	public float CelVerticalOffset;

	private Cel MaskCel;

	private Material _modifiedWaterMaterial;

	private Material _PP_Horizontal;

	private Material _PP_Vertical;

	private const string MASK_PARAM = "_ShoreMask";

	private const string MASK_AREASIZE_PARAM = "_ShoreMask_AreaSize";

	private const string MASK_PIVOT_PARAM = "_ShoreMask_Pivot";

	public float GetDecay(float defaultDecay)
	{
		return defaultDecay;
	}

	public float GetHeightMax(float defaultHeightMax)
	{
		return defaultHeightMax;
	}

	public float GetHeightMin(float defaultHeightMin)
	{
		return defaultHeightMin;
	}

	public Material GetSolidMaterial(Material defaultSolidMaterial)
	{
		return OceanFloorMaterial;
	}

	public Material GetTransparentMaterial(Material defaultTransparentMaterial)
	{
		SceneUtil.SmartDestroy(_modifiedWaterMaterial);
		int qualityLevel = QualitySettings.GetQualityLevel();
		if (qualityLevel < 0 || qualityLevel >= WaterMaterialPerQualitySetting.Length)
		{
			_modifiedWaterMaterial = new Material(WaterMaterialFallback);
		}
		else
		{
			_modifiedWaterMaterial = new Material(WaterMaterialPerQualitySetting[qualityLevel]);
		}
		_modifiedWaterMaterial.name += " (Modified by SurroundingWaterOverride)";
		if (Application.isPlaying)
		{
			StartCoroutine(RequestMaskUpdate());
		}
		return _modifiedWaterMaterial;
	}

	public void ModifyMeshData(ref ProceduralMistSurrounding.CMeshData data)
	{
		List<Vector3> list = data.Vertices.ToList();
		List<int> triangles = data.Triangles;
		List<Color> list2 = data.Colors.ToList();
		ProceduralMistTools.FindPerimeter(list, triangles, out var outerLoop);
		foreach (int item2 in outerLoop)
		{
			Color value = list2[item2];
			value.a = 0f;
			list2[item2] = value;
		}
		Color item = new Color(0f, 0f, 0f, 0f);
		float padding = PaddingSize / (float)Mathf.Max(PaddingSubdivisions, 1);
		for (int i = 0; i < PaddingSubdivisions; i++)
		{
			ProceduralMistTools.Extrude(list, triangles, outerLoop, padding, out var extrudedVertices, out var extrudedTriangles, out var newVertices);
			list = extrudedVertices;
			triangles = extrudedTriangles;
			outerLoop = newVertices;
			for (int j = 0; j < newVertices.Count; j++)
			{
				list2.Add(item);
			}
		}
		data.Vertices = list.ToArray();
		data.Triangles = triangles;
		data.Colors = list2.ToArray();
	}

	private IEnumerator RequestMaskUpdate()
	{
		MultiplaneSystem multiplaneSystem = MultiplaneSystem.GetInstance();
		while (multiplaneSystem == null)
		{
			multiplaneSystem = MultiplaneSystem.GetInstance();
			yield return new WaitForSeconds(0.5f);
		}
		UpdateMask(multiplaneSystem);
	}

	private void UpdateMask(MultiplaneSystem multiplaneSystem)
	{
		if (MaskCel == null)
		{
			Bounds visualsBounds = ProceduralScenario.GetVisualsBounds((from x in EnvironmentLoader.Instance.GetMaps()
				select x.GetComponent<ProceduralMapTile>()).ToArray());
			Vector3 extents = visualsBounds.extents;
			extents.y = CelDepth - CelVerticalOffset;
			visualsBounds.extents = extents;
			MaskCel = multiplaneSystem.NewCel(new CelParams
			{
				LevelBounds = visualsBounds,
				Resolution = Vector2Int.one * ShoreMaskResolution,
				TextureFormat = RenderTextureFormat.R8,
				DepthBufferPrecision = 16,
				ReplacementMaterial = MultiplaneRenderingMaterial,
				PostProcessing = GeneratePPStack(),
				Content = new WaterCelContent(),
				MinClipHeight = 0f - CelDepth,
				CullingMask = (-1 & LayerMask.NameToLayer("Background"))
			});
		}
		MaskCel.RequestRefresh();
		Vector2 vector = Vector2.one * MaskCel.CelParams.OrthographicAreaSize;
		Vector2 vector2 = new Vector2(MaskCel.CelParams.LevelBounds.center.x, MaskCel.CelParams.LevelBounds.center.z);
		_modifiedWaterMaterial.SetTexture("_ShoreMask", MaskCel.RenderTextureTarget);
		_modifiedWaterMaterial.SetVector("_ShoreMask_AreaSize", vector);
		_modifiedWaterMaterial.SetVector("_ShoreMask_Pivot", vector2);
	}

	private List<Material> GeneratePPStack()
	{
		if ((object)_PP_Horizontal == null)
		{
			_PP_Horizontal = new Material(MultiplaneRenderingPostProcessing);
		}
		if ((object)_PP_Vertical == null)
		{
			_PP_Vertical = new Material(MultiplaneRenderingPostProcessing);
		}
		_PP_Horizontal.SetVector("_Direction", Vector2.right);
		_PP_Vertical.SetVector("_Direction", Vector2.up);
		_PP_Horizontal.SetFloat("_Blur", Radius);
		_PP_Vertical.SetFloat("_Blur", Radius);
		List<Material> list = new List<Material>(Iterations * 2);
		for (int i = 0; i < Iterations; i++)
		{
			list.Add(_PP_Horizontal);
			list.Add(_PP_Vertical);
		}
		return list;
	}

	private void OnDestroy()
	{
		SceneUtil.SmartDestroy(_modifiedWaterMaterial);
		SceneUtil.SmartDestroy(_PP_Horizontal);
		SceneUtil.SmartDestroy(_PP_Vertical);
	}
}
