using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using DG.Tweening;
using SETUtil;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldspaceUI;

public class HealthBar : MonoBehaviour
{
	[Header("HealthBar")]
	[SerializeField]
	private TextMeshProUGUI m_HealthPoints;

	[SerializeField]
	private Image m_HealthBar;

	[SerializeField]
	private Image m_HealthIcon;

	[Header("PreviewBar & Info")]
	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_AnimDuration = 1f;

	[SerializeField]
	private TextMeshProUGUI m_InfoLabel;

	[SerializeField]
	private TextMeshProUGUI m_InfoNumber;

	[SerializeField]
	private Image m_InfoIcon;

	[SerializeField]
	private float m_PreviewFadeDuration = 0.5f;

	[SerializeField]
	private float m_PreviewFadeToValue = 0.3f;

	[Header("Regular Damage")]
	[SerializeField]
	private Color m_RegularDamageColor;

	[SerializeField]
	private Sprite m_RegularDamageBarIcon;

	[SerializeField]
	private Sprite m_RegularDeadIcon;

	[Header("High Damage")]
	[SerializeField]
	private Color m_HigherDamageColor;

	[SerializeField]
	private Sprite m_HigherDamageBarIcon;

	[SerializeField]
	private Sprite m_RetaliateDamageIcon;

	[SerializeField]
	private Sprite m_HigherDeadIcon;

	[Header("Low Damage")]
	[SerializeField]
	private Color m_LowerDamageColor;

	[SerializeField]
	private Sprite m_LowerDamageBarIcon;

	[SerializeField]
	private Sprite m_LowerDeadIcon;

	[Header("Healing")]
	[SerializeField]
	private Color m_HealColor;

	[SerializeField]
	private Sprite m_HealBarIcon;

	[SerializeField]
	private Color m_OverhealColor;

	[SerializeField]
	private Vector2 m_Scale1Health = Vector2.one;

	[SerializeField]
	private Vector2 m_Scale5Health = Vector2.one;

	[SerializeField]
	private List<HealthConfigUI> m_HealthConfigs;

	private bool m_IsAnimActive;

	private bool m_InAttackFlow;

	private IEnumerator m_PreviewSliderRoutine;

	private IEnumerator m_HealthSliderRoutine;

	private Color healthColor;

	private Tween animPreview;

	private float _zoom = -1f;

	private Queue<Action> pendingActions = new Queue<Action>();

	private static readonly int HealthParam = Shader.PropertyToID("_Health");

	private static readonly int MaxHealthParam = Shader.PropertyToID("_MaxHealth");

	private static readonly int HealthColorParam = Shader.PropertyToID("_HealthColor");

	private static readonly int OriginalMaxHealthParam = Shader.PropertyToID("_OriginalMaxHealth");

	private static readonly int OriginalMaxHealthVisibilityParam = Shader.PropertyToID("_OriginalMaxHealthVisibility");

	private static readonly int OverhealParam = Shader.PropertyToID("_Overheal");

	private static readonly int OverhealVisibilityParam = Shader.PropertyToID("_OverhealVisibility");

	private static readonly int PreviewParam = Shader.PropertyToID("_Preview");

	private static readonly int PreviewVisibilityParam = Shader.PropertyToID("_PreviewVisibility");

	private static readonly int SizeParam = Shader.PropertyToID("_Size");

	private static readonly int PreviewColor = Shader.PropertyToID("_PreviewColor");

	private Material _healthBarMat;

	private static readonly int SmallStepThicknessParam = Shader.PropertyToID("_SmallStepThickness");

	public bool IsAnimated => m_IsAnimActive;

	private void Awake()
	{
		Material material = new Material(m_HealthBar.material);
		material.name += " (instance)";
		m_HealthBar.material = material;
		_healthBarMat = material;
	}

	private void OnDestroy()
	{
		SceneUtil.SmartDestroy(_healthBarMat);
	}

	private void AdjustHealthBarMarks()
	{
		if ((!(_zoom < 0f) || m_HealthConfigs.Count <= 0) && !(_healthBarMat == null))
		{
			int currentMaxHealth = Mathf.FloorToInt(_healthBarMat.GetFloat(MaxHealthParam));
			HealthZoomConfigUI healthZoomConfigUI = (from it in m_HealthConfigs
				where it.health <= currentMaxHealth
				orderby it.health descending
				select it).FirstOrDefault()?.GetZoomConfig(_zoom);
			if (healthZoomConfigUI != null)
			{
				_healthBarMat.SetFloat(SmallStepThicknessParam, healthZoomConfigUI.SmallStepThickness);
			}
		}
	}

	public void OnUpdatedZoom(float zoom)
	{
		if (zoom != _zoom)
		{
			_zoom = zoom;
			AdjustHealthBarMarks();
		}
	}

	public void UpdateHealth(int maxHealth, int currentHealth, int originalMaxHealth, bool animate, Color allegianceColor, Action onUpdate = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		if (m_IsAnimActive)
		{
			pendingActions.Enqueue(() =>
			{
				UpdateHealth(maxHealth, currentHealth, originalMaxHealth, animate, allegianceColor, onUpdate);
			});
			return;
		}
		_healthBarMat.SetColor(HealthColorParam, allegianceColor);
		onUpdate?.Invoke();
		CancelPreviewAnimation();
		_healthBarMat.SetFloat(MaxHealthParam, maxHealth);
		if (maxHealth > originalMaxHealth)
		{
			_healthBarMat.SetFloat(OriginalMaxHealthParam, originalMaxHealth);
			_healthBarMat.SetFloat(OriginalMaxHealthVisibilityParam, 1f);
			_healthBarMat.SetFloat(OverhealVisibilityParam, 1f);
		}
		else
		{
			_healthBarMat.SetFloat(OverhealVisibilityParam, 0f);
			_healthBarMat.SetFloat(OriginalMaxHealthVisibilityParam, 0f);
		}
		int num = Mathf.Min(currentHealth, originalMaxHealth);
		m_HealthPoints.text = currentHealth.ToString();
		m_HealthPoints.color = ((currentHealth > originalMaxHealth) ? m_OverhealColor : UIInfoTools.Instance.White);
		if (_healthBarMat.GetFloat(PreviewParam) > (float)currentHealth)
		{
			_healthBarMat.SetFloat(HealthParam, num);
			_healthBarMat.SetFloat(OverhealParam, currentHealth);
			if (animate)
			{
				m_IsAnimActive = true;
				CancelPreviewSliderRoutine();
				m_PreviewSliderRoutine = AnimateSlider(PreviewParam, currentHealth, m_AnimDuration, () =>
				{
					m_IsAnimActive = false;
					ResetColors();
					if (pendingActions.Count > 0)
					{
						pendingActions.Dequeue()?.Invoke();
					}
				});
				StartCoroutine(m_PreviewSliderRoutine);
			}
			else
			{
				_healthBarMat.SetFloat(PreviewParam, currentHealth);
			}
		}
		else if (_healthBarMat.GetFloat(HealthParam) < (float)currentHealth)
		{
			_healthBarMat.SetFloat(PreviewParam, currentHealth);
			if (animate)
			{
				m_IsAnimActive = true;
				if (m_HealthSliderRoutine != null)
				{
					StopCoroutine(m_HealthSliderRoutine);
					m_HealthSliderRoutine = null;
				}
				m_HealthSliderRoutine = AnimateSlider(HealthParam, num, m_AnimDuration, () =>
				{
					m_HealthSliderRoutine = AnimateSlider(OverhealParam, currentHealth, m_AnimDuration, () =>
					{
						m_IsAnimActive = false;
						ResetColors();
						if (pendingActions.Count > 0)
						{
							pendingActions.Dequeue()?.Invoke();
						}
					});
					StartCoroutine(m_HealthSliderRoutine);
				});
				StartCoroutine(m_HealthSliderRoutine);
			}
			else
			{
				_healthBarMat.SetFloat(OverhealParam, currentHealth);
				_healthBarMat.SetFloat(HealthParam, num);
			}
		}
		else if (pendingActions.Count > 0)
		{
			pendingActions.Dequeue()?.Invoke();
		}
		AdjustHealthBarMarks();
	}

	private IEnumerator AnimateSlider(int parameter, float valueToAnimateTo, float timeToAnimate, UnityAction completionAction)
	{
		float timeElapsed = 0f;
		float startVal = _healthBarMat.GetFloat(parameter);
		if (startVal != valueToAnimateTo)
		{
			while (timeElapsed < timeToAnimate)
			{
				_healthBarMat.SetFloat(parameter, Mathf.Lerp(startVal, valueToAnimateTo, timeElapsed / timeToAnimate));
				if (!TimeManager.IsPaused)
				{
					timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
				}
				yield return null;
			}
			_healthBarMat.SetFloat(parameter, valueToAnimateTo);
		}
		completionAction?.Invoke();
	}

	public void UpdateColors(int totalAttack, int baseAttack)
	{
		if (totalAttack == baseAttack)
		{
			_healthBarMat.SetColor(PreviewColor, m_RegularDamageColor);
		}
		else if (totalAttack > baseAttack)
		{
			_healthBarMat.SetColor(PreviewColor, m_HigherDamageColor);
		}
		else
		{
			_healthBarMat.SetColor(PreviewColor, m_LowerDamageColor);
		}
	}

	public void PreviewAttack(int currentHealth, int totalAttack, int baseAttack, int originalMaxHealth, bool highlight = true)
	{
		pendingActions.Clear();
		totalAttack = Mathf.Max(totalAttack, 0);
		int num = currentHealth - totalAttack;
		_healthBarMat.SetFloat(OverhealParam, num);
		_healthBarMat.SetFloat(HealthParam, Mathf.Min(num, originalMaxHealth));
		_healthBarMat.SetFloat(PreviewParam, currentHealth);
		m_HealthPoints.text = Math.Max(0, currentHealth).ToString();
		HighlightPreview(highlight);
		UpdateColors(totalAttack, baseAttack);
		CancelPreviewSliderRoutine();
	}

	public void PreviewRetaliate(int currentHealth, int retaliateDamage, int originalMaxHealth, bool highlight = true)
	{
		pendingActions.Clear();
		retaliateDamage = Mathf.Max(retaliateDamage, 0);
		int num = currentHealth - retaliateDamage;
		_healthBarMat.SetFloat(OverhealParam, num);
		_healthBarMat.SetFloat(HealthParam, Mathf.Min(num, originalMaxHealth));
		_healthBarMat.SetFloat(PreviewParam, currentHealth);
		_healthBarMat.SetColor(PreviewColor, m_HigherDamageColor);
		HighlightPreview(highlight);
		CancelPreviewSliderRoutine();
	}

	public void PreviewHeal(int maxHealth, int currentHealth, int originalMaxHealth, int healAmount, bool highlight = true)
	{
		pendingActions.Clear();
		if (maxHealth != currentHealth)
		{
			HighlightPreview(highlight);
			healAmount = Mathf.Max(healAmount, 0);
			int num = Mathf.Min(maxHealth, currentHealth + healAmount);
			_healthBarMat.SetFloat(HealthParam, Mathf.Min(currentHealth, originalMaxHealth));
			_healthBarMat.SetFloat(OverhealParam, currentHealth);
			_healthBarMat.SetFloat(PreviewParam, num);
			_healthBarMat.SetColor(PreviewColor, m_HealColor);
			CancelPreviewSliderRoutine();
		}
	}

	public void HighlightPreview(bool highlight)
	{
		if (highlight)
		{
			if (animPreview == null)
			{
				AnimatePreviewAlpha(1f, m_PreviewFadeToValue);
			}
		}
		else
		{
			CancelPreviewAnimation();
		}
	}

	private void AnimatePreviewAlpha(float fromAlpha, float toAlpha)
	{
		animPreview = _healthBarMat.DOFloat(toAlpha, PreviewVisibilityParam, m_PreviewFadeDuration).From(fromAlpha).SetLoops(-1, LoopType.Yoyo);
	}

	private void CancelPreviewAnimation()
	{
		_healthBarMat.SetFloat(PreviewVisibilityParam, 1f);
		if (animPreview != null)
		{
			animPreview.Kill();
		}
		animPreview = null;
	}

	private void CancelPreviewSliderRoutine()
	{
		if (m_PreviewSliderRoutine != null)
		{
			StopCoroutine(m_PreviewSliderRoutine);
			m_PreviewSliderRoutine = null;
			m_IsAnimActive = false;
		}
	}

	public void ResetColors()
	{
		if (!m_IsAnimActive && !m_InAttackFlow)
		{
			_healthBarMat.SetColor(PreviewColor, m_RegularDamageColor);
		}
	}

	public void ResetPreview(int currentHealth, int originalMaxHealth)
	{
		pendingActions.Clear();
		if (m_IsAnimActive || m_InAttackFlow)
		{
			if (m_PreviewSliderRoutine != null)
			{
				StopCoroutine(m_PreviewSliderRoutine);
				m_PreviewSliderRoutine = null;
			}
			if (m_HealthSliderRoutine != null)
			{
				StopCoroutine(m_HealthSliderRoutine);
				m_HealthSliderRoutine = null;
			}
			m_IsAnimActive = false;
		}
		m_HealthPoints.text = currentHealth.ToString();
		_healthBarMat.SetFloat(HealthParam, Mathf.Min(currentHealth, originalMaxHealth));
		_healthBarMat.SetFloat(PreviewParam, currentHealth);
		_healthBarMat.SetFloat(OverhealParam, currentHealth);
		CancelPreviewAnimation();
	}

	public void PrepareForAttack(int currentHealth, int originalMaxHealth)
	{
		pendingActions.Clear();
		m_InAttackFlow = true;
		_healthBarMat.SetFloat(HealthParam, Mathf.Min(currentHealth, originalMaxHealth));
		_healthBarMat.SetFloat(PreviewParam, currentHealth);
		_healthBarMat.SetFloat(OverhealParam, currentHealth);
		CancelPreviewAnimation();
	}

	public void FinalizeAttackFlow()
	{
		m_InAttackFlow = false;
	}

	public void InitHealth(int maxHealth, int currentHealth, int originalMaxHealth, Color color, Sprite healthIcon)
	{
		_healthBarMat.SetFloat(MaxHealthParam, maxHealth);
		_healthBarMat.SetFloat(HealthParam, Mathf.Min(currentHealth, originalMaxHealth));
		_healthBarMat.SetFloat(OverhealParam, currentHealth);
		_healthBarMat.SetFloat(PreviewParam, currentHealth);
		_healthBarMat.SetFloat(OverhealVisibilityParam, (maxHealth > originalMaxHealth) ? 1f : 0f);
		_healthBarMat.SetFloat(OriginalMaxHealthParam, maxHealth);
		_healthBarMat.SetFloat(OriginalMaxHealthVisibilityParam, (maxHealth > originalMaxHealth) ? 1f : 0f);
		_healthBarMat.SetColor(HealthColorParam, color);
		m_HealthIcon.sprite = healthIcon;
		Vector2 sizeDelta = (transform as RectTransform).sizeDelta;
		_healthBarMat.SetVector(SizeParam, sizeDelta);
	}

	public void Focus(bool focus)
	{
		_healthBarMat.SetFloat(PreviewVisibilityParam, focus ? 1f : 0f);
	}

	private void OnDisable()
	{
		CancelPreviewAnimation();
	}
}
