using System;
using System.Collections.Generic;
using FFSNet;
using FHGUI.DTO;
using GLOOM;

namespace FHGUI;

public static class GameSettingsService
{
	public static ERulesFlag ValidCampaignRules => ERulesFlag.None;

	public static UICampaignSettingsWindow.Params GetSettingsDTO()
	{
		bool flag = SaveData.Instance.Global.CurrentGameState == EGameState.Map;
		if (FFSNetwork.IsShuttingDown || FFSNetwork.IsStartingUp)
		{
			flag = false;
		}
		flag = flag && ((!FFSNetwork.IsHost) ? (!FFSNetwork.IsOnline) : (PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0));
		ERulesFlag rules = (SaveData.Instance?.Global?.CurrentAdventureData?.Rules).GetValueOrDefault();
		return PopulateSettingsDTO(rules, flag, (ERulesFlag f, bool b) =>
		{
			PartyAdventureData currentAdventureData = SaveData.Instance.Global.CurrentAdventureData;
			if (b)
			{
				currentAdventureData.ChangeCampaignRules(rules | f);
			}
			else
			{
				currentAdventureData.ChangeCampaignRules(rules & ~f);
			}
		});
	}

	public static void ClientSetRules(ERulesFlag rules)
	{
		SaveData.Instance.Global.CurrentAdventureData.ChangeCampaignRules(rules, networkAction: false);
	}

	public static UICampaignSettingsWindow.Params PopulateSettingsDTO(ERulesFlag rules, bool interactable, Action<ERulesFlag, bool> onRuleChangeCallback)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (ERulesFlag value in Enum.GetValues(typeof(ERulesFlag)))
		{
			if (value != ERulesFlag.None && ValidCampaignRules.HasFlag(value))
			{
				if (IsHouseRule(value))
				{
					flag2 = true;
				}
				else
				{
					flag = true;
				}
			}
		}
		List<CampaignSettingDTO> list = new List<CampaignSettingDTO>();
		if (flag)
		{
			list.Add(new CampaignSettingDTO
			{
				NameLocalized = LocalizationManager.GetTranslation("GUI_CAMPAIGN_SPECIFIC_RULES_TITLE"),
				IsCaption = true
			});
			list.Add(new CampaignSettingDTO
			{
				NameLocalized = LocalizationManager.GetTranslation("GUI_CAMPAIGN_SPECIFIC_RULES_DESC"),
				IsDescription = true
			});
			foreach (ERulesFlag value2 in Enum.GetValues(typeof(ERulesFlag)))
			{
				if (value2 != ERulesFlag.None && ValidCampaignRules.HasFlag(value2) && !IsHouseRule(value2))
				{
					list.Add(GetRuleDTO(value2, rules, interactable && SaveData.Instance.Global.CurrentGameState == EGameState.None, onRuleChangeCallback));
				}
			}
		}
		List<CampaignSettingDTO> list2 = new List<CampaignSettingDTO>();
		if (flag2)
		{
			list2.Add(new CampaignSettingDTO
			{
				NameLocalized = LocalizationManager.GetTranslation("GUI_GENERAL_RULES_TITLE"),
				IsCaption = true
			});
			list2.Add(new CampaignSettingDTO
			{
				NameLocalized = LocalizationManager.GetTranslation("GUI_GENERAL_RULES_DESC"),
				IsDescription = true
			});
			foreach (ERulesFlag value3 in Enum.GetValues(typeof(ERulesFlag)))
			{
				if (value3 != ERulesFlag.None && ValidCampaignRules.HasFlag(value3) && IsHouseRule(value3))
				{
					list2.Add(GetRuleDTO(value3, rules, interactable, onRuleChangeCallback));
				}
			}
		}
		return new UICampaignSettingsWindow.Params
		{
			CampaignSpecific = list,
			General = list2
		};
	}

	private static CampaignSettingDTO GetRuleDTO(ERulesFlag rule, ERulesFlag rules, bool interactable, Action<ERulesFlag, bool> onValueChangeCallback)
	{
		return new CampaignSettingDTO
		{
			NameLocalized = LocalizationManager.GetTranslation("CampaignRule_" + rule.ToString() + "_Name"),
			DescriptionLocalized = LocalizationManager.GetTranslation("CampaignRule_" + rule.ToString() + "_Desc"),
			TooltipLocalized = LocalizationManager.GetTranslation("CampaignRule_" + rule.ToString() + "_Desc"),
			Value = new CampaignSettingDTO.SettingValue(rules.HasFlag(rule), false),
			Interactable = interactable,
			OnValueChange = (object result) =>
			{
				if (result is bool arg)
				{
					onValueChangeCallback(rule, arg);
				}
			}
		};
	}

	private static bool IsHouseRule(ERulesFlag ruleFlag)
	{
		switch (ruleFlag)
		{
		case ERulesFlag.ReducedRandomness:
		case ERulesFlag.AutoOutpost:
		case ERulesFlag.AutoCardSelect:
		case ERulesFlag.CanRespecCharacters:
			return true;
		default:
			return false;
		}
	}

	public static bool HasNonDefaultSettings(List<CampaignSettingDTO> settings)
	{
		for (int i = 0; i < settings.Count; i++)
		{
			CampaignSettingDTO campaignSettingDTO = settings[i];
			if (!campaignSettingDTO.IsCaption && !campaignSettingDTO.IsDescription && !campaignSettingDTO.Value.Current.Equals(campaignSettingDTO.Value.Default))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasModifiedSettings(List<CampaignSettingDTO> settings)
	{
		return true;
	}

	public static bool CanShowHouseRulesOptions()
	{
		if (SaveData.Instance?.Global == null || SaveData.Instance.Global.GameMode != EGameMode.Campaign)
		{
			return false;
		}
		foreach (ERulesFlag value in Enum.GetValues(typeof(ERulesFlag)))
		{
			if (value != ERulesFlag.None && ValidCampaignRules.HasFlag(value))
			{
				return true;
			}
		}
		return false;
	}
}
