using System;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardPickerSlot : MonoBehaviour
{
	[SerializeField]
	private RectTransform cardContainer;

	[SerializeField]
	private Button buttonSelect;

	[SerializeField]
	private GUIAnimator selectionAnimator;

	private FHItemTooltip itemCardUI;

	private Action<CItem> onSelected;

	private Action<CItem> onDeselected;

	private CItem item;

	private bool selected;

	public Selectable Selectable => buttonSelect;

	public void Init(CItem item, Action<CItem> onSelected, Action<CItem> onDeselected)
	{
		this.item = item;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		selected = false;
		if (itemCardUI == null)
		{
			Clear();
			GameObject gameObject = ObjectPool.Spawn(UIInfoTools.Instance.ItemTooltip.GameObject, cardContainer, Vector3.zero, Quaternion.identity);
			itemCardUI = gameObject.GetComponent<FHItemTooltip>();
			ItemDTOUtilities.GetFullItemInfo(item, out var itemData, out var _);
			itemCardUI.Show(new FHItemTooltip.ItemTooltipParams
			{
				ItemData = itemData
			});
			LayoutElement component = gameObject.GetComponent<LayoutElement>();
			if (component != null)
			{
				component.ignoreLayout = false;
			}
			UIUtility.ForceRecalclulateLayouts(itemCardUI.gameObject.transform as RectTransform);
		}
	}

	public void ToggleSelectSlot()
	{
		if (buttonSelect.interactable)
		{
			if (!selected)
			{
				selectionAnimator.gameObject.SetActive(value: true);
				selectionAnimator.Play();
				selected = true;
				onSelected?.Invoke(item);
			}
			else
			{
				Deselect();
			}
		}
	}

	public void Deselect()
	{
		selectionAnimator.Stop();
		selectionAnimator.gameObject.SetActive(value: false);
		selected = false;
		onDeselected?.Invoke(item);
	}

	private void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			Clear();
		}
	}

	public void Clear()
	{
		if (itemCardUI != null)
		{
			ObjectPool.Recycle(itemCardUI.gameObject, UIInfoTools.Instance.ItemTooltip.GameObject, "Tooltips Pool");
			itemCardUI = null;
		}
		selectionAnimator.Stop();
		selectionAnimator.gameObject.SetActive(value: false);
		selected = false;
	}
}
