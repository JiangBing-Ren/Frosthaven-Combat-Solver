using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectSnowRock : CObjectObstacle
{
	public CObjectSnowRock(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectSnowRock(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, List<TileIndex> pathingBlockers, CActor owner, string mapGuid, bool ignoresFlyAndJump, bool setPositionToCenterOfAllPathBlockers = false)
		: base(name, type, arrayIndex, position, rotation, pathingBlockers, owner, mapGuid, ignoresFlyAndJump, setPositionToCenterOfAllPathBlockers)
	{
	}

	public CObjectSnowRock(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, mapGuid)
	{
	}

	public CObjectSnowRock(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, mapGuid)
	{
	}
}
