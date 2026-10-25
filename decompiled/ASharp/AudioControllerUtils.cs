using System.Linq;
using ClockStone;
using UnityEngine;
using UnityEngine.Video;

public static class AudioControllerUtils
{
	public static void PlaySound(string audioItem, bool optional = false)
	{
		if (!string.IsNullOrEmpty(audioItem))
		{
			Play(audioItem, optional);
		}
	}

	private static AudioObject Play(string audioItem, bool optional = false)
	{
		if (AudioController.IsValidAudioID(audioItem))
		{
			return AudioController.Play(audioItem);
		}
		if (!optional)
		{
			Log.AudioError("Audio item not found: " + audioItem);
		}
		return null;
	}

	public static void PlaySeqSounds(string audioItemFirst, string audioItemSecond)
	{
		if (string.IsNullOrEmpty(audioItemFirst))
		{
			PlaySound(audioItemSecond);
			return;
		}
		AudioObject audioObject = Play(audioItemFirst);
		if (audioObject == null)
		{
			PlaySound(audioItemSecond);
		}
		else if (!string.IsNullOrEmpty(audioItemSecond))
		{
			audioObject.PlayAfter(audioItemSecond);
		}
	}

	public static void StopSound(string audioItem, float fadeOutDuration = -1f)
	{
		if (!string.IsNullOrEmpty(audioItem) && AudioController.IsValidAudioID(audioItem))
		{
			AudioController.Stop(audioItem, fadeOutDuration);
		}
	}

	public static bool IsPlaying(string audioItem)
	{
		if (!string.IsNullOrEmpty(audioItem))
		{
			return AudioController.IsPlaying(audioItem);
		}
		return false;
	}

	public static bool AdjustMasterVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float num = volume / 100f;
			AudioController.SetGlobalVolume(num);
			GameObject gameObject = GameObject.Find("MainMenuVideo");
			if (gameObject != null)
			{
				VideoPlayer component = gameObject.GetComponent<VideoPlayer>();
				if (component != null)
				{
					component.SetDirectAudioVolume(0, num);
				}
			}
			return true;
		}
		return false;
	}

	public static bool AdjustMusicVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			AudioController.SetCategoryVolume("Music", volume / 100f);
			return true;
		}
		return false;
	}

	public static bool AdjustAmbientVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			return true;
		}
		return false;
	}

	public static bool AdjustUIVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			AudioController.SetCategoryVolume("UI", volume2);
			AudioController.SetCategoryVolume("MultiplayerPing", volume2);
			return true;
		}
		return false;
	}

	public static bool AdjustStoryVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			foreach (AudioCategory item in SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Where((AudioCategory it) => it.Name.EndsWith("_VO")))
			{
				AudioController.SetCategoryVolume(item.Name, volume2);
			}
			return true;
		}
		return false;
	}

	public static bool AdjustEffectsVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			AudioController.SetCategoryVolume("GAIN", volume2);
			foreach (AudioCategory item in SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Where((AudioCategory it) => it.Name.EndsWith("_SFX")))
			{
				AudioController.SetCategoryVolume(item.Name, volume2);
			}
			return true;
		}
		return false;
	}

	private static bool TryAdjustVolume()
	{
		if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			return true;
		}
		Log.AudioError("AudioSettings: Trying to adjust volume but no instance of AudioController exists.");
		return false;
	}
}
