using System;
using System.Collections;
using System.Collections.Generic;
using SETUtil;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEGSUser : NewPlatformUser
{
	private PlatformUserData _platformUserData = new PlatformUserData
	{
		Name = "0",
		PlatformPlayerID = "0",
		PlatformAccountID = "0",
		PlatformPresence = PresenceStatus.Offline,
		Avatar = null
	};

	private HashSet<UnityEngine.Object> _unityResources = new HashSet<UnityEngine.Object>();

	private Texture2D _avatarTexture;

	private Sprite _avatarSprite;

	private bool _isSignedIn;

	public override bool IsSignedIn => _isSignedIn;

	public override string PlatformPlayerID => _platformUserData.PlatformPlayerID;

	public override string PlatformAccountID => _platformUserData.PlatformAccountID;

	public override string Name => _platformUserData.Name;

	public override PresenceStatus PlatformPresence => _platformUserData.PlatformPresence;

	public override IEnumerator SignIn()
	{
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
		onComplete?.Invoke(null);
	}
}
