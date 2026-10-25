using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ScenarioDialogueLine
{
	public enum EExpression
	{
		Default,
		Angry
	}

	public static readonly EExpression[] Expressions = (EExpression[])Enum.GetValues(typeof(EExpression));

	public string Text { get; private set; }

	public string Character { get; private set; }

	public EExpression Expression { get; private set; }

	public string NarrativeImageId { get; private set; }

	public string NarrativeAudioId { get; private set; }

	public ScenarioDialogueLine(string text, string character, EExpression expression, string narrativeImageId = null, string narrativeAudioId = null)
	{
		Text = text;
		Character = character;
		Expression = expression;
		NarrativeImageId = narrativeImageId;
		NarrativeAudioId = narrativeAudioId;
	}

	public static bool ParseDialogueLine(List<MappingEntry> entries, string fileName, out ScenarioDialogueLine dialogLine)
	{
		dialogLine = null;
		bool flag = true;
		string text = string.Empty;
		string character = string.Empty;
		EExpression eExpression = EExpression.Default;
		string narrativeImageId = string.Empty;
		string narrativeAudioId = string.Empty;
		foreach (MappingEntry entry in entries)
		{
			switch (entry.Key.ToString())
			{
			case "Text":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Text", fileName, out var value4))
				{
					text = value4;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Character":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Character", fileName, out var value2))
				{
					character = value2;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Expression":
				eExpression = Expressions.SingleOrDefault((EExpression x) => x.ToString() == entry.Value.ToString());
				if (eExpression == EExpression.Default)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Expression: " + entry.Value?.ToString() + ". File:\n" + fileName);
					flag = false;
				}
				break;
			case "NarrativeImageId":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "NarrativeImageId", fileName, out var value3))
				{
					narrativeImageId = value3;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "NarrativeAudioId":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "NarrativeAudioId", fileName, out var value))
				{
					narrativeAudioId = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of DialogLine" + fileName);
				flag = false;
				break;
			}
		}
		if (flag)
		{
			dialogLine = new ScenarioDialogueLine(text, character, eExpression, narrativeImageId, narrativeAudioId);
		}
		return flag;
	}
}
