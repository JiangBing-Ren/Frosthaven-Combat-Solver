using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CPhaseAction : CPhase
{
	public class CPhaseAbility
	{
		public CAbility m_Ability;

		public CActor TargetingActor;

		public Guid? ActionID;

		public int? ItemID;

		public CBaseCard m_BaseCard;

		public Action m_EndAction;

		public Action m_PostEndAction;

		public bool KillAfterAction;

		public bool IsCostAbility;

		public CPhaseAbility(CAbility ability, CActor targetingActor, CBaseCard card, Guid? actionID, int? item = null, Action endAction = null, bool killAfter = false, bool isCostAbility = false)
		{
			m_Ability = ability.GetQueuedAbility(targetingActor);
			TargetingActor = targetingActor;
			ActionID = actionID;
			ItemID = item;
			m_BaseCard = card;
			m_EndAction = endAction;
			KillAfterAction = killAfter;
			IsCostAbility = isCostAbility;
		}
	}

	public Action m_OnEndPhaseAction;

	public CActor ActorInitiator;

	public List<CActiveBonus> TempLockedBonuses = new List<CActiveBonus>();

	private List<CPhaseAbility> m_PhaseAbilities = new List<CPhaseAbility>();

	private List<CPhaseAbility> m_CurrentPhaseAbilities;

	private List<CPhaseAbility> m_PreviousPhaseAbilities = new List<CPhaseAbility>();

	private CPhaseAbility m_FirstPhaseAbility;

	private int m_ActionXP;

	private int m_ActionConsumeXP;

	private bool m_EndAbilitySynced;

	private List<CPhaseAbility> m_SavedAbilities = new List<CPhaseAbility>();

	private int? m_CurrentItemID;

	private bool m_ApplyActionXP;

	private Guid? m_CurrentActionID;

	private List<ElementInfusionBoardManager.EElement> m_ElementsToInfuse = new List<ElementInfusionBoardManager.EElement>();

	private bool m_DontMoveAbilityState;

	private GameState.CurrentActorAction m_CurrentAction;

	private bool m_HasAnyAbilityHappened;

	private bool m_HasAnyNonItemAbilityHappened;

	private List<CItem> m_ItemsLoggedAsUsedThisPhase = new List<CItem>();

	private List<CActionAugmentation> m_ActionAugmentationsToConsume = new List<CActionAugmentation>();

	private List<CActionAugmentation> m_ActionAugmentationsConsumed = new List<CActionAugmentation>();

	private Stack<CActiveBonus> m_CurrentTriggeredActiveBonuses = new Stack<CActiveBonus>();

	private bool m_ShownEndOfActionActiveBonuses;

	private bool m_CheckedForStartActionSongs;

	private bool m_StartActionNonToggleActivesAdded;

	private bool m_EndActionNonToggleActivesAdded;

	private static Dictionary<CAbility.EAbilityType, int> s_DamageInflictedByAbilityTypeThisAction = new Dictionary<CAbility.EAbilityType, int>();

	private static int s_DamageInflictedThisAction;

	private static int s_DamagePreventedThisAction;

	private static int s_HexesMovedThisAction;

	private static int s_ObstaclesDestroyedThisAction;

	private static int s_TargetsDamagedInPrevAttackThisAction;

	private static List<CActor> s_TargetsListDamagedInPrevAttackThisAction = new List<CActor>();

	private static Guid s_TargetsDamagedInPrevAttackThisActionAbilityID;

	private static int s_TargetsActuallyDamagedInPrevAttackThisAction;

	private static Guid s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID;

	private static int s_TargetsDamagedInPrevDamageAbilityThisAction;

	private static Guid s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID;

	private static List<CActor> s_ActorsHealedThisAction = new List<CActor>();

	public static CPhaseAbility LastPlayedAbility;

	public List<CPhaseAbility> RemainingPhaseAbilities => m_PhaseAbilities;

	public CActiveBonus CurrentTriggeredActiveBonus
	{
		get
		{
			return m_CurrentTriggeredActiveBonus;
		}
		set
		{
			m_CurrentTriggeredActiveBonus = value;
		}
	}

	public List<CActionAugmentation> ActionAugmentationsConsumed => m_ActionAugmentationsConsumed;

	public CPhaseAbility CurrentPhaseAbility
	{
		get
		{
			List<CPhaseAbility> currentPhaseAbilities = CurrentPhaseAbilities;
			if (currentPhaseAbilities == null || currentPhaseAbilities.Count == 0)
			{
				return null;
			}
			return currentPhaseAbilities[currentPhaseAbilities.Count - 1];
		}
	}

	public List<CPhaseAbility> CurrentPhaseAbilities
	{
		get
		{
			if (m_CurrentPhaseAbilities == null || m_CurrentPhaseAbilities.Count <= 0)
			{
				return null;
			}
			return m_CurrentPhaseAbilities;
		}
	}

	public List<CPhaseAbility> PreviousPhaseAbilities => m_PreviousPhaseAbilities;

	public IEnumerable<CPhaseAbility> CurrentAndRemainingPhaseAbilities => m_CurrentPhaseAbilities.Concat(m_PhaseAbilities);

	public IEnumerable<CPhaseAbility> CurrentAndPreviousPhaseAbilities => m_CurrentPhaseAbilities.Concat(m_PreviousPhaseAbilities);

	public List<CItem> ItemsUsedThisPhase => m_ItemsLoggedAsUsedThisPhase;

	public bool IsNextAbilitySharedAnim
	{
		get
		{
			if (m_CurrentPhaseAbilities.Count > 1)
			{
				return m_CurrentPhaseAbilities[1].m_Ability.SkipAnim;
			}
			if (m_PhaseAbilities.Count > 0)
			{
				return m_PhaseAbilities[0].m_Ability.SkipAnim;
			}
			return false;
		}
	}

	public bool HasConsumedElements => m_ActionAugmentationsConsumed.Any((CActionAugmentation x) => x.Elements.Count > 0);

	public List<CActionAugmentation> ActionAugmentationsToConsume => m_ActionAugmentationsToConsume;

	public static int DamagePreventedThisAction => s_DamagePreventedThisAction;

	public static int DamageInflictedThisAction => s_DamageInflictedThisAction;

	public static Dictionary<CAbility.EAbilityType, int> DamageInflictedByAbilityTypeThisAction => s_DamageInflictedByAbilityTypeThisAction;

	public static int HexesMovedThisAction => s_HexesMovedThisAction;

	public static int ObstaclesDestroyedThisAction => s_ObstaclesDestroyedThisAction;

	public static int TargetsDamagedInPrevAttackThisAction => s_TargetsDamagedInPrevAttackThisAction;

	public static List<CActor> TargetsListDamagedInPrevAttackThisAction => s_TargetsListDamagedInPrevAttackThisAction;

	public static int TargetsDamagedInPrevDamageAbilityThisAction => s_TargetsDamagedInPrevDamageAbilityThisAction;

	public static List<CActor> ActorsKilledThisAction
	{
		get
		{
			if (PhaseManager.CurrentPhaseAction == null)
			{
				return new List<CActor>();
			}
			IEnumerable<CPhaseAbility> source = PhaseManager.CurrentPhaseAction.m_PreviousPhaseAbilities.Concat(PhaseManager.CurrentPhaseAction.m_CurrentPhaseAbilities);
			CAction action = source.FirstOrDefault((CPhaseAbility x) => x?.m_Ability?.GetAction() != null)?.m_Ability?.GetAction();
			if (action == null)
			{
				return new List<CActor>();
			}
			return (source.Where((CPhaseAbility x) => x.m_Ability.ActorsTargeted != null && action.Abilities.Any((CAbility y) => y.ID == x.m_Ability.ID))?.SelectMany((CPhaseAbility x) => x.m_Ability.ActorsTargeted).Where((CActor actor) => actor.IsDead))?.ToList() ?? new List<CActor>();
		}
	}

	public static List<CActor> ActorsHealedThisAction => s_ActorsHealedThisAction;

	public CPhaseAbility FirstPhaseAbility => m_FirstPhaseAbility;

	private CActiveBonus m_CurrentTriggeredActiveBonus
	{
		get
		{
			if (m_CurrentTriggeredActiveBonuses.Count == 0)
			{
				return null;
			}
			return m_CurrentTriggeredActiveBonuses.Peek();
		}
		set
		{
			if (value != null)
			{
				if (!m_CurrentTriggeredActiveBonuses.Contains(value))
				{
					m_CurrentTriggeredActiveBonuses.Push(value);
				}
			}
			else if (m_CurrentTriggeredActiveBonuses.Count > 0)
			{
				m_CurrentTriggeredActiveBonuses.Pop();
			}
		}
	}

	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse => m_ElementsToInfuse;

	public CAbility FindAbilityInPhase(CAbility ability)
	{
		if (CurrentPhaseAbilities != null)
		{
			CPhaseAbility cPhaseAbility = CurrentPhaseAbilities.LastOrDefault((CPhaseAbility x) => x.m_Ability.ID == ability.ID);
			if (cPhaseAbility != null)
			{
				return cPhaseAbility.m_Ability;
			}
		}
		if (PreviousPhaseAbilities != null)
		{
			CPhaseAbility cPhaseAbility2 = PreviousPhaseAbilities.LastOrDefault((CPhaseAbility x) => x.m_Ability.ID == ability.ID);
			if (cPhaseAbility2 != null)
			{
				return cPhaseAbility2.m_Ability;
			}
		}
		return null;
	}

	protected override void OnEndPhase()
	{
		foreach (CObjectShadow shadowProp in ScenarioManager.CurrentScenarioState.ShadowProps)
		{
			shadowProp.WillBeRemoved = false;
		}
		base.OnEndPhase();
		if (m_OnEndPhaseAction != null)
		{
			m_OnEndPhaseAction();
		}
	}

	public CPhaseAction()
	{
		m_OnEndPhaseAction = null;
		m_PhaseType = PhaseType.Action;
		m_ActionXP = 0;
		m_ActionConsumeXP = 0;
		s_DamageInflictedByAbilityTypeThisAction.Clear();
		s_DamageInflictedThisAction = 0;
		s_DamagePreventedThisAction = 0;
		s_HexesMovedThisAction = 0;
		s_ObstaclesDestroyedThisAction = 0;
		s_TargetsDamagedInPrevAttackThisAction = 0;
		s_TargetsListDamagedInPrevAttackThisAction = new List<CActor>();
		s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		s_TargetsDamagedInPrevAttackThisActionAbilityID = default;
		s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = default;
		s_ActorsHealedThisAction.Clear();
		m_ActionAugmentationsToConsume = new List<CActionAugmentation>();
		m_ActionAugmentationsConsumed = new List<CActionAugmentation>();
		ActorInitiator = GameState.CurrentAction.ActionInitiator;
		foreach (CAbility ability in GameState.CurrentAction.Action.Abilities)
		{
			if (ability.OnDeath)
			{
				continue;
			}
			m_PhaseAbilities.Add(new CPhaseAbility(ability, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
			if (ability.SubAbilities == null)
			{
				continue;
			}
			foreach (CAbility subAbility in ability.SubAbilities)
			{
				if (!subAbility.IsInlineSubAbility)
				{
					subAbility.ProcessIfDead = ability.ProcessIfDead;
					subAbility.ParentAbility = ability;
					m_PhaseAbilities.Add(new CPhaseAbility(subAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
				}
			}
		}
		m_FirstPhaseAbility = ((m_PhaseAbilities.Count > 0) ? m_PhaseAbilities[0] : null);
		if (GameState.CurrentSelectAndPlayCardAbility == null || !GameState.CurrentSelectAndPlayCardAbility.BlockXP)
		{
			m_ActionXP += GameState.CurrentAction.Action.ActionXP;
		}
		foreach (CPhaseAbility phaseAbility in m_PhaseAbilities)
		{
			if (phaseAbility.m_Ability is CAbilityMerged cAbilityMerged)
			{
				cAbilityMerged.CopyMergedAbilities();
			}
		}
		AddMonsterConsumes();
		if (GameState.CurrentAction.Action.Infusions != null)
		{
			foreach (ElementInfusionBoardManager.EElement infusion in GameState.CurrentAction.Action.Infusions)
			{
				m_ElementsToInfuse.Add(infusion);
			}
		}
		m_CurrentPhaseAbilities = new List<CPhaseAbility>();
		m_EndAbilitySynced = true;
		m_CurrentAction = GameState.CurrentAction;
		GameState.CurrentAction = null;
		foreach (CActiveBonus item in CharacterClassManager.FindAllActiveBonuses())
		{
			item.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerAction);
			item.ApplyToAction = false;
		}
		if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
		{
			new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
			{
				m_Action = m_CurrentAction.Action,
				m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
			};
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInitialized, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void ResetActionAugmentationsToConsume()
	{
		m_ActionAugmentationsToConsume.Clear();
	}

	public void AddMonsterConsumes()
	{
		if (GameState.InternalCurrentActor == null || !GameState.InternalCurrentActor.IsMonsterType)
		{
			return;
		}
		m_ActionAugmentationsToConsume.AddRange(GameState.CurrentActionSelectedAugmentations);
		if (GameState.CurrentActionSelectedAugmentations.Count <= 0)
		{
			return;
		}
		foreach (CActionAugmentation consume in GameState.CurrentActionSelectedAugmentations)
		{
			foreach (CActionAugmentationOp consumeOp in consume.AugmentationOps)
			{
				if (consumeOp.Ability != null && consumeOp.Ability.ActiveBonusYML != null)
				{
					consumeOp.Ability.ActiveBonusYML.UseAlt = true;
				}
				if (!(consumeOp.ParentAbilityName != string.Empty))
				{
					continue;
				}
				CPhaseAbility cPhaseAbility = null;
				cPhaseAbility = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
				{
					if (x.m_Ability.Name == consumeOp.ParentAbilityName)
					{
						Guid? actionID = x.ActionID;
						Guid actionID2 = consume.ActionID;
						if (!actionID.HasValue)
						{
							return false;
						}
						if (!actionID.HasValue)
						{
							return true;
						}
						return actionID.GetValueOrDefault() == actionID2;
					}
					return false;
				});
				if (cPhaseAbility == null)
				{
					foreach (CPhaseAbility phaseAbility in m_PhaseAbilities)
					{
						if (phaseAbility.m_Ability is CAbilityMerged cAbilityMerged)
						{
							foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
							{
								if (mergedAbility.Name == consumeOp.ParentAbilityName && phaseAbility.ActionID == consume.ActionID)
								{
									cPhaseAbility = phaseAbility;
									break;
								}
							}
						}
						if (phaseAbility.m_Ability.SubAbilities.Any((CAbility sub) => sub.Name == consumeOp.ParentAbilityName))
						{
							cPhaseAbility = new CPhaseAbility(phaseAbility.m_Ability.SubAbilities.Find((CAbility sub) => sub.Name == consumeOp.ParentAbilityName), GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID);
						}
					}
				}
				if (cPhaseAbility == null)
				{
					continue;
				}
				if (cPhaseAbility.m_Ability.ActiveBonusYML != null)
				{
					cPhaseAbility.m_Ability.ActiveBonusYML.UseAlt = true;
				}
				if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
				{
					CAbilityOverride cAbilityOverride = cPhaseAbility.m_Ability.OverrideAbilityValues(consumeOp.AbilityOverride, perform: false);
					if (cAbilityOverride.AbilityType.HasValue && cAbilityOverride.AbilityType != cAbilityOverride.OriginalAbility.AbilityType)
					{
						CAbility ability = CAbility.CopyAbility(cPhaseAbility.m_Ability, generateNewID: false, fullCopy: true);
						m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(ability, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
						m_PhaseAbilities.Remove(cPhaseAbility);
						m_FirstPhaseAbility = ((m_PhaseAbilities.Count > 0) ? m_PhaseAbilities[0] : null);
					}
					if (cAbilityOverride.SubAbilities == null || cAbilityOverride.SubAbilities.Count <= 0)
					{
						continue;
					}
					foreach (CAbility subAbility in cAbilityOverride.SubAbilities)
					{
						if (!subAbility.IsInlineSubAbility)
						{
							CAbility cAbility = CAbility.CopyAbility(subAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, cPhaseAbility.m_Ability?.VarsContext);
							AbilityData.MiscAbilityData miscAbilityData = cAbility.MiscAbilityData;
							if ((miscAbilityData == null || miscAbilityData.FilterSpecified != true) && cPhaseAbility.m_Ability.AbilityFilter != null)
							{
								cAbility.AbilityFilter = cPhaseAbility.m_Ability.AbilityFilter.Copy();
								cAbility.MiscAbilityData.FilterSpecified = true;
							}
							cAbility.ParentAbility = cPhaseAbility.m_Ability;
							if (m_PhaseAbilities.IndexOf(cPhaseAbility) == m_PhaseAbilities.Count - 1)
							{
								m_PhaseAbilities.Add(new CPhaseAbility(cAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
							}
							else
							{
								m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility) + 1, new CPhaseAbility(cAbility, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
							}
						}
					}
				}
				else if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					CAbility ability2 = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
					CPhaseAbility cPhaseAbility2 = null;
					cPhaseAbility2 = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability.Name == consumeOp.Ability.Name);
					if (cPhaseAbility2 != null)
					{
						m_PhaseAbilities.Remove(cPhaseAbility2);
					}
					m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(ability2, GameState.InternalCurrentActor, GameState.CurrentAction.BaseCard, GameState.CurrentAction.Action.ID));
				}
			}
			foreach (ElementInfusionBoardManager.EElement infusion in consume.Infusions)
			{
				m_ElementsToInfuse.Add(infusion);
			}
		}
	}

	public void ToggleActionAugmentation(CActionAugmentation actionAugmentation, bool passing = false)
	{
		if (!passing)
		{
			CToggledActionAugmentation_MessageData cToggledActionAugmentation_MessageData = new CToggledActionAugmentation_MessageData();
			cToggledActionAugmentation_MessageData.m_CurrentAbilityAfterToggling = CurrentPhaseAbility.m_Ability;
			cToggledActionAugmentation_MessageData.m_CurrentAugment = actionAugmentation;
			ScenarioRuleClient.MessageHandler(cToggledActionAugmentation_MessageData);
		}
		if (!m_ActionAugmentationsConsumed.Contains(actionAugmentation))
		{
			if (!m_ActionAugmentationsToConsume.Contains(actionAugmentation))
			{
				m_ActionAugmentationsToConsume.Add(actionAugmentation);
				foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
				{
					if (consumeOp.Ability != null && consumeOp.Ability.ActiveBonusYML != null)
					{
						consumeOp.Ability.ActiveBonusYML.UseAlt = true;
					}
					if (consumeOp.ParentAbilityName == string.Empty)
					{
						if (!GameState.InternalCurrentActor.IsMonsterType)
						{
							if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
							{
								m_PhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(consumeOp.Ability, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
							}
							else
							{
								Log.Warning("No parent specified for ability override");
							}
						}
						continue;
					}
					CAbility cAbility = FindAbilityFromPhaseAbilityWithActionAugmentationParentName(actionAugmentation, consumeOp, out var phaseAbility, out var isCurrentPhaseAbility, out var isActiveMergedAbility);
					if (cAbility == null)
					{
						continue;
					}
					if (cAbility.ActiveBonusYML != null)
					{
						cAbility.ActiveBonusYML.UseAlt = true;
					}
					if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
					{
						if (cAbility.AbilityType == CAbility.EAbilityType.Null)
						{
							CAbility cAbility2 = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
							if (phaseAbility == CurrentPhaseAbility)
							{
								StackInlineSubAbilities(new List<CAbility> { cAbility2 }, null, performNow: true, stopPlayerSkipping: false, false);
								cAbility.AbilityHasHappened = cAbility2.AbilityHasHappened;
								ScenarioRuleClient.MessageHandler(new CStackedAugmentToNullAbility_MessageData(GameState.InternalCurrentActor));
							}
							else
							{
								m_PhaseAbilities.Add(new CPhaseAbility(cAbility2, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
							}
							continue;
						}
						CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
						if (eNegativeCondition != CCondition.ENegativeCondition.NA)
						{
							cAbility.NegativeConditions.Add(eNegativeCondition, CAbility.CopyAbility(consumeOp.Ability, generateNewID: false));
							continue;
						}
						CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
						if (ePositiveCondition != CCondition.EPositiveCondition.NA)
						{
							cAbility.PositiveConditions.Add(ePositiveCondition, CAbility.CopyAbility(consumeOp.Ability, generateNewID: false));
							continue;
						}
						CAbility ability = CAbility.CopyAbility(consumeOp.Ability, generateNewID: false);
						m_PhaseAbilities.Add(new CPhaseAbility(ability, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
					}
					else if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
					{
						bool flag = consumeOp.AbilityOverride.AbilityType.HasValue && consumeOp.AbilityOverride.AbilityType != cAbility.AbilityType;
						bool flag2 = CurrentPhaseAbility == phaseAbility && cAbility.TargetingActor != null && (!(cAbility is CAbilityMerged) || isActiveMergedAbility) && actionAugmentation.CostAbility == null;
						if (flag)
						{
							foreach (CItem item in cAbility.ActiveSingleTargetItems.Concat(cAbility.ActiveOverrideItems))
							{
								cAbility.TargetingActor.Inventory.DeselectItem(item);
							}
							for (int num = cAbility.CurrentOverrides.Count - 1; num >= 0; num--)
							{
								CAbilityOverride abilityOverride = cAbility.CurrentOverrides[num];
								cAbility.UndoOverride(abilityOverride, perform: false);
							}
						}
						if (flag2)
						{
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
						}
						CAbilityOverride cAbilityOverride = cAbility.OverrideAbilityValues(consumeOp.AbilityOverride, flag2);
						if (flag)
						{
							FinalizeOverrideAbilityType(cAbility, cAbilityOverride);
						}
						if (cAbilityOverride.SubAbilities != null && cAbilityOverride.SubAbilities.Count > 0)
						{
							foreach (CAbility subAbility2 in cAbilityOverride.SubAbilities)
							{
								if (!subAbility2.IsInlineSubAbility)
								{
									CAbility cAbility3 = CAbility.CopyAbility(subAbility2, generateNewID: false);
									AbilityData.MiscAbilityData miscAbilityData = cAbility3.MiscAbilityData;
									if ((miscAbilityData == null || miscAbilityData.FilterSpecified != true) && cAbility.AbilityFilter != null)
									{
										cAbility3.AbilityFilter = cAbility.AbilityFilter.Copy();
										cAbility3.MiscAbilityData.FilterSpecified = true;
									}
									cAbility3.ParentAbility = cAbility;
									if (isCurrentPhaseAbility)
									{
										m_PhaseAbilities.Insert(0, new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
									}
									else if (m_PhaseAbilities.IndexOf(phaseAbility) == m_PhaseAbilities.Count - 1)
									{
										m_PhaseAbilities.Add(new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
									}
									else
									{
										m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(phaseAbility) + 1, new CPhaseAbility(cAbility3, GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
									}
								}
							}
						}
						if (phaseAbility?.m_Ability is CAbilityAttack { AttackSummary: not null } cAbilityAttack)
						{
							cAbilityAttack.AttackSummary.UpdateTargetData(cAbilityAttack, cAbilityAttack.ActiveSingleTargetItems, cAbilityAttack.ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
						}
					}
					else
					{
						Log.Warning("Unable to find matching parent ability '" + consumeOp.ParentAbilityName + "' for consume.");
					}
				}
				if (actionAugmentation.CostAbility != null)
				{
					if (!string.IsNullOrEmpty(actionAugmentation.CostAbility.ParentName))
					{
						CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
						{
							if (x.m_Ability.Name == actionAugmentation.CostAbility.ParentName)
							{
								Guid? actionID = x.ActionID;
								Guid actionID2 = actionAugmentation.ActionID;
								if (!actionID.HasValue)
								{
									return false;
								}
								if (!actionID.HasValue)
								{
									return true;
								}
								return actionID.GetValueOrDefault() == actionID2;
							}
							return false;
						});
						if (cPhaseAbility != null)
						{
							CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
							StackInlineSubAbilities(new List<CAbility> { actionAugmentation.CostAbility }, null, performNow: true, stopPlayerSkipping: false, false);
							foreach (CItem activeOverrideItem in currentPhaseAbility.m_Ability.ActiveOverrideItems)
							{
								foreach (CAbilityOverride @override in activeOverrideItem.YMLData.Data.Overrides)
								{
									currentPhaseAbility.m_Ability.UndoOverride(@override, perform: false, activeOverrideItem);
								}
								GameState.InternalCurrentActor.Inventory.DeselectItem(activeOverrideItem);
							}
						}
						else
						{
							cPhaseAbility = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
							{
								if (x.m_Ability.Name == actionAugmentation.CostAbility.ParentName)
								{
									Guid? actionID = x.ActionID;
									Guid actionID2 = actionAugmentation.ActionID;
									if (!actionID.HasValue)
									{
										return false;
									}
									if (!actionID.HasValue)
									{
										return true;
									}
									return actionID.GetValueOrDefault() == actionID2;
								}
								return false;
							});
							if (cPhaseAbility != null)
							{
								m_PhaseAbilities.Insert(m_PhaseAbilities.IndexOf(cPhaseAbility), new CPhaseAbility(CAbility.CopyAbility(actionAugmentation.CostAbility, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID, null, null, killAfter: false, isCostAbility: true));
							}
							else
							{
								Log.Warning("Unable to find matching parent ability '" + actionAugmentation.CostAbility.ParentName + "' for consume.");
							}
						}
					}
					else
					{
						m_PhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(actionAugmentation.CostAbility, generateNewID: false), GameState.InternalCurrentActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID));
					}
					PhaseManager.StepComplete();
				}
				foreach (ElementInfusionBoardManager.EElement infusion in actionAugmentation.Infusions)
				{
					m_ElementsToInfuse.Add(infusion);
				}
				m_ActionConsumeXP += actionAugmentation.XP;
				SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionAddConsumeAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", m_ActionAugmentationsToConsume, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
			}
			else
			{
				foreach (ElementInfusionBoardManager.EElement infusion2 in actionAugmentation.Infusions)
				{
					m_ElementsToInfuse.Remove(infusion2);
				}
				m_ActionAugmentationsToConsume.Remove(actionAugmentation);
				m_ActionConsumeXP -= actionAugmentation.XP;
				if (actionAugmentation.CostAbility != null)
				{
					CPhaseAbility cPhaseAbility2 = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
					{
						if (x.m_Ability.Name == actionAugmentation.CostAbility.Name)
						{
							Guid? actionID = x.ActionID;
							Guid actionID2 = actionAugmentation.ActionID;
							if (!actionID.HasValue)
							{
								return false;
							}
							if (!actionID.HasValue)
							{
								return true;
							}
							return actionID.GetValueOrDefault() == actionID2;
						}
						return false;
					});
					if (cPhaseAbility2 != null)
					{
						m_PhaseAbilities.Remove(cPhaseAbility2);
					}
					else
					{
						UnstackAbility(actionAugmentation.CostAbility);
					}
				}
				foreach (CActionAugmentationOp consumeOp2 in actionAugmentation.AugmentationOps)
				{
					if (consumeOp2.ParentAbilityName == string.Empty)
					{
						if (consumeOp2.Ability == null)
						{
							continue;
						}
						CPhaseAbility cPhaseAbility3 = m_PhaseAbilities.FirstOrDefault((CPhaseAbility x) =>
						{
							if (x.m_Ability.Name == consumeOp2.Ability.Name)
							{
								Guid? actionID = x.ActionID;
								Guid actionID2 = actionAugmentation.ActionID;
								if (!actionID.HasValue)
								{
									return false;
								}
								if (!actionID.HasValue)
								{
									return true;
								}
								return actionID.GetValueOrDefault() == actionID2;
							}
							return false;
						});
						if (cPhaseAbility3 != null)
						{
							m_PhaseAbilities.Remove(cPhaseAbility3);
						}
						else
						{
							UnstackAbility(consumeOp2.Ability);
						}
						continue;
					}
					CAbility cAbility4 = FindAbilityFromPhaseAbilityWithActionAugmentationParentName(actionAugmentation, consumeOp2, out var phaseAbility2, out var isCurrentPhaseAbility2, out var _);
					if (consumeOp2.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
					{
						if (cAbility4 == null)
						{
							continue;
						}
						if (cAbility4.AbilityType == CAbility.EAbilityType.Null)
						{
							if ((phaseAbility2 == null) | isCurrentPhaseAbility2)
							{
								UnstackAbility(consumeOp2.Ability);
							}
							else
							{
								m_PhaseAbilities.Remove(phaseAbility2);
							}
							continue;
						}
						CCondition.ENegativeCondition eNegativeCondition2 = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp2.Ability.AbilityType.ToString());
						if (eNegativeCondition2 != CCondition.ENegativeCondition.NA)
						{
							cAbility4.NegativeConditions.Remove(eNegativeCondition2);
							continue;
						}
						CCondition.EPositiveCondition ePositiveCondition2 = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp2.Ability.AbilityType.ToString());
						if (ePositiveCondition2 != CCondition.EPositiveCondition.NA)
						{
							cAbility4.PositiveConditions.Remove(ePositiveCondition2);
						}
						else if ((phaseAbility2 == null) | isCurrentPhaseAbility2)
						{
							UnstackAbility(consumeOp2.Ability);
						}
						else
						{
							m_PhaseAbilities.Remove(phaseAbility2);
						}
					}
					else
					{
						if (consumeOp2.Type != CActionAugmentationOp.EActionAugmentationType.AbilityOverride || cAbility4 == null)
						{
							continue;
						}
						CAbilityOverride cAbilityOverride2 = cAbility4.FindOverride(consumeOp2.AbilityOverride);
						bool flag3 = cAbilityOverride2?.OriginalAbility != null && cAbilityOverride2.AbilityType.HasValue && cAbilityOverride2.AbilityType != cAbilityOverride2.OriginalAbility?.AbilityType;
						bool flag4 = CurrentPhaseAbility == phaseAbility2 && cAbility4.TargetingActor != null && !passing;
						if (flag4)
						{
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
						}
						if (!passing)
						{
							cAbility4.UndoOverride(cAbilityOverride2, flag4 && !flag3);
						}
						if (flag3)
						{
							foreach (CItem item2 in cAbility4.ActiveSingleTargetItems.Concat(cAbility4.ActiveOverrideItems))
							{
								cAbility4.TargetingActor.Inventory.DeselectItem(item2);
							}
							for (int num2 = cAbility4.CurrentOverrides.Count - 1; num2 >= 0; num2--)
							{
								CAbilityOverride abilityOverride2 = cAbility4.CurrentOverrides[num2];
								cAbility4.UndoOverride(abilityOverride2, perform: false);
							}
							ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
							FinalizeOverrideAbilityType(cAbility4);
						}
						if (cAbilityOverride2.SubAbilities != null && cAbilityOverride2.SubAbilities.Count > 0)
						{
							foreach (CAbility subAbility in cAbilityOverride2.SubAbilities)
							{
								if (!subAbility.IsInlineSubAbility)
								{
									CPhaseAbility cPhaseAbility4 = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability.ID == subAbility.ID);
									if (cPhaseAbility4 != null)
									{
										m_PhaseAbilities.Remove(cPhaseAbility4);
									}
									else if (phaseAbility2 != null)
									{
										m_PhaseAbilities.Remove(phaseAbility2);
									}
									else
									{
										UnstackAbility(cAbility4);
									}
								}
							}
						}
						if (phaseAbility2?.m_Ability is CAbilityAttack { AttackSummary: not null } cAbilityAttack2)
						{
							cAbilityAttack2.AttackSummary.UpdateTargetData(cAbilityAttack2, cAbilityAttack2.ActiveSingleTargetItems, cAbilityAttack2.ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
						}
					}
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionRemoveConsumeAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", m_ActionAugmentationsToConsume, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
			}
			if (CurrentPhaseAbility != null)
			{
				foreach (CItem item3 in GameState.InternalCurrentActor.Inventory.SelectedItems.FindAll((CItem a) => a.YMLData.ItemType == CItem.EItemType.Override))
				{
					if (item3.YMLData.Data.CompareAbility == null || item3.YMLData.Data.CompareAbility.CompareAbility(CurrentPhaseAbility.m_Ability) || item3.YMLData.Data.ItemRequirements == null || item3.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(CurrentPhaseAbility.m_Ability.TargetingActor, CurrentPhaseAbility.m_Ability, null, item3))
					{
						continue;
					}
					foreach (CAbilityOverride override2 in item3.YMLData.Data.Overrides)
					{
						CurrentPhaseAbility.m_Ability.UndoOverride(override2, perform: false, item3);
					}
					GameState.InternalCurrentActor.Inventory.DeselectItem(item3);
				}
			}
		}
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
		{
			CItem.EItemTrigger[] acceptedTriggers = (((CurrentPhaseAbility?.m_Ability?.ActorsToTarget?.Count).GetValueOrDefault() <= 0) ? new CItem.EItemTrigger[2]
			{
				CItem.EItemTrigger.EntireAction,
				CItem.EItemTrigger.SingleAbility
			} : new CItem.EItemTrigger[3]
			{
				CItem.EItemTrigger.EntireAction,
				CItem.EItemTrigger.SingleAbility,
				CItem.EItemTrigger.SingleTarget
			});
			cPlayerActor.Inventory.HighlightUsableItems(CurrentPhaseAbility?.m_Ability, acceptedTriggers);
		}
	}

	private CAbility FindAbilityFromPhaseAbilityWithActionAugmentationParentName(CActionAugmentation actionAugmentation, CActionAugmentationOp consumeOp, out CPhaseAbility phaseAbility, out bool isCurrentPhaseAbility, out bool isActiveMergedAbility)
	{
		CAbility result = null;
		isCurrentPhaseAbility = false;
		isActiveMergedAbility = false;
		phaseAbility = m_CurrentPhaseAbilities.LastOrDefault((CPhaseAbility x) => x.m_Ability.Name == consumeOp.ParentAbilityName && x.ActionID == actionAugmentation.ActionID && !x.m_Ability.AbilityHasBeenCancelled);
		if (phaseAbility != null)
		{
			isCurrentPhaseAbility = true;
			return phaseAbility.m_Ability;
		}
		if (phaseAbility == null)
		{
			foreach (CPhaseAbility currentPhaseAbility in m_CurrentPhaseAbilities)
			{
				if (currentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged)
				{
					foreach (CAbility copiedMergedAbility in cAbilityMerged.CopiedMergedAbilities)
					{
						if (copiedMergedAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
						{
							phaseAbility = currentPhaseAbility;
							result = copiedMergedAbility;
							isCurrentPhaseAbility = true;
							if (cAbilityMerged.ActiveAbility.Name == copiedMergedAbility.Name)
							{
								isActiveMergedAbility = true;
							}
							break;
						}
					}
					continue;
				}
				if (currentPhaseAbility.m_Ability is CAbilityControlActor cAbilityControlActor)
				{
					foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
					{
						if (controlAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
						{
							phaseAbility = currentPhaseAbility;
							result = controlAbility;
							isCurrentPhaseAbility = true;
							break;
						}
					}
					continue;
				}
				foreach (CAbility subAbility in currentPhaseAbility.m_Ability.SubAbilities)
				{
					if (subAbility.Name == consumeOp.ParentAbilityName && currentPhaseAbility.ActionID == actionAugmentation.ActionID)
					{
						phaseAbility = currentPhaseAbility;
						result = subAbility;
						isCurrentPhaseAbility = true;
						break;
					}
				}
			}
		}
		if (phaseAbility == null)
		{
			phaseAbility = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
			{
				if (x.m_Ability.Name == consumeOp.ParentAbilityName)
				{
					Guid? actionID = x.ActionID;
					Guid actionID2 = actionAugmentation.ActionID;
					if (!actionID.HasValue)
					{
						return false;
					}
					if (!actionID.HasValue)
					{
						return true;
					}
					return actionID.GetValueOrDefault() == actionID2;
				}
				return false;
			});
			if (phaseAbility != null)
			{
				return phaseAbility.m_Ability;
			}
			if (phaseAbility == null)
			{
				foreach (CPhaseAbility phaseAbility2 in m_PhaseAbilities)
				{
					if (phaseAbility2.m_Ability is CAbilityMerged cAbilityMerged2)
					{
						foreach (CAbility copiedMergedAbility2 in cAbilityMerged2.CopiedMergedAbilities)
						{
							if (copiedMergedAbility2.Name == consumeOp.ParentAbilityName && phaseAbility2.ActionID == actionAugmentation.ActionID)
							{
								phaseAbility = phaseAbility2;
								result = copiedMergedAbility2;
								break;
							}
						}
						continue;
					}
					if (phaseAbility2.m_Ability is CAbilityControlActor cAbilityControlActor2)
					{
						foreach (CAbility controlAbility2 in cAbilityControlActor2.ControlActorData.ControlAbilities)
						{
							if (controlAbility2.Name == consumeOp.ParentAbilityName && phaseAbility2.ActionID == actionAugmentation.ActionID)
							{
								phaseAbility = phaseAbility2;
								result = controlAbility2;
								break;
							}
						}
						continue;
					}
					foreach (CAbility subAbility2 in phaseAbility2.m_Ability.SubAbilities)
					{
						if (subAbility2.Name == consumeOp.ParentAbilityName && phaseAbility2.ActionID == actionAugmentation.ActionID)
						{
							phaseAbility = phaseAbility2;
							result = subAbility2;
							break;
						}
					}
				}
			}
		}
		return result;
	}

	private void ToggleLockOtherBonuses(CActiveBonus fromActiveBonus, bool toggleOn)
	{
		if (toggleOn)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				IEnumerable<CActiveBonus> first = allAliveActor.ActiveBonuses.Where((CActiveBonus x) => x.ToggledBonus && x.Ability.ActiveBonusData.IsToggleBonus && !x.ToggleLocked && x.Ability is CAbilityAddActiveBonus);
				IEnumerable<CActiveBonus> second = allAliveActor.ActiveBonuses.Where((CActiveBonus x) => x.CostAbilityExecuted);
				foreach (CActiveBonus item in first.Concat(second))
				{
					if (item != fromActiveBonus)
					{
						TempLockedBonuses.Add(item);
						item.ToggleLocked = true;
					}
				}
			}
			return;
		}
		if (TempLockedBonuses.Count <= 0)
		{
			return;
		}
		foreach (CActiveBonus tempLockedBonuse in TempLockedBonuses)
		{
			tempLockedBonuse.ToggleLocked = false;
		}
		TempLockedBonuses.Clear();
	}

	public void HandleActiveBonusCostAbilityToggling(CActiveBonus fromActiveBonus, CAbility costAbility, bool toggleOn)
	{
		if (costAbility == null)
		{
			return;
		}
		ToggleLockOtherBonuses(fromActiveBonus, toggleOn);
		if (toggleOn)
		{
			if (costAbility.MiscAbilityData == null)
			{
				costAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			costAbility.MiscAbilityData.IsCostAbility = true;
			GameState.CurrentActor.Inventory.LockInSelectedItemsAndResetUnselected();
			StackInlineSubAbilities(new List<CAbility> { costAbility }, null, performNow: true, stopPlayerSkipping: true, false, stopPlayerUndo: false, fromActiveBonus);
			PhaseManager.StepComplete();
		}
		else
		{
			GameState.CurrentActor.Inventory.OnAbilityEnd();
			CPhaseAbility cPhaseAbility = m_PhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability.ID == costAbility.ID);
			if (cPhaseAbility != null)
			{
				m_PhaseAbilities.Remove(cPhaseAbility);
			}
			else
			{
				UnstackAbility(costAbility, fromActiveBonus);
			}
		}
	}

	public bool HasAnyUntoggleableActionAugments()
	{
		foreach (CActionAugmentation item in m_ActionAugmentationsToConsume)
		{
			if (!CanUntoggleActionAugment(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyActionAugmentationsConsumed()
	{
		return m_ActionAugmentationsConsumed.Count > 0;
	}

	public List<CActionAugmentation> ActionAugmentationsAvailableForCurrentAbility()
	{
		List<CActionAugmentation> list = m_ActionAugmentationsToConsume.ToList();
		GameState.CurrentActorAction currentAction = m_CurrentAction;
		CPhaseAbility cacheCurrentPhaseAbility = CurrentPhaseAbility;
		if (cacheCurrentPhaseAbility == null)
		{
			return list;
		}
		if (currentAction.BaseCard is CAbilityCard && currentAction.Action.Augmentations != null && currentAction.Action.Augmentations.Count > 0)
		{
			foreach (CActionAugmentation actionAugmentation in currentAction.Action.Augmentations)
			{
				if (m_ActionAugmentationsConsumed.Any((CActionAugmentation a) => a.ActionID == actionAugmentation.ActionID && a.Name == actionAugmentation.Name))
				{
					continue;
				}
				List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(GameState.CurrentActor);
				bool flag = true;
				foreach (ElementInfusionBoardManager.EElement item in actionAugmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
				{
					flag = availableElements.Remove(item);
					if (!flag)
					{
						break;
					}
				}
				if (actionAugmentation.Elements.Count((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any) > availableElements.Count)
				{
					flag = false;
				}
				if (!flag || (actionAugmentation.Requirements != null && !actionAugmentation.Requirements.MeetsAbilityRequirements(GameState.InternalCurrentActor, cacheCurrentPhaseAbility.m_Ability)))
				{
					continue;
				}
				if (actionAugmentation.CostAbility != null)
				{
					if (!string.IsNullOrEmpty(actionAugmentation.CostAbility.ParentName))
					{
						if (cacheCurrentPhaseAbility != null && cacheCurrentPhaseAbility.m_Ability.Name == actionAugmentation.CostAbility.ParentName)
						{
							Guid? actionID = cacheCurrentPhaseAbility.ActionID;
							Guid actionID2 = actionAugmentation.ActionID;
							if (actionID.HasValue && (!actionID.HasValue || actionID.GetValueOrDefault() == actionID2) && actionAugmentation.CostAbility.MeetsStartAbilityRequirements(GameState.InternalCurrentActor))
							{
								list.Add(actionAugmentation);
							}
						}
					}
					else
					{
						list.Add(actionAugmentation);
					}
					continue;
				}
				foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
				{
					if (consumeOp.ParentAbilityName == string.Empty)
					{
						list.Add(actionAugmentation);
						continue;
					}
					CPhaseAbility cPhaseAbility = null;
					cPhaseAbility = m_CurrentPhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
					{
						if (x.m_Ability.Name == consumeOp.ParentAbilityName)
						{
							Guid? actionID3 = x.ActionID;
							Guid actionID4 = actionAugmentation.ActionID;
							if (actionID3.HasValue && (!actionID3.HasValue || actionID3.GetValueOrDefault() == actionID4) && !x.m_Ability.IsInlineSubAbility && (x.m_Ability == cacheCurrentPhaseAbility.m_Ability || x.m_Ability.ParentAbility == cacheCurrentPhaseAbility.m_Ability))
							{
								if (x.m_Ability.AbilityType != CAbility.EAbilityType.Null)
								{
									return !x.m_Ability.AbilityHasHappened;
								}
								return true;
							}
						}
						return false;
					});
					if (cPhaseAbility == null && cacheCurrentPhaseAbility?.m_Ability?.SubAbilities != null)
					{
						foreach (CAbility subAbility in cacheCurrentPhaseAbility.m_Ability.SubAbilities)
						{
							if (subAbility != cacheCurrentPhaseAbility.m_Ability && subAbility.Name == consumeOp.ParentAbilityName && subAbility.IsInlineSubAbility)
							{
								cPhaseAbility = cacheCurrentPhaseAbility;
								break;
							}
						}
					}
					if (cPhaseAbility == null)
					{
						foreach (CPhaseAbility currentPhaseAbility in m_CurrentPhaseAbilities)
						{
							if (currentPhaseAbility.m_Ability is CAbilityMerged { ActiveAbility: var activeAbility } && activeAbility?.Name == consumeOp.ParentAbilityName)
							{
								cPhaseAbility = currentPhaseAbility;
								break;
							}
						}
					}
					if (cPhaseAbility != null && (consumeOp.AbilityOverride == null || consumeOp.AbilityOverride.StartAbilityRequirements == null || consumeOp.AbilityOverride.StartAbilityRequirements.MeetsAbilityRequirements(GameState.InternalCurrentActor, cPhaseAbility.m_Ability)))
					{
						list.Add(actionAugmentation);
					}
				}
			}
		}
		return list;
	}

	public bool CanUntoggleActionAugment(CActionAugmentation actionAugmentation)
	{
		if (m_ActionAugmentationsToConsume.Contains(actionAugmentation))
		{
			if (actionAugmentation.CostAbility != null && m_PreviousPhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
			{
				if (x.m_Ability.Name == actionAugmentation.CostAbility.Name)
				{
					Guid? actionID = x.ActionID;
					Guid actionID2 = actionAugmentation.ActionID;
					if (!actionID.HasValue)
					{
						return false;
					}
					if (!actionID.HasValue)
					{
						return true;
					}
					return actionID.GetValueOrDefault() == actionID2;
				}
				return false;
			}) != null)
			{
				return false;
			}
			foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
			{
				if (consumeOp.Ability != null && m_PreviousPhaseAbilities.SingleOrDefault((CPhaseAbility x) =>
				{
					if (x.m_Ability.Name == consumeOp.Ability.Name)
					{
						Guid? actionID = x.ActionID;
						Guid actionID2 = actionAugmentation.ActionID;
						if (!actionID.HasValue)
						{
							return false;
						}
						if (!actionID.HasValue)
						{
							return true;
						}
						return actionID.GetValueOrDefault() == actionID2;
					}
					return false;
				}) != null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool ShouldUntoggleActionAugmentAfterPassingAbility(CActionAugmentation actionAugmentation)
	{
		foreach (CActionAugmentationOp consumeOp in actionAugmentation.AugmentationOps)
		{
			if (RemainingPhaseAbilities.Any((CPhaseAbility x) => x.m_Ability.AutoTrigger && x.m_Ability.Name == consumeOp.ParentAbilityName))
			{
				return false;
			}
			if (CurrentPhaseAbility != null && CurrentPhaseAbility.m_Ability != null)
			{
				if (consumeOp.ParentAbilityType != CurrentPhaseAbility.m_Ability.AbilityType && CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Push)
				{
					return false;
				}
				if (CurrentPhaseAbility.m_Ability.AbilityHasBeenCancelled && CurrentPhaseAbility.m_Ability.AbilityInterruptCopy != null)
				{
					return false;
				}
			}
			if (consumeOp.AbilityOverride != null && consumeOp.AbilityOverride.ControlActorData?.ControlAbilities != null && consumeOp.AbilityOverride.ControlActorData.ControlAbilities.Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnUpdate()
	{
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.Update();
		}
	}

	public static bool IsFirstAbility(CAbility ability)
	{
		if (ability == null)
		{
			return false;
		}
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (currentPhaseAction?.m_FirstPhaseAbility == null)
		{
			return true;
		}
		CPhaseAbility cPhaseAbility = currentPhaseAction.CurrentPhaseAbilities.FirstOrDefault((CPhaseAbility x) => x.m_Ability.ID == ability.ID);
		if (cPhaseAbility == null)
		{
			return false;
		}
		return currentPhaseAction.m_FirstPhaseAbility == cPhaseAbility;
	}

	public static bool AboutToEndACancelledAbility()
	{
		if (PhaseManager.CurrentPhaseAction != null)
		{
			CAbility cAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility?.m_Ability;
			if (cAbility != null && cAbility.AbilityHasBeenCancelledImmediately)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CurrentlyEndingAnAbility()
	{
		if (PhaseManager.CurrentPhaseAction != null)
		{
			CAbility cAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility?.m_Ability;
			if (cAbility != null && cAbility.HasEnded)
			{
				return true;
			}
		}
		return false;
	}

	private void Perform(bool fullAbilityRestart)
	{
		m_CurrentActionID = CurrentPhaseAbility.ActionID;
		if (fullAbilityRestart)
		{
			if (PreviousPhaseAbilities.Count > 0 && ((PreviousPhaseAbilities.Last() == m_FirstPhaseAbility && !m_HasAnyAbilityHappened) || (PreviousPhaseAbilities.Count > 0 && PreviousPhaseAbilities.Where((CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Null).Count() == PreviousPhaseAbilities.Count)))
			{
				m_FirstPhaseAbility = CurrentPhaseAbility;
			}
			CStartActorAbility_MessageData cStartActorAbility_MessageData = new CStartActorAbility_MessageData(GameState.InternalCurrentActor);
			cStartActorAbility_MessageData.m_IsFirstAbility = m_FirstPhaseAbility == CurrentPhaseAbility;
			cStartActorAbility_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			cStartActorAbility_MessageData.merged = false;
			if (cStartActorAbility_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.MergedMoveAttack)
			{
				cStartActorAbility_MessageData.merged = true;
			}
			ScenarioRuleClient.MessageHandler(cStartActorAbility_MessageData);
			if (GameState.InternalCurrentActor is CPlayerActor { Inventory: var inventory })
			{
				inventory.HighlightUsableItems(CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			}
		}
		CurrentPhaseAbility.m_Ability.Perform();
		LogActionPerform();
	}

	public void UndoItemAbility()
	{
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		RemainingPhaseAbilities.RemoveAll((CPhaseAbility x) => x.m_Ability.IsItemAbility);
		GameState.InternalCurrentActor.Inventory.Undo();
		EndAbilitySynchronise(undoItemAbility: true);
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUndoItemAbility, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void UndoChooseAbility()
	{
		CurrentPhaseAbility.m_Ability.ParentAbility.Undo();
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		GameState.InternalCurrentActor.Inventory.Undo();
		EndAbilitySynchronise();
	}

	public void OnFirstAbilityStarted()
	{
		if ((!m_HasAnyNonItemAbilityHappened || CurrentPhaseAbility.m_Ability.IsItemAbility) && !CurrentPhaseAbility.m_Ability.IsScenarioModifierAbility)
		{
			if (CurrentPhaseAbility.m_Ability is CAbilityAttack cAbilityAttack)
			{
				_ = cAbilityAttack.IsDefaultAttack;
			}
			if (CurrentPhaseAbility.m_Ability is CAbilityMove cAbilityMove)
			{
				_ = cAbilityMove.IsDefaultMove;
			}
			CBaseCard cBaseCard = CurrentPhaseAbility?.m_BaseCard;
			if (cBaseCard == null && CurrentPhaseAbility?.m_Ability?.GetAbilityBaseCard() != null && CurrentPhaseAbility.m_Ability.GetAbilityBaseCard().CardType != CBaseCard.ECardType.Dummy)
			{
				cBaseCard = CurrentPhaseAbility.m_Ability.GetAbilityBaseCard();
			}
			if (!CurrentPhaseAbility.m_Ability.GetMiscVar("SkipCombatLog").GetBool())
			{
				SEventAction sEventAction = new SEventAction(ESESubTypeAction.ActionOnFirstAbilityStarted, (cBaseCard != null) ? cBaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability);
				SEventLogMessageHandler.AddEventLogMessage(sEventAction, processImmediately: true);
				CAbilityStartUpdateCombatLog_MessageData cAbilityStartUpdateCombatLog_MessageData = new CAbilityStartUpdateCombatLog_MessageData(GameState.InternalCurrentActor);
				cAbilityStartUpdateCombatLog_MessageData.m_AbilityType = CurrentPhaseAbility.m_Ability.AbilityType;
				cAbilityStartUpdateCombatLog_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
				cAbilityStartUpdateCombatLog_MessageData.m_Event = sEventAction;
				ScenarioRuleClient.MessageHandler(cAbilityStartUpdateCombatLog_MessageData);
			}
		}
		UpdateItemsUsedInPhase();
	}

	public void UpdateItemsUsedInPhase()
	{
		COverrideItemUsedUpdateCombatLog_MessageData cOverrideItemUsedUpdateCombatLog_MessageData = new COverrideItemUsedUpdateCombatLog_MessageData(GameState.InternalCurrentActor);
		foreach (CItem activeOverrideItem in CurrentPhaseAbility.m_Ability.ActiveOverrideItems)
		{
			if (!m_ItemsLoggedAsUsedThisPhase.Contains(activeOverrideItem))
			{
				m_ItemsLoggedAsUsedThisPhase.Add(activeOverrideItem);
				cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate.Add(activeOverrideItem);
			}
		}
		foreach (CItem activeSingleTargetItem in CurrentPhaseAbility.m_Ability.ActiveSingleTargetItems)
		{
			if (!m_ItemsLoggedAsUsedThisPhase.Contains(activeSingleTargetItem))
			{
				m_ItemsLoggedAsUsedThisPhase.Add(activeSingleTargetItem);
				cOverrideItemUsedUpdateCombatLog_MessageData.m_ItemsToUpdate.Add(activeSingleTargetItem);
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUpdateItemsUsedInPhase, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		ScenarioRuleClient.MessageHandler(cOverrideItemUsedUpdateCombatLog_MessageData);
	}

	public void RefreshConsumeBar()
	{
		CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
		{
			m_Action = m_CurrentAction.Action,
			m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnNextStep(bool passing = false)
	{
		bool overridingCurrentActor = GameState.OverridingCurrentActor;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionNextStep, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (!m_EndAbilitySynced && (CurrentPhaseAbility == null || CurrentPhaseAbility.m_Ability == null || !CurrentPhaseAbility.m_Ability.IsInlineSubAbility))
		{
			return;
		}
		if (CurrentPhaseAbility == null)
		{
			EndAbilitySynchronise();
			return;
		}
		bool flag = (CurrentPhaseAbility.m_Ability.AbilityHasHappened && !CurrentPhaseAbility.m_Ability.ExplicitlyDoesNothing) || CurrentPhaseAbility.m_Ability.AugmentsAdded;
		m_HasAnyAbilityHappened |= flag;
		m_HasAnyNonItemAbilityHappened = m_HasAnyNonItemAbilityHappened || (flag && !CurrentPhaseAbility.m_Ability.IsItemAbility);
		if (passing && !flag)
		{
			for (int num = m_ActionAugmentationsToConsume.Count - 1; num >= 0; num--)
			{
				CActionAugmentation actionAugmentation = m_ActionAugmentationsToConsume[num];
				if (ShouldUntoggleActionAugmentAfterPassingAbility(actionAugmentation))
				{
					ToggleActionAugmentation(actionAugmentation, passing: true);
				}
			}
		}
		if (CurrentPhaseAbility == null)
		{
			EndAbilitySynchronise();
			return;
		}
		CPhaseAbility cPhaseAbility = CurrentPhaseAbility;
		if (m_PhaseAbilities.Count > 0 && m_PhaseAbilities[0].m_Ability.IsSubAbility && CurrentPhaseAbility.m_Ability != m_PhaseAbilities[0].m_Ability.ParentAbility)
		{
			CPhaseAbility cPhaseAbility2 = PreviousPhaseAbilities.SingleOrDefault((CPhaseAbility x) => x.m_Ability == m_PhaseAbilities[0].m_Ability.ParentAbility);
			if (cPhaseAbility2 != null && cPhaseAbility2.m_Ability.AbilityType == CAbility.EAbilityType.Move)
			{
				cPhaseAbility = cPhaseAbility2;
			}
		}
		if (cPhaseAbility.m_Ability.AbilityType != CAbility.EAbilityType.Null)
		{
			while (m_PhaseAbilities.Count > 0)
			{
				CAbility firstPhaseAbility = m_PhaseAbilities[0]?.m_Ability;
				bool isSubAbility = firstPhaseAbility.IsSubAbility;
				bool flag2 = firstPhaseAbility.ParentAbility != null;
				bool flag3 = cPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Move && m_HasAnyAbilityHappened && (firstPhaseAbility.MiscAbilityData == null || firstPhaseAbility.MiscAbilityData.TreatAsNonSubAbility != true);
				bool flag4 = cPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Move && cPhaseAbility.m_Ability.AbilityHasHappened && firstPhaseAbility.MiscAbilityData != null && firstPhaseAbility.MiscAbilityData.TreatAsNonSubAbility == true;
				bool isInlineSubAbility = firstPhaseAbility.IsInlineSubAbility;
				bool flag5 = firstPhaseAbility.InlineSubAbilityTiles != null && firstPhaseAbility.InlineSubAbilityTiles.Count > 0;
				bool flag6 = isInlineSubAbility & flag5;
				bool flag7 = flag2 && !firstPhaseAbility.ParentAbility.AbilityHasHappened;
				bool flag8 = flag2 && CurrentPhaseAbility?.m_Ability?.ID != firstPhaseAbility.ParentAbility.ID && CurrentPhaseAbilities.Any((CPhaseAbility x) => x.m_Ability.ID == firstPhaseAbility.ParentAbility.ID && !x.m_Ability.AbilityHasBeenCancelled);
				bool flag9 = flag2 && !firstPhaseAbility.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && firstPhaseAbility.ParentAbility.TilesSelected != null && firstPhaseAbility.ParentAbility.TilesSelected.Count > 0;
				if (!isSubAbility || flag3 || flag4 || flag6 || !flag7 || flag8 || (!passing && flag2 && flag9))
				{
					break;
				}
				m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
			}
		}
		if (!CurrentPhaseAbility.ItemID.HasValue)
		{
			GameState.InternalCurrentActor.ActorActionHasHappened |= flag;
			if (CurrentPhaseAbility.m_BaseCard != null)
			{
				CurrentPhaseAbility.m_BaseCard.ActionHasHappened |= flag;
			}
		}
		else
		{
			GameState.InternalCurrentActor.Inventory.HandleUsedItem(CurrentPhaseAbility.ItemID.Value);
			GameState.InternalCurrentActor.Inventory.OnAbilityEnd();
		}
		string animOverload = CurrentPhaseAbility.m_Ability.AnimOverload;
		CurrentPhaseAbility.m_Ability.AbilityEnded();
		if (CurrentPhaseAbility.m_Ability.AbilityHasHappened || CurrentPhaseAbility.m_Ability.AugmentsAdded)
		{
			GameState.InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Abilities);
			if (!CurrentPhaseAbility.m_Ability.IsItemAbility)
			{
				m_ApplyActionXP = true;
			}
			if (m_CurrentActionID.HasValue)
			{
				for (int num2 = m_ActionAugmentationsToConsume.Count - 1; num2 >= 0; num2--)
				{
					CActionAugmentation cActionAugmentation = m_ActionAugmentationsToConsume[num2];
					if (cActionAugmentation.AugmentationOps.Count > 0)
					{
						foreach (CActionAugmentationOp augmentationOp in cActionAugmentation.AugmentationOps)
						{
							if (CurrentPhaseAbility.m_Ability.Name == augmentationOp.ParentAbilityName || (augmentationOp.Ability != null && CurrentPhaseAbility.m_Ability.Name == augmentationOp.Ability.Name) || (cActionAugmentation.CostAbility != null && CurrentPhaseAbility.m_Ability.ID == cActionAugmentation.CostAbility.ID))
							{
								cActionAugmentation.ConsumeConsumables(GameState.InternalCurrentActor);
								m_ActionAugmentationsConsumed.Add(cActionAugmentation);
								m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
								break;
							}
						}
					}
					else
					{
						cActionAugmentation.ConsumeConsumables(GameState.InternalCurrentActor);
						m_ActionAugmentationsConsumed.Add(cActionAugmentation);
						m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
					}
				}
			}
			CActionAbilityHasHappened_MessageData cActionAbilityHasHappened_MessageData = new CActionAbilityHasHappened_MessageData(GameState.InternalCurrentActor);
			cActionAbilityHasHappened_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			ScenarioRuleClient.MessageHandler(cActionAbilityHasHappened_MessageData);
		}
		if (CurrentPhaseAbility != null)
		{
			LastPlayedAbility = CurrentPhaseAbility;
		}
		if (cPhaseAbility.m_Ability.AbilityHasHappened && !cPhaseAbility.m_Ability.AbilityHasBeenCancelled)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				GameState.ShowItemSelection(playerActor, playerActor, cPhaseAbility.m_Ability, CItem.EItemTrigger.OnAbilityEnded);
			}
		}
		CActor cActor = CurrentPhaseAbility?.m_Ability?.TargetingActor;
		if (cActor == null)
		{
			cActor = GameState.InternalCurrentActor;
		}
		CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
		CurrentPhaseAbility.m_EndAction?.Invoke();
		m_PreviousPhaseAbilities.Add(CurrentPhaseAbility);
		m_CurrentPhaseAbilities.Remove(CurrentPhaseAbility);
		if (m_CurrentTriggeredActiveBonus != null)
		{
			if (m_CurrentTriggeredActiveBonus.Ability.ActiveBonusData.CostAbility != null && (m_CurrentTriggeredActiveBonus.Ability.ActiveBonusData.CostAbility.ID == PreviousPhaseAbilities.Last().m_Ability.ID || m_CurrentTriggeredActiveBonus.Ability.ActiveBonusData.CostAbility.ID == m_CurrentPhaseAbilities.Last().m_Ability.ID))
			{
				m_CurrentTriggeredActiveBonus.ToggleLocked = false;
				m_CurrentTriggeredActiveBonus.CostAbilityExecuted = true;
			}
			if (m_CurrentTriggeredActiveBonus is CDuringTurnAbilityActiveBonus cDuringTurnAbilityActiveBonus)
			{
				cDuringTurnAbilityActiveBonus.TriggerAbility();
			}
			m_CurrentTriggeredActiveBonus = null;
		}
		bool flag10 = false;
		if ((CurrentPhaseAbility != null) & overridingCurrentActor)
		{
			CActor actorBeingControlled = GameState.OverridenActionActorStack.Peek().ActorBeingControlled;
			if (actorBeingControlled == cActor && CurrentPhaseAbility?.TargetingActor != actorBeingControlled)
			{
				for (int num3 = GameState.OverridenActionActorStack.Count - 1; num3 >= 0; num3--)
				{
					GameState.EndOverrideCurrentActorForOneAction();
					flag10 = true;
					if (CurrentPhaseAbility.TargetingActor == GameState.InternalCurrentActor)
					{
						break;
					}
				}
			}
		}
		else if ((CurrentPhaseAbility == null && RemainingPhaseAbilities.Count == 0) & overridingCurrentActor)
		{
			for (int num4 = GameState.OverridenActionActorStack.Count - 1; num4 >= 0; num4--)
			{
				GameState.EndOverrideCurrentActorForOneAction();
				flag10 = true;
			}
		}
		currentPhaseAbility.m_PostEndAction?.Invoke();
		bool isLastAbility = CurrentPhaseAbility == null && m_PhaseAbilities.Count == 0;
		bool flag11 = m_CurrentPhaseAbilities.Count > 0 && (CurrentPhaseAbility.m_Ability.TargetingActor == null || ScenarioManager.Scenario.HasActor(CurrentPhaseAbility.m_Ability.TargetingActor) || CurrentPhaseAbility.m_Ability.ProcessIfDead);
		CEndActorAbilityAnimSync_MessageData cEndActorAbilityAnimSync_MessageData = new CEndActorAbilityAnimSync_MessageData(GameState.InternalCurrentActor);
		cEndActorAbilityAnimSync_MessageData.m_IsLastAbility = isLastAbility;
		cEndActorAbilityAnimSync_MessageData.m_Ability = LastPlayedAbility?.m_Ability;
		cEndActorAbilityAnimSync_MessageData.m_AdvanceGameState = !flag11;
		ScenarioRuleClient.MessageHandler(cEndActorAbilityAnimSync_MessageData);
		if (flag11)
		{
			if (m_CurrentItemID.HasValue && m_CurrentItemID != CurrentPhaseAbility?.ItemID)
			{
				m_CurrentItemID = null;
			}
			bool fullAbilityRestart = false;
			GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
			if (!CurrentPhaseAbility.m_Ability.HasStarted || (PreviousPhaseAbilities.Count > 0 && (!PreviousPhaseAbilities.Last().m_Ability.OnDeath || !CurrentPhaseAbility.m_Ability.OnDeath) && !PreviousPhaseAbilities.Last().m_Ability.StackedAttackEffectAbility && !CurrentPhaseAbility.m_Ability.AbilityHasHappened && !CurrentPhaseAbility.m_Ability.WasInterrupted))
			{
				if (m_FirstPhaseAbility != PreviousPhaseAbilities[0])
				{
					m_FirstPhaseAbility = PreviousPhaseAbilities[0];
					if (m_FirstPhaseAbility.m_Ability?.ParentAbility != null)
					{
						CPhaseAbility cPhaseAbility3 = PreviousPhaseAbilities.Concat(CurrentPhaseAbilities).LastOrDefault((CPhaseAbility x) => x.m_Ability == m_FirstPhaseAbility.m_Ability.ParentAbility);
						if (cPhaseAbility3 != null)
						{
							m_FirstPhaseAbility = cPhaseAbility3;
						}
					}
				}
				CurrentPhaseAbility.m_Ability.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.m_Ability.ControllingActor);
				fullAbilityRestart = true;
			}
			if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
			{
				RefreshConsumeBar();
			}
			Perform(fullAbilityRestart);
			return;
		}
		CPhaseAbility currentPhaseAbility2 = CurrentPhaseAbility;
		if ((currentPhaseAbility2 != null && currentPhaseAbility2.m_Ability?.SkipAnim == true) & flag)
		{
			CurrentPhaseAbility.m_Ability.AnimOverload = string.Empty;
			m_DontMoveAbilityState = true;
			GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
			CurrentPhaseAbility.m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(GameState.InternalCurrentActor)
			{
				WaitTickFrame = 10000,
				WaitActor = GameState.InternalCurrentActor,
				ClearEvents = false
			};
			ScenarioRuleClient.MessageHandler(message);
			return;
		}
		if (m_PhaseAbilities.Count > 0 && m_PhaseAbilities[0].m_Ability.SkipAnim && (ScenarioManager.Scenario.HasActor(m_PhaseAbilities[0].TargetingActor) || m_PhaseAbilities[0].m_Ability.ProcessIfDead))
		{
			m_DontMoveAbilityState = true;
			m_CurrentPhaseAbilities.Add(m_PhaseAbilities[0]);
			m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
			if (PreviousPhaseAbilities.Count > 0 && ((PreviousPhaseAbilities.Last() == m_FirstPhaseAbility && !m_HasAnyAbilityHappened) || (PreviousPhaseAbilities.Count > 0 && PreviousPhaseAbilities.Where((CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Null).Count() == PreviousPhaseAbilities.Count)))
			{
				m_FirstPhaseAbility = CurrentPhaseAbility;
			}
			if (CurrentPhaseAbility.TargetingActor != null && CurrentPhaseAbility.TargetingActor != GameState.InternalCurrentActor)
			{
				CActor targetingActor = CurrentPhaseAbility.TargetingActor;
				bool killAfterAction = CurrentPhaseAbility.KillAfterAction;
				GameState.OverrideCurrentActorForOneAction(targetingActor, null, killAfterAction);
			}
			if (flag)
			{
				if (CurrentPhaseAbility.m_Ability.AnimOverload != null && CurrentPhaseAbility.m_Ability.AnimOverload.Contains("+Skip"))
				{
					CurrentPhaseAbility.m_Ability.AnimOverload = "+Skip";
				}
				else
				{
					CurrentPhaseAbility.m_Ability.AnimOverload = string.Empty;
				}
			}
			else if (string.IsNullOrEmpty(CurrentPhaseAbility.m_Ability.AnimOverload))
			{
				CurrentPhaseAbility.m_Ability.AnimOverload = animOverload;
			}
			else if (CurrentPhaseAbility.m_Ability.AnimOverload.Contains("+Skip"))
			{
				CurrentPhaseAbility.m_Ability.AnimOverload = CurrentPhaseAbility.m_Ability.AnimOverload.Replace("+Skip", "");
			}
			GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
			CItem.EItemTrigger[] triggers = new CItem.EItemTrigger[2]
			{
				CItem.EItemTrigger.SingleTarget,
				CItem.EItemTrigger.SingleAbility
			};
			GameState.InternalCurrentActor.Inventory.HandleUsedItems(triggers);
			CurrentPhaseAbility.m_Ability.Start(GameState.CurrentActor, GameState.CurrentActor);
			CStartActorAbility_MessageData cStartActorAbility_MessageData = new CStartActorAbility_MessageData(GameState.InternalCurrentActor);
			cStartActorAbility_MessageData.m_IsFirstAbility = m_FirstPhaseAbility == CurrentPhaseAbility;
			cStartActorAbility_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			cStartActorAbility_MessageData.merged = false;
			if (cStartActorAbility_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.MergedMoveAttack)
			{
				cStartActorAbility_MessageData.merged = true;
			}
			ScenarioRuleClient.MessageHandler(cStartActorAbility_MessageData);
			CWaitForProgressChoreographer_MessageData message2 = new CWaitForProgressChoreographer_MessageData(GameState.InternalCurrentActor)
			{
				WaitTickFrame = 10000,
				WaitActor = GameState.InternalCurrentActor,
				ClearEvents = false
			};
			ScenarioRuleClient.MessageHandler(message2);
			return;
		}
		if (!flag10)
		{
			m_EndAbilitySynced = false;
		}
		else
		{
			m_DontMoveAbilityState = false;
		}
		for (int num5 = m_PhaseAbilities.Count - 1; num5 >= 0; num5--)
		{
			if (!m_PhaseAbilities[num5].m_Ability.ProcessIfDead && ((m_PhaseAbilities[num5].TargetingActor == null && GameState.InternalCurrentActor.IsDead) || (m_PhaseAbilities[num5].TargetingActor != null && m_PhaseAbilities[num5].TargetingActor.IsDead)))
			{
				m_PhaseAbilities.RemoveAt(num5);
			}
		}
		for (int num6 = m_CurrentPhaseAbilities.Count - 1; num6 >= 0; num6--)
		{
			if (!m_CurrentPhaseAbilities[num6].m_Ability.ProcessIfDead && ((m_CurrentPhaseAbilities[num6].TargetingActor == null && GameState.InternalCurrentActor.IsDead) || (m_CurrentPhaseAbilities[num6].TargetingActor != null && m_CurrentPhaseAbilities[num6].TargetingActor.IsDead)))
			{
				m_CurrentPhaseAbilities.RemoveAt(num6);
			}
		}
		isLastAbility = CurrentPhaseAbility == null && m_PhaseAbilities.Count == 0;
		if (isLastAbility && !m_HasAnyNonItemAbilityHappened)
		{
			ElementInfusionBoardManager.UndoInfuse(m_ElementsToInfuse, GameState.CurrentActor);
		}
		else
		{
			CActionAbilityHasHappened_MessageData cActionAbilityHasHappened_MessageData2 = new CActionAbilityHasHappened_MessageData(GameState.InternalCurrentActor);
			cActionAbilityHasHappened_MessageData2.PendingElementsToInfuse = ((m_ElementsToInfuse.Count > 0) & isLastAbility) && !overridingCurrentActor;
			ScenarioRuleClient.MessageHandler(cActionAbilityHasHappened_MessageData2);
		}
		if (isLastAbility)
		{
			ScenarioManager.CurrentScenarioState.UpdateElementsToInfuse();
		}
		if (CurrentPhaseAbility == null && GameState.InternalCurrentActor == GameState.TurnActor && GameState.TurnActor != null && GameState.TurnActor.IsDead)
		{
			GameState.EndTurnCheckNextActorAndMoveToNextPhase();
		}
	}

	public void AutoConsume()
	{
		for (int num = m_ActionAugmentationsToConsume.Count - 1; num >= 0; num--)
		{
			CActionAugmentation cActionAugmentation = m_ActionAugmentationsToConsume[num];
			cActionAugmentation.ConsumeConsumables(GameState.InternalCurrentActor);
			m_ActionAugmentationsConsumed.Add(cActionAugmentation);
			m_ActionAugmentationsToConsume.Remove(cActionAugmentation);
		}
	}

	public List<ElementInfusionBoardManager.EElement> GetReservedElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (m_CurrentActionID.HasValue && m_ActionAugmentationsToConsume != null && m_ActionAugmentationsToConsume.Count > 0)
		{
			foreach (CActionAugmentation item in m_ActionAugmentationsToConsume)
			{
				foreach (ElementInfusionBoardManager.EElement item2 in item.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public void EndAbilitySynchronise(bool undoItemAbility = false)
	{
		m_EndAbilitySynced = true;
		ScenarioRuleClient.MessageHandler(new CClearAllActorEvents_MessageData());
		bool flag = false;
		if (GameState.PlayedInfusion)
		{
			GameState.NextPhase();
			return;
		}
		if (m_CurrentPhaseAbilities.Count > 0 && (ScenarioManager.Scenario.HasActor(CurrentPhaseAbility.TargetingActor) || CurrentPhaseAbility.m_Ability.ProcessIfDead))
		{
			if (m_CurrentItemID.HasValue && m_CurrentItemID != CurrentPhaseAbility?.ItemID)
			{
				GameState.InternalCurrentActor.Inventory.SelectedItems.SingleOrDefault((CItem s) => s.ID == m_CurrentItemID.Value);
				if (!undoItemAbility)
				{
					_ = new CItem.EItemTrigger[2]
					{
						CItem.EItemTrigger.SingleTarget,
						CItem.EItemTrigger.SingleAbility
					};
				}
				m_CurrentItemID = null;
			}
			bool fullAbilityRestart = false;
			if (PreviousPhaseAbilities.Count > 0 && !PreviousPhaseAbilities.Last().m_Ability.OnDeath && !PreviousPhaseAbilities.Last().m_Ability.StackedAttackEffectAbility && !CurrentPhaseAbility.m_Ability.AbilityHasHappened)
			{
				if (m_FirstPhaseAbility != PreviousPhaseAbilities[0])
				{
					m_FirstPhaseAbility = PreviousPhaseAbilities[0];
				}
				if (CurrentPhaseAbility.m_Ability.HasStarted)
				{
					CurrentPhaseAbility.m_Ability.Restart();
				}
				fullAbilityRestart = true;
			}
			if (!CurrentPhaseAbility.m_Ability.HasStarted)
			{
				CurrentPhaseAbility.m_Ability.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor);
			}
			if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
			{
				CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
				{
					m_Action = m_CurrentAction.Action,
					m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			Perform(fullAbilityRestart);
		}
		else if (m_PhaseAbilities.Count > 0 && (ScenarioManager.Scenario.HasActor(m_PhaseAbilities[0].TargetingActor) || m_PhaseAbilities[0].m_Ability.ProcessIfDead))
		{
			AbilityData.MiscAbilityData miscAbilityData = m_PhaseAbilities[0].m_Ability.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.PerformXTimesBasedOn.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = m_PhaseAbilities[0].m_Ability.MiscAbilityData;
				if (miscAbilityData2 == null || miscAbilityData2.PerformXTimesBasedOn.Value != CAbility.EStatIsBasedOnXType.None)
				{
					List<CPhaseAbility> list = new List<CPhaseAbility>();
					CAbility.EStatIsBasedOnXType basedOnOverride = m_PhaseAbilities[0].m_Ability.MiscAbilityData?.PerformXTimesBasedOn.Value ?? CAbility.EStatIsBasedOnXType.None;
					int statIsBasedOnXValue = m_PhaseAbilities[0].m_Ability.GetStatIsBasedOnXValue(m_PhaseAbilities[0].TargetingActor, null, m_PhaseAbilities[0].m_Ability.AbilityFilter, basedOnOverride);
					if (statIsBasedOnXValue > 0)
					{
						for (int num = 0; num < statIsBasedOnXValue; num++)
						{
							CPhaseAbility cPhaseAbility = new CPhaseAbility(CAbility.CopyAbility(m_PhaseAbilities[0].m_Ability, generateNewID: false, fullCopy: true), m_PhaseAbilities[0].TargetingActor, m_CurrentAction.BaseCard, m_CurrentAction.Action.ID);
							cPhaseAbility.m_Ability.MiscAbilityData.PerformXTimesBasedOn = null;
							cPhaseAbility.m_Ability.AnimOverload = "None";
							list.Add(cPhaseAbility);
						}
						m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
						m_PhaseAbilities.InsertRange(0, list);
					}
					else
					{
						m_PhaseAbilities[0].m_Ability.SetCancelAbilityFlag(cancel: true);
						m_PhaseAbilities[0].m_Ability.MiscAbilityData.PerformXTimesBasedOn = null;
					}
				}
			}
			m_CurrentPhaseAbilities.Add(m_PhaseAbilities[0]);
			m_PhaseAbilities.Remove(m_PhaseAbilities[0]);
			if (!flag)
			{
				HandleRemainingPhaseAbilitiesFromEndAbilitySync();
			}
		}
		else
		{
			flag = true;
			if (!m_EndActionNonToggleActivesAdded && GameState.InternalCurrentActor.Class is CCharacterClass cCharacterClass)
			{
				foreach (CEndActionAbilityActiveBonus endActionActiveBonus in (from x in cCharacterClass.FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.EndActionAbility, GameState.InternalCurrentActor)
					where x is CEndActionAbilityActiveBonus
					select x).ToList())
				{
					if (!endActionActiveBonus.IsRestricted(GameState.InternalCurrentActor) && endActionActiveBonus.RequirementsMet() && endActionActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (endActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || m_PreviousPhaseAbilities.Any((CPhaseAbility x) => endActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Any((CAbility.EAbilityType y) => x.m_Ability.IsValidAbilityForActiveBonusOfType(y)))))
					{
						endActionActiveBonus.AddAbility = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: true, fullCopy: true);
						CPhaseAbility item = new CPhaseAbility(endActionActiveBonus.AddAbility, GameState.InternalCurrentActor, endActionActiveBonus.BaseCard, m_CurrentAction.Action.ID);
						m_CurrentPhaseAbilities.Add(item);
						endActionActiveBonus.TriggerAbility();
						m_HasAnyAbilityHappened = true;
						flag = false;
					}
				}
				m_EndActionNonToggleActivesAdded = true;
				if (!flag)
				{
					HandleRemainingPhaseAbilitiesFromEndAbilitySync();
				}
			}
		}
		if (flag)
		{
			if (ActorInitiator != null && GameState.TurnActor == ActorInitiator)
			{
				GameState.CurrentActor = ActorInitiator;
			}
			if (!undoItemAbility)
			{
				GameState.InternalCurrentActor.Inventory.HandleUsedItems();
			}
			else
			{
				GameState.InternalCurrentActor.Inventory.RefreshItems(CItem.EItemSlotState.Locked);
			}
			foreach (CActiveBonus pendingStartEndTurnAbilityBonusTrigger in GameState.PendingStartEndTurnAbilityBonusTriggers)
			{
				if (pendingStartEndTurnAbilityBonusTrigger is CStartRoundAbilityActiveBonus cStartRoundAbilityActiveBonus)
				{
					cStartRoundAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus)
				{
					cStartTurnAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CEndTurnAbilityActiveBonus cEndTurnAbilityActiveBonus)
				{
					cEndTurnAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CEndRoundAbilityActiveBonus cEndRoundAbilityActiveBonus)
				{
					cEndRoundAbilityActiveBonus.TriggerAbility();
				}
				else if (pendingStartEndTurnAbilityBonusTrigger is CDuringTurnAbilityActiveBonus cDuringTurnAbilityActiveBonus)
				{
					cDuringTurnAbilityActiveBonus.TriggerAbility();
				}
				if (pendingStartEndTurnAbilityBonusTrigger.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns) && GameState.OverridingCurrentActor)
				{
					GameState.EndOverrideCurrentActorForOneAction();
				}
			}
			GameState.PendingStartEndTurnAbilityBonusTriggers.Clear();
			GameState.InternalCurrentActor.LastActionHasHappened = PreviousPhaseAbilities.Any((CPhaseAbility x) => x.m_Ability.AbilityHasHappened && x.m_Ability.GetAction() != null);
			foreach (CActiveBonus item2 in CharacterClassManager.FindAllActiveBonuses().Distinct().ToList())
			{
				item2.OnActionEnded(GameState.InternalCurrentActor);
			}
			foreach (CScenarioModifier item3 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAction))
			{
				item3.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
			}
			if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ItemCard)
			{
				PhaseManager.EndItemAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase)
			{
				PhaseManager.EndAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.OverrideTurnAction)
			{
				PhaseManager.EndOverrideTurnAbilities();
			}
			else if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActiveBonus || GameState.CurrentActionInitiator == GameState.EActionInitiator.ScenarioModifier || GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
			{
				PhaseManager.EndActiveBonusOrScenarioModifierAbility();
			}
			else if (m_CurrentAction.BaseCard != null)
			{
				if (!m_ShownEndOfActionActiveBonuses && GameState.InternalCurrentActor.Type == CActor.EType.Player)
				{
					foreach (CActiveBonus item4 in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor).ToList())
					{
						if (item4.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.EndActionAbility) && item4.Ability.ActiveBonusData.IsToggleBonus && !item4.IsRestricted(GameState.InternalCurrentActor) && item4.RequirementsMet() && item4.CanToggleActiveBonus(GameState.InternalCurrentActor))
						{
							m_ShownEndOfActionActiveBonuses = true;
							CShowEndOfActionToggleBonuses_MessageData message2 = new CShowEndOfActionToggleBonuses_MessageData(GameState.InternalCurrentActor);
							ScenarioRuleClient.MessageHandler(message2);
							return;
						}
					}
				}
				CEndAction_MessageData cEndAction_MessageData = new CEndAction_MessageData(GameState.InternalCurrentActor);
				cEndAction_MessageData.m_ActionHappened = m_HasAnyNonItemAbilityHappened;
				cEndAction_MessageData.m_ActionName = m_CurrentAction.BaseCard.Name;
				cEndAction_MessageData.m_Card = m_CurrentAction.BaseCard;
				ScenarioRuleClient.MessageHandler(cEndAction_MessageData);
				if (m_HasAnyNonItemAbilityHappened)
				{
					GameState.InternalCurrentActor.GainXP(m_ActionConsumeXP, (m_CurrentAction.BaseCard.CardType == CBaseCard.ECardType.CharacterAbility) ? "AbilityCards" : null);
					if (m_ApplyActionXP)
					{
						GameState.InternalCurrentActor.GainXP(m_ActionXP, (m_CurrentAction.BaseCard.CardType == CBaseCard.ECardType.CharacterAbility) ? "AbilityCards" : null);
						m_ActionXP = 0;
					}
					foreach (ElementInfusionBoardManager.EElement item5 in m_ElementsToInfuse)
					{
						ElementInfusionBoardManager.Infuse(item5, GameState.InternalCurrentActor, endOfMonsterSet: false, asStrong: false, endOfPlayerAction: true);
					}
					GameState.InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Actions);
					if (GameState.InternalCurrentActor.ExhaustAfterAction)
					{
						bool actorWasAsleep = GameState.InternalCurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
						GameState.KillActor(GameState.InternalCurrentActor, GameState.InternalCurrentActor, CActor.ECauseOfDeath.Suicide, out var _, null, actorWasAsleep);
					}
				}
				if (CurrentPhaseAbility == null)
				{
					GameState.NextPhase();
				}
				else
				{
					HandleRemainingPhaseAbilitiesFromEndAbilitySync();
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionEndAbilitySynchronise, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability, flag));
		if (flag)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionEnded, (LastPlayedAbility?.m_BaseCard != null) ? LastPlayedAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (LastPlayedAbility?.m_Ability?.ID).GetValueOrDefault(), LastPlayedAbility?.TargetingActor, LastPlayedAbility?.m_Ability, flag));
		}
	}

	private void HandleRemainingPhaseAbilitiesFromEndAbilitySync(bool FullAbilityRestart = true)
	{
		if (CurrentPhaseAbility.TargetingActor != null && CurrentPhaseAbility.TargetingActor != GameState.InternalCurrentActor)
		{
			CActor targetingActor = CurrentPhaseAbility.TargetingActor;
			bool killAfterAction = CurrentPhaseAbility.KillAfterAction;
			GameState.OverrideCurrentActorForOneAction(targetingActor, null, killAfterAction);
		}
		GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
		CItem.EItemTrigger[] triggers = new CItem.EItemTrigger[2]
		{
			CItem.EItemTrigger.SingleTarget,
			CItem.EItemTrigger.SingleAbility
		};
		GameState.InternalCurrentActor.Inventory.HandleUsedItems(triggers);
		_ = CurrentPhaseAbility.m_Ability;
		CurrentPhaseAbility.m_Ability.Start(GameState.CurrentActor, GameState.CurrentActor);
		List<CPhaseAbility> list = m_PhaseAbilities.Concat(m_CurrentPhaseAbilities).ToList();
		if (!m_CheckedForStartActionSongs)
		{
			foreach (CPhaseAbility item in list)
			{
				item.m_Ability.ProcessSongOverridesAndAbilities(GameState.InternalCurrentActor, CSong.ESongActivationType.ActionStart);
			}
			m_CheckedForStartActionSongs = true;
		}
		if (!m_StartActionNonToggleActivesAdded && GameState.InternalCurrentActor.Class is CCharacterClass cCharacterClass)
		{
			List<CActiveBonus> source = cCharacterClass.FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.StartActionAbility, GameState.InternalCurrentActor);
			List<CAbility> list2 = new List<CAbility>();
			foreach (CStartActionAbilityActiveBonus startActionActiveBonus in source.Where((CActiveBonus x) => x is CStartActionAbilityActiveBonus))
			{
				if (!startActionActiveBonus.IsRestricted(GameState.InternalCurrentActor) && startActionActiveBonus.RequirementsMet() && startActionActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && (startActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || list.Any((CPhaseAbility x) => startActionActiveBonus.Ability.ActiveBonusData.ValidAbilityTypes.Any((CAbility.EAbilityType y) => x.m_Ability.IsValidAbilityForActiveBonusOfType(y)))))
				{
					startActionActiveBonus.AddAbility = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: true, fullCopy: true);
					list2.Add(startActionActiveBonus.AddAbility);
					startActionActiveBonus.TriggerAbility();
				}
			}
			if (list2.Count > 0)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list2, null, performNow: false, stopPlayerSkipping: false, null, stopPlayerUndo: false, null, ignorePerformNow: true);
			}
			m_StartActionNonToggleActivesAdded = true;
		}
		if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActiveBonus || GameState.CurrentActionInitiator == GameState.EActionInitiator.ScenarioModifier || GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
		{
			CurrentPhaseAbility.m_Ability.SetCanUndo(canUndo: false);
		}
		if (m_CurrentAction.BaseCard is CAbilityCard && m_CurrentAction.Action.Augmentations != null && m_CurrentAction.Action.Augmentations.Count > 0)
		{
			CShowAugmentationBar_MessageData message = new CShowAugmentationBar_MessageData(GameState.InternalCurrentActor)
			{
				m_Action = m_CurrentAction.Action,
				m_Ability = ((CurrentPhaseAbility?.m_Ability != null) ? CurrentPhaseAbility.m_Ability : m_CurrentAction.Action.Abilities[0])
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		Perform(FullAbilityRestart);
	}

	public bool AbilityHappened()
	{
		if (m_CurrentActionID.HasValue && m_HasAnyAbilityHappened)
		{
			return true;
		}
		return false;
	}

	protected override void OnTileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnTileSelected, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.TileSelected(selectedTile, optionalTileList);
		}
		ScenarioRuleClient.MessageHandler(new CTileSelectionFinished_MessageData());
	}

	protected override void OnTileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnTileDeselected, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.TileDeselected(selectedTile, optionalTileList);
		}
		ScenarioRuleClient.MessageHandler(new CTileSelectionFinished_MessageData());
	}

	protected override void OnApplySingleTarget(CActor actor)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnApplySingleTarget, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (CurrentPhaseAbility == null)
		{
			return;
		}
		CAbility cAbility = CurrentPhaseAbility.m_Ability;
		if (cAbility is CAbilityMerged cAbilityMerged)
		{
			cAbility = cAbilityMerged.ActiveAbility;
		}
		if (!cAbility.IsWaitingForSingleTargetItem() || cAbility.ApplySingleTargetItem(actor))
		{
			if (cAbility.IsWaitingForSingleTargetActiveBonus())
			{
				cAbility.ApplySingleTargetActiveBonus(actor);
			}
			CurrentPhaseAbility.m_Ability.Perform();
		}
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		bool fullAbilityRestart = false;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionOnStepComplete, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (passingStep && CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.AbilityPassStep();
		}
		if (CurrentPhaseAbility?.m_Ability != null && !CurrentPhaseAbility.m_Ability.HasStarted)
		{
			CurrentPhaseAbility.m_Ability.Start(CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.TargetingActor, CurrentPhaseAbility.m_Ability.ControllingActor);
			Perform(fullAbilityRestart: true);
		}
		else if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
			Perform(fullAbilityRestart: false);
		}
		else if (CurrentPhaseAbility == null || CurrentPhaseAbility.m_Ability.AbilityComplete(m_DontMoveAbilityState || (CurrentPhaseAbility.m_Ability.DontMoveAbilityState.HasValue && CurrentPhaseAbility.m_Ability.DontMoveAbilityState.Value), out fullAbilityRestart))
		{
			m_DontMoveAbilityState = false;
			NextStep();
		}
		else
		{
			m_DontMoveAbilityState = false;
			Perform(fullAbilityRestart);
		}
	}

	public void InjectAbility(CAbility ability, CActor targetingActor, CBaseCard baseAbilityCard, Action endAction = null, CItem item = null)
	{
		m_PhaseAbilities.Insert(0, new CPhaseAbility(ability, targetingActor, baseAbilityCard, null, item?.ID, endAction));
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInjectAbility, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void InjectAbilities(List<CAbility> abilities, CActor targetingActor)
	{
		for (int num = abilities.Count - 1; num >= 0; num--)
		{
			CAbility cAbility = abilities[num];
			InjectAbility(cAbility, targetingActor, cAbility.GetAbilityBaseCard());
		}
	}

	public void InjectXP(int XP)
	{
		m_ActionXP += XP;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInjectXP, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void InjectInfusions(List<ElementInfusionBoardManager.EElement> elements)
	{
		m_ElementsToInfuse.AddRange(elements);
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionInjectInfusions, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void StackItemAbilities(CItem item)
	{
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Count > 0)
		{
			CurrentPhaseAbility?.m_Ability?.InterruptAbility();
			for (int num = item.YMLData.Data.Abilities.Count - 1; num >= 0; num--)
			{
				m_CurrentPhaseAbilities.Add(new CPhaseAbility(CAbility.CopyAbility(item.YMLData.Data.Abilities[num], generateNewID: false), GameState.InternalCurrentActor, item, null, item.ID));
			}
			m_CurrentItemID = item.ID;
			for (int i = 0; i < item.YMLData.Data.Abilities.Count; i++)
			{
				m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1 - i].m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackItemAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
			Perform(fullAbilityRestart: true);
		}
		else
		{
			Log.ErrorWithReport("Attempting to stack a null ability");
		}
	}

	public void StackInlineSubAbilities(List<CAbility> inlineSubAbilities, CActor controllingActor = null, bool performNow = false, bool stopPlayerSkipping = false, bool? dontMoveAbilityState = null, bool stopPlayerUndo = false, CActiveBonus fromActiveBonus = null, bool ignorePerformNow = false, CActor overrideActor = null)
	{
		CActor targetingActor = ((overrideActor == null) ? GameState.InternalCurrentActor : overrideActor);
		if (!m_EndAbilitySynced)
		{
			ignorePerformNow = true;
		}
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.InterruptAbility();
		}
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		int num = 0;
		for (int num2 = inlineSubAbilities.Count - 1; num2 >= 0; num2--)
		{
			CAbility cAbility = CAbility.CopyAbility(inlineSubAbilities[num2], generateNewID: false, fullCopy: true);
			if (cAbility.SubAbilities != null)
			{
				foreach (CAbility subAbility in cAbility.SubAbilities)
				{
					if (!subAbility.IsInlineSubAbility)
					{
						subAbility.ParentAbility = cAbility;
						if (cAbility.StartAbilityRequirements == null || cAbility.StartAbilityRequirements.MeetsAbilityRequirements(GameState.InternalCurrentActor, subAbility))
						{
							m_CurrentPhaseAbilities.Add(new CPhaseAbility(subAbility, targetingActor, (m_CurrentPhaseAbilities.Count > 0) ? m_CurrentPhaseAbilities[0].m_BaseCard : m_PreviousPhaseAbilities.Last().m_BaseCard, (CurrentPhaseAbility != null) ? CurrentPhaseAbility.ActionID : m_PreviousPhaseAbilities.Last().ActionID));
							num++;
						}
					}
				}
			}
			CPhaseAbility cPhaseAbility = null;
			if (m_CurrentPhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_CurrentPhaseAbilities[0];
			}
			else if (m_PreviousPhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_PreviousPhaseAbilities[0];
			}
			else if (m_PhaseAbilities.Count > 0)
			{
				cPhaseAbility = m_PhaseAbilities[0];
			}
			m_CurrentPhaseAbilities.Add(new CPhaseAbility(cAbility, targetingActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
			num++;
		}
		CAbility ability = m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1].m_Ability;
		List<CAbility> list = new List<CAbility>();
		for (int i = 0; i < num; i++)
		{
			CAbility ability2 = m_CurrentPhaseAbilities[m_CurrentPhaseAbilities.Count - 1 - i].m_Ability;
			list.Add(ability2);
		}
		if (fromActiveBonus != null)
		{
			m_CurrentTriggeredActiveBonus = fromActiveBonus;
		}
		if (!ignorePerformNow && CurrentPhaseAbility?.m_Ability == ability)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		}
		foreach (CAbility item in list)
		{
			item.ControllingActor = controllingActor;
			if (stopPlayerSkipping)
			{
				item.SetCanSkip(canSkip: false);
			}
			if (stopPlayerUndo)
			{
				item.SetCanUndo(canUndo: false);
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackInlineSubAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
		if (list.Count > 0)
		{
			CEndActorAbilityAnimSync_MessageData cEndActorAbilityAnimSync_MessageData = new CEndActorAbilityAnimSync_MessageData(GameState.InternalCurrentActor);
			cEndActorAbilityAnimSync_MessageData.m_IsLastAbility = false;
			cEndActorAbilityAnimSync_MessageData.m_Ability = LastPlayedAbility?.m_Ability;
			ScenarioRuleClient.MessageHandler(cEndActorAbilityAnimSync_MessageData);
			m_HasAnyAbilityHappened = true;
		}
		CStackedInlineSubAbilities_MessageData message2 = new CStackedInlineSubAbilities_MessageData
		{
			m_Ability = CurrentPhaseAbility?.m_Ability
		};
		ScenarioRuleClient.MessageHandler(message2);
	}

	public void StackNextAbilities(List<CAbility> nextAbilities, CActor targetingActor, bool killAfter = false, bool stackToNextCurrent = false, bool copyCurrentActionID = false, bool interruptCurrent = false, bool addDirectlyToCurrentAbilities = false, bool forcedInterrupt = false, bool interruptRegardlessOfStrength = false, bool dontCreateCopies = false, CItem item = null, Guid? actionID = null)
	{
		bool flag = false;
		if (nextAbilities != null && nextAbilities.Count > 0)
		{
			CAbility cAbility = nextAbilities[0];
			if (cAbility != null)
			{
				TValue miscVar = cAbility.GetMiscVar("InterruptCurrentAbility");
				if (!miscVar.isUnknown)
				{
					interruptCurrent = miscVar.GetBool();
				}
				TValue miscVar2 = cAbility.GetMiscVar("StackNextToCurrent");
				if (!miscVar2.isUnknown)
				{
					stackToNextCurrent = miscVar2.GetBool();
				}
				TValue miscVar3 = cAbility.GetMiscVar("ApplyImmediately");
				if (!miscVar3.isUnknown)
				{
					flag = miscVar3.GetBool();
				}
				TValue miscVar4 = cAbility.GetMiscVar("InterruptRegardlessOfStrength");
				if (!miscVar4.isUnknown)
				{
					interruptRegardlessOfStrength = miscVar4.GetBool();
				}
			}
		}
		CPhaseAbility currentPhaseAbility = CurrentPhaseAbility;
		int num = 0;
		if (CurrentPhaseAbility != null)
		{
			num = m_CurrentPhaseAbilities.IndexOf(CurrentPhaseAbility);
		}
		CAbility cAbility2 = null;
		if (interruptCurrent && currentPhaseAbility?.m_Ability != null && !currentPhaseAbility.m_Ability.AbilityHasBeenCancelled && !addDirectlyToCurrentAbilities)
		{
			cAbility2 = currentPhaseAbility.m_Ability.CreateInterruptCopy(targetingActor, killAfter, forcedInterrupt, interruptRegardlessOfStrength);
			if (cAbility2 == null)
			{
				interruptCurrent = false;
			}
			else
			{
				currentPhaseAbility.m_Ability.SetCancelAbilityFlag(cancel: true);
			}
		}
		else if (currentPhaseAbility == null)
		{
			interruptCurrent = false;
		}
		foreach (CAbility nextAbility in nextAbilities)
		{
			if (flag)
			{
				CAbility cAbility3 = CAbility.CopyAbility(nextAbility, generateNewID: false);
				cAbility3.Start(targetingActor, targetingActor);
				cAbility3.ApplyToActor(targetingActor);
				continue;
			}
			bool killAfter2;
			if (addDirectlyToCurrentAbilities)
			{
				CBaseCard abilityBaseCard = nextAbility.GetAbilityBaseCard();
				killAfter2 = killAfter;
				CPhaseAbility cPhaseAbility = new CPhaseAbility(nextAbility, targetingActor, abilityBaseCard, actionID, item?.ID, null, killAfter2);
				cPhaseAbility.m_Ability.Start(targetingActor, targetingActor);
				m_CurrentPhaseAbilities.Add(cPhaseAbility);
				continue;
			}
			if (interruptCurrent && CurrentPhaseAbility != null)
			{
				CAbility cAbility4 = nextAbility;
				if (!dontCreateCopies)
				{
					cAbility4 = CAbility.CopyAbility(nextAbility, generateNewID: false, fullCopy: true);
				}
				CAbility ability = cAbility4;
				CBaseCard abilityBaseCard2 = nextAbility.GetAbilityBaseCard();
				killAfter2 = killAfter;
				CPhaseAbility cPhaseAbility2 = new CPhaseAbility(ability, targetingActor, abilityBaseCard2, actionID, item?.ID, null, killAfter2);
				if (copyCurrentActionID)
				{
					cPhaseAbility2.ActionID = CurrentPhaseAbility.ActionID;
				}
				m_CurrentPhaseAbilities.Insert(num, cPhaseAbility2);
				cPhaseAbility2.m_Ability.TargetingActor = targetingActor;
				cPhaseAbility2.m_Ability.FilterActor = targetingActor;
				continue;
			}
			if (stackToNextCurrent && CurrentPhaseAbility != null)
			{
				CAbility cAbility5 = nextAbility;
				if (!dontCreateCopies)
				{
					cAbility5 = CAbility.CopyAbility(nextAbility, generateNewID: false, fullCopy: true);
				}
				CAbility ability2 = cAbility5;
				CBaseCard abilityBaseCard3 = nextAbility.GetAbilityBaseCard(targetingActor);
				killAfter2 = killAfter;
				CPhaseAbility cPhaseAbility3 = new CPhaseAbility(ability2, targetingActor, abilityBaseCard3, actionID, item?.ID, null, killAfter2);
				if (copyCurrentActionID)
				{
					cPhaseAbility3.ActionID = CurrentPhaseAbility.ActionID;
				}
				m_CurrentPhaseAbilities.Insert(num, cPhaseAbility3);
				cPhaseAbility3.m_Ability.TargetingActor = targetingActor;
				cPhaseAbility3.m_Ability.FilterActor = targetingActor;
				continue;
			}
			CAbility cAbility6 = nextAbility;
			if (!dontCreateCopies)
			{
				cAbility6 = CAbility.CopyAbility(nextAbility, generateNewID: false, fullCopy: true);
			}
			CAbility ability3 = cAbility6;
			CBaseCard abilityBaseCard4 = nextAbility.GetAbilityBaseCard(targetingActor);
			killAfter2 = killAfter;
			CPhaseAbility cPhaseAbility4 = new CPhaseAbility(ability3, targetingActor, abilityBaseCard4, actionID, item?.ID, null, killAfter2);
			if (copyCurrentActionID)
			{
				cPhaseAbility4.ActionID = CurrentPhaseAbility.ActionID;
			}
			if (stackToNextCurrent)
			{
				cPhaseAbility4.m_Ability.TargetingActor = targetingActor;
				cPhaseAbility4.m_Ability.FilterActor = targetingActor;
				m_CurrentPhaseAbilities.Add(cPhaseAbility4);
			}
			else if (interruptCurrent)
			{
				m_PhaseAbilities.Insert(0, cPhaseAbility4);
			}
			else
			{
				m_PhaseAbilities.Add(cPhaseAbility4);
			}
		}
		if (cAbility2 != null)
		{
			List<CPhaseAbility> currentPhaseAbilities = m_CurrentPhaseAbilities;
			int index = num;
			CAbility ability4 = cAbility2;
			CActor targetingActor2 = cAbility2.TargetingActor;
			CBaseCard abilityBaseCard5 = cAbility2.GetAbilityBaseCard();
			bool killAfter2 = killAfter;
			currentPhaseAbilities.Insert(index, new CPhaseAbility(ability4, targetingActor2, abilityBaseCard5, null, null, null, killAfter2));
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionStackNextAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
	}

	public void UnstackItemAbilities(CItem item)
	{
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Count > 0)
		{
			foreach (CPhaseAbility item2 in m_CurrentPhaseAbilities.Where((CPhaseAbility w) => w.ItemID == item.ID).ToList())
			{
				COnItemAbilityUnstacked_MessageData message = new COnItemAbilityUnstacked_MessageData(GameState.InternalCurrentActor)
				{
					m_ItemAbility = item2.m_Ability,
					m_Item = item
				};
				ScenarioRuleClient.MessageHandler(message);
				m_CurrentPhaseAbilities.Remove(item2);
			}
			m_CurrentItemID = CurrentPhaseAbility?.ItemID;
			SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionUnstackItemAbilities, (CurrentPhaseAbility?.m_BaseCard != null) ? CurrentPhaseAbility.m_BaseCard.Name : "", GameState.CurrentActionSelectedAugmentations, (CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), CurrentPhaseAbility?.TargetingActor, CurrentPhaseAbility?.m_Ability));
			Perform(fullAbilityRestart: false);
		}
		else
		{
			Log.ErrorWithReport("Attempting to unstack a null ability");
		}
	}

	public void UnstackAbility(CAbility ability, CActiveBonus fromActiveBonus = null)
	{
		foreach (CPhaseAbility item in m_CurrentPhaseAbilities.Where((CPhaseAbility w) => w.m_Ability.ID == ability.ID || (w.m_Ability.ParentAbility != null && w.m_Ability.ParentAbility.ID == ability.ID && !w.m_Ability.AbilityHasHappened)).ToList())
		{
			if (item.m_Ability.HasStarted)
			{
				item.m_Ability.AbilityEnded();
			}
			m_CurrentPhaseAbilities.Remove(item);
		}
		foreach (CPhaseAbility item2 in m_PhaseAbilities.Where((CPhaseAbility w) => w.m_Ability.ID == ability.ID || (w.m_Ability.ParentAbility != null && w.m_Ability.ParentAbility.ID == ability.ID && !w.m_Ability.AbilityHasHappened)).ToList())
		{
			m_PhaseAbilities.Remove(item2);
		}
		if (fromActiveBonus != null && m_CurrentTriggeredActiveBonus == fromActiveBonus)
		{
			m_CurrentTriggeredActiveBonus = null;
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData(isRestart: true));
		if (CurrentPhaseAbility != null)
		{
			CurrentPhaseAbility.m_Ability.Restart();
			CStartActorAbility_MessageData cStartActorAbility_MessageData = new CStartActorAbility_MessageData(GameState.InternalCurrentActor);
			cStartActorAbility_MessageData.m_IsFirstAbility = m_FirstPhaseAbility == CurrentPhaseAbility;
			cStartActorAbility_MessageData.m_Ability = CurrentPhaseAbility.m_Ability;
			cStartActorAbility_MessageData.merged = false;
			cStartActorAbility_MessageData.m_FromUnstack = true;
			if (cStartActorAbility_MessageData.m_Ability.AbilityType == CAbility.EAbilityType.MergedMoveAttack)
			{
				cStartActorAbility_MessageData.merged = true;
			}
			ScenarioRuleClient.MessageHandler(cStartActorAbility_MessageData);
			CurrentPhaseAbility.m_Ability.Perform();
		}
		else
		{
			EndAbilitySynchronise();
		}
	}

	public void FinalizeOverrideAbilityType(CAbility overridingAbility, CAbilityOverride abilityOverride = null)
	{
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.Find((CPhaseAbility x) => x.m_Ability.ID == overridingAbility.ID);
		if (cPhaseAbility == null)
		{
			return;
		}
		int index = m_CurrentPhaseAbilities.IndexOf(cPhaseAbility);
		CAbility cAbility = CAbility.CopyAbility(overridingAbility, generateNewID: false, fullCopy: true);
		m_CurrentPhaseAbilities.Insert(index, new CPhaseAbility(cAbility, GameState.InternalCurrentActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
		m_CurrentPhaseAbilities.Remove(cPhaseAbility);
		cAbility.AllTargets = false;
		int numberTargets = (cAbility.OriginalTargetCount = overridingAbility.OriginalTargetCount);
		cAbility.NumberTargets = numberTargets;
		if (m_FirstPhaseAbility == cPhaseAbility)
		{
			m_FirstPhaseAbility = m_CurrentPhaseAbilities[index];
		}
		if (abilityOverride != null)
		{
			if (abilityOverride.NumberOfTargets.HasValue && abilityOverride.TargetIsBase.HasValue && abilityOverride.TargetIsBase.Value)
			{
				numberTargets = (cAbility.OriginalTargetCount = overridingAbility.NumberTargets);
				cAbility.NumberTargets = numberTargets;
			}
			CurrentPhaseAbility.m_Ability.CurrentOverrides.Add(abilityOverride);
		}
		GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
		foreach (CConditionalOverride conditionalOverride in overridingAbility.ActiveConditionalOverrides)
		{
			cAbility.ConditionalOverrides.RemoveAll((CConditionalOverride x) => x.Name.Equals(conditionalOverride.Name, StringComparison.InvariantCultureIgnoreCase));
		}
		cAbility.Start(GameState.CurrentActor, GameState.CurrentActor);
		Perform(fullAbilityRestart: false);
		if (GameState.InternalCurrentActor is CPlayerActor { Inventory: var inventory })
		{
			inventory.HighlightUsableItems(CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
		}
		CFinalizedOverrideAbilityType cFinalizedOverrideAbilityType = new CFinalizedOverrideAbilityType(CurrentPhaseAbility?.TargetingActor);
		cFinalizedOverrideAbilityType.m_Ability = CurrentPhaseAbility?.m_Ability;
		ScenarioRuleClient.MessageHandler(cFinalizedOverrideAbilityType);
	}

	public void SwapAbility(CAbility abilityToRemove, CAbility abilityToAdd, bool canUndo = true)
	{
		CPhaseAbility cPhaseAbility = m_CurrentPhaseAbilities.Find((CPhaseAbility x) => x.m_Ability == abilityToRemove);
		int num = m_CurrentPhaseAbilities.IndexOf(cPhaseAbility);
		m_CurrentPhaseAbilities.Insert(num, new CPhaseAbility(abilityToAdd, GameState.InternalCurrentActor, cPhaseAbility.m_BaseCard, cPhaseAbility.ActionID));
		m_CurrentPhaseAbilities.Remove(cPhaseAbility);
		if (m_FirstPhaseAbility == cPhaseAbility)
		{
			m_FirstPhaseAbility = m_CurrentPhaseAbilities[num];
		}
		CurrentPhaseAbility.m_Ability.SetCanUndo(canUndo);
		if (num == m_CurrentPhaseAbilities.Count - 1)
		{
			GameState.CurrentActor = CurrentPhaseAbility.TargetingActor;
			CurrentPhaseAbility.m_Ability.Start(GameState.InternalCurrentActor, GameState.InternalCurrentActor);
			Perform(fullAbilityRestart: true);
		}
	}

	public bool HasConsumedActionAugmentation(CActionAugmentation augment)
	{
		return m_ActionAugmentationsConsumed.Exists((CActionAugmentation it) => it.ActionID == augment.ActionID && it.Name == augment.Name);
	}

	public static void UpdateDamageInflictedThisAction(int damageInflictedThisAction, CAbility.EAbilityType damageSourceAbilityType)
	{
		s_DamageInflictedThisAction += damageInflictedThisAction;
		if (damageSourceAbilityType != CAbility.EAbilityType.None)
		{
			if (!s_DamageInflictedByAbilityTypeThisAction.ContainsKey(damageSourceAbilityType))
			{
				s_DamageInflictedByAbilityTypeThisAction[damageSourceAbilityType] = 0;
			}
			s_DamageInflictedByAbilityTypeThisAction[damageSourceAbilityType] += damageInflictedThisAction;
		}
	}

	public static void UpdateDamagePreventedThisAction(int damagePreventedThisAction)
	{
		s_DamagePreventedThisAction += damagePreventedThisAction;
	}

	public static void UpdateHexesMovedThisAction(int hexesMovedThisAction)
	{
		s_HexesMovedThisAction += hexesMovedThisAction;
	}

	public static void UpdateObstaclesDestroyedThisAction(int obstaclesDestroyedThisAction)
	{
		s_ObstaclesDestroyedThisAction += obstaclesDestroyedThisAction;
	}

	public static void UpdateTargetsDamagedInPrevAttackThisAction(CAbilityAttack abilityAttack)
	{
		if (s_TargetsDamagedInPrevAttackThisActionAbilityID != abilityAttack.ID)
		{
			s_TargetsDamagedInPrevAttackThisActionAbilityID = abilityAttack.ID;
			s_TargetsDamagedInPrevAttackThisAction = 0;
			s_TargetsListDamagedInPrevAttackThisAction.Clear();
		}
		s_TargetsDamagedInPrevAttackThisAction++;
		foreach (CActor item in abilityAttack.ActorsToTarget)
		{
			if (!s_TargetsListDamagedInPrevAttackThisAction.Contains(item))
			{
				s_TargetsListDamagedInPrevAttackThisAction.Add(item);
			}
		}
	}

	public static void UpdateTargetsActuallyDamagedInPrevAttackThisAction(CAbilityAttack abilityAttack)
	{
		if (s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID != abilityAttack.ID)
		{
			s_TargetsActuallyDamagedInPrevAttackThisActionAbilityID = abilityAttack.ID;
			s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		}
		s_TargetsActuallyDamagedInPrevAttackThisAction++;
	}

	public static void UpdateTargetsDamagedInPrevDamageAbilityThisAction(CAbilityDamage abilityDamage)
	{
		if (s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID != abilityDamage.ID)
		{
			s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = abilityDamage.ID;
			s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		}
		s_TargetsDamagedInPrevDamageAbilityThisAction++;
	}

	public static void UpdateActorsHealedThisAction(CActor actorHealed)
	{
		s_ActorsHealedThisAction.Add(actorHealed);
	}

	public static void Reset()
	{
		s_DamageInflictedByAbilityTypeThisAction.Clear();
		s_DamageInflictedThisAction = 0;
		s_DamagePreventedThisAction = 0;
		s_HexesMovedThisAction = 0;
		s_ObstaclesDestroyedThisAction = 0;
		s_TargetsDamagedInPrevAttackThisAction = 0;
		s_TargetsListDamagedInPrevAttackThisAction.Clear();
		s_TargetsActuallyDamagedInPrevAttackThisAction = 0;
		s_TargetsDamagedInPrevAttackThisActionAbilityID = Guid.Empty;
		s_TargetsDamagedInPrevDamageAbilityThisAction = 0;
		s_TargetsDamagedInPrevDamageAbilityThisActionAbilityID = Guid.Empty;
		s_ActorsHealedThisAction.Clear();
	}

	public static void LogActionPerform(bool clearElements = false)
	{
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (currentPhaseAction == null)
		{
			return;
		}
		List<CActionAugmentation> list = GameState.CurrentActionSelectedAugmentations?.ToList();
		if (list != null && currentPhaseAction?.CurrentPhaseAbility?.m_Ability != null)
		{
			list.RemoveAll((CActionAugmentation x) => x.AugmentationOps.Count > 0 && !x.AugmentationOps.Any((CActionAugmentationOp op) => op.ParentAbilityName.Equals(currentPhaseAction.CurrentPhaseAbility.m_Ability.Name)));
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAction(ESESubTypeAction.ActionPerform, (currentPhaseAction?.CurrentPhaseAbility?.m_BaseCard != null) ? currentPhaseAction.CurrentPhaseAbility.m_BaseCard.Name : "", list, (currentPhaseAction?.CurrentPhaseAbility?.m_Ability?.ID).GetValueOrDefault(), currentPhaseAction?.CurrentPhaseAbility?.TargetingActor, currentPhaseAction?.CurrentPhaseAbility?.m_Ability));
		if (((list != null) & clearElements) && list.Count > 0)
		{
			GameState.CurrentActionSelectedAugmentations.Clear();
		}
	}
}
