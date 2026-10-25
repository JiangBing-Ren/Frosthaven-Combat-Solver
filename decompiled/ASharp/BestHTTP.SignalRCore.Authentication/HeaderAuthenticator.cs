using System;

namespace BestHTTP.SignalRCore.Authentication;

public sealed class HeaderAuthenticator : IAuthenticationProvider
{
	private string _credentials;

	public bool IsPreAuthRequired => false;

	public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

	public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

	public HeaderAuthenticator(string credentials)
	{
		_credentials = credentials;
	}

	public void StartAuthentication()
	{
	}

	public void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("Authorization", "Bearer " + _credentials);
	}

	public Uri PrepareUri(Uri uri)
	{
		return uri;
	}
}
