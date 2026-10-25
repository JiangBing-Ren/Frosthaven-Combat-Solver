using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public static class SEventLogMessageHandler
{
	private static ConcurrentQueue<SEvent> s_EventLogQueue = new ConcurrentQueue<SEvent>();

	private static volatile bool s_BlockEventLogMessageProcessing;

	public static void Initialise()
	{
		s_EventLogQueue.Clear();
	}

	public static void Reset()
	{
		s_BlockEventLogMessageProcessing = false;
	}

	public static void AddEventLogMessage(SEvent sEvent, bool processImmediately = false)
	{
		if (!GameState.IsInLevelEditor && !ScenarioManager.IsLoadingNotFirsLoad)
		{
			if (processImmediately)
			{
				ProcessEventLogMessage(sEvent);
			}
			else
			{
				s_EventLogQueue.Enqueue(sEvent);
			}
		}
	}

	public static List<T> GetQueuedEvents<T>() where T : SEvent
	{
		List<T> result = new List<T>();
		lock (s_EventLogQueue)
		{
			if (s_EventLogQueue.Count > 0)
			{
				result = s_EventLogQueue.OfType<T>().ToList();
			}
		}
		return result;
	}

	public static List<SEventAttackModifier> GetQueuedModifiers()
	{
		return GetQueuedEvents<SEventAttackModifier>();
	}

	public static List<SEventAbilityCondition> GetQueuedConditions()
	{
		return GetQueuedEvents<SEventAbilityCondition>();
	}

	public static List<SEventAbility> GetQueuedAbilities()
	{
		return GetQueuedEvents<SEventAbility>();
	}

	public static void ProcessQueue(uint count = uint.MaxValue)
	{
		if (!s_BlockEventLogMessageProcessing)
		{
			SEvent result;
			while (count != 0 && s_EventLogQueue.TryDequeue(out result))
			{
				ProcessEventLogMessage(result);
				count--;
			}
		}
	}

	private static void ProcessEventLogMessage(SEvent sEvent)
	{
		if (ScenarioManager.CurrentScenarioState.ScenarioEventLog != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.AddEvent(sEvent);
		}
	}

	public static void ToggleEventLogProcessing(bool process)
	{
		if (process)
		{
			if (s_BlockEventLogMessageProcessing)
			{
				s_BlockEventLogMessageProcessing = false;
			}
		}
		else if (!s_BlockEventLogMessageProcessing)
		{
			s_BlockEventLogMessageProcessing = true;
		}
	}
}
