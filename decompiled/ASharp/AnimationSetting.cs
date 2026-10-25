using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public struct AnimationSetting
{
	public enum LegacyTweenAction
	{
		MOVE_X,
		MOVE_Y,
		MOVE_Z,
		MOVE_LOCAL_X,
		MOVE_LOCAL_Y,
		MOVE_LOCAL_Z,
		MOVE_CURVED,
		MOVE_CURVED_LOCAL,
		MOVE_SPLINE,
		MOVE_SPLINE_LOCAL,
		SCALE_X,
		SCALE_Y,
		SCALE_Z,
		ROTATE_X,
		ROTATE_Y,
		ROTATE_Z,
		ROTATE_AROUND,
		ROTATE_AROUND_LOCAL,
		CANVAS_ROTATEAROUND,
		CANVAS_ROTATEAROUND_LOCAL,
		CANVAS_PLAYSPRITE,
		ALPHA,
		TEXT_ALPHA,
		CANVAS_ALPHA,
		CANVASGROUP_ALPHA,
		ALPHA_VERTEX,
		COLOR,
		CALLBACK_COLOR,
		TEXT_COLOR,
		CANVAS_COLOR,
		CANVAS_MOVE_X,
		CANVAS_MOVE_Y,
		CANVAS_MOVE_Z,
		CALLBACK,
		MOVE,
		MOVE_LOCAL,
		MOVE_TO_TRANSFORM,
		ROTATE,
		ROTATE_LOCAL,
		SCALE,
		VALUE3,
		GUI_MOVE,
		GUI_MOVE_MARGIN,
		GUI_SCALE,
		GUI_ALPHA,
		GUI_ROTATE,
		DELAYED_SOUND,
		CANVAS_MOVE,
		CANVAS_SCALE,
		CANVAS_SIZEDELTA
	}

	public RectTransform Transform;

	public LegacyTweenAction Animation;

	public Vector3 ToValue;

	public float ToUniqueValue;

	[ConditionalField("UseSpecificFromValue", "true", true)]
	public float FromUniqueValue;

	public bool UseSpecificFromValue;

	public float Duration;

	public Ease AnimationType;

	public AnimationSetting(RectTransform transform, LegacyTweenAction animation, Vector3 toValue, float duration, Ease animationType)
	{
		Transform = transform;
		ToValue = toValue;
		Duration = duration;
		AnimationType = animationType;
		Animation = animation;
		ToUniqueValue = 0f;
		FromUniqueValue = 0f;
		UseSpecificFromValue = false;
	}

	private AnimationSetting(RectTransform transform, LegacyTweenAction animation, float toUniqueValue, float fromUniqueValue, bool useSpecificFromValue, float duration, Ease animationType)
	{
		Transform = transform;
		ToValue = Vector3.zero;
		Duration = duration;
		AnimationType = animationType;
		Animation = animation;
		ToUniqueValue = toUniqueValue;
		FromUniqueValue = fromUniqueValue;
		UseSpecificFromValue = useSpecificFromValue;
	}

	public AnimationSetting(RectTransform transform, LegacyTweenAction animation, float toUniqueValue, float duration, Ease animationType)
		: this(transform, animation, toUniqueValue, 0f, useSpecificFromValue: false, duration, animationType)
	{
	}

	public AnimationSetting(RectTransform transform, LegacyTweenAction animation, float toUniqueValue, float fromUniqueValue, float duration, Ease animationType)
		: this(transform, animation, toUniqueValue, fromUniqueValue, useSpecificFromValue: true, duration, animationType)
	{
	}
}
