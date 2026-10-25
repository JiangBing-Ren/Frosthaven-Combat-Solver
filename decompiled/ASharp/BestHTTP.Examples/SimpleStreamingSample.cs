using System;
using BestHTTP.SignalR;
using UnityEngine;

namespace BestHTTP.Examples;

public sealed class SimpleStreamingSample : MonoBehaviour
{
	private readonly Uri URI = new Uri(GUIHelper.BaseURL + "/streaming-connection");

	private Connection signalRConnection;

	private GUIMessageList messages = new GUIMessageList();

	private void Start()
	{
		signalRConnection = new Connection(URI);
		signalRConnection.OnNonHubMessage += signalRConnection_OnNonHubMessage;
		signalRConnection.OnStateChanged += signalRConnection_OnStateChanged;
		signalRConnection.OnError += signalRConnection_OnError;
		signalRConnection.Open();
	}

	private void OnDestroy()
	{
		signalRConnection.Close();
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, drawHeader: true, () =>
		{
			GUILayout.Label("Messages");
			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			messages.Draw(Screen.width - 20, 0f);
			GUILayout.EndHorizontal();
		});
	}

	private void signalRConnection_OnNonHubMessage(Connection connection, object data)
	{
		messages.Add("[Server Message] " + data.ToString());
	}

	private void signalRConnection_OnStateChanged(Connection connection, ConnectionStates oldState, ConnectionStates newState)
	{
		messages.Add($"[State Change] {oldState} => {newState}");
	}

	private void signalRConnection_OnError(Connection connection, string error)
	{
		messages.Add("[Error] " + error);
	}
}
