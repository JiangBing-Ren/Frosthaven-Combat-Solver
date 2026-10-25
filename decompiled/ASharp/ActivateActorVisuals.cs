using ScenarioRuleLibrary;
using UnityEngine;

public class ActivateActorVisuals : MonoBehaviour
{
	public bool m_active;

	public static void SetActive(CActor actor, bool active)
	{
		SetActive(Choreographer.s_Choreographer.FindClientActorGameObject(actor), active);
	}

	public static void SetActive(GameObject actorGameObject, bool active)
	{
		if (actorGameObject == null)
		{
			return;
		}
		ActivateActorVisuals activateActorVisuals = actorGameObject.GetComponent<ActivateActorVisuals>();
		if (!active || (!(activateActorVisuals == null) && !activateActorVisuals.m_active))
		{
			if (activateActorVisuals == null)
			{
				activateActorVisuals = actorGameObject.AddComponent<ActivateActorVisuals>();
			}
			activateActorVisuals.SetActive(active);
		}
	}

	private void SetActive(bool active)
	{
		m_active = active;
		CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
		Renderer[] renderers = characterManager.Renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = active;
		}
		Collider[] colliders = characterManager.Colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = active;
		}
	}
}
