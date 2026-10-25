namespace FH.Platforms;

public class NewPlatformPS5Entitlements : NewPlatformEntitlements
{
	protected override bool IsEntitlementActiveOnPlatform(EntitlementDefinition entitlement)
	{
		return false;
	}

	protected override void OpenPlatformStore(EntitlementDefinition entitlement)
	{
	}
}
