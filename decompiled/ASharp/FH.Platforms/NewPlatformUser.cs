using System;
using System.Collections;
using UnityEngine;

namespace FH.Platforms;

public abstract class NewPlatformUser
{
	public abstract string PlatformPlayerID { get; }

	public abstract string PlatformAccountID { get; }

	public abstract bool IsSignedIn { get; }

	public abstract string Name { get; }

	public abstract PresenceStatus PlatformPresence { get; }

	public abstract IEnumerator SignIn();

	public abstract void Dispose();

	public abstract void GetAvatar(string playerId, Action<Sprite> onComplete);

	internal virtual void GetCurrentUserData(Action<PlatformUserData> onComplete)
	{
		GetAvatar(PlatformPlayerID, (Sprite avatar) =>
		{
			onComplete(new PlatformUserData
			{
				Name = Name,
				PlatformPlayerID = PlatformPlayerID,
				PlatformAccountID = PlatformAccountID,
				PlatformPresence = PlatformPresence,
				Avatar = avatar
			});
		});
	}

	internal virtual void GetOtherUserData(string platformPlayerId, Action<PlatformUserData> onComplete)
	{
		GetAvatar(platformPlayerId, (Sprite avatar) =>
		{
			onComplete(new PlatformUserData
			{
				Name = null,
				PlatformPlayerID = platformPlayerId,
				PlatformAccountID = PlatformAccountID,
				PlatformPresence = PlatformPresence,
				Avatar = avatar
			});
		});
	}

	internal void GetOtherAvatar(string platformPlayerID, Action<Sprite> onComplete)
	{
		GetAvatar(platformPlayerID, onComplete);
	}

	protected void OnSignInFailed(string error)
	{
		Log.ErrorFormat(error);
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", "", Application.Quit, error);
	}
}
