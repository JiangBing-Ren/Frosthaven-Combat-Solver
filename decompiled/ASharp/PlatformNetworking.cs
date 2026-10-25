using System.Collections;
using UnityEngine.Events;

public class PlatformNetworking
{
	public INetworkSessionService EpicCurrentSessionService;

	private IEnumerator m_EPICWaitForLoadingRoutine;

	public string EPICPendingInviteLobbyID;

	public bool EPICInvitesSupported => true;

	public bool HasEPICInvitePending => EPICPendingInviteLobbyID.IsNOTNullOrEmpty();

	public void JoinOnPendingEPICInvite(UnityAction<string> joinCompleteAction)
	{
		string ePICPendingInviteLobbyID = EPICPendingInviteLobbyID;
		EPICPendingInviteLobbyID = null;
		joinCompleteAction(ePICPendingInviteLobbyID);
	}

	public void RegisterNetworkSessionServiceWithEpic(INetworkSessionService serviceToRegister)
	{
		EpicCurrentSessionService = serviceToRegister;
	}

	public void EpicGameLobbyJoinRequestedCallback()
	{
	}

	private IEnumerator AcceptEPICInviteAfterFinishedLoading(UnityAction inviteAcceptAction)
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		inviteAcceptAction?.Invoke();
	}
}
