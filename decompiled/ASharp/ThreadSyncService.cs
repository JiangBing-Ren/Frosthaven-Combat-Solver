using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadSyncService : Singleton<ThreadSyncService>
{
	private Queue<Action> m_MainThreadTasksQueue = new Queue<Action>();

	private Thread m_MainThread;

	protected override void AwakeInit()
	{
		m_MainThread = Thread.CurrentThread;
	}

	public void ScheduleMainThreadTask(Action task)
	{
		if (Thread.CurrentThread == m_MainThread)
		{
			task?.Invoke();
			return;
		}
		lock (m_MainThreadTasksQueue)
		{
			m_MainThreadTasksQueue.Enqueue(task);
		}
	}

	private void Update()
	{
		while (m_MainThreadTasksQueue.Count > 0)
		{
			Action action = null;
			lock (m_MainThreadTasksQueue)
			{
				action = m_MainThreadTasksQueue.Dequeue();
			}
			action?.Invoke();
		}
	}
}
