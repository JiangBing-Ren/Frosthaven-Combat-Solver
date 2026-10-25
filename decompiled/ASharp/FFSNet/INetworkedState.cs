namespace FFSNet;

public interface INetworkedState
{
	void UpdateControllerID(int controllerID);

	void UpdateState(GameAction action);

	void ApplyState();

	void ResetState();

	void ClearScenarioState();

	ulong GetRevisionHash();
}
