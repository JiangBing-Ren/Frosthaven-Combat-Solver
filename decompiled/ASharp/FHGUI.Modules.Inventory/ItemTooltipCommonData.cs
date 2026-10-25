using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace FHGUI.Modules.Inventory;

public class ItemTooltipCommonData : MonoBehaviour
{
	[Serializable]
	public struct UIItemSideData
	{
		[SerializeField]
		public TextLocalizedListener Description;

		[SerializeField]
		public RectTransform ItemCardLayoutContainer;

		[SerializeField]
		public RectTransform EffectContainer;

		[SerializeField]
		public ItemEffectUI EffectPrefab;
	}

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private GameObject _priceContainer;

	[SerializeField]
	private TMP_Text _price;

	[Header("Item Side - Front")]
	[SerializeField]
	private UIItemSideData _itemSideFront;

	[Header("Item Side - Back (can be empty)")]
	[SerializeField]
	private UIItemSideData _itemSideBack;

	[SerializeField]
	private RectTransform _hintsContainer;

	[SerializeField]
	private RectTransform _genericHintContainer;

	[SerializeField]
	private RectTransform _actionHintsContainer;

	[SerializeField]
	private TextLocalizedListener _hintPrototype;

	[SerializeField]
	private TextLocalizedListener _actionHintPrototype;

	[SerializeField]
	private RectTransform _actionContainer;

	[SerializeField]
	private FHButton _buttonActionPrefab;

	private List<FHButton> _actionButtons = new List<FHButton>(5);

	private List<TextLocalizedListener> _genericHints = new List<TextLocalizedListener>(5);

	private List<TextLocalizedListener> _actionHints = new List<TextLocalizedListener>(5);

	private FHItemTooltip.ItemTooltipAction[] _buttonActions;

	private List<ItemEffectUI> _effectsListFront;

	private List<ItemEffectUI> _effectsListBack;

	public GameObject GameObject => gameObject;

	private void Awake()
	{
		_effectsListFront = _itemSideFront.EffectContainer.GetComponentsInChildren<ItemEffectUI>().ToList();
		if (_itemSideBack.EffectContainer != null)
		{
			_effectsListBack = _itemSideBack.EffectContainer.GetComponentsInChildren<ItemEffectUI>().ToList();
		}
		_actionButtons = new List<FHButton>(_actionContainer.GetComponentsInChildren<FHButton>());
		_genericHints = new List<TextLocalizedListener>(_genericHintContainer.GetComponentsInChildren<TextLocalizedListener>());
		_actionHints = new List<TextLocalizedListener>(_actionHintsContainer.GetComponentsInChildren<TextLocalizedListener>());
	}

	public void ShowImmediate(FHItemTooltip.ItemTooltipParams itemTooltipParams)
	{
		gameObject.SetActive(value: true);
		SetData_Internal(itemTooltipParams);
	}

	public IEnumerator Show(FHItemTooltip.ItemTooltipParams itemTooltipParams)
	{
		gameObject.SetActive(value: true);
		yield return null;
		SetData_Internal(itemTooltipParams);
	}

	private void SetData_Internal(FHItemTooltip.ItemTooltipParams itemTooltipParams)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
		ItemDataDTO itemData = itemTooltipParams.ItemData;
		ItemDataDTO flipItem = itemTooltipParams.FlipItem;
		FHItemTooltip.ItemTooltipData data = itemTooltipParams.Data;
		_icon.sprite = itemData.Icon;
		_name.SetTextKey(itemData.NameKey);
		InitEffectsAndLayout(_itemSideFront, itemData, _effectsListFront);
		if (flipItem != null && _itemSideBack.EffectContainer != null)
		{
			InitEffectsAndLayout(_itemSideBack, flipItem, _effectsListBack);
		}
		SetCost(data);
		SetupActions(data);
		SetupHints(data);
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
	}

	public void Hide()
	{
		RemoveButtonActions();
		gameObject.SetActive(value: false);
	}

	private void SetCost(FHItemTooltip.ItemTooltipData data)
	{
		bool flag = !string.IsNullOrEmpty(data.Cost.CostLiteral);
		_priceContainer.gameObject.SetActive(flag);
		if (flag)
		{
			_price.text = data.Cost.CostLiteral;
		}
	}

	private void SetupActions(FHItemTooltip.ItemTooltipData data)
	{
		RemoveButtonActions();
		_buttonActions = ExtractActions(data);
		UIHelperUtil.UpdateUIList(_buttonActions?.Length ?? 0, _actionButtons, _buttonActionPrefab, _actionContainer, (int i, FHButton button) =>
		{
			FHItemTooltip.ItemTooltipAction itemTooltipAction = _buttonActions[i];
			UIHelperUtil.SetTextKey(button.Text, itemTooltipAction.Name);
			button.Text.fontStyle = FontStyles.Superscript;
			button.interactable = itemTooltipAction.IsInteractable;
			if (itemTooltipAction.OnClick != null)
			{
				button.onClick.AddListener(itemTooltipAction.OnClick.Invoke);
			}
		});
	}

	private void SetupHints(params FHItemTooltip.ItemTooltipData[] data)
	{
		FHItemTooltip.ItemTooltipHint[] hints = ExtractHints(data);
		int num = hints?.Length ?? 0;
		_hintsContainer.gameObject.SetActive(num > 0);
		FHItemTooltip.ItemTooltipHint[] genericHints = hints.Where((FHItemTooltip.ItemTooltipHint hint) => !hint.IsAction).ToArray();
		FHItemTooltip.ItemTooltipHint[] actionHints = hints.Where((FHItemTooltip.ItemTooltipHint hint) => hint.IsAction).ToArray();
		_genericHintContainer.gameObject.SetActive(genericHints.Length != 0);
		UIHelperUtil.UpdateUIList(genericHints.Length, _genericHints, _hintPrototype, _genericHintContainer, (int i, TextLocalizedListener hintText) =>
		{
			if (hints != null)
			{
				FHItemTooltip.ItemTooltipHint itemTooltipHint = genericHints[i];
				TextMeshProUGUI component = hintText.GetComponent<TextMeshProUGUI>();
				if (component != null)
				{
					component.text = itemTooltipHint.Text;
				}
			}
		});
		_actionHintsContainer.gameObject.SetActive(actionHints.Length != 0);
		UIHelperUtil.UpdateUIList(actionHints.Length, _actionHints, _actionHintPrototype, _actionHintsContainer, (int i, TextLocalizedListener hintText) =>
		{
			if (hints != null)
			{
				FHItemTooltip.ItemTooltipHint itemTooltipHint = actionHints[i];
				TextMeshProUGUI component = hintText.GetComponent<TextMeshProUGUI>();
				if (component != null)
				{
					component.text = itemTooltipHint.Text;
				}
			}
		});
	}

	private FHItemTooltip.ItemTooltipAction[] ExtractActions(FHItemTooltip.ItemTooltipData data)
	{
		List<FHItemTooltip.ItemTooltipAction> list = CollectionPool<List<FHItemTooltip.ItemTooltipAction>, FHItemTooltip.ItemTooltipAction>.Get();
		if (data.Actions != null)
		{
			list.AddRange(data.Actions);
		}
		FHItemTooltip.ItemTooltipAction[] result = list.ToArray();
		CollectionPool<List<FHItemTooltip.ItemTooltipAction>, FHItemTooltip.ItemTooltipAction>.Release(list);
		return result;
	}

	private FHItemTooltip.ItemTooltipHint[] ExtractHints(FHItemTooltip.ItemTooltipData[] data)
	{
		if (data == null)
		{
			return null;
		}
		List<FHItemTooltip.ItemTooltipHint> list = CollectionPool<List<FHItemTooltip.ItemTooltipHint>, FHItemTooltip.ItemTooltipHint>.Get();
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i].Hints != null)
			{
				list.AddRange(data[i].Hints);
			}
		}
		FHItemTooltip.ItemTooltipHint[] result = list.ToArray();
		CollectionPool<List<FHItemTooltip.ItemTooltipHint>, FHItemTooltip.ItemTooltipHint>.Release(list);
		return result;
	}

	private void RemoveButtonActions()
	{
		if (_buttonActions == null)
		{
			return;
		}
		for (int i = 0; i < _buttonActions.Length; i++)
		{
			FHItemTooltip.ItemTooltipAction itemTooltipAction = _buttonActions[i];
			if (itemTooltipAction.OnClick != null)
			{
				_actionButtons[i].onClick.RemoveListener(itemTooltipAction.OnClick.Invoke);
			}
		}
	}

	public void InitEffectsAndLayout(UIItemSideData side, ItemDataDTO itemData, List<ItemEffectUI> effectsList)
	{
		GameObject gameObject = side.EffectContainer.gameObject;
		ItemDataDTO.ItemEffectDTO[] effects = itemData.Effects;
		gameObject.SetActive(effects != null && effects.Length != 0);
		side.Description.SetTextKey(itemData.DescriptionKey);
		UIHelperUtil.UpdateUIList(itemData.Effects?.Length ?? 0, effectsList, side.EffectPrefab, side.EffectContainer, (int i, ItemEffectUI el) =>
		{
			el.GetComponent<ItemEffectUI>().Init(itemData.Effects[i]);
		});
		side.ItemCardLayoutContainer.DestroyChildren();
		if (PersistentData.CreateItemCardLayout(itemData.ScenarioItemID, side.ItemCardLayoutContainer))
		{
			side.ItemCardLayoutContainer.gameObject.SetActive(value: true);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(side.ItemCardLayoutContainer.parent as RectTransform);
	}
}
