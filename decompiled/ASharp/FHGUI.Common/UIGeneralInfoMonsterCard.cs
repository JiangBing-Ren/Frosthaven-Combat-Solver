using System;
using System.Linq;
using FHGUI.Common.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoMonsterCard : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoMonsterCard.MonsterCardGeneralInfoData>, IUIGeneralInfo
{
	public struct MonsterCardGeneralInfoData : IGeneralInfoData
	{
		public MonsterCardDTO Card;

		public MonsterSimpleDTO? Monster;

		public Func<MonsterCardUIContext> GetMonsterCardContext;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private RectTransform _cardPivot;

	[SerializeField]
	private UIGeneralInfoSection _monsterContainer;

	[SerializeField]
	private UIGeneralInfoSection _glossary;

	private MonsterRoundCardUI _card;

	private MonsterCardGeneralInfoData _data;

	public bool Show(MonsterCardGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = _data.Card.Name;
		_monsterContainer.Label.Text.text = _data.Monster?.Name ?? string.Empty;
		_monsterContainer.Fill(_data.Monster.HasValue, (UIBaseStatsView view) =>
		{
			view.Init(_data.Monster.Value.Stats);
		});
		_card = ObjectPool.SpawnCard(data.Card.ID, ObjectPool.ECardType.Monster, _cardPivot, resetLocalScale: true, resetToMiddle: true).GetComponent<MonsterRoundCardUI>();
		_card.transform.localPosition = Vector3.zero;
		_card.Init(_data.GetMonsterCardContext());
		BuildGlossary();
		return true;
	}

	private void BuildGlossary()
	{
		UIGlossaryData[] glossaryEntries = LocalizationGlossaryUtils.GetGlossaryContents(_card.gameObject).ToArray();
		_glossary.Fill(show: true, (UIGlossaryContainer container) =>
		{
			container.Init(null, glossaryEntries);
		});
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (_card != null)
		{
			ObjectPool.RecycleCard(_card.CardID, ObjectPool.ECardType.Monster, _card.gameObject);
		}
	}
}
