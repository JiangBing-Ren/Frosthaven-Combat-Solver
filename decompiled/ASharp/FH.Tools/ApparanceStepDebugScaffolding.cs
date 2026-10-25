using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libs.GameConsole;

namespace FH.Tools;

public class ApparanceStepDebugScaffolding : IDisposable
{
	public delegate(bool preserve, List<ApparanceEntity> masterCollection) Aggregate(ApparanceEntity e);

	private static bool _allowDebug;

	private static bool _entitiesToggle;

	public static Action<Aggregate> OnToggle;

	public static Action<Aggregate, Action<string>> StepNext;

	public static Action<Aggregate, Action<string>> PrintDebug;

	private HashSet<ApparanceEntity> _entities = new HashSet<ApparanceEntity>();

	private static (Aggregate aggregate, List<ApparanceEntity> masterCollection) NewAggregate()
	{
		List<ApparanceEntity> masterCollection = new List<ApparanceEntity>();
		return (aggregate: (ApparanceEntity e) =>
		{
			if (masterCollection.Contains(e))
			{
				return (preserve: false, masterCollection: masterCollection);
			}
			masterCollection.Add(e);
			return (preserve: true, masterCollection: masterCollection);
		}, masterCollection: masterCollection);
	}

	[ConsoleCommand(Command = "Apparance_Debug_Allow", Description = "Allow registering entities. This is the first thing you do.")]
	public static void CommandAllowDebug(IConsole consode)
	{
		_allowDebug = !_allowDebug;
		OnToggle = null;
		StepNext = null;
		PrintDebug = null;
		consode.WriteLine("Apparance debug is now " + (_allowDebug ? "ENABLED" : "DISABLED"));
	}

	[ConsoleCommand(Command = "ApparanceDebugToggle", Description = "This enabled the Apparance Debug Scaffolding. It will disable apparance entities by default and allow you to step through each. This is the second thing you do.")]
	public static void CommandToggleDebug(IConsole consode)
	{
		_entitiesToggle = !_entitiesToggle;
		OnToggle?.Invoke(NewAggregate().aggregate);
		consode.WriteLine("Apparance entities are now " + (_entitiesToggle ? "ACTIVE" : "INACTIVE"));
	}

	[ConsoleCommand(Command = "ApparanceDebugPrint", Description = "Apparance Debug Scaffolding. What are all the entities in the current scene.")]
	public static void CommandPrintDebug(IConsole console)
	{
		StringBuilder msg = new StringBuilder();
		msg.AppendLine("Apparance Entities:");
		PrintDebug?.Invoke(NewAggregate().aggregate, (string s) =>
		{
			msg.AppendLine(s);
		});
		console.WriteLine(msg);
	}

	[ConsoleCommand(Command = "ApparanceDebugStepNext", Description = "Apparance Debug Scaffolding. What are all the entities in the current scene.")]
	public static void CommandStepNext(IConsole console)
	{
		StringBuilder msg = new StringBuilder();
		console.WriteLine("Apparance Entities:");
		(Aggregate, List<ApparanceEntity>) tuple = NewAggregate();
		StepNext?.Invoke(tuple.Item1, (string s) =>
		{
			msg.Append(s);
		});
		List<ApparanceEntity> item = tuple.Item2;
		ApparanceEntity apparanceEntity = item.FirstOrDefault((ApparanceEntity a) => a != null && !a.IsPopulated);
		if ((bool)apparanceEntity)
		{
			apparanceEntity.IsPopulated = true;
			console.WriteLine($"{apparanceEntity} enabled. {item.Count((ApparanceEntity a) => a == null || a.IsPopulated)}/{item.Count()}");
		}
		console.WriteLine(msg);
	}

	public static void ToggleDebug(bool state)
	{
		_entitiesToggle = state;
	}

	public ApparanceStepDebugScaffolding()
	{
		OnToggle = (Action<Aggregate>)Delegate.Combine(OnToggle, new Action<Aggregate>(OnToggledDebug));
		StepNext = (Action<Aggregate, Action<string>>)Delegate.Combine(StepNext, new Action<Aggregate, Action<string>>(AggregateStep));
		PrintDebug = (Action<Aggregate, Action<string>>)Delegate.Combine(PrintDebug, new Action<Aggregate, Action<string>>(OnPrintDebug));
	}

	public void Dispose()
	{
		OnToggle = (Action<Aggregate>)Delegate.Remove(OnToggle, new Action<Aggregate>(OnToggledDebug));
		StepNext = (Action<Aggregate, Action<string>>)Delegate.Remove(StepNext, new Action<Aggregate, Action<string>>(AggregateStep));
		PrintDebug = (Action<Aggregate, Action<string>>)Delegate.Remove(PrintDebug, new Action<Aggregate, Action<string>>(OnPrintDebug));
	}

	public void RegisterEntity(ApparanceEntity entity)
	{
		if (_allowDebug)
		{
			_entities.Add(entity);
			entity.IsPopulated = _entitiesToggle;
		}
	}

	private void OnToggledDebug(Aggregate aggregation)
	{
		if (!_allowDebug)
		{
			return;
		}
		_entities.RemoveWhere((ApparanceEntity a) => a == null || !aggregation(a).preserve);
		foreach (ApparanceEntity entity in _entities)
		{
			if ((bool)entity)
			{
				entity.IsPopulated = !_entitiesToggle;
			}
		}
		_entities.RemoveWhere((ApparanceEntity e) => e == null);
	}

	private void AggregateStep(Aggregate aggregation, Action<string> outFeedback)
	{
		if (!_allowDebug)
		{
			outFeedback?.Invoke("ERROR: Should enable first!\n");
			return;
		}
		_entities.RemoveWhere((ApparanceEntity a) => a == null || !aggregation(a).preserve);
	}

	private void OnPrintDebug(Aggregate aggregation, Action<string> outFeedback)
	{
		if (!_allowDebug)
		{
			outFeedback?.Invoke("ERROR: Should enable first!\n");
			return;
		}
		_entities.RemoveWhere((ApparanceEntity a) => a == null || !aggregation(a).preserve);
		foreach (ApparanceEntity entity in _entities)
		{
			outFeedback?.Invoke(string.Format("{0} in {1} : {2}", entity, entity.transform.parent, entity.IsPopulated ? "Populated" : "Empty"));
		}
	}
}
