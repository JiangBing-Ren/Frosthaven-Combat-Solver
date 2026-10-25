using GLOOM;
using UnityEngine;

public static class UIModdingNotifications
{
	private const string NOTIFICATION_TITLE = "GUI_MODDING_MENU";

	private const string NOTIFICATION_TAG = "MODDING";

	private const string ID_PLAY_RULESET_NOTIFICATION = "PLAY_RULESET";

	private const string ID_RULESET_NOTIFICATION = "RULESET_{0}";

	private const string ID_EXPORT_NOTIFICATION = "EXPORT";

	public static void ShowCreateRulesetNotification(string rulesetName)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_CREATE_RULESET_NOTIFICATION"), rulesetName);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, $"RULESET_{rulesetName}", "MODDING");
	}

	public static void ShowDeleteRulesetNotification(string rulesetName)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_DELETE_RULESET_NOTIFICATION"), rulesetName);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, $"RULESET_{rulesetName}", "MODDING");
	}

	public static void ShowPlayRulesetNotification(string rulesetName)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_PLAY_RULESET_NOTIFICATION"), rulesetName);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, "PLAY_RULESET", "MODDING");
	}

	public static void ShowValidatedNotification(string name)
	{
		string text = "<color=#" + UIInfoTools.DefInstance.positiveTextColor.ToHex() + ">" + string.Format(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATED_NOTIFICATION"), name) + "</color>";
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, null, "MODDING");
	}

	public static void ShowCreatedModNotification(string modName)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_CREATED_MOD_NOTIFICATION"), modName);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, text, "MODDING");
	}

	public static void ShowExportSuccessNotification(string result)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_EXPORT_SUCCESS_NOTIFICATION"), result);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, () =>
		{
			GUIUtility.systemCopyBuffer = result;
			ShowExportPathCopiedNotification();
		}, "GUI_EXPORTING_MODS_COPY_PATH", UIInfoTools.Instance.defaultNotificationDuration, "EXPORT", "MODDING");
	}

	public static void ShowExportPathCopiedNotification()
	{
		string text = "<color=#" + UIInfoTools.DefInstance.positiveTextColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_EXPORTING_MODS_COPY_PATH_NOTIFICATION") + "</color>";
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, "EXPORT", "MODDING");
	}

	public static void ShowUploadedModNotification(string modName)
	{
		string text = string.Format(LocalizationManager.GetTranslation("GUI_MODDING_UPLOAD_SUCCESSFUL"), modName);
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_MODDING_MENU", text, UIInfoTools.Instance.defaultNotificationDuration, null, "MODDING");
	}
}
