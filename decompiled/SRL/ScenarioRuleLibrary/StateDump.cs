using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using AStar;
using Libs.Utils;

namespace ScenarioRuleLibrary;

public class StateDump
{
	public delegate void FNOnObjectAdded(string key, object obj);

	private class RefComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private struct TFieldInfo
	{
		public string Name;

		public StateDumpAttribute Attr;

		public FieldInfo Field;

		public TFieldInfo(FieldInfo field)
		{
			Name = field.Name;
			Attr = field.GetCustomAttribute<StateDumpAttribute>();
			if (Attr == null)
			{
				Attr = defaultAttribute;
			}
			Field = field;
			if (Attr.Skip || !Name.EndsWith(">k__BackingField", StringComparison.Ordinal))
			{
				return;
			}
			Name = Name.Substring(1, Name.Length - 17);
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
			bindingFlags = ((!field.IsStatic) ? (bindingFlags | BindingFlags.Instance) : (bindingFlags | BindingFlags.Static));
			PropertyInfo property = field.DeclaringType.GetProperty(Name, bindingFlags);
			if (!(property == null))
			{
				Attr = property.GetCustomAttribute<StateDumpAttribute>();
				if (Attr == null)
				{
					Attr = defaultAttribute;
				}
			}
		}

		public override string ToString()
		{
			return "TFieldInfo(" + Name + ")";
		}
	}

	private class CTypeInfo
	{
		public readonly Type Type;

		public readonly string Name;

		public readonly StateDumpAttribute Attr;

		public readonly bool IsSimple;

		public readonly bool IsClass;

		private IStateDumper m_customDumper;

		private List<TFieldInfo> m_instanceFields;

		private List<TFieldInfo> m_staticFields;

		private CTypeInfo m_baseType;

		private static Dictionary<Type, CTypeInfo> s_registry = new Dictionary<Type, CTypeInfo>();

		public List<TFieldInfo> InstanceFields
		{
			get
			{
				if (m_instanceFields == null)
				{
					AnalyzeFields();
				}
				return m_instanceFields;
			}
		}

		public List<TFieldInfo> StaticFields
		{
			get
			{
				if (m_staticFields == null)
				{
					AnalyzeFields();
				}
				return m_staticFields;
			}
		}

		public CTypeInfo BaseType
		{
			get
			{
				if (m_baseType != null)
				{
					return m_baseType;
				}
				Type baseType = Type.BaseType;
				if (baseType == null)
				{
					return null;
				}
				m_baseType = Get(baseType);
				return m_baseType;
			}
		}

		public IStateDumper CustomDumper
		{
			get
			{
				for (CTypeInfo cTypeInfo = this; cTypeInfo != null; cTypeInfo = cTypeInfo.BaseType)
				{
					if (cTypeInfo.m_customDumper != null)
					{
						return cTypeInfo.m_customDumper;
					}
				}
				return null;
			}
			set
			{
				m_customDumper = value;
			}
		}

		public CTypeInfo(Type type)
		{
			Type = type;
			Name = TValue.FriendlyTypeName(type);
			IsSimple = IsSimpleType(type);
			IsClass = type.IsClass;
			if (!IsSimple)
			{
				Attr = type.GetCustomAttribute<StateDumpAttribute>();
			}
			if (Attr == null)
			{
				Attr = defaultAttribute;
			}
		}

		public override string ToString()
		{
			return "CTypeInfo(" + TValue.FriendlyTypeName(Type) + ")";
		}

		private static bool IsSimpleType(Type type)
		{
			if (type == null)
			{
				return true;
			}
			if (type == typeof(string))
			{
				return true;
			}
			if (type.IsPrimitive)
			{
				return true;
			}
			if (type.IsEnum)
			{
				return true;
			}
			if (type == typeof(Thread))
			{
				return true;
			}
			if (type == typeof(Guid))
			{
				return true;
			}
			if (type == typeof(Point))
			{
				return true;
			}
			if (type == typeof(TileIndex))
			{
				return true;
			}
			if (type == typeof(CVector3))
			{
				return true;
			}
			if (type == typeof(Tuple<int, int>))
			{
				return true;
			}
			if (type.FullName.StartsWith("UnityEngine.Vector", StringComparison.Ordinal))
			{
				return true;
			}
			if (type.FullName.StartsWith("UnityEngine.Color", StringComparison.Ordinal))
			{
				return true;
			}
			if (type.FullName.StartsWith("ScenarioRuleLibrary.YML.", StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}

		private void AnalyzeFields()
		{
			m_instanceFields = new List<TFieldInfo>();
			m_staticFields = new List<TFieldInfo>();
			FieldInfo[] fields = Type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsLiteral || (fieldInfo.IsInitOnly && fieldInfo.IsStatic))
				{
					continue;
				}
				TFieldInfo item = new TFieldInfo(fieldInfo);
				if (!item.Attr.Skip)
				{
					if (fieldInfo.IsStatic)
					{
						m_staticFields.Add(item);
					}
					else
					{
						m_instanceFields.Add(item);
					}
				}
			}
		}

		public static CTypeInfo Get(Type type)
		{
			if (type == null)
			{
				return null;
			}
			if (s_registry.TryGetValue(type, out var value))
			{
				return value;
			}
			value = new CTypeInfo(type);
			s_registry.Add(type, value);
			return value;
		}

		public static CTypeInfo Get(object obj)
		{
			return Get(obj?.GetType());
		}
	}

	private FNOnObjectAdded onObjectAdded;

	[ThreadStatic]
	public static bool IsDumping;

	private static Dictionary<Type, IStateDumper> CustomDumpers = new Dictionary<Type, IStateDumper>();

	private static RefComparer refComparer;

	private static StateDumpAttribute defaultAttribute = new StateDumpAttribute();

	private StringBuilder sb = new StringBuilder(1048576);

	private int ident;

	private List<string> currentPath = new List<string>();

	private Dictionary<object, int> referenceIDs = new Dictionary<object, int>(refComparer);

	private int lastReferenceID;

	private HashSet<object> alreadyDumpedObjects = new HashSet<object>(refComparer);

	private static readonly char[] c_NewLineChars = new char[2] { '\r', '\n' };

	public static void AddCustomDumper(Type type, IStateDumper dumper)
	{
		CTypeInfo.Get(type).CustomDumper = dumper;
	}

	public void Add(string key, object value, StateDumpAttribute fieldAttribute = null)
	{
		bool isDumping = IsDumping;
		IsDumping = true;
		try
		{
			onObjectAdded?.Invoke(key, value);
			if (fieldAttribute == null)
			{
				fieldAttribute = defaultAttribute;
			}
			if (fieldAttribute.Skip)
			{
				return;
			}
			if (value is TValue tValue)
			{
				value = tValue.GetObject<object>(null, null, calcExpression: false);
			}
			if (!IsDefaultValue(value))
			{
				string text = ResolveValue(value, out var typeInfo);
				if (text != null)
				{
					AddValue(key, text);
					DumpMembers(key, value, typeInfo, fieldAttribute);
				}
			}
		}
		catch (Exception arg)
		{
			Log.Error($"StateDump: Error dumping '{key}' of {TValue.ToString(value)}: {arg}");
		}
		finally
		{
			IsDumping = isDumping;
		}
	}

	public void Add(Type type)
	{
		bool isDumping = IsDumping;
		IsDumping = true;
		try
		{
			string text = TValue.FriendlyTypeName(type);
			StartNewLine(text);
			OpenSection(text);
			try
			{
				DumpStaticMembersUsingReflection(type);
			}
			catch (Exception ex)
			{
				Add("ERROR enumerating '" + text + "'", ex.Message);
			}
			CloseSection();
		}
		catch (Exception arg)
		{
			Log.Error($"StateDump: Error dumping {TValue.ToString(type)}: {arg}");
		}
		finally
		{
			IsDumping = isDumping;
		}
	}

	public string Finish()
	{
		return sb.ToString();
	}

	public static string Dump(object obj, string key = "obj", StateDumpAttribute fieldAttribute = null, FNOnObjectAdded onObjectAdded = null)
	{
		StateDump stateDump = new StateDump();
		stateDump.onObjectAdded = onObjectAdded;
		if (obj is Type type)
		{
			stateDump.Add(type);
		}
		else
		{
			stateDump.Add(key, obj, fieldAttribute);
		}
		return stateDump.Finish();
	}

	private int GetReferenceID(object obj)
	{
		if (!referenceIDs.TryGetValue(obj, out var value))
		{
			value = ++lastReferenceID;
			referenceIDs.Add(obj, value);
		}
		return value;
	}

	private static bool IsDefaultValue(object value)
	{
		if (value != null)
		{
			if (!(value is bool flag))
			{
				if (!(value is int num))
				{
					if (!(value is uint num2))
					{
						if (!(value is float num3))
						{
							if (!(value is Enum value2))
							{
								if (!(value is Point point))
								{
									if (!(value is ICollection collection))
									{
										if (!(value is CVars cVars))
										{
											if (value is CEnhancement cEnhancement)
											{
												return cEnhancement.Enhancement == EEnhancement.NoEnhancement;
											}
											return false;
										}
										return cVars.Empty();
									}
									return collection.Count == 0;
								}
								if (point.X == 0)
								{
									return point.Y == 0;
								}
								return false;
							}
							return Convert.ToInt32(value2) == 0;
						}
						return num3 == 0f;
					}
					return num2 == 0;
				}
				return num == 0;
			}
			return !flag;
		}
		return true;
	}

	private static StateDumpAttribute GetAttribute(Type type)
	{
		if (type == null)
		{
			return defaultAttribute;
		}
		StateDumpAttribute customAttribute = type.GetCustomAttribute<StateDumpAttribute>();
		if (customAttribute != null)
		{
			return customAttribute;
		}
		return defaultAttribute;
	}

	public void AddValue(string key, string value)
	{
		StartNewLine(key);
		sb.Append(value);
	}

	public void AddValue(string key, int value)
	{
		StartNewLine(key);
		sb.Append(value);
	}

	public void OpenSection(string key)
	{
		currentPath.Add(key);
		ident++;
	}

	public void CloseSection()
	{
		ident--;
		currentPath.RemoveLast();
	}

	public void StartNewLine(string key)
	{
		if (sb.Length > 0)
		{
			sb.AppendLine();
		}
		for (int i = 0; i < ident; i++)
		{
			sb.Append('\t');
		}
		for (int j = 0; j < currentPath.Count; j++)
		{
			string value = currentPath[j];
			sb.Append(value);
			if (j + 1 < currentPath.Count)
			{
				if (currentPath[j + 1][0] != '[')
				{
					sb.Append('.');
				}
			}
			else if (key[0] != '[')
			{
				sb.Append('.');
			}
		}
		sb.Append(key);
		sb.Append(": ");
	}

	public void Append(string data)
	{
		sb.Append(data);
	}

	public void Append(int data)
	{
		sb.Append(data);
	}

	private string ResolveValue(object obj, out CTypeInfo typeInfo)
	{
		try
		{
			typeInfo = CTypeInfo.Get(obj);
			if (typeInfo == null)
			{
				return "null";
			}
			if (obj is float num)
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
			if (obj is double num2)
			{
				return num2.ToString(CultureInfo.InvariantCulture);
			}
			if (obj is string s)
			{
				return "\"" + EscapeSpecialCharacters(s) + "\"";
			}
			if (obj is Delegate obj2)
			{
				Delegate[] invocationList = obj2.GetInvocationList();
				if (invocationList.Length == 1)
				{
					if (obj2.Target == null)
					{
						return TValue.FriendlyTypeName(obj2.Method.DeclaringType) + "." + obj2.Method.Name;
					}
					return TValue.FriendlyTypeName(obj2.Target.GetType()) + "." + obj2.Method.Name;
				}
				return $"{typeInfo.Name} [{invocationList.Length}]";
			}
			if (typeInfo.Attr.Skip)
			{
				return null;
			}
			string text = "";
			if (typeInfo.IsClass && !typeInfo.IsSimple)
			{
				int referenceID = GetReferenceID(obj);
				text = $"    # {referenceID:X8}";
			}
			if (obj is IDumpState dumpState)
			{
				string text2 = dumpState.ToStateDumpString(this);
				if (text2 == null)
				{
					return null;
				}
				text2 = EscapeSpecialCharacters(text2);
				return text2 + text;
			}
			if (!typeInfo.Attr.UseToString)
			{
				IStateDumper customDumper = typeInfo.CustomDumper;
				if (customDumper != null)
				{
					string text3 = customDumper.ToStateDumpString(this, obj);
					if (text3 == null)
					{
						return null;
					}
					text3 = EscapeSpecialCharacters(text3);
					return text3 + text;
				}
			}
			if (typeInfo.IsSimple)
			{
				return obj.ToString();
			}
			string text4 = ((!typeInfo.Attr.UseToString) ? typeInfo.Name : EscapeSpecialCharacters(obj.ToString()));
			if (obj is Array array)
			{
				string text5 = "";
				for (int i = 0; i < array.Rank; i++)
				{
					if (i > 0)
					{
						text5 += ", ";
					}
					text5 += array.GetLength(i);
				}
				text = " [" + text5 + "]" + text;
			}
			else if (obj is IList list)
			{
				text = $" [{list.Count}]{text}";
			}
			else if (obj is IDictionary dictionary)
			{
				text = $" [{dictionary.Count}]{text}";
			}
			else if (obj is ICollection collection)
			{
				text = $" [{collection.Count}]{text}";
			}
			return text4 + text;
		}
		catch (Exception ex)
		{
			typeInfo = null;
			return "ERROR(" + EscapeSpecialCharacters(ex.Message) + ")";
		}
	}

	private static string EscapeSpecialCharacters(string s)
	{
		return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
	}

	private void DumpMembers(string key, object obj, CTypeInfo typeInfo, StateDumpAttribute fieldAttribute)
	{
		if (typeInfo.IsSimple)
		{
			return;
		}
		bool flag;
		if (fieldAttribute.AlwaysDumpMembers || typeInfo.Attr.AlwaysDumpMembers)
		{
			flag = typeInfo.Attr.AlwaysDumpMembers || !alreadyDumpedObjects.Contains(obj);
		}
		else
		{
			if (fieldAttribute.NoMembers || typeInfo.Attr.NoMembers)
			{
				return;
			}
			if (typeInfo.IsClass)
			{
				if (alreadyDumpedObjects.Contains(obj))
				{
					sb.Append("    #ref");
					return;
				}
				flag = true;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			alreadyDumpedObjects.Add(obj);
		}
		OpenSection(key);
		try
		{
			if (obj is IDumpState dumpState)
			{
				dumpState.DumpMembers(this);
			}
			else if (typeInfo.CustomDumper != null)
			{
				typeInfo.CustomDumper.DumpMembers(this, obj);
			}
			else if (obj is Delegate obj2)
			{
				Delegate[] invocationList = obj2.GetInvocationList();
				if (invocationList.Length > 1)
				{
					List<string> list = new List<string>(invocationList.Length);
					foreach (Delegate obj3 in invocationList)
					{
						string item = ResolveValue(obj3, out var _);
						list.Add(item);
					}
					list.Sort(StringComparer.Ordinal);
					for (int j = 0; j < list.Count; j++)
					{
						string value = list[j];
						StartNewLine($"[{j}]");
						sb.Append(value);
					}
				}
			}
			else if (obj is Array { Rank: >1 } array)
			{
				MultiDimentionalArrayEnumerator multiDimentionalArrayEnumerator = new MultiDimentionalArrayEnumerator(array);
				while (multiDimentionalArrayEnumerator.MoveNext())
				{
					object current = multiDimentionalArrayEnumerator.Current;
					Add("[" + multiDimentionalArrayEnumerator.IndexString() + "]", current, fieldAttribute);
				}
			}
			else if (obj is IList { Count: var count } list2)
			{
				for (int k = 0; k < count; k++)
				{
					object value2 = list2[k];
					Add($"[{k}]", value2, fieldAttribute);
				}
			}
			else if (obj is IDictionary dictionary)
			{
				int num = 0;
				foreach (object key2 in dictionary.Keys)
				{
					string text = ResolveValue(key2, out var typeInfo3);
					if (text != null)
					{
						object value3 = dictionary[key2];
						if (!typeInfo3.IsSimple)
						{
							Add($"<key{num}>", key2, fieldAttribute);
							DumpMembers($"<key{num}>", key2, typeInfo3, fieldAttribute);
							Add($"<value{num}>", value3, fieldAttribute);
						}
						else
						{
							Add("[" + text + "]", value3, fieldAttribute);
						}
						num++;
					}
				}
			}
			else if (obj is ICollection collection)
			{
				int num2 = 0;
				foreach (object item2 in collection)
				{
					Add($"[{num2}]", item2, fieldAttribute);
					num2++;
				}
			}
			else if (obj is IEnumerable enumerable)
			{
				int num3 = 0;
				foreach (object item3 in enumerable)
				{
					Add($"[{num3}]", item3, fieldAttribute);
					num3++;
				}
			}
			else
			{
				DumpMembersUsingReflection(obj, typeInfo);
			}
		}
		catch (Exception ex)
		{
			Add("ERROR enumerating '" + key + "'", ex.Message);
		}
		CloseSection();
	}

	public void DumpMembersUsingReflection(object obj)
	{
		CTypeInfo typeInfo = CTypeInfo.Get(obj);
		DumpMembersUsingReflection(obj, typeInfo);
	}

	private void DumpMembersUsingReflection(object obj, CTypeInfo typeInfo)
	{
		while (typeInfo != null && !typeInfo.IsSimple && !typeInfo.Type.FullName.StartsWith("UnityEngine.", StringComparison.Ordinal))
		{
			DumpFieldsUsingReflection(typeInfo, obj, typeInfo.InstanceFields);
			typeInfo = typeInfo.BaseType;
		}
	}

	public void DumpStaticMembersUsingReflection(Type type)
	{
		CTypeInfo typeInfo = CTypeInfo.Get(type);
		DumpStaticMembersUsingReflection(typeInfo);
	}

	private void DumpStaticMembersUsingReflection(CTypeInfo typeInfo)
	{
		while (typeInfo != null && !typeInfo.IsSimple)
		{
			DumpFieldsUsingReflection(typeInfo, null, typeInfo.StaticFields);
			typeInfo = typeInfo.BaseType;
		}
	}

	private void DumpFieldsUsingReflection(CTypeInfo typeInfo, object obj, List<TFieldInfo> fields)
	{
		foreach (TFieldInfo field in fields)
		{
			object value;
			try
			{
				value = field.Field.GetValue(obj);
			}
			catch (Exception ex)
			{
				value = "ERROR(" + EscapeSpecialCharacters(ex.Message) + ")";
			}
			Add(field.Name, value, field.Attr);
		}
	}

	public static bool AnalyzeDiff(string dump1, string dump2, out string title, out string description, out bool majorDesync, string MajorDelimiterLine = null)
	{
		int startIdx = 0;
		int startIdx2 = 0;
		ReadOnlySpan<char> readOnlySpan = default;
		ReadOnlySpan<char> readOnlySpan2 = default;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = true;
		while (startIdx < dump1.Length || startIdx2 < dump2.Length)
		{
			readOnlySpan = ReadLine(dump1, ref startIdx);
			readOnlySpan2 = ReadLine(dump2, ref startIdx2);
			if (MajorDelimiterLine != null)
			{
				if (flag2 && readOnlySpan.SequenceEqual(MajorDelimiterLine))
				{
					flag2 = false;
					readOnlySpan = default;
				}
				if (flag3 && readOnlySpan2.SequenceEqual(MajorDelimiterLine))
				{
					flag3 = false;
					readOnlySpan2 = "";
				}
			}
			if (!readOnlySpan.SequenceEqual(readOnlySpan2))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			title = "DO NOT SEND!";
			description = "identical states";
			majorDesync = false;
			return false;
		}
		majorDesync = flag2 | flag3;
		ReadOnlySpan<char> readOnlySpan3 = GetKey(readOnlySpan);
		ReadOnlySpan<char> other = GetKey(readOnlySpan2);
		if (readOnlySpan3.SequenceEqual(other))
		{
			title = "different " + readOnlySpan3.ToString();
			description = readOnlySpan.ToString() + "\n---\n" + readOnlySpan2.ToString();
			return true;
		}
		if (readOnlySpan3.Length > 0 && other.Length > 0)
		{
			while ((startIdx < dump1.Length && other.Length > 0) || (startIdx2 < dump2.Length && readOnlySpan3.Length > 0))
			{
				if (startIdx < dump1.Length && GetKey(ReadLine(dump1, ref startIdx)).SequenceEqual(other))
				{
					other = default;
					readOnlySpan2 = default;
					break;
				}
				if (startIdx2 < dump2.Length && GetKey(ReadLine(dump2, ref startIdx2)).SequenceEqual(readOnlySpan3))
				{
					readOnlySpan3 = default;
					readOnlySpan = default;
					break;
				}
			}
		}
		title = readOnlySpan3.ToString() + " <---> " + other.ToString();
		description = readOnlySpan.ToString() + "\n---\n" + readOnlySpan2.ToString();
		return true;
	}

	private static ReadOnlySpan<char> ReadLine(string text, ref int startIdx)
	{
		if (startIdx >= text.Length)
		{
			return default;
		}
		int num = text.IndexOfAny(c_NewLineChars, startIdx);
		if (num < 0)
		{
			num = text.Length;
		}
		ReadOnlySpan<char> span = text.AsSpan(startIdx, num - startIdx);
		startIdx = num + 1;
		if (startIdx < text.Length && text[startIdx] == 23 - text[startIdx - 1])
		{
			startIdx++;
		}
		return span.Trim();
	}

	private static ReadOnlySpan<char> GetKey(ReadOnlySpan<char> line)
	{
		int num = line.IndexOf(':');
		if (num < 0)
		{
			return line;
		}
		return line.Slice(0, num);
	}

	public static string Test()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		StateDump stateDump = new StateDump();
		stateDump.Add("Scenario.AllActors", ScenarioManager.Scenario.AllActors);
		stateDump.Add("Scenario", ScenarioManager.Scenario);
		stateDump.Add("CurrentScenarioState", ScenarioManager.CurrentScenarioState);
		stateDump.Add(typeof(ScenarioManager));
		stateDump.Add(typeof(GameState));
		stateDump.Add(typeof(PhaseManager));
		string contents = stateDump.Finish();
		long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
		File.WriteAllText("../StateDumps/state.yml", contents);
		return $"State dumped in {elapsedMilliseconds}ms";
	}
}
