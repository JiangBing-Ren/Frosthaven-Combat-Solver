using System;
using FHGUI.DTO;
using FHGUI.Modules.Inventory;
using SETUtil.Extend;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Tooltips;

public class FHItemTooltip : MonoBehaviour, ITooltip<FHItemTooltip.ItemTooltipParams>, IPooleable
{
	public delegate ItemTooltipParams ItemTooltipParamsFunc(ItemDataDTO item);

	public record ItemTooltipParams
	{
		public ItemDataDTO ItemData;

		public ItemDataDTO FlipItem;

		public ItemTooltipData Data;

		public RectTransform AnchorTarget;

		public ItemTooltipParams()
		{
		}

		public ItemTooltipParams(ItemDataDTO item, ItemDataDTO flipItem = null, ItemTooltipData? data = null)
		{
			ItemData = item;
			FlipItem = flipItem;
			Data = data.GetValueOrDefault();
		}
	}

	public struct ItemTooltipData
	{
		public ItemTooltipAction[] Actions;

		public ItemTooltipCost Cost;

		public ItemTooltipHint[] Hints;
	}

	public struct ItemTooltipAction(string name, Action onClick, bool isInteractable)
	{
		public string Name = name;

		public Action OnClick = onClick;

		public bool IsInteractable = isInteractable;
	}

	public struct ItemTooltipHint
	{
		public string Text;

		public bool IsAction;
	}

	public struct ItemTooltipCost
	{
		public string CostLiteral;
	}

	[SerializeField]
	private ItemTooltipCommonData _singleItemTooltip;

	[SerializeField]
	private ItemTooltipCommonData _doubleItemTooltip;

	private Coroutine _showCrt;

	public GameObject GameObject => gameObject;

	private void Awake()
	{
		this.GetOrAdd<LayoutElement>().ignoreLayout = true;
	}

	public void Show(ItemTooltipParams data)
	{
		gameObject.SetActive(value: true);
		if (data.FlipItem != null)
		{
			Cleanup();
			_showCrt = StartCoroutine(_doubleItemTooltip.Show(data));
		}
		else
		{
			Cleanup();
			_showCrt = StartCoroutine(_singleItemTooltip.Show(data));
		}
		UIContextualTutorialModule.OnEvent("ItemShown", data);
	}

	private void Cleanup()
	{
		if (_showCrt != null)
		{
			StopCoroutine(_showCrt);
			_showCrt = null;
		}
		_singleItemTooltip.gameObject.SetActive(value: false);
		_doubleItemTooltip.gameObject.SetActive(value: false);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		Cleanup();
	}

	public void OnReturnedToPool()
	{
		_singleItemTooltip.gameObject.SetActive(value: false);
		_doubleItemTooltip.gameObject.SetActive(value: false);
	}

	public void OnRemovedFromPool()
	{
	}
}
