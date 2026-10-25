using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorSpawnerPanel : MonoBehaviour
{
	[Header("UI Elements")]
	public TMP_Text Title;

	public TMP_Dropdown TriggerTypeDropDown;

	public TMP_Dropdown ActivationTypeDropDown;

	public TMP_Dropdown SpawnAllegienceDropdown;

	public InputField StartRoundInputField;

	public Toggle LoopPatternToggle;

	public Toggle ShouldCountTowardsKillAllEnemiesToggle;

	public InputField HoverPanelNameLocInputField;

	public InputField PartySizeOneSpawnRoundInterval;

	public InputField PartySizeTwoSpawnRoundInterval;

	public InputField PartySizeThreeSpawnRoundInterval;

	public InputField PartySizeFourSpawnRoundInterval;

	public LevelEditorSpawnerRoundEntriesPanel entriesPanel;

	public TMP_Dropdown PartySizeOneConfigDropDown;

	public TMP_Dropdown PartySizeTwoConfigDropDown;

	public TMP_Dropdown PartySizeThreeConfigDropDown;

	public TMP_Dropdown PartySizeFourConfigDropDown;

	[SerializeField]
	private Button m_locateButton;

	[SerializeField]
	private Button m_moveButton;

	[SerializeField]
	private Button m_deleteButton;

	[SerializeField]
	private Button m_applyButton;

	[SerializeField]
	private Button m_rotateButton;

	[HideInInspector]
	private CSpawner m_spawner;

	private List<CActor.EType> m_TypesToAllow;

	public void SetupButtonCallbacks(Action onLocateUnitPressed, Action onMoveUnitPressed, Action onDeleteUnitPressed, Action onApplyToUnitPressed, Action onRotateToUnitPressed)
	{
		m_locateButton.onClick.RemoveAllListeners();
		m_locateButton.onClick.AddListener(() =>
		{
			onLocateUnitPressed?.Invoke();
		});
		m_moveButton.onClick.RemoveAllListeners();
		m_moveButton.onClick.AddListener(() =>
		{
			onMoveUnitPressed?.Invoke();
		});
		m_deleteButton.onClick.RemoveAllListeners();
		m_deleteButton.onClick.AddListener(() =>
		{
			onDeleteUnitPressed?.Invoke();
		});
		m_applyButton.onClick.RemoveAllListeners();
		m_applyButton.onClick.AddListener(() =>
		{
			onApplyToUnitPressed?.Invoke();
		});
		m_rotateButton.onClick.RemoveAllListeners();
		m_rotateButton.onClick.AddListener(() =>
		{
			onRotateToUnitPressed?.Invoke();
		});
	}

	public void DisplayForSpawner(CSpawner spawner)
	{
		m_spawner = spawner;
		Title.text = spawner.SpawnerGuid;
		PartySizeOneConfigDropDown.options.Clear();
		PartySizeTwoConfigDropDown.options.Clear();
		PartySizeThreeConfigDropDown.options.Clear();
		PartySizeFourConfigDropDown.options.Clear();
		PartySizeOneConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
		PartySizeTwoConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
		PartySizeThreeConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
		PartySizeFourConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
		PartySizeOneConfigDropDown.value = (int)spawner.GetConfigForPartySize(1);
		PartySizeTwoConfigDropDown.value = (int)spawner.GetConfigForPartySize(2);
		PartySizeThreeConfigDropDown.value = (int)spawner.GetConfigForPartySize(3);
		PartySizeFourConfigDropDown.value = (int)spawner.GetConfigForPartySize(4);
		DisplayForSpawnerData(spawner.SpawnerData);
		entriesPanel.gameObject.SetActive(value: true);
		entriesPanel.DisplaySpawnRoundEntriesForSpawner(spawner.SpawnerData);
		SpawnAllegienceDropdown.options.Clear();
		m_TypesToAllow = CActor.Types.ToList();
		m_TypesToAllow.Remove(CActor.EType.Player);
		m_TypesToAllow.Remove(CActor.EType.HeroSummon);
		m_TypesToAllow.Remove(CActor.EType.Unknown);
		SpawnAllegienceDropdown.AddOptions(m_TypesToAllow.Select((CActor.EType t) => t.ToString()).ToList());
		int num = m_TypesToAllow.IndexOf(spawner.ActorTypeToSpawn);
		if (num < 0)
		{
			num = 0;
		}
		SpawnAllegienceDropdown.SetValueWithoutNotify(num);
	}

	private void DisplayForSpawnerData(SpawnerData spawnerData)
	{
		TriggerTypeDropDown.options.Clear();
		TriggerTypeDropDown.AddOptions(SpawnerData.SpawnerTriggerTypes.Select((SpawnerData.ESpawnerTriggerType s) => s.ToString()).ToList());
		TriggerTypeDropDown.value = (int)spawnerData.SpawnerTriggerType;
		ActivationTypeDropDown.options.Clear();
		ActivationTypeDropDown.AddOptions(SpawnerData.SpawnerActivationTypes.Select((SpawnerData.ESpawnerActivationType s) => s.ToString()).ToList());
		ActivationTypeDropDown.value = (int)spawnerData.SpawnerActivationType;
		StartRoundInputField.text = spawnerData.SpawnStartRound.ToString();
		HoverPanelNameLocInputField.text = spawnerData.SpawnerHoverNameLoc;
		LoopPatternToggle.isOn = spawnerData.LoopSpawnPattern;
		ShouldCountTowardsKillAllEnemiesToggle.isOn = spawnerData.ShouldCountTowardsKillAllEnemies;
		if (spawnerData.SpawnRoundInterval != null)
		{
			PartySizeOneSpawnRoundInterval.text = spawnerData.SpawnRoundInterval.ElementAtOrDefault(0).ToString();
			PartySizeTwoSpawnRoundInterval.text = spawnerData.SpawnRoundInterval.ElementAtOrDefault(1).ToString();
			PartySizeThreeSpawnRoundInterval.text = spawnerData.SpawnRoundInterval.ElementAtOrDefault(2).ToString();
			PartySizeFourSpawnRoundInterval.text = spawnerData.SpawnRoundInterval.ElementAtOrDefault(3).ToString();
		}
	}

	public void SaveValuesToSpawner()
	{
		m_spawner.SpawnerData.SpawnerTriggerType = SpawnerData.SpawnerTriggerTypes[TriggerTypeDropDown.value];
		m_spawner.SpawnerData.SpawnerActivationType = SpawnerData.SpawnerActivationTypes[ActivationTypeDropDown.value];
		m_spawner.SpawnerData.SpawnStartRound = ((!string.IsNullOrEmpty(StartRoundInputField.text)) ? int.Parse(StartRoundInputField.text) : 0);
		m_spawner.SpawnerData.LoopSpawnPattern = LoopPatternToggle.isOn;
		m_spawner.SpawnerData.ShouldCountTowardsKillAllEnemies = ShouldCountTowardsKillAllEnemiesToggle.isOn;
		m_spawner.SpawnerData.SpawnerHoverNameLoc = HoverPanelNameLocInputField.text;
		m_spawner.SetConfigForPartySize(1, (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value);
		m_spawner.SetConfigForPartySize(2, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value);
		m_spawner.SetConfigForPartySize(3, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value);
		m_spawner.SetConfigForPartySize(4, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
		int item = ((!string.IsNullOrEmpty(PartySizeOneSpawnRoundInterval.text)) ? int.Parse(PartySizeOneSpawnRoundInterval.text) : 0);
		int item2 = ((!string.IsNullOrEmpty(PartySizeTwoSpawnRoundInterval.text)) ? int.Parse(PartySizeTwoSpawnRoundInterval.text) : 0);
		int item3 = ((!string.IsNullOrEmpty(PartySizeThreeSpawnRoundInterval.text)) ? int.Parse(PartySizeThreeSpawnRoundInterval.text) : 0);
		int item4 = ((!string.IsNullOrEmpty(PartySizeFourSpawnRoundInterval.text)) ? int.Parse(PartySizeFourSpawnRoundInterval.text) : 0);
		m_spawner.SpawnerData.SpawnRoundInterval = new List<int> { item, item2, item3, item4 };
		m_spawner.SetSpawnType(m_TypesToAllow[SpawnAllegienceDropdown.value]);
	}

	private void OnEnable()
	{
		entriesPanel.gameObject.SetActive(value: true);
	}

	private void OnDisable()
	{
		entriesPanel.gameObject.SetActive(value: false);
	}
}
