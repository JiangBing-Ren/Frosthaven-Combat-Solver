using System;
using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIAlchemistInventoryElement : UIBehaviour, IGeneralInfoProvider
{
	public class Params
	{
		public BrewableRecipeDTO RecipeDto;

		public OutpostResources AvailableResources;

		public bool CanBeBrewed;

		public bool Owned;

		public bool HasResources;

		public UIAlchemistItemElementInteractionEvents ElementInteractions;

		public Action<Params> OnDeselect;

		public ItemTooltipHintsFunc TooltipHints;

		public ItemTooltipCostFunc TooltipCost;

		public int CloneIndex;

		public TooltipPositionParams PotionTooltipPosParams;
	}

	public delegate FHItemTooltip.ItemTooltipHint[] ItemTooltipHintsFunc(BrewableRecipeDTO item);

	public delegate FHItemTooltip.ItemTooltipCost ItemTooltipCostFunc(BrewableRecipeDTO item);

	[Header("Components")]
	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_IconBackground;

	[SerializeField]
	private TMP_Text m_ItemName;

	[SerializeField]
	private TMP_Text m_Quantity;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image[] m_HerbCostIcons;

	[SerializeField]
	private GameObject m_Owned;

	[SerializeField]
	private FHButton m_Button;

	[Header("Tooltip")]
	[SerializeField]
	private UIPrefabTooltipTarget m_TooltipTarget;

	[SerializeField]
	private GameObject TooltipPrototype;

	[SerializeField]
	private UIItemTooltipTarget Tooltip;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_IconResearchBasicHerbs;

	[SerializeField]
	private Sprite m_IconResearchAdvancedHerbs;

	[SerializeField]
	private Sprite m_BackgroundAvalidable;

	[SerializeField]
	private Sprite m_BackgroundNotAvalidable;

	[SerializeField]
	private Sprite m_BackgroundSelected;

	[SerializeField]
	private Sprite m_BackgroundIconAvalidable;

	[SerializeField]
	private Sprite m_BackgroundIconNotAvalidable;

	[SerializeField]
	private Sprite m_BackgroundIconSelected;

	[Header("Colors")]
	[SerializeField]
	private Color m_CostTintColorAvailable;

	[SerializeField]
	private Color m_CostTintColorNotAvailable;

	[SerializeField]
	private Color m_NameTintColorAvailable;

	[SerializeField]
	private Color m_NameTintColorCannotBuy;

	private bool m_SuppressActionCallbacks;

	private bool m_Initialized;

	public Params Data { get; private set; }

	public bool IsSelected { get; private set; }

	public bool IsHighlighted { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if (m_Button == null)
			{
				m_Button = GetComponent<FHButton>();
			}
			if ((bool)m_Button)
			{
				m_Button.Selected += HandleOnSelect;
				m_Button.Deselected += HandleOnDeselect;
				m_Button.PointerEntered += HandleOnPointerEnter;
				m_Button.PointerExited += HandleOnPointerExit;
			}
		}
	}

	public void Init(Params slotParams)
	{
		InitCache();
		Data = slotParams;
		UpdateHighlights();
		UpdateName();
		UpdateIcon();
		UpdateHerbCost();
		UpdateTags();
		SetupTooltip();
	}

	private void UpdateTags()
	{
		if (m_Owned != null)
		{
			m_Owned.gameObject.SetActive(Data.Owned);
		}
	}

	private void SetupTooltip()
	{
		if (m_TooltipTarget == null)
		{
			return;
		}
		m_TooltipTarget.enabled = false;
		Tooltip.enabled = false;
		if (Data.RecipeDto == null)
		{
			return;
		}
		if (!Data.RecipeDto.Hidden)
		{
			Tooltip.enabled = true;
			FHItemTooltip.ItemTooltipParams data = new FHItemTooltip.ItemTooltipParams(Data.RecipeDto.Potion, null, new FHItemTooltip.ItemTooltipData
			{
				Cost = Data.TooltipCost(Data.RecipeDto),
				Hints = Data.TooltipHints(Data.RecipeDto)
			});
			Tooltip.Initialize(data, Data.PotionTooltipPosParams);
			return;
		}
		m_TooltipTarget.enabled = true;
		string caption = (Data.RecipeDto.Hidden ? (LocalizationManager.GetTranslation("GUI_ALCHEMIST_UNKNOWN_RECIPE") + "\n") : (Data.RecipeDto.NameKey + "\n"));
		string hints = string.Empty;
		FHItemTooltip.ItemTooltipHint[] array = Data.TooltipHints(Data.RecipeDto);
		for (int i = 0; i < array.Length; i++)
		{
			FHItemTooltip.ItemTooltipHint itemTooltipHint = array[i];
			hints = hints + itemTooltipHint.Text + "\n";
		}
		m_TooltipTarget.Initialize(TooltipPrototype, (GameObject go) =>
		{
			UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
			{
				Icon = GetIcon(),
				CaptionLiteral = caption,
				DescriptionLiteral = FHUILocFormatting.FontSize("+2", Data.TooltipCost(Data.RecipeDto).CostLiteral),
				Hints = hints
			};
			go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
		});
	}

	private Sprite GetIcon()
	{
		if (Data.RecipeDto.Hidden)
		{
			if (!Data.RecipeDto.Is3HerbRecipe)
			{
				return m_IconResearchBasicHerbs;
			}
			return m_IconResearchAdvancedHerbs;
		}
		return Data.RecipeDto.Icon;
	}

	private void UpdateHighlights()
	{
		if (m_Background != null)
		{
			m_Background.sprite = GetBackgroundSprite();
		}
		if (m_IconBackground != null)
		{
			m_Background.sprite = GetIconBackgroundSprite();
		}
	}

	private void UpdateName()
	{
		if ((bool)m_ItemName)
		{
			string text = string.Empty;
			if (Data.RecipeDto != null)
			{
				text = (Data.RecipeDto.Hidden ? LocalizationManager.GetTranslation("GUI_ALCHEMIST_UNKNOWN_RECIPE") : Data.RecipeDto.NameLocalized);
			}
			m_ItemName.text = text;
			m_ItemName.color = m_NameTintColorAvailable;
			if (Data.RecipeDto != null)
			{
				m_ItemName.color = (Data.AvailableResources.Has(Data.RecipeDto.HerbResources) ? m_ItemName.color : m_NameTintColorCannotBuy);
			}
		}
	}

	private void UpdateIcon()
	{
		if (m_Icon == null)
		{
			return;
		}
		if (Data.RecipeDto == null)
		{
			m_Icon.enabled = false;
			return;
		}
		if (m_Icon != null)
		{
			m_Icon.sprite = GetIcon();
		}
		if (m_Quantity != null)
		{
			m_Quantity.enabled = Data.RecipeDto.TotalCrafts != -1 && !Data.RecipeDto.Hidden;
			m_Quantity.text = $"{Data.RecipeDto.RemainingCrafts}/{Data.RecipeDto.TotalCrafts}";
		}
	}

	private void UpdateHerbCost()
	{
		for (int i = 0; i < m_HerbCostIcons.Length; i++)
		{
			OutpostHerbType outpostHerbType = Data.RecipeDto?.GetHerbAt(i) ?? OutpostHerbType.None;
			Image image = m_HerbCostIcons[i];
			bool flag = outpostHerbType != OutpostHerbType.None;
			image.gameObject.SetActive(flag);
			if (flag)
			{
				image.sprite = UIInfoTools.Instance.GetResourceConfig(outpostHerbType.ToString()).Icon;
				bool flag2 = Data.AvailableResources.HasHerb(outpostHerbType);
				image.color = (flag2 ? m_CostTintColorAvailable : m_CostTintColorNotAvailable);
			}
		}
	}

	private Sprite GetBackgroundSprite()
	{
		if (Data == null)
		{
			return m_BackgroundAvalidable;
		}
		if (!Data.HasResources)
		{
			return m_BackgroundNotAvalidable;
		}
		if (IsSelected)
		{
			return m_BackgroundSelected;
		}
		return m_BackgroundAvalidable;
	}

	private Sprite GetIconBackgroundSprite()
	{
		if (Data == null)
		{
			return m_BackgroundIconAvalidable;
		}
		if (!Data.HasResources)
		{
			return m_BackgroundIconNotAvalidable;
		}
		if (IsSelected)
		{
			return m_BackgroundIconSelected;
		}
		return m_BackgroundIconAvalidable;
	}

	public void Select()
	{
		if (!IsSelected)
		{
			m_SuppressActionCallbacks = true;
			IsSelected = true;
			if ((bool)m_Button)
			{
				m_Button.OnSelect(null);
			}
			m_SuppressActionCallbacks = false;
			UpdateHighlights();
		}
	}

	public void Deselect()
	{
		if (IsSelected)
		{
			m_SuppressActionCallbacks = true;
			IsSelected = false;
			if ((bool)m_Button)
			{
				m_Button.OnDeselect(null);
			}
			m_SuppressActionCallbacks = false;
			UpdateHighlights();
		}
	}

	public void HandleOnSelect()
	{
		if (!m_SuppressActionCallbacks)
		{
			Data?.ElementInteractions?.OnClick?.Invoke(Data);
		}
		UpdateHighlights();
	}

	public void HandleOnDeselect()
	{
		if (!m_SuppressActionCallbacks)
		{
			Data?.OnDeselect?.Invoke(Data);
		}
		UpdateHighlights();
	}

	public void HandleOnPointerEnter()
	{
		IsHighlighted = true;
		UpdateHighlights();
		if (!m_SuppressActionCallbacks)
		{
			Data?.ElementInteractions?.OnHover?.Invoke(Data, isFlagRaised: true);
		}
	}

	public void HandleOnPointerExit()
	{
		IsHighlighted = false;
		UpdateHighlights();
		if (!m_SuppressActionCallbacks)
		{
			Data?.ElementInteractions?.OnHover?.Invoke(Data, isFlagRaised: false);
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (Data?.RecipeDto?.Potion == null || Data.RecipeDto.Hidden)
		{
			return null;
		}
		return UIGeneralInfoUtils.GetItemInfo(Data.RecipeDto.Potion);
	}

	protected override void OnDestroy()
	{
		if ((bool)m_Button)
		{
			m_Button.Selected -= HandleOnSelect;
			m_Button.Deselected -= HandleOnDeselect;
			m_Button.PointerEntered -= HandleOnPointerEnter;
			m_Button.PointerExited -= HandleOnPointerExit;
		}
		base.OnDestroy();
	}
}
