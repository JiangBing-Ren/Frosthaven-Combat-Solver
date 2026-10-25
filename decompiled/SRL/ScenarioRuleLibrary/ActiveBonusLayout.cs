using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ActiveBonusLayout
{
	public bool UseAlt;

	private CardLayoutGroup LayoutNorm;

	private CardLayoutGroup LayoutAlt;

	private List<string> ListLayoutsNorm;

	private List<string> ListLayoutsAlt;

	public string CardName { get; set; }

	public List<string> IconNames { get; private set; }

	public List<int> TrackerPattern { get; set; }

	public DiscardType Discard { get; private set; }

	public CardLayoutGroup Layout
	{
		get
		{
			if (!UseAlt || LayoutAlt == null)
			{
				return LayoutNorm;
			}
			return LayoutAlt;
		}
	}

	public List<string> ListLayouts
	{
		get
		{
			if (!UseAlt || ListLayoutsAlt == null || ListLayoutsAlt.Count <= 0)
			{
				return ListLayoutsNorm;
			}
			return ListLayoutsAlt;
		}
	}

	public ActiveBonusLayout(string cardName, List<string> iconNames, CardLayoutGroup layout, CardLayoutGroup layoutAlt, List<string> listLayouts, List<string> listLayoutsAlt, List<int> trackerPattern, DiscardType discard)
	{
		CardName = cardName;
		IconNames = iconNames;
		LayoutNorm = layout;
		LayoutAlt = layoutAlt;
		ListLayoutsNorm = listLayouts;
		ListLayoutsAlt = listLayoutsAlt;
		TrackerPattern = trackerPattern;
		Discard = discard;
	}

	public ActiveBonusLayout Copy()
	{
		return new ActiveBonusLayout(CardName, IconNames, LayoutNorm, LayoutAlt, ListLayoutsNorm, ListLayoutsAlt, TrackerPattern, Discard)
		{
			UseAlt = UseAlt
		};
	}
}
