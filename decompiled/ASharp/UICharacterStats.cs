using System;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStats : MonoBehaviour
{
	private enum PortraitType
	{
		Full,
		Banner,
		Distribution,
		Adventure,
		FullOffset
	}

	[SerializeField]
	private TMP_Text characterName;

	[SerializeField]
	private TMP_Text characterLevel;

	[SerializeField]
	private ImageProgressBar characterXP;

	[SerializeField]
	private Image characterPortrait;

	[SerializeField]
	private Image characterBackground;

	[SerializeField]
	private LayoutElementExtended layoutElement;

	[SerializeField]
	private PortraitType portraitType;

	public void Init(CPlayerActor cPlayer, Sprite portrait = null)
	{
		characterName.text = LocalizationManager.GetTranslation(cPlayer.ActorLocKey());
		characterLevel.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), cPlayer.Level);
		if (characterPortrait != null)
		{
			characterPortrait.sprite = GetPortrait(cPlayer, portrait);
			if (portraitType == PortraitType.FullOffset)
			{
				CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(cPlayer.CharacterClass.CharacterModel);
				characterPortrait.SetNativeSize();
				characterPortrait.rectTransform.anchoredPosition = characterConfigUI.scenarioPortraitOffset;
			}
		}
		if (characterBackground != null)
		{
			characterBackground.color = UIInfoTools.Instance.GetCharacterColor(cPlayer);
		}
		CMapCharacter cMapCharacter = null;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			cMapCharacter = SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState?.MapParty.SelectedCharacters.ToList().Find((CMapCharacter x) => x.CharacterID == cPlayer.Class.ID);
		}
		int num = 80;
		int num2 = 0;
		if (cMapCharacter != null)
		{
			GetExpRanges(cMapCharacter, out var prevLevelThreshold, out var nextLevelThreshold);
			num = nextLevelThreshold - prevLevelThreshold;
			num2 = cMapCharacter.EXP + cPlayer.XP - prevLevelThreshold;
		}
		characterXP.SetAmount(num2, num);
	}

	private Sprite GetPortrait(CPlayerActor cPlayer, Sprite portrait)
	{
		if (portrait != null)
		{
			return portrait;
		}
		switch (portraitType)
		{
		case PortraitType.Full:
		case PortraitType.FullOffset:
			return UIInfoTools.Instance.GetCharacterHeroPortrait(cPlayer.Class.DefaultModel, cPlayer.CharacterClass.CharacterYML.CustomCharacterConfig);
		case PortraitType.Banner:
			return UIInfoTools.GetHeroPortrait(cPlayer.Class.DefaultModel);
		case PortraitType.Distribution:
			return UIInfoTools.Instance.GetCharacterDistributionPortrait(cPlayer.CharacterClass.CharacterModel);
		case PortraitType.Adventure:
			return UIInfoTools.Instance.GetNewAdventureCharacterPortrait(cPlayer.CharacterClass.CharacterModel);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void GetExpRanges(CMapCharacter charData, out int prevLevelThreshold, out int nextLevelThreshold)
	{
		int num = charData.Level;
		do
		{
			prevLevelThreshold = ((num > 0) ? charData.GetXPThreshold(num - 1) : 0);
			nextLevelThreshold = ((num >= 0) ? charData.GetXPThreshold(num) : 0);
			num--;
		}
		while (num >= -1 && (prevLevelThreshold == int.MaxValue || nextLevelThreshold == int.MaxValue));
	}

	public void Show(bool show)
	{
		if (!(gameObject == null))
		{
			gameObject.SetActive(show);
			if (layoutElement != null)
			{
				layoutElement.overrideMinHeightWithPrefered = show;
			}
		}
	}
}
