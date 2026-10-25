using UnityEngine;

namespace Gloomhaven;

public class AudioSettings : MonoBehaviour
{
	[SerializeField]
	private UISliderController masterVolumeSlider;

	[SerializeField]
	private UISliderController musicVolumeSlider;

	[SerializeField]
	private UISliderController ambientVolumeSlider;

	[SerializeField]
	private UISliderController effectsVolumeSlider;

	[SerializeField]
	private UISliderController uiVolumeSlider;

	[SerializeField]
	private UISliderController storyVolumeSlider;

	[SerializeField]
	private ButtonSwitch muteInBackgroundToggle;

	private void Awake()
	{
		muteInBackgroundToggle.OnValueChanged.AddListener(ToggleMuteAudioInBackground);
		masterVolumeSlider.sliderOnPointerUp.AddListener(AdjustMasterVolume);
		musicVolumeSlider.sliderOnPointerUp.AddListener(AdjustMusicVolume);
		ambientVolumeSlider.sliderOnPointerUp.AddListener(AdjustAmbientVolume);
		effectsVolumeSlider.sliderOnPointerUp.AddListener(AdjustEffectsVolume);
		uiVolumeSlider.sliderOnPointerUp.AddListener(AdjustUIVolume);
		storyVolumeSlider.sliderOnPointerUp.AddListener(AdjustStoryVolume);
	}

	private void OnEnable()
	{
		int masterVolume = SaveData.Instance.Global.MasterVolume;
		int musicVolume = SaveData.Instance.Global.MusicVolume;
		int ambientVolume = SaveData.Instance.Global.AmbientVolume;
		int sFXVolume = SaveData.Instance.Global.SFXVolume;
		int uIVolume = SaveData.Instance.Global.UIVolume;
		int storyVolume = SaveData.Instance.Global.StoryVolume;
		masterVolumeSlider.SetAmount(masterVolume);
		musicVolumeSlider.SetAmount(musicVolume);
		ambientVolumeSlider.SetAmount(ambientVolume);
		effectsVolumeSlider.SetAmount(sFXVolume);
		uiVolumeSlider.SetAmount(uIVolume);
		storyVolumeSlider.SetAmount(storyVolume);
		muteInBackgroundToggle.SetValue(SaveData.Instance.Global.MuteAudioInBackground);
		AdjustMasterVolume(masterVolume, saveChange: false);
		AdjustMusicVolume(musicVolume, saveChange: false);
		AdjustAmbientVolume(musicVolume, saveChange: false);
		AdjustEffectsVolume(sFXVolume, saveChange: false);
		AdjustUIVolume(uIVolume, saveChange: false);
		AdjustStoryVolume(storyVolume, saveChange: false);
	}

	public void AdjustMasterVolume(float volume)
	{
		AdjustMasterVolume(volume, saveChange: true);
	}

	private void ToggleMuteAudioInBackground(bool active)
	{
		if (active != SaveData.Instance.Global.MuteAudioInBackground)
		{
			SaveData.Instance.Global.MuteAudioInBackground = active;
			SaveData.Instance.SaveGlobalData();
		}
	}

	private void AdjustMasterVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustMasterVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.MasterVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void AdjustMusicVolume(float volume)
	{
		AdjustMusicVolume(volume, saveChange: true);
	}

	private void AdjustMusicVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustMusicVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.MusicVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void AdjustAmbientVolume(float volume)
	{
		AdjustAmbientVolume(volume, saveChange: true);
	}

	private void AdjustAmbientVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustAmbientVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.AmbientVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void AdjustUIVolume(float volume)
	{
		AdjustUIVolume(volume, saveChange: true);
	}

	private void AdjustUIVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustUIVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.UIVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void AdjustStoryVolume(float volume)
	{
		AdjustStoryVolume(volume, saveChange: true);
	}

	private void AdjustStoryVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustStoryVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.StoryVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void AdjustEffectsVolume(float volume)
	{
		AdjustEffectsVolume(volume, saveChange: true);
	}

	private void AdjustEffectsVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustEffectsVolume(volume) & saveChange)
		{
			SaveData.Instance.Global.SFXVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}
}
