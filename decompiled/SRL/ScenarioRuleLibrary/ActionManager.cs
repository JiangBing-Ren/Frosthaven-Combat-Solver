using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ActionManager
{
	private static List<CAction> s_Actions = new List<CAction>();

	public static List<CAction> Actions => s_Actions;

	public static void Load()
	{
		s_Actions.Clear();
		ValidateActionManager();
	}

	public static void Unload()
	{
		s_Actions.Clear();
	}

	public static void ValidateActionManager()
	{
	}
}
