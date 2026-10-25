namespace FFSNet;

public enum DataActionType
{
	NONE,
	SendInitialGameState,
	SendCurrentGameState,
	CompareScenarioStates,
	SendPlayerLogFromHost,
	SendPlayerLogFromClient,
	SendStateDumpFromHost,
	SendFHGameState
}
