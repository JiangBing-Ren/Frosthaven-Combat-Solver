using UnityEngine;

public class AudioSystem : MonoBehaviour
{
	public static AudioSystem instance;

	[AudioPlaylistName]
	public string m_MusicPlaylist;

	[AudioEventName]
	public string m_AmbientEvent;

	private void Awake()
	{
		instance = this;
	}
}
