using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario;

public abstract class FHScenarioViewLayer<T> where T : FHScenarioViewState
{
	public T State { get; private set; }

	public void Set(T newState, FHScenarioViewController.StateOperationType operationType)
	{
		if (State != newState)
		{
			State?.OnHide();
			State = newState;
		}
		Do(operationType);
	}

	public abstract bool ProcessMessage(CMessageData messageData);

	public void Do(FHScenarioViewController.StateOperationType operationType)
	{
		if (State != null)
		{
			switch (operationType)
			{
			case FHScenarioViewController.StateOperationType.Show:
				State.OnShow();
				State.Update();
				break;
			case FHScenarioViewController.StateOperationType.Update:
				State.Update();
				break;
			case FHScenarioViewController.StateOperationType.Hide:
				State.OnHide();
				break;
			case FHScenarioViewController.StateOperationType.Clear:
				State.OnHide();
				State = null;
				break;
			default:
				throw new ArgumentOutOfRangeException("operationType", operationType, null);
			case FHScenarioViewController.StateOperationType.None:
				break;
			}
		}
	}

	public IEnumerable<IUIViewModule> GetStateModules()
	{
		if (State == null)
		{
			yield break;
		}
		foreach (IUIViewModule item in State.UseModules())
		{
			yield return item;
		}
	}
}
