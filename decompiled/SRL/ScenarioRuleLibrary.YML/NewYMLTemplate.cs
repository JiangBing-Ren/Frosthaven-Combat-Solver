using System;
using System.Collections.Generic;
using System.IO;
using SharedLibrary.Client;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class NewYMLTemplate
{
	private static List<NewYMLTemplate> loadedYML;

	public string FileName { get; private set; }

	public string FileContents { get; private set; }

	public static List<NewYMLTemplate> LoadedYML
	{
		get
		{
			if (loadedYML == null)
			{
				loadedYML = LoadAll();
			}
			return loadedYML;
		}
	}

	public bool ProcessFile(string fileName, string fileContents = null)
	{
		try
		{
			FileName = fileName;
			FileContents = fileContents ?? File.ReadAllText(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(FileContents);
			bool success = false;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					entry.Key.ToString();
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + FileName);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
			return false;
		}
	}

	public static void ReloadYML()
	{
		loadedYML = LoadAll();
	}

	private static List<NewYMLTemplate> LoadAll()
	{
		List<NewYMLTemplate> list = new List<NewYMLTemplate>();
		string[] files = Directory.GetFiles(Path.Combine(SharedClient.RulebaseDataRoot, "NewYMLTemplatePath"), "*.yml", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			try
			{
				NewYMLTemplate newYMLTemplate = new NewYMLTemplate();
				if (newYMLTemplate.ProcessFile(text))
				{
					list.Add(newYMLTemplate);
				}
			}
			catch (Exception ex)
			{
				SharedClient.ValidationRecord.RecordParseFailure(text, "", ex);
			}
		}
		return list;
	}
}
