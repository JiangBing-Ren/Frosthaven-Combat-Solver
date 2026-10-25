using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InitiativeTrackActorAvatarBase))]
public abstract class InitiativeTrackActorAvatarExtension : MonoBehaviour
{
	[SerializeField]
	protected Image m_FrameImage;

	[SerializeField]
	protected Image m_InnerFrameImage;

	[field: SerializeField]
	public InitiativeTrackActorAvatarBase Base { get; private set; }

	public bool IsSelectableByClick { get; protected set; } = true;

	public virtual string GetInitiativeText(CActor actor)
	{
		if (actor != null && actor.Initiative() != 0)
		{
			return actor.Initiative().ToString();
		}
		return "-";
	}

	public virtual string GetInitiativeText(CMonsterClass mClass, CEnemyActor actor)
	{
		if (actor != null && mClass.Initiative(actor, includeOverrideCard: false) != 0)
		{
			return mClass.Initiative(actor, includeOverrideCard: false).ToString();
		}
		return "-";
	}

	public abstract InitiativeTrack.ActorPhaseState GetPhaseState(CActor actor);

	public virtual void PreSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		IsSelectableByClick = aaParams.IsActiveButton;
	}

	public abstract void PostSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams);

	public abstract void OnClick(CActor actor, bool isSelected);

	public abstract void OnSelect(CActor actor);
}
