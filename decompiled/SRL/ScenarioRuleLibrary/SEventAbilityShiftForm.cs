using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityShiftForm : SEventAbility
{
	public CAbilityShiftForm.EShiftFormState ShiftFormState { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ShiftFormState", ShiftFormState);
	}

	public SEventAbilityShiftForm(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ShiftFormState")
				{
					ShiftFormState = (CAbilityShiftForm.EShiftFormState)info.GetValue("ShiftFormState", typeof(CAbilityShiftForm.EShiftFormState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityNull entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityShiftForm(SEventAbilityBaseData baseData, CAbilityShiftForm.EShiftFormState shiftFormState)
		: base(baseData)
	{
		ShiftFormState = shiftFormState;
	}
}
