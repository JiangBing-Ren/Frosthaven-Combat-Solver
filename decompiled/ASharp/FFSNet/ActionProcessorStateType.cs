namespace FFSNet;

public enum ActionProcessorStateType
{
	NONE,
	Halted,
	ProcessFreely,
	ProcessOneAndHalt,
	ProcessOneAndSwitchBack,
	SwitchBackToSavedState
}
