using System;
using BestHTTP.SignalRCore;

namespace BestHTTP.Examples;

public sealed class RedirectLoggerAccessTokenAuthenticator : IAuthenticationProvider
{
	private HubConnection _connection;

	public bool IsPreAuthRequired => false;

	public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

	public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

	public RedirectLoggerAccessTokenAuthenticator(HubConnection connection)
	{
		_connection = connection;
	}

	public void StartAuthentication()
	{
	}

	public void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("x-redirect-count", _connection.RedirectCount.ToString());
		if (HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) == SupportedProtocols.HTTP)
		{
			request.Uri = PrepareUri(request.Uri);
		}
	}

	public Uri PrepareUri(Uri uri)
	{
		if (_connection.NegotiationResult != null && !string.IsNullOrEmpty(_connection.NegotiationResult.AccessToken))
		{
			string text = (string.IsNullOrEmpty(uri.Query) ? "?" : (uri.Query + "&"));
			return new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, text + "access_token=" + _connection.NegotiationResult.AccessToken).Uri;
		}
		return uri;
	}
}
