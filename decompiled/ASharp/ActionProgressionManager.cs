using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;

public class ActionProgressionManager
{
	private class ActionProgression : IComparable<ActionProgression>
	{
		private readonly Func<ICallbackPromise> action;

		private readonly int priority;

		public string ID { get; }

		public ActionProgression(string id, Func<ICallbackPromise> action, int priority = 0)
		{
			ID = id;
			this.action = action;
			this.priority = priority;
		}

		public ICallbackPromise Process()
		{
			return action();
		}

		public int CompareTo(ActionProgression other)
		{
			if (this == other)
			{
				return 0;
			}
			if (other == null)
			{
				return 1;
			}
			int num = priority;
			return num.CompareTo(other.priority);
		}
	}

	private List<ActionProgression> actions;

	private ActionProgression currentAction;

	private HashSet<object> pauseRequests;

	private string filterRequests;

	public ActionProgressionManager()
	{
		pauseRequests = new HashSet<object>();
		actions = new List<ActionProgression>();
	}

	public void AddAction(string id, Func<ICallbackPromise> action, int priority = 0)
	{
		actions.Add(new ActionProgression(id, action, priority));
		CheckProgressNextAction();
	}

	private void CheckProgressNextAction()
	{
		if (currentAction != null || pauseRequests.Count > 0 || actions.Count == 0)
		{
			return;
		}
		actions.Sort();
		ActionProgression actionProgression = (filterRequests.IsNullOrEmpty() ? actions[0] : actions.FirstOrDefault((ActionProgression it) => it.ID == filterRequests));
		if (actionProgression != null)
		{
			currentAction = actionProgression;
			Log.GUIInfo("Start Action " + currentAction.ID);
			currentAction.Process().Done(() =>
			{
				Log.GUIInfo("Finish Action " + currentAction.ID);
				actions.Remove(currentAction);
				currentAction = null;
				CheckProgressNextAction();
			});
		}
	}

	public void ClearActions()
	{
		actions.Clear();
		currentAction = null;
	}

	public void RequestPauseActions(object request)
	{
		Log.GUIInfo($"Request Pause Actions by {request}");
		pauseRequests.Add(request);
	}

	public void RequestResumeActions(object request, string filterId = null)
	{
		filterRequests = filterId;
		pauseRequests.Remove(request);
		Log.GUIInfo($"Request Resume Actions by {request} (paused {pauseRequests.Count > 0})");
		CheckProgressNextAction();
	}
}
