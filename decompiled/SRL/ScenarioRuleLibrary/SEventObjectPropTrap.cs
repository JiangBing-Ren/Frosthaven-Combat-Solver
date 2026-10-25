using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectPropTrap : SEventObjectProp
{
	public int Damage { get; set; }

	public string ActionActor { get; private set; }

	public List<string> EffectTextInOrderOfApplication { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Damage", Damage);
		info.AddValue("ActionActor", ActionActor);
		info.AddValue("EffectTextInOrderOfApplication", EffectTextInOrderOfApplication);
	}

	public SEventObjectPropTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Damage":
					Damage = info.GetInt32("Damage");
					break;
				case "ActionActor":
					ActionActor = info.GetString("ActionActor");
					break;
				case "EffectTextInOrderOfApplication":
					EffectTextInOrderOfApplication = (List<string>)info.GetValue("EffectTextInOrderOfApplication", typeof(List<string>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventObjectPropTrap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectPropTrap(int damage, string characterID, string action, ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectGuid, string objectName, string ownerGuid, string text, string overrideInternalActor, Point position)
		: base(objectPropSubType, objectType, objectGuid, objectName, characterID, action, ownerGuid, text, null, position)
	{
		Damage = damage;
		ActionActor = overrideInternalActor;
		if (string.IsNullOrEmpty(overrideInternalActor) && GameState.InternalCurrentActor != null)
		{
			ActionActor = GameState.InternalCurrentActor.Class.ID;
		}
		EffectTextInOrderOfApplication = new List<string>();
	}
}
