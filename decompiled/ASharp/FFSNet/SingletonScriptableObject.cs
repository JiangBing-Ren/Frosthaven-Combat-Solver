using System.Linq;
using UnityEngine;

namespace FFSNet;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
			}
			return instance;
		}
	}
}
