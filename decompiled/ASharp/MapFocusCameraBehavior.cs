using System;
using DG.Tweening;
using UnityEngine;

public class MapFocusCameraBehavior : MoveAndZoomCameraBehavior
{
	public const string FOCUS_ID = "MAP_FOCUS";

	public MapFocusCameraBehavior(float? zoomTo, Vector3? moveTo, float duration, Action onFinished = null)
		: base("MAP_FOCUS", zoomTo, moveTo, duration, Ease.Unset, null, onFinished)
	{
	}

	public MapFocusCameraBehavior(float zoomTo, float duration, Action onFinished = null)
		: this(zoomTo, null, duration, onFinished)
	{
	}

	public MapFocusCameraBehavior(Vector3 moveTo, float duration, Action onFinished = null)
		: this(null, moveTo, duration, onFinished)
	{
	}
}
