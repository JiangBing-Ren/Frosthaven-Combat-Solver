using System.Collections.Generic;
using System.Threading;
using MEC;
using UnityEngine;

public static class MECExtensionMethods
{
	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject && gameObject.activeInHierarchy && coroutine.MoveNext()))
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject1 && gameObject1.activeInHierarchy && (bool)gameObject2 && gameObject2.activeInHierarchy && coroutine.MoveNext()))
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2, GameObject gameObject3)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject1 && gameObject1.activeInHierarchy && (bool)gameObject2 && gameObject2.activeInHierarchy && (bool)gameObject3 && gameObject3.activeInHierarchy && coroutine.MoveNext()))
		{
			yield return coroutine.Current;
		}
	}
}
