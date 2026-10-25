using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnFinishedMovement : CBespokeBehaviour
{
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

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnFinishedMovement(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		m_AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
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
	}

	public override void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
		if (PhaseManager.CurrentPhaseAction != null && RequirementsMet(moveAbility, movedActor, moveHexes, finalMovement, difficultTerrainTilesEntered, hazardousTerrainTilesEntered) && IsValidAbilityType(moveAbility) && IsValidTarget(moveAbility, movedActor))
		{
			if (!moveAbility.AbilityHasBeenCancelled)
			{
				GameState.ShowToggleActiveBonusSelection(movedActor as CPlayerActor, movedActor, moveAbility, CAbility.EAbilityType.None, BehaviourType);
			}
			if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(movedActor))
			{
				DoAbility(movedActor, moveAbility);
			}
		}
	}

	public bool RequirementsMet(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered)
	{
		m_Moved += moveHexes;
		m_DifficultTerrainTilesEntered += difficultTerrainTilesEntered;
		m_HazardousTerrainTilesEntered += hazardousTerrainTilesEntered;
		if (m_LastCheckedActor != movedActor)
		{
			m_LastCheckedActor = movedActor;
			m_LastCheckedActorPosition = null;
		}
		m_LastCheckedActorPosition = new TileIndex(m_LastCheckedActor.ArrayIndex);
		if ((((m_MoveLimit > 0 && m_Moved >= m_MoveLimit) || m_MoveLimit == 0) & finalMovement) && (!m_RequiresDifficultTerrain || m_DifficultTerrainTilesEntered > 0))
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
		if (m_ActiveBonusData.GetMiscVar("TriggerOnSummoner").GetBool() && cActor is CHeroSummonActor { Summoner: not null } cHeroSummonActor)
		{
			cActor = cHeroSummonActor.Summoner;
		}
		CAbility cAbility = CAbility.CopyAbility(GetAbilityToAdd(m_AbilityAddActiveBonus), generateNewID: false);
		List<CTile> val = null;
		if (moveAbility is CAbilityMove { TilesMoved: var tilesMoved })
		{
			val = tilesMoved.Select((Point x) => ScenarioManager.GetTile(x)).ToList();
		}
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, moveAbility, actor, cActor, ("TilesMoved", new TValue(val))))
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
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
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
}
