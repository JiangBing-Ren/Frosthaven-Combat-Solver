using System;
using FFSNet;
using FHGUI;
using UnityEngine;

public class HouseRulesSettings : MonoBehaviour
{
	[SerializeField]
	private UICampaignSettingsWindow Settings;

	private void OnEnable()
	{
		Refresh();
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Remove(PartyAdventureData.OnRulesChanged, new Action(Refresh));
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Combine(PartyAdventureData.OnRulesChanged, new Action(Refresh));
	}

	private void OnDisable()
	{
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Remove(PartyAdventureData.OnRulesChanged, new Action(Refresh));
	}

	private void Refresh()
	{
		Settings.Init(GameSettingsService.GetSettingsDTO());
	}

	public static void ProxySetHouseRuleSettings(GameAction action)
	{
		ERulesFlag dataInt = (ERulesFlag)action.DataInt;
		GameSettingsService.ClientSetRules(dataInt);
		Log.NetworkInfo("House rule setting switched to " + dataInt);
	}
}
