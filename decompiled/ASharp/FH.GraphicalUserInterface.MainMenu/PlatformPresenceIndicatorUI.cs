using FH.Platforms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FH.GraphicalUserInterface.MainMenu;

public sealed class PlatformPresenceIndicatorUI : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Image _presenceIndicator;

	[SerializeField]
	private Color _presenceColorOnline = Color.green;

	[SerializeField]
	private Color _presenceColorOffline = Color.grey;

	[SerializeField]
	private Color _presenceColorAway = Color.red;

	private void OnEnable()
	{
		PlatformLayer.Instance.NewPlatformLayer.GetCurrentUserData((PlatformUserData user) =>
		{
			Debug.Log("PlatformPresenceIndicatorUI: OnEnable: user.PlatformPresence: " + user.PlatformPresence);
			if (user.PlatformPresence == PresenceStatus.UnavailableOnPlatform)
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				if ((bool)_text)
				{
					_text.text = user.Name;
				}
				if ((bool)_image && (bool)user.Avatar)
				{
					_image.sprite = user.Avatar;
				}
				if ((bool)_presenceIndicator)
				{
					_presenceIndicator.color = GetPresenceColor(user.PlatformPresence);
				}
			}
		});
	}

	private Color GetPresenceColor(PresenceStatus platformPresence)
	{
		return platformPresence switch
		{
			PresenceStatus.Online => _presenceColorOnline, 
			PresenceStatus.Offline => _presenceColorOffline, 
			PresenceStatus.Away => _presenceColorAway, 
			_ => Color.black, 
		};
	}
}
