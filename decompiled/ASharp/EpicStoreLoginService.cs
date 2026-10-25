using System;
using Assets.Script.Misc;

public class EpicStoreLoginService : INetworkLoginService
{
	public bool IsSignedIn()
	{
		return SaveData.Instance.Global.EpicLogin;
	}

	public ICallbackPromise SignIn()
	{
		if (!PlatformLayer.Instance.NewPlatformLayer.IsSignedIn())
		{
			throw new Exception("Platform not signed in");
		}
		return CallbackPromise.Resolved();
	}

	public ICallbackPromise SignOut()
	{
		SaveData.Instance.Global.EpicLogin = false;
		return CallbackPromise.Resolved();
	}

	public bool IsSigningOut()
	{
		return false;
	}

	public bool IsSigningIn()
	{
		return false;
	}
}
