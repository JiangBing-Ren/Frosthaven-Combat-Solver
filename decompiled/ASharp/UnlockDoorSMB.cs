using ScenarioRuleLibrary;
using UnityEngine;

public class UnlockDoorSMB : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		DeathDissolve componentInChildren = animator.gameObject.GetComponentInChildren<DeathDissolve>();
		if (componentInChildren != null)
		{
			componentInChildren.ExternalPlay();
		}
		UnityGameEditorObject componentInParent = animator.gameObject.GetComponentInParent<UnityGameEditorObject>();
		if (!(componentInParent != null))
		{
			return;
		}
		for (int i = 0; i < ScenarioManager.CurrentScenarioState.DoorProps.Count; i++)
		{
			if (ScenarioManager.CurrentScenarioState.DoorProps[i].InstanceName == componentInParent.gameObject.name)
			{
				if (Choreographer.s_Choreographer.DoorsUnlocking != null && Choreographer.s_Choreographer.DoorsUnlocking.Contains(ScenarioManager.CurrentScenarioState.DoorProps[i]))
				{
					Choreographer.s_Choreographer.OpenDoor(ScenarioManager.CurrentScenarioState.DoorProps[i], initialLoad: false, openDoorOnly: false);
					Choreographer.s_Choreographer.DoorsUnlocking.Remove(ScenarioManager.CurrentScenarioState.DoorProps[i]);
				}
				break;
			}
		}
	}
}
