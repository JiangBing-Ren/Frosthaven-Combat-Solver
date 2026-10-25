using System;
using BestHTTP.SignalRCore;

namespace BestHTTP.Examples;

public sealed class PreAuthAccessTokenAuthenticator : IAuthenticationProvider
{
	private Uri authenticationUri;

	public bool IsPreAuthRequired => true;

	public string Token { get; private set; }

	public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

	public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

	public PreAuthAccessTokenAuthenticator(Uri authUri)
	{
		authenticationUri = authUri;
	}

	public void StartAuthentication()
	{
		new HTTPRequest(authenticationUri, OnAuthenticationRequestFinished).Send();
	}

	private void OnAuthenticationRequestFinished(HTTPRequest req, HTTPResponse resp)
	{
		switch (req.State)
		{
		case HTTPRequestStates.Finished:
			if (resp.IsSuccess)
			{
				Token = resp.DataAsText;
				if (OnAuthenticationSucceded != null)
				{
					OnAuthenticationSucceded(this);
				}
			}
			else
			{
				AuthenticationFailed($"Request Finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}");
			}
			break;
		case HTTPRequestStates.Error:
			AuthenticationFailed("Request Finished with Error! " + ((req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
			break;
		case HTTPRequestStates.Aborted:
			AuthenticationFailed("Request Aborted!");
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			AuthenticationFailed("Connection Timed Out!");
			break;
		case HTTPRequestStates.TimedOut:
			AuthenticationFailed("Processing the request Timed Out!");
			break;
		}
	}

	private void AuthenticationFailed(string reason)
	{
		if (OnAuthenticationFailed != null)
		{
			OnAuthenticationFailed(this, reason);
		}
	}

	public void PrepareRequest(HTTPRequest request)
	{
		if (HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) == SupportedProtocols.HTTP)
		{
			request.Uri = PrepareUri(request.Uri);
		}
	}

	public Uri PrepareUri(Uri uri)
	{
		if (!string.IsNullOrEmpty(Token))
		{
			string text = (string.IsNullOrEmpty(uri.Query) ? "?" : (uri.Query + "&"));
			return new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, text + "access_token=" + Token).Uri;
		}
		return uri;
	}
}
