using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH080 : ScenarioScript
{
	public static List<Point> cabinetLocations = new List<Point>
	{
		new Point(7, 11),
		new Point(2, 20),
		new Point(7, 19),
		new Point(6, 3)
	};

	public static Dictionary<string, CObjectActor> cabinets = new Dictionary<string, CObjectActor>();

	private CEnemyActor boss;

	private CScenarioModifier scenarioAbilityHolderBossImmobilizeBuff;

	private CScenarioModifier scenarioAbilityHolderBossShieldBuff;

	protected override bool Initialize()
	{
		boss = ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList()[0];
		scenarioAbilityHolderBossImmobilizeBuff = GetScenarioModifier("scenarioAbilityHolderBossImmobilizeBuff");
		scenarioAbilityHolderBossShieldBuff = GetScenarioModifier("scenarioAbilityHolderBossShieldBuff");
		GetScenarioModifier("LocalizationD1").SetHiddenState(ScenarioScript.PartySize != 2);
		GetScenarioModifier("LocalizationD2").SetHiddenState(ScenarioScript.PartySize != 3);
		GetScenarioModifier("LocalizationD3").SetHiddenState(ScenarioScript.PartySize != 4);
		InitializeCabinets();
		return base.Initialize();
	}

	private void InitializeCabinets()
	{
		cabinets.Clear();
		char c = 'a';
		foreach (Point cabinetLocation in cabinetLocations)
		{
			CObjectActor cObjectActor = ScenarioManager.Tiles[cabinetLocation.X, cabinetLocation.Y].m_Props.Find((CObjectProp p) => p.RuntimeAttachedActor != null && !p.RuntimeAttachedActor.IsDead && p.RuntimeAttachedActor.Type == CActor.EType.Enemy)?.RuntimeAttachedActor;
			if (cObjectActor != null)
			{
				cabinets[c.ToString()] = cObjectActor;
			}
			c = (char)(c + 1);
		}
	}

	protected override void TickDeathInternal()
	{
		if (!(_tickActor is CObjectActor))
		{
			return;
		}
		string key = "";
		foreach (KeyValuePair<string, CObjectActor> cabinet in cabinets)
		{
			if (cabinet.Value == _tickActor)
			{
				RemoveBossBonus(cabinet.Key[0]);
				key = cabinet.Key;
				break;
			}
		}
		cabinets.Remove(key);
		base.TickDeathInternal();
	}

	private void RemoveBossBonus(char letter)
	{
		switch (letter)
		{
		case 'a':
			RemoveScenarioAbilityFromBoss(scenarioAbilityHolderBossImmobilizeBuff);
			break;
		case 'b':
			RemoveScenarioAbilityFromBoss(scenarioAbilityHolderBossShieldBuff);
			break;
		}
	}

	private void RemoveScenarioAbilityFromBoss(CScenarioModifier holder)
	{
		for (int num = holder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = holder.ActiveBonuses[num];
			if (cActiveBonus.Actor == boss)
			{
				cActiveBonus.Finish();
			}
		}
	}

	public static bool IsCabinetDead(string cabinetLetterStr)
	{
		if (string.IsNullOrEmpty(cabinetLetterStr) || cabinetLetterStr.Length > 1)
		{
			return false;
		}
		char c = cabinetLetterStr[0];
		if (c < 'a' || c > 'd')
		{
			return false;
		}
		CObjectActor value;
		return !cabinets.TryGetValue(cabinetLetterStr, out value);
	}
}
