using System.Collections.Generic;
using UnityEngine;

namespace Chronos;

public class RewindableParticleSystemTimeline : ComponentTimeline<ParticleSystem>, IParticleSystemTimeline, IComponentTimeline<ParticleSystem>, IComponentTimeline
{
	private enum State
	{
		Playing,
		Paused,
		Stopping,
		Stopped
	}

	private enum EmissionAction
	{
		EnableEmission,
		DisableEmission,
		Play,
		Stop
	}

	private struct StateEvent(State state, float time)
	{
		public State state = state;

		public float time = time;
	}

	private struct EmissionEvent(EmissionAction action, float time)
	{
		public EmissionAction action = action;

		public float time = time;
	}

	private delegate void ChildNativeAction(ParticleSystem target);

	private delegate void ChildChronosAction(IParticleSystemTimeline target);

	private delegate bool ChildNativeCheck(ParticleSystem target);

	private delegate bool ChildChronosCheck(IParticleSystemTimeline target);

	private float absoluteSimulationTime;

	private float loopedSimulationTime;

	private float relativeStartTime;

	private List<StateEvent> stateEvents;

	private State stateOnStart;

	private List<EmissionEvent> emissionEvents;

	private bool enableEmissionOnStart;

	private State _state;

	private bool _enableEmission;

	public float playbackSpeed { get; set; }

	public float time
	{
		get
		{
			return (loopedSimulationTime - relativeStartTime) % component.main.duration;
		}
		set
		{
			loopedSimulationTime = relativeStartTime + value;
		}
	}

	public bool isPlaying
	{
		get
		{
			if (state != State.Playing)
			{
				return state == State.Stopping;
			}
			return true;
		}
	}

	public bool isPaused => state == State.Paused;

	public bool isStopped => state == State.Stopped;

	private float stateEventsTime => timeline.time;

	private float emissionEventsTime => absoluteSimulationTime;

	private State state
	{
		get
		{
			return _state;
		}
		set
		{
			if (AssertForwardProperty("state", Severity.Error) && _state != value)
			{
				RegisterState(value);
				_state = value;
			}
		}
	}

	public bool enableEmission
	{
		get
		{
			return _enableEmission;
		}
		set
		{
			if (AssertForwardProperty("enableEmission", Severity.Warn))
			{
				if (_enableEmission && !value)
				{
					RegisterEmission(EmissionAction.DisableEmission);
				}
				else if (!_enableEmission & value)
				{
					RegisterEmission(EmissionAction.EnableEmission);
				}
				_enableEmission = value;
			}
		}
	}

	private void RegisterState(State state)
	{
		stateEvents.Add(new StateEvent(state, stateEventsTime));
	}

	private void RegisterEmission(EmissionAction action)
	{
		emissionEvents.Add(new EmissionEvent(action, emissionEventsTime));
	}

	public RewindableParticleSystemTimeline(Timeline timeline, ParticleSystem component)
		: base(timeline, component)
	{
		emissionEvents = new List<EmissionEvent>();
		stateEvents = new List<StateEvent>();
	}

	public override void CopyProperties(ParticleSystem source)
	{
		playbackSpeed = source.main.simulationSpeed;
		State state = (this.state = ((!source.main.playOnAwake) ? State.Stopped : State.Playing));
		stateOnStart = state;
		enableEmissionOnStart = (_enableEmission = source.emission.enabled);
		time = 0f;
		if (source.useAutoRandomSeed)
		{
			if (source.isPlaying)
			{
				source.Pause(withChildren: true);
			}
			source.useAutoRandomSeed = false;
			source.randomSeed = (uint)Random.Range(1, int.MaxValue);
		}
	}

	public override void Update()
	{
		if (timeline.timeScale < 0f)
		{
			if (stateEvents.Count > 0)
			{
				StateEvent item = stateEvents[stateEvents.Count - 1];
				if (stateEventsTime <= item.time)
				{
					stateEvents.Remove(item);
					if (stateEvents.Count > 0)
					{
						_state = stateEvents[stateEvents.Count - 1].state;
					}
					else
					{
						_state = stateOnStart;
					}
				}
			}
			for (int num = emissionEvents.Count - 1; num >= 0; num--)
			{
				if (emissionEvents[num].time > emissionEventsTime)
				{
					emissionEvents.RemoveAt(num);
				}
			}
		}
		component.Simulate(0f, withChildren: true, restart: true);
		if (loopedSimulationTime > 0f)
		{
			ParticleSystem.EmissionModule emission = component.emission;
			emission.enabled = enableEmissionOnStart;
			float num2 = 0f;
			for (int i = 0; i < emissionEvents.Count; i++)
			{
				EmissionEvent emissionEvent = emissionEvents[i];
				component.Simulate(emissionEvent.time - num2, withChildren: true, restart: false);
				emission.enabled = emissionEvent.action == EmissionAction.Play || emissionEvent.action == EmissionAction.EnableEmission;
				num2 = emissionEvent.time;
			}
			component.Simulate(loopedSimulationTime - num2, withChildren: true, restart: false);
			if (state == State.Stopping && component.particleCount == 0 && timeline.timeScale > 0f)
			{
				state = State.Stopped;
			}
		}
		if (state == State.Playing || state == State.Stopping)
		{
			absoluteSimulationTime += timeline.deltaTime * playbackSpeed;
			if (state == State.Playing && !component.main.loop && absoluteSimulationTime >= component.main.duration)
			{
				state = State.Stopping;
			}
			float num3 = Timekeeper.instance.maxParticleLoops;
			if (num3 > 0f && state != State.Stopping)
			{
				loopedSimulationTime = absoluteSimulationTime % (component.main.duration * num3);
			}
			else
			{
				loopedSimulationTime = absoluteSimulationTime;
			}
		}
	}

	public void Play(bool withChildren = true)
	{
		if (!AssertForwardMethod("Play", Severity.Warn))
		{
			return;
		}
		if (state != State.Paused)
		{
			RegisterEmission(EmissionAction.Play);
			relativeStartTime = loopedSimulationTime;
		}
		state = State.Playing;
		if (withChildren)
		{
			ExecuteOnChildren((ParticleSystem ps) =>
			{
				ps.Play(withChildren: false);
			}, (IParticleSystemTimeline ps) =>
			{
				ps.Play(withChildren: false);
			});
		}
	}

	public void Pause(bool withChildren = true)
	{
		if (!AssertForwardMethod("Pause", Severity.Warn))
		{
			return;
		}
		state = State.Paused;
		if (withChildren)
		{
			ExecuteOnChildren((ParticleSystem ps) =>
			{
				ps.Pause(withChildren: false);
			}, (IParticleSystemTimeline ps) =>
			{
				ps.Pause(withChildren: false);
			});
		}
	}

	public void Stop(bool withChildren = true)
	{
		if (!AssertForwardMethod("Stop", Severity.Warn))
		{
			return;
		}
		state = State.Stopping;
		RegisterEmission(EmissionAction.Stop);
		if (withChildren)
		{
			ExecuteOnChildren((ParticleSystem ps) =>
			{
				ps.Stop(withChildren: false);
			}, (IParticleSystemTimeline ps) =>
			{
				ps.Stop(withChildren: false);
			});
		}
	}

	public bool IsAlive(bool withChildren = true)
	{
		if (state == State.Stopped)
		{
			return false;
		}
		if (withChildren)
		{
			return CheckOnChildren((ParticleSystem ps) => ps.IsAlive(withChildren: false), (IParticleSystemTimeline ps) => ps.IsAlive(withChildren: false));
		}
		return true;
	}

	private void ExecuteOnChildren(ChildNativeAction native, ChildChronosAction chronos)
	{
		ParticleSystem[] componentsInChildren = base.timeline.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (!(particleSystem == component))
			{
				Timeline timeline = particleSystem.GetComponent<Timeline>();
				if (timeline != null)
				{
					chronos(timeline._particleSystem);
				}
				else
				{
					native(particleSystem);
				}
			}
		}
	}

	private bool CheckOnChildren(ChildNativeCheck native, ChildChronosCheck chronos)
	{
		ParticleSystem[] componentsInChildren = base.timeline.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (particleSystem == component)
			{
				continue;
			}
			Timeline timeline = particleSystem.GetComponent<Timeline>();
			if (timeline != null)
			{
				if (!chronos(timeline._particleSystem))
				{
					return false;
				}
			}
			else if (!native(particleSystem))
			{
				return false;
			}
		}
		return true;
	}

	private bool AssertForwardMethod(string method, Severity severity)
	{
		if (timeline.timeScale <= 0f)
		{
			switch (severity)
			{
			case Severity.Error:
				throw new ChronosException("Cannot call " + method + " on the particle system while time is paused or rewinding.");
			case Severity.Warn:
				Debug.LogWarning("Trying to call " + method + " on the particle system while time is paused or rewinding, ignoring.");
				break;
			}
		}
		return timeline.timeScale > 0f;
	}

	private bool AssertForwardProperty(string property, Severity severity)
	{
		if (timeline.timeScale <= 0f)
		{
			switch (severity)
			{
			case Severity.Error:
				throw new ChronosException("Cannot set " + property + " on the particle system while time is paused or rewinding.");
			case Severity.Warn:
				Debug.LogWarning("Trying to set " + property + " on the particle system while time is paused or rewinding, ignoring.");
				break;
			}
		}
		return timeline.timeScale > 0f;
	}
}
