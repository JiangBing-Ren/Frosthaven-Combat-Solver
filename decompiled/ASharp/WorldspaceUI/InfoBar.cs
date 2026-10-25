using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldspaceUI;

public class InfoBar : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Info Elements")]
	[SerializeField]
	private TextMeshProUGUI m_InfoLabel;

	[SerializeField]
	private TextMeshProUGUI m_InfoNumber;

	[SerializeField]
	private Image m_InfoIcon;

	[SerializeField]
	private CanvasGroup m_InfoGroup;

	[Header("Regular Damage")]
	[SerializeField]
	public Color m_RegularAttackColor;

	[SerializeField]
	private Sprite m_RegularAttackIcon;

	[SerializeField]
	private Sprite m_RegularDamageIcon;

	[SerializeField]
	private Sprite m_RegularDeadIcon;

	[SerializeField]
	private ParticleSystem m_RegularLabelPFX;

	[Header("High Attack")]
	[SerializeField]
	public Color m_HighAttackColor;

	[SerializeField]
	private Sprite m_HighAttackIcon;

	[SerializeField]
	private Sprite m_HighDamageIcon;

	[SerializeField]
	private Sprite m_HighDeadIcon;

	[SerializeField]
	private ParticleSystem m_HighLabelPFX;

	[Header("Low Attack")]
	[SerializeField]
	public Color m_LowAttackColor;

	[SerializeField]
	private Sprite m_LowAttackIcon;

	[SerializeField]
	private Sprite m_LowDamageIcon;

	[SerializeField]
	private Sprite m_LowDeadIcon;

	[SerializeField]
	private ParticleSystem m_LowLabelPFX;

	[Header("Critical Damage")]
	[SerializeField]
	public Color m_CriticalAttackColor;

	[SerializeField]
	private Sprite m_CriticalAttackIcon;

	[SerializeField]
	private Sprite m_CriticalDamageIcon;

	[SerializeField]
	private Sprite m_CriticalDeadIcon;

	[SerializeField]
	private ParticleSystem m_CriticalLabelPFX;

	[Header("Null Damage")]
	[SerializeField]
	public Color m_NullAttackColor;

	[SerializeField]
	private Sprite m_NullAttackIcon;

	[SerializeField]
	private Sprite m_NullDamageIcon;

	[SerializeField]
	private Sprite m_NullDeadIcon;

	[SerializeField]
	private ParticleSystem m_NullLabelPFX;

	[Header("Healing")]
	[SerializeField]
	public Color m_HealColor;

	[SerializeField]
	private Sprite m_HealIcon;

	[Header("Gold")]
	[SerializeField]
	public Color m_GoldColor;

	[SerializeField]
	private Sprite m_GoldIcon;

	[SerializeField]
	private float m_GoldFadeOutTime = 1f;

	[SerializeField]
	private float m_GoldFadeInTime = 0.3f;

	[SerializeField]
	private float m_GoldWaitTime = 1f;

	[Header("Loot")]
	[SerializeField]
	public Color m_LootColor;

	[SerializeField]
	private Sprite m_LootIcon;

	[SerializeField]
	private float m_LootFadeOutTime = 1f;

	[SerializeField]
	private float m_LootFadeInTime = 0.3f;

	[SerializeField]
	private float m_LootWaitTime = 1f;

	[Header("XP")]
	[SerializeField]
	public Color m_XPColor;

	[SerializeField]
	private Sprite m_XPIcon;

	[SerializeField]
	private float m_XPFadeOutTime = 1f;

	[SerializeField]
	private float m_XPFadeInTime = 0.3f;

	[SerializeField]
	private float m_XPWaitTime = 1f;

	[Header("Misc")]
	[SerializeField]
	private Sprite m_RetaliateAttackIcon;

	[SerializeField]
	private RectTransform m_Icon;

	[SerializeField]
	private List<Graphic> m_PreviewDamageGraphics;

	private Vector3 m_IconPosition;

	[SerializeField]
	private float m_DamageWaitTime = 1f;

	[SerializeField]
	private float m_DamageFadeOutTime = 1f;

	[Space]
	[SerializeField]
	private AttackValueBreakdown m_AttackValueBreakdown;

	[Header("Visibility Events")]
	public UnityEvent OnShow;

	public UnityEvent OnHide;

	private RectTransform m_RectTransform;

	private Vector2 m_OriginalSize;

	private bool m_IsAnimActive;

	private bool m_IsAttackFlowActive;

	private bool m_IsBreakdownAvailable;

	private UnityAction m_PendingPreviewAction;

	private List<UnityAction> m_PendingActions;

	private IEnumerator m_InfoLabelFadeRoutine;

	private Tween healAnim;

	private void Awake()
	{
		m_PendingActions = new List<UnityAction>();
		m_RectTransform = GetComponent<RectTransform>();
		m_OriginalSize = m_RectTransform.sizeDelta;
		m_IconPosition = m_Icon.localPosition;
		Hide();
	}

	private void OnDestroy()
	{
		if (healAnim != null)
		{
			healAnim.Kill();
		}
		healAnim = null;
	}

	private void OnEnable()
	{
		m_RectTransform.sizeDelta = m_OriginalSize;
		m_Icon.localPosition = m_IconPosition;
		m_AttackValueBreakdown.gameObject.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_IsBreakdownAvailable)
		{
			m_AttackValueBreakdown.gameObject.SetActive(value: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_AttackValueBreakdown.gameObject.SetActive(value: false);
	}

	public void InitBreakdown(CAttackSummary.TargetSummary attackSummary, int baseDamage)
	{
		m_AttackValueBreakdown.Init(attackSummary, baseDamage);
	}

	public void PreviewAttack(int currentHealth, int totalAttack, int baseAttack, bool isEnemyAttacking = false, bool justDamage = false, bool advantadge = false, bool disadvantadge = false)
	{
		if (m_IsAnimActive || m_IsAttackFlowActive)
		{
			m_PendingPreviewAction = () =>
			{
				PreviewAttack(currentHealth, totalAttack, baseAttack, isEnemyAttacking, justDamage, advantadge, disadvantadge);
			};
			return;
		}
		if (!isEnemyAttacking)
		{
			Show();
			m_IsBreakdownAvailable = true;
		}
		totalAttack = Mathf.Max(totalAttack, 0);
		_ = currentHealth;
		_ = totalAttack;
		m_InfoLabel.text = (justDamage ? LocalizationManager.GetTranslation("Damage").ToUpper() : LocalizationManager.GetTranslation("Attack").ToUpper());
		m_InfoNumber.text = totalAttack.ToString();
		m_InfoIcon.color = Color.white;
		TextMeshProUGUI infoLabel = m_InfoLabel;
		Color color = (m_InfoNumber.color = GetColorForColorCode(GetCodeForDamage(totalAttack, baseAttack, advantadge, disadvantadge)));
		infoLabel.color = color;
		SetPreviewDamageGraphicsColor(m_InfoLabel.color);
		if (totalAttack == baseAttack && !advantadge && !disadvantadge)
		{
			m_InfoIcon.sprite = ((totalAttack >= currentHealth) ? m_RegularDeadIcon : m_RegularAttackIcon);
		}
		else if ((totalAttack > baseAttack) | advantadge)
		{
			m_InfoIcon.sprite = ((totalAttack >= currentHealth) ? m_HighDeadIcon : m_HighAttackIcon);
		}
		else
		{
			m_InfoIcon.sprite = ((totalAttack >= currentHealth) ? m_LowDeadIcon : m_LowAttackIcon);
		}
	}

	private void Show()
	{
		m_InfoGroup.alpha = 1f;
		OnShow?.Invoke();
	}

	private void Hide()
	{
		m_InfoGroup.alpha = 0f;
		OnHide?.Invoke();
	}

	public void PreviewRetaliate(int currentHealth, int retaliateDamage)
	{
		retaliateDamage = Mathf.Max(retaliateDamage, 0);
		Show();
		m_InfoLabel.text = LocalizationManager.GetTranslation("Retaliate").ToUpper();
		m_InfoNumber.text = retaliateDamage.ToString();
		m_InfoIcon.sprite = ((retaliateDamage >= currentHealth) ? m_HighDeadIcon : m_RetaliateAttackIcon);
		m_InfoLabel.color = m_HighAttackColor;
		m_InfoNumber.color = m_HighAttackColor;
		SetPreviewDamageGraphicsColor(m_HighAttackColor);
	}

	public void PreviewHeal(int maxHealth, int currentHealth, bool isOverheal, int healAmount)
	{
		if (maxHealth != currentHealth)
		{
			healAmount = Mathf.Max(healAmount, 0);
			int num = Mathf.Min(maxHealth, currentHealth + healAmount);
			Show();
			m_InfoLabel.text = LocalizationManager.GetTranslation(isOverheal ? "Overheal" : "Heal").ToUpper();
			m_InfoNumber.text = (num - currentHealth).ToString();
			m_InfoIcon.sprite = m_HealIcon;
			m_InfoLabel.color = m_HealColor;
			m_InfoNumber.color = m_HealColor;
			ResetPreviewDamageGraphicsColor();
		}
	}

	public void OnShowLoot(float fadeInTime, float fadeOutTime, float waitTime)
	{
		m_IsAnimActive = true;
		if (m_IsAttackFlowActive)
		{
			return;
		}
		FadeInfoLabel(1f, fadeInTime, 0f, null, () =>
		{
			if (m_IsAttackFlowActive)
			{
				Show();
				if (m_InfoLabelFadeRoutine != null)
				{
					StopCoroutine(m_InfoLabelFadeRoutine);
				}
			}
		}, () =>
		{
			if (!m_IsAttackFlowActive)
			{
				FadeInfoLabel(0f, fadeOutTime, waitTime, null, () =>
				{
					if (m_IsAttackFlowActive)
					{
						Show();
						if (m_InfoLabelFadeRoutine != null)
						{
							StopCoroutine(m_InfoLabelFadeRoutine);
						}
					}
				}, () =>
				{
					if (!m_IsAttackFlowActive)
					{
						m_IsAnimActive = false;
						Hide();
					}
				});
			}
		});
	}

	public void ShowGold(int gold)
	{
		Hide();
		m_InfoNumber.text = gold.ToString();
		m_InfoLabel.text = LocalizationManager.GetTranslation("Gold").ToUpper();
		m_InfoIcon.color = m_GoldColor;
		m_InfoIcon.sprite = m_GoldIcon;
		m_InfoLabel.color = m_GoldColor;
		m_InfoNumber.color = m_GoldColor;
		OnShowLoot(m_GoldFadeInTime, m_GoldFadeOutTime, m_GoldWaitTime);
	}

	public void ShowLootGain(int numTokens)
	{
		Hide();
		m_InfoNumber.text = numTokens.ToString();
		m_InfoLabel.text = LocalizationManager.GetTranslation("LOOT_TOKENS_GAINED").ToUpper();
		m_InfoIcon.color = m_LootColor;
		m_InfoIcon.sprite = m_LootIcon;
		m_InfoLabel.color = m_LootColor;
		m_InfoNumber.color = m_LootColor;
		OnShowLoot(m_LootFadeInTime, m_LootFadeOutTime, m_LootWaitTime);
	}

	public void ShowXP(int xp)
	{
		if (m_IsAnimActive || m_IsAttackFlowActive)
		{
			m_PendingActions.Add(() =>
			{
				ShowXP(xp);
			});
			return;
		}
		Hide();
		m_InfoNumber.text = "";
		m_InfoLabel.text = string.Format("{0} +{1}", LocalizationManager.GetTranslation("XP").ToUpper(), xp);
		m_InfoIcon.color = m_XPColor;
		m_InfoIcon.sprite = m_XPIcon;
		m_InfoLabel.color = m_XPColor;
		m_InfoNumber.color = m_XPColor;
		m_IsAnimActive = true;
		if (m_IsAttackFlowActive)
		{
			return;
		}
		FadeInfoLabel(1f, m_XPFadeInTime, 0f, null, () =>
		{
			if (m_IsAttackFlowActive)
			{
				Show();
				if (m_InfoLabelFadeRoutine != null)
				{
					StopCoroutine(m_InfoLabelFadeRoutine);
				}
			}
		}, () =>
		{
			if (!m_IsAttackFlowActive)
			{
				FadeInfoLabel(0f, m_XPFadeOutTime, m_XPWaitTime, null, () =>
				{
					if (m_IsAttackFlowActive)
					{
						Show();
						if (m_InfoLabelFadeRoutine != null)
						{
							StopCoroutine(m_InfoLabelFadeRoutine);
						}
					}
				}, () =>
				{
					if (!m_IsAttackFlowActive)
					{
						m_IsAnimActive = false;
						Hide();
					}
				});
			}
		});
	}

	public void ResetColors(bool forceReset = false)
	{
		if (!m_IsAnimActive || forceReset)
		{
			m_InfoIcon.sprite = m_RegularAttackIcon;
			m_InfoLabel.color = m_RegularAttackColor;
			m_InfoNumber.color = m_RegularAttackColor;
			m_InfoIcon.color = Color.white;
			ResetPreviewDamageGraphicsColor();
		}
	}

	public void MoveToAttackMode(float moveTime, AnimationCurve curve)
	{
		m_IsAttackFlowActive = true;
		FadeInfoLabel(0f, moveTime, 0f, curve);
	}

	public void UpdateAttackValue(int attackValue)
	{
		m_InfoNumber.text = attackValue.ToString();
	}

	public void ResetPreview()
	{
		m_PendingPreviewAction = null;
		m_IsBreakdownAvailable = false;
		if (!m_IsAnimActive)
		{
			Hide();
		}
	}

	public void PrepareForAttack(CAttackSummary.TargetSummary targetSummary)
	{
		ResetColors(forceReset: true);
		if (m_InfoLabelFadeRoutine != null)
		{
			StopCoroutine(m_InfoLabelFadeRoutine);
		}
		m_InfoLabel.text = LocalizationManager.GetTranslation("Attack").ToUpper();
		Show();
		m_IsAnimActive = true;
	}

	public void ShowDamage(int amount, bool forceShow = false, EAttackModifierDamageColorCode? colorCode = null)
	{
		if (m_IsAnimActive && !forceShow)
		{
			return;
		}
		Show();
		m_InfoLabel.text = LocalizationManager.GetTranslation("Damage").ToUpper();
		m_InfoNumber.text = Mathf.Abs(amount).ToString();
		if (Mathf.Approximately(0f, m_InfoIcon.color.a) || Mathf.Approximately(0f, m_InfoLabel.color.a) || Mathf.Approximately(0f, m_InfoNumber.color.a))
		{
			m_InfoIcon.color = Color.white;
			m_InfoLabel.color = m_RegularAttackColor;
			m_InfoNumber.color = m_RegularAttackColor;
			SetPreviewDamageGraphicsColor(m_RegularAttackColor);
		}
		if (colorCode.HasValue)
		{
			ParticleSystem particleSystem = null;
			switch (colorCode)
			{
			case EAttackModifierDamageColorCode.CriticalDamage:
				particleSystem = m_CriticalLabelPFX;
				m_InfoIcon.sprite = m_CriticalDamageIcon;
				break;
			case EAttackModifierDamageColorCode.ZeroDamage:
				particleSystem = m_NullLabelPFX;
				m_InfoIcon.sprite = m_NullDamageIcon;
				break;
			case EAttackModifierDamageColorCode.PositiveDamage:
				particleSystem = m_HighLabelPFX;
				m_InfoIcon.sprite = m_HighDamageIcon;
				break;
			case EAttackModifierDamageColorCode.negativeDamage:
				particleSystem = m_LowLabelPFX;
				m_InfoIcon.sprite = m_LowDamageIcon;
				break;
			case EAttackModifierDamageColorCode.RegularDamage:
				particleSystem = m_RegularLabelPFX;
				m_InfoIcon.sprite = m_RegularDamageIcon;
				break;
			}
			Color colorForColorCode = GetColorForColorCode(colorCode.Value);
			m_InfoLabel.color = colorForColorCode;
			m_InfoNumber.color = colorForColorCode;
			SetPreviewDamageGraphicsColor(colorForColorCode);
			particleSystem.gameObject.SetActive(value: true);
			particleSystem.Play();
		}
		else if (m_InfoIcon.sprite == m_RegularDeadIcon || m_InfoIcon.sprite == m_RegularAttackIcon)
		{
			m_InfoIcon.sprite = m_RegularDamageIcon;
		}
		else if (m_InfoIcon.sprite == m_CriticalAttackIcon || m_InfoIcon.sprite == m_CriticalDeadIcon)
		{
			m_InfoIcon.sprite = m_CriticalDamageIcon;
		}
		else if (m_InfoIcon.sprite == m_NullAttackIcon || m_InfoIcon.sprite == m_NullDeadIcon)
		{
			m_InfoIcon.sprite = m_NullDamageIcon;
		}
		else if (m_InfoIcon.sprite == m_HighAttackIcon || m_InfoIcon.sprite == m_HighDeadIcon || m_InfoIcon.sprite == m_RetaliateAttackIcon)
		{
			m_InfoIcon.sprite = m_HighDamageIcon;
		}
		else if (m_InfoIcon.sprite == m_LowAttackIcon || m_InfoIcon.sprite == m_LowDeadIcon)
		{
			m_InfoIcon.sprite = m_LowDamageIcon;
		}
		m_IsAnimActive = true;
		if (m_IsAttackFlowActive)
		{
			return;
		}
		FadeInfoLabel(0f, m_DamageFadeOutTime, 0f, null, () =>
		{
			if (m_IsAttackFlowActive)
			{
				Show();
				if (m_InfoLabelFadeRoutine != null)
				{
					StopCoroutine(m_InfoLabelFadeRoutine);
				}
			}
		}, () =>
		{
			if (!m_IsAttackFlowActive)
			{
				m_IsAnimActive = false;
				Hide();
			}
		});
	}

	public void ShowHeal(int amount, bool overheal)
	{
		m_IsAnimActive = true;
		if (healAnim != null)
		{
			healAnim.Kill();
		}
		healAnim = null;
		Show();
		m_InfoLabel.text = LocalizationManager.GetTranslation(overheal ? "Overheal" : "Heal").ToUpper();
		m_InfoNumber.text = Mathf.Abs(amount).ToString();
		m_InfoIcon.sprite = m_HealIcon;
		m_InfoIcon.color = m_HealColor;
		m_InfoLabel.color = m_HealColor;
		m_InfoNumber.color = m_HealColor;
		ResetPreviewDamageGraphicsColor();
		healAnim = DOTween.To((float value) =>
		{
			Color color = m_InfoLabel.color;
			color.a = value;
			m_InfoLabel.color = color;
			m_InfoNumber.color = color;
			color = m_InfoIcon.color;
			color.a = value;
			m_InfoIcon.color = color;
		}, 1f, 0f, m_DamageFadeOutTime).SetDelay(m_DamageWaitTime).OnComplete(() =>
		{
			healAnim = null;
			m_IsAnimActive = false;
			Hide();
			if (m_PendingPreviewAction == null && m_PendingActions.Count > 0)
			{
				int index = m_PendingActions.Count - 1;
				UnityAction unityAction = m_PendingActions[index];
				m_PendingActions.RemoveAt(index);
				unityAction();
			}
		});
	}

	public void FinalizeAttackFlow()
	{
		if (m_IsAttackFlowActive)
		{
			m_IsAttackFlowActive = false;
			m_IsAnimActive = false;
			Hide();
			if (m_PendingPreviewAction != null)
			{
				m_PendingPreviewAction();
				m_PendingPreviewAction = null;
			}
			else if (m_PendingActions.Count > 0)
			{
				int index = m_PendingActions.Count - 1;
				UnityAction unityAction = m_PendingActions[index];
				m_PendingActions.RemoveAt(index);
				unityAction();
			}
		}
	}

	public void FadeInfoLabel(float targetAlpha, float duration, float delay, AnimationCurve curve = null, UnityAction onUpdate = null, UnityAction onComplete = null)
	{
		if (targetAlpha > 0f)
		{
			OnShow?.Invoke();
		}
		if (m_InfoLabelFadeRoutine != null)
		{
			StopCoroutine(m_InfoLabelFadeRoutine);
		}
		m_IsAnimActive = true;
		m_InfoLabelFadeRoutine = GloomUtility.FadeCanvasGroup(m_InfoGroup, delay, duration, targetAlpha, (curve != null) ? curve : AnimationCurve.Linear(0f, 0f, 1f, 1f), (float value) =>
		{
			onUpdate?.Invoke();
			if (m_PendingPreviewAction != null)
			{
				m_IsAnimActive = false;
				onComplete?.Invoke();
				if (m_InfoLabelFadeRoutine != null)
				{
					StopCoroutine(m_InfoLabelFadeRoutine);
				}
			}
		}, () =>
		{
			if (m_InfoGroup.alpha == 0f)
			{
				OnHide?.Invoke();
			}
			m_IsAnimActive = false;
			onComplete?.Invoke();
			if (m_PendingPreviewAction == null && m_PendingActions.Count > 0)
			{
				int index = m_PendingActions.Count - 1;
				UnityAction unityAction = m_PendingActions[index];
				m_PendingActions.RemoveAt(index);
				unityAction();
			}
		}, respectPause: true);
		StartCoroutine(m_InfoLabelFadeRoutine);
	}

	public void SetInfoGroupVisible(bool shouldSetVisible)
	{
		if (shouldSetVisible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public Color GetColorForColorCode(EAttackModifierDamageColorCode colorCode)
	{
		return colorCode switch
		{
			EAttackModifierDamageColorCode.CriticalDamage => m_CriticalAttackColor, 
			EAttackModifierDamageColorCode.ZeroDamage => m_NullAttackColor, 
			EAttackModifierDamageColorCode.PositiveDamage => m_HighAttackColor, 
			EAttackModifierDamageColorCode.negativeDamage => m_LowAttackColor, 
			EAttackModifierDamageColorCode.RegularDamage => m_RegularAttackColor, 
			_ => m_RegularAttackColor, 
		};
	}

	public EAttackModifierDamageColorCode GetCodeForDamage(int totalAttack, int baseAttack, bool advantadge, bool disadvantadge)
	{
		if (totalAttack == baseAttack && !advantadge && !disadvantadge)
		{
			return EAttackModifierDamageColorCode.RegularDamage;
		}
		if ((totalAttack > baseAttack) | advantadge)
		{
			return EAttackModifierDamageColorCode.PositiveDamage;
		}
		return EAttackModifierDamageColorCode.negativeDamage;
	}

	private void ResetPreviewDamageGraphicsColor()
	{
		SetPreviewDamageGraphicsColor(m_RegularAttackColor);
	}

	private void SetPreviewDamageGraphicsColor(Color color)
	{
		for (int i = 0; i < m_PreviewDamageGraphics.Count; i++)
		{
			m_PreviewDamageGraphics[i].color = color;
		}
	}
}
