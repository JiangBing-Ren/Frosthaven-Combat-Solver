using System;

namespace FHGUI.DTO;

public class CampaignSettingDTO
{
	public class Option
	{
		public string NameKey;

		public string NameLocalized;

		public string DescriptionKey;

		public string DescriptionLocalized;

		public SettingValue Value;

		public bool IsDefault;
	}

	public class SettingValue
	{
		public object Current;

		public object Default;

		public object Min;

		public object Max;

		public static implicit operator SettingValue(bool val)
		{
			return new SettingValue(val);
		}

		public static implicit operator SettingValue(int val)
		{
			return new SettingValue(val);
		}

		public static implicit operator SettingValue(string val)
		{
			return new SettingValue(val);
		}

		public SettingValue(object current)
			: this(current, current, current, current)
		{
		}

		public SettingValue(object current, object @default)
		{
			Current = current;
			Default = @default;
		}

		public SettingValue(object current, object @default, object min, object max)
		{
			Current = current;
			Default = @default;
			Min = min;
			Max = max;
		}
	}

	public Option[] Options;

	public string NameLocalized;

	public string DescriptionLocalized;

	public string TooltipLocalized;

	public SettingValue Value;

	public bool Interactable;

	public bool IsCaption;

	public bool IsDescription;

	public Action<object> OnValueChange;

	public override string ToString()
	{
		return string.Format("CampaignSetting[type:{0}current:{1}, default{2}, options:{3}]", Value.GetType(), Value, Value.Default, (Options == null) ? "None" : ((object)Options.Length));
	}

	public bool HasOptions()
	{
		if (Options != null)
		{
			return Options.Length > 1;
		}
		return false;
	}
}
