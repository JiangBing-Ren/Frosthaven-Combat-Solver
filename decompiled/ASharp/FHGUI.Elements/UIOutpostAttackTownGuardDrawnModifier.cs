using System;
using FHGUI.DTO;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIOutpostAttackTownGuardDrawnModifier : MonoBehaviour
{
	public struct Params
	{
		public AttackModifierDTO Modifer;

		public Action OnSelectCallback;

		public bool Selected;

		public bool IsAbilityActive;

		public bool PendingSelection;

		public bool Interactable;

		public bool Discarded;
	}

	[Header("Components")]
	[SerializeField]
	private GameObject ActiveAbilityHighlight;

	[SerializeField]
	private FHUIAttackModifier AttackModifierIcon;

	[SerializeField]
	private Image ModifiedValueContainer;

	[SerializeField]
	private TMP_Text ModifiedValueText;

	[SerializeField]
	private FHButton Button;

	[SerializeField]
	private GameObject Selected;

	[SerializeField]
	private GameObject NoneSelected;

	[SerializeField]
	private GameObject Interactable;

	[SerializeField]
	private GameObject NoneInteractable;

	[SerializeField]
	private Image Icon;

	[Header("Settings")]
	[SerializeField]
	private float AlphaNormal = 1f;

	[SerializeField]
	private float AlphaInactive = 0.35f;

	[SerializeField]
	private Material IconMaterialNormal;

	[SerializeField]
	private Material IconMaterialDiscarded;

	public GameObject MoveContainer;

	private bool m_Initialized;

	private bool IsSkipped
	{
		get
		{
			if (Data.Modifer.Usage != OutpostAttackState.DrawnCardUsageSummary.CardUsageState.SkippedDueToChoice && Data.Modifer.Usage != OutpostAttackState.DrawnCardUsageSummary.CardUsageState.SkippedDueToRollingAndDiscardedAfterDisadvantage)
			{
				return Data.Modifer.Usage == OutpostAttackState.DrawnCardUsageSummary.CardUsageState.SkippedDueToDisadvantage;
			}
			return true;
		}
	}

	public Params Data { get; private set; }

	public UIOutpostAttackTownGuardDrawnModifier(FHUIAttackModifier attackModifierIcon)
	{
		AttackModifierIcon = attackModifierIcon;
	}

	public void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		if ((bool)Button)
		{
			Button.onClick.AddListener(() =>
			{
				Data.OnSelectCallback?.Invoke();
			});
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		AttackModifierIcon.Init(Data.Modifer);
		if ((bool)Button)
		{
			Button.interactable = Data.OnSelectCallback != null;
		}
		if ((bool)ModifiedValueContainer)
		{
			ModifiedValueContainer.gameObject.SetActive(Data.Modifer.IsModified);
			ModifiedValueContainer.SetAlpha((IsSkipped || Data.Discarded || (Data.Interactable && !Data.PendingSelection && !Data.Selected)) ? AlphaInactive : AlphaNormal);
		}
		if ((bool)ModifiedValueText)
		{
			ModifiedValueText.gameObject.SetActive(Data.Modifer.IsModified);
			if (Data.Modifer.IsModified)
			{
				ModifiedValueText.text = Data.Modifer.LocalizedValue;
			}
		}
		if ((bool)Selected)
		{
			Selected.SetActive(Data.Selected);
		}
		if ((bool)NoneSelected)
		{
			NoneSelected.SetActive(Data.Interactable && !Data.PendingSelection && !Data.Selected);
		}
		if ((bool)Interactable)
		{
			Interactable.SetActive(Data.Selected || (Data.Interactable && Data.PendingSelection));
		}
		if ((bool)NoneInteractable)
		{
			NoneInteractable.SetActive(!Data.Interactable && !IsSkipped);
		}
		Icon.SetAlpha((IsSkipped || Data.Discarded || (Data.Interactable && !Data.PendingSelection && !Data.Selected)) ? AlphaInactive : AlphaNormal);
		Icon.material = (Data.Discarded ? IconMaterialDiscarded : IconMaterialNormal);
		if (ActiveAbilityHighlight != null)
		{
			ActiveAbilityHighlight.SetActive(Data.IsAbilityActive);
		}
	}
}
