using UnityEngine;

internal class DummyInvitePlayer : IInvitePlayer
{
	public string Username { get; }

	public Texture2D Avatar { get; }

	public bool IsInvited { get; set; }

	public bool IsOnline { get; }

	public string CurrentlyPlayingGameName { get; }

	public DummyInvitePlayer(string username, Texture2D avatar, bool isInvited, bool isOnline, string currentlyPlayingGameName = null)
	{
		Username = username;
		Avatar = avatar;
		IsInvited = isInvited;
		IsOnline = isOnline;
		CurrentlyPlayingGameName = currentlyPlayingGameName;
	}
}
