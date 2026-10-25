using System;
using System.Collections;
using Libs.GameConsole;

namespace FH.Platforms;

public class NewPlatformEGSNetworking : INewPlatformNetworking
{
	private NewPlatformEGSUser _user;

	private string _lobbyKey;

	public bool InvitesSupported => true;

	public string PendingSessionJoin => null;

	private Action<string> _joinGameSession { get; }

	public NewPlatformEGSNetworking(NewPlatformEGSUser user, Action<string> JoinGameSession)
	{
		_user = user;
		_joinGameSession = JoinGameSession;
	}

	internal IEnumerator InitCrt()
	{
		yield break;
	}

	public void AdvertiseLobbyKey(string key)
	{
		if (key == null)
		{
			NukeAnyCurrentLobby(null);
			return;
		}
		throw new NotImplementedException("AdvertiseLobbyKey is not implemented for this platform.");
	}

	public void OpenInviteOverlay()
	{
	}

	public void ClearPendingSessionJoin()
	{
	}

	[ConsoleCommand(Command = "eos_list_friends", Description = "List all invitable friends in the EOS network.")]
	public static void Debug_ListFriends(IConsole console)
	{
		console.WriteLine("Listing EOS friends is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_invite_friend_to_game", Description = "Invite a friend to the game. Usage: eos_invite_friend <friendEpicAccountId>")]
	public static void Debug_InviteFriend(IConsole console, string friendEpicAccountId)
	{
		console.WriteLine("Inviting EOS friends is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_current_lobby", Description = "Get the current lobby ID if available.")]
	public static void Debug_GetCurrentLobby(IConsole console)
	{
		console.WriteLine("Getting current lobby is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_create_lobby", Description = "Create a new lobby")]
	public static void Debug_CreateLobby(IConsole console)
	{
		console?.WriteLine("Creating a lobby is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_open_invite_overlay", Description = "Open the invite overlay for the current lobby.")]
	public static void Debug_OpenInviteOverlay(IConsole console)
	{
		console.WriteLine("Opening invite overlay is not supported on this platform.");
	}

	private void NukeAnyCurrentLobby(Action whenDone)
	{
	}

	[ConsoleCommand(Command = "eos_list_invites", Description = "List all pending invites in the EOS network.")]
	public static void Debug_ListInvites(IConsole console)
	{
		console.WriteLine("Listing EOS invites is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_leave_party")]
	public static void Debug_LeaveLobby(IConsole console)
	{
		console.WriteLine("Accepting EOS invites is not supported on this platform.");
	}

	[ConsoleCommand(Command = "eos_join_lobby_by_id", Description = "Accept an invite to a lobby. Usage: eos_accept_invite <inviteId>")]
	public static void Debug_JoinLobby(IConsole console, string id)
	{
		console.WriteLine("Accepting EOS invites is not supported on this platform.");
	}
}
