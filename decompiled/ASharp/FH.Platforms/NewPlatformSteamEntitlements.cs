using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;

namespace FH.Platforms;

public class NewPlatformSteamEntitlements : NewPlatformEntitlements
{
	private Dictionary<string, DlcInformation> _steamEntitlements;

	internal override IEnumerator Init(NewPlatform platform)
	{
		_steamEntitlements = new Dictionary<string, DlcInformation>();
		foreach (DlcInformation item in SteamApps.DlcInformation())
		{
			_steamEntitlements.Add(item.Name, item);
		}
		yield return base.Init(platform);
	}

	protected override bool IsEntitlementActiveOnPlatform(EntitlementDefinition entitlement)
	{
		string sTEAM = entitlement.STEAM;
		if (_steamEntitlements.TryGetValue(sTEAM, out var value))
		{
			return SteamApps.IsDlcInstalled(value.AppId);
		}
		return false;
	}

	protected override void OpenPlatformStore(EntitlementDefinition entitlement)
	{
		string sTEAM = entitlement.STEAM;
		SteamFriends.OpenStoreOverlay(_steamEntitlements.TryGetValue(sTEAM, out var value) ? value.AppId : SteamClient.AppId);
	}

	public override void Dispose()
	{
		_steamEntitlements.Clear();
		base.Dispose();
	}
}
