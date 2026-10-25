using System;
using Assets.Script.Misc;

public class DummyNetworkLoginService : INetworkLoginService
{
	private bool changing;

	private bool signedIn;

	private bool signinInitialised;

	private float delaySign;

	public DummyNetworkLoginService(bool signedIn = true, float delaySign = 0f)
	{
		this.signedIn = signedIn;
		this.delaySign = delaySign;
	}

	public bool IsSignedIn()
	{
		if (!signinInitialised)
		{
			signedIn = SaveData.Instance.Global.EpicLogin;
			signinInitialised = true;
		}
		return signedIn;
	}

	public ICallbackPromise SignIn()
	{
		throw new NotImplementedException();
	}

	public ICallbackPromise SignOut()
	{
		throw new NotImplementedException();
	}

	private ICallbackPromise DummyDelay()
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		CoroutineHelper.RunDelayedAction(delaySign, callbackPromise.Resolve);
		return callbackPromise;
	}

	public bool IsSigningOut()
	{
		return changing;
	}

	public bool IsSigningIn()
	{
		return changing;
	}
}
