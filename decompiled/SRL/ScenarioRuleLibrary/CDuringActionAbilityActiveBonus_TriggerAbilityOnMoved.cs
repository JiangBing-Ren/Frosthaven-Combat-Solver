using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnMoved : CBespokeBehaviour
{
	public class TriggerAbilityOnMovedSerialization
	{
		public int m_Moved;

		public int m_MoveLimit;

		public bool m_RequiresHazardousTerrain;

		public bool m_RequiresDifficultTerrain;

		public int m_DifficultTerrainTilesEntered;

		public int m_HazardousTerrainTilesEntered;

		public string m_LastCheckedActorGUID;

		public Point? m_LastCheckedActorPosition;
	}

	[StateDump(Skip = true)]
	private CAbilityAddActiveBonus m_AbilityAddActiveBonus;

	[StateDump(Skip = true)]
	private int m_Moved;

	[StateDump(Skip = true)]
	private int m_MoveLimit;

	[StateDump(Skip = true)]
	private bool m_RequiresHazardousTerrain;

	[StateDump(Skip = true)]
	private bool m_RequiresDifficultTerrain;

	[StateDump(Skip = true)]
	private int m_DifficultTerrainTilesEntered;

	[StateDump(Skip = true)]
	private int m_HazardousTerrainTilesEntered;

	[StateDump(Skip = true)]
	private CActor m_LastCheckedActor;

	[StateDump(Skip = true)]
	private TileIndex m_LastCheckedActorPosition;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnMoved;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnMoved(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		m_AbilityAddActiveBonus = (CAbilityAddActiveBonus)ability;
		m_MoveLimit = m_AbilityAddActiveBonus.ActiveBonusData.TargetCount;
		m_RequiresHazardousTerrain = ability.ActiveBonusData.Requirements != null && ability.ActiveBonusData.Requirements.EnterHazardousTerrain.HasValue && ability.ActiveBonusData.Requirements.EnterHazardousTerrain.Value;
		m_RequiresDifficultTerrain = ability.ActiveBonusData.Requirements != null && ability.ActiveBonusData.Requirements.EnterDifficultTerrain.HasValue && ability.ActiveBonusData.Requirements.EnterDifficultTerrain.Value;
	}

	public override void OnBehaviourTriggered()
	{
		base.OnBehaviourTriggered();
		m_Moved = 0;
		m_DifficultTerrainTilesEntered = 0;
		m_HazardousTerrainTilesEntered = 0;
	}

	public override void OnRestrictionReset()
	{
		m_Moved = 0;
		m_DifficultTerrainTilesEntered = 0;
		m_HazardousTerrainTilesEntered = 0;
		m_LastCheckedActor = null;
		m_LastCheckedActorPosition = null;
	}

	public override void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
		if (thisMoveHexes != 0 && IsValidAbilityType(moveAbility) && IsValidTarget(moveAbility, movedActor) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(movedActor) && m_ActiveBonus.RequirementsMet(movedActor, moveAbility) && RequirementsMet(moveAbility, movedActor, moveHexes, finalMovement, difficultTerrainTilesEntered, hazardousTerrainTilesEntered, prevPosition))
		{
			DoAbility(movedActor, moveAbility);
		}
	}

	public bool RequirementsMet(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, Point prevPosition)
	{
		m_Moved += moveHexes;
		m_DifficultTerrainTilesEntered += difficultTerrainTilesEntered;
		m_HazardousTerrainTilesEntered += hazardousTerrainTilesEntered;
		if (m_LastCheckedActor != movedActor)
		{
			m_LastCheckedActor = movedActor;
			m_LastCheckedActorPosition = null;
		}
		bool flag = false;
		AbilityData.EActiveBonusFlag eActiveBonusFlag = AbilityData.EActiveBonusFlag.NotInRangeToInRangeMove | AbilityData.EActiveBonusFlag.InRangeToNotInRangeMove;
		if ((m_ActiveBonusData.Flags & eActiveBonusFlag) != 0)
		{
			List<Point> list = new List<Point>();
			list.Add(prevPosition);
			list.Add(movedActor.ArrayIndex);
			for (int i = 0; i < list.Count - 1; i++)
			{
				Point p = list[i];
				Point p2 = list[i + 1];
				bool flag2 = Point.AxialDistance(Point.OddRToAxial(p), Point.OddRToAxial(m_Actor.ArrayIndex)) <= m_Ability.Range;
				bool flag3 = Point.AxialDistance(Point.OddRToAxial(p2), Point.OddRToAxial(m_Actor.ArrayIndex)) <= m_Ability.Range;
				if ((!flag2 & flag3) && (m_ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.NotInRangeToInRangeMove) != 0)
				{
					flag = true;
					break;
				}
				if (flag2 && !flag3 && (m_ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.InRangeToNotInRangeMove) != 0)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		bool flag4 = m_LastCheckedActorPosition == null || m_LastCheckedActorPosition.X != m_LastCheckedActor.ArrayIndex.X || m_LastCheckedActorPosition.Y != m_LastCheckedActor.ArrayIndex.Y || moveAbility.AbilityType == CAbility.EAbilityType.Push || moveAbility.AbilityType == CAbility.EAbilityType.Pull || moveAbility.AbilityType == CAbility.EAbilityType.Teleport2;
		m_LastCheckedActorPosition = new TileIndex(m_LastCheckedActor.ArrayIndex);
		if ((flag & flag4) && ((m_MoveLimit > 0 && m_Moved >= m_MoveLimit) || m_MoveLimit == 0) && (!m_RequiresDifficultTerrain || m_DifficultTerrainTilesEntered > 0))
		{
			if (m_RequiresHazardousTerrain)
			{
				return m_HazardousTerrainTilesEntered > 0;
			}
			return true;
		}
		return false;
	}

	private void DoAbility(CActor actor, CAbility moveAbility)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : actor);
		CAbility cAbility = CAbility.CopyAbility(m_AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, moveAbility, actor, cActor))
		{
			cAbility.CanUndoOverride = false;
			if (cAbility.IsInlineSubAbility)
			{
				cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			}
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			if (GameState.InternalCurrentActor != cActor)
			{
				GameState.OverrideCurrentActorForOneAction(cActor);
			}
			if (PhaseManager.CurrentPhaseAction != null)
			{
				PhaseManager.CurrentPhaseAction.StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, m_ActiveBonus.TriggerAbilitiesImmediately);
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
			OnRestrictionReset();
		}
	}

	protected override bool IsValidAbilityType(CAbility conditionAbility)
	{
		if (m_ActiveBonusData.ValidAbilityTypes.Count <= 0)
		{
			if (conditionAbility.AbilityType != CAbility.EAbilityType.Move && conditionAbility.AbilityType != CAbility.EAbilityType.Push && conditionAbility.AbilityType != CAbility.EAbilityType.Pull && conditionAbility.AbilityType != CAbility.EAbilityType.Teleport2)
			{
				return conditionAbility.AbilityType == CAbility.EAbilityType.Fear;
			}
			return true;
		}
		return m_ActiveBonusData.ValidAbilityTypes.Contains(conditionAbility.AbilityType);
	}

	public override void Save(List<Tuple<string, object>> toSave)
	{
		base.Save(toSave);
		TriggerAbilityOnMovedSerialization value = new TriggerAbilityOnMovedSerialization
		{
			m_Moved = m_Moved,
			m_MoveLimit = m_MoveLimit,
			m_RequiresHazardousTerrain = m_RequiresHazardousTerrain,
			m_RequiresDifficultTerrain = m_RequiresDifficultTerrain,
			m_DifficultTerrainTilesEntered = m_DifficultTerrainTilesEntered,
			m_HazardousTerrainTilesEntered = m_HazardousTerrainTilesEntered,
			m_LastCheckedActorGUID = m_LastCheckedActor?.ActorGuid,
			m_LastCheckedActorPosition = m_LastCheckedActorPosition?.ToPoint()
		};
		toSave.Add(new Tuple<string, object>("TriggerAbilityOnMovedSerialization", JsonConvert.SerializeObject(value)));
	}

	public override void Load(List<Tuple<string, object>> toSave)
	{
		base.Load(toSave);
		Tuple<string, object> tuple = toSave.FirstOrDefault((Tuple<string, object> x) => x.Item1.Equals("TriggerAbilityOnMovedSerialization", StringComparison.InvariantCultureIgnoreCase));
		if (tuple != null)
		{
			TriggerAbilityOnMovedSerialization triggerAbilityOnMovedSerialization = JsonConvert.DeserializeObject<TriggerAbilityOnMovedSerialization>(tuple.Item2 as string);
			if (triggerAbilityOnMovedSerialization != null)
			{
				m_Moved = triggerAbilityOnMovedSerialization.m_Moved;
				m_MoveLimit = triggerAbilityOnMovedSerialization.m_MoveLimit;
				m_RequiresHazardousTerrain = triggerAbilityOnMovedSerialization.m_RequiresHazardousTerrain;
				m_RequiresDifficultTerrain = triggerAbilityOnMovedSerialization.m_RequiresDifficultTerrain;
				m_DifficultTerrainTilesEntered = triggerAbilityOnMovedSerialization.m_DifficultTerrainTilesEntered;
				m_HazardousTerrainTilesEntered = triggerAbilityOnMovedSerialization.m_HazardousTerrainTilesEntered;
				m_LastCheckedActor = ScenarioManager.FindActor(triggerAbilityOnMovedSerialization.m_LastCheckedActorGUID);
				m_LastCheckedActorPosition = (triggerAbilityOnMovedSerialization.m_LastCheckedActorPosition.HasValue ? new TileIndex(triggerAbilityOnMovedSerialization.m_LastCheckedActorPosition.Value) : null);
			}
		}
	}
}
