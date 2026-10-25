using System;
using UnityEngine;

namespace MeshSlicing;

[CreateAssetMenu(menuName = "Defs/SliceFillerGenerationSettingsDef")]
public class SliceFillerGenerationSettingsDef : ScriptableObject
{
	public Material FillerMaterial;

	public float CuttingPlaneHeight = 0.5f;

	public string[] ExcludeNamesFromMeshSlicing = new string[3] { "NoSlice", "Grass", "Bush" };

	public string[] ExcludeMaterialsFromMeshSlicing = new string[2] { "Grass", "Bush" };

	public string[] ForceIncludeNamesFromMeshSlicing = Array.Empty<string>();

	public string[] ForceIncludeMaterialsFromMeshSlicing = Array.Empty<string>();

	public bool ShouldExclude(GameObject container)
	{
		if (container.name == "_ProceduralMeshSlice")
		{
			return true;
		}
		for (int i = 0; i < ForceIncludeNamesFromMeshSlicing.Length; i++)
		{
			if (container.name.Contains(ForceIncludeNamesFromMeshSlicing[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < ExcludeNamesFromMeshSlicing.Length; j++)
		{
			if (container.name.Contains(ExcludeNamesFromMeshSlicing[j]))
			{
				return true;
			}
		}
		return false;
	}

	public bool ShouldExclude(Renderer renderer)
	{
		if (renderer.name == "_ProceduralMeshSlice")
		{
			return true;
		}
		for (int i = 0; i < ForceIncludeNamesFromMeshSlicing.Length; i++)
		{
			if (renderer.name.Contains(ForceIncludeNamesFromMeshSlicing[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < ForceIncludeMaterialsFromMeshSlicing.Length; j++)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material obj in sharedMaterials)
			{
				if ((object)obj != null && obj.name.Contains(ForceIncludeMaterialsFromMeshSlicing[j]))
				{
					return false;
				}
			}
		}
		for (int l = 0; l < ExcludeNamesFromMeshSlicing.Length; l++)
		{
			if (renderer.name.Contains(ExcludeNamesFromMeshSlicing[l]))
			{
				return true;
			}
		}
		for (int m = 0; m < ExcludeMaterialsFromMeshSlicing.Length; m++)
		{
			Material[] sharedMaterials2 = renderer.sharedMaterials;
			foreach (Material obj2 in sharedMaterials2)
			{
				if ((object)obj2 != null && obj2.name.Contains(ExcludeMaterialsFromMeshSlicing[m]))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static SliceFillerGenerationSettingsDef LoadResource()
	{
		return Resources.Load<SliceFillerGenerationSettingsDef>("Settings/SliceFillerGenerationSettingsDef");
	}
}
