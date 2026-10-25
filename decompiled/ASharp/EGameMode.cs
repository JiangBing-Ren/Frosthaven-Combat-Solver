using System;

[Serializable]
public enum EGameMode
{
	None,
	MainMenu,
	Campaign,
	LevelEditor,
	[Obsolete]
	Guildmaster,
	SingleScenario,
	Autotest,
	FrontEndTutorial
}
