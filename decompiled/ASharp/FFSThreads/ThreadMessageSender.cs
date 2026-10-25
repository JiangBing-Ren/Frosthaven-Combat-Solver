namespace FFSThreads;

public class ThreadMessageSender
{
	public delegate void ThreadMessagingCallbackHandler(ThreadMessage message);

	private ThreadMessagingCallbackHandler m_CallbackHandler;

	public void SendMessage(ThreadMessage message)
	{
		m_CallbackHandler(message);
	}

	public ThreadMessageSender(ThreadMessagingCallbackHandler handler)
	{
		m_CallbackHandler = handler;
	}
}
