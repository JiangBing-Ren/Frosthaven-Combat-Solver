using UnityEngine;

namespace FHGUI.DTO;

public struct PersonalQuestStepDTO
{
	public string PQName;

	public string Name;

	public string ProgressDesc;

	public Sprite Icon;

	public string Tooltip;

	public uint Current;

	public uint Max;

	public bool IsComplete;

	public bool IsCheckbox;

	public bool ShowIfCompleted;

	public float Progress
	{
		get
		{
			if (Max != 0)
			{
				return (float)Current / (float)Max;
			}
			return 1f;
		}
	}
}
