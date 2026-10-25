using System.Collections;

namespace FH.Platforms;

[PlatformID("Editor")]
internal class NewPlatformEditor : NewPlatform
{
	public override bool IsAvailableAndReady => true;

	public static NewPlatformEditor Create()
	{
		NewPlatformEditorUser user = new NewPlatformEditorUser();
		NewPlatformEditorStorage storage = new NewPlatformEditorStorage();
		return new NewPlatformEditor(new NewPlatformEditorEntitlements(), new NewPlatformEditorLocalization(), new NewPlatformEditorModding(), new NewPlatformEditorNetworking(), new NewPlatformEditorPlayerStatistics(), user, storage);
	}

	protected NewPlatformEditor(NewPlatformEditorEntitlements entitlements, NewPlatformEditorLocalization localization, NewPlatformEditorModding modding, NewPlatformEditorNetworking networking, NewPlatformEditorPlayerStatistics stats, NewPlatformEditorUser user, NewPlatformEditorStorage storage)
		: base(entitlements, localization, modding, networking, stats, user, storage)
	{
	}

	public override IEnumerator InitCrt()
	{
		yield break;
	}
}
