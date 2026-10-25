using System;

namespace Achievements;

public abstract class CounterAchievement : Achievement
{
	public int Max { get; protected set; }

	public int Progress { get; private set; }

	protected abstract int CheckProgress(AchievementContext context);

	public override bool Check(AchievementContext context)
	{
		if (Completed)
		{
			return false;
		}
		Progress = CheckProgress(context);
		Progress = Math.Max(0, Math.Min(Progress, Max));
		Completed = Progress == Max;
		return Completed;
	}

	public override void Reset()
	{
		base.Reset();
		Progress = 0;
	}

	public override void Complete()
	{
		base.Complete();
		Progress = Max;
	}

	public override string GetStringProgress()
	{
		return $"{Progress}/{Max}";
	}
}
