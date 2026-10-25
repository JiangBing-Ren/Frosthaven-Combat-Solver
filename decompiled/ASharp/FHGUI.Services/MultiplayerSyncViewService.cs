using System;
using FHRL;

namespace FHGUI.Services;

public class MultiplayerSyncViewService : FHMapViewService, IReadyCheckProcessor
{
	private const string SYNC_READY_CHECK_COMMAND_ID = "SYNC=";

	private string _syncPointId;

	private Action _syncAction;

	private Action _cancelAction;

	public MultiplayerSyncViewService(FHMapView view)
		: base(view)
	{
	}

	public bool IsApplicableReadyCheckCommand(string readyCheckID)
	{
		return readyCheckID == "SYNC=" + _syncPointId;
	}

	public bool ProcessReadyCheckResult(MultiplayerGameController.Result readyCheckResult)
	{
		if (!IsApplicableReadyCheckCommand(readyCheckResult.ReadyCheckID))
		{
			return false;
		}
		if (readyCheckResult.Action == MultiplayerGameController.Action.AllPlayersReady)
		{
			_syncAction?.Invoke();
			Reset();
		}
		if (readyCheckResult.Action == MultiplayerGameController.Action.ReadyCanceled)
		{
			_cancelAction?.Invoke();
			Reset();
		}
		return true;
	}

	public void SetMultiplayerSyncPoint(string syncPointId, Action onSyncDone, Action onCancelSync = null)
	{
		if (_view.IsSoloGame)
		{
			onSyncDone?.Invoke();
			return;
		}
		_syncPointId = syncPointId;
		_syncAction = onSyncDone;
		_cancelAction = onCancelSync;
		bool isReadyCheckRunning = _view.IsReadyCheckRunning;
		string readyCheckID = _view.MultiplayerGameState.ReadyCheckID;
		if (!isReadyCheckRunning)
		{
			Choreographer.ExecuteCommand("StartReadyCheck", _view.PlayerID, "SYNC=" + _syncPointId);
		}
		else if (IsApplicableReadyCheckCommand(readyCheckID))
		{
			Choreographer.ExecuteCommand("SetReady", _view.PlayerID, true);
		}
		else
		{
			Log.GUIError("Tried to set multiplayer sync point but there's a different ready check command running! [" + readyCheckID + "]");
		}
	}

	public void ClearSyncPoint()
	{
		if (!_view.IsPartyLeader)
		{
			return;
		}
		bool isReadyCheckRunning = _view.IsReadyCheckRunning;
		string readyCheckID = _view.MultiplayerGameState.ReadyCheckID;
		if (isReadyCheckRunning)
		{
			if (IsApplicableReadyCheckCommand(readyCheckID))
			{
				Choreographer.ExecuteCommand("CancelReadyCheck", _view.PlayerID);
			}
			else
			{
				Log.GUIError("Tried to clear multiplayer sync point but there's a different ready check command running! [" + readyCheckID + "]");
			}
		}
	}

	public void CancelSyncPoint()
	{
		if (_view.IsPartyLeader && _view.IsReadyCheckRunning)
		{
			Choreographer.ExecuteCommand("CancelReadyCheck", _view.PlayerID);
		}
	}

	private void Reset()
	{
		_syncAction = null;
		_syncPointId = null;
		_cancelAction = null;
	}
}
