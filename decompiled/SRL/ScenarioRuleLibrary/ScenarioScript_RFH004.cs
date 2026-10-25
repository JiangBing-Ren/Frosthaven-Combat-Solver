using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH004 : ScenarioScript
{
	[Serializable]
	public class StorageRFH004
	{
		public bool ReachedMarko;

		public int DoorOpenRound;
	}

	private CObjective_Scripted killRoom1Enemies;

	private CObjective_Scripted playersAlliesDie;

	private CObjective_ActorsEscaped escapeAllies;

	private const string markoID = "MarkoID";

	private const string valrathMerchantID = "ValrathMerchantID";

	public CObjectDoor door;

	private StorageRFH004 serializedProperties;

	private CEnemyActor Marko => ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.Class.ID == "MarkoID") as CEnemyActor;

	private CEnemyActor valrathMerchant => ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.Class.ID == "ValrathMerchantID") as CEnemyActor;

	public bool ReachedMarko
	{
		get
		{
			return serializedProperties.ReachedMarko;
		}
		set
		{
			serializedProperties.ReachedMarko = value;
		}
	}

	private int DoorOpenRound
	{
		get
		{
			return serializedProperties.DoorOpenRound;
		}
		set
		{
			serializedProperties.DoorOpenRound = value;
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new StorageRFH004();
		escapeAllies = GetObjective<CObjective_ActorsEscaped>("AllyEscape");
		killRoom1Enemies = GetObjective<CObjective_Scripted>("KillRoom1Enemies");
		killRoom1Enemies.HideOnComplete = true;
		playersAlliesDie = GetObjective<CObjective_Scripted>("PlayersAlliesDie");
		playersAlliesDie.Total = 1;
		playersAlliesDie.ValidActors = () => ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => !(x is CObjectActor) && x.OriginalType == CActor.EType.Ally).ToList();
		door = ScenarioScript.FindDoor(9, 10);
		UpdateKillRoom1Objective();
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (StorageRFH004)JsonConvert.DeserializeObject(storage, typeof(StorageRFH004));
		}
		InitMarko();
		InitValrathMerchant();
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		UpdateKillRoom1Objective();
		UpdateSurviveObjective();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void UpdateSurviveObjective()
	{
		if (!playersAlliesDie.ScriptedIsComplete && playersAlliesDie.ValidActors().Contains(_tickActor))
		{
			playersAlliesDie.Current++;
			playersAlliesDie.ScriptedIsComplete = playersAlliesDie.Current >= playersAlliesDie.Total;
		}
	}

	private void UpdateKillRoom1Objective()
	{
		if (!killRoom1Enemies.ScriptedIsComplete)
		{
			IEnumerable<CEnemyActor> source = ScenarioManager.Scenario.AllEnemies.Where((CEnemyActor x) => x.Tile.m_HexMap.RoomName == "Room_1");
			killRoom1Enemies.Total = source.Count();
			killRoom1Enemies.Current = source.Count((CEnemyActor x) => x.IsDead);
			killRoom1Enemies.ScriptedIsComplete = killRoom1Enemies.Current >= killRoom1Enemies.Total;
			if (killRoom1Enemies.ScriptedIsComplete)
			{
				(ScenarioManager.GetTile(9, 10)?.FindDoor()).UnlockLockedDoorWithoutOpening();
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		if (_tickActor == Marko || _tickActor == valrathMerchant)
		{
			if (Marko != null && valrathMerchant != null && valrathMerchant.GetDistanceTo(Marko) <= 1 && !ReachedMarko)
			{
				ReachedMarko = true;
				SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "ReachedMarko"));
			}
			if (ScenarioManager.CurrentScenarioState.IsOnEscapeZone(_tickActor))
			{
				EscapeMap(_tickActor);
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			OverrideValrathMerchantRoundCard();
			if (_tickActor != Marko)
			{
				OverrideMarkoRoundCard();
			}
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			OverrideValrathMerchantFocus();
			OverrideMarkoFocus();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TryEscapeMap();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 134);
			break;
		}
	}

	protected override bool AllowEscapeZoneInternal(CActor actor)
	{
		if (actor is CPlayerActor)
		{
			return escapeAllies.IsComplete;
		}
		return base.AllowEscapeZoneInternal(actor);
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		InitMarko();
		ActivateSpawners();
	}

	private void ActivateSpawners()
	{
		if (DoorOpenRound != 0)
		{
			return;
		}
		DoorOpenRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			spawner.SetActive(active: true, forceDontSpawn: true);
		}
	}

	private void InitMarko()
	{
		CEnemyActor marko = Marko;
		if (marko != null)
		{
			marko.AllowEscape = true;
			OverrideMarkoRoundCard();
		}
	}

	private void InitValrathMerchant()
	{
		CEnemyActor cEnemyActor = valrathMerchant;
		if (cEnemyActor != null)
		{
			cEnemyActor.AllowEscape = true;
		}
	}

	private void OverrideValrathMerchantRoundCard()
	{
		if (valrathMerchant == null || _tickActor != valrathMerchant || valrathMerchant.IsDead)
		{
			return;
		}
		List<CMonsterAbilityCard> abilityCardsPool = valrathMerchant.MonsterClass.CardsPoolClass.AbilityCardsPool;
		if (door.DoorIsLocked)
		{
			valrathMerchant.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 11534);
		}
		else
		{
			valrathMerchant.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 11535);
		}
	}

	private void OverrideMarkoRoundCard()
	{
		if (Marko == null || Marko.IsDead)
		{
			return;
		}
		List<CMonsterAbilityCard> abilityCardsPool = Marko.MonsterClass.CardsPoolClass.AbilityCardsPool;
		if (!ReachedMarko)
		{
			Marko.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 99990009);
		}
		else
		{
			Marko.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 99990010);
		}
	}

	private void OverrideValrathMerchantFocus()
	{
		if (valrathMerchant == null || _tickActor != valrathMerchant || valrathMerchant.IsDead)
		{
			return;
		}
		CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
		if (!door.DoorIsOpen)
		{
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(door.PropTile.m_ArrayIndex);
		}
		else if (!ReachedMarko)
		{
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.TargetGUID = Marko.ActorGuid;
		}
		else
		{
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			CTile cTile = (from x in ScenarioManager.CurrentScenarioState.GetEscapeZones()
				select ScenarioManager.GetTile(x) into x
				where x.LivingActor == null
				orderby _tickActor.GetDistanceTo(x)
				select x).FirstOrDefault();
			if (cTile != null)
			{
				cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(cTile.m_ArrayIndex);
			}
			else
			{
				cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(_tickActor.ArrayIndex);
			}
		}
		ScenarioScript._css.AllyMonsters.Find((EnemyState actor) => actor.ActorGuid == _tickActor.ActorGuid).SetAIFocusOverride(cAIFocusOverrideDetails);
	}

	private void OverrideMarkoFocus()
	{
		if (Marko != null && _tickActor == Marko && !Marko.IsDead)
		{
			CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			CTile cTile = (from x in ScenarioManager.CurrentScenarioState.GetEscapeZones()
				select ScenarioManager.GetTile(x) into x
				where x.LivingActor == null
				orderby _tickActor.GetDistanceTo(x)
				select x).FirstOrDefault();
			if (cTile != null)
			{
				cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(cTile.m_ArrayIndex);
			}
			else
			{
				cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(_tickActor.ArrayIndex);
			}
			ScenarioScript._css.AllyMonsters.Find((EnemyState actor) => actor.ActorGuid == _tickActor.ActorGuid).SetAIFocusOverride(cAIFocusOverrideDetails);
		}
	}

	public override void OnMessagetrigger(CLevelMessage message)
	{
		if (message.Pages.Any((CLevelMessagePage x) => x.PageTextKey.Contains("RFS_04_Waylaid_Room_2", StringComparison.InvariantCultureIgnoreCase) || x.PageTextKey.Contains("RFS_04_Waylaid_Special_2_1", StringComparison.InvariantCultureIgnoreCase)))
		{
			message.ShouldPauseGame = GameState.TurnActor != null && !(GameState.TurnActor is CPlayerActor);
		}
	}
}
