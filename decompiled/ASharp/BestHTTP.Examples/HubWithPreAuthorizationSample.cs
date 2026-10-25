using System;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using BestHTTP.SignalRCore.Messages;
using UnityEngine;

namespace BestHTTP.Examples;

public sealed class HubWithPreAuthorizationSample : MonoBehaviour
{
	private readonly Uri URI = new Uri(GUIHelper.BaseURL + "/HubWithAuthorization");

	private readonly Uri AuthURI = new Uri(GUIHelper.BaseURL + "/generateJwtToken");

	private HubConnection hub;

	private Vector2 scrollPos;

	private string uiText;

	private void Start()
	{
		hub = new HubConnection(URI, new JsonProtocol(new LitJsonEncoder()));
		hub.AuthenticationProvider = new PreAuthAccessTokenAuthenticator(AuthURI);
		hub.AuthenticationProvider.OnAuthenticationSucceded += AuthenticationProvider_OnAuthenticationSucceded;
		hub.AuthenticationProvider.OnAuthenticationFailed += AuthenticationProvider_OnAuthenticationFailed;
		hub.OnConnected += Hub_OnConnected;
		hub.OnError += Hub_OnError;
		hub.OnClosed += Hub_OnClosed;
		hub.OnMessage += Hub_OnMessage;
		hub.StartConnect();
		uiText = "StartConnect called\n";
	}

	private void AuthenticationProvider_OnAuthenticationSucceded(IAuthenticationProvider provider)
	{
		string text = $"Pre-Authentication Succeded! Token: '{(hub.AuthenticationProvider as PreAuthAccessTokenAuthenticator).Token}' \n";
		Debug.Log(text);
		uiText += text;
	}

	private void AuthenticationProvider_OnAuthenticationFailed(IAuthenticationProvider provider, string reason)
	{
		uiText += $"Authentication Failed! Reason: '{reason}'\n";
	}

	private void OnDestroy()
	{
		if (hub != null)
		{
			hub.StartClose();
		}
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, drawHeader: true, () =>
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
			GUILayout.BeginVertical();
			GUILayout.Label(uiText);
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		});
	}

	private void Hub_OnConnected(HubConnection hub)
	{
		uiText += "Hub Connected\n";
		hub.Invoke<string>("Echo", new object[1] { "Message from the client" }).OnSuccess((string ret) =>
		{
			uiText += $" 'Echo' returned: '{ret}'\n";
		});
	}

	private bool Hub_OnMessage(HubConnection hub, Message message)
	{
		return true;
	}

	private void Hub_OnClosed(HubConnection hub)
	{
		uiText += "Hub Closed\n";
	}

	private void Hub_OnError(HubConnection hub, string error)
	{
		uiText = uiText + "Hub Error: " + error + "\n";
	}
}
