using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIActiveBonusBar : Singleton<UIActiveBonusBar>
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseActiveBonus activeBonusPrefab;

	[SerializeField]
	private UnityEvent OnChangedActiveBonusAvailable;

	private Dictionary<CActiveBonus, UIUseActiveBonus> activeBonusSlots = new Dictionary<CActiveBonus, UIUseActiveBonus>();

	private Stack<UIUseActiveBonus> pool = new Stack<UIUseActiveBonus>();

	private List<CActor> actors;

	private bool isShown;

	private CAbility.EAbilityType m_LastShownType;

	private List<CActiveBonus> m_LastValidBonuses;

	private CActiveBonus.EActiveBonusBehaviourType m_LastShownBehaviourType;

	private bool m_LastAbilityAlreadyStarted;

	private Action<CActiveBonus, CActor> onClickActiveBonus;

	private Func<CActiveBonus, bool> isMandatoryChecker;

	private bool m_LastShownDamageToTake;

	private int m_LastDamageToTake;

	private Transform lastCreatedSlot;

	private static readonly List<CActiveBonus.EActiveBonusBehaviourType> c_behavioursThatNeedBehaviourTypeParameterSet = new List<CActiveBonus.EActiveBonusBehaviourType> { CActiveBonus.EActiveBonusBehaviourType.EndActionAbility };

	public bool IsShowing => isShown;

	public List<CActiveBonus> ShowingActiveBonuses => activeBonusSlots.Keys.ToList();

	public List<CActiveBonus> ShowingPreventDamageBonuses => activeBonusSlots.Keys.Where((CActiveBonus x) => x.Type() == CAbility.EAbilityType.PreventDamage).ToList();

	public bool HasShownUsableSlots()
	{
		if (IsShowing)
		{
			return activeBonusSlots.Any((KeyValuePair<CActiveBonus, UIUseActiveBonus> s) => s.Value.gameObject.activeSelf);
		}
		return false;
	}

	public IEnumerable<ElementInfusionBoardManager.EElement> GetAllConsumes()
	{
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.Where((KeyValuePair<CActiveBonus, UIUseActiveBonus> x) => x.Value.IsSelected()))
		{
			foreach (ElementInfusionBoardManager.EElement selectedConsume in item.Value.GetSelectedConsumes())
			{
				yield return selectedConsume;
			}
		}
	}

	private void CreateBonus(CActor actor, CActiveBonus bonus, bool isInteractable = true)
	{
		Log.NetworkInfo("Creating active bonus. BaseCardID: " + bonus.BaseCard.ID + ", AbilityName: " + bonus.Ability.Name);
		if (!activeBonusSlots.ContainsKey(bonus))
		{
			UIUseActiveBonus slotFromPool = GetSlotFromPool();
			activeBonusSlots[bonus] = slotFromPool;
			slotFromPool.Show();
		}
		else
		{
			activeBonusSlots[bonus].gameObject.SetActive(value: true);
		}
		activeBonusSlots[bonus].SetActiveBonus(actor, bonus, onClickActiveBonus, onClickActiveBonus, isMandatoryChecker, isInteractable);
	}

	private void RemoveBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			activeBonusSlots[bonus].gameObject.SetActive(value: false);
			pool.Push(activeBonusSlots[bonus]);
			activeBonusSlots.Remove(bonus);
			Log.NetworkInfo("Removed ActiveBonus. BaseCardID: " + bonus.BaseCard.ID + ", AbilityName: " + bonus.Ability.Name);
		}
	}

	public void RefreshBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			activeBonusSlots[bonus].Refresh();
		}
	}

	public void SetInteractionAvailableSlots(bool interactable, bool onlyChangeUnselected = false)
	{
		int num = 0;
		List<string> list = new List<string>();
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			string text = activeBonusSlot.Key.Ability.ActiveBonusData.GetVar("DistinctID").GetString();
			if (text != null && activeBonusSlot.Key.ToggledBonus)
			{
				list.Add(text);
			}
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot2 in activeBonusSlots)
		{
			bool flag = true;
			string text2 = activeBonusSlot2.Key.Ability.ActiveBonusData.GetVar("DistinctID").GetString();
			if (text2 != null && !activeBonusSlot2.Key.ToggledBonus && list.Contains(text2))
			{
				flag = false;
			}
			if (activeBonusSlot2.Key.ToggleLocked)
			{
				flag = false;
			}
			if (activeBonusSlot2.Key.CostAbilityExecuted && !activeBonusSlot2.Key.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				flag = false;
			}
			bool flag2 = activeBonusSlot2.Value.IsSelected();
			num += (flag2 ? 1 : 0);
			if (!onlyChangeUnselected || !flag2)
			{
				activeBonusSlot2.Value.SetInteractable(interactable & flag);
			}
		}
	}

	private UIUseActiveBonus GetSlotFromPool()
	{
		UIUseActiveBonus uIUseActiveBonus;
		if (pool.Count > 0)
		{
			uIUseActiveBonus = pool.Pop();
			uIUseActiveBonus.gameObject.SetActive(value: true);
			return uIUseActiveBonus;
		}
		uIUseActiveBonus = UnityEngine.Object.Instantiate(activeBonusPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseActiveBonus.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseActiveBonus.transform;
		return uIUseActiveBonus;
	}

	public UIUseActiveBonus GetSlotForActiveBonus(CActiveBonus bonus)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			return activeBonusSlots[bonus];
		}
		return null;
	}

	private void Clear(bool toggle = false)
	{
		Log.GUIInfo("[ACTIVE BONUS BAR]: Clear.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (toggle)
			{
				activeBonusSlot.Value.ClearSelection();
			}
			activeBonusSlot.Value.Hide();
			pool.Push(activeBonusSlot.Value);
		}
		if (!activeBonusSlots.IsNullOrEmpty())
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
		activeBonusSlots.Clear();
	}

	public void Remove(CActiveBonus bonus, bool clearSelection = false)
	{
		if (activeBonusSlots.ContainsKey(bonus))
		{
			UIUseActiveBonus uIUseActiveBonus = activeBonusSlots[bonus];
			if (clearSelection)
			{
				uIUseActiveBonus.ClearSelection();
			}
			uIUseActiveBonus.Hide();
			pool.Push(uIUseActiveBonus);
			activeBonusSlots.Remove(bonus);
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void Hide(bool toggle = false)
	{
		Log.GUIInfo("[ACTIVE BONUS BAR]: Hide");
		isShown = false;
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		Clear(toggle);
	}

	public void LockToggledActiveBonuses()
	{
		Log.GUIInfo("[ACTIVE BONUS BAR]: Lock toggled active bonuses.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key.ToggledBonus && !activeBonusSlot.Key.ToggleLocked)
			{
				ScenarioRuleClient.LockActiveBonus(activeBonusSlot.Key, GameState.ThreadIsSleeping);
			}
		}
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (Tuple<CActiveBonus, CActor> activeBonuse in GetActiveBonuses(actors, m_LastShownType, m_LastShownBehaviourType, m_LastAbilityAlreadyStarted))
		{
			if (!activeBonusSlots.ContainsKey(activeBonuse.Item1))
			{
				CreateBonus(activeBonuse.Item2, activeBonuse.Item1);
				flag = true;
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			if (!item.Value.IsSelected() && item.Key.HasConsumeElements() && !CanConsume(item.Key, InfusionBoardUI.Instance?.GetAvailableElements()))
			{
				flag = true;
				RemoveBonus(item.Key);
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			item.Value.ClearSelection();
		}
		List<Tuple<CActiveBonus, CActor>> list = ((m_LastShownType != CAbility.EAbilityType.PreventDamage) ? GetActiveBonuses(actors, m_LastShownType, m_LastShownBehaviourType, m_LastAbilityAlreadyStarted) : GetPreventDamageActiveBonuses(actors, m_LastShownType, m_LastShownDamageToTake, m_LastDamageToTake));
		bool flag = false;
		foreach (Tuple<CActiveBonus, CActor> item2 in list)
		{
			if (!activeBonusSlots.ContainsKey(item2.Item1))
			{
				CreateBonus(item2.Item2, item2.Item1);
				flag = true;
			}
		}
		if (flag)
		{
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	private void Init(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType, Action<CActiveBonus, CActor> onClickActiveBonus = null, Func<CActiveBonus, bool> isMandatory = null, bool abilityAlreadyStarted = false, bool typeChanged = false)
	{
		this.actors = actors;
		isMandatoryChecker = isMandatory;
		this.onClickActiveBonus = onClickActiveBonus;
		m_LastShownType = abilityType;
		m_LastShownBehaviourType = behaviourType;
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		m_LastAbilityAlreadyStarted = abilityAlreadyStarted;
		InfusionBoardUI.Instance?.OnUnreserveElement.AddListener(OnUnreservedElement);
		InfusionBoardUI.Instance?.OnReservedElement.AddListener(OnReservedElement);
	}

	public void ShowActiveBonus(CActor actor, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, Action<CActiveBonus, bool> onClickActiveBonus = null, bool abilityAlreadyStarted = false, CAbility ability = null, List<CActiveBonus.EActiveBonusBehaviourType> ignoreBehaviours = null, bool clearPrev = true)
	{
		ShowActiveBonus(new List<CActor> { actor }, abilityType, behaviourType, onClickActiveBonus, abilityAlreadyStarted, ability, ignoreBehaviours, null, clearPrev);
	}

	public void ShowActiveBonus(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, Action<CActiveBonus, bool> onClickActiveBonus = null, bool abilityAlreadyStarted = false, CAbility ability = null, List<CActiveBonus.EActiveBonusBehaviourType> ignoreBehaviours = null, Func<CActiveBonus, bool> filter = null, bool clearPrev = true)
	{
		Action<CActiveBonus, CActor> action = (CActiveBonus bonus, CActor actor) =>
		{
			onClickActiveBonus?.Invoke(bonus, activeBonusSlots[bonus].IsSelected());
		};
		bool flag = false;
		if (ability?.ActorsToTarget != null)
		{
			flag = ability.ActorsToTarget.Count > 0;
		}
		List<Tuple<CActiveBonus, CActor>> activeBonuses = GetActiveBonuses(actors, abilityType, behaviourType, abilityAlreadyStarted, flag, ability, ignoreBehaviours);
		bool flag2 = false;
		List<CActiveBonus> list = activeBonuses.Select((Tuple<CActiveBonus, CActor> x) => x.Item1).Distinct().ToList();
		if (isShown)
		{
			if (m_LastValidBonuses != null && m_LastValidBonuses.Except(list).Any())
			{
				flag2 = true;
			}
			else if (m_LastValidBonuses != null && list.Except(m_LastValidBonuses).Any())
			{
				flag2 = true;
			}
			else if (filter != null || abilityType != m_LastShownType || activeBonusSlots.Any((KeyValuePair<CActiveBonus, UIUseActiveBonus> x) => x.Key.ToggledBonus != x.Value.IsSelected()))
			{
				flag2 = true;
			}
			else if (!flag)
			{
				return;
			}
		}
		m_LastValidBonuses = list;
		Init(actors, abilityType, behaviourType, action, null, abilityAlreadyStarted, flag2);
		if (filter != null)
		{
			activeBonuses.RemoveAll((Tuple<CActiveBonus, CActor> x) => !filter(x.Item1));
		}
		foreach (Tuple<CActiveBonus, CActor> item in activeBonuses)
		{
			UIActiveAbility.CheckForLayoutToGenerate(item.Item1);
			CreateBonus(item.Item2, item.Item1);
		}
		SetInteractionAvailableSlots(actors.Any((CActor actor) => actor.IsUnderMyControl()));
		if (flag2 & clearPrev)
		{
			CAbility cAbility = null;
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
			}
			foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots.ToList())
			{
				bool flag3 = true;
				if (activeBonuses.Any((Tuple<CActiveBonus, CActor> x) => x.Item1 == activeBonusSlot.Key))
				{
					flag3 = false;
				}
				if ((activeBonusSlot.Key.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideAbility) || activeBonusSlot.Key.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideMoveAbility)) && activeBonusSlot.Key.Ability.ActiveBonusData.ActiveBonusAbilityOverrides.Any((CAbilityOverride x) => x.AbilityType.HasValue && x.AbilityType.Value != CAbility.EAbilityType.None) && activeBonusSlot.Key.ToggledBonus)
				{
					flag3 = false;
				}
				if (activeBonusSlot.Key.GetBehaviour(CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility) is CDuringTurnAbilityActiveBonus_TriggerAbility cDuringTurnAbilityActiveBonus_TriggerAbility && activeBonusSlot.Key.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (cAbility == null || cAbility.ID == cDuringTurnAbilityActiveBonus_TriggerAbility.GetAbilityToAdd(cAbilityAddActiveBonus).ID || cAbilityAddActiveBonus.CanBeToggledDuringAbility(cAbility)))
				{
					flag3 = false;
				}
				if (activeBonusSlot.Key.Ability.ActiveBonusData.CostAbility != null && (cAbility == null || cAbility.ID == activeBonusSlot.Key.Ability.ActiveBonusData.CostAbility.ID || activeBonusSlot.Key.CostAbilityExecuted))
				{
					flag3 = false;
				}
				if (PhaseManager.CurrentPhaseAction?.TempLockedBonuses != null && PhaseManager.CurrentPhaseAction.TempLockedBonuses.Contains(activeBonusSlot.Key))
				{
					flag3 = false;
				}
				if (flag3)
				{
					Remove(activeBonusSlot.Key, clearSelection: true);
				}
			}
		}
		if (activeBonuses.Count > 0)
		{
			isShown = true;
		}
		SaveElementsState();
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item2 in activeBonusSlots.OrderBy((KeyValuePair<CActiveBonus, UIUseActiveBonus> x) => x.Key.ID))
		{
			item2.Value.transform.SetAsLastSibling();
		}
		OnChangedActiveBonusAvailable.Invoke();
	}

	public void ShowActiveBonus(CPlayerActor actor, List<CActiveBonus> activeBonuses)
	{
		if (!GameState.ThreadIsSleeping)
		{
			return;
		}
		Hide(toggle: true);
		Action<CActiveBonus, CActor> action = (CActiveBonus bonus, CActor cActor) =>
		{
		};
		onClickActiveBonus = action;
		foreach (CActiveBonus activeBonuse in activeBonuses)
		{
			UIActiveAbility.CheckForLayoutToGenerate(activeBonuse);
			CreateBonus(actor, activeBonuse);
		}
		SetInteractionAvailableSlots(actor.IsUnderMyControl());
		if (activeBonuses.Count > 0)
		{
			isShown = true;
		}
		SaveElementsState();
		OnChangedActiveBonusAvailable.Invoke();
	}

	private List<Tuple<CActiveBonus, CActor>> GetActiveBonuses(List<CActor> actors, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null, List<CActiveBonus.EActiveBonusBehaviourType> ignoreBehaviours = null)
	{
		List<Tuple<CActiveBonus, CActor>> list = new List<Tuple<CActiveBonus, CActor>>();
		for (int i = 0; i < actors.Count; i++)
		{
			foreach (CActiveBonus activeBonuse in GetActiveBonuses(actors[i], abilityType, behaviourType, abilityAlreadyStarted, showSingleTargetBonus, ability, ignoreBehaviours))
			{
				list.Add(new Tuple<CActiveBonus, CActor>(activeBonuse, actors[i]));
			}
		}
		return list;
	}

	private List<CActiveBonus> GetActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None, bool abilityAlreadyStarted = false, bool showSingleTargetBonus = false, CAbility ability = null, List<CActiveBonus.EActiveBonusBehaviourType> ignoreBehaviours = null)
	{
		Log.GUIInfo("[ACTIVE BONUS BAR]: Get active bonuses. ShowSingleTargetBonus: " + showSingleTargetBonus);
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (abilityType == CAbility.EAbilityType.None && behaviourType != CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility && behaviourType != CActiveBonus.EActiveBonusBehaviourType.EndActionAbility && behaviourType != CActiveBonus.EActiveBonusBehaviourType.BetweenActionAbilities)
		{
			return list;
		}
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance?.GetAvailableElements();
		List<CActiveBonus> list2 = (from x in CActiveBonus.FindApplicableActiveBonuses(actor)
			where !x.IsSong
			select x).ToList();
		List<CActiveBonus> list3 = CActiveBonus.FindAllApplicableSongActiveBonuses(actor);
		foreach (CActiveBonus activeBonus in list2)
		{
			if (ignoreBehaviours != null)
			{
				bool flag = false;
				foreach (CActiveBonus.EActiveBonusBehaviourType ignoreBehaviour in ignoreBehaviours)
				{
					if (activeBonus.HasBehaviour(ignoreBehaviour))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			bool isToggleBonus = activeBonus.Ability.ActiveBonusData.IsToggleBonus;
			bool flag2 = activeBonus is CForgoActionsForCompanionActiveBonus;
			if ((!isToggleBonus && !flag2) || !(!activeBonus.Ability.ActiveBonusData.IsSingleTargetBonus | showSingleTargetBonus))
			{
				continue;
			}
			bool flag3 = activeBonus.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType);
			bool flag4 = (activeBonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility) && PhaseManager.IsInActionSelectionPhase) || activeBonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.EndActionAbility) || (activeBonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BetweenActionAbilities) && (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == null || (PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability.GetAbilityBaseCard() != activeBonus.BaseCard && !PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability.AbilityHasHappened)) && PhaseManager.PhaseType == CPhase.PhaseType.Action);
			if (!flag3 && !flag4)
			{
				continue;
			}
			bool flag5 = true;
			if (ability != null)
			{
				foreach (CBespokeBehaviour bespokeBehaviour in activeBonus.BespokeBehaviours)
				{
					if (!bespokeBehaviour.ValidTargetTypeFilters(ability))
					{
						flag5 = false;
						break;
					}
				}
			}
			if (!flag5 || (ability != null && (ability == null || ability.ActiveBonusData.HasBehaviour())) || (ability != null && activeBonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideAbility) && ability.IsMergedAbility) || activeBonus.Ability.AbilityType == CAbility.EAbilityType.PreventDamage || activeBonus.Ability.AbilityType == CAbility.EAbilityType.Shield)
			{
				continue;
			}
			bool flag6 = false;
			flag6 = ((!c_behavioursThatNeedBehaviourTypeParameterSet.Any((CActiveBonus.EActiveBonusBehaviourType x) => activeBonus.Ability.ActiveBonusData.Behaviours.Contains(x))) ? (behaviourType == CActiveBonus.EActiveBonusBehaviourType.None || behaviourType == CActiveBonus.EActiveBonusBehaviourType.BuffAttack || behaviourType == CActiveBonus.EActiveBonusBehaviourType.BuffSingleAttack || activeBonus.Ability.ActiveBonusData.HasBehaviour(behaviourType)) : activeBonus.Ability.ActiveBonusData.HasBehaviour(behaviourType));
			if (flag6 && !activeBonus.IsRestricted(actor) && activeBonus.RequirementsMet(actor, ability) && (!activeBonus.HasConsumeElements() || CanConsume(activeBonus, availableElements)) && CanConsumeResources(activeBonus, actor) && (!activeBonus.IsAura || activeBonus.ValidActorsInRangeOfAura.Contains(actor)))
			{
				bool flag7 = activeBonus.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.None || activeBonus.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.Attack;
				if (ability is CAbilityAttack attackAbility)
				{
					flag7 = activeBonus.IsValidAttackType(attackAbility);
				}
				if (flag7 && (!activeBonus.Ability.ActiveBonusData.IsSingleTargetBonus || activeBonus.HasValidSingleTarget(ability?.ActorsToTarget)))
				{
					list.Add(activeBonus);
				}
			}
		}
		foreach (CActiveBonus item in list3.FindAll((CActiveBonus it) => it.IsSong && it.Ability.ActiveBonusData.IsToggleBonus && it.Ability.ActiveBonusData.IsSingleTargetBonus == showSingleTargetBonus && !it.IsRestricted(actor) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor)))
		{
			foreach (CSong.SongEffect songEffect in item.Ability.Song.SongEffects)
			{
				if (songEffect.IsValidAbilityType(abilityType))
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
					break;
				}
			}
		}
		if (abilityAlreadyStarted)
		{
			list.RemoveAll((CActiveBonus it) => it.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideMoveAbility));
		}
		if (abilityAlreadyStarted)
		{
			list.RemoveAll((CActiveBonus it) => it.ToggleLocked);
		}
		if (abilityAlreadyStarted && abilityType != CAbility.EAbilityType.Move)
		{
			list.RemoveAll((CActiveBonus it) => it.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideAbility, CActiveBonus.EActiveBonusBehaviourType.CastAbilityFromSummon, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbility));
		}
		return list;
	}

	public void ShowReduceDamageActiveBonuses(CActor actor, CAbility damageAbility, bool isLethalDamage, int damageToTake, bool showPrevent, bool showShield, Action<CActiveBonus, CActor> onClickActiveBonus = null)
	{
		if (isShown)
		{
			return;
		}
		m_LastShownDamageToTake = isLethalDamage;
		m_LastDamageToTake = damageToTake;
		CAbility.EAbilityType abilityType = damageAbility?.AbilityType ?? CAbility.EAbilityType.None;
		isShown = true;
		Init(new List<CActor> { actor }, CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.None, onClickActiveBonus, (CActiveBonus bonus) => !bonus.Ability.ActiveBonusData.ToggleIsOptional);
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (showPrevent)
		{
			list.AddRange(GetPreventDamageActiveBonuses(actor, abilityType, isLethalDamage, damageToTake));
		}
		if (showShield)
		{
			list.AddRange(GetShieldIncomingAttackActiveBonuses(actor, abilityType));
		}
		CActiveBonus cActiveBonus = null;
		foreach (CActiveBonus item in list)
		{
			if (cActiveBonus == null || cActiveBonus.Ability.GetAbilityBaseCard()?.ID != item.Ability.GetAbilityBaseCard()?.ID || !(cActiveBonus.Ability.Name == item.Ability.Name) || cActiveBonus.Ability.TargetingActor != item.Ability.TargetingActor)
			{
				CreateBonus(actor, item);
				cActiveBonus = item;
			}
		}
		if (list.Count > 0)
		{
			SaveElementsState();
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void ShowReduceDamageActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, bool isLethalDamage, int damageToTake, bool showPrevent, bool showShield, Action<CActiveBonus, CActor> onClickActiveBonus = null)
	{
		if (isShown)
		{
			return;
		}
		m_LastShownDamageToTake = isLethalDamage;
		m_LastDamageToTake = damageToTake;
		isShown = true;
		Init(new List<CActor> { actor }, CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.None, onClickActiveBonus, (CActiveBonus bonus) => !bonus.Ability.ActiveBonusData.ToggleIsOptional);
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (showPrevent)
		{
			list.AddRange(GetPreventDamageActiveBonuses(actor, abilityType, isLethalDamage, damageToTake));
		}
		if (showShield)
		{
			list.AddRange(GetShieldIncomingAttackActiveBonuses(actor, abilityType));
		}
		CActiveBonus cActiveBonus = null;
		foreach (CActiveBonus item in list)
		{
			if (cActiveBonus == null || !(cActiveBonus.Ability.Name == item.Ability.Name) || cActiveBonus.Ability.TargetingActor != item.Ability.TargetingActor)
			{
				CreateBonus(actor, item);
				cActiveBonus = item;
			}
		}
		if (list.Count > 0)
		{
			SaveElementsState();
			OnChangedActiveBonusAvailable.Invoke();
		}
	}

	public void RefreshLethalReduceDamageActiveBonuses(CPlayerActor actor, bool isLethal)
	{
		if (!isShown || !actors.Contains(actor))
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key is CPreventDamageActiveBonus { PreventOnlyIfLethal: not false })
			{
				if (isLethal)
				{
					activeBonusSlot.Value.SetInteractable(interactable: true);
				}
				else if (!activeBonusSlot.Value.IsSelected())
				{
					activeBonusSlot.Value.SetInteractable(interactable: false);
				}
			}
		}
	}

	public void RefreshSingleTargetActiveBonusesFromSRLState()
	{
		if (!isShown)
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (activeBonusSlot.Key.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				if (activeBonusSlot.Key.ToggledBonus)
				{
					activeBonusSlot.Value.Select();
				}
				else
				{
					activeBonusSlot.Value.ClearSelection();
				}
			}
		}
	}

	private List<Tuple<CActiveBonus, CActor>> GetPreventDamageActiveBonuses(List<CActor> actors, CAbility.EAbilityType abilityType, bool isLetal, int damageToTake)
	{
		List<Tuple<CActiveBonus, CActor>> list = new List<Tuple<CActiveBonus, CActor>>();
		for (int i = 0; i < actors.Count; i++)
		{
			foreach (CActiveBonus preventDamageActiveBonuse in GetPreventDamageActiveBonuses(actors[i], abilityType, isLetal, damageToTake))
			{
				list.Add(new Tuple<CActiveBonus, CActor>(preventDamageActiveBonuse, actors[i]));
			}
		}
		return list;
	}

	public List<CActiveBonus> GetPreventDamageActiveBonuses(CActor actor, CAbility.EAbilityType abilityType, bool isLethal, int damageToTake)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance?.GetAvailableElements();
		List<CActiveBonus> list = actor.FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage);
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(list.FindAll((CActiveBonus it) => it is CPreventDamageActiveBonus cPreventDamageActiveBonus && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(actor) && (it.Ability.GetMiscVar("DamageToTakeMustBeOrLessThan").isUnknown || damageToTake <= it.Ability.GetMiscVar("DamageToTakeMustBeOrLessThan").GetInt()) && (!cPreventDamageActiveBonus.PreventOnlyIfLethal || (cPreventDamageActiveBonus.PreventOnlyIfLethal & isLethal)) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType)) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor) && it.RequirementsMet(actor)));
		if (abilityType != CAbility.EAbilityType.Retaliate)
		{
			list2.RemoveAll((CActiveBonus x) => x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventBySettingRetaliateToValue));
		}
		if (abilityType == CAbility.EAbilityType.Attack)
		{
			return list2.FindAll((CActiveBonus x) => !(x is CPreventDamageActiveBonus { HasTracker: not false } cPreventDamageActiveBonus) || cPreventDamageActiveBonus.Remaining > 0).ToList();
		}
		return list2.FindAll((CActiveBonus x) => x is CPreventDamageActiveBonus cPreventDamageActiveBonus && (!cPreventDamageActiveBonus.HasTracker || cPreventDamageActiveBonus.Remaining > 0) && !cPreventDamageActiveBonus.PreventDamageAttackSourcesOnly).ToList();
	}

	public List<CActiveBonus> GetShieldIncomingAttackActiveBonuses(CActor actor, CAbility.EAbilityType abilityType)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance?.GetAvailableElements();
		List<CActiveBonus> activeBonuses = actor.ActiveBonuses;
		List<CActiveBonus> list = new List<CActiveBonus>();
		list.AddRange(activeBonuses.FindAll((CActiveBonus it) => it.Ability.AbilityType == CAbility.EAbilityType.Shield && it.Ability.ActiveBonusData.IsToggleBonus && (!it.Ability.IsItemAbility || !it.Ability.ActiveBonusData.AuraFilter.HasTargetTypeFilters() || it.Ability.ActiveBonusData.AuraFilter.IsValidTarget(actor, it.Actor, isTargetedAbility: false, useTargetOriginalType: false, false)) && !it.IsRestricted(actor) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(abilityType)) && (!it.HasConsumeElements() || CanConsume(it, availableElements)) && CanConsumeResources(it, actor) && it.RequirementsMet(actor)));
		return list.FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffShield, CActiveBonus.EActiveBonusBehaviourType.ShieldIncomingAttacks, CActiveBonus.EActiveBonusBehaviourType.ShieldAndDisadvantage, CActiveBonus.EActiveBonusBehaviourType.ShieldAndRetaliate)).ToList();
	}

	private bool CanConsume(CActiveBonus bonus, List<ElementInfusionBoardManager.EElement> availableElements)
	{
		if (availableElements.Count == 0)
		{
			return false;
		}
		ElementInfusionBoardManager.EElement eElement = bonus.Ability.ActiveBonusData.Consuming[0];
		if (eElement != ElementInfusionBoardManager.EElement.Any)
		{
			return availableElements.Contains(eElement);
		}
		return true;
	}

	private bool CanConsumeResources(CActiveBonus bonus, CActor actor)
	{
		if (bonus.Ability.ActiveBonusData.RequiredResources.Count > 0)
		{
			List<CCharacterResource> source = actor.CharacterResources.ToList();
			foreach (CActiveBonus item in (from it in activeBonusSlots
				where !it.Value.IsSelected()
				select it.Key).ToList())
			{
				List<CCharacterResource> toggledResourcesToConsume = item.ToggledResourcesToConsume;
				if (toggledResourcesToConsume == null || toggledResourcesToConsume.Count <= 0)
				{
					continue;
				}
				foreach (CCharacterResource resource in item.ToggledResourcesToConsume)
				{
					source.SingleOrDefault((CCharacterResource x) => x.ID == resource.ID).Amount -= resource.Amount;
				}
			}
			foreach (KeyValuePair<string, int> requiredResource in bonus.Ability.ActiveBonusData.RequiredResources)
			{
				if (!actor.CharacterHasResource(requiredResource.Key, requiredResource.Value))
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<CActiveBonus> GetNonSelectedActiveBonus()
	{
		return (from it in activeBonusSlots
			where !it.Value.IsSelected()
			select it.Key).ToList();
	}

	public List<CActiveBonus> GetSelectedActiveBonuses()
	{
		return (from it in activeBonusSlots
			where it.Value.IsSelected()
			select it.Key).ToList();
	}

	public void HighlightMandatoryActiveBonus()
	{
		if (isMandatoryChecker == null)
		{
			return;
		}
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> activeBonusSlot in activeBonusSlots)
		{
			if (!activeBonusSlot.Value.IsSelected() && isMandatoryChecker(activeBonusSlot.Key))
			{
				activeBonusSlot.Value.Highlight();
			}
		}
	}

	public void HideSingleTargetActiveBonuses()
	{
		Log.GUIInfo("[ACTIVE BONUS BAR]: Hide single target bonuses.");
		foreach (KeyValuePair<CActiveBonus, UIUseActiveBonus> item in activeBonusSlots.ToList())
		{
			if (item.Key.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				Remove(item.Key, clearSelection: true);
			}
		}
	}

	private void SaveElementsState()
	{
		if (activeBonusSlots.Count != 0)
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
	}

	public void ProxyUseActiveBonus(GameAction action)
	{
		if (Singleton<UIScenarioMultiplayerController>.Instance.damageMessage == null)
		{
			ActiveBonusToken activeBonusToken = action.SupplementaryDataToken as ActiveBonusToken;
			KeyValuePair<CActiveBonus, UIUseActiveBonus> tObj = activeBonusSlots.FirstOrDefault((KeyValuePair<CActiveBonus, UIUseActiveBonus> x) => x.Key.BaseCard.ID == activeBonusToken.BaseCardID && x.Key.ID == activeBonusToken.ActiveBonusID && x.Key.Ability.Name == activeBonusToken.AbilityName);
			if (tObj.IsTNull())
			{
				throw new Exception("Error using active bonus. No active bonus found (BaseCardID: " + activeBonusToken.BaseCardID + ", AbilityName: " + activeBonusToken.AbilityName + ", SelectedElementID: " + activeBonusToken.SelectedElementID + ").");
			}
			if (activeBonusToken.Selected)
			{
				tObj.Value.SetSelectedOptions(activeBonusToken.SelectedOptionsID.ToList());
				tObj.Value.SetAnyConsume((ElementInfusionBoardManager.EElement)activeBonusToken.SelectedElementID);
			}
			if (activeBonusToken.Selected != tObj.Value.IsSelected())
			{
				tObj.Value.OnPointerDown();
			}
		}
	}
}
