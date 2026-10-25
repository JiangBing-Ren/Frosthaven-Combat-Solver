using System;
using System.Linq;
using FFSNet.Backend;

namespace FFSNet;

public static class Synchronizer
{
	public static void SendGameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool validateOnServerBeforeExecuting = false, bool disableAutoReplication = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, string supplementaryDataString = null, IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null)
	{
		ReplicateAction(actionType, targetPhaseType, executeLocallyFirst: false, validateOnServerBeforeExecuting, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataString, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, null);
	}

	public static void SendGameActionClassID(GameActionType actionType, ActionPhaseType targetPhaseType, int actorID, int supplementaryDataIDMin, bool supplementaryDataBool, string classID)
	{
		ReplicateAction(actionType, targetPhaseType, executeLocallyFirst: false, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, supplementaryDataIDMin, 0, 0, supplementaryDataBool, null, null, null, null, null, classID);
	}

	public static void SendSideAction(GameActionType actionType, IProtocolToken supplementaryDataToken = null, bool canBeUnreliable = false, bool sendToHostOnly = false, int targetPlayerID = 0, int dataInt = 0, int dataInt2 = 0, bool dataBool = false, string dataString = null)
	{
		if (FFSNetwork.IsOnline && !FFSNetwork.HasDesynchronized)
		{
			Log.NetworkInfo("Sending SideAction (" + actionType.ToString() + ") to " + (sendToHostOnly ? "server" : "others") + ". DataTOKEN: " + ((supplementaryDataToken != null) ? "YES" : "NO") + ", Unreliable: " + canBeUnreliable);
			NetworkActionEventToken networkActionEventToken = new NetworkActionEventToken();
			networkActionEventToken.Token = new NetworkAction(actionType, PlayerRegistry.MyPlayer, supplementaryDataToken, targetPlayerID, dataInt, dataInt2, dataBool, dataString);
			if (sendToHostOnly)
			{
				FFSNetwork.SendTokenToServer(networkActionEventToken);
			}
			else
			{
				FFSNetwork.SendTokenToClients(networkActionEventToken);
			}
		}
	}

	public static void AutoExecuteServerAuthGameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool disableAutoReplication = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, string supplementaryDataString = null, IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null)
	{
		ReplicateAction(actionType, targetPhaseType, executeLocallyFirst: true, validateOnServerBeforeExecuting: true, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataString, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, null);
	}

	public static void ReplicateControllableStateChange(GameActionType actionType, ActionPhaseType targetPhaseType, int controllableID, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, string supplementaryDataString = null, IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, bool validateOnServerBeforeExecuting = false)
	{
		ReplicateAction(actionType, targetPhaseType, executeLocallyFirst: false, validateOnServerBeforeExecuting, disableAutoReplication: false, controllableID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataString, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, null);
	}

	private static void ReplicateAction(GameActionType actionType, ActionPhaseType targetPhaseType, bool executeLocallyFirst, bool validateOnServerBeforeExecuting, bool disableAutoReplication, int actorID, int supplementaryDataIDMin, int supplementaryDataIDMed, int supplementaryDataIDMax, bool supplementaryDataBoolean, string supplementaryDataString, IProtocolToken supplementaryDataToken, IProtocolToken supplementaryDataToken2, IProtocolToken supplementaryDataToken3, IProtocolToken supplementaryDataToken4, string classID)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		GameAction gameAction = new GameAction(actionType, targetPhaseType, executeLocallyFirst, validateOnServerBeforeExecuting, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataString, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, classID);
		Log.NetworkInfo($"[Core] Sending GameAction {gameAction}");
		if (FFSNetwork.IsHost)
		{
			if (executeLocallyFirst)
			{
				ActionProcessor.QueueUpAction(gameAction);
			}
			else
			{
				foreach (NetworkPlayer player in PlayerRegistry.AllPlayers)
				{
					if (player != PlayerRegistry.MyPlayer && !PlayerRegistry.JoiningPlayers.Any((NetworkBackendPlayer p) => p.PlayerID == player.PlayerID.ID))
					{
						SendGameActionToPlayer(gameAction, player);
					}
				}
			}
		}
		if (FFSNetwork.IsClient)
		{
			SendGameActionToPlayer(gameAction, PlayerRegistry.HostPlayer);
		}
	}

	private static void SendGameActionToPlayer(GameAction action, NetworkPlayer targetPlayer)
	{
		if (targetPlayer == null)
		{
			Log.NetworkWarning("Error sending game action. The target player does not exist anymore.");
		}
		else if (action.ClassID == null)
		{
			GameActionEventToken gameActionEventToken = new GameActionEventToken();
			gameActionEventToken.ActionID = action.ActionID;
			gameActionEventToken.ActionTypeID = action.ActionTypeID;
			gameActionEventToken.PlayerID = action.PlayerID;
			gameActionEventToken.ActorID = action.ActorID;
			gameActionEventToken.TargetPhaseID = action.TargetPhaseID;
			gameActionEventToken.SupplementaryDataIDMin = action.SupplementaryDataIDMin;
			gameActionEventToken.SupplementaryDataIDMed = action.SupplementaryDataIDMed;
			gameActionEventToken.SupplementaryDataIDMax = action.SupplementaryDataIDMax;
			gameActionEventToken.SupplementaryDataBoolean = action.SupplementaryDataBoolean;
			gameActionEventToken.SupplementaryDataString = action.SupplementaryDataString;
			gameActionEventToken.SupplementaryDataToken = action.SupplementaryDataToken;
			gameActionEventToken.SupplementaryDataToken2 = action.SupplementaryDataToken2;
			gameActionEventToken.SupplementaryDataToken3 = action.SupplementaryDataToken3;
			gameActionEventToken.SupplementaryDataToken4 = action.SupplementaryDataToken4;
			gameActionEventToken.ValidateAction = action.ValidateAction;
			gameActionEventToken.DoNotForwardAction = action.DoNotForwardAction;
			if (targetPlayer == PlayerRegistry.HostPlayer)
			{
				FFSNetwork.SendTokenToServer(gameActionEventToken);
			}
			else
			{
				FFSNetwork.SendTokenToClient(gameActionEventToken, targetPlayer.PlayerID);
			}
		}
		else
		{
			GameActionEventClassIDToken gameActionEventClassIDToken = new GameActionEventClassIDToken();
			gameActionEventClassIDToken.ActionID = action.ActionID;
			gameActionEventClassIDToken.ActionTypeID = action.ActionTypeID;
			gameActionEventClassIDToken.PlayerID = action.PlayerID;
			gameActionEventClassIDToken.ActorID = action.ActorID;
			gameActionEventClassIDToken.TargetPhaseID = action.TargetPhaseID;
			gameActionEventClassIDToken.SupplementaryDataIDMin = action.SupplementaryDataIDMin;
			gameActionEventClassIDToken.SupplementaryDataBoolean = action.SupplementaryDataBoolean;
			gameActionEventClassIDToken.ClassID = action.ClassID;
			if (targetPlayer == PlayerRegistry.HostPlayer)
			{
				FFSNetwork.SendTokenToServer(gameActionEventClassIDToken);
			}
			else
			{
				FFSNetwork.SendTokenToClient(gameActionEventClassIDToken, targetPlayer.PlayerID);
			}
		}
	}

	public static void ForwardGameActionToClients(GameAction action)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		Log.NetworkInfo(string.Format("Forwarding GameAction {0} to {1}", action, action.ValidateAction ? "all clients." : "remaining clients."));
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		foreach (NetworkPlayer player2 in PlayerRegistry.AllPlayers)
		{
			if (player2 != PlayerRegistry.MyPlayer && !PlayerRegistry.JoiningPlayers.Any((NetworkBackendPlayer p) => p.PlayerID == player2.PlayerID.ID) && (action.ValidateAction || player2 != player))
			{
				SendGameActionToPlayer(action, player2);
			}
		}
	}

	public static void RequestControllable(bool onlyIfHasNoControllables = true)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		if (onlyIfHasNoControllables)
		{
			if (PlayerRegistry.MyPlayer == null)
			{
				PlayerRegistry.MyPlayerInitialized.AddListener(() =>
				{
					RequestControllable();
				});
				return;
			}
			if (PlayerRegistry.MyPlayer.MyControllables.Any())
			{
				return;
			}
		}
		FFSNetwork.SendTokenToServer(new ControllableAssignmentRequestToken());
	}

	public static void RequestCustomData(DataActionType dataActionType)
	{
		Log.NetworkInfo("Requesting custom data (" + dataActionType.ToString() + ") from the server.");
		FFSNetwork.SendTokenToServer(new GameDataRequestToken
		{
			DataActionID = (int)dataActionType
		});
	}

	public static void RequestCustomDataFromClients(DataActionType dataActionType)
	{
		Log.NetworkInfo("Requesting custom data (" + dataActionType.ToString() + ") from the clients.");
		FFSNetwork.SendTokenToClients(new GameDataRequestToken
		{
			DataActionID = (int)dataActionType
		});
	}

	public static void StreamCustomData(DataActionType actionType, byte[] data, NetworkBackendPlayer connection)
	{
		StreamCompressedCustomData(Utility.CompressData(data));
	}

	public static void StreamCompressedCustomData(byte[] compressed)
	{
		foreach (StreamingDataToken item in StreamingDataToken.SplitByteArrayIntoTokens(compressed))
		{
			FFSNetwork.SendTokenToServer(item);
		}
	}

	public static void NotifyJoiningPlayersAboutReachingSavePoint()
	{
		try
		{
			GHNetworkCallbacks.s_ClientsWaitingForSavePoint = false;
			if (FFSNetwork.HasDesynchronized)
			{
				return;
			}
			foreach (NetworkBackendPlayer joiningPlayer in PlayerRegistry.JoiningPlayers)
			{
				if (joiningPlayer != null)
				{
					FFSNetwork.SendTokenToClient(new SavePointReachedEventToken(), joiningPlayer);
				}
			}
			global::Singleton<FHMapChoreographer>.InstanceFast?.View?.RefreshMultiplayerNotifications();
		}
		catch (Exception ex)
		{
			Log.NetworkWarning("Exception in NotifyJoiningPlayersAboutReachingSavePoint.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}
}
