using System;
using System.Collections;
using System.IO;
using System.Linq;
using Libs.GameConsole;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundView : MonoBehaviour
{
	public GameObject m_BG_Image;

	[SerializeField]
	private GameObject m_Logo;

	[SerializeField]
	private GameObject m_Blur;

	private GameObject m_VideoPlayerGO;

	private VideoPlayer m_VideoPlayer;

	private bool m_Disabled;

	[ConsoleVariable(Alias = "preroll_mainscreen_video", Persistent = true)]
	private static bool _preroll_kvar = true;

	public void LogoSetActive(bool active)
	{
		m_Logo.SetActive(active);
	}

	public void BlurSetActive(bool active)
	{
		m_Blur.SetActive(active);
	}

	private void OnEnable()
	{
		if (m_Disabled)
		{
			return;
		}
		try
		{
			m_VideoPlayerGO = GameObject.Find("MainMenuVideo");
			if (!(m_VideoPlayerGO == null))
			{
				if (m_VideoPlayer == null)
				{
					m_VideoPlayer = m_VideoPlayerGO.GetComponent<VideoPlayer>();
				}
				m_VideoPlayer.GetComponent<Camera>().enabled = false;
				string randomVideo = GetRandomVideo();
				m_VideoPlayer.enabled = false;
				if (m_VideoPlayer.url == randomVideo)
				{
					m_VideoPlayer.Pause();
					return;
				}
				m_VideoPlayer.Stop();
				m_VideoPlayer.clip = null;
				m_VideoPlayer.url = null;
				m_VideoPlayer.time = 0.0;
				PrepareVideo(randomVideo);
			}
		}
		catch
		{
			m_Disabled = true;
		}
	}

	private string GetRandomVideo()
	{
		if (SceneController.Instance.MainMenuVideos == null || SceneController.Instance.MainMenuVideos.Count <= 0)
		{
			return null;
		}
		int num = GloomUtility.ExclusiveRandomNumber(SharedClient.GlobalRNG, SceneController.Instance.MainMenuVideoHashSet, SceneController.Instance.MainMenuVideos.Count - 1);
		SceneController.Instance.MainMenuVideoHashSet.Add(num);
		if (SceneController.Instance.MainMenuVideos.Count > num && File.Exists(SceneController.Instance.MainMenuVideos[num]))
		{
			return SceneController.Instance.MainMenuVideos[num];
		}
		return null;
	}

	private void PrepareVideo(string videoURL)
	{
		try
		{
			if (!videoURL.IsNullOrEmpty())
			{
				if (SceneController.Instance.MainMenuVideoHashSet.Count == SceneController.Instance.MainMenuVideos.Count)
				{
					SceneController.Instance.MainMenuVideoHashSet.Clear();
				}
				int num = SceneController.Instance.MainMenuVideos.IndexOf(videoURL);
				SceneController.Instance.MainMenuVideoHashSet.Add(num);
				if (SceneController.Instance.MainMenuVideos.Count > num && File.Exists(SceneController.Instance.MainMenuVideos[num]))
				{
					m_VideoPlayer.source = VideoSource.Url;
					m_VideoPlayer.url = videoURL;
					m_VideoPlayer.time = 0.0;
					m_VideoPlayer.enabled = true;
					m_VideoPlayer.playOnAwake = false;
					m_VideoPlayer.skipOnDrop = true;
					m_VideoPlayer.waitForFirstFrame = false;
					m_VideoPlayer.Prepare();
					m_VideoPlayer.prepareCompleted += videoPlayerOnPrepareCompleted;
				}
			}
		}
		catch
		{
			m_Disabled = true;
		}
		void videoPlayerOnPrepareCompleted(VideoPlayer source)
		{
			m_VideoPlayer.prepareCompleted -= videoPlayerOnPrepareCompleted;
			if (_preroll_kvar)
			{
				CoroutineHelper.RunCoroutine(PrerollVideo());
			}
		}
	}

	private IEnumerator PrerollVideo()
	{
		if (!(m_VideoPlayer == null) && !m_VideoPlayer.isPlaying)
		{
			m_VideoPlayer.targetCamera.enabled = false;
			m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
			m_VideoPlayer.Play();
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}
			if (!(m_VideoPlayer == null))
			{
				m_VideoPlayer.Pause();
				m_VideoPlayer.time = 0.0;
			}
		}
	}

	private void Update()
	{
		if (m_Disabled || m_VideoPlayerGO == null || SceneController.Instance.IsLoading || SceneController.Instance.ScenarioIsLoading)
		{
			return;
		}
		try
		{
			if (!m_VideoPlayer.url.IsNullOrEmpty() && m_VideoPlayer.isPrepared && !m_VideoPlayer.isPlaying)
			{
				if ((bool)m_BG_Image && m_BG_Image.activeSelf)
				{
					m_BG_Image.SetActive(value: false);
				}
				m_VideoPlayer.SetDirectAudioVolume(0, AudioController.GetGlobalVolume());
				m_VideoPlayerGO.GetComponent<Camera>().enabled = true;
				m_VideoPlayer.Play();
			}
		}
		catch
		{
			m_Disabled = true;
		}
	}

	[ConsoleCommand(Command = "random_main_menu_video")]
	public static void ShowRandomVideo_Cheat(IConsole console)
	{
		BackgroundView backgroundView = UnityEngine.Object.FindObjectOfType<BackgroundView>();
		if (backgroundView != null)
		{
			backgroundView.enabled = false;
			backgroundView.enabled = true;
		}
	}

	[ConsoleCommand(Command = "play_main_menu_video")]
	public static void PlayVideo_Cheat(IConsole console, string video)
	{
		BackgroundView backgroundView = UnityEngine.Object.FindObjectOfType<BackgroundView>();
		if (backgroundView != null)
		{
			string text = SceneController.Instance?.MainMenuVideos.FirstOrDefault((string v) => v.Contains(video, StringComparison.InvariantCultureIgnoreCase));
			if (text != null)
			{
				backgroundView.PrepareVideo(text);
			}
		}
	}
}
