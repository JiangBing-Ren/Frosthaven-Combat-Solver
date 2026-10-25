using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoopAnimator : MonoBehaviour
{
	[SerializeField]
	private List<AnimationSetting> effects;

	[SerializeField]
	private bool autoStart;

	[SerializeField]
	private bool ignoreTimeScale;

	private List<Tween> currentAnim = new List<Tween>();

	private bool looping;

	private Dictionary<AnimationSetting, AnimationSetting> undoEffects;

	private void Awake()
	{
		CreateUndo();
	}

	private void OnEnable()
	{
		if (autoStart)
		{
			StartLoop();
		}
	}

	[ContextMenu("Start")]
	public void StartLoop(bool resetToInitial = false)
	{
		if (!looping && gameObject.activeSelf)
		{
			CreateUndo();
			if (resetToInitial)
			{
				ResetToInitialState();
			}
			looping = true;
			effects.ForEach((AnimationSetting it) =>
			{
				Loop(it);
			});
		}
	}

	[ContextMenu("Stop")]
	public void StopLoop(bool resetToInitial = false)
	{
		if (!looping)
		{
			return;
		}
		looping = false;
		foreach (Tween item in currentAnim)
		{
			item.Kill();
		}
		currentAnim.Clear();
		if (resetToInitial)
		{
			ResetToInitialState();
		}
	}

	public void ResetToInitialState()
	{
		foreach (AnimationSetting value in undoEffects.Values)
		{
			switch (value.Animation)
			{
			case AnimationSetting.LegacyTweenAction.SCALE:
				value.Transform.localScale = value.ToValue;
				break;
			case AnimationSetting.LegacyTweenAction.MOVE:
				value.Transform.anchoredPosition = value.ToValue;
				break;
			case AnimationSetting.LegacyTweenAction.MOVE_LOCAL:
				value.Transform.localPosition = value.ToValue;
				break;
			case AnimationSetting.LegacyTweenAction.CANVASGROUP_ALPHA:
				value.Transform.GetComponent<CanvasGroup>().alpha = value.ToUniqueValue;
				break;
			case AnimationSetting.LegacyTweenAction.CANVAS_ALPHA:
			{
				Graphic component = value.Transform.GetComponent<Graphic>();
				Color color = component.color;
				color.a = value.ToUniqueValue;
				component.color = color;
				break;
			}
			default:
				Log.GUIError("Effect not supported :" + value.Animation);
				break;
			}
		}
	}

	private void OnDisable()
	{
		StopLoop();
	}

	private void CreateUndo()
	{
		if (undoEffects != null)
		{
			return;
		}
		undoEffects = new Dictionary<AnimationSetting, AnimationSetting>();
		foreach (AnimationSetting effect in effects)
		{
			switch (effect.Animation)
			{
			case AnimationSetting.LegacyTweenAction.SCALE:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.localScale, effect.Duration, effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.MOVE:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.anchoredPosition, effect.Duration, effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.MOVE_LOCAL:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.localPosition, effect.Duration, effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.CANVASGROUP_ALPHA:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.UseSpecificFromValue ? effect.FromUniqueValue : effect.Transform.GetComponent<CanvasGroup>().alpha, effect.Duration, effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.CANVAS_ALPHA:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.UseSpecificFromValue ? effect.FromUniqueValue : effect.Transform.GetComponent<Graphic>().color.a, effect.Duration, effect.AnimationType);
				break;
			default:
				Log.GUIError("Effect not supported :" + effect.Animation);
				break;
			}
		}
	}

	private Tween Animate(AnimationSetting effect)
	{
		Tween tween = null;
		switch (effect.Animation)
		{
		case AnimationSetting.LegacyTweenAction.SCALE:
			tween = effect.Transform.DOScale(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
			break;
		case AnimationSetting.LegacyTweenAction.MOVE:
			tween = effect.Transform.DOAnchorPos(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
			break;
		case AnimationSetting.LegacyTweenAction.MOVE_LOCAL:
			tween = effect.Transform.DOLocalMove(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
			break;
		case AnimationSetting.LegacyTweenAction.CANVASGROUP_ALPHA:
			tween = effect.Transform.GetComponent<CanvasGroup>().DOFade(effect.ToUniqueValue, effect.Duration).SetEase(effect.AnimationType);
			break;
		case AnimationSetting.LegacyTweenAction.CANVAS_ALPHA:
			tween = effect.Transform.GetComponent<Graphic>().DOFade(effect.ToUniqueValue, effect.Duration).SetEase(effect.AnimationType);
			break;
		default:
			Log.GUIError("Effect not supported :" + effect.Animation);
			break;
		}
		tween?.SetUpdate(ignoreTimeScale);
		return tween;
	}

	private void Loop(AnimationSetting effect)
	{
		PlayEffect(effect, () =>
		{
			PlayEffect(undoEffects[effect], () =>
			{
				Loop(effect);
			});
		});
	}

	private void PlayEffect(AnimationSetting effect, Action onComplete)
	{
		if (looping)
		{
			Tween anim = Animate(effect);
			anim.OnComplete(() =>
			{
				currentAnim.Remove(anim);
				onComplete?.Invoke();
			});
			currentAnim.Add(anim);
		}
	}
}
