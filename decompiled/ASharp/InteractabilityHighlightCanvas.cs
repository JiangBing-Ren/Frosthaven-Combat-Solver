using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InteractabilityHighlightCanvas : MonoBehaviour
{
	public static InteractabilityHighlightCanvas s_Instance;

	public RectTransform HighlightCanvasRect;

	public GameObject UIHighlightPrefab;

	public GameObject HexHighlightPrefab;

	public List<GameObject> AdditionalHighlighPrefabs = new List<GameObject>();

	public List<GameObject> AdditionalHexHighlighPrefabs = new List<GameObject>();

	private List<InteractabilityHighlightRect> m_ShowingHighlightRects = new List<InteractabilityHighlightRect>();

	private List<GameObject> m_ShowingHighlightHexes = new List<GameObject>();

	public List<InteractabilityHighlightRect> ShowingHighlightRects => m_ShowingHighlightRects;

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
	}

	private void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		try
		{
			ClearHighlights();
		}
		catch
		{
			Log.GUIInfo("Exception caught trying to clear Interactability Highlights OnDestroy");
		}
	}

	public void AddHighlightToTile(TileIndex indexToPlaceTileHighlight, string prefabKey = null)
	{
		if (indexToPlaceTileHighlight == null)
		{
			return;
		}
		CClientTile cClientTile = CAutoTileClick.TileIndexToClientTile(indexToPlaceTileHighlight);
		if (cClientTile == null)
		{
			return;
		}
		GameObject original = HexHighlightPrefab;
		if (!string.IsNullOrEmpty(prefabKey))
		{
			GameObject gameObject = AdditionalHexHighlighPrefabs.SingleOrDefault((GameObject x) => x.name.Contains(prefabKey, StringComparison.InvariantCultureIgnoreCase));
			if (gameObject != null)
			{
				original = gameObject;
			}
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(original, cClientTile.m_GameObject.transform.parent);
		gameObject2.transform.position = cClientTile.m_GameObject.transform.position;
		gameObject2.name = $"InteractabilityHexHighlight_{indexToPlaceTileHighlight.X}_{indexToPlaceTileHighlight.Y}";
		m_ShowingHighlightHexes.Add(gameObject2);
	}

	public bool TryAddHighlightToTile(TileIndex indexToPlaceTileHighlight, string highlightPrefab = null)
	{
		if (indexToPlaceTileHighlight == null)
		{
			return false;
		}
		string value = $"{indexToPlaceTileHighlight.X}_{indexToPlaceTileHighlight.Y}";
		foreach (GameObject showingHighlightHex in m_ShowingHighlightHexes)
		{
			if (showingHighlightHex.name.Contains(value, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}
		AddHighlightToTile(indexToPlaceTileHighlight, highlightPrefab);
		return true;
	}

	public bool TryRemoveHighlightFromTile(TileIndex indexToPlaceTileHighlight)
	{
		if (indexToPlaceTileHighlight == null)
		{
			return false;
		}
		string sufix = $"{indexToPlaceTileHighlight.X}_{indexToPlaceTileHighlight.Y}";
		GameObject gameObject = m_ShowingHighlightHexes.FirstOrDefault((GameObject highlight) => highlight != null && highlight.name.Contains(sufix, StringComparison.OrdinalIgnoreCase));
		if (gameObject == null)
		{
			return false;
		}
		m_ShowingHighlightHexes.Remove(gameObject);
		UnityEngine.Object.Destroy(gameObject);
		return true;
	}

	public void AddHighlightForRectTransform(RectTransform rectToHighlight, InteractabilityIsolatedUIControl control, Vector4 padding = default(Vector4), string prefabKey = null)
	{
		if (rectToHighlight == null)
		{
			return;
		}
		GameObject original = UIHighlightPrefab;
		if (!string.IsNullOrEmpty(prefabKey))
		{
			GameObject gameObject = AdditionalHighlighPrefabs.SingleOrDefault((GameObject x) => x.name.Contains(prefabKey, StringComparison.InvariantCultureIgnoreCase));
			if (gameObject != null)
			{
				original = gameObject;
			}
		}
		InteractabilityHighlightRect component = UnityEngine.Object.Instantiate(original, transform).GetComponent<InteractabilityHighlightRect>();
		component.SetRectToFollow(rectToHighlight, HighlightCanvasRect, padding);
		component.SetInetractabilityControl(control);
		m_ShowingHighlightRects.Add(component);
	}

	public bool TryAddHighlightForRectTransform(RectTransform rectToHighlight, Vector4 padding = default(Vector4), string highlightPrefab = null)
	{
		if (rectToHighlight == null)
		{
			return false;
		}
		foreach (InteractabilityHighlightRect showingHighlightRect in m_ShowingHighlightRects)
		{
			if (showingHighlightRect.m_RectTransformToFollow == rectToHighlight)
			{
				return false;
			}
		}
		AddHighlightForRectTransform(rectToHighlight, null, padding, highlightPrefab);
		return true;
	}

	public bool TryRemoveHighlightForRectTransform(RectTransform rectToHighlight)
	{
		if (rectToHighlight == null)
		{
			return false;
		}
		InteractabilityHighlightRect interactabilityHighlightRect = m_ShowingHighlightRects.FirstOrDefault((InteractabilityHighlightRect highlight) => highlight.m_RectTransformToFollow == rectToHighlight);
		if (interactabilityHighlightRect == null)
		{
			return false;
		}
		m_ShowingHighlightRects.Remove(interactabilityHighlightRect);
		UnityEngine.Object.Destroy(interactabilityHighlightRect.gameObject);
		return true;
	}

	public void ClearHighlights()
	{
		foreach (GameObject showingHighlightHex in m_ShowingHighlightHexes)
		{
			UnityEngine.Object.Destroy(showingHighlightHex);
		}
		m_ShowingHighlightHexes.Clear();
		foreach (InteractabilityHighlightRect showingHighlightRect in m_ShowingHighlightRects)
		{
			showingHighlightRect.SetShowing(shouldShow: false);
		}
		m_ShowingHighlightRects.Clear();
	}
}
