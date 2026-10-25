using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class AlchemistModule : FHMapViewModule
{
	public enum State
	{
		None,
		Brew,
		Research,
		Distill
	}

	public struct Params
	{
		public BrewableRecipeDTO Recipe;

		public ItemDataDTO Potion;

		public IEnumerable<OutpostHerbType> InputOptions;

		public IEnumerable<OutpostHerbType> OutputOptions;

		public OutpostHerbType SelectedDistillOption;

		public Action<ItemID, OutpostHerbType> OnDistillOptionSelected;

		public Action<Params> OnCraftCallback;

		public Action<ItemID, OutpostHerbType> OnOptionDeselectedCallback;

		public Action<int> OnHerbPicker;

		public Action<int, OutpostHerbType> OnBrewHerbPicked;

		public OutpostResources AvailableResources;

		public Func<Params, IEnumerable<string>> CraftHint;

		public int PickingIndex;

		public bool BasicBrewEnabled;

		public bool AdvancedBrewEnabled;

		public bool IsValidCraft;

		public bool IsBasicBrew
		{
			get
			{
				if (Recipe != null && Recipe.Hidden)
				{
					return !Recipe.Is3HerbRecipe;
				}
				return false;
			}
		}

		public bool IsAdvancedBrew
		{
			get
			{
				if (Recipe != null && Recipe.Hidden)
				{
					return Recipe.Is3HerbRecipe;
				}
				return false;
			}
		}

		public bool IsBasicResearch
		{
			get
			{
				if (Recipe != null && !Recipe.Hidden)
				{
					return !Recipe.Is3HerbRecipe;
				}
				return false;
			}
		}

		public bool IsAdvancedResearch
		{
			get
			{
				if (Recipe != null && !Recipe.Hidden)
				{
					return Recipe.Is3HerbRecipe;
				}
				return false;
			}
		}

		public bool IsBasicDistill
		{
			get
			{
				if (Potion != null)
				{
					return OutputOptions.Count() == 2;
				}
				return false;
			}
		}

		public bool IsAdvancedDistill
		{
			get
			{
				if (Potion != null)
				{
					return OutputOptions.Count() == 3;
				}
				return false;
			}
		}

		public bool IsPoison
		{
			get
			{
				if (Recipe != null)
				{
					return Recipe.IsPoison;
				}
				return false;
			}
		}
	}

	[Header("Components")]
	[SerializeField]
	private UIHerbPicker m_HerbPicker;

	[SerializeField]
	private FHButton m_CraftButton;

	[SerializeField]
	private TMP_Text m_CraftButtonLabel;

	[SerializeField]
	private UIOutpostItemElement m_PotionIcon;

	[SerializeField]
	private Image m_PotionIconImage;

	[SerializeField]
	private Image m_ResearchIcon;

	[SerializeField]
	private UIOutpostCraftingIngredientElement[] InputHerbSlots;

	[SerializeField]
	private UIOutpostCraftingIngredientElement[] OutputHerbSlots;

	[SerializeField]
	private GameObject m_BrewHighlights_Advanced;

	[SerializeField]
	private GameObject m_DistillHighlights_Advanced;

	[SerializeField]
	private GameObject m_BrewHighlights_Basic;

	[SerializeField]
	private GameObject m_DistillHighlights_Basic;

	[SerializeField]
	private TMP_Text m_PotionName;

	[SerializeField]
	private TMP_Text m_StateDescription;

	[SerializeField]
	private TMP_Text m_CraftHints;

	[SerializeField]
	private GameObject m_ResearchHighlight;

	[SerializeField]
	private GameObject m_BrewHighlight;

	[SerializeField]
	private GameObject m_PoisonHighlight;

	[SerializeField]
	private GameObject m_HintUnhealthyMixture;

	[SerializeField]
	private GameObject m_HintDeadlyMixture;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_IconResearchBasicHerbs;

	[SerializeField]
	private Sprite m_IconResearchAdvancedHerbs;

	[SerializeField]
	private Sprite m_IconLeftMouseButton;

	[SerializeField]
	private Sprite m_IconRightMouseButton;

	[Header("Settings")]
	[SerializeField]
	private float m_ActiveCanvasAlpha = 1f;

	[SerializeField]
	private float m_InactiveCanvasAlpha = 0.2f;

	[SerializeField]
	private Material m_GrayScaleMaterial;

	[Header("Tooltips")]
	[SerializeField]
	private UITextTooltipTarget m_ResearchIconTooltip;

	[SerializeField]
	private RectTransform _tooltipPos;

	[SerializeField]
	private TextAnchor _tooltipAnchor;

	private UIItemElementInteractionEvents m_PotionInteractions = new UIItemElementInteractionEvents();

	private bool m_Initialized;

	public Params Data { get; set; }

	public State CurrentState
	{
		get
		{
			if (Data.Recipe != null && Data.Recipe.Hidden)
			{
				return State.Research;
			}
			if (Data.Recipe != null)
			{
				return State.Brew;
			}
			if (Data.Potion != null)
			{
				return State.Distill;
			}
			return State.None;
		}
	}

	private void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		if ((bool)m_CraftButton)
		{
			m_CraftButton.onClick.AddListener(HandleOnCraftRequest);
		}
		m_PotionInteractions.OnHover = HandleOnPotionHover;
		if ((bool)m_ResearchIconTooltip)
		{
			string text = FHUILocFormatting.FormatSimpleTooltip("GUI_ALCHEMIST_UNKNOWN_RECIPE", "GUI_ALCHEMIST_UNKNOWN_RECIPE_DESCRIPTION");
			m_ResearchIconTooltip.SetText(text);
		}
		for (int i = 0; i < InputHerbSlots.Length; i++)
		{
			UIOutpostCraftingIngredientElement uIOutpostCraftingIngredientElement = InputHerbSlots[i];
			if (!(uIOutpostCraftingIngredientElement == null))
			{
				uIOutpostCraftingIngredientElement.OnStateChanged = (Action<UIOutpostCraftingIngredientElement>)Delegate.Combine(uIOutpostCraftingIngredientElement.OnStateChanged, new Action<UIOutpostCraftingIngredientElement>(HandleOnHerbSlotHighlighted));
				uIOutpostCraftingIngredientElement.GetComponent<FHInputEventContainer>()?.SetHintsVisible(visible: false);
			}
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Clear();
		Data = @params;
		UpdateItemIcon();
		PopulateBrewHerbs(Data.InputOptions, InputHerbSlots, CurrentState != State.Distill);
		PopulateDistillHerbs(Data.OutputOptions, OutputHerbSlots, CurrentState == State.Distill, Data.SelectedDistillOption);
		UpdateCraftButton();
		UpdateStateDescription();
		UpdateHints();
		UpdateVisualHighlights();
		UpdatePicker();
		UpdateResultIconTooltip();
	}

	private void UpdateItemIcon()
	{
		State currentState = CurrentState;
		bool flag = Data.IsPoison || currentState == State.Brew || currentState == State.Distill;
		if (m_PotionIcon != null)
		{
			m_PotionIcon.gameObject.SetActive(flag);
			if (flag)
			{
				ItemDataDTO itemDataDTO = Data.Recipe?.Potion ?? Data.Potion;
				if (itemDataDTO != null)
				{
					m_PotionIcon.SetItem(itemDataDTO, m_PotionInteractions, new FHItemTooltip.ItemTooltipParams(itemDataDTO), new TooltipPositionParams
					{
						TransformAnchor = _tooltipPos,
						Anchor = _tooltipAnchor
					});
				}
				else
				{
					m_PotionIcon.SetEmpty();
				}
			}
		}
		if (m_PotionIconImage != null)
		{
			bool flag2 = (currentState == State.Distill && !Data.Potion.CanBeDistilled) || (currentState == State.Brew && !Data.IsValidCraft);
			m_PotionIconImage.material = (flag2 ? m_GrayScaleMaterial : null);
		}
		if (m_ResearchIcon != null)
		{
			bool flag3 = !Data.IsPoison && currentState == State.Research;
			m_ResearchIcon.transform.parent.gameObject.SetActive(flag3);
			if (flag3)
			{
				m_ResearchIcon.sprite = (Data.Recipe.Is3HerbRecipe ? m_IconResearchAdvancedHerbs : m_IconResearchBasicHerbs);
			}
		}
		if ((bool)m_PotionName)
		{
			m_PotionName.gameObject.SetActive(flag);
			if (flag)
			{
				m_PotionName.text = Data.Recipe?.NameLocalized ?? Data.Potion?.NameLocalized ?? string.Empty;
			}
		}
	}

	private void PopulateDistillHerbs(IEnumerable<OutpostHerbType> options, UIOutpostCraftingIngredientElement[] slots, bool isActiveGroup, OutpostHerbType selectedDistillOption = OutpostHerbType.None)
	{
		List<OutpostHerbType> list = options?.ToList();
		State currentState = CurrentState;
		for (int i = 0; i < slots.Length; i++)
		{
			UIOutpostCraftingIngredientElement uIOutpostCraftingIngredientElement = slots[i];
			OutpostHerbType outpostHerbType = ((list != null && list.Count > i) ? list[i] : OutpostHerbType.None);
			UIOutpostCraftingIngredientElement.Params obj = default;
			bool flag = outpostHerbType != OutpostHerbType.None;
			obj.Index = i;
			if (outpostHerbType != OutpostHerbType.None)
			{
				if (selectedDistillOption == outpostHerbType)
				{
					uIOutpostCraftingIngredientElement.Select();
				}
				else
				{
					uIOutpostCraftingIngredientElement.Deselect();
				}
				obj.Available = selectedDistillOption == OutpostHerbType.None || outpostHerbType == selectedDistillOption;
				obj.Icon = UIInfoTools.Instance.GetResourceConfig(outpostHerbType.ToString()).Icon;
				obj.Type = outpostHerbType;
				obj.NameLocalized = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(outpostHerbType));
				obj.Interactable = flag;
				obj.OnDeselect = HandleOnDistillOptionDeselect;
			}
			else
			{
				uIOutpostCraftingIngredientElement.Deselect();
			}
			uIOutpostCraftingIngredientElement.Init(obj);
			FHInputEventContainer component = uIOutpostCraftingIngredientElement.GetComponent<FHInputEventContainer>();
			if ((bool)component)
			{
				component.Init(GetIngredientSlotInteractions(obj, currentState, flag));
			}
			CanvasGroup component2 = uIOutpostCraftingIngredientElement.GetComponent<CanvasGroup>();
			if ((bool)component2)
			{
				component2.alpha = ((isActiveGroup && outpostHerbType != OutpostHerbType.None) ? m_ActiveCanvasAlpha : m_InactiveCanvasAlpha);
			}
		}
	}

	private void PopulateBrewHerbs(IEnumerable<OutpostHerbType> options, UIOutpostCraftingIngredientElement[] slots, bool isActiveGroup)
	{
		List<OutpostHerbType> list = options?.ToList();
		State currentState = CurrentState;
		for (int i = 0; i < slots.Length; i++)
		{
			UIOutpostCraftingIngredientElement obj = slots[i];
			OutpostHerbType outpostHerbType = ((list != null && list.Count > i) ? list[i] : OutpostHerbType.None);
			bool flag = isActiveGroup && (i < 2 || Data.AdvancedBrewEnabled);
			UIOutpostCraftingIngredientElement.Params obj2 = new UIOutpostCraftingIngredientElement.Params
			{
				Index = i,
				Available = true
			};
			if (outpostHerbType != OutpostHerbType.None)
			{
				obj2.Available = Data.AvailableResources.HasHerb(outpostHerbType);
				obj2.Icon = UIInfoTools.Instance.GetResourceConfig(outpostHerbType.ToString()).Icon;
				obj2.Type = outpostHerbType;
				obj2.Interactable = flag;
				obj2.NameLocalized = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(outpostHerbType));
			}
			obj.Init(obj2);
			FHInputEventContainer component = obj.GetComponent<FHInputEventContainer>();
			if ((bool)component)
			{
				component.Init(GetIngredientSlotInteractions(obj2, currentState, flag));
			}
			CanvasGroup component2 = obj.GetComponent<CanvasGroup>();
			if ((bool)component2)
			{
				component2.alpha = (flag ? m_ActiveCanvasAlpha : m_InactiveCanvasAlpha);
			}
		}
	}

	private void UpdateCraftButton()
	{
		if (m_CraftButton != null)
		{
			m_CraftButton.interactable = Data.IsValidCraft;
		}
		if (m_CraftButtonLabel != null)
		{
			string term = CurrentState switch
			{
				State.None => "GUI_ALCHEMIST_BREW", 
				State.Brew => "GUI_ALCHEMIST_BREW", 
				State.Research => "GUI_ALCHEMIST_RESEARCH", 
				State.Distill => "GUI_ALCHEMIST_DISTILL", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			m_CraftButtonLabel.text = LocalizationManager.GetTranslation(term);
		}
	}

	private void UpdateHints()
	{
		if ((bool)m_HintUnhealthyMixture)
		{
			m_HintUnhealthyMixture.gameObject.SetActive(Data.BasicBrewEnabled && !Data.AdvancedBrewEnabled);
		}
		if ((bool)m_HintDeadlyMixture)
		{
			m_HintDeadlyMixture.gameObject.SetActive(Data.AdvancedBrewEnabled);
		}
		if (m_CraftHints == null)
		{
			return;
		}
		bool flag = false;
		string text = string.Empty;
		foreach (string item in Data.CraftHint(Data))
		{
			flag = true;
			text = text + item + "\n";
		}
		m_CraftHints.enabled = flag;
		if (flag)
		{
			m_CraftHints.text = text;
		}
	}

	private void UpdateStateDescription()
	{
		if (!(m_StateDescription == null))
		{
			string term = CurrentState switch
			{
				State.None => "GUI_ALCHEMIST_BREW_DESCRIPTION", 
				State.Brew => "GUI_ALCHEMIST_BREW_DESCRIPTION", 
				State.Research => "GUI_ALCHEMIST_BREW_DESCRIPTION", 
				State.Distill => "ALCHEMIST_DISTILL_DESCRIPTION", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			m_StateDescription.text = LocalizationManager.GetTranslation(term);
		}
	}

	private void UpdatePicker()
	{
		if ((object)m_HerbPicker == null)
		{
			return;
		}
		if (Data.PickingIndex == 0)
		{
			m_HerbPicker.Hide();
			UIOutpostCraftingIngredientElement[] inputHerbSlots = InputHerbSlots;
			for (int i = 0; i < inputHerbSlots.Length; i++)
			{
				inputHerbSlots[i].Deselect();
			}
			return;
		}
		int slotIndex = Data.PickingIndex - 1;
		UIOutpostCraftingIngredientElement uIOutpostCraftingIngredientElement = InputHerbSlots[slotIndex];
		for (int j = 0; j < InputHerbSlots.Length; j++)
		{
			UIOutpostCraftingIngredientElement uIOutpostCraftingIngredientElement2 = InputHerbSlots[j];
			if (uIOutpostCraftingIngredientElement2 != uIOutpostCraftingIngredientElement && uIOutpostCraftingIngredientElement2.IsSelected)
			{
				uIOutpostCraftingIngredientElement2.Deselect();
			}
		}
		uIOutpostCraftingIngredientElement.Select();
		UIHerbPicker.HerbPickerData data = new UIHerbPicker.HerbPickerData
		{
			Resources = Data.AvailableResources,
			TitleLocKey = "GUI_ALCHEMIST_PICKHERB",
			IsEmptySelectable = true
		};
		m_HerbPicker.Show(data, (OutpostHerbType type) =>
		{
			OnBrewHerbSelected(slotIndex, type);
		});
		UIHelperUtil.TrySetPositionConstraint(m_HerbPicker.gameObject, uIOutpostCraftingIngredientElement.transform);
	}

	private void UpdateVisualHighlights()
	{
		bool isPoison = Data.IsPoison;
		if ((bool)m_BrewHighlights_Basic)
		{
			m_BrewHighlights_Basic.SetActive(Data.IsBasicBrew || Data.IsBasicResearch);
		}
		if ((bool)m_DistillHighlights_Basic)
		{
			m_DistillHighlights_Basic.SetActive(Data.IsBasicDistill);
		}
		if ((bool)m_BrewHighlights_Advanced)
		{
			m_BrewHighlights_Advanced.SetActive(Data.IsAdvancedBrew || Data.IsAdvancedResearch);
		}
		if ((bool)m_DistillHighlights_Advanced)
		{
			m_DistillHighlights_Advanced.SetActive(Data.IsAdvancedDistill);
		}
		if ((bool)m_ResearchHighlight)
		{
			m_ResearchHighlight.SetActive(!isPoison && CurrentState == State.Research);
		}
		if ((bool)m_BrewHighlight)
		{
			m_BrewHighlight.SetActive(!isPoison && CurrentState == State.Brew);
		}
		if ((bool)m_PoisonHighlight)
		{
			m_PoisonHighlight.SetActive(isPoison);
		}
	}

	private void UpdateResultIconTooltip()
	{
	}

	private void HandleOnCraftRequest()
	{
		Data.OnCraftCallback(Data);
	}

	private void HandleOnIngredientSelect(UIOutpostCraftingIngredientElement.Params target)
	{
		Data.OnDistillOptionSelected?.Invoke(Data.Potion.ID, (OutpostHerbType)target.Type);
	}

	private void HandleOnBrewIngredientSlotSelected(UIOutpostCraftingIngredientElement.Params target)
	{
		UIOutpostCraftingIngredientElement uIOutpostCraftingIngredientElement = InputHerbSlots[target.Index];
		Data.OnHerbPicker?.Invoke((!uIOutpostCraftingIngredientElement.IsSelected) ? (target.Index + 1) : 0);
	}

	private void HandleOnBrewIngredientSlotRightClick(UIOutpostCraftingIngredientElement.Params target)
	{
		UIOutpostCraftingIngredientElement obj = InputHerbSlots[target.Index];
		obj.Deselect();
		_ = obj.IsEmpty;
		Data.OnBrewHerbPicked(target.Index, OutpostHerbType.None);
	}

	private void OnBrewHerbSelected(int slotIndex, OutpostHerbType herb)
	{
		InputHerbSlots[slotIndex].Deselect();
		Data.OnBrewHerbPicked(slotIndex, herb);
	}

	private void HandleOnDistillOptionDeselect()
	{
		Data.OnOptionDeselectedCallback?.Invoke(Data.Potion.ID, Data.SelectedDistillOption);
	}

	private void HandleOnHerbSlotHighlighted(UIOutpostCraftingIngredientElement slot)
	{
		slot.GetComponent<FHInputEventContainer>()?.SetHintsVisible(slot.IsHighlighted || slot.IsSelected);
	}

	private void HandleOnPotionHover(UIOutpostItemElement itemElement, bool hover)
	{
		if (hover)
		{
			ShowTooltip(itemElement.ItemData, showActions: false);
		}
		else
		{
			HideTooltip();
		}
	}

	private void HideTooltip(bool immediate = false)
	{
	}

	private void ShowTooltip(ItemDataDTO itemData, bool showActions = true)
	{
	}

	private void Clear()
	{
		HideTooltip(immediate: true);
	}

	private IEnumerable<FHInputEventContainer.Params> GetIngredientSlotInteractions(UIOutpostCraftingIngredientElement.Params slotParams, State state, bool isInteractable)
	{
		if (!isInteractable)
		{
			yield break;
		}
		if (state != State.Distill)
		{
			yield return new FHInputEventContainer.Params
			{
				ActionIcon = m_IconLeftMouseButton,
				HintKey = "GUI_SELECT",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					HandleOnBrewIngredientSlotSelected(slotParams);
				}
			};
		}
		if (state == State.Distill)
		{
			yield return new FHInputEventContainer.Params
			{
				ActionIcon = m_IconLeftMouseButton,
				HintKey = "GUI_SELECT",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					HandleOnIngredientSelect(slotParams);
				}
			};
		}
		if (state != State.Distill && slotParams.Icon != null)
		{
			yield return new FHInputEventContainer.Params
			{
				ActionIcon = m_IconRightMouseButton,
				HintKey = "GUI_LEAVE",
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					HandleOnBrewIngredientSlotRightClick(slotParams);
				}
			};
		}
	}
}
