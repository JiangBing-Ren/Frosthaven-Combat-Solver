using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.Penumbra;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJobFactory_Benchmark : PenumbraJobFactory
{
	private const string JOB_FILTER_REF = "Benchmark";

	public override Filtering Filtering => new Filtering((Filtering.Type.FilterContains, "Benchmark"), (Filtering.Type.FilterMissing, "Frosthaven"), (Filtering.Type.FilterMissing, "FH66FH98"));

	public override string ActionBindingFilter => "BenchmarkActions";

	public override IEnumerator GetJobs(ILogger logger, bool sortByPriority, Action<IEnumerable<PenumbraJob>> outResults)
	{
		List<CCustomLevelData> scs = new List<CCustomLevelData>();
		yield return PenumbraJobFactory_ScenarioTestingAll.LoadKronosData(logger, scs);
		List<PenumbraJob> list = new List<PenumbraJob>();
		list.AddRange(from data in scs
			where data.Name.StartsWith("FH-66")
			select new PenumbraJob_RunScenario
			{
				ActionBindingFilter = ActionBindingFilter,
				JobContextName = data.Name,
				target = data
			});
		list.Add(new PenumbraJob_LoadSimpleScene
		{
			ActionBindingFilter = ActionBindingFilter,
			JobContextName = "Benchmark",
			target = "GYM_Benchmark"
		});
		outResults(list);
	}
}
