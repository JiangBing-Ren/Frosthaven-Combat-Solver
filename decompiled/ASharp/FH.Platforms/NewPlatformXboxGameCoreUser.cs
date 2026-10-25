using System;
using System.Collections;
using System.Collections.Generic;
using SETUtil;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformXboxGameCoreUser : NewPlatformUser
{
	private HashSet<UnityEngine.Object> _unityResources = new HashSet<UnityEngine.Object>();

	private Texture2D _avatarTexture;

	private Sprite _avatarSprite;

	public override string PlatformPlayerID => "0";

	public override string PlatformAccountID => "0";

	public override string Name => "NONE";

	public override PresenceStatus PlatformPresence => PresenceStatus.UnavailableOnPlatform;

	public override bool IsSignedIn => true;

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
