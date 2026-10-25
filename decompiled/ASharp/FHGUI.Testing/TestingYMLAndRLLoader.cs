using FH.YML;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using UnityEngine;

namespace FHGUI.Testing;

[DefaultExecutionOrder(-10102)]
public class TestingYMLAndRLLoader : MonoBehaviour
{
	public bool IsStandaloneScene
	{
		get
		{
			if (SceneController.Instance == null)
			{
				return Application.isEditor;
			}
			return false;
		}
	}

	public void Load()
	{
		RecompileRuleset();
		SharedClient.Initialise(string.Empty, YMLLoading.RulebaseFolder);
		ScenarioRuleClient.Initialise(reinitialise: true);
		MapRuleLibraryClient.Instance.Initialise(reinitialise: true);
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		if (YMLDefRepository.Repo.DefRepository == null)
		{
			YMLDefRepository.Repo.DebugGenerateDefRepository();
		}
	}

	private bool RecompileRuleset()
	{
		string rulesetPath = YMLLoading.GetRulesetPath();
		string rulesetZipPath = YMLLoading.GetRulesetZipPath();
		return YMLLoading.CompileRuleset(rulesetPath, rulesetZipPath, onlyIfOutdated: true);
	}

	public static void Unload()
	{
		YMLDefRepository.Repo.DebugUnload();
	}

	private void Awake()
	{
		if (!IsStandaloneScene)
		{
			Object.DestroyImmediate(gameObject);
		}
		else
		{
			Load();
		}
	}

	private void OnDestroy()
	{
		if (IsStandaloneScene)
		{
			Unload();
		}
	}
}
