using UnityEngine;

namespace Chronos;

public interface IParticleSystemTimeline : IComponentTimeline<ParticleSystem>, IComponentTimeline
{
	float playbackSpeed { get; set; }

	float time { get; set; }

	bool enableEmission { get; set; }

	bool isPlaying { get; }

	bool isPaused { get; }

	bool isStopped { get; }

	void Play(bool withChildren = true);

	void Pause(bool withChildren = true);

	void Stop(bool withChildren = true);

	bool IsAlive(bool withChildren = true);
}
