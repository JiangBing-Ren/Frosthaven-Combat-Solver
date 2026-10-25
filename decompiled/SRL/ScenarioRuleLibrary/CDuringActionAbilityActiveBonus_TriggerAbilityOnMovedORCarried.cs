using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedORCarried : CBespokeBehaviour
{
	private CAbilityAddActiveBonus m_AbilityAddActiveBonus;

	private int m_Moved;

	private int m_MoveLimit;

	private bool m_RequiresHazardousTerrain;

	private bool m_RequiresDifficultTerrain;

	private int m_DifficultTerrainTilesEntered;

	private int m_HazardousTerrainTilesEntered;

	private CActor m_LastCheckedActor;

	private TileIndex m_LastCheckedActorPosition;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnMovedORCarried;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedORCarried(CActor actor, CAbility ability, CActiveBonus activeBonus)
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
		if (IsValidAbilityType(moveAbility) && IsValidTarget(moveAbility, movedActor, useTargetOriginalType: true) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(movedActor) && RequirementsMet(moveAbility, movedActor, moveHexes, finalMovement, difficultTerrainTilesEntered, hazardousTerrainTilesEntered))
		{
			DoAbility(movedActor);
		}
	}

	public override void OnCarried(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes)
	{
		if (IsValidAbilityType(moveAbility) && IsValidTarget(moveAbility, movedActor) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(movedActor) && RequirementsMet(moveAbility, movedActor, moveHexes, finalMovement, difficultTerrainTilesEntered, hazardousTerrainTilesEntered))
		{
			DoAbility(movedActor);
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
		bool flag = m_LastCheckedActorPosition == null || m_LastCheckedActorPosition.X != m_LastCheckedActor.ArrayIndex.X || m_LastCheckedActorPosition.Y != m_LastCheckedActor.ArrayIndex.Y || moveAbility.AbilityType == CAbility.EAbilityType.Push || moveAbility.AbilityType == CAbility.EAbilityType.Pull || moveAbility.AbilityType == CAbility.EAbilityType.Teleport2;
		m_LastCheckedActorPosition = new TileIndex(m_LastCheckedActor.ArrayIndex);
		if ((((m_MoveLimit > 0 && m_Moved >= m_MoveLimit) || m_MoveLimit == 0) & flag) && (!m_RequiresDifficultTerrain || m_DifficultTerrainTilesEntered > 0))
		{
			if (m_RequiresHazardousTerrain)
			{
				return m_HazardousTerrainTilesEntered > 0;
			}
			return true;
		}
		return false;
	}

	private void DoAbility(CActor actor)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : actor);
		CAbility cAbility = CAbility.CopyAbility(m_AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, null, actor, cActor))
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
