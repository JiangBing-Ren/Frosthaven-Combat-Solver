using System;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyItemSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private GameObject newTooltip;

	[Header("Item")]
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextMeshProUGUI itemName;

	[SerializeField]
	private TextMeshProUGUI itemNameShine;

	[Header("Owner")]
	[SerializeField]
	private RawImage highlightBackground;

	[SerializeField]
	private GameObject belongsToOther;

	[SerializeField]
	private Image ownerIcon;

	[SerializeField]
	private Image ownerIconShine;

	[SerializeField]
	private Image validOwnerIcon;

	[Header("Price")]
	[SerializeField]
	private GameObject price;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private TextMeshProUGUI warningPriceText;

	[SerializeField]
	private GUIAnimator warningCostAnimation;

	[SerializeField]
	private GameObject priceIcon;

	[Header("Selection")]
	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private GUIAnimator assignAnimation;

	[SerializeField]
	private Color shineColor;

	private CItem item;

	private Action<UIPartyItemSlot> onSelect;

	private Action<UIPartyItemSlot, bool> onDeselect;

	private Action<UIPartyItemSlot, bool> onHovered;

	private CMapCharacter character;

	private CMapCharacter owner;

	private const string EMPTY_PRICE = "-";

	private bool isSelected;

	private bool isInOtherSlot;

	private bool selectable;

	public CMapCharacter Owner => owner;

	public CItem Item => item;

	public bool IsEquippedInOtherSlot
	{
		get
		{
			if (isInOtherSlot)
			{
				return item.IsSlotIndexSet;
			}
			return false;
		}
	}

	private void Awake()
	{
		button.onClick.AddListener(Toggle);
		button.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: false);
		});
		assignAnimation.OnAnimationStarted.AddListener(() =>
		{
			ownerIconShine.color = shineColor;
		});
		warningCostAnimation.OnAnimationStarted.AddListener(() =>
		{
			ownerIconShine.color = Color.red;
		});
	}

	public void SetItem(CMapCharacter character, CItem item, CMapCharacter owner = null, Action<UIPartyItemSlot> onSelected = null, Action<UIPartyItemSlot, bool> onDeselected = null, Action<UIPartyItemSlot, bool> onHovered = null, bool isInOtherSlot = false, bool selectable = true)
	{
		this.character = character;
		onSelect = onSelected;
		onDeselect = onDeselected;
		this.onHovered = onHovered;
		this.item = item;
		this.selectable = selectable;
		itemIcon.sprite = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art);
		TextMeshProUGUI textMeshProUGUI = itemName;
		string text = (itemNameShine.text = LocalizationManager.GetTranslation(item.YMLData.Name));
		textMeshProUGUI.text = text;
		itemName.color = UIInfoTools.Instance.GetItemColor(item.YMLData.Rarity);
		RefreshNewNotification();
		if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			TextMeshProUGUI textMeshProUGUI2 = priceText;
			text = (warningPriceText.text = item.SellPrice.ToString());
			textMeshProUGUI2.text = text;
			priceText.color = UIInfoTools.Instance.goldColor;
			validOwnerIcon.gameObject.SetActive(value: false);
			warningCostAnimation.gameObject.SetActive(value: true);
			priceIcon.SetActive(value: true);
		}
		else
		{
			priceText.text = "-";
			priceText.color = UIInfoTools.DefInstance.greyedOutTextColor;
			priceIcon.SetActive(value: false);
			warningCostAnimation.gameObject.SetActive(value: false);
			validOwnerIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(item.YMLData.ValidEquipCharacterClassIDs[0]);
			validOwnerIcon.gameObject.SetActive(value: true);
		}
		SetSelected(selected: false);
		SetOwner(owner, isInOtherSlot);
		ShowWarningCost(show: false);
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		selectedImage.enabled = selected;
	}

	public void SetOwner(CMapCharacter ownerCharacter, bool isInOtherSlot = false)
	{
		owner = ownerCharacter;
		this.isInOtherSlot = isInOtherSlot;
		if (ownerCharacter != null)
		{
			ECharacter model = ownerCharacter.CharacterYMLData.Model;
			ownerIcon.sprite = UIInfoTools.Instance.GetCharacterSprite(model, highlight: false, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			ownerIconShine.sprite = UIInfoTools.Instance.GetCharacterSprite(model, highlight: true, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			ownerIcon.color = UIInfoTools.Instance.GetCharacterColor(model, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			highlightBackground.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(model, highlighted: true, ownerCharacter.CharacterYMLData.CustomCharacterConfig).texture;
			ownerIcon.enabled = true;
			price.SetActive(owner.CharacterID != character.CharacterID);
			belongsToOther.SetActive((owner.CharacterID != character.CharacterID) | isInOtherSlot);
		}
		else
		{
			highlightBackground.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.CharacterYMLData.Model, highlighted: true, character.CharacterYMLData.CustomCharacterConfig).texture;
			belongsToOther.SetActive(value: false);
			ownerIcon.enabled = false;
			price.SetActive(value: false);
		}
		RefreshMultiplayerInteraction();
	}

	private void OnHovered(bool hovered)
	{
		if (hovered)
		{
			newTooltip.SetActive(value: false);
		}
		onHovered?.Invoke(this, hovered);
	}

	public void RefreshNewNotification()
	{
		newTooltip.SetActive(item.IsNew);
	}

	public void RefreshMultiplayerInteraction()
	{
		bool flag = item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty() || item.YMLData.ValidEquipCharacterClassIDs.Contains(character.CharacterID);
		if (!FFSNetwork.IsOnline)
		{
			button.interactable = selectable & flag;
			return;
		}
		ControllableRegistry.GetControllableID(character);
		CMapCharacter cMapCharacter = owner ?? character;
		button.interactable = (selectable & flag) && cMapCharacter != null && (!AdventureState.MapState.MapParty.SelectedCharacters.Contains(cMapCharacter) || cMapCharacter.IsMapCharacterUnderMyControl) && owner == null;
	}

	public void Toggle()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			onSelect?.Invoke(this);
		}
	}

	private void Deselect()
	{
		onDeselect?.Invoke(this, arg2: true);
	}

	public void AssignOwner(CMapCharacter characterID)
	{
		SetSelected(selected: true);
		SetOwner(characterID);
		assignAnimation.Play();
	}

	public void RemoveEquipped()
	{
		isInOtherSlot = false;
		SetSelected(selected: false);
		assignAnimation.Stop();
	}

	public void ShowWarningCost(bool show)
	{
		if (show && warningCostAnimation.gameObject.activeSelf)
		{
			warningCostAnimation.Play();
		}
		else
		{
			warningCostAnimation.Stop();
		}
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			warningCostAnimation.Stop();
			assignAnimation.Stop();
		}
	}
}
