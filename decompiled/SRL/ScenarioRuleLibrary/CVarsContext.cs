using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class CVarsContext : IGetVar, ICallMethod, IGetItemByIndex
{
	[JsonProperty(IsReference = true)]
	public object obj;

	[JsonProperty(IsReference = true)]
	public CVarsContext parentContext;

	private static int s_LastID = 0;

	private static Dictionary<string, Type> s_TypesByName;

	private static HashSet<Type> s_RegisteredTypes;

	private static Dictionary<string, List<Type>> s_TypesPerNamespace;

	private static string[] s_BlackListedTypePrefixes = new string[125]
	{
		"System.IO", "System.Net", "System.Web", "System.Reflection", "System.Runtime", "System.Security", "System.Diagnostics", "System.Threading", "System.Resources", "System.Buffers",
		"System.Environment", "System.AppDomain", "System.Activator", "System.OperatingSystem", "System.Platform", "System.CodeDom", "System.Linq.Expressions", "System.ComponentModel", "System.Configuration", "System.Data",
		"System.Drawing", "System.Dynamic", "System.Management", "System.Transactions", "System.Xml", "Internal", "Mono", "Microsoft", "NativeCompiler", "OpenUrl",
		"RuntimeSupport", "ScriptBatch", "AsmodeeNet", "AssetBundleBrowser", "Backtrace", "Base", "Bee", "BestHTTP", "hronos.Reflection", "com.daysofwonder",
		"DevLocker", "DG", "ENet", "Epic", "ExCSS", "ExitGames", "FH.Platforms", "FullscreenEditor", "Fusion", "Photon",
		"Galaxy", "Google", "Ionic", "LeTai", "LitJson", "Manatee", "MoonSharp", "MS", "Newtonsoft", "NiceIO",
		"NUnit", "Playeveryware", "RuntimeInspectorNamespace", "ScriptCompilationBuildProgram", "SETUtil", "SevenZip", "SimpleJSON", "Steamworks", "TinyJson", "UdpKit",
		"Libs", "UnityEngine.IO", "UnityEngine.WWW", "UnityEngine.WWWForm", "UnityEngine.WWWTranscoder", "UnityEngine.AddressableAssets", "UnityEngine.Advertisements", "UnityEngine.Application", "UnityEngine.Bindings", "UnityEngine.Windows",
		"UnityEngine.Device", "UnityEngine.Network", "UnityEngine.Networking", "UnityEngine.RPC", "UnityEngine.Events", "UnityEngine.EventSystems", "UnityEngine.InputSystem", "UnityEngine.BitStream", "UnityEngine.ScriptingRuntime", "UnityEngine.ScriptingUtility",
		"UnityEngine.Analytics", "UnityEngine.Diagnostics", "UnityEngine.LowLevel", "UnityEngine.Experimental", "UnityEngine.WSA", "UnityEngine.Serialization", "UnityEngine.TestTools", "UnityEngine.TestRunner", "UnityEngine.Social", "UnityEngine.SocialPlatforms",
		"UnityEngine.NVIDIA", "UnityEngine.Playables", "UnityEngine.PlayerLoop", "UnityEngine.Profiling", "UnityEngine.ResourceManagement", "UnityEngine.XR", "UnityEngine.Internal", "UnityEngineInternal", "Unity.IO", "Unity.Collections.LowLevel.Unsafe",
		"Unity.Burst", "Unity.CodeEditor", "Unity.CompilationPipeline", "Unity.Curl", "Unity.Loading", "Unity.MemoryProfiler", "Unity.PerformanceTesting", "Unity.Properties", "UnityTools", "VSCodeEditor",
		"WebSocketSharp", "Zxcvbn", "zzzUnity", "UnityEditor", "UnityEditorInternal"
	};

	private static Type[] s_ListMethodArgs = new Type[4]
	{
		typeof(IList),
		typeof(List<CExpression>),
		typeof(CVarsContext),
		typeof(bool)
	};

	[JsonProperty]
	public int ID { get; private set; }

	public CVarsContext(object obj = null, CVarsContext parentContext = null, int ID = -1)
	{
		this.ID = ((ID >= 0) ? ID : (++s_LastID));
		this.obj = obj;
		this.parentContext = parentContext;
	}

	public override string ToString()
	{
		return $"CVarsContext(ID: {ID}, obj: {TValue.ToString(obj)}), parentContext: {parentContext}";
	}

	public void Dump(StringBuilder sb, string ident = "", string new_line = "\n", string tab = "\t", bool verbose = true, List<object> alreadyDumped = null)
	{
		sb.Append($"ID: {ID}");
		if (alreadyDumped == null)
		{
			alreadyDumped = new List<object>();
		}
		else if (alreadyDumped.Contains(this))
		{
			return;
		}
		alreadyDumped.Add(this);
		sb.Append(new_line + ident + "obj: ");
		if (verbose)
		{
			TValue.Dump(obj, sb, ident, new_line, tab, verbose, alreadyDumped);
		}
		else
		{
			sb.Append(TValue.ToString(obj));
		}
		if (parentContext != null)
		{
			sb.Append(new_line + ident + "parentContext:");
			ident += tab;
			sb.Append(new_line + ident);
			parentContext.Dump(sb, ident, new_line, tab, verbose, alreadyDumped);
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

	public string DumpContext()
	{
		return Dump();
	}

	public TValue GetVar(string key, CVarsContext context = null, bool asValue = true)
	{
		if (obj != null)
		{
			TValue objectVar = GetObjectVar(obj, key, context ?? this, asValue);
			if (!objectVar.isUnknown)
			{
				return objectVar;
			}
		}
		switch (key)
		{
		case "obj":
			return new TValue(obj);
		case "parentContext":
			return new TValue(parentContext);
		case "thisContext":
			return new TValue(this);
		default:
			if (parentContext != null)
			{
				TValue var = parentContext.GetVar(key, context ?? this, asValue);
				if (!var.isUnknown)
				{
					return var;
				}
			}
			else
			{
				TValue globalVar = GetGlobalVar(key, context ?? this, asValue);
				if (!globalVar.isUnknown)
				{
					return globalVar;
				}
			}
			return TValue.Unknown;
		}
	}

	public static TValue GetObjectVar(object obj, string key, CVarsContext context = null, bool asValue = true)
	{
		if (obj == null)
		{
			return TValue.Unknown;
		}
		if (obj is IGetVar getVar)
		{
			TValue var = getVar.GetVar(key, context, asValue);
			if (!var.isUnknown)
			{
				return var;
			}
		}
		if (obj is Type type)
		{
			if (!IsValidType(type))
			{
				return TValue.Unknown;
			}
			return GetStaticVar(type, key, context, asValue);
		}
		if (obj is IList list)
		{
			switch (key)
			{
			case "count":
			case "Count":
			case "length":
			case "Length":
				return list.Count;
			case "empty":
			case "Empty":
				return list.Count == 0;
			case "first":
			case "First":
				if (list.Count == 0)
				{
					return TValue.Unknown;
				}
				return new TValue(list[0]);
			case "last":
			case "Last":
			{
				int count = list.Count;
				if (count == 0)
				{
					return TValue.Unknown;
				}
				return new TValue(list[count - 1]);
			}
			}
		}
		if (obj is IDictionary dictionary)
		{
			if (key == null)
			{
				return TValue.Error("Attempt to index dictionary with null");
			}
			if (!dictionary.Contains(key))
			{
				return TValue.Unknown;
			}
			return new TValue(dictionary[key]);
		}
		if (obj is string text)
		{
			switch (key)
			{
			case "count":
			case "Count":
			case "length":
			case "Length":
				return text.Length;
			case "empty":
			case "Empty":
				return text.Length == 0;
			}
		}
		Type type2 = obj.GetType();
		while (type2 != null && IsValidType(type2))
		{
			PropertyInfo property = type2.GetProperty(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				object value = property.GetValue(obj);
				return new TValue(value);
			}
			FieldInfo field = type2.GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return new TValue(field.GetValue(obj));
			}
			type2 = type2.BaseType;
		}
		return TValue.Unknown;
	}

	private static void BuildTypeRegistry()
	{
		if (s_TypesByName != null)
		{
			return;
		}
		s_TypesByName = new Dictionary<string, Type>();
		s_RegisteredTypes = new HashSet<Type>();
		Assembly assembly = typeof(CVarsContext).Assembly;
		RegisterTypes(assembly);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			if (!(assembly2 == assembly))
			{
				RegisterTypes(assembly2);
			}
		}
		if (s_TypesPerNamespace == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = s_TypesPerNamespace.Keys.ToList();
		list.Sort();
		foreach (string item in list)
		{
			stringBuilder.AppendLine(item);
			List<Type> list2 = s_TypesPerNamespace[item];
			list2.Sort((Type a, Type b) => a.FullName.CompareTo(b.FullName));
			foreach (Type item2 in list2)
			{
				stringBuilder.AppendLine("\t" + item2.FullName);
			}
		}
		File.WriteAllText("types.txt", stringBuilder.ToString());
	}

	private static void RegisterTypes(Assembly assembly)
	{
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.IsGenericType || type.DeclaringType != null)
			{
				continue;
			}
			string name = type.Name;
			if (string.IsNullOrEmpty(name) || IsBlackListed(type))
			{
				continue;
			}
			s_TypesByName.TryAdd(name, type);
			s_RegisteredTypes.Add(type);
			if (s_TypesPerNamespace != null)
			{
				string text = type.Namespace;
				if (text == null)
				{
					text = "<null>";
				}
				else if (text == "")
				{
					text = "<empty>";
				}
				if (!s_TypesPerNamespace.TryGetValue(text, out var value))
				{
					value = new List<Type>();
					s_TypesPerNamespace.Add(text, value);
				}
				value.Add(type);
			}
		}
	}

	private static bool IsBlackListed(Type type)
	{
		string fullName = type.FullName;
		if (string.IsNullOrEmpty(fullName))
		{
			return true;
		}
		string[] array = s_BlackListedTypePrefixes;
		foreach (string text in array)
		{
			if (fullName.StartsWith(text, StringComparison.Ordinal))
			{
				if (fullName.Length == text.Length)
				{
					return true;
				}
				if (fullName[text.Length] == '.')
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsValidType(Type type)
	{
		BuildTypeRegistry();
		if (s_RegisteredTypes.Contains(type))
		{
			return true;
		}
		if (IsBlackListed(type))
		{
			return false;
		}
		return true;
	}

	public static TValue GetGlobalVar(string key, CVarsContext context = null, bool asValue = true)
	{
		TValue staticVar = GetStaticVar(typeof(CExpression.Globals), key, context, asValue);
		if (!staticVar.isUnknown)
		{
			return staticVar;
		}
		BuildTypeRegistry();
		if (s_TypesByName.TryGetValue(key, out var value))
		{
			return new TValue(value);
		}
		return TValue.Unknown;
	}

	public static TValue GetStaticVar(Type type, string key, CVarsContext context = null, bool asValue = true)
	{
		PropertyInfo property = type.GetProperty(key, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (property != null)
		{
			object value = property.GetValue(null);
			return new TValue(value);
		}
		FieldInfo field = type.GetField(key, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (field != null)
		{
			return new TValue(field.GetValue(null));
		}
		Type nestedType = type.GetNestedType(key);
		if (nestedType != null)
		{
			return new TValue(nestedType);
		}
		return TValue.Unknown;
	}

	public TValue CallMethod(string key, List<TValue> args, CVarsContext context = null, bool asValue = true)
	{
		if (obj != null)
		{
			TValue result = CallObjectMethod(obj, key, args, context ?? this, asValue);
			if (!result.isUnknown)
			{
				return result;
			}
		}
		if (parentContext != null)
		{
			TValue result2 = parentContext.CallMethod(key, args, context ?? this, asValue);
			if (!result2.isUnknown)
			{
				return result2;
			}
		}
		else
		{
			TValue result3 = CallGlobalFunction(key, args, context ?? this, asValue);
			if (!result3.isUnknown)
			{
				return result3;
			}
		}
		return TValue.Unknown;
	}

	public static TValue CallObjectMethod(object obj, string key, List<TValue> args, CVarsContext context = null, bool asValue = true)
	{
		if (obj == null)
		{
			return TValue.Unknown;
		}
		if (obj is ICallMethod callMethod)
		{
			TValue result = callMethod.CallMethod(key, args, context, asValue);
			if (!result.isUnknown)
			{
				return result;
			}
		}
		if (obj is Type type)
		{
			return CallStaticMethod(type, key, args, context, asValue);
		}
		Type type2 = obj.GetType();
		if (!IsValidType(type2))
		{
			return TValue.Unknown;
		}
		TValue result2 = TryCallMethod(type2.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), key, obj, args, context, asValue);
		if (!result2.isUnknown)
		{
			return result2;
		}
		return TValue.Unknown;
	}

	public static TValue CallGlobalFunction(string key, List<TValue> args, CVarsContext context = null, bool asValue = true)
	{
		TValue result = CallStaticMethod(typeof(CExpression.Globals), key, args, context, asValue);
		if (!result.isUnknown)
		{
			return result;
		}
		return TValue.Unknown;
	}

	public static TValue CallStaticMethod(Type type, string key, List<TValue> args, CVarsContext context = null, bool asValue = true)
	{
		if (!IsValidType(type))
		{
			return TValue.Unknown;
		}
		TValue result = TryCallMethod(type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), key, null, args, context, asValue);
		if (!result.isUnknown)
		{
			return result;
		}
		return TValue.Unknown;
	}

	public static TValue CallListMethod(IList lst, string key, List<CExpression> args, CVarsContext context = null, bool asValue = true)
	{
		MethodInfo method = typeof(CExpression.ListMethods).GetMethod(key, s_ListMethodArgs);
		if (method == null)
		{
			return TValue.Unknown;
		}
		object[] parameters = new object[4] { lst, args, context, asValue };
		object val = method.Invoke(null, parameters);
		return new TValue(val);
	}

	private static TValue TryCallMethod(MethodInfo[] methods, string name, object obj, List<TValue> args, CVarsContext context, bool asValue)
	{
		MethodInfo methodInfo = null;
		foreach (MethodInfo methodInfo2 in methods)
		{
			if (!(methodInfo2.Name != name))
			{
				TValue result = TryCallMethod(methodInfo2, obj, args, context, asValue);
				if (!result.isUnknown)
				{
					return result;
				}
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				if (parameters.Length >= 1 && parameters.Length <= 3 && parameters[0].ParameterType == typeof(List<TValue>) && (parameters.Length < 2 || parameters[1].ParameterType == typeof(CVarsContext)) && (parameters.Length < 3 || (parameters[2].ParameterType == typeof(bool) && parameters[2].Name == "asValue")) && (methodInfo == null || parameters.Length > methodInfo.GetParameters().Length))
				{
					methodInfo = methodInfo2;
				}
			}
		}
		if (methodInfo != null)
		{
			object[] array = new object[methodInfo.GetParameters().Length];
			array[0] = args;
			if (array.Length > 1)
			{
				array[1] = context;
			}
			if (array.Length > 2)
			{
				array[2] = asValue;
			}
			object val = methodInfo.Invoke(obj, array);
			return new TValue(val);
		}
		return TValue.Unknown;
	}

	private static TValue TryCallMethod(MethodInfo method, object obj, List<TValue> args, CVarsContext context, bool asValue)
	{
		ParameterInfo[] parameters = method.GetParameters();
		if (args.Count > parameters.Length)
		{
			return TValue.Unknown;
		}
		object[] array = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			if (i >= args.Count)
			{
				if (!parameterInfo.HasDefaultValue)
				{
					return TValue.Unknown;
				}
				if (parameterInfo.ParameterType == typeof(CVarsContext))
				{
					array[i] = context;
				}
				else if (parameterInfo.ParameterType == typeof(bool) && parameterInfo.Name == "asValue")
				{
					array[i] = asValue;
				}
				else
				{
					array[i] = parameterInfo.DefaultValue;
				}
			}
			else
			{
				if (!ResolveArg(args[i], parameterInfo, out var val))
				{
					return TValue.Unknown;
				}
				array[i] = val;
			}
		}
		object val2 = method.Invoke(obj, array);
		return new TValue(val2);
	}

	private static bool ResolveArg(TValue arg, ParameterInfo param, out object val)
	{
		val = null;
		if (param.ParameterType == typeof(bool))
		{
			if (!arg.isConvertibleToInt)
			{
				return false;
			}
			val = arg.GetBool();
			return true;
		}
		if (param.ParameterType == typeof(int))
		{
			if (!arg.isConvertibleToInt)
			{
				return false;
			}
			val = arg.GetInt();
			return true;
		}
		if (param.ParameterType == typeof(string))
		{
			if (!arg.isString)
			{
				return false;
			}
			val = arg.GetString();
			return true;
		}
		if (param.ParameterType == typeof(TValue))
		{
			val = arg;
			return true;
		}
		if (param.ParameterType.IsEnum)
		{
			if (arg.type == TValue.EType.Enum && arg.objVal as Type != param.ParameterType)
			{
				return false;
			}
			if (arg.isConvertibleToInt)
			{
				val = Enum.ToObject(param.ParameterType, arg.GetInt());
				if (!param.ParameterType.IsDefined(typeof(FlagsAttribute), inherit: false) && !Enum.IsDefined(param.ParameterType, val))
				{
					return false;
				}
				return true;
			}
			if (arg.isString)
			{
				if (!Enum.TryParse(param.ParameterType, arg.GetString(), out val))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		if (!arg.GetObject(out val))
		{
			return false;
		}
		if (val == null)
		{
			if (param.ParameterType.IsClass)
			{
				return true;
			}
			return false;
		}
		Type type = val.GetType();
		if (!param.ParameterType.IsAssignableFrom(type))
		{
			if (val is List<object> list && typeof(IList).IsAssignableFrom(param.ParameterType) && param.ParameterType.GenericTypeArguments.Length == 1 && list.All((object o) => o == null || param.ParameterType.GenericTypeArguments[0].IsInstanceOfType(o)))
			{
				IList list2 = (IList)Activator.CreateInstance(param.ParameterType);
				foreach (object item in list)
				{
					list2.Add(item);
				}
				val = list2;
				return true;
			}
			return false;
		}
		return true;
	}

	public TValue GetItemByIndex(TValue key, CVarsContext context = null, bool asValue = true)
	{
		return GetObjectItemByIndex(obj, key, context ?? this, asValue);
	}

	public static TValue GetObjectItemByIndex(object obj, TValue key, CVarsContext context = null, bool asValue = true)
	{
		if (obj == null)
		{
			return TValue.Unknown;
		}
		if (obj is IGetItemByIndex getItemByIndex)
		{
			TValue itemByIndex = getItemByIndex.GetItemByIndex(key, context, asValue);
			if (!itemByIndex.isUnknown)
			{
				return itemByIndex;
			}
		}
		if (obj is IList list)
		{
			if (!key.GetInt(out var val, context))
			{
				return TValue.Error($"Attempting to index list with non-integer index: {key}");
			}
			if (val < 0 || val >= list.Count)
			{
				return TValue.Error($"Index out of range: {val} / {list.Count}");
			}
			return new TValue(list[val]);
		}
		if (obj is IDictionary dictionary)
		{
			key.GetObject(out var val2, context);
			if (val2 == null)
			{
				return TValue.Error($"Attempting to index dictionary with invalid key: {key}");
			}
			if (!dictionary.Contains(val2))
			{
				return TValue.Unknown;
			}
			return new TValue(dictionary[val2]);
		}
		if (obj is string text)
		{
			if (!key.GetInt(out var val3, context))
			{
				return TValue.Error($"Attempting to index string with non-integer index: {key}");
			}
			if (val3 < 0 || val3 >= text.Length)
			{
				return TValue.Error($"Index out of range: {val3} / {text.Length}");
			}
			return new TValue(text[val3].ToString());
		}
		MethodInfo method = obj.GetType().GetMethod("get_Item");
		if (method != null)
		{
			TValue result = TryCallMethod(method, obj, new List<TValue> { key }, context, asValue);
			if (!result.isUnknown)
			{
				return result;
			}
		}
		return TValue.Unknown;
	}
}
