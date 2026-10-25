using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHRL;
using GLOOM.MainMenu;
using UnityEngine;

namespace FHGUI;

public class UISelectCampaignGameWindow : UICreateGameStep
{
	[Header("Components")]
	[SerializeField]
	private UICampaignSelector m_CampaignSelector;

	[Header("Effects")]
	[SerializeField]
	private GameObject m_HighlightConfirmButton;

	[SerializeField]
	private GUIAnimator m_EnableConfirmAnimator;

	protected override void Setup(IGameModeService service, GameData data, Action onConfirmed, Action onCancelled = null)
	{
		base.Setup(service, data, onConfirmed, onCancelled);
		EnableConfirmationButton(enable: true);
		m_CampaignSelector.Init(GetCampaignDTOs());
		m_CampaignSelector.OnSelectedCampaignChanged = OnSelectedCampaignChanged;
	}

	private void OnSelectedCampaignChanged()
	{
		m_Data.AddParam("GameModeID", m_CampaignSelector.SelectedCampaign.Id);
	}

	private async void Confirm()
	{
		if (await Validate())
		{
			OnConfirmedStep();
		}
	}

	private async void ConfirmRejected()
	{
		await Validate();
	}

	private static List<CampaignDTO> GetCampaignDTOs()
	{
		List<CampaignDTO> list = new List<CampaignDTO>();
		foreach (string validCampaign in CampaignSettings.ValidCampaigns)
		{
			list.Add(new CampaignDTO
			{
				Id = validCampaign,
				NameKey = "CAMPAIGN_" + validCampaign + "_Name",
				CaptionKey = "CAMPAIGN_" + validCampaign + "_Type",
				DescriptionKey = "CAMPAIGN_" + validCampaign + "_Description",
				Illustration = UIInfoTools.Instance.GetCampaignIllustration(validCampaign, active: false),
				IllustrationActive = UIInfoTools.Instance.GetCampaignIllustration(validCampaign, active: true),
				IsAvailable = true
			});
		}
		return list;
	}
}
