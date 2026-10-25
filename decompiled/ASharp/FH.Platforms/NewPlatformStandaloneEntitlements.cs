namespace FH.Platforms;

public class NewPlatformStandaloneEntitlements : NewPlatformEntitlements
{
	protected override bool IsEntitlementActiveOnPlatform(EntitlementDefinition entitlement)
	{
		return false;
	}

	protected override void OpenPlatformStore(EntitlementDefinition entitlement)
	{
	}
}
