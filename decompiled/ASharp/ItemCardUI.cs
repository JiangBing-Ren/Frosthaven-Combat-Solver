using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardUI : LocalizedListener, IPooleable
{
	public int CardID;

	public TextMeshProUGUI titleText;

	public GameObject pricePanel;

	public TextMeshProUGUI priceText;

	public GameObject layoutHolder;

	public Image cardBackground;

	public Image cardIcon;

	public Image itemSymbolImage;

	public Image conditionImage;

	public Sprite[] conditionSprites;

	public Image permanentlyConsumedImage;

	public bool lockUI = true;

	public UIItemModifiersTooltip modifiersTooltip;

	public Image validOwnerIcon;

	public Image persistentImage;

	public Image roundImage;

	[NonSerialized]
	public bool m_IsSelected;

	public string prefabPath;

	public CreateLayout layout;

	public CItem item;

	public GameObject modifiersIcon;

	public ItemCardEffects cardEffects;

	[SerializeField]
	public List<InfuseElement> infuseElements = new List<InfuseElement>();

	[StateDump(Skip = true)]
	private CItem.EItemSlotState lastState;

	public string CardName { get; set; }

	public static event EventHandler SelectionChanged;

	protected virtual void OnSelectionChanged()
	{
		if (SelectionChanged != null)
		{
			SelectionChanged(this, EventArgs.Empty);
		}
	}

	private void Start()
	{
		m_IsSelected = false;
		OnLanguageChanged();
		UITextTooltipTarget[] componentsInChildren = GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			if (!(componentsInChildren[i] != null))
			{
				continue;
			}
			TextMeshProUGUI component = componentsInChildren[i].GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				if (TryParseGlossaryTexts(component, out var parsedText))
				{
					componentsInChildren[i].Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
					componentsInChildren[i].SetText(parsedText);
				}
				else
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}
	}

	private bool TryParseGlossaryTexts(TextMeshProUGUI textBox, out string parsedText)
	{
		parsedText = string.Empty;
		string[] separator = new string[2] { "<sprite name=", ">" };
		string[] array = textBox.text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].All(char.IsLetterOrDigit) && !list.Contains(array[i]))
			{
				string text = ((!(array[i] == "Heal") || !textBox.transform.parent.name.StartsWith("Summon")) ? CreateLayout.LocaliseText("$Glossary_" + array[i] + "$", skipWarnings: true) : CreateLayout.LocaliseText("$Glossary_Summon_Health$", skipWarnings: true));
				if (!text.StartsWith("UNDEFINED"))
				{
					parsedText += (parsedText.IsNullOrEmpty() ? text : ("\n\n" + text));
					list.Add(array[i]);
				}
				else
				{
					array[i] = string.Empty;
				}
			}
			else
			{
				array[i] = string.Empty;
			}
		}
		if (parsedText.IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public void CreateCard()
	{
		titleText.text = LocalizationManager.GetTranslation(item.Name);
		itemSymbolImage.sprite = UIInfoTools.Instance.GetItemSlotIcon(item.YMLData.Slot.ToString());
		conditionImage.gameObject.SetActive(item.YMLData.Usage != CItem.EUsageType.Unrestricted && item.YMLData.Usage != CItem.EUsageType.None);
		if (conditionImage.gameObject.activeSelf)
		{
			conditionImage.sprite = GetUsageIcon(item.YMLData.Usage, item.YMLData.FlipItemSide);
		}
		persistentImage.gameObject.SetActive(item.YMLData.ShowPersistentBonusIcon);
		roundImage.gameObject.SetActive(item.YMLData.ShowRoundBonusIcon);
		permanentlyConsumedImage.gameObject.SetActive(item.YMLData.PermanentlyConsumed == true);
		Sprite itemBackgroundSprite = UIInfoTools.Instance.GetItemBackgroundSprite(item.YMLData.Art);
		cardBackground.sprite = ((itemBackgroundSprite != null) ? itemBackgroundSprite : cardBackground.sprite);
		Sprite itemMiniSprite = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art);
		cardIcon.sprite = itemMiniSprite;
		cardIcon.enabled = itemBackgroundSprite == null && itemMiniSprite != null;
		layout.FullLayout.transform.SetParent(layoutHolder.transform);
		(layout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		infuseElements = layout._infuseElements;
		modifiersIcon.SetActive(item.YMLData.Data.AdditionalModifiers.Count > 0);
		if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			validOwnerIcon.gameObject.SetActive(value: false);
			priceText.text = item.YMLData.Cost.ToString();
			pricePanel.gameObject.SetActive(value: true);
		}
		else
		{
			pricePanel.gameObject.SetActive(value: false);
			validOwnerIcon.sprite = UIInfoTools.Instance.GetCharacterAssemblyIcon(item.YMLData.ValidEquipCharacterClassIDs.FirstOrDefault());
			validOwnerIcon.gameObject.SetActive(value: true);
		}
	}

	public void Show(bool highlightElement = true, bool showModifiers = false, bool showModifiersOnLeft = false)
	{
		if (highlightElement)
		{
			UIManager.Instance.HighlightElement(gameObject, lockUI);
		}
		gameObject.SetActive(value: true);
		UpdateState();
		EnableShowModifiers(showModifiers, showModifiersOnLeft);
	}

	private Sprite GetUsageIcon(CItem.EUsageType type, CItem.EFlipItemSide slipSide)
	{
		switch (type)
		{
		case CItem.EUsageType.Spent:
			return conditionSprites[0];
		case CItem.EUsageType.Consumed:
			return conditionSprites[1];
		case CItem.EUsageType.Flip:
			if (slipSide != CItem.EFlipItemSide.Front)
			{
				return conditionSprites[2];
			}
			return conditionSprites[3];
		default:
			return conditionSprites[0];
		}
	}

	public void EnableShowModifiers(bool enable, bool showLeft = false)
	{
		modifiersTooltip.EnableTooltip = enable && item.YMLData.Data.AdditionalModifiers.Count > 0;
		if (modifiersTooltip.EnableTooltip)
		{
			modifiersTooltip.Initialize(item.YMLData.Data.AdditionalModifiers);
			modifiersTooltip.ShowTooltip(showLeft);
		}
		else
		{
			modifiersTooltip.HideTooltip();
		}
	}

	public void Hide()
	{
		UIManager.Instance.UnhighlightElement(gameObject, lockUI);
		transform.localScale = Vector3.one;
		GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		gameObject.SetActive(value: false);
		modifiersTooltip.HideTooltip();
	}

	public void ToggleSelect()
	{
		ToggleSelect(m_IsSelected);
	}

	public void ToggleSelect(bool select)
	{
		m_IsSelected = select;
		OnSelectionChanged();
	}

	public void OnReturnedToPool()
	{
		modifiersTooltip.HideTooltip();
		EnableShowModifiers(enable: false);
		lockUI = true;
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.enabled = false;
		}
		transform.localScale = Vector3.one;
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (infuseElement.IsAnyElement && infuseElement.IsSelected)
			{
				infuseElement.Init(ElementInfusionBoardManager.EElement.Any, infuseElement.IsEnhancement);
			}
		}
		cardEffects.RestoreCard();
		lastState = CItem.EItemSlotState.None;
	}

	public void OnRemovedFromPool()
	{
	}

	public void UpdateState(bool force = false)
	{
		UpdateState(item.SlotState, force);
	}

	public void UpdateState(CItem.EItemSlotState state, bool force = false)
	{
		if ((force || state != lastState) && cardEffects != null)
		{
			switch (state)
			{
			case CItem.EItemSlotState.Consumed:
				cardEffects.ToggleEffect(active: true, ItemCardEffects.FXTask.Consumed);
				break;
			case CItem.EItemSlotState.Spent:
				cardEffects.ToggleEffect(active: true, ItemCardEffects.FXTask.Spent);
				break;
			default:
				if (force || lastState == CItem.EItemSlotState.Consumed || lastState == CItem.EItemSlotState.Spent)
				{
					cardEffects.RestoreCard();
				}
				break;
			}
		}
		lastState = state;
	}

	public List<InfuseElement> UnselectedInfusions()
	{
		List<InfuseElement> list = new List<InfuseElement>();
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (!infuseElement.IsSelected)
			{
				list.Add(infuseElement);
			}
		}
		return list;
	}

	public List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (infuseElement.IsAnyElement)
			{
				list.Add(infuseElement.SelectedElement);
			}
		}
		return list;
	}

	public void ResetItemElements()
	{
		foreach (InfuseElement infuseElement in infuseElements)
		{
			infuseElement.ResetElementToInitial();
		}
	}

	protected override void OnLanguageChanged()
	{
		if (CardName != null)
		{
			titleText.text = LocalizationManager.GetTranslation(CardName);
		}
	}
}
