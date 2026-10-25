using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FHGUI.Tweening;

public class UITweenTextChange : MonoBehaviour
{
	[SerializeField]
	private UITweenDef m_TweenDef;

	private Vector3 _initialScale;

	private TMP_Text _text;

	private RectTransform _rt;

	private Tween _tween;

	private bool _skipNextChange;

	private string _lastText = string.Empty;

	private void Awake()
	{
		_text = GetComponent<TMP_Text>();
		_rt = GetComponent<RectTransform>();
		_initialScale = _rt.localScale;
	}

	private void OnEnable()
	{
		_lastText = _text.text ?? "";
	}

	private void OnDisable()
	{
		_tween?.Kill();
	}

	private void LateUpdate()
	{
		DoTextCheck();
		_tween?.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
	}

	private void DoTextCheck()
	{
		if (_lastText == null)
		{
			_lastText = _text.text;
		}
		else if (_lastText.Equals(_text.text, StringComparison.InvariantCulture))
		{
			return;
		}
		_lastText = _text.text;
		if (_skipNextChange)
		{
			_skipNextChange = false;
		}
		else
		{
			StartTween();
		}
	}

	private void StartTween()
	{
		_tween?.Kill();
		if (m_TweenDef != null)
		{
			_tween = m_TweenDef.StartTween(_rt, ResetScale, OnKill, ResetScale).SetUpdate(UpdateType.Manual);
		}
	}

	private void OnKill()
	{
		_tween = null;
		ResetScale();
	}

	private void ResetScale()
	{
		m_TweenDef.SnapToEndValue(_rt);
		_rt.localScale = _initialScale;
	}

	public void SkipNextChange()
	{
		_skipNextChange = true;
	}
}
