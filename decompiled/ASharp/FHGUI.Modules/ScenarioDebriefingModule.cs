using System;
using System.Collections.Generic;
using DG.Tweening;
using FFSNet;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Services;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class ScenarioDebriefingModule : MonoBehaviour, IUIViewModule
{
	public struct Params
	{
		public string ScenarioNameKey;

		public BattleResultDTO BattleResults;

		public Action OnRetry;

		public Action OnReturnToMap;
	}

	[Header("Components")]
	[SerializeField]
	private ScenarioResultModule ScenarioResults;

	[SerializeField]
	private TextLocalizedListener ScenarioName;

	[SerializeField]
	private Image HeaderBackground;

	[SerializeField]
	private RectTransform HeroSlotContainer;

	[SerializeField]
	private UIDebriefingHeroSlot HeroSlotPrototype;

	[SerializeField]
	public FHButton OpenBattleStats;

	[SerializeField]
	public RectTransform ButtonsContainer;

	[SerializeField]
	public GameObject ButtonsVerticalSpacer;

	[SerializeField]
	private Button ButtonSettings;

	[SerializeField]
	private Button ButtonCodex;

	[Header("Settings")]
	[SerializeField]
	private Sprite WinSrpite;

	[SerializeField]
	private Sprite LoseSrpite;

	private List<UIDebriefingHeroSlot> m_HeroSlots = new List<UIDebriefingHeroSlot>();

	private bool invalidateButtonLayout;

	private bool m_Initialzied;

	[Header("Animation Targets")]
	[SerializeField]
	private RectTransform headerRect;

	[SerializeField]
	private RectTransform footerRect;

	[SerializeField]
	private CanvasGroup footerCanvasGroup;

	public GameObject GameObject { get; }

	public Params Data { get; private set; }

	public bool AreBattleStatsShown => ScenarioResults.isActiveAndEnabled;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			OpenBattleStats.onClick.AddListener(HandleOnOpenBattleStats);
			ScenarioResults.gameObject.SetActive(value: false);
			HeroSlotContainer.GetComponentsInChildren(includeInactive: true, m_HeroSlots);
			ButtonSettings.onClick.AddListener(() =>
			{
				Singleton<ESCMenu>.InstanceFast?.Show();
			});
			ButtonCodex.onClick.AddListener(() =>
			{
				Singleton<FHUICodex>.InstanceFast?.Show();
			});
		}
	}

	public void Init(Params @params, EResult result)
	{
		InitCache();
		Data = @params;
		if ((bool)ScenarioName)
		{
			ScenarioName.SetTextKey(Data.ScenarioNameKey);
		}
		if ((bool)HeaderBackground)
		{
			HeaderBackground.sprite = ((result == EResult.Lose || result == EResult.Resign) ? LoseSrpite : WinSrpite);
		}
		PopulateHeroSlots(@params.BattleResults);
		UpdateButtonsLayout();
		ScenarioResultModule.Params obj = new ScenarioResultModule.Params
		{
			BattleStats = new UIDebriefingBattleStats.Params
			{
				TableData = ScenarioResultViewService.BuildBattleStatsTable(@params.BattleResults)
			},
			BattleGoals = new UIDebriefingBattleGoals.Params
			{
				BattleResults = Data.BattleResults
			},
			CampaignRewards = new UIDebriefingCampaignRewards.Params
			{
				BattleResults = Data.BattleResults
			},
			HeroRewards = new UIDebriefingHeroRewards.Params
			{
				TableData = ScenarioResultViewService.BuildHeroRewardsTable(@params.BattleResults)
			}
		};
		ScenarioResults.Init(obj);
		invalidateButtonLayout = true;
	}

	private void PopulateHeroSlots(BattleResultDTO battleResults)
	{
		int num = 4;
		HeroSlotContainer.GetComponentsInChildren(includeInactive: true, m_HeroSlots);
		HelperTools.NormalizePool(ref m_HeroSlots, HeroSlotPrototype.gameObject, HeroSlotContainer.transform, num);
		for (int i = 0; i < num; i++)
		{
			UIDebriefingHeroSlot.Params obj = new UIDebriefingHeroSlot.Params
			{
				HeroStats = ((battleResults.IndividualRewards.Count > i) ? battleResults.IndividualRewards[i] : null),
				NetworkPlayer = (FFSNetwork.IsOnline ? PlayerRegistry.GetPlayerAssignedToSlot(PartySlotID.FromIndex(i)) : null)
			};
			m_HeroSlots[i].Init(obj);
			m_HeroSlots[i].Hide();
		}
	}

	private void UpdateButtonsLayout()
	{
		int num = 0;
		Transform transform = ButtonsContainer.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		ButtonsVerticalSpacer.SetActive(num <= 1);
	}

	private void HandleOnOpenBattleStats()
	{
		ScenarioResults.Show();
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
		invalidateButtonLayout = true;
		BeingShowAnimations();
	}

	private void LateUpdate()
	{
		if (invalidateButtonLayout)
		{
			invalidateButtonLayout = false;
			UpdateButtonsLayout();
		}
	}

	public void InvalidateButtonLayout()
	{
		invalidateButtonLayout = true;
	}

	[ContextMenu("Test Show Anim", false)]
	public void RunTestAnim()
	{
		if (Application.isPlaying && m_Initialzied)
		{
			BeingShowAnimations();
		}
	}

	private void BeingShowAnimations()
	{
		headerRect.pivot = new Vector2(0.5f, 0f);
		headerRect.DOPivotY(0.55f, 0.6f).SetDelay(0.6f).SetEase(Ease.InCubic);
		footerRect.pivot = new Vector2(0.5f, 1.2f);
		footerRect.DOPivotY(0f, 0.6f).SetDelay(0.8f).SetEase(Ease.InCubic);
		footerCanvasGroup.DOFade(1f, 0.6f).SetDelay(0.8f).SetEase(Ease.InCubic);
		for (int i = 0; i < m_HeroSlots.Count; i++)
		{
			m_HeroSlots[i].Show(1f + (float)i * 0.4f);
		}
	}
}
