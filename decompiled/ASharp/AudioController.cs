using System;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ClockStone;
using Libs.GameConsole;
using UnityEngine;

[AddComponentMenu("ClockStone/Audio/AudioController")]
public class AudioController : SingletonMonoBehaviour<AudioController>, ISerializationCallbackReceiver
{
	public const string AUDIO_TOOLKIT_VERSION = "9.0";

	public static bool LogToGameConsole;

	public GameObject AudioObjectPrefab;

	public bool Persistent;

	public bool UnloadAudioClipsOnDestroy;

	public bool UsePooledAudioObjects = true;

	public bool PlayWithZeroVolume;

	public bool EqualPowerCrossfade;

	public float musicCrossFadeTime;

	public float ambienceSoundCrossFadeTime;

	public bool specifyCrossFadeInAndOutSeperately;

	[SerializeField]
	private float _musicCrossFadeTime_In;

	[SerializeField]
	private float _musicCrossFadeTime_Out;

	[SerializeField]
	private float _ambienceSoundCrossFadeTime_In;

	[SerializeField]
	private float _ambienceSoundCrossFadeTime_Out;

	public AudioCategory[] AudioCategories;

	public Playlist[] musicPlaylists = new Playlist[1];

	public Action<Playlist> playlistFinishedEvent;

	[Obsolete]
	public string[] musicPlaylist;

	public bool crossfadePlaylist;

	public float delayBetweenPlaylistTracks = 1f;

	protected static PoolableReference<AudioObject> _currentMusicReference = new PoolableReference<AudioObject>();

	protected static PoolableReference<AudioObject> _currentAmbienceReference = new PoolableReference<AudioObject>();

	private string _currentPlaylistName;

	protected AudioListener _currentAudioListener;

	private static Transform _musicParent = null;

	private static Transform _ambienceParent = null;

	private bool _musicEnabled = true;

	private bool _ambienceSoundEnabled = true;

	private bool _soundMuted;

	private bool _categoriesValidated;

	[SerializeField]
	private bool _isAdditionalAudioController;

	[SerializeField]
	private bool _audioDisabled;

	private Dictionary<string, AudioItem> _audioItems;

	private static List<int> _playlistPlayed;

	private static bool _isPlaylistPlaying = false;

	[SerializeField]
	private float _volume = 1f;

	private static double _systemTime;

	private static double _lastSystemTime = -1.0;

	private static double _systemDeltaTime = -1.0;

	private static List<AudioController> _additionalControllerToRegister;

	private List<AudioController> _additionalAudioControllers;

	public AudioController_CurrentInspectorSelection _currentInspectorSelection = new AudioController_CurrentInspectorSelection();

	public bool DisableAudio
	{
		get
		{
			return _audioDisabled;
		}
		set
		{
			if (value != _audioDisabled)
			{
				_audioDisabled = value;
			}
		}
	}

	public bool isAdditionalAudioController
	{
		get
		{
			return _isAdditionalAudioController;
		}
		set
		{
			_isAdditionalAudioController = value;
		}
	}

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			if (value != _volume)
			{
				_volume = value;
				_ApplyVolumeChange();
			}
		}
	}

	public bool musicEnabled
	{
		get
		{
			return _musicEnabled;
		}
		set
		{
			if (_musicEnabled == value)
			{
				return;
			}
			_musicEnabled = value;
			if (!_currentMusic)
			{
				return;
			}
			if (value)
			{
				if (_currentMusic.IsPaused())
				{
					_currentMusic.Play();
				}
			}
			else
			{
				_currentMusic.Pause();
			}
		}
	}

	public bool ambienceSoundEnabled
	{
		get
		{
			return _ambienceSoundEnabled;
		}
		set
		{
			if (_ambienceSoundEnabled == value)
			{
				return;
			}
			_ambienceSoundEnabled = value;
			if (!_currentAmbienceSound)
			{
				return;
			}
			if (value)
			{
				if (_currentAmbienceSound.IsPaused())
				{
					_currentAmbienceSound.Play();
				}
			}
			else
			{
				_currentAmbienceSound.Pause();
			}
		}
	}

	public bool soundMuted
	{
		get
		{
			return _soundMuted;
		}
		set
		{
			_soundMuted = value;
			_ApplyVolumeChange();
		}
	}

	public float musicCrossFadeTime_In
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _musicCrossFadeTime_In;
			}
			return musicCrossFadeTime;
		}
		set
		{
			_musicCrossFadeTime_In = value;
		}
	}

	public float musicCrossFadeTime_Out
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _musicCrossFadeTime_Out;
			}
			return musicCrossFadeTime;
		}
		set
		{
			_musicCrossFadeTime_Out = value;
		}
	}

	public float ambienceSoundCrossFadeTime_In
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _ambienceSoundCrossFadeTime_In;
			}
			return ambienceSoundCrossFadeTime;
		}
		set
		{
			_ambienceSoundCrossFadeTime_In = value;
		}
	}

	public float ambienceSoundCrossFadeTime_Out
	{
		get
		{
			if (specifyCrossFadeInAndOutSeperately)
			{
				return _ambienceSoundCrossFadeTime_Out;
			}
			return ambienceSoundCrossFadeTime;
		}
		set
		{
			_ambienceSoundCrossFadeTime_Out = value;
		}
	}

	public static double systemTime => _systemTime;

	public static double systemDeltaTime => _systemDeltaTime;

	public static Transform musicParent
	{
		get
		{
			return _musicParent;
		}
		set
		{
			_musicParent = value;
		}
	}

	public static Transform ambienceParent
	{
		get
		{
			return _ambienceParent;
		}
		set
		{
			_ambienceParent = value;
		}
	}

	private static AudioObject _currentMusic
	{
		get
		{
			return _currentMusicReference.Get();
		}
		set
		{
			_currentMusicReference.Set(value, allowNonePoolable: true);
		}
	}

	private static AudioObject _currentAmbienceSound
	{
		get
		{
			return _currentAmbienceReference.Get();
		}
		set
		{
			_currentAmbienceReference.Set(value, allowNonePoolable: true);
		}
	}

	public override bool isSingletonObject => !_isAdditionalAudioController;

	public static AudioObject PlayMusic(string audioID, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, volume, delay, startTime);
	}

	public static AudioObject PlayMusic(string audioID, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, worldPosition, parentObj, volume, delay, startTime);
	}

	public static AudioObject PlayMusic(string audioID, Transform parentObj, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		_isPlaylistPlaying = false;
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusic(audioID, parentObj.position, parentObj, volume, delay, startTime);
	}

	public static bool StopMusic()
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopMusic(0f);
	}

	public static bool StopMusic(float fadeOut)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopMusic(fadeOut);
	}

	public static bool PauseMusic(float fadeOut = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PauseMusic(fadeOut);
	}

	public static bool IsMusicPaused()
	{
		if (_currentMusic != null)
		{
			return _currentMusic.IsPaused();
		}
		return false;
	}

	public static bool UnpauseMusic(float fadeIn = 0f)
	{
		if (!SingletonMonoBehaviour<AudioController>.Instance._musicEnabled)
		{
			return false;
		}
		if (_currentMusic != null && _currentMusic.IsPaused())
		{
			_currentMusic.Unpause(fadeIn);
			return true;
		}
		return false;
	}

	public static AudioObject PlayAmbienceSound(string audioID, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAmbienceSound(audioID, volume, delay, startTime);
	}

	public static AudioObject PlayAmbienceSound(string audioID, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAmbienceSound(audioID, worldPosition, parentObj, volume, delay, startTime);
	}

	public static AudioObject PlayAmbienceSound(string audioID, Transform parentObj, float volume = 1f, float delay = 0f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayAmbienceSound(audioID, parentObj.position, parentObj, volume, delay, startTime);
	}

	public static bool StopAmbienceSound()
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopAmbienceSound(0f);
	}

	public static bool StopAmbienceSound(float fadeOut)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._StopAmbienceSound(fadeOut);
	}

	public static bool PauseAmbienceSound(float fadeOut = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PauseAmbienceSound(fadeOut);
	}

	public static bool IsAmbienceSoundPaused()
	{
		if (_currentAmbienceSound != null)
		{
			return _currentAmbienceSound.IsPaused();
		}
		return false;
	}

	public static bool UnpauseAmbienceSound(float fadeIn = 0f)
	{
		if (!SingletonMonoBehaviour<AudioController>.Instance._ambienceSoundEnabled)
		{
			return false;
		}
		if (_currentAmbienceSound != null && _currentAmbienceSound.IsPaused())
		{
			_currentAmbienceSound.Unpause(fadeIn);
			return true;
		}
		return false;
	}

	public static int EnqueueMusic(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._EnqueueMusic(audioID);
	}

	private Playlist _GetCurrentPlaylist()
	{
		if (string.IsNullOrEmpty(_currentPlaylistName))
		{
			return null;
		}
		return GetPlaylistByName(_currentPlaylistName);
	}

	public Playlist GetPlaylistByName(string playlistName)
	{
		for (int i = 0; i < musicPlaylists.Length; i++)
		{
			if (playlistName == musicPlaylists[i].name)
			{
				return musicPlaylists[i];
			}
		}
		if (_additionalAudioControllers != null)
		{
			for (int j = 0; j < _additionalAudioControllers.Count; j++)
			{
				AudioController audioController = _additionalAudioControllers[j];
				for (int k = 0; k < audioController.musicPlaylists.Length; k++)
				{
					if (playlistName == audioController.musicPlaylists[k].name)
					{
						return audioController.musicPlaylists[k];
					}
				}
			}
		}
		return null;
	}

	public static string[] GetMusicPlaylist(string playlistName = null)
	{
		Playlist playlist = ((!string.IsNullOrEmpty(playlistName)) ? SingletonMonoBehaviour<AudioController>.Instance.GetPlaylistByName(playlistName) : SingletonMonoBehaviour<AudioController>.Instance._GetCurrentPlaylist());
		if (playlist == null)
		{
			return null;
		}
		string[] array = new string[(playlist.playlistItems != null) ? playlist.playlistItems.Length : 0];
		if (array.Length != 0)
		{
			Array.Copy(playlist.playlistItems, array, array.Length);
		}
		return array;
	}

	public static bool SetCurrentMusicPlaylist(string playlistName)
	{
		if (SingletonMonoBehaviour<AudioController>.Instance.GetPlaylistByName(playlistName) == null)
		{
			Debug.LogError("Playlist with name " + playlistName + " not found");
			return false;
		}
		SingletonMonoBehaviour<AudioController>.Instance._currentPlaylistName = playlistName;
		return true;
	}

	public static AudioObject PlayMusicPlaylist(string playlistName = null)
	{
		if (!string.IsNullOrEmpty(playlistName) && !SetCurrentMusicPlaylist(playlistName))
		{
			return null;
		}
		return SingletonMonoBehaviour<AudioController>.Instance._PlayMusicPlaylist();
	}

	public static AudioObject PlayNextMusicOnPlaylist()
	{
		if (IsPlaylistPlaying())
		{
			return SingletonMonoBehaviour<AudioController>.Instance._PlayNextMusicOnPlaylist(0f);
		}
		return null;
	}

	public static AudioObject PlayPreviousMusicOnPlaylist()
	{
		if (IsPlaylistPlaying())
		{
			return SingletonMonoBehaviour<AudioController>.Instance._PlayPreviousMusicOnPlaylist(0f);
		}
		return null;
	}

	public static bool IsPlaylistPlaying()
	{
		if (_isPlaylistPlaying)
		{
			if (!_currentMusic)
			{
				_isPlaylistPlaying = false;
				return false;
			}
			return true;
		}
		return false;
	}

	public static void ClearPlaylists()
	{
		SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists = null;
	}

	public static void AddPlaylist(string playlistName, string[] audioItemIDs)
	{
		Playlist elToAdd = new Playlist(playlistName, audioItemIDs);
		ArrayHelper.AddArrayElement(ref SingletonMonoBehaviour<AudioController>.Instance.musicPlaylists, elToAdd);
	}

	public static void PreloadAudio(string audioID)
	{
		SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID).LoadAudioClip();
	}

	public static AudioObject Play(string audioID, GameObject source = null)
	{
		AudioListener currentAudioListener = GetCurrentAudioListener();
		if (currentAudioListener == null)
		{
			Debug.LogWarning("No AudioListener found in the scene");
			return null;
		}
		return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, 1f, 0f, 0f, source);
	}

	public static AudioObject Play(string audioID, float volume, float delay = 0f, float startTime = 0f, GameObject source = null)
	{
		AudioListener currentAudioListener = GetCurrentAudioListener();
		if (currentAudioListener == null)
		{
			Debug.LogWarning("No AudioListener found in the scene");
			return null;
		}
		return Play(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime, source);
	}

	public static AudioObject Play(string audioID, Transform parentObj, GameObject source = null, bool attachToParent = true)
	{
		return Play(audioID, parentObj.position, attachToParent ? parentObj : null, 1f, 0f, 0f, source);
	}

	public static AudioObject Play(string audioID, Transform parentObj, float volume, float delay = 0f, float startTime = 0f, GameObject source = null)
	{
		return Play(audioID, parentObj.position, parentObj, volume, delay, startTime, source);
	}

	public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj = null, GameObject source = null)
	{
		int sourceID = ((!(source == null)) ? source.GetInstanceID() : 0);
		return SingletonMonoBehaviour<AudioController>.Instance._PlayEx(audioID, AudioChannelType.Default, 1f, worldPosition, parentObj, 0f, 0f, playWithoutAudioObject: false, 0.0, null, 1f, sourceID);
	}

	public static AudioObject Play(string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay = 0f, float startTime = 0f, GameObject source = null)
	{
		int sourceID = ((!(source == null)) ? source.GetInstanceID() : 0);
		return SingletonMonoBehaviour<AudioController>.Instance._PlayEx(audioID, AudioChannelType.Default, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject: false, 0.0, null, 1f, sourceID);
	}

	public static AudioObject PlayScheduled(string audioID, double dspTime, Vector3 worldPosition, Transform parentObj = null, float volume = 1f, float startTime = 0f)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._PlayEx(audioID, AudioChannelType.Default, volume, worldPosition, parentObj, 0f, startTime, playWithoutAudioObject: false, dspTime);
	}

	public static AudioObject PlayAfter(string audioID, AudioObject playingAudio, double deltaDspTime = 0.0, float volume = 1f, float startTime = 0f)
	{
		double num = AudioSettings.dspTime;
		if (playingAudio.IsPlaying())
		{
			num += (double)playingAudio.timeUntilEnd;
		}
		num += deltaDspTime;
		return PlayScheduled(audioID, num, playingAudio.transform.position, playingAudio.transform.parent, volume, startTime);
	}

	public static bool Stop(string audioID, float fadeOutLength, int sourceID = 0)
	{
		if (SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID) == null)
		{
			Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
			return false;
		}
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(audioID, includePausedAudio: false, sourceID);
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (fadeOutLength < 0f)
			{
				audioObject.Stop();
			}
			else
			{
				audioObject.Stop(fadeOutLength);
			}
		}
		return playingAudioObjects.Count > 0;
	}

	public static bool Stop(string audioID)
	{
		return Stop(audioID, -1f);
	}

	public static void StopAll(float fadeOutLength)
	{
		SingletonMonoBehaviour<AudioController>.Instance._StopMusic(fadeOutLength);
		SingletonMonoBehaviour<AudioController>.Instance._StopAmbienceSound(fadeOutLength);
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null)
			{
				audioObject.Stop(fadeOutLength);
			}
		}
	}

	public static void StopAll()
	{
		StopAll(-1f);
	}

	public static void PauseAll(float fadeOutLength = 0f)
	{
		SingletonMonoBehaviour<AudioController>.Instance._PauseMusic(fadeOutLength);
		SingletonMonoBehaviour<AudioController>.Instance._PauseAmbienceSound(fadeOutLength);
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null)
			{
				audioObject.Pause(fadeOutLength);
			}
		}
	}

	public static void UnpauseAll(float fadeInLength = 0f)
	{
		UnpauseMusic(fadeInLength);
		UnpauseAmbienceSound(fadeInLength);
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(includePausedAudio: true);
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null && audioObject.IsPaused() && (instance.musicEnabled || !(_currentMusic == audioObject)) && (instance.ambienceSoundEnabled || !(_currentAmbienceSound == audioObject)))
			{
				audioObject.Unpause(fadeInLength);
			}
		}
	}

	public static void PauseCategory(string categoryName, float fadeOutLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			PauseMusic(fadeOutLength);
		}
		if (_currentAmbienceSound != null && _currentAmbienceSound.category.Name == categoryName)
		{
			PauseAmbienceSound(fadeOutLength);
		}
		List<AudioObject> playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
		for (int i = 0; i < playingAudioObjectsInCategory.Count; i++)
		{
			playingAudioObjectsInCategory[i].Pause(fadeOutLength);
		}
	}

	public static void UnpauseCategory(string categoryName, float fadeInLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			UnpauseMusic(fadeInLength);
		}
		if (_currentAmbienceSound != null && _currentAmbienceSound.category.Name == categoryName)
		{
			UnpauseAmbienceSound(fadeInLength);
		}
		List<AudioObject> playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName, includePausedAudio: true);
		for (int i = 0; i < playingAudioObjectsInCategory.Count; i++)
		{
			AudioObject audioObject = playingAudioObjectsInCategory[i];
			if (audioObject.IsPaused())
			{
				audioObject.Unpause(fadeInLength);
			}
		}
	}

	public static void StopCategory(string categoryName, float fadeOutLength = 0f)
	{
		if (_currentMusic != null && _currentMusic.category.Name == categoryName)
		{
			StopMusic(fadeOutLength);
		}
		if (_currentAmbienceSound != null && _currentAmbienceSound.category.Name == categoryName)
		{
			StopAmbienceSound(fadeOutLength);
		}
		List<AudioObject> playingAudioObjectsInCategory = GetPlayingAudioObjectsInCategory(categoryName);
		for (int i = 0; i < playingAudioObjectsInCategory.Count; i++)
		{
			playingAudioObjectsInCategory[i].Stop(fadeOutLength);
		}
	}

	public static void RefreshTimescale()
	{
		foreach (AudioObject playingAudioObject in GetPlayingAudioObjects())
		{
			if (!IsImmuneToTimeSacle(playingAudioObject.category))
			{
				playingAudioObject.RefreshTimescale();
			}
		}
	}

	public static bool IsImmuneToTimeSacle(AudioCategory category)
	{
		AudioCategory audioCategory = GetCurrentAmbienceSound()?.category;
		AudioCategory audioCategory2 = GetCurrentMusic()?.category;
		if (category == audioCategory || category == audioCategory2 || category.Name == "Music" || category.Name == "Ambience" || category.Name == "UI")
		{
			return true;
		}
		return false;
	}

	public static bool IsPlaying(string audioID)
	{
		return GetPlayingAudioObjects(audioID).Count > 0;
	}

	public static List<AudioObject> GetPlayingAudioObjects(string audioID, bool includePausedAudio = false, int sourceID = 0)
	{
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		List<AudioObject> list = new List<AudioObject>(playingAudioObjects.Count);
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null && audioObject.audioID.ToLower() == audioID.ToLower() && (sourceID == 0 || audioObject.sourceID == sourceID))
			{
				list.Add(audioObject);
			}
		}
		return list;
	}

	public static List<AudioObject> GetPlayingAudioObjectsInCategory(string categoryName, bool includePausedAudio = false)
	{
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		List<AudioObject> list = new List<AudioObject>(playingAudioObjects.Count);
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null && audioObject.DoesBelongToCategory(categoryName))
			{
				list.Add(audioObject);
			}
		}
		return list;
	}

	public static List<AudioObject> GetPlayingAudioObjects(bool includePausedAudio = false)
	{
		object[] allOfType = RegisteredComponentController.GetAllOfType(typeof(AudioObject));
		List<AudioObject> list = new List<AudioObject>(allOfType.Length);
		for (int i = 0; i < allOfType.Length; i++)
		{
			AudioObject audioObject = (AudioObject)allOfType[i];
			if (audioObject.IsPlaying() || (includePausedAudio && audioObject.IsPaused()))
			{
				list.Add(audioObject);
			}
		}
		return list;
	}

	public static int GetPlayingAudioObjectsCount(string audioID, bool includePausedAudio = false)
	{
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(includePausedAudio);
		int num = 0;
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null && audioObject.audioID.ToLower() == audioID.ToLower())
			{
				num++;
			}
		}
		return num;
	}

	public static void EnableMusic(bool b)
	{
		SingletonMonoBehaviour<AudioController>.Instance.musicEnabled = b;
	}

	public static void EnableAmbienceSound(bool b)
	{
		SingletonMonoBehaviour<AudioController>.Instance.ambienceSoundEnabled = b;
	}

	public static void MuteSound(bool b)
	{
		SingletonMonoBehaviour<AudioController>.Instance.soundMuted = b;
	}

	public static bool IsMusicEnabled()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.musicEnabled;
	}

	public static bool IsAmbienceSoundEnabled()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.ambienceSoundEnabled;
	}

	public static bool IsSoundMuted()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.soundMuted;
	}

	public static AudioListener GetCurrentAudioListener()
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		if (instance._currentAudioListener != null && instance._currentAudioListener.gameObject == null)
		{
			instance._currentAudioListener = null;
		}
		if (instance._currentAudioListener == null)
		{
			instance._currentAudioListener = (AudioListener)UnityEngine.Object.FindObjectOfType(typeof(AudioListener));
		}
		return instance._currentAudioListener;
	}

	public static AudioObject GetCurrentMusic()
	{
		return _currentMusic;
	}

	public static AudioObject GetCurrentAmbienceSound()
	{
		return _currentAmbienceSound;
	}

	public static AudioCategory GetCategory(string name)
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		AudioCategory audioCategory = instance._GetCategory(name);
		if (audioCategory != null)
		{
			return audioCategory;
		}
		if (instance._additionalAudioControllers != null)
		{
			for (int i = 0; i < instance._additionalAudioControllers.Count; i++)
			{
				audioCategory = instance._additionalAudioControllers[i]._GetCategory(name);
				if (audioCategory != null)
				{
					return audioCategory;
				}
			}
		}
		return null;
	}

	public static void SetCategoryVolume(string name, float volume)
	{
		List<AudioCategory> list = _GetAllCategories(name);
		if (list.Count == 0)
		{
			Debug.LogWarning("No audio category with name " + name);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Volume = volume;
		}
	}

	public static float GetCategoryVolume(string name)
	{
		AudioCategory category = GetCategory(name);
		if (category != null)
		{
			return category.Volume;
		}
		Debug.LogWarning("No audio category with name " + name);
		return 0f;
	}

	public static void FadeOutCategory(string name, float fadeOutLength, float startToFadeTime = 0f)
	{
		List<AudioCategory> list = _GetAllCategories(name);
		if (list.Count == 0)
		{
			Debug.LogWarning("No audio category with name " + name);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].FadeOut(fadeOutLength, startToFadeTime);
		}
	}

	public static void FadeInCategory(string name, float fadeInTime, bool stopCurrentFadeOut = true)
	{
		List<AudioCategory> list = _GetAllCategories(name);
		if (list.Count == 0)
		{
			Debug.LogWarning("No audio category with name " + name);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].FadeIn(fadeInTime, stopCurrentFadeOut);
		}
	}

	public static void SetGlobalVolume(float volume)
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		instance.Volume = volume;
		if (instance._additionalAudioControllers != null)
		{
			for (int i = 0; i < instance._additionalAudioControllers.Count; i++)
			{
				instance._additionalAudioControllers[i].Volume = volume;
			}
		}
	}

	public static float GetGlobalVolume()
	{
		return SingletonMonoBehaviour<AudioController>.Instance.Volume;
	}

	public static AudioCategory NewCategory(string categoryName)
	{
		int num = ((SingletonMonoBehaviour<AudioController>.Instance.AudioCategories != null) ? SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length : 0);
		AudioCategory[] audioCategories = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories;
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories = new AudioCategory[num + 1];
		if (num > 0)
		{
			audioCategories.CopyTo(SingletonMonoBehaviour<AudioController>.Instance.AudioCategories, 0);
		}
		AudioCategory audioCategory = new AudioCategory(SingletonMonoBehaviour<AudioController>.Instance);
		audioCategory.Name = categoryName;
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[num] = audioCategory;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
		return audioCategory;
	}

	public static void RemoveCategory(string categoryName)
	{
		int num = -1;
		int num2 = ((SingletonMonoBehaviour<AudioController>.Instance.AudioCategories != null) ? SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length : 0);
		for (int i = 0; i < num2; i++)
		{
			if (SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[i].Name == categoryName)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("AudioCategory does not exist: " + categoryName);
			return;
		}
		AudioCategory[] array = new AudioCategory[SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length - 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[i];
		}
		for (int i = num + 1; i < SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Length; i++)
		{
			array[i - 1] = SingletonMonoBehaviour<AudioController>.Instance.AudioCategories[i];
		}
		SingletonMonoBehaviour<AudioController>.Instance.AudioCategories = array;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
	}

	public static void AddToCategory(AudioCategory category, AudioItem audioItem)
	{
		int num = ((category.AudioItems != null) ? category.AudioItems.Length : 0);
		AudioItem[] audioItems = category.AudioItems;
		category.AudioItems = new AudioItem[num + 1];
		if (num > 0)
		{
			audioItems.CopyTo(category.AudioItems, 0);
		}
		category.AudioItems[num] = audioItem;
		SingletonMonoBehaviour<AudioController>.Instance._InvalidateCategories();
	}

	public static AudioItem AddToCategory(AudioCategory category, AudioClip audioClip, string audioID)
	{
		AudioItem audioItem = new AudioItem();
		audioItem.Name = audioID;
		audioItem.subItems = new AudioSubItem[1];
		AudioSubItem audioSubItem = new AudioSubItem();
		audioSubItem.Clip = audioClip;
		audioItem.subItems[0] = audioSubItem;
		AddToCategory(category, audioItem);
		return audioItem;
	}

	public static bool RemoveAudioItem(string audioID)
	{
		AudioItem audioItem = SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID);
		if (audioItem != null)
		{
			int num = audioItem.category._GetIndexOf(audioItem);
			if (num < 0)
			{
				return false;
			}
			AudioItem[] audioItems = audioItem.category.AudioItems;
			AudioItem[] array = new AudioItem[audioItems.Length - 1];
			for (int i = 0; i < num; i++)
			{
				array[i] = audioItems[i];
			}
			for (int i = num + 1; i < audioItems.Length; i++)
			{
				array[i - 1] = audioItems[i];
			}
			audioItem.category.AudioItems = array;
			if (SingletonMonoBehaviour<AudioController>.Instance._categoriesValidated)
			{
				SingletonMonoBehaviour<AudioController>.Instance._audioItems.Remove(audioID);
			}
			return true;
		}
		return false;
	}

	public static bool IsValidAudioID(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID) != null;
	}

	public static AudioItem GetAudioItem(string audioID)
	{
		return SingletonMonoBehaviour<AudioController>.Instance._GetAudioItem(audioID);
	}

	public static void DetachAllAudios(GameObject gameObjectWithAudios)
	{
		AudioObject[] componentsInChildren = gameObjectWithAudios.GetComponentsInChildren<AudioObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.SetParent(null);
		}
	}

	public static float GetAudioItemMaxDistance(string audioID)
	{
		AudioItem audioItem = GetAudioItem(audioID);
		if (audioItem.overrideAudioSourceSettings)
		{
			return audioItem.audioSource_MaxDistance;
		}
		return audioItem.category.GetAudioObjectPrefab().GetComponent<AudioSource>().maxDistance;
	}

	public void UnloadAllAudioClips()
	{
		for (int i = 0; i < AudioCategories.Length; i++)
		{
			AudioCategories[i].UnloadAllAudioClips();
		}
	}

	private void _ApplyVolumeChange()
	{
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(includePausedAudio: true);
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			if (audioObject != null)
			{
				audioObject._ApplyVolumeBoth();
			}
		}
	}

	internal AudioItem _GetAudioItem(string audioID)
	{
		_ValidateCategories();
		if (_audioItems.TryGetValue(audioID, out var value))
		{
			return value;
		}
		return null;
	}

	public string[] GetAllAudioKeys()
	{
		return _audioItems.Keys.ToArray();
	}

	protected AudioObject _PlayMusic(string audioID, float volume, float delay, float startTime)
	{
		if (_musicParent == null)
		{
			AudioListener currentAudioListener = GetCurrentAudioListener();
			if (currentAudioListener == null)
			{
				Debug.LogWarning("No AudioListener found in the scene");
				return null;
			}
			return _PlayMusic(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
		}
		return _PlayMusic(audioID, _musicParent.position, _musicParent, volume, delay, startTime);
	}

	protected AudioObject _PlayAmbienceSound(string audioID, float volume, float delay, float startTime)
	{
		if (_ambienceParent == null)
		{
			AudioListener currentAudioListener = GetCurrentAudioListener();
			if (currentAudioListener == null)
			{
				Debug.LogWarning("No AudioListener found in the scene");
				return null;
			}
			return _PlayAmbienceSound(audioID, currentAudioListener.transform.position + currentAudioListener.transform.forward, null, volume, delay, startTime);
		}
		return _PlayAmbienceSound(audioID, _ambienceParent.position, _ambienceParent, volume, delay, startTime);
	}

	protected bool _StopMusic(float fadeOutLength)
	{
		if (_currentMusic != null)
		{
			_currentMusic.Stop(fadeOutLength);
			_currentMusic = null;
			return true;
		}
		return false;
	}

	protected bool _PauseMusic(float fadeOut)
	{
		if (_currentMusic != null)
		{
			_currentMusic.Pause(fadeOut);
			return true;
		}
		return false;
	}

	protected bool _StopAmbienceSound(float fadeOutLength)
	{
		if (_currentAmbienceSound != null)
		{
			_currentAmbienceSound.Stop(fadeOutLength);
			_currentAmbienceSound = null;
			return true;
		}
		return false;
	}

	protected bool _PauseAmbienceSound(float fadeOut)
	{
		if (_currentAmbienceSound != null)
		{
			_currentAmbienceSound.Pause(fadeOut);
			return true;
		}
		return false;
	}

	protected AudioObject _PlayMusic(string audioID, Vector3 position, Transform parentObj, float volume, float delay, float startTime)
	{
		bool flag;
		if (_currentMusic != null && _currentMusic.IsPlaying())
		{
			flag = true;
			_currentMusic.Stop(musicCrossFadeTime_Out);
		}
		else
		{
			flag = false;
		}
		if (musicCrossFadeTime_In <= 0f)
		{
			flag = false;
		}
		_currentMusic = _PlayEx(audioID, AudioChannelType.Music, volume, position, parentObj, delay, startTime, playWithoutAudioObject: false, 0.0, null, (!flag) ? 1 : 0);
		if (flag && (bool)_currentMusic)
		{
			_currentMusic.FadeIn(musicCrossFadeTime_In);
		}
		if (!IsMusicEnabled())
		{
			_currentMusic.Pause();
		}
		return _currentMusic;
	}

	protected AudioObject _PlayAmbienceSound(string audioID, Vector3 position, Transform parentObj, float volume, float delay, float startTime)
	{
		bool flag;
		if (_currentAmbienceSound != null && _currentAmbienceSound.IsPlaying())
		{
			flag = true;
			_currentAmbienceSound.Stop(ambienceSoundCrossFadeTime_Out);
		}
		else
		{
			flag = false;
		}
		if (ambienceSoundCrossFadeTime_In <= 0f)
		{
			flag = false;
		}
		_currentAmbienceSound = _PlayEx(audioID, AudioChannelType.Ambience, volume, position, parentObj, delay, startTime, playWithoutAudioObject: false, 0.0, null, (!flag) ? 1 : 0);
		if (flag && (bool)_currentAmbienceSound)
		{
			_currentAmbienceSound.FadeIn(ambienceSoundCrossFadeTime_In);
		}
		if (!IsAmbienceSoundEnabled())
		{
			_currentAmbienceSound.Pause();
		}
		return _currentAmbienceSound;
	}

	protected int _EnqueueMusic(string audioID)
	{
		Playlist playlist = _GetCurrentPlaylist();
		int num = ((playlist == null) ? 1 : (musicPlaylists.Length + 1));
		string[] array = new string[num];
		playlist?.playlistItems.CopyTo(array, 0);
		array[num - 1] = audioID;
		playlist.playlistItems = array;
		return num;
	}

	protected AudioObject _PlayMusicPlaylist()
	{
		_ResetLastPlayedList();
		return _PlayNextMusicOnPlaylist(0f);
	}

	private AudioObject _PlayMusicTrackWithID(int nextTrack, float delay, bool addToPlayedList)
	{
		if (nextTrack < 0)
		{
			return null;
		}
		_playlistPlayed.Add(nextTrack);
		_isPlaylistPlaying = true;
		Playlist playlist = _GetCurrentPlaylist();
		AudioObject audioObject = _PlayMusic(playlist.playlistItems[nextTrack], 1f, delay, 0f);
		if (audioObject != null)
		{
			audioObject._isCurrentPlaylistTrack = true;
			audioObject.primaryAudioSource.loop = false;
		}
		return audioObject;
	}

	internal AudioObject _PlayNextMusicOnPlaylist(float delay)
	{
		int nextTrack = _GetNextMusicTrack();
		return _PlayMusicTrackWithID(nextTrack, delay, addToPlayedList: true);
	}

	internal AudioObject _PlayPreviousMusicOnPlaylist(float delay)
	{
		int nextTrack = _GetPreviousMusicTrack();
		return _PlayMusicTrackWithID(nextTrack, delay, addToPlayedList: false);
	}

	private void _ResetLastPlayedList()
	{
		_playlistPlayed.Clear();
	}

	protected int _GetNextMusicTrack()
	{
		Playlist playlist = _GetCurrentPlaylist();
		if (playlist == null || playlist.playlistItems == null)
		{
			Debug.LogWarning("There is no current playlist set");
			return -1;
		}
		if (playlist.playlistItems.Length == 1)
		{
			if (_playlistPlayed.Count == 0)
			{
				return 0;
			}
			if (playlist.loopPlaylist)
			{
				return 0;
			}
			return -1;
		}
		if (playlist.shufflePlaylist)
		{
			return _GetNextMusicTrackShuffled();
		}
		return _GetNextMusicTrackInOrder();
	}

	protected int _GetPreviousMusicTrack()
	{
		Playlist playlist = _GetCurrentPlaylist();
		if (playlist.playlistItems.Length == 1)
		{
			return 0;
		}
		if (playlist.shufflePlaylist)
		{
			return _GetPreviousMusicTrackShuffled();
		}
		return _GetPreviousMusicTrackInOrder();
	}

	private int _GetPreviousMusicTrackShuffled()
	{
		if (_playlistPlayed.Count >= 2)
		{
			int result = _playlistPlayed[_playlistPlayed.Count - 2];
			_RemoveLastPlayedOnList();
			_RemoveLastPlayedOnList();
			return result;
		}
		return -1;
	}

	private void _RemoveLastPlayedOnList()
	{
		_playlistPlayed.RemoveAt(_playlistPlayed.Count - 1);
	}

	private int _GetNextMusicTrackShuffled()
	{
		HashSet<int> hashSet = new HashSet<int>();
		int num = _playlistPlayed.Count;
		Playlist playlist = _GetCurrentPlaylist();
		if (playlist.loopPlaylist)
		{
			int num2 = Mathf.Clamp(playlist.playlistItems.Length / 4, 2, 10);
			if (num > playlist.playlistItems.Length - num2)
			{
				num = playlist.playlistItems.Length - num2;
				if (num < 1)
				{
					num = 1;
				}
			}
		}
		else if (num >= playlist.playlistItems.Length)
		{
			return -1;
		}
		for (int i = 0; i < num; i++)
		{
			hashSet.Add(_playlistPlayed[_playlistPlayed.Count - 1 - i]);
		}
		List<int> list = new List<int>();
		for (int j = 0; j < playlist.playlistItems.Length; j++)
		{
			if (!hashSet.Contains(j))
			{
				list.Add(j);
			}
		}
		if (playlist.forcedFirstTracks > 0 && playlist.forcedFirstTracks > _playlistPlayed.Count)
		{
			return list[0];
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private int _GetNextMusicTrackInOrder()
	{
		if (_playlistPlayed.Count == 0)
		{
			return 0;
		}
		int num = _playlistPlayed[_playlistPlayed.Count - 1] + 1;
		Playlist playlist = _GetCurrentPlaylist();
		if (num >= playlist.playlistItems.Length)
		{
			if (!playlist.loopPlaylist)
			{
				return -1;
			}
			num = 0;
		}
		return num;
	}

	private int _GetPreviousMusicTrackInOrder()
	{
		Playlist playlist = _GetCurrentPlaylist();
		if (_playlistPlayed.Count < 2)
		{
			if (playlist.loopPlaylist)
			{
				return playlist.playlistItems.Length - 1;
			}
			return -1;
		}
		int num = _playlistPlayed[_playlistPlayed.Count - 1] - 1;
		_RemoveLastPlayedOnList();
		_RemoveLastPlayedOnList();
		if (num < 0)
		{
			if (!playlist.loopPlaylist)
			{
				return -1;
			}
			num = playlist.playlistItems.Length - 1;
		}
		return num;
	}

	protected AudioObject _PlayEx(string audioID, AudioChannelType channel, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, double dspTime = 0.0, AudioObject useExistingAudioObject = null, float startVolumeMultiplier = 1f, int sourceID = 0)
	{
		if (_audioDisabled)
		{
			return null;
		}
		AudioItem audioItem = _GetAudioItem(audioID);
		if (audioItem == null)
		{
			Debug.LogWarning("Audio item with name '" + audioID + "' does not exist");
			return null;
		}
		if (audioItem._lastPlayedTime > 0.0 && dspTime == 0.0 && systemTime - audioItem._lastPlayedTime < (double)audioItem.MinTimeBetweenPlayCalls)
		{
			return null;
		}
		if (audioItem.MaxInstanceCount > 0)
		{
			List<AudioObject> playingAudioObjects = GetPlayingAudioObjects(audioID);
			bool flag = playingAudioObjects.Count >= audioItem.MaxInstanceCount;
			if (flag && !audioItem.InterrupOnExceed)
			{
				return null;
			}
			if (flag)
			{
				bool flag2 = playingAudioObjects.Count > audioItem.MaxInstanceCount;
				AudioObject audioObject = null;
				for (int i = 0; i < playingAudioObjects.Count; i++)
				{
					if ((flag2 || !playingAudioObjects[i].isFadingOut) && (audioObject == null || playingAudioObjects[i].startedPlayingAtTime < audioObject.startedPlayingAtTime))
					{
						audioObject = playingAudioObjects[i];
					}
				}
				if (audioObject != null)
				{
					audioObject.Stop(flag2 ? 0f : 0.2f);
				}
			}
		}
		return PlayAudioItem(audioItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObject, dspTime, channel, startVolumeMultiplier, sourceID);
	}

	private static bool _IsBlockedByPlayingAudio(AudioItem sndItem, int sourceID)
	{
		List<AudioObject> playingAudioObjects = GetPlayingAudioObjects();
		for (int i = 0; i < playingAudioObjects.Count; i++)
		{
			AudioObject audioObject = playingAudioObjects[i];
			AudioItem audioItem = ((audioObject != null) ? audioObject.audioItem : null);
			if (audioItem == null || audioItem.blockEvents == null)
			{
				continue;
			}
			AudioStopEvent[] blockEvents = audioItem.blockEvents;
			foreach (AudioStopEvent audioStopEvent in blockEvents)
			{
				if (string.Equals(audioStopEvent.AudioEvent, sndItem.Name, StringComparison.OrdinalIgnoreCase))
				{
					if (audioStopEvent.NotificationLevel == AudioStopEvent.Level.Global)
					{
						return true;
					}
					if (sourceID != 0 && audioObject.sourceID == sourceID)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public AudioObject PlayAudioItem(AudioItem sndItem, float volume, Vector3 worldPosition, Transform parentObj = null, float delay = 0f, float startTime = 0f, bool playWithoutAudioObject = false, AudioObject useExistingAudioObj = null, double dspTime = 0.0, AudioChannelType channel = AudioChannelType.Default, float startVolumeMultiplier = 1f, int sourceID = 0)
	{
		if (_IsBlockedByPlayingAudio(sndItem, sourceID))
		{
			return null;
		}
		AudioStopEvent[] stopEvents = sndItem.stopEvents;
		foreach (AudioStopEvent audioStopEvent in stopEvents)
		{
			if (audioStopEvent.NotificationLevel == AudioStopEvent.Level.Global)
			{
				Stop(audioStopEvent.AudioEvent);
			}
			else if (sourceID != 0)
			{
				Stop(audioStopEvent.AudioEvent, -1f, sourceID);
			}
		}
		AudioObject audioObject = null;
		sndItem._lastPlayedTime = systemTime;
		AudioSubItem[] array = AudioControllerHelper._ChooseSubItems(sndItem, useExistingAudioObj);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		foreach (AudioSubItem audioSubItem in array)
		{
			if (audioSubItem == null)
			{
				continue;
			}
			AudioObject audioObject2 = PlayAudioSubItem(audioSubItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, useExistingAudioObj, dspTime, channel, startVolumeMultiplier);
			if (!audioObject2)
			{
				continue;
			}
			audioObject = audioObject2;
			audioObject.sourceID = sourceID;
			audioObject.audioID = sndItem.Name;
			if (sndItem.overrideAudioSourceSettings)
			{
				audioObject2._audioSource_MinDistance_Saved = audioObject2.primaryAudioSource.minDistance;
				audioObject2._audioSource_MaxDistance_Saved = audioObject2.primaryAudioSource.maxDistance;
				audioObject2._audioSource_SpatialBlend_Saved = audioObject2.primaryAudioSource.spatialBlend;
				audioObject2.primaryAudioSource.minDistance = sndItem.audioSource_MinDistance;
				audioObject2.primaryAudioSource.maxDistance = sndItem.audioSource_MaxDistance;
				audioObject2.primaryAudioSource.spatialBlend = sndItem.spatialBlend;
				if (audioObject2.secondaryAudioSource != null)
				{
					audioObject2.secondaryAudioSource.minDistance = sndItem.audioSource_MinDistance;
					audioObject2.secondaryAudioSource.maxDistance = sndItem.audioSource_MaxDistance;
					audioObject2.secondaryAudioSource.spatialBlend = sndItem.spatialBlend;
				}
			}
		}
		return audioObject;
	}

	internal AudioCategory _GetCategory(string name)
	{
		for (int i = 0; i < AudioCategories.Length; i++)
		{
			AudioCategory audioCategory = AudioCategories[i];
			if (audioCategory.Name == name)
			{
				return audioCategory;
			}
		}
		return null;
	}

	private void Update()
	{
		if (!_isAdditionalAudioController)
		{
			_UpdateSystemTime();
		}
	}

	private static void _UpdateSystemTime()
	{
		double timeSinceLaunch = SystemTime.timeSinceLaunch;
		if (_lastSystemTime >= 0.0)
		{
			_systemDeltaTime = timeSinceLaunch - _lastSystemTime;
			if (_systemDeltaTime > (double)(Time.maximumDeltaTime + 0.01f))
			{
				_systemDeltaTime = (Timekeeper.instance ? Timekeeper.instance.m_GlobalClock.deltaTime : Time.deltaTime);
			}
			_systemTime += _systemDeltaTime;
		}
		else
		{
			_systemDeltaTime = 0.0;
			_systemTime = 0.0;
		}
		_lastSystemTime = timeSinceLaunch;
	}

	protected override void Awake()
	{
		base.Awake();
		if (Persistent)
		{
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	private void OnEnable()
	{
		if (isAdditionalAudioController)
		{
			if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<AudioController>.Instance._RegisterAdditionalAudioController(this);
				return;
			}
			if (_additionalControllerToRegister == null)
			{
				_additionalControllerToRegister = new List<AudioController>();
			}
			_additionalControllerToRegister.Add(this);
		}
		else
		{
			if (_additionalControllerToRegister == null)
			{
				return;
			}
			for (int i = 0; i < _additionalControllerToRegister.Count; i++)
			{
				AudioController audioController = _additionalControllerToRegister[i];
				if ((bool)audioController && audioController.enabled)
				{
					SingletonMonoBehaviour<AudioController>.Instance._RegisterAdditionalAudioController(audioController);
				}
			}
			_additionalControllerToRegister = null;
		}
	}

	private void OnDisable()
	{
		if (isAdditionalAudioController && (bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			SingletonMonoBehaviour<AudioController>.Instance._UnregisterAdditionalAudioController(this);
		}
	}

	protected override void OnDestroy()
	{
		if (UnloadAudioClipsOnDestroy)
		{
			UnloadAllAudioClips();
		}
		base.OnDestroy();
	}

	private void AwakeSingleton()
	{
		_UpdateSystemTime();
		if (AudioObjectPrefab == null)
		{
			Debug.LogError("No AudioObject prefab specified in AudioController. To make your own AudioObject prefab create an empty game object, add Unity's AudioSource, the AudioObject script, and the PoolableObject script (if pooling is wanted ). Then create a prefab and set it in the AudioController.");
		}
		else
		{
			_ValidateAudioObjectPrefab(AudioObjectPrefab);
		}
		_ValidateCategories();
		if (_playlistPlayed == null)
		{
			_playlistPlayed = new List<int>();
			_isPlaylistPlaying = false;
		}
		_SetDefaultCurrentPlaylist();
	}

	protected void _ValidateCategories()
	{
		if (!_categoriesValidated)
		{
			InitializeAudioItems();
			_categoriesValidated = true;
		}
	}

	protected void _InvalidateCategories()
	{
		_categoriesValidated = false;
	}

	public void InitializeAudioItems()
	{
		if (isAdditionalAudioController)
		{
			return;
		}
		_audioItems = new Dictionary<string, AudioItem>(StringComparer.OrdinalIgnoreCase);
		_InitializeAudioItems(this);
		if (_additionalAudioControllers == null)
		{
			return;
		}
		for (int i = 0; i < _additionalAudioControllers.Count; i++)
		{
			AudioController audioController = _additionalAudioControllers[i];
			if (audioController != null)
			{
				_InitializeAudioItems(audioController);
			}
		}
	}

	private void _InitializeAudioItems(AudioController audioController)
	{
		for (int i = 0; i < audioController.AudioCategories.Length; i++)
		{
			AudioCategory audioCategory = audioController.AudioCategories[i];
			audioCategory.audioController = audioController;
			audioCategory._AnalyseAudioItems(_audioItems);
			if ((bool)audioCategory.AudioObjectPrefab)
			{
				_ValidateAudioObjectPrefab(audioCategory.AudioObjectPrefab);
			}
		}
	}

	private void _RegisterAdditionalAudioController(AudioController ac)
	{
		if (_additionalAudioControllers == null)
		{
			_additionalAudioControllers = new List<AudioController>();
		}
		_additionalAudioControllers.Add(ac);
		_InvalidateCategories();
		_SyncCategoryVolumes(ac, this);
	}

	private void _SyncCategoryVolumes(AudioController toSync, AudioController syncWith)
	{
		for (int i = 0; i < toSync.AudioCategories.Length; i++)
		{
			AudioCategory audioCategory = toSync.AudioCategories[i];
			AudioCategory audioCategory2 = syncWith._GetCategory(audioCategory.Name);
			if (audioCategory2 != null)
			{
				audioCategory.Volume = audioCategory2.Volume;
			}
		}
	}

	private void _UnregisterAdditionalAudioController(AudioController ac)
	{
		if (_additionalAudioControllers != null)
		{
			for (int i = 0; i < _additionalAudioControllers.Count; i++)
			{
				if (_additionalAudioControllers[i] == ac)
				{
					_additionalAudioControllers.RemoveAt(i);
					_InvalidateCategories();
					break;
				}
			}
		}
		else
		{
			Debug.LogWarning("_UnregisterAdditionalAudioController: AudioController " + ac.name + " not found");
		}
	}

	private static List<AudioCategory> _GetAllCategories(string name)
	{
		AudioController instance = SingletonMonoBehaviour<AudioController>.Instance;
		List<AudioCategory> list = new List<AudioCategory>();
		AudioCategory audioCategory = instance._GetCategory(name);
		if (audioCategory != null)
		{
			list.Add(audioCategory);
		}
		if (instance._additionalAudioControllers != null)
		{
			for (int i = 0; i < instance._additionalAudioControllers.Count; i++)
			{
				audioCategory = instance._additionalAudioControllers[i]._GetCategory(name);
				if (audioCategory != null)
				{
					list.Add(audioCategory);
				}
			}
		}
		return list;
	}

	public AudioObject PlayAudioSubItem(AudioSubItem subItem, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject, AudioObject useExistingAudioObj, double dspTime = 0.0, AudioChannelType channel = AudioChannelType.Default, float startVolumeMultiplier = 1f)
	{
		_ValidateCategories();
		AudioItem item = subItem.item;
		AudioSubItemType subItemType = subItem.SubItemType;
		if (subItemType != AudioSubItemType.Clip && subItemType == AudioSubItemType.Item)
		{
			if (subItem.ItemModeAudioID.Length == 0)
			{
				Debug.LogWarning("No item specified in audio sub-item with ITEM mode (audio item: '" + item.Name + "')");
				return null;
			}
			return _PlayEx(subItem.ItemModeAudioID, channel, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject, dspTime, useExistingAudioObj);
		}
		if (subItem.Clip == null)
		{
			return null;
		}
		AudioCategory category = item.category;
		float num = subItem.Volume * item.Volume * volume;
		if (subItem.RandomVolume != 0f || item.loopSequenceRandomVolume != 0f)
		{
			float num2 = subItem.RandomVolume + item.loopSequenceRandomVolume;
			num += UnityEngine.Random.Range(0f - num2, num2);
			num = Mathf.Clamp01(num);
		}
		float num3 = num * category.VolumeTotal;
		AudioController audioController = _GetAudioController(subItem);
		if (!audioController.PlayWithZeroVolume && (num3 <= 0f || Volume <= 0f))
		{
			return null;
		}
		GameObject gameObject = category.GetAudioObjectPrefab();
		if (gameObject == null)
		{
			gameObject = ((!(audioController.AudioObjectPrefab != null)) ? AudioObjectPrefab : audioController.AudioObjectPrefab);
		}
		if (playWithoutAudioObject)
		{
			gameObject.GetComponent<AudioSource>().PlayOneShot(subItem.Clip, AudioObject.TransformVolume(num3));
			return null;
		}
		GameObject gameObject2;
		AudioObject audioObject;
		if (useExistingAudioObj == null)
		{
			if (item.DestroyOnLoad)
			{
				gameObject2 = ((!audioController.UsePooledAudioObjects) ? ObjectPoolController.InstantiateWithoutPool(gameObject, worldPosition, Quaternion.identity) : ObjectPoolController.Instantiate(gameObject, worldPosition, Quaternion.identity));
			}
			else
			{
				gameObject2 = ObjectPoolController.InstantiateWithoutPool(gameObject, worldPosition, Quaternion.identity);
				UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			}
			if ((bool)parentObj)
			{
				gameObject2.transform.SetParent(parentObj);
			}
			audioObject = gameObject2.gameObject.GetComponent<AudioObject>();
		}
		else
		{
			gameObject2 = useExistingAudioObj.gameObject;
			audioObject = useExistingAudioObj;
		}
		audioObject.subItem = subItem;
		if ((object)useExistingAudioObj == null)
		{
			audioObject._lastChosenSubItemIndex = item._lastChosen;
		}
		audioObject.primaryAudioSource.clip = subItem.Clip;
		gameObject2.name = "AudioObject:" + audioObject.primaryAudioSource.clip.name;
		audioObject.originalPitch = AudioObject.TransformPitch(subItem.PitchShift);
		audioObject.primaryAudioSource.pitch = audioObject.originalPitch;
		IsImmuneToTimeSacle(category);
		audioObject.primaryAudioSource.panStereo = subItem.Pan2D;
		if (subItem.RandomStartPosition)
		{
			startTime = UnityEngine.Random.Range(0f, audioObject.clipLength);
		}
		audioObject.primaryAudioSource.time = startTime + subItem.ClipStartTime;
		audioObject.primaryAudioSource.loop = item.Loop == AudioItem.LoopMode.LoopSubitem || item.Loop == (AudioItem.LoopMode)3;
		audioObject._volumeExcludingCategory = num;
		audioObject._volumeFromScriptCall = volume;
		audioObject.category = category;
		audioObject.channel = channel;
		if (subItem.FadeIn > 0f)
		{
			audioObject.FadeIn(subItem.FadeIn);
		}
		audioObject._ApplyVolumePrimary(startVolumeMultiplier);
		if ((bool)category.GetAudioMixerGroup())
		{
			audioObject.primaryAudioSource.outputAudioMixerGroup = category.audioMixerGroup;
		}
		if (subItem.RandomPitch != 0f || item.loopSequenceRandomPitch != 0f)
		{
			float num4 = subItem.RandomPitch + item.loopSequenceRandomPitch;
			audioObject.originalPitch *= AudioObject.TransformPitch(UnityEngine.Random.Range(0f - num4, num4));
			audioObject.primaryAudioSource.pitch = audioObject.originalPitch;
			IsImmuneToTimeSacle(category);
		}
		if (subItem.RandomDelay > 0f)
		{
			delay += UnityEngine.Random.Range(0f, subItem.RandomDelay);
		}
		if (dspTime > 0.0)
		{
			audioObject.PlayScheduled(dspTime + (double)delay + (double)subItem.Delay + (double)item.Delay);
		}
		else
		{
			audioObject.Play(delay + subItem.Delay + item.Delay);
		}
		if (subItem.FadeIn > 0f)
		{
			audioObject.FadeIn(subItem.FadeIn);
		}
		if (LogToGameConsole)
		{
			GameConsole.Engine.Print(((parentObj != null) ? (parentObj.name + " is ") : "") + "playing audio " + item.Name + " with audio file " + subItem.Clip.name);
		}
		return audioObject;
	}

	private AudioController _GetAudioController(AudioSubItem subItem)
	{
		if (subItem.item != null && subItem.item.category != null)
		{
			return subItem.item.category.audioController;
		}
		return this;
	}

	internal void _NotifyPlaylistTrackCompleteleyPlayed(AudioObject audioObject)
	{
		audioObject._isCurrentPlaylistTrack = false;
		if (!IsPlaylistPlaying() || !(_currentMusic == audioObject))
		{
			return;
		}
		AudioObject audioObject2 = _PlayNextMusicOnPlaylist(delayBetweenPlaylistTracks);
		if (audioObject2 == null)
		{
			_isPlaylistPlaying = false;
			if (playlistFinishedEvent != null)
			{
				playlistFinishedEvent(_GetCurrentPlaylist());
			}
		}
		else
		{
			audioObject.OnPlaylistTrackCompleteleyPlayed(audioObject2);
		}
	}

	private void _ValidateAudioObjectPrefab(GameObject audioPrefab)
	{
		if (UsePooledAudioObjects)
		{
			if (audioPrefab.GetComponent<PoolableObject>() == null)
			{
				Debug.LogWarning("AudioObject prefab does not have the PoolableObject component. Pooling will not work.");
			}
			else
			{
				ObjectPoolController.Preload(audioPrefab);
			}
		}
		if (audioPrefab.GetComponent<AudioObject>() == null)
		{
			Debug.LogError("AudioObject prefab must have the AudioObject script component!");
		}
	}

	public void OnAfterDeserialize()
	{
		if (musicPlaylist != null && musicPlaylist.Length != 0)
		{
			List<string> list = new List<string>(musicPlaylist);
			musicPlaylists[0] = new Playlist();
			musicPlaylists[0].playlistItems = list.ToArray();
			musicPlaylist = null;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	private void _SetDefaultCurrentPlaylist()
	{
		if (musicPlaylists != null && musicPlaylists.Length >= 1 && musicPlaylists[0] != null)
		{
			_currentPlaylistName = musicPlaylists[0].name;
		}
	}
}
