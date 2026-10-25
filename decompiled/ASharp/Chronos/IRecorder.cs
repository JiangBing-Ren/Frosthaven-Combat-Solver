namespace Chronos;

public interface IRecorder
{
	void Reset();

	int EstimateMemoryUsage();
}
