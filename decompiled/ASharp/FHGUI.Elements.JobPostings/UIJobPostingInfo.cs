using System;
using FFHGUI.Elements;
using FHGUI.DTO.Outpost;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.JobPostings;

public class UIJobPostingInfo : MonoBehaviour
{
	public class JobPostingContext
	{
		public bool IsHost;

		public Action<CampaignSectionID> AcceptJob;
	}

	[SerializeField]
	private TMP_Text _title;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private RectTransform _textContainer;

	[SerializeField]
	private FHToggle _playButton;

	[SerializeField]
	private UINarrationComponent _narration;

	[SerializeField]
	private FHButton _acceptJobButton;

	private JobPostingDTO _jobPosting;

	private JobPostingContext _context;

	public JobPostingDTO Data => _jobPosting;

	private void Awake()
	{
		_playButton.onValueChanged.AddListener(ToggleNarration);
		_acceptJobButton.onClick.AddListener(AcceptJob);
	}

	public void Init(JobPostingDTO jobPosting, JobPostingContext context)
	{
		gameObject.SetActive(value: true);
		_context = context;
		_jobPosting = jobPosting;
		_textContainer.gameObject.SetActive(value: true);
		_title.text = jobPosting.Name;
		_description.text = jobPosting.Description;
		_scroll.ScrollToTop();
		UpdateJobState(jobPosting, context);
		ResetNarration();
		_playButton.SetTooltipText(LocalizationManager.GetTranslation("GUI_NARRATION_PAUSE"), LocalizationManager.GetTranslation("GUI_NARRATION_PLAY"), string.Empty);
		_narration.Init(_jobPosting.Key, new UINarrationComponent.NarrationAssetBundleParams
		{
			AssetBundleDirName = "eventnarrations",
			AssetBundleName = "misc_eventnarrations"
		}, loadSFX: true, autoPlay: false, useGlobalMute: false, null, OnNarrationEnd);
	}

	private void OnNarrationEnd()
	{
		_playButton.SetIsOnWithoutNotify(value: false);
		_scroll.verticalNormalizedPosition = 0f;
	}

	public void UpdateJobState(JobPostingDTO jobPosting, JobPostingContext context)
	{
		_acceptJobButton.gameObject.SetActive(value: true);
		_acceptJobButton.gameObject.SetActive(jobPosting.State == JobPostingDTO.JobState.None);
		_acceptJobButton.interactable = context.IsHost;
	}

	public void SetEmpty()
	{
		_textContainer.gameObject.SetActive(value: false);
		_acceptJobButton.gameObject.SetActive(value: false);
	}

	private void AcceptJob()
	{
		_context?.AcceptJob?.Invoke(_jobPosting.ID);
	}

	private void OnDisable()
	{
		Cleanup();
	}

	public void Cleanup()
	{
		_narration.Cleanup();
		_context = null;
	}

	private void ResetNarration()
	{
		_narration.Cleanup();
		_playButton.SetIsOnWithoutNotify(value: false);
	}

	private void ToggleNarration(bool active)
	{
		if (active)
		{
			if (_narration.HasEnded)
			{
				_narration.RestartNarration();
			}
			else if (_narration.Progress > 0f)
			{
				_narration.UnPause();
			}
			else
			{
				_narration.PlayNarration();
			}
		}
		else if (_narration.Progress > 0f)
		{
			_narration.Pause();
		}
	}
}
