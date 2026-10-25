using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectIceTerrain : CObjectProp
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectIceTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectIceTerrain(string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(prefabName, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		SetIceTerrain(isIceTerrain: true);
	}

	public CObjectIceTerrain(SharedLibrary.Random scenarioGenerationRNG, string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(scenarioGenerationRNG, prefabName, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		SetIceTerrain(isIceTerrain: true);
	}

	public void SetIceTerrain(bool isIceTerrain)
	{
		ScenarioManager.PathFinder.Nodes[ArrayIndex.X, ArrayIndex.Y].IsIceTerrain = isIceTerrain;
		if (isIceTerrain)
		{
			ScenarioManager.PathFinder.HasIceTerrain = true;
		}
	}

	public override void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true, bool forceDestroy = false, CAbility destroyedByAbility = null, bool checkLinkedProp = true)
	{
		SetIceTerrain(isIceTerrain: false);
		base.DestroyProp(spawnDelay, sendMessageToClient, forceDestroy, destroyedByAbility, checkLinkedProp);
		ReleasePFAfterDestroy = true;
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		return false;
	}
}
