using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClockStone;
using SETUtil.Extend;
using UnityEngine;
using UnityEngine.Events;

namespace FFHGUI.Elements;

public class UINarrationComponent : MonoBehaviour
{
	public record NarrationAssetBundleParams
	{
		public string AssetBundleName;

		public string AssetBundleDirName;

		public string FileFormat = "wav";
	}

	private AudioSource _narrationVO;

	private AudioSource _narrationSFX;

	private List<AudioClip> _narrationVOClips = new List<AudioClip>();

	private List<AudioClip> _narrationSFXClips = new List<AudioClip>();

	private Coroutine _narrationCrt;

	private NarrationAssetBundleParams _assetBundleParams;

	private bool _loadSFX;

	private string _audioKey;

	private bool _useGlobalMute;

	private int _currentClipIndex;

	[SerializeField]
	private UnityEvent _onNarrationStartEventHandler = new UnityEvent();

	[SerializeField]
	private UnityEvent _onNarrationEndEventHandler = new UnityEvent();

	[SerializeField]
	private UnityEvent _onNarrationStoppedEventHandler = new UnityEvent();

	[SerializeField]
	private UnityEvent<int, int> _onNarrationNewStepEventHandler = new UnityEvent<int, int>();

	[SerializeField]
	private UnityEvent<float> _onNarrationProgressEventHandler = new UnityEvent<float>();

	private Action _onNarrationStart;

	private Action _onNarrationEnd;

	private Action _onNarrationStopped;

	private Action<int, int> _onNarrationNewStep;

	private Action<float> _onNarrationProgress;

	private bool _setup;

	public bool HasStarted
	{
		get
		{
			if (_narrationVO != null)
			{
				return _narrationVO.time > 0f;
			}
			return false;
		}
	}

	public bool HasEnded
	{
		get
		{
			if (_narrationVO != null && CurrentStep == TotalSteps)
			{
				return _narrationVO?.time >= _narrationVO?.clip?.length;
			}
			return false;
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (!(_narrationVO != null) || !_narrationVO.isPlaying)
			{
				if (_narrationSFX != null)
				{
					return _narrationSFX.isPlaying;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsMuted
	{
		get
		{
			if (_narrationVO != null)
			{
				return _narrationVO.mute;
			}
			return false;
		}
	}

	public bool IsPaused
	{
		get
		{
			if (_narrationVO != null && _narrationVO.time > 0f)
			{
				return !_narrationVO.isPlaying;
			}
			return false;
		}
	}

	public int TotalSteps => _narrationVOClips?.Count ?? 0;

	public int CurrentStep => _currentClipIndex;

	public float Progress => GetTotalProgress();

	public float CurrentStepProgress => GetCurrentStepProgress();

	public float TotalLength => GetTotalLength();

	public static float MasterVolume => SaveData.Instance?.Global.MasterVolume ?? 0;

	public static float SFXVolume => SaveData.Instance?.Global.SFXVolume ?? 0;

	public static float StoryVolume => SaveData.Instance?.Global.StoryVolume ?? 0;

	public static bool GlobalNarrationMuted => SaveData.Instance?.Global.MuteStoryVoiceOver ?? true;

	private void Awake()
	{
		_onNarrationStartEventHandler.AddListener(OnNarrationStart);
		_onNarrationEndEventHandler.AddListener(OnNarrationEnd);
		_onNarrationStoppedEventHandler.AddListener(OnNarrationStopped);
		_onNarrationNewStepEventHandler.AddListener(OnNarrationNewStep);
		_onNarrationProgressEventHandler.AddListener(OnNarrationProgress);
	}

	public void Init(string audioKey, NarrationAssetBundleParams assetBundleParams, bool loadSFX = true, bool autoPlay = false, bool useGlobalMute = true, Action onNarrationStart = null, Action onNarrationEnd = null, Action onNarrationStopped = null, Action<int, int> onNarrationNewStep = null, Action<float> onNarrationProgress = null)
	{
		_setup = false;
		_loadSFX = loadSFX;
		_audioKey = audioKey;
		_assetBundleParams = assetBundleParams;
		_useGlobalMute = useGlobalMute;
		_onNarrationStart = onNarrationStart;
		_onNarrationEnd = onNarrationEnd;
		_onNarrationStopped = onNarrationStopped;
		_onNarrationNewStep = onNarrationNewStep;
		_onNarrationProgress = onNarrationProgress;
		StopNarration();
		SetMute(useGlobalMute && GlobalNarrationMuted);
		if (autoPlay)
		{
			Play();
		}
	}

	public void Cleanup()
	{
		_setup = false;
		_narrationVOClips.Clear();
		_narrationSFXClips.Clear();
		_assetBundleParams = null;
		_onNarrationStart = null;
		_onNarrationEnd = null;
		_onNarrationStopped = null;
		_onNarrationNewStep = null;
		_onNarrationProgress = null;
		StopNarration();
	}

	private void SetupNarration()
	{
		if (_setup || _audioKey.IsNullOrEmpty())
		{
			return;
		}
		List<AudioSource> list = gameObject.GetComponents<AudioSource>().ToList();
		int count = list.Count;
		int num = ((!_loadSFX) ? 1 : 2);
		for (int i = count; i < num; i++)
		{
			list.Add(gameObject.AddComponent<AudioSource>());
		}
		gameObject.GetOrAdd<AudioObject>();
		_narrationVO = list.ElementAtOrDefault(0);
		_narrationSFX = list.ElementAtOrDefault(1);
		_narrationVOClips.Clear();
		_narrationSFXClips.Clear();
		AudioClip audioClip = LoadNarrationAudioClip(_audioKey + "_VO");
		AudioClip audioClip2 = (_loadSFX ? LoadNarrationAudioClip(_audioKey + "_SFX") : null);
		if (audioClip != null || audioClip2 != null)
		{
			_narrationVOClips.Add(audioClip);
			_narrationSFXClips.Add(audioClip2);
			return;
		}
		for (int j = 1; j < 100; j++)
		{
			AudioClip audioClip3 = LoadNarrationAudioClip($"{_audioKey}_{j}_VO");
			AudioClip audioClip4 = (_loadSFX ? LoadNarrationAudioClip($"{_audioKey}_{j}_SFX") : null);
			if (audioClip3 == null && audioClip4 == null)
			{
				break;
			}
			_narrationVOClips.Add(audioClip3);
			_narrationSFXClips.Add(audioClip4);
		}
		_setup = true;
	}

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		if (_assetBundleParams == null)
		{
			return null;
		}
		if (filename != null)
		{
			return AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>(_assetBundleParams.AssetBundleName, filename, _assetBundleParams.AssetBundleDirName, _assetBundleParams.FileFormat, null, suppressError: true);
		}
		return null;
	}

	private IEnumerator PlayNarration_Crt(int index = 0)
	{
		_currentClipIndex = index;
		if (_narrationVOClips.Count == 0)
		{
			yield break;
		}
		if (index >= _narrationVOClips.Count)
		{
			_onNarrationEndEventHandler?.Invoke();
			yield break;
		}
		SetVolume();
		while (index < _narrationVOClips.Count)
		{
			_onNarrationNewStepEventHandler?.Invoke(_currentClipIndex, _narrationVOClips.Count);
			AudioClip audioClip = _narrationVOClips[index];
			if ((bool)audioClip)
			{
				_narrationVO.clip = audioClip;
				_narrationVO.Play();
			}
			AudioClip clip = _narrationSFXClips[index];
			if ((bool)audioClip)
			{
				_narrationSFX.clip = clip;
				_narrationSFX.Play();
			}
			index++;
			while (IsPaused || _narrationVO.isPlaying || (_narrationSFX != null && _narrationSFX.isPlaying))
			{
				if (!IsPaused)
				{
					_onNarrationProgressEventHandler?.Invoke(Progress);
				}
				yield return null;
			}
			if (index + 1 >= _narrationVOClips.Count)
			{
				_onNarrationEndEventHandler?.Invoke();
				yield break;
			}
		}
		_currentClipIndex = 0;
	}

	private void StopNarration()
	{
		if ((!(_narrationVO == null) && _narrationVO.isPlaying) || (!(_narrationSFX == null) && _narrationSFX.isPlaying))
		{
			_currentClipIndex = 0;
			if (_narrationVO != null)
			{
				_narrationVO.Stop();
				_narrationVO.time = 0f;
				_narrationVO.clip = null;
			}
			if (_narrationSFX != null)
			{
				_narrationSFX.Stop();
				_narrationVO.time = 0f;
				_narrationSFX.clip = null;
			}
			if (_narrationCrt != null)
			{
				StopCoroutine(_narrationCrt);
			}
			_narrationCrt = null;
			_onNarrationStoppedEventHandler?.Invoke();
		}
	}

	private void ToggleNarration(bool active)
	{
		if (!_audioKey.IsNullOrEmpty())
		{
			SetupNarration();
			StopNarration();
			if (active)
			{
				PlayNarration();
			}
		}
	}

	public void PlayNarration(int startIndex = 0)
	{
		SetupNarration();
		_onNarrationStartEventHandler?.Invoke();
		_narrationCrt = StartCoroutine(PlayNarration_Crt());
	}

	private void SetVolume()
	{
		if (SaveData.Instance != null)
		{
			float num = 0.01f * (float)SaveData.Instance.Global.MasterVolume;
			_narrationVO.volume = 0.6f * num * 0.01f * (float)SaveData.Instance.Global.StoryVolume;
			if (_narrationSFX != null)
			{
				_narrationSFX.volume = num * 0.01f * (float)SaveData.Instance.Global.SFXVolume;
			}
		}
		else
		{
			_narrationVO.volume = 0.2f;
			if (_narrationSFX != null)
			{
				_narrationSFX.volume = 0.2f;
			}
		}
	}

	private float GetCurrentAudioSourceClipProgress(AudioSource source)
	{
		if (source != null && source.clip != null)
		{
			return source.time / source.clip.length;
		}
		return 0f;
	}

	private float GetTotalLength()
	{
		if (_narrationVO == null && _narrationSFX == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < _narrationVOClips.Count; i++)
		{
			num += Mathf.Max(_narrationVOClips[i]?.length ?? 0f, _narrationSFXClips[0]?.length ?? 0f);
		}
		return num;
	}

	private float GetCurrentStepProgress()
	{
		return Mathf.Min(GetCurrentAudioSourceClipProgress(_narrationVO), GetCurrentAudioSourceClipProgress(_narrationSFX));
	}

	private float GetCurrentStepLength()
	{
		if (_narrationVOClips.Count == 0)
		{
			return 0f;
		}
		AudioClip audioClip = _narrationVOClips[_currentClipIndex];
		return Mathf.Max(b: _narrationSFXClips[_currentClipIndex]?.length ?? 0f, a: audioClip.length);
	}

	private float GetStepLength(int index)
	{
		if (index >= _narrationVOClips.Count)
		{
			return 0f;
		}
		AudioClip audioClip = _narrationVOClips[index];
		return Mathf.Max(b: _narrationSFXClips[index]?.length ?? 0f, a: audioClip?.length ?? 0f);
	}

	private float GetTotalProgress()
	{
		if (_narrationVO == null && _narrationSFX == null)
		{
			return 0f;
		}
		if (_narrationVOClips.Count == 0)
		{
			return 0f;
		}
		float totalLength = GetTotalLength();
		float num = 0f;
		for (int i = 0; i < _currentClipIndex + 1; i++)
		{
			AudioClip audioClip = _narrationVOClips[i];
			AudioClip audioClip2 = _narrationSFXClips[i];
			if ((audioClip != null && _narrationVO != null && _narrationVO.clip == audioClip) || (audioClip2 != null && _narrationSFX != null && _narrationSFX.clip == audioClip))
			{
				num += GetCurrentStepProgress() * GetCurrentStepLength();
				break;
			}
			num += GetStepLength(i);
		}
		if (totalLength != 0f)
		{
			return num / totalLength;
		}
		return 0f;
	}

	public void Play()
	{
		ToggleNarration(active: true);
	}

	public void RestartNarration()
	{
		ToggleNarration(active: true);
	}

	public void ResetNarration()
	{
		if (!_audioKey.IsNullOrEmpty())
		{
			Stop();
		}
	}

	public void Stop()
	{
		ToggleNarration(active: false);
	}

	public void Pause()
	{
		if (_narrationVO != null)
		{
			_narrationVO.Pause();
		}
		if (_narrationSFX != null)
		{
			_narrationSFX.Pause();
		}
	}

	public void UnPause()
	{
		if (_narrationVO != null)
		{
			_narrationVO.UnPause();
		}
		if (_narrationSFX != null)
		{
			_narrationSFX.UnPause();
		}
	}

	public void TogglePause()
	{
		if (_narrationVO != null && _narrationVO.isPlaying)
		{
			Pause();
		}
		else
		{
			UnPause();
		}
	}

	public void SetMute(bool mute)
	{
		if (mute)
		{
			Mute();
		}
		else
		{
			Unmute();
		}
	}

	public void Mute()
	{
		if (_narrationVO != null)
		{
			_narrationVO.mute = true;
		}
		if (_narrationSFX != null)
		{
			_narrationSFX.mute = true;
		}
	}

	public void Unmute()
	{
		if (_narrationVO != null)
		{
			_narrationVO.mute = false;
		}
		if (_narrationSFX != null)
		{
			_narrationSFX.mute = true;
		}
	}

	public void ToggleMute()
	{
		if (_narrationVO != null)
		{
			_narrationVO.mute = !_narrationVO.mute;
		}
		if (_narrationSFX != null)
		{
			_narrationSFX.mute = !_narrationSFX.mute;
		}
	}

	private void OnNarrationStart()
	{
		_onNarrationStart?.Invoke();
	}

	private void OnNarrationEnd()
	{
		_onNarrationEnd?.Invoke();
	}

	private void OnNarrationStopped()
	{
		_onNarrationStopped?.Invoke();
	}

	private void OnNarrationNewStep(int step, int maxSteps)
	{
		_onNarrationNewStep?.Invoke(step, maxSteps);
	}

	private void OnNarrationProgress(float progress)
	{
		_onNarrationProgress?.Invoke(progress);
	}
}
