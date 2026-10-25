namespace UnityEngine.UI;

[AddComponentMenu("Layout/Vertical Layout Group Extended", 151)]
public class VerticalLayoutGroupExtended : HorizontalOrVerticalLayoutGroupExtended
{
	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxisExtended(0, isVertical: true);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxisExtended(1, isVertical: true);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxisExtended(0, isVertical: true);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxisExtended(1, isVertical: true);
	}
}
