using UnityEngine;

namespace ClockStone;

[AddComponentMenu("ClockStone/PoolableObject")]
public class PoolableObject : MonoBehaviour
{
	public int maxPoolSize = 10;

	public int preloadCount;

	public bool doNotDestroyOnLoad;

	public bool sendAwakeStartOnDestroyMessage = true;

	public bool sendPoolableActivateDeactivateMessages;

	public bool useReflectionInsteadOfMessages;

	internal bool _isInPool;

	internal ObjectPoolController.ObjectPool _pool;

	internal int _serialNumber;

	internal int _usageCount;

	internal bool _awakeJustCalledByUnity;

	internal bool _instantiatedByObjectPoolController;

	private bool _justInvokingOnDestroy;

	protected void Awake()
	{
		_awakeJustCalledByUnity = true;
	}

	protected void OnDestroy()
	{
		if (!_justInvokingOnDestroy && _pool != null)
		{
			_pool.Remove(this);
		}
	}

	public int GetSerialNumber()
	{
		return _serialNumber;
	}

	public int GetUsageCount()
	{
		return _usageCount;
	}

	public int DeactivateAllPoolableObjectsOfMyKind()
	{
		if (_pool != null)
		{
			return _pool._SetAllAvailable();
		}
		return 0;
	}

	public bool IsDeactivated()
	{
		return _isInPool;
	}

	internal void _PutIntoPool()
	{
		if (_pool == null)
		{
			Debug.LogError("Tried to put object into pool which was not created with ObjectPoolController", this);
			return;
		}
		if (_isInPool)
		{
			if (transform.parent != _pool.poolParent)
			{
				Debug.LogWarning("Object was already in pool but parented to Pool-Parent. Reparented.", this);
				transform.SetParent(_pool.poolParent);
			}
			else
			{
				Debug.LogWarning("Object is already in Pool", this);
			}
			return;
		}
		if (!ObjectPoolController._isDuringInstantiate)
		{
			if (sendAwakeStartOnDestroyMessage)
			{
				_justInvokingOnDestroy = true;
				_pool.CallMethodOnObject(gameObject, "OnDestroy", includeChildren: true, includeInactive: true, useReflectionInsteadOfMessages);
				_justInvokingOnDestroy = false;
			}
			if (sendPoolableActivateDeactivateMessages)
			{
				_pool.CallMethodOnObject(gameObject, "OnPoolableObjectDeactivated", includeChildren: true, includeInactive: true, useReflectionInsteadOfMessages);
			}
		}
		_isInPool = true;
		transform.SetParent(_pool.poolParent);
		gameObject.SetActive(value: false);
	}

	internal void TakeFromPool(Transform parent, bool activateObject)
	{
		if (!_isInPool)
		{
			Debug.LogError("Tried to take an object from Pool which is not available!", this);
			return;
		}
		_isInPool = false;
		_usageCount++;
		transform.SetParent(parent);
		if (!activateObject)
		{
			return;
		}
		_awakeJustCalledByUnity = false;
		gameObject.SetActive(value: true);
		if (sendAwakeStartOnDestroyMessage && !_awakeJustCalledByUnity)
		{
			_pool.CallMethodOnObject(gameObject, "Awake", includeChildren: true, includeInactive: false, useReflectionInsteadOfMessages);
			if (gameObject._GetActive())
			{
				_pool.CallMethodOnObject(gameObject, "Start", includeChildren: true, includeInactive: false, useReflectionInsteadOfMessages);
			}
		}
		if (sendPoolableActivateDeactivateMessages)
		{
			_pool.CallMethodOnObject(gameObject, "OnPoolableObjectActivated", includeChildren: true, includeInactive: true, useReflectionInsteadOfMessages);
		}
	}
}
