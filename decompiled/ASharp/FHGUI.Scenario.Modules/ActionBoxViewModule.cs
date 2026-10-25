using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using FHGUI.Scenario.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Scenario.Modules;

public class ActionBoxViewModule : FHScenarioViewModule
{
	[SerializeField]
	private Transform _buttonsRoot;

	[SerializeField]
	private Transform _itemsRoot;

	[SerializeField]
	private TMP_Text _infoText;

	public Action<TacItemDTO> OnItemClicked;

	public void SetButtons(params ButtonsBarParam[] buttons)
	{
		ButtonsBarElement.SetButtons(_buttonsRoot, buttons);
	}

	public void ClearButtons()
	{
		ButtonsBarElement.ClearButtons(_buttonsRoot);
	}

	public void ClearUsableItems()
	{
		ButtonsBarElement.ClearButtons(_itemsRoot);
	}

	public void SetUsableItems(List<TacItemDTO> items)
	{
		ButtonsBarParam[] buttonParams = items.Select((TacItemDTO item) => new ButtonsBarParam(item.Image, null, () =>
		{
			TacItemDTO obj = item;
			OnItemClicked?.Invoke(obj);
		})).ToArray();
		ButtonsBarElement.SetButtons(_itemsRoot, buttonParams);
	}

	public void SetInfoText(string labelKey)
	{
		if (!string.IsNullOrWhiteSpace(labelKey))
		{
			if (labelKey.StartsWith('@'))
			{
				UIHelperUtil.SetNonLocalizedText(_infoText, labelKey.Remove(0, 1));
			}
			else
			{
				UIHelperUtil.SetTextKey(_infoText, labelKey);
			}
		}
		_infoText.gameObject.SetActive(value: true);
	}

	public void ClearInfoText()
	{
		_infoText.gameObject.SetActive(value: false);
	}
}
