using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.GameConsole;
using Libs.Penumbra;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJobFactory_ScenarioTestingSubsetBase : PenumbraJobFactory
{
	public virtual string JobFilterRef => "Base";

	protected virtual string[] _scenarioNames => new string[1] { "FS_001" };

	public override Filtering Filtering => new Filtering(Filtering.Type.FilterContains, JobFilterRef);

	public override string ActionBindingFilter => "ScenarioProcessing";

	[ConsoleCommand(Command = "RunPenumbra_Basic", Description = "Run sub set of scenes")]
	public static void StartFromConsoleBasic(IConsole console)
	{
		PenumbraAgent.Instance.StartPenumbra(0u, "CustomSet," + new PenumbraJobFactory_ScenarioTestingSubsetBase().JobFilterRef);
	}

	public override IEnumerator GetJobs(ILogger logger, bool sortByPriority, Action<IEnumerable<PenumbraJob>> outResults)
	{
		List<CCustomLevelData> scs = new List<CCustomLevelData>();
		yield return PenumbraJobFactory_ScenarioTestingAll.LoadKronosData(logger, scs);
		outResults(from data in scs
			where MatchScenarioNames(data)
			select new PenumbraJob_RunScenario
			{
				ActionBindingFilter = ActionBindingFilter,
				JobContextName = data.Name,
				target = data
			});
	}

	private bool MatchScenarioNames(CCustomLevelData data)
	{
		if (data.Name.StartsWith("FH") || data.Name.StartsWith("FS"))
		{
			return _scenarioNames.Any((string a) => data.Name.Contains(a));
		}
		return false;
	}
}
