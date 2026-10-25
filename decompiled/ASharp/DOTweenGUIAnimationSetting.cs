using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class DOTweenGUIAnimationSetting
{
	[TextArea]
	[SerializeField]
	public string Information;

	[Header("Easing")]
	public bool UseAnimationCurve;

	[ConditionalField("UseAnimationCurve", false, true)]
	public Ease AnimationType;

	[ConditionalField("UseAnimationCurve", true, true)]
	[SerializeField]
	private AnimationCurve animationCurve;

	[Header("Times")]
	public float Duration;

	public float Delay;

	[Space]
	public UnityEvent OnTweenStart;

	public Tween BuildTween()
	{
		Tween tween = BuildTweenAction()?.SetDelay(Delay);
		tween?.OnStart(OnTweenStart.Invoke);
		if (UseAnimationCurve)
		{
			return tween?.SetEase(animationCurve);
		}
		return tween?.SetEase(AnimationType);
	}

	protected abstract Tween BuildTweenAction();

	public abstract void RestoreOriginalValue();

	public abstract void SetFinalValue();

	public virtual void Initialize()
	{
	}

	protected DOTweenGUIAnimationSetting(float duration, float delay = 0f, Ease easing = Ease.Unset)
	{
		Duration = duration;
		Delay = delay;
		AnimationType = easing;
	}
}
[Serializable]
public abstract class DOTweenGUIAnimationSetting<T, T2> : DOTweenGUIAnimationSetting where T2 : Component
{
	[Header("Object to animate")]
	public T2 Target;

	[Header("Values")]
	public T OriginalValue;

	public T ToValue;

	public override void RestoreOriginalValue()
	{
		SetValue(OriginalValue);
	}

	public override void SetFinalValue()
	{
		SetValue(ToValue);
	}

	protected abstract void SetValue(T value);

	protected DOTweenGUIAnimationSetting(T2 target, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(duration, delay, easing)
	{
		Target = target;
	}

	public override string ToString()
	{
		return $"{Target.name} ({base.ToString()} from {OriginalValue} to {ToValue})";
	}
}
[Serializable]
public abstract class DOTweenGUIAnimationSetting<T, T2, T3> : DOTweenGUIAnimationSetting<T, T3> where T3 : Component
{
	[Header("Animation type")]
	public T2 Animation;

	protected DOTweenGUIAnimationSetting(T3 target, T2 type, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target, duration, delay, easing)
	{
		Animation = type;
	}

	public override string ToString()
	{
		return $"{Target.name} ({Animation} from {OriginalValue} to {ToValue})";
	}
}
