using System;
using System.Collections.Generic;
using GLOOM;
using UnityEngine;

public class MessageHandler : Singleton<MessageHandler>
{
	private struct MessageInfo(Message message, Action onClosed)
	{
		public Action onClosed = onClosed;

		public Message message = message;
	}

	public UIMessage messagePrefab;

	private List<MessageInfo> pendingMessages = new List<MessageInfo>();

	private MessageInfo currentMessage;

	private void Start()
	{
		messagePrefab.Hide();
		messagePrefab.OnClosePressed.AddListener(ShowNext);
	}

	private void ShowNext()
	{
		pendingMessages.Remove(currentMessage);
		if (pendingMessages.Count > 0)
		{
			ShowImmendiately(pendingMessages[0]);
		}
	}

	public void Show(string title, Sprite image, string description, Action onClosed = null)
	{
		Show(new Message(title, image, description), onClosed);
	}

	public void Show(Message message, Action onClosed = null)
	{
		MessageInfo messageInfo = new MessageInfo(message, onClosed);
		pendingMessages.Add(messageInfo);
		if (pendingMessages.Count == 1)
		{
			ShowImmendiately(messageInfo);
		}
	}

	private void ShowImmendiately(MessageInfo message)
	{
		currentMessage = message;
		messagePrefab.Show(message.message, message.onClosed);
	}
}
