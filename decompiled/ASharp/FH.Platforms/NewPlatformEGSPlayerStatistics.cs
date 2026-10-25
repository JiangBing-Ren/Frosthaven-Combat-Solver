using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEGSPlayerStatistics : NewPlatformPlayerStatistics
{
	private Dictionary<string, string> _eosAchievementsRemap = new Dictionary<string, string>();

	private NewPlatformEGSUser _user;

	public override bool AchievementsSupported => true;

	protected override float PlatformPushCooldownSeconds => 60f;

	public NewPlatformEGSPlayerStatistics(NewPlatformEGSUser user)
	{
		_user = user;
	}

	internal override IEnumerator Init(INewPlatformStorage storage)
	{
		while (!_user.IsSignedIn)
		{
			yield return null;
		}
		yield return base.Init(storage);
	}

	protected override bool IsSingleAchievementCompleted(string achievementID)
	{
		if (!_eosAchievementsRemap.TryGetValue(achievementID, out var value))
		{
			value = achievementID;
		}
		return false;
	}

	protected override bool SetSingleAchievementCompleted(string achievementID)
	{
		if (!_eosAchievementsRemap.TryGetValue(achievementID, out var value))
		{
			value = achievementID;
			Debug.Log(achievementID + " not found in achievement mapping, trying with direct string.");
		}
		return false;
	}

	protected override void ClearAllPlatformAchievements()
	{
		throw new NotImplementedException();
	}

	protected override bool SetStatValue(string statID, int value)
	{
		Debug.LogError("Stats are not implemented for EGS");
		return false;
	}

	protected override Task<bool> PushToPlatform()
	{
		return Task.Run(async () => false);
	}
}
