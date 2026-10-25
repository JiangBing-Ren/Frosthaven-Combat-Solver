using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH132 : ScenarioScript
{
	[Serializable]
	public class Storage132
	{
		public bool AltarLooted;

		public int NextCatIdolRound;
	}

	public Point AltarPosition = new Point(13, 4);

	[StateDump(Skip = true)]
	public Action OnAltarStateChanged;

	private CMap altarRoom;

	private CMap startingRoom;

	private CObjectDoor door1;

	private CObjectDoor door2;

	private CScenarioModifierTriggerAbility CatIdolSpeedModifier;

	private List<CScenarioModifierTriggerAbility> CatIdolTransferModifiers = new List<CScenarioModifierTriggerAbility>();

	private const string CatIdolResource = "CatIdol";

	private const string CatIdolSpeedResource = "CatIdolSpeedToken";

	private CScenarioModifier SpecialRule1;

	private CScenarioModifier SpecialRule2;

	private CScenarioModifier SpecialRule3;

	private CScenarioModifier SpecialRule4;

	private CObjective_Scripted KillFirstRoom;

	private CObjective_Scripted EndTurnNearAltar;

	private Storage132 serializedProperties;

	public bool AltarLooted
	{
		get
		{
			return serializedProperties.AltarLooted;
		}
		private set
		{
			serializedProperties.AltarLooted = value;
		}
	}

	public int NextCatIdolRound
	{
		get
		{
			return serializedProperties.NextCatIdolRound;
		}
		private set
		{
			serializedProperties.NextCatIdolRound = value;
		}
	}

	protected override bool Initialize()
	{
		altarRoom = ScenarioScript.FindRoom("Room_6");
		startingRoom = ScenarioScript.FindRoom("Room_5");
		door1 = ScenarioScript.FindDoor(17, 4);
		door2 = ScenarioScript.FindDoor(9, 4);
		CatIdolSpeedModifier = GetScenarioModifier("CatIdolReduceSpeedModifier") as CScenarioModifierTriggerAbility;
		CatIdolTransferModifiers.Add(GetScenarioModifier("GiveCatIdolDuringMovement") as CScenarioModifierTriggerAbility);
		CatIdolTransferModifiers.Add(GetScenarioModifier("GiveCatIdolDuringTurn") as CScenarioModifierTriggerAbility);
		SpecialRule1 = GetScenarioModifier("SpecialRule1");
		SpecialRule2 = GetScenarioModifier("SpecialRule2");
		SpecialRule3 = GetScenarioModifier("SpecialRule3");
		SpecialRule4 = GetScenarioModifier("SpecialRule4");
		KillFirstRoom = GetObjective<CObjective_Scripted>("KillFirstRoom");
		EndTurnNearAltar = GetObjective<CObjective_Scripted>("EndTurnNearAltar");
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage132)JsonConvert.DeserializeObject(storage, typeof(Storage132));
			ToggleAltarState(serializedProperties.AltarLooted);
		}
		else
		{
			serializedProperties = new Storage132
			{
				AltarLooted = false,
				NextCatIdolRound = -1
			};
			ToggleAltarState(looted: false);
		}
		UpdateKillFirstRoomObjective();
	}

	private void UpdateEndTurnNearAltarObjective()
	{
		EndTurnNearAltar.SetActivation(!AltarLooted && altarRoom.Revealed);
		EndTurnNearAltar.Total = 1;
		EndTurnNearAltar.Current = (AltarLooted ? 1 : 0);
		EndTurnNearAltar.ScriptedIsComplete = EndTurnNearAltar.Current >= EndTurnNearAltar.Total;
		SpecialRule1.SetDeactivated(!AltarLooted);
		SpecialRule2.SetDeactivated(!AltarLooted || ScenarioScript.PartySize < 3);
		SpecialRule3.SetDeactivated(!AltarLooted);
		SpecialRule4.SetDeactivated(!AltarLooted);
		GameState.CheckObjectives();
	}

	private void UpdateKillFirstRoomObjective()
	{
		List<CActor> list = (from x in ScenarioManager.CurrentScenarioState.AllEnemyStates
			where x.StartingMapGuid == startingRoom.MapGuid && x.Actor != null
			select x.Actor).ToList();
		KillFirstRoom.Total = list.Count;
		KillFirstRoom.Current = list.Count((CActor x) => x.IsDead);
		KillFirstRoom.ScriptedIsComplete = KillFirstRoom.Current >= KillFirstRoom.Total;
		KillFirstRoom.HideOnComplete = true;
		GameState.CheckObjectives();
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
		{
			CActor tickActor = _tickActor;
			CheckLootAltar();
			TryEscapeMap(tickActor);
			break;
		}
		case EScenarioModifierTriggerPhase.EndRound:
			if (OncePerRound() && ScenarioScript.RoundNumber == NextCatIdolRound)
			{
				EndRoundCatIdolTokens();
			}
			break;
		}
	}

	private void EndRoundCatIdolTokens()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (playerActor.CharacterHasResource("CatIdol", 1))
			{
				AddCatIdolTokens(playerActor, 1);
			}
			else
			{
				AddCatIdolTokens(playerActor, -1);
			}
		}
		NextCatIdolRound = ScenarioScript.RoundNumber + 2;
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == altarRoom)
		{
			ToggleAltarState(AltarLooted);
		}
	}

	private void ToggleAltarState(bool looted)
	{
		AltarLooted = looted;
		if (OnAltarStateChanged != null)
		{
			OnAltarStateChanged();
		}
		List<CTile> list = new List<CTile>();
		GameState.GetAdjacentTiles(AltarPosition, list);
		ScenarioManager.ToggleTileHighlights(list.Select((CTile x) => x.m_ArrayIndex), !AltarLooted && altarRoom.Revealed);
	}

	private void CheckLootAltar()
	{
		CActor tickActor = _tickActor;
		if (!(tickActor is CPlayerActor) || AltarLooted || tickActor.GetDistanceTo(AltarPosition) > 1)
		{
			return;
		}
		tickActor.ShowLootAnimations();
		ToggleAltarState(looted: true);
		List<CActor> list = new List<CActor>();
		foreach (CSpawner item in ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x.StartingMapGuid == altarRoom.MapGuid))
		{
			item.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true, forceUseAnyEntry: true, addIntoInitiativeOrder: true);
			item.HideSpawnerProp(forced: true);
			CEnemyActor cEnemyActor = ScenarioManager.GetTile(item.ArrayIndex).LivingActor as CEnemyActor;
			if (cEnemyActor != null && cEnemyActor.MonsterClass.RoundAbilityCard == null)
			{
				cEnemyActor.MonsterClass.SelectRoundAbilityCard();
			}
			if (cEnemyActor != null)
			{
				list.Add(cEnemyActor);
				GameState.InitiativeSortedActors.Remove(cEnemyActor);
			}
		}
		list.Sort(GameState.s_ActorInitiativeComparer);
		foreach (CActor item2 in list)
		{
			GameState.AddActorIntoInitiativeOrder(item2);
		}
		door2.SetHasExtraLock(hasExtraLock: false);
		GiveCatIdol(null, tickActor);
		NextCatIdolRound = ScenarioScript.RoundNumber + 1;
		GameState.TriggerCampaignSection("FS_132_Temple_of_Feline_Power_Special_2");
		UpdateEndTurnNearAltarObjective();
	}

	public static void AddCatIdolTokens(CActor actor, int amount)
	{
		int catIdolTokens = GetCatIdolTokens(actor);
		SetCatIdolTokens(actor, catIdolTokens + amount);
	}

	public static int GetCatIdolTokens(CActor actor)
	{
		return actor.GetCharacterResourceAmount("CatIdolSpeedToken");
	}

	public static void SetCatIdolTokens(CActor actor, int val)
	{
		int catIdolTokens = GetCatIdolTokens(actor);
		if (catIdolTokens > 0)
		{
			actor.RemoveCharacterResource("CatIdolSpeedToken", catIdolTokens);
		}
		actor.AddCharacterResource("CatIdolSpeedToken", val);
	}

	public static void GiveCatIdol(CActor from, CActor to)
	{
		ScenarioScript_FH132 scenarioScript_FH = ScenarioManager.CurrentScenarioState.ScenarioScripts.SingleOrDefault((ScenarioScript x) => x is ScenarioScript_FH132) as ScenarioScript_FH132;
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
		if (!(from x in to.FindApplicableActiveBonuses()
			where x.Ability.Name == "AReduceSpeedCatIdol"
			select x).Any())
		{
			if (from == null)
			{
				SetCatIdolTokens(to, 1);
			}
			scenarioScript_FH.CatIdolSpeedModifier.PerformScenarioModifier(ScenarioScript.RoundNumber, to, ScenarioScript.PartySize, forceActivate: true);
		}
		foreach (CScenarioModifierTriggerAbility catIdolTransferModifier in scenarioScript_FH.CatIdolTransferModifiers)
		{
			catIdolTransferModifier.PerformScenarioModifier(ScenarioScript.RoundNumber, to, ScenarioScript.PartySize, forceActivate: true);
		}
		to.AddCharacterResource("CatIdol", 1);
		if (from != null && ScenarioScript.PartySize >= 3)
		{
			AddCatIdolTokens(to, 1);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor is CEnemyActor)
		{
			if (ScenarioManager.Scenario.AllAliveMonsters.Count - 1 == 0 && !altarRoom.Revealed)
			{
				door1.ForceActivate(null);
				CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
				{
					m_Point = AltarPosition,
					m_Zoom = 0.3f
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.TriggerCampaignSection("FS_132_Temple_of_Feline_Power_Special_1");
				UpdateEndTurnNearAltarObjective();
			}
			UpdateKillFirstRoomObjective();
		}
	}
}
