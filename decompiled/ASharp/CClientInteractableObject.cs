using UnityEngine;

public class CClientInteractableObject : MonoBehaviour
{
	public GameObject nonInteractedState;

	public GameObject interactedState;

	public virtual void SetInteracted(bool interacting)
	{
		nonInteractedState.SetActive(!interacting);
		interactedState.SetActive(interacting);
	}
}
