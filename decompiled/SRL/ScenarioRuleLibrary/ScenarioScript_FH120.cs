using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH120 : ScenarioScript
{
	private CMap room3;

	private CMap room5;

	private CObjective_Scripted DestroyGlowingOrb;

	private CObjective_Scripted DestroyDoor;

	private CObjective_Scripted InvestigateStructure;

	private CScenarioModifier Draw4Taken;

	private CScenarioModifier ExhaustIfNoTaken;

	private CScenarioModifier Draw3Taken;

	protected override bool Initialize()
	{
		room3 = ScenarioScript.FindRoom("Room_3");
		room5 = ScenarioScript.FindRoom("Room_5");
		DestroyGlowingOrb = GetObjective("DestroyGlowingOrb") as CObjective_Scripted;
		DestroyGlowingOrb.Total = 1;
		DestroyDoor = GetObjective("DestroyDoor") as CObjective_Scripted;
		DestroyDoor.Total = 1;
		DestroyDoor.HideOnComplete = true;
		InvestigateStructure = GetObjective("InvestigateStructure") as CObjective_Scripted;
		InvestigateStructure.Total = 1;
		InvestigateStructure.HideOnComplete = true;
		Draw4Taken = GetScenarioModifier("Draw4Taken");
		ExhaustIfNoTaken = GetScenarioModifier("ExhaustIfNoTaken");
		Draw3Taken = GetScenarioModifier("Draw3Taken");
		return base.Initialize();
	}

	public override CBaseCard.ECardPile OverrideRecoverCardPile(CActor actor)
	{
		if (!room3.Revealed)
		{
			return CBaseCard.ECardPile.None;
		}
		return CBaseCard.ECardPile.Taken;
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.SpecificPropType == ESpecificPropType.MysteriousMachine)
		{
			DestroyGlowingOrb.Current = 1;
			DestroyGlowingOrb.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.AfterShortRest:
		{
			if (!(_tickActor is CPlayerActor actor2))
			{
				break;
			}
			int num = 0;
			if (room3.Revealed)
			{
				num = 4;
				if (room5.Revealed)
				{
					num = 3;
				}
			}
			if (num > 0)
			{
				DrawCardsFromTaken(actor2, num);
			}
			break;
		}
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			if (_tickActor is CPlayerActor actor && room3.Revealed)
			{
				MoveHandCardsToTaken(actor);
			}
			break;
		}
	}

	private void MoveHandCardsToTaken(CActor actor)
	{
		List<CAbilityCard> cardPileList = actor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Hand);
		List<CAbilityCard> cardPileList2 = actor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Taken);
		foreach (CAbilityCard item in cardPileList.ToList())
		{
			actor.CharacterClass.MoveAbilityCard(item, cardPileList, cardPileList2, "HandAbilityCards", "TakenAbilityCards");
		}
	}

	private void DrawCardsFromTaken(CPlayerActor actor, int count)
	{
		MoveHandCardsToTaken(actor);
		List<CAbilityCard> cardPileList = actor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Hand);
		List<CAbilityCard> cardPileList2 = actor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Taken);
		cardPileList2.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
		if (actor.Class.ID == "GeminateID")
		{
			IEnumerable<CAbilityCard> source = cardPileList2.Where((CAbilityCard x) => x.GetAbilityCardYML.RequiredForm == CPlayerActor.EForm.Range);
			IEnumerable<CAbilityCard> source2 = cardPileList2.Where((CAbilityCard x) => x.GetAbilityCardYML.RequiredForm == CPlayerActor.EForm.Melee);
			if (actor.Form == CPlayerActor.EForm.Range || actor.Form == CPlayerActor.EForm.Choose)
			{
				foreach (CAbilityCard item in source.Take(count).ToList())
				{
					actor.CharacterClass.MoveAbilityCard(item, cardPileList2, cardPileList, "TakenAbilityCards", "HandAbilityCards");
				}
			}
			if (actor.Form != CPlayerActor.EForm.Melee && actor.Form != CPlayerActor.EForm.Choose)
			{
				return;
			}
			{
				foreach (CAbilityCard item2 in source2.Take(count).ToList())
				{
					actor.CharacterClass.MoveAbilityCard(item2, cardPileList2, cardPileList, "TakenAbilityCards", "HandAbilityCards");
				}
				return;
			}
		}
		foreach (CAbilityCard item3 in cardPileList2.Take(count).ToList())
		{
			actor.CharacterClass.MoveAbilityCard(item3, cardPileList2, cardPileList, "TakenAbilityCards", "HandAbilityCards");
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == room3)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				MoveHandCardsToTaken(playerActor);
			}
			DestroyDoor.Current = 0;
			DestroyDoor.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			Draw4Taken.SetDeactivated(deactivate: false);
			ExhaustIfNoTaken.SetDeactivated(deactivate: false);
		}
		else if (_tickRoomRevealed == room5)
		{
			InvestigateStructure.Current = 1;
			InvestigateStructure.ScriptedIsComplete = true;
			DestroyDoor.Current = 1;
			DestroyDoor.ScriptedIsComplete = true;
			DestroyGlowingOrb.Current = 0;
			DestroyGlowingOrb.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			Draw4Taken.SetDeactivated(deactivate: true);
			Draw3Taken.SetDeactivated(deactivate: false);
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = new Point(19, 6),
				m_Zoom = 0.5f
			};
			ScenarioRuleClient.MessageHandler(message);
			GameState.TriggerCampaignSection("FS_120_Under_the_Influence_Special_5");
		}
	}

	public override string TakenPileTooltipKey()
	{
		if (!Draw3Taken.Deactivated && !Draw3Taken.IsHidden)
		{
			return "FH_Scenario_Campaign_120_SCENARIO_MODIFIER_3";
		}
		if (!Draw4Taken.Deactivated && !Draw4Taken.IsHidden)
		{
			return "FH_Scenario_Campaign_120_SCENARIO_MODIFIER_1";
		}
		return string.Empty;
	}

	public override string TakenPileNameKey()
	{
		return "GUI_CARD_SECTION_TAKEN_120";
	}
}
