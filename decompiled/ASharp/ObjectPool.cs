using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronos;
using FH.ApparanceBaking;
using Libs.GameConsole;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	public enum StartupPoolMode
	{
		Awake,
		Start,
		CallManually
	}

	public enum ECardType
	{
		None,
		Ability,
		Item,
		Monster
	}

	public delegate bool CardGeneratorDelegate(int cardId, out GameObject generatedCardGO);

	[Serializable]
	public class Pool
	{
		public GameObject Prefab;

		public int InitialPoolSize;

		public StartupPoolMode StartupMode;

		public string PoolContainerName;
	}

	public class CardPool
	{
		public int CardID { get; private set; }

		public ECardType CardType { get; private set; }

		public List<GameObject> Instances { get; private set; }

		public CardPool(int cardID, ECardType cardType, List<GameObject> instances)
		{
			CardID = cardID;
			CardType = cardType;
			Instances = instances;
		}

		public void DestroyInstances()
		{
			foreach (GameObject instance in Instances)
			{
				UnityEngine.Object.Destroy(instance);
			}
			Instances.Clear();
		}
	}

	[ConsoleVariable(Alias = "pool_keep_monster_cards_loaded")]
	public static bool KeepMonsterCardsLoaded = true;

	public static bool KeepRecycledCardsActiveState = false;

	public static Action<GameObject> onSpawned;

	public bool IsRestarting;

	public static ObjectPool instance;

	public Pool[] StartupPools;

	private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

	private Dictionary<ECardType, List<CardPool>> cardPools = new Dictionary<ECardType, List<CardPool>>();

	private Dictionary<ECardType, CardGeneratorDelegate> cardPoolGenerators = new Dictionary<ECardType, CardGeneratorDelegate>();

	private Dictionary<GameObject, GameObject> waitingToBeRecycled = new Dictionary<GameObject, GameObject>();

	private List<GameObject> enhancedCards = new List<GameObject>();

	public const string EffectsPoolName = "@ VFX Pool";

	public const string TooltipsPoolName = "Tooltips Pool";

	private static bool isQuitting;

	public static List<GameObject> StartupPoolGameObjects => instance.StartupPools.Select((Pool o) => o.Prefab).ToList();

	public static bool ItemsWaitingToBeRecyled => instance.waitingToBeRecycled.Count > 0;

	private void Awake()
	{
		instance = this;
		Pool[] startupPools = StartupPools;
		foreach (Pool pool in startupPools)
		{
			if (pool.StartupMode == StartupPoolMode.Awake)
			{
				WarmUpPool(pool.Prefab, pool.InitialPoolSize, pool.PoolContainerName);
			}
		}
	}

	private void Start()
	{
		Pool[] startupPools = StartupPools;
		foreach (Pool pool in startupPools)
		{
			if (pool.StartupMode == StartupPoolMode.Start)
			{
				WarmUpPool(pool.Prefab, pool.InitialPoolSize, pool.PoolContainerName);
			}
		}
	}

	private void OnApplicationQuit()
	{
		isQuitting = true;
	}

	private Transform GetPoolContainer(GameObject prefab)
	{
		return GetPoolContainer(prefab.name + " Pool");
	}

	private Transform GetPoolContainer(string poolName)
	{
		Transform transform = base.transform.Find(poolName);
		if (transform == null)
		{
			transform = new GameObject(poolName).transform;
			transform.SetParent(base.transform);
		}
		return transform.transform;
	}

	public static void RemoveAbilityCard(int cardID)
	{
		if (instance.cardPools.TryGetValue(ECardType.Ability, out var value))
		{
			value.RemoveAll((CardPool r) => r.CardID == cardID);
			if (value.Count == 0)
			{
				instance.cardPools.Remove(ECardType.Ability);
			}
		}
	}

	public static bool HasAbilityCard(int cardID)
	{
		if (instance.cardPools.TryGetValue(ECardType.Ability, out var value))
		{
			return value.Any((CardPool r) => r.CardID == cardID);
		}
		return false;
	}

	public static bool HasMonsterCard(int cardID)
	{
		if (instance.cardPools.TryGetValue(ECardType.Monster, out var value))
		{
			return value.Any((CardPool r) => r.CardID == cardID);
		}
		return false;
	}

	public static void RemoveItemCard(int cardID)
	{
		if (instance.cardPools.TryGetValue(ECardType.Item, out var value))
		{
			value.RemoveAll((CardPool r) => r.CardID == cardID);
			if (value.Count == 0)
			{
				instance.cardPools.Remove(ECardType.Item);
			}
		}
	}

	public static void WarmUpPool(GameObject prefab, int poolCapacity, string poolContainerName = null)
	{
		if (!(prefab == null))
		{
			if (!instance.pooledObjects.TryGetValue(prefab, out var value))
			{
				value = new List<GameObject>(poolCapacity);
				instance.pooledObjects.Add(prefab, value);
			}
			while (value.Count < poolCapacity)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab, string.IsNullOrEmpty(poolContainerName) ? instance.GetPoolContainer(prefab) : instance.GetPoolContainer(poolContainerName));
				gameObject.name += $"_{value.Count:00}";
				gameObject.SetActive(value: false);
				value.Add(gameObject);
			}
		}
	}

	public static async Task WarmUpPoolAsync(GameObject prefab, int poolCapacity, string poolContainerName = null)
	{
		using (new MiniProfilerScope("ObjectPool.WarmUpPoolAsync"))
		{
			if (!(prefab == null))
			{
				if (!instance.pooledObjects.TryGetValue(prefab, out var pool))
				{
					pool = new List<GameObject>(poolCapacity);
					instance.pooledObjects.Add(prefab, pool);
				}
				while (pool.Count < poolCapacity)
				{
					await MF.YieldIfOverBudget("await in YieldIfOverBudget in WarmUpPoolAsync");
					GameObject gameObject = UnityEngine.Object.Instantiate(prefab, string.IsNullOrEmpty(poolContainerName) ? instance.GetPoolContainer(prefab) : instance.GetPoolContainer(poolContainerName));
					gameObject.name += $"_{pool.Count:00}";
					gameObject.SetActive(value: false);
					pool.Add(gameObject);
				}
			}
		}
	}

	public static void SetCardGenerator(ECardType cardType, CardGeneratorDelegate generator)
	{
		instance.cardPoolGenerators[cardType] = generator;
	}

	public static void CreateCardPool(int cardID, ECardType cardType, GameObject cardInstance, int amountToCache)
	{
		List<GameObject> list = new List<GameObject>();
		cardInstance.transform.SetParent(instance.GetPoolContainer($"@ {cardType} Cards Pool"));
		list.Add(cardInstance);
		for (int i = 1; i < amountToCache; i++)
		{
			GameObject item = UnityEngine.Object.Instantiate(cardInstance, instance.GetPoolContainer($"@ {cardType} Cards Pool"));
			list.Add(item);
		}
		foreach (GameObject item2 in list)
		{
			item2.SetActive(value: false);
		}
		if (instance.cardPools.TryGetValue(cardType, out var value))
		{
			value.Add(new CardPool(cardID, cardType, list));
			return;
		}
		instance.cardPools[cardType] = new List<CardPool>
		{
			new CardPool(cardID, cardType, list)
		};
	}

	public static void DestroyCardPools()
	{
		foreach (CardPool item in instance.cardPools.SelectMany((KeyValuePair<ECardType, List<CardPool>> p) => p.Value))
		{
			item.DestroyInstances();
		}
		instance.cardPools.Clear();
	}

	public static List<GameObject> GetAllCachedAbilityCards(int cardID)
	{
		List<GameObject> allCards = new List<GameObject>();
		if (instance.cardPools.TryGetValue(ECardType.Ability, out var value))
		{
			CardPool cardPool = value.SingleOrDefault((CardPool s) => s.CardID == cardID);
			if (cardPool != null)
			{
				allCards.AddRange(cardPool.Instances);
				allCards.AddRange(from s in UnityEngine.Object.FindObjectsOfType<AbilityCardUI>(includeInactive: true)
					where s.CardID == cardID && !allCards.Contains(s.gameObject)
					select s.gameObject);
			}
		}
		return allCards;
	}

	public static void ClearAllMonsterCards()
	{
		if (!instance.cardPools.TryGetValue(ECardType.Monster, out var value))
		{
			return;
		}
		foreach (CardPool item in value)
		{
			item.DestroyInstances();
		}
		value.Clear();
	}

	public static GameObject SpawnCard(int cardID, ECardType cardType, Transform parent, bool resetLocalScale = false, bool resetToMiddle = false, bool resetLocalRotation = false, bool activate = true)
	{
		try
		{
			if (!instance.cardPools.TryGetValue(cardType, out var value))
			{
				value = new List<CardPool>();
				instance.cardPools.Add(cardType, value);
			}
			if (value.All((CardPool p) => p.CardID != cardID))
			{
				if (instance.cardPoolGenerators.TryGetValue(cardType, out var value2) && value2 != null && value2(cardID, out var generatedCardGO))
				{
					CreateCardPool(cardID, cardType, generatedCardGO, 2);
					return SpawnCard(cardID, cardType, parent, resetLocalScale, resetToMiddle, resetLocalRotation, activate);
				}
				Log.Error("[OBJECTPOOL] Error: Unable to spawn card with ID " + cardID);
				return null;
			}
			CardPool cardPool;
			try
			{
				cardPool = value.Single((CardPool s) => s.CardID == cardID);
			}
			catch
			{
				Log.Error("[OBJECTPOOL] Duplicate pools with the same ID and card type exist!  CardID: " + cardID + "  Card Type: " + cardType.ToString() + ".  Unable to spawn card.");
				return null;
			}
			if (cardPool.Instances.Count == 0)
			{
				Log.Error("[OBJECTPOOL] Card pool " + cardID + " is empty.  Unable to spawn new card.");
				return null;
			}
			GameObject gameObject = null;
			if (cardPool.Instances.Count > 1)
			{
				for (int num = cardPool.Instances.Count - 1; num > 0; num--)
				{
					gameObject = cardPool.Instances[num];
					cardPool.Instances.RemoveAt(num);
				}
			}
			if (gameObject == null)
			{
				gameObject = UnityEngine.Object.Instantiate(cardPool.Instances[0], instance.GetPoolContainer(cardPool.Instances[0]));
			}
			gameObject.transform.SetParent(parent);
			if (resetLocalScale)
			{
				gameObject.transform.localScale = Vector3.one;
			}
			if (resetToMiddle)
			{
				RectTransform obj2 = gameObject.transform as RectTransform;
				Vector2 vector = (obj2.pivot = new Vector2(0.5f, 0.5f));
				Vector2 anchorMin = (obj2.anchorMax = vector);
				obj2.anchorMin = anchorMin;
				obj2.anchoredPosition = Vector2.zero;
			}
			if (resetLocalRotation)
			{
				gameObject.transform.localRotation = Quaternion.identity;
			}
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0f);
			if (activate)
			{
				gameObject.SetActive(value: true);
			}
			((IPooleable)gameObject.GetComponent(typeof(IPooleable)))?.OnRemovedFromPool();
			return gameObject;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, $"[OBJECTPOOL] An exception occurred generating card {cardID}");
			return null;
		}
	}

	public static void RecycleCard(int cardID, ECardType cardType, GameObject cardInstance)
	{
		if (cardInstance == null)
		{
			Debug.LogWarning($"Trying to recycle card {cardID} of type {cardType} that is destroyed. Skip.");
		}
		else
		{
			if (!(instance != null))
			{
				return;
			}
			if (!instance.cardPools.TryGetValue(cardType, out var value) || value.All((CardPool e) => e.CardID != cardID))
			{
				Log.Error("[OBJECTPOOL] Error: Unable to Recycle card with ID " + cardID);
				return;
			}
			CardPool cardPool;
			try
			{
				cardPool = value.Single((CardPool s) => s.CardID == cardID);
			}
			catch
			{
				Log.Error("[OBJECTPOOL] Duplicate pools with the same ID and card type exist!  CardID: " + cardID + "  Card Type: " + cardType.ToString() + ".  Unable to spawn card.");
				return;
			}
			AbilityCardUI component = cardInstance.GetComponent<AbilityCardUI>();
			if ((bool)component)
			{
				if (component.fullAbilityCard != null && component.fullAbilityCard.transform.parent != component.transform)
				{
					component.fullAbilityCard.transform.SetParent(component.transform);
					component.CancelLostAnimation();
				}
				component.SetHoverDetectionActive(active: true);
				component.SetMode(CardHandMode.DeckSelection, null);
			}
			cardInstance.transform.SetParent(instance.GetPoolContainer($"@ {cardType} Cards Pool"));
			RectTransform obj2 = cardInstance.transform as RectTransform;
			cardInstance.transform.localRotation = Quaternion.identity;
			obj2.anchoredPosition = Vector2.zero;
			Vector2 vector = (obj2.pivot = new Vector2(0.5f, 0.5f));
			Vector2 anchorMin = (obj2.anchorMax = vector);
			obj2.anchorMin = anchorMin;
			if (!KeepRecycledCardsActiveState)
			{
				cardInstance.SetActive(value: false);
			}
			if (!cardPool.Instances.Contains(cardInstance))
			{
				cardPool.Instances.Add(cardInstance);
			}
			else
			{
				Log.Warning($"Trying to recycle card {cardID} that is already present in the pool.");
			}
			((IPooleable)cardInstance.GetComponent(typeof(IPooleable)))?.OnReturnedToPool();
		}
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
	{
		try
		{
			if (prefab == null)
			{
				Log.Error("[OBJECTPOOL] Attempting to spawn null object from Object Pool");
				return null;
			}
			GameObject gameObject = null;
			if (!forceInstantiate && instance.pooledObjects.TryGetValue(prefab, out var value))
			{
				while (value.Count != 0)
				{
					gameObject = value[0];
					value.RemoveAt(0);
					if (!(gameObject == null) && !instance.spawnedObjects.ContainsKey(gameObject))
					{
						Transform parent2 = gameObject.transform.parent;
						gameObject.transform.SetParent(parent, worldPositionStays: false);
						gameObject.transform.localPosition = position;
						gameObject.transform.rotation = rotation;
						IPooleable pooleable = (IPooleable)gameObject.GetComponent(typeof(IPooleable));
						gameObject.SetActive(value: true);
						pooleable?.OnRemovedFromPool();
						instance.spawnedObjects.Add(gameObject, prefab);
						if (parent2.childCount == 0)
						{
							parent2.transform.SetParent(null);
							UnityEngine.Object.Destroy(parent2.gameObject);
						}
						onSpawned?.Invoke(gameObject);
						return gameObject;
					}
				}
			}
			gameObject = UnityEngine.Object.Instantiate(prefab, parent);
			gameObject.transform.localPosition = position;
			gameObject.transform.rotation = rotation;
			int num = (instance.pooledObjects.ContainsKey(prefab) ? instance.pooledObjects[prefab].Count : instance.spawnedObjects.Values.Count((GameObject x) => x == prefab));
			GameObject gameObject2 = gameObject;
			gameObject2.name = gameObject2.name + "_" + num;
			instance.spawnedObjects.Add(gameObject, prefab);
			if (!instance.pooledObjects.ContainsKey(prefab))
			{
				instance.pooledObjects.Add(prefab, new List<GameObject>());
			}
			onSpawned?.Invoke(gameObject);
			return gameObject;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "[OBJECTPOOL] An exception occurred spawning an object from the object pool.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_OBJECTPOOL_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, bool forceInstantiate = false)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity, forceInstantiate);
	}

	public static T Spawn<T>(T prefab, Transform parent, bool forceInstantiate = false) where T : MonoBehaviour
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity, forceInstantiate).GetComponent<T>();
	}

	public static bool SpawnedContains(GameObject prefab)
	{
		return instance.spawnedObjects.ContainsValue(prefab);
	}

	public static IEnumerable<GameObject> GetMatches(GameObject prefab)
	{
		return from pair in instance.spawnedObjects
			where pair.Value == prefab
			select pair.Key;
	}

	public static void Recycle(GameObject objInstance, GameObject prefab = null, string poolContainerName = null)
	{
		if (isQuitting)
		{
			return;
		}
		try
		{
			if (prefab == null && !instance.spawnedObjects.TryGetValue(objInstance, out prefab))
			{
				foreach (KeyValuePair<GameObject, List<GameObject>> pooledObject in instance.pooledObjects)
				{
					List<GameObject> value = pooledObject.Value;
					if (value != null && value.Contains(objInstance))
					{
						Log.Error("[OBJECTPOOL] Attempting to destroy pooled object " + objInstance.name + ". Are you recycling the object multiple times?");
						return;
					}
				}
				Log.Error("[OBJECTPOOL] GameObject " + objInstance.name + " was not found in spawned object pool.  It will be destroyed.");
				UnityEngine.Object.Destroy(objInstance);
				return;
			}
			instance.spawnedObjects.Remove(objInstance);
			if (instance.pooledObjects.TryGetValue(prefab, out var value2))
			{
				if (!value2.Contains(objInstance))
				{
					value2.Add(objInstance);
				}
				else
				{
					Log.Info("[OBJECTPOOL] Attempting to recycle an object instance that is already in the pool.  Obj Name = " + objInstance.name);
				}
				objInstance.transform.SetParent(string.IsNullOrEmpty(poolContainerName) ? instance.GetPoolContainer(prefab) : instance.GetPoolContainer(poolContainerName), worldPositionStays: false);
				objInstance.SetActive(value: false);
			}
			else
			{
				instance.pooledObjects.Add(prefab, new List<GameObject> { objInstance });
			}
			((IPooleable)prefab.GetComponent(typeof(IPooleable)))?.OnReturnedToPool();
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "[OBJECTPOOL] An exception occurred recycling an object to the object pool.");
		}
	}

	public static void Recycle(GameObject objInstance, float waitForRecycle, GameObject prefab = null, string poolContainerName = null)
	{
		instance.StartCoroutine(RecycleCoroutine(objInstance, waitForRecycle, prefab, poolContainerName));
	}

	public static void RecyclePendingObjects()
	{
		if (instance != null && instance.waitingToBeRecycled != null)
		{
			foreach (KeyValuePair<GameObject, GameObject> item in instance.waitingToBeRecycled)
			{
				Recycle(item.Key, item.Value);
			}
			instance.waitingToBeRecycled.Clear();
		}
		if (instance.IsRestarting)
		{
			foreach (CObjectProp destroyedProp in ScenarioManager.CurrentScenarioState.DestroyedProps)
			{
				GameObject gameObject = GameObject.Find(destroyedProp?.InstanceName);
				if (gameObject != null)
				{
					EnvironmentLoader.Instance.BeforePropDestroyed(gameObject, destroyedProp);
				}
			}
			return;
		}
		EnvironmentLoader.Instance?.ClearDestroytedContent();
	}

	public static IEnumerator RecycleCoroutine(GameObject objInstance, float waitForRecycle, GameObject prefab = null, string poolContainerName = null)
	{
		if (Mathf.Approximately(waitForRecycle, float.MaxValue) || instance.waitingToBeRecycled.ContainsKey(objInstance))
		{
			yield break;
		}
		instance.waitingToBeRecycled.Add(objInstance, prefab);
		yield return Timekeeper.instance.WaitForSeconds(waitForRecycle);
		if (instance.waitingToBeRecycled.ContainsKey(objInstance))
		{
			if (objInstance != null)
			{
				Recycle(objInstance, prefab, poolContainerName);
				instance.waitingToBeRecycled.Remove(objInstance);
			}
			else
			{
				Log.Warning("[OBJECTPOOL] Object Pool attempted to recycle an object instance that has already been destroyed");
			}
		}
	}

	public static void RecycleAll(GameObject prefab)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				list.Add(spawnedObject.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			Recycle(list[i], prefab);
		}
	}

	public static void DestroyPooled(GameObject prefab)
	{
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				UnityEngine.Object.Destroy(value[i]);
			}
			value.Clear();
		}
		instance.pooledObjects.Remove(prefab);
	}

	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}

	public static void RecycleAndDestroyObjects()
	{
		if (instance == null)
		{
			return;
		}
		try
		{
			RecyclePendingObjects();
			instance.spawnedObjects.Clear();
			try
			{
				instance.StopAllCoroutines();
			}
			catch
			{
			}
			foreach (GameObject item in instance.pooledObjects.Keys.ToList().Except(AutoGenerateObjectPools.GlobalEffects).Except(StartupPoolGameObjects))
			{
				DestroyPooled(item);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during RecycleAndDestroyObjects.");
		}
	}
}
