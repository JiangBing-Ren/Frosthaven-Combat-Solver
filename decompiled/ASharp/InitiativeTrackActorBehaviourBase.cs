using System;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class InitiativeTrackActorBehaviourBase : MonoBehaviour, IComparable
{
	[SerializeField]
	private GUIAnimator m_ExtraTurnAnimation;

	private CActor m_Actor;

	private CClass m_Class;

	[field: SerializeField]
	public CActor.EType ActorType { get; private set; }

	[field: SerializeField]
	public InitiativeTrackActorAvatarBase Avatar { get; private set; }

	[field: SerializeField]
	public InitiativeTrackActorBehaviourExtension Extension { get; private set; }

	public bool IsSelected { get; private set; }

	public CClass Class => m_Class;

	public CActor Actor
	{
		get
		{
			return m_Actor;
		}
		protected set
		{
			int num = name.IndexOf(" - ");
			if (num == -1)
			{
				name = name + " - " + value?.GetPrefabName();
			}
			else
			{
				name = name.Substring(0, num) + " - " + value?.GetPrefabName();
			}
			m_Actor = value;
			Avatar.Init(Extension.OnAvatarHighlight, m_Actor, interactable: false);
		}
	}

	public void Select()
	{
		IsSelected = true;
		Avatar.OnSelect();
		Extension.OnSelect();
	}

	public void Deselect()
	{
		IsSelected = false;
		Avatar.OnDeselect();
		Extension.OnDeselect();
	}

	public void RefreshInitiative()
	{
		Avatar.OnRefreshInitiative(m_Class);
	}

	public void RefreshAbilities()
	{
		Avatar.OnRefreshAbilities();
	}

	public void SetAttributes(CActor actor, bool isActiveButton, InitiativeTrack.TargetingParams targetingParams, CClass monsterClass = null)
	{
		SetAttributes(actor, isActiveButton, actor == targetingParams.ActingActor, targetingParams.TargetedActors?.Contains(actor) ?? false, targetingParams.IsPositiveAbility, monsterClass);
	}

	public void SetAttributes(CActor actor, bool isActiveButton, bool isActing, bool isTargeted, bool isPositiveAbility, CClass monsterClass = null)
	{
		int summonsCount = ((actor != null) ? (actor.IsPlayerByDefault() ? ScenarioManager.Scenario.HeroSummons.Count((CHeroSummonActor s) => s.Summoner == actor) : ScenarioManager.Scenario.AllMonsters.Count((CEnemyActor e) => !e.IsDead && e.Summoner == actor)) : 0);
		InitiativeTrack.ActorAttributesParams attributesDirect = new InitiativeTrack.ActorAttributesParams
		{
			Actor = actor,
			Class = (monsterClass ?? actor?.Class ?? m_Class),
			TextInitiative = ((monsterClass is CMonsterClass mClass) ? Avatar.Extension.GetInitiativeText(mClass, actor as CEnemyActor) : Avatar.Extension.GetInitiativeText(actor)),
			CurrentHealth = (actor?.Health ?? 0),
			MaxHealth = (actor?.MaxHealth ?? 0),
			IsActiveButton = isActiveButton,
			IsFactionDisplayed = (ScenarioManager.Scenario.Enemies.Count > 0 && ScenarioManager.Scenario.Enemy2Monsters.Count > 0),
			IsActing = isActing,
			IsTargeted = isTargeted,
			IsTargetedByPositiveAbility = isPositiveAbility,
			SummonsCount = summonsCount,
			PhaseState = Avatar.Extension.GetPhaseState(actor)
		};
		SetAttributesDirect(attributesDirect);
	}

	public void SetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		Actor = aaParams.Actor;
		m_Class = aaParams.Class as CMonsterClass;
		Extension.OnSetAttributesDirect(aaParams);
		Avatar.OnSetAttributesDirect(aaParams);
		ShowExtraTurn(m_Actor != null && m_Actor.IsTakingExtraTurn);
		InteractabilityIsolatedUIControl component = GetComponent<InteractabilityIsolatedUIControl>();
		if (component != null)
		{
			component.ControlIdentifier = Actor?.Class?.ID;
			component.ControlSecondIdentifier = Actor?.ActorGuid;
		}
	}

	public void ShowDetails(bool active)
	{
		Avatar.OnShowDetails(active);
	}

	public int GetOrderPriority()
	{
		if (m_Actor == null)
		{
			return 103;
		}
		int num = ((Class is CMonsterClass cMonsterClass) ? cMonsterClass.Initiative(m_Actor as CEnemyActor, includeOverrideCard: false) : Actor.Initiative());
		if (num == 0)
		{
			if (!m_Actor.IsPlayerByDefault())
			{
				return 102;
			}
			return 101;
		}
		return num;
	}

	public virtual int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = obj as InitiativeTrackActorBehaviourBase;
		if (initiativeTrackActorBehaviourBase == null)
		{
			Log.GUIError("Comparing non InitiativeTrackActorBehaviourBase class");
			return 1;
		}
		if (FFSNetwork.IsOnline && PhaseManager.IsInCardsSelectionPhase && m_Actor.IsPlayerByDefault() && initiativeTrackActorBehaviourBase.Actor.IsPlayerByDefault() && (!m_Actor.IsUnderMyControl() || !initiativeTrackActorBehaviourBase.Actor.IsUnderMyControl()))
		{
			if (m_Actor.IsUnderMyControl() != initiativeTrackActorBehaviourBase.Actor.IsUnderMyControl())
			{
				if (!m_Actor.IsUnderMyControl())
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
		int orderPriority = initiativeTrackActorBehaviourBase.GetOrderPriority();
		int orderPriority2 = GetOrderPriority();
		if (orderPriority != orderPriority2 || orderPriority > 100 || orderPriority2 > 100)
		{
			return orderPriority2.CompareTo(orderPriority);
		}
		int num = 100 - m_Actor.SubInitiative();
		int value = 100 - initiativeTrackActorBehaviourBase.Actor.SubInitiative();
		if (m_Actor is CHeroSummonActor cHeroSummonActor)
		{
			if (initiativeTrackActorBehaviourBase.Actor == cHeroSummonActor.Summoner)
			{
				return -1;
			}
			int num2 = 100 - cHeroSummonActor.Summoner.SubInitiative();
			if (initiativeTrackActorBehaviourBase.Actor is CHeroSummonActor cHeroSummonActor2)
			{
				if (cHeroSummonActor.Summoner != cHeroSummonActor2.Summoner)
				{
					return num2.CompareTo(100 - cHeroSummonActor2.Summoner.SubInitiative());
				}
				return cHeroSummonActor.SummonedOrderIndex.CompareTo(cHeroSummonActor2.SummonedOrderIndex);
			}
			return num2.CompareTo(value);
		}
		return num.CompareTo(value);
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
		Avatar.OnActiveBonusTriggered(activeBonus);
	}

	public void ShowExtraTurn(bool show)
	{
		if (show)
		{
			m_ExtraTurnAnimation.Play();
		}
		else
		{
			m_ExtraTurnAnimation.Stop();
		}
	}

	private void OnDisable()
	{
		ShowExtraTurn(show: false);
	}
}
