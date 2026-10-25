using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

public class DistributeSelectPlayerActorService : IDistributePointsService
{
	private class SelectPlayerActor : IDistributePointsActor
	{
		private CPlayerActor m_Actor;

		private bool m_IsSelected;

		public CActor Actor => m_Actor;

		public bool IsSelected
		{
			get
			{
				return m_IsSelected;
			}
			set
			{
				m_IsSelected = value;
			}
		}

		public string Name => LocalizationManager.GetTranslation(m_Actor.ActorLocKey());

		public Sprite Portrait => UIInfoTools.Instance.GetCharacterDistributionPortrait(m_Actor.CharacterClass.CharacterModel, m_Actor.CharacterClass.CharacterYML.CustomCharacterConfig);

		public SelectPlayerActor(CPlayerActor actor)
		{
			m_Actor = actor;
			m_IsSelected = false;
		}

		public bool CanRemovePoints()
		{
			return IsSelected;
		}

		public bool CanAssignPoints()
		{
			return !IsSelected;
		}
	}

	private string titleLocKey;

	private List<SelectPlayerActor> actors;

	public int AvailablePoints
	{
		get
		{
			if (!actors.Exists((SelectPlayerActor it) => it.IsSelected))
			{
				return 1;
			}
			return 0;
		}
	}

	public DistributeSelectPlayerActorService(List<CPlayerActor> actors, string titleLocKey)
	{
		this.actors = actors.ConvertAll((CPlayerActor it) => new SelectPlayerActor(it));
		this.titleLocKey = titleLocKey;
	}

	public List<IDistributePointsActor> GetActors()
	{
		return actors.Cast<IDistributePointsActor>().ToList();
	}

	public Sprite GetTitleIcon()
	{
		return null;
	}

	public string GetTitleText()
	{
		return LocalizationManager.GetTranslation(titleLocKey);
	}

	public CActor GetSelectedActor()
	{
		return actors.SingleOrDefault((SelectPlayerActor it) => it.IsSelected)?.Actor;
	}

	public void Reset()
	{
		foreach (SelectPlayerActor actor in actors)
		{
			actor.IsSelected = false;
		}
	}

	public void ResetPoints(bool networkAction = true)
	{
		foreach (SelectPlayerActor actor in actors)
		{
			RemovePoint(actor, networkAction);
		}
	}

	public bool CanRemovePointsFrom(IDistributePointsActor actor)
	{
		SelectPlayerActor selectPlayerActor = (SelectPlayerActor)actor;
		if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			return selectPlayerActor.CanRemovePoints();
		}
		return false;
	}

	public bool CanAddPointsTo(IDistributePointsActor actor)
	{
		SelectPlayerActor selectPlayerActor = (SelectPlayerActor)actor;
		if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			return selectPlayerActor.CanAssignPoints();
		}
		return false;
	}

	public void AddPoint(IDistributePointsActor actor, bool networkAction = true)
	{
		Reset();
		((SelectPlayerActor)actor).IsSelected = true;
		if (networkAction)
		{
			NetworkRedistribution(((SelectPlayerActor)actor).Actor, select: true);
		}
	}

	public void RemovePoint(IDistributePointsActor actor, bool networkAction = true)
	{
		((SelectPlayerActor)actor).IsSelected = false;
		if (networkAction)
		{
			NetworkRedistribution(((SelectPlayerActor)actor).Actor, select: false);
		}
	}

	private void NetworkRedistribution(CActor actor, bool select)
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			Synchronizer.SendGameActionClassID(GameActionType.DistributeUISelect, ActionPhaseType.HealthRedistribution, actor.ID, (int)actor.Type, select, actor.Class.ID);
		}
	}

	public int GetAssignedPoints(IDistributePointsActor actor)
	{
		if (!((SelectPlayerActor)actor).IsSelected)
		{
			return 0;
		}
		return 1;
	}

	public IDistributePointsActor GetActor(GameAction gameAction)
	{
		return actors.SingleOrDefault((SelectPlayerActor x) => x.Actor.Type == (CActor.EType)gameAction.SupplementaryDataIDMin && x.Actor.ID == gameAction.ActorID && x.Actor.Class.ID == gameAction.ClassID);
	}

	public int GetMaxPoints(IDistributePointsActor actor)
	{
		return 1;
	}

	public int GetCurrentPoints(IDistributePointsActor actor)
	{
		return 0;
	}
}
