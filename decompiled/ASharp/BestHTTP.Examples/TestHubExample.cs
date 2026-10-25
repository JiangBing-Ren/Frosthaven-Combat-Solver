using System;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using BestHTTP.SignalRCore.Messages;
using UnityEngine;

namespace BestHTTP.Examples;

public class TestHubExample : MonoBehaviour
{
	private sealed class Person
	{
		public string Name { get; set; }

		public long Age { get; set; }

		public override string ToString()
		{
			return $"[Person Name: '{Name}', Age: {Age.ToString()}]";
		}
	}

	private readonly Uri URI = new Uri(GUIHelper.BaseURL + "/TestHub");

	private HubConnection hub;

	private Vector2 scrollPos;

	private string uiText;

	private void Start()
	{
		HubOptions hubOptions = new HubOptions();
		hubOptions.SkipNegotiation = false;
		hub = new HubConnection(URI, new JsonProtocol(new LitJsonEncoder()), hubOptions);
		hub.OnConnected += Hub_OnConnected;
		hub.OnError += Hub_OnError;
		hub.OnClosed += Hub_OnClosed;
		hub.OnMessage += Hub_OnMessage;
		hub.On("Send", (string arg) =>
		{
			uiText += $" On Send: {arg}\n";
		});
		hub.On("Person", (Person person) =>
		{
			uiText += $" On Person: {person}\n";
		});
		hub.On("TwoPersons", (Person person1, Person person2) =>
		{
			uiText += $" On TwoPersons: {person1}, {person2}\n";
		});
		hub.StartConnect();
		uiText = "StartConnect called\n";
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
		hub.Send("Send", "my message");
		hub.Invoke<string>("NoParam", Array.Empty<object>()).OnSuccess((string ret) =>
		{
			uiText += $" 'NoParam' returned: {ret}\n";
		});
		hub.Invoke<int>("Add", new object[2] { 10, 20 }).OnSuccess((int result) =>
		{
			uiText += $" 'Add(10, 20)' returned: {result}\n";
		}).OnError((Exception error) =>
		{
			uiText += $" 'Add(10, 20)' error: {error}\n";
		});
		hub.Invoke<Person>("GetPerson", new object[2] { "Mr. Smith", 26 }).OnSuccess((Person result) =>
		{
			uiText += $" 'GetPerson(\"Mr. Smith\", 26)' returned: {result}\n";
		}).OnError((Exception error) =>
		{
			uiText += $" 'GetPerson(\"Mr. Smith\", 26)' error: {error}\n";
		});
		hub.Invoke<int>("SingleResultFailure", new object[2] { 10, 20 }).OnSuccess((int result) =>
		{
			uiText += $" 'SingleResultFailure(10, 20)' returned: {result}\n";
		}).OnError((Exception error) =>
		{
			uiText += $" 'SingleResultFailure(10, 20)' error: {error}\n";
		});
		hub.Invoke<int[]>("Batched", new object[1] { 10 }).OnSuccess((int[] result) =>
		{
			uiText += $" 'Batched(10)' returned items: {result.Length}\n";
		}).OnError((Exception error) =>
		{
			uiText += $" 'Batched(10)' error: {error}\n";
		});
		hub.Stream<int>("ObservableCounter", new object[2] { 10, 1000 }).OnItem((StreamItemContainer<int> result) =>
		{
			uiText += $" 'ObservableCounter(10, 1000)' OnItem: {result.LastAdded}\n";
		}).OnSuccess((StreamItemContainer<int> result) =>
		{
			uiText += $" 'ObservableCounter(10, 1000)' OnSuccess. Final count: {result.Items.Count}\n";
		})
			.OnError((Exception error) =>
			{
				uiText += $" 'ObservableCounter(10, 1000)' error: {error}\n";
			});
		StreamItemContainer<int> value = hub.Stream<int>("ChannelCounter", new object[2] { 10, 1000 }).OnItem((StreamItemContainer<int> result) =>
		{
			uiText += $" 'ChannelCounter(10, 1000)' OnItem: {result.LastAdded}\n";
		}).OnSuccess((StreamItemContainer<int> result) =>
		{
			uiText += $" 'ChannelCounter(10, 1000)' OnSuccess. Final count: {result.Items.Count}\n";
		})
			.OnError((Exception error) =>
			{
				uiText += $" 'ChannelCounter(10, 1000)' error: {error}\n";
			})
			.value;
		hub.CancelStream(value);
		hub.Stream<Person>("GetRandomPersons", new object[2] { 20, 2000 }).OnItem((StreamItemContainer<Person> result) =>
		{
			uiText += $" 'GetRandomPersons(20, 1000)' OnItem: {result.LastAdded}\n";
		}).OnSuccess((StreamItemContainer<Person> result) =>
		{
			uiText += $" 'GetRandomPersons(20, 1000)' OnSuccess. Final count: {result.Items.Count}\n";
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
