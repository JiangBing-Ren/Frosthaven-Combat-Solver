using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioMechanic_054 : UIScenarioMechanicsEntry
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_MonstersContainer;

	[SerializeField]
	private GameObject m_MonsterIconPrototype;

	[SerializeField]
	private GameObject m_CoralIcon;

	[Header("Components")]
	[SerializeField]
	private float m_ConsumedAlpha = 0.3f;

	private List<GameObject> m_MonsterIcons;

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			m_MonsterIconPrototype.gameObject.SetActive(value: false);
		}
	}

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		InitCache();
		if (ScenarioScript.current is ScenarioScript_FH054 scenarioScript)
		{
			Populate(scenarioScript, localizationParams);
		}
	}

	private void Populate(ScenarioScript_FH054 scenarioScript, LocalizationParam[] localizationParams)
	{
		if (m_MonstersContainer == null || m_MonsterIconPrototype == null)
		{
			return;
		}
		int num = scenarioScript.TokensSpawners.Length;
		HelperTools.NormalizePool(ref m_MonsterIcons, m_MonsterIconPrototype, m_MonstersContainer, num);
		for (int i = 0; i < num; i++)
		{
			CSpawner cSpawner = scenarioScript.TokensSpawners[i];
			if (cSpawner.CurrentSpawnRoundEntryIndex < cSpawner.SpawnRoundEntries.Count)
			{
				string monsterId = cSpawner.SpawnRoundEntries[cSpawner.CurrentSpawnRoundEntryIndex].SpawnClass[ScenarioScript.PartySize - 1];
				int count = cSpawner.SpawnRoundEntries.Count((SpawnRoundEntry e) => e.SpawnClass[ScenarioScript.PartySize - 1] != "DON'T SPAWN");
				int item = i + 1;
				bool consumed = scenarioScript.TokensSpent.Contains(item) && scenarioScript.Tokens.Count > 0;
				PopulateMonsterIcon(m_MonsterIcons[i], monsterId, count, consumed);
			}
		}
		UITextTooltipTarget uITextTooltipTarget = m_CoralIcon?.GetComponent<UITextTooltipTarget>();
		if (uITextTooltipTarget != null)
		{
			uITextTooltipTarget.SetText(FHUILocFormatting.PopulateParams("FH_54_Destroying_a_Coral_Rubble_Damage_D", localizationParams));
		}
		CanvasGroup component = m_CoralIcon.GetComponent<CanvasGroup>();
		if (component != null)
		{
			bool flag = scenarioScript.TokensSpent.Contains(6) && scenarioScript.Tokens.Count > 0;
			component.alpha = (flag ? m_ConsumedAlpha : 1f);
		}
	}

	private void PopulateMonsterIcon(GameObject iconInstance, string monsterId, int count, bool consumed)
	{
		CClass cClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == monsterId);
		if (cClass != null)
		{
			string defaultModel = cClass.DefaultModel;
			Sprite monsterPortrait = UIInfoTools.GetMonsterPortrait(monsterId, defaultModel, out var _);
			Image image = iconInstance.FindInChildren("Avatar_Image")?.GetComponent<Image>();
			if ((bool)image)
			{
				image.sprite = monsterPortrait;
			}
			iconInstance.gameObject.SetActive(value: true);
			UITextTooltipTarget component = iconInstance.GetComponent<UITextTooltipTarget>();
			if (component != null)
			{
				component.SetText(FHUILocFormatting.PopulateParams("FH_54_Destroying_a_Coral_Rubble_Monster_Summon_D", count, LocalizationManager.GetTranslation(cClass.LocKey)));
			}
			CanvasGroup component2 = iconInstance.GetComponent<CanvasGroup>();
			if (component2 != null)
			{
				component2.alpha = (consumed ? m_ConsumedAlpha : 1f);
			}
		}
	}
}
