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

public class LinkingSectionModuleModule : FHMapViewModule
{
	[Header("Prototypes")]
	[SerializeField]
	private GameObject m_EffectPrototype;

	[SerializeField]
	private FHButton m_OptionButtonPrototype;

	[Header("Visuals")]
	[SerializeField]
	private TMP_Text m_SectionName;

	[SerializeField]
	private TMP_Text m_SectionDescription;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private FHButton m_ProceedButton;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer m_ReadyUsersListContainer;

	[Header("Containers")]
	[SerializeField]
	private RectTransform m_EffectsContainer;

	[SerializeField]
	private RectTransform m_OptionsContainer;

	[Header("Settings")]
	[SerializeField]
	private Color m_ColorOptionTextNormal;

	[SerializeField]
	private Color m_ColorOptionTextDisabled;

	[Header("Layout")]
	[SerializeField]
	private LayoutElement m_DescriptionContentLayoutElement;

	private List<FHButton> m_Buttons = new List<FHButton>(5);

	private List<GameObject> m_Effects = new List<GameObject>(5);

	private string m_CurrentVoiceover;

	private float m_DefaultContentLayoutElementMinHeight;

	private AudioSource _voSrc;

	private AudioSource _sfxSrc;

	public Action<int, OutpostEventDTO.Option> OnOptionSelected;

	public Action OnDone;

	private bool m_Initialized;

	public LinkingSectionDTO Data { get; private set; }

	private void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		foreach (RectTransform item2 in m_EffectsContainer)
		{
			GameObject item = item2.gameObject;
			m_Effects.Add(item);
		}
		foreach (RectTransform item3 in m_OptionsContainer)
		{
			FHButton component = item3.GetComponent<FHButton>();
			if (!(component == null))
			{
				m_Buttons.Add(component);
			}
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

	public void Init(LinkingSectionDTO dto)
	{
		InitCache();
		Data = dto;
		PopulateVisuals();
		PopulateEffects();
		PopulateOptions();
		UpdateLayout();
		PlayNarration();
	}

	private void PopulateVisuals()
	{
		if ((object)m_SectionName != null)
		{
			m_SectionName.text = Data.Caption;
		}
		if ((object)m_SectionDescription != null)
		{
			m_SectionDescription.text = Data.Text;
		}
		if ((object)m_Illustration != null)
		{
			m_Illustration.overrideSprite = Data.Illustration;
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

	private void PopulateOptions()
	{
		if (Data.Options.Count == 0)
		{
			m_OptionsContainer.gameObject.SetActive(value: false);
			ShowConfirmButton();
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
					OnOptionSelected?.Invoke(id, option);
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

	private void PlayNarration()
	{
		if (!string.IsNullOrEmpty(Data.Voiceover) && (bool)SingletonMonoBehaviour<AudioController>.Instance)
		{
			StartCoroutine(PlayVoiceOver(Data.Voiceover));
		}
	}

	private void SyncVoiceOverVolume()
	{
	}

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_eventnarrations", filename, "eventnarrations", "wav", null, suppressError: true);
	}

	private IEnumerator PlayVoiceOver(string narrativeAudioId)
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
		StopNarration();
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
		AudioClip clip = LoadNarrationAudioClip(audioKey + "_SFX");
		if ((bool)audioClip)
		{
			_sfxSrc.clip = clip;
			_sfxSrc.Play();
		}
		SyncVoiceOverVolume();
	}

	private void StopNarration()
	{
		if ((bool)_sfxSrc)
		{
			_sfxSrc.Stop();
			_sfxSrc.clip = null;
		}
		if ((bool)_voSrc)
		{
			_voSrc.Stop();
			_voSrc.clip = null;
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
		});
		m_ProceedButton.SetTextKey("GUI_CONTINUE");
		if (m_ReadyUsersListContainer != null)
		{
			m_ReadyUsersListContainer.Init(Data.ReadyUsers);
		}
	}
}
