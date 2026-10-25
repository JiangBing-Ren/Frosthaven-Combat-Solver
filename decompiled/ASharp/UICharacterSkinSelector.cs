using System;
using System.Collections.Generic;
using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSkinSelector : MonoBehaviour
{
	[Header("Skin settings")]
	[SerializeField]
	private Sprite[] skinToggleSprites;

	[SerializeField]
	private Image skinToggleImage;

	[SerializeField]
	private UITextTooltipTarget skinTooltip;

	[SerializeField]
	private TextMeshProUGUI skinName;

	private Action<string> onSelectedSkin;

	private bool defaultSkin;

	private List<string> skins;

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void Display(CMapCharacter characterData, Action<string> onSelectedSkin)
	{
		this.onSelectedSkin = onSelectedSkin;
		skins = UIInfoTools.Instance.GetCharacterAppearanceSkin(characterData.CharacterYMLData.Model.ToString(), characterData.CharacterYMLData.CustomCharacterConfig);
		if (skins != null && skins.Count > 0)
		{
			defaultSkin = string.IsNullOrEmpty(characterData.SkinId);
			skinToggleImage.sprite = skinToggleSprites[(!defaultSkin) ? 1u : 0u];
			skinTooltip.SetText(LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : "GUI_SKIN_ON_TOOLTIP"));
			skinName.text = LocalizationManager.GetTranslation($"{characterData.CharacterYMLData.Model}_SKIN_{skins[0]}");
			gameObject.SetActive(value: true);
		}
		else
		{
			Hide();
		}
	}

	public void ToggleSkin()
	{
		if (skins != null && skins.Count != 0)
		{
			defaultSkin = !defaultSkin;
			skinToggleImage.sprite = skinToggleSprites[(!defaultSkin) ? 1u : 0u];
			skinTooltip.SetText(LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : "GUI_SKIN_ON_TOOLTIP"), refreshTooltip: true);
			onSelectedSkin?.Invoke(defaultSkin ? null : skins[0]);
		}
	}
}
