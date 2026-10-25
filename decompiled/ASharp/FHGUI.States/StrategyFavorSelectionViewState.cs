using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Elements;
using FHGUI.Modals;
using FHGUI.Services;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class StrategyFavorSelectionViewState : FHMapViewState
{
	private StrategyFavorModal.Params _originalParams;

	private StrategyFavorModal.Params _tempParams;

	public override StateCategory Category => StateCategory.Briefing;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private StrategyFavorModal StrategyFavorModal => View.Modules.BriefingModules.StrategyFavorModal;

	private BattleBriefingViewService _briefingViewService => View.GetService<BattleBriefingViewService>();

	private DefRepository _defRepository => View.GameController.DefRepository;

	public override void OnShow()
	{
		_originalParams = GetModalDataFromGameState();
		LoadPayload();
		UpdateModule();
	}

	private void UpdateModule()
	{
		StrategyFavorModal.Init(_tempParams, ConfirmChooseMonstersAndCards, OnMonsterCardClick, OnModalClose, CanConfirmState());
	}

	public override void OnHide()
	{
		ClearPayload();
	}

	private StrategyFavorModal.Params GetModalDataFromGameState()
	{
		uint maxPotentialFavorPoints = _briefingViewService.GetMaxPotentialFavorPoints();
		return new StrategyFavorModal.Params
		{
			Monsters = GetValidMonstersAndCards().ToArray(),
			CurrentPoints = (int)(maxPotentialFavorPoints - _briefingViewService.GetFavorDifficulty()),
			MaxPoints = maxPotentialFavorPoints
		};
	}

	private IEnumerable<StrategyFavorMonsterBannerSlot.MonsterSlotData> GetValidMonstersAndCards()
	{
		return from monster in _briefingViewService.GetAllScenarioMonsterTypesForStrategyFavor()
			select monster.ID.ID into id
			select new StrategyFavorMonsterBannerSlot.MonsterSlotData
			{
				MonsterID = id,
				Portrait = UIInfoTools.Instance.GetMonsterConfig(id).Portrait,
				Cards = GetMonsterCardsFromGameState(id).ToArray()
			};
	}

	private IEnumerable<MonsterCardUIContext> GetMonsterCardsFromGameState(string monsterID)
	{
		MonsterID monsterID2 = new MonsterID(monsterID);
		MonsterDef monster = _defRepository.Monsters[monsterID2];
		CMonsterClass monsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monster.ID.ID);
		MonsterCardID[] selectedCards = _briefingViewService.GetRemovedCardsForStrategyFavor(monsterID).ToArray();
		foreach (MonsterCardID cardID in _briefingViewService.GetAvailableCardsForStrategyFavor(monsterID2))
		{
			yield return new MonsterCardUIContext
			{
				CardYML = UIGeneralInfoUtils.GetMonsterCardYML((int)cardID.ID),
				EnemyForBaseStats = null,
				FallbackMonsterClass = monsterClass,
				CardPile = ((!selectedCards.Any((MonsterCardID cc) => cc.ID == cardID.ID)) ? CardPileType.None : CardPileType.Discarded),
				ShowFaction = false,
				OverrideLevel = GameController.ScenarioController.GetScenarioLevel()
			};
		}
	}

	protected override void ClearPayload()
	{
		_originalParams = null;
		_tempParams = null;
	}

	protected override void LoadPayload()
	{
		if ((object)_tempParams == null)
		{
			_tempParams = (StrategyFavorModal.Params)_originalParams.Clone();
		}
	}

	private IEnumerable<MonsterCardUIContext> GetMonsterCardsFromTempState(string monsterID)
	{
		return _tempParams.Monsters.FirstOrDefault((StrategyFavorMonsterBannerSlot.MonsterSlotData m) => m.MonsterID == monsterID).Cards;
	}

	private MonsterCardUIContext GetMonsterCardFromTempState(string monsterID, int cardID)
	{
		return GetMonsterCardsFromTempState(monsterID).FirstOrDefault((MonsterCardUIContext c) => c.CardYML.ID == cardID);
	}

	private void OnMonsterCardClick(string monsterID, int cardID)
	{
		if (HasRemovedCard(monsterID, cardID))
		{
			if (Action_ResetRemovedCardForMonster(monsterID, cardID))
			{
				UpdateModule();
			}
		}
		else if (Action_RemoveCardForMonster(monsterID, cardID))
		{
			UpdateModule();
		}
	}

	private bool CanConfirmState()
	{
		return HasChangedState();
	}

	private bool HasChangedState()
	{
		return _originalParams != _tempParams;
	}

	private void ConfirmChooseMonstersAndCards()
	{
		if (!CanConfirmState())
		{
			return;
		}
		List<(string, int)> list = new List<(string, int)>(5);
		List<(string, int)> list2 = new List<(string, int)>(5);
		for (int i = 0; i < _tempParams.Monsters.Count; i++)
		{
			StrategyFavorMonsterBannerSlot.MonsterSlotData monsterSlotData = _originalParams.Monsters[i];
			StrategyFavorMonsterBannerSlot.MonsterSlotData monsterSlotData2 = _tempParams.Monsters[i];
			for (int j = 0; j < monsterSlotData2.Cards.Length; j++)
			{
				MonsterCardUIContext monsterCardUIContext = monsterSlotData.Cards[j];
				MonsterCardUIContext monsterCardUIContext2 = monsterSlotData2.Cards[j];
				if (monsterCardUIContext.CardPile != monsterCardUIContext2.CardPile)
				{
					if (monsterCardUIContext.CardPile == CardPileType.Discarded && monsterCardUIContext2.CardPile == CardPileType.None)
					{
						list2.Add((monsterSlotData2.MonsterID, monsterCardUIContext2.CardYML.ID));
					}
					else if (monsterCardUIContext.CardPile == CardPileType.None && monsterCardUIContext2.CardPile == CardPileType.Discarded)
					{
						list.Add((monsterSlotData2.MonsterID, monsterCardUIContext2.CardYML.ID));
					}
				}
			}
		}
		_briefingViewService.StoreStrategyFavorState(list, list2);
		PopThisState();
	}

	private void OnModalClose()
	{
		if (HasChangedState())
		{
			string translation = LocalizationManager.GetTranslation("GUI_HANDMANAGEMENT_WARNING_UNSAVEDCHANGES");
			string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRM");
			Singleton<UIConfirmationBoxManager>.InstanceFast.ShowGenericConfirmation(translation2, translation, PopThisState);
		}
		else
		{
			PopThisState();
		}
	}

	private bool HasRemovedCard(string monsterID, int cardID)
	{
		return GetMonsterCardFromTempState(monsterID, cardID).CardPile == CardPileType.Discarded;
	}

	private bool CanRemoveCardForMonster(string monsterID, int cardID)
	{
		if (_tempParams.CurrentPoints <= 0)
		{
			return false;
		}
		return _tempParams.Monsters.First((StrategyFavorMonsterBannerSlot.MonsterSlotData m) => m.MonsterID == monsterID).SelectedCards == 0;
	}

	private bool Action_RemoveCardForMonster(string monsterID, int cardID)
	{
		if (!CanRemoveCardForMonster(monsterID, cardID))
		{
			return false;
		}
		GetMonsterCardFromTempState(monsterID, cardID).CardPile = CardPileType.Discarded;
		_tempParams.CurrentPoints--;
		return true;
	}

	private bool Action_ResetRemovedCardForMonster(string monsterID, int cardID)
	{
		if (!HasRemovedCard(monsterID, cardID))
		{
			return false;
		}
		GetMonsterCardFromTempState(monsterID, cardID).CardPile = CardPileType.None;
		_tempParams.CurrentPoints++;
		return true;
	}
}
