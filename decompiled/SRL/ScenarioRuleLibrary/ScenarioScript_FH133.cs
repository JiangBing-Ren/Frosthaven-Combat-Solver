using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH133 : ScenarioScript
{
	private List<CScenarioModifierTriggerAbility> CatIdolModifiers = new List<CScenarioModifierTriggerAbility>();

	private const string CatIdolResource = "CatIdol";

	protected override bool Initialize()
	{
		CatIdolModifiers.Add(GetScenarioModifier("CatIdolMovementBuff") as CScenarioModifierTriggerAbility);
		CatIdolModifiers.Add(GetScenarioModifier("CatIdolAdvantageBuff") as CScenarioModifierTriggerAbility);
		CatIdolModifiers.Add(GetScenarioModifier("GiveCatIdolDuringTurn") as CScenarioModifierTriggerAbility);
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
		{
			CPlayerActor actor = ScenarioManager.Scenario.AllPlayers[0];
			List<CTile> validTiles = ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor x) => x.Tile).ToList();
			ScenarioScript.PickHexes("$FH/FH_Scenariro_Campaign_133_ChooseCatIdolHolder$", actor, validTiles, (CAbilityNullHex ability, List<CTile> selectedTiles) =>
			{
				GiveCatIdol(null, selectedTiles[0].LivingActor);
			}, 1, animateActor: true, null, canSkip: false, canUndo: false);
			break;
		}
		case EScenarioModifierTriggerPhase.EndTurn:
			TryEscapeMap();
			break;
		}
	}

	public static void GiveCatIdol(CActor from, CActor to)
	{
		ScenarioScript_FH133 scenarioScript_FH = ScenarioManager.CurrentScenarioState.ScenarioScripts.SingleOrDefault((ScenarioScript x) => x is ScenarioScript_FH133) as ScenarioScript_FH133;
		if (from != null)
		{
			foreach (CActiveBonus item in from x in @from.FindApplicableActiveBonuses()
				where x.Ability.ActiveBonusData.GetMiscVar("CatIdolTransferable").GetBool()
				select x)
			{
				item.Finish();
			}
			from.RemoveCharacterResource("CatIdol", 1, updateWorldSpaceUI: true);
		}
		if (to == null)
		{
			return;
		}
		_ = from x in to.FindApplicableActiveBonuses()
			where x.Ability.Name == "AReduceSpeedCatIdol"
			select x;
		foreach (CScenarioModifierTriggerAbility catIdolModifier in scenarioScript_FH.CatIdolModifiers)
		{
			catIdolModifier.PerformScenarioModifier(ScenarioScript.RoundNumber, to, ScenarioScript.PartySize, forceActivate: true);
		}
		to.AddCharacterResource("CatIdol", 1);
	}

	protected override void OnStartRevealingRoom(bool wasRevealed)
	{
		if (wasRevealed && _tickRoomRevealed.RoomName == "Room_19")
		{
			if ((PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability is CAbilityMove { RemainingMoves: >0 } cAbilityMove)
			{
				cAbilityMove.ForceInterrupt();
			}
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = ScenarioManager.Tiles[10, 11].m_ArrayIndex,
				m_Zoom = 1f,
				m_delay = 0f
			};
			ScenarioRuleClient.MessageHandler(message);
			GameState.TriggerCampaignSection("FS_133_Bolt_Room_Special");
		}
		base.OnStartRevealingRoom(wasRevealed);
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("plate_count", ScenarioScript.PartySize.ToString());
	}
}
