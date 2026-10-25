using System;
using UnityEngine;

namespace FHGUI.Common;

[Serializable]
public struct UIImageSettings
{
	public Vector2 OffsetMin;

	public Vector2 OffsetMax;

	public Vector2 AnchoredPosition;

	public Vector3 LocalScale;

	public static UIImageSettings Default => new UIImageSettings(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

	public bool HasChangesToApply
	{
		get
		{
			if (!(OffsetMin.sqrMagnitude > 0f) && !(OffsetMax.sqrMagnitude > 0f) && !(AnchoredPosition.sqrMagnitude > 0f))
			{
				return LocalScale != Vector3.one;
			}
			return true;
		}
	}

	public UIImageSettings(Vector2 anchoredPos, Vector2 offsetMin, Vector2 offsetMax, Vector3 localScale)
	{
		AnchoredPosition = anchoredPos;
		OffsetMin = offsetMin;
		OffsetMax = offsetMax;
		LocalScale = localScale;
	}

	public UIImageSettings(RectTransform rectTransform)
	{
		AnchoredPosition = rectTransform.anchoredPosition;
		OffsetMin = rectTransform.offsetMin;
		OffsetMax = rectTransform.offsetMax;
		LocalScale = rectTransform.localScale;
	}
}
