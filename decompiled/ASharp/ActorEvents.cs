using System.Collections.Generic;
using UnityEngine;

public class ActorEvents : MonoBehaviour
{
	public enum ActorEvent
	{
		ProgressChoreographer,
		FinishedDrawingModifiers,
		StartDrawingModifiers
	}

	private Animator m_AnimatorCached;

	private StateMachineBehaviour m_LastEventRecievedStateMachineBehaviour;

	private List<ActorEvent> m_ActorEvents = new List<ActorEvent>();

	private void OnEnable()
	{
		if (m_AnimatorCached == null)
		{
			m_AnimatorCached = GetComponent<Animator>();
		}
		if (m_AnimatorCached == null || m_AnimatorCached.runtimeAnimatorController == null)
		{
			return;
		}
		AnimationClip[] animationClips = m_AnimatorCached.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (!animationClip.name.ToLowerInvariant().Contains("_idle_") && !animationClip.name.ToLowerInvariant().EndsWith("_idle") && !animationClip.name.ToLowerInvariant().Contains("_run_") && !animationClip.name.ToLowerInvariant().EndsWith("_run") && !animationClip.name.ToLowerInvariant().Contains("_move_") && !animationClip.name.ToLowerInvariant().EndsWith("_move"))
			{
				List<AnimationEvent> list = new List<AnimationEvent>();
				AnimationEvent[] events = animationClip.events;
				foreach (AnimationEvent item in events)
				{
					list.Add(item);
				}
				animationClip.events = list.ToArray();
			}
		}
	}

	public void ProgressChoreographer(StateMachineBehaviour lastEventRecievedStateMachineBehaviour = null)
	{
		m_ActorEvents.Add(ActorEvent.ProgressChoreographer);
		if (lastEventRecievedStateMachineBehaviour != null)
		{
			m_LastEventRecievedStateMachineBehaviour = lastEventRecievedStateMachineBehaviour;
		}
	}

	public void RegisterEvent(ActorEvent ev)
	{
		if (!m_ActorEvents.Contains(ev))
		{
			m_ActorEvents.Add(ev);
		}
	}

	public bool ReceivedEventThenClear(ActorEvent actorEvent)
	{
		bool flag = ((!(Choreographer.s_Choreographer.m_SMB_Control_ControlledByStateBehaviour != null)) ? m_ActorEvents.Contains(actorEvent) : (m_ActorEvents.Contains(actorEvent) && m_LastEventRecievedStateMachineBehaviour == Choreographer.s_Choreographer.m_SMB_Control_ControlledByStateBehaviour));
		if (flag)
		{
			m_ActorEvents.Remove(actorEvent);
			m_LastEventRecievedStateMachineBehaviour = null;
		}
		return flag;
	}

	public bool ReceivedEvent(ActorEvent actorEvent)
	{
		return m_ActorEvents.Contains(actorEvent);
	}

	public static ActorEvents GetActorEvents(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return null;
		}
		ActorEvents actorEvents = gameObject.GetComponent<ActorEvents>();
		if (actorEvents == null)
		{
			actorEvents = gameObject.GetComponentInChildren<ActorEvents>();
		}
		return actorEvents;
	}

	public void ClearActorEventState()
	{
		m_ActorEvents.Remove(ActorEvent.ProgressChoreographer);
		m_LastEventRecievedStateMachineBehaviour = null;
	}
}
