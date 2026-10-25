namespace FFSThreads;

public class ThreadMessage_ShowErrorMessage : ThreadMessage
{
	public string MessageKey { get; private set; }

	public ThreadMessage_ShowErrorMessage(string messageKey)
		: base(EThreadMessageType.ShowErrorMessage)
	{
		MessageKey = messageKey;
	}
}
