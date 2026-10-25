using System;
using System.Collections;
using System.Collections.Generic;
using FH.ApparanceBaking;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH102 : ScenarioScriptFrontend
{
	private bool m_InvalidateTiles = true;

	private static List<CTile> movingTiles = new List<CTile>();

	private ScenarioScript_FH102 _script102 => (ScenarioScript_FH102)_script;

	public override bool _ManualActorHeightControl => true;

	protected override void InitializeInternal()
	{
		_script102.OnTilesChanged = OnTileDataChanged;
		movingTiles.Clear();
		Choreographer.s_Choreographer.CharacterAdded -= OnCharacterOrPropSpawned;
		Choreographer.s_Choreographer.PropSpawned -= OnCharacterOrPropSpawned;
		Choreographer.s_Choreographer.CharacterAdded += OnCharacterOrPropSpawned;
		Choreographer.s_Choreographer.PropSpawned += OnCharacterOrPropSpawned;
	}

	protected override void DeinitializeInternal()
	{
		base.DeinitializeInternal();
		if (!(Choreographer.s_Choreographer == null))
		{
			Choreographer.s_Choreographer.CharacterAdded -= OnCharacterOrPropSpawned;
			Choreographer.s_Choreographer.PropSpawned -= OnCharacterOrPropSpawned;
		}
	}

	private static void OnCharacterOrPropSpawned(GameObject go, CActor actor, CObjectProp prop)
	{
		if (actor != null)
		{
			CTile tile = ScenarioManager.GetTile(actor.StartArrayIndex);
			if (!movingTiles.Contains(tile))
			{
				return;
			}
		}
		else if (prop != null && !movingTiles.Contains(prop.PropTile))
		{
			return;
		}
		CoroutineHelper.instance.StartCoroutine(SpawnGOCoroutine(go));
	}

	private void OnTileDataChanged()
	{
		m_InvalidateTiles = true;
	}

	protected override void TickInternal()
	{
		if (!m_InvalidateTiles)
		{
			return;
		}
		m_InvalidateTiles = false;
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile tile in tiles)
		{
			if (tile == null)
			{
				continue;
			}
			string tileGuid = tile.m_Hex.TileGuid;
			GameObject tile2 = EnvironmentLoader.Instance.GetTile(tileGuid);
			if (tile2 == null)
			{
				continue;
			}
			bool walkable = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Walkable;
			bool activeInHierarchy = tile2.gameObject.activeInHierarchy;
			tile2.gameObject.SetActive(walkable);
			if (walkable && !activeInHierarchy)
			{
				movingTiles.Add(tile);
				CoroutineHelper.instance.StartCoroutine(SpawnGOCoroutine(tile2, () =>
				{
					movingTiles.Remove(tile);
				}));
			}
		}
	}

	private static IEnumerator SpawnGOCoroutine(GameObject go, Action onDone = null)
	{
		float duration = 1.5f;
		float startTime = Time.time;
		float endTime = startTime + duration;
		while (Time.time < endTime)
		{
			float t = (Time.time - startTime) / duration;
			UpdateFloatProgress(go, t, updateScale: true);
			yield return new WaitForEndOfFrame();
		}
		onDone?.Invoke();
	}

	private static void UpdateFloatProgress(GameObject go, float t, bool updateScale)
	{
		Vector3 position = go.transform.position;
		position.y = Mathf.Lerp(Mathf.Lerp(-2f, 0.3f, t), Mathf.Lerp(0.3f, -0.07641175f, t), t);
		go.transform.position = position;
		if (updateScale)
		{
			go.transform.localScale = Vector3.one * Mathf.Lerp(0.01f, 1f, t);
		}
	}
}
