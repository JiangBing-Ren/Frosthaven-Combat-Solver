using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsScenario : CPlayerStats, ISerializable
{
	public bool PlayerSurvivedScenario { get; set; }

	public int RoundsPlayed { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PlayerSurvivedScenario", PlayerSurvivedScenario);
		info.AddValue("RoundsPlayed", RoundsPlayed);
	}

	public CPlayerStatsScenario(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PlayerSurvivedScenario"))
				{
					if (name == "RoundsPlayed")
					{
						RoundsPlayed = info.GetInt32("RoundsPlayed");
					}
				}
				else
				{
					PlayerSurvivedScenario = info.GetBoolean("PlayerSurvivedScenario");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsScenario entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsScenario(CPlayerStatsScenario original)
		: base(original)
	{
		PlayerSurvivedScenario = original.PlayerSurvivedScenario;
		RoundsPlayed = original.RoundsPlayed;
	}

	public CPlayerStatsScenario(string characterID)
		: base(characterID)
	{
		PlayerSurvivedScenario = false;
	}

	public CPlayerStatsScenario GetPlayerStats(int requiredRound, bool turn)
	{
		if (turn)
		{
			CPlayerStatsScenario playerStatsForRound = GetPlayerStatsForRound(requiredRound);
			CPlayerStatsStartTurn startTurn = playerStatsForRound.StartTurn.FirstOrDefault();
			CPlayerStatsEndTurn endTurn = playerStatsForRound.EndTurn.FirstOrDefault();
			return playerStatsForRound.GetPlayerStatsForTurn(startTurn, endTurn);
		}
		return GetPlayerStatsForRound(requiredRound);
	}

	public CPlayerStatsScenario GetPlayerStatsForRound(int requiredRound)
	{
		CPlayerStatsScenario cPlayerStatsScenario = new CPlayerStatsScenario(CharacterID);
		cPlayerStatsScenario.PlayerSurvivedScenario = PlayerSurvivedScenario;
		cPlayerStatsScenario.RoundsPlayed = RoundsPlayed;
		cPlayerStatsScenario.Kills.AddRange(Kills.Where((CPlayerStatsKill x) => x.Round == requiredRound));
		cPlayerStatsScenario.Deaths.AddRange(Deaths.Where((CPlayerStatsKill x) => x.Round == requiredRound));
		cPlayerStatsScenario.DamageDealt.AddRange(DamageDealt.Where((CPlayerStatsDamage x) => x.Round == requiredRound));
		cPlayerStatsScenario.ActorsInitialized.AddRange(ActorsInitialized.Where((CPlayerStatsAction x) => x.Round == requiredRound));
		cPlayerStatsScenario.DamageReceived.AddRange(DamageReceived.Where((CPlayerStatsDamage x) => x.Round == requiredRound));
		cPlayerStatsScenario.DestroyedObstacles.AddRange(DestroyedObstacles.Where((CPlayerStatsDestroyObstacle x) => x.Round == requiredRound));
		cPlayerStatsScenario.PropsDestroyedByAbilities.AddRange(PropsDestroyedByAbilities.Where((CPlayerStatsDestroyProp x) => x.Round == requiredRound));
		cPlayerStatsScenario.PropsCreatedByAbilities.AddRange(PropsCreatedByAbilities.Where((CPlayerStatsCreateProp x) => x.Round == requiredRound));
		cPlayerStatsScenario.Infusions.AddRange(Infusions.Where((CPlayerStatsElement x) => x.Round == requiredRound));
		cPlayerStatsScenario.Consumed.AddRange(Consumed.Where((CPlayerStatsElement x) => x.Round == requiredRound));
		cPlayerStatsScenario.Abilities.AddRange(Abilities.Where((CPlayerStatsAbilities x) => x.Round == requiredRound));
		cPlayerStatsScenario.ActionsEnded.AddRange(ActionsEnded.Where((CPlayerStatsActionEnded x) => x.Round == requiredRound));
		cPlayerStatsScenario.Modifiers.AddRange(Modifiers.Where((CPlayerStatsModifiers x) => x.Round == requiredRound));
		cPlayerStatsScenario.Heals.AddRange(Heals.Where((CPlayerStatsHeal x) => x.Round == requiredRound));
		cPlayerStatsScenario.Items.AddRange(Items.Where((CPlayerStatsItem x) => x.Round == requiredRound));
		cPlayerStatsScenario.Loot.AddRange(Loot.Where((CPlayerStatsLoot x) => x.Round == requiredRound));
		cPlayerStatsScenario.Monsters.AddRange(Monsters.Where((CPlayerStatsMonsters x) => x.Round == requiredRound));
		cPlayerStatsScenario.Door.AddRange(Door.Where((CPlayerStatsDoor x) => x.Round == requiredRound));
		cPlayerStatsScenario.ActivatedProps.AddRange(ActivatedProps.Where((CPlayerStatsPropActivated x) => x.Round == requiredRound));
		cPlayerStatsScenario.Trap.AddRange(Trap.Where((CPlayerStatsTrap x) => x.Round == requiredRound));
		cPlayerStatsScenario.Hand.AddRange(Hand.Where((CPlayerStatsHand x) => x.Round == requiredRound));
		cPlayerStatsScenario.XP.AddRange(XP.Where((CPlayerStatsXP x) => x.Round == requiredRound));
		cPlayerStatsScenario.BattleGoalPerks.AddRange(BattleGoalPerks.Where((CPlayerStatsBattlePerks x) => x.Round == requiredRound));
		cPlayerStatsScenario.Donations.AddRange(Donations.Where((CPlayerStatsDonations x) => x.Round == requiredRound));
		cPlayerStatsScenario.PersonalQuests.AddRange(PersonalQuests.Where((CPlayerStatsPersonalQuests x) => x.Round == requiredRound));
		cPlayerStatsScenario.Enhancements.AddRange(Enhancements.Where((CPlayerStatsEnhancements x) => x.Round == requiredRound));
		cPlayerStatsScenario.LoseCards.AddRange(LoseCards.Where((CPlayerStatsLoseCard x) => x.Round == requiredRound));
		cPlayerStatsScenario.RecoverCards.AddRange(RecoverCards.Where((CPlayerStatsRecoverCard x) => x.Round == requiredRound));
		cPlayerStatsScenario.DiscardCard.AddRange(DiscardCard.Where((CPlayerStatsDiscardCard x) => x.Round == requiredRound));
		cPlayerStatsScenario.EndTurn.AddRange(EndTurn.Where((CPlayerStatsEndTurn x) => x.Round == requiredRound));
		cPlayerStatsScenario.EndRound.AddRange(EndRound.Where((CPlayerStatsEndRound x) => x.Round == requiredRound));
		cPlayerStatsScenario.StartTurn.AddRange(StartTurn.Where((CPlayerStatsStartTurn x) => x.Round == requiredRound));
		cPlayerStatsScenario.LostAdjacency.AddRange(LostAdjacency.Where((CPlayerStatsLostAdjacency x) => x.Round == requiredRound));
		cPlayerStatsScenario.HeroSummons.AddRange(HeroSummons.Where((CPlayerStatsHeroSummon x) => x.Round == requiredRound));
		cPlayerStatsScenario.Agility.AddRange(Agility.Where((CPlayerStatsAgility x) => x.Round == requiredRound));
		cPlayerStatsScenario.AttackedByAbilities.AddRange(AttackedByAbilities.Where((CPlayerStatsAbilities x) => x.Round == requiredRound));
		cPlayerStatsScenario.Form.AddRange(Form.Where((CPlayerStatsForm x) => x.Round == requiredRound));
		cPlayerStatsScenario.ActiveBonuses.AddRange(ActiveBonuses.Where((CPlayerStatsActiveBonuses x) => x.Round == requiredRound));
		cPlayerStatsScenario.ForcedActorsIntoTraps.AddRange(ForcedActorsIntoTraps.Where((CPlayerStatsForcedActorsIntoTraps x) => x.Round == requiredRound));
		cPlayerStatsScenario.ArrayIndexChanged.AddRange(ArrayIndexChanged.Where((CPlayerStatsArrayIndexChanged x) => x.Round == requiredRound));
		cPlayerStatsScenario.RemovedCondition.AddRange(RemovedCondition.Where((CPlayerStatsRemovedCondition x) => x.Round == requiredRound));
		cPlayerStatsScenario.Healed.AddRange(Healed.Where((CPlayerStatsActorHealthChanged x) => x.Round == requiredRound));
		cPlayerStatsScenario.ActorActiveBonuses.AddRange(ActorActiveBonuses.Where((CPlayerStatsActorActiveBonuses x) => x.Round == requiredRound));
		return cPlayerStatsScenario;
	}

	public CPlayerStatsScenario GetPlayerStatsForTurn(CPlayerStatsStartTurn startTurn, CPlayerStatsEndTurn endTurn)
	{
		CPlayerStatsScenario cPlayerStatsScenario = new CPlayerStatsScenario(CharacterID)
		{
			PlayerSurvivedScenario = PlayerSurvivedScenario,
			RoundsPlayed = RoundsPlayed
		};
		if (startTurn == null)
		{
			return cPlayerStatsScenario;
		}
		Func<CPlayerStatsBase, bool> predicate = (CPlayerStatsBase x) => (x == startTurn || x.HappenedAfter(startTurn)) && (endTurn == null || x == endTurn || x.HappenedBefore(endTurn));
		cPlayerStatsScenario.Kills.AddRange(Kills.Where((CPlayerStatsKill x) => predicate(x)));
		cPlayerStatsScenario.Deaths.AddRange(Deaths.Where((CPlayerStatsKill x) => predicate(x)));
		cPlayerStatsScenario.DamageDealt.AddRange(DamageDealt.Where((CPlayerStatsDamage x) => predicate(x)));
		cPlayerStatsScenario.ActorsInitialized.AddRange(ActorsInitialized.Where((CPlayerStatsAction x) => predicate(x)));
		cPlayerStatsScenario.DamageReceived.AddRange(DamageReceived.Where((CPlayerStatsDamage x) => predicate(x)));
		cPlayerStatsScenario.DestroyedObstacles.AddRange(DestroyedObstacles.Where((CPlayerStatsDestroyObstacle x) => predicate(x)));
		cPlayerStatsScenario.PropsDestroyedByAbilities.AddRange(PropsDestroyedByAbilities.Where((CPlayerStatsDestroyProp x) => predicate(x)));
		cPlayerStatsScenario.PropsCreatedByAbilities.AddRange(PropsCreatedByAbilities.Where((CPlayerStatsCreateProp x) => predicate(x)));
		cPlayerStatsScenario.Infusions.AddRange(Infusions.Where((CPlayerStatsElement x) => predicate(x)));
		cPlayerStatsScenario.Consumed.AddRange(Consumed.Where((CPlayerStatsElement x) => predicate(x)));
		cPlayerStatsScenario.Abilities.AddRange(Abilities.Where((CPlayerStatsAbilities x) => predicate(x)));
		cPlayerStatsScenario.ActionsEnded.AddRange(ActionsEnded.Where((CPlayerStatsActionEnded x) => predicate(x)));
		cPlayerStatsScenario.Modifiers.AddRange(Modifiers.Where((CPlayerStatsModifiers x) => predicate(x)));
		cPlayerStatsScenario.Heals.AddRange(Heals.Where((CPlayerStatsHeal x) => predicate(x)));
		cPlayerStatsScenario.Items.AddRange(Items.Where((CPlayerStatsItem x) => predicate(x)));
		cPlayerStatsScenario.Loot.AddRange(Loot.Where((CPlayerStatsLoot x) => predicate(x)));
		cPlayerStatsScenario.Monsters.AddRange(Monsters.Where((CPlayerStatsMonsters x) => predicate(x)));
		cPlayerStatsScenario.Door.AddRange(Door.Where((CPlayerStatsDoor x) => predicate(x)));
		cPlayerStatsScenario.ActivatedProps.AddRange(ActivatedProps.Where((CPlayerStatsPropActivated x) => predicate(x)));
		cPlayerStatsScenario.Trap.AddRange(Trap.Where((CPlayerStatsTrap x) => predicate(x)));
		cPlayerStatsScenario.Hand.AddRange(Hand.Where((CPlayerStatsHand x) => predicate(x)));
		cPlayerStatsScenario.XP.AddRange(XP.Where((CPlayerStatsXP x) => predicate(x)));
		cPlayerStatsScenario.BattleGoalPerks.AddRange(BattleGoalPerks.Where((CPlayerStatsBattlePerks x) => predicate(x)));
		cPlayerStatsScenario.Donations.AddRange(Donations.Where((CPlayerStatsDonations x) => predicate(x)));
		cPlayerStatsScenario.PersonalQuests.AddRange(PersonalQuests.Where((CPlayerStatsPersonalQuests x) => predicate(x)));
		cPlayerStatsScenario.Enhancements.AddRange(Enhancements.Where((CPlayerStatsEnhancements x) => predicate(x)));
		cPlayerStatsScenario.LoseCards.AddRange(LoseCards.Where((CPlayerStatsLoseCard x) => predicate(x)));
		cPlayerStatsScenario.RecoverCards.AddRange(RecoverCards.Where((CPlayerStatsRecoverCard x) => predicate(x)));
		cPlayerStatsScenario.DiscardCard.AddRange(DiscardCard.Where((CPlayerStatsDiscardCard x) => predicate(x)));
		cPlayerStatsScenario.EndTurn.AddRange(EndTurn.Where((CPlayerStatsEndTurn x) => predicate(x)));
		cPlayerStatsScenario.EndRound.AddRange(EndRound.Where((CPlayerStatsEndRound x) => predicate(x)));
		cPlayerStatsScenario.StartTurn.AddRange(StartTurn.Where((CPlayerStatsStartTurn x) => predicate(x)));
		cPlayerStatsScenario.LostAdjacency.AddRange(LostAdjacency.Where((CPlayerStatsLostAdjacency x) => predicate(x)));
		cPlayerStatsScenario.HeroSummons.AddRange(HeroSummons.Where((CPlayerStatsHeroSummon x) => predicate(x)));
		cPlayerStatsScenario.Agility.AddRange(Agility.Where((CPlayerStatsAgility x) => predicate(x)));
		cPlayerStatsScenario.AttackedByAbilities.AddRange(AttackedByAbilities.Where((CPlayerStatsAbilities x) => predicate(x)));
		cPlayerStatsScenario.Form.AddRange(Form.Where((CPlayerStatsForm x) => predicate(x)));
		cPlayerStatsScenario.ActiveBonuses.AddRange(ActiveBonuses.Where((CPlayerStatsActiveBonuses x) => predicate(x)));
		cPlayerStatsScenario.ForcedActorsIntoTraps.AddRange(ForcedActorsIntoTraps.Where((CPlayerStatsForcedActorsIntoTraps x) => predicate(x)));
		cPlayerStatsScenario.ArrayIndexChanged.AddRange(ArrayIndexChanged.Where((CPlayerStatsArrayIndexChanged x) => predicate(x)));
		cPlayerStatsScenario.RemovedCondition.AddRange(RemovedCondition.Where((CPlayerStatsRemovedCondition x) => predicate(x)));
		cPlayerStatsScenario.Healed.AddRange(Healed.Where((CPlayerStatsActorHealthChanged x) => predicate(x)));
		cPlayerStatsScenario.ActorActiveBonuses.AddRange(ActorActiveBonuses.Where((CPlayerStatsActorActiveBonuses x) => predicate(x)));
		return cPlayerStatsScenario;
	}
}
