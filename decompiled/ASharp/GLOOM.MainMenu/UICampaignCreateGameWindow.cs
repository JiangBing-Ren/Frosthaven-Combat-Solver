using System;
using FHGUI;
using UnityEngine;

namespace GLOOM.MainMenu;

public class UICampaignCreateGameWindow : UICreateGameDataStep
{
	[Header("Campaign Settings")]
	[SerializeField]
	private UICampaignSettingsWindow m_CampaignSettings;

	[SerializeField]
	private FHButton m_OpenCampaignSettings;

	protected override void InitCache()
	{
		if (!m_Initialized)
		{
			base.InitCache();
			m_OpenCampaignSettings.onClick.AddListener(m_CampaignSettings.Show);
		}
	}

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		base.Setup(service, gameData, onConfirmed, onCancelled);
		UICampaignSettingsWindow.Params obj = GameSettingsService.PopulateSettingsDTO(gameData.Rules, interactable: true, (ERulesFlag rule, bool active) =>
		{
			if (active)
			{
				gameData.Rules |= rule;
			}
			else
			{
				gameData.Rules &= ~rule;
			}
		});
		m_CampaignSettings.Init(obj);
		m_OpenCampaignSettings.gameObject.SetActive(obj.HasSettings);
	}

	protected override GameData BuildGameModel(GameData data)
	{
		base.BuildGameModel(data);
		return data;
	}
}
