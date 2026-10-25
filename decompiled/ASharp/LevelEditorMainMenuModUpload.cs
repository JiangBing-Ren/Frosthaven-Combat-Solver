using System;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMainMenuModUpload : MonoBehaviour, IProgress<float>
{
	private enum ELevelEditorModUploadState
	{
		None,
		ConfirmingForUpload,
		UploadingNew,
		Uploading,
		UploadCompleteSuccess,
		UploadCompleteFailed
	}

	public LevelEditorMainMenu ParentMenu;

	public Slider ProgressSlider;

	public TextMeshProUGUI ConfirmationText;

	public GameObject ButtonGroup;

	public TextMeshProUGUI ConfirmationButtonText;

	public GameObject CancelButton;

	private CCustomLevelData m_LevelToUpload;

	private ELevelEditorModUploadState m_UploadState;

	private string m_UploadLog;

	public void SetUploadingNewLevel(CCustomLevelData levelToUpload)
	{
		m_LevelToUpload = levelToUpload;
		SetUploadState(ELevelEditorModUploadState.ConfirmingForUpload);
	}

	private void SetUploadState(ELevelEditorModUploadState stateToSet)
	{
		if (stateToSet != m_UploadState)
		{
			m_UploadState = stateToSet;
			switch (m_UploadState)
			{
			case ELevelEditorModUploadState.None:
				ParentMenu.RefreshUIFromDataManagers();
				gameObject.SetActive(value: false);
				break;
			case ELevelEditorModUploadState.ConfirmingForUpload:
				gameObject.SetActive(value: true);
				ProgressSlider.gameObject.SetActive(value: false);
				ConfirmationText.gameObject.SetActive(value: true);
				ButtonGroup.SetActive(value: true);
				CancelButton.gameObject.SetActive(value: true);
				ConfirmationButtonText.text = "Confirm";
				ConfirmationText.text = "Are you sure you want to upload \"" + m_LevelToUpload.Name + "\" to the Steam workshop?";
				break;
			case ELevelEditorModUploadState.UploadingNew:
			case ELevelEditorModUploadState.Uploading:
				ProgressSlider.gameObject.SetActive(value: true);
				ConfirmationText.gameObject.SetActive(value: false);
				ButtonGroup.SetActive(value: false);
				break;
			case ELevelEditorModUploadState.UploadCompleteSuccess:
				ProgressSlider.gameObject.SetActive(value: false);
				ConfirmationText.gameObject.SetActive(value: true);
				ButtonGroup.SetActive(value: true);
				CancelButton.gameObject.SetActive(value: false);
				ConfirmationButtonText.text = "OK";
				ConfirmationText.text = "Successfully uploaded \"" + m_LevelToUpload.Name + "\" to the Steam workshop!";
				break;
			case ELevelEditorModUploadState.UploadCompleteFailed:
				ProgressSlider.gameObject.SetActive(value: false);
				ConfirmationText.gameObject.SetActive(value: true);
				ButtonGroup.SetActive(value: true);
				CancelButton.gameObject.SetActive(value: false);
				ConfirmationButtonText.text = "OK";
				ConfirmationText.text = "Failed to upload \"" + m_LevelToUpload.Name + "\" to the Steam workshop. \nError: " + m_UploadLog;
				break;
			}
		}
	}

	public void Report(float value)
	{
		if (m_UploadState == ELevelEditorModUploadState.Uploading || m_UploadState == ELevelEditorModUploadState.UploadingNew)
		{
			ProgressSlider.value = value;
		}
	}

	public void OnUploadFailed(string uploadReport)
	{
		m_UploadLog = uploadReport;
		SetUploadState(ELevelEditorModUploadState.UploadCompleteFailed);
	}

	public void OnUploadSuccessful()
	{
		if (m_UploadState == ELevelEditorModUploadState.UploadingNew)
		{
			ProgressSlider.value = 0f;
			SetUploadState(ELevelEditorModUploadState.Uploading);
		}
		else if (m_UploadState == ELevelEditorModUploadState.Uploading)
		{
			SetUploadState(ELevelEditorModUploadState.UploadCompleteSuccess);
			ProgressSlider.value = 0f;
		}
	}

	public void OnConfirmPressed()
	{
		Log.GUIError("Unable to upload. Modding not supported on this platform");
	}

	public void OnCancelPressed()
	{
		SetUploadState(ELevelEditorModUploadState.None);
	}
}
