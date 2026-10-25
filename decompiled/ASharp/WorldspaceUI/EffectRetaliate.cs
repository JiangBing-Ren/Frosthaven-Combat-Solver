using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class EffectRetaliate : MonoBehaviour
{
	[SerializeField]
	private Image _retaliateRingImage;

	[SerializeField]
	private TMP_Text _retaliateDamageText;

	[SerializeField]
	private Image _retaliateRangeImage;

	[SerializeField]
	private TMP_Text _retaliateRangeText;

	[SerializeField]
	private LayoutElement _layoutElement;

	private bool _isInit;

	private bool _effectDeactivated;

	private bool _isWarningActive;

	private RectTransform _ringRectTransform;

	public int Damage { get; private set; }

	public int Range { get; private set; }

	public void Initialise()
	{
		if (!_isInit)
		{
			_effectDeactivated = false;
			_ringRectTransform = _retaliateRingImage.GetComponent<RectTransform>();
			SetValues(0, 0);
			_isInit = true;
		}
	}

	public void SetValues(int damage, int range)
	{
		Damage = damage;
		Range = range;
		float num = 1f;
		if ((bool)_retaliateDamageText)
		{
			_retaliateDamageText.text = damage.ToString();
			num++;
		}
		bool flag = range > 1;
		if ((bool)_retaliateRangeText)
		{
			_retaliateRangeText.text = range.ToString();
			_retaliateRangeText.gameObject.SetActive(flag);
			if (flag)
			{
				num++;
			}
		}
		if ((bool)_retaliateRangeImage)
		{
			_retaliateRangeImage.gameObject.SetActive(flag);
			if (flag)
			{
				num++;
			}
		}
		if ((bool)_layoutElement)
		{
			float num2 = _layoutElement.minHeight * num;
			_layoutElement.minWidth = num2;
			_layoutElement.preferredWidth = num2;
		}
	}

	public void Activate()
	{
		PlayActivated();
	}

	public void Warn(bool active)
	{
		if (active)
		{
			_isWarningActive = true;
			PlayWarning();
		}
		else
		{
			_isWarningActive = false;
		}
	}

	[ContextMenu("PlayWarning")]
	private void PlayWarning()
	{
		if (_isWarningActive && !_effectDeactivated)
		{
			Vector3 fromValue = new Vector3(0.9f, 0.9f, 0.9f);
			Vector3 endValue = new Vector3(1.05f, 1.05f, 1.05f);
			DOTween.Kill(_ringRectTransform);
			DOTween.Sequence(_ringRectTransform).Append(_retaliateRingImage.DOFade(1f, 0.5f).From(0f)).Insert(0f, _ringRectTransform.DOScale(endValue, 0.5f).From(fromValue))
				.SetLoops(-1, LoopType.Yoyo)
				.OnKill(ResetRingImage);
		}
	}

	private void ResetRingImage()
	{
		Color color = _retaliateRingImage.color;
		color.a = 0f;
		_retaliateRingImage.color = color;
	}

	[ContextMenu("PlayActivated")]
	public void PlayActivated()
	{
		if (!_effectDeactivated)
		{
			if (_isWarningActive)
			{
				DOTween.Kill(_ringRectTransform);
				_isWarningActive = false;
			}
			DOTween.Kill(_ringRectTransform);
			DOTween.Sequence(_ringRectTransform).Append(_retaliateRingImage.DOFade(1f, 0.5f).From(0f)).Insert(0f, _ringRectTransform.DOScale(Vector3.one * 1.1f, 0.5f).From(new Vector3(0.9f, 0.9f, 0.9f)))
				.SetLoops(3, LoopType.Yoyo)
				.OnKill(ResetRingImage);
		}
	}

	public void CancelEffectRetaliateTweens()
	{
		_effectDeactivated = true;
		DOTween.Kill(_ringRectTransform);
	}

	private void OnDisable()
	{
		CancelEffectRetaliateTweens();
	}
}
