using System;
using System.Collections;
using Chronos;
using UnityEngine;

public class ActionProcessorCoroutineHelper : MonoBehaviour
{
	public static ActionProcessorCoroutineHelper instance;

	private void Awake()
	{
		instance = this;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public static void StopCoroutineHelper(Coroutine coroutine)
	{
		instance.StopCoroutine(coroutine);
	}

	public static void StopAllCoroutinesOnHelper()
	{
		instance.StopAllCoroutines();
	}

	public static Coroutine RunCoroutine(IEnumerator coroutine)
	{
		return instance.StartCoroutine(coroutine);
	}

	public static IEnumerator DelayedStartCoroutine(float delay, Action<Animator> function, Animator animator)
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		function(animator);
	}

	public static Coroutine RunDelayedAction(float delay, Action function)
	{
		return instance.StartCoroutine(DelayedStartCoroutine(delay, function));
	}

	public static IEnumerator DelayedStartCoroutine(float delay, Action function)
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		function();
	}
}
