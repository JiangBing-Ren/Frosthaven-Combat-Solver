using System;
using System.Collections.Generic;
using FFSNet;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCardPickerSlot : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Transform cardHolder;

	private AbilityCardUI abilityCardUI;

	private MonsterRoundCardUI monsterCardUI;

	private IAbilityCardOption option;

	private Action<IAbilityCardOption, bool> onSelected;

	private Action<IAbilityCardOption, bool> onDeselected;

	private bool isSelected;

	public IAbilityCardOption AbilityCardOption => option;

	public Selectable Selectable => button;

	private void Awake()
	{
		button.onClick.AddListener(Toggle);
	}

	public void SetOption(CActor actor, IAbilityCardOption option, Action<IAbilityCardOption, bool> onSelected, Action<IAbilityCardOption, bool> onDeselected)
	{
		this.option = option;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		isSelected = false;
		Clear();
		if (option.AbilityCard.CardType == CBaseCard.ECardType.CharacterAbility)
		{
			abilityCardUI = ObjectPool.SpawnCard(option.AbilityCard.ID, ObjectPool.ECardType.Ability, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
			IEnumerable<CEnhancement> enhancements = ((actor is CPlayerActor actor2) ? UIPlayerCharacterUtils.GetEnhancements(actor2) : null);
			abilityCardUI.Init(option.AbilityCard as CAbilityCard, disableEventDetection: false, enhancements);
			abilityCardUI.fullAbilityCard.ToggleSideInteractivity(active: true, CBaseCard.ActionType.TopAction);
			abilityCardUI.fullAbilityCard.ToggleSideInteractivity(active: true, CBaseCard.ActionType.BottomAction);
			abilityCardUI.fullAbilityCard.BlockRaycasts(block: false);
		}
		if (option is MonsterCardChoice monsterCardChoice && option.AbilityCard is CMonsterAbilityCard cMonsterAbilityCard)
		{
			monsterCardUI = ObjectPool.SpawnCard(option.AbilityCard.ID, ObjectPool.ECardType.Monster, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<MonsterRoundCardUI>();
			monsterCardUI.Init(cMonsterAbilityCard.GetMonsterCardYML, monsterCardChoice.MonsterClass);
			ImageMaterialSwapper component = monsterCardUI.GetComponent<ImageMaterialSwapper>();
			if (component != null)
			{
				component.SwapToTargetMaterial();
			}
		}
		if (option.ActionTypeDisabled != CBaseCard.ActionType.NA && abilityCardUI != null)
		{
			abilityCardUI.fullAbilityCard.ToggleSideInteractivity(active: false, option.ActionTypeDisabled);
		}
	}

	public void Clear()
	{
		if (abilityCardUI != null)
		{
			ObjectPool.RecycleCard(abilityCardUI.CardID, ObjectPool.ECardType.Ability, abilityCardUI.gameObject);
			abilityCardUI = null;
		}
		if (!(monsterCardUI != null))
		{
			return;
		}
		if (monsterCardUI != null)
		{
			ImageMaterialSwapper component = monsterCardUI.GetComponent<ImageMaterialSwapper>();
			if (component != null)
			{
				component.ResetToPreSwap();
			}
		}
		ObjectPool.RecycleCard(monsterCardUI.CardID, ObjectPool.ECardType.Monster, monsterCardUI.gameObject);
		monsterCardUI = null;
	}

	private void Toggle()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			Select();
		}
	}

	private void OnDisable()
	{
		Clear();
	}

	public void Deselect()
	{
		if (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl())
		{
			Deselect(true);
		}
	}

	public void Deselect(bool networkActionIfOnline = true)
	{
		isSelected = false;
		if (abilityCardUI != null && option.ActionTypeDisabled != CBaseCard.ActionType.NA)
		{
			abilityCardUI.fullAbilityCard.ToggleSelect(active: false, (option.ActionTypeDisabled == CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction);
		}
		if (monsterCardUI != null)
		{
			ImageMaterialSwapper component = monsterCardUI.GetComponent<ImageMaterialSwapper>();
			if (component != null)
			{
				component.SwapToTargetMaterial();
			}
		}
		onDeselected?.Invoke(option, networkActionIfOnline);
		if ((FFSNetwork.IsOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl()) & networkActionIfOnline)
		{
			Synchronizer.SendGameAction(GameActionType.DeselectAbilityCard, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, option.AbilityCard.ID);
		}
	}

	public void Select()
	{
		if (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl())
		{
			Select(true);
		}
	}

	public void Select(bool networkActionIfOnline = true)
	{
		isSelected = true;
		if (abilityCardUI != null && option.ActionTypeDisabled != CBaseCard.ActionType.NA)
		{
			abilityCardUI.fullAbilityCard.ToggleSelect(active: true, (option.ActionTypeDisabled == CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction);
		}
		if (monsterCardUI != null)
		{
			ImageMaterialSwapper component = monsterCardUI.GetComponent<ImageMaterialSwapper>();
			if (component != null)
			{
				component.ResetToPreSwap();
			}
		}
		onSelected?.Invoke(option, networkActionIfOnline);
		if ((FFSNetwork.IsOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl()) & networkActionIfOnline)
		{
			Synchronizer.SendGameAction(GameActionType.SelectAbilityCard, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, option.AbilityCard.ID);
		}
	}

	public void SetFocused(bool focused)
	{
		if (abilityCardUI != null)
		{
			abilityCardUI.SetType(focused ? CardPileType.Hand : CardPileType.Discarded);
		}
	}
}
