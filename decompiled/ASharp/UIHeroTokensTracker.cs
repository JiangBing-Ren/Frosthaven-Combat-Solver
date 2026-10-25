using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroTokensTracker : MonoBehaviour, ICharacterSpecificUIElement
{
	[SerializeField]
	private HeroGaugeVisualsUIDataSO _gaugeVisualsDataSo;

	[SerializeField]
	private Image _gaugeBackgroundImage;

	[SerializeField]
	private Image _gaugeTokenImage;

	[SerializeField]
	private Transform _tokenGroupTransform;

	[SerializeField]
	private List<Image> _tokenImages = new List<Image>();

	[SerializeField]
	private Transform _tokenSummaryTransform;

	[SerializeField]
	private TMP_Text _tokenSummaryText;

	[SerializeField]
	private Image _tokenSummaryImage;

	private Sprite _activeTokenSprite;

	private Sprite _inactiveTokenSprite;

	public bool IsCharacterElement(CPlayerActor playerActor)
	{
		if (playerActor == null)
		{
			return false;
		}
		switch (playerActor.CharacterClass.CharacterModel)
		{
		case ECharacter.Infuser:
		case ECharacter.Blinkblade:
		case ECharacter.Pyroclast:
		case ECharacter.Deathwalker:
		case ECharacter.Deepwraith:
		case ECharacter.Crashingtide:
		case ECharacter.Geminate:
		case ECharacter.Metalmosaic:
		case ECharacter.Shattersong:
			return true;
		default:
			return false;
		}
	}

	public void Init(CPlayerActor playerActor)
	{
		if (!IsCharacterElement(playerActor))
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		int currentTokens = GetCurrentTokens(playerActor);
		ECharacter characterModel = playerActor.CharacterClass.CharacterModel;
		bool gaugeVisuals = _gaugeVisualsDataSo.GetGaugeVisuals(characterModel, currentTokens, out var gaugeSprite, out var tokenSprite);
		_gaugeBackgroundImage.enabled = gaugeVisuals;
		_gaugeTokenImage.enabled = gaugeVisuals && tokenSprite != null;
		_tokenGroupTransform.gameObject.SetActive(!gaugeVisuals);
		_tokenSummaryTransform.gameObject.SetActive(!gaugeVisuals);
		if (gaugeVisuals)
		{
			_gaugeBackgroundImage.sprite = gaugeSprite;
			_gaugeTokenImage.sprite = tokenSprite;
			return;
		}
		CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(characterModel);
		_activeTokenSprite = characterConfigUI.ActiveToken;
		_inactiveTokenSprite = characterConfigUI.InactiveToken;
		int num = GetMaxTokens(playerActor);
		foreach (Image tokenImage in _tokenImages)
		{
			tokenImage.gameObject.SetActive(num > 0);
			num--;
		}
		SetTokenValue(currentTokens);
	}

	private void SetTokenValue(int value)
	{
		bool flag = value > _tokenImages.Count;
		_tokenSummaryTransform.gameObject.SetActive(flag);
		_tokenGroupTransform.gameObject.SetActive(!flag);
		if (flag)
		{
			_tokenSummaryText.text = value.ToString();
			_tokenSummaryImage.sprite = _activeTokenSprite;
			return;
		}
		foreach (Image tokenImage in _tokenImages)
		{
			tokenImage.sprite = ((value > 0) ? _activeTokenSprite : _inactiveTokenSprite);
			value--;
		}
	}

	private static int GetCurrentTokens(CPlayerActor playerActor)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			return playerActor.CharacterClass.CharacterModel switch
			{
				ECharacter.Blinkblade => playerActor.GetTimeTokens(), 
				ECharacter.Geminate => GetGeminateFormIndex(playerActor), 
				ECharacter.Deathwalker => ScenarioManager.CurrentScenarioState.ShadowProps.Count((CObjectProp x) => x.Owner == playerActor), 
				ECharacter.Deepwraith => playerActor.GetTrophyTokens(), 
				ECharacter.Shattersong => playerActor.GetResonanceTokens(), 
				ECharacter.Metalmosaic => playerActor.Pressure switch
				{
					CPlayerActor.EPressure.None => 0, 
					CPlayerActor.EPressure.Low => 1, 
					CPlayerActor.EPressure.Regular => 2, 
					CPlayerActor.EPressure.High => 3, 
					CPlayerActor.EPressure.Over => 4, 
					_ => throw new ArgumentOutOfRangeException(), 
				}, 
				ECharacter.Pyroclast => ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Count((CObjectProp prop) => prop is CObjectHazardousTerrain cObjectHazardousTerrain && cObjectHazardousTerrain.Owner == playerActor), 
				ECharacter.Crashingtide => CustomAbility.GetTideActiveBonuses(playerActor).Count, 
				ECharacter.Infuser => CustomAbility.GetNumberOfActiveInfusions(playerActor), 
				_ => 0, 
			};
		}
	}

	private static int GetGeminateFormIndex(CPlayerActor playerActor)
	{
		return ((playerActor.Form == CPlayerActor.EForm.None) ? playerActor.PendingForm : playerActor.Form) switch
		{
			CPlayerActor.EForm.None => 0, 
			CPlayerActor.EForm.Choose => 0, 
			CPlayerActor.EForm.Melee => 1, 
			CPlayerActor.EForm.Range => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static int GetMaxTokens(CPlayerActor playerActor)
	{
		return playerActor.CharacterClass.CharacterModel switch
		{
			ECharacter.Blinkblade => 5, 
			ECharacter.Deepwraith => 5, 
			ECharacter.Shattersong => 5, 
			ECharacter.Deathwalker => 5, 
			ECharacter.Metalmosaic => 4, 
			ECharacter.Pyroclast => 6, 
			_ => 0, 
		};
	}
}
