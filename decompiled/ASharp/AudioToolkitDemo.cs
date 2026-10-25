using System.Collections.Generic;
using ClockStone;
using UnityEngine;

public class AudioToolkitDemo : MonoBehaviour
{
	public AudioClip customAudioClip;

	private float musicVolume = 1f;

	private float ambienceVolume = 1f;

	private bool musicPaused;

	private Vector2 playlistScrollPos = Vector2.zero;

	private PoolableReference<AudioObject> introLoopOutroAudio;

	private bool wasClipAdded;

	private bool wasCategoryAdded;

	private List<bool> disableGUILevels = new List<bool>();

	private void OnGUI()
	{
		DrawGuiLeftSide();
		DrawGuiRightSide();
		DrawGuiBottom();
	}

	private void DrawGuiLeftSide()
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		gUIStyle.normal.textColor = new Color(1f, 1f, 0.5f);
		string text = $"ClockStone Audio Toolkit - Demo";
		GUI.Label(new Rect(22f, 10f, 500f, 20f), text, gUIStyle);
		int num = 10;
		int num2 = 35;
		int num3 = 200;
		int num4 = 130;
		num += 30;
		if (!SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			GUI.Label(new Rect(250f, num, num3, 30f), "No Audio Controller found!");
			return;
		}
		GUI.Label(new Rect(250f, num, num3, 30f), "Global Volume");
		AudioController.SetGlobalVolume(GUI.HorizontalSlider(new Rect(250f, num + 20, num4, 30f), AudioController.GetGlobalVolume(), 0f, 1f));
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Cross-fade to music track 1"))
		{
			AudioController.PlayMusic("MusicTrack1");
		}
		num += num2;
		GUI.Label(new Rect(250f, num + 10, num3, 30f), "Music Volume");
		float num5 = GUI.HorizontalSlider(new Rect(250f, num + 30, num4, 30f), musicVolume, 0f, 1f);
		if (num5 != musicVolume)
		{
			musicVolume = num5;
			AudioController.SetCategoryVolume("Music", musicVolume);
		}
		GUI.Label(new Rect(250 + num4 + 30, num + 10, num3, 30f), "Ambience Volume");
		float num6 = GUI.HorizontalSlider(new Rect(250 + num4 + 30, num + 30, num4, 30f), ambienceVolume, 0f, 1f);
		if (num6 != ambienceVolume)
		{
			ambienceVolume = num6;
			AudioController.SetCategoryVolume("Ambience", ambienceVolume);
		}
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Cross-fade to music track 2"))
		{
			AudioController.PlayMusic("MusicTrack2");
		}
		num += num2;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Fade out music category"))
		{
			AudioController.FadeOutCategory("Music", 2f);
		}
		num += num2;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Fade in music category"))
		{
			AudioController.FadeInCategory("Music", 2f);
		}
		num += num2;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Stop Music"))
		{
			AudioController.StopMusic(0.3f);
		}
		num += num2;
		bool flag = GUI.Toggle(new Rect(20f, num, num3, 30f), musicPaused, "Pause All Audio");
		if (flag != musicPaused)
		{
			musicPaused = flag;
			if (musicPaused)
			{
				AudioController.PauseAll(0.1f);
			}
			else
			{
				AudioController.UnpauseAll(0.1f);
			}
		}
		num += 20;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Play Sound with random pitch"))
		{
			AudioController.Play("RandomPitchSound");
		}
		num += num2;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Play Sound with alternatives"))
		{
			AudioObject audioObject = AudioController.Play("AlternativeSound");
			if (audioObject != null)
			{
				audioObject.completelyPlayedDelegate = OnAudioCompleteleyPlayed;
			}
		}
		num += num2;
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Play Both"))
		{
			AudioObject audioObject2 = AudioController.Play("RandomAndAlternativeSound");
			if (audioObject2 != null)
			{
				audioObject2.completelyPlayedDelegate = OnAudioCompleteleyPlayed;
			}
		}
		num += num2;
		GUI.Label(new Rect(20f, num, 100f, 20f), "Playlists: ");
		num += 20;
		playlistScrollPos = GUI.BeginScrollView(new Rect(20f, num, num3, 100f), playlistScrollPos, new Rect(0f, 0f, num3, 33f * (float)SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists.Length));
		for (int i = 0; i < SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists.Length; i++)
		{
			if (GUI.Button(new Rect(20f, (float)i * 33f, num3 - 20, 30f), SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[i].name))
			{
				AudioController.SetCurrentMusicPlaylist(SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[i].name);
			}
		}
		num += 105;
		GUI.EndScrollView();
		if (GUI.Button(new Rect(20f, num, num3, 30f), "Play Music Playlist"))
		{
			AudioController.PlayMusicPlaylist();
		}
		num += num2;
		if (AudioController.IsPlaylistPlaying() && GUI.Button(new Rect(20f, num, num3, 30f), "Next Track on Playlist"))
		{
			AudioController.PlayNextMusicOnPlaylist();
		}
		num += 32;
		if (AudioController.IsPlaylistPlaying() && GUI.Button(new Rect(20f, num, num3, 30f), "Previous Track on Playlist"))
		{
			AudioController.PlayPreviousMusicOnPlaylist();
		}
		num += num2;
		SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[0].loopPlaylist = GUI.Toggle(new Rect(20f, num, num3, 30f), SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[0].loopPlaylist, "Loop Playlist");
		num += 20;
		SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[0].shufflePlaylist = GUI.Toggle(new Rect(20f, num, num3, 30f), SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists[0].shufflePlaylist, "Shuffle Playlist ");
		num += 20;
		SingletonMonoBehaviour<AudioController>.Instance.soundMuted = GUI.Toggle(new Rect(20f, num, num3, 30f), SingletonMonoBehaviour<AudioController>.Instance.soundMuted, "Sound Muted");
	}

	private void DrawGuiRightSide()
	{
		int num = 50;
		int num2 = 35;
		int num3 = 300;
		if (!wasCategoryAdded)
		{
			if (customAudioClip != null && GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Create new category with custom AudioClip"))
			{
				AudioController.AddToCategory(AudioController.NewCategory("Custom Category"), customAudioClip, "CustomAudioItem");
				wasClipAdded = true;
				wasCategoryAdded = true;
			}
		}
		else
		{
			if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Play custom AudioClip"))
			{
				AudioController.Play("CustomAudioItem");
			}
			if (wasClipAdded)
			{
				num += num2;
				if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Remove custom AudioClip") && AudioController.RemoveAudioItem("CustomAudioItem"))
				{
					wasClipAdded = false;
				}
			}
		}
		num = 130;
		if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Play gapless audio loop"))
		{
			AudioObject audioObject = AudioController.Play("GaplessLoopTest");
			if ((bool)audioObject)
			{
				audioObject.Stop(1f, 4f);
			}
		}
		num += num2;
		if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Play random loop sequence"))
		{
			AudioController.Play("RandomLoopSequence");
		}
		num += num2;
		if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 50f), "Play intro-loop-outro sequence\ngatling gun"))
		{
			introLoopOutroAudio = new PoolableReference<AudioObject>(AudioController.Play("IntroLoopOutro_Gun"));
		}
		num += 20;
		num += num2;
		BeginDisabledGroup(introLoopOutroAudio == null || !(introLoopOutroAudio.Get() != null));
		if (GUI.Button(new Rect(Screen.width - (num3 + 20), num, num3, 30f), "Finish gatling gun sequence"))
		{
			introLoopOutroAudio.Get().FinishSequence();
		}
		EndDisabledGroup();
	}

	private void DrawGuiBottom()
	{
		if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 40, 300f, 30f), "Video tutorial & more info..."))
		{
			Application.OpenURL("http://unity.clockstone.com");
		}
	}

	private void OnAudioCompleteleyPlayed(AudioObject audioObj)
	{
		Debug.Log("Finished playing " + audioObj.audioID + " with clip " + audioObj.primaryAudioSource.clip.name);
	}

	private void BeginDisabledGroup(bool condition)
	{
		disableGUILevels.Add(condition);
		GUI.enabled = !IsGUIDisabled();
	}

	private void EndDisabledGroup()
	{
		int count = disableGUILevels.Count;
		if (count > 0)
		{
			disableGUILevels.RemoveAt(count - 1);
			GUI.enabled = !IsGUIDisabled();
		}
		else
		{
			Debug.LogWarning("misplaced EndDisabledGroup");
		}
	}

	private bool IsGUIDisabled()
	{
		foreach (bool disableGUILevel in disableGUILevels)
		{
			if (disableGUILevel)
			{
				return true;
			}
		}
		return false;
	}
}
