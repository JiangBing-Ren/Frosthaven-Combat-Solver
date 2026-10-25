using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoCard : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoCard.UIGeneralInfoCardData>, IUIGeneralInfo
{
	public struct UIGeneralInfoCardData : IGeneralInfoData
	{
		public AbilityCardYMLData Card;

		public CBaseCard.ECardPile PileType;

		public CActiveBonus[] ActiveBonuses;

		public CEnhancement[] Enhancements;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _typeDescription;

	[Header("Left side")]
	[SerializeField]
	private RectTransform _cardContainer;

	[Header("Right side")]
	[SerializeField]
	private UIGeneralInfoSection _glossary;

	[SerializeField]
	private XPTrackerContainer _xpTracker;

	private AbilityCardUI _card;

	private UIGeneralInfoCardData _data;

	private Coroutine _glossaryCrt;

	private List<int> _xpTrackerList = new List<int>(10);

	public bool Show(UIGeneralInfoCardData data, params object[] parameters)
	{
		Cleanup();
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = LocalizationManager.GetTranslation(data.Card.Name);
		_card = ObjectPool.SpawnCard(data.Card.ID, ObjectPool.ECardType.Ability, _cardContainer, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
		RectTransform rectTransform = (RectTransform)_card.transform;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one * 0.8f;
		CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == data.Card.ID);
		_card.Init(abilityCard, disableEventDetection: true, data.Enhancements);
		_card.SetMode(CardHandMode.Preview, null);
		ShowXPTrack();
		_glossaryCrt = StartCoroutine(BuildGlossary());
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowOpened, null, "GeneralInfoCard"));
		return true;
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		Cleanup();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowClosed, null, "GeneralInfoCard"));
	}

	private IEnumerator BuildGlossary()
	{
		yield return null;
		UIGlossaryData[] glossaryEntries = LocalizationGlossaryUtils.GetGlossaryContents(_card.gameObject).ToArray();
		_glossary.Fill(show: true, (UIGlossaryContainer container) =>
		{
			container.Init(null, glossaryEntries);
		});
	}

	private void ShowXPTrack()
	{
		_xpTrackerList.Clear();
		CActiveBonus cActiveBonus = _data.ActiveBonuses?.FirstOrDefault((CActiveBonus bonus) => bonus.HasTracker);
		int activeIndex = -1;
		if (cActiveBonus != null)
		{
			_xpTrackerList.AddRange(cActiveBonus.Layout.TrackerPattern);
			activeIndex = cActiveBonus.TrackerIndex;
		}
		else
		{
			List<int> list = _data.Card.TopActionFullLayout.ParentGroup.Collection.FirstOrDefault((CardLayoutGroup l) =>
			{
				List<int> xP = l.XP;
				return xP != null && xP.Count > 0;
			})?.XP ?? _data.Card.BottomActionFullLayout.ParentGroup.Collection.FirstOrDefault((CardLayoutGroup l) =>
			{
				List<int> xP = l.XP;
				return xP != null && xP.Count > 0;
			})?.XP;
			if (list != null)
			{
				_xpTrackerList.AddRange(list);
			}
		}
		_xpTracker.gameObject.SetActive(_xpTrackerList.Count > 0);
		_xpTracker.Init(_xpTrackerList, activeIndex);
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (_glossaryCrt != null)
		{
			StopCoroutine(_glossaryCrt);
		}
		_glossaryCrt = null;
		if (_card != null)
		{
			ObjectPool.RecycleCard(_card.CardID, ObjectPool.ECardType.Ability, _card.gameObject);
			_card = null;
		}
	}
}
