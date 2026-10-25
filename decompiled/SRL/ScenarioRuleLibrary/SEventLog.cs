using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLog : ISerializable
{
	private List<SEvent> TempCachedUnprocessedEvents;

	private List<SEvent> TempCachedEventsAboutToBeScraped;

	public List<SEvent> Events { get; private set; }

	public int ProcessedCount { get; private set; }

	public int SerializedEventCount { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Events", (Events != null) ? Events.Where((SEvent w) => w != null && !w.m_DoNotSerialize).ToList() : null);
		info.AddValue("ProcessedCount", ProcessedCount);
		info.AddValue("SerializedEventCount", SerializedEventCount);
	}

	public SEventLog(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Events":
					Events = (List<SEvent>)info.GetValue("Events", typeof(List<SEvent>));
					break;
				case "ProcessedCount":
					ProcessedCount = info.GetInt32("ProcessedCount");
					break;
				case "SerializedEventCount":
					SerializedEventCount = info.GetInt32("SerializedEventCount");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventLog entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Events == null)
		{
			Events = new List<SEvent>
			{
				new SEventInternal(ESESubTypeInternal.Start)
			};
		}
		else if (SerializedEventCount == 0)
		{
			SerializedEventCount = Events.Count;
		}
	}

	public SEventLog()
	{
		Events = new List<SEvent>
		{
			new SEventInternal(ESESubTypeInternal.Start)
		};
	}

	public void AddEvent(SEvent eventToAdd)
	{
		lock (Events)
		{
			Events.Add(eventToAdd);
			SerializedEventCount++;
		}
	}

	public void WriteToFile(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			using StreamWriter streamWriter = new StreamWriter(path);
			foreach (SEvent @event in Events)
			{
				if (!(@event is SEventInternal))
				{
					if (@event is SEventPhase)
					{
					}
				}
				else
				{
					streamWriter.WriteLine();
				}
			}
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("An exception ocurred while attempting to write the scenario event log to file " + path + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static IEnumerable<SEvent> GetUnprocessedEvents()
	{
		SEventLog instance = ScenarioManager.CurrentScenarioState.ScenarioEventLog;
		lock (instance.Events)
		{
			for (int i = instance.ProcessedCount; i < instance.Events.Count; i++)
			{
				yield return instance.Events[i];
			}
		}
	}

	public static void StartScrapingEvents(List<SEvent> events)
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.Events != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedEventsAboutToBeScraped = events;
		}
	}

	public static void FinishScrapingEvents()
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.Events != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedEventsAboutToBeScraped = null;
		}
	}

	public static List<SEvent> CacheUnprocessedEvents()
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.Events == null)
		{
			return null;
		}
		if (ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents != null)
		{
			return ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents;
		}
		List<SEvent> list = GetUnprocessedEvents().ToList();
		if (list.Count == 0)
		{
			return null;
		}
		ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents = list;
		IncProcessedEvents(list.Count);
		return ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents;
	}

	public static void ClearUnprocessedEventsCache()
	{
		ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents = null;
	}

	public static void IncProcessedEvents(int amount = 1)
	{
		ScenarioManager.CurrentScenarioState.ScenarioEventLog.ProcessedCount += amount;
	}

	public static IEnumerable<SEvent> GetEvents(bool onlyUnprocessed, bool toScrape)
	{
		if (toScrape)
		{
			return ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedEventsAboutToBeScraped;
		}
		if (onlyUnprocessed)
		{
			if (ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents != null)
			{
				return ScenarioManager.CurrentScenarioState.ScenarioEventLog.TempCachedUnprocessedEvents;
			}
			return GetUnprocessedEvents();
		}
		return ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events;
	}

	public static List<SEvent> FindLoggedEvents(bool onlyUnprocessed, ESEType eventType, bool toScrape)
	{
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEvent> result;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			result = (from x in GetEvents(onlyUnprocessed, toScrape).ToList()
				where x.Type.Equals(eventType)
				select x).ToList();
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEvent FindLastEventOfType(bool onlyUnprocessed, ESEType eventType, bool toScrape)
	{
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		SEvent result;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			result = GetEvents(onlyUnprocessed, toScrape).ToList().LastOrDefault((SEvent x) => x.Type.Equals(eventType));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAbility FindLastAbilityEventOfAbilityType(bool onlyUnprocessed, CAbility.EAbilityType abilityType, bool toScrape, ESESubTypeAbility subType = ESESubTypeAbility.None, bool checkQueue = false)
	{
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAbility> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAbility>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAbility> queuedAbilities = SEventLogMessageHandler.GetQueuedAbilities();
			list.AddRange(queuedAbilities);
		}
		return FindLastAbilityEventOfAbilityType(list, abilityType, subType);
	}

	public static SEventAbility FindLastAbilityEventOfAbilityType<T>(List<T> abilityEvents, CAbility.EAbilityType abilityType, ESESubTypeAbility subType = ESESubTypeAbility.None) where T : SEventAbility
	{
		SEventAbility result = null;
		if (abilityEvents.Count > 0)
		{
			result = ((subType == ESESubTypeAbility.None) ? abilityEvents.LastOrDefault((T x) => x.AbilityType.Equals(abilityType)) : abilityEvents.LastOrDefault((T x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType)));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAbilityCondition FindLastConditionAbilityEventOfAbilityType(bool onlyUnprocessed, CAbility.EAbilityType abilityType, bool toScrape, ESESubTypeAbility subType = ESESubTypeAbility.None, bool checkQueue = false)
	{
		SEventAbilityCondition result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAbilityCondition> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAbilityCondition>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAbilityCondition> queuedConditions = SEventLogMessageHandler.GetQueuedConditions();
			list.AddRange(queuedConditions);
		}
		if (list.Count > 0)
		{
			result = ((subType == ESESubTypeAbility.None) ? list.LastOrDefault((SEventAbilityCondition x) => x.AbilityType.Equals(abilityType)) : list.LastOrDefault((SEventAbilityCondition x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType)));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAction FindLastActionEventOfSubType(bool onlyUnprocessed, ESESubTypeAction subTypeAction, bool toScrape)
	{
		SEventAction result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAction> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAction>().ToList();
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventAction x) => x.SubTypeAction.Equals(subTypeAction));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventObjectProp FindLastObjectEventOfSubTypeAndObjectType(bool onlyUnprocessed, ESESubTypeObjectProp subTypeObjectProp, ScenarioManager.ObjectImportType objectType, bool toScrape, bool checkQueue = false)
	{
		SEventObjectProp result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventObjectProp> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventObjectProp>().ToList();
		}
		if (checkQueue)
		{
			list.AddRange(SEventLogMessageHandler.GetQueuedEvents<SEventObjectProp>());
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventObjectProp x) => x.ObjectPropSubType.Equals(subTypeObjectProp) && x.ObjectType.Equals(objectType));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAttackModifier FindLastAttackModifierEventWithID(bool onlyUnprocessed, int actorId, string actorClass, bool toScrape, bool checkQueue = false, int attackIndex = 0)
	{
		SEventAttackModifier result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAttackModifier> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAttackModifier>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAttackModifier> queuedModifiers = SEventLogMessageHandler.GetQueuedModifiers();
			list.AddRange(queuedModifiers);
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventAttackModifier x) => x.TargetActorID.Equals(actorId) && x.SecondaryActorClass.Equals(actorClass) && x.AttackIndex.Equals(attackIndex));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static List<SEventActor> FindAllActorEventsOfSubTypeAndActorType(bool onlyUnprocessed, ESESubTypeActor subType, CActor.EType actorType, bool toScrape)
	{
		List<SEventActor> list = new List<SEventActor>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventActor>().ToList();
		}
		list = list.FindAll((SEventActor x) => x.ActorSubType.Equals(subType) && x.MainActorType.Equals(actorType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventActor> FindAllActorEventsOfSubType(bool onlyUnprocessed, ESESubTypeActor subType, bool toScrape, bool checkQueue = false)
	{
		List<SEventActor> list = new List<SEventActor>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventActor>().ToList();
		}
		if (checkQueue)
		{
			List<SEventActor> queuedEvents = SEventLogMessageHandler.GetQueuedEvents<SEventActor>();
			list.AddRange(queuedEvents);
		}
		list = list.FindAll((SEventActor x) => x.ActorSubType.Equals(subType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfType(bool onlyUnprocessed, CAbility.EAbilityType abilityType, bool toScrape)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAbility>().ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilityType.Equals(abilityType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfTypeAndSubType(bool onlyUnprocessed, CAbility.EAbilityType abilityType, ESESubTypeAbility subType, bool toScrape)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAbility>().ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfSubType(bool onlyUnprocessed, ESESubTypeAbility subType, bool toScrape, bool excludeTargeting = false)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<string> excluder = new List<string>
		{
			"Advantage", "Bless", "Curse", "Disarm", "Immobilize", "Invisible", "Muddle", "Poison", "Strengthen", "Stun",
			"Wound"
		};
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = (from x in GetEvents(onlyUnprocessed, toScrape).ToList().OfType<SEventAbility>()
				where x.GetType() != typeof(SEventAbilityCondition)
				select x).ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilitySubType.Equals(subType) && (!excludeTargeting || !excluder.Contains(x.AbilityType.ToString())));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<T> FindAllEventsOfType<T>(bool onlyUnprocessed, bool toScrape, bool checkQueue = false) where T : SEvent
	{
		List<T> list = new List<T>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = GetEvents(onlyUnprocessed, toScrape).ToList().OfType<T>().ToList();
		}
		if (checkQueue)
		{
			List<T> queuedEvents = SEventLogMessageHandler.GetQueuedEvents<T>();
			list.AddRange(queuedEvents);
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static T FindLastAbilityEventOfAbilityType<T>(bool onlyUnprocessed, CAbility.EAbilityType abilityType, ESESubTypeAbility subType = ESESubTypeAbility.None, bool checkQueue = false) where T : SEventAbility
	{
		return FindLastAbilityEventOfAbilityType(FindAllEventsOfType<T>(onlyUnprocessed, toScrape: false, checkQueue), abilityType, subType) as T;
	}

	public static void ClearEventLog()
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.Events != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Clear();
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.ProcessedCount = 0;
			SEventLogMessageHandler.Initialise();
		}
	}

	public static void ClearProcessedEventCount()
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.ProcessedCount = 0;
		}
	}
}
