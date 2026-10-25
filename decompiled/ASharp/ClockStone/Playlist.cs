using System;

namespace ClockStone;

[Serializable]
public class Playlist
{
	public string name;

	public string[] playlistItems;

	public bool loopPlaylist = true;

	public bool shufflePlaylist = true;

	public int forcedFirstTracks;

	public Playlist()
	{
		name = "Default";
		playlistItems = null;
	}

	public Playlist(string name, string[] playlistItems)
	{
		this.name = name;
		this.playlistItems = playlistItems;
	}
}
