using System;

namespace ScenarioRuleLibrary.YML;

public interface IBinarySerializableYMLProcessor
{
	Type YMLDataType { get; }

	bool ProcessRawData(string fileName, object data, string rawData);

	bool AddToDataSource(object data);
}
