namespace FFSThreads;

public class ThreadMessage_IncrementProgressBar : ThreadMessage
{
	public float IncrementAmount { get; private set; }

	public ThreadMessage_IncrementProgressBar(float incrementAmount)
		: base(EThreadMessageType.IncrementProgressBar)
	{
		IncrementAmount = incrementAmount;
	}
}
