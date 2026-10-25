using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using FH.Tools;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectDoor : CObjectProp, ISerializable
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EDoorType
	{
		ThinDoor,
		ThickDoor,
		HexOpeningDoor,
		ThinNarrowDoor,
		ThickNarrowDoor,
		EmptyDoor,
		EmptyDoorThin
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ELockType
	{
		None,
		Pressure,
		Gold,
		Kill,
		Other
	}

	public enum EDoorPressurePlateLockType
	{
		AllActivatedToOpen,
		AnyActivatedToOpen,
		XActivatedToOpen
	}

	public static readonly EDoorType[] DoorTypes = (EDoorType[])Enum.GetValues(typeof(EDoorType));

	public static readonly ELockType[] LockTypes = (ELockType[])Enum.GetValues(typeof(ELockType));

	public static readonly EDoorPressurePlateLockType[] DoorPressurePlateLockTypes = (EDoorPressurePlateLockType[])Enum.GetValues(typeof(EDoorPressurePlateLockType));

	[StateDump(Skip = true)]
	private bool m_ForceActivate;

	[StateDump(Skip = true)]
	private bool _openVisualOnly;

	[NonSerialized]
	public CObjectDoor[] TwinDoors = Array.Empty<CObjectDoor>();

	public EDoorType DoorType { get; private set; }

	public ELockType LockType { get; private set; }

	public EDoorPressurePlateLockType DoorPressurePlateLockType { get; private set; }

	public int DoorPressurePlateLockCount { get; private set; }

	public bool DoorHasExtraLock { get; private set; }

	public bool DoorExtraLockIsLocked { get; private set; }

	public bool IsDungeonEntrance { get; private set; }

	public bool IsDungeonExit { get; private set; }

	public List<CObjectPressurePlate> LinkedPressurePlates { get; private set; }

	public bool DoorOpenedByMovingActor { get; private set; }

	[StateDump(Skip = true)]
	public List<CMap> RoomsRevealedInLastOpening { get; private set; }

	public override bool Activated
	{
		get
		{
			if (DoorType == EDoorType.EmptyDoor || DoorType == EDoorType.EmptyDoorThin)
			{
				return true;
			}
			return base.Activated;
		}
		protected set
		{
			base.Activated = value;
		}
	}

	public float Angle => Rotation.Y;

	public bool DoorIsClosed => !DoorIsOpen;

	public bool DoorIsLocked
	{
		get
		{
			bool flag = false;
			if (DoorHasPressurePlateLock)
			{
				switch (DoorPressurePlateLockType)
				{
				case EDoorPressurePlateLockType.AllActivatedToOpen:
					flag = LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden).Any((CObjectPressurePlate p) => !p.Activated);
					break;
				case EDoorPressurePlateLockType.AnyActivatedToOpen:
					flag = !LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden).Any((CObjectPressurePlate p) => p.Activated);
					break;
				case EDoorPressurePlateLockType.XActivatedToOpen:
					flag = LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden && p.Activated).Count() < DoorPressurePlateLockCount;
					break;
				}
			}
			if (!flag && (!DoorHasExtraLock || !DoorExtraLockIsLocked) && !HexMap.Destroyed)
			{
				if (Hex2Map != null)
				{
					return Hex2Map.Destroyed;
				}
				return false;
			}
			return true;
		}
	}

	public bool DoorHasPressurePlateLock
	{
		get
		{
			if (LinkedPressurePlates != null)
			{
				return LinkedPressurePlates.Count > 0;
			}
			return false;
		}
	}

	public bool DoorIsOpen => ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridgeOpen;

	public CMap HexMap => ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y].m_HexMap;

	public CMap Hex2Map => ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y].m_Hex2Map;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DoorType", DoorType);
		info.AddValue("LockType", LockType);
		info.AddValue("DoorPressurePlateLockType", DoorPressurePlateLockType);
		info.AddValue("DoorPressurePlateLockCount", DoorPressurePlateLockCount);
		info.AddValue("DoorHasExtraLock", DoorHasExtraLock);
		info.AddValue("DoorExtraLockIsLocked", DoorExtraLockIsLocked);
		info.AddValue("IsDungeonEntrance", IsDungeonEntrance);
		info.AddValue("IsDungeonExit", IsDungeonExit);
		info.AddValue("LinkedPressurePlates", LinkedPressurePlates);
		info.AddValue("DoorOpenedByMovingActor", DoorOpenedByMovingActor);
	}

	public CObjectDoor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "DoorType":
					DoorType = (EDoorType)info.GetValue("DoorType", typeof(EDoorType));
					break;
				case "LockType":
					LockType = (ELockType)info.GetValue("LockType", typeof(ELockType));
					break;
				case "DoorPressurePlateLockType":
					DoorPressurePlateLockType = (EDoorPressurePlateLockType)info.GetValue("DoorPressurePlateLockType", typeof(EDoorPressurePlateLockType));
					break;
				case "DoorPressurePlateLockCount":
					DoorPressurePlateLockCount = info.GetInt32("DoorPressurePlateLockCount");
					break;
				case "DoorHasExtraLock":
					DoorHasExtraLock = info.GetBoolean("DoorHasExtraLock");
					break;
				case "DoorExtraLockIsLocked":
					DoorExtraLockIsLocked = info.GetBoolean("DoorExtraLockIsLocked");
					break;
				case "IsDungeonEntrance":
					IsDungeonEntrance = info.GetBoolean("IsDungeonEntrance");
					break;
				case "IsDungeonExit":
					IsDungeonExit = info.GetBoolean("IsDungeonExit");
					break;
				case "LinkedPressurePlates":
					LinkedPressurePlates = (List<CObjectPressurePlate>)info.GetValue("LinkedPressurePlates", typeof(List<CObjectPressurePlate>));
					break;
				case "DoorOpenedByMovingActor":
					DoorOpenedByMovingActor = info.GetBoolean("DoorOpenedByMovingActor");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectDoor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal new void OnDeserialized(StreamingContext context)
	{
		if (LinkedPressurePlates == null)
		{
			LinkedPressurePlates = new List<CObjectPressurePlate>();
		}
	}

	public CObjectDoor(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, string mapGuid, EDoorType doorType, bool isDungeonEntrance, bool isDungeonExit)
		: base(name, type, arrayIndex, position, rotation, null, mapGuid)
	{
		DoorType = doorType;
		LockType = ELockType.None;
		DoorPressurePlateLockType = EDoorPressurePlateLockType.AllActivatedToOpen;
		IsDungeonEntrance = isDungeonEntrance;
		IsDungeonExit = isDungeonExit;
		LinkedPressurePlates = new List<CObjectPressurePlate>();
	}

	public void InitDoor()
	{
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].Walkable = true;
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridge = true;
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridgeOpen = false;
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].HasHealth = PropHealthDetails != null && PropHealthDetails.HasHealth;
	}

	public void LinkPressurePlate(CObjectPressurePlate pressurePlate, bool updateApparanceLockType = true)
	{
		LinkedPressurePlates.Add(pressurePlate);
		if (updateApparanceLockType)
		{
			SetDoorApparanceLockType(ELockType.Pressure);
		}
	}

	public void UnlinkPressurePlate(CObjectPressurePlate pressurePlate, bool updateApparanceLockType = true)
	{
		LinkedPressurePlates.Remove(pressurePlate);
		if (updateApparanceLockType)
		{
			SetDoorApparanceLockType(DoorHasPressurePlateLock ? ELockType.Pressure : ELockType.None);
		}
	}

	public void SetDoorApparanceLockType(ELockType lockTypeToSet)
	{
		LockType = lockTypeToSet;
	}

	public void SetDoorPressurePlateLockType(EDoorPressurePlateLockType lockTypeToSet)
	{
		DoorPressurePlateLockType = lockTypeToSet;
	}

	public void SetDoorPressurePlateLockCount(int count)
	{
		DoorPressurePlateLockCount = count;
	}

	public void SetHasExtraLock(bool hasExtraLock)
	{
		DoorHasExtraLock = hasExtraLock;
		SetExtraLockState(hasExtraLock, openDoorIfUnlocked: false);
	}

	public void SetExtraLockState(bool lockedStateToSet, bool openDoorIfUnlocked = true, CActor actorActivating = null)
	{
		DoorExtraLockIsLocked = lockedStateToSet;
		if (openDoorIfUnlocked && !DoorIsLocked)
		{
			ForceActivate(actorActivating);
		}
		if (!DoorExtraLockIsLocked)
		{
			CUnlockLockedDoor_MessageData message = new CUnlockLockedDoor_MessageData(actorActivating)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		else
		{
			CLockUnlockedDoor_MessageData message2 = new CLockUnlockedDoor_MessageData(actorActivating)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
	}

	public void SetDoorType(EDoorType typeToSet)
	{
		DoorType = typeToSet;
	}

	public bool ForceActivate(CActor actor)
	{
		m_ForceActivate = true;
		return Activate(actor);
	}

	public void SetActivatedFromLevelEditor(bool activate)
	{
		Activated = activate;
	}

	public bool SetDoorOpenedByMovingActor(CActor actor)
	{
		DoorOpenedByMovingActor = true;
		return Activate(actor);
	}

	public bool IsConnectedToMap(CMap checkMap)
	{
		if (HexMap != checkMap)
		{
			if (Hex2Map != null)
			{
				return Hex2Map == checkMap;
			}
			return false;
		}
		return true;
	}

	public bool IsConnectedToDestroyedMap()
	{
		if (!HexMap.Destroyed)
		{
			if (Hex2Map != null)
			{
				return Hex2Map.Destroyed;
			}
			return false;
		}
		return true;
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (_openVisualOnly)
		{
			base.Activate(actor);
			ScenarioScript.current?.OnDoorOpened(this, actor, creditActor, _openVisualOnly);
			ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridgeOpen = true;
			return false;
		}
		if (!Activated && (m_ForceActivate || actor != null))
		{
			bool initial = false;
			if (ScenarioManager.CurrentScenarioState.RoundNumber == 0)
			{
				initial = true;
			}
			if (RoomsRevealedInLastOpening == null)
			{
				RoomsRevealedInLastOpening = new List<CMap>();
			}
			else
			{
				RoomsRevealedInLastOpening.Clear();
			}
			SetExtraLockState(lockedStateToSet: false, openDoorIfUnlocked: false, actor);
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			if (actor != null)
			{
				CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(actor);
				cPauseLoco_MessageData.m_Pause = true;
				ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData);
			}
			string characterID = "";
			if (actor != null)
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					characterID = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor { Summoner: var summoner })
				{
					characterID = summoner?.CharacterClass.ID;
				}
			}
			if (m_ForceActivate)
			{
				m_ForceActivate = false;
			}
			ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridgeOpen = true;
			CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
			Log.SimpleInfo(actor?.ActorLocKey() ?? "Scenario opens door");
			if (!cTile.m_HexMap.Revealed)
			{
				RoomsRevealedInLastOpening.Add(cTile.m_HexMap);
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Activated, ObjectType, PropGuid, PrefabName, characterID, "Activate", m_OwnerGuid, "", RoomsRevealedInLastOpening, ArrayIndex.ToPoint()));
			base.Activate(actor);
			cTile.m_HexMap.Reveal(initial, noIDRegen: false, forLevelEditor: false, DoorOpenedByMovingActor);
			if (cTile.m_Hex2Map != null)
			{
				if (!cTile.m_Hex2Map.Revealed)
				{
					RoomsRevealedInLastOpening.Add(cTile.m_Hex2Map);
				}
				cTile.m_Hex2Map.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, DoorOpenedByMovingActor);
			}
			GameState.SortIntoInitiativeAndIDOrder();
			CObjectDoor[] twinDoors = TwinDoors;
			foreach (CObjectDoor obj in twinDoors)
			{
				obj._openVisualOnly = true;
				obj.Activate(actor, creditActor);
			}
			ScenarioScript.current?.OnDoorOpened(this, actor, creditActor, _openVisualOnly);
			CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
			if (cPhaseAction?.CurrentPhaseAbility?.TargetingActor is CPlayerActor cPlayerActor2)
			{
				CAbility abilityToOverride = cPhaseAction.CurrentPhaseAbility.m_Ability;
				if (abilityToOverride?.AbilityInterruptCopy != null)
				{
					abilityToOverride = cPhaseAction.CurrentPhaseAbilities?.LastOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability == abilityToOverride.AbilityInterruptCopy)?.m_Ability;
				}
				if (abilityToOverride != null)
				{
					foreach (PerksYMLData item in cPlayerActor2.CharacterClass.Perks.Where((PerksYMLData p) => (p.Flags & EPerkFlag.ApplyOverridesAfterRoomRevealed) != 0))
					{
						if (item.CompareAbility.AbilityType != abilityToOverride.AbilityType)
						{
							continue;
						}
						foreach (CAbilityOverride @override in item.Overrides)
						{
							abilityToOverride.OverrideAbilityValues(@override, perform: false);
						}
					}
				}
			}
		}
		return false;
	}

	public void UnlockLockedDoorWithoutOpening(CActor actorUnlocking = null)
	{
		if (DoorHasExtraLock && DoorExtraLockIsLocked)
		{
			DoorExtraLockIsLocked = false;
			CUnlockLockedDoor_MessageData message = new CUnlockLockedDoor_MessageData(actorUnlocking)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void CloseOpenedDoor(bool lockDoor = false)
	{
		if (!Activated)
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(new Point(ArrayIndex));
		if (cActor != null)
		{
			GameState.ActorBeenDamaged(cActor, ScenarioManager.TrapDamage, checkIfPlayerCanAvoidDamage: true, null, null, CAbility.EAbilityType.Trap, 0, isTrapDamage: true);
			GameState.ActorHealthCheck(cActor, cActor, isTrap: true);
		}
		CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
		CTile refTile = null;
		CAbilityTeleport.EnsureTileIsClearForTeleport(cTile, ref refTile, null, "Hit");
		List<CObjectProp> list = new List<CObjectProp>();
		for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = cTile.m_Props[num];
			if (!(cObjectProp is CObjectDoor))
			{
				list.Add(cObjectProp);
				cObjectProp.DestroyProp();
			}
		}
		if ((PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability is CAbilityMove { RemainingMoves: >0 } cAbilityMove)
		{
			cAbilityMove.ForceInterrupt();
		}
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsBridgeOpen = false;
		Log.SimpleInfo("Scenario closes door");
		CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = new CDeactivatePropAnim_MessageData(null);
		cDeactivatePropAnim_MessageData.m_Prop = this;
		ScenarioRuleClient.MessageHandler(cDeactivatePropAnim_MessageData);
		Activated = false;
		if (lockDoor)
		{
			if (!DoorHasExtraLock)
			{
				SetHasExtraLock(hasExtraLock: true);
			}
			else
			{
				SetExtraLockState(lockedStateToSet: true);
			}
		}
	}

	public static EDoorType[] SwappableTypes(EDoorType type)
	{
		switch (type)
		{
		case EDoorType.ThinDoor:
		case EDoorType.ThinNarrowDoor:
			return new EDoorType[3]
			{
				EDoorType.ThinDoor,
				EDoorType.ThinNarrowDoor,
				EDoorType.EmptyDoorThin
			};
		case EDoorType.ThickDoor:
		case EDoorType.ThickNarrowDoor:
			return new EDoorType[3]
			{
				EDoorType.ThickDoor,
				EDoorType.ThickNarrowDoor,
				EDoorType.EmptyDoor
			};
		case EDoorType.EmptyDoor:
			return new EDoorType[2]
			{
				EDoorType.EmptyDoor,
				EDoorType.EmptyDoorThin
			};
		case EDoorType.EmptyDoorThin:
			return new EDoorType[2]
			{
				EDoorType.EmptyDoor,
				EDoorType.EmptyDoorThin
			};
		default:
			return new EDoorType[0];
		}
	}

	public static List<Tuple<int, string>> Compare(CObjectDoor door1, CObjectDoor door2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(door1, door2, isMPCompare));
			if (door1.DoorType != door2.DoorType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1801, "CObjectDoor DoorType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorType",
						door1.DoorType.ToString(),
						door2.DoorType.ToString()
					}
				});
			}
			if (door1.LockType != door2.LockType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1802, "CObjectDoor LockType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"LockType",
						door1.LockType.ToString(),
						door2.LockType.ToString()
					}
				});
			}
			if (door1.DoorHasExtraLock != door2.DoorHasExtraLock)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1803, "CObjectDoor DoorHasExtraLock does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorHasExtraLock",
						door1.DoorHasExtraLock.ToString(),
						door2.DoorHasExtraLock.ToString()
					}
				});
			}
			if (door1.DoorExtraLockIsLocked != door2.DoorExtraLockIsLocked)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1804, "CObjectDoor DoorExtraLockIsLocked does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorExtraLockIsLocked",
						door1.DoorExtraLockIsLocked.ToString(),
						door2.DoorExtraLockIsLocked.ToString()
					}
				});
			}
			if (door1.IsDungeonEntrance != door2.IsDungeonEntrance)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1805, "CObjectDoor IsDungeonEntrance does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"IsDungeonEntrance",
						door1.IsDungeonEntrance.ToString(),
						door2.IsDungeonEntrance.ToString()
					}
				});
			}
			if (door1.IsDungeonExit != door2.IsDungeonExit)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1806, "CObjectDoor IsDungeonExit does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"IsDungeonExit",
						door1.IsDungeonExit.ToString(),
						door2.IsDungeonExit.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(door1.LinkedPressurePlates, door2.LinkedPressurePlates))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1807, "CObjectDoor LinkedPressurePlates Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"ChestTreasureTablesID",
						(door1.LinkedPressurePlates == null) ? "is null" : "is not null",
						(door2.LinkedPressurePlates == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (door1.LinkedPressurePlates.Count != door2.LinkedPressurePlates.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1808, "CObjectDoor LinkedPressurePlates Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
						new string[3]
						{
							"ObjectType",
							door1.ObjectType.ToString(),
							door2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
						new string[3]
						{
							"ChestTreasureTablesID Count",
							door1.LinkedPressurePlates.Count.ToString(),
							door2.LinkedPressurePlates.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CObjectPressurePlate pressurePlate in door1.LinkedPressurePlates)
				{
					if (!door2.LinkedPressurePlates.Exists((CObjectPressurePlate e) => e.PropGuid == pressurePlate.PropGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1809, "CObjectDoor LinkedPressurePlate in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
							new string[3]
							{
								"ObjectType",
								door1.ObjectType.ToString(),
								door2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
							new string[3] { "PressurePlate Prop GUID", pressurePlate.PropGuid, "Missing" },
							new string[3] { "PressurePlate PrefabName", pressurePlate.PrefabName, "Missing" },
							new string[3]
							{
								"PressurePlateType",
								pressurePlate.PressurePlateType.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (CObjectPressurePlate pressurePlate2 in door2.LinkedPressurePlates)
				{
					if (!door1.LinkedPressurePlates.Exists((CObjectPressurePlate e) => e.PropGuid == pressurePlate2.PropGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1810, "CObjectDoor LinkedPressurePlate in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
							new string[3]
							{
								"ObjectType",
								door1.ObjectType.ToString(),
								door2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
							new string[3] { "PressurePlate Prop GUID", "Missing", pressurePlate2.PropGuid },
							new string[3] { "PressurePlate PrefabName", "Missing", pressurePlate2.PrefabName },
							new string[3]
							{
								"PressurePlateType",
								"Missing",
								pressurePlate2.PressurePlateType.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (CObjectPressurePlate pressurePlate3 in door1.LinkedPressurePlates)
				{
					try
					{
						CObjectPressurePlate plate = door2.LinkedPressurePlates.Single((CObjectPressurePlate s) => s.PropGuid == pressurePlate3.PropGuid);
						list.AddRange(CObjectPressurePlate.Compare(pressurePlate3, plate, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(1811, "Exception during LinkedPressurePlate compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(1899, "Exception during CObjectDoor compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
