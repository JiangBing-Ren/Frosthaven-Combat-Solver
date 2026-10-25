using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.ApparanceBaking;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH131 : ScenarioScriptFrontend
{
	[ConsoleVariable(Alias = "_131_IcebergBobbingSpeed")]
	public static float IcebergBobbingSpeed = 1f;

	[ConsoleVariable(Alias = "_131_IcebergBobbingHeight")]
	public static float IcebergBobbingHeight = 0.14f;

	[ConsoleVariable(Alias = "_131_IcebergBobbingAngle")]
	public static float IcebergBobbingAngle = 0.5f;

	[ConsoleVariable(Alias = "_131_IcebergHeight")]
	public static float IcebergHeight = -0.16f;

	[ConsoleVariable(Alias = "_131_IcebergScale")]
	public static float IcebergScale = 1.113817f;

	[ConsoleVariable(Alias = "_131_IcebergMaxTilt")]
	public static float IcebergMaxTilt = 10f;

	[ConsoleVariable(Alias = "_131_TileHeightOffset")]
	public static float TileHeightOffset = 0.9f;

	private GameObject iceberg;

	private MeshFilter waterPlane;

	private Plane iceBergTop = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

	private float curAngle;

	private Dictionary<GameObject, float> objectHeights = new Dictionary<GameObject, float>();

	private Dictionary<CActor, Point> playerPositions = new Dictionary<CActor, Point>();

	private const int width = 200;

	private const int height = 200;

	private const float cellSize = 2f;

	private ScenarioScript_FH131 _script131 => (ScenarioScript_FH131)_script;

	protected override void InitializeInternal()
	{
		InputFHManager.Controls.DisableAction(CameraController.s_CameraController, InputFHManager.Controls.Camera.Rotate);
		InputFHManager.Controls.DisableAction(CameraController.s_CameraController, InputFHManager.Controls.Camera.DragRotate);
	}

	protected override void DeinitializeInternal()
	{
		base.DeinitializeInternal();
		if (CameraController.s_CameraController != null)
		{
			InputFHManager.Controls.EnableAction(CameraController.s_CameraController, InputFHManager.Controls.Camera.Rotate);
			InputFHManager.Controls.EnableAction(CameraController.s_CameraController, InputFHManager.Controls.Camera.DragRotate);
		}
	}

	protected override void TickInternal()
	{
		if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForCardSelection && Singleton<CardsHandManager>.Instance.IsActive() && WorldspaceStarHexDisplay.Instance.CurrentDisplayState == WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.CharacterPlacement)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (!playerPositions.TryGetValue(playerActor, out var value) || value != playerActor.ArrayIndex)
				{
					playerPositions[playerActor] = playerActor.ArrayIndex;
					_script131.CalcTilt();
				}
			}
		}
		UpdateTiles();
	}

	private void UpdateHeight(GameObject go, bool cache, bool instant = false)
	{
		Vector3 position = go.transform.position;
		float a = position.y;
		if (cache && objectHeights.TryGetValue(go, out var value))
		{
			a = value;
		}
		float y = position.y;
		Vector3 origin = new Vector3(position.x, iceBergTop.distance + 100f, position.z);
		Ray ray = new Ray(origin, Vector3.down);
		if (iceBergTop.Raycast(ray, out var enter))
		{
			y = ray.GetPoint(enter).y;
		}
		float value2 = (position.y = (instant ? y : Mathf.Lerp(a, y, Time.deltaTime)));
		go.transform.position = position;
		if (cache)
		{
			objectHeights[go] = value2;
		}
	}

	private Mesh GenerateMesh()
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[40401];
		int[] array2 = new int[240000];
		Vector2[] array3 = new Vector2[array.Length];
		float num = 200f;
		float num2 = 200f;
		for (int i = 0; i <= 200; i++)
		{
			for (int j = 0; j <= 200; j++)
			{
				int num3 = i * 201 + j;
				array[num3] = new Vector3((float)j * 2f - num, 0f, (float)i * 2f - num2);
				array3[num3] = new Vector2((float)j / 200f, (float)i / 200f);
			}
		}
		int num4 = 0;
		for (int k = 0; k < 200; k++)
		{
			for (int l = 0; l < 200; l++)
			{
				int num5 = (array2[num4] = k * 201 + l);
				array2[num4 + 1] = num5 + 200 + 1;
				array2[num4 + 2] = num5 + 1;
				array2[num4 + 3] = num5 + 1;
				array2[num4 + 4] = num5 + 200 + 1;
				array2[num4 + 5] = num5 + 200 + 2;
				num4 += 6;
			}
		}
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.uv = array3;
		mesh.RecalculateNormals();
		return mesh;
	}

	private void UpdateTiles()
	{
		float num = 0f;
		if (_script131.endRoundLeft == 3)
		{
			num = -0.5f;
		}
		else if (_script131.endRoundRight == 3)
		{
			num = 0.5f;
		}
		else if (_script131.endRoundLeft == 4)
		{
			num = -1f;
		}
		else if (_script131.endRoundRight == 4)
		{
			num = 1f;
		}
		else if (_script131.endRoundLeft == 5)
		{
			num = -1.5f;
		}
		else if (_script131.endRoundRight == 5)
		{
			num = 1.5f;
		}
		bool flag = false;
		if (iceberg == null)
		{
			iceberg = GameObject.Find("EN_CST_IcebergLevel131_A_NoFade");
			if (iceberg != null)
			{
				iceberg.transform.localPosition = new Vector3(3.331989f, IcebergHeight, 3.332462f);
				iceberg.transform.localScale = Vector3.one * IcebergScale;
				iceberg.transform.rotation = Quaternion.Euler(IcebergMaxTilt * num, 90f, 0f);
				flag = true;
			}
		}
		if (waterPlane == null)
		{
			GameObject gameObject = GameObject.Find("Default Coastal Background_Ice");
			if (gameObject != null)
			{
				foreach (GameObject item in gameObject.FindAllInChildren("_ProceduralMesh"))
				{
					if (!(item.GetComponent<MeshRenderer>() == null) && item != null)
					{
						waterPlane = item.GetComponent<MeshFilter>();
						if (waterPlane != null)
						{
							waterPlane.mesh = GenerateMesh();
							break;
						}
					}
				}
			}
		}
		if (!(iceberg != null))
		{
			return;
		}
		Vector3 localPosition = iceberg.transform.localPosition;
		localPosition.y = IcebergHeight;
		localPosition.y += Mathf.Sin(Time.time * IcebergBobbingSpeed) * IcebergBobbingHeight;
		float num2 = Mathf.Sin(Time.time * IcebergBobbingSpeed) * IcebergBobbingAngle;
		curAngle = (flag ? (IcebergMaxTilt * num) : Mathf.Lerp(curAngle, (IcebergMaxTilt + num2) * num, Time.deltaTime));
		iceberg.transform.localPosition = (flag ? localPosition : Vector3.Lerp(iceberg.transform.localPosition, localPosition, Time.deltaTime));
		iceberg.transform.rotation = Quaternion.Euler(curAngle, 90f, num2);
		iceBergTop = new Plane(Quaternion.Euler(num2, 0f, 0f - curAngle) * Vector3.up, new Vector3(iceberg.transform.position.x, iceberg.transform.position.y + TileHeightOffset, iceberg.transform.position.z));
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null)
			{
				continue;
			}
			string tileGuid = cTile.m_Hex.TileGuid;
			GameObject tile = EnvironmentLoader.Instance.GetTile(tileGuid);
			if (!(tile == null))
			{
				if (_script131.EdgeTiles.Contains(cTile))
				{
					tile.SetActive(value: false);
				}
				else
				{
					UpdateHeight(tile, cache: false, instant: true);
				}
			}
		}
		foreach (CActor item2 in ScenarioManager.Scenario.AllActorsThreadSafe)
		{
			GameObject gameObject2 = Choreographer.s_Choreographer.FindClientActorGameObject(item2);
			if (!(gameObject2 == null) && !ActorBehaviour.GetActorBehaviour(gameObject2).IsJumping)
			{
				UpdateHeight(gameObject2, cache: true, instant: true);
			}
		}
		foreach (CObjectProp item3 in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.ToList())
		{
			GameObject gameObject3 = Choreographer.FindPropGameObject(item3);
			if (!(gameObject3 == null))
			{
				UpdateHeight(gameObject3, cache: false, instant: true);
			}
		}
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			GameObject gameObject4 = Choreographer.FindPropGameObject(spawner.Prop);
			if (!(gameObject4 == null))
			{
				UpdateHeight(gameObject4, cache: false, instant: true);
			}
		}
		CClientTile[,] clientTileArray = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray;
		foreach (CClientTile cClientTile in clientTileArray)
		{
			if (cClientTile != null)
			{
				UpdateHeight(cClientTile.m_GameObject, cache: false, instant: true);
			}
		}
		foreach (IHexControl allActiveStar in WorldspaceStarHexDisplay.Instance.GetAllActiveStars())
		{
			if (allActiveStar != null)
			{
				UpdateHeight(allActiveStar.GameObject, cache: false, instant: true);
			}
		}
	}
}
