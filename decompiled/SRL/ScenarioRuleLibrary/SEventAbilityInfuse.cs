using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityInfuse : SEventAbility
{
	public CAbilityInfuse.EInfuseState InfuseState { get; private set; }

	public List<ElementInfusionBoardManager.EElement> ElementsInfused { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("InfuseState", InfuseState);
		info.AddValue("ElementsInfused", ElementsInfused);
	}

	public SEventAbilityInfuse(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "InfuseState"))
				{
					if (name == "ElementsInfused")
					{
						ElementsInfused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsInfused", typeof(List<ElementInfusionBoardManager.EElement>));
					}
				}
				else
				{
					InfuseState = (CAbilityInfuse.EInfuseState)info.GetValue("InfuseState", typeof(CAbilityInfuse.EInfuseState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityInfuse entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityInfuse(SEventAbilityBaseData baseData, CAbilityInfuse.EInfuseState infuseState, List<ElementInfusionBoardManager.EElement> elementsInfused)
		: base(baseData)
	{
		InfuseState = infuseState;
		ElementsInfused = elementsInfused;
	}
}
