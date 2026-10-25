using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GLOOM;
using ScenarioRuleLibrary;

public static class LocalizationScenarioModifierConveter
{
	public static string LocalizeText(this CScenarioModifier scenarioModifier, string scenarioID)
	{
		string translation = "";
		if (!LocalizationManager.TryGetTranslation(scenarioID + "_" + scenarioModifier.Name + "_DESC", out translation))
		{
			if (scenarioModifier.LocKey.IsNullOrEmpty())
			{
				Log.GUIWarningFormat("Missin LocKey in CScenarioModifier {0}", scenarioModifier.ScenarioAbilityID);
				return translation;
			}
			translation = LocalizationManager.GetTranslation(scenarioModifier.LocKey);
			string translation2 = string.Empty;
			LocalizationManager.TryGetTranslation(scenarioModifier.TriggerLocKey, out translation2);
			translation = Regex.Replace(translation, "{hazardous}", ScenarioManager.HazardousTerrainDamage.ToString());
			translation = Regex.Replace(translation, "{trap}", ScenarioManager.TrapDamage.ToString());
			try
			{
				object[] array = scenarioModifier.CustomLocKeyArgs?.Select((object arg) =>
				{
					object result = ((arg is string text2) ? ((!LocalizationManager.TryGetTranslation(text2, out var translation8)) ? text2 : translation8) : arg);
					return result;
				}).ToArray();
				translation = string.Format(translation, array ?? new object[0]);
			}
			catch (FormatException)
			{
				Log.Warning("Invalid CustomLocKeyArgs for scenarioModifier.LocKey: " + scenarioModifier.LocKey);
				translation = "INVALID CUSTOM_LOC_KEY_ARGS";
			}
			switch (scenarioModifier.ScenarioModifierType)
			{
			case EScenarioModifierType.SetElements:
			{
				CScenarioModifierSetElements cScenarioModifierSetElements = scenarioModifier as CScenarioModifierSetElements;
				StringBuilder stringBuilder = new StringBuilder();
				for (int num3 = 0; num3 < cScenarioModifierSetElements.StrongElements.Count; num3++)
				{
					string translation5 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.StrongElements[num3].ToString().ToUpper());
					if (num3 < cScenarioModifierSetElements.StrongElements.Count - 1)
					{
						stringBuilder.Append(translation5 + ", ");
					}
					else
					{
						stringBuilder.Append(translation5);
					}
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int num4 = 0; num4 < cScenarioModifierSetElements.WaningElements.Count; num4++)
				{
					string translation6 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.WaningElements[num4].ToString().ToUpper());
					if (num4 < cScenarioModifierSetElements.WaningElements.Count - 1)
					{
						stringBuilder2.Append(translation6 + ", ");
					}
					else
					{
						stringBuilder2.Append(translation6);
					}
				}
				StringBuilder stringBuilder3 = new StringBuilder();
				for (int num5 = 0; num5 < cScenarioModifierSetElements.InertElements.Count; num5++)
				{
					string translation7 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.InertElements[num5].ToString().ToUpper());
					if (num5 < cScenarioModifierSetElements.InertElements.Count - 1)
					{
						stringBuilder3.Append(translation7 + ", ");
					}
					else
					{
						stringBuilder3.Append(translation7);
					}
				}
				if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder, stringBuilder2, stringBuilder3, translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder, stringBuilder2, translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder, stringBuilder3, translation2);
				}
				else if (cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder2, stringBuilder3, translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder, translation2);
				}
				else if (cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder2, translation2);
				}
				else if (cScenarioModifierSetElements.InertElements.Count > 0)
				{
					translation = string.Format(translation, stringBuilder3, translation2);
				}
				break;
			}
			case EScenarioModifierType.TriggerAbility:
			{
				CAbility cAbility = (scenarioModifier as CScenarioModifierTriggerAbility).TriggerAbility(null, 2, copyAbility: false);
				translation = string.Format(translation, (cAbility != null) ? cAbility.AbilityType.ToString() : "NULL", translation2);
				break;
			}
			case EScenarioModifierType.AddConditionsToAbilities:
			{
				CScenarioModifierAddConditionsToAbilities cScenarioModifierAddConditionsToAbilities = scenarioModifier as CScenarioModifierAddConditionsToAbilities;
				string text = "";
				for (int num = 0; num < cScenarioModifierAddConditionsToAbilities.PositiveConditions.Count; num++)
				{
					text = text + LocalizationManager.GetTranslation(cScenarioModifierAddConditionsToAbilities.PositiveConditions[num].ToString()) + ", ";
					if (num < cScenarioModifierAddConditionsToAbilities.PositiveConditions.Count - 1 || cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count > 0)
					{
						text += ", ";
					}
				}
				for (int num2 = 0; num2 < cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count; num2++)
				{
					text += LocalizationManager.GetTranslation(cScenarioModifierAddConditionsToAbilities.NegativeConditions[num2].ToString());
					if (num2 < cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count - 1)
					{
						text += ", ";
					}
				}
				if (scenarioModifier.ScenarioModifierFilter != null && scenarioModifier.ScenarioModifierFilter.FilterAbilityType != CAbility.EAbilityType.None)
				{
					string translation3 = LocalizationManager.GetTranslation(scenarioModifier.ScenarioModifierFilter.FilterAbilityType.ToString());
					if (scenarioModifier.ScenarioModifierFilter.FilterActorType != CAbilityFilter.EFilterActorType.None)
					{
						string translation4 = LocalizationManager.GetTranslation(scenarioModifier.ScenarioModifierFilter.ActorLocKey);
						translation = string.Format(translation, translation3, text, translation4);
					}
					else
					{
						translation = string.Format(translation, translation3, text);
					}
				}
				translation = string.Format(translation, text);
				break;
			}
			}
		}
		return translation;
	}
}
