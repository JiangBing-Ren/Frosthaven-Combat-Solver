using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelMessage : ISerializable, IEquatable<CLevelMessage>
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum ELevelMessageLayoutType
	{
		MiddleOfScreenBox,
		HelpText,
		OffCenterBoxLeft,
		OffCenterBoxRight,
		StoryDialog,
		FixedLowerRight,
		EmptyMessageFlag
	}

	public static readonly ELevelMessageLayoutType[] LevelMessageLayouts = (ELevelMessageLayoutType[])Enum.GetValues(typeof(ELevelMessageLayoutType));

	public string MessageName { get; set; }

	public ELevelMessageLayoutType LayoutType { get; set; }

	public string TitleKey { get; set; }

	public string TitleKeyController { get; set; }

	public float DisplayDelay { get; set; }

	public CLevelTrigger DisplayTrigger { get; set; }

	public CLevelTrigger DismissTrigger { get; set; }

	public CLevelUIInteractionProfile InteractabilityProfileForMessage { get; set; }

	public List<CLevelMessagePage> Pages { get; set; }

	public bool ShouldPauseGame { get; set; }

	public bool CanRepeat { get; set; }

	public bool ShowScreenBG { get; set; }

	public CLevelCameraProfile CameraProfile { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("MessageName", MessageName);
		info.AddValue("LayoutType", LayoutType);
		info.AddValue("TitleKey", TitleKey);
		info.AddValue("DisplayDelay", DisplayDelay);
		info.AddValue("DisplayTrigger", DisplayTrigger);
		info.AddValue("DismissTrigger", DismissTrigger);
		info.AddValue("InteractabilityProfileForMessage", InteractabilityProfileForMessage);
		info.AddValue("Pages", Pages);
		info.AddValue("ShouldPauseGame", ShouldPauseGame);
		info.AddValue("ShowScreenBG", ShowScreenBG);
		info.AddValue("CameraProfile", CameraProfile);
		info.AddValue("TitleKeyController", TitleKeyController);
		info.AddValue("CanRepeat", CanRepeat);
	}

	public CLevelMessage(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CanRepeat":
					CanRepeat = info.GetBoolean("CanRepeat");
					break;
				case "MessageName":
					MessageName = info.GetString("MessageName");
					break;
				case "LayoutType":
					LayoutType = (ELevelMessageLayoutType)info.GetValue("LayoutType", typeof(ELevelMessageLayoutType));
					break;
				case "TitleKey":
					TitleKey = info.GetString("TitleKey");
					break;
				case "TitleKeyController":
					TitleKeyController = info.GetString("TitleKeyController");
					break;
				case "DisplayDelay":
					DisplayDelay = info.GetSingle("DisplayDelay");
					break;
				case "DisplayTrigger":
					DisplayTrigger = (CLevelTrigger)info.GetValue("DisplayTrigger", typeof(CLevelTrigger));
					break;
				case "DismissTrigger":
					DismissTrigger = (CLevelTrigger)info.GetValue("DismissTrigger", typeof(CLevelTrigger));
					break;
				case "InteractabilityProfileForMessage":
					InteractabilityProfileForMessage = (CLevelUIInteractionProfile)info.GetValue("InteractabilityProfileForMessage", typeof(CLevelUIInteractionProfile));
					break;
				case "Pages":
					Pages = (List<CLevelMessagePage>)info.GetValue("Pages", typeof(List<CLevelMessagePage>));
					break;
				case "ShouldPauseGame":
					ShouldPauseGame = info.GetBoolean("ShouldPauseGame");
					break;
				case "ShowScreenBG":
					ShowScreenBG = info.GetBoolean("ShowScreenBG");
					break;
				case "CameraProfile":
					CameraProfile = (CLevelCameraProfile)info.GetValue("CameraProfile", typeof(CLevelCameraProfile));
					if (CameraProfile == null)
					{
						CameraProfile = new CLevelCameraProfile();
					}
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CLevelMessage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelMessage(string messageName, ELevelMessageLayoutType messageLayoutType, string title, float displayDelay, CLevelTrigger displayTrig, CLevelTrigger dismissTrig, CLevelUIInteractionProfile interactabilityProfile, List<CLevelMessagePage> pages, bool shouldPause, bool showScreenBG, CLevelCameraProfile camProfile, string titleController = null, bool canRepeat = false)
	{
		CanRepeat = canRepeat;
		MessageName = messageName;
		LayoutType = messageLayoutType;
		TitleKey = title;
		DisplayDelay = displayDelay;
		DisplayTrigger = displayTrig;
		DismissTrigger = dismissTrig;
		InteractabilityProfileForMessage = interactabilityProfile;
		Pages = pages;
		ShouldPauseGame = shouldPause;
		ShowScreenBG = showScreenBG;
		CameraProfile = camProfile;
		TitleKeyController = titleController;
	}

	public CLevelMessage()
	{
		CanRepeat = false;
		MessageName = string.Empty;
		LayoutType = ELevelMessageLayoutType.MiddleOfScreenBox;
		TitleKey = string.Empty;
		DisplayDelay = 0f;
		DisplayTrigger = new CLevelTrigger();
		DismissTrigger = new CLevelTrigger();
		InteractabilityProfileForMessage = new CLevelUIInteractionProfile();
		Pages = new List<CLevelMessagePage>();
		ShouldPauseGame = false;
		ShowScreenBG = false;
		CameraProfile = new CLevelCameraProfile();
		TitleKeyController = null;
	}

	public static CLevelMessage CreateLevelMessageFromYML(ScenarioMessage scenarioMessage, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger predefinedTriggerType, string mapGUID = null)
	{
		if (scenarioMessage == null)
		{
			return null;
		}
		string messageName = "YMLDefined_" + predefinedTriggerType.ToString() + (string.IsNullOrEmpty(mapGUID) ? "" : ("_" + mapGUID));
		CLevelTrigger displayTrigger = new CLevelTrigger(predefinedTriggerType, mapGUID);
		CLevelTrigger dismissTrigger = new CLevelTrigger
		{
			IsTriggeredByDismiss = true
		};
		CLevelMessage cLevelMessage = new CLevelMessage();
		cLevelMessage.MessageName = messageName;
		cLevelMessage.LayoutType = (ELevelMessageLayoutType)scenarioMessage.MessageLayoutType;
		cLevelMessage.DisplayTrigger = displayTrigger;
		cLevelMessage.DismissTrigger = dismissTrigger;
		if (scenarioMessage.DialogueLines != null)
		{
			cLevelMessage.Pages = scenarioMessage.DialogueLines.Select((ScenarioDialogueLine k) => new CLevelMessagePage(k)).ToList();
		}
		else
		{
			cLevelMessage.Pages = new List<CLevelMessagePage>();
		}
		return cLevelMessage;
	}

	public static CLevelMessage CreateLevelMessageForCampaign(List<string> localisedKeys, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger predefinedTriggerType, string mapGUID = null)
	{
		if (localisedKeys == null)
		{
			return null;
		}
		string messageName = "CampaignDefined_" + predefinedTriggerType.ToString() + (string.IsNullOrEmpty(mapGUID) ? "" : ("_" + mapGUID));
		CLevelTrigger displayTrigger = new CLevelTrigger(predefinedTriggerType, mapGUID);
		CLevelTrigger dismissTrigger = new CLevelTrigger
		{
			IsTriggeredByDismiss = true
		};
		CLevelMessage cLevelMessage = new CLevelMessage();
		cLevelMessage.MessageName = messageName;
		cLevelMessage.LayoutType = ELevelMessageLayoutType.StoryDialog;
		cLevelMessage.DisplayTrigger = displayTrigger;
		cLevelMessage.DismissTrigger = dismissTrigger;
		cLevelMessage.Pages = new List<CLevelMessagePage>();
		foreach (string localisedKey in localisedKeys)
		{
			cLevelMessage.Pages.Add(new CLevelMessagePage(localisedKey));
		}
		return cLevelMessage;
	}

	public static CLevelMessage CreateLevelMessageForCampaign(List<string> localisedKeys, string name, string mapGUID = null, bool shouldPause = true)
	{
		if (localisedKeys == null)
		{
			return null;
		}
		CLevelTrigger displayTrigger = new CLevelTrigger(CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioStart, mapGUID);
		CLevelTrigger dismissTrigger = new CLevelTrigger
		{
			IsTriggeredByDismiss = true
		};
		CLevelMessage cLevelMessage = new CLevelMessage();
		cLevelMessage.MessageName = name;
		cLevelMessage.LayoutType = ELevelMessageLayoutType.StoryDialog;
		cLevelMessage.DisplayTrigger = displayTrigger;
		cLevelMessage.DismissTrigger = dismissTrigger;
		cLevelMessage.ShouldPauseGame = shouldPause;
		cLevelMessage.Pages = new List<CLevelMessagePage>();
		foreach (string localisedKey in localisedKeys)
		{
			cLevelMessage.Pages.Add(new CLevelMessagePage(localisedKey));
		}
		return cLevelMessage;
	}

	public bool Equals(CLevelMessage other)
	{
		return MessageName.Equals(other.MessageName);
	}

	public override int GetHashCode()
	{
		return MessageName.GetHashCode();
	}
}
