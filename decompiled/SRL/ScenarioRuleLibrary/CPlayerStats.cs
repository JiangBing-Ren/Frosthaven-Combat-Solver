using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStats : ISerializable
{
	public string CharacterID { get; private set; }

	public List<CPlayerStatsKill> Kills { get; private set; }

	public List<CPlayerStatsKill> Deaths { get; private set; }

	public List<CPlayerStatsDamage> DamageDealt { get; private set; }

	public List<CPlayerStatsDamage> DamageReceived { get; private set; }

	public List<CPlayerStatsAction> ActorsInitialized { get; private set; }

	public List<CPlayerStatsDestroyObstacle> DestroyedObstacles { get; private set; }

	public List<CPlayerStatsDestroyProp> PropsDestroyedByAbilities { get; private set; }

	public List<CPlayerStatsCreateProp> PropsCreatedByAbilities { get; private set; }

	public List<CPlayerStatsElement> Infusions { get; private set; }

	public List<CPlayerStatsElement> Consumed { get; private set; }

	public List<CPlayerStatsAbilities> Abilities { get; private set; }

	public List<CPlayerStatsActionEnded> ActionsEnded { get; private set; }

	public List<CPlayerStatsModifiers> Modifiers { get; private set; }

	public List<CPlayerStatsHeal> Heals { get; private set; }

	public List<CPlayerStatsItem> Items { get; private set; }

	public List<CPlayerStatsLoot> Loot { get; private set; }

	public List<CPlayerStatsMonsters> Monsters { get; private set; }

	public List<CPlayerStatsDoor> Door { get; private set; }

	public List<CPlayerStatsPropActivated> ActivatedProps { get; private set; }

	public List<CPlayerStatsTrap> Trap { get; private set; }

	public List<CPlayerStatsHand> Hand { get; private set; }

	public List<CPlayerStatsXP> XP { get; private set; }

	public List<CPlayerStatsBattlePerks> BattleGoalPerks { get; private set; }

	public List<CPlayerStatsDonations> Donations { get; private set; }

	public List<CPlayerStatsPersonalQuests> PersonalQuests { get; private set; }

	public List<CPlayerStatsEnhancements> Enhancements { get; private set; }

	public List<CPlayerStatsLoseCard> LoseCards { get; private set; }

	public List<CPlayerStatsRecoverCard> RecoverCards { get; private set; }

	public List<CPlayerStatsDiscardCard> DiscardCard { get; private set; }

	public List<CPlayerStatsStartTurn> StartTurn { get; private set; }

	public List<CPlayerStatsEndTurn> EndTurn { get; private set; }

	public List<CPlayerStatsEndRound> EndRound { get; private set; }

	public List<CPlayerStatsLostAdjacency> LostAdjacency { get; private set; }

	public List<CPlayerStatsHeroSummon> HeroSummons { get; private set; }

	public List<CPlayerStatsAgility> Agility { get; private set; }

	public List<CPlayerStatsAbilities> AttackedByAbilities { get; private set; }

	public List<CPlayerStatsForm> Form { get; private set; }

	public List<CPlayerStatsActiveBonuses> ActiveBonuses { get; private set; }

	public List<CPlayerStatsForcedActorsIntoTraps> ForcedActorsIntoTraps { get; private set; }

	public List<CPlayerStatsArrayIndexChanged> ArrayIndexChanged { get; private set; }

	public List<CPlayerStatsRemovedCondition> RemovedCondition { get; private set; }

	public List<CPlayerStatsActorHealthChanged> Healed { get; private set; }

	public List<CPlayerStatsActorActiveBonuses> ActorActiveBonuses { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ActorActiveBonuses", ActorActiveBonuses);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Kills", Kills);
		info.AddValue("Deaths", Deaths);
		info.AddValue("DamageDealt", DamageDealt);
		info.AddValue("DamageReceived", DamageReceived);
		info.AddValue("ActorsInitialized", ActorsInitialized);
		info.AddValue("DestroyedObstacles", DestroyedObstacles);
		info.AddValue("PropsDestroyedByAbilities", PropsDestroyedByAbilities);
		info.AddValue("PropsCreatedByAbilities", PropsCreatedByAbilities);
		info.AddValue("Infusions", Infusions);
		info.AddValue("Consumed", Consumed);
		info.AddValue("Abilities", Abilities);
		info.AddValue("ActionsEnded", ActionsEnded);
		info.AddValue("Modifiers", Modifiers);
		info.AddValue("Heals", Heals);
		info.AddValue("Items", Items);
		info.AddValue("Loot", Loot);
		info.AddValue("Monsters", Monsters);
		info.AddValue("Door", Door);
		info.AddValue("ActivatedProps", ActivatedProps);
		info.AddValue("Trap", Trap);
		info.AddValue("Hand", Hand);
		info.AddValue("XP", XP);
		info.AddValue("BattleGoalPerks", BattleGoalPerks);
		info.AddValue("Donations", Donations);
		info.AddValue("PersonalQuests", PersonalQuests);
		info.AddValue("Enhancements", Enhancements);
		info.AddValue("LoseCards", LoseCards);
		info.AddValue("RecoverCards", RecoverCards);
		info.AddValue("DiscardCard", DiscardCard);
		info.AddValue("EndTurn", EndTurn);
		info.AddValue("EndRound", EndRound);
		info.AddValue("StartTurn", StartTurn);
		info.AddValue("LostAdjacency", LostAdjacency);
		info.AddValue("HeroSummons", HeroSummons);
		info.AddValue("Agility", Agility);
		info.AddValue("AttackedByAbilities", AttackedByAbilities);
		info.AddValue("Form", Form);
		info.AddValue("ActiveBonuses", ActiveBonuses);
		info.AddValue("ForcedActorsIntoTraps", ForcedActorsIntoTraps);
		info.AddValue("ArrayIndexChanged", ArrayIndexChanged);
		info.AddValue("Healed", Healed);
		info.AddValue("RemovedCondition", RemovedCondition);
	}

	public CPlayerStats(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "Kills":
					Kills = (List<CPlayerStatsKill>)info.GetValue("Kills", typeof(List<CPlayerStatsKill>));
					break;
				case "ActorActiveBonuses":
					ActorActiveBonuses = (List<CPlayerStatsActorActiveBonuses>)info.GetValue("ActorActiveBonuses", typeof(List<CPlayerStatsActorActiveBonuses>));
					break;
				case "ArrayIndexChanged":
					ArrayIndexChanged = (List<CPlayerStatsArrayIndexChanged>)info.GetValue("ArrayIndexChanged", typeof(List<CPlayerStatsArrayIndexChanged>));
					break;
				case "Healed":
					Healed = (List<CPlayerStatsActorHealthChanged>)info.GetValue("Healed", typeof(List<CPlayerStatsActorHealthChanged>));
					break;
				case "RemovedCondition":
					RemovedCondition = (List<CPlayerStatsRemovedCondition>)info.GetValue("RemovedCondition", typeof(List<CPlayerStatsRemovedCondition>));
					break;
				case "Deaths":
					Deaths = (List<CPlayerStatsKill>)info.GetValue("Deaths", typeof(List<CPlayerStatsKill>));
					break;
				case "DamageDealt":
					DamageDealt = (List<CPlayerStatsDamage>)info.GetValue("DamageDealt", typeof(List<CPlayerStatsDamage>));
					break;
				case "DamageReceived":
					DamageReceived = (List<CPlayerStatsDamage>)info.GetValue("DamageReceived", typeof(List<CPlayerStatsDamage>));
					break;
				case "ActorsInitialized":
					ActorsInitialized = (List<CPlayerStatsAction>)info.GetValue("ActorsInitialized", typeof(List<CPlayerStatsAction>));
					break;
				case "DestroyedObstacles":
					DestroyedObstacles = (List<CPlayerStatsDestroyObstacle>)info.GetValue("DestroyedObstacles", typeof(List<CPlayerStatsDestroyObstacle>));
					break;
				case "PropsDestroyedByAbilities":
					PropsDestroyedByAbilities = (List<CPlayerStatsDestroyProp>)info.GetValue("PropsDestroyedByAbilities", typeof(List<CPlayerStatsDestroyProp>));
					break;
				case "PropsCreatedByAbilities":
					PropsCreatedByAbilities = (List<CPlayerStatsCreateProp>)info.GetValue("PropsCreatedByAbilities", typeof(List<CPlayerStatsCreateProp>));
					break;
				case "Infusions":
					Infusions = (List<CPlayerStatsElement>)info.GetValue("Infusions", typeof(List<CPlayerStatsElement>));
					break;
				case "Consumed":
					Consumed = (List<CPlayerStatsElement>)info.GetValue("Consumed", typeof(List<CPlayerStatsElement>));
					break;
				case "Abilities":
					Abilities = (List<CPlayerStatsAbilities>)info.GetValue("Abilities", typeof(List<CPlayerStatsAbilities>));
					break;
				case "ActionsEnded":
					ActionsEnded = (List<CPlayerStatsActionEnded>)info.GetValue("ActionsEnded", typeof(List<CPlayerStatsActionEnded>));
					break;
				case "Modifiers":
					Modifiers = (List<CPlayerStatsModifiers>)info.GetValue("Modifiers", typeof(List<CPlayerStatsModifiers>));
					break;
				case "Heals":
					Heals = (List<CPlayerStatsHeal>)info.GetValue("Heals", typeof(List<CPlayerStatsHeal>));
					break;
				case "Items":
					Items = (List<CPlayerStatsItem>)info.GetValue("Items", typeof(List<CPlayerStatsItem>));
					break;
				case "Loot":
					Loot = (List<CPlayerStatsLoot>)info.GetValue("Loot", typeof(List<CPlayerStatsLoot>));
					break;
				case "Monsters":
					Monsters = (List<CPlayerStatsMonsters>)info.GetValue("Monsters", typeof(List<CPlayerStatsMonsters>));
					break;
				case "Door":
					Door = (List<CPlayerStatsDoor>)info.GetValue("Door", typeof(List<CPlayerStatsDoor>));
					break;
				case "ActivatedProps":
					ActivatedProps = (List<CPlayerStatsPropActivated>)info.GetValue("ActivatedProps", typeof(List<CPlayerStatsPropActivated>));
					break;
				case "Trap":
					Trap = (List<CPlayerStatsTrap>)info.GetValue("Trap", typeof(List<CPlayerStatsTrap>));
					break;
				case "Hand":
					Hand = (List<CPlayerStatsHand>)info.GetValue("Hand", typeof(List<CPlayerStatsHand>));
					break;
				case "XP":
					XP = (List<CPlayerStatsXP>)info.GetValue("XP", typeof(List<CPlayerStatsXP>));
					break;
				case "BattleGoalPerks":
					BattleGoalPerks = (List<CPlayerStatsBattlePerks>)info.GetValue("BattleGoalPerks", typeof(List<CPlayerStatsBattlePerks>));
					break;
				case "Donations":
					Donations = (List<CPlayerStatsDonations>)info.GetValue("Donations", typeof(List<CPlayerStatsDonations>));
					break;
				case "PersonalQuests":
					PersonalQuests = (List<CPlayerStatsPersonalQuests>)info.GetValue("PersonalQuests", typeof(List<CPlayerStatsPersonalQuests>));
					break;
				case "Enhancements":
					Enhancements = (List<CPlayerStatsEnhancements>)info.GetValue("Enhancements", typeof(List<CPlayerStatsEnhancements>));
					break;
				case "LoseCards":
					LoseCards = (List<CPlayerStatsLoseCard>)info.GetValue("LoseCards", typeof(List<CPlayerStatsLoseCard>));
					break;
				case "RecoverCards":
					RecoverCards = (List<CPlayerStatsRecoverCard>)info.GetValue("RecoverCards", typeof(List<CPlayerStatsRecoverCard>));
					break;
				case "DiscardCard":
					DiscardCard = (List<CPlayerStatsDiscardCard>)info.GetValue("DiscardCard", typeof(List<CPlayerStatsDiscardCard>));
					break;
				case "EndTurn":
					EndTurn = (List<CPlayerStatsEndTurn>)info.GetValue("EndTurn", typeof(List<CPlayerStatsEndTurn>));
					break;
				case "EndRound":
					EndRound = (List<CPlayerStatsEndRound>)info.GetValue("EndRound", typeof(List<CPlayerStatsEndRound>));
					break;
				case "StartTurn":
					StartTurn = (List<CPlayerStatsStartTurn>)info.GetValue("StartTurn", typeof(List<CPlayerStatsStartTurn>));
					break;
				case "LostAdjacency":
					LostAdjacency = (List<CPlayerStatsLostAdjacency>)info.GetValue("LostAdjacency", typeof(List<CPlayerStatsLostAdjacency>));
					break;
				case "HeroSummons":
					HeroSummons = (List<CPlayerStatsHeroSummon>)info.GetValue("HeroSummons", typeof(List<CPlayerStatsHeroSummon>));
					break;
				case "Agility":
					Agility = (List<CPlayerStatsAgility>)info.GetValue("Agility", typeof(List<CPlayerStatsAgility>));
					break;
				case "AttackedByAbilities":
					AttackedByAbilities = (List<CPlayerStatsAbilities>)info.GetValue("AttackedByAbilities", typeof(List<CPlayerStatsAbilities>));
					break;
				case "Form":
					Form = (List<CPlayerStatsForm>)info.GetValue("Form", typeof(List<CPlayerStatsForm>));
					break;
				case "ActiveBonuses":
					ActiveBonuses = (List<CPlayerStatsActiveBonuses>)info.GetValue("ActiveBonuses", typeof(List<CPlayerStatsActiveBonuses>));
					break;
				case "ForcedActorsIntoTraps":
					ForcedActorsIntoTraps = (List<CPlayerStatsForcedActorsIntoTraps>)info.GetValue("ForcedActorsIntoTraps", typeof(List<CPlayerStatsForcedActorsIntoTraps>));
					break;
				case "Character":
					CharacterID = ((ECharacter)info.GetValue("Character", typeof(ECharacter))/*cast due to constrained. prefix*/).ToString() + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStats entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (Kills == null)
		{
			Kills = new List<CPlayerStatsKill>();
		}
		if (Deaths == null)
		{
			Deaths = new List<CPlayerStatsKill>();
		}
		if (DamageDealt == null)
		{
			DamageDealt = new List<CPlayerStatsDamage>();
		}
		if (DamageReceived == null)
		{
			DamageReceived = new List<CPlayerStatsDamage>();
		}
		if (ActorsInitialized == null)
		{
			ActorsInitialized = new List<CPlayerStatsAction>();
		}
		if (DestroyedObstacles == null)
		{
			DestroyedObstacles = new List<CPlayerStatsDestroyObstacle>();
		}
		if (PropsDestroyedByAbilities == null)
		{
			PropsDestroyedByAbilities = new List<CPlayerStatsDestroyProp>();
		}
		if (PropsCreatedByAbilities == null)
		{
			PropsCreatedByAbilities = new List<CPlayerStatsCreateProp>();
		}
		if (Infusions == null)
		{
			Infusions = new List<CPlayerStatsElement>();
		}
		if (Consumed == null)
		{
			Consumed = new List<CPlayerStatsElement>();
		}
		if (Abilities == null)
		{
			Abilities = new List<CPlayerStatsAbilities>();
		}
		if (ActionsEnded == null)
		{
			ActionsEnded = new List<CPlayerStatsActionEnded>();
		}
		if (Modifiers == null)
		{
			Modifiers = new List<CPlayerStatsModifiers>();
		}
		if (Heals == null)
		{
			Heals = new List<CPlayerStatsHeal>();
		}
		if (Items == null)
		{
			Items = new List<CPlayerStatsItem>();
		}
		if (Loot == null)
		{
			Loot = new List<CPlayerStatsLoot>();
		}
		if (Monsters == null)
		{
			Monsters = new List<CPlayerStatsMonsters>();
		}
		if (Door == null)
		{
			Door = new List<CPlayerStatsDoor>();
		}
		if (ActivatedProps == null)
		{
			ActivatedProps = new List<CPlayerStatsPropActivated>();
		}
		if (Trap == null)
		{
			Trap = new List<CPlayerStatsTrap>();
		}
		if (Hand == null)
		{
			Hand = new List<CPlayerStatsHand>();
		}
		if (XP == null)
		{
			XP = new List<CPlayerStatsXP>();
		}
		if (BattleGoalPerks == null)
		{
			BattleGoalPerks = new List<CPlayerStatsBattlePerks>();
		}
		if (Donations == null)
		{
			Donations = new List<CPlayerStatsDonations>();
		}
		if (PersonalQuests == null)
		{
			PersonalQuests = new List<CPlayerStatsPersonalQuests>();
		}
		if (Enhancements == null)
		{
			Enhancements = new List<CPlayerStatsEnhancements>();
		}
		if (LoseCards == null)
		{
			LoseCards = new List<CPlayerStatsLoseCard>();
		}
		if (RecoverCards == null)
		{
			RecoverCards = new List<CPlayerStatsRecoverCard>();
		}
		if (DiscardCard == null)
		{
			DiscardCard = new List<CPlayerStatsDiscardCard>();
		}
		if (EndTurn == null)
		{
			EndTurn = new List<CPlayerStatsEndTurn>();
		}
		if (EndRound == null)
		{
			EndRound = new List<CPlayerStatsEndRound>();
		}
		if (StartTurn == null)
		{
			StartTurn = new List<CPlayerStatsStartTurn>();
		}
		if (LostAdjacency == null)
		{
			LostAdjacency = new List<CPlayerStatsLostAdjacency>();
		}
		if (HeroSummons == null)
		{
			HeroSummons = new List<CPlayerStatsHeroSummon>();
		}
		if (Agility == null)
		{
			Agility = new List<CPlayerStatsAgility>();
		}
		if (AttackedByAbilities == null)
		{
			AttackedByAbilities = new List<CPlayerStatsAbilities>();
		}
		if (Form == null)
		{
			Form = new List<CPlayerStatsForm>();
		}
		if (ActiveBonuses == null)
		{
			ActiveBonuses = new List<CPlayerStatsActiveBonuses>();
		}
		if (ForcedActorsIntoTraps == null)
		{
			ForcedActorsIntoTraps = new List<CPlayerStatsForcedActorsIntoTraps>();
		}
		if (ArrayIndexChanged == null)
		{
			ArrayIndexChanged = new List<CPlayerStatsArrayIndexChanged>();
		}
		if (Healed == null)
		{
			Healed = new List<CPlayerStatsActorHealthChanged>();
		}
		if (RemovedCondition == null)
		{
			RemovedCondition = new List<CPlayerStatsRemovedCondition>();
		}
		if (ActorActiveBonuses == null)
		{
			ActorActiveBonuses = new List<CPlayerStatsActorActiveBonuses>();
		}
	}

	public CPlayerStats(CPlayerStats original)
	{
		CharacterID = original.CharacterID;
		Kills = original.Kills.ToList();
		Deaths = original.Deaths.ToList();
		DamageDealt = original.DamageDealt.ToList();
		DamageReceived = original.DamageReceived.ToList();
		ActorsInitialized = original.ActorsInitialized.ToList();
		DestroyedObstacles = original.DestroyedObstacles.ToList();
		PropsDestroyedByAbilities = original.PropsDestroyedByAbilities.ToList();
		PropsCreatedByAbilities = original.PropsCreatedByAbilities.ToList();
		Infusions = original.Infusions.ToList();
		Consumed = original.Consumed.ToList();
		Abilities = original.Abilities.ToList();
		ActionsEnded = original.ActionsEnded.ToList();
		Modifiers = original.Modifiers.ToList();
		Heals = original.Heals.ToList();
		Items = original.Items.ToList();
		Loot = original.Loot.ToList();
		Monsters = original.Monsters.ToList();
		Door = original.Door.ToList();
		ActivatedProps = original.ActivatedProps.ToList();
		Trap = original.Trap.ToList();
		Hand = original.Hand.ToList();
		XP = original.XP.ToList();
		BattleGoalPerks = original.BattleGoalPerks.ToList();
		Donations = original.Donations.ToList();
		PersonalQuests = original.PersonalQuests.ToList();
		Enhancements = original.Enhancements.ToList();
		LoseCards = original.LoseCards.ToList();
		RecoverCards = original.RecoverCards.ToList();
		DiscardCard = original.DiscardCard.ToList();
		EndTurn = original.EndTurn.ToList();
		EndRound = original.EndRound.ToList();
		StartTurn = original.StartTurn.ToList();
		LostAdjacency = original.LostAdjacency.ToList();
		HeroSummons = original.HeroSummons.ToList();
		Agility = original.Agility.ToList();
		AttackedByAbilities = original.AttackedByAbilities.ToList();
		Healed = original.Healed.ToList();
		Form = original.Form.ToList();
		ActiveBonuses = original.ActiveBonuses.ToList();
		ForcedActorsIntoTraps = original.ForcedActorsIntoTraps.ToList();
		ArrayIndexChanged = original.ArrayIndexChanged.ToList();
		RemovedCondition = original.RemovedCondition.ToList();
		ActorActiveBonuses = original.ActorActiveBonuses.ToList();
	}

	public CPlayerStats(string characterID)
	{
		CharacterID = characterID;
		Kills = new List<CPlayerStatsKill>();
		Deaths = new List<CPlayerStatsKill>();
		DamageDealt = new List<CPlayerStatsDamage>();
		DamageReceived = new List<CPlayerStatsDamage>();
		ActorsInitialized = new List<CPlayerStatsAction>();
		DestroyedObstacles = new List<CPlayerStatsDestroyObstacle>();
		PropsDestroyedByAbilities = new List<CPlayerStatsDestroyProp>();
		PropsCreatedByAbilities = new List<CPlayerStatsCreateProp>();
		Infusions = new List<CPlayerStatsElement>();
		Consumed = new List<CPlayerStatsElement>();
		Abilities = new List<CPlayerStatsAbilities>();
		ActionsEnded = new List<CPlayerStatsActionEnded>();
		Modifiers = new List<CPlayerStatsModifiers>();
		Heals = new List<CPlayerStatsHeal>();
		Items = new List<CPlayerStatsItem>();
		Loot = new List<CPlayerStatsLoot>();
		Monsters = new List<CPlayerStatsMonsters>();
		Door = new List<CPlayerStatsDoor>();
		ActivatedProps = new List<CPlayerStatsPropActivated>();
		Trap = new List<CPlayerStatsTrap>();
		Hand = new List<CPlayerStatsHand>();
		XP = new List<CPlayerStatsXP>();
		BattleGoalPerks = new List<CPlayerStatsBattlePerks>();
		Donations = new List<CPlayerStatsDonations>();
		PersonalQuests = new List<CPlayerStatsPersonalQuests>();
		Enhancements = new List<CPlayerStatsEnhancements>();
		LoseCards = new List<CPlayerStatsLoseCard>();
		RecoverCards = new List<CPlayerStatsRecoverCard>();
		DiscardCard = new List<CPlayerStatsDiscardCard>();
		EndTurn = new List<CPlayerStatsEndTurn>();
		EndRound = new List<CPlayerStatsEndRound>();
		StartTurn = new List<CPlayerStatsStartTurn>();
		LostAdjacency = new List<CPlayerStatsLostAdjacency>();
		HeroSummons = new List<CPlayerStatsHeroSummon>();
		Agility = new List<CPlayerStatsAgility>();
		AttackedByAbilities = new List<CPlayerStatsAbilities>();
		Healed = new List<CPlayerStatsActorHealthChanged>();
		Form = new List<CPlayerStatsForm>();
		ActiveBonuses = new List<CPlayerStatsActiveBonuses>();
		ForcedActorsIntoTraps = new List<CPlayerStatsForcedActorsIntoTraps>();
		ArrayIndexChanged = new List<CPlayerStatsArrayIndexChanged>();
		RemovedCondition = new List<CPlayerStatsRemovedCondition>();
		ActorActiveBonuses = new List<CPlayerStatsActorActiveBonuses>();
	}

	public void UpdatePlayerStats(CPlayerStats scenarioStats)
	{
		Kills.AddRange(scenarioStats.Kills);
		Deaths.AddRange(scenarioStats.Deaths);
		DamageDealt.AddRange(scenarioStats.DamageDealt);
		DamageReceived.AddRange(scenarioStats.DamageReceived);
		ActorsInitialized.AddRange(scenarioStats.ActorsInitialized);
		DestroyedObstacles.AddRange(scenarioStats.DestroyedObstacles);
		PropsDestroyedByAbilities.AddRange(scenarioStats.PropsDestroyedByAbilities);
		PropsCreatedByAbilities.AddRange(scenarioStats.PropsCreatedByAbilities);
		Infusions.AddRange(scenarioStats.Infusions);
		Consumed.AddRange(scenarioStats.Consumed);
		Abilities.AddRange(scenarioStats.Abilities);
		ActionsEnded.AddRange(scenarioStats.ActionsEnded);
		Modifiers.AddRange(scenarioStats.Modifiers);
		Heals.AddRange(scenarioStats.Heals);
		Items.AddRange(scenarioStats.Items);
		Loot.AddRange(scenarioStats.Loot);
		Monsters.AddRange(scenarioStats.Monsters);
		Door.AddRange(scenarioStats.Door);
		ActivatedProps.AddRange(scenarioStats.ActivatedProps);
		Trap.AddRange(scenarioStats.Trap);
		Hand.AddRange(scenarioStats.Hand);
		XP.AddRange(scenarioStats.XP);
		BattleGoalPerks.AddRange(scenarioStats.BattleGoalPerks);
		Donations.AddRange(scenarioStats.Donations);
		PersonalQuests.AddRange(scenarioStats.PersonalQuests);
		Enhancements.AddRange(scenarioStats.Enhancements);
		LoseCards.AddRange(scenarioStats.LoseCards);
		RecoverCards.AddRange(scenarioStats.RecoverCards);
		DiscardCard.AddRange(scenarioStats.DiscardCard);
		EndTurn.AddRange(scenarioStats.EndTurn);
		EndRound.AddRange(scenarioStats.EndRound);
		StartTurn.AddRange(scenarioStats.StartTurn);
		LostAdjacency.AddRange(scenarioStats.LostAdjacency);
		HeroSummons.AddRange(scenarioStats.HeroSummons);
		Agility.AddRange(scenarioStats.Agility);
		AttackedByAbilities.AddRange(scenarioStats.AttackedByAbilities);
		Form.AddRange(scenarioStats.Form);
		ActiveBonuses.AddRange(scenarioStats.ActiveBonuses);
		ForcedActorsIntoTraps.AddRange(scenarioStats.ForcedActorsIntoTraps);
		ArrayIndexChanged.AddRange(scenarioStats.ArrayIndexChanged);
		RemovedCondition.AddRange(scenarioStats.RemovedCondition);
		Healed.AddRange(scenarioStats.Healed);
		ActorActiveBonuses.AddRange(scenarioStats.ActorActiveBonuses);
	}

	public void Clear()
	{
		Kills.Clear();
		Deaths.Clear();
		DamageDealt.Clear();
		DamageReceived.Clear();
		ActorsInitialized.Clear();
		DestroyedObstacles.Clear();
		PropsDestroyedByAbilities.Clear();
		PropsCreatedByAbilities.Clear();
		Infusions.Clear();
		Consumed.Clear();
		Abilities.Clear();
		ActionsEnded.Clear();
		Modifiers.Clear();
		Heals.Clear();
		Items.Clear();
		Loot.Clear();
		Monsters.Clear();
		Door.Clear();
		ActivatedProps.Clear();
		Trap.Clear();
		Hand.Clear();
		XP.Clear();
		BattleGoalPerks.Clear();
		Donations.Clear();
		PersonalQuests.Clear();
		Enhancements.Clear();
		LoseCards.Clear();
		RecoverCards.Clear();
		DiscardCard.Clear();
		EndTurn.Clear();
		EndRound.Clear();
		StartTurn.Clear();
		LostAdjacency.Clear();
		HeroSummons.Clear();
		Agility.Clear();
		AttackedByAbilities.Clear();
		Form.Clear();
		ActiveBonuses.Clear();
		ForcedActorsIntoTraps.Clear();
		ArrayIndexChanged.Clear();
		RemovedCondition.Clear();
		Healed.Clear();
		ActorActiveBonuses.Clear();
	}

	public bool IsClear()
	{
		return Kills.Count() + Deaths.Count() + DamageDealt.Count() + DamageReceived.Count() + ActorsInitialized.Count() + DestroyedObstacles.Count() + PropsDestroyedByAbilities.Count() + PropsCreatedByAbilities.Count() + Infusions.Count() + Consumed.Count() + Abilities.Count() + ActionsEnded.Count() + Modifiers.Count() + Heals.Count() + Items.Count() + Loot.Count() + Monsters.Count() + Door.Count() + ActivatedProps.Count() + Trap.Count() + Hand.Count() + XP.Count() + BattleGoalPerks.Count() + Donations.Count() + PersonalQuests.Count() + Enhancements.Count() + LoseCards.Count() + RecoverCards.Count() + DiscardCard.Count() + EndTurn.Count() + StartTurn.Count() + LostAdjacency.Count() + HeroSummons.Count() + Agility.Count() + AttackedByAbilities.Count() + Form.Count() + ActiveBonuses.Count() + ForcedActorsIntoTraps.Count() + ArrayIndexChanged.Count() + RemovedCondition.Count() + Healed.Count() + ActorActiveBonuses.Count() == 0;
	}
}
