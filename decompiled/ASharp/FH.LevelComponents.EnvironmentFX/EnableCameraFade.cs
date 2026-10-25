using SETUtil.Materials;
using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

public class EnableCameraFade : MonoBehaviour
{
	private const string CAMERA_FADE_PARAM = "_CAMERA_FADE_STRENGTH";

	private const string DEPRECATED_CAMERA_FADE_KEYWORD = "_SkipFade";

	private MaterialModification.MaterialCache _materialCache;

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		if (_materialCache == null)
		{
			_materialCache = new MaterialModification.MaterialCache();
		}
		UpdateMaterials(fadeIsEnabled: true);
	}

	private bool CanMaterialCameraFade(Material material)
	{
		return material.shader.FindPropertyIndex("_CAMERA_FADE_STRENGTH") > 0;
	}

	private bool DoesMaterialHaveDeprecatedFade(Material material)
	{
		return material.shader.FindPropertyIndex("_SkipFade") > 0;
	}

	private void UpdateMaterials(bool fadeIsEnabled)
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow(renderer.gameObject.name.Contains("_NoFade") ? null : _materialCache);
			foreach (Material material in materialClonesSlow)
			{
				if (material == null)
				{
					Log.Warning("Missing material in " + renderer.name + " / " + name, renderer.gameObject);
				}
				else if (CanMaterialCameraFade(material))
				{
					SetSingleMaterialProperty(material, "_CAMERA_FADE_STRENGTH", fadeIsEnabled);
					if (DoesMaterialHaveDeprecatedFade(material))
					{
						material.SetFloat("_SkipFade", 1f);
					}
				}
			}
		}
	}

	private void SetSingleMaterialProperty(Material material, string propertyIndex, bool value)
	{
		material.SetFloat(propertyIndex, value ? 1f : 0f);
	}

	[ContextMenu("Validate Material+ References")]
	private void ValidateMaterialReferences()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				CanMaterialCameraFade(material);
			}
		}
	}
}
