using System;
using System.Linq;
using DG.Tweening;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyCharacterEquippementSlot : MonoBehaviour
{
	[Serializable]
	private struct SlotConfig
	{
		public CItem.EItemSlot slotType;

		public Sprite availableSprite;

		public Sprite unavailableSprite;
	}

	[SerializeField]
	private Image slotIcon;

	[SerializeField]
	protected TextMeshProUGUI itemName;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image validOwnerIcon;

	[SerializeField]
	private SlotConfig[] slotConfigs;

	[SerializeField]
	private Image focusMask;

	[Header("Hover")]
	[SerializeField]
	private RawImage highlight;

	[SerializeField]
	private Image highlightIcon;

	[SerializeField]
	private float highlightMoveDuration;

	[Header("Locked settings")]
	[SerializeField]
	private Sprite lockedIcon;

	[SerializeField]
	private Color lockedItemNameColor = Color.gray;

	private Color invisibleColor = new Color(0f, 0f, 0f, 0f);

	[Header("Unavailable settings")]
	[SerializeField]
	private Color unavailableItemNameColor = Color.gray;

	[SerializeField]
	private Color unavailableHighlightColor;

	[Header("Available settings")]
	[SerializeField]
	private Color availableItemNameColor;

	[SerializeField]
	private Color availableHighlightColor;

	[Header("Equipped settings")]
	[SerializeField]
	private Color equippedHighlightColor;

	[SerializeField]
	private float equippedMaskPosition;

	[SerializeField]
	private Color equippedColor;

	[SerializeField]
	private GUIAnimator equipAnimator;

	[SerializeField]
	private TextMeshProUGUI itemNameShine;

	private CItem item;

	private CItem.EItemSlot slot;

	private int slotNum;

	private Action<UIPartyCharacterEquippementSlot> onSelected;

	private Action<UIPartyCharacterEquippementSlot, bool> onHovered;

	private Action<UIPartyCharacterEquippementSlot, bool> onHoveredItem;

	private bool isLocked;

	private bool hovered;

	private bool availableItems;

	private Tween highlightAnimation;

	private bool isFocused;

	private string emptySlotName;

	public CItem.EItemSlot Slot => slot;

	public CItem Item => item;

	public int SlotNum => slotNum;

	public bool IsLocked => isLocked;

	private void Awake()
	{
		button.onClick.AddListener(Select);
		button.onMouseEnter.AddListener(OnHovered);
		button.onMouseExit.AddListener(OnUnhovered);
		highlight.material = new Material(highlight.material);
	}

	private void Init(string characterID, CItem.EItemSlot slot, Action<UIPartyCharacterEquippementSlot> onSelected = null, int slotNum = 0, bool isLocked = false, Action<UIPartyCharacterEquippementSlot, bool> onHovered = null, Action<UIPartyCharacterEquippementSlot, bool> onHoveredItem = null, string overrideEmptySlotName = null)
	{
		hovered = false;
		this.slot = slot;
		this.slotNum = slotNum;
		this.onSelected = onSelected;
		this.onHovered = onHovered;
		this.onHoveredItem = onHoveredItem;
		emptySlotName = (overrideEmptySlotName.IsNullOrEmpty() ? string.Format(LocalizationManager.GetTranslation($"GUI_INVENTORY_SLOT_{slot}"), slotNum + 1) : overrideEmptySlotName);
		equipAnimator.Stop(goToEnd: true);
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characterID);
		highlight.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(cCharacterClass.CharacterModel, highlighted: true, cCharacterClass.CharacterYML.CustomCharacterConfig).texture;
		this.isLocked = isLocked;
		button.interactable = !isLocked;
		Focus(focus: true);
	}

	public void InitUnlocked(string characterID, CItem.EItemSlot slot, CItem item = null, bool availableItems = false, Action<UIPartyCharacterEquippementSlot> onSelected = null, Action<UIPartyCharacterEquippementSlot, bool> onHovered = null, Action<UIPartyCharacterEquippementSlot, bool> onHoveredItem = null, int slotNum = 0, string overrideEmptySlotName = null)
	{
		this.availableItems = availableItems;
		Init(characterID, slot, onSelected, slotNum, isLocked: false, onHovered, onHoveredItem, overrideEmptySlotName);
		if (item != null)
		{
			AddItem(item);
		}
		else
		{
			RemoveItem();
		}
	}

	public void InitLocked(string characterID, CItem.EItemSlot slot, int slotNum = 0, Action<UIPartyCharacterEquippementSlot, bool> onHovered = null)
	{
		availableItems = false;
		Init(characterID, slot, onSelected, slotNum, isLocked: true, onHovered);
		RemoveItem();
	}

	public void Select()
	{
		onSelected?.Invoke(this);
	}

	public void ShowTooltip(bool show)
	{
		onHoveredItem?.Invoke(this, show);
	}

	private void OnUnhovered()
	{
		hovered = false;
		focusMask.enabled = !isFocused;
		RefreshHighlight();
		onHovered?.Invoke(this, hovered);
	}

	private void OnHovered()
	{
		hovered = true;
		RefreshHighlight();
		focusMask.enabled = false;
		onHovered?.Invoke(this, hovered);
	}

	public void Focus(bool focus)
	{
		isFocused = focus;
		focusMask.enabled = !isFocused && !hovered;
	}

	private void RefreshHighlight(bool animate = true)
	{
		if (hovered && !isLocked)
		{
			RawImage rawImage = highlight;
			Color color;
			if (item != null)
			{
				color = equippedHighlightColor;
			}
			else
			{
				color = (availableItems ? availableHighlightColor : unavailableHighlightColor);
			}
			rawImage.color = color;
			AnimateHighlight(0f, animate);
			highlightIcon.enabled = true;
		}
		else
		{
			highlightIcon.enabled = false;
			highlight.color = ((item == null) ? invisibleColor : equippedColor);
			AnimateHighlight((item == null) ? 0f : equippedMaskPosition, animate);
		}
	}

	private void AnimateHighlight(float to, bool animate = false)
	{
		CanceAnimation();
		Vector2 tiling = highlight.materialForRendering.GetTextureScale("_Mask");
		if (!animate)
		{
			highlight.materialForRendering.SetTextureScale("_Mask", new Vector2(to, tiling.y));
			return;
		}
		float duration = Math.Abs(to - tiling.x) / equippedMaskPosition * highlightMoveDuration;
		highlightAnimation = DOTween.To((float val) =>
		{
			highlight.materialForRendering.SetTextureScale("_Mask", new Vector2(val, tiling.y));
		}, tiling.x, to, duration).SetTarget(highlight.gameObject).OnComplete(() =>
		{
			highlightAnimation = null;
		});
	}

	private void CanceAnimation()
	{
		if (highlightAnimation != null)
		{
			highlightAnimation.Kill();
		}
		highlightAnimation = null;
	}

	public void AddItem(CItem item, bool animate = false)
	{
		this.item = item;
		slotIcon.sprite = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art);
		TextMeshProUGUI textMeshProUGUI = itemName;
		string text = (itemNameShine.text = LocalizationManager.GetTranslation(item.YMLData.Name));
		textMeshProUGUI.text = text;
		itemName.color = UIInfoTools.Instance.GetItemColor(item.YMLData.Rarity);
		RefreshHighlight(animate);
		equipAnimator.Stop(goToEnd: true);
		if (animate)
		{
			equipAnimator.Play();
		}
		if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			validOwnerIcon.gameObject.SetActive(value: false);
			return;
		}
		validOwnerIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(item.YMLData.ValidEquipCharacterClassIDs[0]);
		validOwnerIcon.gameObject.SetActive(value: true);
	}

	public void RemoveItem()
	{
		item = null;
		itemName.text = emptySlotName;
		validOwnerIcon.gameObject.SetActive(value: false);
		if (isLocked)
		{
			TextMeshProUGUI textMeshProUGUI = itemName;
			textMeshProUGUI.text = textMeshProUGUI.text + " - " + LocalizationManager.GetTranslation("GUI_LOCKED");
			slotIcon.sprite = lockedIcon;
			itemName.color = lockedItemNameColor;
		}
		else
		{
			SlotConfig slotConfig = slotConfigs.FirstOrDefault((SlotConfig it) => it.slotType == slot);
			slotIcon.sprite = (availableItems ? slotConfig.availableSprite : slotConfig.unavailableSprite);
			itemName.color = (availableItems ? availableItemNameColor : unavailableItemNameColor);
		}
		RefreshHighlight(animate: false);
		equipAnimator.Stop(goToEnd: true);
	}

	private void OnDisable()
	{
		CanceAnimation();
		equipAnimator.Stop();
		equipAnimator.GoToFinishState();
	}
}
