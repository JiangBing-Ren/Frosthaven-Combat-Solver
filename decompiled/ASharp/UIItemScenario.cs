using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIItemScenario : MonoBehaviour, IGeneralInfoProvider
{
	[Serializable]
	[StateDump(Skip = true)]
	protected class UIItemStateSetting
	{
		public CItem.EItemSlotState state;

		public Sprite[] itemSprites;

		public Sprite extraItemSprite;

		public Material itemImageMaterial;

		[Header("Overlay")]
		public Sprite overlaySprite;

		public Color overlayColor = Color.white;

		[Header("Effect")]
		public GameObject effect;
	}

	[SerializeField]
	private Image ItemImage;

	[SerializeField]
	private Image overlayItemImage;

	[SerializeField]
	private GUIAnimator transitionStateAnimator;

	[SerializeField]
	private Image transitionStateImage;

	[SerializeField]
	protected UIItemStateSetting[] stateSettings;

	[SerializeField]
	protected Transform cardHolder;

	[SerializeField]
	protected Vector2 cardPivot;

	private CItem.EItemSlot slot;

	private CInventory m_Inventory;

	private ItemCardUI m_ItemCardUI;

	private int m_Index;

	private Button button;

	private UseItemService useItemLogic;

	private CItem.EItemSlotState? currentState;

	[NonSerialized]
	private CItem item;

	public CItem Item
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
		}
	}

	public CItem.EItemSlot Slot
	{
		get
		{
			return slot;
		}
		set
		{
			slot = value;
		}
	}

	public Selectable Selectable
	{
		get
		{
			if (!button)
			{
				return button = GetComponent<Button>();
			}
			return button;
		}
	}

	public void SetState(CItem.EItemSlotState itemState)
	{
		if (itemState == CItem.EItemSlotState.TempLocked)
		{
			itemState = CItem.EItemSlotState.Spent;
		}
		if (currentState != itemState)
		{
			UpdateDisplay(itemState, slot, currentState.HasValue && (currentState == CItem.EItemSlotState.Consumed || currentState == CItem.EItemSlotState.Spent));
			currentState = itemState;
		}
	}

	public void SetItem(CActor inventoryOwner, CItem.EItemSlot slot, int index = 0)
	{
		currentState = null;
		useItemLogic = new UseItemService(inventoryOwner);
		Slot = slot;
		m_Inventory = inventoryOwner.Inventory;
		m_Index = index;
		Refresh();
	}

	public void Refresh()
	{
		Item = m_Inventory.GetItem(Slot, m_Index);
		SetState((Item != null) ? Item.SlotState : CItem.EItemSlotState.None);
		if (Item != null)
		{
			if (m_ItemCardUI != null)
			{
				ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			}
			GameObject gameObject = ObjectPool.SpawnCard(Item.YMLData.ID, ObjectPool.ECardType.Item, cardHolder, resetLocalScale: true);
			m_ItemCardUI = gameObject.GetComponent<ItemCardUI>();
			if (m_ItemCardUI == null)
			{
				Log.GUIError("Unable to spawn item card " + Item.YMLData.StringID);
				return;
			}
			m_ItemCardUI.lockUI = false;
			CanvasGroup canvasGroup = m_ItemCardUI.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = m_ItemCardUI.gameObject.AddComponent<CanvasGroup>();
			}
			canvasGroup.enabled = true;
			canvasGroup.blocksRaycasts = false;
			m_ItemCardUI.item = Item;
			RectTransform obj = m_ItemCardUI.transform as RectTransform;
			obj.pivot = cardPivot;
			obj.anchoredPosition = Vector2.zero;
			m_ItemCardUI.Hide();
		}
		else if (m_ItemCardUI != null)
		{
			m_ItemCardUI.Hide();
		}
	}

	private void OnDestroy()
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
		}
	}

	private void OnDisable()
	{
		if (m_ItemCardUI != null && m_ItemCardUI.gameObject.activeSelf)
		{
			m_ItemCardUI.Hide();
		}
		transitionStateAnimator.Stop();
	}

	public void OnPointerEnter()
	{
		if (Item != null)
		{
			m_ItemCardUI?.Show(highlightElement: false);
		}
	}

	public void OnPointerExit()
	{
		m_ItemCardUI?.Hide();
	}

	public void OnPointerDown()
	{
		useItemLogic.UseItem(Item);
	}

	public void RefreshItemState()
	{
		SetState((Item != null) ? Item.SlotState : CItem.EItemSlotState.None);
	}

	public void UpdateDisplay(CItem.EItemSlotState state, CItem.EItemSlot slot, bool animateTransition = false)
	{
		transitionStateAnimator.Stop();
		if (animateTransition)
		{
			transitionStateImage.sprite = ItemImage.sprite;
			transitionStateImage.material = ItemImage.material;
			transitionStateAnimator.Play();
		}
		UIItemStateSetting uIItemStateSetting = stateSettings.First((UIItemStateSetting it) => it.state == state);
		int num = Mathf.Clamp((int)(slot - 1), 0, uIItemStateSetting.itemSprites.Length);
		ItemImage.sprite = ((Item == null) ? uIItemStateSetting.itemSprites[num] : UIInfoTools.Instance.GetItemMiniSprite(Item.YMLData.Art));
		ItemImage.material = uIItemStateSetting.itemImageMaterial;
		overlayItemImage.gameObject.SetActive(uIItemStateSetting.overlaySprite != null);
		overlayItemImage.sprite = uIItemStateSetting.overlaySprite;
		overlayItemImage.color = uIItemStateSetting.overlayColor;
		foreach (UIItemStateSetting item in stateSettings.Where((UIItemStateSetting it) => it.effect != null))
		{
			item.effect.SetActive(value: false);
		}
		if (uIItemStateSetting.effect != null)
		{
			uIItemStateSetting.effect.SetActive(value: true);
		}
		m_ItemCardUI?.UpdateState();
	}

	public List<InfuseElement> UnselectedInfusions()
	{
		return m_ItemCardUI?.UnselectedInfusions();
	}

	public List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElements()
	{
		return m_ItemCardUI?.SelectedAnyInfusionElements();
	}

	public void ResetItemElements()
	{
		if (m_ItemCardUI != null)
		{
			m_ItemCardUI.ResetItemElements();
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (item == null)
		{
			return null;
		}
		return UIGeneralInfoUtils.GetItemInfo(item);
	}
}
