using System;
using System.Collections;
using Chronos;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
	public static CoroutineHelper instance;

	private void Awake()
	{
		instance = this;
	}

	private void OnDestroy()
	{
		instance = this;
	}

	public static void StopCoroutineHelper(Coroutine coroutine)
	{
		instance.StopCoroutine(coroutine);
	}

	public static Coroutine RunCoroutine(IEnumerator coroutine)
	{
		return instance.StartCoroutine(coroutine);
	}

	public static Coroutine RunCoroutine(IEnumerator coroutine, Action onCompleteAction)
	{
		return instance.StartCoroutine(CallbackCrt(coroutine, onCompleteAction));
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

	public static Coroutine RunDelayedFramesAction(int waitFrames, Action function, bool endOfFrame = false)
	{
		return instance.StartCoroutine(DelayedFramesStartCoroutine(waitFrames, function, endOfFrame));
	}

	private static IEnumerator DelayedFramesStartCoroutine(int waitFrames, Action function, bool endOfFrame = false)
	{
		if ((waitFrames == 0) & endOfFrame)
		{
			yield return new WaitForEndOfFrame();
		}
		while (waitFrames > 0)
		{
			yield return ((waitFrames == 1) & endOfFrame) ? new WaitForEndOfFrame() : null;
			waitFrames--;
		}
		function();
	}

	private static IEnumerator CallbackCrt(IEnumerator coroutine, Action onCompleteAction)
	{
		yield return instance.StartCoroutine(coroutine);
		onCompleteAction();
	}
}
