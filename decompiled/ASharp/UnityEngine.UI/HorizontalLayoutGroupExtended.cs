namespace UnityEngine.UI;

[AddComponentMenu("Layout/Horizontal Layout Group Extended", 150)]
public class HorizontalLayoutGroupExtended : HorizontalOrVerticalLayoutGroupExtended
{
	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxisExtended(0, isVertical: false);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxisExtended(1, isVertical: false);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxisExtended(0, isVertical: false);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxisExtended(1, isVertical: false);
	}
}
