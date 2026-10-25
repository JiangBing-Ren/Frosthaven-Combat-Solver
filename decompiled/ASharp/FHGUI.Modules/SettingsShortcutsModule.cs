using System;
using FHGUI.Common;
using UnityEngine;

namespace FHGUI.Modules;

public class SettingsShortcutsModule : FHMapViewModule
{
	public class Params
	{
		public Action OnSettingsClicked;

		public Action OnWikiClicked;
	}

	[Header("Components")]
	[SerializeField]
	private FHButton _settingsButton;

	[SerializeField]
	private FHButton _wikiButton;

	private Params _params;

	protected override bool PlaySoundOnEnable => false;

	private void Awake()
	{
		_settingsButton.onClick.AddListener(() =>
		{
			_params?.OnSettingsClicked?.Invoke();
		});
		_wikiButton.onClick.AddListener(() =>
		{
			_params?.OnWikiClicked?.Invoke();
		});
	}

	protected override void OnHide()
	{
		base.OnHide();
		_params = null;
	}

	public void Init(Params @params)
	{
		_params = @params;
		_settingsButton.interactable = @params.OnSettingsClicked != null;
		_wikiButton.interactable = @params.OnWikiClicked != null;
		if (_wikiButton != null)
		{
			UIContextualTutorialModule.OnEvent("Codex", _wikiButton);
		}
	}
}
