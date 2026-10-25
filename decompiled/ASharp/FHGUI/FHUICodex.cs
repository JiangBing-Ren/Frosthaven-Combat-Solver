using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using I2.Loc;
using Libs.Utils;
using ScenarioRuleLibrary;
using SharedLibrary.Achievments;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodex : Singleton<FHUICodex>
{
	public class Topic
	{
		public CodexConfig.CodexEntry Data;

		public FHTabButton Button;

		public List<string> LocalizedKeywords;

		public Topic(CodexConfig.CodexEntry data, FHTabButton button)
		{
			Data = data;
			Button = button;
			UpdateLocalization();
		}

		public void UpdateLocalization()
		{
			int capacity = 1 + (Data.keywords?.Count ?? 0);
			LocalizedKeywords = new List<string>(capacity);
			LocalizedKeywords.Add(GLOOM.LocalizationManager.GetTranslation(Data.LocalizationKey));
			if (Data.keywords == null)
			{
				return;
			}
			foreach (string keyword in Data.keywords)
			{
				if (keyword.IsNOTNullOrEmpty())
				{
					LocalizedKeywords.Add(GLOOM.LocalizationManager.GetTranslation(keyword));
				}
			}
		}

		public bool IsUnlocked()
		{
			if (string.IsNullOrEmpty(Data.unlockTag))
			{
				return true;
			}
			return GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(Data.unlockTag);
		}

		public bool IsSeen()
		{
			if (Data.unlockTag.IsNullOrEmpty())
			{
				return true;
			}
			if (GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(Data.unlockTag))
			{
				return GlobalStatRepository.Instance.SeenCodexTags.HasValue(Data.unlockTag);
			}
			return false;
		}

		public void MarkAsSeen()
		{
			if (!IsSeen())
			{
				GlobalStatRepository.Instance.SeenCodexTags.NewValue(Data.unlockTag);
			}
		}

		public bool MatchFilters(string category, List<string> searchFilters)
		{
			if (searchFilters == null || searchFilters.Count == 0)
			{
				return Data.Category == category;
			}
			foreach (string searchFilter in searchFilters)
			{
				foreach (string localizedKeyword in LocalizedKeywords)
				{
					if (localizedKeyword.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[Header("Components")]
	[SerializeField]
	private UIWindow m_Window;

	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[SerializeField]
	private TMP_InputField m_SearchInputField;

	[SerializeField]
	private FHTabsGroup m_CategoriesContainer;

	[SerializeField]
	private GameObject m_CategoriesDummyTarget;

	[SerializeField]
	private FHTabsGroup m_ArticlesContainer;

	[SerializeField]
	private GameObject m_ActicleDummyTarget;

	[SerializeField]
	private ScrollRect m_ArticlesScrollRect;

	[SerializeField]
	private Transform m_ContentContainer;

	[SerializeField]
	private FHButton m_CloseButton;

	[SerializeField]
	private RectTransform m_PanelContainer;

	[SerializeField]
	private TextMeshProUGUI m_TopicsCount;

	[SerializeField]
	private RectTransform m_AbilityCardsTooltipTarget;

	[Header("Prototypes")]
	[SerializeField]
	private FHTabButton m_ArticleButtonPrototype;

	[SerializeField]
	private FHTabButton m_CategoryButtonPrototype;

	[Header("Config")]
	[SerializeField]
	private CodexConfig m_Config;

	[SerializeField]
	private bool m_PreserveSelectionOnReopen;

	[SerializeField]
	private Vector3 m_OptionMenuOffset;

	private List<Topic> m_Topics;

	private List<FHTabButton> m_CategoryButtons;

	private List<FHTabButton> m_ArticleButtons;

	private string m_ActiveTopic;

	private string m_ActiveCategory;

	private List<string> m_ActiveFilters = new List<string>();

	private GameObject currentContentInstance;

	private bool m_Invalidate;

	private bool m_ResetSearchInputField;

	private bool m_Initialized;

	private InteractabilityIsolatedUIControl m_interactabilityControl;

	private Dictionary<int, GameObject> m_CodexEntriesPool = new Dictionary<int, GameObject>();

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_SearchInputField.onValueChanged.AddListener(OnFilter);
			if (m_CategoriesContainer != null)
			{
				m_SearchInputField.onEndEdit.AddListener(ResetSearchInputField);
			}
			m_CategoriesContainer.transform.DestroyChildren();
			if (m_ArticlesContainer != null)
			{
				m_ArticlesContainer.transform.DestroyChildren();
			}
			if (m_ContentContainer != null)
			{
				m_ContentContainer.DestroyChildren();
			}
			if (m_CloseButton != null)
			{
				m_CloseButton.onClick.AddListener(Hide);
			}
			PopulateCategories();
			PopulateArticles();
			m_interactabilityControl = GetComponent<InteractabilityIsolatedUIControl>();
			if (m_interactabilityControl != null)
			{
				m_interactabilityControl.Initialise();
			}
			bool keepMonsterCardsLoaded = ObjectPool.KeepMonsterCardsLoaded;
			ObjectPool.KeepMonsterCardsLoaded = true;
			StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
			ObjectPool.KeepMonsterCardsLoaded = keepMonsterCardsLoaded;
		}
	}

	private void Init(string topicId = null)
	{
		InitCache();
		SelectCategory(GetDefaultCategory());
		SelectTopic(topicId, focus: true);
	}

	private void PopulateCategories()
	{
		if (!(m_Config == null) && !(m_CategoriesContainer == null) && !(m_CategoryButtonPrototype == null))
		{
			int num = m_Config.Categories.Length;
			m_CategoriesContainer.Tabs = new List<FHTabButton>(num);
			m_CategoriesContainer.Contents = new List<GameObject>(num);
			m_CategoryButtons = new List<FHTabButton>(num);
			UIHelperUtil.UpdateUIList(num, m_CategoryButtons, m_CategoryButtonPrototype, m_CategoriesContainer.transform, setCategoryData);
			if (m_CategoryButtons.Count > 0)
			{
				m_CategoriesContainer.StartTab = m_CategoryButtons[0];
				m_CategoriesContainer.Reinitialize(m_CategoryButtons[0]);
			}
		}
		void setCategoryData(int i, FHTabButton tabButton)
		{
			CodexConfig.CodexCategory data = m_Config.Categories[i];
			tabButton.SetTextKey(data.LocalizationKey);
			Image image = tabButton.transform.Find("CategoryIcon")?.GetComponent<Image>();
			if (image != null)
			{
				image.overrideSprite = data.Icon;
			}
			tabButton.onClick.AddListener(() =>
			{
				SelectCategory(data.ID);
			});
			m_CategoriesContainer.Tabs.Add(tabButton);
			m_CategoriesContainer.Contents.Add(m_CategoriesDummyTarget);
		}
	}

	private void PopulateArticles()
	{
		List<CodexConfig.CodexEntry> allEntries;
		if (!(m_Config == null) && !(m_ArticlesContainer == null) && !(m_ArticleButtonPrototype == null))
		{
			allEntries = m_Config.AllEntries();
			int count = allEntries.Count;
			m_ArticlesContainer.Tabs = new List<FHTabButton>(count);
			m_ArticlesContainer.Contents = new List<GameObject>(count);
			m_Topics = new List<Topic>(count);
			m_ArticleButtons = new List<FHTabButton>(count);
			UIHelperUtil.UpdateUIList(count, m_ArticleButtons, m_ArticleButtonPrototype, m_ArticlesContainer.transform, setEntryData);
			if (m_ArticleButtons.Count > 0)
			{
				m_ArticlesContainer.StartTab = m_ArticleButtons[0];
				m_ArticlesContainer.Reinitialize(m_ArticleButtons[0]);
			}
		}
		void setEntryData(int i, FHTabButton tabButton)
		{
			CodexConfig.CodexEntry data = allEntries[i];
			CodexConfig.CodexCategory codexCategory = m_Config.Categories.FirstOrDefault((CodexConfig.CodexCategory codexCategory2) => codexCategory2.ID == data.Category);
			m_ArticlesContainer.Tabs.Add(tabButton);
			tabButton.name = data.ID;
			tabButton.SetTextKey(data.LocalizationKey);
			Image image = tabButton.transform.Find("CategoryIcon")?.GetComponent<Image>();
			if (image != null)
			{
				image.overrideSprite = codexCategory?.Icon;
			}
			Topic topic = new Topic(data, tabButton);
			m_Topics.Add(topic);
			tabButton.onClick.AddListener(() =>
			{
				SelectTopic(topic.Data.ID);
			});
			m_ArticlesContainer.Contents.Add(m_CategoriesDummyTarget);
		}
	}

	private void SelectCategory(string category)
	{
		m_ActiveCategory = category;
		for (int i = 0; i < m_CategoryButtons.Count; i++)
		{
			CodexConfig.CodexCategory codexCategory = m_Config.Categories[i];
			m_CategoryButtons[i].IsFocused = codexCategory.ID == category;
		}
		m_Invalidate = true;
	}

	private void SelectTopic(string topicId, bool focus = false)
	{
		if (m_ActiveTopic != null && m_ActiveTopic == topicId)
		{
			return;
		}
		if (m_PreserveSelectionOnReopen && topicId == null)
		{
			topicId = m_ActiveTopic;
		}
		Topic topic = GetTopic(topicId) ?? GetDefaultTopic();
		if (topic == null)
		{
			return;
		}
		if (topic.Data.Category != m_ActiveCategory)
		{
			SelectCategory(topic.Data.Category);
		}
		m_ActiveTopic = topic.Data.ID;
		m_ArticlesContainer.SwitchToTab(topic.Button);
		if (focus)
		{
			m_ArticlesScrollRect.ScrollVerticallyTo(topic.Button.transform as RectTransform);
		}
		if (m_ContentContainer != null)
		{
			if ((bool)currentContentInstance)
			{
				int valueOrDefault = (currentContentInstance?.GetComponent<FHUICodexEntry>()?.Data?.EntryContent?.GetInstanceID()).GetValueOrDefault();
				if (!ReturnEntryToPool(valueOrDefault, currentContentInstance))
				{
					UnityEngine.Object.Destroy(currentContentInstance);
				}
			}
			currentContentInstance = GetEntryFromPool(topic.Data.EntryContent.GetInstanceID());
			if (currentContentInstance == null)
			{
				currentContentInstance = UnityEngine.Object.Instantiate(topic.Data.EntryContent, m_ContentContainer);
			}
			currentContentInstance.SetActive(value: true);
			FHUICodexEntry component = currentContentInstance.GetComponent<FHUICodexEntry>();
			if (component is FHUICodexHeroEntry fHUICodexHeroEntry)
			{
				fHUICodexHeroEntry.SetTooltipContainer(m_AbilityCardsTooltipTarget);
			}
			component?.Setup(topic.Data);
			topic.MarkAsSeen();
		}
		m_Invalidate = true;
	}

	private GameObject GetEntryFromPool(int instanceID)
	{
		if (m_CodexEntriesPool.TryGetValue(instanceID, out var value))
		{
			m_CodexEntriesPool[instanceID] = null;
			return value;
		}
		return null;
	}

	private bool ReturnEntryToPool(int instanceID, GameObject inst)
	{
		if (instanceID == 0 || !inst)
		{
			return false;
		}
		if (!m_CodexEntriesPool.Keys.Contains(instanceID))
		{
			m_CodexEntriesPool.Add(instanceID, inst);
			inst.gameObject.SetActive(value: false);
			return true;
		}
		if ((bool)m_CodexEntriesPool[instanceID])
		{
			UnityEngine.Object.Destroy(inst);
			return false;
		}
		m_CodexEntriesPool[instanceID] = inst;
		inst.SetActive(value: false);
		return true;
	}

	private void Update()
	{
		if (m_Invalidate)
		{
			Refresh();
			m_Invalidate = false;
		}
	}

	private void LateUpdate()
	{
		if (m_ResetSearchInputField)
		{
			m_ResetSearchInputField = false;
			if (EventSystem.current.currentSelectedGameObject == m_SearchInputField.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	protected virtual void OnEnable()
	{
		InitCache();
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		Clear();
		if (!GloomUtility.IsAppQuitting)
		{
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
		}
	}

	private void Refresh()
	{
		bool flag = !m_ActiveFilters.IsNullOrEmpty();
		foreach (Topic topic in m_Topics)
		{
			bool flag2 = topic.MatchFilters(m_ActiveCategory, m_ActiveFilters);
			bool flag3 = !topic.IsUnlocked();
			flag2 &= !(flag3 & flag);
			topic.Button.interactable = flag2 && !flag3;
			if (!flag3)
			{
				topic.Button.SetTextKey(topic.Data.LocalizationKey);
			}
			else
			{
				topic.Button.SetTextKey("GUI_UNKNOWN");
			}
			topic.Button.gameObject.SetActive(flag2);
			GameObject gameObject = topic.Button.transform.Find("NewEntryIndicator").gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(!flag3 && !topic.IsSeen());
			}
		}
		int num = m_Topics.Count((Topic entry) => entry.Data.Category == m_ActiveCategory);
		int num2 = m_Topics.Count((Topic entry) => entry.Data.Category == m_ActiveCategory && entry.IsSeen());
		for (int num3 = 0; num3 < m_CategoryButtons.Count; num3++)
		{
			CodexConfig.CodexCategory config = m_Config.Categories[num3];
			m_CategoryButtons[num3].interactable = m_Topics.Count((Topic entry) => entry.Data.Category == config.ID) > 0;
		}
		if ((bool)m_TopicsCount)
		{
			m_TopicsCount.text = $"{num2}/{num}";
		}
		if (m_interactabilityControl != null)
		{
			m_interactabilityControl.Initialise();
		}
	}

	private void Clear()
	{
		m_SearchInputField.SetTextWithoutNotify(null);
		m_ActiveFilters.Clear();
		m_ActiveTopic = null;
		m_ActiveCategory = null;
	}

	public Topic GetTopic(string id)
	{
		return m_Topics.FirstOrDefault((Topic topic) => topic.Data.ID == id);
	}

	private Topic GetDefaultTopic()
	{
		return m_Topics.FirstOrDefault((Topic topic) => topic.Data.Category == m_ActiveCategory);
	}

	private string GetDefaultCategory()
	{
		CodexConfig.CodexCategory[] categories = m_Config.Categories;
		foreach (CodexConfig.CodexCategory codexCategory in categories)
		{
			if (m_Config.Entries.Count((CodexConfig.CodexEntry entry) => entry.Category == codexCategory.ID) > 0)
			{
				return codexCategory.ID;
			}
		}
		return null;
	}

	private void OnFilter(string filter)
	{
		m_ActiveFilters.Clear();
		if (filter.IsNOTNullOrEmpty())
		{
			m_ActiveFilters.AddRange(filter.Split(' '));
		}
		m_Invalidate = true;
	}

	private void ResetSearchInputField(string _)
	{
		if (EventSystem.current.currentSelectedGameObject == m_SearchInputField.gameObject)
		{
			m_ResetSearchInputField = true;
		}
	}

	private void OnLanguageChanged()
	{
		foreach (Topic topic in m_Topics)
		{
			topic.UpdateLocalization();
		}
	}

	public void Toggle(bool fromOptionMenu = false)
	{
		if (IsVisible())
		{
			Hide();
		}
		else
		{
			Show(GetFirstUnseenTopicId(), fromOptionMenu);
		}
	}

	public void Show(string topicId = null, bool fromOptionMenu = false, bool preserveOffset = false)
	{
		m_Window.Show();
		if (!preserveOffset)
		{
			m_PanelContainer.localPosition = (fromOptionMenu ? m_OptionMenuOffset : Vector3.zero);
		}
		Init(topicId);
	}

	public void Hide()
	{
		m_Window.Hide();
	}

	public string GetFirstUnseenTopicId()
	{
		foreach (CodexConfig.CodexEntry item in m_Config.AllEntries())
		{
			if (!string.IsNullOrEmpty(item.unlockTag) && GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(item.unlockTag) && !GlobalStatRepository.Instance.SeenCodexTags.HasValue(item.unlockTag))
			{
				return item.ID;
			}
		}
		return null;
	}

	public bool HasUnseenTopics()
	{
		ListAchievementStat unlockedTags = GlobalStatRepository.Instance.UnlockedCodexTags;
		ListAchievementStat seenTags = GlobalStatRepository.Instance.SeenCodexTags;
		return m_Config.AllEntries().Any(IsUnseen);
		bool IsUnseen(CodexConfig.CodexEntry entry)
		{
			if (!string.IsNullOrEmpty(entry.unlockTag) && unlockedTags.HasValue(entry.unlockTag))
			{
				return !seenTags.HasValue(entry.unlockTag);
			}
			return false;
		}
	}

	public bool IsVisible()
	{
		return m_Window.IsVisible;
	}

	public static bool UnlockCodexTag(CActor actor)
	{
		return UnlockCodexTag(FHUICodexUtils.CodexActorIDToUnlockTag(actor.Class.ID));
	}

	public static bool UnlockCodexTag(BuildingTypeID buildingID)
	{
		return UnlockCodexTag(FHUICodexUtils.CodexBuildingIDToUnlockTag(buildingID));
	}

	public static bool UnlockCodexTag(ItemID itemID)
	{
		return UnlockCodexTag(FHUICodexUtils.CodexItemIDToUnlockTag(itemID));
	}

	public static bool UnlockCodexTag(string tagId)
	{
		if (GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(tagId))
		{
			return false;
		}
		GlobalStatRepository.Instance.UnlockedCodexTags.NewValue(tagId);
		return true;
	}

	public static bool MarkCodexTagAsSeen(string tagId)
	{
		if (GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(tagId))
		{
			return false;
		}
		GlobalStatRepository.Instance.UnlockedCodexTags.NewValue(tagId);
		return true;
	}
}
