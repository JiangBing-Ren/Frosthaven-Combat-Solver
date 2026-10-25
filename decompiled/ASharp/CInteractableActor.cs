using AStar;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;

public class CInteractableActor : CInteractable, IGeneralInfoProvider
{
	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[SerializeField]
	private bool triggerMouseOver;

	private CActor m_Actor;

	public void Awake()
	{
	}

	private void Start()
	{
		if (SaveData.Instance?.Global == null || SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
		{
			Object.Destroy(this);
		}
	}

	public void Init(CharacterManager characterManager)
	{
		m_Actor = characterManager.CharacterActor;
	}

	public override void ShowNormalInterface(bool disabled, EInputType inputType)
	{
		if (m_Actor != null && TileBehaviour.s_Callback != null)
		{
			Point arrayIndex = m_Actor.ArrayIndex;
			CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[arrayIndex.X, arrayIndex.Y];
			TileBehaviour.s_Callback(clientTile, null, networkActionIfOnline: true, isUserClick: true, SaveData.Instance.Global.EnableSecondClickHexToConfirm, inputType);
		}
	}

	public override void OnCursorEnter()
	{
		if (m_Actor != null && triggerMouseOver && !m_Actor.IsDead && !m_Actor.Deactivated && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			_actorEventsChannel.ActorHighlighted(this, m_Actor, highlighted: true, showCards: false);
		}
	}

	public override void OnCursorExit()
	{
		if (m_Actor != null && triggerMouseOver && !m_Actor.IsDead && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			_actorEventsChannel.ActorHighlighted(this, m_Actor, highlighted: false, showCards: false);
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		return UIGeneralInfoUtils.GetActor(m_Actor);
	}
}
