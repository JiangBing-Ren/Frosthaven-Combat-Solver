using System.Collections;
using System.Diagnostics;
using FH.YML;
using FHGUI.Common;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.Profiling;

namespace FHGUI.Testing;

[DefaultExecutionOrder(-100)]
public class TestGlossary : MonoBehaviour
{
	public FullAbilityCard Card;

	public UIGlossaryContainer GlossaryContainer;

	private IEnumerator Start()
	{
		Card.gameObject.SetActive(value: false);
		SharedClient.Initialise(string.Empty, YMLLoading.RulebaseFolder);
		ScenarioRuleClient.Initialise(reinitialise: true);
		MapRuleLibraryClient.Instance.Initialise(reinitialise: true);
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		if (YMLDefRepository.Repo.DefRepository == null)
		{
			YMLDefRepository.Repo.DebugGenerateDefRepository();
		}
		yield return UIInfoTools.InitInstanceCrt();
		Card.gameObject.SetActive(value: true);
	}

	public void OnEnable()
	{
		DoTestGlossary();
	}

	private void DoTestGlossary()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Profiler.enabled = true;
		LocalizationGlossaryUtils.GetGlossaryContents(Card.gameObject);
		stopwatch.Stop();
		UnityEngine.Debug.Log($"Glossary took {stopwatch.Elapsed}");
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "TEST"))
		{
			DoTestGlossary();
		}
	}
}
