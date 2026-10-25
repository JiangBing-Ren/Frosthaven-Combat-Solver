using System;
using AStar;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIEventManager : MonoBehaviour
{
	public static event Action<UIEvent> OnEventAdded;

	public static void AddTileSelectEvent(Point tilePoint, CInteractable.EInputType inputType)
	{
		if (PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
		{
			AddUIEvent(new UIEvent(UIEvent.EUIEventType.TileSelectedForAbility, null, tile: new TileIndex(tilePoint), contextId: cPhaseAction.CurrentPhaseAbility.m_BaseCard?.Name, contextInt: 0, contextActorGUID: null, inputType: inputType));
		}
	}

	public static void AddActorEndedTurnEvent(CActor actorEndedTurn)
	{
		AddUIEvent(new UIEvent(UIEvent.EUIEventType.ActorEndedTurnOnTile, tile: new TileIndex(actorEndedTurn.ArrayIndex), contextId: actorEndedTurn.ActorGuid, actorPrefabName: actorEndedTurn.GetPrefabName()));
	}

	public static void AddUIEvent(UIEvent loggedEvent)
	{
		if (!ProceduralMapTile.IsInLevelEditorMode)
		{
			OnEventAdded?.Invoke(loggedEvent);
		}
	}
}
