using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class CExpression
{
	public enum EType
	{
		Invalid,
		Constant,
		Variable,
		Operator
	}

	public struct CallStackItem
	{
		public CExpression expression;

		public CVarsContext context;

		public bool asValue;

		public override string ToString()
		{
			return expression.ToString();
		}
	}

	public static class Globals
	{
		public static ScenarioState CurrentScenarioState => ScenarioManager.CurrentScenarioState;

		public static ScenarioScript CurrentScenarioScript => ScenarioScript.current;

		public static CAbility CurrentAbility => (PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability;

		public static List<CActor> AllActors => ScenarioManager.Scenario.AllActors;

		public static List<CActor> AllAliveActors => ScenarioManager.Scenario.AllAliveActors;

		public static List<CPlayerActor> AllPlayers => ScenarioManager.Scenario.PlayerActors;

		public static List<CHeroSummonActor> AllHeroSummons => ScenarioManager.Scenario.HeroSummons;

		public static List<CEnemyActor> AllAliveMonsters => ScenarioManager.Scenario.AllAliveMonsters;

		public static List<CEnemyActor> AllEnemyMonsters => ScenarioManager.Scenario.AllEnemyMonsters;

		public static List<CObjectActor> AllObjects => ScenarioManager.Scenario.Objects;

		public static List<CObjectProp> AllTraps => CurrentScenarioState.Props.Where((CObjectProp x) => x is CObjectTrap && !x.Activated && x.PropTile.m_HexMap.Revealed).ToList();

		public static CSRLYML YML => ScenarioRuleClient.SRLYML;

		public static List<CAbility> AllAbilities => CSRLYML.AllAbilities;

		public static void log(string message)
		{
			Log.Info(message);
		}

		public static void errorWithReport(string message)
		{
			Log.ErrorWithReport(message);
		}

		public static void breakpoint(CVarsContext context = null)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}

		public static void breakpointIf(bool condition, CVarsContext context = null)
		{
			if (condition && Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}

		public static int abs(int a)
		{
			if (a < 0)
			{
				return -a;
			}
			return a;
		}

		public static int max(int a, int b)
		{
			if (a <= b)
			{
				return b;
			}
			return a;
		}

		public static int min(int a, int b)
		{
			if (a >= b)
			{
				return b;
			}
			return a;
		}

		public static List<object> list(List<TValue> args, CVarsContext context = null)
		{
			List<object> list = new List<object>(args.Count);
			foreach (TValue arg in args)
			{
				arg.GetObject(out var val);
				list.Add(val);
			}
			return list;
		}
	}

	public static class ListMethods
	{
		public static object Dump(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 0)
			{
				return TValue.Error($"IList.Dump called with {args.Count} parameters");
			}
			return TValue.Dump(lst);
		}

		public static object Contains(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Contains called with {args.Count} parameters");
			}
			object objB = args[0].Calc(context).GetObject<object>();
			int count = lst.Count;
			for (int i = 0; i < count; i++)
			{
				if (object.Equals(lst[i], objB))
				{
					return true;
				}
			}
			return false;
		}

		public static object Find(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Find called with {args.Count} parameters");
			}
			CExpression cExpression = args[0];
			int count = lst.Count;
			for (int i = 0; i < count; i++)
			{
				object obj = lst[i];
				CVarsContext context2 = new CVarsContext(obj, context);
				if (cExpression.Calc(context2).GetBool())
				{
					return obj;
				}
			}
			return null;
		}

		public static object Any(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Any called with {args.Count} parameters");
			}
			CExpression cExpression = args[0];
			int count = lst.Count;
			for (int i = 0; i < count; i++)
			{
				CVarsContext context2 = new CVarsContext(lst[i], context);
				if (cExpression.Calc(context2).GetBool())
				{
					return true;
				}
			}
			return false;
		}

		public static object Where(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Where called with {args.Count} parameters");
			}
			CExpression cExpression = args[0];
			int count = lst.Count;
			List<object> list = new List<object>(count);
			for (int i = 0; i < count; i++)
			{
				object obj = lst[i];
				CVarsContext context2 = new CVarsContext(obj, context);
				if (cExpression.Calc(context2).GetBool())
				{
					list.Add(obj);
				}
			}
			return list;
		}

		public static object OrderBy(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.OrderBy called with {args.Count} parameters");
			}
			CExpression arg0 = args[0];
			int count = lst.Count;
			List<object> list = new List<object>(count);
			for (int i = 0; i < count; i++)
			{
				object item = lst[i];
				list.Add(item);
			}
			return list.OrderBy((object obj) =>
			{
				CVarsContext context2 = new CVarsContext(obj, context);
				return arg0.Calc(context2).GetInt(context2);
			}).ToList();
		}

		public static object Select(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Select called with {args.Count} parameters");
			}
			CExpression cExpression = args[0];
			int count = lst.Count;
			List<object> list = new List<object>(count);
			for (int i = 0; i < count; i++)
			{
				CVarsContext context2 = new CVarsContext(lst[i], context);
				list.Add(cExpression.Calc(context2).GetObject<object>());
			}
			return list;
		}

		public static object SelectMany(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Select called with {args.Count} parameters");
			}
			CExpression cExpression = args[0];
			int count = lst.Count;
			List<object> list = new List<object>(count);
			for (int i = 0; i < count; i++)
			{
				CVarsContext context2 = new CVarsContext(lst[i], context);
				IList list2 = cExpression.Calc(context2).GetObject<IList>();
				if (list2 == null)
				{
					continue;
				}
				foreach (object item in list2)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static object Append(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			_ = args[0];
			int count = lst.Count;
			List<object> list = new List<object>(count + args.Count);
			for (int i = 0; i < count; i++)
			{
				object item = lst[i];
				list.Add(item);
			}
			for (int j = 0; j < args.Count; j++)
			{
				args[j].Calc(context).GetObject(out var val);
				if (val is IList list2)
				{
					foreach (object item2 in list2)
					{
						list.Add(item2);
					}
				}
				else
				{
					list.Add(val);
				}
			}
			return list;
		}

		public static object Except(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Except called with {args.Count} parameters");
			}
			object obj = args[0].Calc(context).GetObject<object>();
			IList list = obj as IList;
			int count = lst.Count;
			int num = list?.Count ?? 0;
			List<object> list2 = new List<object>(count);
			for (int i = 0; i < count; i++)
			{
				object obj2 = lst[i];
				if (obj2 == obj)
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < num; j++)
				{
					if (list[j] == obj2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.Add(obj2);
				}
			}
			return list2;
		}

		public static object Skip(IList list, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 1)
			{
				return TValue.Error($"IList.Except called with {args.Count} parameters");
			}
			int num = args[0].Calc(context).GetInt();
			List<object> list2 = new List<object>(list.Count);
			for (int i = num; i < list.Count; i++)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		public static object Clear(IList list, List<CExpression> args, CVarsContext context, bool asValue)
		{
			List<object> list2 = new List<object>(list.Count);
			list2.Clear();
			return list2;
		}

		public static object Distinct(IList lst, List<CExpression> args, CVarsContext context, bool asValue)
		{
			if (args.Count != 0)
			{
				return TValue.Error($"IList.Distinct called with {args.Count} parameters");
			}
			List<object> list = new List<object>(lst.Count);
			foreach (object item in lst)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public class Operator
	{
		public string name;

		public int priority;

		public Operator(string name, int priority)
		{
			this.name = name;
			this.priority = priority;
			Operators.all.Add(this);
		}

		public virtual bool CanBeUnary()
		{
			return false;
		}

		public virtual bool CanBeBinary()
		{
			return true;
		}

		public virtual TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
		{
			return TValue.Error("Not implemented operator: " + name);
		}

		public override string ToString()
		{
			return "Operator " + name;
		}

		public virtual string ToString(List<CExpression> operands)
		{
			if (operands == null || operands.Count == 0)
			{
				return name + "()";
			}
			if (operands.Count == 1)
			{
				return $"{name}{operands[0]}";
			}
			if (operands.Count == 2)
			{
				return $"{operands[0]} {name} {operands[1]}";
			}
			string text = name + "(";
			for (int i = 0; i < operands.Count; i++)
			{
				CExpression cExpression = operands[i];
				if (i > 0)
				{
					text += ", ";
				}
				text += cExpression.ToString();
			}
			return text + ")";
		}
	}

	public static class Operators
	{
		public class Brackets : Operator
		{
			public static Brackets instance;

			public Brackets()
				: base("()", -1)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands != null && operands.Count == 1)
				{
					return $"({operands[0]})";
				}
				return base.ToString(operands);
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				return operands[0].Calc(context, asValue);
			}
		}

		public class Index : Operator
		{
			public static Index instance;

			public Index()
				: base("[]", -1)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands != null && operands.Count == 2)
				{
					return $"{operands[0]}[{operands[1]}]";
				}
				return base.ToString(operands);
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				TValue tValue = cExpression.Calc(context, asValue);
				tValue.GetObject(out var val);
				if (val == null)
				{
					return TValue.Error($"Attempt to index invalid object: {cExpression} -> {tValue}");
				}
				TValue key = operands[1].Calc(context, asValue);
				return CVarsContext.GetObjectItemByIndex(val, key, context, asValue);
			}
		}

		public class Dot : Operator
		{
			public static Dot instance;

			public Dot()
				: base(".", 2)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands != null && operands.Count == 2)
				{
					return $"{operands[0]}.{operands[1]}";
				}
				return base.ToString(operands);
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				TValue tValue = cExpression.Calc(context, asValue: false);
				tValue.GetObject(out var val);
				if (val == null)
				{
					return TValue.Error($"Attempt to get value from invalid object: {cExpression} -> {tValue}");
				}
				CExpression cExpression2 = operands[1];
				if (cExpression2.type != EType.Variable)
				{
					return TValue.Error($"Non-variable argument passed to operator .: {cExpression2}");
				}
				string key = cExpression2.value.GetString();
				return CVarsContext.GetObjectVar(val, key, context, asValue);
			}
		}

		public class Elvis : Operator
		{
			public static Elvis instance;

			public Elvis()
				: base("?.", 2)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands != null && operands.Count == 2)
				{
					return $"{operands[0]}?.{operands[1]}";
				}
				return base.ToString(operands);
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				operands[0].Calc(context, asValue: false).GetObject(out var val);
				if (val == null)
				{
					return TValue.Null;
				}
				CExpression cExpression = operands[1];
				if (cExpression.type != EType.Variable)
				{
					return TValue.Error($"Non-variable argument passed to operator .: {cExpression}");
				}
				string key = cExpression.value.GetString();
				return CVarsContext.GetObjectVar(val, key, context, asValue);
			}
		}

		public class CallFunction : Operator
		{
			public static CallFunction instance;

			public CallFunction()
				: base("FUNC", -1)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands == null || operands.Count < 1)
				{
					return base.ToString(operands);
				}
				string text = $"{operands[0]}(";
				for (int i = 1; i < operands.Count; i++)
				{
					CExpression cExpression = operands[i];
					if (i > 1)
					{
						text += ", ";
					}
					text += cExpression.ToString();
				}
				return text + ")";
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count < 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				if (cExpression.type != EType.Variable)
				{
					return TValue.Error($"Non-variable argument passed to operator FUNC: {cExpression}");
				}
				string text = cExpression.value.GetString();
				List<TValue> list = new List<TValue>(operands.Count - 1);
				for (int i = 1; i < operands.Count; i++)
				{
					CExpression cExpression2 = operands[i];
					TValue tValue = cExpression2.Calc(context);
					if (tValue.isError)
					{
						return tValue;
					}
					if (tValue.isUnknown)
					{
						return TValue.Error($"Unknown function argument: {cExpression2}");
					}
					list.Add(tValue);
				}
				TValue result = context?.CallMethod(text, list, context, asValue) ?? CVarsContext.CallGlobalFunction(text, list, context, asValue);
				if (result.isUnknown)
				{
					return TValue.Error("Attempted to call unknown function: " + text);
				}
				return result;
			}
		}

		public class CallMethod : Operator
		{
			public static CallMethod instance;

			public CallMethod()
				: base("METHOD", -1)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands == null || operands.Count < 2)
				{
					return base.ToString(operands);
				}
				string text = $"{operands[0]}.{operands[1]}(";
				for (int i = 2; i < operands.Count; i++)
				{
					CExpression cExpression = operands[i];
					if (i > 2)
					{
						text += ", ";
					}
					text += cExpression.ToString();
				}
				return text + ")";
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count < 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				TValue tValue = cExpression.Calc(context, asValue: false);
				tValue.GetObject(out var val);
				if (val == null)
				{
					return TValue.Error($"Attempt to call method of invalid object: {cExpression} -> {tValue}");
				}
				CExpression cExpression2 = operands[1];
				if (cExpression2.type != EType.Variable)
				{
					return TValue.Error($"Non-variable argument passed to operator METHOD: {cExpression2}");
				}
				string text = cExpression2.value.GetString();
				if (val is IList lst)
				{
					List<CExpression> range = operands.GetRange(2, operands.Count - 2);
					TValue result = CVarsContext.CallListMethod(lst, text, range, context, asValue);
					if (result.isUnknown)
					{
						return TValue.Error("Attempted to call unknown list method: " + text);
					}
					return result;
				}
				List<TValue> list = new List<TValue>(operands.Count - 2);
				for (int i = 2; i < operands.Count; i++)
				{
					CExpression cExpression3 = operands[i];
					TValue tValue2 = cExpression3.Calc(context);
					if (tValue2.isError)
					{
						return tValue2;
					}
					if (tValue2.isUnknown)
					{
						return TValue.Error($"Unknown method argument: {cExpression3}");
					}
					list.Add(tValue2);
				}
				TValue result2 = CVarsContext.CallObjectMethod(val, text, list, context, asValue);
				if (result2.isUnknown)
				{
					return TValue.Error($"Attempted to call unknown method: {tValue}.{text}");
				}
				return result2;
			}
		}

		public class CallMethodSafe : Operator
		{
			public static CallMethodSafe instance;

			public CallMethodSafe()
				: base("METHOD?", -1)
			{
				instance = this;
			}

			public override string ToString(List<CExpression> operands)
			{
				if (operands == null || operands.Count < 2)
				{
					return base.ToString(operands);
				}
				string text = $"{operands[0]}?.{operands[1]}(";
				for (int i = 2; i < operands.Count; i++)
				{
					CExpression cExpression = operands[i];
					if (i > 2)
					{
						text += ", ";
					}
					text += cExpression.ToString();
				}
				return text + ")";
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count < 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context, asValue: false);
				tValue.GetObject(out var val);
				if (val == null)
				{
					return TValue.Null;
				}
				CExpression cExpression = operands[1];
				if (cExpression.type != EType.Variable)
				{
					return TValue.Error($"Non-variable argument passed to operator METHOD?: {cExpression}");
				}
				string text = cExpression.value.GetString();
				if (val is IList lst)
				{
					List<CExpression> range = operands.GetRange(2, operands.Count - 2);
					TValue result = CVarsContext.CallListMethod(lst, text, range, context, asValue);
					if (result.isUnknown)
					{
						return TValue.Error("Attempted to call unknown list method: " + text);
					}
					return result;
				}
				List<TValue> list = new List<TValue>(operands.Count - 2);
				for (int i = 2; i < operands.Count; i++)
				{
					CExpression cExpression2 = operands[i];
					TValue tValue2 = cExpression2.Calc(context);
					if (tValue2.isError)
					{
						return tValue2;
					}
					if (tValue2.isUnknown)
					{
						return TValue.Error($"Unknown method argument: {cExpression2}");
					}
					list.Add(tValue2);
				}
				TValue result2 = CVarsContext.CallObjectMethod(val, text, list, context, asValue);
				if (result2.isUnknown)
				{
					return TValue.Error($"Attempted to call unknown method: {tValue}.{text}");
				}
				return result2;
			}
		}

		public class DumpOperand : Operator
		{
			public DumpOperand()
				: base("??", 20)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override bool CanBeBinary()
			{
				return false;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				TValue tValue = cExpression.Calc(context, asValue);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append($"Calculated {cExpression} -> {TValue.Dump(tValue)}\n\t");
				cExpression.Dump(stringBuilder, "\t");
				stringBuilder.Append("\nContext:");
				if (context == null)
				{
					stringBuilder.Append(" null");
				}
				else
				{
					stringBuilder.Append("\n\t");
					context.Dump(stringBuilder, "\t");
				}
				Log.Info(stringBuilder.ToString());
				return tValue;
			}
		}

		public class LogOperand : Operator
		{
			public LogOperand()
				: base("?", 3)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override bool CanBeBinary()
			{
				return false;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				CExpression cExpression = operands[0];
				TValue tValue = cExpression.Calc(context, asValue);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append($"Calculated {cExpression} -> {tValue}\n");
				stringBuilder.Append("\nContext:");
				if (context == null)
				{
					stringBuilder.Append(" null");
				}
				else
				{
					stringBuilder.Append("\n\t");
					context.Dump(stringBuilder, "\t");
				}
				Log.Info(stringBuilder.ToString());
				return tValue;
			}
		}

		public class Plus : Operator
		{
			public Plus()
				: base("+", 6)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || (operands.Count != 1 && operands.Count != 2))
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (operands.Count == 1)
				{
					if (!tValue.isConvertibleToInt)
					{
						return TValue.Error($"Unary plus used with non-integer value {tValue}");
					}
					return tValue.GetInt();
				}
				TValue tValue2 = operands[1].Calc(context);
				if (tValue.isConvertibleToInt)
				{
					if (tValue2.isConvertibleToInt)
					{
						return tValue.GetInt() + tValue2.GetInt();
					}
					if (tValue2.isString)
					{
						return tValue.ValueString() + tValue2.GetString();
					}
					return TValue.Error($"Invalid operand for operator plus: {tValue2}");
				}
				if (tValue.isString)
				{
					return tValue.GetString() + tValue2.ValueString();
				}
				if (tValue2.isString)
				{
					return tValue.ValueString() + tValue2.GetString();
				}
				return TValue.Error($"Invalid operand for operator plus: {tValue}");
			}
		}

		public class Minus : Operator
		{
			public Minus()
				: base("-", 6)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || (operands.Count != 1 && operands.Count != 2))
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (operands.Count == 1)
				{
					if (!tValue.isConvertibleToInt)
					{
						return TValue.Error($"Unary minus used with non-integer value {tValue}");
					}
					return -tValue.GetInt();
				}
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"Binary minus used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"Binary minus used with non-integer value {tValue2}");
				}
				return tValue.GetInt() - tValue2.GetInt();
			}
		}

		public class Mul : Operator
		{
			public Mul()
				: base("*", 5)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"Binary mul used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"Binary mul used with non-integer value {tValue2}");
				}
				return tValue.GetInt() * tValue2.GetInt();
			}
		}

		public class Div : Operator
		{
			public Div()
				: base("/", 5)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"Binary div used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"Binary div used with non-integer value {tValue2}");
				}
				if (tValue2.GetInt() == 0)
				{
					return TValue.Error("Division by zero");
				}
				return tValue.GetInt() / tValue2.GetInt();
			}
		}

		public class Mod : Operator
		{
			public Mod()
				: base("%", 5)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"Binary mod used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"Binary mod used with non-integer value {tValue2}");
				}
				if (tValue2.GetInt() == 0)
				{
					return TValue.Error("Modulo of zero");
				}
				return tValue.GetInt() % tValue2.GetInt();
			}
		}

		public class Eq : Operator
		{
			public Eq()
				: base("==", 9)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				TValue val = operands[1].Calc(context);
				return tValue.Eq(val);
			}
		}

		public class NEq : Operator
		{
			public NEq()
				: base("!=", 9)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				TValue val = operands[1].Calc(context);
				return !tValue.Eq(val);
			}
		}

		public class Gt : Operator
		{
			public Gt()
				: base(">", 8)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"> used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"> used with non-integer value {tValue2}");
				}
				return tValue.GetInt() > tValue2.GetInt();
			}
		}

		public class GtEq : Operator
		{
			public GtEq()
				: base(">=", 8)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($">= used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($">= used with non-integer value {tValue2}");
				}
				return tValue.GetInt() >= tValue2.GetInt();
			}
		}

		public class Lt : Operator
		{
			public Lt()
				: base("<", 8)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"< used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"< used with non-integer value {tValue2}");
				}
				return tValue.GetInt() < tValue2.GetInt();
			}
		}

		public class LtEq : Operator
		{
			public LtEq()
				: base("<=", 8)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue tValue = operands[0].Calc(context);
				if (!tValue.isConvertibleToInt)
				{
					return TValue.Error($"<= used with non-integer value {tValue}");
				}
				TValue tValue2 = operands[1].Calc(context);
				if (!tValue2.isConvertibleToInt)
				{
					return TValue.Error($"<= used with non-integer value {tValue2}");
				}
				return tValue.GetInt() <= tValue2.GetInt();
			}
		}

		public class Not : Operator
		{
			public Not(string name)
				: base(name, 3)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override bool CanBeBinary()
			{
				return false;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				return !operands[0].Calc(context, asValue).GetBool();
			}
		}

		public class NotNot : Operator
		{
			public NotNot()
				: base("!!", 3)
			{
			}

			public override bool CanBeUnary()
			{
				return true;
			}

			public override bool CanBeBinary()
			{
				return false;
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 1)
				{
					return TValue.Error("Invalid number of operands");
				}
				return operands[0].Calc(context, asValue).GetBool();
			}
		}

		public class Or : Operator
		{
			public Or(string name)
				: base(name, 14)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue result = operands[0].Calc(context);
				if (result.GetBool())
				{
					return result;
				}
				return operands[1].Calc(context);
			}
		}

		public class And : Operator
		{
			public And(string name)
				: base(name, 13)
			{
			}

			public override TValue Calc(List<CExpression> operands, CVarsContext context, bool asValue)
			{
				if (operands == null || operands.Count != 2)
				{
					return TValue.Error("Invalid number of operands");
				}
				TValue result = operands[0].Calc(context);
				if (!result.GetBool())
				{
					return result;
				}
				return operands[1].Calc(context);
			}
		}

		public static List<Operator> all = new List<Operator>();

		private static bool initted = false;

		public static void Init()
		{
			if (!initted)
			{
				initted = true;
				new Brackets();
				new Index();
				new Dot();
				new Elvis();
				new CallFunction();
				new CallMethod();
				new CallMethodSafe();
				new DumpOperand();
				new LogOperand();
				new Plus();
				new Minus();
				new Mul();
				new Div();
				new Mod();
				new Eq();
				new NEq();
				new Gt();
				new GtEq();
				new Lt();
				new LtEq();
				new Not("!");
				new Not("not");
				new NotNot();
				new Or("||");
				new Or("or");
				new And("&&");
				new And("and");
			}
		}
	}

	public static class Parser
	{
		public struct Stream
		{
			public string text;

			public int curOfs;

			public int endOfs;

			public char curChar
			{
				get
				{
					if (curOfs >= endOfs)
					{
						return '\0';
					}
					return text[curOfs];
				}
			}

			public string remainingText => text.Substring(curOfs, endOfs - curOfs);
		}

		public static bool IsBlank(char c)
		{
			if (c != ' ' && c != '\t' && c != '\r')
			{
				return c == '\n';
			}
			return true;
		}

		public static void SkipBlanks(ref Stream stream)
		{
			while (IsBlank(stream.curChar))
			{
				stream.curOfs++;
			}
		}

		public static bool IsDigit(char c)
		{
			if (c >= '0')
			{
				return c <= '9';
			}
			return false;
		}

		public static bool IsIDFirstChar(char c)
		{
			switch (c)
			{
			default:
				if (c >= 'A')
				{
					return c <= 'Z';
				}
				return false;
			case '_':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				return true;
			}
		}

		public static bool IsIDChar(char c)
		{
			if (!IsIDFirstChar(c))
			{
				return IsDigit(c);
			}
			return true;
		}

		public static bool IsAt(ref Stream stream, string s)
		{
			if (stream.curOfs + s.Length > stream.endOfs)
			{
				return false;
			}
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				char c2 = stream.text[stream.curOfs + i];
				if (c != c2)
				{
					return false;
				}
			}
			return true;
		}

		public static bool ReadInt(ref Stream stream, out TValue val)
		{
			int curOfs = stream.curOfs;
			int num = 0;
			while (IsDigit(stream.curChar))
			{
				int num2 = stream.curChar - 48;
				num = 10 * num + num2;
				stream.curOfs++;
			}
			if (stream.curOfs == curOfs)
			{
				val = TValue.Unknown;
				return false;
			}
			val = num;
			return true;
		}

		public static bool ReadIdentifier(ref Stream stream, out TValue val)
		{
			if (!IsIDFirstChar(stream.curChar))
			{
				val = TValue.Unknown;
				return false;
			}
			int curOfs = stream.curOfs;
			stream.curOfs++;
			while (IsIDChar(stream.curChar))
			{
				stream.curOfs++;
			}
			val = stream.text.Substring(curOfs, stream.curOfs - curOfs);
			return true;
		}

		public static bool ReadQuotedString(ref Stream stream, out TValue val)
		{
			char curChar = stream.curChar;
			if (curChar != '\'' && curChar != '"')
			{
				val = TValue.Unknown;
				return false;
			}
			stream.curOfs++;
			int curOfs = stream.curOfs;
			while (stream.curChar != curChar && stream.curChar != '\0')
			{
				stream.curOfs++;
			}
			if (stream.curChar != curChar)
			{
				val = TValue.Error("Unterninated string");
				return true;
			}
			val = stream.text.Substring(curOfs, stream.curOfs - curOfs);
			stream.curOfs++;
			return true;
		}

		public static CExpression ReadOperand(ref Stream stream)
		{
			SkipBlanks(ref stream);
			if (stream.curOfs >= stream.endOfs)
			{
				return null;
			}
			if (stream.curChar == '(')
			{
				stream.curOfs++;
				CExpression cExpression = ReadExpression(ref stream);
				SkipBlanks(ref stream);
				if (cExpression == null)
				{
					return Error("Expression expected");
				}
				if (stream.curChar != ')')
				{
					return Error("')' expected");
				}
				stream.curOfs++;
				return new CExpression
				{
					type = EType.Operator,
					value = new TValue(Operators.Brackets.instance),
					operands = new List<CExpression> { cExpression }
				};
			}
			int curOfs = stream.curOfs;
			if (ReadIdentifier(ref stream, out var val))
			{
				val.GetString(out var val2);
				switch (val2)
				{
				case "true":
				case "True":
				case "TRUE":
					return new CExpression
					{
						type = EType.Constant,
						value = true
					};
				case "false":
				case "False":
				case "FALSE":
					return new CExpression
					{
						type = EType.Constant,
						value = false
					};
				case "null":
				case "Null":
				case "NULL":
					return new CExpression
					{
						type = EType.Constant,
						value = TValue.Null
					};
				case "unknown":
				case "Unknown":
				case "UNKNOWN":
					return new CExpression
					{
						type = EType.Constant,
						value = TValue.Unknown
					};
				case "not":
					stream.curOfs = curOfs;
					return null;
				default:
					return new CExpression
					{
						type = EType.Variable,
						value = val
					};
				}
			}
			if (ReadInt(ref stream, out var val3))
			{
				return new CExpression
				{
					type = EType.Constant,
					value = val3
				};
			}
			if (ReadQuotedString(ref stream, out var val4))
			{
				return new CExpression
				{
					type = ((!val4.isError) ? EType.Constant : EType.Invalid),
					value = val4
				};
			}
			return null;
		}

		public static Operator ReadOperator(ref Stream stream)
		{
			Operator obj = null;
			for (int i = 0; i < Operators.all.Count; i++)
			{
				Operator obj2 = Operators.all[i];
				if (obj2.priority >= 0 && IsAt(ref stream, obj2.name) && (obj == null || obj.name.Length < obj2.name.Length))
				{
					obj = obj2;
				}
			}
			if (obj != null)
			{
				stream.curOfs += obj.name.Length;
			}
			return obj;
		}

		public static List<CExpression> ReadCallArgs(ref Stream stream)
		{
			List<CExpression> list = new List<CExpression>();
			while (true)
			{
				CExpression cExpression = ReadExpression(ref stream);
				if (cExpression == null)
				{
					break;
				}
				list.Add(cExpression);
				SkipBlanks(ref stream);
				if (stream.curChar != ',')
				{
					break;
				}
				stream.curOfs++;
			}
			return list;
		}

		public static CExpression ReadExpression(ref Stream stream, int maxPriority = int.MaxValue)
		{
			Operators.Init();
			CExpression cExpression = ReadOperand(ref stream);
			if (cExpression != null && cExpression.type == EType.Invalid)
			{
				return cExpression;
			}
			while (true)
			{
				SkipBlanks(ref stream);
				if (stream.curOfs >= stream.endOfs)
				{
					break;
				}
				switch (stream.curChar)
				{
				case '(':
				{
					if (cExpression == null || (cExpression.type != EType.Variable && (cExpression.type != EType.Operator || (cExpression.value.objVal != Operators.Dot.instance && cExpression.value.objVal != Operators.Elvis.instance))))
					{
						return Error("Invalid function / method call");
					}
					stream.curOfs++;
					List<CExpression> list = ReadCallArgs(ref stream);
					SkipBlanks(ref stream);
					if (stream.curChar != ')')
					{
						return Error("')' expexcted");
					}
					stream.curOfs++;
					if (cExpression.type == EType.Variable)
					{
						list.Insert(0, cExpression);
						cExpression = new CExpression
						{
							type = EType.Operator,
							value = new TValue(Operators.CallFunction.instance),
							operands = list
						};
					}
					else
					{
						list.Insert(0, cExpression.operands[1]);
						list.Insert(0, cExpression.operands[0]);
						cExpression = new CExpression
						{
							type = EType.Operator,
							value = new TValue((cExpression.value.objVal == Operators.Dot.instance) ? ((Operator)Operators.CallMethod.instance) : ((Operator)Operators.CallMethodSafe.instance)),
							operands = list
						};
					}
					continue;
				}
				case '[':
				{
					if (cExpression == null)
					{
						return Error("Invalid use of index operator without left hand side expression");
					}
					stream.curOfs++;
					CExpression cExpression2 = ReadExpression(ref stream);
					if (cExpression2 == null || cExpression2.type == EType.Invalid)
					{
						return Error("Invalid index expression");
					}
					SkipBlanks(ref stream);
					if (stream.curChar != ']')
					{
						return Error("']' expected");
					}
					stream.curOfs++;
					cExpression = new CExpression
					{
						type = EType.Operator,
						value = new TValue(Operators.Index.instance),
						operands = new List<CExpression> { cExpression, cExpression2 }
					};
					continue;
				}
				}
				int curOfs = stream.curOfs;
				Operator obj = ReadOperator(ref stream);
				if (obj == null)
				{
					break;
				}
				if (cExpression != null && !obj.CanBeBinary())
				{
					return Error("Unary operator used as binary: " + obj.name);
				}
				if ((cExpression != null || !obj.CanBeUnary()) && (obj.priority > maxPriority || (obj.priority == maxPriority && maxPriority > 4 && maxPriority < 15)))
				{
					stream.curOfs = curOfs;
					break;
				}
				if (obj is Operators.Dot || obj is Operators.Elvis)
				{
					if (cExpression == null)
					{
						return Error("Invalid use of operator . without left hand side expression");
					}
					SkipBlanks(ref stream);
					if (!ReadIdentifier(ref stream, out var val))
					{
						return Error("Identifier (variable name) expected");
					}
					CExpression item = new CExpression
					{
						type = EType.Variable,
						value = val
					};
					cExpression = new CExpression
					{
						type = EType.Operator,
						value = new TValue((obj is Operators.Dot) ? ((Operator)Operators.Dot.instance) : ((Operator)Operators.Elvis.instance)),
						operands = new List<CExpression> { cExpression, item }
					};
					continue;
				}
				CExpression cExpression3 = ReadExpression(ref stream, obj.priority);
				if (cExpression3 == null)
				{
					return Error("Missing right hand side expression");
				}
				if (cExpression3.type == EType.Invalid)
				{
					return Error(cExpression3.value.GetError() ?? "Invalid expression");
				}
				if (cExpression == null)
				{
					if (!obj.CanBeUnary())
					{
						return Error("Invalid use of operator " + obj.name + " as unary operator");
					}
					cExpression = new CExpression
					{
						type = EType.Operator,
						value = new TValue(obj),
						operands = new List<CExpression> { cExpression3 }
					};
				}
				else
				{
					cExpression = new CExpression
					{
						type = EType.Operator,
						value = new TValue(obj),
						operands = new List<CExpression> { cExpression, cExpression3 }
					};
				}
			}
			return cExpression;
		}
	}

	[JsonProperty]
	public List<CExpression> operands;

	[ThreadStatic]
	public static List<CallStackItem> s_CallStack;

	[JsonProperty]
	public EType type { get; private set; }

	[JsonProperty]
	public TValue value { get; private set; }

	[JsonIgnore]
	public TValue result { get; private set; }

	public static List<CallStackItem> CallStack => s_CallStack;

	public static CExpression CurrentExpression
	{
		get
		{
			if (s_CallStack.Count <= 0)
			{
				return null;
			}
			return s_CallStack[s_CallStack.Count - 1].expression;
		}
	}

	public static CVarsContext CurrentContext
	{
		get
		{
			if (s_CallStack.Count <= 0)
			{
				return null;
			}
			return s_CallStack[s_CallStack.Count - 1].context;
		}
	}

	public TValue Calc(CVarsContext context, bool asValue = true)
	{
		if (s_CallStack == null)
		{
			s_CallStack = new List<CallStackItem>();
		}
		s_CallStack.Add(new CallStackItem
		{
			expression = this,
			context = context,
			asValue = asValue
		});
		TValue tValue = TValue.Unknown;
		switch (type)
		{
		case EType.Invalid:
			tValue = TValue.Error("Attempting to calculate an invalid expression");
			break;
		case EType.Constant:
			tValue = value;
			break;
		case EType.Variable:
		{
			value.GetString(out var val);
			tValue = context?.GetVar(val, context, asValue) ?? CVarsContext.GetGlobalVar(val, null, asValue);
			break;
		}
		case EType.Operator:
			tValue = value.GetObject<Operator>().Calc(operands, context, asValue);
			break;
		}
		result = tValue;
		s_CallStack.RemoveAt(s_CallStack.Count - 1);
		return tValue;
	}

	public override string ToString()
	{
		switch (type)
		{
		case EType.Invalid:
			if (!value.isError)
			{
				return "InvalidExpression";
			}
			return value.ToString();
		case EType.Constant:
			return value.LogString();
		case EType.Variable:
			return value.GetString();
		case EType.Operator:
			return value.GetObject<Operator>()?.ToString(operands);
		default:
			return "UnknownExpressionType";
		}
	}

	public void Dump(StringBuilder sb, string ident = "", string new_line = "\n", string tab = "\t", bool verbose = true, List<object> alreadyDumped = null)
	{
		if (type != EType.Operator)
		{
			sb.Append($"{this} -> {result}");
			return;
		}
		Operator arg = value.GetObject<Operator>();
		sb.Append($"{arg} ({this}) -> {result}");
		if (operands != null)
		{
			ident += tab;
			for (int i = 0; i < operands.Count; i++)
			{
				CExpression cExpression = operands[i];
				sb.Append(new_line + ident);
				cExpression.Dump(sb, ident, new_line, tab, verbose);
			}
		}
	}

	public string Dump(string ident, string new_line = "\n", string tab = "\t", bool verbose = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Dump(stringBuilder, ident, new_line, tab, verbose);
		return stringBuilder.ToString();
	}

	public string Dump()
	{
		return Dump("");
	}

	public string DumpExpression()
	{
		return Dump();
	}

	public static CExpression Parse(string text, int startOfs = 0, int endOfs = -1)
	{
		if (endOfs < 0)
		{
			endOfs = text.Length;
		}
		Parser.Stream stream = new Parser.Stream
		{
			text = text,
			curOfs = startOfs,
			endOfs = endOfs
		};
		CExpression cExpression = Parser.ReadExpression(ref stream);
		if (cExpression == null)
		{
			return null;
		}
		if (stream.curOfs != stream.endOfs)
		{
			return Error($"Expression terminated early: '{cExpression}' + '{stream.remainingText}'");
		}
		return cExpression;
	}

	private static CExpression Error(string err, bool log = true)
	{
		return new CExpression
		{
			type = EType.Invalid,
			value = TValue.Error(err, log)
		};
	}
}
