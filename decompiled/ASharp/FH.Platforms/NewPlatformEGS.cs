using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace FH.Platforms;

[PlatformID("EGS")]
internal class NewPlatformEGS : NewPlatform
{
	private class RichPresence
	{
		private (string key, string value, string mission, string hero, int turn) _currentStatus;

		private (string key, string value, string mission, string hero, int turn) _previousStatus;

		private readonly NewPlatformEGSUser _user;

		public RichPresence(NewPlatformEGSUser user)
		{
			_user = user;
		}

		public IEnumerator Tick()
		{
			List<string> list = (from a in RuntimeTools.GetCurrentActiveScenes()
				select a.name).ToList();
			if (list.Contains("MainMenu"))
			{
				_currentStatus = (key: "display", value: "#MainMenu", mission: null, hero: null, turn: 0);
			}
			else if (list.Contains("FHCampaignMap"))
			{
				_currentStatus = (key: "display", value: "#InOutpost", mission: null, hero: null, turn: 0);
			}
			else if (list.Contains("ProcGen"))
			{
				_currentStatus = (key: "display", value: "#InMission", mission: null, hero: null, turn: 0);
			}
			(string, string, string, string, int) currentStatus = _currentStatus;
			(string, string, string, string, int) previousStatus = _previousStatus;
			if (currentStatus.Item1 != previousStatus.Item1 || currentStatus.Item2 != previousStatus.Item2 || currentStatus.Item3 != previousStatus.Item3 || currentStatus.Item4 != previousStatus.Item4 || currentStatus.Item5 != previousStatus.Item5)
			{
				SetRichPresenceValue(_currentStatus.value);
				_previousStatus = _currentStatus;
			}
			yield break;
		}

		public void SetRichPresenceValue(string value)
		{
			throw new Exception("Wrong platform");
		}
	}

	private bool _initialised;

	private readonly RichPresence _richPresence;

	public override bool IsAvailableAndReady => _initialised;

	public static NewPlatformEGS Create(Action<string> joinGameSession)
	{
		NewPlatformEGSUser user = new NewPlatformEGSUser();
		NewPlatformEGSStorage storage = new NewPlatformEGSStorage(user);
		return new NewPlatformEGS(new NewPlatformEGSEntitlements(user), new NewPlatformEGSLocalization(), new NewPlatformEGSModding(), new NewPlatformEGSNetworking(user, joinGameSession), new NewPlatformEGSPlayerStatistics(user), user, storage);
	}

	protected NewPlatformEGS(NewPlatformEGSEntitlements entitlements, NewPlatformEGSLocalization localization, NewPlatformEGSModding modding, NewPlatformEGSNetworking networking, NewPlatformEGSPlayerStatistics stats, NewPlatformEGSUser user, NewPlatformEGSStorage storage)
		: base(entitlements, localization, modding, networking, stats, user, storage)
	{
		_richPresence = new RichPresence(user);
	}

	public override IEnumerator InitCrt()
	{
		yield break;
	}

	public override IEnumerator Tick()
	{
		yield return base.Tick();
		yield return _richPresence?.Tick();
	}

	public override void Dispose()
	{
		PlayerStatistics.Dispose();
		Entitlements?.Dispose();
		base.Dispose();
	}
}
