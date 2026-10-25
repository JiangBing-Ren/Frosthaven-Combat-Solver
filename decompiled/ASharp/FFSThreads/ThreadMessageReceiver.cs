using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FFSThreads;

public class ThreadMessageReceiver
{
	private const float c_WaitBetweenLoops = 0.1f;

	private bool m_BlockMessageProcessing;

	private bool m_StopMessageProcessing;

	private List<ThreadMessage> m_MessageQueue = new List<ThreadMessage>();

	private Coroutine m_MessageHandlerCoroutine;

	public bool IsBusy => m_MessageQueue.Count > 0;

	public void StartMessageProcessing()
	{
		m_StopMessageProcessing = false;
		if (m_MessageHandlerCoroutine != null)
		{
			CoroutineHelper.StopCoroutineHelper(m_MessageHandlerCoroutine);
		}
		m_MessageHandlerCoroutine = CoroutineHelper.RunCoroutine(ThreadMessageHandler());
	}

	public void StopMessageProcessing()
	{
		m_StopMessageProcessing = true;
	}

	public void ToggleMessageProcessing(bool value)
	{
		m_BlockMessageProcessing = value;
	}

	public void QueueMessage(ThreadMessage message)
	{
		if (message != null)
		{
			lock (m_MessageQueue)
			{
				m_MessageQueue.Add(message);
			}
		}
	}

	public IEnumerator ThreadMessageHandler()
	{
		while (!m_StopMessageProcessing)
		{
			do
			{
				if (!m_BlockMessageProcessing)
				{
					ThreadMessage threadMessage = null;
					if (m_MessageQueue.Count > 0)
					{
						threadMessage = m_MessageQueue[0];
						if (threadMessage == null)
						{
							lock (m_MessageQueue)
							{
								m_MessageQueue.RemoveAt(0);
							}
							continue;
						}
					}
					if (threadMessage != null)
					{
						ProcessMessage(threadMessage);
						lock (m_MessageQueue)
						{
							m_MessageQueue.Remove(threadMessage);
						}
					}
				}
				else
				{
					yield return new WaitForSeconds(0.1f);
				}
			}
			while (!m_StopMessageProcessing && m_MessageQueue.Count > 0);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void ProcessMessage(ThreadMessage message)
	{
		switch (message.Type)
		{
		case EThreadMessageType.ShowErrorMessage:
			if (message is ThreadMessage_ShowErrorMessage threadMessage_ShowErrorMessage)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle(threadMessage_ShowErrorMessage.MessageKey, "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			}
			else
			{
				Log.Error("ThreadMessageReceiver.ProcessMessage: Unable to cast message to " + EThreadMessageType.ShowErrorMessage);
			}
			break;
		case EThreadMessageType.IncrementProgressBar:
			if (message is ThreadMessage_IncrementProgressBar threadMessage_IncrementProgressBar)
			{
				SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(threadMessage_IncrementProgressBar.IncrementAmount);
			}
			else
			{
				Log.Error("ThreadMessageReceiver.ProcessMessage: Unable to cast message to " + EThreadMessageType.IncrementProgressBar);
			}
			break;
		case EThreadMessageType.UpdateProgessBar:
			if (message is ThreadMessage_UpdateProgressBar threadMessage_UpdateProgressBar)
			{
				SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(threadMessage_UpdateProgressBar.UpdateAmount);
			}
			else
			{
				Log.Error("ThreadMessageReceiver.ProcessMessage: Unable to cast message to " + EThreadMessageType.UpdateProgessBar);
			}
			break;
		default:
			Log.Error("ThreadMessageReceiver.ProcessMessage: Invalid message type " + message.Type);
			break;
		}
	}
}
