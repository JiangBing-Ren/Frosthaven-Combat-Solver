using UnityEngine;

namespace Chronos;

public class AnimatorTimeline : ComponentTimeline<Animator>
{
	private float _speed;

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
			AdjustProperties();
		}
	}

	private int recordedFrames => Mathf.Clamp((int)(timeline.recordingDuration * 60f), 1, 10000);

	public AnimatorTimeline(Timeline timeline, Animator component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Animator source)
	{
		_speed = source.speed;
	}

	public override void AdjustProperties(float timeScale)
	{
		if (timeScale > 0f)
		{
			component.speed = speed * timeScale;
		}
		else
		{
			component.speed = 0f;
		}
	}

	public override void OnStartOrReEnable()
	{
		if (timeline.rewindable)
		{
			component.StartRecording(recordedFrames);
		}
	}

	public override void OnDisable()
	{
		if (timeline.rewindable)
		{
			component.StopRecording();
		}
	}

	public override void Update()
	{
		if (!timeline.rewindable)
		{
			return;
		}
		float timeScale = timeline.timeScale;
		if (timeline.lastTimeScale >= 0f && timeScale < 0f)
		{
			component.StopRecording();
			if (component.recorderStartTime < 0f)
			{
				Debug.LogWarning("Animator timeline failed to record for unknown reasons.\nSee: http://forum.unity3d.com/threads/341203/", component);
				return;
			}
			component.StartPlayback();
			component.playbackTime = component.recorderStopTime;
		}
		else if (component.recorderMode == AnimatorRecorderMode.Playback && timeScale > 0f)
		{
			component.StopPlayback();
			component.StartRecording(recordedFrames);
		}
		else if (timeScale < 0f && component.recorderMode == AnimatorRecorderMode.Playback)
		{
			float playbackTime = Mathf.Max(component.recorderStartTime, component.playbackTime + timeline.deltaTime);
			component.playbackTime = playbackTime;
		}
	}
}
