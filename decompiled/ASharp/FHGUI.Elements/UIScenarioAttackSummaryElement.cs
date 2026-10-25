using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioAttackSummaryElement : MonoBehaviour
{
	public struct SummaryData
	{
		public CAttackSummary.TargetSummary Summary;

		public List<AttackModifierYMLData> ModiferDrawn;

		public List<AttackModifierYMLData> ModiferChoice;

		public List<AttackModifierYMLData> ModiferNotUsed;

		public List<AttackModifierYMLData> Selected;

		public List<AttackModifierYMLData> ActiveAbilityModifiers;

		public Action<AttackModifierYMLData> OnSelectCallback;

		public bool HasAdvantage => Summary.OverallAdvantage == EAdvantageStatuses.Advantage;

		public bool HasDisadvantage => Summary.OverallAdvantage == EAdvantageStatuses.Disadvantage;

		public bool IsSimilar(SummaryData other)
		{
			if (ModiferDrawn == null || other.ModiferDrawn == null)
			{
				return false;
			}
			if (ModiferDrawn.Count != other.ModiferDrawn.Count)
			{
				return false;
			}
			for (int i = 0; i < ModiferDrawn.Count; i++)
			{
				if (ModiferDrawn[i].ID != other.ModiferDrawn[i].ID)
				{
					return false;
				}
			}
			return true;
		}
	}

	public struct Params
	{
		public SummaryData Summary;

		public bool ShowWithAnimation;
	}

	[Header("Elements")]
	[SerializeField]
	private RectTransform m_ContainerDrawnModifier;

	[SerializeField]
	private RectTransform m_ContainerChoice;

	[SerializeField]
	private CanvasGroup m_ContainerChoiceCanvas;

	[SerializeField]
	private UIOutpostAttackTownGuardDrawnModifier m_ModifierPrototype;

	[SerializeField]
	private Image m_Advantage;

	[SerializeField]
	private Image m_Disadvantage;

	[SerializeField]
	private Image m_TargetActorIcon;

	private readonly List<UIOutpostAttackTownGuardDrawnModifier> m_DrawnModifiers = new List<UIOutpostAttackTownGuardDrawnModifier>();

	private readonly List<UIOutpostAttackTownGuardDrawnModifier> m_ChoiceModifiers = new List<UIOutpostAttackTownGuardDrawnModifier>();

	private bool m_Invalidate;

	private readonly List<UIOutpostAttackTownGuardDrawnModifier.Params> paramsList = new List<UIOutpostAttackTownGuardDrawnModifier.Params>();

	private List<Tween> activeTweens = new List<Tween>();

	[Header("Settings")]
	[SerializeField]
	private float m_StartDelay = 0.55f;

	[SerializeField]
	private float m_ShowDuration = 0.55f;

	[SerializeField]
	private float m_PerSlotShowDelay = 0.2f;

	[SerializeField]
	private bool m_UseMoveTween;

	[SerializeField]
	private RectTransform AnimationContainer;

	[SerializeField]
	private RectTransform StartPoint;

	public Params Data { get; private set; }

	public bool InTransition { get; private set; }

	public void Init(Params @params)
	{
		Clear();
		Data = @params;
		Build();
		m_Invalidate = true;
		InTransition = false;
	}

	private void Update()
	{
		if (m_Invalidate)
		{
			m_Invalidate = false;
			if (Data.ShowWithAnimation)
			{
				StartCoroutine(BeginDrawTransition());
			}
			else
			{
				m_ContainerChoiceCanvas.alpha = 1f;
			}
		}
	}

	private void OnDisable()
	{
		InTransition = false;
	}

	private void Build()
	{
		FillIcons(Data.Summary.ModiferDrawn, m_DrawnModifiers, m_ContainerDrawnModifier, interactable: false);
		FillIcons(Data.Summary.ModiferChoice, m_ChoiceModifiers, m_ContainerChoice, interactable: false);
		m_Advantage.gameObject.SetActive(Data.Summary.HasAdvantage);
		m_Disadvantage.gameObject.SetActive(Data.Summary.HasDisadvantage);
		if ((bool)m_TargetActorIcon)
		{
			m_TargetActorIcon.sprite = GetIcon(Data.Summary.Summary.Actor);
		}
	}

	private Sprite GetIcon(CActor actor)
	{
		if (!(actor is CEnemyActor cEnemyActor))
		{
			if (!(actor is CHeroSummonActor cHeroSummonActor))
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					return UIInfoTools.Instance.GetCharacterConfigUI(cPlayerActor.CharacterClass.CharacterModel)?.InfoIllustration;
				}
				return null;
			}
			return UIInfoTools.Instance.GetSummonConfig(cHeroSummonActor.HeroSummonClass)?.Illusration;
		}
		return UIInfoTools.Instance.GetMonsterConfig(cEnemyActor.MonsterClass)?.Illustration;
	}

	private void FillIcons(List<AttackModifierYMLData> modifier, List<UIOutpostAttackTownGuardDrawnModifier> iconsCache, RectTransform container, bool interactable)
	{
		paramsList.Clear();
		container.gameObject.SetActive(modifier != null && modifier.Count > 0);
		if (modifier != null)
		{
			paramsList.AddRange(modifier.Select((AttackModifierYMLData ymlData) => new UIOutpostAttackTownGuardDrawnModifier.Params
			{
				Modifer = new AttackModifierDTO(ymlData, 0, 0),
				Interactable = interactable,
				PendingSelection = (interactable && !(Data.Summary.Selected?.Any() ?? true)),
				Selected = (Data.Summary.Selected?.Contains(ymlData) ?? false),
				Discarded = (Data.Summary.ModiferNotUsed?.Contains(ymlData) ?? false),
				OnSelectCallback = ((interactable && Data.Summary.OnSelectCallback != null) ? ((Action)(() =>
				{
					Data.Summary.OnSelectCallback?.Invoke(ymlData);
				})) : null),
				IsAbilityActive = (Data.Summary.ActiveAbilityModifiers?.Contains(ymlData) ?? false)
			}));
		}
		UIHelperUtil.UpdateUIList(paramsList.Count, iconsCache, m_ModifierPrototype, container, onShow);
		void onShow(int index, UIOutpostAttackTownGuardDrawnModifier item)
		{
			item.Init(paramsList[index]);
			item.gameObject.SetActive(value: true);
		}
	}

	private IEnumerator BeginDrawTransition()
	{
		InTransition = true;
		activeTweens.ForEach((Tween tween) =>
		{
			tween.Kill();
		});
		activeTweens.Clear();
		m_ContainerChoiceCanvas.alpha = 0f;
		List<UIOutpostAttackTownGuardDrawnModifier> list = m_DrawnModifiers.Where((UIOutpostAttackTownGuardDrawnModifier uIOutpostAttackTownGuardDrawnModifier) => uIOutpostAttackTownGuardDrawnModifier.Data.Modifer != null && uIOutpostAttackTownGuardDrawnModifier.gameObject.activeInHierarchy).ToList();
		float num = m_StartDelay;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			UIOutpostAttackTownGuardDrawnModifier icon = list[num2];
			icon.MoveContainer.gameObject.SetActive(value: true);
			if (m_UseMoveTween)
			{
				icon.MoveContainer.transform.SetParent(AnimationContainer);
				icon.MoveContainer.transform.position = StartPoint.transform.position;
			}
			if (m_UseMoveTween)
			{
				Tween travelTween = null;
				travelTween = icon.MoveContainer.transform.DOMove(icon.transform.position, m_ShowDuration).SetEase(Ease.OutSine).SetDelay(num)
					.OnComplete(() =>
					{
						icon.MoveContainer.transform.SetParent(icon.transform);
						activeTweens.Remove(travelTween);
					});
				activeTweens.Add(travelTween);
			}
			CanvasGroup component = icon.MoveContainer.GetComponent<CanvasGroup>();
			if (component != null)
			{
				component.alpha = 0f;
				Tween fadeTween = null;
				fadeTween = component.DOFade(1f, m_ShowDuration).SetEase(Ease.OutSine).SetDelay(num)
					.OnComplete(() =>
					{
						activeTweens.Remove(fadeTween);
					});
				activeTweens.Add(fadeTween);
			}
			num += m_PerSlotShowDelay;
			if (num2 == Data.Summary.ModiferDrawn.Count - 1)
			{
				num += m_PerSlotShowDelay;
			}
		}
		if (Data.Summary.HasAdvantage || Data.Summary.HasDisadvantage)
		{
			float delay = m_StartDelay + (float)Data.Summary.ModiferDrawn.Count * m_PerSlotShowDelay;
			Tween choiceGroupTween = null;
			choiceGroupTween = m_ContainerChoiceCanvas.DOFade(1f, m_ShowDuration).SetDelay(delay).OnComplete(() =>
			{
				activeTweens.Remove(choiceGroupTween);
			});
			activeTweens.Add(choiceGroupTween);
		}
		while (activeTweens.Count > 0)
		{
			yield return null;
		}
		InTransition = false;
	}

	public void Clear()
	{
		Data = default;
		InTransition = false;
		activeTweens.ForEach((Tween tween) =>
		{
			tween.Kill();
		});
		activeTweens.Clear();
		m_ContainerChoiceCanvas.alpha = 0f;
		for (int num = 0; num < m_DrawnModifiers.Count; num++)
		{
			CanvasGroup component = m_DrawnModifiers[num].MoveContainer.GetComponent<CanvasGroup>();
			if (component != null)
			{
				component.alpha = 1f;
			}
		}
	}
}
