using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CGenericActiveBonus : CActiveBonus
{
	public interface IListener
	{
		bool OnEvent(EActiveBonusBehaviourType evt, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, params (string, TValue)[] additionalVars);
	}

	[StateDump(Skip = true)]
	private static List<IListener>[] s_EventListeners = new List<IListener>[18];

	private static int s_InFireEvent;

	private static List<IListener> s_ProcessedListeners = new List<IListener>();

	public static void Reset()
	{
		for (int i = 0; i < s_EventListeners.Length; i++)
		{
			s_EventListeners[i]?.Clear();
		}
	}

	private static List<IListener> GetListeners(EActiveBonusBehaviourType evt, bool create)
	{
		if (evt < EActiveBonusBehaviourType.Generic_First || evt >= EActiveBonusBehaviourType.Generic_Last)
		{
			return null;
		}
		int num = (int)(evt - 103);
		List<IListener> list = s_EventListeners[num];
		if ((list == null) & create)
		{
			list = new List<IListener>();
			s_EventListeners[num] = list;
		}
		return list;
	}

	public static void AddListener(EActiveBonusBehaviourType evt, IListener listener)
	{
		List<IListener> listeners = GetListeners(evt, create: true);
		if (listeners != null && !listeners.Contains(listener))
		{
			listeners.Add(listener);
		}
	}

	public static void DelListener(EActiveBonusBehaviourType evt, IListener listener)
	{
		List<IListener> listeners = GetListeners(evt, create: false);
		if (listeners == null)
		{
			return;
		}
		int num = listeners.IndexOf(listener);
		if (num >= 0)
		{
			if (s_InFireEvent != 0)
			{
				listeners[num] = null;
			}
			else
			{
				listeners.RemoveAt(num);
			}
		}
	}

	public static void FireEvent(EActiveBonusBehaviourType evt, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, params (string, TValue)[] additionalVars)
	{
		FireEvent(evt, triggerAbility, triggerActor, triggerTarget, null, additionalVars);
	}

	public static void FireEvent(EActiveBonusBehaviourType evt, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, List<CActiveBonus> triggeredBonuses, params (string, TValue)[] additionalVars)
	{
		List<IListener> listeners = GetListeners(evt, create: false);
		if (listeners == null || listeners.Count == 0)
		{
			return;
		}
		s_ProcessedListeners.Clear();
		s_InFireEvent++;
		try
		{
			for (int i = 0; i < listeners.Count; i++)
			{
				IListener listener = listeners[i];
				if (listener == null)
				{
					listeners.RemoveAt(i);
					i--;
				}
				else
				{
					if (s_ProcessedListeners.Contains(listener))
					{
						continue;
					}
					s_ProcessedListeners.Add(listener);
					bool flag = listener.OnEvent(evt, triggerAbility, triggerActor, triggerTarget, additionalVars);
					if ((triggeredBonuses != null) & flag)
					{
						CGenericActiveBonus_BespokeBehaviour cGenericActiveBonus_BespokeBehaviour = listener as CGenericActiveBonus_BespokeBehaviour;
						if (cGenericActiveBonus_BespokeBehaviour?.ActiveBonus != null)
						{
							triggeredBonuses.Add(cGenericActiveBonus_BespokeBehaviour.ActiveBonus);
						}
					}
				}
			}
		}
		finally
		{
			s_InFireEvent--;
			s_ProcessedListeners.Clear();
		}
	}

	public CGenericActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		AddBespokeBehaviour(new CGenericActiveBonus_BespokeBehaviour(actor, ability, this));
	}

	public override bool ShouldRestartAbilityOnToggle(CAbility currentAbility)
	{
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMove cAbilityMove && cAbilityMove.ActiveBonusData.Duration == EActiveBonusDurationType.NA && cAbilityMove.HasPassedState(CAbilityMove.EMoveState.ActorIsMoving))
		{
			return false;
		}
		return base.ShouldRestartAbilityOnToggle(currentAbility);
	}
}
