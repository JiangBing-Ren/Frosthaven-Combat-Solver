using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public static class UIMultiplayerNotifications
{
	public const string NOTIFICATION_TITLE = "GUI_MULTIPLAYER";

	private const string NOTIFICATION_TAG = "MULTIPLAYER";

	private const string ID_SELECTED_OTHER_PLAYER_CARD = "SelectedOtherPlayerCard";

	private const string ID_SELECTED_CARDS_WAITING_PLAYERS_JOIN = "SelectedCardWhileWaitingForPlayersToJoin";

	private const string ID_STARTED_SESSION = "StartedSession";

	private const string ID_SHARE_CODE = "ShareCode";

	private const string ID_WAIT_PLAYERS_JOIN = "WaitingPlayersJoin";

	private const string ID_WAIT_PLAYERS_JOINING = "WaitingPlayersJoining";

	private const string ID_GENERATE_CODE = "GenerateCode";

	private const string ID_WAIT_PLAYERS_CHAR_ASSIGNED = "WaitingPlayersCharacterAssigned";

	private const string ID_SELECTED_QUEST = "SelectedQuest";

	private const string ID_CANCEL_SELECTED_CARDS = "CancelSelectedCards";

	private const string ID_WAIT_MY_SLOTS_ASSIGNED = "WaitingAllMySlotsAssigned";

	private const string ID_CITY_EVENT = "CityEvent";

	private const string ID_TOWN_RECORDS = "TownRecords";

	private const string ID_TIMEOUT = "Timeout";

	public static void ShowPossibleTimeout()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("Timeout", new UINotificationManager.NotificationData
		{
			titleLoc = "GUI_MULTIPLAYER",
			message = LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_TIMEOUT_END_SESSION"),
			dataButton1 = new UINotificationManager.NotificationDataButton(() =>
			{
				if (FFSNetwork.IsHost)
				{
					FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
					if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
					{
						Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
					}
				}
				else
				{
					Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
				}
			}, "GUI_MULTIPLAYER_END_SESSION"),
			audioItem = UIInfoTools.Instance?.genericSystemMessageAudioItem
		}, "MULTIPLAYER");
	}

	public static void HidePossibleTimeout()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("Timeout");
	}

	public static void ShowPlayerDisconnected(NetworkPlayer player, bool showEndSessionOption)
	{
		if (showEndSessionOption)
		{
			Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
			{
				titleLoc = "GUI_MULTIPLAYER",
				message = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_DISCONNECTED_END_SESSION"), player.Username),
				dataButton1 = new UINotificationManager.NotificationDataButton(() =>
				{
					FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
					if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
					{
						Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
					}
				}, "GUI_MULTIPLAYER_END_SESSION"),
				audioItem = UIInfoTools.Instance?.playerDisconnectedAudioItem
			}, UIInfoTools.Instance.disconnectedPlayerNotificationDuration, null, "MULTIPLAYER");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<NetworkControllable> list = player.MyControllables.ToList();
		int count = list.Count;
		for (int num = 0; num < count; num++)
		{
			if (num > 0)
			{
				stringBuilder.Append((num == count - 1) ? LocalizationManager.GetTranslation("AND") : ", ");
			}
			if (AdventureState.MapState.IsCampaign)
			{
				CMapCharacter mapCharacterByControllerID = ControllableRegistry.GetMapCharacterByControllerID(list[num].ID);
				stringBuilder.Append(LocalizationManager.GetTranslation(mapCharacterByControllerID.CharacterYMLData.LocKey.ToString()));
			}
			else
			{
				stringBuilder.Append(LocalizationManager.GetTranslation(((ECharacter)list[num].ID/*cast due to constrained. prefix*/).ToString()));
			}
		}
		string message = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_DISCONNECTED"), player.Username, PlayerRegistry.HostPlayer.Username, stringBuilder);
		Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = "GUI_MULTIPLAYER",
			message = message,
			audioItem = UIInfoTools.Instance?.playerDisconnectedAudioItem
		}, 10f, null, "MULTIPLAYER");
	}

	public static void ShowPlayerJoined(NetworkPlayer player)
	{
		string message = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_JOINED"), player.Username);
		Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = "GUI_MULTIPLAYER",
			message = message,
			audioItem = UIInfoTools.Instance?.playerJoinedAudioItem
		}, 5f, null, "MULTIPLAYER");
	}

	public static void ShowSelectedOtherPlayerCard()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_WARNING_SELECT_CARDS_OTHER_PLAYER") + "</color>", UIInfoTools.Instance.selectedOtherPlayerCardNotificationDuration, "SelectedOtherPlayerCard", "MULTIPLAYER");
	}

	public static void ShowSelectedCardWhileWaitingForPlayersToJoin()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_WARNING_SELECT_CARDS_WAITING_PLAYERS_JOIN") + "</color>", UIInfoTools.Instance.selectedOtherPlayerCardNotificationDuration, "SelectedCardWhileWaitingForPlayersToJoin", "MULTIPLAYER");
	}

	public static void ShowCancelReadiedCards()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_WARNING_CANCEL_SELECTED_CARDS") + "</color>", UIInfoTools.Instance.defaultNotificationDuration, "CancelSelectedCards", "MULTIPLAYER");
	}

	public static void HideCancelReadiedCards()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("CancelSelectedCards");
	}

	public static void ShowStartedSession()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_MULTIPLAYER_STARTED"), UIInfoTools.Instance.defaultNotificationDuration, "StartedSession", "MULTIPLAYER");
	}

	public static void ShowShareInviteCode()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_SHARE_INVITE_CODE"), UIInfoTools.Instance.defaultNotificationDuration, "ShareCode", "MULTIPLAYER");
	}

	public static void ShowGenerateInviteCode()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_GENERATE_INVITE_CODE"), UIInfoTools.Instance.defaultNotificationDuration, "GenerateCode", "MULTIPLAYER");
	}

	public static void ShowWaitingPlayersJoin()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersJoin", "GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_WAITING_PLAYERS_JOIN"), () =>
		{
			FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
			if (Singleton<UIResultsManager>.InstanceFast != null && Singleton<UIResultsManager>.InstanceFast.IsShown)
			{
				Singleton<UIResultsManager>.InstanceFast.MPSessionEndedOnResults();
			}
		}, "GUI_MULTIPLAYER_END_SESSION", "MULTIPLAYER");
	}

	public static void HideWaitingPlayersJoin()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersJoin");
	}

	public static void ShowWaitingPlayersJoining()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersJoining", "GUI_MULTIPLAYER", LocalizationManager.GetTranslation("GUI_NOTIFICATION_WAITING_PLAYERS_JOINING"), "MULTIPLAYER");
	}

	public static void HideWaitingPlayersJoining()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersJoining");
	}

	public static void HideAllMultiplayerNotification(bool instant = false)
	{
		Singleton<UINotificationManager>.Instance.HideNotificationsByTag("MULTIPLAYER", instant);
	}

	public static void ShowPlayerControlsCharacter(NetworkPlayer player, string controlled)
	{
		string id = controlled + "_CONTROL";
		string message = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_PLAYER_WON_CONTROL"), player.Username, controlled);
		Singleton<UINotificationManager>.Instance.HideNotification(id);
		Singleton<UINotificationManager>.Instance.ShowNotification(new UINotificationManager.NotificationData
		{
			titleLoc = "GUI_MULTIPLAYER",
			message = message,
			audioItem = UIInfoTools.Instance?.genericSystemMessageAudioItem
		}, UIInfoTools.Instance.defaultNotificationDuration, id, "MULTIPLAYER");
	}

	public static void ShowWaitingPlayersCharacterAssigned()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingPlayersCharacterAssigned", "GUI_MULTIPLAYER", string.Format("<color=#{1}>{0}", LocalizationManager.GetTranslation("GUI_MULTIPLAYER_WARNING_TEXT_WaitingForCharAssignment"), UIInfoTools.Instance.warningColor.ToHex()), "MULTIPLAYER");
	}

	public static void HideWaitingPlayersCharacterAssigned()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingPlayersCharacterAssigned");
	}

	public static void ShowSelectedQuest()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_QUEST_SELECTED"), PlayerRegistry.HostPlayer.Username);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_NOTIFICATION_QUEST_SELECTED_TITLE", text, UIInfoTools.Instance.defaultNotificationDuration, "SelectedQuest", "MULTIPLAYER");
	}

	public static void ShowSelectedCityEvent()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CITY_EVENT_SELECTED"), PlayerRegistry.HostPlayer.Username);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, "CityEvent", "MULTIPLAYER");
	}

	public static void HideSelectedCityEvent()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("CityEvent");
	}

	public static void ShowSelectedTownRecords()
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_TOWN_RECORDS_SELECTED"), PlayerRegistry.HostPlayer.Username);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MULTIPLAYER", text, UIInfoTools.Instance.defaultNotificationDuration, "TownRecords", "MULTIPLAYER");
	}

	public static void HideSelectedTownRecords()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("TownRecords");
	}

	public static void ShowWaitingAssignAllMySlots()
	{
		Singleton<UINotificationManager>.Instance.ShowPermanentNotification("WaitingAllMySlotsAssigned", "GUI_MULTIPLAYER", string.Format("<color=#{1}>{0}", string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_ASSIGN_ALL_MY_SLOTS"), PlayerRegistry.MyPlayer.Username), UIInfoTools.Instance.warningColor.ToHex()), "MULTIPLAYER");
	}

	public static void HideWaitingAssignAllMySlots()
	{
		Singleton<UINotificationManager>.Instance.HideNotification("WaitingAllMySlotsAssigned");
	}
}
