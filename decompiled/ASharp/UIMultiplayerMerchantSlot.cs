using FFSNet;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerMerchantSlot : UIMenuOptionButton
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private MultiplayerUserState owner;

	[SerializeField]
	private Image focusMask;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	public int SlotIndex;

	private NetworkPlayer player;

	private bool isUnfocused;

	public string CharacterID { get; set; }

	public string CharacterName { get; set; }

	public string Title => title?.text;

	public NetworkPlayer AssignedPlayer => player;

	private void Init(string title, string characterID, string characterName, bool isSelected, bool interactable)
	{
		if (this.title != null)
		{
			this.title.text = title;
		}
		CharacterID = characterID;
		CharacterName = characterName;
		player = null;
		SetSelected(isSelected);
		SetInteractable(interactable);
		gameObject.SetActive(value: true);
	}

	public void SetEmpty(string title, bool isSelected = false, bool interactable = false)
	{
		if (ScenarioManager.IsInTactical)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			Init(title, null, null, isSelected, interactable);
		}
	}

	public void SetCharacter(CMapCharacter character, bool isSelected = false, bool interactable = true)
	{
		Init(LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey), character.CharacterID, character.CharacterName, isSelected, interactable);
	}

	public void AssignTo(NetworkPlayer player, bool isOnline)
	{
		if (player != null && this.player == player)
		{
			UpdateConnectionStatus(isOnline);
			return;
		}
		this.player = player;
		if (player != null)
		{
			owner.Show(player, isOnline);
		}
		else
		{
			owner.Hide();
		}
	}

	public void UpdateConnectionStatus(bool isOnline)
	{
		owner.RefreshOnline(isOnline);
	}

	public void Focus(bool focus, bool interactable = true)
	{
		isUnfocused = !focus;
		focusMask.enabled = isUnfocused && !isHovered;
		focusMask.raycastTarget = !interactable;
	}

	protected override void OnHovered(bool hovered)
	{
		base.OnHovered(hovered);
		focusMask.enabled = isUnfocused && !hovered;
	}

	public void SetInteractable(bool isInteractable)
	{
		button.interactable = isInteractable;
		tooltip.enabled = !isInteractable;
	}
}
