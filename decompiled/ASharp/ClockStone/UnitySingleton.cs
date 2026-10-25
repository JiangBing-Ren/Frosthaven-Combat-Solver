using UnityEngine;

namespace ClockStone;

public class UnitySingleton<T> where T : MonoBehaviour
{
	private static T _instance;

	internal static GameObject _autoCreatePrefab;

	private static int _GlobalInstanceCount;

	private static bool _awakeSingletonCalled;

	public static T GetSingleton(bool throwErrorIfNotFound, bool autoCreate, bool searchInObjectHierarchy = true)
	{
		if (!_instance)
		{
			T val = null;
			if (searchInObjectHierarchy)
			{
				T[] array = Object.FindObjectsOfType<T>();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] is ISingletonMonoBehaviour { isSingletonObject: not false })
					{
						val = array[i];
						break;
					}
				}
			}
			if (!val)
			{
				if (!autoCreate || !(_autoCreatePrefab != null))
				{
					if (throwErrorIfNotFound)
					{
						Debug.LogError("No singleton component " + typeof(T).Name + " found in the scene.");
					}
					return null;
				}
				Object.Instantiate(_autoCreatePrefab).name = _autoCreatePrefab.name;
				if (!Object.FindObjectOfType<T>())
				{
					Debug.LogError("Auto created object does not have component " + typeof(T).Name);
					return null;
				}
			}
			else
			{
				_AwakeSingleton(val);
			}
			_instance = val;
		}
		return _instance;
	}

	private UnitySingleton()
	{
	}

	internal static void _Awake(T instance)
	{
		_GlobalInstanceCount++;
		if (_GlobalInstanceCount > 1)
		{
			Debug.LogError("More than one instance of SingletonMonoBehaviour " + typeof(T).Name);
		}
		else
		{
			_instance = instance;
		}
		_AwakeSingleton(instance);
	}

	internal static void _Destroy()
	{
		if (_GlobalInstanceCount > 0)
		{
			_GlobalInstanceCount--;
			if (_GlobalInstanceCount == 0)
			{
				_awakeSingletonCalled = false;
				_instance = null;
			}
		}
	}

	private static void _AwakeSingleton(T instance)
	{
		if (!_awakeSingletonCalled)
		{
			_awakeSingletonCalled = true;
			instance.SendMessage("AwakeSingleton", SendMessageOptions.DontRequireReceiver);
		}
	}
}
