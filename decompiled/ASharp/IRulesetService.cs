using System;
using System.Collections.Generic;

public interface IRulesetService
{
	List<IRuleset> GetRulesets();

	void UpdateRuleset(IRuleset ruleset, RulesetDataView newData, Action<IRuleset> onSuccessfullyUpdated, Action onFailed);

	void DeleteRuleset(IRuleset ruleset);

	void CreateRuleset(RulesetDataView rulesetData, Action<IRuleset> onSuccessfullyCreated, Action onFailed);

	void CompileRuleset(IRuleset ruleset, Action<bool> onCompiledCallback);
}
