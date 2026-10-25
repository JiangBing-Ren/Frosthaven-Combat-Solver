using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class SpawnObjectAnimateMaterial_SMB : StateMachineBehaviour
{
	public GameObject particles;

	public float lifetime = 5f;

	public AnimationCurve myCurve;

	public string animProperty;

	public float animStrength = 1f;

	private Renderer[] rend;

	private float t;

	public float animTime = 1f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		Object.Destroy(Object.Instantiate(particles, animator.transform), lifetime);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		t += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
		float value = myCurve.Evaluate(t) * animStrength;
		rend = animator.GetComponentsInChildren<Renderer>();
		Renderer[] array = rend;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetMaterialClonesSlow()[0].SetFloat(animProperty, value);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		t = 0f;
	}
}
