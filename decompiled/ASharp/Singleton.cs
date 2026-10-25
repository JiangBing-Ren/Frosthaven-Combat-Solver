using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Singleton<T> : MonoBehaviour, IExternallyAwaked where T : MonoBehaviour
{
	private static List<T> _instances = new List<T>();

	private bool _destroyed;

	public static T Instance
	{
		get
		{
			if (_instances.Count == 0)
			{
				T[] array = Object.FindObjectsOfType<T>(includeInactive: true);
				for (int i = 0; i < array.Length; i++)
				{
					((IExternallyAwaked)array[i]).ExternalAwake();
				}
				if (_instances.Count == 0)
				{
					Debug.LogWarning("No Singleton instance of '" + typeof(T).Name + "' found. Either replace Instance with InstanceFast, or remove this check.");
				}
			}
			if (_instances.Count > 1)
			{
				T val = null;
				foreach (T instance in _instances)
				{
					if ((bool)instance && instance.isActiveAndEnabled)
					{
						if (!(val == null))
						{
							Debug.LogError("Second " + typeof(T).Name + " singleton instance \"" + instance.name + "\" detected. It will return previous one: \"" + val.name + "\"!", val);
							break;
						}
						val = instance;
					}
				}
				return val;
			}
			return _instances.FirstOrDefault();
		}
	}

	public static T InstanceFast
	{
		get
		{
			if (_instances.Count == 0)
			{
				return null;
			}
			if (_instances.Count > 0)
			{
				foreach (T instance in _instances)
				{
					if (instance.isActiveAndEnabled)
					{
						return instance;
					}
				}
			}
			return _instances[0];
		}
	}

	public static bool IsInitialized => _instances.Count > 0;

	private void TryTrackInstance()
	{
		if (!_destroyed && !_instances.Contains((T)(MonoBehaviour)this))
		{
			_instances.Add((T)(MonoBehaviour)this);
			AwakeInit();
		}
	}

	private void TryUntrackInstance()
	{
		if (_instances.Contains((T)(MonoBehaviour)this))
		{
			_instances.Remove((T)(MonoBehaviour)this);
			_destroyed = true;
			AwakenedDestroy();
		}
	}

	protected virtual void Awake()
	{
		TryTrackInstance();
	}

	protected virtual void OnDestroy()
	{
		TryUntrackInstance();
	}

	protected virtual void AwakeInit()
	{
	}

	protected virtual void AwakenedDestroy()
	{
	}

	public void ExternalAwake()
	{
		TryTrackInstance();
	}

	public void ExternalDestroy()
	{
		TryUntrackInstance();
	}
}
