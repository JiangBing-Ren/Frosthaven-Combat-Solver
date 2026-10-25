using System.Collections.Generic;
using UnityEngine;

namespace FH.LevelComponents.TopDownProjection;

public struct CelParams
{
	public const float AREA_BORDER = 10f;

	public Vector2Int Resolution;

	public RenderTextureFormat TextureFormat;

	public Shader CustomOverrideShader;

	public List<Material> PostProcessing;

	public int DepthBufferPrecision;

	public Bounds LevelBounds;

	public ICelContent Content;

	public float MinClipHeight;

	public LayerMask CullingMask;

	public Material ReplacementMaterial;

	public float OrthographicAreaSize => Mathf.Max(LevelBounds.size.x, LevelBounds.size.z) + 10f;
}
