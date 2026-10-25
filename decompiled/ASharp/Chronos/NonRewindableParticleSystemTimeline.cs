using UnityEngine;

namespace Chronos;

public class NonRewindableParticleSystemTimeline : ComponentTimeline<ParticleSystem>, IParticleSystemTimeline, IComponentTimeline<ParticleSystem>, IComponentTimeline
{
	private bool warnedRewind;

	private float _playbackSpeed;

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			_playbackSpeed = value;
			AdjustProperties();
		}
	}

	public float time
	{
		get
		{
			return component.time;
		}
		set
		{
			component.time = value;
		}
	}

	public bool enableEmission
	{
		get
		{
			return component.emission.enabled;
		}
		set
		{
			ParticleSystem.EmissionModule emission = component.emission;
			emission.enabled = value;
		}
	}

	public bool isPlaying => component.isPlaying;

	public bool isPaused => component.isPaused;

	public bool isStopped => component.isStopped;

	public NonRewindableParticleSystemTimeline(Timeline timeline, ParticleSystem component)
		: base(timeline, component)
	{
	}

	public void Play(bool withChildren = true)
	{
		component.Play(withChildren);
	}

	public void Pause(bool withChildren = true)
	{
		component.Pause(withChildren);
	}

	public void Stop(bool withChildren = true)
	{
		component.Stop(withChildren);
	}

	public bool IsAlive(bool withChildren = true)
	{
		return component.IsAlive(withChildren);
	}

	public override void CopyProperties(ParticleSystem source)
	{
		_playbackSpeed = source.main.simulationSpeed;
	}

	public override void AdjustProperties(float timeScale)
	{
		if (timeScale < 0f && !warnedRewind)
		{
			Debug.LogWarning("Trying to rewind a non-rewindable particle system.", timeline);
			warnedRewind = true;
		}
		ParticleSystem.MainModule main = component.main;
		main.simulationSpeed = playbackSpeed * timeScale;
	}
}
