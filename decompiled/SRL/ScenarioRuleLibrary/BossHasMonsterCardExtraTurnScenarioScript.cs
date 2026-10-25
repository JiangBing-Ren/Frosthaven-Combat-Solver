using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public abstract class BossHasMonsterCardExtraTurnScenarioScript : ScenarioScript
{
	private CEnemyActor _boss;

	private CMonsterAbilityCard _bossCard;

	private CMonsterAbilityCard _monsterCard;

	public CMonsterClass BossClass;

	private CMonsterClass _monsterClass;

	private bool _bossIsFirst;

	private bool _monsterIsFirst = true;

	protected abstract string BossClassID { get; }

	protected abstract string BossCardsClassID { get; }

	protected abstract string MonsterClassID { get; }

	protected abstract string MonsterEliteClassID { get; }

	public CEnemyActor Boss => _boss;

	protected override bool Initialize()
	{
		_boss = ScenarioManager.Scenario.AllEnemies.Find((CEnemyActor enemy) => enemy.Class.ID == BossClassID);
		BossClass = MonsterClassManager.Find(BossCardsClassID);
		_monsterClass = MonsterClassManager.Find(MonsterClassID);
		return base.Initialize();
	}

	protected virtual void TickPhaseEndTurn()
	{
		if (_tickActor.Class.ID == BossClassID)
		{
			int num = ScenarioManager.Scenario.AllEnemies.FindAll((CEnemyActor enemy) => (enemy.Class.ID == MonsterClassID || enemy.Class.ID == MonsterEliteClassID) && !enemy.IsDead).Count();
			_boss.OverrideRoundAbilityCard = null;
			_boss.RecalcLockedInitiative();
			GameState.SortIntoInitiativeAndIDOrder();
			if (num == 0 && _monsterIsFirst)
			{
				_boss.OverrideRoundAbilityCard = _bossCard;
				_boss.RecalcLockedInitiative();
				_boss.PlayedThisRound = false;
				GameState.SortIntoInitiativeAndIDOrder();
				_monsterIsFirst = false;
			}
		}
		if (_tickActor.Class.ID == BossClassID && _bossIsFirst)
		{
			_boss.OverrideRoundAbilityCard = null;
			_boss.RecalcLockedInitiative();
			_boss.PlayedThisRound = false;
			GameState.SortIntoInitiativeAndIDOrder();
			_bossIsFirst = false;
		}
		if ((_tickActor.Class.ID == MonsterClassID || _tickActor.Class.ID == MonsterEliteClassID) && _monsterIsFirst && !_boss.IsDead)
		{
			List<CEnemyActor> source = ScenarioManager.Scenario.AllEnemies.FindAll((CEnemyActor enemy) => (enemy.Class.ID == MonsterClassID || enemy.Class.ID == MonsterEliteClassID) && !enemy.IsDead);
			int num2 = source.Count();
			int num3 = source.Where((CEnemyActor enemy) => enemy.PlayedThisRound).Count();
			if (num2 - num3 == 0)
			{
				_boss.OverrideRoundAbilityCard = _bossCard;
				_boss.RecalcLockedInitiative();
				_boss.PlayedThisRound = false;
				GameState.AddActorIntoInitiativeOrder(_boss);
				GameState.SortIntoInitiativeAndIDOrder();
				_monsterIsFirst = false;
			}
		}
	}

	protected virtual void TickPhaseStartRound()
	{
		if (_tickActor.Class.ID == BossClassID && !_boss.IsDead)
		{
			BossClass.DiscardRoundAbilityCard();
			BossClass.SelectRoundAbilityCard();
		}
	}

	protected virtual void TickAfterSelectMonsterClassAbilityCards()
	{
		if (_tickActor.Class.ID == BossClassID && !_boss.IsDead)
		{
			_bossCard = BossClass.RoundAbilityCard;
			_monsterCard = _tickActor.MonsterClass.RoundAbilityCard;
			_bossCard.ActionHasHappened = false;
			if (_monsterCard.Initiative < _bossCard.Initiative)
			{
				_monsterIsFirst = true;
				_bossIsFirst = false;
				_boss.OverrideRoundAbilityCard = null;
				_boss.RecalcLockedInitiative();
				_boss.PlayedThisRound = false;
				GameState.SortIntoInitiativeAndIDOrder();
			}
			else
			{
				_monsterIsFirst = false;
				_bossIsFirst = true;
				_boss.OverrideRoundAbilityCard = _bossCard;
				_boss.RecalcLockedInitiative();
				_boss.PlayedThisRound = false;
				GameState.SortIntoInitiativeAndIDOrder();
			}
		}
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_bossCard?.ID);
		string text2 = JsonConvert.SerializeObject(_monsterCard?.ID);
		string text3 = JsonConvert.SerializeObject(_boss?.OverrideRoundAbilityCard?.ID);
		string text4 = JsonConvert.SerializeObject(_bossIsFirst);
		string text5 = JsonConvert.SerializeObject(_monsterIsFirst);
		string text6 = JsonConvert.SerializeObject(new string[5] { text, text2, text3, text4, text5 });
		Trace("[BossHasMonsterCardExtraTurnScenarioScript]Serialize: " + text6 + "[-]", null, 133);
		return text6;
	}

	protected override void DeserializeInternal(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		string[] array = JsonConvert.DeserializeObject<string[]>(s);
		int? bossCardId = JsonConvert.DeserializeObject<int?>(array[0]);
		int? monsterCardId = JsonConvert.DeserializeObject<int?>(array[1]);
		int? bossOverrideRoundAbilityCard = JsonConvert.DeserializeObject<int?>(array[2]);
		_bossIsFirst = JsonConvert.DeserializeObject<bool>(array[3]);
		_monsterIsFirst = JsonConvert.DeserializeObject<bool>(array[4]);
		if (bossCardId.HasValue)
		{
			_bossCard = BossClass.AbilityCardsPool.First((CMonsterAbilityCard ac) => ac.ID == bossCardId);
		}
		if (monsterCardId.HasValue)
		{
			_monsterCard = _monsterClass.AbilityCardsPool.First((CMonsterAbilityCard ac) => ac.ID == monsterCardId);
		}
		if (bossOverrideRoundAbilityCard.HasValue)
		{
			_boss.OverrideRoundAbilityCard = BossClass.AbilityCardsPool.First((CMonsterAbilityCard ac) => ac.ID == bossOverrideRoundAbilityCard);
		}
	}

	public override Dictionary<string, List<CEnemyActor>> GetExtraInitiativeActors()
	{
		if (_boss.IsDead)
		{
			return null;
		}
		return new Dictionary<string, List<CEnemyActor>> { 
		{
			BossCardsClassID,
			new List<CEnemyActor> { _boss }
		} };
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.ActorGuid == _boss.ActorGuid)
		{
			_monsterClass.CardsPoolClass.RoundAbilityCard = _monsterCard;
		}
	}
}
