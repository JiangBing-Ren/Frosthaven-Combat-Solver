namespace FFSNet;

public interface IControllable
{
	bool IsAlive { get; }

	void OnControlAssigned(NetworkPlayer controller);

	void OnControlReleased();

	string GetName();

	string GetID();
}
