using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.DTO;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Common;

public struct HeroSummonGeneralDTO
{
	public string Name;

	public Sprite Icon;

	public string Description;

	public string ActorGuid;

	public int? SourceCard;

	public CPlayerActor Owner;

	public BaseStatsDTO Stats;

	public StatusEffectDTO[] StatusEffects;

	public IEnumerable<StatusEffectDTO> PositiveEffects => StatusEffects.Where((StatusEffectDTO effect) => effect.EffectType == StatusEffectDTO.StatusEffectType.Positive);

	public IEnumerable<StatusEffectDTO> NegativeEffects => StatusEffects.Where((StatusEffectDTO effect) => effect.EffectType == StatusEffectDTO.StatusEffectType.Negative);

	public IEnumerable<StatusEffectDTO> PermanentEffects => StatusEffects.Where((StatusEffectDTO effect) => effect.EffectType == StatusEffectDTO.StatusEffectType.Permanent);

	public HeroSummonGeneralDTO(CHeroSummonActor actor, bool getStatuses = true)
		: this(actor.SummonData, actor.Level)
	{
		ActorGuid = actor.ActorGuid;
		SourceCard = actor.FindSummonCard()?.ID;
		Stats = new BaseStatsDTO(actor);
		StatusEffects = (getStatuses ? UICharacterUtils.GetAllStatusEffects(actor).ToArray() : null);
		Owner = ((actor.Summoner != null) ? actor.Summoner : null);
	}

	public HeroSummonGeneralDTO(HeroSummonYMLData summonYmlData, int level = 0)
	{
		Name = LocalizationManager.GetTranslation(summonYmlData.LocKey);
		Description = null;
		Icon = UIInfoTools.GetSummonPortrait(summonYmlData.ID, summonYmlData.Model, out var _);
		Stats = new BaseStatsDTO(summonYmlData, level);
		StatusEffects = null;
		ActorGuid = null;
		SourceCard = null;
		Owner = null;
	}
}
