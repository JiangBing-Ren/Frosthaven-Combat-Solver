using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DOTweenGuiAnimationSettingMove : DOTweenGUIAnimationSetting<Vector3, GUIAnimationMoveType, RectTransform>
{
	protected override Tween BuildTweenAction()
	{
		Tween result = null;
		switch (Animation)
		{
		case GUIAnimationMoveType.MOVE:
			result = Target.DOAnchorPos(ToValue, Duration);
			break;
		case GUIAnimationMoveType.MOVE_LOCAL:
			result = Target.DOLocalMove(ToValue, Duration);
			break;
		case GUIAnimationMoveType.SIZE_DELTA:
			result = Target.DOSizeDelta(ToValue, Duration);
			break;
		case GUIAnimationMoveType.UV:
		{
			RawImage image = Target.GetComponent<RawImage>();
			if (image == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGuiAnimationSettingMove: Target ({0}) doesn't have a RawImage component", Target.name);
				return null;
			}
			Rect rect = image.uvRect;
			Vector3 startPos = rect.position;
			Vector3 dist = ToValue - (Vector3)rect.position;
			result = DOTween.To((float val) =>
			{
				rect.position = val * dist + startPos;
				image.uvRect = rect;
			}, 0f, 1f, Duration).SetTarget(Target);
			break;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			break;
		}
		return result;
	}

	protected override void SetValue(Vector3 value)
	{
		switch (Animation)
		{
		case GUIAnimationMoveType.MOVE:
			Target.anchoredPosition = value;
			break;
		case GUIAnimationMoveType.MOVE_LOCAL:
			Target.localPosition = value;
			break;
		case GUIAnimationMoveType.SIZE_DELTA:
			Target.sizeDelta = value;
			break;
		case GUIAnimationMoveType.UV:
		{
			RawImage component = Target.GetComponent<RawImage>();
			if (component == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGuiAnimationSettingMove: Target ({0}) doesn't have a RawImage component", Target.name);
			}
			else
			{
				Rect uvRect = component.uvRect;
				uvRect.position = value;
				component.uvRect = uvRect;
			}
			break;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			break;
		}
	}

	public DOTweenGuiAnimationSettingMove(RectTransform target, GUIAnimationMoveType type, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target, type, duration, delay, easing)
	{
	}

	public DOTweenGuiAnimationSettingMove(Graphic target, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target.rectTransform, GUIAnimationMoveType.UV, duration, delay, easing)
	{
	}
}
