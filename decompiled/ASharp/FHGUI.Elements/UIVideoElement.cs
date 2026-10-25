using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FHGUI.Elements;

[RequireComponent(typeof(VideoPlayer))]
public class UIVideoElement : RawImage
{
	public struct Params
	{
		public string URL;

		public float Volume;

		public bool Loop;

		public Action OnComplete;
	}

	[Header("Settings")]
	[SerializeField]
	private Color PrepairColor;

	[SerializeField]
	private Color PlayingColor;

	[SerializeField]
	private float fadeInDuration = 0.3f;

	private VideoPlayer m_Source;

	private bool m_Initialized;

	public Params Data { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		InitCache();
	}

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Source = GetComponent<VideoPlayer>();
			m_Source.prepareCompleted += SourceOnPrepareCompleted;
			m_Source.loopPointReached += LoopPointReached;
			color = PrepairColor;
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		color = PrepairColor;
		Data = @params;
		m_Source.url = @params.URL;
		m_Source.Prepare();
	}

	private void SourceOnPrepareCompleted(VideoPlayer source)
	{
		texture = source.texture;
		this.DOColor(PlayingColor, fadeInDuration);
		SetVolume(Data.Volume);
	}

	private void LoopPointReached(VideoPlayer source)
	{
		if (!Data.Loop)
		{
			Data.OnComplete?.Invoke();
		}
	}

	public void SetVolume(float volume, int trackIndex = -1)
	{
		for (ushort num = 0; num < m_Source.audioTrackCount; num++)
		{
			if (trackIndex == -1 || num == trackIndex)
			{
				m_Source.SetDirectAudioVolume(num, volume);
			}
		}
	}
}
