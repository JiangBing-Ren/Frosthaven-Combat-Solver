using System;
using System.Collections;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldspaceUI;

public class AttackModElementUI : MonoBehaviour, IPooleable
{
	[NonSerialized]
	public AttackModifierYMLData m_Modifier;

	[SerializeField]
	private RectTransform m_IconRect;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private CanvasGroup m_IconGroup;

	[SerializeField]
	private RectTransform m_ValueRect;

	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	[SerializeField]
	private Image m_ValueImage;

	[SerializeField]
	public CanvasGroup m_ValueGroup;

	[Space]
	[SerializeField]
	private CanvasGroup m_ReplaceModifierCanvasGroup;

	[SerializeField]
	private CanvasGroup m_ReplaceModifierGlow;

	[SerializeField]
	private Image m_ReplaceModifierShine;

	[Space]
	[SerializeField]
	private ParticleSystem m_NeutralPFX;

	[SerializeField]
	private ParticleSystem m_PositivePFX;

	[SerializeField]
	private ParticleSystem m_NegativePFX;

	[SerializeField]
	private ParticleSystem m_CriticalPFX;

	[SerializeField]
	private ParticleSystem m_NullPFX;

	[Space]
	[SerializeField]
	private Color m_HealColor;

	private IEnumerator m_IconScaleRoutine;

	private IEnumerator m_IconPivotRoutine;

	private IEnumerator m_FadeRoutine;

	private IEnumerator m_RollRoutine;

	private IEnumerator m_ValueHighlightRoutine;

	private Vector2 size;

	private void Awake()
	{
		size = GetComponent<RectTransform>().sizeDelta;
		m_ReplaceModifierShine.material = new Material(m_ReplaceModifierShine.material);
		Hide();
	}

	public void SetTextStyleForModifiers(AttackModifierYMLData modifier, Color color, bool showOriginalModifier = true)
	{
		if (modifier != null)
		{
			string attackModifierText = UIInfoTools.Instance.GetAttackModifierText(modifier, showOriginalModifier);
			m_ValueText.gameObject.SetActive(attackModifierText.IsNOTNullOrEmpty());
			m_ValueText.color = ((modifier.Card.Heal || modifier.Card.HealAlly) ? m_HealColor : color);
			m_ValueText.text = attackModifierText;
			Sprite attackModifierIcon = UIInfoTools.Instance.GetAttackModifierIcon(modifier, showOriginalModifier);
			m_ValueImage.gameObject.SetActive(attackModifierIcon != null);
			m_ValueImage.sprite = attackModifierIcon;
			m_ValueImage.color = (modifier.AddTarget ? color : UIInfoTools.Instance.White);
		}
		else
		{
			m_ValueText.color = Color.grey;
			m_ValueText.text = "--";
			m_ValueImage.gameObject.SetActive(value: false);
		}
	}

	public void PlayPfxForModifiers(EAttackModifierDamageColorCode code)
	{
		ParticleSystem particleSystem = null;
		switch (code)
		{
		case EAttackModifierDamageColorCode.CriticalDamage:
			particleSystem = m_CriticalPFX;
			break;
		case EAttackModifierDamageColorCode.ZeroDamage:
			particleSystem = m_NullPFX;
			break;
		case EAttackModifierDamageColorCode.PositiveDamage:
			particleSystem = m_PositivePFX;
			break;
		case EAttackModifierDamageColorCode.negativeDamage:
			particleSystem = m_NegativePFX;
			break;
		case EAttackModifierDamageColorCode.RegularDamage:
			particleSystem = m_NeutralPFX;
			break;
		}
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.Play();
	}

	public void Reset()
	{
		if (m_IconScaleRoutine != null)
		{
			StopCoroutine(m_IconScaleRoutine);
		}
		if (m_IconPivotRoutine != null)
		{
			StopCoroutine(m_IconPivotRoutine);
		}
		if (m_FadeRoutine != null)
		{
			StopCoroutine(m_FadeRoutine);
		}
		if (m_RollRoutine != null)
		{
			StopCoroutine(m_RollRoutine);
		}
		if (m_ValueHighlightRoutine != null)
		{
			StopCoroutine(m_ValueHighlightRoutine);
		}
		m_IconRect.localScale = Vector3.zero;
		m_IconRect.localRotation = Quaternion.identity;
		m_IconGroup.alpha = 0f;
		m_ValueRect.localRotation = Quaternion.Euler(0f, -180f, 0f);
		m_ValueGroup.alpha = 0f;
		m_ValueRect.anchoredPosition = Vector2.zero;
		m_ValueRect.localScale = Vector3.one;
		m_ReplaceModifierCanvasGroup.gameObject.SetActive(value: false);
		RectTransform obj = transform as RectTransform;
		Vector2 anchorMax = (obj.anchorMin = new Vector2(0.5f, 0.5f));
		obj.anchorMax = anchorMax;
		obj.sizeDelta = size;
	}

	public void ShowIcon(float initialIconScaleInDelay, float initialIconScaleInDuration, AnimationCurve initialIconScaleInCurve)
	{
		m_IconGroup.alpha = 1f;
		m_ValueGroup.alpha = 0f;
		m_IconRect.localScale = Vector3.zero;
		m_IconScaleRoutine = CorrutineUtils.RectTransformLocalScale(m_IconRect, initialIconScaleInDelay, initialIconScaleInDuration, Vector3.one, initialIconScaleInCurve);
		StartCoroutine(m_IconScaleRoutine);
	}

	public void ShowValue(float iconSwivelDuration, AnimationCurve iconSwivelCurve)
	{
		m_ValueGroup.alpha = 0f;
		m_IconRect.localRotation = Quaternion.identity;
		m_ValueRect.localRotation = Quaternion.Euler(0f, -180f, 0f);
		m_ValueRect.localScale = Vector3.one;
		m_IconPivotRoutine = CorrutineUtils.RectTransformPivotAroundVertical(m_IconRect, 0f, iconSwivelDuration, 180f, iconSwivelCurve, (float currentRotation) =>
		{
			m_ValueRect.localRotation = Quaternion.Euler(0f, -180f + currentRotation, 0f);
			if (m_IconGroup.alpha != 0f && currentRotation >= 90f)
			{
				m_IconGroup.alpha = 0f;
				m_ValueGroup.alpha = 1f;
			}
		}, () =>
		{
			m_ValueRect.localRotation = Quaternion.Euler(Vector3.zero);
		});
		StartCoroutine(m_IconPivotRoutine);
	}

	public void ShowReplaceValue(float showDuration, AnimationCurve showCurve, float glowDuration, AnimationCurve glowCurve, float shineDelay, float shineDuration, AnimationCurve shineCurve, float reveralDuration, AnimationCurve revealCurve, Action onRevealed, Action onCompleted = null)
	{
		m_ReplaceModifierCanvasGroup.alpha = 0.2f;
		m_ReplaceModifierCanvasGroup.transform.localScale = new Vector3(0.3f, 0.3f);
		m_ReplaceModifierGlow.alpha = 1f;
		m_ReplaceModifierGlow.transform.localScale = Vector3.one;
		m_ReplaceModifierShine.material.SetTextureOffset("_Mask", new Vector2(0f, -1f));
		m_ReplaceModifierCanvasGroup.gameObject.SetActive(value: true);
		m_IconPivotRoutine = CorrutineUtils.ProgressTo(0f, showDuration, 0f, 1f, showCurve, (float progress) =>
		{
			m_ReplaceModifierCanvasGroup.alpha = 0.2f + 0.8f * progress;
			float num = 0.3f + 0.7f * progress;
			m_ReplaceModifierCanvasGroup.transform.localScale = new Vector3(num, num);
		}, () =>
		{
			m_ReplaceModifierCanvasGroup.transform.localScale = Vector3.one;
			m_ReplaceModifierCanvasGroup.alpha = 1f;
			m_IconPivotRoutine = CorrutineUtils.ProgressTo(0f, glowDuration, 0f, 1f, glowCurve, (float progress) =>
			{
				m_ReplaceModifierGlow.alpha = 1f - progress;
				m_ReplaceModifierGlow.transform.localScale = new Vector3(1f + 0.2f * progress, 1f + 0.2f * progress);
			}, () =>
			{
				m_ReplaceModifierGlow.alpha = 0f;
				m_IconPivotRoutine = CorrutineUtils.ProgressTo(shineDelay, shineDuration, -1f, 1f, shineCurve, (float progress) =>
				{
					m_ReplaceModifierShine.material.SetTextureOffset("_Mask", new Vector2(0f, -1f + 2f * progress));
				}, () =>
				{
					m_IconPivotRoutine = CorrutineUtils.RectTransformPivotAroundVertical(m_ValueRect, 0f, reveralDuration / 2f, 90f, revealCurve, null, () =>
					{
						m_ReplaceModifierCanvasGroup.gameObject.SetActive(value: false);
						onRevealed?.Invoke();
						m_ValueRect.localRotation = Quaternion.Euler(new Vector3(0f, 270f, 0f));
						m_IconPivotRoutine = CorrutineUtils.RectTransformPivotAroundVertical(m_ValueRect, 0f, reveralDuration / 2f, 360f, revealCurve, null, () =>
						{
							m_ValueRect.localRotation = Quaternion.Euler(Vector3.zero);
							onCompleted?.Invoke();
						});
						StartCoroutine(m_IconPivotRoutine);
					});
					StartCoroutine(m_IconPivotRoutine);
				});
				StartCoroutine(m_IconPivotRoutine);
			});
			StartCoroutine(m_IconPivotRoutine);
		});
		StartCoroutine(m_IconPivotRoutine);
	}

	public void HideValue(float modifierFadeAwayDelay, float modifierFadeAwayDuration, AnimationCurve modifierFadeAwayCurve)
	{
		m_FadeRoutine = GloomUtility.FadeCanvasGroup(m_ValueGroup, modifierFadeAwayDelay, modifierFadeAwayDuration, 0f, modifierFadeAwayCurve, null, null, respectPause: true);
		StartCoroutine(m_FadeRoutine);
	}

	public void Hide()
	{
		m_IconGroup.alpha = 0f;
		m_ValueGroup.alpha = 0f;
	}

	public void ShowHighlight(float duration, AnimationCurve curve)
	{
		m_ValueRect.localScale = Vector3.one;
		m_ValueHighlightRoutine = CorrutineUtils.RectTransformLocalScaleInOut(m_ValueRect, 0f, duration, new Vector3(0.5f, 0.5f, 1f), curve);
		StartCoroutine(m_ValueHighlightRoutine);
	}

	public void SetAdvantadgeColor(Color color)
	{
		m_IconImage.color = color;
	}

	public void OnReturnedToPool()
	{
	}

	public void OnRemovedFromPool()
	{
		Reset();
	}
}
