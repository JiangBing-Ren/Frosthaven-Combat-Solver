using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public abstract class BinarySerializableYMLData : ISerializable
{
	private static ConcurrentDictionary<Type, List<PropertyInfo>> _serializablePropertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

	[BinarySerialize]
	public List<string> RawYML { get; set; } = new List<string>();

	[BinarySerialize]
	public string FileName { get; set; }

	public void AddRawYML(MappingEntry entry)
	{
		if (DataLoadingFlags.IsCompilingRulebase)
		{
			RawYML.Add(entry.GetYML());
		}
	}

	public BinarySerializableYMLData()
	{
	}

	public BinarySerializableYMLData(string fileName)
	{
	}

	protected BinarySerializableYMLData(SerializationInfo info, StreamingContext context)
	{
		List<PropertyInfo> serializableProperties = GetSerializableProperties();
		foreach (SerializationEntry entry in info)
		{
			serializableProperties.First((PropertyInfo p) => p.Name == entry.Name).SetValue(this, entry.Value);
		}
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		foreach (PropertyInfo serializableProperty in GetSerializableProperties())
		{
			if (serializableProperty.GetCustomAttribute<BinarySerializeAttribute>() != null)
			{
				object value = serializableProperty.GetValue(this);
				info.AddValue(serializableProperty.Name, value);
			}
		}
	}

	private List<PropertyInfo> GetSerializableProperties()
	{
		Type type = GetType();
		if (!_serializablePropertiesCache.TryGetValue(type, out var value))
		{
			value = (from property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where property.GetCustomAttribute<BinarySerializeAttribute>() != null
				select property).ToList();
			if (!_serializablePropertiesCache.TryAdd(type, value))
			{
				value = _serializablePropertiesCache[type];
			}
		}
		return value;
	}
}
