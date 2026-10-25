using System;
using System.IO;

namespace ScenarioRuleLibrary.YML;

public abstract class BinarySerializableYMLProcessor<TYMLData> : IBinarySerializableYMLProcessor where TYMLData : BinarySerializableYMLData, new()
{
	public Type YMLDataType => typeof(TYMLData);

	bool IBinarySerializableYMLProcessor.ProcessRawData(string fileName, object data, string rawData)
	{
		return ProcessRawData((TYMLData)data, rawData);
	}

	bool IBinarySerializableYMLProcessor.AddToDataSource(object data)
	{
		return AddToDataSource((TYMLData)data);
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		TYMLData data = new TYMLData
		{
			FileName = fileName
		};
		bool flag = ProcessRawData(data, fileStream.ReadToEnd());
		if (flag)
		{
			flag = AddToDataSource(data);
		}
		return flag;
	}

	public abstract bool AddToDataSource(TYMLData data);

	public abstract bool ProcessRawData(TYMLData data, string rawData);
}
