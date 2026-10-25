using System.Collections.Generic;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionMultiplayer : MainOption
{
	[SerializeField]
	private MenuOptionIcon joinIcon;

	[SerializeField]
	private MenuOptionIcon hostIcon;

	[SerializeField]
	private MenuOptionIcon backIcon;

	[SerializeField]
	private UIMultiplayerJoinSessionWindow joinSessionWindow;

	[SerializeField]
	private UIMultiplayerHowToHostWindow hostToHostWindow;

	[SerializeField]
	private UIMainMenuSubptionsPanel suboptionsPanel;

	private MenuSuboption joinSession;

	private MenuSuboption howToHost;

	private MenuSuboption backOption;

	protected override void Awake()
	{
		base.Awake();
		joinSessionWindow.OnStartCancel.AddListener(DisableInteraction);
		joinSessionWindow.OnFinishCancel.AddListener(EnableInteraction);
		joinSessionWindow.OnStartJoin.AddListener(DisableInteraction);
		joinSessionWindow.OnFailedJoin.AddListener(EnableInteraction);
		joinSession = new MenuSuboption("GUI_MULTIPLAYER_JOIN", joinIcon, joinSessionWindow.Show, joinSessionWindow.Hide);
		joinSession.IsInteractable = PlatformLayer.UserData.IsSignedIn;
		howToHost = new MenuSuboption("GUI_MAIN_MENU_MULTIPLAYER_HOW_TO_HOST", hostIcon, hostToHostWindow.Show, hostToHostWindow.Hide);
		backOption = new MenuSuboption("GUI_BACK", backIcon, Deselect);
		hostToHostWindow.OnHidden.AddListener(howToHost.Deselect);
		joinSessionWindow.OnHidden.AddListener(joinSession.Deselect);
	}

	private void DisableInteraction()
	{
		suboptionsPanel.SetInteractable(interactable: false);
	}

	private void EnableInteraction()
	{
		suboptionsPanel.SetInteractable(interactable: true);
	}

	public override void Select()
	{
		base.Select();
		Button.SetSelected(isSelected: true);
		joinSession.Reset();
		howToHost.Reset();
		backOption.Reset();
		suboptionsPanel.Show(new List<MenuSuboption> { joinSession, howToHost, backOption }, Button.transform as RectTransform, Deselect);
	}

	public override void Deselect()
	{
		EnableInteraction();
		base.Deselect();
		suboptionsPanel.Hide();
	}

	public void SelectJoinMultiplayer()
	{
		Select();
		joinSession.Select();
	}
}
