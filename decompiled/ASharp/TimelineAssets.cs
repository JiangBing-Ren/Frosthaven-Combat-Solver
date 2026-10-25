using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class TimelineAssets : MonoBehaviour
{
	public TimelineAsset m_CreateTimeline;

	public List<TimelineAsset> m_TimelineAssets;

	public TimelineAsset FindTimelineAsset(string name)
	{
		return m_TimelineAssets.Find((TimelineAsset x) => x.name == name);
	}
}
