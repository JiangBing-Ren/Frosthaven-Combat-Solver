using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClockStone;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class ViewSectionModule : FHMapViewModule
{
	[Header("Prototypes")]
	[SerializeField]
	private GameObject m_EffectPrototype;

	[Header("Visuals")]
	[SerializeField]
	private TMP_Text m_SectionName;

	[SerializeField]
	private TMP_Text m_SectionDescription;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private FHButton m_BackButton;

	[SerializeField]
	private FHButton m_ProceedButton;

	[SerializeField]
	private FHButton m_CloseButton;

	[Header("Containers")]
	[SerializeField]
	private RectTransform m_EffectsContainer;

	[Header("Layout")]
	[SerializeField]
	private LayoutElement m_DescriptionContentLayoutElement;

	private string m_CurrentVoiceover;

	private float m_DefaultContentLayoutElementMinHeight;

	private List<GameObject> m_Effects = new List<GameObject>(5);

	private AudioSource _voSrc;

	private AudioSource _sfxSrc;

	private bool m_Initialized;

	private Action _onClose;

	private Action _onNext;

	private Action _onPrevious;

	public LinkingSectionDTO Data { get; private set; }

	private void Awake()
	{
		m_ProceedButton.onClick.AddListener(OnProceed);
		m_BackButton.onClick.AddListener(OnBack);
		m_CloseButton.onClick.AddListener(Close);
	}

	protected override void OnHide()
	{
		base.OnHide();
		StopNarration();
		_onClose = null;
		_onNext = null;
		_onPrevious = null;
	}

	public void Init(LinkingSectionDTO dto, Action onClose, Action onNext = null, Action onPrevious = null)
	{
		if ((bool)m_DescriptionContentLayoutElement)
		{
			m_DefaultContentLayoutElementMinHeight = m_DescriptionContentLayoutElement.minHeight;
		}
		Data = dto;
		_onClose = onClose;
		_onNext = onNext;
		_onPrevious = onPrevious;
		SetButtonState(onPrevious != null, onNext != null);
		PopulateVisuals();
		PopulateEffects();
		UpdateLayout();
		PlayNarration();
	}

	private void SetButtonState(bool canBack, bool canProceed)
	{
		bool flag = canBack | canProceed;
		m_BackButton.interactable = canBack;
		m_BackButton.gameObject.SetActive(flag);
		m_ProceedButton.interactable = flag;
		m_ProceedButton.gameObject.SetActive(flag);
		m_CloseButton.gameObject.SetActive(!flag);
		m_ProceedButton.SetTextKey((canBack && !canProceed) ? "GUI_CLOSE" : "GUI_CONTINUE");
	}

	private void OnProceed()
	{
		if (_onNext != null)
		{
			_onNext();
		}
		else
		{
			Close();
		}
	}

	private void OnBack()
	{
		_onPrevious?.Invoke();
	}

	public void Close()
	{
		_onClose?.Invoke();
		gameObject.SetActive(value: false);
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

	private void PlayNarration()
	{
		if (!string.IsNullOrEmpty(Data.Voiceover) && (bool)SingletonMonoBehaviour<AudioController>.Instance)
		{
			StartCoroutine(PlayVoiceOver(Data.Voiceover));
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
}
