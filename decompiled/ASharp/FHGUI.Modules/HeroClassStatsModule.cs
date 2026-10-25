using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class HeroClassStatsModule : FHMapViewModule
{
	[Header("Stats")]
	public TMP_Text TextHealth;

	public GameObject ExpContainer;

	public TMP_Text TextExp;

	public Image ExpBar;

	[Header("Main Elements")]
	public Transform ContainerComplexity;

	public Transform ContainerElementalAffinities;

	public Transform ContainerTraits;

	public UIPoligonalChart StatsChart;

	[Header("Interactions")]
	public FHButton ViewCodex;

	[Header("Settings")]
	public Sprite[] complexityBipSprites;

	private Action _onAbilityCardsClick;

	private List<Image> _complexityPool = new List<Image>();

	private List<Image> _affinitiesPool = new List<Image>();

	private List<Image> _traitsPool = new List<Image>();

	private bool _initialized;

	protected override bool PlaySoundOnEnable => false;

	private void InitCache()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_affinitiesPool.Clear();
		if ((bool)ContainerElementalAffinities)
		{
			ContainerElementalAffinities.GetComponentsInChildren(_affinitiesPool);
		}
		_traitsPool.Clear();
		if ((bool)ContainerTraits)
		{
			ContainerTraits.GetComponentsInChildren(_traitsPool);
		}
		_complexityPool.Clear();
		for (int i = 0; i < ContainerComplexity.childCount; i++)
		{
			Image component = ContainerComplexity.GetChild(i).GetComponent<Image>();
			if ((object)component != null)
			{
				_complexityPool.Add(component);
			}
		}
	}

	public void Init(ClassInfoDTO classInfo, CharacterDataDTO characterInfo, string tooltipLocKey, bool isReadOnly, bool showExp)
	{
		InitCache();
		PopulateHealth(classInfo);
		PopulateExp(characterInfo, showExp);
		BuildComplexity(classInfo);
		if ((bool)ContainerElementalAffinities)
		{
			SetIcons(classInfo.Affinities, _affinitiesPool, ContainerElementalAffinities);
		}
		if ((bool)ContainerTraits)
		{
			SetIcons(classInfo.Traits, _traitsPool, ContainerTraits);
		}
		StatsChart.Init(GetPolygonalData(classInfo));
		if (ViewCodex != null)
		{
			ViewCodex.onClick.AddListener(() =>
			{
				Singleton<FHUICodex>.InstanceFast?.Show(classInfo.CodexInfoID);
			});
		}
	}

	private void PopulateHealth(ClassInfoDTO classInfo)
	{
		if (TextHealth != null)
		{
			TextHealth.text = $"<sprite name=Health> {classInfo.Health}";
		}
	}

	private void PopulateExp(CharacterDataDTO classInfo, bool showExp)
	{
		if (ExpContainer != null)
		{
			ExpContainer.gameObject.SetActive(showExp);
		}
		if ((ExpBar != null) & showExp)
		{
			ExpBar.fillAmount = (float)classInfo.Experience.Value / (float)classInfo.Experience.Max;
		}
		if (TextExp != null)
		{
			TextExp.text = (classInfo.IsMaxLevel ? LocalizationManager.GetTranslation("GUI_CHARACTER_MAXLEVEL") : $"{classInfo.Experience.Value}/{classInfo.Experience.Max}");
		}
	}

	private void BuildComplexity(ClassInfoDTO classInfo)
	{
		if ((object)ContainerComplexity == null || _complexityPool == null || _complexityPool.Count < 1)
		{
			return;
		}
		HelperTools.NormalizePool(ref _complexityPool, _complexityPool[0].gameObject, ContainerComplexity, classInfo.MaxComplexity);
		for (int i = 0; i < _complexityPool.Count; i++)
		{
			bool flag = i < classInfo.Complexity;
			Image image = _complexityPool[i].transform.GetChild(0)?.GetComponent<Image>();
			if (!(image == null))
			{
				image.gameObject.SetActive(flag);
				if (flag)
				{
					image.overrideSprite = ((complexityBipSprites != null && complexityBipSprites.Length > i) ? complexityBipSprites[i] : null);
				}
			}
		}
	}

	private List<UIPoligonalChart.PolygonalData> GetPolygonalData(ClassInfoDTO info)
	{
		return new List<UIPoligonalChart.PolygonalData>
		{
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Melee,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Melee_Name")
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Defense,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Defense_Name"),
				LabelOffset = new Vector2(-20f, 15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Control,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Control_Name"),
				LabelOffset = new Vector2(-20f, -15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Support,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Support_Name")
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Mobility,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Mobility_Name"),
				LabelOffset = new Vector2(20f, -15f)
			},
			new UIPoligonalChart.PolygonalData
			{
				Value = info.Ranged,
				MaxValue = 5f,
				Text = LocalizationManager.GetTranslation("GUI_HERO_STAT_Ranged_Name"),
				LabelOffset = new Vector2(20f, 15f)
			}
		};
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
}
