using System;
using System.Collections;

namespace FH.Platforms;

public abstract class NewPlatform : IDisposable
{
	internal readonly NewPlatformEntitlements Entitlements;

	internal readonly NewPlatformLocalization Localization;

	internal readonly INewPlatformModding Modding;

	internal readonly INewPlatformNetworking Networking;

	internal readonly NewPlatformPlayerStatistics PlayerStatistics;

	internal readonly INewPlatformStorage Storage;

	internal readonly NewPlatformUser User;

	public abstract bool IsAvailableAndReady { get; }

	public string ID { get; private set; }

	protected NewPlatform(NewPlatformEntitlements entitlements, NewPlatformLocalization localization, INewPlatformModding modding, INewPlatformNetworking networking, NewPlatformPlayerStatistics stats, NewPlatformUser user, INewPlatformStorage storage)
	{
		Entitlements = entitlements;
		Localization = localization;
		Modding = modding;
		Networking = networking;
		PlayerStatistics = stats;
		User = user;
		Storage = storage;
		object[] customAttributes = GetType().GetCustomAttributes(typeof(PlatformIDAttribute), inherit: true);
		if (customAttributes.Length != 0)
		{
			ID = ((PlatformIDAttribute)customAttributes[0]).ID;
			return;
		}
		throw new Exception("Platform class is missing the Platform ID attribute");
	}

	public abstract IEnumerator InitCrt();

	public virtual IEnumerator Tick()
	{
		yield return PlayerStatistics.Tick();
	}

	public virtual void Dispose()
	{
		User.Dispose();
	}
}
