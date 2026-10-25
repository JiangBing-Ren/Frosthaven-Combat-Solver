using System;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class UICampaignCard : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private FHToggle m_Toggle;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private Image m_TitleBackground;

	[SerializeField]
	private Image m_TextBackground;

	[SerializeField]
	private TextMeshProUGUI m_CampaignName;

	[SerializeField]
	private TextMeshProUGUI m_Caption;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[Header("Sprites")]
	[SerializeField]
	private Sprite _textBackgroundState_On;

	[SerializeField]
	private Sprite _textBackgroundState_Off;

	[SerializeField]
	private Sprite _titleBackgroundState_On;

	[SerializeField]
	private Sprite _titleBackgroundState_Off;

	[Header("Colors")]
	[SerializeField]
	private Color _descriptionState_On;

	[SerializeField]
	private Color _descriptionState_Off;

	[Header("Effects")]
	[SerializeField]
	private UIHoverResizeEffects _descriptionResizeEffects;

	[SerializeField]
	private TrueShadow _captionShadowEffects;

	public Action<UICampaignCard> OnSelect;

	private bool m_Initialzed;

	[field: SerializeField]
	public Selectable Selectable { get; private set; }

	public CampaignDTO Data { get; private set; }

	private void Awake()
	{
		UIAudioEffects component = GetComponent<UIAudioEffects>();
		if ((bool)component)
		{
			component.CanPlayClickSound = () => m_Toggle == null || !m_Toggle.isOn;
		}
	}

	private void Init()
	{
		if (!m_Initialzed)
		{
			m_Initialzed = true;
			m_Toggle.onValueChanged.AddListener(HandleOnToggle);
		}
	}

	public void Init(CampaignDTO dto)
	{
		Init();
		Data = dto;
		Populate();
		UpdateHighlights();
	}

	private void Populate()
	{
		if (Data != null)
		{
			m_CampaignName.text = LocalizationManager.GetTranslation(Data.NameKey);
			m_Caption.text = LocalizationManager.GetTranslation(Data.CaptionKey);
			m_Description.text = LocalizationManager.GetTranslation(Data.DescriptionKey);
			m_Illustration.material = (Data.IsAvailable ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
			m_Illustration.sprite = Data.Illustration;
		}
	}

	public void Select()
	{
		m_Toggle.Select();
		m_Toggle.SetIsOnWithoutNotify(value: true);
		UpdateHighlights();
	}

	public void Deselect()
	{
		m_Toggle.SetIsOnWithoutNotify(value: false);
		UpdateHighlights();
	}

	private void UpdateHighlights()
	{
		bool isOn = m_Toggle.isOn;
		if (m_TitleBackground != null)
		{
			m_TitleBackground.sprite = (isOn ? _titleBackgroundState_On : _titleBackgroundState_Off);
		}
		if (m_TextBackground != null)
		{
			m_TextBackground.sprite = (isOn ? _textBackgroundState_On : _textBackgroundState_Off);
		}
		if (m_Caption != null)
		{
			m_Caption.color = (isOn ? _descriptionState_On : _descriptionState_Off);
		}
		if (m_Description != null)
		{
			m_Description.color = (isOn ? _descriptionState_On : _descriptionState_Off);
		}
		if (m_Illustration != null)
		{
			m_Illustration.sprite = ((!isOn) ? Data?.Illustration : Data?.IllustrationActive);
		}
		if (_captionShadowEffects != null)
		{
			_captionShadowEffects.enabled = !isOn;
		}
		_descriptionResizeEffects.SetLock(isOn);
	}

	private void HandleOnToggle(bool value)
	{
		if (value)
		{
			OnSelect?.Invoke(this);
		}
		UpdateHighlights();
	}

	private void OnDisable()
	{
		Data = null;
	}
}
