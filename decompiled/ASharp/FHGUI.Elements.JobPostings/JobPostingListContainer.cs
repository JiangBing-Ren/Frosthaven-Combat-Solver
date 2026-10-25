using System;
using FHGUI.DTO.Outpost;

namespace FHGUI.Elements.JobPostings;

public class JobPostingListContainer : ListContainerBase<UIJobPostingRow, Action<UIJobPostingRow>, JobPostingDTO>
{
	protected override void InitElement(UIJobPostingRow row, JobPostingDTO elementParams, Action<UIJobPostingRow> onSelect)
	{
		row.Init(elementParams, onSelect);
	}
}
