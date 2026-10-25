using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityAdjustInitiative : CAbilityTargeting
{
	public CAbilityAdjustInitiative()
		: base(EAbilityType.AdjustInitiative)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Error: Unable to find base ability card for ability " + Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
			ResetPreview(actor);
			if (!ActiveBonusData.IsToggleBonus && ActiveBonusData.GetMiscVar("ApplyToAllOfSameClass").GetBool())
			{
				foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x.Class == actor.Class || (x.Class is CMonsterClass cMonsterClass && cMonsterClass.NonEliteVariant == actor.Class)))
				{
					if (!ActorsToTarget.Contains(item) && item != actor)
					{
						abilityBaseCard.AddActiveBonus(this, item, TargetingActor);
					}
					if (item.InitiativeLockedDuringRound.HasValue)
					{
						item.RecalcLockedInitiative();
					}
				}
				GameState.SortIntoInitiativeAndIDOrder();
				CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		return true;
	}

	private void ResetPreview(CActor actor)
	{
		if (Ability.ActiveBonusData.IsToggleBonus)
		{
			return;
		}
		if (Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative))
		{
			actor.Class.UpdateInitiativeBonusPreview(-Strength);
			if (actor.Class is CMonsterClass { NonEliteVariant: var nonEliteVariant } cMonsterClass)
			{
				nonEliteVariant?.UpdateInitiativeBonusPreview(-Strength);
				cMonsterClass.EliteVariant?.UpdateInitiativeBonusPreview(-Strength);
			}
		}
		else if (Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.SetInitiative))
		{
			actor.Class.ResetSetInitiative();
			if (actor.Class is CMonsterClass { NonEliteVariant: var nonEliteVariant2 } cMonsterClass2)
			{
				nonEliteVariant2?.ResetSetInitiative();
				cMonsterClass2.EliteVariant?.ResetSetInitiative();
			}
		}
	}

	public override void ClearTargets()
	{
		foreach (CActor item in ActorsToTarget)
		{
			ResetPreview(item);
		}
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
		base.ClearTargets();
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		List<CActor> oldActors = ActorsToTarget.ToList();
		base.TileSelected(selectedTile, optionalTileList);
		RefreshInitiativeBonusPreview(oldActors);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		List<CActor> oldActors = ActorsToTarget.ToList();
		base.TileDeselected(selectedTile, optionalTileList);
		RefreshInitiativeBonusPreview(oldActors);
	}

	private void RefreshInitiativeBonusPreview(List<CActor> oldActors)
	{
		if (!ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative) || ActiveBonusData.IsToggleBonus)
		{
			return;
		}
		List<CActor> list = ActorsToTarget.ToList();
		List<CActor> list2 = list.Except(oldActors).ToList();
		List<CActor> list3 = oldActors.Except(list).ToList();
		foreach (CActor item in list2)
		{
			CAdjustInitiativeActiveBonus_AdjustInitiative.ApplyInitiativePreview(Strength, item);
		}
		foreach (CActor item2 in list3)
		{
			CAdjustInitiativeActiveBonus_AdjustInitiative.ApplyInitiativePreview(-Strength, item2);
		}
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}
}
