using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CAbilitySummon : CAbility
{
	public enum SummonState
	{
		SelectSummonPosition,
		Summon,
		NoTilesAvailable,
		SummonDone
	}

	private SummonState m_State;

	private bool m_Summoned;

	private CActor m_ActorToCopy;

	public List<CActor> SummonedActors { get; set; }

	[JsonProperty]
	public List<string> SummonIDs { get; set; }

	public string SelectedSummonID
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState == null || ScenarioManager.CurrentScenarioState.Players.Count <= 0)
			{
				return SummonIDs[0];
			}
			return SummonIDs[ScenarioManager.CurrentScenarioState.Players.Count - 1];
		}
	}

	public HeroSummonYMLData SelectedSummonYMLData => ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == SelectedSummonID);

	public MonsterYMLData SelectedMonsterYMLData => ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == SelectedSummonID);

	public bool IsMonsterSummon => SelectedMonsterYMLData != null;

	public string SelectedLocKey
	{
		get
		{
			if (!IsMonsterSummon)
			{
				return SelectedSummonYMLData.LocKey;
			}
			return SelectedMonsterYMLData.LocKey;
		}
	}

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.HeroSummons;
		}
	}

	public CAbilitySummon(List<string> summons)
	{
		if (summons.Count != 4)
		{
			throw new Exception("Invalid size for summons list.  Must be exactly 4.  Current size: " + summons.Count);
		}
		SummonIDs = summons;
		SummonedActors = new List<CActor>();
		Range = 1;
	}

	public static CAbility CreateCompanionSummon(string companionSummonID)
	{
		return CAbility.CreateAbility(EAbilityType.Summon, 1, useSpecialBaseStat: false, 1, 1, 0, EAbilityFlag.None, new CAbilityFilterContainer(), "Summon", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultAttack", new List<CEnhancement>(), null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, new List<string> { companionSummonID, companionSummonID, companionSummonID, companionSummonID }, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster: false, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_ActorToCopy = VarsContext.GetVar("ActorToCopy").GetObject<CActor>();
		if (m_ActorToCopy != null)
		{
			string iD = m_ActorToCopy.Class.ID;
			SummonIDs = new List<string> { iD, iD, iD, iD };
		}
		m_State = SummonState.SelectSummonPosition;
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
		{
			m_CanUndo = false;
			m_CanSkip = false;
		}
		m_Summoned = false;
		if (UseSubAbilityTargeting && IsInlineSubAbility)
		{
			foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
			{
				if (ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex) == null)
				{
					m_TilesSelected.Add(inlineSubAbilityTile);
				}
			}
			if (m_TilesSelected.Count > 0)
			{
				m_State = SummonState.Summon;
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		if (ParentAbility != null)
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AllTargetsAdjacentToParentTargets.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.Value)
				{
					List<CTile> list = new List<CTile>();
					if (ParentAbility is CAbilityCreate cAbilityCreate)
					{
						list.AddRange(cAbilityCreate.TilesSelected);
					}
					else if (ParentAbility is CAbilityCreateTerrain cAbilityCreateTerrain)
					{
						list.AddRange(cAbilityCreateTerrain.TilesSelected);
					}
					else
					{
						list.AddRange(ParentAbility.ActorsTargeted.Select((CActor x) => x.Tile));
					}
					if (list.Count == 0)
					{
						SetCancelAbilityFlag(cancel: true);
					}
				}
			}
		}
		if (GameState.CurrentSelectAndPlayCardAbility != null)
		{
			List<CTile> list2 = GameState.CurrentSelectAndPlayCardAbility.GetVar("ValidSummonPositions").GetObject<List<CTile>>(GameState.CurrentSelectAndPlayCardAbility.VarsContext);
			if (list2 != null)
			{
				Targeting = EAbilityTargeting.Custom;
				if (TargetingVars == null)
				{
					TargetingVars = new List<(string, TValue)>();
				}
				TargetingVars.Add(("AddTilesInRange", new TValue(list2)));
			}
		}
		if (SelectedSummonID == "Empty")
		{
			SetCancelAbilityFlag(cancel: true);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (m_State == SummonState.SelectSummonPosition && TilesInRange.Count == 0)
		{
			return false;
		}
		return base.CanBePerformed(performer);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		List<CTile> list2;
		List<string> list3;
		List<CTile> list4;
		bool flag;
		switch (m_State)
		{
		case SummonState.SelectSummonPosition:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (AreaEffect == null)
				{
					if (Targeting == EAbilityTargeting.Custom)
					{
						SharedAbilityTargeting.GetValidActorsInRange(this);
					}
					else
					{
						TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: true, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: true);
					}
					List<CTile> list = new List<CTile>();
					foreach (CTile item2 in TilesInRange)
					{
						if (CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, IsMonsterSummon ? SelectedMonsterYMLData.ID : null, null, VarsContext) && item2.m_HexMap.Revealed)
						{
							list.Add(item2);
						}
					}
					TilesInRange = list.ToList();
				}
				for (int num = TilesInRange.Count - 1; num >= 0; num--)
				{
					CTile cTile = TilesInRange[num];
					if (ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex) != null)
					{
						TilesInRange.Remove(cTile);
					}
				}
				if (TilesInRange.Count == 0 && m_ActorToCopy != null)
				{
					int num2 = Range + 1;
					while (TilesInRange.Count == 0)
					{
						TilesInRange.AddRange(GameState.GetTilesInRange(TargetingActor, num2, Targeting, emptyTilesOnly: true, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true));
						num2++;
					}
				}
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(TilesInRange, this);
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message);
				break;
			}
			list2 = new List<CTile>();
			list3 = new List<string>();
			list4 = new List<CTile>();
			if (Targeting == EAbilityTargeting.Custom)
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
				if (TilesSelected.Count > 0)
				{
					list4.AddRange(TilesSelected);
				}
				else
				{
					list4.AddRange(TilesInRange);
				}
			}
			else
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.UseParentTiles.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.UseParentTiles.Value && ParentAbility != null && ParentAbility.ActorsTargeted != null && ParentAbility.ActorsTargeted.Count > 0 && ParentAbility.AreaEffect == null)
					{
						CTile propTile = null;
						foreach (CActor item3 in ParentAbility.ActorsTargeted)
						{
							CTile cTile2 = ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y];
							if (!list4.Contains(cTile2) && ScenarioManager.PathFinder.Nodes[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(cTile2, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, IsMonsterSummon ? SelectedMonsterYMLData.ID : null, null, VarsContext) && CObjectProp.FindPropWithPathingBlocker(cTile2.m_ArrayIndex, ref propTile) == null)
							{
								list4.Add(cTile2);
								continue;
							}
							list2.Add(cTile2);
							list3.Add("Invalid parent tile");
						}
						if (list4.Count == 0 && list2.Count == 0)
						{
							list2.Add(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
							list3.Add("Actor has no target");
						}
						goto IL_06b8;
					}
				}
				if (OnDeath)
				{
					CTile item = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y];
					list4.Add(item);
				}
				else
				{
					CTile cTile3 = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y];
					AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
						if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value && ParentAbility.AreaEffect == null)
						{
							CActor cActor = ParentAbility.ActorsTargeted[0];
							cTile3 = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
						}
					}
					if (Range <= 0)
					{
						Range = 1;
					}
					TilesInRange = GameState.GetTilesInRange(cTile3.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
					List<CTile> list5 = new List<CTile>();
					foreach (CTile item4 in TilesInRange)
					{
						if (CAbilityFilter.IsValidTile(item4, TileFilter, includeInitial: false, null, null, VarsContext) && item4.FindProp(ScenarioManager.ObjectImportType.Trap) == null)
						{
							list5.Add(item4);
						}
					}
					TilesInRange = list5.ToList();
					CAIMoveNew.GetPossibleEmptyTiles(TargetingActor, cTile3, TilesInRange, list2, list3, list4, SelectedMonsterYMLData, IsMonsterSummon);
				}
			}
			goto IL_06b8;
		case SummonState.Summon:
		{
			m_CanUndo = false;
			if (!TrySpawnSummon(TargetingActor, SelectedSummonYMLData?.ID, SelectedMonsterYMLData?.ID, m_TilesSelected, out var error, AnimOverload, this, m_ActorToCopy, SummonedActors, out var done, ref m_Summoned, ref m_CancelAbility, TargetingActor?.GetMirrorAbilitiesActor()))
			{
				Log.ErrorWithReport(error);
				return false;
			}
			if (done)
			{
				m_State = SummonState.SummonDone;
			}
			if (m_CancelAbility)
			{
				PhaseManager.StepComplete();
			}
			break;
		}
		case SummonState.NoTilesAvailable:
			{
				PhaseManager.StepComplete();
				break;
			}
			IL_06b8:
			flag = false;
			if (IsMonsterSummon)
			{
				CMonsterClass cMonsterClass = MonsterClassManager.Find(SelectedMonsterYMLData?.ID);
				int nextID = cMonsterClass.GetNextID();
				if (nextID == -1)
				{
					flag = true;
				}
				else
				{
					cMonsterClass.RecycleID(nextID);
				}
			}
			else
			{
				CHeroSummonClass cHeroSummonClass = CharacterClassManager.FindHeroSummonClass(SelectedSummonYMLData.ID);
				int nextID2 = cHeroSummonClass.GetNextID();
				if (nextID2 == -1)
				{
					flag = true;
				}
				else
				{
					cHeroSummonClass.RecycleID(nextID2);
				}
			}
			if (list4.Count > 0 && !flag)
			{
				CAIMoveNew.SelectTilesBasedOnTargetingFocus(TargetingActor, Range, list4, Targeting, TilesSelected, NumberTargetsRemaining, AllTargets);
				m_State = SummonState.Summon;
				Perform();
				return true;
			}
			Log.SimpleInfo(TargetingActor?.Class?.ID + " Failed to summon adjacent to " + ParentAbility?.ActorsTargeted[0]?.Class?.ID);
			for (int i = 0; i < list2.Count; i++)
			{
				Log.SimpleInfo("Tile at x:" + list2[i].m_ArrayIndex.X + " y:" + list2[i].m_ArrayIndex.Y + ", fail reason=" + list3[i]);
			}
			m_State = SummonState.NoTilesAvailable;
			Perform();
			return true;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!TilesSelected.Contains(selectedTile) && m_State == SummonState.SelectSummonPosition)
		{
			if (TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
						m_NumberTargetsRemaining--;
					}
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(TargetingActor));
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == SummonState.SelectSummonPosition)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == SummonState.SelectSummonPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == SummonState.SelectSummonPosition;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State >= SummonState.SummonDone;
	}

	public override string GetDescription()
	{
		return "Summon";
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool Validate()
	{
		foreach (string summonID in SummonIDs)
		{
			if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonID) == null && ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonID) == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(GetAbilityBaseCard().Name, "Unable to find Summon " + summonID + ".  GetAbilityBaseCard() Name: " + GetAbilityBaseCard().Name);
				return false;
			}
		}
		return true;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SummonState summonState = m_State;
		if (m_State == SummonState.SummonDone && !m_Summoned)
		{
			summonState = SummonState.NoTilesAvailable;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilitySummon(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), summonState, SummonIDs[ScenarioManager.CurrentScenarioState.Players.Count - 1]));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public static bool TrySpawnSummon(CActor summoner, string summonID, string monsterID, IList<CTile> tilesSelected, out string error, string animOverload = null)
	{
		bool summoned = false;
		bool cancelAbility = false;
		bool done;
		return TrySpawnSummon(summoner, summonID, monsterID, tilesSelected.ToList(), out error, animOverload, null, null, null, out done, ref summoned, ref cancelAbility);
	}

	public static bool TrySpawnSummon(CActor summoner, string summonID, string monsterID, List<CTile> tilesSelected, out string error, string animOverload, CAbility ability, CActor actorToCopy, List<CActor> summonedActors, out bool done, ref bool summoned, ref bool cancelAbility, CActor summonerOverride = null)
	{
		done = false;
		error = "";
		bool isSummon = true;
		if (ability != null && ability.GetMiscVar("DontTreatAsSummon").GetBool())
		{
			isSummon = false;
		}
		if (summoner == null)
		{
			error = "No summoner";
			return false;
		}
		if (summonerOverride == null)
		{
			summonerOverride = summoner;
		}
		summonedActors = ((summonedActors != null) ? summonedActors : new List<CActor>());
		HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonID);
		MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == monsterID);
		bool flag = monsterYMLData != null;
		if (summonerOverride.Type == CActor.EType.Player)
		{
			summonerOverride.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
		}
		CMonsterClass cMonsterClass = null;
		CHeroSummonClass cHeroSummonClass = null;
		if (flag)
		{
			cMonsterClass = MonsterClassManager.Find(monsterYMLData.ID);
		}
		else
		{
			cHeroSummonClass = CharacterClassManager.FindHeroSummonClass(heroSummonYMLData.ID);
		}
		if (cMonsterClass == null && (cHeroSummonClass == null || !cHeroSummonClass.IsValidClassName()))
		{
			error = "Unable to find class for summon " + heroSummonYMLData.ID;
			return false;
		}
		foreach (CTile item in tilesSelected)
		{
			if (cMonsterClass == null && cHeroSummonClass == null)
			{
				continue;
			}
			if (cMonsterClass != null)
			{
				int num = cMonsterClass.Health();
				int maxHealth = num;
				CEnemyActor cEnemyActor = null;
				bool flag2 = true;
				if (cMonsterClass is CObjectClass)
				{
					int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
					ObjectState objectState = new ObjectState(cMonsterClass.ID, chosenModelIndex, null, item.m_HexMap.MapGuid, new TileIndex(item.m_ArrayIndex), num, maxHealth, summonerOverride.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, summonerOverride.ActorGuid, 1, summonerOverride.OriginalType);
					cEnemyActor = ScenarioManager.Scenario.AddObject(objectState, initial: false);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
					}
					else
					{
						summoned = false;
					}
				}
				else
				{
					int chosenModelIndex2 = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
					List<PositiveConditionPair> list = new List<PositiveConditionPair>();
					List<NegativeConditionPair> list2 = new List<NegativeConditionPair>();
					CActor.EType eType = summonerOverride.OriginalType;
					if (summonerOverride is CEnemyActor { Type: CActor.EType.Ally, IsSummon: not false, PlayerSummoner: not null })
					{
						eType = CActor.EType.Enemy;
					}
					if (actorToCopy != null)
					{
						num = actorToCopy.Health;
						list = actorToCopy.Tokens.CheckPositiveTokens;
						list2 = actorToCopy.Tokens.CheckNegativeTokens;
						eType = ((eType == CActor.EType.Player) ? CActor.EType.Ally : summonerOverride.OriginalType);
						flag2 = false;
					}
					else if (summonerOverride is CPlayerActor { Type: CActor.EType.Player })
					{
						eType = CActor.EType.Ally;
						flag2 = GameState.TurnActor != null;
					}
					EnemyState enemyState = new EnemyState(cMonsterClass.ID, chosenModelIndex2, null, item.m_HexMap.MapGuid, new TileIndex(item.m_ArrayIndex), num, maxHealth, summonerOverride.Level, list, list2, flag2, CActor.ECauseOfDeath.StillAlive, isSummon, summonerOverride.ActorGuid, 1, eType);
					switch (eType)
					{
					case CActor.EType.Enemy:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
						}
						else
						{
							summoned = false;
						}
						break;
					case CActor.EType.Player:
					case CActor.EType.Ally:
						cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (actorToCopy != null && summonerOverride.OriginalType == CActor.EType.Player)
						{
							CBaseCard cBaseCard = ((ability != null) ? summonerOverride.FindCardWithAbility(ability) : null);
							foreach (PositiveConditionPair item2 in list)
							{
								cEnemyActor.Tokens.AddPositiveToken(item2.PositiveCondition, item2.RoundDuration, item2.ConditionDecTrigger, cEnemyActor, summonerOverride);
							}
							foreach (NegativeConditionPair item3 in list2)
							{
								cEnemyActor.Tokens.AddNegativeToken(item3.NegativeCondition, item3.RoundDuration, item3.ConditionDecTrigger, cEnemyActor, item3.Strength);
							}
							cBaseCard?.AddActiveBonus(ability, cEnemyActor, summonerOverride);
						}
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
						}
						else
						{
							summoned = false;
						}
						break;
					case CActor.EType.Enemy2:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
						}
						else
						{
							summoned = false;
						}
						break;
					case CActor.EType.Neutral:
						cEnemyActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
						}
						else
						{
							summoned = false;
						}
						break;
					default:
						error = "Non monster tried to summon a monster actor";
						return false;
					}
				}
				if (cEnemyActor != null)
				{
					summonedActors.Add(cEnemyActor);
					if (summoner != null && summoner.InitiativeLockedDuringRound.HasValue)
					{
						cEnemyActor.RecalcLockedInitiative();
					}
					cEnemyActor.MarkWontPlayThisRound(flag2);
					GameState.SortIntoInitiativeAndIDOrder();
					ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor, currentScenarioState.Players.Count);
					if (ability != null && ability.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == EAbilityStatType.Strength))
					{
						maxHealth = cEnemyActor.MaxHealth;
						int strength = ability.Strength;
						ability.SetStatBasedOnX(summonerOverride, ability.StatIsBasedOnXEntries, ability.AbilityFilter);
						num = Math.Min(ability.Strength, maxHealth);
						if (num != cEnemyActor.Health)
						{
							cEnemyActor.Health = num;
						}
						ability.Strength = strength;
					}
					foreach (CObjectProp prop in item.m_Props)
					{
						if (prop.ObjectType == ScenarioManager.ObjectImportType.PressurePlate)
						{
							prop.Activate(cEnemyActor, cEnemyActor);
						}
					}
					ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				}
			}
			else
			{
				CBaseCard cBaseCard2 = ((ability != null) ? summonerOverride.FindCardWithAbility(ability) : null);
				int chosenModelIndex3 = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cHeroSummonClass.Models.Count);
				float baseHealthModifier = 1f;
				if (ScenarioManager.CurrentScenarioState != null)
				{
					baseHealthModifier = ScenarioManager.CurrentScenarioState.SummonHealthModifier;
				}
				HeroSummonState heroSummonState = new HeroSummonState(cHeroSummonClass.ID, chosenModelIndex3, null, item.m_HexMap.MapGuid, new TileIndex(item.m_ArrayIndex), heroSummonYMLData.GetHealth(summonerOverride.Level, baseHealthModifier), heroSummonYMLData.GetHealth(summonerOverride.Level, baseHealthModifier), summonerOverride.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, 1, summonerOverride.ActorGuid);
				CHeroSummonActor cHeroSummonActor = ScenarioManager.Scenario.AddHeroSummon(heroSummonState, initial: false, noIDRegen: false, ability);
				if (cHeroSummonActor != null)
				{
					cHeroSummonActor.SummonedOrderIndex = ScenarioManager.Scenario.HeroSummons.Count;
					if (GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon && summonerOverride is CPlayerActor cPlayerActor2)
					{
						cPlayerActor2.SetCompanionGuid(cHeroSummonActor.ActorGuid);
					}
					cBaseCard2?.AddActiveBonus(ability, cHeroSummonActor, summonerOverride);
					cHeroSummonActor.MarkWontPlayThisRound(summoner == null || GameState.TurnActor == summoner || summoner.PlayedThisRound);
					if (summoner != null && summoner.InitiativeLockedDuringRound.HasValue)
					{
						cHeroSummonActor.RecalcLockedInitiative();
					}
					summonedActors.Add(cHeroSummonActor);
					GameState.SortIntoInitiativeAndIDOrder();
					ScenarioState currentScenarioState2 = ScenarioManager.CurrentScenarioState;
					cHeroSummonActor.OnSummoned();
					currentScenarioState2.RefreshHeroSummons(cHeroSummonActor);
					currentScenarioState2.PerformOnSpawnedModifiers(cHeroSummonActor, currentScenarioState2.Players.Count);
					ScenarioManager.Scenario.OnFinishedHeroSummon(heroSummonState);
					foreach (CObjectProp prop2 in item.m_Props)
					{
						if (prop2.ObjectType == ScenarioManager.ObjectImportType.PressurePlate)
						{
							prop2.Activate(cHeroSummonActor, cHeroSummonActor);
						}
					}
				}
				else
				{
					summoned = false;
				}
			}
			done = true;
		}
		if (tilesSelected.Count > 0)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			if (ability != null)
			{
				ability.AbilityHasHappened = true;
				string text = ability.GetMiscVar("OverrideSummonModel").GetString();
				if (!string.IsNullOrEmpty(text))
				{
					foreach (CActor summonedActor in summonedActors)
					{
						summonedActor.OverrideCharacterModel = text;
					}
				}
			}
			CSummon_MessageData message = new CSummon_MessageData(ability?.AnimOverload, summoner)
			{
				m_ActorSummoning = summoner,
				m_SummonAbility = (ability as CAbilitySummon),
				m_SummonedActors = summonedActors.ToList(),
				m_SummonTiles = tilesSelected
			};
			ScenarioRuleClient.MessageHandler(message);
			summoned = true;
		}
		else
		{
			cancelAbility = true;
		}
		return true;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}
}
