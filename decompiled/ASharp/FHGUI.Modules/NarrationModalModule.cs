using System;
using System.Collections;
using System.Collections.Generic;
using ClockStone;
using DG.Tweening;
using FHGUI.Elements;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class NarrationModalModule : FHMapViewModule, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public struct Params
	{
		public List<UIMultiplayerUser.Data> SuggestingUsers;

		public NarrationSetting Narration;

		public int Step;

		public bool IsPartyLeader;

		public bool IsOnline;

		public bool Complete;
	}

	[Header("Components")]
	[SerializeField]
	private Image Illustration;

	[SerializeField]
	private Image Illustration_Prev;

	[SerializeField]
	private TMP_Text Text;

	[SerializeField]
	private ScrollRect ScrollRect;

	[SerializeField]
	private FHToggle AudioToggle;

	[SerializeField]
	private FHButton NextButton;

	[SerializeField]
	private FHButton RestartButton;

	[SerializeField]
	private RectTransform ButtonsContainer;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer SyncUserList;

	[Header("Skip")]
	[SerializeField]
	private float HoldDelay = 1.2f;

	[SerializeField]
	private float HoldDeadZone = 0.35f;

	[SerializeField]
	private GameObject Skip;

	[SerializeField]
	private Image SkipProgress;

	[Header("Settings")]
	[SerializeField]
	private float IllustrationTransitionDuration;

	[SerializeField]
	private float VoiceOverStartDelay = 0.5f;

	[SerializeField]
	private float AutoAdvanceDelay = 0.5f;

	[SerializeField]
	private float VoiceOverOnCancelFadeOutDuration = 0.2f;

	[SerializeField]
	private float TextScrollStartDelay = 7f;

	[SerializeField]
	private float TextScrollSpeed = 1f;

	public Action OnDone;

	public Action<int> OnStepChange;

	private NarrationSetting _narrationSettings;

	private int _currentStoryIndex;

	private bool _done = true;

	private bool _isPartyLeader;

	private string _currentVoiceover;

	private Coroutine _stepVoiceOverRoutine;

	private Coroutine _stepAutoAdvanceRoutine;

	private Coroutine _stepTextScrollRoutine;

	private Coroutine _skipRoutine;

	private AudioObject _narrationMusic;

	private AudioSource _narrationVO;

	private AudioSource _narrationSFX;

	private static Dictionary<string, string> NarrationsMusicMap;

	private List<Action> ExecuteOnEnable = new List<Action>();

	private float _holdDuration;

	private bool IsLastSegment => _currentStoryIndex + 1 >= _narrationSettings.Story.Count;

	public Params Data { get; private set; }

	private bool IsGlobalMute => SaveData.Instance?.Global.MuteStoryVoiceOver ?? true;

	static NarrationModalModule()
	{
		NarrationsMusicMap = new Dictionary<string, string>();
		NarrationsMusicMap["FS_001_A_Town_In_Flames_Intro"] = "StoryMusic_Combat 2";
		NarrationsMusicMap["FS_001_A_Town_In_Flames_Outro"] = "StoryMusic_Ominous";
		NarrationsMusicMap["FS_002_Algox_Scouting_Intro"] = "StoryMusic_Combat 6";
		NarrationsMusicMap["FS_002_Algox_Scouting_Outro"] = "StoryMusic_Mountain";
		NarrationsMusicMap["FS_003_Algox_Offensive_Intro"] = "StoryMusic_Combat 1";
		NarrationsMusicMap["FS_003_Algox_Offensive_Outro"] = "StoryMusic_Sadness";
		NarrationsMusicMap["FS_004A_Heart_Of_Ice_Intro"] = "StoryMusic_Combat 3";
		NarrationsMusicMap["FS_004A_Heart_Of_Ice_Outro_A"] = "StoryMusic_Ominous";
		NarrationsMusicMap["FS_004A_Heart_Of_Ice_Outro_B"] = "StoryMusic_Ominous";
		NarrationsMusicMap["FS_004B_Heart_Of_Ice_Outro_A"] = "StoryMusic_Mountain";
		NarrationsMusicMap["FS_004B_Heart_Of_Ice_Outro_B"] = "StoryMusic_Mountain";
	}

	public void Awake()
	{
		NextButton.onClick.AddListener(NextStory);
		AudioToggle.onValueChanged.AddListener(OnNarrationToggle);
		RestartButton.onClick.AddListener(RestartNarration);
	}

	protected override void OnShow()
	{
		base.OnShow();
		foreach (Action item in ExecuteOnEnable)
		{
			item();
		}
		ExecuteOnEnable.Clear();
	}

	protected override void OnHide()
	{
		base.OnHide();
		StopVoiceOver();
	}

	public void Init(Params @params)
	{
		Data = @params;
		_narrationSettings = @params.Narration;
		_isPartyLeader = @params.IsPartyLeader;
		_currentStoryIndex = @params.Step;
		_done = @params.Complete;
		Text.text = string.Empty;
		Illustration.enabled = false;
		Illustration_Prev.enabled = false;
		AudioToggle.SetIsOnWithoutNotify(IsGlobalMute);
		CancelSkip();
		SyncUserList.Init(@params.SuggestingUsers);
		UpdateStory(instant: true);
	}

	private bool IsWaitingForAChoice()
	{
		if (_narrationSettings == null)
		{
			return false;
		}
		if (_currentStoryIndex < 0 || _currentStoryIndex >= _narrationSettings.Story.Count)
		{
			return false;
		}
		NarrationSetting.Setting setting = _narrationSettings.Story[_currentStoryIndex];
		if (setting.Options != null)
		{
			return setting.Options.Length != 0;
		}
		return false;
	}

	private void NextStory()
	{
		if (!IsWaitingForAChoice())
		{
			StopVoiceOver();
			StopAutoAdvance();
			StopTextScrolling();
			_currentStoryIndex++;
			if (_currentStoryIndex >= _narrationSettings.Story.Count)
			{
				EndNarration();
				return;
			}
			UpdateStory(instant: false);
			ScrollRect.normalizedPosition = new Vector2(0f, 1f);
		}
	}

	private void UpdateStory(bool instant)
	{
		NarrationSetting.Setting setting = _narrationSettings.Story[_currentStoryIndex];
		bool flag = setting.Options != null && setting.Options.Length != 0;
		string validTermName = I2Utils.GetValidTermName(setting.Text);
		Text.text = GLOOM.LocalizationManager.GetTranslation(validTermName);
		if (setting.Voiceover != _currentVoiceover)
		{
			if (!string.IsNullOrEmpty(_currentVoiceover))
			{
				StopVoiceOver();
			}
			if (!string.IsNullOrEmpty(setting.Voiceover))
			{
				_currentVoiceover = setting.Voiceover;
				if (gameObject.activeInHierarchy)
				{
					_stepVoiceOverRoutine = StartCoroutine(PlayVoiceOver(_currentVoiceover));
					_stepTextScrollRoutine = StartCoroutine(ScrollText());
				}
				else
				{
					ExecuteOnEnable.Add(() =>
					{
						_stepVoiceOverRoutine = StartCoroutine(PlayVoiceOver(_currentVoiceover));
						_stepTextScrollRoutine = StartCoroutine(ScrollText());
					});
				}
			}
		}
		UpdateIllustration(instant);
		if (NextButton != null)
		{
			NextButton.gameObject.SetActive(!flag);
			NextButton.interactable = !IsLastSegment || !Data.Complete;
			FHSelectableExtensions.SetNonLocalizedText(nonlocalizedText: IsLastSegment ? GLOOM.LocalizationManager.GetTranslation(Data.IsOnline ? "GUI_READY" : "GUI_CONTINUE") : (GLOOM.LocalizationManager.GetTranslation("GUI_NEXT") + " (" + GLOOM.LocalizationManager.GetTranslation("GUI_HOLD_TO_SKIP") + ")"), selectable: NextButton);
		}
		if (RestartButton != null)
		{
			RestartButton.gameObject.SetActive(IsLastSegment && _currentStoryIndex > 0);
			RestartButton.SetTextKey("GUI_REREAD");
		}
		if ((bool)ButtonsContainer)
		{
			ButtonsContainer.gameObject.SetActive(flag);
			if (flag)
			{
				ButtonsBarParam[] array = new ButtonsBarParam[setting.Options.Length];
				for (int num = 0; num < setting.Options.Length; num++)
				{
					NarrationSetting.Setting.ChoiceOption choiceOption = setting.Options[num];
					array[num] = new ButtonsBarParam
					{
						Interactable = _isPartyLeader,
						Callback = choiceOption.Action,
						LabelKey = choiceOption.Text
					};
				}
				ButtonsBarElement.SetButtons(ButtonsContainer, array);
			}
		}
		OnStepChange?.Invoke(_currentStoryIndex);
	}

	private void UpdateIllustration(bool instant)
	{
		NarrationSetting.Setting setting = _narrationSettings.Story[_currentStoryIndex];
		NarrationSetting.Setting? setting2 = ((_currentStoryIndex > 0) ? new NarrationSetting.Setting?(_narrationSettings.Story[_currentStoryIndex - 1]) : ((NarrationSetting.Setting?)null));
		Sprite narrativeIllustration = UIInfoTools.Instance.GetNarrativeIllustration(setting.Image, throwError: false);
		Sprite sprite = (setting2.HasValue ? UIInfoTools.Instance.GetNarrativeIllustration(setting2.Value.Image, throwError: false) : null);
		if (!instant && narrativeIllustration != null && sprite != null && narrativeIllustration != sprite)
		{
			AnimateIllustrations(narrativeIllustration, sprite);
			return;
		}
		if (narrativeIllustration != null)
		{
			Illustration.sprite = narrativeIllustration;
			Illustration.enabled = true;
		}
		if ((bool)Illustration_Prev)
		{
			Illustration_Prev.enabled = false;
		}
	}

	private void AnimateIllustrations(Sprite next, Sprite prev)
	{
		if (!(Illustration_Prev == null) && !(Illustration == null))
		{
			Illustration.enabled = true;
			Illustration_Prev.enabled = true;
			Illustration_Prev.sprite = prev;
			Illustration.sprite = next;
			Illustration_Prev.color = new Color(1f, 1f, 1f, 1f);
			Illustration.color = new Color(1f, 1f, 1f, 0f);
			Illustration_Prev.DOFade(0f, IllustrationTransitionDuration).SetEase(Ease.Flash);
			Illustration.DOFade(1f, IllustrationTransitionDuration).SetEase(Ease.Flash);
		}
	}

	private void RestartNarration()
	{
		_currentStoryIndex = -1;
		_done = false;
		Text.text = string.Empty;
		Illustration.enabled = false;
		Illustration_Prev.enabled = false;
		StopVoiceOver();
		_currentVoiceover = null;
		StopAutoAdvance();
		NextStory();
	}

	private void EndNarration()
	{
		if (!_done)
		{
			_done = true;
			StopVoiceOver();
			StopMusic();
			OnDone?.Invoke();
		}
	}

	private void OnNarrationToggle(bool toggleOn)
	{
		if (!(SaveData.Instance == null))
		{
			SaveData.Instance.Global.MuteStoryVoiceOver = toggleOn;
			SyncVoiceOverVolume();
			if (!SaveData.Instance.IsSavingData)
			{
				SaveData.Instance.SaveGlobalData();
			}
		}
	}

	private void SyncVoiceOverVolume()
	{
		bool isGlobalMute = IsGlobalMute;
		float num = 0.01f * (float)SaveData.Instance.Global.MasterVolume;
		if (_narrationSFX != null && _narrationVO.isPlaying)
		{
			_narrationSFX.volume = (isGlobalMute ? 0f : (num * 0.01f * (float)SaveData.Instance.Global.SFXVolume));
		}
		if (_narrationVO != null && _narrationVO.isPlaying)
		{
			_narrationVO.volume = (isGlobalMute ? 0f : (num * 0.01f * (float)SaveData.Instance.Global.StoryVolume));
			Log.SimpleInfo($"Syncing story narration volume, muted: {isGlobalMute} audio src vol: {_narrationVO.volume} narration vol: {0.01f * (float)SaveData.Instance.Global.StoryVolume}" + $" master vol: {num}");
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!IsLastSegment && eventData.button == PointerEventData.InputButton.Left)
		{
			_skipRoutine = StartCoroutine(TrackSkip());
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		if (IsLastSegment || (_skipRoutine != null && _holdDuration < HoldDeadZone))
		{
			if (_audioProfile != null)
			{
				AudioControllerUtils.PlaySound(_audioProfile.mouseUpAudioItem);
			}
			NextStory();
		}
		CancelSkip();
	}

	private void StopVoiceOver()
	{
		if (!_currentVoiceover.IsNullOrEmpty())
		{
			if (_stepVoiceOverRoutine != null)
			{
				StopCoroutine(_stepVoiceOverRoutine);
			}
			if ((bool)_narrationSFX)
			{
				_narrationSFX.Stop();
			}
			if ((bool)_narrationVO)
			{
				_narrationVO.Stop();
			}
		}
	}

	private void StopMusic()
	{
		if (_narrationMusic != null)
		{
			_narrationMusic.Stop(1f);
		}
	}

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_storynarrations", filename, "storynarrations", "wav", null, suppressError: true);
	}

	private IEnumerator PlayVoiceOver(string narrativeAudioId)
	{
		if (string.IsNullOrEmpty(narrativeAudioId))
		{
			yield break;
		}
		yield return new WaitForSeconds(VoiceOverStartDelay);
		string audioKeyFromLocKey = GetAudioKeyFromLocKey(narrativeAudioId);
		if (_narrationSFX == null)
		{
			AudioSource[] components = GetComponents<AudioSource>();
			if (components.Length < 2)
			{
				_narrationVO = gameObject.AddComponent<AudioSource>();
				_narrationSFX = gameObject.AddComponent<AudioSource>();
			}
			else
			{
				_narrationVO = components[0];
				_narrationSFX = components[1];
			}
		}
		AudioClip audioClip = LoadNarrationAudioClip(audioKeyFromLocKey + "_VO");
		if ((bool)audioClip)
		{
			_narrationVO.clip = audioClip;
			_narrationVO.Play();
		}
		AudioClip clip = LoadNarrationAudioClip(audioKeyFromLocKey + "_SFX");
		if ((bool)audioClip)
		{
			_narrationSFX.clip = clip;
			_narrationSFX.Play();
		}
		SyncVoiceOverVolume();
		if ((bool)audioClip)
		{
			Log.SimpleInfo($"Playing story narration with audio src vol: {_narrationVO.volume} narration vol: {0.01f * (float)SaveData.Instance.Global.StoryVolume}" + $" master vol: {0.01f * (float)SaveData.Instance.Global.MasterVolume}");
		}
		if (!IsLastSegment && _narrationVO != null && audioClip != null)
		{
			_stepAutoAdvanceRoutine = StartCoroutine(AutoAdvanceAfter(_narrationVO));
		}
		foreach (string key in NarrationsMusicMap.Keys)
		{
			if (!audioKeyFromLocKey.ToUpper().StartsWith(key.ToUpper()))
			{
				continue;
			}
			string text = NarrationsMusicMap[key];
			if (_narrationMusic != null)
			{
				if (_narrationMusic.audioID != text)
				{
					if (_narrationMusic.IsPlaying())
					{
						_narrationMusic.Stop(1f);
					}
					_narrationMusic = AudioController.Play(text);
				}
				else if (!_narrationMusic.IsPlaying())
				{
					_narrationMusic = AudioController.Play(text);
				}
			}
			else
			{
				_narrationMusic = AudioController.Play(text);
			}
			break;
		}
	}

	private IEnumerator AutoAdvanceAfter(AudioSource audioObject)
	{
		while (audioObject.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForSeconds(AutoAdvanceDelay);
		NextStory();
	}

	private void StopAutoAdvance()
	{
		if (_stepAutoAdvanceRoutine != null)
		{
			StopCoroutine(_stepAutoAdvanceRoutine);
			_stepAutoAdvanceRoutine = null;
		}
	}

	private void StopTextScrolling()
	{
		if (_stepTextScrollRoutine != null)
		{
			StopCoroutine(_stepTextScrollRoutine);
		}
	}

	public static string GetAudioKeyFromLocKey(string locKey)
	{
		if (locKey.StartsWith("FHS/"))
		{
			return locKey.Substring(4, locKey.Length - 4);
		}
		if (locKey.StartsWith("FH/"))
		{
			return locKey.Substring(3, locKey.Length - 3);
		}
		return locKey;
	}

	public static void CrossFadeAudio(MonoBehaviour coroutineHolder, AudioSource originalSource, AudioClip newClip, float maxDuration, out AudioSource tempCopy, out Coroutine coroutine)
	{
		tempCopy = null;
		coroutine = null;
		if (originalSource.clip == null || !originalSource.isPlaying)
		{
			originalSource.clip = newClip;
			originalSource.Play();
		}
		else
		{
			tempCopy = originalSource.gameObject.AddComponent<AudioSource>();
			coroutine = coroutineHolder.StartCoroutine(CrossFadeAudioCoroutine(originalSource, newClip, tempCopy, maxDuration));
		}
	}

	private static IEnumerator CrossFadeAudioCoroutine(AudioSource originalSource, AudioClip newClip, AudioSource tempCopy, float maxDuration)
	{
		tempCopy.clip = originalSource.clip;
		tempCopy.time = originalSource.time;
		tempCopy.volume = originalSource.volume;
		tempCopy.loop = originalSource.loop;
		tempCopy.pitch = originalSource.pitch;
		tempCopy.spatialBlend = originalSource.spatialBlend;
		tempCopy.outputAudioMixerGroup = originalSource.outputAudioMixerGroup;
		tempCopy.Play();
		originalSource.clip = newClip;
		originalSource.volume = 0f;
		originalSource.Play();
		float num = tempCopy.clip.length - tempCopy.time;
		float duration = Mathf.Min((num > 0f) ? num : 0.1f, maxDuration);
		float time = 0f;
		float targetVolume = tempCopy.volume;
		while (time < duration)
		{
			if (tempCopy == null || tempCopy.gameObject == null)
			{
				yield break;
			}
			float t = time / duration;
			tempCopy.volume = Mathf.Lerp(targetVolume, 0f, t);
			originalSource.volume = Mathf.Lerp(0f, targetVolume, t);
			time += Time.deltaTime;
			yield return null;
		}
		if (!(tempCopy == null) && !(tempCopy.gameObject == null))
		{
			tempCopy.Stop();
			originalSource.volume = targetVolume;
			UnityEngine.Object.Destroy(tempCopy);
		}
	}

	private IEnumerator ScrollText()
	{
		yield return new WaitForSeconds(TextScrollStartDelay);
		while (ScrollRect.normalizedPosition.y > 0f)
		{
			if (ScrollRect.velocity.y > 0f)
			{
				StopTextScrolling();
				StopAutoAdvance();
				yield break;
			}
			float height = ScrollRect.content.rect.height;
			float height2 = ScrollRect.viewport.rect.height;
			float scrollableHeight = height - height2;
			if (scrollableHeight < 0.001f)
			{
				yield return null;
			}
			ScrollRect.verticalNormalizedPosition -= TextScrollSpeed / scrollableHeight * Time.deltaTime;
			yield return null;
		}
		_stepTextScrollRoutine = null;
	}

	private void CancelSkip()
	{
		Skip.gameObject.SetActive(value: false);
		SkipProgress.fillAmount = 0f;
		_holdDuration = 0f;
		if (_skipRoutine != null)
		{
			StopCoroutine(_skipRoutine);
		}
		_skipRoutine = null;
	}

	private IEnumerator TrackSkip()
	{
		_holdDuration = 0f;
		while (_holdDuration < HoldDelay)
		{
			_holdDuration += Time.unscaledDeltaTime;
			if (_holdDuration < HoldDeadZone)
			{
				yield return null;
			}
			Skip.gameObject.SetActive(_holdDuration > HoldDeadZone);
			SkipProgress.fillAmount = _holdDuration / HoldDelay;
			yield return null;
		}
		Skip.gameObject.SetActive(value: false);
		_holdDuration = 0f;
		SkipNarration();
	}

	private void SkipNarration()
	{
		CancelSkip();
		StopAutoAdvance();
		StopMusic();
		StopVoiceOver();
		if (!Data.IsOnline)
		{
			EndNarration();
			return;
		}
		_currentStoryIndex = _narrationSettings.Story.Count - 1;
		UpdateStory(instant: true);
	}
}
