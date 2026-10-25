using System;
using System.Collections.Generic;
using System.Linq;
using FH.Tools;

namespace ScenarioRuleLibrary;

public class ElementInfusionBoardManager
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EElement
	{
		Fire,
		Ice,
		Air,
		Earth,
		Light,
		Dark,
		Any
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EColumn
	{
		Inert,
		Waning,
		Strong
	}

	public static readonly EElement[] Elements = (EElement[])Enum.GetValues(typeof(EElement));

	private static EColumn[] s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];

	[StateDump(Skip = true)]
	private static EColumn[] s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];

	public static Dictionary<string, List<EElement>> s_EnemyConsumedElements = new Dictionary<string, List<EElement>>();

	public static Dictionary<string, List<EElement>> s_EnemyInfusedElements = new Dictionary<string, List<EElement>>();

	private static Dictionary<string, List<EElement>> s_ElementsToInfuse = new Dictionary<string, List<EElement>>();

	[StateDump(Skip = true)]
	private static Dictionary<string, List<EElement>> s_ElementsToInfuseSaved = new Dictionary<string, List<EElement>>();

	public static EColumn[] GetElementColumn => s_ElementColumn;

	public static void Reset()
	{
		EElement[] elements = Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			Consume(elements[i], null);
		}
		s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_EnemyConsumedElements.Clear();
		s_EnemyInfusedElements.Clear();
		s_ElementsToInfuse.Clear();
		s_ElementsToInfuseSaved.Clear();
	}

	public static void SaveState()
	{
		s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumn.CopyTo(s_ElementColumnSaved, 0);
		s_ElementsToInfuseSaved = new Dictionary<string, List<EElement>>(s_ElementsToInfuse);
	}

	public static void RestoreState()
	{
		s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumnSaved.CopyTo(s_ElementColumn, 0);
		s_ElementsToInfuse = new Dictionary<string, List<EElement>>(s_ElementsToInfuseSaved);
	}

	public static void SetElementColumn(EColumn[] elementColumn)
	{
		s_ElementColumn = elementColumn.ToArray();
	}

	public static void SetElementsToInfuse(Dictionary<string, List<EElement>> ElementsToInfuse)
	{
		s_ElementsToInfuse.Clear();
		if (ElementsToInfuse == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<EElement>> item in ElementsToInfuse)
		{
			s_ElementsToInfuse.Add(item.Key, item.Value.ToList());
		}
	}

	public static List<EElement> GetAvailableElements(CActor actor = null)
	{
		List<EElement> list = new List<EElement>();
		List<EElement> list2 = ((!(PhaseManager.CurrentPhase is CPhaseAction cPhaseAction) || (actor != null && actor.Class is CMonsterClass)) ? new List<EElement>() : cPhaseAction.GetReservedElements());
		EElement[] elements = Elements;
		foreach (EElement eElement in elements)
		{
			if ((actor == null || actor.CanConsume(eElement)) && eElement != EElement.Any && ElementColumn(eElement, actor) != EColumn.Inert && !list2.Contains(eElement))
			{
				list.Add(eElement);
			}
		}
		if (actor is CEnemyActor actor2)
		{
			List<EElement> enemyConsumedElements = GetEnemyConsumedElements(actor2);
			if (enemyConsumedElements != null)
			{
				foreach (EElement item in enemyConsumedElements)
				{
					if (!list.Contains(item) && !list2.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	public static EColumn ElementColumn(EElement element, CActor actor = null)
	{
		if (ScenarioScript.current != null && ScenarioScript.current.IsActorConsideringElementAsStrong(actor, element))
		{
			return EColumn.Strong;
		}
		return s_ElementColumn[(int)element];
	}

	public static Dictionary<string, List<EElement>> CopyElementsToInfuse(Dictionary<string, List<EElement>> ElementsToInfuse)
	{
		if (ElementsToInfuse == null)
		{
			return null;
		}
		Dictionary<string, List<EElement>> dictionary = new Dictionary<string, List<EElement>>(ElementsToInfuse.Count);
		foreach (KeyValuePair<string, List<EElement>> item in ElementsToInfuse)
		{
			dictionary.Add(item.Key, item.Value.ToList());
		}
		return dictionary;
	}

	public static Dictionary<string, List<EElement>> GetElementsToInfuse()
	{
		return s_ElementsToInfuse;
	}

	public static Dictionary<string, List<EElement>> GetElementsToInfuseCopy()
	{
		return CopyElementsToInfuse(s_ElementsToInfuse);
	}

	public static void ProcessEndTurnInfusions(CActor.EType actortype = CActor.EType.Unknown)
	{
		Log.SimpleInfo("[ELEMENTS] Ending turn - infusing pending elements");
		if (s_ElementsToInfuse != null)
		{
			foreach (KeyValuePair<string, List<EElement>> item in s_ElementsToInfuse)
			{
				if (item.Value == null)
				{
					continue;
				}
				foreach (EElement item2 in item.Value)
				{
					if (item2 != EElement.Any)
					{
						s_ElementColumn[(int)item2] = EColumn.Strong;
						ScenarioScript.current?.UpdateElementsBoard();
						SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Infused, item2, item.Key, actortype, IsSummon: false, null, null));
					}
				}
			}
		}
		s_ElementsToInfuse.Clear();
		PrintElementColumns();
	}

	public static void EndRound(CActor.EType lastActorType, bool strongToWaningEnabled = true)
	{
		List<EElement> waningToInertDisabled = ScenarioManager.CurrentScenarioState.ScenarioSetup.WaningToInertDisabled;
		ProcessEndTurnInfusions(lastActorType);
		Log.SimpleInfo("[ELEMENTS] Ending round - moving element columns down 1");
		for (int i = 0; i < s_ElementColumn.Length; i++)
		{
			EColumn eColumn = s_ElementColumn[i];
			if (eColumn > EColumn.Inert && (eColumn != EColumn.Waning || waningToInertDisabled == null || !waningToInertDisabled.Contains((EElement)i)) && (eColumn != EColumn.Strong || strongToWaningEnabled))
			{
				s_ElementColumn[i]--;
			}
		}
		ScenarioScript.current?.UpdateElementsBoard();
		s_EnemyConsumedElements.Clear();
		s_EnemyInfusedElements.Clear();
		PrintElementColumns();
	}

	public static void TellClientInfusing(EElement element, CActor actorInfusing, bool forceLog = false)
	{
		PlayInfuseVFX(element, actorInfusing);
		CActorIsInfusing_MessageData message = new CActorIsInfusing_MessageData(actorInfusing)
		{
			m_Element = element,
			m_ForceLog = forceLog
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void CheckMonsterSetFinished(CActor lastActor, CActor nextActor)
	{
		if (lastActor != null)
		{
			string nonEliteClass = GetNonEliteClass(lastActor);
			string nonEliteClass2 = GetNonEliteClass(nextActor);
			if (nonEliteClass != nonEliteClass2)
			{
				MonsterSetFinished(lastActor);
			}
		}
	}

	public static void MonsterSetFinished(CActor actor)
	{
		string nonEliteClass = GetNonEliteClass(actor);
		List<EElement> enemyConsumedElements = GetEnemyConsumedElements(actor);
		if (enemyConsumedElements != null && enemyConsumedElements.Count > 0)
		{
			s_EnemyConsumedElements.Remove(nonEliteClass);
		}
		List<EElement> enemyInfusedElements = GetEnemyInfusedElements(actor);
		if (enemyInfusedElements != null && enemyInfusedElements.Count > 0)
		{
			foreach (EElement item in enemyInfusedElements)
			{
				Infuse(item, actor, endOfMonsterSet: true);
			}
			s_EnemyInfusedElements.Remove(nonEliteClass);
		}
		Log.SimpleInfo("[ELEMENTS] Monster set for " + nonEliteClass + " finished, setting infused/consumed elements");
		ProcessEndTurnInfusions(actor.OriginalType);
	}

	public static bool Infuse(EElement element, CActor actorInfusing, bool endOfMonsterSet = false, bool asStrong = false, bool endOfPlayerAction = false)
	{
		if (actorInfusing != null && !actorInfusing.CanInfuse(element, endOfMonsterSet))
		{
			return false;
		}
		if (asStrong)
		{
			SetElementInstantly(element, EColumn.Strong);
			TellClientInfusing(element, actorInfusing, endOfMonsterSet | endOfPlayerAction);
			return true;
		}
		if (actorInfusing is CEnemyActor cEnemyActor && !endOfMonsterSet)
		{
			EnemyInfuse(element, cEnemyActor.MonsterClass.ID, cEnemyActor);
			return true;
		}
		string key = ((actorInfusing != null) ? actorInfusing.Class.ID : "");
		if (element == EElement.Any && actorInfusing != null && actorInfusing.IsMonsterType)
		{
			EElement[] array = Elements.Where((EElement x) => ElementColumn(x) != EColumn.Strong && x != EElement.Any && actorInfusing.CanInfuse(x, endOfMonsterSet)).ToArray();
			if (array.Count() > 0)
			{
				int num = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, array.Count());
				EElement eElement = array[num];
				if (s_ElementsToInfuse.TryGetValue(key, out var value))
				{
					value.Add(eElement);
				}
				else
				{
					value = new List<EElement>();
					value.Add(eElement);
					s_ElementsToInfuse.Add(key, value);
				}
				TellClientInfusing(eElement, actorInfusing, endOfMonsterSet);
			}
		}
		else
		{
			if (!s_ElementsToInfuse.TryGetValue(key, out var value2))
			{
				value2 = new List<EElement>();
				s_ElementsToInfuse.Add(key, value2);
			}
			if (!value2.Contains(element))
			{
				value2.Add(element);
				TellClientInfusing(element, actorInfusing, endOfMonsterSet | endOfPlayerAction);
			}
		}
		return true;
	}

	public static List<EElement> Infuse(List<EElement> elements, CActor actorInfusing, bool asStrong = false)
	{
		elements = elements.FindAll((EElement e) => actorInfusing == null || Infuse(e, actorInfusing, endOfMonsterSet: false, asStrong));
		if (elements.Count > 0)
		{
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(null, actorInfusing)
			{
				m_ChangedElements = elements,
				m_InfuseAbility = null,
				m_IsConsume = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		return elements;
	}

	public static void UndoInfuse(List<EElement> elements, CActor actorInfusing)
	{
		foreach (EElement item in ScenarioManager.CurrentScenarioState.ElementsToInfuse.SelectMany((KeyValuePair<string, List<EElement>> x) => x.Value).Distinct())
		{
			elements.Remove(item);
		}
		string key = ((actorInfusing != null) ? actorInfusing.Class.ID : "");
		if (s_ElementsToInfuse.TryGetValue(key, out var elementsToInfuse))
		{
			elements.ForEach((EElement element) =>
			{
				elementsToInfuse.Remove(element);
			});
		}
		CElementInfuseCancelled_MessageData message = new CElementInfuseCancelled_MessageData(null, actorInfusing)
		{
			m_InfusedElements = elements,
			m_InfuseAbility = null
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void Consume(EElement element, CActor actorConsuming, bool endOfMonsterSet = false)
	{
		if (actorConsuming != null && !actorConsuming.CanConsume(element, endOfMonsterSet))
		{
			return;
		}
		if (actorConsuming is CEnemyActor cEnemyActor && !endOfMonsterSet)
		{
			EnemyConsume(element, cEnemyActor.MonsterClass.ID, cEnemyActor);
			return;
		}
		PlayConsumeVFX(element, actorConsuming);
		Log.SimpleInfo("[ELEMENTS] Consuming " + element);
		s_ElementColumn[(int)element] = EColumn.Inert;
		ScenarioScript.current?.OnConsumedElement(element, actorConsuming);
		if (actorConsuming == null)
		{
			return;
		}
		bool isSummon = false;
		if (actorConsuming.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actorConsuming.ActorGuid);
			if (cEnemyActor2 != null)
			{
				isSummon = cEnemyActor2.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Consumed, element, actorConsuming.Class.ID, actorConsuming.Type, isSummon, actorConsuming.Tokens.CheckPositiveTokens, actorConsuming.Tokens.CheckNegativeTokens));
	}

	public static void EnemyConsume(EElement element, string monsterClass, CActor actor)
	{
		PlayConsumeVFX(element, actor);
		Log.SimpleInfo("[ELEMENTS] Enemy consuming " + element);
		s_ElementColumn[(int)element] = EColumn.Inert;
		ScenarioScript.current?.OnConsumedElement(element, actor);
		monsterClass = monsterClass.Replace("Elite", "");
		List<EElement> value = null;
		if (s_EnemyConsumedElements.TryGetValue(monsterClass, out value))
		{
			s_EnemyConsumedElements[monsterClass].Add(element);
		}
		else
		{
			value = new List<EElement>();
			value.Add(element);
			s_EnemyConsumedElements.Add(monsterClass, value);
		}
		if (monsterClass == null)
		{
			return;
		}
		bool isSummon = false;
		if (actor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Consumed, element, monsterClass, actor.Type, isSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
	}

	private static void PlayConsumeVFX(EElement element, CActor actor)
	{
		if (actor != null && element != EElement.Any)
		{
			string vFXPropType = "ElementConsume" + element.ToString() + "VFX";
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = vFXPropType,
				m_ArrayIndex = actor.ArrayIndex
			});
		}
	}

	private static void PlayInfuseVFX(EElement element, CActor actor)
	{
		if (actor != null && element != EElement.Any)
		{
			string vFXPropType = "ElementInfuse" + element.ToString() + "VFX";
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = vFXPropType,
				m_ArrayIndex = actor.ArrayIndex
			});
		}
	}

	public static void EnemyInfuse(EElement element, string monsterClass, CActor actor)
	{
		Log.SimpleInfo("[ELEMENTS] Enemy infusing " + element);
		PlayInfuseVFX(element, actor);
		monsterClass = monsterClass.Replace("Elite", "");
		List<EElement> value = null;
		if (s_EnemyInfusedElements.TryGetValue(monsterClass, out value))
		{
			s_EnemyInfusedElements[monsterClass].Add(element);
		}
		else
		{
			value = new List<EElement>();
			value.Add(element);
			s_EnemyInfusedElements.Add(monsterClass, value);
		}
		if (monsterClass == null)
		{
			return;
		}
		bool isSummon = false;
		if (actor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Infused, element, monsterClass, actor.Type, isSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
	}

	private static string GetNonEliteClass(CActor actor)
	{
		if (actor == null || actor.MonsterClass?.ID == null)
		{
			return null;
		}
		return actor.MonsterClass.ID.Replace("Elite", "");
	}

	public static List<EElement> GetEnemyConsumedElements(CActor actor)
	{
		string nonEliteClass = GetNonEliteClass(actor);
		if (nonEliteClass != null && s_EnemyConsumedElements.TryGetValue(nonEliteClass, out var value))
		{
			return value;
		}
		return new List<EElement>();
	}

	public static List<EElement> GetEnemyInfusedElements(CActor actor)
	{
		string nonEliteClass = GetNonEliteClass(actor);
		if (nonEliteClass != null && s_EnemyInfusedElements.TryGetValue(nonEliteClass, out var value))
		{
			return value;
		}
		return new List<EElement>();
	}

	public static void SetElementInstantly(EElement element, EColumn column, bool fromScenarioScript = false)
	{
		Log.SimpleInfo("[ELEMENTS] Setting " + element.ToString() + " to " + column.ToString() + " instantly");
		s_ElementColumn[(int)element] = column;
		if (!fromScenarioScript)
		{
			ScenarioScript.current?.UpdateElementsBoard();
		}
		PrintElementColumns();
	}

	public static void PrintElementColumns()
	{
		string text = "[ELEMENTS] Element Board State:";
		EElement[] elements = Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			EElement eElement = elements[i];
			text = text + "\n" + eElement.ToString() + " is " + s_ElementColumn[(int)eElement];
		}
		Log.SimpleInfo(text);
	}
}
