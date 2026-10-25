using UnityEngine;
using UnityEngine.Playables;

public class OnSpawnPropFX : MonoBehaviour
{
	public PlayableDirector Director;

	public TimelineAssets Timelines;

	public SpawnPFXOnEnable SpawnPFX;

	private void Start()
	{
		PropParent componentInParent = GetComponentInParent<PropParent>();
		if (componentInParent != null && !componentInParent.PlacedInScenario)
		{
			if (Director != null && Timelines != null)
			{
				Director.Play(Timelines.m_CreateTimeline);
			}
			if (SpawnPFX != null)
			{
				SpawnPFX.enabled = true;
			}
		}
	}
}
