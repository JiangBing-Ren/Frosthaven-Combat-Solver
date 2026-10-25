using System.Collections.Generic;
using AStar;
using FH.ApparanceBaking;
using FHGUI.Common;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIContextualTutorialHighlightAndFocusCamera : UIContextualTutorialBase
{
	[SerializeField]
	private RectTransform _captionContainer;

	[SerializeField]
	private TMP_Text _caption;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private LayoutElement _scrollViewContentLayout;

	[SerializeField]
	private RectTransform _illustrationContainer;

	[SerializeField]
	private Image _illustration;

	[SerializeField]
	private Toggle _toggleContextualHints;

	[Header("Settings")]
	[SerializeField]
	private int _scrollViewContentMinHeight;

	private GameObject go;

	private List<TileIndex> cachedHighlightedTiles = new List<TileIndex>();

	private List<RectTransform> cachedHighlightedRects = new List<RectTransform>();

	private bool m_InvalidatePosition;

	private RectTransform rectToPlace;

	private Vector2 targetPos;

	private bool hasBeenPlaced;

	private void Awake()
	{
		_toggleContextualHints.onValueChanged.AddListener(HandleOnToggleContextualHints);
	}

	public override bool Show(ContextualData data, params object[] parameters)
	{
		base.Show(data, parameters);
		rectToPlace = GetComponent<RectTransform>();
		if (data.Caption.IsNOTNullOrEmpty())
		{
			_captionContainer.gameObject.SetActive(value: true);
			_caption.text = LocalizationManager.GetTranslation(data.Caption);
		}
		else
		{
			_captionContainer.gameObject.SetActive(value: false);
		}
		_description.text = LocalizationManager.GetTranslation(data.ID);
		Sprite sprite = data.Illustration ?? UIInfoTools.Instance.GetContextualHintIllustration(data.ID);
		if (sprite != null)
		{
			_illustrationContainer.gameObject.SetActive(value: true);
			_illustration.sprite = sprite;
			_scrollViewContentLayout.minHeight = _scrollViewContentMinHeight;
		}
		else
		{
			_illustrationContainer.gameObject.SetActive(value: false);
			_scrollViewContentLayout.minHeight = -1f;
		}
		cachedHighlightedTiles.Clear();
		cachedHighlightedRects.Clear();
		if (data.Actors != null)
		{
			go = Choreographer.s_Choreographer.FindClientActor(data.Actor);
			foreach (CActor actor2 in data.Actors)
			{
				foreach (CTile item in actor2.GetTilesCanAttackFrom())
				{
					TileIndex tileIndex = new TileIndex(item.m_ArrayIndex);
					InteractabilityHighlightCanvas s_Instance = InteractabilityHighlightCanvas.s_Instance;
					if ((object)s_Instance != null && s_Instance.TryAddHighlightToTile(tileIndex))
					{
						cachedHighlightedTiles.Add(tileIndex);
					}
				}
			}
		}
		else if (data.Props != null)
		{
			go = Choreographer.FindPropGameObject(data.Prop);
			foreach (CObjectProp prop in data.Props)
			{
				foreach (Point tile in prop.GetTiles())
				{
					TileIndex tileIndex2 = new TileIndex(tile);
					InteractabilityHighlightCanvas s_Instance2 = InteractabilityHighlightCanvas.s_Instance;
					if ((object)s_Instance2 != null && s_Instance2.TryAddHighlightToTile(tileIndex2))
					{
						cachedHighlightedTiles.Add(tileIndex2);
					}
				}
			}
		}
		else if (data.Tiles != null)
		{
			go = EnvironmentLoader.Instance.GetTile(data.Tiles[0].m_Hex.TileGuid);
			CTile[] tiles = data.Tiles;
			for (int i = 0; i < tiles.Length; i++)
			{
				TileIndex tileIndex3 = new TileIndex(tiles[i].m_ArrayIndex);
				InteractabilityHighlightCanvas s_Instance3 = InteractabilityHighlightCanvas.s_Instance;
				if ((object)s_Instance3 != null && s_Instance3.TryAddHighlightToTile(tileIndex3))
				{
					cachedHighlightedTiles.Add(tileIndex3);
				}
			}
		}
		if (data.UIObjects != null)
		{
			foreach (GameObject uIObject in data.UIObjects)
			{
				RectTransform component = uIObject.GetComponent<RectTransform>();
				InteractabilityHighlightCanvas s_Instance4 = InteractabilityHighlightCanvas.s_Instance;
				if ((object)s_Instance4 != null && s_Instance4.TryAddHighlightForRectTransform(component))
				{
					cachedHighlightedRects.Add(component);
				}
			}
		}
		CActor cActor = Choreographer.s_Choreographer?.CurrentActor ?? GameState.TurnActor;
		if (go != null && (cActor == null || (cActor is CPlayerActor actor && actor.IsUnderMyControl())))
		{
			CameraController.s_CameraController.SetFocalPointGameObject(go);
		}
		m_InvalidatePosition = true;
		_toggleContextualHints.SetIsOnWithoutNotify(!UIContextualTutorialModule.AreTutorialsEnabled);
		return true;
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		ClearHighlights();
		go = null;
	}

	private void ClearHighlights()
	{
		foreach (TileIndex cachedHighlightedTile in cachedHighlightedTiles)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightFromTile(cachedHighlightedTile);
		}
		foreach (RectTransform cachedHighlightedRect in cachedHighlightedRects)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(cachedHighlightedRect);
		}
	}

	private void Update()
	{
		if (go != null && !CameraController.s_CameraController.IsFocalPointGameObject(go) && !Data.FixedPosition)
		{
			CameraController.s_CameraController.SetFocalPointGameObject(go);
			m_InvalidatePosition = true;
		}
		if (rectToPlace != null)
		{
			if (m_InvalidatePosition)
			{
				RepositionModule();
			}
			rectToPlace.anchoredPosition = targetPos;
		}
	}

	private void HandleOnToggleContextualHints(bool active)
	{
		UIContextualTutorialModule.SetTutorialsEnabled(!active);
	}

	private void RepositionModule()
	{
		if (rectToPlace.rect.height <= 0.01f)
		{
			m_InvalidatePosition = true;
			return;
		}
		PlaceRect(rectToPlace, rectToPlace.GetComponentInParent<Canvas>(), cachedHighlightedRects, !hasBeenPlaced, Data.OverrideFallbackPosition);
		m_InvalidatePosition = false;
		hasBeenPlaced = true;
	}

	public void PlaceRect(RectTransform targetRect, Canvas targetCanvas, List<RectTransform> otherRects, bool instant, string overridePosition = null)
	{
		Vector2 anchoredPosition = targetRect.anchoredPosition;
		Vector2 fixedPositionFallback = GetFixedPositionFallback(targetRect, targetCanvas, overridePosition);
		SetNestedUIPosition(targetRect, targetCanvas, fixedPositionFallback);
		if (Data.FixedPosition)
		{
			targetPos = targetRect.anchoredPosition;
			return;
		}
		List<Vector2> list = GenerateCandidatePositions(targetRect, targetCanvas, otherRects);
		Vector2 screenPos = fixedPositionFallback;
		float num = float.MaxValue;
		foreach (Vector2 item in list)
		{
			SetNestedUIPosition(targetRect, targetCanvas, item);
			Rect screenRect = GetScreenRect(targetRect, targetCanvas);
			if (IsWithinCanvas(screenRect, targetCanvas) && !OverlapsAny(screenRect, otherRects))
			{
				float num2 = DistanceToClosest(screenRect.center, otherRects);
				if (num2 < num)
				{
					num = num2;
					screenPos = item;
				}
			}
		}
		SetNestedUIPosition(targetRect, targetCanvas, screenPos);
		targetPos = targetRect.anchoredPosition;
		if (!instant)
		{
			targetRect.anchoredPosition = anchoredPosition;
		}
	}

	private Rect GetScreenRect(RectTransform rt, Canvas canvas)
	{
		Vector3[] array = new Vector3[4];
		rt.GetWorldCorners(array);
		for (int i = 0; i < 4; i++)
		{
			array[i] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, array[i]);
		}
		return new Rect(array[0].x, array[0].y, array[2].x - array[0].x, array[2].y - array[0].y);
	}

	private Vector2 GetFixedPositionFallback(RectTransform targetRect, Canvas targetCanvas, string overridePosition = null)
	{
		if (!(overridePosition == "BottomMiddle"))
		{
			if (overridePosition == "RightMiddle")
			{
				return GetRightEdgeCenter(targetRect, targetCanvas);
			}
			return GetBottomRightCorner(targetRect, targetCanvas);
		}
		return GetBottomMiddleCorner(targetRect, targetCanvas);
	}

	private Vector2 GetRightEdgeCenter(RectTransform targetRect, Canvas targetCanvas)
	{
		RectTransform component = targetCanvas.GetComponent<RectTransform>();
		Rect screenRect = GetScreenRect(targetRect, targetCanvas);
		Rect screenRect2 = GetScreenRect(component, targetCanvas);
		return new Vector2(screenRect2.xMax - screenRect.width * 0.55f, screenRect2.center.y);
	}

	private Vector2 GetBottomRightCorner(RectTransform targetRect, Canvas targetCanvas)
	{
		RectTransform component = targetCanvas.GetComponent<RectTransform>();
		Rect screenRect = GetScreenRect(targetRect, targetCanvas);
		Rect screenRect2 = GetScreenRect(component, targetCanvas);
		return new Vector2(screenRect2.xMax - screenRect.width * 0.55f, screenRect2.yMin + screenRect.height * 0.55f);
	}

	private Vector2 GetBottomMiddleCorner(RectTransform targetRect, Canvas targetCanvas)
	{
		RectTransform component = targetCanvas.GetComponent<RectTransform>();
		Rect screenRect = GetScreenRect(targetRect, targetCanvas);
		Rect screenRect2 = GetScreenRect(component, targetCanvas);
		return new Vector2(screenRect2.xMax * 0.5f, screenRect2.yMin + screenRect.height * 0.55f);
	}

	private List<Vector2> GenerateCandidatePositions(RectTransform targetRect, Canvas targetCanvas, List<RectTransform> otherRects)
	{
		List<Vector2> list = new List<Vector2>();
		Rect screenRect = GetScreenRect(targetRect, targetCanvas);
		Vector2 vector = new Vector2(screenRect.width, screenRect.height) * 0.55f;
		foreach (RectTransform otherRect in otherRects)
		{
			Canvas componentInParent = otherRect.GetComponentInParent<Canvas>();
			Rect screenRect2 = GetScreenRect(otherRect, componentInParent);
			Vector2 min = screenRect2.min;
			Vector2 max = screenRect2.max;
			Vector2 center = screenRect2.center;
			list.Add(new Vector2(min.x - vector.x, min.y - vector.y));
			list.Add(new Vector2(min.x - vector.x, center.y));
			list.Add(new Vector2(min.x - vector.x, max.y + vector.y));
			list.Add(new Vector2(max.x + vector.x, min.y - vector.y));
			list.Add(new Vector2(max.x + vector.x, center.y));
			list.Add(new Vector2(max.x + vector.x, max.y + vector.y));
			list.Add(new Vector2(center.x, max.y + vector.y));
			list.Add(new Vector2(center.x, min.y - vector.y));
		}
		list.Add(GetBottomRightCorner(targetRect, targetCanvas));
		return list;
	}

	private bool OverlapsAny(Rect rect, List<RectTransform> otherRects)
	{
		foreach (RectTransform otherRect in otherRects)
		{
			Canvas componentInParent = otherRect.GetComponentInParent<Canvas>();
			Rect screenRect = GetScreenRect(otherRect, componentInParent);
			if (rect.Overlaps(screenRect))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWithinCanvas(Rect rect, Canvas canvas)
	{
		RectTransform component = canvas.GetComponent<RectTransform>();
		Rect screenRect = GetScreenRect(component, canvas);
		if (screenRect.Contains(rect.min))
		{
			return screenRect.Contains(rect.max);
		}
		return false;
	}

	private float DistanceToClosest(Vector2 point, List<RectTransform> otherRects)
	{
		float num = float.MaxValue;
		foreach (RectTransform otherRect in otherRects)
		{
			Canvas componentInParent = otherRect.GetComponentInParent<Canvas>();
			float num2 = Vector2.Distance(point, GetScreenRect(otherRect, componentInParent).center);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public void SetNestedUIPosition(RectTransform targetRect, Canvas canvas, Vector2 screenPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out var localPoint);
		targetRect.anchoredPosition = localPoint;
	}
}
