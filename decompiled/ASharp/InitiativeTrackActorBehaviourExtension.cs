using UnityEngine;

[RequireComponent(typeof(InitiativeTrackActorBehaviourBase))]
public abstract class InitiativeTrackActorBehaviourExtension : MonoBehaviour
{
	[field: SerializeField]
	public InitiativeTrackActorBehaviourBase Base { get; private set; }

	public abstract void OnSelect();

	public abstract void OnDeselect();

	public abstract void OnAvatarHighlight(bool active);

	public abstract void OnSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams);
}
