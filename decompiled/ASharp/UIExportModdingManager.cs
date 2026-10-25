using GLOOM;
using UnityEngine;

public class UIExportModdingManager : MonoBehaviour
{
	[SerializeField]
	private UIBackgroundTaskMask loadingScreen;

	[SerializeField]
	private TextLocalizedListener hintText;

	public void Export()
	{
		loadingScreen.Show();
	}

	private void OnExportFailed()
	{
		FinishExport();
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_EXPORT_FAILED_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_EXPORT_FAILED_CONFIRMATION"), UIInfoTools.Instance.warningColor, "GUI_CLOSE");
	}

	private void OnExportSuccessed()
	{
		FinishExport();
	}

	private void FinishExport()
	{
		loadingScreen.Hide();
	}
}
