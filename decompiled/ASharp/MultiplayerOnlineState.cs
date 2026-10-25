using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerOnlineState : MonoBehaviour
{
	[SerializeField]
	private List<Graphic> statusGraphics;

	[SerializeField]
	private TextLocalizedListener statusText;

	public void SetOnline(bool isOnline)
	{
		Color color = (isOnline ? UIInfoTools.Instance.playerOnlineColor : UIInfoTools.Instance.playerConnectingColor);
		for (int i = 0; i < statusGraphics.Count; i++)
		{
			statusGraphics[i].color = color;
		}
		if (statusText != null)
		{
			statusText.SetTextKey(isOnline ? "GUI_PLAYER_ONLINE" : "GUI_PLAYER_CONNECTING");
		}
	}
}
