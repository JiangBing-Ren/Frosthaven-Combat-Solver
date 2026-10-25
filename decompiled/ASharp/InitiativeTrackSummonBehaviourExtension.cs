using ScenarioRuleLibrary;

public class InitiativeTrackSummonBehaviourExtension : InitiativeTrackActorBehaviourExtension
{
	public CHeroSummonActor SummonActor => (CHeroSummonActor)Base.Actor;

	public override void OnSelect()
	{
	}

	public override void OnDeselect()
	{
	}

	public override void OnAvatarHighlight(bool active)
	{
	}

	public override void OnSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		OnAvatarHighlight(active: false);
	}
}
