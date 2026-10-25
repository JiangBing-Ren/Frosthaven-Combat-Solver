using UnityEngine;

namespace ClockStone;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, ISingletonMonoBehaviour where T : MonoBehaviour
{
	public static T Instance => UnitySingleton<T>.GetSingleton(throwErrorIfNotFound: true, autoCreate: true);

	public virtual bool isSingletonObject => true;

	public static T DoesInstanceExist()
	{
		return UnitySingleton<T>.GetSingleton(throwErrorIfNotFound: false, autoCreate: false);
	}

	public static void ActivateSingletonInstance()
	{
		UnitySingleton<T>.GetSingleton(throwErrorIfNotFound: true, autoCreate: true);
	}

	public static void SetSingletonAutoCreate(GameObject autoCreatePrefab)
	{
		UnitySingleton<T>._autoCreatePrefab = autoCreatePrefab;
	}

	protected virtual void Awake()
	{
		if (isSingletonObject)
		{
			UnitySingleton<T>._Awake(this as T);
		}
	}

	protected virtual void OnDestroy()
	{
		if (isSingletonObject)
		{
			UnitySingleton<T>._Destroy();
		}
	}
}
