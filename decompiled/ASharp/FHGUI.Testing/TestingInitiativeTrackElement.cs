using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestingInitiativeTrackElement : MonoBehaviour
{
	[Header("Components")]
	public InitiativeTrackPlayerBehaviourExtension BehaviourPlayer;

	public InitiativeTrackEnemyBehaviourExtension BehaviourEnemy;

	public InitiativeTrackSummonBehaviourExtension BehaviourSummon;

	public TMP_Dropdown DropdownTypes;

	public TMP_Dropdown DropdownClasses;

	public Button UpdateButton;

	[Header("Params")]
	public TMP_InputField TextInitiative;

	public TMP_InputField CurrentHealth;

	public TMP_InputField MaxHealth;

	public TMP_InputField SummonsCount;

	public Toggle IsFactionDisplayed;

	public Toggle IsActing;

	public Toggle IsTargeted;

	public Toggle IsTargetedByPositiveAbility;

	public Toggle ShowNextCard;

	public TMP_Dropdown PhaseState;

	private readonly List<string> _types = new List<string> { "Hero", "NPC", "Summon" };

	private IReadOnlyList<CClass> _cClasses;

	private List<string> _phases;

	private List<InitiativeTrackActorBehaviourBase> _elements;

	private void Start()
	{
		_elements = new List<InitiativeTrackActorBehaviourBase> { BehaviourPlayer.Base, BehaviourEnemy.Base, BehaviourSummon.Base };
		WireStaticFields();
		Init();
	}

	private void WireDropdownNextButton(TMP_Dropdown dropdown)
	{
		Button button = dropdown.transform.parent.GetComponentsInChildren<Button>().FirstOrDefault((Button c) => c.name.Contains("Next"));
		if (button == null)
		{
			Debug.LogError("Could not find 'Next' button near dropdown [" + dropdown.name + "]!", dropdown);
			return;
		}
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() =>
		{
			int num = dropdown.value + 1;
			if (num >= dropdown.options.Count)
			{
				num = 0;
			}
			dropdown.value = num;
		});
	}

	private void WireStaticFields()
	{
		WireDropdownNextButton(DropdownTypes);
		WireDropdownNextButton(DropdownClasses);
		WireDropdownNextButton(PhaseState);
		DropdownTypes.ClearOptions();
		DropdownTypes.AddOptions(_types);
		DropdownTypes.onValueChanged.AddListener((int _) =>
		{
			Init();
		});
		InitInputField(TextInitiative, TMP_InputField.ContentType.Standard);
		InitInputField(CurrentHealth, TMP_InputField.ContentType.IntegerNumber);
		InitInputField(MaxHealth, TMP_InputField.ContentType.IntegerNumber);
		InitInputField(SummonsCount, TMP_InputField.ContentType.IntegerNumber);
		InitToggle(IsFactionDisplayed, startingValue: false);
		InitToggle(IsActing, startingValue: false);
		InitToggle(IsTargeted, startingValue: false);
		InitToggle(IsTargetedByPositiveAbility, startingValue: false);
		InitToggle(ShowNextCard, startingValue: false);
		IsTargeted.onValueChanged.AddListener(SetPositiveToggleVisibility);
		SetPositiveToggleVisibility(isOn: false);
		_phases = Enum.GetNames(typeof(InitiativeTrack.ActorPhaseState)).ToList();
		PhaseState.onValueChanged.RemoveAllListeners();
		PhaseState.ClearOptions();
		PhaseState.AddOptions(_phases);
		PhaseState.onValueChanged.AddListener((int _) =>
		{
			UpdateElement();
		});
		UpdateButton.onClick.RemoveAllListeners();
		UpdateButton.onClick.AddListener(UpdateElement);
	}

	private void Init()
	{
		_cClasses = DropdownTypes.value switch
		{
			0 => (IReadOnlyList<CClass>)CharacterClassManager.Classes, 
			1 => MonsterClassManager.Classes, 
			2 => CharacterClassManager.HeroSummonClasses, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		DropdownClasses.onValueChanged.RemoveAllListeners();
		DropdownClasses.ClearOptions();
		DropdownClasses.AddOptions(_cClasses.Select((CClass c) => c.DefaultModel.ToString()).ToList());
		DropdownClasses.onValueChanged.AddListener((int _) =>
		{
			UpdateElement();
		});
		UpdateElement();
	}

	private void SetPositiveToggleVisibility(bool isOn)
	{
		IsTargetedByPositiveAbility.gameObject.SetActive(isOn);
	}

	private void UpdateElement()
	{
		InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = _elements[DropdownTypes.value];
		foreach (InitiativeTrackActorBehaviourBase element in _elements)
		{
			element.gameObject.SetActive(element == initiativeTrackActorBehaviourBase);
		}
		InitiativeTrack.ActorPhaseState phaseState = (Enum.TryParse<InitiativeTrack.ActorPhaseState>(_phases[PhaseState.value], out var result) ? result : InitiativeTrack.ActorPhaseState.None);
		InitiativeTrack.ActorAttributesParams attributesDirect = new InitiativeTrack.ActorAttributesParams
		{
			Actor = null,
			Class = _cClasses[DropdownClasses.value],
			TextInitiative = TextInitiative.text,
			CurrentHealth = int.Parse(CurrentHealth.text),
			MaxHealth = int.Parse(MaxHealth.text),
			SummonsCount = int.Parse(SummonsCount.text),
			IsActiveButton = false,
			IsFactionDisplayed = IsFactionDisplayed.isOn,
			IsActing = IsActing.isOn,
			IsTargeted = IsTargeted.isOn,
			IsTargetedByPositiveAbility = IsTargetedByPositiveAbility.isOn,
			ShowNextCard = ShowNextCard.isOn,
			PhaseState = phaseState
		};
		initiativeTrackActorBehaviourBase.SetAttributesDirect(attributesDirect);
	}

	private void InitInputField(TMP_InputField inputField, TMP_InputField.ContentType contentType)
	{
		inputField.onValueChanged.RemoveAllListeners();
		inputField.contentType = contentType;
		inputField.text = "10";
		inputField.onValueChanged.AddListener((string _) =>
		{
			UpdateElement();
		});
	}

	private void InitToggle(Toggle toggle, bool startingValue)
	{
		toggle.onValueChanged.RemoveAllListeners();
		toggle.isOn = startingValue;
		toggle.onValueChanged.AddListener((bool _) =>
		{
			UpdateElement();
		});
	}
}
