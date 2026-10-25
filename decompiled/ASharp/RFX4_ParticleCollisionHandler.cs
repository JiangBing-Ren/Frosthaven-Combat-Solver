using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RFX4_ParticleCollisionHandler : MonoBehaviour
{
	public GameObject[] EffectsOnCollision;

	public float Offset;

	public float DestroyTimeDelay = 5f;

	public bool UseWorldSpacePosition;

	private ParticleSystem part;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private ParticleSystem ps;

	private void Start()
	{
		part = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		int num = part.GetCollisionEvents(other, collisionEvents);
		for (int i = 0; i < num; i++)
		{
			GameObject[] effectsOnCollision = EffectsOnCollision;
			for (int j = 0; j < effectsOnCollision.Length; j++)
			{
				GameObject gameObject = Object.Instantiate(effectsOnCollision[j], collisionEvents[i].intersection + collisionEvents[i].normal * Offset, default);
				gameObject.transform.LookAt(collisionEvents[i].intersection + collisionEvents[i].normal);
				if (!UseWorldSpacePosition)
				{
					gameObject.transform.parent = transform;
				}
				Object.Destroy(gameObject, DestroyTimeDelay);
			}
		}
	}
}
