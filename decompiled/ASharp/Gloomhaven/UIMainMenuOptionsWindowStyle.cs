using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gloomhaven;

public class UIMainMenuOptionsWindowStyle : MonoBehaviour
{
	[Serializable]
	private class RectTransformSize
	{
		public RectTransform rect;

		public Vector2 deltaSize;

		public Vector2 anchoredPositionOffset;

		private Vector2? defaultDeltaSize;

		private Vector2? defaultAnchoredPosition;

		public void Init()
		{
			if (!defaultDeltaSize.HasValue)
			{
				defaultDeltaSize = rect.sizeDelta;
			}
			if (!defaultAnchoredPosition.HasValue)
			{
				defaultAnchoredPosition = rect.anchoredPosition;
			}
		}

		public void Reset()
		{
			if (defaultDeltaSize.HasValue)
			{
				rect.sizeDelta = defaultDeltaSize.Value;
			}
			if (defaultAnchoredPosition.HasValue)
			{
				rect.anchoredPosition = defaultAnchoredPosition.Value;
			}
		}

		public void Apply()
		{
			Init();
			rect.sizeDelta = deltaSize;
			rect.anchoredPosition = defaultAnchoredPosition.Value + anchoredPositionOffset;
		}
	}

	[SerializeField]
	private GameObject hiddenInMainMenu;

	[SerializeField]
	private List<RectTransformSize> rectTransformSizes;

	private void Awake()
	{
		for (int i = 0; i < rectTransformSizes.Count; i++)
		{
			rectTransformSizes[i].Init();
		}
	}

	public void Refresh()
	{
		if (SaveData.Instance?.Global == null || SaveData.Instance.Global.GameMode == EGameMode.MainMenu)
		{
			hiddenInMainMenu.SetActive(value: false);
			for (int i = 0; i < rectTransformSizes.Count; i++)
			{
				rectTransformSizes[i].Apply();
			}
		}
		else
		{
			hiddenInMainMenu.SetActive(value: true);
			for (int j = 0; j < rectTransformSizes.Count; j++)
			{
				rectTransformSizes[j].Reset();
			}
		}
	}
}
