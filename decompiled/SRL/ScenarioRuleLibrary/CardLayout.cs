using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class CardLayout
{
	private readonly string FileName;

	private readonly string AbilityCardName;

	private readonly DiscardType DiscardType;

	public CardLayoutGroup ParentGroup { get; private set; }

	public List<CardLayoutGroup> PersistentAbilityLayouts { get; private set; }

	public CardLayout(string text, List<CAbility> linkedAbilities, string abilityCardName, DiscardType discard, string filename)
	{
		FileName = filename;
		AbilityCardName = abilityCardName;
		DiscardType = discard;
		PersistentAbilityLayouts = new List<CardLayoutGroup>();
		CardLayoutGroup cardLayoutGroup = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
		CardLayoutGroup cardLayoutGroup2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, cardLayoutGroup);
		cardLayoutGroup2.CreateRowData(new CardLayoutRow(text, AbilityCardName, linkedAbilities), new CardLayoutGroup.RowAttributes());
		cardLayoutGroup.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup2 });
		ParentGroup = cardLayoutGroup;
	}

	public CardLayout(Sequence data, List<CAbility> linkedAbilities, List<AbilityConsume> consumes, string abilityCardName, DiscardType discard, string filename, List<CAbilityOverride> overrides = null)
	{
		FileName = filename;
		AbilityCardName = abilityCardName;
		DiscardType = discard;
		PersistentAbilityLayouts = new List<CardLayoutGroup>();
		CardLayoutGroup parent = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
		CreateChildren(data, ref parent, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
		ParentGroup = parent;
	}

	private void CreateChildren(DataItem dataItem, ref CardLayoutGroup parent, List<CAbility> linkedAbilities, List<AbilityConsume> consumes, CardLayoutGroup.RowAttributes rowAttributes, List<CAbilityOverride> overrides = null)
	{
		if (dataItem is Sequence)
		{
			Sequence sequence = dataItem as Sequence;
			List<MappingEntry> list = new List<MappingEntry>();
			List<MappingEntry> list2 = new List<MappingEntry>();
			List<MappingEntry> list3 = new List<MappingEntry>();
			List<MappingEntry> list4 = new List<MappingEntry>();
			int value = 0;
			List<int> list5 = new List<int>();
			List<CAbility> list6 = new List<CAbility>();
			if (linkedAbilities != null)
			{
				list6.AddRange(linkedAbilities);
			}
			if (consumes != null && consumes.Count > 0)
			{
				foreach (AbilityConsume consume in consumes)
				{
					foreach (CActionAugmentationOp augmentationOp in consume.ConsumeData.AugmentationOps)
					{
						if (augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
						{
							list6.Add(augmentationOp.Ability);
						}
						else if (augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride && augmentationOp.AbilityOverride.SubAbilities != null && augmentationOp.AbilityOverride.SubAbilities.Count > 0)
						{
							list6.AddRange(augmentationOp.AbilityOverride.SubAbilities);
						}
					}
				}
			}
			int count = list6.Count;
			for (int i = 0; i < count; i++)
			{
				CAbility cAbility = list6[i];
				if (cAbility.SubAbilities != null)
				{
					list6.AddRange(cAbility.SubAbilities);
				}
			}
			count = list6.Count;
			for (int j = 0; j < count; j++)
			{
				if (list6[j] is CAbilityControlActor { ControlActorData: var controlActorData } cAbilityControlActor && controlActorData?.ControlAbilities != null)
				{
					list6.AddRange(cAbilityControlActor.ControlActorData.ControlAbilities);
				}
			}
			foreach (DataItem entry in sequence.Entries)
			{
				MappingEntry mappingEntry = (entry as Mapping).Entries[0];
				switch (mappingEntry.Key.ToString())
				{
				case "ColumnSpacing":
					YMLShared.GetIntPropertyValue(mappingEntry.Value, mappingEntry.Key.ToString(), FileName, out value);
					break;
				case "ColumnWidths":
					list3.Add(mappingEntry);
					break;
				case "RowProperties":
					list4.Add(mappingEntry);
					break;
				case "Column":
					list.Add(mappingEntry);
					break;
				case "Row":
				case "Summon":
				case "Consume":
				case "Infuse":
				case "Augment":
				case "Doom":
				case "Song":
				case "Duration":
				case "XP":
				case "XPTracker":
				case "Divider":
				case "MandatoryTrigger":
				case "Conditional":
				case "BlinkbladeState":
				case "AddedEffect":
					list2.Add(mappingEntry);
					break;
				case "Color":
					parent.SetColor(GetColor(mappingEntry));
					break;
				case "Active":
					if (list6.Count <= 0)
					{
						break;
					}
					if (mappingEntry.Value is Mapping)
					{
						List<CAbility> list7 = new List<CAbility>();
						CardLayoutGroup parent2 = null;
						CardLayoutGroup parent3 = null;
						List<string> values = new List<string>();
						List<string> values2 = new List<string>();
						List<string> values3 = new List<string>();
						bool flag = false;
						foreach (MappingEntry entry2 in (mappingEntry.Value as Mapping).Entries)
						{
							switch (entry2.Key.ToString())
							{
							case "Ability":
							{
								if (!YMLShared.GetStringList(entry2.Value, "Ability", FileName, out var values4))
								{
									break;
								}
								foreach (string item in values4)
								{
									list7.Add(GetFromLinkedAbilities(item, list6));
								}
								break;
							}
							case "Layout":
								list2.Add(entry2);
								if (entry2.Value is Sequence)
								{
									parent2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent);
									CreateChildren(entry2.Value, ref parent2, linkedAbilities, new List<AbilityConsume>(), new CardLayoutGroup.RowAttributes(), overrides);
								}
								else if (entry2.Value is Scalar)
								{
									parent2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent);
									CardLayoutGroup cardLayoutGroup2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent);
									cardLayoutGroup2.CreateRowData(new CardLayoutRow((entry2.Value as Scalar).Text, AbilityCardName, list6, overrides), new CardLayoutGroup.RowAttributes());
									parent2.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup2 });
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid layout entry. The entries under Active Bonus Layout must be a Scalar or a Sequence.\nFile: " + FileName);
								}
								break;
							case "LayoutAlt":
								if (entry2.Value is Sequence)
								{
									parent3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent);
									CreateChildren(entry2.Value, ref parent3, linkedAbilities, new List<AbilityConsume>(), new CardLayoutGroup.RowAttributes(), overrides);
								}
								else if (entry2.Value is Scalar)
								{
									parent3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent);
									CardLayoutGroup cardLayoutGroup = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent3);
									cardLayoutGroup.CreateRowData(new CardLayoutRow((entry2.Value as Scalar).Text, AbilityCardName, list6, overrides), new CardLayoutGroup.RowAttributes());
									parent3.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup });
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid layout entry. The entries under Active Bonus Layout must be a Scalar or a Sequence.\nFile: " + FileName);
								}
								break;
							case "ListLayout":
								YMLShared.GetStringList(entry2.Value, "ListLayout", FileName, out values);
								values = values.Select((string x) => new CardLayoutRow(x, AbilityCardName, linkedAbilities, overrides).PropertyLookup(x)).ToList();
								break;
							case "ListLayoutAlt":
								YMLShared.GetStringList(entry2.Value, "ListLayoutAlt", FileName, out values2);
								values2 = values2.Select((string x) => new CardLayoutRow(x, AbilityCardName, linkedAbilities, overrides).PropertyLookup(x)).ToList();
								break;
							case "Icon":
							case "Icons":
								YMLShared.GetStringList(entry2.Value, "Icon", FileName, out values3);
								break;
							case "HideTracker":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "HideTracker", FileName, out var value2))
								{
									flag = value2;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry " + entry2.Key.ToString() + ".\nFile: " + FileName);
								break;
							}
						}
						foreach (CAbility item2 in list7)
						{
							if (item2 != null && item2.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
							{
								item2.ActiveBonusYML = new ActiveBonusLayout(AbilityCardName, values3, parent2, parent3, values, values2, flag ? new List<int>() : item2.ActiveBonusData.Tracker, DiscardType);
							}
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active entries in Layout section must be Mapping type.\nFile: " + FileName);
					}
					break;
				default:
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry '" + mappingEntry.Key.ToString() + "' within layout section.  File: " + FileName);
					break;
				}
			}
			if (list2.Count > 0 && list.Count > 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Rows and Columns are not able to exist together at the same level.  File: " + FileName);
				return;
			}
			if (value < 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Column spacing cannot be negative, setting to 0.  File: " + FileName);
				value = 0;
				return;
			}
			if (list3.Count > 0 && list3.Count == 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Column Widths specified but no columns to apply them to.  File\n" + FileName);
				list3.Clear();
			}
			if (list3.Count > 0 && list3[0].Value is Scalar)
			{
				string text = (list3[0].Value as Scalar).Text;
				_ = text == "*";
				string[] array = text.Split(',');
				foreach (string text2 in array)
				{
					if (int.TryParse(text2, out var result))
					{
						list5.Add(result);
						continue;
					}
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "ColumnWidths definitions are invalid.  Could not parse value " + text2 + " to integers. File:\n" + FileName);
					list5.Clear();
				}
				if (list5.Count != list.Count)
				{
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "ColumnWidths definitions are invalid.  Number of entries does not match the number of columns.  File:\n" + FileName);
				}
			}
			if (list4.Count > 0 && list4[0].Value is Sequence)
			{
				rowAttributes = GetRowAttributes(list4[0].Value as Sequence);
			}
			if (list.Count > 0)
			{
				List<CardLayoutGroup> list8 = new List<CardLayoutGroup>();
				foreach (MappingEntry item3 in list)
				{
					CardLayoutGroup parent4 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Column, parent, GetColor(item3));
					if (item3.Value is Sequence)
					{
						CreateChildren(item3.Value, ref parent4, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
						list8.Add(parent4);
					}
					else if (item3.Value is Scalar)
					{
						List<CardLayoutGroup> list9 = new List<CardLayoutGroup>();
						CardLayoutGroup cardLayoutGroup3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent, GetColor(item3));
						cardLayoutGroup3.CreateRowData(new CardLayoutRow((item3.Value as Scalar).Text, AbilityCardName, list6, overrides), rowAttributes);
						list9.Add(cardLayoutGroup3);
						parent4.CreateRowCollection(list9);
						list8.Add(parent4);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process column value " + item3.Value.ToString() + "  File\n" + FileName);
					}
				}
				parent.CreateColumnCollection(list8, new CardLayoutGroup.ColumnAttributes(list5.ToArray(), value));
			}
			else if (list2.Count > 0)
			{
				List<CardLayoutGroup> list10 = new List<CardLayoutGroup>();
				foreach (MappingEntry item4 in list2)
				{
					string value6;
					List<int> values5;
					bool value5;
					switch (item4.Key.ToString())
					{
					case "Summon":
					{
						CAbility cAbility5 = null;
						string value21 = string.Empty;
						CardLayout specialText = null;
						bool hideSummonBoxes = false;
						foreach (MappingEntry summonEntry in (item4.Value as Mapping).Entries)
						{
							switch (summonEntry.Key.ToString())
							{
							case "Ability":
								try
								{
									cAbility5 = list6.Single((CAbility x) => x.Name == summonEntry.Value.ToString() && x.AbilityType == CAbility.EAbilityType.Summon);
								}
								catch (Exception ex)
								{
									Log.ErrorWithReport("Referenced Summon ability in Layout section '" + item4.Value?.ToString() + "' not found in Data section. File: " + FileName + ex.Message + "\n" + ex.StackTrace);
								}
								break;
							case "SummonName":
								YMLShared.GetStringPropertyValue(summonEntry.Value, "Summon/SummonName", FileName, out value21);
								break;
							case "SpecialText":
								if (summonEntry.Value is Sequence)
								{
									if (YMLShared.GetSequence(summonEntry.Value, "Summon/SpecialText", FileName, out var sequence10))
									{
										specialText = new CardLayout(sequence10, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (summonEntry.Value is Scalar)
								{
									specialText = new CardLayout((summonEntry.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "HideSummonBoxes":
							{
								if (YMLShared.GetBoolPropertyValue(summonEntry.Value, "HideSummonBoxes", FileName, out var value22))
								{
									hideSummonBoxes = value22;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Summon entry " + summonEntry.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						if (cAbility5 != null)
						{
							CardLayoutGroup cardLayoutGroup16 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Summon, parent);
							cardLayoutGroup16.CreateSummonData(new CardLayoutGroup.SummonLayout(cAbility5, value21, specialText, hideSummonBoxes));
							list10.Add(cardLayoutGroup16);
						}
						continue;
					}
					case "Augment":
					{
						string text8 = null;
						string discardText3 = null;
						CAbility cAbility4 = null;
						string text9 = null;
						CardLayout cardLayout4 = null;
						CardLayout cardLayout5 = null;
						foreach (MappingEntry entry3 in (item4.Value as Mapping).Entries)
						{
							switch (entry3.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry3.Value, "ParentAbility", FileName, out var value18))
								{
									break;
								}
								foreach (CAbility item5 in list6)
								{
									if (item5.Name == value18)
									{
										cAbility4 = item5;
										continue;
									}
									CAugment augment = item5.Augment;
									if (augment == null || augment.Abilities.Count <= 0)
									{
										continue;
									}
									foreach (CAbility ability in item5.Augment.Abilities)
									{
										if (ability.Name == value18)
										{
											cAbility4 = ability;
										}
									}
								}
								if (cAbility4 == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value18 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "ActiveBonusLayout", FileName, out var value16))
								{
									text9 = new CardLayoutRow(value16, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value16);
								}
								break;
							}
							case "AugmentAbilityLayout":
								if (entry3.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry3.Value, "Augment/AugmentAbilityLayout", FileName, out var sequence8))
									{
										cardLayout4 = new CardLayout(sequence8, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry3.Value is Scalar)
								{
									cardLayout4 = new CardLayout((entry3.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "NormalContentLayout":
								if (entry3.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry3.Value, "Augment/NormalContentLayout", FileName, out var sequence7))
									{
										cardLayout5 = new CardLayout(sequence7, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry3.Value is Scalar)
								{
									cardLayout5 = new CardLayout((entry3.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "AugmentIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "AugmentIcon", FileName, out var value19))
								{
									text8 = value19;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "DiscardText", FileName, out var value17))
								{
									discardText3 = value17;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Augment entry " + entry3.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag4 = true;
						if (cAbility4?.Augment == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name (ability with Augment within, not an augment's inner ability) required for Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (text9 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (cardLayout4 == null || cardLayout5 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Both AugmentAbilityLayout and NormalContentLayout required for Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (text8 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "AugmentIcon required for Augemnt in layout section. \nFile: " + FileName);
						}
						if (flag4)
						{
							cAbility4.Augment.ActiveBonusLayout = text9;
							CardLayoutGroup cardLayoutGroup13 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Augment, parent);
							cardLayoutGroup13.CreateAugmentData(new CardLayoutGroup.AugmentLayout(text8, discardText3, cardLayout4, cardLayout5));
							list10.Add(cardLayoutGroup13);
						}
						continue;
					}
					case "Doom":
					{
						CAbility cAbility3 = null;
						string text6 = null;
						CardLayout cardLayout3 = null;
						string reminderTextOverride = null;
						string text7 = null;
						string discardText2 = null;
						foreach (MappingEntry entry4 in (item4.Value as Mapping).Entries)
						{
							switch (entry4.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry4.Value, "ParentAbility", FileName, out var value13))
								{
									break;
								}
								foreach (CAbility item6 in list6)
								{
									if (item6.Name == value13)
									{
										cAbility3 = item6;
									}
								}
								if (cAbility3 == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value13 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "ActiveBonusLayout", FileName, out var value12))
								{
									text6 = new CardLayoutRow(value12, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value12);
								}
								break;
							}
							case "DoomAbilityLayout":
								if (entry4.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry4.Value, "Doom/DoomAbilityLayout", FileName, out var sequence6))
									{
										cardLayout3 = new CardLayout(sequence6, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry4.Value is Scalar)
								{
									cardLayout3 = new CardLayout((entry4.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "ReminderTextOverride":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "ReminderTextOverride", FileName, out var value11))
								{
									reminderTextOverride = value11;
								}
								break;
							}
							case "DoomIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "DoomIcon", FileName, out var value15))
								{
									text7 = value15;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "DoomIcon", FileName, out var value14))
								{
									discardText2 = value14;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Doom entry " + entry4.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag3 = true;
						if (!(cAbility3 is CAbilityAddDoom))
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility not AddDoom type as required for Doom in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if ((cAbility3 as CAbilityAddDoom)?.Doom == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name with valid Doom required for Doom in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (text6 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Doom in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (cardLayout3 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "DoomAbilityLayout required for Doom in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (text7 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "DoomIcon required for Doom in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (flag3)
						{
							(cAbility3 as CAbilityAddDoom).Doom.ActiveBonusLayout = text6;
							CardLayoutGroup cardLayoutGroup12 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Doom, parent);
							cardLayoutGroup12.CreateDoomData(new CardLayoutGroup.DoomLayout(text7, discardText2, cardLayout3, reminderTextOverride));
							list10.Add(cardLayoutGroup12);
						}
						continue;
					}
					case "Song":
					{
						CAbility cAbility2 = null;
						string text4 = null;
						CardLayout cardLayout2 = null;
						string songIcon = null;
						string discardText = null;
						foreach (MappingEntry entry5 in (item4.Value as Mapping).Entries)
						{
							switch (entry5.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry5.Value, "ParentAbility", FileName, out var value10))
								{
									break;
								}
								foreach (CAbility item7 in list6)
								{
									if (item7.Name == value10)
									{
										cAbility2 = item7;
									}
								}
								if (cAbility2 == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value10 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "ActiveBonusLayout", FileName, out var value7))
								{
									text4 = new CardLayoutRow(value7, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value7);
								}
								break;
							}
							case "SongAbilityLayout":
								if (entry5.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry5.Value, "Song/SongAbilityLayout", FileName, out var sequence4))
									{
										cardLayout2 = new CardLayout(sequence4, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry5.Value is Scalar)
								{
									cardLayout2 = new CardLayout((entry5.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "SongIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "SongIcon", FileName, out var value8))
								{
									songIcon = value8;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "DoomIcon", FileName, out var value9))
								{
									discardText = value9;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Song entry " + entry5.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag2 = true;
						if (cAbility2?.Song == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name with valid Song required for Song in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (text4 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Song in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (cardLayout2 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "SongAbilityLayout required for Song in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (cardLayout2 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "SongIcon required for Song in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (flag2)
						{
							cAbility2.Song.ActiveBonusLayout = text4;
							CardLayoutGroup cardLayoutGroup10 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Song, parent);
							cardLayoutGroup10.CreateSongData(new CardLayoutGroup.SongLayout(songIcon, discardText, cardLayout2));
							list10.Add(cardLayoutGroup10);
						}
						continue;
					}
					case "Consume":
					{
						string name = string.Empty;
						CardLayout cardLayout = null;
						foreach (MappingEntry entry6 in (item4.Value as Mapping).Entries)
						{
							string text3 = entry6.Key.ToString();
							if (!(text3 == "Name"))
							{
								if (text3 == "Text")
								{
									if (entry6.Value is Sequence)
									{
										if (YMLShared.GetSequence(entry6.Value, "Consume/Text", FileName, out var sequence2))
										{
											cardLayout = new CardLayout(sequence2, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName, overrides);
										}
									}
									else if (entry6.Value is Scalar)
									{
										cardLayout = new CardLayout((entry6.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry '" + entry6.Key.ToString() + "' under consumes in file " + FileName);
								}
							}
							else
							{
								YMLShared.GetStringPropertyValue(entry6.Value, "Consume/Name", FileName, out name);
							}
						}
						if (name != string.Empty && cardLayout != null && consumes != null)
						{
							AbilityConsume abilityConsume = consumes.SingleOrDefault((AbilityConsume x) => x.Name == name);
							if (abilityConsume != null)
							{
								CardLayoutGroup cardLayoutGroup5 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Consume, parent);
								abilityConsume.Text = cardLayout;
								cardLayoutGroup5.CreateConsumeData(abilityConsume);
								list10.Add(cardLayoutGroup5);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to find Consume data for entry " + name + " in file " + FileName);
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Consume entry within Layout section of file " + FileName);
						}
						continue;
					}
					case "Infuse":
					{
						CardLayoutGroup cardLayoutGroup7 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Element, parent);
						if (CardProcessingShared.GetElements(item4.Value, "Infuse", FileName, out var elements))
						{
							cardLayoutGroup7.CreateElementData(elements);
							list10.Add(cardLayoutGroup7);
						}
						continue;
					}
					case "Duration":
					{
						if (YMLShared.GetStringPropertyValue(item4.Value, "Duration", FileName, out var durationString))
						{
							CActiveBonus.EActiveBonusDurationType eActiveBonusDurationType = CActiveBonus.ActiveBonusDurationTypes.SingleOrDefault((CActiveBonus.EActiveBonusDurationType x) => x.ToString() == durationString);
							if (eActiveBonusDurationType != CActiveBonus.EActiveBonusDurationType.NA)
							{
								CardLayoutGroup cardLayoutGroup4 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Duration, parent);
								cardLayoutGroup4.CreateDurationData(eActiveBonusDurationType);
								list10.Add(cardLayoutGroup4);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Active Bonus Duration '" + durationString + "' in file '" + FileName);
							}
						}
						continue;
					}
					case "XP":
					{
						CardLayoutGroup cardLayoutGroup17 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.XP, parent);
						if (YMLShared.GetIntList(item4.Value, "XP", FileName, out var values6))
						{
							cardLayoutGroup17.CreateXPData(values6, values6.Count > 1);
							list10.Add(cardLayoutGroup17);
						}
						continue;
					}
					case "XPTracker":
						value6 = "";
						values5 = null;
						value5 = false;
						if (item4.Value is Mapping { Entries: var entries } mapping5)
						{
							MappingEntry mappingEntry2 = entries.FirstOrDefault((MappingEntry me) => me.Key.ToString() == "Ability");
							if (mappingEntry2 != null)
							{
								YMLShared.GetStringPropertyValue(mappingEntry2.Value, "Ability", FileName, out value6);
							}
							else
							{
								mappingEntry2 = mapping5.Entries.FirstOrDefault((MappingEntry me) => me.Key.ToString() == "XP");
								if (mappingEntry2 != null)
								{
									YMLShared.GetIntList(mappingEntry2.Value, "XP", FileName, out values5);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Can't find 'Ability' or 'XP' property in an 'XPTracker' in \n" + FileName);
								}
							}
							mappingEntry2 = mapping5.Entries.FirstOrDefault((MappingEntry me) => me.Key.ToString() == "ShowPersistentIcon");
							if (mappingEntry2 != null)
							{
								YMLShared.GetBoolPropertyValue(mappingEntry2.Value, "ShowPersistentIcon", FileName, out value5);
							}
						}
						else
						{
							YMLShared.GetStringPropertyValue(item4.Value, "XPTracker", FileName, out value6);
						}
						if (values5 == null)
						{
							CAbility fromLinkedAbilities = GetFromLinkedAbilities(value6, list6);
							if (fromLinkedAbilities != null)
							{
								AbilityData.ActiveBonusData activeBonusData = fromLinkedAbilities.ActiveBonusData;
								if (activeBonusData != null)
								{
									CAbilityRequirements requirements = activeBonusData.Requirements;
									if (requirements != null && requirements.ActiveBonusGreaterOrEqualToTrackerIndex.HasValue && fromLinkedAbilities.ActiveBonusData.Tracker != null)
									{
										values5 = new List<int>();
										for (int num2 = fromLinkedAbilities.ActiveBonusData.Requirements.ActiveBonusGreaterOrEqualToTrackerIndex.Value; num2 < fromLinkedAbilities.ActiveBonusData.Tracker.Count; num2++)
										{
											values5.Add(fromLinkedAbilities.ActiveBonusData.Tracker[num2]);
										}
										goto IL_2603;
									}
								}
							}
							values5 = fromLinkedAbilities?.ActiveBonusData?.Tracker;
						}
						goto IL_2603;
					case "Divider":
					{
						CardLayoutGroup cardLayoutGroup14 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Divider, parent, GetColor(item4, "class"));
						cardLayoutGroup14.CreateDivider();
						list10.Add(cardLayoutGroup14);
						continue;
					}
					case "MandatoryTrigger":
					{
						CardLayoutGroup cardLayoutGroup6 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.MandatoryTrigger, parent, GetColor(item4, "class"));
						CardLayout triggerLayout = null;
						DataItem value3 = item4.Value;
						if (value3 is Scalar scalar)
						{
							triggerLayout = new CardLayout(scalar.Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
						}
						else if (value3 is Mapping mapping)
						{
							foreach (MappingEntry entry7 in mapping.Entries)
							{
								if (!(entry7.Key.ToString() == "Layout"))
								{
									continue;
								}
								if (entry7.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry7.Value, "MandatoryTrigger/Layout", FileName, out var sequence3))
									{
										triggerLayout = new CardLayout(sequence3, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry7.Value is Scalar)
								{
									triggerLayout = new CardLayout((entry7.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
							}
						}
						cardLayoutGroup6.CreateMandatoryTrigger(new CardLayoutGroup.MandatoryTriggerLayout(triggerLayout));
						list10.Add(cardLayoutGroup6);
						continue;
					}
					case "Conditional":
					{
						CardLayoutGroup cardLayoutGroup15 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Conditional, parent, GetColor(item4, "class"));
						CardLayout triggerLayout2 = null;
						bool result3 = false;
						bool result4 = true;
						DataItem value20 = item4.Value;
						if (value20 is Scalar scalar3)
						{
							triggerLayout2 = new CardLayout(scalar3.Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
						}
						else if (value20 is Mapping mapping4)
						{
							foreach (MappingEntry entry8 in mapping4.Entries)
							{
								switch (entry8.Key.ToString())
								{
								case "Text":
									triggerLayout2 = new CardLayout((entry8.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									break;
								case "Layout":
									if (entry8.Value is Sequence)
									{
										if (YMLShared.GetSequence(entry8.Value, "Conditional/Layout", FileName, out var sequence9))
										{
											triggerLayout2 = new CardLayout(sequence9, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
										}
									}
									else if (entry8.Value is Scalar)
									{
										triggerLayout2 = new CardLayout((entry8.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									}
									break;
								case "Top":
									if (!(entry8.Value is Scalar scalar5))
									{
										SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process condtional top element " + item4.Value.ToString() + "  File\n" + FileName);
									}
									else
									{
										bool.TryParse(scalar5.Text, out result3);
									}
									break;
								case "Show":
									if (!(entry8.Value is Scalar scalar4))
									{
										SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process condtional top element " + item4.Value.ToString() + "  File\n" + FileName);
									}
									else
									{
										bool.TryParse(scalar4.Text, out result4);
									}
									break;
								}
							}
						}
						cardLayoutGroup15.CreateConditionalElement(new CardLayoutGroup.ConditionalElementLayout(triggerLayout2, result3, result4));
						list10.Add(cardLayoutGroup15);
						continue;
					}
					case "BlinkbladeState":
					{
						CardLayoutGroup cardLayoutGroup11 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.BlinkbladeState, parent, GetColor(item4, "class"));
						CardLayout layout = null;
						bool isFast = false;
						float width = -1f;
						bool isPercent = false;
						if (item4.Value is Mapping mapping3)
						{
							foreach (MappingEntry entry9 in mapping3.Entries)
							{
								switch (entry9.Key.ToString())
								{
								case "State":
									if (entry9.Value is Scalar)
									{
										isFast = (string.Equals(entry9.Value.ToString(), "Fast", StringComparison.InvariantCultureIgnoreCase) ? true : false);
									}
									else
									{
										SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process blinkblade state " + item4.Value.ToString() + "  File\n" + FileName);
									}
									break;
								case "Width":
								{
									string text5 = entry9.Value.ToString();
									isPercent = text5.Contains('%');
									if (float.TryParse(text5.Replace("%", ""), out var result2))
									{
										width = result2;
									}
									break;
								}
								case "Text":
									layout = new CardLayout((entry9.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									break;
								case "Layout":
									if (entry9.Value is Sequence)
									{
										if (YMLShared.GetSequence(entry9.Value, "BlinkbladeState/Layout", FileName, out var sequence5))
										{
											layout = new CardLayout(sequence5, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
										}
									}
									else if (entry9.Value is Scalar)
									{
										layout = new CardLayout((entry9.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									}
									break;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process blinkblade state " + item4.Value.ToString() + "  File\n" + FileName);
						}
						cardLayoutGroup11.CreateBlinkbladeStateElement(new CardLayoutGroup.BlinkbladeStateLayout(layout, isFast, width, isPercent));
						list10.Add(cardLayoutGroup11);
						continue;
					}
					case "AddedEffect":
					{
						CardLayoutGroup cardLayoutGroup8 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.AddedEffect, parent, GetColor(item4, "class"));
						string addedEffect = "";
						DataItem value4 = item4.Value;
						if (value4 is Scalar scalar2)
						{
							addedEffect = scalar2.Text;
						}
						else if (value4 is Mapping mapping2)
						{
							foreach (MappingEntry entry10 in mapping2.Entries)
							{
								if (entry10.Key.ToString() == "Text")
								{
									addedEffect = (entry10.Value as Scalar).Text;
								}
							}
						}
						cardLayoutGroup8.CreateAddedEffect(addedEffect);
						list10.Add(cardLayoutGroup8);
						continue;
					}
					case "Color":
						{
							if (parent != null)
							{
								parent.SetColor(GetColor(item4));
							}
							continue;
						}
						IL_2603:
						if (values5 != null)
						{
							CardLayoutGroup cardLayoutGroup9 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.XP);
							cardLayoutGroup9.CreateXPData(values5, value5 || values5.Count > 1);
							list10.Add(cardLayoutGroup9);
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to find linked ability and/or active bonus tracker for ability named " + value6 + "  File\n" + FileName);
						}
						continue;
					}
					CardLayoutGroup parent5 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row, parent, GetColor(item4));
					if (overrides == null && consumes != null)
					{
						IEnumerable<CActionAugmentationOp> source = consumes.SelectMany((AbilityConsume x) => x.ConsumeData.AugmentationOps.Where((CActionAugmentationOp y) => y.AbilityOverride != null));
						overrides = (from x in source
							where x.AbilityOverride != null
							select x.AbilityOverride).ToList();
					}
					if (item4.Value is Sequence)
					{
						if (IsRowDataSequence(item4.Value as Sequence))
						{
							CardLayoutGroup.RowAttributes rowAttributes2 = null;
							CardLayoutRow cardLayoutRow = null;
							foreach (DataItem entry11 in (item4.Value as Sequence).Entries)
							{
								MappingEntry mappingEntry3 = (entry11 as Mapping).Entries[0];
								if (mappingEntry3.Key.ToString() == "RowProperties")
								{
									rowAttributes2 = GetRowAttributes(mappingEntry3.Value as Sequence);
								}
								else if (mappingEntry3.Key.ToString() == "Data")
								{
									cardLayoutRow = new CardLayoutRow((mappingEntry3.Value as Scalar).Text, AbilityCardName, list6, overrides);
								}
							}
							if (cardLayoutRow != null)
							{
								parent5.CreateRowData(cardLayoutRow, rowAttributes2 ?? rowAttributes);
								list10.Add(parent5);
							}
						}
						else
						{
							CreateChildren(item4.Value, ref parent5, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
							list10.Add(parent5);
						}
					}
					else if (item4.Value is Scalar)
					{
						parent5.CreateRowData(new CardLayoutRow((item4.Value as Scalar).Text, AbilityCardName, list6, overrides), rowAttributes);
						list10.Add(parent5);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process row value " + item4.Value.ToString() + "  File\n" + FileName);
					}
				}
				list10.Reverse();
				parent.CreateRowCollection(list10);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "No entries found to process.  File\n" + FileName);
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unexpected entry in Layout File:\n" + FileName + "\nData Item is not a Sequence or Scalar.\n" + dataItem.ToString());
		}
	}

	private CAbility GetFromLinkedAbilities(string abilityName, List<CAbility> allLinked)
	{
		CAbility result = null;
		if (allLinked.Any((CAbility x) => x.Name == abilityName))
		{
			result = allLinked.Single((CAbility x) => x.Name == abilityName);
		}
		else if (allLinked.SelectMany((CAbility sm) => sm.SubAbilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.SelectMany((CAbility sm) => sm.SubAbilities).Single((CAbility s) => s.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x.Augment != null && x.Augment.Abilities != null && x.Augment.Abilities.Count > 0).SelectMany((CAbility sm) => sm.Augment.Abilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.Where((CAbility x) => x.Augment != null && x.Augment.Abilities != null && x.Augment.Abilities.Count > 0).SelectMany((CAbility sm) => sm.Augment.Abilities).Single((CAbility s) => s.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x is CAbilityAddDoom { Doom: not null } cAbilityAddDoom && cAbilityAddDoom.Doom.DoomAbilities.Count > 0).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.Where((CAbility x) => x is CAbilityAddDoom { Doom: not null } cAbilityAddDoom && cAbilityAddDoom.Doom.DoomAbilities.Count > 0).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities).Single((CAbility a) => a.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities).Any((CAbility s) => s.Name == abilityName))
		{
			allLinked.Where((CAbility x) => x is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities).Single((CAbility s) => s.Name == abilityName);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + abilityName + " not found in linked abilities.\nFile: " + FileName);
		}
		return result;
	}

	private CardLayoutGroup.RowAttributes GetRowAttributes(Sequence rowAttSequence)
	{
		CardLayoutGroup.RowAttributes rowAttributes = new CardLayoutGroup.RowAttributes();
		foreach (Mapping entry in rowAttSequence.Entries)
		{
			MappingEntry mappingEntry = entry.Entries[0];
			switch (mappingEntry.Key.ToString())
			{
			case "AutoSize":
			{
				if (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "RowProperties/AutoSize", FileName, out var value4))
				{
					rowAttributes.AutoSize = value4;
				}
				break;
			}
			case "AutoSizeMin":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/AutoSizeMin", FileName, out var value8))
				{
					rowAttributes.AutoSizeMin = value8;
				}
				break;
			}
			case "AutoSizeMax":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/AutoSizeMax", FileName, out var value10))
				{
					rowAttributes.AutoSizeMax = value10;
				}
				break;
			}
			case "CharSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/CharSpacing", FileName, out var value6))
				{
					rowAttributes.CharSpacing = value6;
				}
				break;
			}
			case "WordSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/WordSpacing", FileName, out var value2))
				{
					rowAttributes.WordSpacing = value2;
				}
				break;
			}
			case "LineSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/LineSpacing", FileName, out var value9))
				{
					rowAttributes.LineSpacing = value9;
				}
				break;
			}
			case "ParSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/ParSpacing", FileName, out var value7))
				{
					rowAttributes.ParSpacing = value7;
				}
				break;
			}
			case "Alignment":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "RowProperties/Alignment", FileName, out var value5))
				{
					rowAttributes.Alignment = value5;
				}
				break;
			}
			case "Wrapping":
			{
				if (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "RowProperties/Wrapping", FileName, out var value3))
				{
					rowAttributes.Wrapping = value3;
				}
				break;
			}
			case "Overflow":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "RowProperties/Overflow", FileName, out var value))
				{
					rowAttributes.Overflow = value;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process RowAttributes entry " + mappingEntry.Key.ToString() + "in file: " + FileName);
				break;
			}
		}
		return rowAttributes;
	}

	private bool IsRowDataSequence(Sequence sequence)
	{
		if (sequence.Entries.Count > 0 && sequence.Entries[0] is Mapping mapping && mapping.Entries.Count > 0 && mapping.Entries.Any((MappingEntry x) => x.Key.ToString() == "Data"))
		{
			return true;
		}
		return false;
	}

	private string GetColor(MappingEntry entry, string defaultColor = "")
	{
		if (entry.Value is Scalar scalar && entry.Key.ToString() == "Color")
		{
			return scalar.Text;
		}
		if (!(entry.Value is Mapping mapping))
		{
			return defaultColor;
		}
		foreach (MappingEntry entry2 in mapping.Entries)
		{
			if (!(entry2.Key.ToString() != "Color") && YMLShared.GetStringPropertyValue(entry2.Value, "Color", FileName, out var value))
			{
				return value.ToLowerInvariant();
			}
		}
		return defaultColor;
	}
}
