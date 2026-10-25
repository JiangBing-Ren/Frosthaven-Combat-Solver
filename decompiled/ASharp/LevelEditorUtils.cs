using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;

public static class LevelEditorUtils
{
	public static string FormatActorOption(CActor actor)
	{
		return $"{actor.GetPrefabName()} {actor.ID} - [{actor.ActorGuid}]";
	}

	public static string FormatPropOption(CObjectProp prop)
	{
		return $"{((prop.SpecificPropType != ESpecificPropType.None) ? ((object)prop.SpecificPropType) : ((object)prop.ObjectType))} {prop.ArrayIndex} - [{prop.PropGuid}]";
	}

	public static string ExtractGuidFromFormattedOption(string formattedOption)
	{
		if (formattedOption.Contains('['))
		{
			return formattedOption.Substring(formattedOption.LastIndexOf('[') + 1, formattedOption.LastIndexOf(']') - 1 - formattedOption.LastIndexOf('['));
		}
		return formattedOption;
	}

	public static void SetFormattedOptions(TMP_Dropdown dropdown, IEnumerable<CActor> actors, string selectedGuid)
	{
		SetFormattedOptions(dropdown, actors, Array.Empty<CObjectProp>(), selectedGuid);
	}

	public static void SetFormattedOptions(TMP_Dropdown dropdown, IEnumerable<ActorState> actorStates, string selectedGuid)
	{
		SetFormattedOptions(dropdown, actorStates.Select((ActorState a) => a.Actor), Array.Empty<CObjectProp>(), selectedGuid);
	}

	public static void SetFormattedOptions(TMP_Dropdown dropdown, IEnumerable<CObjectProp> props, string selectedGuid)
	{
		SetFormattedOptions(dropdown, Array.Empty<CActor>(), props, selectedGuid);
	}

	public static void SetFormattedOptions(TMP_Dropdown dropdown, IEnumerable<CActor> actors, IEnumerable<CObjectProp> props, string selectedGuid, string NoneOption = null)
	{
		dropdown.ClearOptions();
		List<string> list = new List<string>();
		if (NoneOption != null)
		{
			list.Add(NoneOption);
		}
		int num = 0;
		int valueWithoutNotify = 0;
		foreach (CActor actor in actors)
		{
			list.Add(FormatActorOption(actor));
			if (actor.ActorGuid == selectedGuid)
			{
				valueWithoutNotify = num;
			}
			num++;
		}
		foreach (CObjectProp prop in props)
		{
			list.Add(FormatPropOption(prop));
			if (prop.PropGuid == selectedGuid)
			{
				valueWithoutNotify = num;
			}
			num++;
		}
		dropdown.AddOptions(list);
		dropdown.SetValueWithoutNotify(valueWithoutNotify);
	}
}
