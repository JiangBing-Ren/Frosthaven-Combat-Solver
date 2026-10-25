namespace Achievements;

public abstract class Achievement
{
	public class StorageData
	{
		public string ID;

		public bool Completed;
	}

	public string Name { get; protected set; }

	public string ID { get; protected set; }

	public bool Completed { get; protected set; }

	public virtual bool Check(AchievementContext context)
	{
		_ = Completed;
		return false;
	}

	public virtual void Reset()
	{
		Completed = false;
	}

	public virtual void Complete()
	{
		Completed = true;
	}

	public void Restore(StorageData storage)
	{
		Reset();
		if (storage.Completed)
		{
			Complete();
		}
	}

	public abstract string GetStringProgress();
}
