using System.Collections;
using ScenarioRuleLibrary.YML;

namespace FH.Platforms;

[PlatformID("PS5")]
internal class NewPlatformPS5 : NewPlatform
{
	private bool _initialised;

	public override bool IsAvailableAndReady => _initialised;

	public static NewPlatformPS5 Create()
	{
		NewPlatformPS5User user = new NewPlatformPS5User();
		NewPlatformPS5Storage storage = new NewPlatformPS5Storage(user);
		return new NewPlatformPS5(new NewPlatformPS5Entitlements(), new NewPlatformPS5Localization(), new NewPlatformPS5Modding(), new NewPlatformPS5Networking(), new NewPlatformPS5PlayerStatistics(), user, storage);
	}

	protected NewPlatformPS5(NewPlatformPS5Entitlements entitlements, NewPlatformPS5Localization localization, NewPlatformPS5Modding modding, NewPlatformPS5Networking networking, NewPlatformPS5PlayerStatistics stats, NewPlatformPS5User user, NewPlatformPS5Storage storage)
		: base(entitlements, localization, modding, networking, stats, user, storage)
	{
	}

	public override IEnumerator InitCrt()
	{
		DataLoadingFlags.HandleAbilitiesOnMainThread = true;
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
