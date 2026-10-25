using System.Threading.Tasks;
using UnityEngine;

public class WaitForTask : CustomYieldInstruction
{
	private readonly Task _task;

	public override bool keepWaiting => !_task.IsCompleted;

	public WaitForTask(Task task)
	{
		_task = task;
	}
}
