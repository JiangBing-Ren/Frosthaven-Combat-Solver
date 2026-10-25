namespace FH.Platforms;

public class NewPlatformXboxGameCoreNetworking : INewPlatformNetworking
{
	public bool InvitesSupported => false;

	public string PendingSessionJoin => null;

	public void AdvertiseLobbyKey(string key)
	{
	}

	public void OpenInviteOverlay()
	{
	}

	public void ClearPendingSessionJoin()
	{
	}
}
