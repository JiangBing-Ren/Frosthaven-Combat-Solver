using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class ActiveBonusState : ISerializable
{
	public int ID { get; private set; }

	public int CardID { get; private set; }

	public string CardName { get; set; }

	public string AbilityName { get; private set; }

	[StateDump(Skip = true)]
	public Guid AbilityID { get; private set; }

	[StateDump(Skip = true)]
	public string AbilityJson { get; private set; }

	public string ActorGuid { get; private set; }

	public string CasterGuid { get; private set; }

	public bool IsTopAction { get; private set; }

	public int Remaining { get; set; }

	public int ActiveBonusStartRound { get; private set; }

	public bool IsDoom { get; private set; }

	public int BespokeBehaviourStrength { get; private set; }

	public List<string> RestrictedActorGuids { get; private set; }

	public bool ActionHasHappened { get; private set; }

	public bool ToggledBonus { get; set; }

	public int? AbilityStrength { get; private set; }

	public int? AuraRange { get; private set; }

	public string AdditionalSerialization { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AbilityID", AbilityID);
		info.AddValue("ID", ID);
		info.AddValue("CardID", CardID);
		info.AddValue("CardName", CardName);
		info.AddValue("AbilityName", AbilityName);
		info.AddValue("AbilityJson", AbilityJson);
		info.AddValue("ActorGuid", ActorGuid);
		info.AddValue("CasterGuid", CasterGuid);
		info.AddValue("IsTopAction", IsTopAction);
		info.AddValue("Remaining", Remaining);
		info.AddValue("ActiveBonusStartRound", ActiveBonusStartRound);
		info.AddValue("IsDoom", IsDoom);
		info.AddValue("BespokeBehaviourStrength", BespokeBehaviourStrength);
		info.AddValue("RestrictedActorGuids", RestrictedActorGuids);
		info.AddValue("ActionHasHappened", ActionHasHappened);
		info.AddValue("ToggledBonus", ToggledBonus);
		info.AddValue("AbilityStrength", AbilityStrength);
		info.AddValue("AuraRange", AuraRange);
		info.AddValue("AdditionalSerialization", AdditionalSerialization);
	}

	public ActiveBonusState(SerializationInfo info, StreamingContext context)
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
				case "AbilityID":
					AbilityID = (Guid)info.GetValue("AbilityID", typeof(Guid));
					break;
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "AbilityName":
					AbilityName = info.GetString("AbilityName");
					break;
				case "AbilityJson":
					AbilityJson = info.GetString("AbilityJson");
					break;
				case "CardName":
					CardName = info.GetString("CardName");
					break;
				case "ActorGuid":
					ActorGuid = info.GetString("ActorGuid");
					break;
				case "CasterGuid":
					CasterGuid = info.GetString("CasterGuid");
					break;
				case "IsTopAction":
					IsTopAction = info.GetBoolean("IsTopAction");
					break;
				case "Remaining":
					Remaining = info.GetInt32("Remaining");
					break;
				case "ActiveBonusStartRound":
					ActiveBonusStartRound = info.GetInt32("ActiveBonusStartRound");
					break;
				case "IsDoom":
					IsDoom = info.GetBoolean("IsDoom");
					break;
				case "BespokeBehaviourStrength":
					BespokeBehaviourStrength = info.GetInt32("BespokeBehaviourStrength");
					break;
				case "RestrictedActorGuids":
					RestrictedActorGuids = (List<string>)info.GetValue("RestrictedActorGuids", typeof(List<string>));
					break;
				case "AdditionalSerialization":
					AdditionalSerialization = info.GetString("AdditionalSerialization");
					break;
				case "ActionHasHappened":
					ActionHasHappened = info.GetBoolean("ActionHasHappened");
					break;
				case "ToggledBonus":
					ToggledBonus = info.GetBoolean("ToggledBonus");
					break;
				case "AbilityStrength":
					AbilityStrength = (int?)info.GetValue("AbilityStrength", typeof(int?));
					break;
				case "AuraRange":
					AuraRange = (int?)info.GetValue("AuraRange", typeof(int?));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize ActiveBonusState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		try
		{
			if (CardID == 235 && (AbilityName == "TSummon1" || AbilityName == "TSummon2"))
			{
				AbilityName = "TSummon";
			}
			if (CardID == 253 && (AbilityName == "TSummon1" || AbilityName == "TSummon2" || AbilityName == "TSummon3"))
			{
				AbilityName = "TSummon";
			}
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("Exception while trying to deserialize ActiveBonusState and update multi summon card enhancement IDs\n" + ex.Message + "\n" + ex.StackTrace);
			throw ex;
		}
	}

	public ActiveBonusState(int id, int cardID, string cardName, string abilityName, Guid abilityID, string abilityJson, string actorGuid, string casterGuid, bool isTopAction, int remaining, int activeBonusStartRound, bool isDoom, int bespokeStrength, List<string> restrictedActorGuids, bool actionHasHappened, bool toggledBonus, int abilityStrength, int auraRange, CActiveBonus bonus)
	{
		ID = id;
		CardID = cardID;
		CardName = cardName;
		AbilityName = abilityName;
		AbilityID = abilityID;
		AbilityJson = abilityJson;
		ActorGuid = actorGuid;
		CasterGuid = casterGuid;
		IsTopAction = isTopAction;
		Remaining = remaining;
		ActiveBonusStartRound = activeBonusStartRound;
		IsDoom = isDoom;
		BespokeBehaviourStrength = bespokeStrength;
		RestrictedActorGuids = restrictedActorGuids;
		ActionHasHappened = actionHasHappened;
		ToggledBonus = toggledBonus;
		AbilityStrength = abilityStrength;
		AuraRange = auraRange;
		bonus.Save(this);
	}

	public bool NameIsValid()
	{
		if (CardName != null)
		{
			return CardName != string.Empty;
		}
		return false;
	}

	public static List<Tuple<int, string>> Compare(ActiveBonusState state1, ActiveBonusState state2, string actorGuid, string classID, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.CardID != state2.CardID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1202, "ActiveBonusState CardID does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"CardID",
						state1.CardID.ToString(),
						state2.CardID.ToString()
					},
					new string[3] { "CardName", state1.CardName, state2.CardName }
				});
			}
			if (state1.CardName != state2.CardName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1203, "ActiveBonusState CardName does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName }
				});
			}
			if (state1.AbilityName != state2.AbilityName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1204, "ActiveBonusState AbilityName does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3] { "AbilityName", state1.AbilityName, state2.AbilityName }
				});
			}
			if (state1.ActorGuid != state2.ActorGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1205, "ActiveBonusState ActorGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3] { "ActorGuid", state1.ActorGuid, state2.ActorGuid }
				});
			}
			if (state1.CasterGuid != state2.CasterGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1206, "ActiveBonusState CasterGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3] { "CasterGuid", state1.CasterGuid, state2.CasterGuid }
				});
			}
			if (state1.IsTopAction != state2.IsTopAction)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1207, "ActiveBonusState IsTopAction does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3]
					{
						"IsTopAction",
						state1.IsTopAction.ToString(),
						state2.IsTopAction.ToString()
					}
				});
			}
			if (state1.Remaining != state2.Remaining)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1208, "ActiveBonusState Remaining does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3]
					{
						"Remaining",
						state1.Remaining.ToString(),
						state2.Remaining.ToString()
					}
				});
			}
			if (state1.ActiveBonusStartRound != state2.ActiveBonusStartRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1209, "ActiveBonusState ActiveBonusStartRound does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3]
					{
						"ActiveBonusStartRound",
						state1.ActiveBonusStartRound.ToString(),
						state2.ActiveBonusStartRound.ToString()
					}
				});
			}
			if (state1.IsDoom != state2.IsDoom)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1210, "ActiveBonusState IsDoom does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
					new string[3]
					{
						"IsDoom",
						state1.IsDoom.ToString(),
						state2.IsDoom.ToString()
					}
				});
			}
			if (state1.ActionHasHappened != state2.ActionHasHappened)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1210, "ActiveBonusState ActionHasHappened does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Class ID", classID, classID },
					new string[3] { "CardName", state1.CardName, state2.CardName },
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
			list.Add(new Tuple<int, string>(1299, "Exception during ActiveBonus State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
