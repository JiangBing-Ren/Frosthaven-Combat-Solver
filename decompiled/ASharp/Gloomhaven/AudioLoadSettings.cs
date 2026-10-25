using UnityEngine;

namespace Gloomhaven;

public class AudioLoadSettings : MonoBehaviour
{
	private void Start()
	{
		if (SaveData.Instance != null && SaveData.Instance.Global != null)
		{
			Initialize();
		}
		else
		{
			SceneController.Instance.OnSaveDataLoaded += Initialize;
		}
	}

	private void Initialize()
	{
		SceneController.Instance.OnSaveDataLoaded -= Initialize;
		int masterVolume = SaveData.Instance.Global.MasterVolume;
		int musicVolume = SaveData.Instance.Global.MusicVolume;
		int ambientVolume = SaveData.Instance.Global.AmbientVolume;
		int sFXVolume = SaveData.Instance.Global.SFXVolume;
		int uIVolume = SaveData.Instance.Global.UIVolume;
		int storyVolume = SaveData.Instance.Global.StoryVolume;
		AudioControllerUtils.AdjustMasterVolume(masterVolume);
		AudioControllerUtils.AdjustMusicVolume(musicVolume);
		AudioControllerUtils.AdjustAmbientVolume(ambientVolume);
		AudioControllerUtils.AdjustEffectsVolume(sFXVolume);
		AudioControllerUtils.AdjustUIVolume(uIVolume);
		AudioControllerUtils.AdjustStoryVolume(storyVolume);
	}
}
