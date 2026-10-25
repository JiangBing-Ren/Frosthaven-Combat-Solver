using ScenarioRuleLibrary;

public class UIEvent
{
	public enum EUIEventType
	{
		AbilityCardSelected,
		ConfirmButtonPressed,
		SkipButtonPressed,
		UndoButtonPressed,
		EndTurnPressed,
		CardTopHalfSelected,
		CardBottomHalfSelected,
		ActorSelected,
		AttackModifiersHovered,
		PlayerChoseDamage,
		PlayerChoseBurnAvailable,
		PlayerChoseBurnDiscarded,
		PlayerRerolledShortRest,
		TileSelectedForAbility,
		ShortRestPressed,
		ShortRestConfirmed,
		ShortRestCancelled,
		ShortRestChoseToBurn,
		ShortRestChoseToRedraw,
		AttackModifierValueRevealed,
		LevelMessageDismissed,
		DamageChoiceDialogAppeared,
		DefaultMoveAbilityPressed,
		DefaultAttackAbilityPressed,
		InitiativeAvatarHovered,
		ConsumeElementCardTop,
		ConsumeElementCardBottom,
		RewardsWindowDismissed,
		ChooseLoseCardConfirmed,
		RoomRevealed,
		ScenarioWon,
		ScenarioLost,
		ActorEndedTurnOnTile,
		ActorDidntEndTurnOnTile,
		CameraRoomButtonPressed,
		ObjectiveCompleted,
		AllActorsDeadFiltered,
		PropActivated,
		PropDestroyed,
		PropDeactivated,
		DoorUnlocked,
		ChooseLoseCardsOptions,
		DamageChoiceDialogDisappeared,
		WindowOpened,
		WindowClosed,
		ObjectiveActivated,
		ElementsChanged,
		ActorConditionsChanged,
		ScenarioModifierHiddenStateUpdated,
		UpdateScenarioModifierDescriptions,
		ScenarioLoaded
	}

	public EUIEventType EventType;

	public int RoundInt;

	public string CurrentPhaseActorName;

	public string ContextID;

	public int ContextInt;

	public TileIndex TileForEvent;

	public CPhase.PhaseType CurrentPhaseType;

	public string ContextActorGUID;

	public CInteractable.EInputType InputType;

	public UIEvent(EUIEventType eventType, string actorPrefabName = null, string contextId = null, int contextInt = 0, TileIndex tile = null, string contextActorGUID = null, CInteractable.EInputType inputType = CInteractable.EInputType.None)
	{
		EventType = eventType;
		RoundInt = ((ScenarioManager.CurrentScenarioState == null) ? 1 : ScenarioManager.CurrentScenarioState.RoundNumber);
		CurrentPhaseActorName = ((actorPrefabName != null) ? actorPrefabName : ((GameState.InternalCurrentActor != null) ? GameState.InternalCurrentActor.GetPrefabName() : ""));
		ContextID = ((contextId == null) ? string.Empty : contextId);
		ContextInt = contextInt;
		CurrentPhaseType = PhaseManager.PhaseType;
		TileForEvent = tile;
		ContextActorGUID = contextActorGUID;
		InputType = inputType;
	}
}
