using System;
using GLOOM;
using GLOOM.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIGameSaveCampaignRow : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TextMeshProUGUI Name;

	[SerializeField]
	private TextMeshProUGUI Date;

	[SerializeField]
	private FHButton Button;

	[SerializeField]
	private FHButton Button_Delete;

	[SerializeField]
	private GameObject Selected;

	[SerializeField]
	private Image CollapseArrow;

	[Header("Settings")]
	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm:ss";

	[SerializeField]
	private Color textColorNormal;

	[SerializeField]
	private Color textColorSelected;

	private IGameSaveData saveData;

	private bool isCurrentActiveCampaign;

	public Action<bool> OnCollapse;

	public Action<string> OnDeleteCampaign;

	private bool _selected;

	public bool Collapsed { get; private set; }

	private void Awake()
	{
		Button.onClick.AddListener(ToggleCollapsed);
		Button_Delete.onClick.AddListener(DeleteCampaign);
	}

	public void Init(IGameSaveData data, bool collapsed, bool isActive)
	{
		Name.text = data.CampaignName;
		isCurrentActiveCampaign = isActive;
		saveData = data;
		if ((bool)Date)
		{
			Date.text = data.LastSavedTimeStamp.ToString(formatDate);
		}
		_selected = false;
		Collapsed = collapsed;
		UpdateHighlight();
	}

	public void SetSelect(bool selected)
	{
		Button.Select();
		_selected = selected;
		UpdateHighlight();
	}

	private void UpdateHighlight()
	{
		Selected.SetActive(_selected);
		Date.gameObject.SetActive(Collapsed);
		Name.color = (_selected ? textColorSelected : textColorNormal);
		Date.color = (_selected ? textColorSelected : textColorNormal);
		CollapseArrow.transform.rotation = (Collapsed ? Quaternion.Euler(new Vector3(0f, 0f, 90f)) : Quaternion.Euler(new Vector3(0f, 0f, 0f)));
	}

	public void Collapse(bool collapsed)
	{
		Collapsed = collapsed;
		UpdateHighlight();
	}

	private void ToggleCollapsed()
	{
		Collapse(!Collapsed);
		OnCollapse?.Invoke(Collapsed);
	}

	private void DeleteCampaign()
	{
		if (isCurrentActiveCampaign)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericInfo(LocalizationManager.GetTranslation("GUI_DELETE_CAMPAIGN_TITLE"), LocalizationManager.GetTranslation("GUI_DELETE_CAMPAIGN_DISABLED_DESC"), null, "GUI_CONFIRM");
			return;
		}
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_DELETE_CAMPAIGN_TITLE"), LocalizationManager.GetTranslation("GUI_DELETE_CAMPAIGN_DESC"), () =>
		{
			OnDeleteCampaign?.Invoke(saveData.CampaignName);
		});
	}
}
