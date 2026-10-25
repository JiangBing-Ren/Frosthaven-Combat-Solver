using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIOutpostAttackTownGuardModifiers : MonoBehaviour
{
	public struct Params
	{
		public OutpostAttackEventDTO EventData;

		public List<AttackModifierDTO> TownGuardDeck;

		public OutpostAttackEventSummeryEntryDTO SelectedSummery;

		public Action<int> OnModifierSelectedCallback;

		public int SelectedModiferIndex;

		public OutpostAttackModalModule.State PrevState;

		public OutpostAttackModalModule.State CurrentState;
	}

	[Header("Components")]
	[SerializeField]
	private FHUIAttackModifier AttackModifierPrototype;

	[SerializeField]
	private UIOutpostAttackTownGuardDrawnModifier DrawnModifierPrototype;

	[SerializeField]
	private GameObject TownGuardDeckGroup;

	[SerializeField]
	private RectTransform TownGuardDeckContainer;

	[SerializeField]
	private GameObject DrawnModifiersGroup;

	[SerializeField]
	private RectTransform DrawnModifiersContainer;

	[Header("Animations")]
	[SerializeField]
	private RectTransform AnimationContainer;

	[SerializeField]
	private Image DeckElement;

	[Header("Settings")]
	[SerializeField]
	private int DeckSlotCount = 16;

	[SerializeField]
	private float m_HideDuration = 0.3f;

	[SerializeField]
	private float m_ShowDuration = 0.45f;

	[SerializeField]
	private float m_PerSlotShowDelay = 0.1f;

	[SerializeField]
	private float m_PerSlotHideDelay = 0.05f;

	private readonly List<FHUIAttackModifier> m_TownGuardDeckModifiers = new List<FHUIAttackModifier>();

	private readonly List<UIOutpostAttackTownGuardDrawnModifier> m_DrawnModifiers = new List<UIOutpostAttackTownGuardDrawnModifier>();

	private readonly List<UIOutpostAttackTownGuardDrawnModifier.Params> m_ModifierParams = new List<UIOutpostAttackTownGuardDrawnModifier.Params>();

	private bool m_Initialized;

	private List<Tween> activeTweens = new List<Tween>();

	public Params Data { get; private set; }

	public bool InTransition { get; private set; }

	public void InitCache()
	{
		if (!m_Initialized)
		{
			TownGuardDeckContainer.GetComponentsInChildren(includeInactive: true, m_TownGuardDeckModifiers);
			DrawnModifiersContainer.GetComponentsInChildren(includeInactive: true, m_DrawnModifiers);
			DeckElement.gameObject.SetActive(value: false);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		bool flag = Data.SelectedSummery != null;
		bool flag2 = !flag && Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackNotStarted;
		bool flag3 = Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackStarted && Data.EventData.ToChooseCards.Any();
		bool flag4 = flag || Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackFinished;
		TownGuardDeckGroup.gameObject.SetActive(flag2);
		DrawnModifiersGroup.gameObject.SetActive(flag4 | flag3);
		if (flag3)
		{
			PopulateDrawnModifiers();
		}
		else if (flag2)
		{
			PopulateDeck();
		}
		else if (flag4)
		{
			PopulateDrawnModifiers();
		}
		if (Data.PrevState == OutpostAttackModalModule.State.NewAttack && (Data.CurrentState == OutpostAttackModalModule.State.DrawModifiers || Data.CurrentState == OutpostAttackModalModule.State.Repair || Data.CurrentState == OutpostAttackModalModule.State.Outcome))
		{
			StartCoroutine(BeginDrawTransition());
		}
	}

	private void OnDisable()
	{
		InTransition = false;
		activeTweens.ForEach((Tween tween) =>
		{
			tween.Kill();
		});
		activeTweens.Clear();
	}

	private void PopulateDrawnModifiers()
	{
		m_ModifierParams.Clear();
		if (Data.SelectedSummery != null)
		{
			if (Data.SelectedSummery.DrawnCards != null && Data.SelectedSummery.DrawnCards.Count > 0)
			{
				m_ModifierParams.AddRange(Data.SelectedSummery.DrawnCards.Select((AttackModifierDTO dto) => new UIOutpostAttackTownGuardDrawnModifier.Params
				{
					Modifer = dto
				}));
			}
		}
		else
		{
			if (Data.EventData.DrawnCards != null && Data.EventData.DrawnCards.Count > 0)
			{
				int num = Data.EventData.DrawnCards.Count;
				if (Data.EventData.ToChooseCards != null)
				{
					num -= Data.EventData.ToChooseCards.Count;
				}
				for (int num2 = 0; num2 < num; num2++)
				{
					AttackModifierDTO modifer = Data.EventData.DrawnCards[num2];
					m_ModifierParams.Add(new UIOutpostAttackTownGuardDrawnModifier.Params
					{
						Modifer = modifer
					});
				}
			}
			if (Data.EventData.ToChooseCards != null && Data.EventData.ToChooseCards.Count > 0)
			{
				for (int num3 = 0; num3 < Data.EventData.ToChooseCards.Count; num3++)
				{
					AttackModifierDTO modifer2 = Data.EventData.ToChooseCards[num3];
					int modiferSelectionIndex = num3;
					UIOutpostAttackTownGuardDrawnModifier.Params item = new UIOutpostAttackTownGuardDrawnModifier.Params
					{
						Modifer = modifer2,
						OnSelectCallback = () =>
						{
							Data.OnModifierSelectedCallback?.Invoke(modiferSelectionIndex);
						},
						Selected = (Data.SelectedModiferIndex == modiferSelectionIndex),
						Interactable = true,
						PendingSelection = (Data.SelectedModiferIndex == -1)
					};
					m_ModifierParams.Add(item);
				}
			}
		}
		UIHelperUtil.UpdateUIList(m_ModifierParams.Count, m_DrawnModifiers, DrawnModifierPrototype, DrawnModifiersContainer, onShow);
		void onShow(int index, UIOutpostAttackTownGuardDrawnModifier uIOutpostAttackTownGuardDrawnModifier)
		{
			UIOutpostAttackTownGuardDrawnModifier.Params obj = m_ModifierParams[index];
			uIOutpostAttackTownGuardDrawnModifier.Init(obj);
		}
	}

	private void PopulateDeck()
	{
		UIHelperUtil.UpdateUIList(DeckSlotCount, m_TownGuardDeckModifiers, AttackModifierPrototype, TownGuardDeckContainer, onShow);
		void onShow(int index, FHUIAttackModifier item)
		{
			AttackModifierDTO dto = ((Data.TownGuardDeck != null && Data.TownGuardDeck.Count > index) ? Data.TownGuardDeck[index] : null);
			item.Init(dto);
		}
	}

	private IEnumerator BeginDrawTransition()
	{
		InTransition = true;
		activeTweens.ForEach((Tween tween) =>
		{
			tween.Kill();
		});
		activeTweens.Clear();
		DeckElement.gameObject.SetActive(value: true);
		DeckElement.transform.localScale = Vector3.zero;
		DeckElement.SetAlpha(0f);
		DeckElement.DOFade(1f, m_ShowDuration * 0.25f).SetEase(Ease.OutSine);
		DeckElement.transform.DOScale(Vector3.one, m_ShowDuration).SetEase(Ease.InSine);
		for (int num = 0; num < m_DrawnModifiers.Count; num++)
		{
			UIOutpostAttackTownGuardDrawnModifier uIOutpostAttackTownGuardDrawnModifier = m_DrawnModifiers[num];
			if (uIOutpostAttackTownGuardDrawnModifier.Data.Modifer != null && uIOutpostAttackTownGuardDrawnModifier.gameObject.activeSelf)
			{
				uIOutpostAttackTownGuardDrawnModifier.MoveContainer.gameObject.SetActive(value: false);
			}
		}
		yield return null;
		float num2 = 0f;
		foreach (FHUIAttackModifier icon in m_TownGuardDeckModifiers)
		{
			if (icon.Data != null)
			{
				icon.transform.SetParent(AnimationContainer);
				Tween travelTween = null;
				travelTween = icon.transform.DOMove(DeckElement.transform.position, m_HideDuration).SetEase(Ease.OutSine).SetDelay(num2)
					.OnKill(() =>
					{
						icon.gameObject.SetActive(value: false);
						icon.transform.SetParent(TownGuardDeckContainer);
					})
					.OnComplete(() =>
					{
						icon.gameObject.SetActive(value: false);
						icon.transform.SetParent(TownGuardDeckContainer);
						activeTweens.Remove(travelTween);
					});
				num2 += m_PerSlotHideDelay;
				activeTweens.Add(travelTween);
			}
		}
		while (activeTweens.Count > 0)
		{
			yield return null;
		}
		float num3 = 0f;
		for (int num4 = 0; num4 < m_DrawnModifiers.Count; num4++)
		{
			UIOutpostAttackTownGuardDrawnModifier icon2 = m_DrawnModifiers[num4];
			if (icon2.Data.Modifer != null && icon2.gameObject.activeSelf)
			{
				icon2.MoveContainer.gameObject.SetActive(value: true);
				icon2.MoveContainer.transform.SetParent(AnimationContainer);
				icon2.MoveContainer.transform.position = DeckElement.transform.position;
				Tween travelTween2 = null;
				travelTween2 = icon2.MoveContainer.transform.DOMove(icon2.transform.position, m_ShowDuration).SetEase(Ease.OutSine).SetDelay(num3)
					.OnComplete(() =>
					{
						icon2.MoveContainer.transform.SetParent(icon2.transform);
						activeTweens.Remove(travelTween2);
					});
				activeTweens.Add(travelTween2);
				num3 += m_PerSlotShowDelay;
			}
		}
		while (activeTweens.Count > 0)
		{
			yield return null;
		}
		DeckElement.DOFade(0f, m_ShowDuration).SetEase(Ease.OutSine).OnComplete(() =>
		{
			DeckElement.gameObject.SetActive(value: false);
			DeckElement.SetAlpha(1f);
		});
		InTransition = false;
	}
}
