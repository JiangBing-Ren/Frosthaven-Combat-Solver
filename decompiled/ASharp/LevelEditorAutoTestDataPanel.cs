using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorAutoTestDataPanel : MonoBehaviour
{
	[Header("Target State Management")]
	public TextMeshProUGUI TargetStateStatusLabel;

	public TextMeshProUGUI TargetStateSavingStatusLabel;

	public Button CreateTargetStateButton;

	public Button LoadTargetStateButton;

	public LayoutElement PreventionOverlay;

	[Header("Evaluation Configuration")]
	public TMP_InputField CustomTimeOut;

	private List<LevelEditorListItemInlineButtons> m_EvaluationListItems = new List<LevelEditorListItemInlineButtons>();

	[Header("Test Saving")]
	public TextMeshProUGUI InstructionsTextLabel;

	public TextMeshProUGUI StatusLabel;

	public TextMeshProUGUI SaveButtonLabel;

	public LayoutElement NeedsSaveElement;

	private void OnDisable()
	{
		StatusLabel.text = "";
	}

	public async Task InitFromAutoTest(AutoTestData dataToInitFrom)
	{
		string text = await SaveData.Instance.AutoTestDataManager.GetCurrentAutotestPath();
		InstructionsTextLabel.text = "Saves out to " + text + " as a .testdat file";
		if (LevelEditorController.s_Instance.AutoTestNeedsSaving)
		{
			TargetStateSavingStatusLabel.text = "<color=green>Expected result data succesfully edited, Dont forget to <b>SAVE</b>";
		}
		SetTestDataDirty(LevelEditorController.s_Instance.AutoTestNeedsSaving);
		LevelEditorController.s_Instance.AutoTestNeedsSaving = false;
		if (dataToInitFrom != null)
		{
			SaveButtonLabel.text = "Save AutoTest";
			RefreshTargetStateUIFromCurrentAutotest();
			RefreshEvaluationUIForCurrentTest();
		}
		else
		{
			SaveButtonLabel.text = "Create AutoTest";
			PreventionOverlay.gameObject.SetActive(value: true);
		}
	}

	public async void SaveAutoTest()
	{
		if (string.IsNullOrEmpty(SaveData.Instance.Global.CurrentEditorLevelData.Name))
		{
			StatusLabel.text = "<color=red> - Unable to create Autotest from Unsaved Level Data, Please go to \"Level Data\" Tab and save with a name- </color>";
			return;
		}
		Extensions.RandomState overrideRNGState = null;
		if (SaveData.Instance.Global.CurrentEditorAutoTestData != null)
		{
			overrideRNGState = new Extensions.RandomState(ScenarioManager.CurrentScenarioState.GuidRNGState.State);
		}
		ScenarioManager.CurrentScenarioState.Update(saveHiddenUnits: true);
		CCustomLevelData currentEditorLevelData = SaveData.Instance.Global.CurrentEditorLevelData;
		currentEditorLevelData.ScenarioState = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
		SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.ScenarioEventLog?.Events?.Clear();
		AutoTestData currentAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
		if (currentAutoTestData == null)
		{
			currentAutoTestData = new AutoTestData(await SaveData.Instance.Global.CurrentEditorLevelData.DeepCopySerializableObject<CCustomLevelData>());
		}
		else
		{
			AutoTestData autoTestData = currentAutoTestData;
			autoTestData.ScenarioState = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
		}
		currentAutoTestData.CustomTimeout = (string.IsNullOrEmpty(CustomTimeOut.text) ? 300f : float.Parse(CustomTimeOut.text));
		if (overrideRNGState != null)
		{
			currentAutoTestData.ScenarioState.GuidRNGState = overrideRNGState;
		}
		FileInfo fileInfo = SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile;
		if (fileInfo == null)
		{
			fileInfo = new FileInfo(await SaveData.Instance.AutoTestDataManager.AutoTestSaveFolder(SaveData.Instance.Global.CurrentEditorLevelData.Name));
		}
		if (await SaveData.Instance.AutoTestDataManager.SaveAutoTestData(currentAutoTestData, fileInfo))
		{
			GlobalData global = SaveData.Instance.Global;
			global.CurrentAutoTestDataCopy = await currentAutoTestData.DeepCopySerializableObject<AutoTestData>();
			StatusLabel.text = "<color=green> - Save Successful - </color>";
			SaveData.Instance.Global.CurrentEditorAutoTestData = currentAutoTestData;
		}
		else
		{
			StatusLabel.text = "<color=red> - Save Failed - </color>";
		}
		await InitFromAutoTest(SaveData.Instance.Global.CurrentEditorAutoTestData);
	}

	public void SetTestDataDirty(bool needsSave)
	{
		NeedsSaveElement.gameObject.SetActive(needsSave);
	}

	public void RefreshEvaluationUIForCurrentTest()
	{
		foreach (LevelEditorListItemInlineButtons evaluationListItem in m_EvaluationListItems)
		{
			UnityEngine.Object.Destroy(evaluationListItem.gameObject);
		}
		m_EvaluationListItems = new List<LevelEditorListItemInlineButtons>();
		AutoTestData currentEditorAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
		if (currentEditorAutoTestData != null)
		{
			CustomTimeOut.SetValue(currentEditorAutoTestData.CustomTimeout.ToString());
		}
	}

	public void OnCustomTimeoutEdited()
	{
		SetTestDataDirty(needsSave: true);
	}

	public void RefreshTargetStateUIFromCurrentAutotest()
	{
		AutoTestData currentEditorAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
		if (currentEditorAutoTestData != null)
		{
			PreventionOverlay.gameObject.SetActive(value: false);
			ScenarioState expectedResultingScenarioState = currentEditorAutoTestData.ExpectedResultingScenarioState;
			LoadTargetStateButton.interactable = expectedResultingScenarioState != null;
			if (expectedResultingScenarioState == null)
			{
				TargetStateStatusLabel.text = "Currently there is no Expected Resulting Data state saved to Autotest \"" + currentEditorAutoTestData.Name + "\"";
				return;
			}
			TargetStateStatusLabel.text = "Data for Expected Test Result for <b>\"" + currentEditorAutoTestData.Name + "\"</b> exists, and was last saved at <b>" + currentEditorAutoTestData.ExpectedStateTimeStamp.ToString("MM/dd/yyyy hh:mm tt") + "</b>. It has <b>[" + currentEditorAutoTestData.RecordedUIActions.Events.Count + "]</b> Recorded UI actions, and needs <b>[" + currentEditorAutoTestData.ChoreographerStepCountUntilResult + "]</b> steps of the Choreographer before evaluating.";
		}
	}

	public void CreateStateFromCurrentEditorState()
	{
		AutoTestData currentAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
		if (currentAutoTestData.ExpectedResultingScenarioState != null)
		{
			LevelEditorController.s_Instance.m_LevelEditorUIInstance.ConfirmationDialog.ShowDialogWithText("Confirm overwrite existing data with copy of editor starting data?", CreateStateAction, null);
		}
		else
		{
			CreateStateAction();
		}
		async void CreateStateAction()
		{
			GlobalData global = SaveData.Instance.Global;
			global.CurrentEditorAutoTestData = new AutoTestData(await SaveData.Instance.Global.CurrentEditorLevelData.DeepCopySerializableObject<CCustomLevelData>());
			currentAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
			currentAutoTestData.RecordedUIActions = new CAutoLog();
			currentAutoTestData.ChoreographerStepCountUntilResult = 0;
			currentAutoTestData.ExpectedStateTimeStamp = DateTime.Now;
			SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.ScenarioEventLog?.Events?.Clear();
			AutoTestData autoTestData = currentAutoTestData;
			autoTestData.ExpectedResultingScenarioState = await SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.DeepCopySerializableObject<ScenarioState>();
			RefreshTargetStateUIFromCurrentAutotest();
			TargetStateSavingStatusLabel.text = "<color=green>Initialised Expected result data from starting data";
			SetTestDataDirty(needsSave: true);
		}
	}

	public void LoadIntoTargetState()
	{
		LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
		SceneController.Instance.AutoTestStart(SaveData.Instance.Global.CurrentEditorAutoTestData, EAutoTestControllerState.EditingTargetState, withoutAutotestFunctionality: false, skipMonsterCardGeneration: true);
	}
}
