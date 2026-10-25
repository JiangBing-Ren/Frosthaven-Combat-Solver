using System;
using System.Collections;
using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMultiplayerSaveOwner : MonoBehaviour
{
	public class PlayerEvent : UnityEvent<NetworkPlayer>
	{
	}

	[SerializeField]
	private Image userAvatar;

	[SerializeField]
	private TextMeshProUGUI userName;

	public PlayerEvent OnSelect = new PlayerEvent();

	public PlayerEvent OnDeselect = new PlayerEvent();

	[NonSerialized]
	private SaveOwner _owner;

	public void SetPlayer(SaveOwner owner)
	{
		_owner = owner;
		if (owner != null)
		{
			if (owner.AvatarSet)
			{
				userAvatar.sprite = owner.Avatar?.Avatar;
			}
			else if (gameObject.activeInHierarchy)
			{
				StartCoroutine(LoadAvatarCrt(_owner));
			}
		}
		userAvatar.color = Color.white;
		userName.text = owner?.Username;
	}

	public void OnEnable()
	{
		if (_owner != null)
		{
			if (!_owner.AvatarSet)
			{
				StartCoroutine(LoadAvatarCrt(_owner));
			}
			else
			{
				userAvatar.sprite = _owner.Avatar?.Avatar;
			}
		}
	}

	private IEnumerator LoadAvatarCrt(SaveOwner owner)
	{
		yield return owner.LoadAvatarFromPlatformCrt();
		userAvatar.sprite = owner.Avatar?.Avatar;
	}
}
