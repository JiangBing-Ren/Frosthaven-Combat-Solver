using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH089 : ScenarioScript
{
	private static List<Point> room01a = new List<Point>
	{
		new Point(4, 12),
		new Point(3, 13),
		new Point(5, 11),
		new Point(5, 12),
		new Point(4, 13),
		new Point(4, 14),
		new Point(3, 15),
		new Point(6, 12),
		new Point(5, 13),
		new Point(5, 14),
		new Point(4, 15)
	};

	private static List<Point> RightTiles = new List<Point>
	{
		new Point(4, 12),
		new Point(3, 13),
		new Point(5, 11),
		new Point(5, 12),
		new Point(4, 13),
		new Point(4, 14),
		new Point(3, 15),
		new Point(6, 12),
		new Point(5, 13),
		new Point(5, 14),
		new Point(4, 15),
		new Point(5, 15),
		new Point(6, 14),
		new Point(6, 13),
		new Point(7, 13),
		new Point(7, 14),
		new Point(6, 15),
		new Point(6, 16),
		new Point(5, 17),
		new Point(5, 18)
	};

	private static List<Point> room01g = new List<Point>
	{
		new Point(12, 16),
		new Point(11, 17),
		new Point(11, 18),
		new Point(10, 19),
		new Point(13, 16),
		new Point(12, 17),
		new Point(12, 18),
		new Point(11, 19),
		new Point(11, 20),
		new Point(12, 19),
		new Point(13, 18)
	};

	private static List<Point> LeftTiles = new List<Point>
	{
		new Point(12, 16),
		new Point(11, 17),
		new Point(11, 18),
		new Point(10, 19),
		new Point(13, 16),
		new Point(12, 17),
		new Point(12, 18),
		new Point(11, 19),
		new Point(11, 20),
		new Point(12, 19),
		new Point(13, 18),
		new Point(10, 18),
		new Point(10, 17),
		new Point(11, 16),
		new Point(10, 15),
		new Point(10, 16),
		new Point(9, 17),
		new Point(9, 18),
		new Point(8, 19),
		new Point(8, 20)
	};

	private static List<Point> roomStairs = new List<Point>
	{
		new Point(5, 15),
		new Point(10, 18)
	};

	private static List<Point> statueStepTiles = new List<Point>
	{
		new Point(9, 16),
		new Point(9, 17),
		new Point(9, 18),
		new Point(8, 18)
	};

	private static Point statuePosition = new Point(7, 17);

	private static string room10d_01a_01g = "Room_2";

	private static string fayeID = "Faye089ID";

	private CObjectActor statue;

	private CObjective_Scripted investigateStatueObjective;

	private CObjective_Scripted destroyStatueObjective;

	private CScenarioModifier advantageHolder;

	private CScenarioModifier disadvantageHolder;

	private CObjectDoor door2;

	private CObjectDoor door3;

	private CEnemyActor faye;

	public CMap room2;

	private bool StatueImmune;

	private bool fayeUsedSpecialAttack;

	[StateDump(Skip = true)]
	private bool showNarrativeEvent;

	private int InitialFayeHp => 3 + ScenarioScript._css.Level;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(StatueImmune);
		string text2 = JsonConvert.SerializeObject(fayeUsedSpecialAttack);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[FH089]Serialize: " + text3 + "[-]", null, 78);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[FH089]Deserialize " + s + "[-]", null, 84);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			StatueImmune = JsonConvert.DeserializeObject<bool>(array[0]);
			fayeUsedSpecialAttack = JsonConvert.DeserializeObject<bool>(array[1]);
			CObjectActor cObjectActor = ScenarioManager.Scenario.Objects.Find((CObjectActor x) => x.ArrayIndex == statuePosition);
			if (cObjectActor != null && !cObjectActor.IsDead && StatueImmune)
			{
				cObjectActor.IsImmuneToDamage = true;
				ScenarioManager.ToggleTileHighlights(statueStepTiles, enabled: true);
			}
		}
	}

	protected override bool Initialize()
	{
		room2 = ScenarioScript.FindRoom(room10d_01a_01g);
		advantageHolder = GetScenarioModifier("advantageHolder");
		disadvantageHolder = GetScenarioModifier("disadvantageHolder");
		door2 = GetProp(10, 14, "ThinDoor") as CObjectDoor;
		door3 = GetProp(14, 6, "ThinDoor") as CObjectDoor;
		investigateStatueObjective = (CObjective_Scripted)GetObjective("investigateStatue");
		destroyStatueObjective = (CObjective_Scripted)GetObjective("destroyStatue");
		faye = ScenarioManager.Scenario.AllActors.Find((CActor a) => a?.MonsterClass?.ID == fayeID) as CEnemyActor;
		statue = ScenarioManager.Scenario.Objects.Find((CObjectActor x) => x.ArrayIndex == statuePosition);
		if (statue != null && !statue.IsDead && StatueImmune)
		{
			statue.IsImmuneToDamage = true;
			ScenarioManager.ToggleTileHighlights(statueStepTiles, enabled: true);
		}
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			CheckStatueAdjacency();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			OnStatueRevealed();
			GiveAdvantageBuffs(_tickActor);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			OverrideFayeRoundCard();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			OverrideFayeFocus();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			MarkFayeSpecialAttack();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 144);
			break;
		}
	}

	private void OnStatueRevealed()
	{
		if (_tickActor is CObjectActor cObjectActor && cObjectActor.ArrayIndex == statuePosition)
		{
			statue = cObjectActor;
			statue.IsImmuneToDamage = true;
			StatueImmune = true;
			ScenarioManager.ToggleTileHighlights(statueStepTiles, enabled: true);
			advantageHolder.SetHiddenState(shouldSetHidden: false);
			disadvantageHolder.SetHiddenState(shouldSetHidden: false);
			GiveAdvantageBuffsToAllActors();
			investigateStatueObjective.SetActivation(active: true);
		}
	}

	private void GiveAdvantageBuffsToAllActors()
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			GiveAdvantageBuffs(allAliveActor);
		}
	}

	private void RemoveAdvantageBuffsFromAllActors()
	{
		for (int num = advantageHolder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			advantageHolder.ActiveBonuses[num].Finish();
		}
		for (int num2 = disadvantageHolder.ActiveBonuses.Count - 1; num2 >= 0; num2--)
		{
			disadvantageHolder.ActiveBonuses[num2].Finish();
		}
	}

	private void GiveAdvantageBuffs(CActor actor)
	{
		ApplyScenarioActiveBonusToActor(actor, advantageHolder);
		ApplyScenarioActiveBonusToActor(actor, disadvantageHolder);
	}

	private bool StatueRoomAllEnemiesDead(CActor deadActor = null)
	{
		bool result = true;
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (allAliveActor.Type == CActor.EType.Enemy && deadActor != allAliveActor && allAliveActor.Tile.m_HexMap.RoomName == room10d_01a_01g)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void ApplyScenarioActiveBonusToActor(CActor actor, CScenarioModifier holder)
	{
		if (!holder.ActiveBonuses.Any((CActiveBonus b) => b.Actor == actor))
		{
			CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void OverrideFayeRoundCard()
	{
		if (_tickActor != faye || faye.IsDead)
		{
			return;
		}
		List<CMonsterAbilityCard> abilityCardsPool = faye.MonsterClass.CardsPoolClass.AbilityCardsPool;
		if (!door3.DoorIsOpen)
		{
			faye.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 99992);
		}
		else if (fayeUsedSpecialAttack)
		{
			faye.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 99994);
			fayeUsedSpecialAttack = false;
		}
		else
		{
			faye.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 99993);
		}
	}

	private void OverrideFayeFocus()
	{
		if (_tickActor != faye || faye.IsDead)
		{
			return;
		}
		EnemyState enemyState = ScenarioScript._css.Monsters.Find((EnemyState actor) => actor.IsBoss);
		if (enemyState != null)
		{
			CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.TargetGUID = enemyState.ActorGuid;
			ScenarioScript._css.AllyMonsters.Find((EnemyState actor) => actor.ActorGuid == faye.ActorGuid).SetAIFocusOverride(cAIFocusOverrideDetails);
		}
	}

	private void MarkFayeSpecialAttack()
	{
		if (faye != null && _tickActor == faye)
		{
			CAbility lastAbilityPerformed = faye.LastAbilityPerformed;
			if (lastAbilityPerformed != null && (lastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special1) != CAbility.EAbilityFlag.None && lastAbilityPerformed.AbilityType == CAbility.EAbilityType.Attack)
			{
				fayeUsedSpecialAttack = true;
			}
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor == statue)
		{
			CTile cTile = ScenarioManager.Tiles[8, 17];
			cTile.m_Props.FindAll((CObjectProp p) => p.PropType == EPropType.OneHexObstacle).ForEach((CObjectProp p) =>
			{
				p.DestroyProp();
			});
			TrySpawnActorAtNearestTile(cTile, fayeID, CActor.EType.Ally, out var actor, addIntoInitiativeOrder: true);
			faye = actor as CEnemyActor;
			ScenarioScript.SetHealth(faye, InitialFayeHp);
			SpawnVFXProp(cTile, "IceDestructionGeneric");
			door2.SetHasExtraLock(hasExtraLock: false);
			destroyStatueObjective.SetActivation(active: false);
			showNarrativeEvent = true;
			OverrideFayeRoundCard();
			statue = null;
		}
	}

	public override bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		if (faye == null || faye.IsDead)
		{
			return false;
		}
		if (ability.TargetingActor != faye)
		{
			return false;
		}
		if (!(actor is CEnemyActor cEnemyActor) || cEnemyActor.MonsterClass.MonsterType != EMonsterType.Boss)
		{
			return false;
		}
		return true;
	}

	protected void CheckStatueAdjacency()
	{
		if (statue != null && !statue.IsDead && statue.IsImmuneToDamage && _tickActor.Type == CActor.EType.Player && statueStepTiles.Contains(_tickActor.ArrayIndex))
		{
			StatueImmune = false;
			statue.IsImmuneToDamage = false;
			ScenarioManager.ToggleTileHighlights(statueStepTiles, enabled: false);
			investigateStatueObjective.SetActivation(active: false);
			destroyStatueObjective.SetActivation(active: true);
			showNarrativeEvent = true;
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (showNarrativeEvent)
		{
			showNarrativeEvent = false;
			if (statue != null && message.MessageName == "FS_089_A_Contained_Fire_Special_1_1")
			{
				return true;
			}
			if (statue == null && message.MessageName == "FS_089_A_Contained_Fire_Special_2_1")
			{
				return true;
			}
		}
		return base.AllowTriggerMessageInternal(ev, message);
	}

	public static int CalcTileHeight(CTile tile)
	{
		if (room01a.Contains(tile.m_ArrayIndex))
		{
			return 1;
		}
		if (room01g.Contains(tile.m_ArrayIndex))
		{
			return 1;
		}
		return 0;
	}

	public static bool CheckHeightLeft(Point pos)
	{
		return LeftTiles.Contains(pos);
	}

	public static bool CheckHeightRight(Point pos)
	{
		return RightTiles.Contains(pos);
	}

	public static bool ValidateAdvantage(CActor targetingActor, CActor targetActor)
	{
		int num = CalcTileHeight(targetingActor.Tile);
		int num2 = CalcTileHeight(targetActor.Tile);
		return num > num2;
	}

	public static bool ValidateDisadvantage(CActor targetingActor, CActor targetActor)
	{
		int num = CalcTileHeight(targetingActor.Tile);
		int num2 = CalcTileHeight(targetActor.Tile);
		return num < num2;
	}
}
