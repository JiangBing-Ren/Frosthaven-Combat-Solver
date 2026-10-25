using System;
using System.IO;
using System.Threading.Tasks;

public static class FHTaskExtensions
{
	private class TaskTimer
	{
		private readonly Task _timer;

		public bool IsDone
		{
			get
			{
				if (_timer != null)
				{
					return _timer.IsCompleted;
				}
				return true;
			}
		}

		public TaskTimer(int timeoutInMilliseconds)
		{
			_timer = ((timeoutInMilliseconds > 0) ? Task.Run(async () =>
			{
				await Task.Delay(timeoutInMilliseconds);
			}) : null);
		}
	}

	public static async Task WaitWhile(Func<bool> condition, int frequency = 25)
	{
		while (condition())
		{
			await Task.Delay(frequency);
		}
	}

	public static async Task WaitWhile(Func<bool> condition, int frequency, int timeout)
	{
		TaskTimer timer = new TaskTimer(timeout);
		while (condition())
		{
			if (timer.IsDone)
			{
				throw new TimeoutException();
			}
			await Task.Delay(frequency);
		}
	}

	public static async Task WaitUntil(Func<bool> condition, int frequency = 25)
	{
		while (!condition())
		{
			await Task.Delay(frequency);
		}
	}

	public static async Task WaitUntil(Func<bool> condition, int frequency, int timeout)
	{
		TaskTimer timer = new TaskTimer(timeout);
		while (!condition())
		{
			if (timer.IsDone)
			{
				throw new TimeoutException();
			}
			await Task.Delay(frequency);
		}
	}

	public static async Task<string> CombineAsyncPaths(Task<string> asyncPath, string syncPath)
	{
		return Path.Combine(await asyncPath, syncPath);
	}

	public static async Task<string> CombineAsyncPaths(Task<string> asyncPath, string syncPathA, string syncPathB)
	{
		return Path.Combine(await asyncPath, syncPathA, syncPathB);
	}

	public static bool HasFailed(this Task task, out Exception exception)
	{
		if (task.IsCompleted && !task.IsCompletedSuccessfully)
		{
			exception = task.Exception;
			return true;
		}
		exception = null;
		return false;
	}
}
