using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Tweening;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class ScenarioLauncherModule : FHMapViewModule
{
	[Serializable]
	public class GridSettings
	{
		public Vector2 CellSize;

		public Vector2 Spacing;

		public int ConstraintCount;
	}

	public UIScenarioInfoSummary ScenarioSummary;

	[Header("Enemies")]
	public Transform ContainerMonsterBanners;

	public Transform GroupMonsterBanners;

	public UIOutpostMonsterBanner TemplateMonsterBanner;

	public TextLocalizedListener TextEnemies;

	[Header("Extra info")]
	public GameObject ContainerObjectives;

	public TMP_Text TextObjectives;

	public GameObject ContainerActiveEffects;

	public TMP_Text TextActiveEffects;

	[Header("Rewards")]
	public RectTransform ContainerRewards;

	public RectTransform GroupRewards;

	public Image PrototypeRewards;

	public RectTransform ContainerAdditionalRewards;

	public RectTransform GroupAdditionalRewards;

	[Header("Requirements")]
	public GameObject ContainerRequirementBoat;

	public GameObject ContainerRequirementSled;

	public GameObject ContainerRequirementClimbingGear;

	public TMP_Text TextInfo;

	[Header("Buttons")]
	public ButtonsBarElement ButtonStartScenario;

	[Header("Settings")]
	public int MinVisibleMonsterSlotCount;

	public Color ColorEquipmentAvailable;

	public Color ColorEquipmentMissing;

	public Sprite BossIcon;

	[Header("DebugInfo")]
	public GameObject InfoContianer;

	public Image InfoContianerBanner;

	public Color InfoLabelNormalColor;

	public Color InfoLabelWarrningColor;

	private List<UIOutpostMonsterBanner> _banners = new List<UIOutpostMonsterBanner>();

	private List<Image> _rewards = new List<Image>();

	private List<Image> _additionalRewards = new List<Image>();

	private RectTransform _rectTransform;

	private Vector2 _originalAnchoredPosition;

	private Vector2 _originalOffsetMin;

	private Vector2 _originalOffsetMax;

	private bool _initialized;

	[Header("Rewards Grid Settings")]
	[SerializeField]
	private GridSettings RewardsGridSettingsDefault;

	[SerializeField]
	private GridSettings RewardsGridSettingsCompact;

	protected override bool PlaySoundOnEnable => false;

	private void Awake()
	{
		_rectTransform = transform as RectTransform;
		_originalAnchoredPosition = _rectTransform.anchoredPosition;
		_originalOffsetMin = _rectTransform.offsetMin;
		_originalOffsetMax = _rectTransform.offsetMax;
	}

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_banners.Clear();
			_banners.AddRange(GroupMonsterBanners.GetComponentsInChildren<UIOutpostMonsterBanner>(includeInactive: true));
			_rewards.Clear();
			if (PrototypeRewards != null && GroupRewards != null && PrototypeRewards.transform.parent == GroupRewards.transform)
			{
				_rewards.Add(PrototypeRewards);
			}
			_additionalRewards.Clear();
		}
	}

	public void Init(ScenarioInfoDTO scenarioInfoDto, ScenarioRequirementsDto requirementsDto, ButtonsBarParam startButtonParam, bool hasStartQuestAuthority)
	{
		if (scenarioInfoDto == null)
		{
			gameObject.SetActive(value: false);
			return;
		}
		InitCache();
		gameObject.SetActive(value: true);
		ScenarioSummary.Set(scenarioInfoDto, null);
		bool flag = scenarioInfoDto.HasBoss && !scenarioInfoDto.IsCompleted;
		int num = Mathf.Max(scenarioInfoDto.MonsterDtos.Count + (flag ? 1 : 0), MinVisibleMonsterSlotCount);
		HelperTools.NormalizePool(ref _banners, TemplateMonsterBanner.gameObject, GroupMonsterBanners, num);
		if (flag)
		{
			MonsterDataDto dto = new MonsterDataDto
			{
				Portrait = BossIcon,
				NameLocalised = LocalizationManager.GetTranslation("Boss")
			};
			_banners[0].Set(dto);
		}
		int num2 = (flag ? 1 : 0);
		for (int i = num2; i < num; i++)
		{
			MonsterDataDto dto2 = ((scenarioInfoDto.MonsterDtos.Count > i - num2) ? scenarioInfoDto.MonsterDtos[i - num2] : new MonsterDataDto());
			_banners[i].Set(dto2);
		}
		if ((bool)TextEnemies)
		{
			if (scenarioInfoDto.MonsterDtos.Any((MonsterDataDto x) => x.IsAlly))
			{
				TextEnemies.SetTextKey("GUI_QUESTINFO_ENEMIES_AND_ALLIES");
			}
			else
			{
				TextEnemies.SetTextKey("GUI_QUESTINFO_MONSTERS");
			}
		}
		ContainerMonsterBanners.gameObject.SetActive((scenarioInfoDto.MonsterDtos.Count > 0) | flag);
		if (requirementsDto != null)
		{
			SetGearRequirement(ContainerRequirementBoat, requirementsDto.RequireBoat, requirementsDto.MissingBoat, requirementsDto.MissingBoat ? "GUI_QUEST_MISSING_BOAT" : "GUI_BOAT");
			SetGearRequirement(ContainerRequirementSled, requirementsDto.RequireSled, requirementsDto.MissingSled, requirementsDto.MissingSled ? "GUI_QUEST_MISSING_SLED" : "GUI_SLED");
			SetGearRequirement(ContainerRequirementClimbingGear, requirementsDto.RequireClimbingGear, requirementsDto.MissingClimbingGear, requirementsDto.MissingClimbingGear ? "GUI_QUEST_MISSING_CLIMBING_GEAR" : "GUI_CLIMBING_GEAR");
		}
		else
		{
			SetGearRequirement(ContainerRequirementBoat, isRequired: false, isMissing: false, string.Empty);
			SetGearRequirement(ContainerRequirementSled, isRequired: false, isMissing: false, string.Empty);
			SetGearRequirement(ContainerRequirementClimbingGear, isRequired: false, isMissing: false, string.Empty);
		}
		SetRewards(_rewards, ContainerRewards, GroupRewards, scenarioInfoDto.RewardDtos);
		ApplyGridSettings(GroupRewards.GetComponent<GridLayoutExtended>(), (scenarioInfoDto.RewardDtos.Count >= 10 || (num > 10 && scenarioInfoDto.RewardDtos.Count > 6)) ? RewardsGridSettingsCompact : RewardsGridSettingsDefault);
		SetRewards(_additionalRewards, ContainerAdditionalRewards, GroupAdditionalRewards, scenarioInfoDto.AdditinalRewardDtos);
		SetSection(ContainerObjectives, TextObjectives, scenarioInfoDto.ObjectivesLiteral);
		SetSection(ContainerActiveEffects, TextActiveEffects, scenarioInfoDto.ActiveEffectsLiteral);
		SetHints(scenarioInfoDto, requirementsDto, hasStartQuestAuthority);
		bool flag2 = (startButtonParam.IsValid & hasStartQuestAuthority) && requirementsDto != null && !requirementsDto.HasSomethingMissing;
		ButtonStartScenario.gameObject.SetActive(flag2);
		if (flag2)
		{
			ButtonStartScenario.gameObject.SetActive(startButtonParam.IsValid);
			ButtonStartScenario.SetParam(startButtonParam);
		}
		else
		{
			ButtonStartScenario.ClearParam();
		}
		if (InfoContianer != null)
		{
			InfoContianer.gameObject.SetActive(!string.IsNullOrWhiteSpace(TextInfo.text));
		}
		if (InfoContianerBanner != null)
		{
			InfoContianerBanner.color = ((requirementsDto != null && requirementsDto.HasSomethingMissing) ? InfoLabelWarrningColor : InfoLabelNormalColor);
		}
	}

	private void SetHints(ScenarioInfoDTO scenarioInfoDto, ScenarioRequirementsDto requirementsDto, bool showStartButton)
	{
		string text = string.Empty;
		if (requirementsDto != null && requirementsDto.HasSomethingMissing)
		{
			text += GetWarnings(requirementsDto).First();
		}
		else if (!showStartButton)
		{
			text = text + "\n" + LocalizationManager.GetTranslation("GUI_QUESTINFO_START_LEADERONLY");
		}
		TextInfo.text = text;
	}

	private void SetGearRequirement(GameObject element, bool isRequired, bool isMissing, string tooltipKey)
	{
		if (element == null)
		{
			return;
		}
		element.gameObject.SetActive(isRequired);
		if (isRequired)
		{
			element.GetComponent<UITextTooltipTarget>()?.SetText(LocalizationManager.GetTranslation(tooltipKey));
			Image image = element.FindInChildren("Background")?.GetComponent<Image>();
			if (image != null)
			{
				image.color = (isMissing ? ColorEquipmentMissing : ColorEquipmentAvailable);
			}
		}
	}

	public IEnumerable<string> GetWarnings(ScenarioRequirementsDto requirementsDto)
	{
		if (requirementsDto.InvalidParty)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUESTINFO_INVALID_PARTY_SIZE");
		}
		if (requirementsDto.PartyMemberIsReadyToRetire)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUESTINFO_PARTY_MEMBER_READY_TO_RETIRE");
		}
		if (requirementsDto.CantBeReplayed)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUESTINFO_CANT_BE_REPLAYED");
		}
		if (requirementsDto.MissingPersonalQuest)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_MISSING_PERSONAL_QUEST");
		}
		if (requirementsDto.MissingLurkerCrownShards)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_MISSING_CORAL_SHARDS");
		}
		if (requirementsDto.MissingSticker.IsNOTNullOrEmpty())
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_REQUIRE_STICKER_" + requirementsDto.MissingSticker);
		}
		if (requirementsDto != null && requirementsDto.MissingBoat)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_MISSING_BOAT");
		}
		if (requirementsDto != null && requirementsDto.MissingSled)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_MISSING_SLED");
		}
		if (requirementsDto != null && requirementsDto.MissingClimbingGear)
		{
			yield return LocalizationManager.GetTranslation("GUI_QUEST_MISSING_CLIMBING_GEAR");
		}
	}

	private static void SetSection(GameObject container, TMP_Text tmpText, string textLiteral)
	{
		if (container == null)
		{
			return;
		}
		if (textLiteral.IsNullOrEmpty())
		{
			container.SetActive(value: false);
			return;
		}
		container.SetActive(value: true);
		if ((bool)tmpText)
		{
			tmpText.text = textLiteral;
		}
	}

	private void SetRewards(List<Image> icons, RectTransform container, RectTransform group, List<SpriteWithTooltipDTO> rewardDtos)
	{
		if (container == null || group == null || PrototypeRewards == null)
		{
			return;
		}
		int num = rewardDtos?.Count ?? 0;
		HelperTools.NormalizePool(ref icons, PrototypeRewards.gameObject, group, num);
		for (int i = 0; i < num; i++)
		{
			SpriteWithTooltipDTO spriteWithTooltipDTO = ((rewardDtos.Count > i) ? rewardDtos[i] : null);
			Image image = icons[i];
			image.gameObject.SetActive(spriteWithTooltipDTO != null);
			if (spriteWithTooltipDTO != null)
			{
				image.sprite = rewardDtos[i].Sprite;
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
		container.gameObject.SetActive(num > 0);
	}

	protected override void OnHide()
	{
		base.OnHide();
		SetOffset(Vector2.zero, _originalOffsetMin, _originalOffsetMax);
	}

	public void SetOffset(Vector2 offset, Vector2 offsetMin, Vector2 offsetMax)
	{
		_rectTransform.anchoredPosition = _originalAnchoredPosition + offset;
		_rectTransform.offsetMin = offsetMin;
		_rectTransform.offsetMax = offsetMax;
		UITweenEffects.UpdateInitialAnchoredPosition(_rectTransform);
	}

	private void ApplyGridSettings(GridLayoutExtended grid, GridSettings settings)
	{
		if (!(grid == null) && settings != null)
		{
			grid.cellSize = settings.CellSize;
			grid.spacing = settings.Spacing;
			grid.constraintCount = settings.ConstraintCount;
		}
	}
}
