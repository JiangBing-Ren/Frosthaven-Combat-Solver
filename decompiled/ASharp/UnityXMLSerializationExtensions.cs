using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public static class UnityXMLSerializationExtensions
{
	public static byte[] XMLSerialize_ToArray<T>(this T objToSerialize) where T : class
	{
		if (objToSerialize.IsTNull())
		{
			return null;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(objToSerialize.GetType());
		using MemoryStream w = new MemoryStream();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.Unicode);
		xmlSerializer.Serialize(xmlTextWriter, objToSerialize);
		return ((MemoryStream)xmlTextWriter.BaseStream).ToArray();
	}

	public static string XMLSerialize_ToString<T>(this T objToSerialize) where T : class
	{
		if (objToSerialize.IsTNull())
		{
			return null;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(objToSerialize.GetType());
		using MemoryStream w = new MemoryStream();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.Unicode);
		xmlSerializer.Serialize(xmlTextWriter, objToSerialize);
		return Encoding.Unicode.GetString(((MemoryStream)xmlTextWriter.BaseStream).ToArray());
	}

	public static T XMLDeserialize_ToObject<T>(this string strSerial) where T : class
	{
		if (string.IsNullOrEmpty(strSerial))
		{
			return null;
		}
		using MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(strSerial));
		return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
	}

	public static T XMLDeserialize_ToObject<T>(byte[] objSerial) where T : class
	{
		if (objSerial.IsNullOrEmpty())
		{
			return null;
		}
		using MemoryStream stream = new MemoryStream(objSerial);
		return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
	}
}
