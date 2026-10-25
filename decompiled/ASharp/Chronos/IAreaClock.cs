using UnityEngine;

namespace Chronos;

public interface IAreaClock
{
	float timeScale { get; }

	AreaClockMode mode { get; }

	ClockBlend innerBlend { get; set; }

	AnimationCurve curve { get; set; }

	void Release(Timeline timeline);

	void ReleaseAll();

	float TimeScale(Timeline timeline);

	bool ContainsPoint(Vector3 point);
}
