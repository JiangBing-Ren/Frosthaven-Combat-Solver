using System;

namespace ReportIssueGui;

[AttributeUsage(AttributeTargets.Field)]
public class BindAttribute : Attribute
{
	public string Name;

	public bool BindByType;

	public string Groups;

	public bool Invisible;

	public bool Disabled;

	public bool Checked;

	public bool ToParent;

	public bool SkipParent;

	public bool OnlyEnabled;

	public string CanvasSource;
}
