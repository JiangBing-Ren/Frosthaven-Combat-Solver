using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class CVars : IGetVar, ICallMethod, IDumpState
{
	[JsonProperty]
	private Dictionary<string, TValue> vars;

	public void Clear()
	{
		vars?.Clear();
	}

	public int Count()
	{
		if (vars == null)
		{
			return 0;
		}
		return vars.Count;
	}

	public bool Empty()
	{
		if (vars != null)
		{
			return vars.Count == 0;
		}
		return true;
	}

	public TValue Get(string key)
	{
		if (vars == null)
		{
			return TValue.Unknown;
		}
		if (vars.TryGetValue(key, out var value))
		{
			return value;
		}
		return TValue.Unknown;
	}

	public void Set(string key, TValue value)
	{
		if (value.isUnknown)
		{
			if (vars != null)
			{
				vars.Remove(key);
			}
			return;
		}
		if (vars == null)
		{
			vars = new Dictionary<string, TValue>();
		}
		vars[key] = value;
	}

	public TValue GetVar(string key, CVarsContext context = null, bool asValue = true)
	{
		if (context == null)
		{
			context = new CVarsContext(this);
		}
		TValue tValue = Get(key);
		if (!tValue.isUnknown)
		{
			return tValue.Calc(context, asValue);
		}
		if (key != "obj")
		{
			Get("obj").GetObject(out var val, context);
			if (val != null)
			{
				tValue = CVarsContext.GetObjectVar(val, key, context, asValue);
				if (!tValue.isUnknown)
				{
					return tValue;
				}
			}
		}
		return TValue.Unknown;
	}

	public TValue CallMethod(string key, List<TValue> args, CVarsContext context = null, bool asValue = true)
	{
		Get("obj").GetObject(out var val, context);
		return CVarsContext.CallObjectMethod(val, key, args, context, asValue);
	}

	public override string ToString()
	{
		return $"[{MF.RefStr(this)}] {GetType().Name}({Count()})";
	}

	public string ToStateDumpString(StateDump dump)
	{
		return $"{GetType().Name}({Count()})";
	}

	public void DumpMembers(StateDump dump)
	{
		if (vars == null)
		{
			return;
		}
		foreach (KeyValuePair<string, TValue> var in vars)
		{
			string key = var.Key;
			TValue value = var.Value;
			if (!value.isUnknown)
			{
				dump.Add(key, value);
			}
		}
	}

	public void Dump(StringBuilder sb, string ident = "", string new_line = "\n", string tab = "\t", bool verbose = true, List<object> alreadyDumped = null)
	{
		sb.Append(ToString());
		if (vars == null)
		{
			return;
		}
		if (alreadyDumped == null)
		{
			alreadyDumped = new List<object>();
		}
		else if (alreadyDumped.Contains(this))
		{
			return;
		}
		alreadyDumped.Add(this);
		ident += tab;
		foreach (KeyValuePair<string, TValue> var in vars)
		{
			string key = var.Key;
			TValue value = var.Value;
			sb.Append(new_line + ident + key + ": ");
			if (verbose)
			{
				TValue.Dump(value, sb, ident, new_line, tab, verbose, alreadyDumped);
			}
			else
			{
				sb.Append(value.ToString());
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

	public string DumpVars()
	{
		return Dump();
	}

	public CVars Copy()
	{
		return new CVars
		{
			vars = new Dictionary<string, TValue>(vars)
		};
	}
}
