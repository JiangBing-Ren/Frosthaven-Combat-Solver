using System.Collections.Generic;
using System.Threading.Tasks;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using UnityEngine;

public static class Character3DDisplayUtils
{
	private static readonly int BlinnLightAmount = Shader.PropertyToID("_BlinnLightAmount");

	public static int HERO_LAYER => 64;

	public static GameObject SpawnCharacter(ECharacter character, string skin = null, string playAnimation = null, Transform parent = null)
	{
		string prefabName = character.ToString() + (skin ?? string.Empty);
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Player, prefabName);
		if (characterPrefabFromBundle == null)
		{
			Log.Warning($"Cannot spawn character {character} - prefab missing");
			return null;
		}
		GameObject gameObject = ObjectPool.Spawn(characterPrefabFromBundle, parent);
		SetupCharacter(gameObject);
		return gameObject;
	}

	private static void SetupCharacter(GameObject characterModel)
	{
		CharacterManager characterManager = CharacterManager.GetCharacterManager(characterModel);
		characterManager.InitialiseCharacter(forMenu: true, isPreview: true);
		characterManager.FadeInVisibility(0f);
		characterManager.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
	}

	public static async Task<GameObject> GetCharacterPrefabAsync(ECharacter character, string skin = null)
	{
		string prefabName = character.ToString() + (skin ?? string.Empty);
		GameObject gameObject = await AssetBundleManager.Instance.GetCharacterPrefabFromBundleAsync(CActor.EType.Player, prefabName);
		if (gameObject == null)
		{
			Log.Warning($"Cannot spawn character {character} - prefab missing");
			return null;
		}
		return gameObject;
	}

	public static void SetupHeroMaterials(GameObject character)
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetFloat(BlinnLightAmount, 0f);
		Renderer[] array = character.GetComponent<CharacterManager>()?.Renderers ?? character.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(materialPropertyBlock);
		}
	}

	public static void SetupReflectionProbe(GameObject character, ReflectionProbe reflectionProbe)
	{
		if (!(reflectionProbe == null))
		{
			Renderer[] componentsInChildren = character.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].probeAnchor = reflectionProbe.transform;
			}
		}
	}

	public static void SetLightingLayer(IEnumerable<Light> lights)
	{
		RenderingLayer layer = new RenderingLayer(HERO_LAYER);
		foreach (Light light in lights)
		{
			light.SetLayer(layer);
		}
	}

	public static void SetLightingShadows(IEnumerable<Light> lights, LightShadows shadows)
	{
		new RenderingLayer(HERO_LAYER);
		foreach (Light light in lights)
		{
			light.shadows = shadows;
		}
	}

	public static void SetRendererLayer(IEnumerable<Renderer> renderers)
	{
		RenderingLayer layer = new RenderingLayer(HERO_LAYER);
		foreach (Renderer renderer in renderers)
		{
			renderer.SetLayer(layer);
		}
	}
}
