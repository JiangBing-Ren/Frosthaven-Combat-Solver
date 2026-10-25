using System;
using System.Collections.Generic;

namespace Gloomhaven;

[Serializable]
public class PerformanceProfiles
{
	[Serializable]
	public class Profile
	{
		public string Name;

		public int QSIndex;

		public float MinScoreCPU;

		public float MinScoreGPU;

		public bool Pass(float cpuScore, float gpuScore)
		{
			if (cpuScore >= MinScoreCPU)
			{
				return gpuScore >= MinScoreGPU;
			}
			return false;
		}
	}

	public List<Profile> Profiles = new List<Profile>();

	private void EnsureProfilesMatchQualitySettings()
	{
	}

	public Profile MatchProfile(float cpuScore, float gpuScore)
	{
		EnsureProfilesMatchQualitySettings();
		Profile profile = null;
		foreach (Profile profile2 in Profiles)
		{
			if (profile2.Pass(cpuScore, gpuScore))
			{
				profile = profile2;
			}
		}
		return profile ?? Profiles[0];
	}
}
