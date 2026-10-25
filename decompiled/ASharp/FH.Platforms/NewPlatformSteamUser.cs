using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SETUtil;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformSteamUser : NewPlatformUser
{
	private HashSet<UnityEngine.Object> _unityResources = new HashSet<UnityEngine.Object>();

	private Texture2D _avatarTexture;

	private Sprite _avatarSprite;

	private bool _isSignedIn;

	public override string PlatformPlayerID
	{
		get
		{
			if (!SteamClient.IsValid)
			{
				return "0";
			}
			return SteamClient.SteamId.Value.ToString();
		}
	}

	public override string PlatformAccountID
	{
		get
		{
			if (!SteamClient.IsValid)
			{
				return "0";
			}
			return SteamClient.SteamId.AccountId.ToString();
		}
	}

	public override string Name
	{
		get
		{
			if (!SteamClient.IsValid)
			{
				return "-";
			}
			return SteamClient.Name;
		}
	}

	public override PresenceStatus PlatformPresence
	{
		get
		{
			if (!SteamClient.IsValid)
			{
				return PresenceStatus.Offline;
			}
			if (SteamClient.State != FriendState.Online)
			{
				if (SteamClient.State != FriendState.Away)
				{
					return PresenceStatus.Offline;
				}
				return PresenceStatus.Away;
			}
			return PresenceStatus.Online;
		}
	}

	public override bool IsSignedIn => _isSignedIn;

	public override IEnumerator SignIn()
	{
		_isSignedIn = true;
		yield break;
	}

	public override void Dispose()
	{
		foreach (UnityEngine.Object unityResource in _unityResources)
		{
			if (unityResource != null)
			{
				SceneUtil.SmartDestroy(unityResource);
			}
		}
		_unityResources.Clear();
	}

	public override void GetAvatar(string playerId, Action<Sprite> onComplete)
	{
		ulong result;
		if (_avatarSprite != null)
		{
			onComplete?.Invoke(_avatarSprite);
		}
		else if (ulong.TryParse(playerId, out result))
		{
			PlatformLayer.Instance.StartCoroutine(CreateAvatarCrt(onComplete, result));
		}
		else
		{
			Log.Error("Invalid SteamID");
		}
	}

	private IEnumerator CreateAvatarCrt(Action<Sprite> onComplete, ulong steamID)
	{
		while (!SteamClient.IsValid)
		{
			yield return null;
		}
		Task<Image?> task = Task.Run(() => SteamFriends.GetLargeAvatarAsync(steamID));
		yield return new WaitUntil(() => task.IsCompleted);
		Image? result = task.Result;
		if (result.HasValue)
		{
			Texture2D texture2D = (_avatarTexture = CreateTexture2DFromSteamImage(result.Value));
			_unityResources.Add(texture2D);
			Sprite sprite = (_avatarSprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));
			_unityResources.Add(sprite);
			onComplete?.Invoke(sprite);
		}
	}

	private Texture2D CreateTexture2DFromSteamImage(Image img)
	{
		Texture2D texture2D = new Texture2D((int)img.Width, (int)img.Height);
		for (int i = 0; i < img.Width; i++)
		{
			for (int j = 0; j < img.Height; j++)
			{
				Steamworks.Data.Color pixel = img.GetPixel(i, j);
				texture2D.SetPixel(i, (int)img.Height - j, new Color32(pixel.r, pixel.g, pixel.b, pixel.a));
			}
		}
		texture2D.Apply();
		return texture2D;
	}
}
