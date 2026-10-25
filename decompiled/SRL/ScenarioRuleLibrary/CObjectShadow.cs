using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectShadow : CObjectProp, ISerializable
{
	[StateDump(Skip = true)]
	public bool WillBeRemoved;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectShadow(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectShadow(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
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

	public override void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true, bool forceDestroy = false, CAbility destroyedByAbility = null, bool checkLinkedProp = true)
	{
		base.DestroyProp(spawnDelay, sendMessageToClient, forceDestroy, destroyedByAbility, checkLinkedProp);
		WillBeRemoved = false;
		PropTile.m_Props.Find((CObjectProp p) => p.PrefabName == "ShadowPortal")?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true, destroyedByAbility);
		CNode node = ScenarioManager.PathFinder.GetNode(PropTile.m_ArrayIndex);
		List<CNavLink> list = new List<CNavLink>();
		foreach (CNavLink navLink in node.NavLinks)
		{
			if (!ScenarioManager.PathFinder.GetNode(navLink.DestinationNode.Location).HasShadow)
			{
				list.Add(navLink);
			}
		}
		node.NavLinks.Clear();
		node.NavLinks.AddRange(list);
	}

	public static List<Tuple<int, string>> Compare(CObjectShadow shadow1, CObjectShadow shadow2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(shadow1, shadow2, isMPCompare));
		return list;
	}
}
