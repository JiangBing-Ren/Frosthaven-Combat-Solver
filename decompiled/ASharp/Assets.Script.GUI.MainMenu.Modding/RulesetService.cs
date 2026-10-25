using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Script.GUI.MainMenu.Modding;

internal class RulesetService : IRulesetService
{
	public void CreateRuleset(RulesetDataView rulesetData, Action<IRuleset> onSuccessfullyCreated, Action onFailed)
	{
	}

	private void Validate(List<IMod> mods, Action<bool> onValidatedCallback)
	{
		CoroutineHelper.RunCoroutine(ValidateMods(mods, onValidatedCallback));
	}

	private IEnumerator ValidateMods(List<IMod> mods, Action<bool> onValidatedCallback)
	{
		return null;
	}

	public void CompileRuleset(IRuleset ruleset, Action<bool> onCompiledCallback)
	{
	}

	public void DeleteRuleset(IRuleset ruleset)
	{
	}

	public List<IRuleset> GetRulesets()
	{
		return null;
	}

	public void UpdateRuleset(IRuleset ruleset, RulesetDataView newData, Action<IRuleset> onSuccessfullyUpdated, Action onFailed)
	{
	}
}
