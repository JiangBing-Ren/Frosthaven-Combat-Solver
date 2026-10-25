using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.PersonalQuests;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorObjectiveFilterPanel : MonoBehaviour
{
	[Header("Objective Filter")]
	public GameObject FilterListItemPrefab;

	public GameObject FilterParent;

	public TextMeshProUGUI PanelTitle;

	public LayoutGroup FilterPlayerClassParent;

	public LayoutGroup FilterEnemyClassParent;

	public LayoutGroup FilterSummonClassParent;

	public LayoutGroup FilterActorGUIDClassParent;

	public LayoutGroup FilterPropGUIDClassParent;

	public LayoutGroup FilterItemsParent;

	public LayoutGroup FilterConditionsParent;

	public LayoutGroup FilterActiveBonusParent;

	public LayoutGroup FilterPersonalQuestsParent;

	public Toggle FilterCarryingQuestItemToggle;

	public Toggle FilterLootedGoalChestToggle;

	public Toggle FilterInvertToggle;

	public LevelEditorEnumFlagsDropDown FilterActorTypeFlagDropDown;

	public LevelEditorEnumFlagsDropDown FilterEnemyTypeFlagDropDown;

	public LevelEditorEnumFlagsDropDown FilterLootTypeFlagDropDown;

	public LevelEditorEnumFlagsDropDown FilterRevealedTypeFlagDropDown;

	public TMP_Dropdown FilterPlayerClassesDropDown;

	public TMP_Dropdown FilterEnemyClassesDropDown;

	public TMP_Dropdown FilterSummonClassesDropDown;

	public TMP_Dropdown FilterActorGUIDDropDown;

	public TMP_Dropdown FilterPropGUIDDropDown;

	public TMP_Dropdown FilterItemsDropDown;

	public TMP_Dropdown FilterConditionsDropDown;

	public TMP_Dropdown FilterActiveBonusDropDown;

	public TMP_Dropdown FilterPersonalQuestsDropDown;

	public Button PasteButton;

	private CObjectiveFilter m_FilterCopied;

	private List<LevelEditorClassListItem> m_ObjectiveFilterPlayerClassListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterEnemyClassListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterSummonClassListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterActorGUIDClassListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterPropGUIDClassListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterItemListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterConditionListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterActiveBonusListItems = new List<LevelEditorClassListItem>();

	private List<LevelEditorClassListItem> m_ObjectiveFilterPersonalQuestListItems = new List<LevelEditorClassListItem>();

	private List<CActor> m_ActorOptions = new List<CActor>();

	private List<CObjectProp> m_PropOptions = new List<CObjectProp>();

	private List<string> m_FilterPlayerClassIDs;

	private List<string> m_FilterEnemyClassIDs;

	private List<string> m_FilterHeroSummonClassIDs;

	private List<string> m_FilterItemIDs;

	private List<string> m_FilterAllConditions;

	private List<string> m_FilterActiveBonusTypes;

	private List<string> m_FilterPersonalQuestIDs;

	private bool m_Initialised;

	public CObjectiveFilter FilterBeingShown { get; private set; }

	private void Awake()
	{
		PasteButton.interactable = false;
		m_FilterCopied = null;
		EnsureInitialised();
	}

	private void EnsureInitialised()
	{
		if (!m_Initialised)
		{
			FilterActorTypeFlagDropDown.SetEnumFlagType<CAbilityFilter.EFilterActorType>();
			FilterEnemyTypeFlagDropDown.SetEnumFlagType<CAbilityFilter.EFilterEnemy>();
			FilterLootTypeFlagDropDown.SetEnumFlagType<CAbilityFilter.ELootType>();
			FilterRevealedTypeFlagDropDown.SetEnumFlagType<CAbilityFilter.EFilterRevealedType>();
			m_FilterPlayerClassIDs = CharacterClassManager.Classes.Select((CCharacterClass s) => s.ID).ToList();
			FilterPlayerClassesDropDown.options.Clear();
			FilterPlayerClassesDropDown.AddOptions(m_FilterPlayerClassIDs);
			m_FilterEnemyClassIDs = MonsterClassManager.Classes.Select((CMonsterClass s) => s.ID).Concat(MonsterClassManager.ObjectClasses.Select((CObjectClass s) => s.ID)).ToList();
			FilterEnemyClassesDropDown.options.Clear();
			FilterEnemyClassesDropDown.AddOptions(m_FilterEnemyClassIDs);
			m_FilterHeroSummonClassIDs = ScenarioRuleClient.SRLYML.HeroSummons.Select((HeroSummonYMLData s) => s.ID).ToList();
			FilterSummonClassesDropDown.options.Clear();
			FilterSummonClassesDropDown.AddOptions(m_FilterHeroSummonClassIDs);
			m_FilterItemIDs = ScenarioRuleClient.SRLYML.ItemCards.Select((ItemCardYMLData s) => s.StringID).ToList();
			FilterItemsDropDown.options.Clear();
			FilterItemsDropDown.AddOptions(m_FilterItemIDs);
			m_FilterAllConditions = CCondition.PositiveConditions.Select((CCondition.EPositiveCondition c) => c.ToString()).ToList();
			m_FilterAllConditions.AddRange(CCondition.NegativeConditions.Select((CCondition.ENegativeCondition c) => c.ToString()));
			FilterConditionsDropDown.options.Clear();
			FilterConditionsDropDown.AddOptions(m_FilterAllConditions);
			m_FilterActiveBonusTypes = CAbility.AbilityTypes.Select((CAbility.EAbilityType s) => s.ToString()).ToList();
			FilterActiveBonusDropDown.options.Clear();
			FilterActiveBonusDropDown.AddOptions(m_FilterActiveBonusTypes);
			m_FilterPersonalQuestIDs = MapRuleLibraryClient.MRLYML.PersonalQuests.Select((PersonalQuestYMLData q) => q.ID).ToList();
			FilterPersonalQuestsDropDown.options.Clear();
			FilterPersonalQuestsDropDown.AddOptions(m_FilterPersonalQuestIDs);
			m_Initialised = true;
		}
	}

	public void SetShowing(CObjectiveFilter filter, string titleToUse = "Objective Filter")
	{
		EnsureInitialised();
		FilterBeingShown = filter;
		if (filter == null)
		{
			return;
		}
		PanelTitle.SetText(titleToUse);
		FilterCarryingQuestItemToggle.isOn = FilterBeingShown.FilterCarryingQuestItem;
		FilterLootedGoalChestToggle.isOn = FilterBeingShown.FilterLootedGoalChest;
		FilterInvertToggle.isOn = FilterBeingShown.Invert;
		FilterActorTypeFlagDropDown.SetCurrentValue<CAbilityFilter.EFilterActorType>(FilterBeingShown.FilterActorType.ToString());
		FilterEnemyTypeFlagDropDown.SetCurrentValue<CAbilityFilter.EFilterEnemy>(FilterBeingShown.FilterEnemyType.ToString());
		FilterLootTypeFlagDropDown.SetCurrentValue<CAbilityFilter.ELootType>(FilterBeingShown.FilterLootType.ToString());
		FilterRevealedTypeFlagDropDown.SetCurrentValue<CAbilityFilter.EFilterRevealedType>(FilterBeingShown.FilterRevealedType.ToString());
		ClearClassFilterLists();
		FillActorNameDropDown();
		FillPropGUIDDropDown();
		if (FilterBeingShown.FilterPlayerClassIDs != null)
		{
			foreach (string filterPlayerClassID in FilterBeingShown.FilterPlayerClassIDs)
			{
				FilterPlayerClassesDropDown.SetValueWithoutNotify(m_FilterPlayerClassIDs.IndexOf(filterPlayerClassID));
				OnButtonAddObjectiveFilterPlayerClassPressed();
			}
		}
		FilterPlayerClassesDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterEnemyClassIDs != null)
		{
			foreach (string filterEnemyClassID in FilterBeingShown.FilterEnemyClassIDs)
			{
				FilterEnemyClassesDropDown.SetValueWithoutNotify(m_FilterEnemyClassIDs.IndexOf(filterEnemyClassID));
				OnButtonAddObjectiveFilterEnemyClassPressed();
			}
		}
		FilterEnemyClassesDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterHeroSummonClassIDs != null)
		{
			foreach (string filterHeroSummonClassID in FilterBeingShown.FilterHeroSummonClassIDs)
			{
				FilterSummonClassesDropDown.SetValueWithoutNotify(m_FilterHeroSummonClassIDs.IndexOf(filterHeroSummonClassID));
				OnButtonAddObjectiveFilterSummonClassPressed();
			}
		}
		FilterSummonClassesDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterActorGUIDs != null)
		{
			foreach (string actorGuid in FilterBeingShown.FilterActorGUIDs)
			{
				FilterActorGUIDDropDown.SetValueWithoutNotify(m_ActorOptions.FindIndex((CActor s) => s.ActorGuid == actorGuid) + 1);
				OnButtonAddObjectiveFilterActorGUIDPressed();
			}
		}
		FilterActorGUIDDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterPropGUIDs != null)
		{
			foreach (string propGuid in FilterBeingShown.FilterPropGUIDs)
			{
				FilterPropGUIDDropDown.SetValueWithoutNotify(m_PropOptions.FindIndex((CObjectProp s) => s.PropGuid == propGuid) + 1);
				OnButtonAddObjectiveFilterPropGUIDPressed();
			}
		}
		FilterPropGUIDDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterItemIDs != null)
		{
			foreach (string filterItemID in FilterBeingShown.FilterItemIDs)
			{
				FilterItemsDropDown.SetValueWithoutNotify(m_FilterItemIDs.IndexOf(filterItemID));
				OnButtonAddObjectiveFilterItemPressed();
			}
		}
		FilterItemsDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterNegativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition filterNegativeCondition in FilterBeingShown.FilterNegativeConditions)
			{
				FilterConditionsDropDown.SetValueWithoutNotify(m_FilterAllConditions.IndexOf(filterNegativeCondition.ToString()));
				OnButtonAddObjectiveFilterConditionPressed();
			}
		}
		FilterConditionsDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterPositiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition filterPositiveCondition in FilterBeingShown.FilterPositiveConditions)
			{
				FilterConditionsDropDown.SetValueWithoutNotify(m_FilterAllConditions.IndexOf(filterPositiveCondition.ToString()));
				OnButtonAddObjectiveFilterConditionPressed();
			}
		}
		FilterConditionsDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterActiveBonusTypes != null)
		{
			foreach (string filterActiveBonusType in FilterBeingShown.FilterActiveBonusTypes)
			{
				FilterActiveBonusDropDown.SetValueWithoutNotify(m_FilterActiveBonusTypes.IndexOf(filterActiveBonusType));
				OnButtonAddObjectiveFilterActiveBonusPressed();
			}
		}
		FilterActiveBonusDropDown.SetValueWithoutNotify(0);
		if (FilterBeingShown.FilterPersonalQuestIDs != null)
		{
			foreach (string filterPersonalQuestID in FilterBeingShown.FilterPersonalQuestIDs)
			{
				FilterPersonalQuestsDropDown.SetValueWithoutNotify(m_FilterPersonalQuestIDs.IndexOf(filterPersonalQuestID));
				OnButtonAddObjectiveFilterPersonalQuestPressed();
			}
		}
		FilterPersonalQuestsDropDown.SetValueWithoutNotify(0);
	}

	public void Apply()
	{
		EnsureInitialised();
		CAbilityFilter.EFilterActorType currentFlagEnum = FilterActorTypeFlagDropDown.GetCurrentFlagEnum<CAbilityFilter.EFilterActorType>();
		CAbilityFilter.EFilterEnemy currentFlagEnum2 = FilterEnemyTypeFlagDropDown.GetCurrentFlagEnum<CAbilityFilter.EFilterEnemy>();
		CAbilityFilter.ELootType currentFlagEnum3 = FilterLootTypeFlagDropDown.GetCurrentFlagEnum<CAbilityFilter.ELootType>();
		CAbilityFilter.EFilterRevealedType currentFlagEnum4 = FilterRevealedTypeFlagDropDown.GetCurrentFlagEnum<CAbilityFilter.EFilterRevealedType>();
		CAbility.EAbilityType filterAbilityType = CAbility.EAbilityType.None;
		bool isOn = FilterCarryingQuestItemToggle.isOn;
		bool isOn2 = FilterLootedGoalChestToggle.isOn;
		bool isOn3 = FilterInvertToggle.isOn;
		List<string> list = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterPlayerClassListItem in m_ObjectiveFilterPlayerClassListItems)
		{
			if (!list.Contains(objectiveFilterPlayerClassListItem.ClassName))
			{
				list.Add(objectiveFilterPlayerClassListItem.ClassName);
			}
		}
		List<string> list2 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterEnemyClassListItem in m_ObjectiveFilterEnemyClassListItems)
		{
			if (!list2.Contains(objectiveFilterEnemyClassListItem.ClassName))
			{
				list2.Add(objectiveFilterEnemyClassListItem.ClassName);
			}
		}
		List<string> list3 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterSummonClassListItem in m_ObjectiveFilterSummonClassListItems)
		{
			if (!list3.Contains(objectiveFilterSummonClassListItem.ClassName))
			{
				list3.Add(objectiveFilterSummonClassListItem.ClassName);
			}
		}
		List<string> filterObjectClassIDs = new List<string>();
		List<string> list4 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterActorGUIDClassListItem in m_ObjectiveFilterActorGUIDClassListItems)
		{
			if (!list4.Contains(objectiveFilterActorGUIDClassListItem.ClassName))
			{
				list4.Add(objectiveFilterActorGUIDClassListItem.ClassName);
			}
		}
		List<string> list5 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterPropGUIDClassListItem in m_ObjectiveFilterPropGUIDClassListItems)
		{
			if (!list5.Contains(objectiveFilterPropGUIDClassListItem.ClassName))
			{
				list5.Add(objectiveFilterPropGUIDClassListItem.ClassName);
			}
		}
		List<string> list6 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterItemListItem in m_ObjectiveFilterItemListItems)
		{
			if (!list6.Contains(objectiveFilterItemListItem.ClassName))
			{
				list6.Add(objectiveFilterItemListItem.ClassName);
			}
		}
		List<CCondition.ENegativeCondition> list7 = new List<CCondition.ENegativeCondition>();
		List<CCondition.EPositiveCondition> list8 = new List<CCondition.EPositiveCondition>();
		foreach (LevelEditorClassListItem item in m_ObjectiveFilterConditionListItems)
		{
			CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.FirstOrDefault((CCondition.ENegativeCondition c) => c.ToString() == item.ClassName);
			if (eNegativeCondition != CCondition.ENegativeCondition.NA)
			{
				list7.Add(eNegativeCondition);
				continue;
			}
			CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.FirstOrDefault((CCondition.EPositiveCondition c) => c.ToString() == item.ClassName);
			if (ePositiveCondition != CCondition.EPositiveCondition.NA)
			{
				list8.Add(ePositiveCondition);
			}
		}
		List<string> list9 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterActiveBonusListItem in m_ObjectiveFilterActiveBonusListItems)
		{
			if (!list9.Contains(objectiveFilterActiveBonusListItem.ClassName))
			{
				list9.Add(objectiveFilterActiveBonusListItem.ClassName);
			}
		}
		List<string> list10 = new List<string>();
		foreach (LevelEditorClassListItem objectiveFilterPersonalQuestListItem in m_ObjectiveFilterPersonalQuestListItems)
		{
			if (!list10.Contains(objectiveFilterPersonalQuestListItem.ClassName))
			{
				list10.Add(objectiveFilterPersonalQuestListItem.ClassName);
			}
		}
		FilterBeingShown = new CObjectiveFilter(currentFlagEnum, currentFlagEnum2, currentFlagEnum3, filterAbilityType, currentFlagEnum4, list, list2, list3, filterObjectClassIDs, list4, list5, list6, list7, list8, list9, list10, isOn, isOn2, isOn3);
	}

	private void FillActorNameDropDown()
	{
		FilterActorGUIDDropDown.options.Clear();
		m_ActorOptions.Clear();
		m_ActorOptions.AddRange(ScenarioManager.Scenario.PlayerActors);
		m_ActorOptions.AddRange(ScenarioManager.Scenario.AllMonsters);
		List<string> list = new List<string> { "NONE" };
		list.AddRange(m_ActorOptions.Select((CActor a) => LocalizationManager.GetTranslation(a.Class.LocKey) + " - [" + a.ActorGuid + "]"));
		FilterActorGUIDDropDown.AddOptions(list);
		FilterActorGUIDDropDown.value = 0;
	}

	private void FillPropGUIDDropDown()
	{
		FilterPropGUIDDropDown.options.Clear();
		m_PropOptions.Clear();
		m_PropOptions.AddRange(ScenarioManager.CurrentScenarioState.Props);
		List<string> list = new List<string> { "NONE" };
		list.AddRange(m_PropOptions.Select((CObjectProp a) => a.PropType.ToString() + " - [" + a.PropGuid + "]"));
		FilterPropGUIDDropDown.AddOptions(list);
		FilterPropGUIDDropDown.value = 0;
	}

	private void ClearClassFilterLists()
	{
		for (int num = m_ObjectiveFilterPlayerClassListItems.Count - 1; num >= 0; num--)
		{
			OnRemoveFilterPlayerClassCallback(m_ObjectiveFilterPlayerClassListItems[num]);
		}
		for (int num2 = m_ObjectiveFilterEnemyClassListItems.Count - 1; num2 >= 0; num2--)
		{
			OnRemoveFilterEnemyClassCallback(m_ObjectiveFilterEnemyClassListItems[num2]);
		}
		for (int num3 = m_ObjectiveFilterSummonClassListItems.Count - 1; num3 >= 0; num3--)
		{
			OnRemoveFilterSummonClassCallback(m_ObjectiveFilterSummonClassListItems[num3]);
		}
		for (int num4 = m_ObjectiveFilterActorGUIDClassListItems.Count - 1; num4 >= 0; num4--)
		{
			OnRemoveFilterActorGUIDCallback(m_ObjectiveFilterActorGUIDClassListItems[num4]);
		}
		for (int num5 = m_ObjectiveFilterPropGUIDClassListItems.Count - 1; num5 >= 0; num5--)
		{
			OnRemoveFilterPropGUIDCallback(m_ObjectiveFilterPropGUIDClassListItems[num5]);
		}
		for (int num6 = m_ObjectiveFilterItemListItems.Count - 1; num6 >= 0; num6--)
		{
			OnRemoveFilterItemCallback(m_ObjectiveFilterItemListItems[num6]);
		}
		for (int num7 = m_ObjectiveFilterConditionListItems.Count - 1; num7 >= 0; num7--)
		{
			OnRemoveFilterConditionCallback(m_ObjectiveFilterConditionListItems[num7]);
		}
		for (int num8 = m_ObjectiveFilterActiveBonusListItems.Count - 1; num8 >= 0; num8--)
		{
			OnRemoveFilterActiveBonusCallback(m_ObjectiveFilterActiveBonusListItems[num8]);
		}
		for (int num9 = m_ObjectiveFilterPersonalQuestListItems.Count - 1; num9 >= 0; num9--)
		{
			OnRemoveFilterPersonalQuestCallback(m_ObjectiveFilterPersonalQuestListItems[num9]);
		}
	}

	public void OnButtonAddObjectiveFilterPlayerClassPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterPlayerClassParent.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.ID == FilterPlayerClassesDropDown.options[FilterPlayerClassesDropDown.value].text).ID);
			component.OnRemoveButtonPressedAction = OnRemoveFilterPlayerClassCallback;
			m_ObjectiveFilterPlayerClassListItems.Add(component);
		}
	}

	public void OnButtonAddObjectiveFilterEnemyClassPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterEnemyClassParent.transform);
		if (!(gameObject != null))
		{
			return;
		}
		LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
		string classId = FilterEnemyClassesDropDown.options[FilterEnemyClassesDropDown.value].text;
		string text = MonsterClassManager.Classes.FirstOrDefault((CMonsterClass m) => m.ID == classId)?.ID;
		if (string.IsNullOrEmpty(text))
		{
			text = MonsterClassManager.ObjectClasses.FirstOrDefault((CObjectClass m) => m.ID == classId)?.ID;
		}
		component.Init(text);
		component.OnRemoveButtonPressedAction = OnRemoveFilterEnemyClassCallback;
		m_ObjectiveFilterEnemyClassListItems.Add(component);
	}

	public void OnButtonAddObjectiveFilterSummonClassPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterSummonClassParent.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(ScenarioRuleClient.SRLYML.HeroSummons.FirstOrDefault((HeroSummonYMLData m) => m.ID == FilterSummonClassesDropDown.options[FilterSummonClassesDropDown.value].text).ID);
			component.OnRemoveButtonPressedAction = OnRemoveFilterSummonClassCallback;
			m_ObjectiveFilterSummonClassListItems.Add(component);
		}
	}

	public void OnButtonAddObjectiveFilterActorGUIDPressed()
	{
		if (FilterActorGUIDDropDown.value != 0)
		{
			GameObject gameObject = null;
			gameObject = Object.Instantiate(FilterListItemPrefab, FilterActorGUIDClassParent.transform);
			if (gameObject != null)
			{
				LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
				component.Init(m_ActorOptions[FilterActorGUIDDropDown.value - 1].ActorGuid);
				component.OnRemoveButtonPressedAction = OnRemoveFilterActorGUIDCallback;
				m_ObjectiveFilterActorGUIDClassListItems.Add(component);
			}
		}
	}

	public void OnButtonAddObjectiveFilterPropGUIDPressed()
	{
		if (FilterPropGUIDDropDown.value != 0)
		{
			GameObject gameObject = null;
			gameObject = Object.Instantiate(FilterListItemPrefab, FilterPropGUIDClassParent.transform);
			if (gameObject != null)
			{
				LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
				component.Init(m_PropOptions[FilterPropGUIDDropDown.value - 1].PropGuid);
				component.OnRemoveButtonPressedAction = OnRemoveFilterPropGUIDCallback;
				m_ObjectiveFilterPropGUIDClassListItems.Add(component);
			}
		}
	}

	public void OnButtonAddObjectiveFilterItemPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterItemsParent.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData c) => c.StringID == FilterItemsDropDown.options[FilterItemsDropDown.value].text).StringID);
			component.OnRemoveButtonPressedAction = OnRemoveFilterItemCallback;
			m_ObjectiveFilterItemListItems.Add(component);
		}
	}

	public void OnButtonAddObjectiveFilterConditionPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterConditionsParent.transform);
		if (!(gameObject != null))
		{
			return;
		}
		LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
		string dropDownValue = FilterConditionsDropDown.options[FilterConditionsDropDown.value].text;
		string itemClassName = "NA";
		CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.FirstOrDefault((CCondition.ENegativeCondition c) => c.ToString() == dropDownValue);
		if (eNegativeCondition != CCondition.ENegativeCondition.NA)
		{
			itemClassName = eNegativeCondition.ToString();
		}
		else
		{
			CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.FirstOrDefault((CCondition.EPositiveCondition c) => c.ToString() == dropDownValue);
			if (ePositiveCondition != CCondition.EPositiveCondition.NA)
			{
				itemClassName = ePositiveCondition.ToString();
			}
		}
		component.Init(itemClassName);
		component.OnRemoveButtonPressedAction = OnRemoveFilterConditionCallback;
		m_ObjectiveFilterConditionListItems.Add(component);
	}

	public void OnButtonAddObjectiveFilterActiveBonusPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterActiveBonusParent.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(CAbility.AbilityTypes.FirstOrDefault((CAbility.EAbilityType c) => c.ToString() == FilterActiveBonusDropDown.options[FilterActiveBonusDropDown.value].text).ToString());
			component.OnRemoveButtonPressedAction = OnRemoveFilterActiveBonusCallback;
			m_ObjectiveFilterActiveBonusListItems.Add(component);
		}
	}

	public void OnButtonAddObjectiveFilterPersonalQuestPressed()
	{
		GameObject gameObject = null;
		gameObject = Object.Instantiate(FilterListItemPrefab, FilterPersonalQuestsParent.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(MapRuleLibraryClient.MRLYML.PersonalQuests.FirstOrDefault((PersonalQuestYMLData c) => c.ID == FilterPersonalQuestsDropDown.options[FilterPersonalQuestsDropDown.value].text).ID);
			component.OnRemoveButtonPressedAction = OnRemoveFilterPersonalQuestCallback;
			m_ObjectiveFilterPersonalQuestListItems.Add(component);
		}
	}

	public void OnRemoveFilterPlayerClassCallback(LevelEditorClassListItem playerClassItem)
	{
		m_ObjectiveFilterPlayerClassListItems.Remove(playerClassItem);
		Object.Destroy(playerClassItem.gameObject);
	}

	public void OnRemoveFilterEnemyClassCallback(LevelEditorClassListItem enemyClassItem)
	{
		m_ObjectiveFilterEnemyClassListItems.Remove(enemyClassItem);
		Object.Destroy(enemyClassItem.gameObject);
	}

	public void OnRemoveFilterSummonClassCallback(LevelEditorClassListItem enemyClassItem)
	{
		m_ObjectiveFilterSummonClassListItems.Remove(enemyClassItem);
		Object.Destroy(enemyClassItem.gameObject);
	}

	public void OnRemoveFilterActorGUIDCallback(LevelEditorClassListItem enemyClassItem)
	{
		m_ObjectiveFilterActorGUIDClassListItems.Remove(enemyClassItem);
		Object.Destroy(enemyClassItem.gameObject);
	}

	public void OnRemoveFilterPropGUIDCallback(LevelEditorClassListItem propClassItem)
	{
		m_ObjectiveFilterPropGUIDClassListItems.Remove(propClassItem);
		Object.Destroy(propClassItem.gameObject);
	}

	public void OnRemoveFilterItemCallback(LevelEditorClassListItem itemItem)
	{
		m_ObjectiveFilterItemListItems.Remove(itemItem);
		Object.Destroy(itemItem.gameObject);
	}

	public void OnRemoveFilterConditionCallback(LevelEditorClassListItem conditionItem)
	{
		m_ObjectiveFilterConditionListItems.Remove(conditionItem);
		Object.Destroy(conditionItem.gameObject);
	}

	public void OnRemoveFilterActiveBonusCallback(LevelEditorClassListItem activeBonusItem)
	{
		m_ObjectiveFilterActiveBonusListItems.Remove(activeBonusItem);
		Object.Destroy(activeBonusItem.gameObject);
	}

	public void OnRemoveFilterPersonalQuestCallback(LevelEditorClassListItem personalQuestItem)
	{
		m_ObjectiveFilterPersonalQuestListItems.Remove(personalQuestItem);
		Object.Destroy(personalQuestItem.gameObject);
	}

	public async void OnCopyPressed()
	{
		if (FilterBeingShown != null)
		{
			m_FilterCopied = await FilterBeingShown.DeepCopySerializableObject<CObjectiveFilter>();
		}
		PasteButton.interactable = m_FilterCopied != null;
	}

	public async void OnPastePressed()
	{
		SetShowing(await m_FilterCopied.DeepCopySerializableObject<CObjectiveFilter>());
	}
}
