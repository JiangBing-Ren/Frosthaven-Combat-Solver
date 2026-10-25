using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace FH.YML;

public class GenericYML<TType> : IBinarySerializableYMLProcessor, IGenericYMLRepo where TType : new()
{
	public ConcurrentQueue<TType> Items { get; private set; } = new ConcurrentQueue<TType>();

	public Type YMLDataType => typeof(TType);

	IEnumerable<object> IGenericYMLRepo.GetAllItems()
	{
		return ((IEnumerable<TType>)Items).Select((Func<TType, object>)((TType s) => s));
	}

	public bool ProcessRawData(string fileName, object data, string rawData)
	{
		bool flag = true;
		try
		{
			return DeserializeObject(data, rawData, fileName);
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
			return false;
		}
	}

	public bool AddToDataSource(object data)
	{
		Items.Enqueue((TType)data);
		return true;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		TType val = new TType();
		bool flag = ProcessRawData(fileName, val, fileStream.ReadToEnd());
		if (flag)
		{
			flag = AddToDataSource(val);
		}
		return flag;
	}

	private static bool DeserializeObject<TType>(string text, string fileName, out TType item)
	{
		bool result = DeserializeObject(typeof(TType), text, fileName, out var item2);
		item = (TType)item2;
		return result;
	}

	private static bool DeserializeObject(Type type, string text, string fileName, out object item)
	{
		item = Activator.CreateInstance(type, nonPublic: true);
		return DeserializeObject(item, text, fileName);
	}

	private static bool DeserializeObject(object item, string text, string fileName)
	{
		YamlParser yamlParser = new YamlParser();
		TextInput input = new TextInput(text);
		YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var _);
		return DeserializeObject(item, (Mapping)yamlStream.Documents[0].Root, fileName);
	}

	private static bool DeserializeField(FieldInfo f, DataItem entry, string keyName, string fileName, object item)
	{
		bool flag = true;
		if (f.FieldType == typeof(string))
		{
			if (YMLShared.GetStringPropertyValue(entry, f.Name, fileName, out var value))
			{
				f.SetValue(item, value);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(List<string>))
		{
			if (YMLShared.GetStringList(entry, f.Name, fileName, out var values))
			{
				f.SetValue(item, values);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(int))
		{
			if (YMLShared.GetIntPropertyValue(entry, f.Name, fileName, out var value2))
			{
				f.SetValue(item, value2);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(uint))
		{
			if (YMLShared.GetIntPropertyValue(entry, f.Name, fileName, out var value3))
			{
				f.SetValue(item, (uint)value3);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(List<int>))
		{
			if (YMLShared.GetIntList(entry, f.Name, fileName, out var values2))
			{
				f.SetValue(item, values2);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(List<uint>))
		{
			if (YMLShared.GetIntList(entry, f.Name, fileName, out var values3))
			{
				f.SetValue(item, values3.Select((int a) => (uint)a).ToList());
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(bool))
		{
			if (YMLShared.GetBoolPropertyValue(entry, f.Name, fileName, out var value4))
			{
				f.SetValue(item, value4);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(float))
		{
			if (YMLShared.GetFloatPropertyValue(entry, f.Name, fileName, out var value5))
			{
				f.SetValue(item, value5);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType == typeof(double))
		{
			if (YMLShared.GetFloatPropertyValue(entry, f.Name, fileName, out var value6))
			{
				f.SetValue(item, (double)value6);
			}
			else
			{
				flag = false;
			}
		}
		else if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
		{
			if (!(entry is Sequence sequence))
			{
				throw new Exception("Child of '" + keyName + "' entry is not a sequecny. File " + fileName);
			}
			int num = 0;
			IList list = Activator.CreateInstance(f.FieldType) as IList;
			foreach (DataItem entry2 in sequence.Entries)
			{
				if (entry2 is Mapping mapping)
				{
					if (!DeserializeObject(f.FieldType.GenericTypeArguments[0], mapping, fileName, out var item2))
					{
						flag = false;
						break;
					}
					list.Add(item2);
				}
				else
				{
					if (f.FieldType.GenericTypeArguments[0].GetFields().Length != 1)
					{
						throw new Exception($"Element at {num} of '{keyName}' entry is not a mapping. File {fileName}");
					}
					object obj = Activator.CreateInstance(f.FieldType.GenericTypeArguments[0]);
					flag = DeserializeField(f.FieldType.GenericTypeArguments[0].GetFields()[0], entry2, "", fileName, obj);
					if (!flag)
					{
						break;
					}
					list.Add(obj);
				}
				num++;
			}
			if (flag)
			{
				f.SetValue(item, list);
			}
		}
		else if (f.FieldType.IsEnum)
		{
			if (YMLShared.GetStringPropertyValue(entry, f.FieldType.Name, fileName, out var value7))
			{
				if (!Enum.TryParse(f.FieldType, value7, out var result))
				{
					throw new Exception($"Element of '{entry} entry is not a valid value for enum '{f.FieldType}'. File {fileName}");
				}
				f.SetValue(item, result);
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			if (!f.FieldType.IsClass && !f.FieldType.IsValueType)
			{
				throw new Exception($"Unsupported field type by Generic YML parser: {f.FieldType}. File {fileName}");
			}
			if (entry is Mapping mapping2)
			{
				if (DeserializeObject(f.FieldType, mapping2, fileName, out var item3))
				{
					f.SetValue(item, item3);
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				if (f.FieldType.GetFields().Length != 1)
				{
					throw new Exception($"Element of '{entry}' entry is not a mapping. File {fileName}");
				}
				object obj2 = Activator.CreateInstance(f.FieldType);
				flag = DeserializeField(f.FieldType.GetFields()[0], entry, "", fileName, obj2);
				if (flag)
				{
					f.SetValue(item, obj2);
				}
			}
		}
		return flag;
	}

	private static bool DeserializeObject(Type type, Mapping mapping, string fileName, out object item)
	{
		item = Activator.CreateInstance(type, nonPublic: true);
		return DeserializeObject(item, mapping, fileName);
	}

	private static bool DeserializeObject(object item, Mapping mapping, string fileName)
	{
		Type type = item.GetType();
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		bool flag = true;
		foreach (MappingEntry entry in mapping.Entries)
		{
			FieldInfo fieldInfo = fields.FirstOrDefault((FieldInfo f) => f.Name == entry.Key.ToString());
			if (fieldInfo == null)
			{
				throw new Exception($"Invalid field '{entry.Key}' for type {type}!");
			}
			flag = flag && DeserializeField(fieldInfo, entry.Value, entry.Key.ToString(), fileName, item);
		}
		return flag;
	}
}
