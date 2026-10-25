using System.Collections.Generic;
using System.Linq;
using MessengerExtensions;
using UnityEngine;

namespace ClockStone;

public static class ObjectPoolController
{
	internal class ObjectPool
	{
		private List<PoolableObject> _pool;

		private GameObject _prefab;

		private PoolableObject _poolableObjectComponent;

		private Transform _poolParent;

		internal Transform poolParent
		{
			get
			{
				_ValidatePoolParentDummy();
				return _poolParent;
			}
		}

		public ObjectPool(GameObject prefab)
		{
			_prefab = prefab;
			_poolableObjectComponent = prefab.GetComponent<PoolableObject>();
		}

		private void _ValidatePooledObjectDataContainer()
		{
			if (_pool == null)
			{
				_pool = new List<PoolableObject>();
				_ValidatePoolParentDummy();
			}
		}

		private void _ValidatePoolParentDummy()
		{
			if (!_poolParent)
			{
				GameObject gameObject = new GameObject("POOL:" + _poolableObjectComponent.name);
				_poolParent = gameObject.transform;
				gameObject._SetActive(active: false);
				if (_poolableObjectComponent.doNotDestroyOnLoad)
				{
					Object.DontDestroyOnLoad(gameObject);
				}
			}
		}

		internal void Remove(PoolableObject poolObj)
		{
			_pool.Remove(poolObj);
		}

		internal int GetObjectCount()
		{
			if (_pool != null)
			{
				return _pool.Count;
			}
			return 0;
		}

		internal GameObject GetPooledInstance(Vector3? position, Quaternion? rotation, bool activateObject, Transform parent = null)
		{
			_ValidatePooledObjectDataContainer();
			PoolableObject poolableObject = null;
			for (int i = 0; i < _pool.Count; i++)
			{
				PoolableObject poolableObject2 = _pool.ElementAt(i);
				if (poolableObject2 != null && poolableObject2._isInPool)
				{
					poolableObject = poolableObject2;
					Transform transform = poolableObject2.transform;
					transform.position = (position.HasValue ? position.Value : _poolableObjectComponent.transform.position);
					transform.rotation = (rotation.HasValue ? rotation.Value : _poolableObjectComponent.transform.rotation);
					transform.localScale = _poolableObjectComponent.transform.localScale;
					break;
				}
			}
			if (poolableObject == null && _pool.Count < _poolableObjectComponent.maxPoolSize)
			{
				poolableObject = _NewPooledInstance(position, rotation, activateObject, addToPool: false);
				poolableObject.transform.SetParent(parent);
				return poolableObject.gameObject;
			}
			if (poolableObject != null)
			{
				poolableObject.TakeFromPool(parent, activateObject);
				return poolableObject.gameObject;
			}
			return null;
		}

		internal PoolableObject PreloadInstance(bool preloadActive)
		{
			_ValidatePooledObjectDataContainer();
			return _NewPooledInstance(null, null, preloadActive, addToPool: true);
		}

		private PoolableObject _NewPooledInstance(Vector3? position, Quaternion? rotation, bool createActive, bool addToPool)
		{
			_isDuringInstantiate = true;
			GameObject gameObject = Object.Instantiate(_prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);
			PoolableObject component = gameObject.GetComponent<PoolableObject>();
			component._pool = this;
			component._serialNumber = ++_globalSerialNumber;
			component.name += component._serialNumber;
			if (component.doNotDestroyOnLoad)
			{
				Object.DontDestroyOnLoad(poolParent);
			}
			_pool.Add(component);
			if (addToPool)
			{
				component._PutIntoPool();
			}
			else
			{
				component._usageCount++;
				if (createActive)
				{
					gameObject.SetActive(value: true);
					if (component.sendPoolableActivateDeactivateMessages)
					{
						CallMethodOnObject(component.gameObject, "OnPoolableObjectActivated", includeChildren: true, includeInactive: true, component.useReflectionInsteadOfMessages);
					}
				}
			}
			_isDuringInstantiate = false;
			return component;
		}

		internal int _SetAllAvailable()
		{
			int num = 0;
			for (int i = 0; i < _pool.Count; i++)
			{
				PoolableObject poolableObject = _pool.ElementAt(i);
				if (poolableObject != null && !poolableObject._isInPool)
				{
					poolableObject._PutIntoPool();
					num++;
				}
			}
			return num;
		}

		internal void CallMethodOnObject(GameObject obj, string method, bool includeChildren, bool includeInactive, bool useReflection)
		{
			if (useReflection)
			{
				if (includeChildren)
				{
					obj.InvokeMethodInChildren(method, includeInactive);
				}
				else
				{
					obj.InvokeMethod(method, includeInactive);
				}
				return;
			}
			if (!obj._GetActive())
			{
				Debug.LogWarning("Tried to call method \"" + method + "\" on an inactive GameObject using Unity-Messaging-System. This only works on active GameObjects and Components! Check \"useReflectionInsteadOfMessages\"!", obj);
			}
			if (includeChildren)
			{
				obj.BroadcastMessage(method, null, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				obj.SendMessage(method, null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	internal static int _globalSerialNumber = 0;

	internal static bool _isDuringInstantiate = false;

	private static Dictionary<int, ObjectPool> _pools = new Dictionary<int, ObjectPool>();

	public static bool isDuringPreload { get; private set; }

	public static GameObject Instantiate(GameObject prefab, Transform parent = null)
	{
		PoolableObject component = prefab.GetComponent<PoolableObject>();
		if (component == null)
		{
			return _InstantiateGameObject(prefab, Vector3.zero, Quaternion.identity, parent);
		}
		return _GetPool(component).GetPooledInstance(null, null, prefab.activeSelf, parent) ?? InstantiateWithoutPool(prefab, parent);
	}

	public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion quaternion, Transform parent = null)
	{
		PoolableObject component = prefab.GetComponent<PoolableObject>();
		if (component == null)
		{
			return _InstantiateGameObject(prefab, position, quaternion, parent);
		}
		return _GetPool(component).GetPooledInstance(position, quaternion, prefab.activeSelf, parent) ?? InstantiateWithoutPool(prefab, position, quaternion, parent);
	}

	public static GameObject InstantiateWithoutPool(GameObject prefab, Transform parent = null)
	{
		return InstantiateWithoutPool(prefab, Vector3.zero, Quaternion.identity, parent);
	}

	public static GameObject InstantiateWithoutPool(GameObject prefab, Vector3 position, Quaternion quaternion, Transform parent = null)
	{
		_isDuringInstantiate = true;
		GameObject gameObject = _InstantiateGameObject(prefab, position, quaternion, parent);
		_isDuringInstantiate = false;
		PoolableObject component = gameObject.GetComponent<PoolableObject>();
		if (component != null)
		{
			component._instantiatedByObjectPoolController = true;
			if (component.doNotDestroyOnLoad)
			{
				Object.DontDestroyOnLoad(gameObject);
			}
			Object.Destroy(component);
		}
		return gameObject;
	}

	private static GameObject _InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		return Object.Instantiate(prefab, position, rotation, parent);
	}

	public static void Destroy(GameObject obj)
	{
		_DetachChildrenAndDestroy(obj.transform, destroyImmediate: false);
	}

	public static void DestroyImmediate(GameObject obj)
	{
		_DetachChildrenAndDestroy(obj.transform, destroyImmediate: true);
	}

	public static void Preload(GameObject prefab)
	{
		PoolableObject component = prefab.GetComponent<PoolableObject>();
		if (component == null)
		{
			Debug.LogWarning("Can not preload because prefab '" + prefab.name + "' is not poolable");
			return;
		}
		ObjectPool objectPool = _GetPool(component);
		int num = component.preloadCount - objectPool.GetObjectCount();
		if (num <= 0)
		{
			return;
		}
		isDuringPreload = true;
		bool activeSelf = prefab.activeSelf;
		try
		{
			for (int i = 0; i < num; i++)
			{
				objectPool.PreloadInstance(activeSelf);
			}
		}
		finally
		{
			isDuringPreload = false;
		}
	}

	internal static ObjectPool _GetPool(PoolableObject prefabPoolComponent)
	{
		GameObject gameObject = prefabPoolComponent.gameObject;
		int instanceID = gameObject.GetInstanceID();
		if (!_pools.TryGetValue(instanceID, out var value))
		{
			value = new ObjectPool(gameObject);
			_pools.Add(instanceID, value);
		}
		return value;
	}

	private static void _DetachChildrenAndDestroy(Transform transform, bool destroyImmediate)
	{
		PoolableObject component = transform.GetComponent<PoolableObject>();
		if (transform.childCount > 0)
		{
			List<PoolableObject> list = new List<PoolableObject>();
			transform.GetComponentsInChildren(includeInactive: true, list);
			if (component != null)
			{
				list.Remove(component);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] == null) && !list[i]._isInPool)
				{
					if (destroyImmediate)
					{
						DestroyImmediate(list[i].gameObject);
					}
					else
					{
						Destroy(list[i].gameObject);
					}
				}
			}
		}
		if (component != null)
		{
			component._PutIntoPool();
		}
		else if (destroyImmediate)
		{
			Object.DestroyImmediate(transform.gameObject);
		}
		else
		{
			Object.Destroy(transform.gameObject);
		}
	}
}
