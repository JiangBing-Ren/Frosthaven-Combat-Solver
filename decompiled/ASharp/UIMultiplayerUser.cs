using System;
using System.Collections;
using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerUser : MonoBehaviour
{
	[Serializable]
	public struct Data
	{
		public int Id;

		public string Username;

		public Sprite Avatar;

		public bool IsLeader;
	}

	[SerializeField]
	protected TextMeshProUGUI username;

	[SerializeField]
	protected Image userAvatar;

	[SerializeField]
	protected GameObject hostMask;

	[SerializeField]
	protected string hostFormat;

	protected NetworkPlayer player;

	public virtual void Show(NetworkPlayer player)
	{
		gameObject.SetActive(value: true);
		if (player != this.player)
		{
			this.player = player;
			if (username != null)
			{
				username.text = ((player.IsClient || hostFormat.IsNullOrEmpty()) ? player.Username : CreateLayout.LocaliseText(hostFormat, skipWarnings: false, player.Username));
			}
			if (hostMask != null)
			{
				hostMask.SetActive(!player.IsClient);
			}
			if (userAvatar != null && gameObject.activeInHierarchy)
			{
				userAvatar.color = UIInfoTools.GetPlaceholderPlayerColor(player);
				StartCoroutine(UpdateAvatarCrt());
			}
		}
	}

	private IEnumerator UpdateAvatarCrt()
	{
		yield return player.WaitForAvatarLoadedCrt();
		userAvatar.overrideSprite = player.Avatar;
		userAvatar.color = ((player.Avatar == null) ? UIInfoTools.GetPlaceholderPlayerColor(player) : Color.white);
	}

	public void Show(Data userData)
	{
		if (username != null)
		{
			username.text = ((!userData.IsLeader || hostFormat.IsNullOrEmpty()) ? userData.Username : CreateLayout.LocaliseText(hostFormat, skipWarnings: false, userData.Username));
		}
		if (hostMask != null)
		{
			hostMask.SetActive(userData.IsLeader);
		}
		if (userAvatar != null)
		{
			userAvatar.overrideSprite = userData.Avatar;
			userAvatar.color = ((userData.Avatar == null) ? UIInfoTools.GetPlaceholderPlayerColor(userData.Id) : Color.white);
		}
		gameObject.SetActive(value: true);
	}

	private void Update()
	{
		if (userAvatar != null && player != null && player.Avatar != null && userAvatar.overrideSprite != player.Avatar)
		{
			userAvatar.overrideSprite = player.Avatar;
			userAvatar.color = ((player.Avatar == null) ? UIInfoTools.GetPlaceholderPlayerColor(player) : Color.white);
		}
	}

	public virtual void Hide()
	{
		player = null;
		gameObject.SetActive(value: false);
	}
}
