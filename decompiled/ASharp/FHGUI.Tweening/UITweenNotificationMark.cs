using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

public class UITweenNotificationMark : MonoBehaviour
{
	[SerializeField]
	private UIScaleTweenDef m_ParamsOnEnable;

	[SerializeField]
	private UIScaleTweenDef m_ParamsPulse;

	private Vector3 _initialScale;

	private RectTransform _rt;

	private Tween _tween;

	private void Awake()
	{
		_rt = GetComponent<RectTransform>();
		_initialScale = _rt.localScale;
	}

	private void OnEnable()
	{
		if (m_ParamsOnEnable != null)
		{
			ApplyTweenParams(m_ParamsOnEnable);
			_tween.OnComplete(() =>
			{
				ApplyTweenParams(m_ParamsPulse);
			});
		}
		else if (m_ParamsPulse != null)
		{
			ApplyTweenParams(m_ParamsPulse);
		}
	}

	private void ApplyTweenParams(UIScaleTweenDef paramsDef)
	{
		if (!(paramsDef == null))
		{
			_rt.localScale = _initialScale;
			_tween = paramsDef.StartTween(_rt);
		}
	}

	private void OnDisable()
	{
		_tween?.Kill();
		_tween = null;
	}
}
