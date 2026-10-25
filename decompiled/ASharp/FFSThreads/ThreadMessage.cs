using System.Threading;

namespace FFSThreads;

public class ThreadMessage
{
	public EThreadMessageType Type { get; }

	public string ThreadCreatedOn { get; }

	public ThreadMessage(EThreadMessageType type)
	{
		Type = type;
		ThreadCreatedOn = Thread.CurrentThread.Name;
	}
}
