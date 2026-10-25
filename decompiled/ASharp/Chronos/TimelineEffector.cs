using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Chronos;

public abstract class TimelineEffector : MonoBehaviour
{
	protected internal const float DefaultRecordingInterval = 0.5f;

	private bool enabledOnce;

	[SerializeField]
	private float _recordingInterval = 0.5f;

	internal List<IComponentTimeline> components;

	protected abstract Timeline timeline { get; }

	public float recordingInterval
	{
		get
		{
			return _recordingInterval;
		}
		set
		{
			_recordingInterval = value;
			if (recorder != null)
			{
				recorder.Reset();
			}
		}
	}

	public AnimationTimeline _animation { get; protected set; }

	public AnimatorTimeline animator { get; protected set; }

	public List<AudioSourceTimeline> audioSources { get; protected set; }

	public AudioSourceTimeline audioSource { get; protected set; }

	public NavMeshAgentTimeline navMeshAgent { get; protected set; }

	public IParticleSystemTimeline _particleSystem { get; protected set; }

	public RigidbodyTimeline3D _rigidbody { get; protected set; }

	public RigidbodyTimeline2D _rigidbody2D { get; protected set; }

	public new TransformTimeline transform { get; protected set; }

	public WindZoneTimeline windZone { get; protected set; }

	public TerrainTimeline terrain { get; protected set; }

	public TrailRendererTimeline trailRenderer { get; protected set; }

	public WheelColliderTimeline wheelCollider { get; protected set; }

	protected IRecorder recorder
	{
		get
		{
			if (_rigidbody != null)
			{
				return _rigidbody;
			}
			if (_rigidbody2D != null)
			{
				return _rigidbody2D;
			}
			if (transform != null)
			{
				return transform;
			}
			return null;
		}
	}

	public TimelineEffector()
	{
		components = new List<IComponentTimeline>();
		audioSources = new List<AudioSourceTimeline>();
	}

	protected virtual void Awake()
	{
		CacheComponents();
	}

	private void Start()
	{
		OnStartOrReEnable();
	}

	private void OnEnable()
	{
		if (enabledOnce)
		{
			OnStartOrReEnable();
		}
		else
		{
			enabledOnce = true;
		}
	}

	protected virtual void OnStartOrReEnable()
	{
		for (int i = 0; i < components.Count; i++)
		{
			IComponentTimeline componentTimeline = components[i];
			componentTimeline.AdjustProperties();
			componentTimeline.OnStartOrReEnable();
		}
	}

	protected virtual void Update()
	{
		for (int i = 0; i < components.Count; i++)
		{
			components[i].Update();
		}
	}

	protected virtual void FixedUpdate()
	{
		for (int i = 0; i < components.Count; i++)
		{
			components[i].FixedUpdate();
		}
	}

	protected virtual void OnDisable()
	{
		for (int i = 0; i < components.Count; i++)
		{
			components[i].OnDisable();
		}
	}

	public virtual void CacheComponents()
	{
		components.Clear();
		Animator component = GetComponent<Animator>();
		if (animator == null && component != null)
		{
			animator = new AnimatorTimeline(timeline, component);
			animator.Initialize();
			components.Add(animator);
		}
		else if (animator != null && component == null)
		{
			animator = null;
		}
		Animation component2 = GetComponent<Animation>();
		if (_animation == null && component2 != null)
		{
			_animation = new AnimationTimeline(timeline, component2);
			_animation.Initialize();
			components.Add(_animation);
		}
		else if (_animation != null && component2 == null)
		{
			_animation = null;
		}
		AudioSource[] array = GetComponents<AudioSource>();
		for (int i = 0; i < audioSources.Count; i++)
		{
			AudioSourceTimeline audioSourceTimeline = audioSources[i];
			bool flag = false;
			foreach (AudioSource audioSource in array)
			{
				if (audioSourceTimeline.component == audioSource)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				audioSources.Remove(audioSourceTimeline);
			}
		}
		foreach (AudioSource audioSource2 in array)
		{
			bool flag2 = false;
			for (int l = 0; l < audioSources.Count; l++)
			{
				if (audioSources[l].component == audioSource2)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				AudioSourceTimeline audioSourceTimeline2 = new AudioSourceTimeline(timeline, audioSource2);
				audioSourceTimeline2.Initialize();
				audioSources.Add(audioSourceTimeline2);
				components.Add(audioSourceTimeline2);
			}
		}
		this.audioSource = ((audioSources.Count > 0) ? audioSources[0] : null);
		NavMeshAgent component3 = GetComponent<NavMeshAgent>();
		if (navMeshAgent == null && component3 != null)
		{
			navMeshAgent = new NavMeshAgentTimeline(timeline, component3);
			navMeshAgent.Initialize();
			components.Add(navMeshAgent);
		}
		else if (_animation != null && component3 == null)
		{
			navMeshAgent = null;
		}
		ParticleSystem component4 = GetComponent<ParticleSystem>();
		if (_particleSystem == null && component4 != null)
		{
			if (timeline.rewindable)
			{
				_particleSystem = new RewindableParticleSystemTimeline(timeline, component4);
				_particleSystem.Initialize();
			}
			else
			{
				_particleSystem = new NonRewindableParticleSystemTimeline(timeline, component4);
				_particleSystem.Initialize();
			}
			components.Add(_particleSystem);
		}
		else if (_particleSystem != null && component4 == null)
		{
			_particleSystem = null;
		}
		WindZone component5 = GetComponent<WindZone>();
		if (windZone == null && component5 != null)
		{
			windZone = new WindZoneTimeline(timeline, component5);
			windZone.Initialize();
			components.Add(windZone);
		}
		else if (windZone != null && component5 == null)
		{
			windZone = null;
		}
		Terrain component6 = GetComponent<Terrain>();
		if (terrain == null && component6 != null)
		{
			terrain = new TerrainTimeline(timeline, component6);
			terrain.Initialize();
			components.Add(terrain);
		}
		else if (terrain != null && component6 == null)
		{
			terrain = null;
		}
		TrailRenderer component7 = GetComponent<TrailRenderer>();
		if (trailRenderer == null && component7 != null)
		{
			trailRenderer = new TrailRendererTimeline(timeline, component7);
			trailRenderer.Initialize();
			components.Add(trailRenderer);
		}
		else if (trailRenderer != null && component7 == null)
		{
			trailRenderer = null;
		}
		WheelCollider component8 = GetComponent<WheelCollider>();
		if (wheelCollider == null && component8 != null)
		{
			wheelCollider = new WheelColliderTimeline(timeline, component8);
			wheelCollider.Initialize();
			components.Add(wheelCollider);
		}
		else if (wheelCollider != null && component8 == null)
		{
			wheelCollider = null;
		}
		Rigidbody component9 = GetComponent<Rigidbody>();
		Rigidbody2D component10 = GetComponent<Rigidbody2D>();
		Transform component11 = GetComponent<Transform>();
		if (_rigidbody == null && component9 != null)
		{
			_rigidbody = new RigidbodyTimeline3D(timeline, component9);
			_rigidbody.Initialize();
			components.Add(_rigidbody);
			_rigidbody2D = null;
			transform = null;
		}
		else if (_rigidbody2D == null && component10 != null)
		{
			_rigidbody2D = new RigidbodyTimeline2D(timeline, component10);
			_rigidbody2D.Initialize();
			components.Add(_rigidbody2D);
			_rigidbody = null;
			transform = null;
		}
		else if (transform == null)
		{
			transform = new TransformTimeline(timeline, component11);
			transform.Initialize();
			components.Add(transform);
			_rigidbody = null;
			_rigidbody2D = null;
		}
	}

	public virtual void Reset()
	{
		for (int i = 0; i < components.Count; i++)
		{
			components[i].Reset();
		}
	}

	public void ResetRecording()
	{
		recorder.Reset();
	}

	public int EstimateMemoryUsage()
	{
		if (Application.isPlaying)
		{
			if (recorder == null)
			{
				return 0;
			}
			return recorder.EstimateMemoryUsage();
		}
		Timeline timeline = GetComponent<Timeline>() ?? GetComponentInParent<Timeline>();
		if (!timeline.rewindable)
		{
			return 0;
		}
		if (GetComponent<Rigidbody>() != null)
		{
			return RecorderTimeline<Rigidbody, RigidbodyTimeline3D.Snapshot>.EstimateMemoryUsage(timeline.recordingDuration, recordingInterval);
		}
		if (GetComponent<Rigidbody2D>() != null)
		{
			return RecorderTimeline<Rigidbody2D, RigidbodyTimeline2D.Snapshot>.EstimateMemoryUsage(timeline.recordingDuration, recordingInterval);
		}
		if (GetComponent<Transform>() != null)
		{
			return RecorderTimeline<Transform, TransformTimeline.Snapshot>.EstimateMemoryUsage(timeline.recordingDuration, recordingInterval);
		}
		return 0;
	}
}
