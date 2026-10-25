using System;

[Serializable]
public enum EResult
{
	None,
	InProgress,
	InProgressAppKill,
	Lose,
	Win,
	Resign,
	Restart
}
