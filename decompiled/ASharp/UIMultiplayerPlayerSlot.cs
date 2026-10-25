using System.Collections.Generic;
using FFSNet;
using FHGUI;
using UnityEngine;

public class UIMultiplayerPlayerSlot : MonoBehaviour
{
	public struct Params
	{
		public NetworkPlayer Player;

		public IEnumerable<FHInputEventElement.IInputEventData> Actions;

		public bool IsPrimaryActionPositive;
	}

	[Header("Components")]
	[SerializeField]
	private UIMultiplayerUser UserAvatar;

	[SerializeField]
	private GameObject Empty;

	[SerializeField]
	private FHButton Button;

	[SerializeField]
	private UITextTooltipTarget Tooltip;

	[SerializeField]
	private FHInputEventContainer ActionContainer;

	[SerializeField]
	private GameObject PositiveHighlight;

	[SerializeField]
	private GameObject NegativeHighlight;

	public void Init(Params @params)
	{
		if (@params.Player != null)
		{
			UserAvatar.Show(@params.Player);
		}
		else
		{
			UserAvatar.Hide();
		}
		Button.enabled = @params.Player == null || @params.Player.IsClient;
		Empty.gameObject.SetActive(@params.Player == null);
		ActionContainer.Init(@params.Actions);
		Tooltip.SetText((@params.Player != null) ? @params.Player.Username : string.Empty);
		PositiveHighlight.SetActive(@params.IsPrimaryActionPositive);
		NegativeHighlight.SetActive(!@params.IsPrimaryActionPositive);
	}
}
