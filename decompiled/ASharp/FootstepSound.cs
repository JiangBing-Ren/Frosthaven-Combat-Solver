using System.Collections;
using ClockStone;
using ScenarioRuleLibrary;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
	[AudioEventName]
	[SerializeField]
	private string m_FootstepAudioEvent;

	[AudioEventName]
	[SerializeField]
	private string m_ClothAudioEvent;

	[SerializeField]
	private GameObject m_PFXPrefab;

	[SerializeField]
	private GameObject m_PFXPrefabEnhanced;

	[AudioEventName]
	[SerializeField]
	private string m_ClothAudioEnhanced;

	[SerializeField]
	[Tooltip("0 - Left Foot, 1 - Right Foot, 2+ everything else for weird creatures")]
	private Transform[] m_FeetArray;

	private AudioObject m_ClothAudio;

	private bool m_Stopped;

	public void StopClothSound()
	{
		if ((bool)m_ClothAudio)
		{
			m_ClothAudio.FadeOut(0.2f);
		}
		m_Stopped = true;
		StartCoroutine(Unmute());
	}

	private IEnumerator Unmute()
	{
		yield return new WaitForSeconds(1f);
		m_Stopped = false;
	}

	public void MakeFootstepSound(AnimationEvent animEvent)
	{
		if (m_Stopped || animEvent.animatorClipInfo.weight < 0.15f)
		{
			return;
		}
		if (!string.IsNullOrEmpty(m_FootstepAudioEvent))
		{
			AudioController.Play(m_FootstepAudioEvent, base.gameObject);
		}
		if (m_PFXPrefab == null && m_PFXPrefabEnhanced == null)
		{
			return;
		}
		if (m_FeetArray.Length < animEvent.intParameter)
		{
			Log.AudioError("Entry Missing for character " + base.gameObject.name + " foot index more then size of FeetArray");
			return;
		}
		if (animEvent.intParameter <= 0)
		{
			Log.AudioError("Foot index in AnimEvent is below 1");
			return;
		}
		CActor actor = ActorBehaviour.GetActor(base.gameObject);
		if (m_PFXPrefabEnhanced != null && actor != null && actor.HasEnhancedSteps())
		{
			GameObject gameObject = ObjectPool.Spawn(m_PFXPrefabEnhanced, m_FeetArray[animEvent.intParameter - 1]);
			if (gameObject != null)
			{
				ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(m_PFXPrefabEnhanced), m_PFXPrefabEnhanced);
			}
		}
		else
		{
			GameObject gameObject2 = ObjectPool.Spawn(m_PFXPrefab, m_FeetArray[animEvent.intParameter - 1]);
			if (gameObject2 != null)
			{
				ObjectPool.Recycle(gameObject2, VFXShared.GetEffectLifetime(m_PFXPrefab), m_PFXPrefab);
			}
		}
	}

	public void MakeClothSound(AnimationEvent animEvent)
	{
		if (!m_Stopped)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
			if (actorBehaviour == null)
			{
				actorBehaviour = ActorBehaviour.GetMultibodyActorBehaviour(gameObject);
			}
			CActor cActor = ((actorBehaviour == null) ? null : actorBehaviour.Actor);
			if (!string.IsNullOrEmpty(m_ClothAudioEnhanced) && cActor != null && cActor.HasEnhancedSteps())
			{
				m_ClothAudio = AudioController.Play(m_ClothAudioEnhanced, gameObject);
			}
			else if (!string.IsNullOrEmpty(m_ClothAudioEvent))
			{
				m_ClothAudio = AudioController.Play(m_ClothAudioEvent, gameObject);
			}
		}
	}
}
