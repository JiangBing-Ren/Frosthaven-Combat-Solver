using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ClockStone;

[Serializable]
public class AudioCategory
{
	public string Name;

	private AudioCategory _parentCategory;

	private AudioFader _audioFader;

	[SerializeField]
	private string _parentCategoryName;

	public GameObject AudioObjectPrefab;

	public AudioItem[] AudioItems;

	[SerializeField]
	private float _volume = 1f;

	public AudioMixerGroup audioMixerGroup;

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
			_ApplyVolumeChange();
		}
	}

	public float VolumeTotal
	{
		get
		{
			_UpdateFadeTime();
			float num = audioFader.Get();
			if (parentCategory != null)
			{
				return parentCategory.VolumeTotal * _volume * num;
			}
			return _volume * num;
		}
	}

	public AudioCategory parentCategory
	{
		get
		{
			if (string.IsNullOrEmpty(_parentCategoryName))
			{
				return null;
			}
			if (_parentCategory == null)
			{
				if (audioController != null)
				{
					_parentCategory = audioController._GetCategory(_parentCategoryName);
				}
				else
				{
					Debug.LogWarning("_audioController == null");
				}
			}
			return _parentCategory;
		}
		set
		{
			_parentCategory = value;
			if (value != null)
			{
				_parentCategoryName = _parentCategory.Name;
			}
			else
			{
				_parentCategoryName = null;
			}
		}
	}

	private AudioFader audioFader
	{
		get
		{
			if (_audioFader == null)
			{
				_audioFader = new AudioFader();
			}
			return _audioFader;
		}
	}

	public AudioController audioController { get; set; }

	public bool isFadingIn => audioFader.isFadingIn;

	public bool isFadingOut => audioFader.isFadingOut;

	public bool isFadeOutComplete => audioFader.isFadingOutComplete;

	public AudioCategory(AudioController audioController)
	{
		this.audioController = audioController;
	}

	public GameObject GetAudioObjectPrefab()
	{
		if (AudioObjectPrefab != null)
		{
			return AudioObjectPrefab;
		}
		if (parentCategory != null)
		{
			return parentCategory.GetAudioObjectPrefab();
		}
		return audioController.AudioObjectPrefab;
	}

	public AudioMixerGroup GetAudioMixerGroup()
	{
		if (audioMixerGroup != null)
		{
			return audioMixerGroup;
		}
		if (parentCategory != null)
		{
			return parentCategory.GetAudioMixerGroup();
		}
		return null;
	}

	internal void _AnalyseAudioItems(Dictionary<string, AudioItem> audioItemsDict)
	{
		if (AudioItems == null)
		{
			return;
		}
		AudioItem[] audioItems = AudioItems;
		foreach (AudioItem audioItem in audioItems)
		{
			if (audioItem != null)
			{
				audioItem._Initialize(this);
				if (audioItemsDict != null && !audioItemsDict.TryAdd(audioItem.Name, audioItem))
				{
					Debug.LogWarning("Multiple audio items with name '" + audioItem.Name + "'", audioController);
				}
			}
		}
	}

	internal int _GetIndexOf(AudioItem audioItem)
	{
		if (AudioItems == null)
		{
			return -1;
		}
		for (int i = 0; i < AudioItems.Length; i++)
		{
			if (audioItem == AudioItems[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void _ApplyVolumeChange()
	{
		List<AudioObject> playingAudioObjects = AudioController.GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (_IsCategoryParentOf(audioObject.category, this))
			{
				audioObject._ApplyVolumeBoth();
			}
		}
	}

	private bool _IsCategoryParentOf(AudioCategory toTest, AudioCategory parent)
	{
		for (AudioCategory audioCategory = toTest; audioCategory != null; audioCategory = audioCategory.parentCategory)
		{
			if (audioCategory == parent)
			{
				return true;
			}
		}
		return false;
	}

	public void UnloadAllAudioClips()
	{
		for (int i = 0; i < AudioItems.Length; i++)
		{
			AudioItems[i].UnloadAudioClip();
		}
	}

	public void FadeIn(float fadeInTime, bool stopCurrentFadeOut = true)
	{
		_UpdateFadeTime();
		audioFader.FadeIn(fadeInTime, stopCurrentFadeOut);
	}

	public void FadeOut(float fadeOutLength, float startToFadeTime = 0f)
	{
		_UpdateFadeTime();
		audioFader.FadeOut(fadeOutLength, startToFadeTime);
	}

	private void _UpdateFadeTime()
	{
		audioFader.time = AudioController.systemTime;
	}
}
