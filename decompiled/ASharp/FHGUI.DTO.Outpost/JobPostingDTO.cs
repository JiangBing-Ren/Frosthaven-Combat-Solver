using System;
using FHRL;

namespace FHGUI.DTO.Outpost;

public struct JobPostingDTO : IEquatable<JobPostingDTO>
{
	public enum JobState
	{
		None,
		Accepted,
		Completed
	}

	public CampaignSectionID ID;

	public string Name;

	public string Key;

	public string Description;

	public JobState State;

	public bool IsCompleted => State == JobState.Completed;

	public bool IsAccepted => State == JobState.Accepted;

	public bool Equals(JobPostingDTO other)
	{
		return ID.Equals(other.ID);
	}

	public override bool Equals(object obj)
	{
		if (obj is JobPostingDTO other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}
}
