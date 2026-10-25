using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class CAIFocusOverrideDetails : ISerializable
{
	public enum EOverrideType
	{
		None,
		OverrideFocus,
		OverrideFocusIfCanAttack
	}

	public enum EOverrideTargetType
	{
		None,
		Actor,
		Prop
	}

	public static readonly EOverrideType[] OverrideTypes = (EOverrideType[])Enum.GetValues(typeof(EOverrideType));

	public static readonly EOverrideTargetType[] OverrideTargetTypes = (EOverrideTargetType[])Enum.GetValues(typeof(EOverrideTargetType));

	[JsonProperty]
	public EOverrideType OverrideType;

	[JsonProperty]
	public EOverrideTargetType OverrideTargetType;

	[JsonProperty]
	public string TargetGUID;

	[JsonProperty]
	public string TargetClassID;

	[JsonProperty]
	public EPropType TargetPropType;

	[JsonProperty]
	public bool IsDisabled;

	[JsonProperty]
	public TileIndex ForcedUseTileIndex;

	[JsonProperty]
	public bool DisallowDoorways;

	[JsonProperty]
	public bool FocusBenign;

	[JsonProperty]
	public bool UseFurthestFocus;

	[JsonProperty]
	public bool IsTemporary;

	[JsonProperty]
	public bool IsSummonsReturn;

	[JsonProperty]
	public bool IgnoreActivatedProps;

	[JsonProperty]
	public int MoveRange = -1;

	[JsonProperty]
	public bool UseSummonerToMeasureFrom;

	[JsonProperty]
	public bool IgnoreHazards;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("OverrideType", OverrideType);
		info.AddValue("OverrideTargetType", OverrideTargetType);
		info.AddValue("TargetGUID", TargetGUID);
		info.AddValue("TargetClassID", TargetClassID);
		info.AddValue("TargetPropType", TargetPropType);
		info.AddValue("IsDisabled", IsDisabled);
		info.AddValue("ForcedUseTileIndex", ForcedUseTileIndex);
		info.AddValue("DisallowDoorways", DisallowDoorways);
		info.AddValue("FocusBenign", FocusBenign);
		info.AddValue("UseFurthestFocus", UseFurthestFocus);
		info.AddValue("IsTemporary", IsTemporary);
		info.AddValue("IsSummonsReturn", IsSummonsReturn);
		info.AddValue("IgnoreActivatedProps", IgnoreActivatedProps);
		info.AddValue("MoveRange", MoveRange);
		info.AddValue("UseSummonerToMeasureFrom", UseSummonerToMeasureFrom);
		info.AddValue("IgnoreHazards", IgnoreHazards);
	}

	public CAIFocusOverrideDetails(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "IgnoreHazards":
					IgnoreHazards = info.GetBoolean("IgnoreHazards");
					break;
				case "OverrideType":
					OverrideType = (EOverrideType)info.GetValue("OverrideType", typeof(EOverrideType));
					break;
				case "OverrideTargetType":
					OverrideTargetType = (EOverrideTargetType)info.GetValue("OverrideTargetType", typeof(EOverrideTargetType));
					break;
				case "TargetGUID":
					TargetGUID = info.GetString("TargetGUID");
					break;
				case "TargetClassID":
					TargetClassID = info.GetString("TargetClassID");
					break;
				case "TargetPropType":
					TargetPropType = (EPropType)info.GetValue("TargetPropType", typeof(EPropType));
					break;
				case "IsDisabled":
					IsDisabled = info.GetBoolean("IsDisabled");
					break;
				case "ForcedUseTileIndex":
					ForcedUseTileIndex = (TileIndex)info.GetValue("ForcedUseTileIndex", typeof(TileIndex));
					break;
				case "DisallowDoorways":
					DisallowDoorways = info.GetBoolean("DisallowDoorways");
					break;
				case "FocusBenign":
					FocusBenign = info.GetBoolean("FocusBenign");
					break;
				case "UseFurthestFocus":
					UseFurthestFocus = info.GetBoolean("UseFurthestFocus");
					break;
				case "UseSummonerToMeasureFrom":
					UseSummonerToMeasureFrom = info.GetBoolean("UseSummonerToMeasureFrom");
					break;
				case "IsTemporary":
					IsTemporary = info.GetBoolean("IsTemporary");
					break;
				case "IsSummonsReturn":
					IsSummonsReturn = info.GetBoolean("IsSummonsReturn");
					break;
				case "IgnoreActivatedProps":
					IgnoreActivatedProps = info.GetBoolean("IgnoreActivatedProps");
					break;
				case "MoveRange":
					MoveRange = info.GetInt32("MoveRange");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CAIFocusOverrideDetails entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAIFocusOverrideDetails()
	{
	}

	public static void ClearAIFocusOverride(CActor actor)
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		if (actorState == null)
		{
			Log.GameplayError($"Trying to focus on {actor} that doesn't have a state.", actor);
		}
		else
		{
			actorState.SetAIFocusOverride(null);
		}
	}

	public static CAIFocusOverrideDetails SetAIFocusOverrideOnTileOrNearestEmpty(CActor actor, CTile focusTile)
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		if (actorState == null)
		{
			Log.GameplayError($"Trying to focus on {actor} that doesn't have a state.", actor);
			return null;
		}
		CAIFocusOverrideDetails cAIFocusOverrideDetails = actorState.AIFocusOverride;
		if (cAIFocusOverrideDetails == null || cAIFocusOverrideDetails.OverrideType != EOverrideType.OverrideFocus || cAIFocusOverrideDetails.OverrideTargetType != EOverrideTargetType.Prop)
		{
			cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideType = EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.OverrideTargetType = EOverrideTargetType.Prop;
			cAIFocusOverrideDetails.IsDisabled = false;
			cAIFocusOverrideDetails.MoveRange = 0;
			actorState.SetAIFocusOverride(cAIFocusOverrideDetails);
		}
		if (focusTile != null && focusTile.m_HexMap.Revealed && (focusTile.IsBlocked || (focusTile.LivingActor != null && focusTile.LivingActor != actor)) && actor.Tile != focusTile)
		{
			List<CTile> nearestEmptyTiles = GameState.GetNearestEmptyTiles(focusTile);
			float num = float.MaxValue;
			CTile cTile = null;
			foreach (CTile item in nearestEmptyTiles)
			{
				float num2 = Point.HexDist(item.m_ArrayIndex, actor.ArrayIndex);
				if (num2 < num)
				{
					num = num2;
					cTile = item;
				}
			}
			focusTile = cTile;
		}
		cAIFocusOverrideDetails.ForcedUseTileIndex = ((focusTile != null) ? new TileIndex(focusTile.m_ArrayIndex) : null);
		cAIFocusOverrideDetails.TargetGUID = null;
		return cAIFocusOverrideDetails;
	}

	public CObjectProp GetFarthestProp(CActor actor, List<CObjectProp> props)
	{
		float num = 0f;
		CObjectProp result = null;
		new List<CActorStatic.CTargetPath>();
		foreach (CObjectProp prop in props)
		{
			Point point = new Point(prop.ArrayIndex);
			ScenarioManager.PathFinder.FindPath(actor, actor.ArrayIndex, point, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: false);
			if ((GameState.GetTilesInRange(point, 1, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true).Count > 0) & foundPath)
			{
				float num2 = CActorStatic.CalculateCrowFlyDistance(actor.ArrayIndex, point);
				if (num2 > num)
				{
					num = num2;
					result = prop;
				}
			}
		}
		return result;
	}

	public ActorState GetFarthestActor(CActor actor, List<ActorState> actors)
	{
		float num = 0f;
		ActorState result = null;
		new List<CActorStatic.CTargetPath>();
		foreach (ActorState actor2 in actors)
		{
			if (actor2.Actor == null || actor2.IsDead || actor2.Actor.Type != CActor.EType.Player)
			{
				continue;
			}
			Point point = new Point(actor2.Location);
			ScenarioManager.PathFinder.FindPath(actor, actor.ArrayIndex, point, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: false);
			if ((GameState.GetTilesInRange(point, 1, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true).Count > 0) & foundPath)
			{
				float num2 = CActorStatic.CalculateCrowFlyDistance(actor.ArrayIndex, point);
				if (num2 > num)
				{
					num = num2;
					result = actor2;
				}
			}
		}
		return result;
	}

	public EOverrideType ResolveOverrideTargetForCurrentScenario(CActor overridenActor, out CActor overrideActor, out Point overridePoint, out bool needToOpenDoor, out bool disallowDoorways, out bool focusBenign, out bool useFurthestFocus, out bool? targetATile, out int overrideMoveRange, out bool useSummonerToMeasureFrom, out bool ignoreHazards)
	{
		bool flag = !IsSummonsReturn;
		overrideActor = null;
		overridePoint = default;
		needToOpenDoor = false;
		disallowDoorways = DisallowDoorways;
		focusBenign = FocusBenign;
		useFurthestFocus = UseFurthestFocus;
		targetATile = null;
		overrideMoveRange = MoveRange;
		useSummonerToMeasureFrom = UseSummonerToMeasureFrom;
		ignoreHazards = IgnoreHazards;
		if (OverrideType == EOverrideType.None || OverrideTargetType == EOverrideTargetType.None || IsDisabled)
		{
			return EOverrideType.None;
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			if (ForcedUseTileIndex != null)
			{
				overridePoint = new Point(ForcedUseTileIndex);
			}
			else
			{
				switch (OverrideTargetType)
				{
				case EOverrideTargetType.Actor:
				{
					ActorState actorState = null;
					if (!string.IsNullOrEmpty(TargetGUID) || !string.IsNullOrEmpty(TargetClassID))
					{
						actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == TargetGUID);
						if (actorState == null && !string.IsNullOrEmpty(TargetClassID))
						{
							actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ClassID == TargetClassID);
						}
					}
					else if (UseFurthestFocus && !CActorStatic.UseNewAIMove)
					{
						actorState = GetFarthestActor(overridenActor, ScenarioManager.CurrentScenarioState.ActorStates);
					}
					if (actorState == null)
					{
						overrideActor = null;
						overridePoint = default;
						needToOpenDoor = false;
						return EOverrideType.None;
					}
					overrideActor = ScenarioManager.FindActor(actorState.ActorGuid);
					if (overrideActor != null)
					{
						overridePoint = overrideActor.ArrayIndex;
					}
					else
					{
						overridePoint = new Point(actorState.Location);
					}
					break;
				}
				case EOverrideTargetType.Prop:
				{
					CObjectProp cObjectProp = null;
					if (!string.IsNullOrEmpty(TargetGUID))
					{
						cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == TargetGUID);
						if (cObjectProp == null && !IgnoreActivatedProps)
						{
							cObjectProp = ScenarioManager.CurrentScenarioState.ActivatedProps.FirstOrDefault((CObjectProp p) => p.PropGuid == TargetGUID);
						}
					}
					else if (TargetPropType != EPropType.None)
					{
						List<CObjectProp> list = new List<CObjectProp>();
						list.AddRange(ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p.PropType == TargetPropType).ToList());
						if (!IgnoreActivatedProps)
						{
							list.AddRange(ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.PropType == TargetPropType).ToList());
						}
						if (UseFurthestFocus)
						{
							cObjectProp = GetFarthestProp(overridenActor, list);
						}
						else
						{
							cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropType == TargetPropType);
							if (cObjectProp == null && !IgnoreActivatedProps)
							{
								cObjectProp = ScenarioManager.CurrentScenarioState.ActivatedProps.FirstOrDefault((CObjectProp p) => p.PropType == TargetPropType);
							}
						}
						if (cObjectProp != null)
						{
							overridePoint = new Point(cObjectProp.ArrayIndex);
							targetATile = false;
						}
						return OverrideType;
					}
					if (cObjectProp == null)
					{
						overrideActor = null;
						overridePoint = default;
						needToOpenDoor = false;
						return EOverrideType.None;
					}
					if (cObjectProp.PropActorHasBeenAssigned)
					{
						overrideActor = ScenarioManager.FindActor(cObjectProp.RuntimeAttachedActor.ActorGuid);
						if (overrideActor.Health > 0)
						{
							flag = false;
						}
					}
					if (overrideActor != null)
					{
						overridePoint = overrideActor.ArrayIndex;
					}
					else
					{
						overridePoint = new Point(cObjectProp.ArrayIndex);
					}
					break;
				}
				}
			}
		}
		if (overrideActor != null && overrideActor.IsDead)
		{
			overrideActor = null;
			overridePoint = default;
			needToOpenDoor = false;
			return EOverrideType.None;
		}
		if (flag)
		{
			CTile cTile = ScenarioManager.Tiles[overridenActor.ArrayIndex.X, overridenActor.ArrayIndex.Y];
			CTile cTile2 = ScenarioManager.Tiles[overridePoint.X, overridePoint.Y];
			if (!cTile.IsMapShared(cTile2) || cTile2.FindProp(ScenarioManager.ObjectImportType.Door) != null)
			{
				bool foundPath = false;
				List<Point> source = ScenarioManager.PathFinder.FindPath(overridenActor, overridenActor.ArrayIndex, overridePoint, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
				if (foundPath)
				{
					needToOpenDoor = false;
					for (int num = 0; num < ScenarioManager.CurrentScenarioState.DoorProps.Count; num++)
					{
						CObjectProp cObjectProp2 = ScenarioManager.CurrentScenarioState.DoorProps[num];
						CObjectDoor doorProp = cObjectProp2 as CObjectDoor;
						if (doorProp != null && !doorProp.IsDungeonEntrance && !doorProp.IsDungeonExit && !doorProp.DoorIsOpen && source.Any((Point p) => p.X == doorProp.ArrayIndex.X && p.Y == doorProp.ArrayIndex.Y))
						{
							needToOpenDoor = true;
							break;
						}
					}
				}
			}
		}
		if (overrideActor == null && !(overridePoint != default(Point)))
		{
			return EOverrideType.None;
		}
		return OverrideType;
	}
}
