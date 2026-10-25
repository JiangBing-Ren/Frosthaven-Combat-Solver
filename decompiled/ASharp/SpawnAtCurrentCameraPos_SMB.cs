using Chronos;
using UnityEngine;

public class SpawnAtCurrentCameraPos_SMB : StateMachineBehaviour
{
	public GameObject particles;

	public float lifetime = 5f;

	public Vector3 particleOffset;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		GameObject gameObject = Object.Instantiate(particles, Camera.main.transform);
		gameObject.SetActive(value: false);
		gameObject.transform.localPosition = particleOffset;
		gameObject.SetActive(value: true);
		Object.Destroy(gameObject, lifetime);
	}
}
