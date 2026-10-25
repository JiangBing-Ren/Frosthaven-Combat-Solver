using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH072 : ScenarioScript
{
	private CObjectActor _iceBlock;

	private string _steelAutomatonGuid;

	private int _slideDifficulty = 3;

	private int _elementSlideMod;

	private int _moveStrength = 2;

	private CScenarioModifier _moveIceBlockModifier;

	private CScenarioModifier _slideDifficultyModifier;

	private CScenarioModifier _iceBlockMoveValueModifier;

	private CObjective_Scripted _winObjective;

	private Point _winTileIndex = new Point(20, 4);

	[StateDump(Skip = true)]
	private bool _room2;

	[StateDump(Skip = true)]
	private bool _room3;

	[StateDump(Skip = true)]
	private bool _room4;

	private int _RoundsAfterRevealingRoom4 = -1;

	private bool _spawnAutomatons;

	private List<Point> _steelCabinetCoordinates = new List<Point>
	{
		new Point(18, 2),
		new Point(16, 2),
		new Point(17, 6)
	};

	private CObjectProp _slideDifficultyVFX;

	private void CalcElementMod()
	{
		_elementSlideMod = 0;
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
		if (availableElements.Contains(ElementInfusionBoardManager.EElement.Fire))
		{
			_elementSlideMod--;
		}
		if (availableElements.Contains(ElementInfusionBoardManager.EElement.Ice))
		{
			_elementSlideMod++;
		}
	}

	protected override void TickPhaseInternal()
	{
		Trace("TickPhase " + _tickActor.Class.ID, null, 46);
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (_tickActor.Type == CActor.EType.Player)
			{
				_iceBlock.ForcedUntargetable = false;
			}
			else
			{
				_iceBlock.ForcedUntargetable = true;
			}
			CalcElementMod();
			UpdateSlideDifficulty();
			UpdateSlideDifficultyVFX();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			if (!_spawnAutomatons || _tickActor != _iceBlock)
			{
				break;
			}
			_RoundsAfterRevealingRoom4++;
			if (_RoundsAfterRevealingRoom4 <= (ScenarioScript.PartySize - 1) * 2 && _RoundsAfterRevealingRoom4 % 2 == 0)
			{
				List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.FindAll((CObjectProp prop) => _steelCabinetCoordinates.Contains(prop.PropTile.m_ArrayIndex));
				if (list.Count == 0)
				{
					_spawnAutomatons = false;
					break;
				}
				Point arrayIndex = list.First().PropTile.m_ArrayIndex;
				list.First().DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
				SpawnActorAtTile(arrayIndex.X, arrayIndex.Y, "SteelAutomatonID", out var _);
			}
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
		{
			if (_tickActor != _iceBlock)
			{
				break;
			}
			int num = ScenarioManager.CurrentScenarioState.Maps.Count((CMap map) => map.Revealed);
			if (num == 1)
			{
				_room2 = true;
				_slideDifficulty = 4;
				TrySpawnActorAtNearestTile(5, 1, "IceWraithID", out var _);
				if (ScenarioScript.PartySize == 4)
				{
					TrySpawnActorAtNearestTile(5, 7, "IceWraithEliteID", out var _);
				}
			}
			if (num == 2)
			{
				_room3 = true;
				_slideDifficulty = 5;
				TrySpawnActorAtNearestTile(4, 3, "LivingBonesID", out var _);
				if (ScenarioScript.PartySize > 2)
				{
					TrySpawnActorAtNearestTile(7, 6, "LivingBonesID", out var _);
				}
			}
			if (num == 3)
			{
				_room4 = true;
				_slideDifficulty = 3;
				_iceBlock.CanUnlockDoors = false;
				_steelAutomatonGuid = ScenarioManager.Scenario.Maps[num].Monsters.Find((EnemyState monster) => monster.ClassID == "SteelAutomatonID")?.ActorGuid;
				_spawnAutomatons = true;
				_RoundsAfterRevealingRoom4 = 0;
			}
			UpdateSlideDifficulty();
			UpdateSlideDifficultyVFX();
			break;
		}
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 144);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	public override bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		if (actor.Class.ID == "IceBlockID")
		{
			if (ability.Name == "CAMove72")
			{
				return true;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Push && ability is CAbilityPush cAbilityPush && cAbilityPush.GetStateName() != CAbilityPush.EPushState.ActorIsSelectingPushTile.ToString())
			{
				return true;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Pull && ability is CAbilityPull cAbilityPull && cAbilityPull.GetStateName() != CAbilityPull.EPullState.ActorIsSelectingPullTile.ToString())
			{
				return true;
			}
			if (ability.AbilityType == CAbility.EAbilityType.ControlActor && ability.IsScenarioModifierAbility)
			{
				return true;
			}
			if (ability.AbilityType == CAbility.EAbilityType.NullTargeting)
			{
				return true;
			}
		}
		return false;
	}

	private void MoveIceBlock(CActor targetingActor)
	{
		if (targetingActor.Type == CActor.EType.Player)
		{
			CAbility cAbility = CAbility.CopyAbility(_moveIceBlockModifier.TriggerAbility(targetingActor, 2, copyAbility: false), generateNewID: true, fullCopy: true);
			cAbility.TargetingActor = targetingActor;
			cAbility.IsScenarioModifierAbility = true;
			cAbility.TargetThisActorAutomatically = _iceBlock;
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SkipAnim = true;
			if (PhaseManager.Phase is CPhaseAction cPhaseAction)
			{
				cPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, targetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
			}
		}
	}

	protected override bool Initialize()
	{
		_moveIceBlockModifier = GetScenarioModifier("MoveIceBlock");
		_slideDifficultyModifier = GetScenarioModifier("SlideDifficulty");
		_iceBlockMoveValueModifier = GetScenarioModifier("IceBlockMoveValue");
		_winObjective = (CObjective_Scripted)GetObjective("WinObjective");
		_iceBlock = ScenarioManager.Scenario.AllObjects.Find((CObjectActor obj) => obj.Class.ID == "IceBlockID");
		_iceBlock.CanUnlockDoors = true;
		ScenarioManager.ToggleTileHighlights(new List<Point> { _winTileIndex }, enabled: true);
		if (ScenarioScript.PartySize > 2)
		{
			_moveStrength = 1;
		}
		else
		{
			_moveStrength = 2;
		}
		_slideDifficultyModifier.CustomLocKeyArgs = new List<object> { _slideDifficulty };
		_iceBlockMoveValueModifier.CustomLocKeyArgs = new List<object> { _moveStrength };
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(_iceBlock, active: false, onlyHealthBar: true);
		ScenarioRuleClient.MessageHandler(message);
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_steelAutomatonGuid);
		string text2 = JsonConvert.SerializeObject(_slideDifficulty);
		string text3 = JsonConvert.SerializeObject(ScenarioManager.PathFinder.GetNode(_winTileIndex).Blocked);
		string text4 = JsonConvert.SerializeObject(_elementSlideMod);
		string text5 = JsonConvert.SerializeObject(_iceBlock.CanUnlockDoors);
		string text6 = JsonConvert.SerializeObject(_RoundsAfterRevealingRoom4);
		string text7 = JsonConvert.SerializeObject(_spawnAutomatons);
		string text8 = JsonConvert.SerializeObject(new string[7] { text, text2, text3, text4, text5, text6, text7 });
		Trace("[ffa000]Serialize: " + text8 + "[-]", null, 235);
		return text8;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 241);
		if (string.IsNullOrEmpty(s))
		{
			UpdateSlideDifficulty();
			UpdateSlideDifficultyVFX();
			return;
		}
		string[] array = JsonConvert.DeserializeObject<string[]>(s);
		_steelAutomatonGuid = JsonConvert.DeserializeObject<string>(array[0]);
		_slideDifficulty = JsonConvert.DeserializeObject<int>(array[1]);
		if (array.Length > 2)
		{
			ScenarioManager.PathFinder.GetNode(_winTileIndex).Blocked = JsonConvert.DeserializeObject<bool>(array[2]);
		}
		if (array.Length > 3)
		{
			_elementSlideMod = JsonConvert.DeserializeObject<int>(array[3]);
			_iceBlock.CanUnlockDoors = JsonConvert.DeserializeObject<bool>(array[4]);
		}
		if (array.Length > 6)
		{
			_RoundsAfterRevealingRoom4 = JsonConvert.DeserializeObject<int>(array[5]);
			_spawnAutomatons = JsonConvert.DeserializeObject<bool>(array[6]);
		}
		else
		{
			_RoundsAfterRevealingRoom4 = 0;
			List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.FindAll((CObjectProp prop) => _steelCabinetCoordinates.Contains(prop.PropTile.m_ArrayIndex));
			_spawnAutomatons = list.Count > 0;
		}
		UpdateSlideDifficulty();
		UpdateSlideDifficultyVFX();
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.ActorGuid == _steelAutomatonGuid)
		{
			_iceBlock.CanUnlockDoors = true;
		}
	}

	protected override void TickAbilityStartedInternal()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(_winTileIndex);
		node.Blocked = true;
		if (_tickAbility.Name == "CAMove72")
		{
			node.Blocked = false;
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility.Name == "CAMove72")
		{
			return;
		}
		if (_tickAbility is CAbilityAttack cAbilityAttack && cAbilityAttack.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			CAttackSummary.TargetSummary targetSummary = cAbilityAttack.TotalTargetSummaries.FirstOrDefault((CAttackSummary.TargetSummary x) => x.ActorToAttack == _iceBlock);
			if (targetSummary != null)
			{
				int num = targetSummary.FinalAttackStrength;
				if (cAbilityAttack.SubAbilities.Any((CAbility x) => x is CAbilityPull || x is CAbilityPush))
				{
					num += 2;
				}
				if (num >= _slideDifficulty + _elementSlideMod)
				{
					CCombatLogMessage_MessageData cCombatLogMessage_MessageData = new CCombatLogMessage_MessageData();
					cCombatLogMessage_MessageData.m_MessageKey = "COMBAT_LOG_MOVES_ICE_BLOCK";
					cCombatLogMessage_MessageData.m_Params = new List<string>
					{
						"Actor_0",
						num.ToString()
					};
					cCombatLogMessage_MessageData.m_Actors = new List<CActor> { _tickAbility.TargetingActor };
					ScenarioRuleClient.MessageHandler(cCombatLogMessage_MessageData);
					MoveIceBlock(_tickAbility.TargetingActor);
				}
			}
		}
		if (_tickAbility.ParentAbility == null || !(_tickAbility.ParentAbility is CAbilityAttack))
		{
			if (_tickAbility.AbilityType == CAbility.EAbilityType.Push && (_tickAbility.ActorsTargeted.Contains(_iceBlock) || (_tickAbility is CAbilityPush cAbilityPush && cAbilityPush.CurrentTarget == _iceBlock)))
			{
				MoveIceBlock(_tickActor);
			}
			if (_tickAbility.AbilityType == CAbility.EAbilityType.Pull && (_tickAbility.ActorsTargeted.Contains(_iceBlock) || (_tickAbility is CAbilityPull cAbilityPull && cAbilityPull.CurrentTarget == _iceBlock)))
			{
				MoveIceBlock(_tickActor);
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor != _iceBlock)
		{
			return;
		}
		foreach (CObjectProp item in _iceBlock.Tile.FindProps(ScenarioManager.ObjectImportType.Trap))
		{
			item.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
		}
		if (_iceBlock.Tile.m_ArrayIndex == _winTileIndex)
		{
			_winObjective.ScriptedIsComplete = true;
		}
		else
		{
			_winObjective.ScriptedIsComplete = false;
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		UpdateSlideDifficultyVFX();
	}

	private void UpdateSlideDifficulty()
	{
		_slideDifficultyModifier.CustomLocKeyArgs = new List<object> { _slideDifficulty + _elementSlideMod };
		ScenarioManager.CurrentScenarioState.UpdateScenarioModifiersDescriptions();
	}

	private void UpdateSlideDifficultyVFX()
	{
		CTile tile = _iceBlock.Tile;
		if (_slideDifficultyVFX == null)
		{
			_slideDifficultyVFX = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.VFXProp && x.PrefabName == EVFXPropType.NumberCounterActorDescending.ToString());
		}
		if (_slideDifficultyVFX == null)
		{
			_slideDifficultyVFX = SpawnVFXProp(tile, EVFXPropType.NumberCounterActorDescending.ToString(), null, null, new CVector3(0f, 0f, 0f));
		}
		else
		{
			_slideDifficultyVFX.PropTile.DelProp(_slideDifficultyVFX);
			_slideDifficultyVFX.SetLocation(new TileIndex(tile.m_ArrayIndex), null, _slideDifficultyVFX.Rotation);
			tile.AddProp(_slideDifficultyVFX);
			_slideDifficultyVFX.SetNewStartingMapGuid(tile.m_HexMap.MapGuid);
			CMoveProp_MessageData message = new CMoveProp_MessageData
			{
				m_MoveProp = _slideDifficultyVFX,
				m_MoveSpeed = 0f,
				m_MoveFromTile = tile,
				m_MoveToTile = tile
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		CChangePropVisualState_MessageData message2 = new CChangePropVisualState_MessageData
		{
			m_prop = _slideDifficultyVFX,
			m_stateName = "State_" + (_slideDifficulty + _elementSlideMod)
		};
		ScenarioRuleClient.MessageHandler(message2);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Ability)
		{
			return false;
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 404);
		if (_room2 && message.MessageName.Contains("FS072_1"))
		{
			Trace("...success", null, 408);
			return true;
		}
		if (_room3 && message.MessageName.Contains("FS072_2"))
		{
			Trace("...success", null, 414);
			return true;
		}
		if (_room4 && message.MessageName.Contains("FS072_3"))
		{
			Trace("...success", null, 420);
			return true;
		}
		Trace("...failed", null, 424);
		return false;
	}

	public override bool AllowPushWhenNoPushTiles(CActor actor)
	{
		return actor == _iceBlock;
	}

	public override bool AllowPullWhenNoPushTiles(CActor actor)
	{
		return actor == _iceBlock;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("move_value", ((ScenarioScript.PartySize > 2) ? 1 : 2).ToString());
	}

	public override bool UpdateElementsBoard()
	{
		if (ScenarioManager.Scenario == null || ScenarioManager.Scenario.CurrentScenarioResult != SEventActorFinishedScenario.EScenarioResult.None)
		{
			return false;
		}
		bool result = false;
		int elementSlideMod = _elementSlideMod;
		CalcElementMod();
		if (elementSlideMod != _elementSlideMod)
		{
			UpdateSlideDifficulty();
			UpdateSlideDifficultyVFX();
			result = true;
		}
		return result;
	}
}
