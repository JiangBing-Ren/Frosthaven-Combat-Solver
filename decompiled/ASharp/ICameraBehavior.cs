public interface ICameraBehavior
{
	bool IsEnabled { get; }

	string ID { get; }

	bool Update(CameraController camera);

	void Enable(CameraController camera);

	void Disable(CameraController camera);

	bool IsType(ECameraBehaviorType type);

	void OnDestroy();
}
