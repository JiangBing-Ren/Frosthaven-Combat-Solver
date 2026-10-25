namespace FH.Platforms;

public class NewPlatformStandaloneNetworking : INewPlatformNetworking
{
	public string PendingSessionJoin => null;

	public bool InvitesSupported => false;

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
