using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FHGUI.Elements;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Scenario.Modules;

public class ScenarioAttackSummaryModule : MonoBehaviour, IUIViewModule
{
	public struct Params
	{
		public List<UIScenarioAttackSummaryElement.SummaryData> Summaries;
	}

	[Header("Elements")]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private UIScenarioAttackSummaryElement m_SummeryPrototype;

	[SerializeField]
	private RectTransform m_ContainerSummaries;

	[SerializeField]
	private TMP_Text m_TargetName;

	[SerializeField]
	private FHButton m_PrevSummery;

	[SerializeField]
	private FHButton m_NextSummery;

	[SerializeField]
	private FHButton m_FocusOnTarget;

	private readonly List<UIScenarioAttackSummaryElement> m_Summaries = new List<UIScenarioAttackSummaryElement>();

	private int m_CurrentShowIndex;

	private bool m_Initialed;

	public GameObject GameObject => gameObject;

	public Params Data { get; private set; }

	private UIScenarioAttackSummaryElement.SummaryData SelectedSummery => default;

	private void InitCache()
	{
		if (!m_Initialed)
		{
			m_Initialed = true;
			m_PrevSummery.onClick.AddListener(HandleOnPrevSummery);
			m_NextSummery.onClick.AddListener(HandleOnNextSummery);
			m_FocusOnTarget.onClick.AddListener(FocusOnTarget);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		Build();
	}

	private void Build()
	{
		int num = Data.Summaries?.Count ?? 0;
		m_CurrentShowIndex = num - 1;
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		if (cAbility != null && Data.Summaries != null)
		{
			CAttackSummary.TargetSummary targetSummary = cAbility.VarsContext.GetVar("AbilityCameFromSummary").GetObject<CAttackSummary.TargetSummary>();
			if (targetSummary != null)
			{
				for (int i = 0; i < num; i++)
				{
					if (Data.Summaries[i].Summary == targetSummary)
					{
						m_CurrentShowIndex = i;
						break;
					}
				}
			}
		}
		if (Data.Summaries != null && Data.Summaries.Count > m_CurrentShowIndex)
		{
			_ = Data.Summaries[m_CurrentShowIndex];
		}
		UIHelperUtil.UpdateUIList(num, m_Summaries, m_SummeryPrototype, m_ContainerSummaries, onShow);
		SelectSummery(m_CurrentShowIndex);
		void onShow(int index, UIScenarioAttackSummaryElement item)
		{
			UIScenarioAttackSummaryElement.SummaryData summaryData = Data.Summaries[index];
			bool showWithAnimation = summaryData.Summary.State == CAttackSummary.TargetSummary.SummaryState.DrawingModifiers && !item.Data.Summary.IsSimilar(summaryData);
			item.Init(new UIScenarioAttackSummaryElement.Params
			{
				Summary = summaryData,
				ShowWithAnimation = showWithAnimation
			});
		}
	}

	private void SelectSummery(int summeryIndex)
	{
		m_CurrentShowIndex = summeryIndex;
		for (int i = 0; i < m_Summaries.Count; i++)
		{
			m_Summaries[i].gameObject.SetActive(i == m_CurrentShowIndex);
		}
		int num = Data.Summaries?.Count ?? 0;
		bool active = Data.Summaries != null && num > 1;
		m_PrevSummery.gameObject.SetActive(active);
		m_PrevSummery.interactable = m_CurrentShowIndex > 0;
		m_NextSummery.gameObject.SetActive(active);
		m_NextSummery.interactable = m_CurrentShowIndex < num - 1;
		UIScenarioAttackSummaryElement.SummaryData? summaryData = ((Data.Summaries != null && Data.Summaries.Count > m_CurrentShowIndex) ? new UIScenarioAttackSummaryElement.SummaryData?(Data.Summaries[m_CurrentShowIndex]) : ((UIScenarioAttackSummaryElement.SummaryData?)null));
		if (summaryData.HasValue)
		{
			CActor actor = summaryData.Value.Summary.Actor;
			string sourceText = LocalizationManager.GetTranslation(actor.ActorLocKey()) + Choreographer.GetActorIDForCombatLogIfNeeded(actor);
			m_TargetName.SetText(sourceText);
		}
		else
		{
			m_TargetName.SetText(string.Empty);
		}
	}

	private void HandleOnNextSummery()
	{
		SelectSummery(m_CurrentShowIndex + 1);
	}

	private void HandleOnPrevSummery()
	{
		SelectSummery(m_CurrentShowIndex - 1);
	}

	private void FocusOnTarget()
	{
		GameObject actorBehaviour = GetActorBehaviour(SelectedSummery.Summary);
		if ((bool)actorBehaviour)
		{
			CameraController.s_CameraController.SetFocalPointGameObject(actorBehaviour);
		}
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide(bool immediate = false)
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		if (immediate)
		{
			gameObject.SetActive(value: false);
			return;
		}
		m_CanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
		{
			gameObject.SetActive(value: false);
		});
		m_Summaries.ForEach((UIScenarioAttackSummaryElement element) =>
		{
			element.Clear();
		});
	}

	public bool InTransition()
	{
		return m_Summaries.Any((UIScenarioAttackSummaryElement element) => element.InTransition);
	}

	private static GameObject GetActorBehaviour(CAttackSummary.TargetSummary summary)
	{
		if (summary == null)
		{
			return null;
		}
		CActor cActor = summary.ActorToAttack;
		if (summary.RedirectedFromActor != null)
		{
			cActor = summary.RedirectedFromActor;
		}
		cActor = (summary.ActorToAttack.IsScenarioActor ? summary.Actor : cActor);
		return Choreographer.s_Choreographer.FindClientActor(cActor);
	}
}
