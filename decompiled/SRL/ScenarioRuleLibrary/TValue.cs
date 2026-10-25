using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public struct TValue : IEquatable<TValue>
{
	public enum EType
	{
		Unknown,
		Error,
		Null,
		Bool,
		Int,
		Enum,
		String,
		Object,
		Expression
	}

	public static TValue Unknown = new TValue
	{
		type = EType.Unknown,
		intVal = 0,
		objVal = null
	};

	public static TValue Null = new TValue
	{
		type = EType.Null,
		intVal = 0,
		objVal = null
	};

	private static string[] friendly_type_prefixes = new string[1] { "System.Collections.Generic." };

	private static string[] optional_friendly_type_prefixes = new string[4] { "ScenarioRuleLibrary.", "UnityEngine.", "Unity.Collections.", "Unity.Mathematics." };

	[JsonProperty]
	public EType type { get; private set; }

	[JsonProperty]
	public int intVal { get; private set; }

	[JsonProperty(IsReference = true)]
	public object objVal { get; private set; }

	public bool isUnknown => type == EType.Unknown;

	public bool isError => type == EType.Error;

	public bool isNull => type == EType.Null;

	public bool isBool => type == EType.Bool;

	public bool isInt => type == EType.Int;

	public bool isEnum => type == EType.Enum;

	public bool isString => type == EType.String;

	public bool isObject => type == EType.Object;

	public bool isExpression => type == EType.Expression;

	public bool isValid => type > EType.Null;

	public bool isInvalid => type <= EType.Null;

	public bool isConvertibleToInt
	{
		get
		{
			if (type != EType.Bool && type != EType.Int)
			{
				return type == EType.Enum;
			}
			return true;
		}
	}

	public bool isManagedObject
	{
		get
		{
			if (type != EType.String && type != EType.Object)
			{
				return type == EType.Expression;
			}
			return true;
		}
	}

	public TValue(bool val)
	{
		type = EType.Bool;
		intVal = (val ? 1 : 0);
		objVal = null;
	}

	public TValue(int val)
	{
		type = EType.Int;
		intVal = val;
		objVal = null;
	}

	public TValue(string val)
	{
		type = ((val == null) ? EType.Null : EType.String);
		intVal = 0;
		objVal = val;
	}

	public TValue(CExpression val)
	{
		type = ((val == null) ? EType.Null : EType.Expression);
		intVal = 0;
		objVal = val;
	}

	public TValue(TValue val)
	{
		type = val.type;
		intVal = val.intVal;
		objVal = val.objVal;
	}

	public TValue(object val)
	{
		if (val != null)
		{
			if (!(val is bool flag))
			{
				if (!(val is int num))
				{
					if (!(val is Enum obj))
					{
						if (!(val is string text))
						{
							if (!(val is CExpression))
							{
								if (val is TValue tValue)
								{
									type = tValue.type;
									intVal = tValue.intVal;
									objVal = tValue.objVal;
								}
								else
								{
									type = EType.Object;
									intVal = 0;
									objVal = val;
								}
							}
							else
							{
								type = EType.Expression;
								intVal = 0;
								objVal = val;
							}
						}
						else
						{
							type = EType.String;
							intVal = 0;
							objVal = text;
						}
					}
					else
					{
						type = EType.Enum;
						intVal = Convert.ToInt32(obj);
						objVal = obj.GetType();
					}
				}
				else
				{
					type = EType.Int;
					intVal = num;
					objVal = null;
				}
			}
			else
			{
				type = EType.Bool;
				intVal = (flag ? 1 : 0);
				objVal = null;
			}
		}
		else
		{
			type = EType.Null;
			intVal = 0;
			objVal = null;
		}
	}

	public static TValue Enum<T>(T val) where T : Enum
	{
		return new TValue
		{
			type = EType.Enum,
			intVal = Convert.ToInt32(val),
			objVal = typeof(T)
		};
	}

	public static TValue Error(string err, bool log = true)
	{
		if (log)
		{
			Log.Error(err);
		}
		return new TValue
		{
			type = EType.Error,
			intVal = 0,
			objVal = err
		};
	}

	public string GetError()
	{
		if (!isError)
		{
			return null;
		}
		return objVal as string;
	}

	public static implicit operator TValue(bool val)
	{
		return new TValue(val);
	}

	public static implicit operator TValue(int val)
	{
		return new TValue(val);
	}

	public static implicit operator TValue(string val)
	{
		return new TValue(val);
	}

	public TValue Calc(CVarsContext context = null, bool asValue = true)
	{
		if (type != EType.Expression)
		{
			return this;
		}
		return (objVal as CExpression).Calc(context, asValue);
	}

	public bool GetBool(out bool val, CVarsContext context = null, bool defVal = false, bool calcExpression = true)
	{
		switch (type)
		{
		case EType.Unknown:
		case EType.Error:
		case EType.Null:
			val = defVal;
			return false;
		case EType.Bool:
		case EType.Int:
		case EType.Enum:
			val = intVal != 0;
			return true;
		case EType.String:
		case EType.Object:
			val = true;
			return false;
		case EType.Expression:
			if (!calcExpression)
			{
				val = true;
				return false;
			}
			if (objVal is CExpression cExpression)
			{
				return cExpression.Calc(context).GetBool(out val, context, defVal, calcExpression);
			}
			break;
		}
		val = defVal;
		return false;
	}

	public bool GetBool(CVarsContext context = null, bool defVal = false, bool calcExpression = true)
	{
		GetBool(out var val, context, defVal, calcExpression);
		return val;
	}

	public bool GetInt(out int val, CVarsContext context = null, int defVal = 0, bool calcExpression = true)
	{
		switch (type)
		{
		case EType.Bool:
		case EType.Int:
		case EType.Enum:
			val = intVal;
			return true;
		case EType.Expression:
			if (!calcExpression)
			{
				val = defVal;
				return false;
			}
			if (objVal is CExpression cExpression)
			{
				return cExpression.Calc(context).GetInt(out val, context, defVal, calcExpression);
			}
			break;
		}
		val = defVal;
		return false;
	}

	public int GetInt(CVarsContext context = null, int defVal = 0, bool calcExpression = true)
	{
		GetInt(out var val, context, defVal, calcExpression);
		return val;
	}

	public bool GetEnum<T>(out T val, CVarsContext context = null, T defVal = default(T), bool calcExpression = true) where T : Enum
	{
		switch (type)
		{
		case EType.Bool:
		case EType.Int:
		case EType.Enum:
			val = (T)System.Enum.ToObject(typeof(T), intVal);
			return true;
		case EType.Expression:
			if (!calcExpression)
			{
				val = defVal;
				return false;
			}
			if (objVal is CExpression cExpression)
			{
				return cExpression.Calc(context).GetEnum(out val, context, defVal, calcExpression);
			}
			break;
		}
		val = defVal;
		return false;
	}

	public T GetEnum<T>(CVarsContext context = null, T defVal = default(T), bool calcExpression = true) where T : Enum
	{
		GetEnum(out var val, context, defVal, calcExpression);
		return val;
	}

	public bool GetString(out string val, CVarsContext context = null, string defVal = null, bool calcExpression = true)
	{
		switch (type)
		{
		case EType.Unknown:
		case EType.Error:
		case EType.Null:
			val = defVal;
			return false;
		case EType.Bool:
			val = ((intVal == 0) ? "false" : "true");
			return false;
		case EType.Int:
			val = intVal.ToString();
			return false;
		case EType.Enum:
			val = System.Enum.ToObject(objVal as Type, intVal).ToString();
			return false;
		case EType.String:
			val = objVal as string;
			return true;
		case EType.Object:
			val = objVal?.ToString();
			return false;
		case EType.Expression:
			if (!calcExpression)
			{
				val = defVal;
				return false;
			}
			if (objVal is CExpression cExpression)
			{
				return cExpression.Calc(context).GetString(out val, context, defVal, calcExpression);
			}
			break;
		}
		val = defVal;
		return false;
	}

	public string GetString(CVarsContext context = null, string defVal = null, bool calcExpression = true)
	{
		GetString(out var val, context, defVal, calcExpression);
		return val;
	}

	public bool GetObject(out object val, CVarsContext context = null, object defVal = null, bool calcExpression = true)
	{
		switch (type)
		{
		case EType.Unknown:
		case EType.Error:
			val = defVal;
			return false;
		case EType.Null:
			val = null;
			return true;
		case EType.Bool:
			val = intVal != 0;
			return false;
		case EType.Int:
			val = intVal;
			return false;
		case EType.Enum:
			val = System.Enum.ToObject(objVal as Type, intVal);
			return false;
		case EType.String:
			val = objVal as string;
			return true;
		case EType.Object:
			val = objVal;
			return true;
		case EType.Expression:
			if (!calcExpression)
			{
				val = objVal;
				return false;
			}
			if (objVal is CExpression cExpression)
			{
				return cExpression.Calc(context).GetObject(out val, context, defVal, calcExpression);
			}
			break;
		}
		val = defVal;
		return false;
	}

	public T GetObject<T>(CVarsContext context = null, T defVal = null, bool calcExpression = true) where T : class
	{
		GetObject(out var val, context, defVal, calcExpression);
		return val as T;
	}

	public bool Equals(TValue val)
	{
		if (val.type == type && val.intVal == intVal)
		{
			return val.objVal == objVal;
		}
		return false;
	}

	public static bool operator ==(TValue val1, TValue val2)
	{
		return val1.Equals(val2);
	}

	public static bool operator !=(TValue val1, TValue val2)
	{
		return !val1.Equals(val2);
	}

	public override bool Equals(object val)
	{
		if (val is TValue val2)
		{
			return Equals(val2);
		}
		return false;
	}

	public bool Eq(TValue val)
	{
		switch (type)
		{
		case EType.Unknown:
			return val.isUnknown;
		case EType.Error:
			if (val.isError)
			{
				return GetError() == val.GetError();
			}
			return false;
		case EType.Null:
			return val.isNull;
		case EType.Bool:
		case EType.Int:
			if (val.isConvertibleToInt)
			{
				return intVal == val.intVal;
			}
			return false;
		case EType.Enum:
			if (val.isConvertibleToInt)
			{
				return intVal == val.intVal;
			}
			if (val.isString)
			{
				return GetString() == val.GetString();
			}
			return false;
		case EType.String:
			if (val.isString || val.isEnum)
			{
				return GetString() == val.GetString();
			}
			return false;
		case EType.Object:
			if (val.isObject)
			{
				return objVal == val.objVal;
			}
			return false;
		case EType.Expression:
			if (val.isExpression)
			{
				return objVal == val.objVal;
			}
			return false;
		default:
			return false;
		}
	}

	public override int GetHashCode()
	{
		switch (type)
		{
		case EType.Unknown:
		case EType.Error:
		case EType.Null:
			return 0;
		case EType.Bool:
		case EType.Int:
		case EType.Enum:
			return intVal.GetHashCode();
		default:
			return objVal.GetHashCode();
		}
	}

	public static string FriendlyTypeName(Type cs_type, bool keep_prefixes = false)
	{
		if (cs_type == null)
		{
			return "<null_type>";
		}
		if (cs_type == typeof(void))
		{
			return "void";
		}
		if (cs_type == typeof(byte))
		{
			return "byte";
		}
		if (cs_type == typeof(bool))
		{
			return "bool";
		}
		if (cs_type == typeof(int))
		{
			return "int";
		}
		if (cs_type == typeof(short))
		{
			return "short";
		}
		if (cs_type == typeof(ushort))
		{
			return "ushort";
		}
		if (cs_type == typeof(uint))
		{
			return "uint";
		}
		if (cs_type == typeof(long))
		{
			return "long";
		}
		if (cs_type == typeof(ulong))
		{
			return "ulong";
		}
		if (cs_type == typeof(float))
		{
			return "float";
		}
		if (cs_type == typeof(double))
		{
			return "double";
		}
		if (cs_type == typeof(string))
		{
			return "string";
		}
		string fullName = cs_type.FullName;
		if (cs_type.IsArray)
		{
			fullName = FriendlyTypeName(cs_type.GetElementType(), keep_prefixes) + "[";
			int arrayRank = cs_type.GetArrayRank();
			for (int i = 1; i < arrayRank; i++)
			{
				fullName += ",";
			}
			return fullName + "]";
		}
		if (fullName == null)
		{
			return "<invalid_type>";
		}
		fullName = fullName.Replace('+', '.');
		for (int j = 0; j < friendly_type_prefixes.Length; j++)
		{
			string text = friendly_type_prefixes[j];
			if (fullName.StartsWith(text, StringComparison.Ordinal))
			{
				fullName = fullName.Substring(text.Length);
			}
		}
		if (!keep_prefixes)
		{
			for (int k = 0; k < optional_friendly_type_prefixes.Length; k++)
			{
				string text2 = optional_friendly_type_prefixes[k];
				if (fullName.StartsWith(text2, StringComparison.Ordinal))
				{
					fullName = fullName.Substring(text2.Length);
				}
			}
		}
		if (cs_type.IsGenericType)
		{
			int num = fullName.IndexOf('`');
			if (num >= 0)
			{
				fullName = fullName.Substring(0, num);
			}
			fullName += "<";
			Type[] genericArguments = cs_type.GetGenericArguments();
			for (int l = 0; l < genericArguments.Length; l++)
			{
				string text3 = FriendlyTypeName(genericArguments[l], keep_prefixes);
				if (l > 0)
				{
					fullName += ", ";
				}
				fullName += text3;
			}
			return fullName + ">";
		}
		return fullName;
	}

	public static string ToString(object obj)
	{
		try
		{
			if (obj != null)
			{
				if (!(obj is DictionaryEntry dictionaryEntry))
				{
					if (!(obj is Delegate obj2))
					{
						if (!(obj is Array array))
						{
							if (!(obj is IList list))
							{
								if (!(obj is IDictionary dictionary))
								{
									if (obj is ICollection collection)
									{
										return $"{FriendlyTypeName(obj.GetType())} [{collection.Count}]";
									}
									return obj.ToString();
								}
								return $"{FriendlyTypeName(obj.GetType())} [{dictionary.Count}]";
							}
							return $"{FriendlyTypeName(obj.GetType())} [{list.Count}]";
						}
						string text = FriendlyTypeName(obj.GetType()) + " [";
						for (int i = 0; i < array.Rank; i++)
						{
							if (i > 0)
							{
								text += ", ";
							}
							text += array.GetLength(i);
						}
						return text + "]";
					}
					Delegate[] invocationList = obj2.GetInvocationList();
					if (invocationList.Length == 1)
					{
						if (obj2.Target != null)
						{
							return $"{FriendlyTypeName(obj.GetType())}({obj2.Target}.{obj2.Method.Name})";
						}
						return $"{FriendlyTypeName(obj.GetType())}({obj2.Method.DeclaringType}.{obj2.Method.Name})";
					}
					return $"{FriendlyTypeName(obj.GetType())} [{invocationList.Length}]";
				}
				return "(" + ToString(dictionaryEntry.Key) + " -> " + ToString(dictionaryEntry.Value);
			}
			return "null";
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public static void Dump(object obj, StringBuilder sb, string ident = "", string new_line = "\n", string tab = "\t", bool verbose = true, List<object> alreadyDumped = null)
	{
		if (alreadyDumped == null)
		{
			alreadyDumped = new List<object>();
		}
		else if (alreadyDumped.Contains(obj))
		{
			sb.Append(ToString(obj));
			return;
		}
		if (obj is TValue { isObject: not false } tValue)
		{
			tValue.GetObject(out obj);
		}
		if (obj is Delegate obj2)
		{
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
			ident += tab;
			Delegate[] invocationList = obj2.GetInvocationList();
			if (invocationList.Length > 1)
			{
				for (int i = 0; i < invocationList.Length; i++)
				{
					sb.Append($"{new_line}{ident}[{i}]: ");
					Delegate obj3 = invocationList[i];
					sb.Append(ToString(obj3));
				}
			}
			return;
		}
		if (obj is Array { Rank: >1 } array)
		{
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
			ident += tab;
			MultiDimentionalArrayEnumerator multiDimentionalArrayEnumerator = new MultiDimentionalArrayEnumerator(array);
			while (multiDimentionalArrayEnumerator.MoveNext())
			{
				sb.Append(new_line + ident + "[" + multiDimentionalArrayEnumerator.IndexString() + "]: ");
				object current = multiDimentionalArrayEnumerator.Current;
				if (verbose)
				{
					Dump(current, sb, ident, new_line, tab, verbose, alreadyDumped);
				}
				else
				{
					sb.Append(ToString(current));
				}
			}
			return;
		}
		if (obj is IList list)
		{
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
			ident += tab;
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				sb.Append($"{new_line}{ident}[{j}]: ");
				object obj4 = list[j];
				if (verbose)
				{
					Dump(obj4, sb, ident, new_line, tab, verbose, alreadyDumped);
				}
				else
				{
					sb.Append(ToString(obj4));
				}
			}
			return;
		}
		if (obj is IDictionary dictionary)
		{
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
			ident += tab;
			{
				foreach (object key in dictionary.Keys)
				{
					sb.Append(new_line + ident + "[" + ToString(key) + "]: ");
					object obj5 = dictionary[key];
					if (verbose)
					{
						Dump(obj5, sb, ident, new_line, tab, verbose, alreadyDumped);
					}
					else
					{
						sb.Append(ToString(obj5));
					}
				}
				return;
			}
		}
		if (obj is ICollection collection)
		{
			Array array2 = new object[collection.Count];
			collection.CopyTo(array2, 0);
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
			ident += tab;
			MultiDimentionalArrayEnumerator multiDimentionalArrayEnumerator2 = new MultiDimentionalArrayEnumerator(array2);
			while (multiDimentionalArrayEnumerator2.MoveNext())
			{
				sb.Append(new_line + ident + "[" + multiDimentionalArrayEnumerator2.IndexString() + "]: ");
				object current3 = multiDimentionalArrayEnumerator2.Current;
				if (verbose)
				{
					Dump(current3, sb, ident, new_line, tab, verbose, alreadyDumped);
				}
				else
				{
					sb.Append(ToString(current3));
				}
			}
		}
		else if (obj is CVars cVars)
		{
			cVars.Dump(sb, ident, new_line, tab, verbose, alreadyDumped);
		}
		else if (obj is CVarsContext cVarsContext)
		{
			cVarsContext.Dump(sb, ident, new_line, tab, verbose, alreadyDumped);
		}
		else
		{
			alreadyDumped.Add(obj);
			sb.Append(ToString(obj));
		}
	}

	public static string Dump(object obj, string ident = "", string new_line = "\n", string tab = "\t", bool verbose = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Dump(obj, stringBuilder, ident, new_line, tab, verbose);
		return stringBuilder.ToString();
	}

	public string ValueString()
	{
		switch (type)
		{
		case EType.Unknown:
			return "Unknown";
		case EType.Null:
			return "Null";
		case EType.Error:
			return objVal as string;
		case EType.Bool:
			if (intVal != 0)
			{
				return "true";
			}
			return "false";
		case EType.Int:
			return intVal.ToString();
		case EType.Enum:
			return System.Enum.ToObject(objVal as Type, intVal)?.ToString();
		case EType.String:
			return objVal as string;
		default:
			return ToString(objVal);
		}
	}

	public string LogString()
	{
		switch (type)
		{
		case EType.Unknown:
			return "Unknown";
		case EType.Null:
			return "Null";
		case EType.Error:
			return "Error(" + (objVal as string) + ")";
		case EType.Bool:
			if (intVal != 0)
			{
				return "true";
			}
			return "false";
		case EType.Int:
			return intVal.ToString();
		case EType.Enum:
			return System.Enum.ToObject(objVal as Type, intVal)?.ToString();
		case EType.String:
			return "\"" + (objVal as string) + "\"";
		default:
			return ToString(objVal);
		}
	}

	public override string ToString()
	{
		switch (type)
		{
		case EType.Unknown:
			return "Unknown";
		case EType.Null:
			return "Null";
		case EType.Error:
			return "Error(" + (objVal as string) + ")";
		case EType.Bool:
			if (intVal != 0)
			{
				return "Bool(true)";
			}
			return "Bool(false)";
		case EType.Int:
			return $"Int({intVal})";
		case EType.Enum:
			return string.Format("{0}({1})", (objVal as Type)?.Name ?? "Enum", System.Enum.ToObject(objVal as Type, intVal));
		case EType.String:
			return "String(" + (objVal as string) + ")";
		case EType.Expression:
			return $"CExpression({objVal})";
		default:
			return ToString(objVal);
		}
	}
}
