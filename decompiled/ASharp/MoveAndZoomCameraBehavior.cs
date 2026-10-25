using System;
using DG.Tweening;
using UnityEngine;

public class MoveAndZoomCameraBehavior : CameraBehavior
{
	private float? zoomTo;

	private Vector3? moveTo;

	private float duration;

	private AnimationCurve animationCurve;

	private Action onFinished;

	private Ease easeType;

	private Tween anim;

	public MoveAndZoomCameraBehavior(string id, float? zoomTo, Vector3? moveTo, float duration, Ease easeType = Ease.Unset, AnimationCurve animationCurve = null, Action onFinished = null)
		: base(id)
	{
		this.zoomTo = zoomTo;
		this.moveTo = moveTo;
		this.animationCurve = animationCurve;
		this.duration = duration;
		this.onFinished = onFinished;
		this.easeType = easeType;
	}

	public MoveAndZoomCameraBehavior(string id, float zoomTo, float duration, Action onFinished = null, Ease easeType = Ease.Unset, AnimationCurve animationCurve = null)
		: this(id, zoomTo, null, duration, easeType, animationCurve, onFinished)
	{
	}

	public MoveAndZoomCameraBehavior(string id, Vector3 moveTo, float duration, Action onFinished = null, Ease easeType = Ease.Unset, AnimationCurve animationCurve = null)
		: this(id, null, moveTo, duration, easeType, animationCurve, onFinished)
	{
	}

	private void Cancel()
	{
		if (anim != null)
		{
			Log.GUIInfo("Cancel  Move and Zoom Camera Behavior animation");
			anim.Kill();
			anim = null;
		}
	}

	public override void Disable(CameraController camera)
	{
		if (IsEnabled && anim != null)
		{
			camera.DisableCameraInput(disableInput: false, this);
			if (moveTo.HasValue)
			{
				float? num = camera.CalculateZoomForY(camera.m_Camera.transform.position.y);
				if (num.HasValue)
				{
					camera.SetZoom(num.Value);
				}
			}
		}
		Cancel();
		base.Disable(camera);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Cancel();
	}

	public override bool IsType(ECameraBehaviorType type)
	{
		if (type == ECameraBehaviorType.Move && moveTo.HasValue)
		{
			return true;
		}
		if (type == ECameraBehaviorType.Zoom && zoomTo.HasValue)
		{
			return true;
		}
		return false;
	}

	public override void Enable(CameraController camera)
	{
		base.Enable(camera);
		if (moveTo.HasValue)
		{
			moveTo = new Vector3(Mathf.Max(camera.m_FocalBounds.min.x, Mathf.Min(camera.m_FocalBounds.max.x, moveTo.Value.x)), moveTo.Value.y, Mathf.Max(camera.m_FocalBounds.min.z, Mathf.Min(camera.m_FocalBounds.max.z, moveTo.Value.z)));
		}
		Cancel();
		if (duration <= 0f)
		{
			OnFinished(camera);
			return;
		}
		Vector3 startFocal = camera.FocusPoint;
		float startFOV = camera.m_Camera.fieldOfView;
		Vector3 startPos = camera.m_Camera.transform.position;
		float yDist = 0f;
		if (zoomTo.HasValue)
		{
			yDist = camera.CalculateYForZoom(zoomTo.Value) - startPos.y;
		}
		camera.DisableCameraInput(disableInput: true, this);
		anim = DOTween.To((float tSmooth) =>
		{
			float? y = null;
			if (zoomTo.HasValue)
			{
				camera.SetZoom(Mathf.Lerp(startFOV, zoomTo.Value, tSmooth));
			}
			if (moveTo.HasValue)
			{
				camera.m_TargetFocalPoint = Vector3.Lerp(startFocal, moveTo.Value, tSmooth);
				y = startPos.y + yDist * tSmooth;
			}
			camera.RefreshPositionToFocusOnTargetPoint(y);
		}, 0f, 1f, duration).SetUpdate(isIndependentUpdate: true).SetTarget(camera.gameObject)
			.OnComplete(() =>
			{
				Log.GUIInfo("Finish Move and Zoom Camera Behavior");
				anim = null;
				camera.DisableCameraInput(disableInput: false, this);
				OnFinished(camera);
			});
		Log.GUIInfo("Create Move and Zoom Camera animation " + camera.name);
		if (animationCurve != null)
		{
			anim.SetEase(animationCurve);
		}
		else
		{
			anim.SetEase(easeType);
		}
	}

	private void OnFinished(CameraController camera)
	{
		if (zoomTo.HasValue)
		{
			camera.SetZoom(zoomTo.Value);
		}
		if (moveTo.HasValue)
		{
			camera.m_TargetFocalPoint = moveTo.Value;
		}
		camera.ResetPositionToFocusOnTargetPoint();
		onFinished?.Invoke();
	}
}
