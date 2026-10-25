using System;
using SETUtil.Materials;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening;

public class DOTweenAnimationExtended : DOTweenAnimation
{
	[Serializable]
	public class TweenValues
	{
		public float valueFloat;

		public Vector3 valueV3;

		public Vector2 valueV2;

		public Color valueColor = new Color(1f, 1f, 1f, 1f);

		public string valueString = "";

		public Rect valueRect = new Rect(0f, 0f, 0f, 0f);
	}

	public bool setStartValue;

	public TweenValues startValues;

	private TweenValues _initialValues;

	protected override void PreCreateHook()
	{
		if (setStartValue)
		{
			CachePreviewValues();
			SetValue(startValues);
		}
	}

	protected override void PostCreateHook()
	{
		Tween tween = base.tween;
		tween.onKill = (TweenCallback)Delegate.Combine(tween.onKill, new TweenCallback(__OnPreviewStop));
	}

	public override void __OnPreviewStop()
	{
		if (setStartValue)
		{
			SetValue(_initialValues);
		}
	}

	private void SetValue(TweenValues values)
	{
		switch (animationType)
		{
		case AnimationType.Move:
			((Transform)target).position = values.valueV3;
			break;
		case AnimationType.LocalMove:
			((Transform)target).localPosition = values.valueV3;
			break;
		case AnimationType.Scale:
			((Transform)target).localScale = (optionalBool0 ? new Vector3(values.valueFloat, values.valueFloat, values.valueFloat) : values.valueV3);
			break;
		case AnimationType.UIWidthHeight:
		{
			Vector2 sizeDelta = (optionalBool0 ? new Vector2(values.valueFloat, values.valueFloat) : values.valueV2);
			((RectTransform)target).sizeDelta = sizeDelta;
			break;
		}
		case AnimationType.Color:
			switch (targetType)
			{
			case TargetType.Renderer:
				((Renderer)target).GetMaterialClonesSlow()[0].color = values.valueColor;
				break;
			case TargetType.Light:
				((Light)target).color = values.valueColor;
				break;
			case TargetType.SpriteRenderer:
				((SpriteRenderer)target).color = values.valueColor;
				break;
			case TargetType.Image:
				((Graphic)target).color = values.valueColor;
				break;
			case TargetType.Text:
				((Text)target).color = values.valueColor;
				break;
			case TargetType.RectTransform:
			case TargetType.Rigidbody:
			case TargetType.Rigidbody2D:
				break;
			}
			break;
		case AnimationType.Fade:
			switch (targetType)
			{
			case TargetType.Renderer:
			{
				Color color = ((Renderer)target).GetMaterialClonesSlow()[0].color;
				color.a = values.valueFloat;
				((Renderer)target).GetMaterialClonesSlow()[0].color = color;
				break;
			}
			case TargetType.Light:
			{
				Color color = ((Light)target).color;
				color.a = values.valueFloat;
				((Light)target).color = color;
				break;
			}
			case TargetType.SpriteRenderer:
			{
				Color color = ((SpriteRenderer)target).color;
				color.a = values.valueFloat;
				((SpriteRenderer)target).color = color;
				break;
			}
			case TargetType.Image:
			{
				Color color = ((Graphic)target).color;
				color.a = values.valueFloat;
				((Graphic)target).color = color;
				break;
			}
			case TargetType.Text:
			{
				Color color = ((Text)target).color;
				color.a = values.valueFloat;
				((Text)target).color = color;
				break;
			}
			case TargetType.CanvasGroup:
				((CanvasGroup)target).alpha = values.valueFloat;
				break;
			case TargetType.RectTransform:
			case TargetType.Rigidbody:
			case TargetType.Rigidbody2D:
				break;
			}
			break;
		case AnimationType.Text:
			if (targetType == TargetType.Text)
			{
				((Text)target).text = values.valueString;
			}
			break;
		default:
			Debug.LogError($"Unsupported tween start type {animationType} on {name}", this);
			break;
		}
	}

	private void CachePreviewValues()
	{
		_initialValues = new TweenValues();
		switch (animationType)
		{
		case AnimationType.Move:
			_initialValues.valueV3 = ((Transform)target).position;
			break;
		case AnimationType.LocalMove:
			_initialValues.valueV3 = ((Transform)target).localPosition;
			break;
		case AnimationType.Scale:
			_initialValues.valueV3 = ((Transform)target).localScale;
			break;
		case AnimationType.UIWidthHeight:
			_initialValues.valueV2 = ((RectTransform)target).sizeDelta;
			break;
		case AnimationType.Color:
			switch (targetType)
			{
			case TargetType.Renderer:
				_initialValues.valueColor = ((Renderer)target).GetMaterialClonesSlow()[0].color;
				break;
			case TargetType.Light:
				_initialValues.valueColor = ((Light)target).color;
				break;
			case TargetType.SpriteRenderer:
				_initialValues.valueColor = ((SpriteRenderer)target).color;
				break;
			case TargetType.Image:
				_initialValues.valueColor = ((Graphic)target).color;
				break;
			case TargetType.Text:
				_initialValues.valueColor = ((Text)target).color;
				break;
			case TargetType.RectTransform:
			case TargetType.Rigidbody:
			case TargetType.Rigidbody2D:
				break;
			}
			break;
		case AnimationType.Fade:
			switch (targetType)
			{
			case TargetType.Renderer:
				_initialValues.valueFloat = ((Renderer)target).GetMaterialClonesSlow()[0].color.a;
				break;
			case TargetType.Light:
				_initialValues.valueFloat = ((Light)target).color.a;
				break;
			case TargetType.SpriteRenderer:
				_initialValues.valueFloat = ((SpriteRenderer)target).color.a;
				break;
			case TargetType.Image:
				_initialValues.valueFloat = ((Graphic)target).color.a;
				break;
			case TargetType.Text:
				_initialValues.valueFloat = ((Text)target).color.a;
				break;
			case TargetType.CanvasGroup:
				_initialValues.valueFloat = ((CanvasGroup)target).alpha;
				break;
			case TargetType.RectTransform:
			case TargetType.Rigidbody:
			case TargetType.Rigidbody2D:
				break;
			}
			break;
		case AnimationType.Text:
			if (targetType == TargetType.Text)
			{
				_initialValues.valueString = ((Text)target).text;
			}
			break;
		default:
			Debug.LogError($"Unsupported tween start type {animationType} on {name}", this);
			break;
		}
	}
}
