using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class StrategyFavorMonsterBannerSlot : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public struct MonsterSlotData : IEquatable<MonsterSlotData>
	{
		public string MonsterID;

		public Sprite Portrait;

		public MonsterCardUIContext[] Cards;

		public int SelectedCards => Cards.Count((MonsterCardUIContext c) => c.CardPile == CardPileType.Discarded);

		public bool Equals(MonsterSlotData other)
		{
			if (MonsterID != other.MonsterID)
			{
				return false;
			}
			if (Cards.Length != other.Cards.Length)
			{
				return false;
			}
			for (int i = 0; i < Cards.Length; i++)
			{
				MonsterCardUIContext monsterCardUIContext = Cards[i];
				MonsterCardUIContext monsterCardUIContext2 = other.Cards[i];
				if (monsterCardUIContext.CardYML.ID != monsterCardUIContext2.CardYML.ID || monsterCardUIContext.CardPile != monsterCardUIContext2.CardPile)
				{
					return false;
				}
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is MonsterSlotData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(MonsterID, Cards);
		}

		public static bool operator ==(MonsterSlotData left, MonsterSlotData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MonsterSlotData left, MonsterSlotData right)
		{
			return !left.Equals(right);
		}
	}

	[SerializeField]
	private Image _portrait;

	[SerializeField]
	private GameObject[] _selectedElements;

	[SerializeField]
	private GameObject _selectedCardIndicator;

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private Vector3 _selectedScale = new Vector3(1.2f, 1.2f, 1f);

	private Action<StrategyFavorMonsterBannerSlot> _onClick;

	public MonsterSlotData? Data { get; private set; }

	public string MonsterID => Data?.MonsterID;

	private void Awake()
	{
	}

	private void OnDisable()
	{
		Data = null;
	}

	public void Init(MonsterSlotData monsterData, Action<StrategyFavorMonsterBannerSlot> onMonsterClick)
	{
		Data = monsterData;
		_onClick = onMonsterClick;
		_selectedCardIndicator.SetActive(monsterData.SelectedCards > 0);
		_portrait.sprite = monsterData.Portrait;
		Deselect();
	}

	public void Select()
	{
		GameObject[] selectedElements = _selectedElements;
		for (int i = 0; i < selectedElements.Length; i++)
		{
			selectedElements[i].SetActive(value: true);
		}
		transform.localScale = _selectedScale;
	}

	public void Deselect()
	{
		GameObject[] selectedElements = _selectedElements;
		for (int i = 0; i < selectedElements.Length; i++)
		{
			selectedElements[i].SetActive(value: false);
		}
		transform.localScale = Vector3.one;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Data.HasValue)
		{
			_onClick?.Invoke(this);
		}
	}
}
