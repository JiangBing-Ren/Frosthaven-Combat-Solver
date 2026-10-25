using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class OutpostAttackModalModule : FHMapViewModule
{
	public struct Params
	{
		public OutpostAttackEventDTO EventData;

		public List<OutpostAttackEventSummeryEntryDTO> HistoryData;

		public List<AttackModifierDTO> TownGuardDeck;

		public int SelectedStep;

		public int SelectedModiferIndex;

		public uint SoldiersToSpend;

		public bool IsPartyLeader;

		public uint AvailableSoldiers;

		public bool CanRepair;

		public Action<uint> PerformAttackCallback;

		public Action NextActionCallback;

		public Action RepairBuildingWithMaterialsCallback;

		public Action RepairBuildingWithMoraleCallback;

		public Action<int> OnModifierChoiceCallback;

		public Action<int> OnModifierSelectedCallback;

		public Action<int> OnSlotPicked;

		public IEnumerable<FHInputEventElement.IInputEventData> ModifySoldiersActions;

		public bool HasOperationalBarracks;
	}

	public enum State
	{
		None,
		NewAttack,
		DrawModifiers,
		Repair,
		Outcome,
		History
	}

	[Header("Common Components")]
	[SerializeField]
	private UIOutpostAttackSoldierDistribution SoldierDistribution;

	[SerializeField]
	private UIOutpostAttackTownGuardModifiers TownGuardModifiers;

	[SerializeField]
	private TMP_Text AttackStep;

	[SerializeField]
	private Image BuildingIllustration;

	[SerializeField]
	private TMP_Text BuildingName;

	[Header("Stats")]
	[SerializeField]
	private TMP_Text DefenceValueText;

	[SerializeField]
	private TMP_Text AttackValueText;

	[SerializeField]
	private Image Icon_Advantage;

	[SerializeField]
	private Image Icon_Disadvantage;

	[SerializeField]
	private Image Icon_TotalValue;

	[SerializeField]
	private TMP_Text TotalValueNumeric;

	[SerializeField]
	private TMP_Text TotalValueSpecialOutcome;

	[SerializeField]
	private TMP_Text AdvantagePromptDescription;

	[SerializeField]
	private TextLocalizedListener TextAdvantagePromptDescription;

	[Header("Repair")]
	[SerializeField]
	private GameObject RepairInfoContainer;

	[SerializeField]
	private TMP_Text RepairInfoDescription;

	[Header("Outcome")]
	[SerializeField]
	private TMP_Text TextOutcome;

	[SerializeField]
	private Image IconOutcome;

	[Header("Buttons")]
	[SerializeField]
	private FHButton ActionButton;

	[SerializeField]
	private FHButton ChooseModifierButton;

	[SerializeField]
	private FHButton SelectCurrentButton;

	[SerializeField]
	private FHButton SpendMoraleButton;

	[SerializeField]
	private FHButton SpendMaterialsButton;

	[SerializeField]
	private FHButton DrawModifiersButton;

	[Header("History Slots")]
	[SerializeField]
	private Transform HistorySlotsContainer;

	[SerializeField]
	private UIOutpostAttackBuildingListElement HistorySlotPrototype;

	[SerializeField]
	private ScrollRect HistorySlotScrollRect;

	[Header("Tooltips")]
	[SerializeField]
	private UITextTooltipTarget AttackIconTooltip;

	[SerializeField]
	private UITextTooltipTarget DefenceIconTooltip;

	[Header("Sprites")]
	[SerializeField]
	private Sprite SpriteProtected;

	[SerializeField]
	private Sprite SpriteWrecked;

	[SerializeField]
	private Sprite SpriteDamaged;

	[SerializeField]
	private Sprite SpriteRepaired;

	[SerializeField]
	private Sprite SpritePositiveAttackValue;

	[SerializeField]
	private Sprite SpriteNegativeAttackValue;

	[Header("Color")]
	[SerializeField]
	private Color ColorOutcomeTextProtected;

	[SerializeField]
	private Color ColorOutcomeTextWrecked;

	[SerializeField]
	private Color ColorOutcomeTextDamaged;

	[SerializeField]
	private Color ColorOutcomeTextRepaired;

	private int SelectedStepIndex;

	private OutpostAttackEventSummeryEntryDTO SelectedSummery;

	private int _soundTargetIndex = -1;

	private State _soundLiveState;

	private List<UIOutpostAttackBuildingListElement> m_BuildingListElements = new List<UIOutpostAttackBuildingListElement>();

	public Action OnTransitionFinished;

	private bool m_Initialized;

	public bool InTransition => TownGuardModifiers?.InTransition ?? false;

	public Params Data { get; private set; }

	public State PrevState { get; private set; }

	private State CurrentState => CalcState();

	private int InProgressStepIndex => Data.EventData.CurrentTargetIndex;

	public int SelectedSummeryIndex
	{
		get
		{
			if (SelectedSummery != null)
			{
				return SelectedStepIndex;
			}
			return -1;
		}
	}

	public void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			ActionButton.onClick.AddListener(NextAction);
			SelectCurrentButton.onClick.AddListener(SelectCurrent);
			SpendMoraleButton.onClick.AddListener(RepairBuildingMorale);
			SpendMaterialsButton.onClick.AddListener(RepairBuildingMaterials);
			ChooseModifierButton.onClick.AddListener(ChooseModifer);
			DrawModifiersButton.onClick.AddListener(OnDrawModifiers);
			FHUILocFormatting.SetTextKey(SpendMaterialsButton, "GUI_REPAIR_SPENDMATERIAL");
			FHUILocFormatting.SetTextKey(SpendMoraleButton, "GUI_REPAIR_SPENDMORALE");
			FHUILocFormatting.SetTextKey(ChooseModifierButton, "GUI_SELECT");
			FHUILocFormatting.SetTextKey(DrawModifiersButton, "GUI_DRAW_MODIFIER");
			HistorySlotsContainer.GetComponentsInChildren(includeInactive: true, m_BuildingListElements);
			HistorySlotScrollRect.verticalNormalizedPosition = 0f;
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		PrevState = CurrentState;
		SelectedSummery = null;
		Data = @params;
		PlayOutcomeSounds();
		SelectHistoryStep(Data.SelectedStep);
		StartCoroutine(TrackTransition());
	}

	private void PlayOutcomeSounds()
	{
		int num = Data.EventData?.CurrentTargetIndex ?? (-1);
		State state = CalcLiveState();
		bool num2 = num == _soundTargetIndex && (_soundLiveState == State.NewAttack || _soundLiveState == State.DrawModifiers);
		bool flag = state == State.Repair || state == State.Outcome;
		_soundTargetIndex = num;
		_soundLiveState = state;
		if (!num2 || !flag)
		{
			return;
		}
		if (Data.EventData.IsSuccessfulDefend)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.outpostAttackSuccessAudioItem);
			return;
		}
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.outpostAttackDefeatAudioItem);
		if (Data.EventData.CurrentTargetBuilding.IsDamaged || Data.EventData.CurrentTargetBuilding.IsWrecked)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.outpostAttackBuildingDamageAudioItem);
		}
	}

	private void Rebuild()
	{
		UpdateInfo();
		UpdateRepairInfo();
		UpdateInteractions();
		UpdateBuildingSlots();
		UIOutpostAttackSoldierDistribution soldierDistribution = SoldierDistribution;
		UIOutpostAttackSoldierDistribution.Params obj = new UIOutpostAttackSoldierDistribution.Params
		{
			AvailableSoldiers = ((SelectedSummery == null) ? Data.AvailableSoldiers : SelectedSummery.SoldersAvailable),
			EventData = Data.EventData,
			SoldiersToSpend = ((SelectedSummery == null) ? Data.SoldiersToSpend : SelectedSummery.SoldersSpent),
			HasOperationalBarracks = Data.HasOperationalBarracks
		};
		IEnumerable<FHInputEventElement.IInputEventData> actions;
		if (SelectedSummery != null)
		{
			IEnumerable<FHInputEventElement.IInputEventData> enumerable = new List<FHInputEventElement.IInputEventData>(0);
			actions = enumerable;
		}
		else
		{
			actions = Data.ModifySoldiersActions;
		}
		obj.Actions = actions;
		soldierDistribution.Init(obj);
		TownGuardModifiers.Init(new UIOutpostAttackTownGuardModifiers.Params
		{
			SelectedSummery = SelectedSummery,
			EventData = Data.EventData,
			TownGuardDeck = Data.TownGuardDeck,
			OnModifierSelectedCallback = (Data.IsPartyLeader ? Data.OnModifierSelectedCallback : null),
			SelectedModiferIndex = Data.SelectedModiferIndex,
			PrevState = PrevState,
			CurrentState = CurrentState
		});
	}

	private void UpdateInfo()
	{
		bool flag = Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackFinished;
		bool flag2 = (SelectedSummery != null) | flag;
		bool flag3 = SelectedSummery?.Defended ?? Data.EventData.IsSuccessfulDefend;
		bool flag4 = SelectedSummery?.Wrecked ?? Data.EventData.CurrentTargetBuilding.IsWrecked;
		bool flag5 = SelectedSummery?.IsRepaired ?? (Data.EventData.CurrentTargetBuilding.IsRepaired && !flag3);
		bool flag6 = SelectedSummery == null && Data.EventData.CurrentTargetBuilding.IsDamaged;
		BuildingStateDTO buildingStateDTO = SelectedSummery?.Building ?? Data.EventData.CurrentTargetBuilding;
		if ((bool)BuildingIllustration)
		{
			BuildingIllustration.sprite = buildingStateDTO.Icon;
		}
		if (BuildingName != null)
		{
			BuildingName.text = LocalizationManager.GetTranslation(buildingStateDTO.NameLocKey);
		}
		if ((bool)TextOutcome)
		{
			TextOutcome.gameObject.SetActive(flag2);
		}
		if ((bool)IconOutcome)
		{
			IconOutcome.gameObject.SetActive(flag2);
		}
		if (flag2)
		{
			string term = "GUI_EMPTY";
			Color color = Color.white;
			Sprite sprite = null;
			if (flag3)
			{
				term = "GUI_PROTECTED";
				color = ColorOutcomeTextProtected;
				sprite = SpriteProtected;
			}
			if (!flag3 && !flag4 && !flag6 && !flag5)
			{
				term = "GUI_REPAIRED";
				color = ColorOutcomeTextRepaired;
				sprite = SpriteRepaired;
			}
			if (flag4)
			{
				term = "GUI_WRECKED";
				color = ColorOutcomeTextWrecked;
				sprite = SpriteWrecked;
			}
			if (flag6)
			{
				term = "GUI_DAMAGED";
				color = ColorOutcomeTextDamaged;
				sprite = SpriteDamaged;
			}
			if (flag5)
			{
				term = "GUI_REPAIRED";
				color = ColorOutcomeTextRepaired;
				sprite = SpriteRepaired;
			}
			TextOutcome.text = LocalizationManager.GetTranslation(term);
			TextOutcome.color = color;
			IconOutcome.sprite = sprite;
		}
		if ((bool)AttackValueText)
		{
			AttackValueText.text = ((SelectedSummery != null) ? SelectedSummery.Threat.ToString() : Data.EventData.ThreatValue.ToString());
		}
		if ((bool)DefenceValueText)
		{
			DefenceValueText.text = (SelectedSummery?.Defense ?? Data.EventData.TotalDefenseValue).ToString();
		}
		if ((bool)Icon_Advantage)
		{
			bool active = SelectedSummery?.Advantage ?? (Data.EventData.ActionType == TownGuardDrawCardActionType.Advantage);
			Icon_Advantage.gameObject.SetActive(active);
		}
		if ((bool)Icon_Disadvantage)
		{
			bool active2 = SelectedSummery?.Disadvantage ?? (Data.EventData.ActionType == TownGuardDrawCardActionType.Disatvantage);
			Icon_Disadvantage.gameObject.SetActive(active2);
		}
		bool flag7 = false;
		bool flag8 = false;
		int num;
		switch (CurrentState)
		{
		case State.History:
			num = SelectedSummery.FinalAttackValue;
			flag8 = SelectedSummery.Wrecked;
			flag7 = SelectedSummery.HasSuccessDrawn;
			break;
		case State.DrawModifiers:
			num = Data.EventData.TotalDefenceValue - Data.EventData.TotalAttackValue;
			if (Data.SelectedModiferIndex != -1)
			{
				AttackModifierDTO attackModifierDTO = Data.EventData.ToChooseCards[Data.SelectedModiferIndex];
				num += attackModifierDTO.Value;
				flag8 = attackModifierDTO.IsFailure;
				flag7 = attackModifierDTO.IsSuccess;
			}
			break;
		case State.Outcome:
		{
			OutpostAttackEventSummeryEntryDTO outpostAttackEventSummeryEntryDTO = Data.HistoryData.FirstOrDefault((OutpostAttackEventSummeryEntryDTO dto) => dto.Index == InProgressStepIndex);
			num = outpostAttackEventSummeryEntryDTO.FinalAttackValue;
			flag8 = outpostAttackEventSummeryEntryDTO.Wrecked;
			flag7 = outpostAttackEventSummeryEntryDTO.HasSuccessDrawn;
			break;
		}
		default:
			num = Data.EventData.TotalDefenceValue - Data.EventData.TotalAttackValue;
			flag8 = Data.EventData.CurrentTargetBuilding.IsWrecked;
			flag7 = Data.EventData.IsSuccessfulDefend;
			break;
		}
		Sprite sprite2;
		if (flag7)
		{
			TotalValueSpecialOutcome.text = LocalizationManager.GetTranslation("GUI_RESULTS_WIN");
			sprite2 = SpritePositiveAttackValue;
		}
		else if (flag8)
		{
			TotalValueSpecialOutcome.text = LocalizationManager.GetTranslation("GUI_RESULTS_LOSE");
			sprite2 = SpriteNegativeAttackValue;
		}
		else
		{
			TotalValueNumeric.text = num.ToString();
			sprite2 = ((num >= 0) ? SpritePositiveAttackValue : SpriteNegativeAttackValue);
		}
		TotalValueSpecialOutcome.enabled = flag7 | flag8;
		TotalValueNumeric.enabled = !TotalValueSpecialOutcome.enabled;
		Icon_TotalValue.sprite = sprite2;
		if ((bool)AdvantagePromptDescription)
		{
			AdvantagePromptDescription.gameObject.SetActive(Data.EventData.WaitingForModifierChoice);
			TextAdvantagePromptDescription.SetTextKey(Data.IsPartyLeader ? "GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_HOST" : "GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_CLIENT");
		}
	}

	private void UpdateRepairInfo()
	{
		if (RepairInfoContainer == null || RepairInfoDescription == null)
		{
			return;
		}
		bool flag = CurrentState == State.Repair && Data.EventData.CurrentTargetBuilding.IsDamaged;
		bool flag2 = SelectedSummery?.IsRepaired ?? Data.EventData.WasRepaired;
		RepairInfoContainer.gameObject.SetActive(flag | flag2);
		if (flag)
		{
			RepairInfoDescription.text = LocalizationManager.GetTranslation(Data.IsPartyLeader ? "GUI_OUTPOST_ATTACK_REPAIR_HINT_HOST" : "GUI_OUTPOST_ATTACK_REPAIR_HINT_CLIENT");
		}
		else if (flag2)
		{
			if (SelectedSummery != null)
			{
				RepairInfoDescription.text = "Repaired with " + SelectedSummery.GetRepairCostLocalized();
			}
			else
			{
				RepairInfoDescription.text = "Repaired with " + Data.EventData.GetRepairCostLocalized();
			}
		}
	}

	private void UpdateInteractions()
	{
		bool flag = SelectedSummery != null;
		bool flag2 = Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackFinished;
		bool flag3 = !flag && !flag2 && !Data.EventData.WaitingForModifierChoice;
		_ = Data.EventData.AvailableMorale;
		bool flag4 = !flag && Data.EventData.CurrentTargetBuilding.IsDamaged && !Data.EventData.WasRepaired;
		bool flag5 = Data.EventData.CurrentTargetIndex + 1 == Data.EventData.TotalTargetCount;
		ActionButton.gameObject.SetActive(!flag3 && !Data.EventData.WaitingForModifierChoice && !flag && (!flag2 || !Data.EventData.CurrentTargetBuilding.IsDamaged));
		ActionButton.interactable = Data.IsPartyLeader;
		string term;
		if (flag)
		{
			term = "GUI_NEXT";
		}
		else if (flag2)
		{
			term = (flag5 ? "GUI_CONTINUE" : "GUI_NEXT");
		}
		else
		{
			term = "GUI_DRAW_MODIFIER";
		}
		FHUILocFormatting.SetText(ActionButton, LocalizationManager.GetTranslation(term));
		DrawModifiersButton.gameObject.SetActive(flag3);
		DrawModifiersButton.interactable = Data.IsPartyLeader;
		SpendMoraleButton.gameObject.SetActive(flag4);
		SpendMaterialsButton.gameObject.SetActive(flag4);
		if (flag4)
		{
			SpendMoraleButton.interactable = Data.IsPartyLeader;
			FHUILocFormatting.SetTextKey(SpendMoraleButton, "GUI_REPAIR_SPENDMORALE");
			SpendMaterialsButton.interactable = Data.IsPartyLeader && Data.CanRepair;
		}
		ChooseModifierButton.gameObject.SetActive(Data.EventData.WaitingForModifierChoice);
		if (Data.EventData.WaitingForModifierChoice)
		{
			ChooseModifierButton.interactable = Data.IsPartyLeader && Data.SelectedModiferIndex != -1;
		}
		SelectCurrentButton.gameObject.SetActive(flag && Data.IsPartyLeader);
	}

	private void SelectCurrent()
	{
		SelectHistoryStep(InProgressStepIndex);
	}

	private void ChooseModifer()
	{
		Data.OnModifierChoiceCallback?.Invoke(Data.SelectedModiferIndex);
	}

	private void OnDrawModifiers()
	{
		Data.PerformAttackCallback?.Invoke(SoldierDistribution.SoldiersToSpend);
	}

	private void NextAction()
	{
		Data.NextActionCallback?.Invoke();
	}

	private void RepairBuildingMaterials()
	{
		Data.RepairBuildingWithMaterialsCallback?.Invoke();
	}

	private void RepairBuildingMorale()
	{
		Data.RepairBuildingWithMoraleCallback?.Invoke();
	}

	private void UpdateBuildingSlots()
	{
		int totalTargetCount = Data.EventData.TotalTargetCount;
		HelperTools.NormalizePool(ref m_BuildingListElements, HistorySlotPrototype.gameObject, HistorySlotsContainer, totalTargetCount);
		for (int i = 0; i < totalTargetCount; i++)
		{
			m_BuildingListElements[i].Init(Data.HistoryData[i], (UIOutpostAttackBuildingListElement e) =>
			{
				SelectHistoryStep(e.Data.Index);
			});
		}
		int num = ((SelectedStepIndex == -1) ? InProgressStepIndex : SelectedStepIndex);
		UIOutpostAttackBuildingListElement uIOutpostAttackBuildingListElement = null;
		foreach (UIOutpostAttackBuildingListElement buildingListElement in m_BuildingListElements)
		{
			OutpostAttackEventSummeryEntryDTO data = buildingListElement.Data;
			bool flag = data != null && data.Index == num;
			if (flag)
			{
				uIOutpostAttackBuildingListElement = buildingListElement;
			}
			buildingListElement.SetSelectedState(flag);
		}
		if (uIOutpostAttackBuildingListElement != null)
		{
			HistorySlotScrollRect.ScrollToFit(uIOutpostAttackBuildingListElement.transform as RectTransform);
		}
	}

	private void SelectHistoryStep(int stepIndex)
	{
		if (stepIndex <= Data.HistoryData.Count)
		{
			SelectedStepIndex = stepIndex;
			SelectedSummery = Data.HistoryData.FirstOrDefault((OutpostAttackEventSummeryEntryDTO dto) => dto.Index == stepIndex && !dto.InProgress);
			Data.OnSlotPicked?.Invoke(SelectedSummeryIndex);
			Rebuild();
		}
	}

	private State CalcState()
	{
		if (Data.EventData == null)
		{
			return State.None;
		}
		if (SelectedSummery != null)
		{
			return State.History;
		}
		return CalcLiveState();
	}

	private State CalcLiveState()
	{
		if (Data.EventData == null)
		{
			return State.None;
		}
		if (Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackNotStarted)
		{
			return State.NewAttack;
		}
		if (Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackStarted)
		{
			return State.DrawModifiers;
		}
		if (Data.EventData.CurrentTargetBuilding.IsDamaged)
		{
			return State.Repair;
		}
		return State.Outcome;
	}

	private IEnumerator TrackTransition()
	{
		while (TownGuardModifiers.InTransition)
		{
			yield return null;
		}
		OnTransitionFinished?.Invoke();
	}
}
