using UnityEngine;

namespace Chronos;

public interface IComponentTimeline
{
	void Initialize();

	void OnStartOrReEnable();

	void Update();

	void FixedUpdate();

	void OnDisable();

	void AdjustProperties();

	void Reset();
}
public interface IComponentTimeline<T> : IComponentTimeline where T : Component
{
	T component { get; }
}
