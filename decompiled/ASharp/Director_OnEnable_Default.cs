using UnityEngine.Playables;

public class Director_OnEnable_Default : EffectOnEnable
{
	private void OnEnable()
	{
		PlayableDirector component = gameObject.GetComponent<PlayableDirector>();
		if (component != null)
		{
			component.Play();
		}
		PlayableDirector[] componentsInChildren = gameObject.GetComponentsInChildren<PlayableDirector>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
	}
}
