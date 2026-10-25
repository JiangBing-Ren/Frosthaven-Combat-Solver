using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class ChangeModelSMB : StateMachineBehaviour
{
	[Tooltip("If enabled ignore Delay property and spawn always at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[Tooltip("Delay amount of seconds ater animation start playing")]
	[SerializeField]
	private float m_Delay;

	[Tooltip("Used to play the same animation state on the new character model once it's instantiated")]
	[SerializeField]
	private string m_AnimationStateName;

	private float m_InstanceDelay;

	private bool m_Changed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		m_Changed = false;
		float length = stateInfo.length;
		m_InstanceDelay = (m_OnAnimationEnd ? (length - 0.2f) : Mathf.Min(m_Delay, length));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		m_InstanceDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		if (m_InstanceDelay <= 0f && !m_Changed)
		{
			ChangeModel(animator);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_Changed)
		{
			ChangeModel(animator);
		}
	}

	private void ChangeModel(Animator animator)
	{
		m_Changed = true;
		GameObject gameObject = animator.gameObject;
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
		CActor animatingActorToWaitFor = actorBehaviour.Actor;
		CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
		GameObject gameObject2 = characterManager.gameObject;
		Quaternion rotation = gameObject2.transform.rotation;
		gameObject2.transform.SetParent(null);
		Choreographer.s_Choreographer.m_ClientPlayers.Remove(gameObject2);
		actorBehaviour.m_WorldspacePanelUI?.Destroy();
		Choreographer.CharacterOutlineRegistration(characterManager, register: false);
		characterManager.DeinitializeCharacter();
		ObjectPool.Recycle(gameObject2, characterManager.CharacterPrefab);
		CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[animatingActorToWaitFor.ArrayIndex.X, animatingActorToWaitFor.ArrayIndex.Y];
		GameObject gameObject3 = Choreographer.s_Choreographer.CreateCharacterActor(tile, animatingActorToWaitFor, isSummoned: true);
		Choreographer.s_Choreographer.m_ClientPlayers.Add(gameObject3);
		ActorBehaviour.GetActorBehaviour(gameObject3).UpdateWorldspaceConditionsUI();
		gameObject3.transform.SetPositionAndRotation(gameObject3.transform.position, rotation);
		bool animationShouldPlay = false;
		Choreographer.s_Choreographer.ProcessActorAnimation(null, animatingActorToWaitFor, new List<string> { m_AnimationStateName }, out animationShouldPlay, out animatingActorToWaitFor);
		InitiativeTrack.Refresh(forceSelectFirstActor: false);
	}
}
