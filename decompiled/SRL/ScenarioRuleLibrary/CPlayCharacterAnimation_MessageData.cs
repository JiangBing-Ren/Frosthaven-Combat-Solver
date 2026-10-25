using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayCharacterAnimation_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CTile m_TileToLookAt;

	public List<CTile> m_TargetedTilesForVFX;

	public List<CTile> m_AreaTilesForVFX;

	public bool m_IsFromItem;

	public bool m_BlockMessageProcessingDuringAnimation;

	public CPlayCharacterAnimation_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PlayCharacterAnimation, actorSpawningMessage, animOverload)
	{
	}
}
