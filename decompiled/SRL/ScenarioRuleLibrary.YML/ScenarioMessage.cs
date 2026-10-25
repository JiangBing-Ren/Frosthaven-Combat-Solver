using System.Collections.Generic;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ScenarioMessage
{
	public int MessageLayoutType { get; private set; }

	public List<ScenarioDialogueLine> DialogueLines { get; private set; }

	public ScenarioMessage(int layoutType, List<ScenarioDialogueLine> dialogueLines)
	{
		MessageLayoutType = layoutType;
		DialogueLines = dialogueLines;
	}

	public static bool ParseMessage(MappingEntry entry, string fileName, out ScenarioMessage message)
	{
		message = null;
		bool flag = true;
		List<ScenarioDialogueLine> list = new List<ScenarioDialogueLine>();
		int layoutType = 0;
		if (YMLShared.GetMapping(entry, fileName, out var mapping))
		{
			foreach (MappingEntry entry2 in mapping.Entries)
			{
				string text = entry2.Key.ToString();
				Sequence sequence;
				if (!(text == "DialogueLines"))
				{
					if (text == "MessageLayoutType")
					{
						if (YMLShared.ParseIntValue(entry2.Value.ToString(), "MessageLayoutType", fileName, out var value))
						{
							layoutType = value;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry2.Key?.ToString() + " in " + entry.Key?.ToString() + "/" + entry2.Key?.ToString() + ", File: " + fileName);
					flag = false;
				}
				else if (YMLShared.GetSequence(entry2, fileName, out sequence))
				{
					foreach (DataItem entry3 in sequence.Entries)
					{
						if (entry3 is Mapping)
						{
							if (ScenarioDialogueLine.ParseDialogueLine((entry3 as Mapping).Entries, fileName, out var dialogLine))
							{
								list.Add(dialogLine);
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected DialogueLine entry, Not mappable: " + fileName);
							flag = false;
						}
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected DialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
					flag = false;
				}
			}
			if (flag && list != null && list.Count > 0)
			{
				message = new ScenarioMessage(layoutType, list);
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
