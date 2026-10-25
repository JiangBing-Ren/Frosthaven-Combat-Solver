using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class AbilityConsume
{
	public class ConsumeLayoutProperties
	{
		public bool Hidden;

		public int GapAbove;

		public int GapBelow;

		public string LayoutPriorTo = string.Empty;

		public ConsumeLayoutProperties Copy()
		{
			return new ConsumeLayoutProperties
			{
				Hidden = Hidden,
				GapAbove = GapAbove,
				GapBelow = GapBelow,
				LayoutPriorTo = LayoutPriorTo
			};
		}
	}

	public const string GENERATED = "Generated";

	private string m_FileName;

	public string Name { get; private set; }

	public CActionAugmentation ConsumeData { get; private set; }

	public CardLayout Text { get; set; }

	public string Description { get; private set; }

	public string OverrideIcon { get; private set; }

	public AbilityConsume(CActionAugmentation consumeData, string fileName = "Generated", string description = null, string overrideIcon = null)
	{
		Name = consumeData.Name;
		ConsumeData = consumeData;
		m_FileName = fileName;
		Description = description;
		OverrideIcon = overrideIcon;
	}

	public static bool CreateAbilityConsume(Mapping mapping, string name, int cardID, string filename, bool isMonster, List<CAbility> actionAbilities, out AbilityConsume abilityConsume)
	{
		bool flag = true;
		abilityConsume = null;
		try
		{
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
			int num = 0;
			int num2 = 0;
			string description = string.Empty;
			string overrideIcon = string.Empty;
			CAbility costAbility = null;
			List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>();
			List<CActionAugmentationOp> list3 = new List<CActionAugmentationOp>();
			int xp = 0;
			ConsumeLayoutProperties consumeLayoutProperties = new ConsumeLayoutProperties();
			bool? flag2 = null;
			int? num3 = null;
			int? num4 = null;
			string previewEffectId = null;
			string previewEffectText = null;
			string consumeGroup = null;
			bool consumeOnlyOneElement = false;
			CAbilityRequirements requirements = null;
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Requirements":
				{
					if (AbilityData.GetAbilityRequirements(entry, filename, out var abilityRequirements))
					{
						requirements = abilityRequirements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Element":
				case "Elements":
				{
					if (CardProcessingShared.GetElements(entry.Value, name + "/Elements", filename, out var elements))
					{
						list = elements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Trophies":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, name + "/Trophies", filename, out var value8))
					{
						num = value8;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Description":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/Description", filename, out var value2))
					{
						description = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "OverrideIcon":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/OverrideIcon", filename, out var value3))
					{
						overrideIcon = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Resonance":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, name + "/Resonance", filename, out var value10))
					{
						num2 = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConsumeOnlyOneElement":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, name + "/ConsumeOnlyOneElement", filename, out var value7))
					{
						consumeOnlyOneElement = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CostAbility":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping5))
					{
						if (AbilityData.ProcessAbilityEntry(filename, mapping5, name + "CostAbility", cardID, isMonster, out var ability, isSubAbility: false, isConsumeAbility: true))
						{
							costAbility = ability;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Abilities":
				{
					new List<CAbility>();
					if (YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							if (YMLShared.GetMapping(entry2, filename, out var mapping3))
							{
								if (AbilityData.ProcessAbilityEntry(filename, mapping3, entry2.Key.ToString(), cardID, isMonster, out var newAbility, isSubAbility: false, isConsumeAbility: true))
								{
									string parentAbilityName = string.Empty;
									CAbility.EAbilityType parentAbilityType = CAbility.EAbilityType.None;
									if (newAbility.ParentName != string.Empty)
									{
										CAbility cAbility = actionAbilities.SingleOrDefault((CAbility x) => x.Name == newAbility.ParentName);
										if (cAbility != null)
										{
											parentAbilityName = cAbility.Name;
											parentAbilityType = cAbility.AbilityType;
										}
									}
									list3.Add(new CActionAugmentationOp(CActionAugmentationOp.EActionAugmentationType.Ability, parentAbilityName, parentAbilityType, newAbility, null));
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
						num3 = 50;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, name + "/XP", filename, out var value12))
					{
						xp = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Infuse":
				{
					if (CardProcessingShared.GetElements(entry.Value, "Infuse", filename, out var elements2))
					{
						list2 = elements2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityOverrides":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping4))
					{
						foreach (MappingEntry entry3 in mapping4.Entries)
						{
							CAbilityOverride newOverride = CAbilityOverride.CreateAbilityOverride(entry3, cardID, isMonster, filename, "", isConsume: true);
							if (newOverride != null)
							{
								List<CAbility> allActionAbilities = new List<CAbility>();
								allActionAbilities.AddRange(actionAbilities);
								GetAllActionAbilitiesRecursive(ref allActionAbilities, ref actionAbilities);
								CAbility cAbility2 = allActionAbilities.SingleOrDefault((CAbility x) => x.Name == newOverride.ParentName);
								if (cAbility2 != null)
								{
									list3.Add(new CActionAugmentationOp(CActionAugmentationOp.EActionAugmentationType.AbilityOverride, cAbility2.Name, cAbility2.AbilityType, null, newOverride));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find parent ability " + newOverride.ParentName + " in action abilities for this override, needs to be specified.  File: " + filename);
								flag = false;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Override was null.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Hidden":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Hidden", filename, out var value5))
					{
						flag2 = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "GapAbove":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapAbove", filename, out var value))
					{
						num3 = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "GapBelow":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapBelow", filename, out var value11))
					{
						num4 = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LayoutPriorTo":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "LayoutPriorTo", filename, out var layoutPriorToValue))
					{
						if (actionAbilities.SingleOrDefault((CAbility x) => x.Name == layoutPriorToValue) != null)
						{
							consumeLayoutProperties.LayoutPriorTo = layoutPriorToValue;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find LayoutPriorTo ability " + layoutPriorToValue + ".  File: " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PreviewEffectId":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/PreviewEffectId", filename, out var value9))
					{
						previewEffectId = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PreviewEffectText":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/PreviewEffectText", filename, out var value6))
					{
						previewEffectText = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConsumeGroup":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Targeting", filename, out var value4))
					{
						consumeGroup = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid consume entry " + entry.Key?.ToString() + ", File: " + filename);
					flag = false;
					break;
				}
			}
			if (flag2.HasValue)
			{
				consumeLayoutProperties.Hidden = flag2.Value;
			}
			if (num3.HasValue)
			{
				consumeLayoutProperties.GapAbove = num3.Value;
			}
			else if (!list3.Any((CActionAugmentationOp x) => x.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride) && consumeLayoutProperties.LayoutPriorTo != actionAbilities.First().Name)
			{
				consumeLayoutProperties.GapAbove = 50;
			}
			if (num4.HasValue)
			{
				consumeLayoutProperties.GapBelow = num4.Value;
			}
			else if (actionAbilities.Count > 0 && consumeLayoutProperties.LayoutPriorTo == actionAbilities.First().Name)
			{
				consumeLayoutProperties.GapBelow = 50;
			}
			if (flag && (list != null || num > 0 || num2 > 0) && (list3.Count > 0 || list2.Count > 0))
			{
				CActionAugmentation consumeData = new CActionAugmentation(name, list, num, num2, costAbility, list2, list3, xp, consumeLayoutProperties, previewEffectId, previewEffectText, consumeGroup, consumeOnlyOneElement, requirements);
				abilityConsume = new AbilityConsume(consumeData, filename, description, overrideIcon);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Consume Elements missing or invalid consume operation specified (did you forget to put parent ability?), File: " + filename);
				flag = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Consume entry. File:\n" + filename, ex);
			flag = false;
		}
		return flag;
	}

	public static void GetAllActionAbilitiesRecursive(ref List<CAbility> allActionAbilities, ref List<CAbility> previouslyFoundAbilities)
	{
		List<CAbility> previouslyFoundAbilities2 = new List<CAbility>();
		foreach (CAbility previouslyFoundAbility in previouslyFoundAbilities)
		{
			previouslyFoundAbilities2.AddRange(previouslyFoundAbility.SubAbilities);
			if (previouslyFoundAbility is CAbilityPerformFromShadow cAbilityPerformFromShadow)
			{
				previouslyFoundAbilities2.AddRange(cAbilityPerformFromShadow.Abilities);
			}
			if (previouslyFoundAbility is CAbilityMerged cAbilityMerged)
			{
				previouslyFoundAbilities2.AddRange(cAbilityMerged.MergedAbilities);
			}
			if (previouslyFoundAbility is CAbilityControlActor cAbilityControlActor)
			{
				previouslyFoundAbilities2.AddRange(cAbilityControlActor.ControlActorData.ControlAbilities);
			}
			if (!(previouslyFoundAbility is CAbilityConsume cAbilityConsume))
			{
				continue;
			}
			previouslyFoundAbilities2.AddRange(cAbilityConsume.Consumes.SelectMany((AbilityConsume x) => x.ConsumeData.AugmentationOps.Select((CActionAugmentationOp y) => y.Ability)));
		}
		allActionAbilities.AddRange(previouslyFoundAbilities2);
		if (previouslyFoundAbilities2.Count > 0)
		{
			GetAllActionAbilitiesRecursive(ref allActionAbilities, ref previouslyFoundAbilities2);
		}
	}
}
