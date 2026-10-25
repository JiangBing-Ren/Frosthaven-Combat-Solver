using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CorrutineUtils
{
	public static IEnumerator RectTransformMove(RectTransform rect, float delay, float duration, Vector3 targetPosition, AnimationCurve curve)
	{
		yield return new WaitForSecondsFrozen(delay);
		Vector3 origPosition = rect.anchoredPosition;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			rect.anchoredPosition = Vector3.Lerp(origPosition, targetPosition, curve.Evaluate(timeElapsed / duration));
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		rect.anchoredPosition = targetPosition;
	}

	public static IEnumerator FadeInOut(CanvasGroup group, float duration, AnimationCurve curve, float alphaMax = 1f, float delay = 0f)
	{
		yield return FadeCanvasGroup(group, delay, duration / 2f, alphaMax, curve);
		yield return FadeCanvasGroup(group, 0f, duration / 2f, 0f, curve);
	}

	public static IEnumerator FadeCanvasGroup(CanvasGroup groupToFade, float delay, float duration, float targetAlpha, AnimationCurve curve, UnityAction<float> onUpdate = null, UnityAction onComplete = null)
	{
		yield return new WaitForSecondsFrozen(delay);
		float startingAlpha = groupToFade.alpha;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			groupToFade.alpha = curve.Evaluate(Mathf.Lerp(startingAlpha, targetAlpha, curve.Evaluate(timeElapsed / duration)));
			onUpdate?.Invoke(groupToFade.alpha);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		groupToFade.alpha = targetAlpha;
		onComplete?.Invoke();
	}

	public static IEnumerator RectTransformLocalScale(RectTransform rectToScale, float delay, float duration, Vector3 targetLocalScale, AnimationCurve curve, UnityAction<Vector3> onUpdate = null)
	{
		yield return new WaitForSecondsFrozen(delay);
		Vector3 startingLocalScale = rectToScale.localScale;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			rectToScale.localScale = Vector3.Lerp(startingLocalScale, targetLocalScale, curve.Evaluate(timeElapsed / duration));
			onUpdate?.Invoke(rectToScale.localScale);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		rectToScale.localScale = targetLocalScale;
	}

	public static IEnumerator RectTransformLocalScaleInOut(RectTransform rectToScale, float delay, float totalDuration, Vector3 targetLocalScale, AnimationCurve curve, UnityAction<Vector3> onUpdate = null)
	{
		yield return new WaitForSecondsFrozen(delay);
		Vector3 startingLocalScale = rectToScale.localScale;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		float duration = totalDuration / 2f;
		while (timeElapsed < duration)
		{
			rectToScale.localScale = Vector3.Lerp(startingLocalScale, targetLocalScale, curve.Evaluate(timeElapsed / duration));
			onUpdate?.Invoke(rectToScale.localScale);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		rectToScale.localScale = targetLocalScale;
		timeAtStart = Time.realtimeSinceStartup;
		timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			rectToScale.localScale = Vector3.Lerp(targetLocalScale, startingLocalScale, curve.Evaluate(timeElapsed / duration));
			onUpdate?.Invoke(rectToScale.localScale);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		rectToScale.localScale = startingLocalScale;
	}

	public static IEnumerator RectTransformPivotAroundVertical(RectTransform rectToPivot, float delay, float duration, float targetRotation, AnimationCurve curve, UnityAction<float> onUpdate = null, Action onCompleted = null)
	{
		yield return new WaitForSecondsFrozen(delay);
		Vector3 startingLocalRotation = rectToPivot.localRotation.eulerAngles;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			rectToPivot.localRotation = Quaternion.Euler(new Vector3(startingLocalRotation.x, Mathf.Lerp(startingLocalRotation.y, targetRotation, curve.Evaluate(timeElapsed / duration)), startingLocalRotation.z));
			onUpdate?.Invoke(rectToPivot.localRotation.eulerAngles.y);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		rectToPivot.localRotation = Quaternion.Euler(new Vector3(startingLocalRotation.x, targetRotation, startingLocalRotation.z));
		onCompleted?.Invoke();
	}

	public static IEnumerator ProgressTo(float delay, float duration, float fromValue, float toValue, AnimationCurve curve, UnityAction<float> onUpdate, Action onCompleted = null)
	{
		if (delay > 0f)
		{
			yield return new WaitForSecondsFrozen(delay);
		}
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			float arg = Mathf.Lerp(fromValue, toValue, curve.Evaluate(timeElapsed / duration));
			onUpdate?.Invoke(arg);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		onCompleted?.Invoke();
	}
}
