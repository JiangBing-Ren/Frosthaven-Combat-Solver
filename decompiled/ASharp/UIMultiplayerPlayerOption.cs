using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMultiplayerPlayerOption : UIMenuOptionButton
{
	public class PlayerEvent : UnityEvent<NetworkPlayer>
	{
	}

	[SerializeField]
	private Image userAvatar;

	[SerializeField]
	private TextMeshProUGUI userName;

	[SerializeField]
	private string formatHost = "<size=130%><sprite name=\"Host_Icon\" ></size> <color=#EACF8C>$GUI_MULTIPLAYER_HOST$</color> {0}";

	[SerializeField]
	private Button removeButton;

	[SerializeField]
	private UITextTooltipTarget textTooltip;

	public int SlotIndex;

	public PlayerEvent OnSelect = new PlayerEvent();

	public PlayerEvent OnRemove = new PlayerEvent();

	private NetworkPlayer player;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)removeButton)
		{
			removeButton.onClick.AddListener(() =>
			{
				OnRemove.Invoke(player);
			});
		}
	}

	public void SetPlayer(NetworkPlayer player, bool isSelected = false)
	{
		this.player = player;
		userAvatar.overrideSprite = player.Avatar;
		userAvatar.color = UIInfoTools.GetPlaceholderPlayerColor(player);
		if ((bool)userName)
		{
			userName.text = (player.IsClient ? player.Username : CreateLayout.LocaliseText(string.Format(formatHost, player.Username)));
		}
		if ((bool)textTooltip)
		{
			textTooltip.SetText(player.Username);
		}
		Init(() =>
		{
			OnSelect.Invoke(player);
		}, null, null, isSelected);
	}

	public override void SetSelected(bool selected)
	{
		base.SetSelected(selected);
		if ((bool)removeButton)
		{
			removeButton.gameObject.SetActive(isSelected && player.IsClient);
		}
	}
}
