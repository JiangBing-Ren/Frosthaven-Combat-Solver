using UnityEngine;

public class RootMotion : MonoBehaviour
{
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void OnAnimatorMove()
	{
	}
}
