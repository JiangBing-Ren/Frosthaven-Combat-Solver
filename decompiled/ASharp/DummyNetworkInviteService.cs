using System;
using System.Collections.Generic;
using Assets.Script.Misc;

public class DummyNetworkInviteService : INetworkInviteService
{
	private List<IInvitePlayer> players;

	private static CallbackPromise sendInvitePromise;

	private IInvitePlayer InvitedPlayer;

	public DummyNetworkInviteService()
	{
		players = new List<IInvitePlayer>
		{
			new DummyInvitePlayer("Paula", null, isInvited: false, isOnline: false),
			new DummyInvitePlayer("Name", null, isInvited: true, isOnline: true)
		};
	}

	public void UpdatePlayers()
	{
	}

	public List<IInvitePlayer> GetPlayersToInvite()
	{
		UpdatePlayers();
		return players;
	}

	public ICallbackPromise SendInvite(IInvitePlayer player)
	{
		throw new NotImplementedException();
	}

	private void OnModifySessionCompleted()
	{
	}

	public static void OnSendInviteCallback()
	{
		if (sendInvitePromise.IsPending)
		{
			sendInvitePromise.Resolve();
			Log.GUIInfo("Session Matchmaking: invite to session sent successfully.");
		}
	}
}
