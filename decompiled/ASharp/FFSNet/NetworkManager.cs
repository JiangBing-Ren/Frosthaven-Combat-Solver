using System;
using MEC;
using UnityEngine;

namespace FFSNet;

public class NetworkManager : ScriptableObject
{
	[Range(1f, 7f)]
	[SerializeField]
	private int maxPlayers = 4;

	[Range(0.05f, 1f)]
	[SerializeField]
	private float sessionListUpdateFrequency = 0.5f;

	[Range(1f, 60f)]
	[SerializeField]
	private float matchmakingTimeOutDuration = 3f;

	[Range(3f, 15f)]
	[SerializeField]
	public int inviteCodeLength = 8;

	[Range(0.05f, 0.5f)]
	[SerializeField]
	private float actionQueueProcessingInterval = 0.3f;

	[Range(1f, 30f)]
	[Header("How long to wait after detecting the next action in the processing queue cannot be played at the current phase.")]
	[SerializeField]
	private float incorrectActionDetectedTimeOutDuration = 5f;

	[Header("Auto-assign removed/disconnected players' controllables to the host player.")]
	[SerializeField]
	private bool autoAssignControllablesToHost = true;

	[Header("Automatically shutdown FFSNetwork in case my NetworkPlayer is detached.")]
	[SerializeField]
	private bool autoShutdownUponMyPlayerRemoved = true;

	[Header("Automatically shutdown FFSNetwork whenever joining a session fails.")]
	[SerializeField]
	private bool autoShutdownUponJoiningFailed = true;

	[Header("Automatically shutdown FFSNetwork whenever a desynchronization occurs.")]
	[SerializeField]
	private bool autoShutdownUponDesynchronization = true;

	[Header("Prints the acting controllable name as part of the error logging for incorrect actions detected.")]
	[SerializeField]
	private bool displayControllableNameForIncorrectActionErrors = true;

	private CoroutineHandle joinSessionRoutine;

	private bool cancelingJoining;

	public int MaxPlayers => maxPlayers;

	public float ActionQueueProcessingInterval => actionQueueProcessingInterval;

	public float IncorrectActionDetectedTimeOutDuration => incorrectActionDetectedTimeOutDuration;

	public bool AutoAssignControllablesToHost => autoAssignControllablesToHost;

	public bool AutoShutdownUponMyPlayerRemoved => autoShutdownUponMyPlayerRemoved;

	public bool AutoShutdownUponJoiningFailed => autoShutdownUponJoiningFailed;

	public bool AutoShutdownUponDesynchronization => autoShutdownUponDesynchronization;

	public bool DisplayControllableNameForIncorrectActionErrors => displayControllableNameForIncorrectActionErrors;

	public int DebugPort { get; private set; } = -1;

	public void OnEnable()
	{
		ActionProcessor.MaxConsecutiveIncorrectActionsAllowed = Mathf.RoundToInt(incorrectActionDetectedTimeOutDuration / ActionQueueProcessingInterval);
		Log.NetworkInfo("NetworkManager enabled.");
	}

	public void SwitchRegion(string region, bool updateImmediately = true)
	{
		throw new InvalidOperationException("Not valid in Photon Fusion");
	}
}
