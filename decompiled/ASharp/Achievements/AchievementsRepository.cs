using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Achievements;

public class AchievementsRepository
{
	public readonly ShardSeekerAchievement ShardSeeker = new ShardSeekerAchievement();

	public readonly CrownUnitedAchievement CrownUnited = new CrownUnitedAchievement();

	public readonly IntoTheForestAchievement IntoTheForest = new IntoTheForestAchievement();

	public readonly DestroyerOfTheIcespeakers DestroyerOfTheIcespeakers = new DestroyerOfTheIcespeakers();

	public readonly DestroyerOfTheSnowspeakers DestroyerOfTheSnowspeakers = new DestroyerOfTheSnowspeakers();

	public readonly FriendOfTheIcespeakers FriendOfTheIcespeakers = new FriendOfTheIcespeakers();

	public readonly FriendOfTheSnowspeakers FriendOfTheSnowspeakers = new FriendOfTheSnowspeakers();

	public readonly FriendOfTheFrostspeakers FriendOfTheFrostspeakers = new FriendOfTheFrostspeakers();

	public readonly UnfetteredDeactivated UnfetteredDeactivated = new UnfetteredDeactivated();

	public readonly UnfetteredAllies UnfetteredAllies = new UnfetteredAllies();

	public readonly FriendOfTheFishKing FriendOfTheFishKing = new FriendOfTheFishKing();

	public readonly HarvestFestival HarvestFestival = new HarvestFestival();

	public readonly RedWell RedWell = new RedWell();

	public readonly BlueWell BlueWell = new BlueWell();

	public readonly Portcullis Portcullis = new Portcullis();

	public readonly Stage Stage = new Stage();

	public readonly IceSpike IceSpike = new IceSpike();

	public readonly Firepepper Firepepper = new Firepepper();

	public readonly BugInAJar BugInAJar = new BugInAJar();

	public readonly EmptyJar EmptyJar = new EmptyJar();

	public readonly ChargedCore ChargedCore = new ChargedCore();

	public readonly Uncharged Uncharged = new Uncharged();

	public readonly BrummixAuthority BrummixAuthority = new BrummixAuthority();

	public readonly BrummixScout BrummixScout = new BrummixScout();

	public readonly BrummixTracker BrummixTracker = new BrummixTracker();

	public readonly BrummixTracksTyro BrummixTracksTyro = new BrummixTracksTyro();

	public readonly BrummixWhisperer BrummixWhisperer = new BrummixWhisperer();

	public readonly ExploreroftheNorth ExploreroftheNorth = new ExploreroftheNorth();

	public readonly CenturionoftheNorth CenturionoftheNorth = new CenturionoftheNorth();

	public readonly PrefectoftheNorth PrefectoftheNorth = new PrefectoftheNorth();

	public readonly TribuneoftheNorth TribuneoftheNorth = new TribuneoftheNorth();

	public readonly LegateoftheNorth LegateoftheNorth = new LegateoftheNorth();

	public readonly SaintoftheNorth SaintoftheNorth = new SaintoftheNorth();

	public readonly InTheNickofTime InTheNickofTime = new InTheNickofTime();

	public readonly WarchiefDenied WarchiefDenied = new WarchiefDenied();

	public readonly WhiteDeath WhiteDeath = new WhiteDeath();

	public readonly JustSlide JustSlide = new JustSlide();

	public readonly Statue Statue = new Statue();

	private List<Achievement> _allAchievements = new List<Achievement>();

	private List<Achievement> _scartch = new List<Achievement>();

	public IEnumerable<Achievement> AllAchievements => _allAchievements;

	public AchievementsRepository()
	{
		_allAchievements = (from f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
			where typeof(Achievement).IsAssignableFrom(f.FieldType)
			select (Achievement)f.GetValue(this)).ToList();
	}

	public void Restore(IEnumerable<Achievement.StorageData> storage)
	{
		Reset();
		foreach (Achievement.StorageData storageData in storage)
		{
			AllAchievements.FirstOrDefault((Achievement a) => a.ID == storageData.ID)?.Restore(storageData);
		}
	}

	public void Reset()
	{
		foreach (Achievement allAchievement in AllAchievements)
		{
			allAchievement.Reset();
		}
	}

	public IEnumerable<Achievement> Check(AchievementContext context)
	{
		_scartch.Clear();
		_scartch.AddRange(AllAchievements.Where((Achievement c) => c.Check(context)));
		return _scartch;
	}
}
