using UnityEngine;
using UnityEngine.UI;

public class SubmenuAutoadjust : MonoBehaviour
{
	[SerializeField]
	private RectTransform areaToAdjust;

	[SerializeField]
	private UIWindow target;

	[SerializeField]
	private Vector2 sizeOnTargetOpen;

	[SerializeField]
	private Vector2 positionOnTargetOpen;

	[SerializeField]
	private Vector2 sizeOnTargetClosed;

	[SerializeField]
	private Vector2 positionOnTargetClosed;

	public void Autoadjust()
	{
		if (target.IsOpen)
		{
			areaToAdjust.sizeDelta = sizeOnTargetOpen;
			areaToAdjust.anchoredPosition = positionOnTargetOpen;
		}
		else
		{
			areaToAdjust.sizeDelta = sizeOnTargetClosed;
			areaToAdjust.anchoredPosition = positionOnTargetClosed;
		}
	}
}
