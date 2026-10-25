using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[CreateAssetMenu(menuName = "Defs/OcclusionFadingSettingsDef")]
public class OcclusionFadingSettingsDef : ScriptableObject
{
	[Serializable]
	private class ExclusionConfig
	{
		[HideInInspector]
		public string Name = "RenderTypeContext";

		public bool ExcludeEntirely;

		public string[] ExcludeNamesFromMeshFading = new string[6] { "NoFade", "Floor", "Tile", "FoWMask", "BaseHex", "Slice" };

		public string[] ExcludeMaterialsFromMeshFading = Array.Empty<string>();

		public string[] ForceIncludeNamesFromMeshFading = Array.Empty<string>();

		public string[] ForceIncludeMaterialsFromMeshFading = Array.Empty<string>();

		public ExclusionConfig(string name)
		{
			Name = name;
		}
	}

	[SerializeField]
	private List<ExclusionConfig> _exclusionConfigs;

	public bool ShouldExclude(GameObject container, ProceduralMapTile.RendererTypeContext context)
	{
		if (container.name == "_ProceduralMeshSlice")
		{
			return true;
		}
		ExclusionConfig exclusionConfig = _exclusionConfigs[(int)context];
		if (exclusionConfig.ExcludeEntirely)
		{
			return true;
		}
		for (int i = 0; i < exclusionConfig.ForceIncludeNamesFromMeshFading.Length; i++)
		{
			if (container.name.Contains(exclusionConfig.ForceIncludeNamesFromMeshFading[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < exclusionConfig.ExcludeNamesFromMeshFading.Length; j++)
		{
			if (container.name.Contains(exclusionConfig.ExcludeNamesFromMeshFading[j]))
			{
				return true;
			}
		}
		return false;
	}

	public bool ShouldExclude(Renderer renderer, ProceduralMapTile.RendererTypeContext context)
	{
		if (renderer.name == "_ProceduralMeshSlice")
		{
			return true;
		}
		ExclusionConfig exclusionConfig = _exclusionConfigs[(int)context];
		if (exclusionConfig.ExcludeEntirely)
		{
			return true;
		}
		for (int i = 0; i < exclusionConfig.ForceIncludeNamesFromMeshFading.Length; i++)
		{
			if (renderer.name.Contains(exclusionConfig.ForceIncludeNamesFromMeshFading[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < exclusionConfig.ForceIncludeMaterialsFromMeshFading.Length; j++)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material obj in sharedMaterials)
			{
				if ((object)obj != null && obj.name.Contains(exclusionConfig.ForceIncludeMaterialsFromMeshFading[j]))
				{
					return false;
				}
			}
		}
		for (int l = 0; l < exclusionConfig.ExcludeNamesFromMeshFading.Length; l++)
		{
			if (renderer.name.Contains(exclusionConfig.ExcludeNamesFromMeshFading[l]))
			{
				return true;
			}
		}
		for (int m = 0; m < exclusionConfig.ExcludeMaterialsFromMeshFading.Length; m++)
		{
			Material[] sharedMaterials2 = renderer.sharedMaterials;
			foreach (Material obj2 in sharedMaterials2)
			{
				if ((object)obj2 != null && obj2.name.Contains(exclusionConfig.ExcludeMaterialsFromMeshFading[m]))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static OcclusionFadingSettingsDef LoadResource()
	{
		return Resources.Load<OcclusionFadingSettingsDef>("Settings/OcclusionFadingSettingsDef");
	}
}
