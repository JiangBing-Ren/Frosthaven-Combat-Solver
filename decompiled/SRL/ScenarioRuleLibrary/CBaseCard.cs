using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CBaseCard : ISerializable, IDumpState
{
	[Serializable]
	public enum ActionType
	{
		NA,
		TopAction,
		DefaultAttackAction,
		DefaultMoveAction,
		BottomAction
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ECardType
	{
		None,
		CharacterAbility,
		HeroSummonAbility,
		MonsterAbility,
		Item,
		AttackModifier,
		ScenarioModifier,
		Dummy,
		Pet
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ECardPile
	{
		None,
		Discarded,
		Round,
		Hand,
		Activated,
		Lost,
		PermanentlyLost,
		SelectedAbilityCardsOutOfPhase,
		Taken,
		OwnedUnused
	}

	public static readonly ActionType[] ActionTypes = (ActionType[])Enum.GetValues(typeof(ActionType));

	private string m_StringID;

	public string Name
	{
		get
		{
			try
			{
				switch (CardType)
				{
				case ECardType.CharacterAbility:
					return ScenarioRuleClient.SRLYML.AbilityCards.Single((AbilityCardYMLData s) => s.ID == ID).Name;
				case ECardType.HeroSummonAbility:
					return ScenarioRuleClient.SRLYML.HeroSummons.Single((HeroSummonYMLData s) => s.ID == m_StringID).LocKey;
				case ECardType.MonsterAbility:
				{
					string name = ScenarioRuleClient.SRLYML.MonsterCards.Single((MonsterCardYMLData s) => s.ID == ID).Name;
					return string.IsNullOrEmpty(name) ? ((this as CBaseAbilityCard).ClassID + ID) : name;
				}
				case ECardType.Item:
					return ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == ID).Name;
				case ECardType.Pet:
					return ScenarioRuleClient.SRLYML.PetCards.Single((ItemCardYMLData s) => s.ID == ID).Name;
				case ECardType.AttackModifier:
					return m_StringID;
				case ECardType.ScenarioModifier:
					return m_StringID;
				case ECardType.Dummy:
					return m_StringID;
				default:
					Log.ErrorWithReport("Invalid card type set for base card.");
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Invalid BaseCard ID.  ID = " + ID + " CardType = " + CardType.ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
				return string.Empty;
			}
		}
	}

	public int ID { get; set; }

	public ECardType CardType { get; private set; }

	public List<CActiveBonus> ActiveBonuses { get; private set; }

	public ECardPile CurrentCardPile { get; set; }

	public ECardPile PendingCardPile { get; set; }

	public bool IsItemOrPet
	{
		get
		{
			if (CardType != ECardType.Item)
			{
				return CardType == ECardType.Pet;
			}
			return true;
		}
	}

	public bool ActionHasHappened { get; set; }

	public string StrictName => Name.Replace("ABILITY_CARD_", "");

	public IEnumerable<CActiveBonus> GetActiveBonusesForActor(CActor actor)
	{
		lock (ActiveBonuses)
		{
			foreach (CActiveBonus activeBonus in ActiveBonuses)
			{
				if (actor == null)
				{
					yield return activeBonus;
				}
				else if (!activeBonus.IsAura && activeBonus.Actor == actor)
				{
					yield return activeBonus;
				}
				else if (activeBonus.IsAura && activeBonus.ValidActorsInRangeOfAura.Contains(actor))
				{
					yield return activeBonus;
				}
				else if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
				{
					if ((activeBonus.Actor as CHeroSummonActor)?.Summoner == actor)
					{
						yield return activeBonus;
					}
					if ((activeBonus.Actor as CEnemyActor)?.PlayerSummoner == actor)
					{
						yield return activeBonus;
					}
				}
			}
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("m_StringID", m_StringID);
		info.AddValue("CardType", CardType);
		info.AddValue("CurrentCardPile", CurrentCardPile);
		info.AddValue("ActionHasHappened", ActionHasHappened);
	}

	protected CBaseCard(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ID":
					ID = info.GetInt32("ID");
					break;
				case "m_StringID":
					m_StringID = info.GetString("m_StringID");
					break;
				case "CardType":
					CardType = (ECardType)info.GetValue("CardType", typeof(ECardType));
					break;
				case "CurrentCardPile":
					CurrentCardPile = (ECardPile)info.GetValue("CurrentCardPile", typeof(ECardPile));
					break;
				case "ActionHasHappened":
					ActionHasHappened = info.GetBoolean("ActionHasHappened");
					break;
				case "Name":
					m_StringID = info.GetString("Name");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CBaseCard entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		ActiveBonuses = new List<CActiveBonus>();
	}

	public CBaseCard(int id, ECardType cardType, string stringID)
	{
		ID = id;
		m_StringID = stringID;
		CardType = cardType;
		ActiveBonuses = new List<CActiveBonus>();
		CurrentCardPile = ECardPile.None;
		ActionHasHappened = false;
	}

	public override string ToString()
	{
		return "[" + MF.RefStr(this) + "] " + GetType().Name + "(" + Name + ")";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return GetType().Name + "(" + Name + ")";
	}

	public static CBaseCard CreateDummy(string stringID = null, int cardID = 0, CAbility forAbility = null, CActor forActor = null)
	{
		if (forAbility?.CachedAbilityBaseCard != null && forAbility.CachedAbilityBaseCard.CardType == ECardType.Dummy)
		{
			return forAbility.CachedAbilityBaseCard;
		}
		if (string.IsNullOrEmpty(stringID))
		{
			stringID = "DummyCardID";
		}
		string arg;
		if (cardID == 0)
		{
			if (GameState.ProtectLastDummyCardID)
			{
				Log.Error("Decrementing protected ProtectLastDummyCardID");
			}
			cardID = --GameState.LastDummyCardID;
			arg = "new";
		}
		else if (cardID > 0)
		{
			Log.Error($"Creating dummy card with positive ID: {cardID}");
			arg = "positive";
		}
		else if (cardID < GameState.LastDummyCardID)
		{
			arg = $"existing (< {GameState.LastDummyCardID})";
			if (GameState.ProtectLastDummyCardID)
			{
				Log.Error("Restoring protected ProtectLastDummyCardID");
			}
			GameState.LastDummyCardID = cardID;
		}
		else
		{
			arg = $"existing (>= {GameState.LastDummyCardID})";
		}
		string text = $"Creating dummy card with {arg} ID: {cardID}";
		if (forAbility != null)
		{
			text += $"\nfor ability: {forAbility}";
		}
		if (forActor != null)
		{
			text += $"\nfor actor: {forActor}";
		}
		text += Log.AddStackTraceForLog();
		Log.Info(text);
		return new CBaseCard(cardID, ECardType.Dummy, stringID);
	}

	public CActiveBonus AddActiveBonus(CAbility ability, CActor actor, CActor caster, int? iD = null, int? remaining = null, bool isAugment = false, bool isSong = false, bool loadingItemBonus = false, bool isDoom = false, int? bespokeStrength = null, int? activeBonusStartRound = null, bool? activeBonusToggledBonus = null, bool bonusCheck = true)
	{
		if (ability.GetAbilityBaseCard() != this)
		{
			if (ability.CachedAbilityBaseCard == null)
			{
				ability.CachedAbilityBaseCard = this;
			}
			else
			{
				Log.Error($"GetAbilityBaseCard() resolved to a different card in CBaseCard.AddActiveBonus(this: {this}, ability: {ability}) -> {ability.CachedAbilityBaseCard}");
			}
		}
		if (!activeBonusStartRound.HasValue)
		{
			activeBonusStartRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		}
		CActiveBonus cActiveBonus = null;
		bool flag = false;
		if (IsItemOrPet && actor.Class is CCharacterClass cCharacterClass && cCharacterClass.ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == ID && s is CItem) is CItem cItem)
		{
			if (bespokeStrength.HasValue)
			{
				ability.Strength = bespokeStrength.Value;
			}
			CActiveBonus.CreateToggledActiveBonusHack = activeBonusToggledBonus == true;
			cActiveBonus = CActiveBonus.CreateActiveBonus(cItem, ability, actor, caster, activeBonusStartRound.Value, iD, remaining, isAugment, isSong, loadingItemBonus, isDoom);
			CActiveBonus.CreateToggledActiveBonusHack = false;
			bool flag2 = false;
			foreach (CActiveBonus activeBonuse in cItem.ActiveBonuses)
			{
				if (CActiveBonus.AreTheSame(cActiveBonus, activeBonuse))
				{
					Log.ErrorWithReport("Attempting to add a duplicate of an active activeBonus to the base card. " + ID);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				cItem.ActiveBonuses.Add(cActiveBonus);
			}
			flag = true;
		}
		if (!flag)
		{
			if (actor?.Augments != null)
			{
				foreach (CAugment augment in actor.Augments)
				{
					if (this != augment.RegisteredBaseCard || augment.AugmentType != CAugment.EAugmentType.Bonus || augment.Abilities == null)
					{
						continue;
					}
					using List<CAbility>.Enumerator enumerator3 = augment.Abilities.GetEnumerator();
					if (enumerator3.MoveNext())
					{
						CAbility current3 = enumerator3.Current;
						cActiveBonus = CActiveBonus.CreateActiveBonus(augment.RegisteredBaseCard, current3, actor, caster, activeBonusStartRound.Value, iD, null, isAugment: true);
						ActiveBonuses.Add(cActiveBonus);
						if (actor.Class is CCharacterClass cCharacterClass2)
						{
							cCharacterClass2.ActivateCard(augment.RegisteredBaseCard);
						}
						return cActiveBonus;
					}
				}
			}
			if (bespokeStrength.HasValue)
			{
				ability.Strength = bespokeStrength.Value;
			}
			if (ability is CAbilityAddActiveBonus { AddAbility: not null } cAbilityAddActiveBonus)
			{
				if (cAbilityAddActiveBonus.AddAbility.GetAbilityBaseCard() == null)
				{
					cAbilityAddActiveBonus.AddAbility.ParentAbilityBaseCard = cAbilityAddActiveBonus.GetAbilityBaseCard();
				}
				if (cAbilityAddActiveBonus.AddAbility is CAbilityChooseAbility { ChooseAbilities: not null } cAbilityChooseAbility)
				{
					foreach (CAbility chooseAbility in cAbilityChooseAbility.ChooseAbilities)
					{
						if (chooseAbility.GetAbilityBaseCard() == null)
						{
							chooseAbility.ParentAbilityBaseCard = cAbilityAddActiveBonus.GetAbilityBaseCard();
						}
					}
				}
			}
			CActiveBonus.CreateToggledActiveBonusHack = activeBonusToggledBonus == true;
			cActiveBonus = CActiveBonus.CreateActiveBonus(this, ability, actor, caster, activeBonusStartRound.Value, iD, remaining, isAugment, isSong, loadingItemBonus, isDoom);
			CActiveBonus.CreateToggledActiveBonusHack = false;
			ActiveBonuses.Add(cActiveBonus);
		}
		if (cActiveBonus.Ability.ActiveBonusData.GiveAbilityCardToActor && this is CAbilityCard item && actor is CPlayerActor cPlayerActor && caster is CPlayerActor cPlayerActor2)
		{
			if (cPlayerActor2.IsTakingExtraTurn)
			{
				cPlayerActor2.CharacterClass.ExtraTurnCards.Remove(item);
			}
			else
			{
				cPlayerActor2.CharacterClass.RoundAbilityCards.Remove(item);
			}
			cPlayerActor2.CharacterClass.ActivatedCards.Remove(item);
			if (!cPlayerActor.CharacterClass.ActivatedCards.Contains(item))
			{
				cPlayerActor.CharacterClass.ActivatedCards.Add(item);
				CSupplyCardsGiven_MessageData cSupplyCardsGiven_MessageData = new CSupplyCardsGiven_MessageData(cPlayerActor2);
				cSupplyCardsGiven_MessageData.m_ActorGivenCards = cPlayerActor;
				cSupplyCardsGiven_MessageData.m_SupplyCardsGiven = new List<CAbilityCard> { item };
				ScenarioRuleClient.MessageHandler(cSupplyCardsGiven_MessageData);
			}
		}
		if (cActiveBonus.IsAura)
		{
			CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
			if (currentVarsContext == null)
			{
				CAbilityFilter.CurrentVarsContext = ability?.VarsContext;
			}
			cActiveBonus.FindValidActorsInRangeOfAura();
			CAbilityFilter.CurrentVarsContext = currentVarsContext;
		}
		if (bonusCheck)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
		if (cActiveBonus is CMirrorDamageActiveBonus)
		{
			caster.CacheMirrorProperties();
			actor.CacheMirrorProperties();
		}
		if (cActiveBonus.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.Tide) && !ScenarioManager.IsLoading)
		{
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_MessageKey = "COMBAT_LOG_INCREASE_TIDES",
				m_Actors = new List<CActor> { actor },
				m_Params = new List<string> { "Actor_0" }
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		actor.LogActiveBonuses();
		return cActiveBonus;
	}

	public void AddActiveBonus(CActiveBonus activeBonus)
	{
		ActiveBonuses.Add(activeBonus);
		if (activeBonus.IsAura)
		{
			foreach (CActor item in activeBonus.ValidActorsInRangeOfAura)
			{
				item.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
			return;
		}
		activeBonus.Actor.CheckForCachedValuesAfterActiveBonusesUpdate();
	}

	public CActiveBonus GetAugmentActiveBonus(CAugment augment)
	{
		CActiveBonus cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment);
		if (cActiveBonus == null && augment.AugmentType == CAugment.EAugmentType.Bonus && augment.Abilities != null && augment.Abilities[0].Augment != null)
		{
			cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment.Abilities[0].Augment);
		}
		return cActiveBonus;
	}

	public CActiveBonus GetSongActiveBonus(CSong song)
	{
		return ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
	}

	public void RemoveAugmentActiveBonus(CAugment augment)
	{
		CActiveBonus item = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment);
		ActiveBonuses.Remove(item);
	}

	public void RemoveSongActiveBonus(CSong song)
	{
		CActiveBonus item = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
		ActiveBonuses.Remove(item);
	}

	public virtual void Reset()
	{
		CurrentCardPile = ECardPile.None;
		ActiveBonuses.Clear();
	}

	public virtual ActionType GetAbilityActionType(CAbility ability)
	{
		return ActionType.NA;
	}

	public static List<Tuple<int, string>> Compare(CBaseCard state1, CBaseCard state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.Name != state2.Name)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2801, "CBaseCard Name does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.ID != state2.ID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2802, "CBaseCard ID does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.CardType != state2.CardType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2803, "CBaseCard CardType does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.CurrentCardPile != state2.CurrentCardPile)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2804, "CBaseCard CurrentCardPile does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					},
					new string[3]
					{
						"CurrentCardPile",
						state1.CurrentCardPile.ToString(),
						state2.CurrentCardPile.ToString()
					}
				});
			}
			if (state1.ActionHasHappened != state2.ActionHasHappened)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2805, "CBaseCard ActionHasHappened does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					},
					new string[3]
					{
						"ActionHasHappened",
						state1.ActionHasHappened.ToString(),
						state2.ActionHasHappened.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2899, "Exception during CBaseCard compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
