using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityDestroyObstacle : CAbility
{
	public enum DestroyObstacleState
	{
		SelectObstaclesPosition,
		PreDestroyObstacleProcessing,
		AnimateCharacter,
		DestroyObstacles,
		DestroyObstaclesDone
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class DestroyObstacleData
	{
		[JsonProperty]
		public bool CanTargetEmptyHexes;

		[JsonProperty]
		public bool CanDestroyObstaclesWithHP;

		[JsonProperty]
		public bool BypassOverrideDisallowDestroyAndMove;

		[JsonProperty]
		public string TargetPrefabName;

		public DestroyObstacleData Copy()
		{
			return (DestroyObstacleData)MemberwiseClone();
		}

		public static DestroyObstacleData DefaultDestroyObstacleData()
		{
			return new DestroyObstacleData
			{
				CanTargetEmptyHexes = false,
				CanDestroyObstaclesWithHP = false,
				BypassOverrideDisallowDestroyAndMove = false,
				TargetPrefabName = null
			};
		}
	}

	private DestroyObstacleState m_State;

	private Dictionary<string, int> m_destroyedObstaclesDictionary;

	private int m_NumberOfTilesBlockedDestroyed;

	public DestroyObstacleState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	[JsonProperty]
	public DestroyObstacleData AbilityDestroyObstacleData { get; set; }

	public CAbilityDestroyObstacle(DestroyObstacleData destroyObstacleData)
	{
		AbilityDestroyObstacleData = destroyObstacleData;
	}

	private bool IsHealthObstacleOnTile(CTile tile)
	{
		CObjectObstacle cObjectObstacle = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
		if (cObjectObstacle == null)
		{
			CTile propTile = null;
			cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref propTile);
		}
		if (cObjectObstacle == null)
		{
			return false;
		}
		if (cObjectObstacle.PropHealthDetails != null)
		{
			return cObjectObstacle.PropHealthDetails.HasHealth;
		}
		return false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = DestroyObstacleState.SelectObstaclesPosition;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
			{
				if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (UseSubAbilityTargeting)
		{
			if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
			{
				m_ValidTilesInAreaEffect.AddRange(ParentAbility.InlineSubAbilityTiles);
			}
			else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
			{
				if (ParentAbility.AreaEffect != null)
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
				}
				else
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.TilesSelected);
				}
			}
		}
		else if (AreaEffect == null)
		{
			if (Range == 0)
			{
				m_ValidTilesInAreaEffect = new List<CTile>();
				m_ValidTilesInAreaEffect.Add(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
			}
			else
			{
				m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			TilesInRange = m_ValidTilesInAreaEffect.ToList();
		}
		else
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks);
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if ((AbilityDestroyObstacleData.BypassOverrideDisallowDestroyAndMove || !prop.OverrideDisallowDestroyAndMove) && (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP || prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth))
			{
				continue;
			}
			foreach (CTile item in from x in prop.GetTiles()
				select ScenarioManager.GetTile(x))
			{
				m_ValidTilesInAreaEffect.Remove(item);
				TilesInRange.Remove(item);
			}
		}
		if (!string.IsNullOrEmpty(AbilityDestroyObstacleData.TargetPrefabName))
		{
			m_ValidTilesInAreaEffect.RemoveAll((CTile t) => t.m_Props.Find((CObjectProp p) => p.PrefabName == AbilityDestroyObstacleData.TargetPrefabName) == null);
			TilesInRange = m_ValidTilesInAreaEffect;
		}
		ExpandTileListWithLinkedProps(m_ValidTilesInAreaEffect);
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
			foreach (CTile item2 in m_ValidTilesInAreaEffect)
			{
				CTile propTile = null;
				CObjectObstacle cObjectObstacle = item2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null && !cObjectObstacle.Activated)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						CTile cTile2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
						if (cTile2 != null && !m_TilesSelected.Contains(cTile2))
						{
							m_TilesSelected.Add(cTile2);
						}
					}
				}
				if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && GameState.GetActorsOnTile(item2, FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), ActorsToIgnore, isTargetedAbility: false, false).Count > 0 && !m_TilesSelected.Contains(item2))
				{
					m_TilesSelected.Add(item2);
				}
				if (AbilityDestroyObstacleData.CanTargetEmptyHexes && CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext) && !m_TilesSelected.Contains(item2))
				{
					m_TilesSelected.Add(item2);
				}
			}
		}
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberOfTilesBlockedDestroyed = 0;
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
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
		switch (m_State)
		{
		case DestroyObstacleState.SelectObstaclesPosition:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
					if (m_TilesSelected.Count <= 0)
					{
						SetCancelAbilityFlag(cancel: true);
						if (IsMergedAbility)
						{
							PhaseManager.StepComplete();
						}
						else
						{
							PhaseManager.NextStep();
						}
					}
					else if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
					{
						PhaseManager.StepComplete();
						break;
					}
				}
				if (!AbilityHasBeenCancelled)
				{
					List<CAbilityFilter.EFilterTile> list3 = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Obstacle };
					if (AbilityDestroyObstacleData.CanTargetEmptyHexes)
					{
						list3.Add(CAbilityFilter.EFilterTile.EmptyHex);
					}
					if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
					{
						list3.Add(CAbilityFilter.EFilterTile.ObjectActor);
					}
					else
					{
						list3.Remove(CAbilityFilter.EFilterTile.Obstacle);
						list3.Add(CAbilityFilter.EFilterTile.ObstacleWithoutHealth);
					}
					CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
					{
						m_TileFilter = list3,
						m_IsPositive = IsPositive(),
						m_SpawnType = ScenarioManager.ObjectImportType.None
					};
					ScenarioRuleClient.MessageHandler(message3);
				}
				break;
			}
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle)
				{
					if (!m_TilesSelected.Contains(item))
					{
						m_TilesSelected.Add(item);
					}
					m_State = DestroyObstacleState.AnimateCharacter;
					Perform();
					break;
				}
			}
			m_State = DestroyObstacleState.DestroyObstaclesDone;
			Perform();
			break;
		case DestroyObstacleState.PreDestroyObstacleProcessing:
			foreach (CTile item2 in m_TilesSelected)
			{
				CTile propTile2 = item2;
				CObjectObstacle cObjectObstacle2 = item2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle2 == null)
				{
					cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile2);
				}
				if (cObjectObstacle2 != null)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(cObjectObstacle2);
				}
			}
			PhaseManager.StepComplete();
			break;
		case DestroyObstacleState.AnimateCharacter:
			AbilityHasHappened = true;
			if (m_TilesSelected.Count != 0)
			{
				List<CObjectProp> list2 = new List<CObjectProp>();
				bool faceObstacle2 = !IsMergedAbility;
				foreach (CTile item3 in m_TilesSelected)
				{
					CTile propTile3 = item3;
					CObjectObstacle cObjectObstacle3 = item3.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					if (cObjectObstacle3 == null)
					{
						cObjectObstacle3 = CObjectProp.FindPropWithPathingBlocker(item3.m_ArrayIndex, ref propTile3);
					}
					if (cObjectObstacle3 != null)
					{
						if (ParentAbility is CAbilityMerged cAbilityMerged2 && cAbilityMerged2.GetMergedWithAbility(this).AbilityHasBeenCancelled)
						{
							faceObstacle2 = true;
						}
						list2.Add(cObjectObstacle3);
					}
					if (!string.IsNullOrEmpty(AbilityDestroyObstacleData.TargetPrefabName))
					{
						CObjectProp cObjectProp2 = propTile3.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == AbilityDestroyObstacleData.TargetPrefabName);
						if (cObjectProp2 != null)
						{
							list2.Add(cObjectProp2);
						}
					}
				}
				CDestroyingObstacle_MessageData message2 = new CDestroyingObstacle_MessageData(AnimOverload, TargetingActor, faceObstacle2)
				{
					m_ActorDestroyingObstacle = TargetingActor,
					m_Tiles = m_TilesSelected.ToList(),
					m_DestroyedProps = list2.ToList(),
					m_DestroyObstacleAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
				if (StackInlineSubAbilities(null, m_TilesSelected[0]))
				{
					m_State = DestroyObstacleState.DestroyObstaclesDone;
				}
			}
			else if (!IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			break;
		case DestroyObstacleState.DestroyObstacles:
		{
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			bool faceObstacle = !IsMergedAbility;
			List<CObjectProp> list = new List<CObjectProp>();
			foreach (CTile item4 in m_TilesSelected)
			{
				AbilityHasHappened = true;
				CTile propTile = item4;
				bool flag = false;
				CObjectObstacle cObjectObstacle = item4.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item4.m_ArrayIndex, ref propTile);
					flag = true;
				}
				if (cObjectObstacle != null)
				{
					if (cObjectObstacle.PropHealthDetails != null && cObjectObstacle.PropHealthDetails.HasHealth && AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
					{
						GameState.KillActor(TargetingActor, cObjectObstacle.RuntimeAttachedActor, CActor.ECauseOfDeath.KillAbility, out var _, this);
					}
					if (ParentAbility is CAbilityMerged cAbilityMerged && cAbilityMerged.GetMergedWithAbility(this).AbilityHasBeenCancelled)
					{
						faceObstacle = true;
					}
					if (!flag)
					{
						m_destroyedObstaclesDictionary.TryGetValue(cObjectObstacle.PrefabName, out var value);
						m_destroyedObstaclesDictionary[cObjectObstacle.PrefabName] = value + 1;
						m_NumberOfTilesBlockedDestroyed += cObjectObstacle.PathingBlockers.Count;
					}
					cObjectObstacle.DestroyProp(0f, sendMessageToClient: false);
					list.Add(cObjectObstacle);
				}
				else if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
				{
					foreach (CActor item5 in GameState.GetActorsOnTile(item4, FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), ActorsToIgnore, isTargetedAbility: false, false))
					{
						GameState.KillActor(TargetingActor, item5, CActor.ECauseOfDeath.KillAbility, out var _, this);
					}
				}
				else if (!string.IsNullOrEmpty(AbilityDestroyObstacleData.TargetPrefabName))
				{
					CObjectProp cObjectProp = propTile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == AbilityDestroyObstacleData.TargetPrefabName);
					if (cObjectProp != null)
					{
						cObjectProp.DestroyProp(0f, sendMessageToClient: false, forceDestroy: false, this);
						list.Add(cObjectProp);
					}
				}
			}
			CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(AnimOverload, TargetingActor, faceObstacle)
			{
				m_ActorDestroyingObstacle = TargetingActor,
				m_Tiles = m_TilesSelected.ToList(),
				m_DestroyedProps = list.ToList(),
				m_DestroyDelay = SpawnDelay,
				m_DestroyObstacleAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.StepComplete();
			return true;
		}
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
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
		bool flag = false;
		if (!AllTargets && !TilesSelected.Contains(selectedTile) && m_State == DestroyObstacleState.SelectObstaclesPosition && m_NumberTargetsRemaining > 0 && m_ValidTilesInAreaEffect.Contains(selectedTile))
		{
			CObjectObstacle cObjectObstacle = selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				CTile propTile = null;
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedTile.m_ArrayIndex, ref propTile);
			}
			bool flag2 = false;
			if (cObjectObstacle != null && !cObjectObstacle.Activated)
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (cTile != null && !m_TilesSelected.Contains(cTile))
					{
						m_TilesSelected.Add(cTile);
						flag2 = true;
					}
				}
			}
			if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && GameState.GetActorsOnTile(selectedTile, FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), ActorsToIgnore, isTargetedAbility: false, false).Count > 0 && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag2 = true;
			}
			if (AbilityDestroyObstacleData.CanTargetEmptyHexes && CAbilityFilter.IsValidTile(selectedTile, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext) && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag2 = true;
			}
			if (flag2)
			{
				m_NumberTargetsRemaining--;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!AllTargets)
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				CObjectObstacle cObjectObstacle = selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					CTile propTile = null;
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
						if (cTile != null)
						{
							m_TilesSelected.Remove(cTile);
						}
					}
					m_NumberTargetsRemaining++;
				}
				else
				{
					m_TilesSelected.Remove(selectedTile);
					m_NumberTargetsRemaining++;
				}
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
			}
			if (flag)
			{
				Perform();
			}
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == DestroyObstacleState.SelectObstaclesPosition)
		{
			Restart();
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == DestroyObstacleState.SelectObstaclesPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == DestroyObstacleState.SelectObstaclesPosition;
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
		return m_State == DestroyObstacleState.DestroyObstaclesDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = DestroyObstacleState.SelectObstaclesPosition;
		m_TilesSelected.Clear();
		if (AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
			{
				if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if ((AbilityDestroyObstacleData.BypassOverrideDisallowDestroyAndMove || !prop.OverrideDisallowDestroyAndMove) && (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP || prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth))
			{
				continue;
			}
			foreach (CTile item in from x in prop.GetTiles()
				select ScenarioManager.GetTile(x))
			{
				m_ValidTilesInAreaEffect.Remove(item);
				TilesInRange.Remove(item);
			}
		}
		if (!string.IsNullOrEmpty(AbilityDestroyObstacleData.TargetPrefabName))
		{
			m_ValidTilesInAreaEffect.RemoveAll((CTile t) => t.m_Props.Find((CObjectProp p) => p.PrefabName == AbilityDestroyObstacleData.TargetPrefabName) == null);
			TilesInRange = m_ValidTilesInAreaEffect;
		}
		ExpandTileListWithLinkedProps(m_ValidTilesInAreaEffect);
		if (NumberTargets == OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
			foreach (CTile item2 in m_ValidTilesInAreaEffect)
			{
				CTile propTile = null;
				CObjectObstacle cObjectObstacle = item2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle == null || cObjectObstacle.Activated)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					CTile cTile2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (cTile2 != null && !m_TilesSelected.Contains(cTile2))
					{
						m_TilesSelected.Add(cTile2);
					}
				}
			}
		}
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_NumberOfTilesBlockedDestroyed = 0;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		GameState.UpdateObstaclesDestroyedThisTurn(m_NumberOfTilesBlockedDestroyed);
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyObstacle(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_destroyedObstaclesDictionary));
	}

	public override string GetDescription()
	{
		return "DestroyObstacle";
	}

	public bool HasPassedState(DestroyObstacleState destroyState)
	{
		return m_State > destroyState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool ShouldCheckLinkedTiles()
	{
		return true;
	}
}
