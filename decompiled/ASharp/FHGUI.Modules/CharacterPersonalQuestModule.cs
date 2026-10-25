using System;
using FHGUI.DTO;
using FHGUI.Elements;
using Libs.Utils;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class CharacterPersonalQuestModule : FHMapViewModule
{
	public struct Params
	{
		public CharacterPersonalQuestDTO PersonalQuest;

		public bool CanRetire;

		public bool HasAuthorityToRetire;

		public Action RetireCallback;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private TMP_Text _goalDescription;

	[SerializeField]
	private GameObject _readyToRetireHint;

	[SerializeField]
	private RectTransform _goalContainer;

	[SerializeField]
	private RectTransform _dataContainer;

	[SerializeField]
	private FHButton _retire;

	[Header("Prefabs")]
	[SerializeField]
	private UIPersonalQuestStepProgress _progressStepPrefab;

	[SerializeField]
	private UIPersonalQuestStepCheckbox _checkboxStepPrefab;

	[SerializeField]
	private UIPersonalQuestStepIcon _iconStepPrefab;

	private Params? _data;

	protected override bool PlaySoundOnEnable => false;

	private void Awake()
	{
		if ((bool)_retire)
		{
			_retire.onClick.AddListener(OnRetireClick);
		}
	}

	public void Init(Params @params)
	{
		if (@params.PersonalQuest == null)
		{
			_dataContainer.gameObject.SetActive(value: false);
			return;
		}
		_dataContainer.gameObject.SetActive(value: true);
		_data = @params;
		CharacterPersonalQuestDTO personalQuest = @params.PersonalQuest;
		if ((bool)_name)
		{
			_name.text = personalQuest.Name;
		}
		if ((bool)_description)
		{
			_description.text = personalQuest.Description;
		}
		if ((bool)_goalDescription)
		{
			_goalDescription.text = personalQuest.GoalDescription;
		}
		if ((bool)_readyToRetireHint)
		{
			_readyToRetireHint.gameObject.SetActive(@params.CanRetire);
		}
		_goalContainer.DestroyChildren();
		if (personalQuest.Steps == null)
		{
			return;
		}
		foreach (PersonalQuestStepDTO step in personalQuest.Steps)
		{
			CreateQuestStep(step);
		}
		if ((bool)_retire)
		{
			_retire.gameObject.SetActive(@params.CanRetire);
			_retire.interactable = @params.HasAuthorityToRetire;
		}
		void CreateQuestStep(PersonalQuestStepDTO step)
		{
			Behaviour original;
			if (step.IsCheckbox)
			{
				original = _checkboxStepPrefab;
			}
			else
			{
				original = ((!(step.Icon != null)) ? ((PersonalQuestStepUI)_progressStepPrefab) : ((PersonalQuestStepUI)_iconStepPrefab));
			}
			((PersonalQuestStepUI)UnityEngine.Object.Instantiate(original, _goalContainer)).Init(step);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		_data = null;
	}

	private void OnRetireClick()
	{
		if (_data.HasValue && _data.Value.CanRetire)
		{
			_data.Value.RetireCallback?.Invoke();
		}
	}
}
