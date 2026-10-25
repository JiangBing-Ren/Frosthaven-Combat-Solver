using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Checkmark : MonoBehaviour
{
	public enum CheckmarkState
	{
		NONE,
		COMPLETED,
		FAILED
	}

	private CheckmarkState state;

	public GameObject completed;

	public GameObject failed;

	private void Awake()
	{
		SetState(state);
	}

	public void SetState(CheckmarkState state)
	{
		this.state = state;
		completed.SetActive(state == CheckmarkState.COMPLETED);
		failed.SetActive(state == CheckmarkState.FAILED);
	}

	public void MarkCompleted()
	{
		SetState(CheckmarkState.COMPLETED);
	}

	public void MarkFailed()
	{
		SetState(CheckmarkState.FAILED);
	}

	public void Clear()
	{
		SetState(CheckmarkState.NONE);
	}
}
