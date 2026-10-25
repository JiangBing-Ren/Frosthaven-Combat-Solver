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
public class PenumbraJobFactory_ScenarioTestingFH : PenumbraJobFactory
{
	public const string JOB_FILTER_REF = "Frosthaven";

	public override Filtering Filtering => new Filtering(Filtering.Type.FilterContains, "Frosthaven");

	public override string ActionBindingFilter => "ScenarioProcessing";

	public override IEnumerator GetJobs(ILogger logger, bool sortByPriority, Action<IEnumerable<PenumbraJob>> outResults)
	{
		List<CCustomLevelData> scs = new List<CCustomLevelData>();
		yield return PenumbraJobFactory_ScenarioTestingAll.LoadKronosData(logger, scs);
		List<PenumbraJob_RunScenario> list = (from data in scs
			where data.Name.StartsWith("FH") || data.Name.StartsWith("FS") || data.Name.StartsWith("RFS")
			select new PenumbraJob_RunScenario
			{
				ActionBindingFilter = ActionBindingFilter,
				JobContextName = data.Name,
				target = data
			}).ToList();
		if (sortByPriority)
		{
			list.Sort((PenumbraJob_RunScenario a, PenumbraJob_RunScenario b) => a.JobContextName.CompareTo(b.JobContextName));
		}
		outResults(list);
	}
}
