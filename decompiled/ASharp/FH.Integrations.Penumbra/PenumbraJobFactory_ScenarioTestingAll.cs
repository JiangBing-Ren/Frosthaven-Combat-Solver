using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.Penumbra;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJobFactory_ScenarioTestingAll : PenumbraJobFactory
{
	public const string JOB_FILTER_REF = "CustomSet";

	public const string ACTION_BINDING_REF = "ScenarioProcessing";

	public override Filtering Filtering => new Filtering(Filtering.Type.FilterMissing, "CustomSet");

	public override string ActionBindingFilter => "ScenarioProcessing";

	public override IEnumerator GetJobs(ILogger logger, bool sortByPriority, Action<IEnumerable<PenumbraJob>> outResults)
	{
		List<CCustomLevelData> scs = new List<CCustomLevelData>();
		yield return LoadKronosData(logger, scs);
		outResults(scs.Select((CCustomLevelData data) => new PenumbraJob_RunScenario
		{
			ActionBindingFilter = ActionBindingFilter,
			JobContextName = data.Name,
			target = data
		}));
	}

	public static IEnumerator LoadKronosData(ILogger logger, List<CCustomLevelData> scs)
	{
		logger.Log("Waiting for scene");
		yield return new WaitForSeconds(30f);
		while (!SceneController.Instance || SceneController.Instance.YML == null || ScenarioRuleClient.SRLYML == null)
		{
			yield return null;
		}
		yield return SceneController.Instance.YML.LoadCampaignAsyncCrt();
		scs.AddRange(from a in ScenarioRuleClient.SRLYML.CustomLevels
			select ScenarioRuleClient.SRLYML.GetCustomLevel(a.Key) into a
			where a != null
			select a);
		logger.Log("Waiting for MapRuleLibrary.Client.MapRuleLibraryClient.MRLYML");
		while (MapRuleLibraryClient.MRLYML == null)
		{
			yield return null;
		}
	}
}
