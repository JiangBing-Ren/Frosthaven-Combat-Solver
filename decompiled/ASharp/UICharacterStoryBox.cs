using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClockStone;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStoryBox : MonoBehaviour
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI dialogText;

	[SerializeField]
	private Image shieldIcon;

	[SerializeField]
	private string audioItemSkip = "PlaySound_UIDialogueBoxSkip";

	[SerializeField]
	private StoryImageViewer narrativeFlow;

	[SerializeField]
	private GUIAnimator showTextLineAnimator;

	[SerializeField]
	private GUIAnimator showDialogAnimator;

	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private FHToggle muteVoiceOverButton;

	[SerializeField]
	private LayoutElementExtended maxSizeContainer;

	[SerializeField]
	private int maxSize = 300;

	[SerializeField]
	private List<LayoutElement> horizontalSpaceLayouts;

	[SerializeField]
	private GameObject dismissIcon;

	private List<DialogLineDTO> dialogs;

	private int currentDialogIndex = -1;

	private Action onFinish;

	private AudioObject _narrationMusic;

	private AudioSource _narrationVO;

	private AudioSource _narrationSFX;

	private InteractabilityIsolatedUIControl interactabilityControl;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactabilityRef { get; private set; }

	public void SetupInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (interactabilityControl == control)
		{
			return;
		}
		interactabilityControl = control;
		if (interactabilityControl == null)
		{
			interactabilityControl = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (interactabilityControl != null)
		{
			interactabilityRef = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = interactabilityControl,
				onInteractabilityChanged = () =>
				{
					RefreshSkipButtonInteractability();
				}
			};
		}
	}

	private void RefreshSkipButtonInteractability()
	{
		if (dismissIcon != null)
		{
			dismissIcon.SetActive(value: true);
		}
		if (interactabilityRef != null)
		{
			bool cachedInteractable = interactabilityRef.cachedInteractable;
			skipButton.interactable = cachedInteractable;
			if (dismissIcon != null)
			{
				dismissIcon.SetActive(cachedInteractable);
			}
		}
	}

	private void Awake()
	{
		skipButton.onClick.AddListener(Skip);
		if ((bool)muteVoiceOverButton)
		{
			muteVoiceOverButton.onValueChanged.AddListener(OnNarrationToggle);
		}
		showDialogAnimator.OnAnimationFinished.AddListener(() =>
		{
			ShowLine(0);
		});
	}

	public void Show(List<DialogLineDTO> lines, Action onFinish = null)
	{
		dialogs = lines;
		this.onFinish = onFinish;
		currentDialogIndex = -1;
		skipButton.interactable = false;
		if (lines.Count == 0)
		{
			Hide();
			return;
		}
		maxSizeContainer.MaxHeight = maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(value: true);
		}
		showTextLineAnimator.GoInitState();
		if (lines[0].narrativeImageID.IsNOTNullOrEmpty())
		{
			showDialogAnimator.GoInitState();
			narrativeFlow.LoadImages((from it in lines
				where it.narrativeImageID.IsNOTNullOrEmpty()
				select it.narrativeImageID).ToList(), () =>
			{
				narrativeFlow.Show(lines[0].narrativeImageID, ShowDialog);
			});
		}
		else
		{
			narrativeFlow.Hide();
			ShowDialog();
		}
	}

	private void ShowDialog()
	{
		dialogText.text = LocalizationManager.GetTranslation(dialogs[0].text);
		DecorateBox(dialogs[0]);
		if ((bool)muteVoiceOverButton)
		{
			muteVoiceOverButton.SetIsOnWithoutNotify(SaveData.Instance.Global.MuteStoryVoiceOver);
		}
		showDialogAnimator.Play();
	}

	private void ShowLine(int dialogIndex)
	{
		if (dialogIndex >= dialogs.Count)
		{
			Hide();
			return;
		}
		maxSizeContainer.MaxHeight = maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(value: true);
		}
		StopCurrentAudio();
		currentDialogIndex = dialogIndex;
		skipButton.interactable = true;
		DialogLineDTO line = dialogs[currentDialogIndex];
		DecorateBox(line);
		DecorateLine(line);
		showTextLineAnimator.Play();
		StartCoroutine(CheckSize());
	}

	private void StopCurrentAudio()
	{
		if (currentDialogIndex >= 0 && dialogs != null && currentDialogIndex < dialogs.Count && dialogs[currentDialogIndex].narrativeAudioId.IsNOTNullOrEmpty())
		{
			if ((bool)_narrationSFX)
			{
				_narrationSFX.Stop();
			}
			if ((bool)_narrationVO)
			{
				_narrationVO.Stop();
			}
			if (_narrationMusic != null)
			{
				_narrationMusic.Stop(1f);
			}
		}
	}

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		AudioClip audioClip = AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_storynarrations", filename, "storynarrations", "wav", null, suppressError: true);
		if (audioClip == null)
		{
			audioClip = AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_eventnarrations", filename, "eventnarrations", "wav", null, suppressError: true);
		}
		return audioClip;
	}

	private void DecorateLine(DialogLineDTO line)
	{
		dialogText.text = LocalizationManager.GetTranslation(line.text);
		if (line.cameraLocation.HasValue)
		{
			CameraController.s_CameraController.m_TargetFocalPoint = line.cameraLocation.Value;
		}
		if (line.narrativeImageID.IsNOTNullOrEmpty())
		{
			narrativeFlow.Show(line.narrativeImageID);
		}
		if (!line.narrativeAudioId.IsNOTNullOrEmpty())
		{
			return;
		}
		string audioKeyFromLocKey = GetAudioKeyFromLocKey(line.narrativeAudioId);
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
		AudioClip audioClip2 = LoadNarrationAudioClip(audioKeyFromLocKey + "_SFX");
		if ((bool)audioClip2)
		{
			_narrationSFX.clip = audioClip2;
			_narrationSFX.Play();
		}
		SyncVoiceOverVolume();
	}

	private string GetAudioKeyFromLocKey(string locKey)
	{
		if (locKey.StartsWith("FHS/"))
		{
			return locKey.Substring(4, locKey.Length - 4);
		}
		if (locKey.StartsWith("FH/"))
		{
			return locKey.Substring(3, locKey.Length - 3);
		}
		if (locKey.StartsWith("Tutorials/"))
		{
			return locKey.Substring(10, locKey.Length - 10);
		}
		return locKey;
	}

	private IEnumerator CheckSize()
	{
		yield return null;
		bool flag = (maxSizeContainer.transform as RectTransform).rect.height == (float)maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(!flag);
		}
		maxSizeContainer.MaxHeight = (flag ? (-1) : maxSize);
	}

	private void DecorateBox(DialogLineDTO line)
	{
		title.text = line.title;
		portrait.sprite = UIInfoTools.Instance.GetStoryCharacterExpression(line.character, line.expression) ?? portrait.sprite;
		shieldIcon.sprite = UIInfoTools.Instance.GetStoryCharacterShield(line.character) ?? shieldIcon.sprite;
	}

	private void OnNarrationToggle(bool toggleOn)
	{
		SaveData.Instance.Global.MuteStoryVoiceOver = toggleOn;
		SyncVoiceOverVolume();
		if (!SaveData.Instance.IsSavingData)
		{
			SaveData.Instance.SaveGlobalData();
		}
	}

	private void SyncVoiceOverVolume()
	{
		float num = 0.01f * (float)SaveData.Instance.Global.MasterVolume;
		bool muteStoryVoiceOver = SaveData.Instance.Global.MuteStoryVoiceOver;
		if (_narrationSFX != null && _narrationVO.isPlaying)
		{
			_narrationSFX.volume = (muteStoryVoiceOver ? 0f : (num * 0.01f * (float)SaveData.Instance.Global.SFXVolume));
		}
		if (_narrationVO != null && _narrationVO.isPlaying)
		{
			_narrationVO.volume = (muteStoryVoiceOver ? 0f : (num * 0.01f * (float)SaveData.Instance.Global.StoryVolume));
		}
	}

	private void ShowNextLine()
	{
		AudioControllerUtils.PlaySound(audioItemSkip);
		ShowLine(currentDialogIndex + 1);
	}

	public void Skip()
	{
		ShowNextLine();
	}

	public void Hide()
	{
		skipButton.interactable = false;
		OnHidden();
		onFinish?.Invoke();
	}

	private void OnHidden()
	{
		StopCurrentAudio();
		showDialogAnimator?.Stop();
		showTextLineAnimator?.Stop();
		narrativeFlow.Hide();
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			OnHidden();
		}
	}
}
