using UnityEngine;

public interface IInvitePlayer
{
	string Username { get; }

	Texture2D Avatar { get; }

	bool IsInvited { get; set; }

	bool IsOnline { get; }

	string CurrentlyPlayingGameName { get; }
}
