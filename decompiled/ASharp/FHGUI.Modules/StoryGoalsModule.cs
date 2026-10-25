using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using Libs.Utils;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class StoryGoalsModule : FHMapViewModule
{
	public struct Params
	{
		public PuzzleDTO Puzzle;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private TMP_Text _goalDescription;

	[SerializeField]
	private RectTransform _goalContainer;

	[SerializeField]
	private RectTransform _dataContainer;

	[Header("Prefabs")]
	[SerializeField]
	private UIPersonalQuestStepProgress _checkboxStepPrefab;

	private Params? _data;

	protected override bool PlaySoundOnEnable => false;

	public void Init(Params @params)
	{
		if (@params.Puzzle == null)
		{
			_dataContainer.gameObject.SetActive(value: false);
			return;
		}
		if (@params.Puzzle.IsDisabled)
		{
			if ((bool)_name)
			{
				_name.text = string.Empty;
			}
			if ((bool)_description)
			{
				_description.text = string.Empty;
			}
			if ((bool)_goalDescription)
			{
				_goalDescription.text = string.Empty;
			}
			_goalContainer.DestroyChildren();
			CreateQuestStep(new PersonalQuestStepDTO
			{
				PQName = LocalizationManager.GetTranslation("GUI_COMING_SOON"),
				IsCheckbox = true,
				IsComplete = false
			});
			return;
		}
		_dataContainer.gameObject.SetActive(value: true);
		_data = @params;
		PuzzleDTO puzzle = @params.Puzzle;
		if ((bool)_name)
		{
			_name.text = puzzle.Name;
		}
		if ((bool)_description)
		{
			_description.text = puzzle.Description;
		}
		if ((bool)_goalDescription)
		{
			_goalDescription.text = puzzle.RewardText;
		}
		_goalContainer.DestroyChildren();
		if (puzzle.VisibleSteps == null || !puzzle.VisibleSteps.Any())
		{
			return;
		}
		foreach (PersonalQuestStepDTO visibleStep in puzzle.VisibleSteps)
		{
			CreateQuestStep(visibleStep);
		}
		void CreateQuestStep(PersonalQuestStepDTO step)
		{
			((PersonalQuestStepUI)Object.Instantiate((Behaviour)_checkboxStepPrefab, (Transform)_goalContainer)).Init(step);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		_data = null;
	}
}
