namespace FFSThreads;

public class ThreadMessage_UpdateProgressBar : ThreadMessage
{
	public float UpdateAmount { get; private set; }

	public ThreadMessage_UpdateProgressBar(float updateAmount)
		: base(EThreadMessageType.UpdateProgessBar)
	{
		UpdateAmount = updateAmount;
	}
}
