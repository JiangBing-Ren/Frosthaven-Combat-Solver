using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class MiscVarMonsterCardConditionalElement : MonsterCardConditionalElement
{
	[TextArea(3, 10)]
	[SerializeField]
	private string _expression;

	private TValue _value;

	public TValue Value
	{
		get
		{
			if (!_value.isInvalid)
			{
				return _value;
			}
			_value = new TValue(CExpression.Parse(_expression));
			return _value;
		}
	}

	protected override bool CanShow(MonsterCardUIContext context)
	{
		CVarsContext context2 = new CVarsContext(context);
		return Value.GetBool(context2);
	}
}
