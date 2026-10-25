using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using Libs.GameConsole;
using Libs.Utils;
using UnityEngine;

public class TallCharacterOcclusionDetection : OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>
{
	private Vector2 _cameraPosition;

	private List<Vector2> _shortCharacters;

	private ShapeDrawer _shapeDrawer;

	private bool _showDebug;

	protected override List<ICharacterOccluder> FetchOccluders()
	{
		List<ICharacterOccluder> list = new List<ICharacterOccluder>();
		foreach (GameObject item in Choreographer.s_Choreographer.AllActorsInScene)
		{
			if (item.TryGetComponent<TallCharacter>(out var component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	protected override void InitializeReferences()
	{
		_shortCharacters = new List<Vector2>();
	}

	protected override TallCharacterOcclusionCheckJob CreateJob(int startIndex, int count)
	{
		return new TallCharacterOcclusionCheckJob(startIndex, count, _cameraPosition, _shortCharacters, _occluders);
	}

	protected override void DisposeJob(TallCharacterOcclusionCheckJob job)
	{
		job.Dispose();
	}

	private void Update()
	{
		_cameraPosition = OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector2(CameraController.s_CameraController.m_Camera.transform.position);
		_shortCharacters.Clear();
		_shortCharacters.AddRange(Choreographer.s_Choreographer.m_ClientPlayers.Select((GameObject x) => OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector2(x.transform.position)));
		Tick();
		if (_showDebug)
		{
			DrawDebug();
		}
	}

	private void DrawDebug()
	{
		int num = 0;
		for (int i = 0; i < _occluders.Count; i++)
		{
			ICharacterOccluder characterOccluder = _occluders[i];
			Color green = Color.green;
			Color red = Color.red;
			Color color = (_lastResults[i] ? green : red);
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(characterOccluder.Front), OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(characterOccluder.Back), 0.1f, color);
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(characterOccluder.Left), OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(characterOccluder.Right), 0.1f, color);
		}
		for (int j = 0; j < _shortCharacters.Count; j++)
		{
			Color cyan = Color.cyan;
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(_cameraPosition), OcclusionDetection<ICharacterOccluder, TallCharacterOcclusionCheckJob, bool>.ToVector3(_shortCharacters[j]), 0.05f, cyan);
			_shapeDrawer.DrawCircleXZ(num++, _shortCharacters[j], 0.2f, 20, 0.05f, cyan);
		}
		_shapeDrawer.DrawCircleXZ(num++, _cameraPosition, 0.3f, 25, 0.05f, Color.magenta);
	}

	[ConsoleCommand(Command = "characters_fade_debug")]
	public static void Cmd_ToggleDrawDebug(IConsole console)
	{
		Object.FindObjectsOfType<TallCharacterOcclusionDetection>().ForEach((TallCharacterOcclusionDetection x) =>
		{
			x.ToggleDebug();
		});
	}

	public void ToggleDebug()
	{
		_showDebug = !_showDebug;
		if (_shapeDrawer == null)
		{
			GameObject gameObject = new GameObject("GetType_DebugDrawer");
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			_shapeDrawer = gameObject.AddComponent<ShapeDrawer>();
			gameObject.SetActive(_showDebug);
		}
		_shapeDrawer.gameObject.SetActive(_showDebug);
	}
}
