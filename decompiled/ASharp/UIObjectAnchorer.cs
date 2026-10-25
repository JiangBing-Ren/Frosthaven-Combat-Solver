using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Libs.Utils.Maths;
using UnityEngine;

public class UIObjectAnchorer : MonoBehaviour
{
	[Serializable]
	public struct AnchorSetting
	{
		public RectTransform Transform;

		[Tooltip("Leave empty if you'd like to use the fallback point instead.")]
		public RectTransform AnchorPoint;
	}

	[Header("Settings")]
	[SerializeField]
	private AnchorSetting[] m_AnchorSettings;

	[Tooltip("Will be used if there is no target point specified in the settings.")]
	[SerializeField]
	private RectTransform m_FallbackAnchorPoint;

	[Min(0f)]
	[SerializeField]
	private float m_MovementTime = 0.3f;

	private bool _isAnchored;

	private readonly Dictionary<RectTransform, Vector2> _transformStartingAnchoredPositions = new Dictionary<RectTransform, Vector2>();

	private readonly Dictionary<RectTransform, Vector2> _transformTargetAnchoredPositions = new Dictionary<RectTransform, Vector2>();

	private readonly Dictionary<RectTransform, Tween> _transformTweens = new Dictionary<RectTransform, Tween>();

	private bool IsInstant => m_MovementTime.IsZero();

	public void ToggleAnchor()
	{
		AnchorSetting[] anchorSettings;
		if (!_transformStartingAnchoredPositions.Any())
		{
			anchorSettings = m_AnchorSettings;
			for (int i = 0; i < anchorSettings.Length; i++)
			{
				AnchorSetting anchorSetting = anchorSettings[i];
				_transformStartingAnchoredPositions[anchorSetting.Transform] = anchorSetting.Transform.anchoredPosition;
			}
		}
		anchorSettings = m_AnchorSettings;
		for (int i = 0; i < anchorSettings.Length; i++)
		{
			AnchorSetting anchorSetting2 = anchorSettings[i];
			RectTransform srt = anchorSetting2.Transform;
			Vector2 vector = ((anchorSetting2.AnchorPoint != null) ? anchorSetting2.AnchorPoint.anchoredPosition : m_FallbackAnchorPoint.anchoredPosition);
			Vector2 vector2 = _transformStartingAnchoredPositions[srt];
			Vector2 anchoredPosition = srt.anchoredPosition;
			Vector2 targetPos = (_isAnchored ? vector2 : vector);
			if (IsInstant)
			{
				snapToTarget();
				continue;
			}
			float num = Vector2.Distance(anchoredPosition, targetPos);
			if (num.IsZero())
			{
				continue;
			}
			float num2 = Vector2.Distance(vector2, vector);
			if (!num2.IsZero())
			{
				float num3 = num / num2;
				if (_transformTweens.TryGetValue(srt, out var value))
				{
					value?.Kill();
				}
				float duration = m_MovementTime * num3;
				_transformTweens[srt] = srt.DOAnchorPos(targetPos, duration).OnComplete(snapToTarget);
			}
			void snapToTarget()
			{
				srt.anchoredPosition = targetPos;
			}
		}
		_isAnchored = !_isAnchored;
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<RectTransform, Tween> transformTween in _transformTweens)
		{
			transformTween.Value?.Complete();
		}
	}
}
