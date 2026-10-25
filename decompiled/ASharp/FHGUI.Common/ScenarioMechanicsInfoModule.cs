using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Services;
using GLOOM;
using Libs.Utils;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class ScenarioMechanicsInfoModule : MonoBehaviour
{
	public struct Params
	{
		public string LevelID;

		public string ScenarioNameKey;

		public List<CObjective> WinObjectives;

		public List<CObjective> LoseObjectives;

		public List<ScenarioMechanicDTO> ScenarioRules;

		public List<SuggestibleSlotDTO> Challenges;

		public List<SuggestibleSlotDTO> Favors;

		public List<SpriteWithTooltipDTO> Loot;

		public object CustomData;
	}

	[SerializeField]
	private AudioMapViewModuleProfile _audioProfile;

	[Header("Components")]
	[SerializeField]
	private UIAnchorLinkNavbar m_AnchorLinkNavbar;

	[SerializeField]
	private TMP_Text TextScenarioName;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private FHButton m_Close;

	[SerializeField]
	private ScrollRect m_ScrollRect;

	[Header("Quest Goals")]
	[SerializeField]
	private GameObject GroupQuestGoals;

	[SerializeField]
	private RectTransform ContainerQuestGoals;

	[SerializeField]
	private MissionObjectiveUI PrototypeQuestGoals;

	[Header("Quest Modifiers")]
	[SerializeField]
	private GameObject GroupModifiers;

	[SerializeField]
	private RectTransform ContainerModifiers;

	[Header("Quest Effects")]
	[SerializeField]
	private GameObject GroupEffects;

	[SerializeField]
	private RectTransform ContainerEffects;

	[SerializeField]
	private GameObject GenericEffectsPrototype;

	[Header("Challenges")]
	[SerializeField]
	private GameObject GroupChallenges;

	[SerializeField]
	private RectTransform ContainerChallenges;

	[SerializeField]
	private UIScenarioBriefingSuggestibleSlot PrototypeChallenge;

	[Header("Favors")]
	[SerializeField]
	private GameObject GroupFavors;

	[SerializeField]
	private RectTransform ContainerFavors;

	[SerializeField]
	private UIScenarioBriefingSuggestibleSlot PrototypeFavor;

	[Header("Loot")]
	[SerializeField]
	private GameObject GroupRemainingLoot;

	[SerializeField]
	private RectTransform ContainerRemainingLoot;

	[SerializeField]
	private Image PrototypeLootIcon;

	[Header("Settings")]
	[SerializeField]
	private Sprite SpriteQuestGoals;

	[SerializeField]
	private Sprite SpriteSpecialRules;

	[SerializeField]
	private Sprite SpriteQuestEffects;

	[SerializeField]
	private Sprite SpriteFavors;

	[SerializeField]
	private Sprite SpriteChallenges;

	[SerializeField]
	private Sprite SpriteRemainingLoot;

	[SerializeField]
	private Material MaterialLootDepleted;

	public RectTransform AbilityCardsToolipLocations;

	[Header("Data")]
	[SerializeField]
	private ScenarioMechanicsConfig ScenarioMechanicsDB;

	private readonly List<MissionObjectiveUI> m_QuestGoalsPool = new List<MissionObjectiveUI>();

	private readonly List<ScenarioModifierUI> m_ModifiersPool = new List<ScenarioModifierUI>();

	private readonly List<UIScenarioBriefingSuggestibleSlot> m_ChallengesPool = new List<UIScenarioBriefingSuggestibleSlot>();

	private readonly List<UIScenarioBriefingSuggestibleSlot> m_FavorsPool = new List<UIScenarioBriefingSuggestibleSlot>();

	private readonly List<Image> m_LootIconPool = new List<Image>();

	private bool m_Initialized;

	private AudioMapViewModuleProfile AudioProfile
	{
		get
		{
			if (!(_audioProfile != null))
			{
				if (!(UIInfoTools.Instance != null))
				{
					return null;
				}
				return UIInfoTools.Instance.defaultAudioMapViewModuleProfile;
			}
			return _audioProfile;
		}
	}

	public ScenarioMechanicsConfig MechanicsData => ScenarioMechanicsDB;

	public Params Data { get; private set; }

	private void OnEnable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnShow);
		}
	}

	private void OnDisable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnHide);
		}
	}

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			ContainerQuestGoals.GetComponentsInChildren(includeInactive: true, m_QuestGoalsPool);
			ContainerModifiers.GetComponentsInChildren(includeInactive: true, m_ModifiersPool);
			ContainerChallenges.GetComponentsInChildren(includeInactive: true, m_ChallengesPool);
			ContainerFavors.GetComponentsInChildren(includeInactive: true, m_FavorsPool);
			ContainerRemainingLoot.GetComponentsInChildren(includeInactive: true, m_LootIconPool);
			PrototypeLootIcon.gameObject.SetActive(value: false);
			m_Close.onClick.AddListener(Hide);
		}
	}

	private void Awake()
	{
		gameObject.SetActive(value: false);
		InitCache();
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		Show();
		TextScenarioName.text = LocalizationManager.GetTranslation(@params.ScenarioNameKey);
		m_Illustration.sprite = UIInfoTools.Instance.GetNarrativeIllustration(@params.LevelID.RemoveChar('\'') + "_Intro_1", throwError: false);
		PopulateQuestGoals();
		PopulateSpecialRules();
		PopulateChallenges();
		PopulateFavors();
		PopulateLoot();
		UpdateTopicAnchors();
	}

	private void PopulateQuestGoals()
	{
		CObjective[] array = (from objective in Data.WinObjectives.Concat(Data.LoseObjectives)
			where !objective.IsHidden && objective.IsActive
			select objective).ToArray();
		Array.Sort(array, (CObjective x, CObjective y) =>
		{
			int num2 = ObjectivePriority(y).CompareTo(ObjectivePriority(x));
			return (num2 != 0) ? num2 : string.Compare(x.EventIdentifier, y.EventIdentifier, StringComparison.Ordinal);
		});
		Array.Reverse(array);
		GroupQuestGoals.SetActive(array.Length != 0);
		UIHelperUtil.UpdateUIList(array.Length, m_QuestGoalsPool, PrototypeQuestGoals, ContainerQuestGoals);
		for (int num = 0; num < array.Length; num++)
		{
			m_QuestGoalsPool[num].Init(array[num]);
		}
	}

	private int ObjectivePriority(CObjective objective)
	{
		if (objective.IsOptional)
		{
			return 2;
		}
		if (ScenarioManager.CurrentScenarioState.LoseObjectives.Contains(objective))
		{
			return 1;
		}
		return 0;
	}

	private void PopulateSpecialRules()
	{
		ContainerModifiers.DestroyChildren();
		ContainerEffects.DestroyChildren();
		RectTransform rectTransform = null;
		LocalizationParam[] rulesLocalizationParams = ScenarioMechanicViewService.GetRulesLocalizationParams();
		foreach (ScenarioMechanicDTO scenarioRule in Data.ScenarioRules)
		{
			RectTransform parent = null;
			if (scenarioRule.StyleCategory == ScenarioMechanicsConfig.Entry.Category.Rule)
			{
				parent = ContainerModifiers;
			}
			if (scenarioRule.StyleCategory == ScenarioMechanicsConfig.Entry.Category.Effect)
			{
				parent = ContainerEffects;
			}
			GameObject gameObject = scenarioRule.Content;
			if (gameObject == null)
			{
				gameObject = ((scenarioRule.StyleCategory == ScenarioMechanicsConfig.Entry.Category.Effect) ? GenericEffectsPrototype : null);
			}
			if (!(gameObject == null))
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, parent);
				UIScenarioMechanicsEntry component = gameObject2.GetComponent<UIScenarioMechanicsEntry>();
				if (component != null)
				{
					component.Init(!scenarioRule.Seen, Data.CustomData, rulesLocalizationParams, scenarioRule.SimpleContentKey);
				}
				if (!scenarioRule.Seen && (object)rectTransform == null)
				{
					rectTransform = gameObject2.transform as RectTransform;
				}
			}
		}
		GroupModifiers.SetActive(ContainerModifiers.transform.childCount > 0);
		GroupEffects.SetActive(ContainerEffects.transform.childCount > 0);
		if (rectTransform != null)
		{
			m_ScrollRect.verticalNormalizedPosition = 1f;
			float normalizedChildPositionVertical = m_ScrollRect.GetNormalizedChildPositionVertical(rectTransform);
			m_ScrollRect.DOVerticalNormalizedPos(normalizedChildPositionVertical, 0.4f).SetDelay(0.4f);
		}
	}

	private void PopulateChallenges()
	{
		GroupChallenges.SetActive(Data.Challenges.Count > 0);
		UIHelperUtil.UpdateUIList(Data.Challenges.Count, m_ChallengesPool, PrototypeChallenge, ContainerChallenges);
		for (int i = 0; i < Data.Challenges.Count; i++)
		{
			m_ChallengesPool[i].Init(Data.Challenges[i], null);
		}
	}

	private void PopulateFavors()
	{
		GroupFavors.SetActive(Data.Favors.Count > 0);
		UIHelperUtil.UpdateUIList(Data.Favors.Count(), m_FavorsPool, PrototypeFavor, ContainerFavors);
		for (int i = 0; i < Data.Favors.Count; i++)
		{
			m_FavorsPool[i].Init(Data.Favors[i], null);
		}
	}

	private void PopulateLoot()
	{
		GroupRemainingLoot.SetActive(Data.Loot.Count > 0);
		UIHelperUtil.UpdateUIList(Data.Loot.Count, m_LootIconPool, PrototypeLootIcon, ContainerRemainingLoot);
		for (int i = 0; i < Data.Loot.Count; i++)
		{
			SpriteWithTooltipDTO spriteWithTooltipDTO = ((Data.Loot.Count > i) ? Data.Loot[i] : null);
			Image image = m_LootIconPool[i];
			image.gameObject.SetActive(spriteWithTooltipDTO != null);
			if (spriteWithTooltipDTO != null)
			{
				image.sprite = Data.Loot[i].Sprite;
				bool flag = Data.Loot[i].AdditionalData != null && Data.Loot[i].AdditionalData is bool && (bool)Data.Loot[i].AdditionalData;
				image.material = (flag ? MaterialLootDepleted : null);
				if (image.TryGetComponent<UITextTooltipTarget>(out var component))
				{
					component.SetText(spriteWithTooltipDTO.Title, refreshTooltip: false, spriteWithTooltipDTO.Description);
				}
				TMP_Text componentInChildren = image.GetComponentInChildren<TMP_Text>();
				if (componentInChildren != null)
				{
					componentInChildren.text = spriteWithTooltipDTO.TextValue;
				}
			}
		}
	}

	private void UpdateTopicAnchors()
	{
		List<UIAnchorLinkNavbar.Params> list = new List<UIAnchorLinkNavbar.Params>();
		if (GroupQuestGoals.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "Objectives",
				Icon = SpriteQuestGoals,
				Tooltip = LocalizationManager.GetTranslation("GUI_QUEST_GOALS")
			});
		}
		if (GroupRemainingLoot.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "RemainingLoot",
				Icon = SpriteRemainingLoot,
				Tooltip = LocalizationManager.GetTranslation("GUI_LABEL_REMAININGLOOT")
			});
		}
		if (GroupModifiers.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "SpecialRules",
				Icon = SpriteSpecialRules,
				Tooltip = LocalizationManager.GetTranslation("GUI_SPECIAL_RULES")
			});
		}
		if (GroupEffects.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "QuestEffects",
				Icon = SpriteQuestEffects,
				Tooltip = LocalizationManager.GetTranslation("GUI_QUEST_EFFECTS")
			});
		}
		if (GroupChallenges.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "Challenges",
				Icon = SpriteChallenges,
				Tooltip = LocalizationManager.GetTranslation("GUI_CHALLENGES")
			});
		}
		if (GroupFavors.activeInHierarchy)
		{
			list.Add(new UIAnchorLinkNavbar.Params
			{
				LinkID = "Favors",
				Icon = SpriteFavors,
				Tooltip = LocalizationManager.GetTranslation("GUI_FAVORS")
			});
		}
		bool activeInHierarchy = m_ScrollRect.verticalScrollbar.gameObject.activeInHierarchy;
		activeInHierarchy &= GroupModifiers.activeInHierarchy || GroupEffects.activeInHierarchy || GroupChallenges.activeInHierarchy || GroupFavors.activeInHierarchy;
		m_AnchorLinkNavbar.gameObject.SetActive(activeInHierarchy);
		if (activeInHierarchy)
		{
			m_AnchorLinkNavbar.Init(list);
		}
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
