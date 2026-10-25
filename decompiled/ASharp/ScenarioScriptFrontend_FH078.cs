using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH078 : ScenarioScriptFrontend
{
	public class ActorCounter
	{
		public GameObject actorGameObject;

		public GameObject counterGameObject;
	}

	private Dictionary<ScenarioScript_FH078.MonsterOrderInfo, ActorCounter> actorCounters = new Dictionary<ScenarioScript_FH078.MonsterOrderInfo, ActorCounter>();

	private List<ScenarioScript_FH078.MonsterOrderInfo> infoToKeep = new List<ScenarioScript_FH078.MonsterOrderInfo>();

	private ScenarioScript_FH078 _script078 => (ScenarioScript_FH078)_script;

	protected override void InitializeInternal()
	{
	}

	protected override void TickInternal()
	{
		lock (_script078.MonsterKillOrder)
		{
			List<ScenarioScript_FH078.MonsterOrderInfo> list = _script078?.MonsterKillOrder?.ToList();
			if (list == null)
			{
				return;
			}
			infoToKeep.Clear();
			foreach (ScenarioScript_FH078.MonsterOrderInfo item in list)
			{
				if (!item.Valid)
				{
					actorCounters.Remove(item);
					continue;
				}
				ActorCounter value = null;
				if (!actorCounters.TryGetValue(item, out value))
				{
					GameObject gameObject = Choreographer.s_Choreographer.FindClientActor(item.Actor);
					GameObject gameObject2 = null;
					if (EnvironmentLoader.IsInstanceValid && EnvironmentLoader.Instance.IsLoadingComplete)
					{
						gameObject2 = EnvironmentLoader.Instance.GetProp(item.CounterGUID);
					}
					if (gameObject2 == null)
					{
						gameObject2 = Choreographer.FindPropGameObject(item.Prop);
					}
					if (gameObject != null && gameObject2 != null)
					{
						value = new ActorCounter
						{
							actorGameObject = gameObject,
							counterGameObject = gameObject2
						};
						actorCounters[item] = value;
						value.counterGameObject.transform.position = value.actorGameObject.transform.position;
						infoToKeep.Add(item);
					}
				}
			}
			actorCounters.RemoveWhere((KeyValuePair<ScenarioScript_FH078.MonsterOrderInfo, ActorCounter> x) => !infoToKeep.Contains(x.Key));
		}
	}
}
