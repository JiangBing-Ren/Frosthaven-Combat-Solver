using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

public class UITweenEffects : MonoBehaviour
{
	private enum ResetReason
	{
		None,
		Reset,
		Start,
		Kill
	}

	[SerializeField]
	private List<UITweenDef> m_TweenDefs = new List<UITweenDef>();

	[SerializeField]
	private bool m_PlayOnEnable = true;

	[SerializeField]
	private bool m_ResetTransformOnStart;

	[SerializeField]
	private bool m_ResetTransformOnEnd;

	[SerializeField]
	private float m_DurationOverride = -1f;

	private Vector3 _initialScale;

	private Vector2 _initialAnchoredPos;

	private RectTransform _rt;

	private bool _isInit;

	private bool _playOnLateUpdate;

	private bool _isInLateUpdate;

	private readonly List<Tween> _tweens = new List<Tween>();

	private void Init()
	{
		if (!_isInit)
		{
			_rt = GetComponent<RectTransform>();
			_initialScale = _rt.localScale;
			_initialAnchoredPos = _rt.anchoredPosition;
			_isInit = true;
		}
	}

	private void OnEnable()
	{
		Init();
		if (m_PlayOnEnable)
		{
			_playOnLateUpdate = true;
			if (UIManager.IsLateActiveStatePhase)
			{
				LateUpdate();
			}
		}
	}

	private void LateUpdate()
	{
		_isInLateUpdate = true;
		if (_playOnLateUpdate)
		{
			Play();
			_playOnLateUpdate = false;
		}
		for (int num = _tweens.Count - 1; num >= 0; num--)
		{
			if (_tweens[num].active)
			{
				_tweens[num].ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
			}
			else
			{
				_tweens.RemoveAt(num);
				if (_tweens.Count == 0 && m_ResetTransformOnEnd)
				{
					ResetTransform(ResetReason.Kill);
				}
			}
		}
		_isInLateUpdate = false;
	}

	public void Play()
	{
		Init();
		Reset();
		if (m_ResetTransformOnStart)
		{
			ResetTransform(ResetReason.Start);
		}
		foreach (UITweenDef item in m_TweenDefs.Where((UITweenDef d) => d != null))
		{
			Tween tween = item.StartTween(_rt, null, null, null, m_DurationOverride)?.SetUpdate(UpdateType.Manual);
			if (tween != null)
			{
				_tweens.Add(tween);
			}
		}
	}

	public bool IsActive()
	{
		if (_tweens == null || _tweens.Count == 0)
		{
			return false;
		}
		foreach (Tween tween in _tweens)
		{
			if (tween.IsActive())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAboutToPlay()
	{
		return _playOnLateUpdate;
	}

	public void Reset()
	{
		if (_tweens.Count == 0)
		{
			return;
		}
		foreach (Tween item in _tweens.ToList())
		{
			item?.Kill();
		}
		_tweens.Clear();
		if (m_ResetTransformOnEnd)
		{
			ResetTransform(ResetReason.Reset);
		}
	}

	private void ResetTransform(ResetReason reason)
	{
		_rt.localScale = _initialScale;
		_rt.anchoredPosition = _initialAnchoredPos;
	}

	private void OnDisable()
	{
		Reset();
	}

	public static void PlayAllOnTransform(RectTransform rt, bool forceReplayAlreadyPlaying = false)
	{
		UITweenEffects[] components = rt.GetComponents<UITweenEffects>();
		foreach (UITweenEffects uITweenEffects in components)
		{
			if (forceReplayAlreadyPlaying || !uITweenEffects._tweens.Any((Tween t) => t.active))
			{
				uITweenEffects.Play();
			}
		}
	}

	public static void UpdateInitialAnchoredPosition(RectTransform rt)
	{
		Vector2 anchoredPosition = rt.anchoredPosition;
		UITweenEffects[] components = rt.GetComponents<UITweenEffects>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i]._initialAnchoredPos = anchoredPosition;
		}
	}

	public static void UpdateInitialScale(RectTransform rt)
	{
		Vector3 localScale = rt.localScale;
		UITweenEffects[] components = rt.GetComponents<UITweenEffects>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i]._initialScale = localScale;
		}
	}
}
