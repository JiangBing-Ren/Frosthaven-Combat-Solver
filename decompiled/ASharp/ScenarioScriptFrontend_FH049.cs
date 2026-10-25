using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using GLOOM;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH049 : ScenarioScriptFrontend
{
	public class ActorCounter
	{
		public GameObject actorGameObject;

		public GameObject counterGameObject;
	}

	private Dictionary<ScenarioScript_FH049.OxygenInfo, ActorCounter> actorCounters = new Dictionary<ScenarioScript_FH049.OxygenInfo, ActorCounter>();

	private List<ScenarioScript_FH049.OxygenInfo> infoToKeep = new List<ScenarioScript_FH049.OxygenInfo>();

	private ScenarioScript_FH049 _script049 => (ScenarioScript_FH049)_script;

	protected override void InitializeInternal()
	{
	}

	protected override void TickInternal()
	{
		lock (_script049.OxygenBonusesCache)
		{
			List<ScenarioScript_FH049.OxygenInfo> list = _script049?.OxygenBonusesCache?.ToList();
			if (list == null)
			{
				return;
			}
			infoToKeep.Clear();
			foreach (ScenarioScript_FH049.OxygenInfo item in list)
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
			actorCounters.RemoveWhere((KeyValuePair<ScenarioScript_FH049.OxygenInfo, ActorCounter> x) => !infoToKeep.Contains(x.Key));
		}
	}

	protected override void ShowAbilityHelpBoxTooltipInternal(CAbility ability)
	{
		base.ShowAbilityHelpBoxTooltipInternal(ability);
		CPlayerActor player = ability.TargetingActor as CPlayerActor;
		if (player == null)
		{
			return;
		}
		ScenarioScript_FH049.OxygenInfo oxygenInfo = _script049.OxygenBonusesCache.FirstOrDefault((ScenarioScript_FH049.OxygenInfo x) => x.Actor == player);
		if (oxygenInfo != null)
		{
			int remaining = oxygenInfo.oxygenbonus.Remaining;
			if (remaining <= 3)
			{
				string translation = LocalizationManager.GetTranslation("FH_Scenario_Campaign_049_REMAINING_OXYGEN");
				List<string> list = new List<string>
				{
					LocalizationManager.GetTranslation(player.ActorLocKey()),
					remaining.ToString()
				};
				HelpBox instance = Singleton<HelpBox>.Instance;
				object[] args = list.ToArray();
				instance.ShowTranslated(string.Format(translation, args), ability, HelpBox.FormatTarget.NONE, "OXYGEN_WARNING");
			}
		}
	}
}
