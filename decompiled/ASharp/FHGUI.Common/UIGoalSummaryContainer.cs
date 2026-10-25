using FHGUI.DTO;
using FHGUI.Elements;

namespace FHGUI.Common;

public class UIGoalSummaryContainer : ListContainerBase<UIGoalSummary, object, GoalDTO>
{
	protected override void InitElement(UIGoalSummary listElement, GoalDTO goal, object listParams)
	{
		listElement.Init(new UIGoalSummary.Params
		{
			DTO = goal,
			IgnoreLocalization = false,
			HintsLiteral = string.Empty,
			OnClickedCallback = null
		});
	}
}
