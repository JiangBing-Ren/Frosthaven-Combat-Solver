using System;

namespace FH.Platforms;

public class NewPlatformPS5Networking : INewPlatformNetworking
{
	public bool InvitesSupported => false;

	public string PendingSessionJoin => null;

	public void AdvertiseLobbyKey(string key)
	{
	}

	public void OpenInviteOverlay()
	{
		throw new NotImplementedException();
	}

	public void ClearPendingSessionJoin()
	{
	}
}
