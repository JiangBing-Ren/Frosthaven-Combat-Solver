using System;
using System.Collections;
using System.Collections.Generic;
using ClockStone;
using FHGUI.DTO;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class RetirementStoryModule : FHMapViewModule
{
	public struct Params
	{
		public Sprite Illustration;

		public Sprite ClassIcon;

		public string StoryKey;

		public List<OutpostEventDTO.Effect> Rewards;

		public Action OnDone;
	}

	[Header("Visuals")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_HeroIllustration;

	[SerializeField]
	private Image m_HeroClassIcon;

	[SerializeField]
	private FHButton m_ProceedButton;

	[Header("Rewards")]
	[SerializeField]
	private RectTransform m_ContainerRewards;

	[SerializeField]
	private GameObject m_RewardPrototype;

	private List<GameObject> m_Rewards = new List<GameObject>(5);

	private AudioSource _voSrc;

	private AudioSource _sfxSrc;

	private bool m_Initialized;

	private void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		foreach (RectTransform containerReward in m_ContainerRewards)
		{
			GameObject item = containerReward.gameObject;
			m_Rewards.Add(item);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		m_HeroIllustration.sprite = @params.Illustration;
		m_HeroClassIcon.sprite = @params.ClassIcon;
		m_Description.text = LocalizationManager.GetTranslation(@params.StoryKey);
		m_ProceedButton.onClick.RemoveAllListeners();
		m_ProceedButton.onClick.AddListener(() =>
		{
			@params.OnDone?.Invoke();
			StopNarration();
		});
		m_ProceedButton.interactable = true;
		PlayNarration(@params.StoryKey);
		PopulateRewards(@params);
	}

	private void PopulateRewards(Params @params)
	{
		int num = @params.Rewards?.Count ?? 0;
		m_ContainerRewards.gameObject.SetActive(num > 0);
		UIHelperUtil.UpdateUIList(num, m_Rewards, m_RewardPrototype, m_ContainerRewards);
		for (int i = 0; i < m_Rewards.Count; i++)
		{
			OutpostEventDTO.Effect effect = ((i < @params.Rewards?.Count) ? @params.Rewards[i] : null);
			GameObject gameObject = m_Rewards[i];
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

	private void PlayNarration(string narrativeAudioId)
	{
		if (!string.IsNullOrEmpty(narrativeAudioId) && (bool)SingletonMonoBehaviour<AudioController>.Instance)
		{
			StartCoroutine(PlayVoiceOver(narrativeAudioId));
		}
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

	private AudioClip LoadNarrationAudioClip(string filename)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<AudioClip>("misc_eventnarrations", filename, "eventnarrations", "wav", null, suppressError: true);
	}
}
