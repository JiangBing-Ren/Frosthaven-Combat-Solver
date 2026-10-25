using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[DebuggerDisplay("{Name}")]
public class CAbilityCard : CBaseAbilityCard
{
	private CAction m_TopAction;

	private CAction m_DefaultAttackAction;

	private CAction m_DefaultMoveAction;

	private CAction m_BottomAction;

	private CAction m_SelectedAction;

	private CAction m_LastSelectedAction;

	private AbilityCardYMLData m_AbilityCardYML;

	public CAction TopAction => m_TopAction;

	public CAction DefaultAttackAction => m_DefaultAttackAction;

	public CAction DefaultMoveAction => m_DefaultMoveAction;

	public CAction BottomAction => m_BottomAction;

	public CAction SelectedAction => m_SelectedAction;

	public CAction LastSelectedAction => m_LastSelectedAction;

	public AbilityCardYMLData GetAbilityCardYML => m_AbilityCardYML;

	public int Level { get; private set; }

	public bool SupplyCard { get; private set; }

	public int CardInstanceID { get; private set; }

	public ActionType LastPlayedActionType { get; set; }

	public DiscardType GetDiscardType(CAbility ability)
	{
		if (ability.GetAction() == TopAction)
		{
			return GetAbilityCardYML.TopDiscardType;
		}
		if (ability.GetAction() == BottomAction)
		{
			return GetAbilityCardYML.BottomDiscardType;
		}
		return DiscardType.None;
	}

	public IEnumerable<CAction> GetActions()
	{
		if (TopAction != null)
		{
			yield return TopAction;
		}
		if (BottomAction != null)
		{
			yield return BottomAction;
		}
	}

	public IEnumerable<CAction> GetAllActions()
	{
		if (TopAction != null)
		{
			yield return TopAction;
		}
		if (BottomAction != null)
		{
			yield return BottomAction;
		}
		if (DefaultAttackAction != null)
		{
			yield return DefaultAttackAction;
		}
		if (DefaultMoveAction != null)
		{
			yield return DefaultMoveAction;
		}
	}

	public CAction GetAction(ActionType type)
	{
		return type switch
		{
			ActionType.TopAction => TopAction, 
			ActionType.BottomAction => BottomAction, 
			ActionType.DefaultAttackAction => DefaultAttackAction, 
			ActionType.DefaultMoveAction => DefaultMoveAction, 
			_ => null, 
		};
	}

	public ActionType GetActionType(CAction action)
	{
		if (action != null)
		{
			Guid iD = action.ID;
			Guid? guid = TopAction?.ID;
			if (iD == guid)
			{
				return ActionType.TopAction;
			}
			iD = action.ID;
			Guid? guid2 = BottomAction?.ID;
			if (iD == guid2)
			{
				return ActionType.BottomAction;
			}
			iD = action.ID;
			Guid? guid3 = DefaultAttackAction?.ID;
			if (iD == guid3)
			{
				return ActionType.DefaultAttackAction;
			}
			iD = action.ID;
			Guid? guid4 = DefaultMoveAction?.ID;
			if (iD == guid4)
			{
				return ActionType.DefaultMoveAction;
			}
		}
		return ActionType.NA;
	}

	public CAbilityCard(int initiative, int id, CAction defaultMoveAction, CAction defaultAttackAction, CAction topAction, CAction bottomAction, AbilityCardYMLData abilityCardYML, string classID, int? cardInstanceID = null)
		: base(initiative, id, classID, ECardType.CharacterAbility, id.ToString())
	{
		CAbilityCard cAbilityCard = this;
		m_DefaultMoveAction = defaultMoveAction;
		m_DefaultAttackAction = defaultAttackAction;
		m_TopAction = topAction;
		m_BottomAction = bottomAction;
		m_AbilityCardYML = abilityCardYML;
		if (int.TryParse(m_AbilityCardYML.Level, out var result))
		{
			Level = result;
		}
		else
		{
			Level = 1;
		}
		SupplyCard = m_AbilityCardYML.SupplyCard.Value;
		if (cardInstanceID.HasValue)
		{
			CardInstanceID = cardInstanceID.Value;
			return;
		}
		bool flag = false;
		int checkID = 0;
		int num = 0;
		while (!flag)
		{
			checkID = ID + num * 10000;
			num++;
			if (!CharacterClassManager.AllAbilityCardInstances.Any((CAbilityCard w) => w.ID == cAbilityCard.ID && w.CardInstanceID == checkID))
			{
				flag = true;
			}
		}
		CardInstanceID = checkID;
	}

	public CAbilityCard Copy(int? cardInstanceID = null)
	{
		return new CAbilityCard(Initiative, ID, DefaultMoveAction.Copy(), DefaultAttackAction.Copy(), TopAction.Copy(), BottomAction.Copy(), GetAbilityCardYML, ClassID, cardInstanceID);
	}

	public void SetSelectedAction(CAction action)
	{
		m_SelectedAction = action;
		if (action != null)
		{
			m_LastSelectedAction = m_SelectedAction;
		}
	}

	public override ActionType GetAbilityActionType(CAbility ability)
	{
		if (GetTopActionAbilities().Any((CAbility x) => x.ID == ability.ID))
		{
			return ActionType.TopAction;
		}
		if (GetBottomActionAbilities().Any((CAbility x) => x.ID == ability.ID))
		{
			return ActionType.BottomAction;
		}
		return ActionType.NA;
	}

	public CAction GetActionForType(ActionType type)
	{
		return type switch
		{
			ActionType.TopAction => m_TopAction, 
			ActionType.DefaultAttackAction => m_DefaultAttackAction, 
			ActionType.DefaultMoveAction => m_DefaultMoveAction, 
			ActionType.BottomAction => m_BottomAction, 
			_ => null, 
		};
	}

	public override void Reset()
	{
		base.Reset();
		TopAction.Reset();
		BottomAction.Reset();
		m_SelectedAction = null;
		m_LastSelectedAction = null;
		LastPlayedActionType = ActionType.NA;
	}

	public void ResetAbilitiesAndEnhancementsOnly()
	{
		TopAction.Reset();
		BottomAction.Reset();
	}

	public bool HasAbility(CAbility ability)
	{
		if (GetAllAbilities().Any((CAbility x) => x.HasID(ability.ID)))
		{
			return true;
		}
		if (TopAction.ModeAbilities.Any((CAbility x) => x.Ability.HasID(ability.ID)))
		{
			return true;
		}
		if (BottomAction.ModeAbilities.Any((CAbility x) => x.Ability.HasID(ability.ID)))
		{
			return true;
		}
		if (TopAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))) || TopAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.AbilityOverride != null && y.AbilityOverride.SubAbilities != null && y.AbilityOverride.SubAbilities.Any((CAbility z) => z.HasID(ability.ID)))) || BottomAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))) || BottomAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.AbilityOverride != null && y.AbilityOverride.SubAbilities != null && y.AbilityOverride.SubAbilities.Any((CAbility z) => z.HasID(ability.ID)))))
		{
			return true;
		}
		return false;
	}

	public bool HasAbilityOfType(CAbility.EAbilityType abilityType)
	{
		return GetAllAbilities().Any((CAbility a) => a.AbilityType == abilityType);
	}

	public IEnumerable<CAbility> GetAllAbilities()
	{
		return GetAllAbilities(this);
	}

	public IEnumerable<CAbility> GetTopActionAbilities()
	{
		return GetActionAbilities(TopAction);
	}

	public IEnumerable<CAbility> GetBottomActionAbilities()
	{
		return GetActionAbilities(BottomAction);
	}

	public CAction GetAction(CAbility ability)
	{
		if (GetTopActionAbilities().Any((CAbility a) => a.Name == ability.Name))
		{
			return TopAction;
		}
		if (GetBottomActionAbilities().Any((CAbility a) => a.Name == ability.Name))
		{
			return BottomAction;
		}
		return null;
	}

	public IEnumerable<CAbility> GetActionAbilities(CAbility ability)
	{
		return GetActionAbilities(GetAction(ability));
	}

	public IEnumerable<CEnhancement> GetAllEnhancements()
	{
		return GetAllAbilities().SelectMany((CAbility ability) => ability.AbilityEnhancements);
	}

	public IEnumerable<CEnhancement> GetTopEnhancements()
	{
		return GetTopActionAbilities().SelectMany((CAbility ability) => ability.AbilityEnhancements);
	}

	public IEnumerable<CEnhancement> GetBottomEnhancements()
	{
		return GetBottomActionAbilities().SelectMany((CAbility ability) => ability.AbilityEnhancements);
	}

	public CAbility FindAbilityOnCard(string name)
	{
		return GetAllAbilities(this).FirstOrDefault((CAbility a) => a.Name == name);
	}

	public CAbility FindAbilityOnCard(Guid id)
	{
		return GetAllAbilities(this).FirstOrDefault((CAbility a) => a.ID == id);
	}

	public bool CanBeRemovedFromActiveBonuses(bool checkMandatory = true)
	{
		if (HasModeActive())
		{
			return false;
		}
		if (checkMandatory && GameState.CurrentOutOfPhaseCardSelectionAbility == null && HasMandatoryBonus())
		{
			return false;
		}
		if (!HasRemoveableBonuses())
		{
			return false;
		}
		return true;
	}

	public bool HasMandatoryBonus()
	{
		if (SelectedAction == null)
		{
			return false;
		}
		if (SelectedAction.ID == TopAction.ID && TopAction.HasMandatoryBonus(this))
		{
			return true;
		}
		if (SelectedAction.ID == BottomAction.ID && BottomAction.HasMandatoryBonus(this))
		{
			return true;
		}
		return false;
	}

	public bool HasRemoveableBonuses()
	{
		if (ActiveBonuses.Any((CActiveBonus bonus) => bonus.Duration == CActiveBonus.EActiveBonusDurationType.Persistent || bonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon))
		{
			return true;
		}
		return false;
	}

	public bool HasAvailableMode()
	{
		return ModeAbilities().Any();
	}

	public IEnumerable<CAbility> ModeAbilities()
	{
		foreach (CAbility modeAbility in TopAction.ModeAbilities)
		{
			yield return modeAbility;
		}
		foreach (CAbility modeAbility2 in BottomAction.ModeAbilities)
		{
			yield return modeAbility2;
		}
	}

	public bool HasModeActive()
	{
		if (SelectedAction.ID == TopAction.ID && TopAction.IsModeActive(this))
		{
			return true;
		}
		if (SelectedAction.ID == BottomAction.ID && BottomAction.IsModeActive(this))
		{
			return true;
		}
		return false;
	}

	public static IEnumerable<CAbility> GetAllAbilities(AbilityCardYMLData ymlData)
	{
		if (ymlData == null)
		{
			yield break;
		}
		if (ymlData.TopActionCardData != null)
		{
			foreach (CAbility actionAbility in GetActionAbilities(ymlData.TopActionCardData))
			{
				yield return actionAbility;
			}
		}
		if (ymlData.BottomActionCardData == null)
		{
			yield break;
		}
		foreach (CAbility actionAbility2 in GetActionAbilities(ymlData.BottomActionCardData))
		{
			yield return actionAbility2;
		}
	}

	public static IEnumerable<CAbility> GetAllAbilities(CAbilityCard card)
	{
		foreach (CAbility actionAbility in GetActionAbilities(card.TopAction))
		{
			yield return actionAbility;
		}
		foreach (CAbility actionAbility2 in GetActionAbilities(card.BottomAction))
		{
			yield return actionAbility2;
		}
	}

	public static IEnumerable<CAbility> GetActionAbilities(CAction action)
	{
		foreach (CAbility withChildAbility in GetWithChildAbilities(action.Abilities))
		{
			yield return withChildAbility;
		}
		foreach (CAbility withChildAbility2 in GetWithChildAbilities(action.ModeAbilities))
		{
			yield return withChildAbility2;
		}
		if (action.Augmentations == null)
		{
			yield break;
		}
		foreach (CAbility item in action.Augmentations.SelectMany(GetAugmentationAbilities))
		{
			yield return item;
		}
	}

	public static CAbility FindActionAbilityOnCard(CAction action, string abilityName)
	{
		return GetActionAbilities(action).FirstOrDefault((CAbility a) => a.Name == abilityName);
	}

	public static IEnumerable<CAbility> GetAugmentationAbilities(CActionAugmentation sourceAugmentation)
	{
		List<CActionAugmentationOp> list = sourceAugmentation?.AugmentationOps;
		if (list != null)
		{
			return list.SelectMany((CActionAugmentationOp augOp) => GetWithChildAbilities(augOp.Ability).Concat(GetOverrideWithChildAbilities(augOp.AbilityOverride)));
		}
		return Enumerable.Empty<CAbility>();
	}

	public static IEnumerable<CAbilityOverride> GetAbilityOverrides(CAction action)
	{
		if (action.Augmentations == null)
		{
			return Enumerable.Empty<CAbilityOverride>();
		}
		return action.Augmentations.SelectMany((CActionAugmentation aug) => aug.AugmentationOps.Select((CActionAugmentationOp op) => op.AbilityOverride));
	}

	public static IEnumerable<CAbility> GetOverrideWithChildAbilities(CAbilityOverride sourceAbilityOverride)
	{
		if (sourceAbilityOverride != null)
		{
			return GetWithChildAbilities(sourceAbilityOverride?.SubAbilities);
		}
		return Enumerable.Empty<CAbility>();
	}

	public static IEnumerable<CAbility> GetWithChildAbilities(IEnumerable<CAbility> sourceAbilities)
	{
		if (sourceAbilities != null)
		{
			return sourceAbilities.SelectMany((CAbility x) => GetWithChildAbilities(x)).Distinct();
		}
		return Enumerable.Empty<CAbility>();
	}

	public static IEnumerable<CAbility> GetWithChildAbilities(CAbility sourceAbility, bool source = true, bool subAbilities = true, bool augments = true, bool abilityData = true, bool consumes = true, bool controlActor = true, bool merged = true, bool doom = true, bool performFromShadow = true, bool chooseAbility = true, bool addActiveBonus = true, bool conditionalOverrides = true)
	{
		if (sourceAbility == null)
		{
			yield break;
		}
		if (source)
		{
			yield return sourceAbility;
		}
		if (subAbilities)
		{
			foreach (CAbility withChildAbility in GetWithChildAbilities(sourceAbility.SubAbilities))
			{
				yield return withChildAbility;
			}
		}
		if (conditionalOverrides)
		{
			foreach (CConditionalOverride conditionalOverride in sourceAbility.ConditionalOverrides)
			{
				if (conditionalOverride.AbilityOverrides == null)
				{
					continue;
				}
				foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
				{
					foreach (CAbility overrideWithChildAbility in GetOverrideWithChildAbilities(abilityOverride))
					{
						yield return overrideWithChildAbility;
					}
				}
			}
		}
		if (augments)
		{
			CAugment augment = sourceAbility.Augment;
			if (augment != null && augment.Abilities != null)
			{
				foreach (CAbility withChildAbility2 in GetWithChildAbilities(sourceAbility.Augment.Abilities))
				{
					yield return withChildAbility2;
				}
			}
		}
		if (abilityData)
		{
			AbilityData.ActiveBonusData activeBonusData = sourceAbility.ActiveBonusData;
			if (activeBonusData != null && activeBonusData.AbilityData != null)
			{
				foreach (CAbility withChildAbility3 in GetWithChildAbilities(sourceAbility.ActiveBonusData.AbilityData))
				{
					yield return withChildAbility3;
				}
			}
		}
		if (consumes && sourceAbility is CAbilityConsume { Consumes: var consumes2 })
		{
			foreach (CAbility item in consumes2.SelectMany((AbilityConsume consume) => GetAugmentationAbilities(consume.ConsumeData)))
			{
				yield return item;
			}
		}
		else if (controlActor && sourceAbility is CAbilityControlActor cAbilityControlActor)
		{
			foreach (CAbility withChildAbility4 in GetWithChildAbilities(cAbilityControlActor.ControlActorData.ControlAbilities))
			{
				yield return withChildAbility4;
			}
		}
		else if (merged && sourceAbility is CAbilityMerged cAbilityMerged)
		{
			foreach (CAbility withChildAbility5 in GetWithChildAbilities(cAbilityMerged.MergedAbilities))
			{
				yield return withChildAbility5;
			}
		}
		else if (doom && sourceAbility is CAbilityAddDoom cAbilityAddDoom)
		{
			foreach (CAbility withChildAbility6 in GetWithChildAbilities(cAbilityAddDoom.Doom.DoomAbilities))
			{
				yield return withChildAbility6;
			}
		}
		else if (performFromShadow && sourceAbility is CAbilityPerformFromShadow cAbilityPerformFromShadow)
		{
			foreach (CAbility withChildAbility7 in GetWithChildAbilities(cAbilityPerformFromShadow.Abilities))
			{
				yield return withChildAbility7;
			}
		}
		else if (chooseAbility && sourceAbility is CAbilityChoose cAbilityChoose)
		{
			foreach (CAbility withChildAbility8 in GetWithChildAbilities(cAbilityChoose.ChooseAbilities))
			{
				yield return withChildAbility8;
			}
		}
		else
		{
			if (!addActiveBonus || !(sourceAbility is CAbilityAddActiveBonus cAbilityAddActiveBonus))
			{
				yield break;
			}
			foreach (CAbility withChildAbility9 in GetWithChildAbilities(cAbilityAddActiveBonus.AddAbility))
			{
				yield return withChildAbility9;
			}
		}
	}
}
