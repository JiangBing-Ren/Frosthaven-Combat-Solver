using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexBossEntry : FHUICodexEntry
{
	[Header("Boss Entry Components")]
	[SerializeField]
	private Image m_IllustrationSingleFrom;

	[SerializeField]
	private Image m_IllustrationNormal;

	[SerializeField]
	private Image m_IllustrationElite;

	[SerializeField]
	private GameObject m_GroupSingleFrom;

	[SerializeField]
	private GameObject m_GroupDualFrom;

	[SerializeField]
	private UIMonsterMiniCardContainer m_MonsterCards;

	[SerializeField]
	private FHUICodexTable m_StatsTable;

	[SerializeField]
	private GameObject GroupImmunities;

	[SerializeField]
	private UIConditionEffectListContainer Immunities;

	[SerializeField]
	private FHTabButton[] m_LevelButtons;

	[SerializeField]
	private FHTabsGroup m_LevelSelectorTabsGroup;

	[SerializeField]
	private FHToggle m_ToggleNormal;

	[SerializeField]
	private FHToggle m_ToggleElite;

	[Header("Settings")]
	[SerializeField]
	private float m_StatRowHeight = 42f;

	[SerializeField]
	private float m_StatColumnWidth = 64f;

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

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorStatText;

	[SerializeField]
	private Color m_ColorLevelText;

	[SerializeField]
	private Color m_ColorHealthText;

	private new CodexConfig.CodexEntry Data;

	private readonly List<MonsterCardUIContext> m_monsterCardsContext = new List<MonsterCardUIContext>(10);

	private bool HasEliteFrom;

	private string m_selectedMonsterVariant;

	private int m_SelectedLevel;

	private string MonsterId => Data.ID + "ID";

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
		HasEliteFrom = MonsterClassManager.Find(EliteMonsterId) != null;
		m_Caption.SetTextKey(entry.LocalizationKey);
		m_GroupSingleFrom.SetActive(!HasEliteFrom);
		m_GroupDualFrom.SetActive(HasEliteFrom);
		Sprite sprite = UIInfoTools.Instance.GetMonsterConfig(MonsterId)?.Illustration;
		m_IllustrationSingleFrom.sprite = sprite;
		m_IllustrationNormal.sprite = sprite;
		if ((bool)m_Caption)
		{
			m_IllustrationElite.sprite = UIInfoTools.Instance.GetMonsterConfig(EliteMonsterId)?.Illustration;
		}
		for (int i = 0; i < m_LevelButtons.Length; i++)
		{
			int level = i;
			m_LevelButtons[i].onClick.AddListener(() =>
			{
				OnLevelSelected(level, m_selectedMonsterVariant);
			});
		}
		m_selectedMonsterVariant = MonsterId;
		OnLevelSelected(FHUICodexUtils.GetMonsterOverrideLevel().GetValueOrDefault(), m_selectedMonsterVariant);
		Rebuild();
	}

	private void SelectVariant(bool elite)
	{
		m_selectedMonsterVariant = (elite ? EliteMonsterId : MonsterId);
		OnLevelSelected(m_SelectedLevel, m_selectedMonsterVariant);
		Rebuild();
	}

	private void Rebuild()
	{
		if (MonsterClassManager.Find(m_selectedMonsterVariant) == null)
		{
			Log.GUIWarning("Missing monster class with id: " + m_selectedMonsterVariant);
			return;
		}
		bool flag = m_selectedMonsterVariant == MonsterId;
		m_IllustrationNormal.material = (flag ? m_MaterialIllustrationSelected : m_MaterialIllustrationNormal);
		m_IllustrationElite.material = ((!flag) ? m_MaterialIllustrationSelected : m_MaterialIllustrationNormal);
		m_IllustrationNormal.transform.localScale = (flag ? (Vector3.one * m_ScaleIllustrationSelected) : (Vector3.one * m_ScaleIllustrationNormal));
		m_IllustrationElite.transform.localScale = ((!flag) ? (Vector3.one * m_ScaleIllustrationSelected) : (Vector3.one * m_ScaleIllustrationNormal));
		PopulateDeck(m_selectedMonsterVariant, FHUICodexUtils.GetMonsterOverrideLevel());
	}

	private void PopulateStatsTable(string monsterId, int? overrideLevel = null)
	{
		int highlightLevel = overrideLevel ?? (ScenarioManager.IsInTactical ? ScenarioManager.CurrentScenarioState.Level : (-1));
		TableData data = FHUICodexUtils.PopulateMonsterData(monsterId, highlightLevel, m_StatRowHeight, m_StatColumnWidth, m_ColorStatText, m_ColorLevelText, m_ColorHealthText, m_StatsTable.GetCellTextPrototype, includeImmunities: false);
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
		CMonsterClass cMonsterClass = MonsterClassManager.Find(monsterId);
		if (cMonsterClass != null)
		{
			if (Data != null)
			{
				PopulateStatsTable(monsterId, m_SelectedLevel);
			}
			m_monsterCardsContext.ForEach((MonsterCardUIContext ctx) =>
			{
				ctx.OverrideLevel = level;
			});
			m_MonsterCards.Init(null, m_monsterCardsContext);
			m_LevelSelectorTabsGroup.SwitchToTab(m_LevelButtons[level]);
			List<StatusEffectDTO> list = UICharacterUtils.GetImmunities(cMonsterClass.GetMonsterStatsData(level).Immunities.Where((CAbility.EAbilityType type) => !FHUICodexUtils.InvalidImmunities.Contains(type)), cMonsterClass).ToList();
			Immunities.Init(list);
			GroupImmunities.SetActive(list.Count > 0);
		}
	}
}
