using System;
using System.Collections.Generic;
using UnityEngine;

namespace FHGUI;

public class FHMapViewModule : MonoBehaviour, IUIViewModule
{
	public RectTransform HighlightRect;

	[SerializeField]
	protected AudioMapViewModuleProfile _audioProfile;

	public GameObject GameObject => gameObject;

	private AudioMapViewModuleProfile AudioProfile
	{
		get
		{
			if (_audioProfile != null)
			{
				return _audioProfile;
			}
			if (!(UIInfoTools.Instance != null))
			{
				return null;
			}
			return UIInfoTools.Instance.defaultAudioMapViewModuleProfile;
		}
	}

	protected virtual bool PlaySoundOnEnable => true;

	public virtual IEnumerable<IUIViewModule> GetDependencies()
	{
		return Array.Empty<IUIViewModule>();
	}

	private void OnEnable()
	{
		OnShow();
	}

	private void OnDisable()
	{
		OnHide();
	}

	protected virtual void OnShow()
	{
		if (PlaySoundOnEnable)
		{
			PlayShowSound();
		}
	}

	protected virtual void OnHide()
	{
		if (PlaySoundOnEnable)
		{
			PlayHideSound();
		}
	}

	protected void PlayShowSound()
	{
		AudioMapViewModuleProfile audioProfile = AudioProfile;
		if (audioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(audioProfile.OnShow);
		}
	}

	protected void PlayHideSound()
	{
		AudioMapViewModuleProfile audioProfile = AudioProfile;
		if (audioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(audioProfile.OnHide);
		}
	}

	protected virtual void OnDataUpdate()
	{
	}
}
