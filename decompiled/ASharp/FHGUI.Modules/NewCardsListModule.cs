using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class NewCardsListModule : FHMapViewModule
{
	public TMP_Text Description;

	public TMP_Text TextChoices;

	public UIOutpostCardsPair TemplateCardsPair;

	public RectTransform ContainerCardsPairs;

	public RectTransform HoverReparentTransform;

	public UITextTooltipTarget RespectButtonTooltip;

	public FHButton RespecButton;

	private List<UIOutpostCardsPair> _pairs = new List<UIOutpostCardsPair>();

	private void InitCache()
	{
		_pairs.Clear();
		_pairs.AddRange(ContainerCardsPairs.GetComponentsInChildren<UIOutpostCardsPair>());
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	public void Init(IEnumerable<CardDTO> levelUpCards, string hinKey, int choicesRemaining, int characterLevel, bool canRespec, bool canRespecThisCharacter, bool canAffordRespec, uint respecGoldCost, Action onClickRespec, UICardInteractionEvents cardInteractionEvents)
	{
		if (_pairs.Count == 0)
		{
			InitCache();
		}
		TextChoices.text = $"{choicesRemaining}";
		Description.text = LocalizationManager.GetTranslation(hinKey);
		List<IGrouping<int, CardDTO>> list = (from c in levelUpCards
			group c by c.Level).ToList();
		RespecButton.gameObject.SetActive(canRespec);
		RespecButton.interactable = canRespecThisCharacter;
		RespecButton.onClick.RemoveAllListeners();
		RespecButton.onClick.AddListener(() =>
		{
			onClickRespec?.Invoke();
		});
		if (RespectButtonTooltip != null)
		{
			Color color = (canAffordRespec ? Color.white : FHUIColors.MISSING_RESOURCE);
			string text = FHUILocFormatting.Color($"{respecGoldCost} <sprite name=Gold>", color);
			RespectButtonTooltip.SetText(text);
		}
		HelperTools.NormalizePool(ref _pairs, TemplateCardsPair.gameObject, ContainerCardsPairs, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			IGrouping<int, CardDTO> grouping = list[num];
			int key = grouping.Key;
			bool isLevelAvailable = characterLevel >= key;
			_pairs[num].Init(key, grouping.First(), grouping.Last(), cardInteractionEvents, isLevelAvailable);
		}
	}

	public void Clear()
	{
		foreach (UIOutpostCardsPair pair in _pairs)
		{
			pair.Clear();
		}
	}
}
