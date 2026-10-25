namespace FH.GraphicalUserInterface.MainMenu;

public static class MainMenuUIAutomation
{
	public static bool LevelEditorAutoSelectMode;

	public static string SaveFile;

	static MainMenuUIAutomation()
	{
		LevelEditorAutoSelectMode = false;
		SaveFile = "";
		Reset();
	}

	public static void Reset()
	{
		LevelEditorAutoSelectMode = false;
		SaveFile = string.Empty;
	}
}
