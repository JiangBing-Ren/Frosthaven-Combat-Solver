using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class ProceduralWallAnalyser
{
	public class Wall
	{
		public WallInfo originalSource;

		public Vector3 originalCentre3;

		public float originalLength;

		public Vector3 unitOut3;

		public Vector3 unitUp3;

		public Vector3 unitAlong3;

		public float originalAngle;

		public Vector2 originalCentre;

		public Vector2 originalStart;

		public Vector2 originalEnd;

		public Vector2 unitOut;

		public Vector2 unitAlong;

		public Wall neighbourLeft;

		public Wall neighbourRight;

		public Corner cornerLeft;

		public Corner cornerRight;

		public Vector2 perimiterStart;

		public Vector2 perimiterEnd;

		public Vector2 outerStart;

		public Vector2 outerEnd;

		public float maxThickness;

		public bool isShared;

		public bool isOwnershipAssigned;

		public Wall sharingWall;

		public bool isThicknessAssigned;

		public float newAngle;

		public bool isFlush;

		public bool isWallOwner;

		public bool isLeftCornerOwner;

		public bool isRightCornerOwner;

		public int wallSeed;

		internal Room m_room;

		internal float m_perimiterOffset;

		public int debugIndex;

		internal Room room => GetRoom();

		internal float perimiterOffset
		{
			get
			{
				GetRoom();
				return m_perimiterOffset;
			}
		}

		internal Room GetRoom()
		{
			if (m_room == null)
			{
				m_room = new Room(this);
			}
			return m_room;
		}

		public Wall(WallInfo s)
		{
			originalSource = s;
			originalCentre3 = originalSource.localToWorld.MultiplyPoint(Vector3.zero);
			originalLength = originalSource.boxBounds.size.x;
			unitOut3 = originalSource.localToWorld.MultiplyVector(Vector3.forward);
			unitAlong3 = originalSource.localToWorld.MultiplyVector(Vector3.right);
			unitUp3 = originalSource.localToWorld.MultiplyVector(Vector3.up);
			originalAngle = originalSource.localToWorld.rotation.eulerAngles.y;
			originalCentre = new Vector2(originalCentre3.x, originalCentre3.z);
			unitOut = new Vector2(unitOut3.x, unitOut3.z);
			unitAlong = new Vector2(unitAlong3.x, unitAlong3.z);
			originalStart = originalCentre - unitAlong * originalLength / 2f;
			originalEnd = originalCentre + unitAlong * originalLength / 2f;
			isWallOwner = true;
			isLeftCornerOwner = false;
			isRightCornerOwner = false;
			wallSeed = 0;
			maxThickness = 0f;
			isShared = false;
			isFlush = false;
			sharingWall = null;
			isThicknessAssigned = false;
			newAngle = originalAngle;
		}

		public void DrawDiagnostics()
		{
			DrawLine(originalStart, originalEnd, Color.yellow);
		}

		public void DrawEnds()
		{
			bool flag = cornerLeft != null;
			bool flag2 = cornerRight != null;
			float num = (flag ? 0f : 1f);
			float num2 = (flag2 ? 0f : 1f);
			bool flag3 = Vector2.Distance(originalStart, originalEnd) < 1f;
			Color colour = (flag3 ? Color.red : Color.Lerp(Color.black, Color.green, flag ? 1f : 0.5f));
			Color colour2 = (flag3 ? Color.red : Color.Lerp(Color.black, Color.blue, flag2 ? 1f : 0.5f));
			DrawLine(originalStart, originalStart, colour, 0f - num, 0f - num - 1f);
			DrawLine(originalStart, originalEnd, flag3 ? Color.red : Color.cyan, 0f - num - 1f, num2 + 1f);
			DrawLine(originalEnd, originalEnd, colour2, num2, num2 + 1f);
		}
	}

	public class Room
	{
		public List<Wall> walls = new List<Wall>();

		public float perimeter;

		public Room(Wall first_wall)
		{
			Wall wall = first_wall;
			while (wall != null)
			{
				wall.m_room = this;
				wall.m_perimiterOffset = perimeter;
				walls.Add(wall);
				if (wall.isWallOwner)
				{
					perimeter += wall.originalLength;
				}
				if (wall.cornerRight != null)
				{
					wall = wall.cornerRight.wallRight;
					if (wall != null && wall == first_wall)
					{
						wall = null;
					}
				}
				else
				{
					wall = null;
				}
			}
		}
	}

	public class Corner
	{
		[Flags]
		public enum Flags
		{
			IsTJunction = 1,
			IsTOnRight = 2
		}

		public Vector2 originalPosition;

		public Vector2 outerPosition;

		public Wall wallLeft;

		public Wall wallRight;

		public Flags flags;

		public bool isClashing;

		public bool IsTJunction
		{
			get
			{
				return (flags & Flags.IsTJunction) != 0;
			}
			set
			{
				if (value)
				{
					flags |= Flags.IsTJunction;
				}
				else
				{
					flags &= ~Flags.IsTJunction;
				}
			}
		}

		public bool IsTOnRight
		{
			get
			{
				return (flags & Flags.IsTOnRight) != 0;
			}
			set
			{
				if (value)
				{
					flags |= Flags.IsTOnRight;
				}
				else
				{
					flags &= ~Flags.IsTOnRight;
				}
			}
		}

		public void DrawDiagnostics()
		{
			DrawLine(originalPosition, originalPosition, Color.yellow, 0f, 3f);
		}
	}

	public struct WallInfo
	{
		public GameObject wallObject;

		public BoxCollider boxBounds;

		public Transform boxParent;

		public Matrix4x4 localToWorld;

		public bool isParty;

		public bool isOwner;

		public bool isFlush;

		public float length;

		public float perimeterOffset;

		public float perimeterLength;

		public int debugIndex;
	}

	private struct Line
	{
		public int w;

		public Vector3 f;

		public Vector3 t;

		public Color c;
	}

	private int rootSeed;

	private List<Wall> Walls = new List<Wall>();

	private List<Corner> Corners = new List<Corner>();

	private static System.Random debugRng = new System.Random();

	private static bool JitterLines = false;

	private static List<Line> debugLines = new List<Line>();

	private static int debugCurrentWall = 0;

	private bool m_DebugShow;

	private const float toleranceCloseness = 0.2f;

	private const float toleranceCloseness2 = 0.040000003f;

	private const float toleranceParallelAngle = MathF.PI / 180f;

	private const float toleranceProportion = 0.01f;

	private const float toleranceZero = 0.01f;

	private const float toleranceAngleDegrees = 0.1f;

	private const float narrowWallThickness = 0.4f;

	private const float clashingWallThickness = 0.5f;

	private const float maxWallThickness = 7.5f;

	private const float Tau = MathF.PI * 2f;

	internal void Get2DShape(List<Vector2> shape2d)
	{
		shape2d.Clear();
		FindCorners();
		if (Corners.Count != 0)
		{
			Corner corner = Corners.First();
			Corner corner2 = corner;
			int num = 0;
			while (num++ < 100 && corner2.wallRight != null && corner2.wallRight.cornerRight != corner)
			{
				shape2d.Add(corner2.originalPosition);
				corner2 = corner2.wallRight.cornerRight;
			}
		}
	}

	public void SetSeed(int seed)
	{
		rootSeed = seed;
	}

	public void SetWalls(List<WallInfo> walls)
	{
		Walls.Clear();
		foreach (WallInfo wall in walls)
		{
			Walls.Add(new Wall(wall));
		}
		debugLines.Clear();
	}

	public void ApplyWalls(List<WallInfo> targets)
	{
		int count = Walls.Count;
		int count2 = targets.Count;
		int num = Math.Min(count, count2);
		for (int i = 0; i < num; i++)
		{
			Wall wall = Walls[i];
			WallInfo value = targets[i];
			float num2 = Math.Max(wall.maxThickness, 0.4f);
			float num3 = num2 * (wall.isShared ? 0f : 0.5f);
			Vector3 position = wall.originalCentre3 + wall.unitOut3 * num3;
			float newAngle = wall.newAngle;
			float originalLength = wall.originalLength;
			Quaternion rotation = Quaternion.Euler(0f, newAngle, 0f);
			value.boxBounds.transform.SetPositionAndRotation(position, rotation);
			value.boxBounds.size = new Vector3(originalLength, targets[i].boxBounds.size.y, num2);
			Transform transform = value.boxBounds.transform;
			value.localToWorld = transform.localToWorldMatrix;
			value.isParty = wall.isShared;
			value.isOwner = wall.isWallOwner;
			value.isFlush = wall.isFlush;
			value.debugIndex = wall.debugIndex;
			value.length = wall.originalLength;
			value.perimeterLength = wall.room.perimeter;
			value.perimeterOffset = wall.perimiterOffset;
			ProceduralWall component = value.wallObject.GetComponent<ProceduralWall>();
			if (component != null)
			{
				component.IsOwner = wall.isWallOwner;
				component.Length = value.length;
				component.perimeterOffset = value.perimeterOffset;
				component.perimiterLength = value.perimeterLength;
				if (wall.cornerLeft != null && wall.neighbourLeft != null)
				{
					component.LeftCorner = new ProceduralWall.CornerInfo
					{
						IsOwner = wall.isLeftCornerOwner,
						Position = new Vector3(wall.cornerLeft.originalPosition.x, 0f, wall.cornerLeft.originalPosition.y),
						InsideAngle = CalculateInternalAngle(wall, wall.neighbourLeft),
						AdjacentDepth = wall.neighbourLeft.maxThickness,
						Flags = (int)wall.cornerLeft.flags
					};
				}
				if (wall.cornerRight != null && wall.neighbourRight != null)
				{
					component.RightCorner = new ProceduralWall.CornerInfo
					{
						IsOwner = wall.isRightCornerOwner,
						Position = new Vector3(wall.cornerRight.originalPosition.x, 0f, wall.cornerRight.originalPosition.y),
						InsideAngle = CalculateInternalAngle(wall, wall.neighbourRight),
						AdjacentDepth = wall.neighbourRight.maxThickness,
						Flags = (int)wall.cornerRight.flags
					};
				}
			}
			targets[i] = value;
		}
	}

	public void ResetWalls(List<WallInfo> targets)
	{
		int count = Walls.Count;
		int count2 = targets.Count;
		int num = Math.Min(count, count2);
		for (int i = 0; i < num; i++)
		{
			Wall wall = Walls[i];
			WallInfo value = targets[i];
			Vector3 size = wall.originalSource.boxBounds.size;
			Vector3 originalCentre = wall.originalCentre3;
			float originalAngle = wall.originalAngle;
			Quaternion rotation = Quaternion.Euler(0f, originalAngle, 0f);
			value.boxBounds.transform.SetPositionAndRotation(originalCentre, rotation);
			value.boxBounds.size = size;
			value.isParty = false;
			value.isOwner = true;
			targets[i] = value;
		}
	}

	private static Vector3 debugJitter()
	{
		if (JitterLines)
		{
			return new Vector3((float)debugRng.NextDouble() * 0.01f * 2f - 0.01f, (float)debugRng.NextDouble() * 0.01f * 2f - 0.01f, (float)debugRng.NextDouble() * 0.01f * 2f - 0.01f);
		}
		return Vector3.zero;
	}

	private static void DrawLine(Vector2 from, Vector2 to, Color colour, float from_height = 0f, float to_height = 0f)
	{
		Gizmos.color = colour;
		Gizmos.DrawLine(new Vector3(from.x, from_height, from.y) + debugJitter(), new Vector3(to.x, to_height, to.y) + debugJitter());
	}

	private static void DrawLine(Vector3 from, Vector3 to, Color colour)
	{
		Gizmos.color = colour;
		Gizmos.DrawLine(from + debugJitter(), to + debugJitter());
	}

	private static void AddLine(Vector2 from, Vector2 to, Color colour, float from_height = 0f, float to_height = 0f)
	{
		debugLines.Add(new Line
		{
			w = debugCurrentWall,
			f = new Vector3(from.x, from_height, from.y),
			t = new Vector3(to.x, to_height, to.y),
			c = colour
		});
	}

	private static void DrawDebugPrimitives(int only_draw_this_wall)
	{
		foreach (Line debugLine in debugLines)
		{
			if (only_draw_this_wall < 0 || debugLine.w == only_draw_this_wall)
			{
				DrawLine(debugLine.f, debugLine.t, debugLine.c);
			}
		}
	}

	public void DrawDiagnostics(int only_draw_this_wall, bool jitter_lines)
	{
		JitterLines = jitter_lines;
		Gizmos.matrix = Matrix4x4.identity;
		for (int i = 0; i < Walls.Count; i++)
		{
			if (only_draw_this_wall < 0 || i == only_draw_this_wall)
			{
				Walls[i].DrawDiagnostics();
			}
		}
		for (int j = 0; j < Corners.Count; j++)
		{
			if (only_draw_this_wall < 0 || (only_draw_this_wall < Walls.Count && (Corners[j].wallLeft == Walls[only_draw_this_wall] || Corners[j].wallRight == Walls[only_draw_this_wall])))
			{
				Corners[j].DrawDiagnostics();
			}
		}
		DrawDebugPrimitives(only_draw_this_wall);
	}

	public void DrawConnections()
	{
		Gizmos.matrix = Matrix4x4.identity;
		for (int i = 0; i < Walls.Count; i++)
		{
			Walls[i].DrawEnds();
		}
	}

	public bool Run()
	{
		CleanUpWalls();
		FindCorners();
		if (!ProjectWalls())
		{
			return false;
		}
		FinaliseCorners();
		DecideRoles();
		return true;
	}

	public void RunDiags()
	{
		CleanUpWalls();
		FindCorners();
	}

	private void CleanUpWalls()
	{
		foreach (Wall wall in Walls)
		{
			float num = ProceduralTileTracker.SnapWallOrientation(wall.originalAngle, out var wall_segment_index);
			Vector2 vector = RotateVector(Vector2.right, num);
			Vector2 vector2 = RotateVector(Vector2.up, num);
			float wallSpacingSnapOffset = ProceduralTileTracker.GetWallSpacingSnapOffset(wall.originalCentre, vector2, wall_segment_index);
			Vector2 vector3 = wall.originalCentre + vector2 * wallSpacingSnapOffset;
			Vector2 vector4 = wall.originalLength / 2f * vector;
			Vector2 position = vector3 - vector4;
			Vector2 position2 = vector3 + vector4;
			position = ProceduralTileTracker.SnapWallVertex(position);
			position2 = ProceduralTileTracker.SnapWallVertex(position2);
			float magnitude = (position - position2).magnitude;
			vector3 = (position + position2) * 0.5f;
			wall.originalAngle = num;
			wall.originalCentre = vector3;
			wall.originalCentre3 = new Vector3(vector3.x, 0f, vector3.y);
			wall.originalEnd = position2;
			wall.originalLength = magnitude;
			wall.originalStart = position;
			wall.unitAlong = vector;
			wall.unitOut = vector2;
			wall.unitAlong3 = new Vector3(vector.x, 0f, vector.y);
			wall.unitOut3 = new Vector3(vector2.x, 0f, vector2.y);
			wall.newAngle = wall.originalAngle;
			wall.outerStart = wall.originalStart;
			wall.outerEnd = wall.originalEnd;
			wall.perimiterStart = wall.outerStart;
			wall.perimiterEnd = wall.outerEnd;
		}
	}

	private void FindCorners(float? distanceThreshold = null)
	{
		Corners.Clear();
		int count = Walls.Count;
		if (count <= 1)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			for (int j = i + 1; j < count; j++)
			{
				Wall wall = Walls[i];
				Wall wall2 = Walls[j];
				if (wall.originalSource.wallObject.transform.parent == wall2.originalSource.wallObject.transform.parent)
				{
					if (IsClose(wall.originalStart, wall2.originalEnd))
					{
						Corner corner = new Corner();
						corner.originalPosition = (wall.originalStart + wall2.originalEnd) / 2f;
						corner.outerPosition = corner.originalPosition;
						corner.wallLeft = wall2;
						corner.wallRight = wall;
						wall.neighbourLeft = wall2;
						wall2.neighbourRight = wall;
						wall.cornerLeft = corner;
						wall2.cornerRight = corner;
						Corners.Add(corner);
					}
					else if (IsClose(wall2.originalStart, wall.originalEnd))
					{
						Corner corner2 = new Corner();
						corner2.originalPosition = (wall2.originalStart + wall.originalEnd) / 2f;
						corner2.outerPosition = corner2.originalPosition;
						corner2.wallLeft = wall;
						corner2.wallRight = wall2;
						wall2.neighbourLeft = wall;
						wall.neighbourRight = wall2;
						wall2.cornerLeft = corner2;
						wall.cornerRight = corner2;
						Corners.Add(corner2);
					}
				}
			}
		}
	}

	private bool ProjectWalls()
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			bool flag = i == 0;
			bool flag2 = i == 1;
			bool flag3 = i == 2;
			for (int j = 0; j < Walls.Count; j++)
			{
				Wall wall = Walls[j];
				bool isThicknessAssigned = wall.isThicknessAssigned;
				Wall wall2 = null;
				float num2 = 7.5f;
				bool flag4 = false;
				bool flag5 = false;
				if (!isThicknessAssigned)
				{
					for (int k = 0; k < Walls.Count; k++)
					{
						debugCurrentWall = num++;
						Wall wall3 = Walls[k];
						if (wall3 == wall)
						{
							continue;
						}
						bool flag6 = wall3 == wall.neighbourLeft || wall3 == wall.neighbourRight;
						bool flag7 = false;
						if (flag6)
						{
							flag7 = CalculateInternalAngle(wall3, wall) < 180f;
						}
						if (wall.neighbourLeft == null)
						{
							GameObject wallObject = wall.originalSource.wallObject;
							ProceduralMapTile componentInParent = wallObject.transform.GetComponentInParent<ProceduralMapTile>();
							string arg = ((componentInParent != null) ? componentInParent.name : "unknown");
							Log.Error($"Unconnected corner to left of wall '{wallObject.name}' in maptile '{arg}' near {wall.originalStart}", wallObject);
							return false;
						}
						if (wall.neighbourRight == null)
						{
							GameObject wallObject2 = wall.originalSource.wallObject;
							ProceduralMapTile componentInParent2 = wallObject2.transform.GetComponentInParent<ProceduralMapTile>();
							string arg2 = ((componentInParent2 != null) ? componentInParent2.name : "unknown");
							Log.Error($"Unconnected corner to right of '{wallObject2.name}' in maptile '{arg2}' near {wall.originalEnd} ", wallObject2);
							return false;
						}
						bool flag8 = wall.neighbourLeft.sharingWall != null && wall3 == wall.neighbourLeft.sharingWall;
						bool flag9 = wall.neighbourRight.sharingWall != null && wall3 == wall.neighbourRight.sharingWall;
						bool flag10;
						if ((flag10 = flag8 | flag9) && CalculateInternalAngle(flag8 ? wall.neighbourLeft : wall.neighbourRight, wall) > 90.1f)
						{
							flag10 = false;
						}
						bool flag11 = !flag6 && !flag10;
						bool flag12 = flag6 && !flag7;
						if (flag12)
						{
							if (wall3 == wall.neighbourLeft)
							{
								wall.cornerLeft.isClashing = true;
							}
							else
							{
								wall.cornerRight.isClashing = true;
							}
						}
						if (!(flag11 | flag12))
						{
							continue;
						}
						float num3 = Vector2.Angle(wall3.unitOut, -wall.unitOut);
						bool flag13 = num3 < 90f;
						bool flag14 = num3 < MathF.PI / 180f;
						if (flag12)
						{
							float num4 = 0.5f;
							if (num4 < num2)
							{
								num2 = num4;
								flag4 = false;
								wall2 = wall3;
								flag5 = false;
							}
							continue;
						}
						float num5 = ProjectPointOntoSection(wall.outerStart, wall.outerEnd, wall3.outerStart);
						float num6 = ProjectPointOntoSection(wall.outerStart, wall.outerEnd, wall3.outerEnd);
						if ((num5 < -0.01f && num6 < -0.01f) || (num5 > 1.01f && num6 > 1.01f))
						{
							continue;
						}
						float val = DistanceToSection(wall.outerStart, wall.unitOut, wall3.outerStart, wall3.outerEnd);
						float b_pos_only = DistanceToSection(wall.outerEnd, wall.unitOut, wall3.outerStart, wall3.outerEnd);
						float b_pos_only2 = DistanceToSection(wall.outerStart, wall.unitOut, wall3.outerStart, wall3.originalStart);
						float b_pos_only3 = DistanceToSection(wall.outerStart, wall.unitOut, wall3.outerEnd, wall3.originalEnd);
						float b_pos_only4 = DistanceToSection(wall.outerEnd, wall.unitOut, wall3.outerStart, wall3.originalStart);
						float b_pos_only5 = DistanceToSection(wall.outerEnd, wall.unitOut, wall3.outerEnd, wall3.originalEnd);
						float b_pos_only6 = DistanceToSection(wall.outerStart, wall.unitOut, wall3.originalStart, wall3.originalEnd);
						float b_pos_only7 = DistanceToSection(wall.outerEnd, wall.unitOut, wall3.originalStart, wall3.originalEnd);
						float b_pos_only8 = DistanceToSection(wall3.outerStart, -wall.unitOut, wall.outerStart, wall.outerEnd);
						float b_pos_only9 = DistanceToSection(wall3.outerEnd, -wall.unitOut, wall.outerStart, wall.outerEnd);
						Vector2 point = (wall.outerEnd + wall.outerStart) / 2f;
						float b_pos_only10 = DistanceToSection(point, wall.unitOut, wall3.outerStart, wall3.outerEnd);
						float a = Math.Max(0f, val);
						a = MinPositive(a, b_pos_only);
						a = MinPositive(a, b_pos_only2);
						a = MinPositive(a, b_pos_only3);
						a = MinPositive(a, b_pos_only4);
						a = MinPositive(a, b_pos_only5);
						a = MinPositive(a, b_pos_only6);
						a = MinPositive(a, b_pos_only7);
						a = MinPositive(a, b_pos_only8);
						a = MinPositive(a, b_pos_only9);
						a = MinPositive(a, b_pos_only10);
						if (!float.IsPositiveInfinity(a))
						{
							float num7 = 0f;
							if ((flag2 & flag14) || (flag & flag14))
							{
								num7 = 0.2f;
							}
							if (a - num7 < num2)
							{
								num2 = a;
								flag4 = flag14;
								wall2 = wall3;
								flag5 = flag13;
							}
						}
					}
				}
				float num8 = 0f;
				if (wall2 != null)
				{
					float num9 = 1f;
					bool flag15 = false;
					if (flag4)
					{
						if (Math.Abs(num2) < 0.2f)
						{
							flag15 = true;
						}
						else if (!wall2.isThicknessAssigned)
						{
							num9 = 0.5f;
						}
					}
					else if (flag5 && !wall2.isThicknessAssigned)
					{
						num9 = 0.45f;
					}
					float num10 = num2 * num9;
					if (flag15)
					{
						if (flag)
						{
							wall.isShared = true;
							wall.isFlush = true;
							wall2.isShared = true;
							if (wall.sharingWall == null)
							{
								_ = wall2.sharingWall;
							}
							wall.sharingWall = wall2;
							wall2.sharingWall = wall;
							wall.maxThickness = 0.4f;
							wall2.maxThickness = 0.4f;
							wall.isThicknessAssigned = true;
							wall2.isThicknessAssigned = true;
						}
					}
					else if ((flag2 & flag4) || (flag3 && !flag4))
					{
						wall.maxThickness = num10;
						wall.isThicknessAssigned = true;
						num8 = num10;
						wall.isFlush = flag4 && !wall2.isShared;
					}
				}
				else if (!wall.isThicknessAssigned)
				{
					if (flag3)
					{
						num8 = num2;
						wall.maxThickness = num2;
						wall.isThicknessAssigned = true;
					}
				}
				else
				{
					num8 = wall.maxThickness / 2f;
				}
				if (num8 > 0f)
				{
					wall.outerStart += wall.unitOut * num8;
					wall.outerEnd += wall.unitOut * num8;
					if (wall.cornerLeft != null && wall.cornerLeft.wallLeft != null)
					{
						Corner cornerLeft = wall.cornerLeft;
						Wall wallLeft = cornerLeft.wallLeft;
						cornerLeft.outerPosition = IntersectionOfSegments(wallLeft.outerStart, wallLeft.outerEnd, wall.outerStart, wall.outerEnd);
						wall.perimiterStart = cornerLeft.outerPosition;
						wallLeft.perimiterEnd = cornerLeft.outerPosition;
					}
					if (wall.cornerRight != null && wall.cornerRight.wallRight != null)
					{
						Corner cornerRight = wall.cornerRight;
						Wall wallRight = cornerRight.wallRight;
						cornerRight.outerPosition = IntersectionOfSegments(wallRight.outerStart, wallRight.outerEnd, wall.outerStart, wall.outerEnd);
						wall.perimiterEnd = cornerRight.outerPosition;
						wallRight.perimiterStart = cornerRight.outerPosition;
					}
				}
			}
		}
		return true;
	}

	private void FinaliseCorners()
	{
		for (int i = 0; i < Corners.Count; i++)
		{
			Corner corner = Corners[i];
			Wall wallLeft = corner.wallLeft;
			Wall wallRight = corner.wallRight;
			if (wallLeft == null || wallRight == null)
			{
				continue;
			}
			bool num = wallLeft.isShared || wallRight.isShared;
			bool isClashing = corner.isClashing;
			if (num | isClashing)
			{
				bool flag = !isClashing;
				float num2;
				if (flag)
				{
					num2 = (wallLeft.isShared ? 0.5f : 0f);
				}
				else
				{
					num2 = (wallLeft.isShared ? (-0.5f) : (-1f));
				}
				float num3;
				if (flag)
				{
					num3 = (wallRight.isShared ? 0.5f : 0f);
				}
				else
				{
					num3 = (wallRight.isShared ? (-0.5f) : (-1f));
				}
				float num4 = wallLeft.maxThickness * num2;
				float num5 = wallRight.maxThickness * num3;
				Vector2 vector = wallLeft.unitOut * num4;
				Vector2 vector2 = wallRight.unitOut * num5;
				Vector2 vector3 = wallLeft.originalEnd - vector;
				Vector2 vector4 = wallRight.originalStart - vector2;
				Vector2 vector5 = IntersectionOfSegments(vector3, vector3 + wallLeft.unitAlong, vector4, vector4 + wallRight.unitAlong);
				Vector2 originalEnd = vector5 + vector;
				Vector2 originalStart = vector5 + vector2;
				if (flag)
				{
					corner.originalPosition = vector5;
				}
				wallLeft.originalEnd = originalEnd;
				wallRight.originalStart = originalStart;
			}
		}
		for (int j = 0; j < Walls.Count; j++)
		{
			Wall wall = Walls[j];
			wall.originalLength = (wall.originalEnd - wall.originalStart).magnitude;
			wall.originalCentre = (wall.originalEnd + wall.originalStart) / 2f;
			wall.originalCentre3 = new Vector3(wall.originalCentre.x, 0f, wall.originalCentre.y);
		}
	}

	private void DecideRoles()
	{
		System.Random random = new System.Random(rootSeed);
		for (int i = 0; i < Walls.Count; i++)
		{
			Wall wall = Walls[i];
			if (!wall.isOwnershipAssigned)
			{
				if (wall.isShared)
				{
					if (wall.originalLength > wall.sharingWall.originalLength)
					{
						wall.isWallOwner = true;
						wall.sharingWall.isWallOwner = false;
					}
					else
					{
						wall.isWallOwner = false;
						wall.sharingWall.isWallOwner = true;
						_ = wall.sharingWall;
					}
					wall.isOwnershipAssigned = true;
					wall.sharingWall.isOwnershipAssigned = true;
				}
				else
				{
					wall.isWallOwner = true;
					wall.isOwnershipAssigned = true;
				}
			}
			wall.isLeftCornerOwner = false;
			if (wall.cornerLeft != null && wall.neighbourLeft != null)
			{
				if (wall.maxThickness < wall.neighbourLeft.maxThickness)
				{
					wall.isLeftCornerOwner = true;
					wall.neighbourLeft.isRightCornerOwner = false;
				}
				else
				{
					wall.isLeftCornerOwner = false;
					wall.neighbourLeft.isRightCornerOwner = true;
				}
			}
			wall.isRightCornerOwner = false;
			if (wall.cornerRight != null && wall.neighbourRight != null)
			{
				if (wall.maxThickness < wall.neighbourRight.maxThickness)
				{
					wall.isRightCornerOwner = true;
					wall.neighbourRight.isLeftCornerOwner = false;
				}
				else
				{
					wall.isRightCornerOwner = false;
					wall.neighbourRight.isLeftCornerOwner = true;
				}
			}
			wall.wallSeed = random.Next();
			int num = 0;
			if (wall.isThicknessAssigned)
			{
				num = ((!wall.isShared) ? (wall.isFlush ? 1 : 0) : ((!wall.isWallOwner) ? 1 : 3));
			}
			else
			{
				num = 2;
			}
			wall.debugIndex = num;
		}
	}

	private void AdjustSharedWall(Wall w)
	{
		Wall sharingWall = w.sharingWall;
		m_DebugShow = true;
		float num = ProjectPointOntoSection(w.originalStart, w.originalEnd, sharingWall.originalStart);
		float num2 = ProjectPointOntoSection(w.originalStart, w.originalEnd, sharingWall.originalEnd);
		m_DebugShow = false;
		bool flag = false;
		if (num > 1f)
		{
			sharingWall.originalEnd = w.originalEnd;
			w.cornerRight.IsTJunction = true;
			w.cornerRight.IsTOnRight = false;
			sharingWall.cornerRight.IsTJunction = true;
			sharingWall.cornerRight.IsTOnRight = false;
			flag = true;
		}
		else if (num2 < 0f)
		{
			sharingWall.originalStart = w.originalStart;
			w.cornerLeft.IsTJunction = true;
			w.cornerLeft.IsTOnRight = true;
			sharingWall.cornerLeft.IsTJunction = true;
			sharingWall.cornerLeft.IsTOnRight = true;
			flag = true;
		}
		if (flag)
		{
			sharingWall.originalLength = (sharingWall.originalEnd - sharingWall.originalStart).magnitude;
			sharingWall.originalCentre = (sharingWall.originalEnd + sharingWall.originalStart) / 2f;
			sharingWall.originalCentre3 = new Vector3(sharingWall.originalCentre.x, sharingWall.originalCentre3.y, sharingWall.originalCentre.y);
			sharingWall.isWallOwner = true;
		}
	}

	private float CalculateInternalAngle(Wall a, Wall b)
	{
		bool flag = a == b.neighbourLeft;
		bool flag2 = a == b.neighbourRight;
		if (!flag && !flag2)
		{
			throw new ApplicationException("Walls are not connected");
		}
		float num = 0f;
		if (flag)
		{
			return AngleBetweenACW(a.originalStart - a.originalEnd, b.originalEnd - b.originalStart);
		}
		return AngleBetweenACW(b.originalStart - b.originalEnd, a.originalEnd - a.originalStart);
	}

	private bool IsClose(Vector2 a, Vector2 b)
	{
		return Vector2.SqrMagnitude(a - b) < 0.040000003f;
	}

	public static float ProjectPointOntoSection(Vector2 a, Vector2 b, Vector2 p)
	{
		Vector2 lhs = p - a;
		float magnitude = (b - a).magnitude;
		Vector2 normalized = (b - a).normalized;
		return Vector2.Dot(lhs, normalized) / magnitude;
	}

	private float DistanceToSection(Vector2 point, Vector2 direction, Vector2 a, Vector2 b)
	{
		Vector2 vector = point;
		Vector2 vector2 = point + direction;
		Vector2 vector3 = a;
		Vector2 vector4 = b;
		float num = vector.x - vector3.x;
		float num2 = vector3.y - vector4.y;
		float num3 = vector.y - vector3.y;
		float num4 = vector3.x - vector4.x;
		float num5 = vector.x - vector2.x;
		float num6 = vector.y - vector2.y;
		float num7 = num5 * num2 - num6 * num4;
		float num8 = (num * num2 - num3 * num4) / num7;
		float num9 = (0f - (num5 * num3 - num6 * num)) / num7;
		if (num8 > -0.2f && num9 > 0.01f && num9 < 0.99f)
		{
			return num8;
		}
		return float.PositiveInfinity;
	}

	private Vector2 IntersectionOfSegments(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{
		float num = p1.x - p3.x;
		float num2 = p3.y - p4.y;
		float num3 = p1.y - p3.y;
		float num4 = p3.x - p4.x;
		float num5 = p1.x - p2.x;
		float num6 = p1.y - p2.y;
		float num7 = num5 * num2 - num6 * num4;
		float num8 = (num * num2 - num3 * num4) / num7;
		return p1 + (p2 - p1) * num8;
	}

	private float AngleBetweenACW(Vector2 a, Vector2 b)
	{
		float num = 0f - Vector2.SignedAngle(b, a);
		if (num < 0f)
		{
			num = 360f + num;
		}
		return num;
	}

	private Vector2 RotateVector(Vector2 v, float angle_degrees)
	{
		float num = MathF.PI * 2f * angle_degrees / 360f;
		float num2 = (float)Math.Cos(num);
		float num3 = (float)Math.Sin(num);
		float x = v.x * num2 + v.y * num3;
		float y = (0f - v.x) * num3 + v.y * num2;
		return new Vector2(x, y);
	}

	private float MinPositive(float a, float b_pos_only)
	{
		if (b_pos_only > -0.01f)
		{
			return Math.Min(a, b_pos_only);
		}
		return a;
	}
}
