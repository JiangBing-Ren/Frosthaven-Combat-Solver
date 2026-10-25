using System.Collections.Generic;
using Apparance.Net;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[RequireComponent(typeof(ApparanceEntity))]
[ExecuteInEditMode]
public class ProceduralTileObserver : ProceduralBase, IProceduralContent
{
	private List<GameObject> Tiles = new List<GameObject>();

	private ParameterCollection Parameters;

	private bool m_bRequestRebuild;

	private bool m_bRequestObserverUpdate;

	private UnityEngine.Vector3 previousPosition;

	private float previousOrientation;

	private Bounds previousBounds;

	private uint previousLocalSeed;

	private bool previousActive;

	private Dictionary<GameObject, UnityEngine.Vector3> TilePositions = new Dictionary<GameObject, UnityEngine.Vector3>();

	public List<GameObject> Debug_TileList => Tiles;

	private void Start()
	{
		m_bRequestObserverUpdate = true;
		m_bRequestRebuild = true;
	}

	private void NotifyObserverMove()
	{
		Bounds bounds = default;
		ProceduralTileTracker.CollectBounds(gameObject, ref bounds);
		ProceduralTileTracker.NotifyObserverMove(gameObject, previousBounds, bounds);
		previousPosition = transform.position;
		previousOrientation = transform.eulerAngles.y;
		previousBounds = bounds;
	}

	private void OnEnable()
	{
		ProceduralTileTracker.RegisterObserver(this);
	}

	private void OnDisable()
	{
		ProceduralTileTracker.UnregisterObserver(this);
		Tiles.Clear();
		m_bRequestRebuild = false;
		m_bRequestObserverUpdate = false;
		previousPosition = UnityEngine.Vector3.zero;
		previousOrientation = 0f;
		previousBounds = default;
		previousLocalSeed = 0u;
		previousActive = false;
	}

	public void NotifyTile(GameObject tile, bool was_in, bool now_in, bool physical_move)
	{
		bool flag = Tiles.Contains(tile);
		if (now_in)
		{
			if (!flag)
			{
				Tiles.Add(tile);
				TilePositions[tile] = tile.transform.position;
			}
		}
		else if (was_in && flag)
		{
			Tiles.Remove(tile);
			TilePositions.Remove(tile);
		}
		Tiles.RemoveAll((GameObject t) => t == null || !((IProceduralTile)t.GetComponent<ProceduralTile>()).IsActive);
		bool flag2 = Tiles.Contains(tile);
		bool flag3 = !flag2 & flag;
		bool flag4 = flag2 && !flag;
		bool flag5 = was_in | now_in;
		bool flag6 = flag2 && TilePositions.ContainsKey(tile) && tile.transform.position != TilePositions[tile];
		if ((flag3 | flag4) || (physical_move & flag5 & flag6))
		{
			m_bRequestRebuild = true;
		}
	}

	public virtual void Update()
	{
		if (gameObject.scene.name != null && gameObject.scene.rootCount != 0)
		{
			UnityEngine.Vector3 position = transform.position;
			float y = transform.eulerAngles.y;
			if (position != previousPosition || y != previousOrientation)
			{
				NotifyObserverMove();
			}
			bool activeInHierarchy = gameObject.activeInHierarchy;
			if (activeInHierarchy != previousActive)
			{
				m_bRequestRebuild = true;
				previousActive = activeInHierarchy;
			}
			if (m_bRequestObserverUpdate)
			{
				m_bRequestObserverUpdate = false;
				NotifyObserverMove();
			}
			if (m_bRequestRebuild)
			{
				m_bRequestRebuild = false;
				Rebuild();
				Apply();
			}
		}
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		ParameterCollection parameterCollection = Parameters.WriteListBegin(3);
		foreach (GameObject tile in Tiles)
		{
			if (tile != null)
			{
				IProceduralTile component = tile.GetComponent<IProceduralTile>();
				if (component != null)
				{
					ParameterCollection parameterCollection2 = parameterCollection.WriteListBegin();
					parameterCollection2.WriteInteger(ProceduralTileTracker.TileAddressFromPosition(component.Position));
					parameterCollection2.WriteInteger(component.Flags);
					parameterCollection2.WriteFloat(component.ContentRadius);
					parameterCollection2.WriteFloat(component.ContentHeight);
					parameterCollection.WriteListEnd();
				}
			}
		}
		Parameters.WriteListEnd();
		WriteExtraParameters(Parameters, 4);
		Parameters.EndWrite();
	}

	public virtual void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
	}

	private void Apply()
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		component.PartialParameterOverride = Parameters;
		component.IsPopulated = false;
		component.NotifyPropertyChanged();
		component.IsPopulated = true;
		component.NotifyPropertyChanged();
	}

	void IProceduralContent.RebuildContent()
	{
		m_bRequestRebuild = true;
	}
}
