using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AbilityCardConfigYML
{
	public ConcurrentBag<AbilityCardConfigYMLData> LoadedYML { get; set; }

	public AbilityCardConfigYML()
	{
		LoadedYML = new ConcurrentBag<AbilityCardConfigYMLData>();
	}

	private bool IsCharacterOrPNG(string value)
	{
		if (CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == value) != ECharacter.None || value.EndsWith(".png"))
		{
			return true;
		}
		return false;
	}

	private bool IsDefaultOrPNG(string value)
	{
		if (value == "Default" || value.EndsWith(".png"))
		{
			return true;
		}
		return false;
	}

	private string GetHTMLColor(string value, string fileName)
	{
		if (value.Length == 3 || value.Length == 4 || value.Length == 6 || value.Length == 8)
		{
			bool flag = true;
			string text = value;
			foreach (char c in text)
			{
				if ((c < 'A' || c > 'F') && (c < '0' || c > '9'))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				value = "#" + value;
				return value;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Incorrect Colour format. Use only hexadecimal characters in file " + fileName);
			return null;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Colour property has to be exactly 3, 4, 6 or 8 characters long in file " + fileName);
		return null;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AbilityCardConfigYMLData abilityCardConfigYMLData = new AbilityCardConfigYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value37))
						{
							abilityCardConfigYMLData.ID = value37;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultCharacter":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultCharacter", fileName, out var character))
						{
							if (CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == character) != ECharacter.None)
							{
								abilityCardConfigYMLData.DefaultCharacter = character;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be a character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TitleSprite":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TitleSprite", fileName, out var value30))
						{
							if (IsCharacterOrPNG(value30))
							{
								abilityCardConfigYMLData.TitleSprite = value30;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TopActionRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TopActionRegular", fileName, out var value27))
						{
							if (IsCharacterOrPNG(value27))
							{
								abilityCardConfigYMLData.TopActionRegular = value27;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TopActionSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TopActionSelected", fileName, out var value14))
						{
							if (IsCharacterOrPNG(value14))
							{
								abilityCardConfigYMLData.TopActionSelected = value14;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TopActionHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TopActionHighlight", fileName, out var value24))
						{
							if (IsCharacterOrPNG(value24))
							{
								abilityCardConfigYMLData.TopActionHighlight = value24;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TopActionDisabled":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TopActionDisabled", fileName, out var value3))
						{
							if (IsCharacterOrPNG(value3))
							{
								abilityCardConfigYMLData.TopActionDisabled = value3;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BottomActionRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "BottomActionRegular", fileName, out var value2))
						{
							if (IsCharacterOrPNG(value2))
							{
								abilityCardConfigYMLData.BottomActionRegular = value2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BottomActionSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "BottomActionSelected", fileName, out var value22))
						{
							if (IsCharacterOrPNG(value22))
							{
								abilityCardConfigYMLData.BottomActionSelected = value22;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BottomActionHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "BottomActionHighlight", fileName, out var value5))
						{
							if (IsCharacterOrPNG(value5))
							{
								abilityCardConfigYMLData.BottomActionHighlight = value5;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BottomActionDisabled":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "BottomActionDisabled", fileName, out var value39))
						{
							if (IsCharacterOrPNG(value39))
							{
								abilityCardConfigYMLData.BottomActionDisabled = value39;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultTopActionRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultTopActionRegular", fileName, out var value8))
						{
							if (IsCharacterOrPNG(value8))
							{
								abilityCardConfigYMLData.DefaultTopActionRegular = value8;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultTopActionHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultTopActionHighlight", fileName, out var value35))
						{
							if (IsCharacterOrPNG(value35))
							{
								abilityCardConfigYMLData.DefaultTopActionHighlight = value35;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultTopActionDisabled":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultTopActionDisabled", fileName, out var value19))
						{
							if (IsCharacterOrPNG(value19))
							{
								abilityCardConfigYMLData.DefaultTopActionDisabled = value19;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultBottomActionRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultBottomActionRegular", fileName, out var value21))
						{
							if (IsCharacterOrPNG(value21))
							{
								abilityCardConfigYMLData.DefaultBottomActionRegular = value21;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultBottomActionHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultBottomActionHighlight", fileName, out var value43))
						{
							if (IsCharacterOrPNG(value43))
							{
								abilityCardConfigYMLData.DefaultBottomActionHighlight = value43;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultBottomActionDisabled":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DefaultBottomActionDisabled", fileName, out var value31))
						{
							if (IsCharacterOrPNG(value31))
							{
								abilityCardConfigYMLData.DefaultBottomActionDisabled = value31;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewRegular", fileName, out var value9))
						{
							if (IsCharacterOrPNG(value9))
							{
								abilityCardConfigYMLData.PreviewRegular = value9;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewHighlight", fileName, out var value11))
						{
							if (IsCharacterOrPNG(value11))
							{
								abilityCardConfigYMLData.PreviewHighlight = value11;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewSelected", fileName, out var value40))
						{
							if (IsCharacterOrPNG(value40))
							{
								abilityCardConfigYMLData.PreviewSelected = value40;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewSelectedHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewSelectedHighlight", fileName, out var value29))
						{
							if (IsCharacterOrPNG(value29))
							{
								abilityCardConfigYMLData.PreviewSelectedHighlight = value29;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewActive":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewActive", fileName, out var value16))
						{
							if (IsCharacterOrPNG(value16))
							{
								abilityCardConfigYMLData.PreviewActive = value16;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewActiveHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewActiveHighlight", fileName, out var value10))
						{
							if (IsCharacterOrPNG(value10))
							{
								abilityCardConfigYMLData.PreviewActiveHighlight = value10;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or character name " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LongRestRegular":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LongRestRegular", fileName, out var value45))
						{
							if (IsDefaultOrPNG(value45))
							{
								abilityCardConfigYMLData.LongRestRegular = value45;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LongRestHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LongRestHighlight", fileName, out var value36))
						{
							if (IsDefaultOrPNG(value36))
							{
								abilityCardConfigYMLData.LongRestHighlight = value36;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LongRestSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LongRestSelected", fileName, out var value32))
						{
							if (IsDefaultOrPNG(value32))
							{
								abilityCardConfigYMLData.LongRestSelected = value32;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LongRestDisabled":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LongRestDisabled", fileName, out var value25))
						{
							if (IsDefaultOrPNG(value25))
							{
								abilityCardConfigYMLData.LongRestDisabled = value25;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewDiscarded":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewDiscarded", fileName, out var value15))
						{
							if (IsDefaultOrPNG(value15))
							{
								abilityCardConfigYMLData.PreviewDiscarded = value15;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLost":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLost", fileName, out var value4))
						{
							if (IsDefaultOrPNG(value4))
							{
								abilityCardConfigYMLData.PreviewLost = value4;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewPermalost":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewPermalost", fileName, out var value44))
						{
							if (IsDefaultOrPNG(value44))
							{
								abilityCardConfigYMLData.PreviewPermalost = value44;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRest":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LongRestRegular", fileName, out var value41))
						{
							if (IsDefaultOrPNG(value41))
							{
								abilityCardConfigYMLData.PreviewLongRest = value41;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRestHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLongRestHighlight", fileName, out var value34))
						{
							if (IsDefaultOrPNG(value34))
							{
								abilityCardConfigYMLData.PreviewLongRestHighlight = value34;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRestDiscarded":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLongRestDiscarded", fileName, out var value26))
						{
							if (IsDefaultOrPNG(value26))
							{
								abilityCardConfigYMLData.PreviewLongRestDiscarded = value26;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRestSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLongRestSelected", fileName, out var value20))
						{
							if (IsDefaultOrPNG(value20))
							{
								abilityCardConfigYMLData.PreviewLongRestSelected = value20;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRestSelectedHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLongRestSelectedHighlight", fileName, out var value17))
						{
							if (IsDefaultOrPNG(value17))
							{
								abilityCardConfigYMLData.PreviewLongRestSelectedHighlight = value17;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLongRestLost":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLongRestLost", fileName, out var value12))
						{
							if (IsDefaultOrPNG(value12))
							{
								abilityCardConfigYMLData.PreviewLongRestLost = value12;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ButtonHolder":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ButtonHolder", fileName, out var value6))
						{
							if (IsDefaultOrPNG(value6))
							{
								abilityCardConfigYMLData.ButtonHolder = value6;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .png or Default " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "InitiativeColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "InitiativeColor", fileName, out var value42))
						{
							string hTMLColor7 = GetHTMLColor(value42, fileName);
							if (hTMLColor7 != null)
							{
								abilityCardConfigYMLData.InitiativeColor = hTMLColor7;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewRegularTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewRegularTextColor", fileName, out var value38))
						{
							string hTMLColor6 = GetHTMLColor(value38, fileName);
							if (hTMLColor6 != null)
							{
								abilityCardConfigYMLData.PreviewRegularTextColor = hTMLColor6;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewActiveTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewActiveTextColor", fileName, out var value33))
						{
							string hTMLColor5 = GetHTMLColor(value33, fileName);
							if (hTMLColor5 != null)
							{
								abilityCardConfigYMLData.PreviewActiveTextColor = hTMLColor5;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewSelectedTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewSelectedTextColor", fileName, out var value28))
						{
							string hTMLColor4 = GetHTMLColor(value28, fileName);
							if (hTMLColor4 != null)
							{
								abilityCardConfigYMLData.PreviewSelectedTextColor = hTMLColor4;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewDiscardedTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewDiscardedTextColor", fileName, out var value23))
						{
							string hTMLColor3 = GetHTMLColor(value23, fileName);
							if (hTMLColor3 != null)
							{
								abilityCardConfigYMLData.PreviewDiscardedTextColor = hTMLColor3;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLostTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewLostTextColor", fileName, out var value18))
						{
							string hTMLColor2 = GetHTMLColor(value18, fileName);
							if (hTMLColor2 != null)
							{
								abilityCardConfigYMLData.PreviewLostTextColor = hTMLColor2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewPermalostTextColor":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PreviewPermalostTextColor", fileName, out var value13))
						{
							string hTMLColor = GetHTMLColor(value13, fileName);
							if (hTMLColor != null)
							{
								abilityCardConfigYMLData.PreviewPermalostTextColor = hTMLColor;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be color in hexadecimal format " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewDiscardedTextOpacity":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "PreviewDiscardedTextOpacity", fileName, out var value7))
						{
							if (value7 >= 0f && value7 <= 1f)
							{
								abilityCardConfigYMLData.PreviewDiscardedTextOpacity = value7;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "PreviewDiscardedTextOpacity is out of range. It has to be a value between 0 and 1 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PreviewLostTextOpacity":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "PreviewLostTextOpacity", fileName, out var value))
						{
							if (value >= 0f && value <= 1f)
							{
								abilityCardConfigYMLData.PreviewLostTextOpacity = value;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "PreviewDiscardedTextOpacity is out of range. It has to be a value between 0 and 1 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
			}
			if (flag)
			{
				LoadedYML.Add(abilityCardConfigYMLData);
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (SystemException ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}
}
