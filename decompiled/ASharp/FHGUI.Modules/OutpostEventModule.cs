using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClockStone;
using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class OutpostEventModule : FHMapViewModule
{
	[Header("Prototypes")]
	[SerializeField]
	private FHButton m_OptionButtonPrototype;

	[SerializeField]
	private GameObject m_EffectPrototype;

	[Header("Visuals")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private Image m_EventTypeIcon;

	[SerializeField]
	private Image m_FadeInIllustration;

	[SerializeField]
	private Image m_FadeOutBackground;

	[SerializeField]
	private TMP_Text m_OptionPrompt;

	[SerializeField]
	private TMP_Text m_EventTypeText;

	[SerializeField]
	private TMP_Text m_EventID;

	[SerializeField]
	private FHButton m_ProceedButton;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer m_ReadyUsersListContainer;

	[Header("Containers")]
	[SerializeField]
	private RectTransform m_OptionsContainer;

	[SerializeField]
	private RectTransform m_EffectsContainer;

	[Header("Settings")]
	[SerializeField]
	private int m_MinSlotCount;

	[SerializeField]
	private Color m_ColorOptionTextNormal;

	[SerializeField]
	private Color m_ColorOptionTextDisabled;

	[Header("Layout")]
	[SerializeField]
	private LayoutElement m_DescriptionContentLayoutElement;

	private List<FHButton> m_Buttons = new List<FHButton>(5);

	private List<GameObject> m_Effects = new List<GameObject>(5);

	private AudioObject _narrationMusic;

	private AudioSource _voSrc;

	private AudioSource _sfxSrc;

	private Coroutine _voCoroutine;

	private List<string> _curVOs = new List<string>();

	private float m_DefaultContentLayoutElementMinHeight;

	public Action<int> OnOptionSelected;

	public Action OnDone;

	private AudioSource _crossfadeSfx;

	private Coroutine _crossfadeSfxCoroutine;

	public OutpostEventDTO Data { get; private set; }

	private void Awake()
	{
		foreach (RectTransform item2 in m_OptionsContainer)
		{
			FHButton component = item2.GetComponent<FHButton>();
			if (!(component == null))
			{
				m_Buttons.Add(component);
			}
		}
		foreach (RectTransform item3 in m_EffectsContainer)
		{
			GameObject item = item3.gameObject;
			m_Effects.Add(item);
		}
		if ((bool)m_DescriptionContentLayoutElement)
		{
			m_DefaultContentLayoutElementMinHeight = m_DescriptionContentLayoutElement.minHeight;
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		StopNarration();
	}

	public void Init(OutpostEventDTO dto)
	{
		Data = dto;
		PopulateVisuals();
		PopulateOptions();
		PopulateEffects();
		UpdateLayout();
		PlayNarration();
	}

	private void PopulateVisuals()
	{
		if ((object)m_Illustration != null)
		{
			m_Illustration.overrideSprite = Data.Illustration;
		}
		if ((object)m_Description != null)
		{
			m_Description.text = Data.Text;
		}
		if ((object)m_EventTypeIcon != null)
		{
			m_EventTypeIcon.overrideSprite = Data.Icon;
		}
		if ((object)m_EventTypeText != null)
		{
			m_EventTypeText.text = Data.EventType;
		}
		if ((object)m_EventID != null)
		{
			m_EventID.text = Data.ID;
		}
		if ((object)m_OptionPrompt != null)
		{
			m_OptionPrompt.gameObject.SetActive(!string.IsNullOrEmpty(Data.OptionPromp));
			m_OptionPrompt.text = Data.OptionPromp;
		}
		PopulateFadeInIllustration();
	}

	private void PopulateFadeInIllustration()
	{
		if (Data.FadeInIllustration != null)
		{
			StartCoroutine(FadeInAdditionalIllustration(3));
			return;
		}
		if (m_FadeOutBackground != null)
		{
			m_FadeOutBackground.gameObject.SetActive(value: false);
		}
		if (m_FadeInIllustration != null)
		{
			m_FadeInIllustration.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator FadeInAdditionalIllustration(int delay)
	{
		if (Data.Options != null && Data.Options.Count > 0)
		{
			if (m_FadeOutBackground != null)
			{
				m_FadeOutBackground.gameObject.SetActive(value: false);
			}
			if (m_FadeInIllustration != null)
			{
				m_FadeInIllustration.gameObject.SetActive(value: false);
			}
			yield return new WaitForSeconds(delay);
		}
		if (m_FadeInIllustration != null)
		{
			m_FadeInIllustration.gameObject.SetActive(value: true);
			m_FadeInIllustration.overrideSprite = Data.FadeInIllustration;
		}
		if (m_FadeOutBackground != null)
		{
			m_FadeOutBackground.gameObject.SetActive(value: true);
		}
	}

	private void PopulateOptions()
	{
		if (Data.Options.Count == 0)
		{
			m_OptionsContainer.gameObject.SetActive(value: false);
			if (Data.NoButtonsAtAll)
			{
				m_ProceedButton.gameObject.SetActive(value: false);
			}
			else
			{
				ShowConfirmButton();
			}
			return;
		}
		m_ProceedButton.gameObject.SetActive(value: false);
		m_OptionsContainer.gameObject.SetActive(value: true);
		int num = Mathf.Max(Data.Options.Count);
		UIHelperUtil.UpdateUIList(num, m_Buttons, m_OptionButtonPrototype, m_OptionsContainer);
		for (int i = 0; i < num; i++)
		{
			OutpostEventDTO.Option option = ((i < Data.Options.Count) ? Data.Options[i] : null);
			FHButton fHButton = m_Buttons[i];
			bool active = i < num;
			bool flag = option != null;
			fHButton.gameObject.SetActive(active);
			fHButton.onClick.RemoveAllListeners();
			TMP_Text componentInChildren = fHButton.GetComponentInChildren<TMP_Text>();
			componentInChildren.enabled = flag;
			fHButton.interactable = flag && option.Interactable;
			if (option != null)
			{
				int id = (int)option.ID;
				componentInChildren.text = option.Text;
				componentInChildren.color = (fHButton.interactable ? m_ColorOptionTextNormal : m_ColorOptionTextDisabled);
				fHButton.onClick.AddListener(() =>
				{
					OnOptionSelected?.Invoke(id);
				});
				fHButton.interactable = option.Interactable;
				UIUsersListContainer componentInChildren2 = fHButton.GetComponentInChildren<UIUsersListContainer>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.Init(option.SuggestingUsers);
				}
			}
		}
	}

	private void UpdateLayout()
	{
		if ((bool)m_DescriptionContentLayoutElement)
		{
			bool flag = Data.Effects.Any((OutpostEventDTO.Effect effect) => !effect.Hidden);
			m_DescriptionContentLayoutElement.minHeight = (flag ? m_DefaultContentLayoutElementMinHeight : (-1f));
		}
	}

	private void PopulateEffects()
	{
		List<OutpostEventDTO.Effect> list = Data.Effects.Where((OutpostEventDTO.Effect effect2) => !effect2.Hidden).ToList();
		int count = list.Count;
		m_EffectsContainer.gameObject.SetActive(count > 0);
		UIHelperUtil.UpdateUIList(count, m_Effects, m_EffectPrototype, m_EffectsContainer);
		for (int num = 0; num < m_Effects.Count; num++)
		{
			OutpostEventDTO.Effect effect = ((num < list.Count) ? list[num] : null);
			GameObject gameObject = m_Effects[num];
			gameObject.gameObject.SetActive(effect != null);
			if (effect != null)
			{
				TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if ((object)componentInChildren != null)
				{
					componentInChildren.text = effect.Text;
				}
			}
		}
	}

	private void PlayNarration()
	{
		if (!string.IsNullOrEmpty(Data.Voiceover) && (bool)SingletonMonoBehaviour<AudioController>.Instance && (_curVOs == null || !_curVOs.SequenceEqual(Data.Voiceovers)))
		{
			if (_voCoroutine != null)
			{
				StopCoroutine(_voCoroutine);
			}
			_curVOs = Data.Voiceovers;
			_voCoroutine = StartCoroutine(PlayVoiceOver(Data.Voiceovers));
		}
	}

	private void SyncVoiceOverVolume()
	{
	}

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_eventnarrations", filename, "eventnarrations", "wav", null, suppressError: true);
	}

	private IEnumerator PlayVoiceOver(List<string> narrativeAudioIds)
	{
		foreach (string narrativeAudioId in narrativeAudioIds)
		{
			if (string.IsNullOrEmpty(narrativeAudioId))
			{
				yield break;
			}
			string audioKey = NarrationModalModule.GetAudioKeyFromLocKey(narrativeAudioId);
			if (_voSrc != null && _voSrc.clip != null && _voSrc.clip.name.StartsWith(audioKey))
			{
				yield break;
			}
			StopNarration(vo: true, sfx: false);
			yield return new WaitForSeconds(0.5f);
			if (_sfxSrc == null)
			{
				AudioSource[] components = GetComponents<AudioSource>();
				if (components.Length < 2)
				{
					_voSrc = gameObject.AddComponent<AudioSource>();
					_sfxSrc = gameObject.AddComponent<AudioSource>();
				}
				else
				{
					_voSrc = components[0];
					_sfxSrc = components[1];
				}
			}
			if (SaveData.Instance != null)
			{
				float num = 0.01f * (float)SaveData.Instance.Global.MasterVolume;
				_voSrc.volume = 0.6f * num * 0.01f * (float)SaveData.Instance.Global.StoryVolume;
				_sfxSrc.volume = num * 0.01f * (float)SaveData.Instance.Global.SFXVolume;
			}
			else
			{
				_voSrc.volume = 0.2f;
				_sfxSrc.volume = 0.2f;
			}
			AudioClip audioClip = LoadNarrationAudioClip(audioKey + "_VO");
			if ((bool)audioClip)
			{
				_voSrc.clip = audioClip;
				_voSrc.Play();
			}
			AudioClip newClip = LoadNarrationAudioClip(audioKey + "_SFX");
			if ((bool)audioClip)
			{
				NarrationModalModule.CrossFadeAudio(this, _sfxSrc, newClip, 3f, out _crossfadeSfx, out _crossfadeSfxCoroutine);
			}
			SyncVoiceOverVolume();
			while (_voSrc.isPlaying)
			{
				yield return new WaitForEndOfFrame();
			}
		}
	}

	private void StopNarration(bool vo = true, bool sfx = true)
	{
		if (_crossfadeSfx != null)
		{
			_crossfadeSfx.Stop();
			UnityEngine.Object.Destroy(_crossfadeSfx);
			_crossfadeSfx = null;
		}
		if (_crossfadeSfxCoroutine != null)
		{
			StopCoroutine(_crossfadeSfxCoroutine);
			_crossfadeSfxCoroutine = null;
		}
		if ((bool)_sfxSrc & sfx)
		{
			_sfxSrc.Stop();
			_sfxSrc.clip = null;
		}
		if ((bool)_voSrc & vo)
		{
			_voSrc.Stop();
			_voSrc.clip = null;
		}
	}

	private void StopMusic()
	{
		if (_narrationMusic != null)
		{
			_narrationMusic.Stop(1f);
		}
	}

	private void ShowConfirmButton()
	{
		m_ProceedButton.gameObject.SetActive(value: true);
		m_ProceedButton.interactable = Data.IsProceedInteractable;
		m_ProceedButton.onClick.RemoveAllListeners();
		m_ProceedButton.onClick.AddListener(() =>
		{
			OnDone?.Invoke();
			m_ProceedButton.interactable = false;
		});
		m_ProceedButton.SetTextKey(Data.LastInChain ? "GUI_COMPLETE" : "GUI_CONTINUE");
		if (m_ReadyUsersListContainer != null)
		{
			m_ReadyUsersListContainer.Init(Data.ReadyUsers);
		}
	}
}
