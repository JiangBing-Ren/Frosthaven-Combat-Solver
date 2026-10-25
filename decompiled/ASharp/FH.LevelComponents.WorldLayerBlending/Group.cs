using System;
using System.Collections.Generic;
using FH.LevelComponents.TopDownProjection;
using UnityEngine;

namespace FH.LevelComponents.WorldLayerBlending;

public class Group : ICelContent, IDisposable
{
	private List<(Renderer renderer, int submeshCount)> _targets;

	private Cel HeightCel;

	private Cel AlbedoCel;

	private Cel NormalCel;

	private const float MIN_CLIP_HEIGHT = -10f;

	private static readonly int _heightTextureProp = Shader.PropertyToID("_WorldLevel_HeightCapture");

	private static readonly int _albTextureProp = Shader.PropertyToID("_WorldLevel_AlbCapture");

	private static readonly int _nrmTextureProp = Shader.PropertyToID("_WorldLevel_NrmCapture");

	private static readonly int _viewAreaSizeProp = Shader.PropertyToID("_WorldLevel_ViewAreaSize");

	public RenderTexture[] Textures => new RenderTexture[3] { AlbedoCel.RenderTextureTarget, HeightCel.RenderTextureTarget, NormalCel.RenderTextureTarget };

	public Cel Albedo => AlbedoCel;

	public Cel Height => HeightCel;

	public Cel Normal => NormalCel;

	public Group(string name, List<MeshRenderer> renderers, MultiplaneSystem multiplaneSystem, Bounds levelBounds, int resolution, Material heightMaterial, Material normalMaterial)
	{
		_targets = new List<(Renderer, int)>(renderers.Count);
		foreach (MeshRenderer renderer in renderers)
		{
			if (!renderer.TryGetComponent<MeshFilter>(out var component))
			{
				Debug.LogError("There exists a MeshRenderer without a corresponding MeshFilter in " + renderer.gameObject.name);
			}
			else if (component.sharedMesh == null)
			{
				Debug.LogError("There exists a MeshFilter with no assigned mesh in " + component.gameObject.name);
			}
			else
			{
				_targets.Add((renderer, renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount));
			}
		}
		HeightCel = multiplaneSystem.NewCel(new CelParams
		{
			LevelBounds = levelBounds,
			Resolution = Vector2Int.one * (resolution / 4),
			TextureFormat = RenderTextureFormat.RHalf,
			DepthBufferPrecision = 16,
			CustomOverrideShader = null,
			Content = this,
			MinClipHeight = -10f,
			CullingMask = 0,
			ReplacementMaterial = heightMaterial
		});
		AlbedoCel = multiplaneSystem.NewCel(new CelParams
		{
			LevelBounds = levelBounds,
			Resolution = Vector2Int.one * resolution,
			TextureFormat = RenderTextureFormat.ARGBHalf,
			DepthBufferPrecision = 16,
			CustomOverrideShader = null,
			Content = this,
			MinClipHeight = -10f,
			CullingMask = 0
		});
		NormalCel = multiplaneSystem.NewCel(new CelParams
		{
			LevelBounds = levelBounds,
			Resolution = Vector2Int.one * resolution,
			TextureFormat = RenderTextureFormat.ARGBHalf,
			DepthBufferPrecision = 16,
			CustomOverrideShader = null,
			Content = this,
			MinClipHeight = -10f,
			CullingMask = 0,
			ReplacementMaterial = normalMaterial
		});
		HeightCel.RenderTextureTarget.name = "Group_" + name + "_Height";
		AlbedoCel.RenderTextureTarget.name = "Group_" + name + "_Albedo";
		NormalCel.RenderTextureTarget.name = "Group_" + name + "_Normal";
	}

	public IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents()
	{
		return _targets;
	}

	public void Update()
	{
		HeightCel.RequestRefresh();
		AlbedoCel.RequestRefresh();
		NormalCel.RequestRefresh();
	}

	public void Dispose()
	{
		HeightCel?.Dispose();
		AlbedoCel?.Dispose();
		NormalCel?.Dispose();
	}
}
