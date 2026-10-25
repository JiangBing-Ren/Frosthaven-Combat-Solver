using System.Collections;

namespace FH.Platforms;

[PlatformID("XboxGameCore")]
internal class NewPlatformXboxGameCore : NewPlatform
{
	private bool _initialised;

	public override bool IsAvailableAndReady => _initialised;

	public static NewPlatformXboxGameCore Create()
	{
		NewPlatformXboxGameCoreUser user = new NewPlatformXboxGameCoreUser();
		NewPlatformXboxGameCoreStorage storage = new NewPlatformXboxGameCoreStorage(user);
		return new NewPlatformXboxGameCore(new NewPlatformXboxGameCoreEntitlements(), new NewPlatformXboxGameCoreLocalization(), new NewPlatformXboxGameCoreModding(), new NewPlatformXboxGameCoreNetworking(), new NewPlatformXboxGameCorePlayerStatistics(), user, storage);
	}

	protected NewPlatformXboxGameCore(NewPlatformXboxGameCoreEntitlements entitlements, NewPlatformXboxGameCoreLocalization localization, NewPlatformXboxGameCoreModding modding, NewPlatformXboxGameCoreNetworking networking, NewPlatformXboxGameCorePlayerStatistics stats, NewPlatformXboxGameCoreUser user, NewPlatformXboxGameCoreStorage storage)
		: base(entitlements, localization, modding, networking, stats, user, storage)
	{
	}

	public override IEnumerator InitCrt()
	{
		yield return User.SignIn();
		yield return Storage.InitCrt();
		yield return Entitlements.Init(this);
		yield return PlayerStatistics.Init(Storage);
		_initialised = true;
	}

	public override void Dispose()
	{
		PlayerStatistics.Dispose();
		Entitlements.Dispose();
		base.Dispose();
	}
}
