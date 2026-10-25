using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

namespace Logging.AdventureMap.OutpostVillage;

public class OutpostCampaignStickerVisuals : MonoBehaviour
{
	[AttributeUsage(AttributeTargets.Field)]
	public class CampaignStickerDropdown : PropertyAttribute
	{
		public readonly string[] Stickers;

		public CampaignStickerDropdown()
		{
			Stickers = CampaignStickers.GetStickerNames().Prepend("NONE").ToArray();
		}
	}

	[CampaignStickerDropdown]
	[SerializeField]
	private string[] _stickers;

	[SerializeField]
	private bool _hideIfAnySticker;

	public IReadOnlyList<string> Stickers => _stickers;

	public void Init(IReadOnlyList<CampaignStickerID> stickers)
	{
		if (_hideIfAnySticker)
		{
			if (_stickers.Any((string sticker) => stickers.Any((CampaignStickerID s) => s.ID.Equals(sticker))))
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
		}
		else if (_stickers.All((string sticker) => stickers.Any((CampaignStickerID s) => s.ID.Equals(sticker))))
		{
			gameObject.SetActive(value: true);
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}
}
