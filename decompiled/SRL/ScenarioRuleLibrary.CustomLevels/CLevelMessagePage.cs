using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelMessagePage : ISerializable
{
	public string SpeakingCharacterGUID { get; set; }

	public int SpeakingCharacterExpression { get; set; }

	public string SpeakingCharacterBackgroundId { get; set; }

	public string PageTextKey { get; set; }

	public string ImagePath { get; set; }

	public string SpeakingCharacterAudioId { get; set; }

	public string PageTextKeyController { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpeakingCharacterGUID", SpeakingCharacterGUID);
		info.AddValue("SpeakingCharacterExpression", SpeakingCharacterExpression);
		info.AddValue("PageTextKey", PageTextKey);
		info.AddValue("ImagePath", ImagePath);
		info.AddValue("SpeakingCharacterBackgroundId", SpeakingCharacterBackgroundId);
		info.AddValue("SpeakingCharacterAudioId", SpeakingCharacterAudioId);
		info.AddValue("PageTextKeyController", PageTextKeyController);
	}

	public CLevelMessagePage(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SpeakingCharacterGUID":
					SpeakingCharacterGUID = info.GetString("SpeakingCharacterGUID");
					break;
				case "SpeakingCharacterExpression":
					SpeakingCharacterExpression = info.GetInt32("SpeakingCharacterExpression");
					break;
				case "PageTextKey":
					PageTextKey = info.GetString("PageTextKey");
					break;
				case "PageTextKeyController":
					PageTextKeyController = info.GetString("PageTextKeyController");
					break;
				case "ImagePath":
					ImagePath = info.GetString("ImagePath");
					break;
				case "SpeakingCharacterBackgroundId":
					SpeakingCharacterBackgroundId = info.GetString("SpeakingCharacterBackgroundId");
					break;
				case "SpeakingCharacterAudioId":
					SpeakingCharacterAudioId = info.GetString("SpeakingCharacterAudioId");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CLevelMessagePage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelMessagePage()
	{
		SpeakingCharacterGUID = string.Empty;
		SpeakingCharacterExpression = 0;
		PageTextKey = string.Empty;
		ImagePath = string.Empty;
		SpeakingCharacterBackgroundId = null;
		SpeakingCharacterAudioId = null;
		PageTextKeyController = null;
	}

	public CLevelMessagePage(ScenarioDialogueLine line)
	{
		SpeakingCharacterGUID = line.Character;
		SpeakingCharacterExpression = (int)line.Expression;
		PageTextKey = line.Text;
		ImagePath = string.Empty;
		SpeakingCharacterBackgroundId = line.NarrativeImageId;
		SpeakingCharacterAudioId = line.NarrativeAudioId;
		PageTextKeyController = null;
	}

	public CLevelMessagePage(string text, string textController = null)
	{
		SpeakingCharacterGUID = "Narrator";
		SpeakingCharacterExpression = 0;
		PageTextKey = text;
		ImagePath = string.Empty;
		SpeakingCharacterBackgroundId = null;
		SpeakingCharacterAudioId = null;
		PageTextKeyController = textController;
	}
}
