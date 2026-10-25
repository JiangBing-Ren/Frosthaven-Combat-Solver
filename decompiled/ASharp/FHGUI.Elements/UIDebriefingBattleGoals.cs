using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public class UIDebriefingBattleGoals : MonoBehaviour
{
	public struct Params
	{
		public BattleResultDTO BattleResults;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform RowHeadersContainer;

	[SerializeField]
	private RectTransform ColumnContainer;

	[SerializeField]
	private UIDebriefingBattleGoalsColumn ColumnPrefab;

	[SerializeField]
	private GameObject BattleGoalLabel;

	[SerializeField]
	private GameObject MasteriesLabel;

	private List<UIDebriefingBattleGoalsColumn> m_HeroColumns = new List<UIDebriefingBattleGoalsColumn>();

	private List<UIDebriefingTableHeader> m_RowHeaders = new List<UIDebriefingTableHeader>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			ColumnContainer.GetComponentsInChildren(includeInactive: true, m_HeroColumns);
			m_RowHeaders.AddRange(RowHeadersContainer.GetComponentsInChildren<UIDebriefingTableHeader>(includeInactive: true));
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		int count = @params.BattleResults.IndividualRewards.Count;
		HelperTools.NormalizePool(ref m_RowHeaders, m_RowHeaders[0].gameObject, RowHeadersContainer.transform, count);
		for (int i = 0; i < count; i++)
		{
			BattleResultDTO.HeroStats heroStats = @params.BattleResults.IndividualRewards[i];
			m_RowHeaders[i].Init(new DebriefingTableDTO.Header
			{
				Icon = UIInfoTools.Instance.GetCharacterConfigUI(heroStats.ClassID).InfoIllustration
			});
		}
		bool active = false;
		bool active2 = false;
		HelperTools.NormalizePool(ref m_HeroColumns, ColumnPrefab.gameObject, ColumnContainer.transform, count);
		for (int j = 0; j < count; j++)
		{
			BattleResultDTO.HeroStats heroStats2 = @params.BattleResults.IndividualRewards[j];
			m_HeroColumns[j].Init(heroStats2);
			if (heroStats2.BattleGoal != null)
			{
				active = true;
			}
			if (heroStats2.Masteries != null && heroStats2.Masteries.Count > 0)
			{
				active2 = true;
			}
		}
		if (BattleGoalLabel != null)
		{
			BattleGoalLabel.SetActive(active);
		}
		if (MasteriesLabel != null)
		{
			MasteriesLabel.SetActive(active2);
		}
	}
}
