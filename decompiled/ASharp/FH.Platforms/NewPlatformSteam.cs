using System;
using System.Collections;
using FH.Platforms.Steam;
using Libs.GameConsole;
using Steamworks;
using UnityEngine;

namespace FH.Platforms;

[PlatformID("Steam")]
internal class NewPlatformSteam : NewPlatform
{
	private const uint _appId = 2347080u;

	private bool _initialised;

	private SteamRichPresence _richPresence;

	public override bool IsAvailableAndReady => _initialised;

	public static NewPlatformSteam Create(Action<string> joinGameSession)
	{
		NewPlatformSteamUser user = new NewPlatformSteamUser();
		NewPlatformSteamStorage storage = new NewPlatformSteamStorage(user);
		return new NewPlatformSteam(new NewPlatformSteamEntitlements(), new NewPlatformSteamLocalization(), new NewPlatformSteamModding(), new NewPlatformSteamNetworking(joinGameSession), new NewPlatformSteamPlayerStatistics(), user, storage);
	}

	protected NewPlatformSteam(NewPlatformSteamEntitlements entitlements, NewPlatformSteamLocalization localization, NewPlatformSteamModding modding, NewPlatformSteamNetworking networking, NewPlatformSteamPlayerStatistics stats, NewPlatformSteamUser user, NewPlatformSteamStorage storage)
		: base(entitlements, localization, modding, networking, stats, user, storage)
	{
		_richPresence = new SteamRichPresence();
	}

	public override IEnumerator InitCrt()
	{
		if (!SteamClient.IsValid)
		{
			SteamClient.Init(2347080u);
		}
		float timeout = 10f;
		while (!SteamClient.IsValid)
		{
			timeout -= Time.deltaTime;
			if (timeout <= 0f)
			{
				Debug.LogError("Steam client has not been correctly instantiated.");
				yield break;
			}
			yield return null;
		}
		yield return User.SignIn();
		yield return Storage.InitCrt();
		yield return Entitlements.Init(this);
		yield return PlayerStatistics.Init(Storage);
		yield return ((NewPlatformSteamNetworking)Networking).InitCrt();
		_initialised = true;
	}

	public override IEnumerator Tick()
	{
		yield return base.Tick();
		yield return _richPresence.Tick();
	}

	public override void Dispose()
	{
		PlayerStatistics.Dispose();
		base.Dispose();
		_richPresence?.Dispose();
		Entitlements?.Dispose();
		SteamClient.Shutdown();
	}

	[ConsoleCommand(Command = "SteamTestRichPresence")]
	public static void TestRichPresence(IConsole console, string key, string value)
	{
		SteamFriends.SetRichPresence(key, value);
		console.WriteLine("Rich presence set to: " + key + " = " + value);
	}

	[ConsoleCommand(Command = "SteamClearRichPresence")]
	public static void TestClearRichPresence(IConsole console)
	{
		SteamFriends.ClearRichPresence();
	}
}
