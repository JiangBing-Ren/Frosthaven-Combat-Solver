using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Elements;
using FHGUI.Modules;
using UnityEngine;

namespace FHGUI.Modals;

public class StrategyFavorModal : FHMapViewModule
{
	public class Params : IEquatable<Params>, ICloneable
	{
		public int CurrentPoints;

		public uint MaxPoints;

		public IList<StrategyFavorMonsterBannerSlot.MonsterSlotData> Monsters;

		public bool Equals(Params other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if ((object)this == other)
			{
				return true;
			}
			if (CurrentPoints == other.CurrentPoints && MaxPoints == other.MaxPoints)
			{
				return Monsters.SequenceEqual(other.Monsters);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Params)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(CurrentPoints, MaxPoints, Monsters);
		}

		public object Clone()
		{
			List<StrategyFavorMonsterBannerSlot.MonsterSlotData> monsters = new List<StrategyFavorMonsterBannerSlot.MonsterSlotData>(Monsters.Select((StrategyFavorMonsterBannerSlot.MonsterSlotData m) => new StrategyFavorMonsterBannerSlot.MonsterSlotData
			{
				MonsterID = m.MonsterID,
				Portrait = m.Portrait,
				Cards = m.Cards.Select((MonsterCardUIContext c) => new MonsterCardUIContext
				{
					CardYML = c.CardYML,
					CardPile = c.CardPile,
					EnemyForBaseStats = c.EnemyForBaseStats,
					FallbackMonsterClass = c.FallbackMonsterClass,
					ShowFaction = c.ShowFaction
				}).ToArray()
			}));
			return new Params
			{
				CurrentPoints = CurrentPoints,
				MaxPoints = MaxPoints,
				Monsters = monsters
			};
		}

		public static bool operator ==(Params left, Params right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(Params left, Params right)
		{
			return !object.Equals(left, right);
		}
	}

	[SerializeField]
	private FHStacksCounter _FavorPoints;

	[SerializeField]
	private FHButton _confirmButton;

	[SerializeField]
	private StategyFavorMonsterContainer _monsterContainer;

	[SerializeField]
	private UIMonsterMiniCardContainer _monsterCardContainer;

	private Params _data;

	private Action _onClose;

	private Action _onConfirmAction;

	private Action<string, int> _onMonsterCardClick;

	private void Awake()
	{
		_confirmButton.onClick.AddListener(Confirm);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_data = null;
		_onClose = null;
		_onConfirmAction = null;
		_onMonsterCardClick = null;
	}

	public void Init(Params initParams, Action onConfirm, Action<string, int> onMonsterCardClick, Action onClose, bool canConfirm)
	{
		_data = initParams;
		_onClose = onClose;
		_onConfirmAction = onConfirm;
		_onMonsterCardClick = onMonsterCardClick;
		_FavorPoints.Init(initParams.CurrentPoints, (int)initParams.MaxPoints);
		_monsterContainer.Init(OnMonsterBannerClick, initParams.Monsters);
		_monsterContainer.SetSelected(_monsterContainer.SelectedIndex);
		_confirmButton.interactable = canConfirm;
		ShowMonsterCards(initParams.Monsters.ElementAt((_monsterContainer.SelectedIndex >= 0) ? _monsterContainer.SelectedIndex : 0).MonsterID);
	}

	private void OnMonsterCardClick(MonsterCardUIContext card)
	{
		_onMonsterCardClick?.Invoke(card.MonsterClass.ID, card.CardYML.ID);
	}

	private void OnMonsterBannerClick(string monsterID)
	{
		ShowMonsterCards(monsterID);
	}

	private void ShowMonsterCards(string monsterID)
	{
		_monsterCardContainer.Init(GetMonsterCardEvents(monsterID), _data.Monsters.First((StrategyFavorMonsterBannerSlot.MonsterSlotData m) => m.MonsterID == monsterID).Cards);
	}

	private UIMonsterCardInteractionEvents GetMonsterCardEvents(string monsterID)
	{
		return new UIMonsterCardInteractionEvents
		{
			OnClick = OnMonsterCardClick
		};
	}

	private void Confirm()
	{
		_onConfirmAction?.Invoke();
	}

	public void Close()
	{
		_onClose?.Invoke();
	}
}
