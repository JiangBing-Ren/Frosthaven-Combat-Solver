using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectTerrainVisual : CObjectProp, ISerializable
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectTerrainVisual(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectTerrainVisual(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
	}

	public CObjectTerrainVisual(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectTerrainVisual(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		return false;
	}
}
