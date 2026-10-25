using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	private struct HoverEffectSettings(RectTransform transform, AnimationSetting.LegacyTweenAction animation, Vector3 toValue, float duration, Ease AnimationType, float uniquevalue = 0f)
	{
		public RectTransform Transform = transform;

		public AnimationSetting.LegacyTweenAction Animation = animation;

		public Vector3 ToValue = toValue;

		public float ToUniqueValue = uniquevalue;

		public float Duration = duration;

		public Ease AnimationType = AnimationType;

		public HoverEffectSettings(HoverEffectSettings effect, Vector3 originalValue)
			: this(effect.Transform, effect.Animation, originalValue, effect.Duration, effect.AnimationType)
		{
		}

		public HoverEffectSettings(HoverEffectSettings effect, float originalValue)
			: this(effect.Transform, effect.Animation, Vector3.zero, effect.Duration, effect.AnimationType, originalValue)
		{
		}
	}

	[SerializeField]
	private List<HoverEffectSettings> effects;

	private List<HoverEffectSettings> undoEffects = new List<HoverEffectSettings>();

	private List<Tween> animationsInProcess = new List<Tween>();

	private void Awake()
	{
		Reset();
	}

	private void CalculateUndoEffects()
	{
		foreach (HoverEffectSettings effect in effects)
		{
			switch (effect.Animation)
			{
			case AnimationSetting.LegacyTweenAction.SCALE:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.localScale));
				break;
			case AnimationSetting.LegacyTweenAction.MOVE:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.anchoredPosition));
				break;
			case AnimationSetting.LegacyTweenAction.MOVE_LOCAL:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.localPosition));
				break;
			case AnimationSetting.LegacyTweenAction.CANVASGROUP_ALPHA:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.GetComponent<CanvasGroup>().alpha));
				break;
			default:
				Log.GUIError("Effect not supported :" + effect.Animation);
				break;
			}
		}
	}

	public void Reset()
	{
		undoEffects.Clear();
		CalculateUndoEffects();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Cancel();
		Animate(effects);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cancel();
		if (undoEffects.Count == 0)
		{
			Reset();
		}
		Animate(undoEffects);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Cancel();
		Animate(effects);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Cancel();
		if (undoEffects.Count == 0)
		{
			Reset();
		}
		Animate(undoEffects);
	}

	private void Animate(IEnumerable<HoverEffectSettings> effects)
	{
		foreach (HoverEffectSettings effect in effects)
		{
			Tween anim = null;
			switch (effect.Animation)
			{
			case AnimationSetting.LegacyTweenAction.SCALE:
				anim = effect.Transform.DOScale(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.MOVE:
				anim = effect.Transform.DOAnchorPos(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.MOVE_LOCAL:
				anim = effect.Transform.DOLocalMove(effect.ToValue, effect.Duration).SetEase(effect.AnimationType);
				break;
			case AnimationSetting.LegacyTweenAction.CANVASGROUP_ALPHA:
				anim = effect.Transform.GetComponent<CanvasGroup>().DOFade(effect.ToUniqueValue, effect.Duration).SetEase(effect.AnimationType);
				break;
			default:
				Log.GUIError("Effect not supported :" + effect.Animation);
				break;
			}
			if (anim != null)
			{
				anim.OnComplete(() =>
				{
					animationsInProcess.Remove(anim);
				});
				animationsInProcess.Add(anim);
			}
		}
	}

	public void Cancel()
	{
		if (animationsInProcess == null)
		{
			return;
		}
		foreach (Tween item in animationsInProcess)
		{
			item.Kill();
		}
		animationsInProcess.Clear();
	}
}
