using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class CharacterConfigYML
{
	public ConcurrentBag<CharacterConfigYMLData> LoadedYML { get; set; }

	public CharacterConfigYML()
	{
		LoadedYML = new ConcurrentBag<CharacterConfigYMLData>();
	}

	private bool IsCharacterOrPNG(string value)
	{
		if (CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == value) != ECharacter.None || value.EndsWith(".png"))
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
			CharacterConfigYMLData characterConfigYMLData = new CharacterConfigYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					string value7;
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value3))
						{
							characterConfigYMLData.ID = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Model":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Model", fileName, out var model))
						{
							ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == model);
							if (eCharacter != ECharacter.None)
							{
								characterConfigYMLData.Model = eCharacter;
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
					case "Icon":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Icon", fileName, out var value17))
						{
							if (IsCharacterOrPNG(value17))
							{
								characterConfigYMLData.Icon = value17;
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
					case "IconHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "IconHighlight", fileName, out var value14))
						{
							if (IsCharacterOrPNG(value14))
							{
								characterConfigYMLData.IconHighlight = value14;
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
					case "IconGold":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "IconGold", fileName, out var value10))
						{
							if (IsCharacterOrPNG(value10))
							{
								characterConfigYMLData.IconGold = value10;
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
					case "NewAdventurePortrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NewAdventurePortrait", fileName, out var value15))
						{
							if (IsCharacterOrPNG(value15))
							{
								characterConfigYMLData.NewAdventurePortrait = value15;
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
					case "NewAdventurePortraitHighlight":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NewAdventurePortraitHighlight", fileName, out var value6))
						{
							if (IsCharacterOrPNG(value6))
							{
								characterConfigYMLData.NewAdventurePortraitHighlight = value6;
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
					case "CampaignRewardIcon":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CampaignRewardIcon", fileName, out var value5))
						{
							if (IsCharacterOrPNG(value5))
							{
								characterConfigYMLData.CampaignRewardIcon = value5;
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
					case "MapMarker":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MapMarker", fileName, out var value12))
						{
							if (IsCharacterOrPNG(value12))
							{
								characterConfigYMLData.MapMarker = value12;
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
					case "ScenarioPortrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ScenarioPortrait", fileName, out var value8))
						{
							if (IsCharacterOrPNG(value8))
							{
								characterConfigYMLData.ScenarioPortrait = value8;
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
					case "ScenarioPreviewPortrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ScenarioPreviewPortrait", fileName, out var value18))
						{
							if (IsCharacterOrPNG(value18))
							{
								characterConfigYMLData.ScenarioPreviewPortrait = value18;
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
					case "InitiativeBackground":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "InitiativeBackground", fileName, out var value9))
						{
							if (IsCharacterOrPNG(value9))
							{
								characterConfigYMLData.InitiativeBackground = value9;
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
					case "TabIcon":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TabIcon", fileName, out var value2))
						{
							if (IsCharacterOrPNG(value2))
							{
								characterConfigYMLData.TabIcon = value2;
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
					case "TabIconSelected":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "TabIconSelected", fileName, out var value13))
						{
							if (IsCharacterOrPNG(value13))
							{
								characterConfigYMLData.TabIconSelected = value13;
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
					case "ActiveAbilityIcon":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ActiveAbilityIcon", fileName, out var value11))
						{
							if (IsCharacterOrPNG(value11))
							{
								characterConfigYMLData.ActiveAbilityIcon = value11;
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
					case "AssemblyPortrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AssemblyPortrait", fileName, out var value4))
						{
							if (IsCharacterOrPNG(value4))
							{
								characterConfigYMLData.AssemblyPortrait = value4;
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
					case "DistributionPortrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DistributionPortrait", fileName, out var value16))
						{
							if (IsCharacterOrPNG(value16))
							{
								characterConfigYMLData.DistributionPortrait = value16;
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
					case "Color":
						if (YMLShared.GetStringPropertyValue(entry.Value, "Color", fileName, out value7))
						{
							string hTMLColor2 = GetHTMLColor(value7, fileName);
							if (hTMLColor2 != null)
							{
								characterConfigYMLData.Color = hTMLColor2;
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
					case "AltColor":
						if (YMLShared.GetStringPropertyValue(entry.Value, "AltColor", fileName, out value7))
						{
							string hTMLColor = GetHTMLColor(value7, fileName);
							if (hTMLColor != null)
							{
								characterConfigYMLData.AltColor = hTMLColor;
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
					case "CardConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CardConfig", fileName, out var cardConfig))
						{
							if (ScenarioRuleClient.SRLYML.Data.AbilityCardConfigs.LoadedYML.SingleOrDefault((AbilityCardConfigYMLData s) => s.ID == cardConfig) != null)
							{
								characterConfigYMLData.CardConfig = cardConfig;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {cardConfig} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Avatar":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Avatar", fileName, out var value))
						{
							characterConfigYMLData.Avatar = value;
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
				LoadedYML.Add(characterConfigYMLData);
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
