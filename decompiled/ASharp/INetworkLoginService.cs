using Assets.Script.Misc;

public interface INetworkLoginService
{
	bool IsSignedIn();

	ICallbackPromise SignIn();

	ICallbackPromise SignOut();

	bool IsSigningOut();

	bool IsSigningIn();
}
