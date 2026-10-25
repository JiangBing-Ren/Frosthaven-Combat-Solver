using SharedLibrary.Achievments;

namespace Achievements;

public class AchievementContext
{
	public GlobalStatRepository GlobalStats => GlobalStatRepository.Instance;
}
