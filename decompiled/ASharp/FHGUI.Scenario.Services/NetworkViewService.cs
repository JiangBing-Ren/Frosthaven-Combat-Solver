using System.Collections.Generic;
using System.Linq;
using FFSNet;
using Libs.Utils;

namespace FHGUI.Scenario.Services;

public class NetworkViewService : FHScenarioViewService
{
	private readonly EReadyUpToggleStates readyUpToggleState;

	private List<NetworkPlayer> _networkPlayersReady = new List<NetworkPlayer>();

	public bool IsCurrentPlayerReady => _networkPlayersReady.Contains(PlayerRegistry.MyPlayer);

	public bool IsCurrentPlayerSpectating => !PlayerRegistry.MyPlayer.AssignedSlots.Any();

	public bool IsOnline => FFSNetwork.IsOnline;

	public bool IsHost
	{
		get
		{
			if (FFSNetwork.IsOnline)
			{
				return FFSNetwork.IsHost;
			}
			return true;
		}
	}

	public IEnumerable<NetworkPlayer> NetworkPlayersReady => _networkPlayersReady;

	public NetworkViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public bool AreAllPlayersReady()
	{
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsSlotAssigned && !_networkPlayersReady.Contains(allPlayer))
			{
				return false;
			}
		}
		return true;
	}

	public void HandleNetworkPlayerReadyChanged(NetworkPlayer player, bool ready)
	{
		if (player == null)
		{
			return;
		}
		if (ready)
		{
			_networkPlayersReady.AddNotContained(player);
		}
		else
		{
			_networkPlayersReady.Remove(player);
		}
		if (ready)
		{
			if (player == PlayerRegistry.MyPlayer)
			{
				if (FFSNetwork.IsHost)
				{
					Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new ControllableStateRevisionToken(PlayerRegistry.MyPlayer), new ReadyUpToken(readyUpToggleState.ToString()));
				}
				else
				{
					Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, null, new ReadyUpToken(readyUpToggleState.ToString()));
				}
			}
		}
		else if (player == PlayerRegistry.MyPlayer)
		{
			Synchronizer.SendGameAction(GameActionType.UnreadyPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: true);
		}
		_view.RefreshView();
		if (AreAllPlayersReady())
		{
			GameProceed();
		}
	}

	public void ConfirmAction(bool localExec = false)
	{
		if (IsOnline)
		{
			Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely);
		}
		Choreographer.s_Choreographer.Pass();
	}

	public void GameProceed(bool localExec = false)
	{
		if (IsOnline && !localExec)
		{
			Synchronizer.SendGameAction(GameActionType.ReadyProceed, ActionProcessor.CurrentPhase);
			return;
		}
		Singleton<UIReadyToggle>.Instance.Reset();
		Choreographer.s_Choreographer.Pass();
		_networkPlayersReady.Clear();
	}

	public void SetCurrentPlayerReady(bool ready)
	{
		HandleNetworkPlayerReadyChanged(PlayerRegistry.MyPlayer, ready);
	}
}
