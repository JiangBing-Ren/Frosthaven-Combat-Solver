using UnityEngine;

namespace Chronos;

public class AudioSourceTimeline : ComponentTimeline<AudioSource>
{
	private float _pitch;

	public float pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = value;
			AdjustProperties();
		}
	}

	public AudioSourceTimeline(Timeline timeline, AudioSource component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(AudioSource source)
	{
		_pitch = source.pitch;
	}

	public override void AdjustProperties(float timeScale)
	{
		component.pitch = pitch * timeScale;
	}
}
