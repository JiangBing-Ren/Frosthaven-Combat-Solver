namespace FH.Platforms;

public interface INewPlatformNetworking
{
	bool InvitesSupported { get; }

	string PendingSessionJoin { get; }

	void AdvertiseLobbyKey(string lobbyKey);

	void OpenInviteOverlay();

	void ClearPendingSessionJoin();
}
