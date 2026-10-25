using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioMechanic_062_SpawnTable : UIScenarioMechanicsEntry
{
	[FormerlySerializedAs("m_MonstersContainer")]
	[Header("Components")]
	[SerializeField]
	private RectTransform m_TokenGroupContainer;

	[SerializeField]
	private GameObject m_MonsterIconPrototype;

	[SerializeField]
	private GameObject m_TokenGroupPrototype;

	[Header("Components")]
	[SerializeField]
	private float m_ConsumedAlpha = 0.3f;

	private readonly Dictionary<int, List<GameObject>> m_MonsterIcons = new Dictionary<int, List<GameObject>>();

	private List<GameObject> m_TokenGroups;

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			m_MonsterIconPrototype.gameObject.SetActive(value: false);
			m_TokenGroupPrototype.gameObject.SetActive(value: false);
		}
	}

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		InitCache();
		if (ScenarioScript.current is ScenarioScript_FH062 scenarioScript)
		{
			Populate(scenarioScript, localizationParams);
		}
	}

	private void Populate(ScenarioScript_FH062 scenarioScript, LocalizationParam[] localizationParams)
	{
		if (m_TokenGroupContainer == null || m_MonsterIconPrototype == null)
		{
			return;
		}
		int count = scenarioScript.SpawnerGUIDs.Count;
		HelperTools.NormalizePool(ref m_TokenGroups, m_TokenGroupPrototype, m_TokenGroupContainer, count);
		new Dictionary<int, List<CSpawner>>();
		for (int i = 0; i < count; i++)
		{
			bool flag = !scenarioScript.RemovedNumberTokens.Contains(i + 1);
			GameObject gameObject = m_TokenGroups[i];
			gameObject.gameObject.SetActive(flag);
			if (!flag)
			{
				continue;
			}
			if (!m_MonsterIcons.TryGetValue(i, out var value))
			{
				value = new List<GameObject>();
				m_MonsterIcons.Add(i, value);
			}
			List<CSpawner> list = scenarioScript.GetTokenSpawners(i).ToList();
			bool consumed = scenarioScript.UsedNumberTokens.Contains(i + 1);
			HelperTools.NormalizePool(ref value, m_MonsterIconPrototype, gameObject.transform, list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				CSpawner cSpawner = list[j];
				int index = Mathf.Min(cSpawner.CurrentSpawnRoundEntryIndex, cSpawner.SpawnRoundEntries.Count - 1);
				string text = cSpawner.SpawnRoundEntries[index].SpawnClass[ScenarioScript.PartySize - 1];
				if (!(text == "DON'T SPAWN"))
				{
					PopulateMonsterIcon(value[j], text, consumed);
				}
			}
		}
	}

	private void PopulateMonsterIcon(GameObject iconInstance, string monsterId, bool consumed)
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
				component.SetText(LocalizationManager.GetTranslation(cClass.LocKey));
			}
			CanvasGroup component2 = iconInstance.GetComponent<CanvasGroup>();
			if (component2 != null)
			{
				component2.alpha = (consumed ? m_ConsumedAlpha : 1f);
			}
		}
	}
}
