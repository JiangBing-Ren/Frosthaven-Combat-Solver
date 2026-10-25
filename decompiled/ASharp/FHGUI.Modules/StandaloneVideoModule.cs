using System;
using FHGUI.Common.Elements;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class StandaloneVideoModule : FHMapViewModule
{
	public struct Params
	{
		public string URL;

		public bool Loop;

		public bool AutoAdvance;

		public bool Fullscreen;

		public bool ShowSkip;

		public Action OnDone;
	}

	[Header("Components")]
	[SerializeField]
	private UIVideoElement m_Video;

	[Header("Wide Screen")]
	[SerializeField]
	private FHButton m_CloseButton;

	[SerializeField]
	private GameObject m_ContainerFullscreen;

	[SerializeField]
	private RectTransform m_ViewParentFullscreen;

	[SerializeField]
	private GameObject[] m_WideScreeOrnaments;

	[SerializeField]
	private UIHoldToSkipElement m_SkipElement;

	[Header("Modal")]
	[SerializeField]
	private GameObject m_ContainerModal;

	[SerializeField]
	private RectTransform m_ViewParentModal;

	[Header("Settings")]
	[SerializeField]
	private float m_WideScreenThresholdRatio = 1.7777778f;

	protected override bool PlaySoundOnEnable => false;

	public void Init(Params @params)
	{
		m_Video.Init(new UIVideoElement.Params
		{
			URL = @params.URL,
			Volume = (float)SaveData.Instance.Global.StoryVolume / 100f * (float)SaveData.Instance.Global.MasterVolume / 100f,
			Loop = @params.Loop,
			OnComplete = ((@params.AutoAdvance && !@params.Loop) ? @params.OnDone : null)
		});
		m_CloseButton.onClick.RemoveAllListeners();
		m_CloseButton.onClick.AddListener(() =>
		{
			@params.OnDone?.Invoke();
		});
		m_ContainerFullscreen.SetActive(@params.Fullscreen);
		m_ContainerModal.SetActive(!@params.Fullscreen);
		if (@params.Fullscreen)
		{
			bool active = (float)Screen.width / (float)Screen.height > m_WideScreenThresholdRatio;
			GameObject[] wideScreeOrnaments = m_WideScreeOrnaments;
			for (int num = 0; num < wideScreeOrnaments.Length; num++)
			{
				wideScreeOrnaments[num].SetActive(active);
			}
			m_Video.transform.parent = m_ViewParentFullscreen;
		}
		else
		{
			m_Video.transform.parent = m_ViewParentModal;
		}
		if ((bool)m_SkipElement)
		{
			m_SkipElement.gameObject.SetActive(@params.ShowSkip);
			if (@params.ShowSkip)
			{
				m_SkipElement.Init(@params.OnDone);
			}
		}
	}
}
