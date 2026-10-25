using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class XPTrackerElement : MonoBehaviour
{
	public struct XPTrackerData
	{
		public int XP;

		public bool HideZeroXP;

		public bool IsActive;

		public bool IsAvailable;
	}

	[SerializeField]
	private TMP_Text _trackerText;

	[SerializeField]
	private Image _trackerIcon;

	[SerializeField]
	private Image _activeIcon;

	[SerializeField]
	private Sprite activeIcon;

	[SerializeField]
	private Sprite pastRoundIcon;

	[SerializeField]
	private Sprite leftRoundIcon;

	public void Init(XPTrackerData data)
	{
		_trackerText.enabled = data.XP != 0 || !data.HideZeroXP;
		if (!data.IsActive)
		{
			if (!data.IsAvailable)
			{
				_ = UIInfoTools.Instance.pastRoundColor;
			}
			else
			{
				_ = UIInfoTools.Instance.leftRoundColor;
			}
		}
		else
		{
			_ = UIInfoTools.Instance.currentRoundColor;
		}
		Color color;
		if (data.IsActive)
		{
			color = UIInfoTools.Instance.currentRoundTextColor;
		}
		else
		{
			color = (data.IsAvailable ? UIInfoTools.Instance.leftRoundTextColor : UIInfoTools.Instance.pastRoundTextColor);
		}
		_trackerText.text = ((data.XP == 0) ? "" : new string('I', data.XP));
		_trackerText.color = color;
		Image trackerIcon = _trackerIcon;
		Sprite sprite;
		if (data.IsActive)
		{
			sprite = activeIcon;
		}
		else
		{
			sprite = (data.IsAvailable ? leftRoundIcon : pastRoundIcon);
		}
		trackerIcon.sprite = sprite;
	}
}
