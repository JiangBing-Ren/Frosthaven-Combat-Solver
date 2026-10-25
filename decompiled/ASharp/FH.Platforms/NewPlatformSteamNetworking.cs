using System;
using System.Collections;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace FH.Platforms;

internal class NewPlatformSteamNetworking : INewPlatformNetworking
{
	private static string _pendingSessionJoin = string.Empty;

	private static bool _sessionJoinArgUsedUp = false;

	public bool InvitesSupported => true;

	public string PendingSessionJoin
	{
		get
		{
			if (!_sessionJoinArgUsedUp)
			{
				return _pendingSessionJoin;
			}
			return null;
		}
	}

	private Action<string> _joinGameSession { get; }

	public NewPlatformSteamNetworking(Action<string> joinGameSession)
	{
		_joinGameSession = joinGameSession;
	}

	public IEnumerator InitCrt()
	{
		SteamFriends.OnGameRichPresenceJoinRequested -= JoinRequested;
		SteamFriends.OnGameRichPresenceJoinRequested += JoinRequested;
		_pendingSessionJoin = LobbyKeyFromSysArgs();
		yield break;
	}

	public void ClearPendingSessionJoin()
	{
		_pendingSessionJoin = string.Empty;
		_sessionJoinArgUsedUp = true;
	}

	private static string LobbyKeyFromSysArgs()
	{
		if (_sessionJoinArgUsedUp)
		{
			return _pendingSessionJoin;
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs != null && commandLineArgs.Length > 1)
		{
			return ParseLobbyKey(commandLineArgs);
		}
		_sessionJoinArgUsedUp = true;
		return null;
	}

	private static string ParseLobbyKey(params string[] args)
	{
		string text = args.FirstOrDefault((string x) => x.Contains("+connect="))?.Split("=").Last();
		if (text == null)
		{
			string text2 = args.FirstOrDefault((string x) => x.Contains("joinmp_"));
			if (text2 == null)
			{
				return null;
			}
			text = text2.Replace("joinmp_", "");
		}
		return text;
	}

	private void JoinRequested(Friend friend, string arg2)
	{
		Debug.Log($"Someone attempts to join(name:{friend.Name}, relationship: {friend.Relationship}, value:{arg2})");
		string text = ParseLobbyKey(arg2);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("Invalid lobby key format " + text);
		}
		else
		{
			_joinGameSession(text);
		}
	}

	public void AdvertiseLobbyKey(string lobbyKey)
	{
		if (string.IsNullOrEmpty(lobbyKey))
		{
			SteamFriends.SetRichPresence("connect", string.Empty);
		}
		else
		{
			SteamFriends.SetRichPresence("connect", "joinmp_" + lobbyKey);
		}
	}

	public void OpenInviteOverlay()
	{
		SteamFriends.OpenOverlay("friends");
	}
}
