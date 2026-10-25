using UnityEngine;

public class TrailMaterialFactory : MonoBehaviour
{
	private void Awake()
	{
		ParticleSystemRenderer component = GetComponent<ParticleSystemRenderer>();
		Material trailMaterial = new Material(component.trailMaterial);
		component.trailMaterial = trailMaterial;
	}
}
