using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class BattleFavorModule : FHMapViewModule
{
	public class Params
	{
		public List<SuggestibleSlotDTO> Favors;

		public uint Difficulty;

		public uint MaxPoints;

		public Func<SuggestibleSlotDTO, IEnumerable<FHInputEventContainer.Params>> SlotInteractions;

		public Func<string, bool, UIPrefabTooltipTarget.TooltipInitParams> TooltipInitParams;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_FavorsContainer;

	[SerializeField]
	private UIScenarioBriefingSuggestibleSlot m_FavorPrototype;

	[SerializeField]
	private TMP_Text m_TextDifficulty;

	[SerializeField]
	private TMP_Text m_TextPoints;

	[SerializeField]
	private Image m_DifficultyColorBanner;

	[SerializeField]
	private FHStacksCounter m_FavorPoints;

	[Header("Settings")]
	[SerializeField]
	private Sprite[] m_DifficultySprites;

	private List<UIScenarioBriefingSuggestibleSlot> m_Favors = new List<UIScenarioBriefingSuggestibleSlot>(5);

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_FavorsContainer.GetComponentsInChildren(m_Favors);
		}
	}

	public void Init(Params initParams)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(initParams.Favors.Count, m_Favors, m_FavorPrototype, m_FavorsContainer, setElement);
		if ((bool)m_TextDifficulty)
		{
			m_TextDifficulty.text = LocalizationManager.GetTranslation(GetDifficultyKey(initParams.Difficulty));
		}
		if ((bool)m_TextPoints)
		{
			m_TextPoints.text = (initParams.MaxPoints - initParams.Difficulty).ToString();
		}
		if ((bool)m_DifficultyColorBanner)
		{
			m_DifficultyColorBanner.overrideSprite = GetDifficultySprite(initParams.Difficulty);
		}
		if ((bool)m_FavorPoints)
		{
			m_FavorPoints.Init((int)(initParams.MaxPoints - initParams.Difficulty), (int)initParams.MaxPoints);
		}
		void setElement(int index, UIScenarioBriefingSuggestibleSlot b)
		{
			SuggestibleSlotDTO suggestibleSlotDTO = initParams.Favors[index];
			b.Init(suggestibleSlotDTO, null, initParams?.TooltipInitParams(suggestibleSlotDTO.Id, arg2: false));
			FHInputEventContainer component = b.GetComponent<FHInputEventContainer>();
			if ((bool)component)
			{
				component.Init(initParams?.SlotInteractions(suggestibleSlotDTO));
			}
			suggestibleSlotDTO.SlotPickerPoint = b.PickerPoint;
		}
	}

	private Sprite GetDifficultySprite(uint difficulty)
	{
		switch (difficulty)
		{
		case 0u:
		case 1u:
			return m_DifficultySprites[0];
		case 2u:
		case 3u:
			return m_DifficultySprites[1];
		default:
			return m_DifficultySprites[2];
		}
	}

	private string GetDifficultyKey(uint difficulty)
	{
		switch (difficulty)
		{
		case 0u:
		case 1u:
			return "DIFFICULTY_LOW";
		case 2u:
		case 3u:
			return "DIFFICULTY_MEDIUM";
		default:
			return "DIFFICULTY_VERY_HIGH";
		}
	}
}
