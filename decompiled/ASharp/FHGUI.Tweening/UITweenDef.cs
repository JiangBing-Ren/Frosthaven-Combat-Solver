using DG.Tweening;
using Libs.Utils.Maths;
using UnityEngine;

namespace FHGUI.Tweening;

public abstract class UITweenDef : ScriptableObject
{
	[Header("General Tween Settings")]
	[SerializeField]
	private float _defaultDuration = 0.35f;

	[SerializeField]
	private AnimationCurve _curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[SerializeField]
	private int _loops;

	[SerializeField]
	private LoopType _loopType;

	public virtual Tween StartTween(RectTransform rt, TweenCallback onStart = null, TweenCallback onKill = null, TweenCallback onComplete = null, float executionDuration = -1f)
	{
		if (Utl.LesserThanOrEqualTo(executionDuration, 0f))
		{
			executionDuration = _defaultDuration;
		}
		if (executionDuration.IsZero())
		{
			SnapToEndValue(rt);
		}
		return StartTween_Internal(rt, executionDuration)?.OnStart(onStart).OnKill(onKill).OnComplete(onComplete)
			.SetEase(_curve)
			.SetLoops(_loops, _loopType);
	}

	protected abstract Tween StartTween_Internal(RectTransform rt, float executionDuration);

	public abstract void SnapToEndValue(RectTransform rt);
}
