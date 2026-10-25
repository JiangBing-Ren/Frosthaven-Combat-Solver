using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DOTweenGUIAnimator : GUIAnimator
{
	[SerializeField]
	protected DOTweenGUIAnimationConfig animations;

	[SerializeField]
	protected bool ignoreScaleTime;

	[SerializeField]
	private bool loop;

	[SerializeField]
	private bool playOnEnable;

	protected List<Tween> animationsInProcess = new List<Tween>();

	private bool initialized;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		foreach (DOTweenGUIAnimationSetting setting in GetSettings())
		{
			setting.Initialize();
		}
	}

	protected override void Animate()
	{
		Tween lastAnimation = null;
		foreach (DOTweenGUIAnimationSetting setting in GetSettings())
		{
			Tween anim = setting.BuildTween()?.SetUpdate(ignoreScaleTime);
			if (anim == null)
			{
				continue;
			}
			animationsInProcess.Add(anim);
			if (loop)
			{
				anim.SetLoops(-1);
				continue;
			}
			anim.OnComplete(() =>
			{
				animationsInProcess.Remove(anim);
				if (enableDebug)
				{
					Log.GUIInfo($"[Animation] Completed tween \"{(anim.target as Object)?.name}\" {anim.id} for \"{gameObject.name}\". Remaining: {animationsInProcess.Count}");
				}
				anim = null;
			});
			if (lastAnimation == null || setting.Delay + setting.Duration >= lastAnimation.Duration() + lastAnimation.Delay())
			{
				lastAnimation = anim;
			}
			if (enableDebug)
			{
				Log.GUIInfo($"[Animation] Create tween \"{(anim.target as Object)?.name}\" for \"{gameObject.name}\" with duration {anim.Duration()} delay {anim.Delay()} id {anim.id} for \"{gameObject.name}\"");
			}
		}
		if (lastAnimation == null)
		{
			return;
		}
		lastAnimation.OnComplete(() =>
		{
			animationsInProcess.Remove(lastAnimation);
			if (enableDebug)
			{
				Log.GUIInfo($"[Animation] Completed last tween \"{(lastAnimation.target as Object)?.name}\" {lastAnimation.id} for \"{gameObject.name}\". Remaining: {animationsInProcess.Count}");
			}
			lastAnimation = null;
			FinishAnimation();
		});
	}

	protected override void Clear()
	{
		base.Clear();
		foreach (Tween item in animationsInProcess)
		{
			if (enableDebug)
			{
				Log.GUIInfo($"DO tween anim cancels {item.id} {gameObject.name}");
			}
			item.Kill();
		}
		animationsInProcess.Clear();
	}

	protected override void ResetInitialState()
	{
		Initialize();
		foreach (DOTweenGUIAnimationSetting item in from it in GetSettings()
			orderby it.Delay descending
			select it)
		{
			item.RestoreOriginalValue();
		}
		if (enableDebug)
		{
			Log.GUIInfo("[Animation] ResetInitialState " + gameObject.name);
		}
	}

	protected override void ResetFinishState()
	{
		Initialize();
		foreach (DOTweenGUIAnimationSetting item in from it in GetSettings()
			orderby it.Delay + it.Duration
			select it)
		{
			item.SetFinalValue();
		}
		if (enableDebug)
		{
			Log.GUIInfo("[Animation] ResetFinishState " + gameObject.name);
		}
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop(goToEnd: true);
	}

	public IReadOnlyList<DOTweenGUIAnimationSetting> GetSettings()
	{
		return animations.GetSettings();
	}
}
