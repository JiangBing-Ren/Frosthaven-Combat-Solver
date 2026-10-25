using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Glossary;
using ScenarioRuleLibrary;
using UnityEngine;

public class EnhancementButtonBase : MonoBehaviour, IGlossaryProvider
{
	[ReadOnlyField]
	public EEnhancement EnhancementType;

	[ReadOnlyField]
	public EEnhancementLine EnhancementLine;

	[ReadOnlyField]
	public EEnhancementMarkType MarkType;

	[ReadOnlyField]
	public string AbilityName;

	[ReadOnlyField]
	public int EnhancementSlot;

	public int AbilityCardID;

	public string AbilityNameLookupText;

	public RectTransform ParentContainer;

	private FullAbilityCardAction _action;

	public FullAbilityCardAction Action
	{
		get
		{
			if (_action == null)
			{
				_action = GetComponentInParent<FullAbilityCardAction>(includeInactive: true);
			}
			return _action;
		}
	}

	public CAbilityCard AbilityCard
	{
		get
		{
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				CAbilityCard cAbilityCard = @class.FindCardWithID(AbilityCardID);
				if (cAbilityCard != null)
				{
					return cAbilityCard;
				}
			}
			return null;
		}
	}

	public CAbility Ability => CAbilityCard.FindActionAbilityOnCard(Action.IsTop ? AbilityCard.TopAction : AbilityCard.BottomAction, AbilityName);

	public CEnhancement Enhancement
	{
		get
		{
			CAbility ability = Ability;
			if (ability == null)
			{
				Log.GUIError($"Ability [{AbilityName}] not found for enhancement on ability card with id [{AbilityCardID}]", this);
				return null;
			}
			CEnhancement cEnhancement = ability.AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == EnhancementLine && s.EnhancementSlot == EnhancementSlot);
			if (cEnhancement == null)
			{
				string text = ability.AbilityEnhancements.Aggregate("Ability enhancements:", (string current, CEnhancement enhancement) => current + $"\n{enhancement}");
				Log.GUIError(string.Format("No suitable {0} for [{1}] (of type {2}) not found on ability card with id [{3}]\n", "CEnhancement", AbilityName, ability.AbilityType, AbilityCardID) + string.Format("UI Expected: {0}=[{1}] | {2}=[{3}]\n", "EnhancementLine", EnhancementLine, "EnhancementSlot", EnhancementSlot) + text, this);
			}
			return cEnhancement;
		}
	}

	public GameObject GameObject => gameObject;

	public virtual void Init(CEnhancement enhancement, string abilityNameLookupText, RectTransform parentContainer)
	{
		EnhancementType = enhancement.Enhancement;
		EnhancementLine = enhancement.EnhancementLine;
		MarkType = enhancement.MarkType;
		AbilityCardID = enhancement.AbilityCardID;
		AbilityName = enhancement.AbilityName;
		EnhancementSlot = enhancement.EnhancementSlot;
		AbilityNameLookupText = abilityNameLookupText;
		ParentContainer = parentContainer;
	}

	public bool HasEnhancement()
	{
		return Ability?.AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == EnhancementLine && s.EnhancementSlot == EnhancementSlot) != null;
	}

	public IEnumerable<UIGlossaryData> GetGlossary()
	{
		if (AbilityCard == null || Ability == null || !HasEnhancement())
		{
			return UIInfoTools.GetFullGlossaryDataFromKey("NoEnhancement");
		}
		return Enhancement.Enhancement switch
		{
			EEnhancement.NoEnhancement => UIInfoTools.GetFullGlossaryDataFromKey("NoEnhancement"), 
			EEnhancement.Poison => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Poison"), 
			EEnhancement.Wound => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Wound"), 
			EEnhancement.Muddle => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Muddle"), 
			EEnhancement.Immobilize => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Immobilize"), 
			EEnhancement.Disarm => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Disarm"), 
			EEnhancement.Curse => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Curse"), 
			EEnhancement.Strengthen => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Strengthen"), 
			EEnhancement.Bless => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Bless"), 
			EEnhancement.Jump => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Jump"), 
			EEnhancement.Fire => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Fire"), 
			EEnhancement.Ice => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Ice"), 
			EEnhancement.Air => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Air"), 
			EEnhancement.Earth => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Earth"), 
			EEnhancement.Light => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Light"), 
			EEnhancement.Dark => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Dark"), 
			EEnhancement.AnyElement => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_AnyElement"), 
			EEnhancement.Area => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Area"), 
			EEnhancement.SummonMove => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_SummonMove"), 
			EEnhancement.SummonAttack => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_SummonAttack"), 
			EEnhancement.SummonRange => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_SummonRange"), 
			EEnhancement.SummonHP => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_SummonHP"), 
			EEnhancement.PlusMove => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusMove"), 
			EEnhancement.PlusAttack => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusAttack"), 
			EEnhancement.PlusRange => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusRange"), 
			EEnhancement.PlusShield => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusShield"), 
			EEnhancement.PlusPush => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusPush"), 
			EEnhancement.PlusPull => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusPull"), 
			EEnhancement.PlusPierce => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusPierce"), 
			EEnhancement.PlusRetaliate => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusRetaliate"), 
			EEnhancement.PlusHeal => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusHeal"), 
			EEnhancement.PlusTarget => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusTarget"), 
			EEnhancement.PlusRetaliateRange => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusRetaliateRange"), 
			EEnhancement.PlusDamage => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusDamage"), 
			EEnhancement.Ward => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Ward"), 
			EEnhancement.Regenerate => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_Regenerate"), 
			EEnhancement.PlusTeleport => UIInfoTools.GetFullGlossaryDataFromKey("Enhancement_PlusTeleport"), 
			_ => Enumerable.Empty<UIGlossaryData>(), 
		};
	}
}
