using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public interface ICallMethod
{
	TValue CallMethod(string key, List<TValue> args, CVarsContext context = null, bool asValue = true);
}
