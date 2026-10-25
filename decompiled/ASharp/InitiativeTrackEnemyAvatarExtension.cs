using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InitiativeTrackEnemyAvatarExtension : InitiativeTrackActorAvatarExtension
{
	[Serializable]
	private struct FactionSettings
	{
		public CActor.EType Type;

		public Sprite FactionMarkerSprite;

		public Sprite FrameSprite;

		public Sprite InnerFrameSprite;
	}

	[Header("Enemy Avatar Extension")]
	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private Sprite m_StandardBackgroundSprite;

	[SerializeField]
	private Sprite m_EliteBackgroundSprite;

	[SerializeField]
	private Sprite m_AllyBackgroundSprite;

	[SerializeField]
	private Sprite m_BossBackgroundSprite;

	[SerializeField]
	private Sprite m_BossInnerFrameSprite;

	[SerializeField]
	private Image m_BossIcon;

	[SerializeField]
	private Image m_EliteIcon;

	[FormerlySerializedAs("AlwaysDisplayFaction")]
	[Space]
	public bool AlwaysDisplayFactionMarker;

	[SerializeField]
	private Image m_FactionImage;

	[SerializeField]
	private List<FactionSettings> m_FactionsSettings = new List<FactionSettings>();

	public override InitiativeTrack.ActorPhaseState GetPhaseState(CActor actor)
	{
		return InitiativeTrack.ActorPhaseState.None;
	}

	public override void PostSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		CActor.EType actorType = ((aaParams.Actor == null) ? CActor.EType.Enemy : aaParams.Actor.Type);
		if (actorType == CActor.EType.Player)
		{
			actorType = CActor.EType.Ally;
		}
		FactionSettings factionSettings = m_FactionsSettings.FirstOrDefault((FactionSettings fs) => fs.Type == actorType);
		bool flag = aaParams.IsFactionDisplayed || AlwaysDisplayFactionMarker;
		m_FactionImage.sprite = factionSettings.FactionMarkerSprite;
		m_FactionImage.enabled = flag && m_FactionImage.sprite != null;
		m_FrameImage.sprite = factionSettings.FrameSprite;
		m_InnerFrameImage.sprite = factionSettings.InnerFrameSprite;
		CActor actor = aaParams.Actor;
		bool flag2 = (actor != null && actor.IsBoss) || (aaParams.Actor?.MonsterClass.MonsterYML.MiniBoss ?? false);
		if (flag2)
		{
			m_BackgroundImage.sprite = m_BossBackgroundSprite;
			m_InnerFrameImage.sprite = m_BossInnerFrameSprite;
		}
		else if (aaParams.Actor != null && aaParams.Actor.Type == CActor.EType.Ally)
		{
			m_BackgroundImage.sprite = m_AllyBackgroundSprite;
		}
		else if (aaParams.Class.ID.Contains("Elite", StringComparison.InvariantCultureIgnoreCase))
		{
			m_BackgroundImage.sprite = m_EliteBackgroundSprite;
		}
		else
		{
			m_BackgroundImage.sprite = m_StandardBackgroundSprite;
		}
		m_BossIcon.enabled = flag2;
		m_EliteIcon.enabled = aaParams.Actor?.IsElite ?? false;
	}

	public override void OnClick(CActor actor, bool isSelected)
	{
	}

	public override void OnSelect(CActor actor)
	{
	}
}
