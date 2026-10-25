using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectObstacle : CObjectProp, ISerializable
{
	public List<TileIndex> PathingBlockers { get; private set; }

	public bool SetPositionToCenterOfAllPathBlockers { get; protected set; }

	public bool IgnoresFlyAndJump { get; protected set; }

	public bool BlockLOS { get; protected set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PathingBlockers", PathingBlockers);
		info.AddValue("SetPositionToCenterOfAllPathBlockers", SetPositionToCenterOfAllPathBlockers);
		info.AddValue("IgnoresFlyAndJump", IgnoresFlyAndJump);
		info.AddValue("BlockLOS", BlockLOS);
	}

	public CObjectObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PathingBlockers":
					PathingBlockers = (List<TileIndex>)info.GetValue("PathingBlockers", typeof(List<TileIndex>));
					break;
				case "SetPositionToCenterOfAllPathBlockers":
					SetPositionToCenterOfAllPathBlockers = info.GetBoolean("SetPositionToCenterOfAllPathBlockers");
					break;
				case "IgnoresFlyAndJump":
					IgnoresFlyAndJump = info.GetBoolean("IgnoresFlyAndJump");
					break;
				case "BlockLOS":
					BlockLOS = info.GetBoolean("BlockLOS");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectObstacle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectObstacle(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, List<TileIndex> pathingBlockers, CActor owner, string mapGuid, bool ignoresFlyAndJump, bool setPositionToCenterOfAllPathBlockers = false, bool blockLOS = false)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		PathingBlockers = pathingBlockers;
		SetPositionToCenterOfAllPathBlockers = setPositionToCenterOfAllPathBlockers;
		IgnoresFlyAndJump = ignoresFlyAndJump;
		BlockLOS = blockLOS;
		SetIsBlocker(isBlocker: true);
	}

	public CObjectObstacle(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectObstacle(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public void SetIsBlocker(bool isBlocker)
	{
		ScenarioManager.PathFinder.GetNode(ArrayIndex).Blocked = isBlocker;
		foreach (TileIndex pathingBlocker in PathingBlockers)
		{
			ScenarioManager.PathFinder.GetNode(pathingBlocker).Blocked = isBlocker;
		}
	}

	public override List<Point> GetTiles()
	{
		return PathingBlockers.Select((TileIndex x) => x.ToPoint()).ToList();
	}

	public override void AddTile(Point tile)
	{
		PathingBlockers.Add(tile.ToTileIndex());
	}

	public override void ClearTiles()
	{
		PathingBlockers.Clear();
	}

	public override void SetLocation(TileIndex arrayIndex, CVector3 position, CVector3 rotation)
	{
		List<Point> pathingBlockersInAxialLocalSpace = GetPathingBlockersInAxialLocalSpace();
		base.SetLocation(arrayIndex, position, rotation);
		PathingBlockers.Clear();
		foreach (Point item in pathingBlockersInAxialLocalSpace)
		{
			Point point = Point.AxialToOddR(Point.OddRToAxial(ArrayIndex.ToPoint()) + item);
			if (ScenarioManager.GetTile(point) != null)
			{
				PathingBlockers.Add(new TileIndex(point.X, point.Y));
			}
		}
	}

	private List<Point> GetPathingBlockersInAxialLocalSpace()
	{
		List<Point> list = new List<Point>(PathingBlockers.Count);
		foreach (TileIndex pathingBlocker in PathingBlockers)
		{
			Point point = Point.OddRToAxial(pathingBlocker.ToPoint());
			Point point2 = Point.OddRToAxial(ArrayIndex.ToPoint());
			Point item = new Point(point.X - point2.X, point.Y - point2.Y);
			list.Add(item);
		}
		return list;
	}

	public override void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true, bool forceDestroy = false, CAbility destroyedByAbility = null, bool checkLinkedProp = true)
	{
		SetIsBlocker(isBlocker: false);
		GameState.LastDestroyedObstacle = this;
		base.DestroyProp(spawnDelay, sendMessageToClient, forceDestroy, destroyedByAbility, checkLinkedProp);
		ReleasePFAfterDestroy = true;
	}

	public void SetIgnoreFlyAndJump(bool ignoresFlyAndJump)
	{
		IgnoresFlyAndJump = ignoresFlyAndJump;
	}

	public void SetBlockLOS(bool blockLOS)
	{
		BlockLOS = blockLOS;
	}

	public static List<Tuple<int, string>> Compare(CObjectObstacle obs1, CObjectObstacle obs2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(obs1, obs2, isMPCompare));
			if (obs1.SetPositionToCenterOfAllPathBlockers != obs2.SetPositionToCenterOfAllPathBlockers)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2001, "CObjectObstacle SetPositionToCenterOfAllPathBlockers does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"SetPositionToCenterOfAllPathBlockers",
						obs1.SetPositionToCenterOfAllPathBlockers.ToString(),
						obs2.SetPositionToCenterOfAllPathBlockers.ToString()
					}
				});
			}
			if (obs1.SetPositionToCenterOfAllPathBlockers != obs2.SetPositionToCenterOfAllPathBlockers)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2005, "CObjectObstacle IgnoresFlyAndJump does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"IgnoresFlyAndJump",
						obs1.IgnoresFlyAndJump.ToString(),
						obs2.IgnoresFlyAndJump.ToString()
					},
					new string[3]
					{
						"BlockLOS",
						obs1.BlockLOS.ToString(),
						obs2.BlockLOS.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(obs1.PathingBlockers, obs2.PathingBlockers))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2002, "CObjectObstacle PathingBlockers Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"PathingBlockers",
						(obs1.PathingBlockers == null) ? "is null" : "is not null",
						(obs2.PathingBlockers == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (obs1.PathingBlockers.Count != obs2.PathingBlockers.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2003, "CObjectObstacle PathingBlockers Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
						new string[3]
						{
							"ObjectType",
							obs1.ObjectType.ToString(),
							obs2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
						new string[3]
						{
							"PathingBlockers Count",
							obs1.PathingBlockers.Count.ToString(),
							obs2.PathingBlockers.Count.ToString()
						}
					});
					break;
				}
				foreach (TileIndex tileIndex in obs1.PathingBlockers)
				{
					if (!obs2.PathingBlockers.Any((TileIndex a) => TileIndex.Compare(a, tileIndex)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2004, "CObjectObstacle PathingBlocker in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
							new string[3]
							{
								"ObjectType",
								obs1.ObjectType.ToString(),
								obs2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
							new string[3]
							{
								"PathingBlocker",
								tileIndex.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (TileIndex tileIndex2 in obs2.PathingBlockers)
				{
					if (!obs1.PathingBlockers.Any((TileIndex a) => TileIndex.Compare(a, tileIndex2)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2004, "CObjectObstacle PathingBlocker in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
							new string[3]
							{
								"ObjectType",
								obs1.ObjectType.ToString(),
								obs2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
							new string[3]
							{
								"PathingBlocker",
								"Missing",
								tileIndex2.ToString()
							}
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2099, "Exception during CObjectObstacle compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}

	public IEnumerable<TileIndex> CalcPathingBlockers(Point targetPos, int angle)
	{
		Point pos = ArrayIndex.ToPoint();
		Point posAxial = Point.OddRToAxial(pos);
		Point targetPosAxial = Point.OddRToAxial(targetPos);
		foreach (Point item in PathingBlockers.Select((TileIndex x) => x.ToPoint()))
		{
			Point point = Point.OddRToAxial(item.Rotate(pos, -(int)Rotation.Y).Rotate(pos, angle)) - posAxial;
			Point point2 = Point.AxialToOddR(targetPosAxial + point);
			yield return new TileIndex(point2);
		}
	}

	public void ReleasePathfinding()
	{
		foreach (TileIndex pathingBlocker in PathingBlockers)
		{
			ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = false;
		}
	}
}
