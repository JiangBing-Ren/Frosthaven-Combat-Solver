using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class UICampaignSettingsWindow : MonoBehaviour
{
	public struct Params
	{
		public List<CampaignSettingDTO> CampaignSpecific;

		public List<CampaignSettingDTO> General;

		public bool HasSettings
		{
			get
			{
				if (!CampaignSpecific.Any((CampaignSettingDTO dto) => !dto.IsCaption && !dto.IsDescription))
				{
					return General.Any((CampaignSettingDTO dto) => !dto.IsCaption && !dto.IsDescription);
				}
				return true;
			}
		}
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_CampaignSpecificSettingsContainer;

	[SerializeField]
	private RectTransform m_GeneralSettingsContainer;

	[SerializeField]
	private TMP_Text Text_ExtednedDecrition;

	[SerializeField]
	private FHButton Button_Confirm;

	[SerializeField]
	private FHButton Button_RestoreDefaults;

	[SerializeField]
	private FHButton Button_Close;

	[Header("Prototypes")]
	[SerializeField]
	private GameObject Prototype_Slider;

	[SerializeField]
	private GameObject Prototype_Toggle;

	[SerializeField]
	private GameObject Prototype_Dropdown;

	[SerializeField]
	private GameObject Prototype_Caption;

	[SerializeField]
	private GameObject Prototype_CaptionCampaignSpecific;

	[SerializeField]
	private GameObject Prototype_Description;

	private Params Data;

	private List<GameObject> m_SettingRows = new List<GameObject>();

	private UICampaignSetting m_Selected;

	private UICampaignSetting m_Focused;

	private UIWindow m_Window;

	private bool m_Initialize;

	private void InitCache()
	{
		if (!m_Initialize)
		{
			m_Initialize = true;
			if ((object)m_Window == null)
			{
				m_Window = GetComponent<UIWindow>();
			}
			if ((bool)Button_Confirm)
			{
				Button_Confirm.onClick.AddListener(HandleOnConfirm);
			}
			if ((bool)Button_RestoreDefaults)
			{
				Button_RestoreDefaults.onClick.AddListener(HandleOnRestore);
			}
			if ((bool)Button_Close)
			{
				Button_Close.onClick.AddListener(HandleOnClose);
			}
			RemovePlaceholders(m_CampaignSpecificSettingsContainer);
			RemovePlaceholders(m_GeneralSettingsContainer);
		}
		static void RemovePlaceholders(RectTransform container)
		{
			for (int num = container.childCount - 1; num >= 0; num--)
			{
				GameObject gameObject = container.GetChild(num).gameObject;
				LayoutElement component = gameObject.GetComponent<LayoutElement>();
				if ((object)component == null || !component.ignoreLayout)
				{
					Object.Destroy(gameObject);
				}
			}
		}
	}

	public void Init(Params @params)
	{
		Data = @params;
		Rebuild();
	}

	private void Rebuild()
	{
		InitCache();
		PopulateSettings();
		UpdateButtonsState();
	}

	private void SelectSetting(UICampaignSetting setting, bool withoutNotify = false)
	{
		m_Selected = setting;
		if (!withoutNotify)
		{
			setting.Select();
		}
		UpdateExtendedDescription();
	}

	private void UpdateExtendedDescription()
	{
		if (Text_ExtednedDecrition == null)
		{
			return;
		}
		CampaignSettingDTO campaignSettingDTO = m_Focused?.Data ?? m_Selected?.Data;
		if (campaignSettingDTO == null)
		{
			Text_ExtednedDecrition.text = string.Empty;
			return;
		}
		string text = campaignSettingDTO.DescriptionLocalized;
		Text_ExtednedDecrition.text = campaignSettingDTO.DescriptionLocalized;
		if (campaignSettingDTO.HasOptions())
		{
			for (int i = 0; i < campaignSettingDTO.Options.Length; i++)
			{
				CampaignSettingDTO.Option option = campaignSettingDTO.Options[i];
				text = text + "\n\n " + option.NameLocalized + (option.IsDefault ? "(Default)" : "") + " \n " + option.DescriptionLocalized;
			}
		}
		Text_ExtednedDecrition.text = text;
	}

	private void PopulateSettings()
	{
		ClearSettingsContainer();
		for (int i = 0; i < Data.CampaignSpecific.Count; i++)
		{
			CreateSetting(Data.CampaignSpecific[i], m_CampaignSpecificSettingsContainer, campaignSpecific: true);
		}
		for (int j = 0; j < Data.General.Count; j++)
		{
			CreateSetting(Data.General[j], m_GeneralSettingsContainer, campaignSpecific: false);
		}
	}

	private void CreateSetting(CampaignSettingDTO setting, RectTransform container, bool campaignSpecific)
	{
		if (setting.IsCaption)
		{
			m_SettingRows.Add(BuildCaption(setting, campaignSpecific ? Prototype_CaptionCampaignSpecific : Prototype_Caption, container));
			return;
		}
		if (setting.IsDescription)
		{
			m_SettingRows.Add(BuildCaption(setting, Prototype_Description, container));
			return;
		}
		if (setting.HasOptions())
		{
			m_SettingRows.Add(BuildSetting<UICampaignSettingDropdown>(setting, Prototype_Dropdown, container).gameObject);
			return;
		}
		object current = setting.Value.Current;
		if (!(current is bool))
		{
			if (current is float || current is int)
			{
				m_SettingRows.Add(BuildSetting<UICampaignSettingSlider>(setting, Prototype_Slider, container).gameObject);
			}
			else
			{
				Log.GUIError($"Fail to create setting instance for setting {setting}");
			}
		}
		else
		{
			m_SettingRows.Add(BuildSetting<UICampaignSettingToggle>(setting, Prototype_Toggle, container).gameObject);
		}
	}

	private UICampaignSetting BuildSetting<T>(CampaignSettingDTO setting, GameObject prototype, RectTransform container) where T : UICampaignSetting
	{
		T component = Object.Instantiate(prototype, container).GetComponent<T>();
		component.Init(setting, HandleOnSettingChange);
		component.OnFocusGain = HandleOnSettingsFocusGain;
		component.OnFocusLoss = HandleOnSettingsFocusLoss;
		component.OnSelected = HandleOnSettingsSelected;
		return component;
	}

	private GameObject BuildCaption(CampaignSettingDTO setting, GameObject prototype, RectTransform container)
	{
		GameObject gameObject = Object.Instantiate(prototype, container);
		gameObject.GetComponentInChildren<TMP_Text>().text = setting.NameLocalized;
		return gameObject;
	}

	private void UpdateButtonsState()
	{
		if ((bool)Button_Confirm)
		{
			Button_Confirm.interactable = GameSettingsService.HasModifiedSettings(Data.CampaignSpecific) || GameSettingsService.HasModifiedSettings(Data.General);
		}
		if ((bool)Button_RestoreDefaults)
		{
			Button_RestoreDefaults.interactable = GameSettingsService.HasNonDefaultSettings(Data.CampaignSpecific) || GameSettingsService.HasNonDefaultSettings(Data.General);
		}
	}

	public void Show()
	{
		if ((object)m_Window != null)
		{
			m_Window.Show();
		}
		else
		{
			gameObject.SetActive(value: true);
		}
		Rebuild();
	}

	public void Hide()
	{
		Clear();
		if (m_Window != null)
		{
			m_Window.Hide();
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	private void ClearSettingsContainer()
	{
		if (m_SettingRows != null && m_SettingRows.Count != 0)
		{
			for (int i = 0; i < m_SettingRows.Count; i++)
			{
				Object.Destroy(m_SettingRows[i].gameObject);
			}
		}
	}

	private void RestoreDefaultSettings()
	{
		foreach (CampaignSettingDTO item in Data.CampaignSpecific.Concat(Data.General))
		{
			if (!item.IsCaption && !item.IsDescription)
			{
				item.Value.Current = item.Value.Default;
				item.OnValueChange?.Invoke(item.Value.Default);
			}
		}
		PopulateSettings();
		UpdateButtonsState();
	}

	private void HandleOnSettingsSelected(UICampaignSetting setting)
	{
		SelectSetting(setting, withoutNotify: true);
	}

	private void HandleOnSettingsFocusGain(UICampaignSetting setting)
	{
		m_Focused = setting;
		UpdateExtendedDescription();
	}

	private void HandleOnSettingsFocusLoss(UICampaignSetting setting)
	{
		if (m_Focused == setting)
		{
			m_Focused = null;
		}
		UpdateExtendedDescription();
	}

	private void HandleOnSettingChange(CampaignSettingDTO settingDto, object newValue)
	{
		settingDto.Value.Current = newValue;
		PopulateSettings();
		UpdateButtonsState();
		settingDto.OnValueChange?.Invoke(newValue);
	}

	private void HandleOnConfirm()
	{
		Hide();
	}

	private void HandleOnRestore()
	{
		RestoreDefaultSettings();
	}

	private void HandleOnClose()
	{
		Hide();
	}

	private void Clear()
	{
		ClearSettingsContainer();
		m_SettingRows.Clear();
		m_Selected = null;
		m_Focused = null;
	}
}
