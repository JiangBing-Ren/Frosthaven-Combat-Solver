using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FHGUI.Scenario.Modules;
using UnityEngine;

namespace FHGUI.Scenario;

[Serializable]
public class FHScenarioViewModulesCatalogue
{
	[Header("Common")]
	public ScenarioPartyColumnModule PartyModule;

	[Header("Action")]
	public UISimpleActionBox SimpleActionBox;

	public ActionBoxViewModule ActionsBox;

	public ScenarioAttackSummaryModule attackSummary;

	[Header("Reaction")]
	public UIReactionActiveBonusTrackerCards ReactionActiveBonusTracker;

	public UIReactionFrozenFistCardRecoverFromSelection ReactionFrozenFistRecoverFromSelection;

	public UIReactionSelectAndPlayCards ReactionSelectAndPlayCards;

	[Header("Debug")]
	public FHScenarioViewModule StateInfo;

	[NonSerialized]
	private List<IUIViewModule> _allModules;

	public IReadOnlyList<IUIViewModule> AllModules
	{
		get
		{
			if (_allModules != null && _allModules.Count != 0)
			{
				return _allModules;
			}
			return _allModules = GatherModules();
		}
	}

	public void Init()
	{
		_allModules = GatherModules();
		foreach (IUIViewModule allModule in _allModules)
		{
			allModule.GameObject.SetActive(value: false);
		}
	}

	private List<IUIViewModule> GatherModules()
	{
		return (from f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
			where typeof(IUIViewModule).IsAssignableFrom(f.FieldType)
			select (IUIViewModule)f.GetValue(this)).ToList();
	}
}
