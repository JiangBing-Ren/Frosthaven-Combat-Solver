using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Libs.GameConsole;
using Libs.Threading;
using Libs.Utils;
using UnityEngine;

namespace FH.Platforms;

public sealed class NewPlatformLayer
{
	private class IOTask
	{
		public string Filename;

		public bool Success;

		public object Data;

		public volatile bool Done;
	}

	private NewPlatform _currentPlatform;

	private IThreadHandle _ioThreadHandle;

	private ConcurrentQueue<IOTask> _ioQueue = new ConcurrentQueue<IOTask>();

	private const string IO_THREAD_NAME = "NewPlatformLayer => IOAsync";

	internal NewPlatform CurrentPlatform => _currentPlatform;

	public string CurrentPlatformName => _currentPlatform?.GetType().Name;

	public bool IsAvailableAndReady => _currentPlatform?.IsAvailableAndReady ?? false;

	public static void StartCoroutine(IEnumerator enumerator)
	{
		if (Application.isPlaying)
		{
			PlatformLayer.Instance.StartCoroutine(enumerator);
		}
	}

	public IEnumerator InitCrt()
	{
		if (Application.isPlaying)
		{
			_currentPlatform = NewPlatformSteam.Create(JoinGameSession);
		}
		else
		{
			_currentPlatform = NewPlatformEditor.Create();
		}
		Debug.Log("Initialising platform layer " + _currentPlatform.GetType().Name);
		yield return _currentPlatform.InitCrt();
		Log.Info($"Platform layer {_currentPlatform.GetType()} has been initialised.");
		ThreadManagerComponent.Manager.GetThread("NewPlatformLayer => IOAsync")?.CancelThread();
		_ioThreadHandle = ThreadManagerComponent.RunThread("NewPlatformLayer => IOAsync", IOAsync);
		StartCoroutine(Tick());
	}

	private IEnumerator Tick()
	{
		while (true)
		{
			yield return _currentPlatform?.Tick();
		}
	}

	private void IOAsync()
	{
		while (!ThreadManager.IsCurrentThreadCancel())
		{
			IOTask result;
			while (_ioQueue.TryDequeue(out result) && !ThreadManager.IsCurrentThreadCancel())
			{
				if (result.Data != null)
				{
					result.Success = _currentPlatform.Storage.SaveToFile(result.Filename, result.Data);
				}
				else
				{
					result.Success = _currentPlatform.Storage.LoadFromFile(result.Filename, out result.Data);
				}
				result.Done = true;
			}
			Thread.Sleep(100);
		}
	}

	public void Dispose()
	{
		if (_ioThreadHandle != null && _ioThreadHandle.IsAlive)
		{
			_ioThreadHandle.CancelThread();
			_ioThreadHandle = null;
		}
		_currentPlatform?.Dispose();
	}

	public IEnumerator WaitForPlatformAvailable()
	{
		Debug.Log("Waiting for platform layer to become available...");
		while (!IsAvailableAndReady)
		{
			yield return null;
		}
		Debug.Log("Platform layer is now available.");
	}

	public IEnumerator StoreData(string id, object data, ByRef<bool> success)
	{
		yield return WaitForPlatformAvailable();
		IOTask task = new IOTask
		{
			Filename = id + ".dat",
			Data = data
		};
		_ioQueue.Enqueue(task);
		while (!task.Done)
		{
			yield return null;
		}
		success.Value = task.Success;
	}

	public IEnumerator ReadData(string id, ByRef<object> data, ByRef<bool> success)
	{
		yield return WaitForPlatformAvailable();
		IOTask task = new IOTask
		{
			Filename = id + ".dat"
		};
		_ioQueue.Enqueue(task);
		while (!task.Done)
		{
			yield return null;
		}
		success.Value = task.Success;
		data.Value = task.Data;
	}

	public void WhenPlatformAvailable(Action action)
	{
		StartCoroutine(WhenPlatformAvailableCrt(action));
	}

	private IEnumerator WhenPlatformAvailableCrt(Action action)
	{
		yield return WaitForPlatformAvailable();
		action?.Invoke();
	}

	public void GetCurrentUserData([CanBeNull] Action<PlatformUserData> onComplete)
	{
		WhenPlatformAvailable(() =>
		{
			_currentPlatform.User.GetCurrentUserData(onComplete);
		});
	}

	public void GetOtherUserData(string platformPlayerID, [CanBeNull] Action<PlatformUserData> onComplete)
	{
		WhenPlatformAvailable(() =>
		{
			_currentPlatform.User.GetOtherUserData(platformPlayerID, onComplete);
		});
	}

	public bool IsSignedIn()
	{
		if (!IsAvailableAndReady)
		{
			Log.Warning("Failed to get user status: Platform layer not available or not yet ready");
			return false;
		}
		return _currentPlatform.User.IsSignedIn;
	}

	public string GetCurrentUserName()
	{
		if (!IsAvailableAndReady || !_currentPlatform.User.IsSignedIn)
		{
			throw new Exception("Platform layer user not available or not yet ready");
		}
		return _currentPlatform.User.Name;
	}

	public string GetCurrentPlayerID()
	{
		if (!IsAvailableAndReady || !_currentPlatform.User.IsSignedIn)
		{
			throw new Exception("Platform layer user not available or not yet ready");
		}
		return _currentPlatform.User.PlatformPlayerID;
	}

	public string GetCurrentAccountID()
	{
		if (!IsAvailableAndReady || !_currentPlatform.User.IsSignedIn)
		{
			throw new Exception("Platform layer user not available or not yet ready");
		}
		return _currentPlatform.User.PlatformAccountID;
	}

	public async Task<bool> IsEntitlementActive(string entitlementID)
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Entitlements.IsEntitlementActive(entitlementID);
	}

	public async Task<EntitlementDefinition> GetEntitlement(string entitlementID)
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Entitlements.GetEntitlement(entitlementID);
	}

	public async Task<IEnumerable<EntitlementDefinition>> GetCompatibleEntitlements()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Entitlements.GetCompatibleEntitlements();
	}

	public async Task<IEnumerable<EntitlementDefinition>> GetActiveEntitlements()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Entitlements.GetActiveEntitlements();
	}

	public async Task<IEnumerable<EntitlementDefinition>> GetAllEntitlements()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Entitlements.GetAllEntitlements();
	}

	public async Task OpenStore(string entitlementID = null)
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		_currentPlatform.Entitlements.OpenStore(entitlementID);
	}

	internal bool AreModsSupported()
	{
		return true;
	}

	private bool Internal_InvitesSupported()
	{
		return _currentPlatform.Networking.InvitesSupported;
	}

	internal async Task<bool> AreInvitesSupported()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return Internal_InvitesSupported();
	}

	internal void GetPlatformInvitesSupported(Action<bool> onResult)
	{
		WhenPlatformAvailable(() =>
		{
			onResult?.Invoke(Internal_InvitesSupported());
		});
	}

	internal void AdvertiseLobbyKey(string lobbyKey)
	{
		Debug.Log("[PLATFORM] Advertising lobby key: " + lobbyKey);
		WhenPlatformAvailable(() =>
		{
			_currentPlatform.Networking.AdvertiseLobbyKey(lobbyKey);
		});
	}

	internal void LeaveSession()
	{
		WhenPlatformAvailable(() =>
		{
			_currentPlatform.Networking.AdvertiseLobbyKey(null);
		});
	}

	public async Task<string> GetPendingSessionJoin()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Networking.PendingSessionJoin;
	}

	public void CheckPendingSessionAndJoin(Action onBeforeJoin = null)
	{
		WhenPlatformAvailable(() =>
		{
			if (_currentPlatform.Networking.PendingSessionJoin != null)
			{
				onBeforeJoin?.Invoke();
				string pendingSessionJoin = _currentPlatform.Networking.PendingSessionJoin;
				JoinGameSession(pendingSessionJoin);
			}
		});
	}

	private void JoinGameSession(string lobbyKey)
	{
		_currentPlatform.Networking.ClearPendingSessionJoin();
		FFSNetwork.JoinSession(lobbyKey);
	}

	public void OpenInviteOverlay()
	{
		WhenPlatformAvailable(() =>
		{
			_currentPlatform.Networking.OpenInviteOverlay();
		});
	}

	public async Task<string> GetLocale()
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.Localization.GetPlatformLocale();
	}

	public async Task ClearAllAchievements()
	{
		if (_currentPlatform.PlayerStatistics.AchievementsSupported)
		{
			await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
			_currentPlatform.PlayerStatistics.ClearAllAchievements();
		}
	}

	public async Task SetAchievementCompleted(IEnumerable<string> m_Achievements)
	{
		if (_currentPlatform.PlayerStatistics.AchievementsSupported)
		{
			await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
			_currentPlatform.PlayerStatistics.SetAchievementCompleted(m_Achievements);
		}
	}

	public async Task<bool> IsAchievementCompleted(string achievementName, bool includeOffline = true)
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		return _currentPlatform.PlayerStatistics.IsAchievementCompleted(achievementName, includeOffline);
	}

	public async Task SetStat(string statName, int value)
	{
		await FHTaskExtensions.WaitUntil(() => IsAvailableAndReady);
		_currentPlatform.PlayerStatistics.SetStat(statName, value);
	}

	public async Task<string> GetPersistentDataPathAsync()
	{
		await FHTaskExtensions.WaitWhile(() => !IsAvailableAndReady || !_currentPlatform.Storage.IsReady);
		return _currentPlatform.Storage.GetPersistentDataPath();
	}

	public async Task<string> GetSaveDataPathAsync()
	{
		await FHTaskExtensions.WaitWhile(() => !IsAvailableAndReady || !_currentPlatform.Storage.IsReady);
		return _currentPlatform.Storage.GetSaveDataPath();
	}

	public void GetPersistentDataPath(Action<string> onComplete)
	{
		WhenPlatformAvailable(() =>
		{
			onComplete?.Invoke(_currentPlatform.Storage.GetPersistentDataPath());
		});
	}

	public void GetSaveDataPath(Action<string> onComplete)
	{
		WhenPlatformAvailable(() =>
		{
			onComplete?.Invoke(_currentPlatform.Storage.GetSaveDataPath());
		});
	}

	internal void GetOtherAvatar(string platformPlayerID, Action<Sprite> onComplete)
	{
		_currentPlatform.User.GetOtherAvatar(platformPlayerID, onComplete);
	}

	[ConsoleCommand(Command = "PlatformLayer_PrintUserID")]
	public static void PrintUserID(IConsole console)
	{
		string text = "Platform Account ID: " + PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform.User.PlatformAccountID;
		console.WriteLine(text);
		Debug.Log(text);
	}

	[ConsoleCommand(Command = "PlatformJoinGameSession", Description = "Join a game session with the specified lobby key.")]
	public static void PlatformJoinGameSession(IConsole console, string lobbyKey)
	{
		if (string.IsNullOrEmpty(lobbyKey))
		{
			console.WriteLine("No lobby key specified, cannot join game session.");
			return;
		}
		if (PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform.Networking.PendingSessionJoin != null)
		{
			console.WriteLine("Already in a pending session, cannot join another one.");
			return;
		}
		console.WriteLine("Joining game session with lobby key: " + lobbyKey);
		PlatformLayer.Instance.NewPlatformLayer.JoinGameSession(lobbyKey);
	}
}
