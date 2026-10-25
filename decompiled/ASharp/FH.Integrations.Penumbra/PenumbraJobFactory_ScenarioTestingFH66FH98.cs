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
public class PenumbraJobFactory_ScenarioTestingFH66FH98 : PenumbraJobFactory
{
	public const string JOB_FILTER_REF = "FH66FH98";

	public override Filtering Filtering => new Filtering(Filtering.Type.FilterContains, "FH66FH98");

	public override string ActionBindingFilter => "ScenarioProcessing";

	[ConsoleCommand(Command = "RunPenumbra_FH66FH98", Description = "Run 66 and 98")]
	public static void StartFromConsoleFH66FH98(IConsole console)
	{
		PenumbraAgent.Instance.StartPenumbra(0u, "CustomSet,FH66FH98");
	}

	public override IEnumerator GetJobs(ILogger logger, bool sortByPriority, Action<IEnumerable<PenumbraJob>> outResults)
	{
		List<CCustomLevelData> scs = new List<CCustomLevelData>();
		yield return PenumbraJobFactory_ScenarioTestingAll.LoadKronosData(logger, scs);
		outResults(from data in scs
			where data.Name.StartsWith("FS_066_Benchmark") || data.Name.StartsWith("FS_098")
			select new PenumbraJob_RunScenario
			{
				ActionBindingFilter = ActionBindingFilter,
				JobContextName = data.Name,
				target = data
			});
	}
}
