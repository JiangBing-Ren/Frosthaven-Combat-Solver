using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH104 : ScenarioScript
{
	private int seasonDamage = 1;

	private List<string> stringMosntersIDs = new List<string> { "ForestImpID", "ForestImpEliteID", "EarthDemonID", "EarthDemonEliteID" };

	private List<string> winterMosntersIDs = new List<string> { "SavvasIcestormID", "SavvasIcestormEliteID", "FrostDemonID", "FrostDemonEliteID" };

	private List<Point> chestPositions = new List<Point>
	{
		new Point(2, 2),
		new Point(9, 2)
	};

	private CScenarioModifier scenarioAbilityHolderElementEarth;

	private CScenarioModifier scenarioAbilityHolderElementIce;

	private List<CActor> actorsThatUsedEarth = new List<CActor>();

	private List<CActor> actorsThatUsedIce = new List<CActor>();

	protected override string SerializeInternal()
	{
		List<string> value = actorsThatUsedEarth.Select((CActor a) => a.ActorGuid).ToList();
		List<string> value2 = actorsThatUsedIce.Select((CActor a) => a.ActorGuid).ToList();
		string text = JsonConvert.SerializeObject(value);
		string text2 = JsonConvert.SerializeObject(value2);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("Serialize: " + text3, null, 33);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("Deserialize " + s, null, 39);
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		string[]? array = JsonConvert.DeserializeObject<string[]>(s);
		List<string> list = JsonConvert.DeserializeObject<List<string>>(array[0]);
		List<string> list2 = JsonConvert.DeserializeObject<List<string>>(array[1]);
		foreach (string guid in list)
		{
			CActor cActor = ScenarioManager.Scenario.AllActors.Find((CActor a) => a.ActorGuid == guid);
			if (cActor != null)
			{
				actorsThatUsedEarth.Add(cActor);
			}
			else
			{
				Log.Error("Didn't find actor with guid: " + guid);
			}
		}
		foreach (string guid2 in list2)
		{
			CActor cActor2 = ScenarioManager.Scenario.AllActors.Find((CActor a) => a.ActorGuid == guid2);
			if (cActor2 != null)
			{
				actorsThatUsedIce.Add(cActor2);
			}
			else
			{
				Log.Error("Didn't find actor with guid: " + guid2);
			}
		}
	}

	protected override bool Initialize()
	{
		scenarioAbilityHolderElementEarth = GetScenarioModifier("scenarioAbilityHolderElementEarth");
		scenarioAbilityHolderElementIce = GetScenarioModifier("scenarioAbilityHolderElementIce");
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 97);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseStartRound()
	{
		if (OncePerRound())
		{
			ResetElementBuffs();
		}
	}

	private void ResetElementBuffs()
	{
		actorsThatUsedEarth.Clear();
		actorsThatUsedIce.Clear();
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			ResetElementBuffs(allAliveActor);
		}
	}

	private void ResetElementBuffs(CActor actor)
	{
		RemoveElementBuffs(actor);
		TryAddElementBuffs(actor);
	}

	private void RemoveElementBuffs(CActor actor)
	{
		RemoveElementBuff(actor, ElementInfusionBoardManager.EElement.Earth);
		RemoveElementBuff(actor, ElementInfusionBoardManager.EElement.Ice);
	}

	private void RemoveElementBuff(CActor actor, ElementInfusionBoardManager.EElement element)
	{
		if (element == ElementInfusionBoardManager.EElement.Earth)
		{
			RemoveScenarioAbilityFromActor(actor, scenarioAbilityHolderElementEarth);
		}
		if (element == ElementInfusionBoardManager.EElement.Ice)
		{
			RemoveScenarioAbilityFromActor(actor, scenarioAbilityHolderElementIce);
		}
	}

	private void TryAddElementBuffs(CActor actor)
	{
		if (IsActorInSpring(actor) && !actorsThatUsedEarth.Contains(actor))
		{
			AddScenarioAbility(actor, scenarioAbilityHolderElementEarth);
		}
		if (IsActorInWinter(actor) && !actorsThatUsedIce.Contains(actor))
		{
			AddScenarioAbility(actor, scenarioAbilityHolderElementIce);
		}
	}

	private void AddScenarioAbility(CActor actor, CScenarioModifier holder)
	{
		CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
		cAbility.IsScenarioModifierAbility = true;
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(actor);
		actor.FindCardWithAbility(cAbility).AddActiveBonus(cAbility, actor, actor);
	}

	private bool HasScenarioAbility(CActor actor, CScenarioModifier holder)
	{
		for (int num = holder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			if (holder.ActiveBonuses[num].Actor == actor)
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveScenarioAbilityFromActor(CActor actor, CScenarioModifier holder)
	{
		for (int num = holder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = holder.ActiveBonuses[num];
			if (cActiveBonus.Actor == actor)
			{
				cActiveBonus.Finish();
			}
		}
	}

	private void TickPhaseEndRound()
	{
		DealDamageToMonsters();
	}

	private void DealDamageToMonsters()
	{
		if (_tickActor != null && _tickActor.Type == CActor.EType.Enemy && !_tickActor.IsDead)
		{
			if (stringMosntersIDs.Contains(_tickActor.MonsterClass.ID) && IsActorInWinter(_tickActor))
			{
				DealScenarioDamage(_tickActor, seasonDamage);
			}
			if (winterMosntersIDs.Contains(_tickActor.MonsterClass.ID) && IsActorInSpring(_tickActor))
			{
				DealScenarioDamage(_tickActor, seasonDamage);
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		RemoveElementBuffs(_tickActor);
		TryAddElementBuffs(_tickActor);
		base.TickRepositionInternal();
	}

	private bool IsActorInSpring(CActor actor)
	{
		return actor.ArrayIndex.X > 5;
	}

	private bool IsActorInWinter(CActor actor)
	{
		if (actor.ArrayIndex.X >= 5)
		{
			if (actor.ArrayIndex.X == 5)
			{
				return actor.ArrayIndex.Y % 2 == 0;
			}
			return false;
		}
		return true;
	}

	public override bool IsActorConsideringElementAsStrong(CActor actor, ElementInfusionBoardManager.EElement element)
	{
		if (actor == null)
		{
			return false;
		}
		return element switch
		{
			ElementInfusionBoardManager.EElement.Earth => HasScenarioAbility(actor, scenarioAbilityHolderElementEarth), 
			ElementInfusionBoardManager.EElement.Ice => HasScenarioAbility(actor, scenarioAbilityHolderElementIce), 
			_ => false, 
		};
	}

	public override bool OnConsumedElement(ElementInfusionBoardManager.EElement element, CActor consumingActor)
	{
		base.OnConsumedElement(element, consumingActor);
		if (element == ElementInfusionBoardManager.EElement.Earth)
		{
			actorsThatUsedEarth.Add(consumingActor);
		}
		if (element == ElementInfusionBoardManager.EElement.Ice)
		{
			actorsThatUsedIce.Add(consumingActor);
		}
		RemoveElementBuff(consumingActor, element);
		return true;
	}

	public override bool CanActorLoot(CObjectProp prop, CActor actor)
	{
		if (prop.ObjectType != ScenarioManager.ObjectImportType.GoalChest || actor.Type != CActor.EType.Player)
		{
			return true;
		}
		Point point = chestPositions.First((Point p) => p.X != prop.ArrayIndex.X || p.Y != prop.ArrayIndex.Y);
		CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
		if (cTile.LivingActor == null || cTile.LivingActor.Type != CActor.EType.Player)
		{
			return false;
		}
		return base.CanActorLoot(prop, actor);
	}
}
