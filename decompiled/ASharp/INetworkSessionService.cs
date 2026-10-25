using System;
using FFSNet;

public interface INetworkSessionService
{
	PlayersChangedEvent OnPlayerJoined { get; set; }

	PlayersChangedEvent OnPlayerLeft { get; set; }

	void EndSession();

	bool IsOnline(NetworkPlayer player);

	void StartSession(Action onHostStarted, Action onHostEnded);

	string GetInviteCode();

	void GenerateInviteCode();
}
