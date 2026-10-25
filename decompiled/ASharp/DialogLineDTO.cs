using GLOOM;
using MapRuleLibrary.YML.Message;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class DialogLineDTO
{
	public string text;

	public string character;

	public EExpression expression;

	public Vector3? cameraLocation;

	public string narrativeImageID;

	public string title;

	public string narrativeAudioId;

	public DialogLineDTO(CLevelMessagePage messagePage)
	{
		text = (string.IsNullOrEmpty(messagePage.PageTextKey) ? "" : messagePage.PageTextKey);
		character = (string.IsNullOrEmpty(messagePage.SpeakingCharacterGUID) ? "" : messagePage.SpeakingCharacterGUID);
		expression = (EExpression)messagePage.SpeakingCharacterExpression;
		if (messagePage.SpeakingCharacterBackgroundId.IsNOTNullOrEmpty())
		{
			narrativeImageID = messagePage.SpeakingCharacterBackgroundId;
		}
		if (messagePage.SpeakingCharacterAudioId.IsNOTNullOrEmpty())
		{
			narrativeAudioId = messagePage.SpeakingCharacterAudioId;
		}
		else
		{
			narrativeAudioId = messagePage.PageTextKey;
		}
		title = LocalizationManager.GetTranslation(UIInfoTools.Instance.GetStoryCharacterDisplayNameKey(character));
	}

	public DialogLineDTO(ScenarioDialogueLine line)
	{
		text = line.Text;
		character = line.Character;
		expression = (EExpression)line.Expression;
		if (line.NarrativeImageId.IsNOTNullOrEmpty())
		{
			narrativeImageID = line.NarrativeImageId;
		}
		title = LocalizationManager.GetTranslation(UIInfoTools.Instance.GetStoryCharacterDisplayNameKey(character));
		if (line.NarrativeAudioId.IsNOTNullOrEmpty())
		{
			narrativeAudioId = line.NarrativeAudioId;
		}
	}

	public DialogLineDTO(MapDialogueLine line)
	{
		text = line.Text;
		character = line.Character;
		expression = (EExpression)line.Expression;
		narrativeImageID = line.NarrativeImageID;
		if (line.CameraLocation != null)
		{
			cameraLocation = new Vector3(line.CameraLocation.X, line.CameraLocation.Y, line.CameraLocation.Z);
		}
		title = LocalizationManager.GetTranslation(UIInfoTools.Instance.GetStoryCharacterDisplayNameKey(character));
		narrativeAudioId = line.NarrativeAudioID;
	}

	public DialogLineDTO(string text, string character, EExpression expression = EExpression.Default, string narrativeImageId = null, string title = null, string narrativeAudioId = null)
	{
		this.text = text;
		this.character = character;
		this.expression = expression;
		narrativeImageID = narrativeImageId;
		this.title = title ?? LocalizationManager.GetTranslation(UIInfoTools.Instance.GetStoryCharacterDisplayNameKey(character));
		this.narrativeAudioId = narrativeAudioId;
	}
}
