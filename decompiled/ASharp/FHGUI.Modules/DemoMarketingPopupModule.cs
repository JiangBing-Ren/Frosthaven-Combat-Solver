using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Video;

namespace FHGUI.Modules;

public class DemoMarketingPopupModule : FHMapViewModule
{
	public struct Params
	{
		public Action OnClose;

		public Action OnAddToWishList;
	}

	[Header("Components")]
	[SerializeField]
	private VideoPlayer m_HeaderVideo;

	[SerializeField]
	private FHButton m_AddToWishlist;

	[SerializeField]
	private FHButton m_Close;

	[Header("Components")]
	[SerializeField]
	private float ShowAudioFadeDuration = 1.6f;

	[SerializeField]
	private float ShowPlayDelay = 1.6f;

	private void SyncAudioVolume(VideoPlayer player)
	{
		float endValue = ((SaveData.Instance != null) ? ((float)SaveData.Instance.Global.StoryVolume / 100f * (float)SaveData.Instance.Global.MasterVolume / 100f) : 1f);
		DOTween.To(() => player.GetDirectAudioVolume(0), (float value) =>
		{
			player.SetDirectAudioVolume(0, value);
		}, endValue, ShowAudioFadeDuration);
	}

	public void Init(Params @params)
	{
		m_AddToWishlist.RemoveAllEventListeners();
		m_AddToWishlist.onClick.AddListener(() =>
		{
			@params.OnAddToWishList?.Invoke();
		});
		m_Close.RemoveAllEventListeners();
		m_Close.onClick.AddListener(() =>
		{
			@params.OnClose?.Invoke();
		});
	}

	protected override void OnShow()
	{
		base.OnShow();
		m_HeaderVideo.SetDirectAudioVolume(0, 0f);
		m_HeaderVideo.started += SyncAudioVolume;
		m_HeaderVideo.loopPointReached += SyncAudioVolume;
		StartCoroutine(PlayVideo());
	}

	private IEnumerator PlayVideo()
	{
		m_HeaderVideo.Play();
		m_HeaderVideo.Pause();
		yield return new WaitForSeconds(ShowPlayDelay);
		m_HeaderVideo.Play();
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_HeaderVideo.started -= SyncAudioVolume;
		m_HeaderVideo.loopPointReached -= SyncAudioVolume;
		DOTween.To(() => m_HeaderVideo.GetDirectAudioVolume(0), (float value) =>
		{
			m_HeaderVideo.SetDirectAudioVolume(0, value);
		}, 0f, 0.2f);
	}
}
