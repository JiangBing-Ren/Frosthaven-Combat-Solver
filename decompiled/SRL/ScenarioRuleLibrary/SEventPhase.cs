using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventPhase : SEvent
{
	public CPhase.PhaseType SubType { get; private set; }

	public ESESubTypePhase SubTypePhase { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubType", SubType);
		info.AddValue("SubTypePhase", SubTypePhase);
	}

	public SEventPhase(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "SubType"))
				{
					if (name == "SubTypePhase")
					{
						SubTypePhase = (ESESubTypePhase)info.GetValue("SubTypePhase", typeof(ESESubTypePhase));
					}
				}
				else
				{
					SubType = (CPhase.PhaseType)info.GetValue("SubType", typeof(CPhase.PhaseType));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventPhase entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventPhase(CPhase.PhaseType subType, ESESubTypePhase subTypePhase, string text = "", bool doNotSerialize = false)
		: base(ESEType.Phase, text, doNotSerialize)
	{
		SubType = subType;
		SubTypePhase = subTypePhase;
	}
}
