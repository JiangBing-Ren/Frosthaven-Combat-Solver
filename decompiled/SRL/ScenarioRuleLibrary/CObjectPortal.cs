using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectPortal : CObjectProp
{
	public string LinkedPortalGuid { get; set; }

	public CObjectiveFilter PortalUsageFilter { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LinkedPortalGuid", LinkedPortalGuid);
		info.AddValue("PortalUsageFilter", PortalUsageFilter);
	}

	public CObjectPortal(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "LinkedPortalGuid"))
				{
					if (name == "PortalUsageFilter")
					{
						PortalUsageFilter = (CObjectiveFilter)info.GetValue("PortalUsageFilter", typeof(CObjectiveFilter));
					}
				}
				else
				{
					LinkedPortalGuid = (string)info.GetValue("LinkedPortalGuid", typeof(string));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectPortal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectPortal(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, CObjectiveFilter portalUsageFilter = null)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		PortalUsageFilter = portalUsageFilter;
	}

	public CObjectPortal(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
		if (PortalUsageFilter.IsValidTarget(actorState))
		{
			CObjectPortal cObjectPortal = (CObjectPortal)ScenarioManager.CurrentScenarioState.PortalProps.SingleOrDefault((CObjectProp y) => y.PropGuid == LinkedPortalGuid);
			if (cObjectPortal != null)
			{
				CTile cTile = ScenarioManager.Tiles[cObjectPortal.ArrayIndex.X, cObjectPortal.ArrayIndex.Y];
				if (!ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].Walkable || ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex) != null)
				{
					for (int num = 0; num < 5; num++)
					{
						List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(cTile, num + 1);
						allUnblockedTilesFromOrigin.Remove(cTile);
						List<CTile> list = new List<CTile>();
						foreach (CTile item in allUnblockedTilesFromOrigin)
						{
							CTile propTile = null;
							if (ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) == null && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile) == null)
							{
								list.Add(item);
							}
						}
						if (list.Count > 0)
						{
							cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
							break;
						}
					}
				}
				Point point = new Point(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y);
				if (!cTile.m_HexMap.Revealed)
				{
					cTile.m_HexMap.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, fromActorOpeningDoor: true);
				}
				CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(actor)
				{
					m_EndLocation = point,
					m_StartLocation = actor.ArrayIndex,
					m_ActorTeleported = actor,
					m_TeleportAbility = null
				};
				actor.ArrayIndex = point;
				ScenarioRuleClient.MessageHandler(message);
				base.Activate(actor);
				Activated = false;
				return true;
			}
		}
		return false;
	}

	public static List<Tuple<int, string>> Compare(CObjectPortal portal1, CObjectPortal portal2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(portal1, portal2, isMPCompare));
			switch (StateShared.CheckNullsMatch(portal1.LinkedPortalGuid, portal2.LinkedPortalGuid))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 3101, "CObjectPressurePlate LinkedDoorGuids null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", portal1.PropGuid, portal2.PropGuid },
					new string[3]
					{
						"ObjectType",
						portal1.ObjectType.ToString(),
						portal2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", portal1.PrefabName, portal2.PrefabName },
					new string[3]
					{
						"LinkedPortalGuid",
						(portal1.LinkedPortalGuid == null) ? "Is Null" : "Is Not Null",
						(portal2.LinkedPortalGuid == null) ? "Is Null" : "Is Not Null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (portal1.LinkedPortalGuid != portal2.LinkedPortalGuid)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 3102, $"CObjectPressurePlate LinkedPortalGuid does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", portal1.PropGuid, portal2.PropGuid },
						new string[3]
						{
							"ObjectType",
							portal1.ObjectType.ToString(),
							portal2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", portal1.PrefabName, portal2.PrefabName },
						new string[3] { "LinkedDoorGuids Entry Value", portal1.LinkedPortalGuid, portal2.LinkedPortalGuid }
					});
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(3199, "Exception during CObjectPortal compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
