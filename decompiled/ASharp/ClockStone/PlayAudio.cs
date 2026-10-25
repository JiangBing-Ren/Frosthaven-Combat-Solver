using UnityEngine;

namespace ClockStone;

public class PlayAudio : AudioTriggerBase
{
	public enum PlayPosition
	{
		Global,
		ChildObject,
		ObjectPosition
	}

	public enum SoundType
	{
		SFX,
		Music,
		AmbienceSound
	}

	public string audioID;

	public SoundType soundType;

	public PlayPosition position;

	public float volume = 1f;

	public float delay;

	public float startTime;

	protected override void Awake()
	{
		if (triggerEvent == EventType.OnDestroy && position == PlayPosition.ChildObject)
		{
			position = PlayPosition.ObjectPosition;
			Debug.LogWarning("OnDestroy event can not be used with ChildObject");
		}
		base.Awake();
	}

	private void _Play()
	{
		switch (position)
		{
		case PlayPosition.Global:
			AudioController.Play(audioID, volume, delay, startTime);
			break;
		case PlayPosition.ChildObject:
			AudioController.Play(audioID, transform, volume, delay, startTime);
			break;
		case PlayPosition.ObjectPosition:
			AudioController.Play(audioID, transform.position, null, volume, delay, startTime);
			break;
		}
	}

	protected override void _OnEventTriggered()
	{
		if (!string.IsNullOrEmpty(audioID))
		{
			switch (soundType)
			{
			case SoundType.SFX:
				_Play();
				break;
			case SoundType.Music:
				_PlayMusic();
				break;
			case SoundType.AmbienceSound:
				_PlayAmbienceSound();
				break;
			}
		}
	}

	private void _PlayMusic()
	{
		switch (position)
		{
		case PlayPosition.Global:
			AudioController.PlayMusic(audioID, volume, delay, startTime);
			break;
		case PlayPosition.ChildObject:
			AudioController.PlayMusic(audioID, transform, volume, delay, startTime);
			break;
		case PlayPosition.ObjectPosition:
			AudioController.PlayMusic(audioID, transform.position, null, volume, delay, startTime);
			break;
		}
	}

	private void _PlayAmbienceSound()
	{
		switch (position)
		{
		case PlayPosition.Global:
			AudioController.PlayAmbienceSound(audioID, volume, delay, startTime);
			break;
		case PlayPosition.ChildObject:
			AudioController.PlayAmbienceSound(audioID, transform, volume, delay, startTime);
			break;
		case PlayPosition.ObjectPosition:
			AudioController.PlayAmbienceSound(audioID, transform.position, null, volume, delay, startTime);
			break;
		}
	}
}
