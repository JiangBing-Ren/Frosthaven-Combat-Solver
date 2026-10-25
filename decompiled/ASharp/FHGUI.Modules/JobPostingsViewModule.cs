using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Elements.JobPostings;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class JobPostingsViewModule : FHMapViewModule
{
	[SerializeField]
	private RectTransform _activeJobsSection;

	[SerializeField]
	private JobPostingListContainer _activeJobs;

	[SerializeField]
	private RectTransform _acceptedJobsSection;

	[SerializeField]
	private JobPostingListContainer _acceptedJobs;

	[SerializeField]
	private RectTransform _completedJobsSection;

	[SerializeField]
	private JobPostingListContainer _completedJobs;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private UIJobPostingInfo _info;

	private List<JobPostingDTO> _activeJobsList = new List<JobPostingDTO>(20);

	private List<JobPostingDTO> _acceptedJobsList = new List<JobPostingDTO>(20);

	private List<JobPostingDTO> _completedJobsList = new List<JobPostingDTO>(20);

	private CampaignSectionID? _selected;

	private UIJobPostingRow _selectedRow;

	private UIJobPostingInfo.JobPostingContext _context;

	private Action _onCloseCallback;

	public void Init(IReadOnlyList<JobPostingDTO> jobs, UIJobPostingInfo.JobPostingContext context, Action onClose = null)
	{
		_context = context;
		_onCloseCallback = onClose;
		_activeJobsList.Clear();
		_acceptedJobsList.Clear();
		_completedJobsList.Clear();
		IOrderedEnumerable<JobPostingDTO> source = jobs.OrderBy((JobPostingDTO j) => j.State);
		_activeJobsList.AddRange(source.Where((JobPostingDTO j) => !j.IsCompleted && !j.IsAccepted));
		_acceptedJobsList.AddRange(source.Where((JobPostingDTO j) => !j.IsCompleted && j.IsAccepted));
		_completedJobsList.AddRange(source.Where((JobPostingDTO j) => j.IsCompleted && !j.IsAccepted));
		if (_activeJobsList.Count == 0)
		{
			_activeJobsSection.gameObject.SetActive(value: false);
		}
		else
		{
			_activeJobsSection.gameObject.SetActive(value: true);
			_activeJobs.Init(OnJobSelected, _activeJobsList);
		}
		if (_acceptedJobsList.Count == 0)
		{
			_acceptedJobsSection.gameObject.SetActive(value: false);
		}
		else
		{
			_acceptedJobsSection.gameObject.SetActive(value: true);
			_acceptedJobs.Init(OnJobSelected, _acceptedJobsList);
		}
		if (_completedJobsList.Count == 0)
		{
			_completedJobsSection.gameObject.SetActive(value: false);
		}
		else
		{
			_completedJobsSection.gameObject.SetActive(value: true);
			_completedJobs.Init(OnJobSelected, _completedJobsList);
		}
		if (_selected.HasValue)
		{
			_selectedRow = GetRowByID(_selected.Value);
			if (_selectedRow != null)
			{
				_selectedRow.Init(GetDataByID(_selected.Value), OnJobSelected, isSelected: true);
				_scroll.ScrollVerticallyTo(_selectedRow.transform as RectTransform);
			}
			CampaignSectionID iD = _info.Data.ID;
			CampaignSectionID? selected = _selected;
			if (iD == selected)
			{
				_info.UpdateJobState(_selectedRow.Data, _context);
			}
			else
			{
				_info.Init(_selectedRow.Data, _context);
			}
		}
		else
		{
			SelectFirst();
		}
	}

	private void SelectFirst()
	{
		UIJobPostingRow uIJobPostingRow = _activeJobs.Elements.FirstOrDefault() ?? _acceptedJobs.Elements.FirstOrDefault() ?? _completedJobs.Elements.FirstOrDefault();
		if (uIJobPostingRow == null)
		{
			_info.SetEmpty();
			return;
		}
		_scroll.ScrollToTop();
		OnJobSelected(uIJobPostingRow);
		uIJobPostingRow.Init(uIJobPostingRow.Data, OnJobSelected, isSelected: true);
	}

	public void Close()
	{
		_info.Cleanup();
		_onCloseCallback?.Invoke();
		gameObject.SetActive(value: false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_selected = null;
		_selectedRow = null;
		_context = null;
		_onCloseCallback = null;
	}

	private JobPostingDTO GetDataByID(CampaignSectionID id)
	{
		int num = _activeJobsList.FindIndex((JobPostingDTO j) => j.ID == id);
		if (num != -1)
		{
			return _activeJobsList[num];
		}
		num = _acceptedJobsList.FindIndex((JobPostingDTO j) => j.ID == id);
		if (num != -1)
		{
			return _acceptedJobsList[num];
		}
		return _completedJobsList.First((JobPostingDTO j) => j.ID == id);
	}

	private UIJobPostingRow GetRowByID(CampaignSectionID id)
	{
		return _activeJobs.Elements.FirstOrDefault((UIJobPostingRow j) => j.Data.ID == id) ?? _acceptedJobs.Elements.FirstOrDefault((UIJobPostingRow j) => j.Data.ID == id) ?? _completedJobs.Elements.FirstOrDefault((UIJobPostingRow j) => j.Data.ID == id);
	}

	private void OnJobSelected(UIJobPostingRow row)
	{
		CampaignSectionID iD = row.Data.ID;
		CampaignSectionID? selected = _selected;
		if (!(iD == selected))
		{
			_selected = row.Data.ID;
			if (_selectedRow != null)
			{
				_selectedRow.Init(_selectedRow.Data, OnJobSelected);
			}
			_selectedRow = row;
			_info.Init(row.Data, _context);
		}
	}
}
