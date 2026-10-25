using System;
using System.Collections;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEditorUser : NewPlatformUser
{
	public override string PlatformPlayerID => "0";

	public override string PlatformAccountID => "0";

	public override string Name => "-";

	public override PresenceStatus PlatformPresence => PresenceStatus.UnavailableOnPlatform;

	public override bool IsSignedIn => true;

	public override IEnumerator SignIn()
	{
		yield break;
	}

	public override void GetAvatar(string playerId, Action<Sprite> onComplete)
	{
		onComplete?.Invoke(null);
	}

	public override void Dispose()
	{
	}
}
