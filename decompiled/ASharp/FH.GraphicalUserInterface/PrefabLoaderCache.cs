using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.GraphicalUserInterface;

public class PrefabLoaderCache
{
	private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

	public readonly string Bundle;

	public readonly string AssetDatabaseFolderName;

	public static PrefabLoaderCache MiscGui = new PrefabLoaderCache("misc_gui", "gui");

	public static PrefabLoaderCache MiscCards = new PrefabLoaderCache("misc_gui", "cards");

	public PrefabLoaderCache(string bundle, string assetDatabaseFolderName)
	{
		Bundle = bundle;
		AssetDatabaseFolderName = assetDatabaseFolderName;
	}

	public GameObject Load(string fileName)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>(Bundle, fileName, AssetDatabaseFolderName);
	}

	public TComponent Load<TComponent>(string fileName) where TComponent : MonoBehaviour
	{
		return Load(fileName).GetComponent<TComponent>();
	}

	public TComponent Instantiate<TComponent>(string fileName, Transform parent = null) where TComponent : MonoBehaviour
	{
		return Object.Instantiate(Load(fileName).GetComponent<TComponent>(), parent);
	}

	public GameObject Instantiate(string fileName, Transform parent = null)
	{
		return Object.Instantiate(Load(fileName), parent);
	}

	public static void GenerateAbilityCardCache(AbilityCardYMLData cardData)
	{
		if (PersistentData.CreateAbilityCard(cardData, out var newAbilityCard, out var _))
		{
			Object.Destroy(newAbilityCard);
		}
	}
}
