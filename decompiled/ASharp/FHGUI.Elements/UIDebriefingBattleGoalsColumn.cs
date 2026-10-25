using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using UnityEngine;

namespace FHGUI.Elements;

public class UIDebriefingBattleGoalsColumn : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private UIGoalSummary BattleGoal;

	[SerializeField]
	private RectTransform MasteriesContainer;

	[SerializeField]
	private UIGoalSummary MasteryPrefab;

	[SerializeField]
	private CharacterPersonalQuestModule PersonalQuest;

	private List<UIGoalSummary> m_Masteries = new List<UIGoalSummary>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			MasteriesContainer.GetComponentsInChildren(includeInactive: true, m_Masteries);
		}
	}

	public void Init(BattleResultDTO.HeroStats heroStats)
	{
		InitCache();
		if (heroStats.BattleGoal != null)
		{
			BattleGoal.gameObject.SetActive(value: true);
			BattleGoal.Init(new UIGoalSummary.Params
			{
				DTO = heroStats.BattleGoal
			});
		}
		else
		{
			BattleGoal.gameObject.SetActive(value: false);
		}
		int num = heroStats.Masteries?.Count ?? 0;
		HelperTools.NormalizePool(ref m_Masteries, MasteryPrefab.gameObject, MasteriesContainer.transform, num);
		for (int i = 0; i < num; i++)
		{
			m_Masteries[i].Init(new UIGoalSummary.Params
			{
				DTO = heroStats.Masteries[i]
			});
		}
		PersonalQuest.Init(new CharacterPersonalQuestModule.Params
		{
			PersonalQuest = heroStats.PersonalQuest,
			CanRetire = heroStats.CanRetire
		});
	}
}
