using UnityEngine;

namespace FHGUI;

public static class FHUIColors
{
	public static Color MERCHANT_HEADER = GetColor("#2D9532FF");

	public static Color CRAFTER_HEADER = GetColor("#FFC96FFF");

	public static Color MULTIPLAYER_READY = GetColor("#47D080FF");

	public static Color MULTIPLAYER_PENDING = GetColor("#52667FFF");

	public static Color VOTE_READY = GetColor("#5ED7C8FF");

	public static Color VOTE_PENDING = GetColor("#CC620EFF");

	public static Color TEXT_LABEL = GetColor("#CDBF84FF");

	public static Color MISSING_RESOURCE = GetColor("#FF624FFF");

	private static Color GetColor(string htmlString)
	{
		if (ColorUtility.TryParseHtmlString(htmlString, out var color))
		{
			return color;
		}
		return Color.white;
	}
}
