using UnityEngine;

public class WaitForSecondsFrozen : CustomYieldInstruction
{
	private float waitTime;

	private float lastCheckTime;

	public override bool keepWaiting
	{
		get
		{
			if (!TimeManager.IsPaused)
			{
				waitTime -= Time.realtimeSinceStartup - lastCheckTime;
			}
			lastCheckTime = Time.realtimeSinceStartup;
			return waitTime > 0f;
		}
	}

	public WaitForSecondsFrozen(float time)
	{
		lastCheckTime = Time.realtimeSinceStartup;
		waitTime = time;
	}
}
