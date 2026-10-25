using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

public class PlayParticleOnAwake : MonoBehaviour
{
	[ContextMenu("Play")]
	private void OnEnable()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play(withChildren: true);
		}
	}
}
