using DevLocker.GFrame.Input.UIScope;
using ScenarioRuleLibrary;
using UnityEngine;

public class ControllerInputPointerActivatorScopeElement : MonoBehaviour, IScopeElement
{
	private void Update()
	{
		switch (PhaseManager.PhaseType)
		{
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
		case CPhase.PhaseType.ActionSelection:
		case CPhase.PhaseType.CheckForInitiativeAdjustments:
		case CPhase.PhaseType.ShortRest:
			Singleton<ControllerInputPointer>.Instance.Show();
			break;
		case CPhase.PhaseType.Action:
			if ((Choreographer.s_Choreographer.m_WaitState.m_State > Choreographer.ChoreographerStateType.Play || WorldspaceStarHexDisplay.Instance.CurrentDisplayState != WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone) & !Singleton<TakeDamagePanel>.Instance.IsOpen)
			{
				Singleton<ControllerInputPointer>.Instance.Show();
			}
			else
			{
				Singleton<ControllerInputPointer>.Instance.Hide();
			}
			break;
		default:
			Singleton<ControllerInputPointer>.Instance.Hide();
			break;
		}
	}

	private void OnDisable()
	{
		if ((bool)Singleton<ControllerInputPointer>.InstanceFast)
		{
			Singleton<ControllerInputPointer>.InstanceFast.Hide();
		}
	}
}
