using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectQuestItem : CObjectProp, ISerializable
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectQuestItem(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectQuestItem(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
	}

	public override bool AutomaticActivate(CActor actor, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		return false;
	}

	public override bool Deactivate()
	{
		if (Activated)
		{
			Activated = false;
			ScenarioManager.CurrentScenarioState.ActivatedProps.Remove(this);
			return true;
		}
		return false;
	}

	public static List<Tuple<int, string>> Compare(CObjectQuestItem questItem1, CObjectQuestItem questItem2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(questItem1, questItem2, isMPCompare));
		return list;
	}
}
