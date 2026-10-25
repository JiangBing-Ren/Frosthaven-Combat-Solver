using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectResource : CObjectProp, ISerializable
{
	public string ResourceID;

	public int RemainingLoot = -1;

	public bool HasLimitedLoot => RemainingLoot != -1;

	public bool HasRemainingLoot
	{
		get
		{
			if (HasLimitedLoot)
			{
				return RemainingLoot > 0;
			}
			return true;
		}
	}

	public CharacterResourceData ResourceData => ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == ResourceID);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ResourceID", ResourceID);
		info.AddValue("RemainingLoot", RemainingLoot);
	}

	public CObjectResource(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ResourceID"))
				{
					if (name == "RemainingLoot")
					{
						RemainingLoot = info.GetInt32("RemainingLoot");
					}
				}
				else
				{
					ResourceID = info.GetString("ResourceID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectResource entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectResource(CharacterResourceData resourceData, string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		if (resourceData != null)
		{
			ResourceID = resourceData.ID;
			if (resourceData.CanLootFilter != null)
			{
				SetCanLootFilter(resourceData.CanLootFilter.Copy());
			}
			RemainingLoot = ResourceData.NumTimesCanBeLooted;
		}
	}

	public override bool AutomaticActivate(CActor actor, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		return false;
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (!HasRemainingLoot)
		{
			return false;
		}
		bool flag = base.Activate(actor, creditActor);
		if (flag)
		{
			if (HasLimitedLoot)
			{
				RemainingLoot--;
				return flag;
			}
			if (!ResourceData.StayActiveAfterLooted)
			{
				PropTile.DelProp(this);
			}
		}
		return flag;
	}

	public static List<Tuple<int, string>> Compare(CObjectResource resource1, CObjectResource resource2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(resource1, resource2, isMPCompare));
		return list;
	}
}
