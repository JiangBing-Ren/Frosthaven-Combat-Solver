using FHGUI.Scenario.Services;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Scenario.Elements;

public class UIReactionAbilityNameInfoElement : MonoBehaviour
{
	public struct Params
	{
		public CPlayerActor PlayerActor;

		public CAbility Ability;

		public CBaseCard Card;
	}

	[Header("Name")]
	[SerializeField]
	private TMP_Text _cardName;

	[SerializeField]
	private UIHelpBoxTooltipTarget _tooltipTarget;

	private Params _params;

	public void Init(Params @params)
	{
		_params = @params;
		_tooltipTarget?.Initialize(delegate(out CAbility ability, out CBaseCard reference)
		{
			ability = _params.Ability;
			reference = _params.Card;
		});
		string cardNameKey = ReactionViewService.GetCardNameKey(_params.Ability, _params.Card);
		string text = LocalizationManager.GetTranslation(cardNameKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: false, useDefaultIfMissing: false, returnNullIfNotFound: true);
		CCharacterClass cCharacterClass = null;
		CBaseCard card = _params.Card;
		CAbilityCard abilityCard = card as CAbilityCard;
		if (abilityCard != null)
		{
			cCharacterClass = CharacterClassManager.Classes.Find((CCharacterClass s) => s.CharacterID == abilityCard.ClassID);
		}
		else if (_params.Ability?.TargetingActor != null)
		{
			cCharacterClass = _params.Ability.TargetingActor.CharacterClass;
		}
		else if (GameState.s_ShortRestingPlayer != null && cardNameKey == GameState.s_ShortRestingPlayer.ActorLocKey())
		{
			cCharacterClass = GameState.s_ShortRestingPlayer.CharacterClass;
		}
		if (cCharacterClass != null)
		{
			ECharacter characterModel = cCharacterClass.CharacterModel;
			text = "<color=#" + UIInfoTools.Instance.GetCharacterColor(characterModel).ToHex() + ">" + text + "</color>";
		}
		_cardName.text = text;
	}
}
