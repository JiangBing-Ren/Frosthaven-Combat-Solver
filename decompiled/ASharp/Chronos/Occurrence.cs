namespace Chronos;

public abstract class Occurrence
{
	public float time { get; internal set; }

	public bool repeatable { get; internal set; }

	public abstract void Forward();

	public abstract void Backward();
}
