using UnityEngine;

namespace Chronos;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	private static bool destroyed = false;

	private static bool persistent = false;

	private static bool automatic = false;

	private static bool missing = false;

	public static bool instantiated
	{
		get
		{
			if (!missing && !destroyed)
			{
				return _instance != null;
			}
			return false;
		}
	}

	public static T instance
	{
		get
		{
			if (!Application.isPlaying)
			{
				T[] array = Object.FindObjectsOfType<T>();
				if (array.Length == 1)
				{
					_instance = array[0];
				}
				else
				{
					if (array.Length == 0)
					{
						throw new UnityException("Missing '" + typeof(T)?.ToString() + "' singleton in the scene.");
					}
					if (array.Length > 1)
					{
						throw new UnityException("More than one '" + typeof(T)?.ToString() + "' singleton in the scene.");
					}
				}
			}
			if (destroyed)
			{
				return null;
			}
			if (missing)
			{
				throw new UnityException("Missing '" + typeof(T)?.ToString() + "' singleton in the scene.");
			}
			lock (_lock)
			{
				if (_instance == null)
				{
					T[] array2 = Object.FindObjectsOfType<T>();
					if (array2.Length == 1)
					{
						_instance = array2[0];
					}
					else if (array2.Length == 0)
					{
						GameObject gameObject = new GameObject();
						_instance = gameObject.AddComponent<T>();
						if (!automatic)
						{
							Object.Destroy(gameObject);
							missing = true;
							throw new UnityException("Missing '" + typeof(T)?.ToString() + "' singleton in the scene.");
						}
						gameObject.name = "(singleton) " + typeof(T).ToString();
						if (persistent)
						{
							Object.DontDestroyOnLoad(gameObject);
						}
					}
					else if (array2.Length > 1)
					{
						throw new UnityException("More than one '" + typeof(T)?.ToString() + "' singleton in the scene.");
					}
				}
				return _instance;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (persistent)
		{
			destroyed = true;
		}
	}

	protected Singleton(bool persistent, bool automatic)
	{
		Singleton<T>.persistent = persistent;
		Singleton<T>.automatic = automatic;
	}
}
