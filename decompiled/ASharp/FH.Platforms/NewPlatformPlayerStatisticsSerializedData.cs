using System;
using System.Collections.Generic;

namespace FH.Platforms;

[Serializable]
internal struct NewPlatformPlayerStatisticsSerializedData
{
	public Queue<string> PendingAchievements;

	public Queue<(string, int)> PendingStats;
}
