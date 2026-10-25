using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FHGUI.Common.Glossary;

[RequireComponent(typeof(UITextTooltipTarget))]
public class AutoGlossaryTooltip : LocalizedListener, IGlossaryProvider
{
	private UITextTooltipTarget _tooltip;

	public UITextTooltipTarget Tooltip
	{
		get
		{
			if (_tooltip == null)
			{
				_tooltip = GetComponent<UITextTooltipTarget>();
			}
			return _tooltip;
		}
	}

	public GameObject GameObject { get; }

	private void Start()
	{
		BuildGlossary();
	}

	private void BuildGlossary()
	{
		if (!(Tooltip != null))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (UIGlossaryData item in GetGlossary())
		{
			stringBuilder.Append(item.GetDescription());
			stringBuilder.Append("<br>");
		}
		Tooltip.SetText(stringBuilder.ToString());
	}

	public IEnumerable<UIGlossaryData> GetGlossary()
	{
		return LocalizationGlossaryUtils.GetGlossaryContents(gameObject);
	}

	protected override void OnLanguageChanged()
	{
		BuildGlossary();
	}
}
