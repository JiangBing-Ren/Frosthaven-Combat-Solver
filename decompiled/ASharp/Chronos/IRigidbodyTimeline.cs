namespace Chronos;

public interface IRigidbodyTimeline
{
	float mass { get; set; }

	float drag { get; set; }

	float angularDrag { get; set; }
}
