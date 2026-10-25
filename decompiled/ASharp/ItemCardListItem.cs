using System;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardListItem : MonoBehaviour
{
	public TextMeshProUGUI Name;

	public TextMeshProUGUI GoldCost;

	public Image Background;

	public Image SlotIcon;

	public Sprite NormalSprite;

	public Sprite SelectedSprite;

	public Sprite DisabledSprite;

	[SerializeField]
	private ExtendedButton Button;

	[SerializeField]
	private CanvasGroup canvasGroup;

	public CItem item;

	private bool belongsToOther;

	public bool Disabled { get; set; }

	public bool Selected { get; set; }

	public int Index { get; set; }

	public CItem.EItemSlot Slot { get; set; }

	public event EventHandler MouseEnterEvent;

	public event EventHandler MouseExitEvent;

	public event EventHandler ClickEvent;

	protected virtual void OnMouseEnterEvent()
	{
		if (MouseEnterEvent != null)
		{
			MouseEnterEvent(this, EventArgs.Empty);
		}
	}

	protected virtual void OnMouseExitEvent()
	{
		if (MouseExitEvent != null)
		{
			MouseExitEvent(this, EventArgs.Empty);
		}
	}

	public void MouseEnter()
	{
		OnMouseEnterEvent();
	}

	public void MouseExit()
	{
		OnMouseExitEvent();
	}

	protected virtual void OnClickEvent()
	{
		if (ClickEvent != null)
		{
			ClickEvent(this, EventArgs.Empty);
		}
	}

	public void Click()
	{
		if (!Disabled)
		{
			Selected = !Selected;
			OnClickEvent();
		}
		UpdateView();
	}

	public void UpdateView()
	{
		if (belongsToOther)
		{
			SlotIcon.color = Color.white;
			Background.sprite = SelectedSprite;
			return;
		}
		SlotIcon.color = ((!Selected) ? Color.black : Color.white);
		if (Disabled)
		{
			Background.sprite = DisabledSprite;
		}
		else if (Selected)
		{
			Background.sprite = SelectedSprite;
		}
		else
		{
			Background.sprite = NormalSprite;
		}
	}

	public void Init(CItem item, bool isInteractable, float hoverXMovement = 999f, int rewardedDurability = -1)
	{
		Init(item, UIInfoTools.Instance.GetItemSlotIcon(item.YMLData.Slot.ToString()), isInteractable, hoverXMovement, rewardedDurability);
	}

	public void Init(CItem item, Sprite icon, bool isInteractable, float hoverXMovement = 999f, int rewardedDurability = -1, bool belongsToOther = false)
	{
		this.belongsToOther = belongsToOther;
		Button.interactable = isInteractable;
		if (!isInteractable)
		{
			canvasGroup.blocksRaycasts = false;
		}
		Name.text = LocalizationManager.GetTranslation(item.Name);
		GoldCost.text = item.YMLData.Cost.ToString();
		Slot = item.YMLData.Slot;
		SlotIcon.sprite = icon;
		Disabled = false;
		this.item = item;
		UpdateView();
		MouseEnterEvent = null;
		MouseExitEvent = null;
		ClickEvent = null;
		if (hoverXMovement != 999f)
		{
			Button.hoverMovement.x = hoverXMovement;
		}
	}
}
