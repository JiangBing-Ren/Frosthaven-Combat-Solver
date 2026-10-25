using System.Collections;
using System.Collections.Generic;
using FH.LevelComponents.TopDownProjection;
using Libs.Utils;
using SETUtil;
using UnityEngine;
using UnityEngine.Rendering;

namespace FH.LevelComponents.WorldLayerBlending;

[ExecuteAlways]
public class KronosWorldLevelComp : MonoBehaviour
{
	public GameObject World;

	public bool CalculateWorldBoundsAutomatically = true;

	public Bounds WorldBounds;

	public float WorldBoundsSize;

	public int MaxGroupAmount = 25;

	public Material heightMaterial;

	public Material normalMaterial;

	public float BlendFalloff = 2f;

	public float BlendMaskOffset = 0.15f;

	public float BlendMaskStrength = 20f;

	public int Resolution = 2048;

	public ComputeShader BlendCompute;

	public RenderTexture CombinedAlbedo;

	public RenderTexture CombinedHeight;

	public RenderTexture CombinedNormal;

	public RenderTexture CombinedBlend;

	public Texture2D BlendedAlbedoTex;

	public Texture2D BlendedNormalTex;

	public Texture2D BlendMaskTex;

	public bool ApplyTexturesAutomatically = true;

	public static readonly int ShaderBlendStrength = Shader.PropertyToID("_WorldBlend_Multiplier");

	private static readonly int ShaderWorldAlbedo = Shader.PropertyToID("_WorldBlend_Albedo");

	private static readonly int ShaderWorldNormal = Shader.PropertyToID("_WorldBlend_Normal");

	private static readonly int ShaderWorldBlendMask = Shader.PropertyToID("_WorldBlend_Mask");

	private static readonly int ShaderWorldBoundsSize = Shader.PropertyToID("_WorldBlend_ViewAreaSize");

	private static readonly int ShaderWorldBoundsOffset = Shader.PropertyToID("_WorldBlend_ViewAreaOffset");

	private static readonly int ComputeBlendFalloff = Shader.PropertyToID("BlendFalloff");

	private static readonly int ComputeBlendMaskOffset = Shader.PropertyToID("BlendMaskOffset");

	private static readonly int ComputeBlendMaskStrength = Shader.PropertyToID("BlendMaskStrength");

	private static readonly int ComputeResultAlbedo = Shader.PropertyToID("Result_Albedo");

	private static readonly int ComputeResultHeight = Shader.PropertyToID("Result_Height");

	private static readonly int ComputeResultNormal = Shader.PropertyToID("Result_Normal");

	private static readonly int ComputeResultBlend = Shader.PropertyToID("Result_Blend");

	private static readonly int ComputeGroupAlbedo = Shader.PropertyToID("Group_Albedo");

	private static readonly int ComputeGroupHeight = Shader.PropertyToID("Group_Height");

	private static readonly int ComputeGroupNormal = Shader.PropertyToID("Group_Normal");

	private static readonly string[] ValidShaders = new string[1] { "Shader Graphs/URP_WorldLayer_Standard" };

	private const string ISOLATED_FLAG = "_Isolated";

	public bool IsWaitingForRender { get; private set; }

	public bool IsProcessing { get; private set; }

	public float ProcessingProgress { get; private set; }

	public bool HasRenderedTextures
	{
		get
		{
			if (!CombinedAlbedo && !CombinedHeight && !CombinedNormal)
			{
				return CombinedBlend;
			}
			return true;
		}
	}

	public bool HasAllRenderedTextures
	{
		get
		{
			if ((bool)CombinedAlbedo && (bool)CombinedHeight && (bool)CombinedNormal)
			{
				return CombinedBlend;
			}
			return false;
		}
	}

	public bool HasAllTextureAssets
	{
		get
		{
			if ((bool)BlendedAlbedoTex && (bool)BlendedNormalTex)
			{
				return BlendMaskTex;
			}
			return false;
		}
	}

	private void Start()
	{
		if (ApplyTexturesAutomatically)
		{
			ApplyBlendStrength(ApplyTexturesToShaders() ? 1f : 0f);
		}
	}

	private static RenderTexture CreateTex(string name, int resolution)
	{
		RenderTexture renderTexture = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGBHalf);
		renderTexture.enableRandomWrite = true;
		renderTexture.name = name;
		renderTexture.Create();
		return renderTexture;
	}

	private void InitializeCombinedTextures()
	{
		CombinedAlbedo = CreateTex("CombinedAlbedo", Resolution);
		CombinedHeight = CreateTex("CombinedHeight", Resolution / 4);
		CombinedNormal = CreateTex("CombinedNormal", Resolution);
		CombinedBlend = CreateTex("CombinedBlend", Resolution / 4);
		BlendCompute.SetTexture(1, "Result_Albedo", CombinedAlbedo);
		BlendCompute.SetTexture(1, "Result_Height", CombinedHeight);
		BlendCompute.SetTexture(1, "Result_Normal", CombinedNormal);
		BlendCompute.SetTexture(1, "Result_Blend", CombinedBlend);
		BlendCompute.Dispatch(1, Resolution / 8, Resolution / 8, 1);
	}

	[ContextMenu("Blend")]
	public void Blend()
	{
		if (BlendCompute == null)
		{
			Debug.LogError("No World Blend compute shader has been assigned.");
			return;
		}
		ClearAllRenderTextures();
		StartCoroutine(BlendCo());
	}

	private IEnumerator BlendCo()
	{
		IsProcessing = true;
		ProcessingProgress = 0f;
		ApplyBlendStrength(0f);
		MeshRenderer[] componentsInChildren = World.GetComponentsInChildren<MeshRenderer>();
		if (FilterRenderersWithWrongShader(componentsInChildren, out var invalidRenderers, out var invalidMaterials))
		{
			string text = "Found meshes that use invalid shaders for blending, these will not work as expected.\n" + $"In total, {invalidMaterials.Count} materials, which affect {invalidRenderers.Count} renderers.";
			text += "\n(!) Invalid Materials are:\n";
			foreach (Material item in invalidMaterials)
			{
				text = text + item.name + " (" + item.shader.name + ")\n";
			}
			text += "\n---\n(!) Affected Mesh Renderers are:\n";
			foreach (MeshRenderer item2 in invalidRenderers)
			{
				text = text + item2.name + "\n";
			}
			Debug.LogError(text);
		}
		List<List<MeshRenderer>> groups = GroupTargets(componentsInChildren);
		DebugTargetGroups(groups);
		if (CalculateWorldBoundsAutomatically)
		{
			WorldBounds = CalculateGlobalBounds(componentsInChildren, out WorldBoundsSize);
		}
		MultiplaneSystem multiplaneSystem = MultiplaneSystem.GetInstance();
		InitializeCombinedTextures();
		BlendCompute.SetFloat(ComputeBlendFalloff, BlendFalloff);
		BlendCompute.SetFloat(ComputeBlendMaskOffset, BlendMaskOffset);
		BlendCompute.SetFloat(ComputeBlendMaskStrength, BlendMaskStrength);
		BlendCompute.SetTexture(0, ComputeResultAlbedo, CombinedAlbedo);
		BlendCompute.SetTexture(0, ComputeResultHeight, CombinedHeight);
		BlendCompute.SetTexture(0, ComputeResultNormal, CombinedNormal);
		BlendCompute.SetTexture(0, ComputeResultBlend, CombinedBlend);
		for (int groupId = 0; groupId < groups.Count; groupId++)
		{
			ProcessingProgress = ((float)groupId + 1f) / (float)groups.Count;
			List<MeshRenderer> renderers = groups[groupId];
			Group group = new Group(groupId.ToString(), renderers, multiplaneSystem, WorldBounds, Resolution, heightMaterial, normalMaterial);
			IsWaitingForRender = true;
			RenderPipelineManager.endCameraRendering += OnCameraRendered;
			group.Update();
			yield return new WaitWhile(() => IsWaitingForRender);
			RenderPipelineManager.endCameraRendering -= OnCameraRendered;
			BlendCompute.SetTexture(0, ComputeGroupAlbedo, group.Albedo.RenderTextureTarget);
			BlendCompute.SetTexture(0, ComputeGroupHeight, group.Height.RenderTextureTarget);
			BlendCompute.SetTexture(0, ComputeGroupNormal, group.Normal.RenderTextureTarget);
			BlendCompute.Dispatch(0, Resolution / 8, Resolution / 8, 1);
			group.Dispose();
			Debug.Log($"Rendered group: {groupId}");
		}
		IsProcessing = false;
		if (ApplyTexturesAutomatically)
		{
			ApplyTexturesToShaders();
		}
		ApplyBlendStrength(1f);
	}

	public bool ApplyTexturesToShaders()
	{
		if (HasAllTextureAssets)
		{
			Shader.SetGlobalTexture(ShaderWorldAlbedo, BlendedAlbedoTex);
			Shader.SetGlobalTexture(ShaderWorldNormal, BlendedNormalTex);
			Shader.SetGlobalTexture(ShaderWorldBlendMask, BlendMaskTex);
		}
		else
		{
			if (!HasAllRenderedTextures)
			{
				Debug.LogError("[" + GetType().Name + "] There were no textures to assign to the shaders " + name);
				return false;
			}
			Shader.SetGlobalTexture(ShaderWorldAlbedo, CombinedAlbedo);
			Shader.SetGlobalTexture(ShaderWorldNormal, CombinedNormal);
			Shader.SetGlobalTexture(ShaderWorldBlendMask, CombinedBlend);
		}
		Shader.SetGlobalFloat(ShaderWorldBoundsSize, WorldBoundsSize + 10f);
		Shader.SetGlobalVector(ShaderWorldBoundsOffset, new Vector2(WorldBounds.center.x, WorldBounds.center.z));
		return true;
	}

	private void OnCameraRendered(ScriptableRenderContext arg1, Camera arg2)
	{
		IsWaitingForRender = false;
	}

	private Bounds CalculateGlobalBounds(IEnumerable<MeshRenderer> renderers, out float size)
	{
		Bounds result = default;
		foreach (MeshRenderer renderer in renderers)
		{
			result.Encapsulate(renderer.bounds);
		}
		size = Mathf.Max(result.size.x, result.size.z);
		result.size = new Vector3(size, result.size.y, size);
		return result;
	}

	[ContextMenu("Clean Up")]
	public void ClearAllRenderTextures()
	{
		SceneUtil.SmartDestroy(CombinedAlbedo);
		SceneUtil.SmartDestroy(CombinedHeight);
		SceneUtil.SmartDestroy(CombinedNormal);
		SceneUtil.SmartDestroy(CombinedBlend);
		CombinedAlbedo = null;
		CombinedHeight = null;
		CombinedNormal = null;
		CombinedBlend = null;
	}

	public void ClearAllTextureAssets()
	{
		SceneUtil.SmartDestroy(BlendedAlbedoTex);
		SceneUtil.SmartDestroy(BlendedNormalTex);
		SceneUtil.SmartDestroy(BlendMaskTex);
		BlendedAlbedoTex = null;
		BlendedNormalTex = null;
		BlendMaskTex = null;
	}

	public void ApplyBlendStrength(float strength)
	{
		Shader.SetGlobalFloat(ShaderBlendStrength, strength);
	}

	private List<List<MeshRenderer>> GroupTargets(MeshRenderer[] targets)
	{
		List<List<MeshRenderer>> list = new List<List<MeshRenderer>>();
		List<MeshRenderer> list2 = new List<MeshRenderer>(targets.Length);
		list2.AddRange(targets);
		while (list2.Count > 0)
		{
			if (list.Count >= MaxGroupAmount)
			{
				string text = $"Reached maximum number of allowed ({MaxGroupAmount}) of WorldLayer renderer groups. " + "All remaining renderers will not blend.\nRemaining renderers are:\n";
				foreach (MeshRenderer item in list2)
				{
					text = text + item.name + "\n";
				}
				Debug.LogWarning(text);
				break;
			}
			MeshRenderer meshRenderer = list2[0];
			list2.RemoveAt(0);
			List<MeshRenderer> list3 = new List<MeshRenderer> { meshRenderer };
			if (!IsTargetIsolated(meshRenderer))
			{
				foreach (MeshRenderer item2 in list2)
				{
					if (!AreTargetsOverlapping(item2, list3))
					{
						list3.Add(item2);
					}
				}
			}
			list2.RemoveRange(list3);
			list.Add(list3);
		}
		return list;
	}

	private static bool FilterRenderersWithWrongShader(IEnumerable<MeshRenderer> renderers, out HashSet<MeshRenderer> invalidRenderers, out HashSet<Material> invalidMaterials)
	{
		invalidRenderers = new HashSet<MeshRenderer>();
		invalidMaterials = new HashSet<Material>();
		foreach (MeshRenderer renderer in renderers)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (!IsValidShader(material))
				{
					invalidRenderers.Add(renderer);
					invalidMaterials.Add(material);
				}
			}
		}
		if (invalidRenderers.Count <= 0)
		{
			return invalidMaterials.Count > 0;
		}
		return true;
	}

	private static bool IsValidShader(Material material)
	{
		string[] validShaders = ValidShaders;
		foreach (string text in validShaders)
		{
			if (material.shader.name == text)
			{
				return true;
			}
		}
		return false;
	}

	private static bool AreTargetsOverlapping(MeshRenderer targetA, MeshRenderer targetB)
	{
		return targetA.bounds.Overlaps(targetB.bounds);
	}

	private static bool AreTargetsOverlapping(MeshRenderer reference, List<MeshRenderer> targetList)
	{
		foreach (MeshRenderer target in targetList)
		{
			if (AreTargetsOverlapping(reference, target))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsTargetIsolated(MeshRenderer target)
	{
		return target.gameObject.name.Contains("_Isolated");
	}

	private static void DebugTargetGroups(List<List<MeshRenderer>> groups)
	{
		Debug.Log($"DEBUGGING {groups.Count} GROUPS:");
		foreach (List<MeshRenderer> group in groups)
		{
			string text = "GROUP [" + group[0].name + "]: ";
			for (int i = 1; i < group.Count; i++)
			{
				MeshRenderer meshRenderer = group[i];
				text = text + meshRenderer.name + " | ";
			}
			Debug.Log(text);
		}
	}
}
