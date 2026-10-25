using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class ScenarioResultModule : MonoBehaviour, IUIViewModule
{
	public struct Params
	{
		public UIDebriefingBattleGoals.Params BattleGoals;

		public UIDebriefingCampaignRewards.Params CampaignRewards;

		public UIDebriefingHeroRewards.Params HeroRewards;

		public UIDebriefingBattleStats.Params BattleStats;
	}

	[Header("Components")]
	[SerializeField]
	private UIDebriefingBattleGoals BattleGoals;

	[SerializeField]
	private UIDebriefingCampaignRewards CampaignRewards;

	[SerializeField]
	private UIDebriefingHeroRewards HeroRewards;

	[SerializeField]
	private UIDebriefingBattleStats BattleStats;

	[SerializeField]
	private FHButton ButtonClose;

	private bool m_Initialzied;

	public GameObject GameObject { get; }

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			ButtonClose.onClick.AddListener(HandleOnClose);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		BattleStats.Init(@params.BattleStats);
		BattleGoals.Init(@params.BattleGoals);
		CampaignRewards.Init(@params.CampaignRewards);
		HeroRewards.Init(@params.HeroRewards);
	}

	private void HandleOnClose()
	{
		Hide();
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
