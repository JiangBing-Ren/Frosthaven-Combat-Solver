using System;
using System.IO;
using System.Text;

namespace UdpKit;

public class UdpPacket : IDisposable
{
	private readonly MemoryStream _dataStream;

	private BinaryWriter _dataWriter;

	private BinaryReader _dataReader;

	private BinaryWriter DataWriter => _dataWriter ?? (_dataWriter = new BinaryWriter(_dataStream));

	private BinaryReader DataReader => _dataReader ?? (_dataReader = new BinaryReader(_dataStream));

	public UdpPacket()
	{
		_dataStream = new MemoryStream();
	}

	public UdpPacket(byte[] data)
	{
		_dataStream = new MemoryStream(data);
	}

	public void WriteString(string data)
	{
		DataWriter.Write(data != null);
		if (data != null)
		{
			DataWriter.Write(data);
		}
	}

	public void WriteString(string data, Encoding encoding)
	{
		DataWriter.Write(data != null);
		if (data != null)
		{
			byte[] bytes = encoding.GetBytes(data);
			WriteByteArrayWithPrefix(bytes);
		}
	}

	public void WriteInt(int data)
	{
		DataWriter.Write(data);
	}

	public void WriteFloat(float data)
	{
		DataWriter.Write((double)data);
	}

	public void WriteUInt(uint data)
	{
		DataWriter.Write(data);
	}

	public void WriteBool(bool data)
	{
		DataWriter.Write(data);
	}

	public void WriteByteArrayWithPrefix(byte[] array)
	{
		if (array == null)
		{
			DataWriter.Write(-1);
			return;
		}
		DataWriter.Write(array.Length);
		if (array.Length != 0)
		{
			DataWriter.Write(array);
		}
	}

	public void WriteULong(ulong data)
	{
		DataWriter.Write(data);
	}

	public string ReadString()
	{
		if (DataReader.ReadBoolean())
		{
			return DataReader.ReadString();
		}
		return null;
	}

	public string ReadString(Encoding encoding)
	{
		if (DataReader.ReadBoolean())
		{
			byte[] bytes = ReadByteArrayWithPrefix();
			return encoding.GetString(bytes);
		}
		return null;
	}

	public int ReadInt()
	{
		return DataReader.ReadInt32();
	}

	public float ReadFloat()
	{
		return (float)DataReader.ReadDouble();
	}

	public uint ReadUInt()
	{
		return DataReader.ReadUInt32();
	}

	public bool ReadBool()
	{
		return DataReader.ReadBoolean();
	}

	public byte[] ReadByteArrayWithPrefix()
	{
		int num = DataReader.ReadInt32();
		switch (num)
		{
		case -1:
			return null;
		case 0:
			return new byte[0];
		default:
		{
			byte[] array = new byte[num];
			DataReader.Read(array, 0, num);
			return array;
		}
		}
	}

	public ulong ReadULong()
	{
		return DataReader.ReadUInt64();
	}

	public byte[] GetData()
	{
		_dataStream.Flush();
		return _dataStream.ToArray();
	}

	public void Dispose()
	{
		_dataReader?.Dispose();
		_dataWriter?.Dispose();
		_dataStream?.Dispose();
	}
}
