using System.Collections;

namespace FH.Platforms;

[PlatformID("Standalone")]
internal class NewPlatformStandalone : NewPlatform
{
	public override bool IsAvailableAndReady
	{
		get
		{
			if (Storage.IsReady)
			{
				return User.IsSignedIn;
			}
			return false;
		}
	}

	public static NewPlatformStandalone Create()
	{
		return new NewPlatformStandalone();
	}

	protected NewPlatformStandalone()
		: base(new NewPlatformStandaloneEntitlements(), new NewPlatformStandaloneLocalization(), new NewPlatformStandaloneModding(), new NewPlatformStandaloneNetworking(), new NewPlatformStandalonePlayerStatistics(), new NewPlatformStandaloneUser(), new NewPlatformStandaloneStorage())
	{
		((NewPlatformStandaloneUser)User).SetStorage(Storage);
	}

	public override IEnumerator InitCrt()
	{
		yield return Storage.InitCrt();
		yield return User.SignIn();
		yield return Entitlements.Init(this);
		yield return PlayerStatistics.Init(Storage);
	}

	public override void Dispose()
	{
		PlayerStatistics.Dispose();
		Entitlements.Dispose();
		base.Dispose();
	}
}
