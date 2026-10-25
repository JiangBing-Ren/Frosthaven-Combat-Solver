public abstract class CameraBehavior : ICameraBehavior
{
	public bool IsEnabled { get; private set; }

	public string ID { get; private set; }

	protected CameraBehavior(string id)
	{
		ID = id;
	}

	public virtual void Enable(CameraController camera)
	{
		IsEnabled = true;
	}

	public virtual void Disable(CameraController camera)
	{
		IsEnabled = false;
	}

	public abstract bool IsType(ECameraBehaviorType type);

	public virtual void OnDestroy()
	{
	}

	public virtual bool Update(CameraController camera)
	{
		return IsEnabled;
	}
}
