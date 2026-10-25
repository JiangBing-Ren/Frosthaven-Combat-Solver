using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexMonsterEntry : FHUICodexEntry
{
	[Header("Monster Entry Components")]
	[SerializeField]
	private Image m_IllustrationNormal;

	[SerializeField]
	private Image m_IllustrationElite;

	[SerializeField]
	private UIMonsterMiniCardContainer m_MonsterCards;

	[SerializeField]
	private FHUICodexTable m_StatsTable;

	[SerializeField]
	private ToggleGroup m_ToggleGroup;

	[SerializeField]
	private FHToggle m_ToggleNormal;

	[SerializeField]
	private FHToggle m_ToggleElite;

	[SerializeField]
	private FHTabsGroup m_LevelSelectorTabsGroup;

	[Header("Settings")]
	[SerializeField]
	private Material m_MaterialIllustrationNormal;

	[SerializeField]
	private Material m_MaterialIllustrationSelected;

	[SerializeField]
	private Color m_ColorIllustrationNormal;

	[SerializeField]
	private Color m_ColorIllustrationSelected;

	[SerializeField]
	private float m_ScaleIllustrationNormal = 0.8f;

	[SerializeField]
	private float m_ScaleIllustrationSelected = 1f;

	[SerializeField]
	private float m_StatRowHeight = 42f;

	[SerializeField]
	private float m_StatColumnWidth = 64f;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorStatText;

	[SerializeField]
	private Color m_ColorLevelText;

	[SerializeField]
	private Color m_ColorHealthText;

	public FHTabButton[] LevelButtons;

	private readonly List<MonsterCardUIContext> m_monsterCardsContext = new List<MonsterCardUIContext>(10);

	private string m_selectedMonsterVariant;

	private int m_SelectedLevel;

	private string NormalMonsterId => Data.ID + "ID";

	private string EliteMonsterId => Data.ID + "EliteID";

	private void Start()
	{
		m_ToggleNormal.onValueChanged.AddListener((bool active) =>
		{
			SelectVariant(!active);
		});
	}

	public override void Setup(CodexConfig.CodexEntry entry)
	{
		base.Setup(entry);
		Data = entry;
		m_Caption.SetTextKey(entry.LocalizationKey);
		m_IllustrationNormal.sprite = UIInfoTools.Instance.GetMonsterConfig(NormalMonsterId)?.Illustration;
		m_IllustrationElite.sprite = UIInfoTools.Instance.GetMonsterConfig(EliteMonsterId)?.Illustration;
		for (int i = 0; i < LevelButtons.Length; i++)
		{
			int level = i;
			LevelButtons[i].onClick.AddListener(() =>
			{
				OnLevelSelected(level, m_selectedMonsterVariant);
			});
		}
		SelectVariant(elite: false);
		OnLevelSelected(FHUICodexUtils.GetMonsterOverrideLevel().GetValueOrDefault(), m_selectedMonsterVariant);
	}

	private void SelectVariant(bool elite)
	{
		m_selectedMonsterVariant = (elite ? EliteMonsterId : NormalMonsterId);
		Rebuild();
	}

	private void Rebuild()
	{
		PopulateDeck(m_selectedMonsterVariant, m_SelectedLevel);
		PopulateStatsTable(m_selectedMonsterVariant);
		bool flag = m_selectedMonsterVariant == NormalMonsterId;
		m_IllustrationNormal.material = (flag ? m_MaterialIllustrationSelected : m_MaterialIllustrationNormal);
		m_IllustrationElite.material = ((!flag) ? m_MaterialIllustrationSelected : m_MaterialIllustrationNormal);
		m_IllustrationNormal.transform.localScale = (flag ? (Vector3.one * m_ScaleIllustrationSelected) : (Vector3.one * m_ScaleIllustrationNormal));
		m_IllustrationElite.transform.localScale = ((!flag) ? (Vector3.one * m_ScaleIllustrationSelected) : (Vector3.one * m_ScaleIllustrationNormal));
	}

	private void PopulateStatsTable(string monsterId)
	{
		int selectedLevel = m_SelectedLevel;
		TableData data = FHUICodexUtils.PopulateMonsterData(monsterId, selectedLevel, m_StatRowHeight, m_StatColumnWidth, m_ColorStatText, m_ColorLevelText, m_ColorHealthText, m_StatsTable.GetCellTextPrototype);
		m_StatsTable.Init(data);
	}

	private void PopulateDeck(string monsterId, int? overrideLevel = null)
	{
		CMonsterClass monsterClass = MonsterClassManager.Find(monsterId);
		MonsterCardDTO[] source = (from c in monsterClass.AbilityCardsPool
			select new MonsterCardDTO(c, UIGeneralInfoUtils.GetMonsterCardPile(c.ID, monsterClass)) into c
			orderby c.Initiative
			select c).ToArray();
		m_monsterCardsContext.Clear();
		m_monsterCardsContext.AddRange(source.Select((MonsterCardDTO card) => FHUICodexUtils.GetCardUIContext(card.ID, monsterClass, overrideLevel)));
		m_MonsterCards.Init(null, m_monsterCardsContext);
	}

	private void OnLevelSelected(int level, string monsterId)
	{
		m_SelectedLevel = level;
		if (Data != null && m_selectedMonsterVariant.IsNOTNullOrEmpty())
		{
			Rebuild();
		}
		if (MonsterClassManager.Find(monsterId) != null)
		{
			m_monsterCardsContext.ForEach((MonsterCardUIContext ctx) =>
			{
				ctx.OverrideLevel = level;
			});
			m_MonsterCards.Init(null, m_monsterCardsContext);
			m_LevelSelectorTabsGroup.SwitchToTab(LevelButtons[level]);
		}
	}
}
