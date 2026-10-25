using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameHouseRulesStep : UICreateGameStep
{
	[Header("House Rules")]
	[SerializeField]
	private UIHouseRulesSelector m_HouseRulesSelector;

	[Header("DLC")]
	[SerializeField]
	private UIDLCSelector m_DLCSelector;

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		m_DLCSelector.SetValue(DLCRegistry.EDLCKey.None);
		base.Setup(service, gameData, onConfirmed, onCancelled);
		EnableConfirmationButton(enable: true);
	}
}
