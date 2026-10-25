using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventCampaignSectionTriggered : SEvent
{
	public string SectionID { get; private set; }

	public bool ShouldPauseGame { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SectionID", SectionID);
		info.AddValue("ShouldPauseGame", ShouldPauseGame);
	}

	public SEventCampaignSectionTriggered(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ShouldPauseGame"))
				{
					if (name == "SectionID")
					{
						SectionID = info.GetString("SectionID");
					}
				}
				else
				{
					ShouldPauseGame = info.GetBoolean("ShouldPauseGame");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventCampaignSectionTriggered entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventCampaignSectionTriggered(string sectionID, bool shouldPause, string text = "")
		: base(ESEType.CampaignSectionTriggered, text)
	{
		SectionID = sectionID;
		ShouldPauseGame = shouldPause;
	}
}
