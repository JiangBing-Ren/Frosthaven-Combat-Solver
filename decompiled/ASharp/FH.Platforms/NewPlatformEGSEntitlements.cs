using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.GameConsole;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEGSEntitlements : NewPlatformEntitlements
{
	private NewPlatformEGSUser _user;

	private List<string> _owned = new List<string>();

	public NewPlatformEGSEntitlements(NewPlatformEGSUser user)
	{
		_user = user;
	}

	internal override IEnumerator Init(NewPlatform platform)
	{
		yield return base.Init(platform);
		EntitlementDefinition[] compatible = GetCompatibleEntitlements().ToArray();
		yield return RefreshOwnershipData(compatible);
		UpdateActivePlatformEntitlements();
	}

	private IEnumerator RefreshOwnershipData(EntitlementDefinition[] compatible)
	{
		_ = compatible.LongLength;
		yield break;
	}

	protected override bool IsEntitlementActiveOnPlatform(EntitlementDefinition entitlement)
	{
		return _owned.Contains(entitlement.EGS);
	}

	protected override void OpenPlatformStore(EntitlementDefinition entitlement)
	{
		Application.OpenURL("https://www.epicgames.com/store/en-US/product/" + entitlement.EGS);
	}

	[ConsoleCommand(Command = "egs_entitlements_test")]
	public static void Test(IConsole console)
	{
		console.WriteLine("Active");
		Task<bool> task = Task.Run(() => PlatformLayer.Instance.NewPlatformLayer.IsEntitlementActive("TEST_DLC"));
		console.WriteLine(task.Result ? "TEST_DLC active" : "TEST_DLC inactive");
		console.WriteLine("Available");
		foreach (EntitlementDefinition item in PlatformLayer.Instance.NewPlatformLayer.GetCompatibleEntitlements().Result)
		{
			console.WriteLine(item.ID);
		}
	}

	[ConsoleCommand(Command = "egs_entitlements_open_store")]
	public static void OpenStore(IConsole console)
	{
		PlatformLayer.Instance.NewPlatformLayer.OpenStore("TEST_DLC").Wait();
	}
}
