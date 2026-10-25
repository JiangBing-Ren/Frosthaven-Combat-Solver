using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

public class DistributeDamageService : IDistributePointsService
{
	private class ActorDamage : IDistributePointsActor
	{
		private CActor actor;

		private ActorBehaviour behavior;

		private int assignedPoints;

		public CActor Actor => actor;

		public int MaxPoints => actor.MaxHealth;

		public int CurrentPoints => actor.Health;

		public int AssignedPoints
		{
			get
			{
				return assignedPoints;
			}
			set
			{
				assignedPoints = value;
				if (AssignedPoints == 0)
				{
					behavior.m_WorldspacePanelUI?.ResetDamagePreview(0);
				}
				else if (AssignedPoints > 0)
				{
					behavior.m_WorldspacePanelUI?.PreviewHeal(AssignedPoints);
				}
				else
				{
					behavior.m_WorldspacePanelUI?.PreviewDamage(-AssignedPoints);
				}
			}
		}

		public string Name
		{
			get
			{
				if (!(actor is CHeroSummonActor { IsCompanionSummon: not false } cHeroSummonActor))
				{
					return LocalizationManager.GetTranslation(actor.ActorLocKey());
				}
				return LocalizationManager.GetTranslation(actor.ActorLocKey()) + " <sprite name=\"" + cHeroSummonActor.SummonData.Model + "\">";
			}
		}

		public Sprite Portrait => UIInfoTools.Instance.GetActorPortrait(actor.GetPrefabName());

		public ActorDamage(CActor actor)
		{
			this.actor = actor;
			assignedPoints = 0;
			behavior = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor));
			behavior.m_WorldspacePanelUI?.Focus(focus: true);
		}

		public bool CanRemovePoints()
		{
			return CurrentPoints + AssignedPoints > 0;
		}

		public bool CanAssignPoints()
		{
			return CurrentPoints + AssignedPoints < MaxPoints;
		}
	}

	private CAbility ability;

	private List<ActorDamage> actors;

	private CActor caster;

	public int AvailablePoints => Math.Abs(actors.Sum((ActorDamage it) => it.AssignedPoints));

	public DistributeDamageService(CActor caster, List<CActor> actors, CAbility ability)
	{
		this.caster = caster;
		this.ability = ability;
		this.actors = actors.ConvertAll((CActor it) => new ActorDamage(it));
	}

	public List<IDistributePointsActor> GetActors()
	{
		return actors.OrderBy((ActorDamage it) =>
		{
			if (it.Actor == caster)
			{
				return 0;
			}
			return (it.Actor is CHeroSummonActor { IsCompanionSummon: not false } cHeroSummonActor && cHeroSummonActor.Summoner == caster) ? 1 : 2;
		}).ThenBy((ActorDamage it) => it.Actor.ActorLocKey()).Cast<IDistributePointsActor>()
			.ToList();
	}

	public Sprite GetTitleIcon()
	{
		if (!ability.PreviewEffectId.IsNullOrEmpty())
		{
			return UIInfoTools.Instance.GetPreviewEffectConfig(ability.PreviewEffectId)?.previewEffectIcon;
		}
		return null;
	}

	public string GetTitleText()
	{
		if (!HasDamageToAssign())
		{
			return LocalizationManager.GetTranslation("GUI_DISTRIBUTE_HEALTH_UNAVAILABLE");
		}
		if (AvailablePoints == 0)
		{
			return LocalizationManager.GetTranslation("GUI_DISTRIBUTE_HEALTH");
		}
		return string.Format(LocalizationManager.GetTranslation("GUI_DISTRIBUTE_HEALTH_POINTS"), AvailablePoints);
	}

	public bool HasDamageToAssign()
	{
		return actors.Any((ActorDamage it) => it.CurrentPoints < it.MaxPoints);
	}

	public List<Tuple<CActor, int>> GetHealthChanges()
	{
		return actors.Select((ActorDamage it) => new Tuple<CActor, int>(it.Actor, it.CurrentPoints + it.AssignedPoints)).ToList();
	}

	public void Reset()
	{
		foreach (ActorDamage actor in actors)
		{
			actor.AssignedPoints = 0;
		}
	}

	public bool CanRemovePointsFrom(IDistributePointsActor actor)
	{
		if ((!FFSNetwork.IsOnline || caster.IsUnderMyControl()) && ((ActorDamage)actor).CanRemovePoints())
		{
			return actors.Any((ActorDamage it) => it.CurrentPoints < it.MaxPoints);
		}
		return false;
	}

	public bool CanAddPointsTo(IDistributePointsActor actor)
	{
		if ((!FFSNetwork.IsOnline || caster.IsUnderMyControl()) && ((ActorDamage)actor).CanAssignPoints())
		{
			return AvailablePoints > 0;
		}
		return false;
	}

	public void AddPoint(IDistributePointsActor actor, bool networkAction = true)
	{
		((ActorDamage)actor).AssignedPoints++;
		if (networkAction)
		{
			NetworkHealthRedistribution(((ActorDamage)actor).Actor, addHealthPoint: true);
		}
	}

	public void RemovePoint(IDistributePointsActor actor, bool networkAction = true)
	{
		((ActorDamage)actor).AssignedPoints--;
		if (networkAction)
		{
			NetworkHealthRedistribution(((ActorDamage)actor).Actor, addHealthPoint: false);
		}
	}

	private void NetworkHealthRedistribution(CActor actor, bool addHealthPoint)
	{
		if (FFSNetwork.IsOnline && caster.IsUnderMyControl())
		{
			Synchronizer.SendGameActionClassID(GameActionType.RedistributeHealthAssign, ActionPhaseType.HealthRedistribution, actor.ID, (int)actor.Type, addHealthPoint, actor.Class.ID);
		}
	}

	public int GetAssignedPoints(IDistributePointsActor actor)
	{
		return ((ActorDamage)actor).AssignedPoints;
	}

	public IDistributePointsActor GetActor(GameAction gameAction)
	{
		return actors.SingleOrDefault((ActorDamage x) => x.Actor.Type == (CActor.EType)gameAction.SupplementaryDataIDMin && x.Actor.ID == gameAction.ActorID && x.Actor.Class.ID == gameAction.ClassID);
	}

	public int GetMaxPoints(IDistributePointsActor actor)
	{
		return ((ActorDamage)actor).MaxPoints;
	}

	public int GetCurrentPoints(IDistributePointsActor actor)
	{
		return ((ActorDamage)actor).CurrentPoints;
	}

	public int GetAssignedPoints(CActor actor)
	{
		return actors.First((ActorDamage it) => it.Actor == actor).AssignedPoints;
	}
}
