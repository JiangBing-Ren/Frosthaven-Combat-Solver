using System;
using System.Collections.Generic;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UnlockCharacterModule : FHMapViewModule
{
	public struct Params
	{
		public PartySlotDTO CharacterSlotData;

		public ClassInfoDTO ClassData;

		public List<CAbilityCard> Cards;

		public Action OnDone;
	}

	[Header("Visuals")]
	[SerializeField]
	private TextLocalizedListener m_HeroClassName;

	[SerializeField]
	private Image m_ClassColor;

	[SerializeField]
	private Image m_ClassIcon;

	[SerializeField]
	private Image m_BigClassIcon;

	[SerializeField]
	private Image m_HeaderColor;

	[SerializeField]
	private Image m_HeroIllustration;

	[SerializeField]
	private TextLocalizedListener m_HeroBio;

	[SerializeField]
	private TextLocalizedListener m_HeroMechanics;

	[SerializeField]
	private UIAbilityCardContainer m_AbilityCard;

	[SerializeField]
	private FHButton m_CloseButton;

	[SerializeField]
	private ScrollRect m_ScrollRect;

	[SerializeField]
	private FHTabsGroup m_Tabs;

	private void Awake()
	{
		m_Tabs.TabOpened.AddListener(ResetScrollPosition);
	}

	public void Init(Params @params)
	{
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(@params.CharacterSlotData.Character);
		Sprite iconClassGold = UIInfoTools.Instance.GetCharacterConfigUI(@params.CharacterSlotData.Character.ToString()).iconClassGold;
		Sprite iconClassGold2 = UIInfoTools.Instance.GetCharacterConfigUI(@params.CharacterSlotData.Character.ToString()).iconClassGold;
		if ((bool)m_ClassColor)
		{
			m_ClassColor.color = characterColor;
		}
		if ((bool)m_ClassIcon)
		{
			m_ClassIcon.sprite = iconClassGold;
		}
		if ((bool)m_BigClassIcon)
		{
			m_BigClassIcon.sprite = iconClassGold2;
		}
		if ((bool)m_HeroClassName)
		{
			m_HeroClassName.SetTextKey(@params.ClassData.ClassNameFull);
		}
		if ((bool)m_HeaderColor)
		{
			m_HeaderColor.color = characterColor;
		}
		if ((bool)m_HeroIllustration)
		{
			m_HeroIllustration.sprite = @params.CharacterSlotData.InfoIllustration;
		}
		if ((bool)m_HeroBio)
		{
			m_HeroBio.SetTextKey(@params.ClassData.Description);
		}
		if ((bool)m_HeroMechanics)
		{
			m_HeroMechanics.SetTextKey(@params.ClassData.MechanicsLocKey);
		}
		try
		{
			if ((bool)m_AbilityCard)
			{
				m_AbilityCard.Init(@params.Cards, null);
			}
		}
		catch
		{
		}
		m_CloseButton.onClick.RemoveAllListeners();
		m_CloseButton.onClick.AddListener(() =>
		{
			@params.OnDone?.Invoke();
		});
	}

	private void ResetScrollPosition(Button arg0)
	{
		m_ScrollRect.verticalNormalizedPosition = 1f;
	}
}
