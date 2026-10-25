namespace FHGUI.Elements;

public class UIOutpostNotificationElementContainer : ListContainerBase<UIOutpostNotificationElement, UIOutpostNotificationElement.OutpostNotificationParams, UIOutpostNotificationElement.OutpostNotificationElementData>
{
	public UIOutpostNotificationElement Selected { get; private set; }

	protected override void InitElement(UIOutpostNotificationElement listElement, UIOutpostNotificationElement.OutpostNotificationElementData elementParams, UIOutpostNotificationElement.OutpostNotificationParams listParams)
	{
		listElement.Init(elementParams, listParams, OnClick);
	}

	private void OnDisable()
	{
		if ((bool)Selected)
		{
			Selected.HideCarousel();
		}
		Selected = null;
	}

	private void OnClick(UIOutpostNotificationElement element)
	{
		if (Selected != null && Selected != element)
		{
			Selected.HideCarousel();
		}
		else
		{
			Selected = element;
		}
	}
}
