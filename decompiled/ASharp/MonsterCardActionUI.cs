using System.Collections.Generic;
using System.Linq;
using FHGUI;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class MonsterCardActionUI : MonoBehaviour
{
	[SerializeField]
	private int _cardID;

	[SerializeField]
	private RectTransform _container;

	public bool IsGenerated;

	public List<LayoutRow> LayoutRows;

	private CEnemyActor _actor;

	private readonly Dictionary<CAbility.EAbilityType, int> _specialBaseStats = new Dictionary<CAbility.EAbilityType, int>();

	public int CardID => _cardID;

	public RectTransform Container => _container;

	public void GenerateLayout(MonsterCardUIContext context)
	{
		GenerateLayout(context.CardYML, context.EnemyForBaseStats, context.FallbackMonsterClass, context.OverrideLevel);
	}

	public void GenerateLayout(MonsterCardYMLData cardYML, CEnemyActor enemyForBaseStats = null, CMonsterClass fallbackMonsterClass = null, int? overrideLevel = null)
	{
		if (IsGenerated && _actor == enemyForBaseStats)
		{
			return;
		}
		foreach (Transform item in _container)
		{
			Object.Destroy(item.gameObject);
		}
		_actor = enemyForBaseStats;
		IsGenerated = true;
		CreateLayout createLayout = new CreateLayout(cardYML, _container.rect, enemyForBaseStats);
		createLayout.FullLayout.transform.SetParent(_container);
		createLayout.FullLayout.transform.localScale = Vector3.one;
		createLayout.FullLayout.transform.localPosition = Vector3.zero;
		LayoutRows = createLayout.LayoutRows;
		RectTransform obj = createLayout.FullLayout.transform as RectTransform;
		obj.anchoredPosition = Vector2.zero;
		obj.sizeDelta = Vector2.zero;
		if (enemyForBaseStats != null || fallbackMonsterClass != null)
		{
			UpdateLayoutRows(cardYML, enemyForBaseStats, fallbackMonsterClass, overrideLevel);
		}
	}

	private void UpdateLayoutRows(MonsterCardYMLData roundCard, CEnemyActor sourceEnemy, CMonsterClass fallbackMonsterClass, int? overrideLevel = null)
	{
		if (fallbackMonsterClass == null)
		{
			fallbackMonsterClass = sourceEnemy.MonsterClass;
		}
		_ = sourceEnemy?.MaxHealth;
		int num = sourceEnemy?.MonsterClass.Move ?? fallbackMonsterClass.Move;
		int num2 = sourceEnemy?.AttackStrengthForUI ?? fallbackMonsterClass.Attack;
		int num3 = sourceEnemy?.MonsterClass.Range ?? fallbackMonsterClass.Range;
		int num4 = sourceEnemy?.MonsterClass.Target ?? fallbackMonsterClass.Target;
		List<CAbility> list = CAbilityCard.GetActionAbilities(roundCard.CardAction).ToList();
		_specialBaseStats.Clear();
		foreach (CAbility item in list)
		{
			BaseStats baseStats = (overrideLevel.HasValue ? fallbackMonsterClass.GetMonsterStatsData(overrideLevel.Value) : fallbackMonsterClass.CurrentMonsterStat);
			if (item.UseSpecialBaseStat && baseStats.SpecialBaseStats.TryGetValue(item.AbilityType, out var value))
			{
				_specialBaseStats.Add(item.AbilityType, value);
			}
		}
		foreach (LayoutRow row in LayoutRows.Where((LayoutRow layoutRow) => layoutRow.BaseValue != int.MaxValue && !layoutRow.IsBuff && layoutRow.Entry != null && layoutRow.Entry.Name != ""))
		{
			if (_specialBaseStats.Keys.Any((CAbility.EAbilityType a) => a.ToString() == row.Entry.Name))
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value = row.BaseValue + _specialBaseStats.Single((KeyValuePair<CAbility.EAbilityType, int> s) => s.Key.ToString() == row.Entry.Name).Value;
			}
			else if (row.Entry.Name == CAbility.EAbilityType.Attack.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value = row.BaseValue + ((!row.FromControlAbility) ? num2 : 0);
				AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Strength, list);
			}
			else if (row.Entry.Name == CAbility.EAbilityType.Move.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value = row.BaseValue + ((!row.FromControlAbility) ? num : 0);
				AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Strength, list);
			}
			else if (row.Entry.Name == "Range")
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				CAbility cAbility = list.SingleOrDefault((CAbility x) => x.AbilityType == row.AbilityType && x.Name == row.AbilityName);
				if (cAbility != null && !row.FromBaseStats)
				{
					row.Value = row.BaseValue;
					row.Value += ((!cAbility.RangeIsBase) ? ((!row.FromControlAbility) ? num3 : 0) : 0);
					AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Range, list);
				}
				else if (row.BaseValue < 1)
				{
					Log.GUIError("Unable to lookup range for ability " + row.AbilityName);
				}
			}
			else if (row.Entry.Name == CAbility.EAbilityStatType.NumberOfTargets.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				CAbility cAbility2 = list.SingleOrDefault((CAbility x) => x.AbilityType == row.AbilityType && x.Name == row.AbilityName);
				if (cAbility2 != null && !row.FromBaseStats)
				{
					row.Value = row.BaseValue;
					row.Value += ((!cAbility2.TargetIsBase) ? ((!row.FromControlAbility) ? num4 : 0) : 0);
					AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.NumberOfTargets, list);
				}
				else if (row.BaseValue < 1)
				{
					Log.GUIError("Unable to lookup number of targets for ability " + row.AbilityName);
				}
			}
			row.UpdateText(row.Bold, row.PlusX);
		}
	}

	private void AddStatIsXValueIfApplicable(LayoutRow row, CAbility.EAbilityStatType abilityStatType, IList<CAbility> abilities)
	{
		CAbility cAbility = abilities.SingleOrDefault((CAbility x) => x.Name == row.AbilityName);
		if (cAbility == null)
		{
			return;
		}
		AbilityData.StatIsBasedOnXData statIsBasedOnXData = cAbility.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == abilityStatType);
		if (statIsBasedOnXData != null)
		{
			if (statIsBasedOnXData.AddTo)
			{
				row.Value += cAbility.GetStatIsBasedOnXValue(cAbility.TargetingActor, statIsBasedOnXData, cAbility.AbilityFilter);
			}
			else
			{
				row.Value = cAbility.GetStatIsBasedOnXValue(cAbility.TargetingActor, statIsBasedOnXData, cAbility.AbilityFilter);
			}
		}
	}

	public void Clean()
	{
		_actor = null;
	}

	private void OnDestroy()
	{
		_actor = null;
	}
}
