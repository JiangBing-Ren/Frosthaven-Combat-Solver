using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexHeroEntry : FHUICodexEntry
{
	[SerializeField]
	private string HeroId;

	[Header("Hero Entry Components")]
	[SerializeField]
	private Image m_IllustrationNormal;

	[SerializeField]
	private UIAbilityCardContainer m_AbilityCardContainer;

	[SerializeField]
	private FHUICodexTable m_HealthTable;

	[SerializeField]
	protected TextLocalizedListener m_ClassDescription;

	[SerializeField]
	protected TextLocalizedListener m_MechanicsDescription;

	[Header("Stats")]
	public FHStacksCounter m_Complexity;

	public Transform m_ContainerElementalAffinities;

	public Transform m_ContainerTraits;

	public UIPoligonalChart m_StatsChart;

	[Header("Settings")]
	[SerializeField]
	private float m_StatRowHeight = 42f;

	[SerializeField]
	private float m_StatColumnWidth = 64f;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorLevelText;

	[SerializeField]
	private Color m_ColorHealthText;

	private new CodexConfig.CodexEntry Data;

	private CharacterClassDef ClassDef;

	private List<Image> _affinitiesPool = new List<Image>();

	private List<Image> _traitsPool = new List<Image>();

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_affinitiesPool.Clear();
			if ((bool)m_ContainerElementalAffinities)
			{
				m_ContainerElementalAffinities.GetComponentsInChildren(_affinitiesPool);
			}
			_traitsPool.Clear();
			if ((bool)m_ContainerTraits)
			{
				m_ContainerTraits.GetComponentsInChildren(_traitsPool);
			}
		}
	}

	public override void Setup(CodexConfig.CodexEntry entry)
	{
		InitCache();
		base.Setup(entry);
		ClassDef = YMLDefRepository.Repo?.DefRepository?.CharacterClasses.Values.FirstOrDefault((CharacterClassDef def) => def.ID.ID == HeroId);
		m_Caption.SetTextKey(entry.LocalizationKey);
		m_ClassDescription.SetTextKey(ClassDef?.Description.RemoveChar('$'));
		m_MechanicsDescription.SetTextKey("CODEX_HeroInfo_" + ClassDef.ID.ID.Replace("ID", ""));
		m_IllustrationNormal.sprite = UIInfoTools.Instance.GetCharacterConfigUI(HeroId).InfoIllustration;
		Rebuild();
	}

	private void Rebuild()
	{
		PopulateDeck();
		PopulateStats();
	}

	private void PopulateStats()
	{
		m_HealthTable.Init(FHUICodexUtils.PopulateCharacterHealthData(ClassDef, m_StatRowHeight, m_StatColumnWidth, m_ColorLevelText, m_ColorHealthText));
		m_StatsChart.Init(GetPolygonalData(ClassDef));
		m_Complexity.Init((int)ClassDef.Complexity, 5);
		List<SpriteWithTooltipDTO> iconDTOs = ClassDef.Affinities.Select((CardEnhancementEffectType ceet) =>
		{
			ElementInfusionBoardManager.EElement element = CrossReference.CardEnhancementTypeToEElement(ceet);
			return new SpriteWithTooltipDTO
			{
				Title = LocalizationManager.GetTranslation("ENHANCEMENT_" + element),
				Sprite = UIInfoTools.Instance.GetElementConfig(element).selectedIcon
			};
		}).ToList();
		SetIcons(iconDTOs, _affinitiesPool, m_ContainerElementalAffinities);
		List<SpriteWithTooltipDTO> iconDTOs2 = ClassDef.Traits.Select((CharacterTraitType traitType) => new SpriteWithTooltipDTO
		{
			Title = LocalizationManager.GetTranslation("TRAIT_" + traitType),
			Sprite = UIInfoTools.Instance.GetCharacterTraitIcon(traitType),
			TextValue = LocalizationManager.GetTranslation("TRAIT_" + traitType)
		}).ToList();
		SetIcons(iconDTOs2, _traitsPool, m_ContainerTraits);
	}

	private void PopulateDeck()
	{
		if (ClassDef == null)
		{
			return;
		}
		List<CAbilityCard> cards = (from def in ClassDef.GetAllCards(YMLDefRepository.Repo?.DefRepository)
			select CharacterClassManager.AllAbilityCardsUnmodifiedForUI.First((CAbilityCard cCard) => cCard.ID == def.ID.ID) into card
			orderby card.ID
			select card).ToList();
		try
		{
			m_AbilityCardContainer.Init(cards, null);
		}
		catch
		{
		}
	}

	public void SetTooltipContainer(RectTransform rectTransform)
	{
		m_AbilityCardContainer?.SetTooltipContainer(rectTransform);
	}

	private void OnDisable()
	{
		m_AbilityCardContainer.Clear();
	}

	private void SetIcons(IReadOnlyList<SpriteWithTooltipDTO> iconDTOs, List<Image> iconsPool, Transform container)
	{
		HelperTools.NormalizePool(ref iconsPool, iconsPool[0].gameObject, container, iconDTOs.Count);
		for (int i = 0; i < iconsPool.Count; i++)
		{
			SpriteWithTooltipDTO spriteWithTooltipDTO = ((iconDTOs.Count > i) ? iconDTOs[i] : null);
			Image image = iconsPool[i];
			image.gameObject.SetActive(spriteWithTooltipDTO != null);
			if (spriteWithTooltipDTO != null)
			{
				image.sprite = iconDTOs[i].Sprite;
				if (image.TryGetComponent<UITextTooltipTarget>(out var component))
				{
					component.SetText(iconDTOs[i].Title, refreshTooltip: false, iconDTOs[i].Description);
				}
				TMP_Text componentInChildren = image.GetComponentInChildren<TMP_Text>();
				if (componentInChildren != null)
				{
					componentInChildren.text = iconDTOs[i].TextValue;
				}
			}
		}
	}

	private List<UIPoligonalChart.PolygonalData> GetPolygonalData(CharacterClassDef def)
	{
		return new List<UIPoligonalChart.PolygonalData>
		{
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Melee,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Melee_Name")
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Defense,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Defense_Name"),
				LabelOffset = new Vector2(-15f, 15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Control,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Control_Name"),
				LabelOffset = new Vector2(-15f, -15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Support,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Support_Name")
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Mobility,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Mobility_Name"),
				LabelOffset = new Vector2(14f, -15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = def.Ranged,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Ranged_Name"),
				LabelOffset = new Vector2(14f, 15f)
			}
		};
	}
}
