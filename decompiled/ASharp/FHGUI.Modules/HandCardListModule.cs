using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class HandCardListModule : CardListModuleBase
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text TextCounter;

	[SerializeField]
	private RectTransform TransformSeparator;

	[SerializeField]
	private RectTransform TransformPreviewOverride;

	[Header("Settings")]
	[SerializeField]
	private Color _invalidCountColor;

	[SerializeField]
	private string _TMPSpriteName_Primary = "GMMeleeGold";

	[SerializeField]
	private string _TMPSpriteName_Alt = "GMRangedGold";

	private Color _startingColor;

	private bool _isDualDeck;

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			if ((object)TextCounter != null)
			{
				_startingColor = TextCounter.color;
			}
		}
	}

	public void Init(Params @params, Color color, Color altColor, bool isDualDeck)
	{
		InitCache();
		_isDualDeck = isDualDeck;
		Init(@params);
		foreach (UIOutpostCardSlot slot in _slots)
		{
			slot.SetFaceColor(slot.HasImage ? Color.white : color);
		}
	}

	public override void Init(Params @params)
	{
		InitCache();
		base.Init(@params);
		bool flag = @params.MinCount > -1 && @params.Cards.Count > @params.MinCount;
		UpdateCounterText(@params.Cards, @params.MinCount, flag);
		if (TransformSeparator != null)
		{
			if (flag)
			{
				TransformSeparator.SetAsLastSibling();
				int siblingIndex = _slots[Data.MinCount].transform.GetSiblingIndex();
				TransformSeparator.gameObject.SetActive(value: true);
				TransformSeparator.SetSiblingIndex(siblingIndex);
			}
			else
			{
				TransformSeparator.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateCounterText(List<CardDTO> cards, int minCount, bool overflow)
	{
		bool flag = overflow || cards.Count < minCount;
		string text;
		if (!_isDualDeck)
		{
			text = $"{cards.Count}/{minCount}";
		}
		else
		{
			int num = minCount / 2;
			int num2 = cards.Count((CardDTO c) => c.IsAltColor);
			int num3 = cards.Count - num2;
			flag = flag || num3 != num || num2 != num;
			string text2 = $"{FHUILocFormatting.Sprite(_TMPSpriteName_Primary)}{num3}/{num}";
			string text3 = $"{FHUILocFormatting.Sprite(_TMPSpriteName_Alt)}{num2}/{num}";
			text = text2 + "   " + text3;
		}
		if ((object)TextCounter != null)
		{
			TextCounter.text = text;
			TextCounter.color = (flag ? _invalidCountColor : _startingColor);
		}
	}
}
