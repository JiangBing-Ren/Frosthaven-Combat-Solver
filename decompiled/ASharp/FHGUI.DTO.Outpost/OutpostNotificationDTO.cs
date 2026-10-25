using System;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

public record OutpostNotificationDTO
{
	public readonly string ID;

	public string Description { get; private set; }

	public Sprite Icon { get; private set; }

	public Action OnClick { get; private set; }

	public OutpostNotificationType OutpostNotificationType { get; private set; }

	public CVars Vars { get; private set; }

	public OutpostNotificationDTO(string id, OutpostNotificationType type, string description, Action onToggle = null, Sprite icon = null, CVars vars = null)
	{
		ID = id;
		OutpostNotificationType = type;
		Description = description;
		OnClick = onToggle;
		Icon = icon;
		Vars = vars;
	}

	public virtual bool Equals(OutpostNotificationDTO other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return ID == other.ID;
	}

	public override int GetHashCode()
	{
		if (ID == null)
		{
			return 0;
		}
		return ID.GetHashCode();
	}

	public string GetStringVar(string key, CVarsContext context = null, string defVal = null, bool calcExpression = true)
	{
		if (Vars != null)
		{
			return Vars.GetVar(key).GetString(context, defVal, calcExpression);
		}
		return defVal;
	}

	public int GetIntVar(string key, CVarsContext context = null, int defVal = 0, bool calcExpression = true)
	{
		if (Vars != null)
		{
			return Vars.GetVar(key).GetInt(context, defVal, calcExpression);
		}
		return defVal;
	}

	public bool GetBoolVar(string key, CVarsContext context = null, bool defVal = false, bool calcExpression = true)
	{
		if (Vars != null)
		{
			return Vars.GetVar(key).GetBool(context, defVal, calcExpression);
		}
		return defVal;
	}
}
