using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorForm : SEventActor
{
	public CPlayerActor.EForm Form;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Form", Form);
	}

	public SEventActorForm(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "Form")
				{
					Form = (CPlayerActor.EForm)info.GetValue("Form", typeof(CPlayerActor.EForm));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorUsedItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorForm(CPlayerActor.EForm form, SEventActorBaseData baseData)
		: base(baseData)
	{
		Form = form;
	}
}
