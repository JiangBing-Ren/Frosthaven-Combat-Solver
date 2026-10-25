using FHRL;
using UnityEngine;

namespace Script.GUI.MainMenu;

public class MainMenuExternalLinksUI : MonoBehaviour
{
	[Header("Links")]
	[SerializeField]
	private FHButton _changelogButton;

	[SerializeField]
	private UIModulePatchNotes _changelogNew;

	[SerializeField]
	private VersionAndChangeLog _changelog;

	[SerializeField]
	private FHButton _discordLinkButton;

	[SerializeField]
	private FHButton _websiteLinkButton;

	[SerializeField]
	private FHButton _steamLinkButton;

	private const string DISCORD_LINK = "https://discord.gg/FUNmzWVfz2";

	private const string WEBSITE_LINK = "https://www.playfrosthaven.com/en/";

	private const string STEAM_LINK = "https://store.steampowered.com/app/2347080/Frosthaven/";

	private const string STEAM_NEWS_LINK = "https://steamcommunity.com/app/2347080/announcements/";

	private void Awake()
	{
		_changelogButton.onClick.AddListener(() =>
		{
			if (_changelog != null)
			{
				_changelog.OpenChangelog();
			}
			Application.OpenURL("https://steamcommunity.com/app/2347080/announcements/");
		});
		_discordLinkButton.onClick.AddListener(() =>
		{
			Application.OpenURL("https://discord.gg/FUNmzWVfz2");
		});
		_websiteLinkButton.onClick.AddListener(() =>
		{
			Application.OpenURL("https://www.playfrosthaven.com/en/");
		});
		_steamLinkButton.onClick.AddListener(() =>
		{
			Application.OpenURL("https://store.steampowered.com/app/2347080/Frosthaven/");
		});
		_changelogButton.gameObject.SetActive(value: true);
		if (ReleaseStage.IsDemo())
		{
			_changelogButton.gameObject.SetActive(value: false);
		}
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
