using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ScenarioRuleLibrary;

public class TextFormatter : IFormatter
{
	private enum ETypeKind
	{
		Primitive,
		Enum,
		Tuple,
		Array,
		List,
		Dictionary,
		ISerializable,
		ReflectionSerializable,
		UNKNOWN
	}

	private class TypeInfo
	{
		public string TypeName;

		public Type CSType;

		public ETypeKind TypeKind = ETypeKind.UNKNOWN;

		public static Dictionary<string, TypeInfo> Registry = new Dictionary<string, TypeInfo>();

		private static Type[] s_PrimitiveTypes = new Type[15]
		{
			typeof(bool),
			typeof(byte),
			typeof(char),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(short),
			typeof(ushort),
			typeof(float),
			typeof(double),
			typeof(string),
			typeof(Guid),
			typeof(TileIndex),
			typeof(byte[])
		};

		public override string ToString()
		{
			return $"[{TypeKind}] {TypeName}";
		}

		public static TypeInfo Get(string name)
		{
			if (Registry.TryGetValue(name, out var value))
			{
				return value;
			}
			return null;
		}

		public static TypeInfo Get(Type type, bool createIfNotFound = true)
		{
			if (type == null)
			{
				return null;
			}
			string assemblyQualifiedName = type.AssemblyQualifiedName;
			TypeInfo typeInfo = Get(assemblyQualifiedName);
			if (typeInfo != null)
			{
				return typeInfo;
			}
			if (!createIfNotFound)
			{
				return null;
			}
			typeInfo = new TypeInfo(assemblyQualifiedName, type);
			Registry.Add(typeInfo.TypeName, typeInfo);
			typeInfo.Analyze();
			return typeInfo;
		}

		private TypeInfo(string name, Type type)
		{
			TypeName = name;
			CSType = type;
		}

		private void Analyze()
		{
			AnalyzeKind();
			AnalyzeElementTypes();
			Type type = Type.GetType(TypeName);
			if (type != CSType)
			{
				Log.Error($"Unresolvable type: {TypeName} -> {type}");
			}
		}

		private void AnalyzeKind()
		{
			if (IsPrimitiveType(CSType))
			{
				TypeKind = ETypeKind.Primitive;
			}
			else if (CSType.IsEnum)
			{
				TypeKind = ETypeKind.Enum;
			}
			else if (CSType.IsArray)
			{
				TypeKind = ETypeKind.Array;
				if (CSType.GetArrayRank() != 1)
				{
					Error("Multi-dimentional array: " + TypeName);
				}
			}
			else if (IsSimpleDictionary())
			{
				TypeKind = ETypeKind.Dictionary;
			}
			else if (typeof(IList).IsAssignableFrom(CSType))
			{
				TypeKind = ETypeKind.List;
			}
			else if (typeof(ITuple).IsAssignableFrom(CSType))
			{
				TypeKind = ETypeKind.Tuple;
			}
			else if (typeof(ISerializable).IsAssignableFrom(CSType))
			{
				TypeKind = ETypeKind.ISerializable;
			}
			else
			{
				TypeKind = ETypeKind.ReflectionSerializable;
				Error("Reflection-serializable type: " + TypeName);
			}
		}

		private static bool IsPrimitiveType(Type type)
		{
			return Array.IndexOf(s_PrimitiveTypes, type) >= 0;
		}

		private bool IsSimpleDictionary()
		{
			if (!TypeName.StartsWith("Dictionary<", StringComparison.Ordinal) && !TypeName.StartsWith("System.Collections.Generic.Dictionary`2[", StringComparison.Ordinal))
			{
				return false;
			}
			if (!IsPrimitiveType(CSType.GenericTypeArguments[0]))
			{
				return false;
			}
			return true;
		}

		private void AnalyzeElementTypes()
		{
			if (CSType.HasElementType)
			{
				Get(CSType.GetElementType());
			}
			Type[] genericTypeArguments = CSType.GenericTypeArguments;
			for (int i = 0; i < genericTypeArguments.Length; i++)
			{
				Get(genericTypeArguments[i]);
			}
		}
	}

	private StreamingContext m_context;

	private IFormatterConverter m_converter = new FormatterConverter();

	private static MethodInfo s_UpdateValueMethod = typeof(SerializationInfo).GetMethod("UpdateValue", BindingFlags.Instance | BindingFlags.NonPublic);

	private static List<(string key, SerializationInfo info)> s_SerializationStack = new List<(string, SerializationInfo)>();

	public SerializationBinder Binder { get; set; }

	public StreamingContext Context { get; set; }

	public ISurrogateSelector SurrogateSelector { get; set; }

	public string Serialize(object graph)
	{
		if (graph == null)
		{
			return null;
		}
		TypeInfo ti = TypeInfo.Get(graph.GetType());
		SerializationInfo serializationInfo = GetSerializationInfo(graph, ti, "__root");
		return SerializeInfo(serializationInfo);
	}

	public object Deserialize(string serialized)
	{
		return null;
	}

	public static void Reset()
	{
		TypeInfo.Registry.Clear();
		s_SerializationStack.Clear();
	}

	public void Serialize(Stream serializationStream, object graph)
	{
		string s = Serialize(graph);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		serializationStream.Write(bytes);
	}

	public object Deserialize(Stream serializationStream)
	{
		int num = (int)serializationStream.Length;
		byte[] array = new byte[num];
		serializationStream.Read(array, 0, num);
		string serialized = Encoding.UTF8.GetString(array);
		return Deserialize(serialized);
	}

	private static void Error(string msg)
	{
		foreach (var (text, serializationInfo) in s_SerializationStack)
		{
			if (serializationInfo == null)
			{
				msg = msg + "\n" + text;
				continue;
			}
			TypeInfo arg = TypeInfo.Get(serializationInfo.ObjectType);
			msg += $"\n{arg} {text}";
		}
		Log.Error(msg);
	}

	private SerializationInfo GetSerializationInfo(object graph, TypeInfo ti, string key)
	{
		SerializationInfo serializationInfo = new SerializationInfo(ti.CSType, m_converter);
		s_SerializationStack.Add((key, serializationInfo));
		bool flag = true;
		switch (ti.TypeKind)
		{
		case ETypeKind.Primitive:
		case ETypeKind.Enum:
			Error($"Serializing {ti} as object!");
			serializationInfo.AddValue("__value", graph);
			flag = false;
			break;
		case ETypeKind.Tuple:
			SerializeTuple(graph, serializationInfo, ti);
			break;
		case ETypeKind.Array:
			SerializeArray(graph, serializationInfo, ti);
			break;
		case ETypeKind.List:
			SerializeList(graph, serializationInfo, ti);
			break;
		case ETypeKind.Dictionary:
			SerializeDictionary(graph, serializationInfo, ti);
			break;
		case ETypeKind.ISerializable:
			SerializeISerializable(graph, serializationInfo, ti);
			break;
		case ETypeKind.ReflectionSerializable:
			SerializeWithReflection(graph, serializationInfo, ti);
			break;
		default:
			Error($"Unknown type kind: {ti}");
			flag = false;
			break;
		}
		if (flag)
		{
			SerializeChildren(serializationInfo);
		}
		s_SerializationStack.RemoveAt(s_SerializationStack.Count - 1);
		return serializationInfo;
	}

	private void SerializeTuple(object obj, SerializationInfo info, TypeInfo ti)
	{
		ITuple tuple = (ITuple)obj;
		int length = tuple.Length;
		for (int i = 0; i < length; i++)
		{
			object value = tuple[i];
			string name = i.ToString();
			info.AddValue(name, value);
		}
	}

	private void SerializeArray(object obj, SerializationInfo info, TypeInfo ti)
	{
		if (((Array)obj).Rank == 1)
		{
			SerializeList(obj, info, ti);
		}
	}

	private void SerializeList(object obj, SerializationInfo info, TypeInfo ti)
	{
		IList list = (IList)obj;
		int count = list.Count;
		info.AddValue("__length", count);
		for (int i = 0; i < count; i++)
		{
			object value = list[i];
			string name = i.ToString();
			info.AddValue(name, value);
		}
	}

	private void SerializeDictionary(object obj, SerializationInfo info, TypeInfo ti)
	{
		foreach (DictionaryEntry item in (IDictionary)obj)
		{
			info.AddValue(item.Key.ToString(), item.Value);
		}
	}

	private void SerializeISerializable(object obj, SerializationInfo info, TypeInfo ti)
	{
		((ISerializable)obj).GetObjectData(info, m_context);
	}

	private void SerializeWithReflection(object obj, SerializationInfo info, TypeInfo ti)
	{
	}

	private void SerializeChildren(SerializationInfo info, bool recursive = true)
	{
		foreach (SerializationEntry item in info)
		{
			object value = item.Value;
			if (value == null || value is SerializationInfo)
			{
				continue;
			}
			s_SerializationStack.Add((item.Name, null));
			TypeInfo typeInfo = TypeInfo.Get(item.Value.GetType());
			s_SerializationStack.RemoveAt(s_SerializationStack.Count - 1);
			if (typeInfo.TypeKind > ETypeKind.Enum)
			{
				SerializationInfo serializationInfo = GetSerializationInfo(item.Value, typeInfo, item.Name);
				s_UpdateValueMethod.Invoke(info, new object[3]
				{
					item.Name,
					serializationInfo,
					typeof(SerializationInfo)
				});
				if (recursive)
				{
					SerializeChildren(serializationInfo);
				}
			}
		}
	}

	private string SerializeInfo(SerializationInfo info)
	{
		StringBuilder stringBuilder = new StringBuilder();
		SerializeInfo(info, stringBuilder, "");
		return stringBuilder.ToString();
	}

	private void SerializeInfo(SerializationInfo info, StringBuilder sb, string ident)
	{
		TypeInfo typeInfo = TypeInfo.Get(info.ObjectType);
		sb.Append(ident);
		sb.Append("__type: ");
		sb.AppendLine(typeInfo.TypeName);
		foreach (SerializationEntry item in info)
		{
			sb.Append(ident);
			sb.Append(item.Name);
			sb.Append(": ");
			if (item.Value == null)
			{
				sb.AppendLine("null");
			}
			else if (item.Value is SerializationInfo info2)
			{
				sb.AppendLine();
				SerializeInfo(info2, sb, ident + "    ");
			}
			else
			{
				sb.AppendLine(item.Value.ToString());
			}
		}
	}

	public static void Test()
	{
		object obj = SaveData.Instance.Global.CurrentEditorLevelData;
		if (obj == null)
		{
			obj = ScenarioManager.CurrentScenarioState;
		}
		if (obj != null)
		{
			Test(obj);
		}
	}

	public static void Test(object graph)
	{
		Log.Info($"Testing TextSerializer({graph})");
		Reset();
		Stopwatch stopwatch = Stopwatch.StartNew();
		string text = new TextFormatter().Serialize(graph);
		if (text == null)
		{
			return;
		}
		long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
		Log.Info($"TextFormatter.Serialize took {elapsedMilliseconds}ms");
		stopwatch.Restart();
		object obj = new TextFormatter().Deserialize(text);
		if (obj != null)
		{
			elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			Log.Info($"TextFormatter.Deserialize took {elapsedMilliseconds}ms");
			byte[] array = BinarySerialize(graph);
			if (MemoryExtensions.SequenceEqual(other: BinarySerialize(obj).AsSpan(), span: array.AsSpan()))
			{
				Log.Info("SUCCESS");
			}
			else
			{
				Log.Error("FAIL");
			}
		}
	}

	private static byte[] BinarySerialize(object graph)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, graph);
		return memoryStream.ToArray();
	}
}
