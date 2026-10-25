using System.Collections.Generic;
using FHGUI.Elements;

namespace FHGUI.Common;

public class XPTrackerContainer : ListContainerBase<XPTrackerElement, object, XPTrackerElement.XPTrackerData>
{
	private readonly List<XPTrackerElement.XPTrackerData> _xpTrackerData = new List<XPTrackerElement.XPTrackerData>(10);

	protected override void InitElement(XPTrackerElement listElement, XPTrackerElement.XPTrackerData elementParams, object listParams)
	{
		listElement.Init(elementParams);
	}

	public void Init(IList<int> xpTrack, int activeIndex = -1, bool hideZeroXP = true)
	{
		_xpTrackerData.Clear();
		for (int i = 0; i < xpTrack.Count; i++)
		{
			_xpTrackerData.Add(new XPTrackerElement.XPTrackerData
			{
				XP = xpTrack[i],
				IsActive = (i == activeIndex),
				HideZeroXP = hideZeroXP,
				IsAvailable = (activeIndex == -1 || i > activeIndex)
			});
		}
		Init(null, _xpTrackerData);
	}
}
