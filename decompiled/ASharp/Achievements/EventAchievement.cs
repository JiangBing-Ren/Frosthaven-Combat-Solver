namespace Achievements;

public abstract class EventAchievement : Achievement
{
	protected abstract bool CheckCompleted(AchievementContext context);

	public override bool Check(AchievementContext context)
	{
		if (Completed)
		{
			return false;
		}
		Completed = CheckCompleted(context);
		return Completed;
	}

	public override string GetStringProgress()
	{
		return Completed.ToString();
	}
}
