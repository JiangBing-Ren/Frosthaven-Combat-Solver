using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

public static class BinarySerializerHelper
{
	public static byte[] SerializeCustom(object obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		WriteValue(writer, obj);
		return memoryStream.ToArray();
	}

	public static byte[] Serialize(object obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static T Deserialize<T>(MemoryStream stream)
	{
		return (T)Deserialize(stream);
	}

	public static object Deserialize(MemoryStream stream)
	{
		return new BinaryFormatter().Deserialize(stream);
	}

	public static T DeserializeCustom<T>(byte[] data)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		return (T)ReadValue(reader, typeof(T));
	}

	public static T DeserializeCustom<T>(MemoryStream stream)
	{
		return (T)DeserializeCustom(typeof(T), stream);
	}

	public static object DeserializeCustom(Type type, MemoryStream stream)
	{
		using BinaryReader reader = new BinaryReader(stream);
		return ReadValue(reader, type);
	}

	private static void WriteValue(BinaryWriter writer, object value)
	{
		if (value == null)
		{
			writer.Write(value: false);
			return;
		}
		Type type = value.GetType();
		if (type.IsEnum)
		{
			writer.Write(value: true);
			WriteEnumValue(writer, value);
			return;
		}
		if (type.IsPrimitive || type == typeof(decimal) || type == typeof(string))
		{
			writer.Write(value: true);
			WritePrimitiveValue(writer, value, type);
			return;
		}
		if (type.IsArray)
		{
			writer.Write(value: true);
			WriteArrayValue(writer, value);
			return;
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			writer.Write(value: true);
			WriteListValue(writer, value);
			return;
		}
		if (type.IsClass)
		{
			writer.Write(value: true);
			WriteClassValue(writer, value);
			return;
		}
		throw new NotSupportedException("Unsupported property type: " + type.FullName);
	}

	private static void WriteEnumValue(BinaryWriter writer, object value)
	{
		Type underlyingType = Enum.GetUnderlyingType(value.GetType());
		if (underlyingType == typeof(int))
		{
			writer.Write((int)value);
			return;
		}
		if (underlyingType == typeof(byte))
		{
			writer.Write((byte)value);
			return;
		}
		throw new NotSupportedException("Unsupported enum underlying type: " + underlyingType.FullName);
	}

	private static void WriteClassValue(BinaryWriter writer, object value)
	{
		PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetCustomAttribute<BinarySerializeAttribute>() != null)
			{
				object value2 = propertyInfo.GetValue(value);
				WriteValue(writer, value2);
			}
		}
	}

	private static void WritePrimitiveValue(BinaryWriter writer, object value, Type type)
	{
		if (type == typeof(int))
		{
			writer.Write((int)value);
			return;
		}
		if (type == typeof(long))
		{
			writer.Write((long)value);
			return;
		}
		if (type == typeof(short))
		{
			writer.Write((short)value);
			return;
		}
		if (type == typeof(byte))
		{
			writer.Write((byte)value);
			return;
		}
		if (type == typeof(bool))
		{
			writer.Write((bool)value);
			return;
		}
		if (type == typeof(char))
		{
			writer.Write((char)value);
			return;
		}
		if (type == typeof(double))
		{
			writer.Write((double)value);
			return;
		}
		if (type == typeof(float))
		{
			writer.Write((float)value);
			return;
		}
		if (type == typeof(decimal))
		{
			writer.Write((decimal)value);
			return;
		}
		if (type == typeof(string))
		{
			writer.Write((string)value);
			return;
		}
		throw new NotSupportedException("Unsupported primitive type: " + type.FullName);
	}

	private static void WriteArrayValue(BinaryWriter writer, object value)
	{
		Array array = (Array)value;
		writer.Write(array.Length);
		foreach (object item in array)
		{
			WriteValue(writer, item);
		}
	}

	private static void WriteListValue(BinaryWriter writer, object value)
	{
		writer.Write(((dynamic)value).Count);
		for (int i = 0; i < ((dynamic)value).Count; i++)
		{
			BinarySerializerHelper.WriteValue(writer, ((dynamic)value)[i]);
		}
	}

	private static object ReadValue(BinaryReader reader, Type type)
	{
		if (!reader.ReadBoolean())
		{
			return null;
		}
		if (type.IsEnum)
		{
			return ReadEnumValue(reader, type);
		}
		if (type.IsPrimitive || type == typeof(decimal) || type == typeof(string))
		{
			return ReadPrimitiveValue(reader, type);
		}
		if (type.IsArray)
		{
			return ReadArrayValue(reader, type);
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			return ReadListValue(reader, type);
		}
		if (type.IsClass)
		{
			return ReadClassValue(reader, type);
		}
		throw new NotSupportedException("Unsupported property type: " + type.FullName);
	}

	private static object ReadListValue(BinaryReader reader, Type type)
	{
		Type type2 = type.GetGenericArguments()[0];
		int num = reader.ReadInt32();
		IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type2));
		for (int i = 0; i < num; i++)
		{
			object value = ReadValue(reader, type2);
			list.Add(value);
		}
		return list;
	}

	private static object ReadEnumValue(BinaryReader reader, Type enumType)
	{
		Type underlyingType = Enum.GetUnderlyingType(enumType);
		if (underlyingType == typeof(int))
		{
			return Enum.ToObject(enumType, reader.ReadInt32());
		}
		if (underlyingType == typeof(byte))
		{
			return Enum.ToObject(enumType, reader.ReadByte());
		}
		throw new NotSupportedException("Unsupported enum underlying type: " + underlyingType.FullName);
	}

	private static object ReadClassValue(BinaryReader reader, Type type)
	{
		object obj = Activator.CreateInstance(type);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetCustomAttribute<BinarySerializeAttribute>() != null)
			{
				object value = ReadValue(reader, propertyInfo.PropertyType);
				propertyInfo.SetValue(obj, value);
			}
		}
		return obj;
	}

	private static object ReadArrayValue(BinaryReader reader, Type type)
	{
		int num = reader.ReadInt32();
		Type elementType = type.GetElementType();
		Array array = Array.CreateInstance(elementType, num);
		for (int i = 0; i < num; i++)
		{
			object value = ReadValue(reader, elementType);
			array.SetValue(value, i);
		}
		return array;
	}

	private static object ReadPrimitiveValue(BinaryReader reader, Type type)
	{
		if (type == typeof(int))
		{
			return reader.ReadInt32();
		}
		if (type == typeof(long))
		{
			return reader.ReadInt64();
		}
		if (type == typeof(short))
		{
			return reader.ReadInt16();
		}
		if (type == typeof(byte))
		{
			return reader.ReadByte();
		}
		if (type == typeof(bool))
		{
			return reader.ReadBoolean();
		}
		if (type == typeof(char))
		{
			return reader.ReadChar();
		}
		if (type == typeof(double))
		{
			return reader.ReadDouble();
		}
		if (type == typeof(float))
		{
			return reader.ReadSingle();
		}
		if (type == typeof(decimal))
		{
			return reader.ReadDecimal();
		}
		if (type == typeof(string))
		{
			return reader.ReadString();
		}
		throw new NotSupportedException("Unsupported primitive type: " + type.FullName);
	}
}
