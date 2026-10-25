using System;
using BestHTTP.WebSocket;
using UnityEngine;

namespace BestHTTP.Examples;

public class WebSocketSample : MonoBehaviour
{
	private string address = "wss://echo.websocket.org";

	private string msgToSend = "Hello World!";

	private string Text = string.Empty;

	private BestHTTP.WebSocket.WebSocket webSocket;

	private Vector2 scrollPos;

	private void OnDestroy()
	{
		if (webSocket != null)
		{
			webSocket.Close();
		}
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, drawHeader: true, () =>
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			GUILayout.Label(Text);
			GUILayout.EndScrollView();
			GUILayout.Space(5f);
			GUILayout.FlexibleSpace();
			address = GUILayout.TextField(address);
			if (this.webSocket == null && GUILayout.Button("Open Web Socket"))
			{
				this.webSocket = new BestHTTP.WebSocket.WebSocket(new Uri(address));
				this.webSocket.StartPingThread = true;
				if (HTTPManager.Proxy != null)
				{
					this.webSocket.InternalRequest.Proxy = new HTTPProxy(HTTPManager.Proxy.Address, HTTPManager.Proxy.Credentials, isTransparent: false);
				}
				BestHTTP.WebSocket.WebSocket webSocket = this.webSocket;
				webSocket.OnOpen = (OnWebSocketOpenDelegate)Delegate.Combine(webSocket.OnOpen, new OnWebSocketOpenDelegate(OnOpen));
				BestHTTP.WebSocket.WebSocket webSocket2 = this.webSocket;
				webSocket2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine(webSocket2.OnMessage, new OnWebSocketMessageDelegate(OnMessageReceived));
				BestHTTP.WebSocket.WebSocket webSocket3 = this.webSocket;
				webSocket3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Combine(webSocket3.OnClosed, new OnWebSocketClosedDelegate(OnClosed));
				BestHTTP.WebSocket.WebSocket webSocket4 = this.webSocket;
				webSocket4.OnError = (OnWebSocketErrorDelegate)Delegate.Combine(webSocket4.OnError, new OnWebSocketErrorDelegate(OnError));
				this.webSocket.Open();
				Text += "Opening Web Socket...\n";
			}
			if (this.webSocket != null && this.webSocket.IsOpen)
			{
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				msgToSend = GUILayout.TextField(msgToSend);
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Send", GUILayout.MaxWidth(70f)))
				{
					Text += "Sending message...\n";
					this.webSocket.Send(msgToSend);
				}
				GUILayout.Space(10f);
				if (GUILayout.Button("Close"))
				{
					this.webSocket.Close(1000, "Bye!");
				}
			}
		});
	}

	private void OnOpen(BestHTTP.WebSocket.WebSocket ws)
	{
		Text += $"-WebSocket Open!\n";
	}

	private void OnMessageReceived(BestHTTP.WebSocket.WebSocket ws, string message)
	{
		Text += $"-Message received: {message}\n";
	}

	private void OnClosed(BestHTTP.WebSocket.WebSocket ws, ushort code, string message)
	{
		Text += $"-WebSocket closed! Code: {code} Message: {message}\n";
		webSocket = null;
	}

	private void OnError(BestHTTP.WebSocket.WebSocket ws, Exception ex)
	{
		string text = string.Empty;
		if (ws.InternalRequest.Response != null)
		{
			text = $"Status Code from Server: {ws.InternalRequest.Response.StatusCode} and Message: {ws.InternalRequest.Response.Message}";
		}
		Text += string.Format("-An error occured: {0}\n", (ex != null) ? ex.Message : ("Unknown Error " + text));
		webSocket = null;
	}
}
