using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective : ISerializable
{
	public static readonly EObjectiveResult[] ObjectiveResults = (EObjectiveResult[])Enum.GetValues(typeof(EObjectiveResult));

	public static readonly EObjectiveType[] ObjectiveTypes = (EObjectiveType[])Enum.GetValues(typeof(EObjectiveType));

	public EObjectiveType ObjectiveType { get; private set; }

	public EObjectiveResult Result { get; private set; }

	public bool IsComplete { get; protected set; }

	public bool UnableToComplete { get; set; }

	public bool IsActiveFromStart { get; protected set; }

	public int? ActivateOnRound { get; protected set; }

	public int RoundActivatedOn { get; protected set; }

	public bool IsActive { get; protected set; }

	public CObjectiveFilter ObjectiveFilter { get; protected set; }

	public string CustomLocKey { get; protected set; }

	public List<object> CustomLocKeyArgs { get; set; }

	public string CustomTileHoverLocKey { get; protected set; }

	public string EventIdentifier { get; protected set; }

	public bool HideCounter { get; protected set; }

	public virtual bool RemovesFromUIOnComplete => true;

	public bool IsHidden { get; protected set; }

	public bool IsOptional { get; protected set; }

	public bool DeactivatedByEvent { get; protected set; }

	public bool WinsDespiteExhaustion { get; protected set; }

	public bool EnoughToWinAlone { get; protected set; }

	public Dictionary<string, bool> RequiredObjectiveStates { get; protected set; }

	public virtual string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (!(this is CObjective_KillAllEnemies cObjective_KillAllEnemies))
				{
					if (!(this is CObjective_KillAllBosses cObjective_KillAllBosses))
					{
						if (!(this is CObjective_ReachRound cObjective_ReachRound))
						{
							if (!(this is CObjective_ActorReachPosition cObjective_ActorReachPosition))
							{
								if (!(this is CObjective_LootX cObjective_LootX))
								{
									if (this is CObjective_CustomTrigger)
									{
										if (string.IsNullOrEmpty(CustomLocKey))
										{
											return "GUI_OBJECTIVE_CUSTOM_TRIGGER_DEFAULT";
										}
										return CustomLocKey;
									}
									if (this is CObjective_Scripted cObjective_Scripted)
									{
										if (string.IsNullOrEmpty(CustomLocKey))
										{
											if (string.IsNullOrEmpty(cObjective_Scripted.ScriptedLocKey))
											{
												return "GUI_OBJECTIVE_SCRIPTED";
											}
											return cObjective_Scripted.ScriptedLocKey;
										}
										return CustomLocKey;
									}
								}
								else
								{
									if (cObjective_LootX.Result == EObjectiveResult.Win)
									{
										return "GUI_OBJECTIVE_LOOT_X_WIN";
									}
									if (cObjective_LootX.Result == EObjectiveResult.Lose)
									{
										return "GUI_OBJECTIVE_LOOT_X_LOSE";
									}
								}
							}
							else
							{
								if (cObjective_ActorReachPosition.Result == EObjectiveResult.Win)
								{
									return "GUI_OBJECTIVE_REACH_POSITION_WIN";
								}
								if (cObjective_ActorReachPosition.Result == EObjectiveResult.Lose)
								{
									return "GUI_OBJECTIVE_REACH_POSITION_LOSE";
								}
							}
						}
						else
						{
							if (cObjective_ReachRound.Result == EObjectiveResult.Win)
							{
								return "GUI_OBJECTIVE_REACH_ROUND_WIN";
							}
							if (cObjective_ReachRound.Result == EObjectiveResult.Lose)
							{
								return "GUI_OBJECTIVE_REACH_ROUND_LOSE";
							}
						}
					}
					else
					{
						if (cObjective_KillAllBosses.Result == EObjectiveResult.Win)
						{
							return "GUI_OBJECTIVE_KILL_BOSSES_WIN";
						}
						if (cObjective_KillAllBosses.Result == EObjectiveResult.Lose)
						{
							return "GUI_OBJECTIVE_KILL_BOSSES_LOSE";
						}
					}
				}
				else
				{
					if (cObjective_KillAllEnemies.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_KILL_ENEMY_WIN";
					}
					if (cObjective_KillAllEnemies.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_KILL_ENEMY_LOSE";
					}
				}
				return string.Empty;
			}
			return CustomLocKey;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ObjectiveType", ObjectiveType);
		info.AddValue("Result", Result);
		info.AddValue("IsComplete", IsComplete);
		info.AddValue("UnableToComplete", UnableToComplete);
		info.AddValue("IsActiveFromStart", IsActiveFromStart);
		info.AddValue("ActivateOnRound", ActivateOnRound);
		info.AddValue("IsActive", IsActive);
		info.AddValue("RoundActivatedOn", RoundActivatedOn);
		info.AddValue("ObjectiveFilter", ObjectiveFilter);
		info.AddValue("CustomLocKey", CustomLocKey);
		info.AddValue("CustomTileHoverLocKey", CustomTileHoverLocKey);
		info.AddValue("EventIdentifier", EventIdentifier);
		info.AddValue("IsHidden", IsHidden);
		info.AddValue("IsOptional", IsOptional);
		info.AddValue("DeactivatedByEvent", DeactivatedByEvent);
		info.AddValue("WinsDespiteExhaustion", WinsDespiteExhaustion);
		info.AddValue("EnoughToWinAlone", EnoughToWinAlone);
		info.AddValue("RequiredObjectiveStates", RequiredObjectiveStates);
		info.AddValue("HideCounter", HideCounter);
	}

	public CObjective(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "HideCounter":
					HideCounter = info.GetBoolean("HideCounter");
					break;
				case "ObjectiveType":
					ObjectiveType = (EObjectiveType)info.GetValue("ObjectiveType", typeof(EObjectiveType));
					break;
				case "Result":
					Result = (EObjectiveResult)info.GetValue("Result", typeof(EObjectiveResult));
					break;
				case "IsComplete":
					IsComplete = info.GetBoolean("IsComplete");
					break;
				case "UnableToComplete":
					UnableToComplete = info.GetBoolean("UnableToComplete");
					break;
				case "IsActiveFromStart":
					IsActiveFromStart = info.GetBoolean("IsActiveFromStart");
					break;
				case "ActivateOnRound":
					ActivateOnRound = (int?)info.GetValue("ActivateOnRound", typeof(int?));
					break;
				case "IsActive":
					IsActive = info.GetBoolean("IsActive");
					break;
				case "RoundActivatedOn":
					RoundActivatedOn = info.GetInt32("RoundActivatedOn");
					break;
				case "ObjectiveFilter":
					ObjectiveFilter = (CObjectiveFilter)info.GetValue("ObjectiveFilter", typeof(CObjectiveFilter));
					break;
				case "CustomLocKey":
					CustomLocKey = info.GetString("CustomLocKey");
					break;
				case "CustomTileHoverLocKey":
					CustomTileHoverLocKey = info.GetString("CustomTileHoverLocKey");
					break;
				case "EventIdentifier":
					EventIdentifier = info.GetString("EventIdentifier");
					break;
				case "IsHidden":
					IsHidden = info.GetBoolean("IsHidden");
					break;
				case "IsOptional":
					IsOptional = info.GetBoolean("IsOptional");
					break;
				case "DeactivatedByEvent":
					DeactivatedByEvent = info.GetBoolean("DeactivatedByEvent");
					break;
				case "WinsDespiteExhaustion":
					WinsDespiteExhaustion = info.GetBoolean("WinsDespiteExhaustion");
					break;
				case "EnoughToWinAlone":
					EnoughToWinAlone = info.GetBoolean("EnoughToWinAlone");
					break;
				case "RequiredObjectiveStates":
					RequiredObjectiveStates = (Dictionary<string, bool>)info.GetValue("RequiredObjectiveStates", typeof(Dictionary<string, bool>));
					break;
				case "ActivationRound":
				{
					int @int = info.GetInt32("ActivationRound");
					if (@int > 0)
					{
						ActivateOnRound = @int;
						break;
					}
					IsActiveFromStart = true;
					IsActive = true;
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		EnsureNonEmptyEventIdentifier();
		if (ObjectiveFilter == null)
		{
			ObjectiveFilter = new CObjectiveFilter();
		}
	}

	public CObjective(EObjectiveType objType, EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
	{
		ObjectiveType = objType;
		Result = result;
		IsComplete = false;
		UnableToComplete = false;
		IsActiveFromStart = activeFromStart;
		ActivateOnRound = activateOnRound;
		IsActive = IsActiveFromStart || (ActivateOnRound.HasValue && ActivateOnRound.Value <= 0);
		RoundActivatedOn = ((!IsActive) ? int.MaxValue : 0);
		ObjectiveFilter = objectiveFilter;
		CustomLocKey = customLoc;
		CustomTileHoverLocKey = customTileHoverLoc;
		EventIdentifier = eventIdentifier;
		IsHidden = isHidden;
		IsOptional = isOptional;
		WinsDespiteExhaustion = winDespiteExhaustion;
		EnoughToWinAlone = enoughToWinAlone;
		RequiredObjectiveStates = requiredObjectiveStates?.ToDictionary((KeyValuePair<string, bool> kv) => kv.Key, (KeyValuePair<string, bool> kv) => kv.Value) ?? null;
		HideCounter = hideCounter;
		EnsureNonEmptyEventIdentifier();
	}

	private void EnsureNonEmptyEventIdentifier()
	{
		if (string.IsNullOrEmpty(EventIdentifier))
		{
			EventIdentifier = Guid.NewGuid().ToString();
		}
	}

	public virtual bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		return IsComplete;
	}

	public virtual float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = 1;
		current = (IsComplete ? 1 : 0);
		if (!IsComplete)
		{
			return 0f;
		}
		return 1f;
	}

	public virtual int GetObjectiveCompletionValue(int partySize)
	{
		return 0;
	}

	public void CheckActivationRound(int currentRound)
	{
		if (ActivateOnRound.HasValue)
		{
			bool isActive = IsActive;
			IsActive = ActivateOnRound.Value <= currentRound;
			if (IsActive && !isActive && !DeactivatedByEvent)
			{
				RoundActivatedOn = currentRound;
				CActivateObjective_MessageData message = new CActivateObjective_MessageData
				{
					m_ActivatedObjective = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public void SetActivation(bool active, bool fromMigration = false)
	{
		bool isActive = IsActive;
		IsActive = active;
		if (fromMigration)
		{
			return;
		}
		if (!active)
		{
			DeactivatedByEvent = true;
		}
		if (IsActive != isActive)
		{
			if (IsActive)
			{
				RoundActivatedOn = ScenarioManager.CurrentScenarioState.RoundNumber;
				CActivateObjective_MessageData message = new CActivateObjective_MessageData
				{
					m_ActivatedObjective = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				CDeactivateObjective_MessageData message2 = new CDeactivateObjective_MessageData
				{
					m_DeactivatedObjective = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
		}
	}

	public bool CheckOtherObjectiveStatesRequirement()
	{
		if (RequiredObjectiveStates == null || RequiredObjectiveStates.Count == 0)
		{
			return true;
		}
		foreach (string objectiveEventIdentifier in RequiredObjectiveStates.Keys)
		{
			if (!string.IsNullOrEmpty(objectiveEventIdentifier))
			{
				CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective o) => o.EventIdentifier == objectiveEventIdentifier);
				if (cObjective != null && cObjective.IsComplete != RequiredObjectiveStates[objectiveEventIdentifier])
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetIsComplete(bool val)
	{
		IsComplete = val;
	}

	public static List<Tuple<int, string>> Compare(CObjective obj1, CObjective obj2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (obj1.ObjectiveType != obj2.ObjectiveType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2201, "CObjective ObjectiveType does not match.", new List<string[]> { new string[3]
				{
					"ObjectiveType",
					obj1.ObjectiveType.ToString(),
					obj2.ObjectiveType.ToString()
				} });
			}
			if (obj1.Result != obj2.Result)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2202, "CObjective Result does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"Result",
						obj1.Result.ToString(),
						obj2.Result.ToString()
					}
				});
			}
			if (obj1.IsComplete != obj2.IsComplete)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2203, "CObjective IsComplete does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"IsComplete",
						obj1.IsComplete.ToString(),
						obj2.IsComplete.ToString()
					}
				});
			}
			if (obj1.UnableToComplete != obj2.UnableToComplete)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2204, "CObjective UnableToComplete does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"UnableToComplete",
						obj1.UnableToComplete.ToString(),
						obj2.UnableToComplete.ToString()
					}
				});
			}
			if (obj1.ActivateOnRound != obj2.ActivateOnRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2205, "CObjective ActivationRound does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"ActivationRound",
						obj1.ActivateOnRound.ToString(),
						obj2.ActivateOnRound.ToString()
					}
				});
			}
			if (obj1.IsActive != obj2.IsActive)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2206, "CObjective IsActive does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"IsActive",
						obj1.IsActive.ToString(),
						obj2.IsActive.ToString()
					}
				});
			}
			if (!obj1.ObjectiveFilter.Compare(obj2.ObjectiveFilter))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2207, "CObjective ObjectiveFilter does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"ObjectiveFilter",
						obj1.ObjectiveFilter.ToString(),
						obj2.ObjectiveFilter.ToString()
					}
				});
			}
			if (obj1.CustomLocKey != obj2.CustomLocKey)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2208, "CObjective CustomLocKey does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3] { "CustomLocKey", obj1.CustomLocKey, obj2.CustomLocKey }
				});
			}
			if (obj1.CustomTileHoverLocKey != obj2.CustomTileHoverLocKey)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2211, "CObjective CustomTileHoverLocKey does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3] { "CustomTileHoverLocKey", obj1.CustomTileHoverLocKey, obj2.CustomTileHoverLocKey }
				});
			}
			if (obj1.EventIdentifier != obj2.EventIdentifier)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2209, "CObjective EventIdentifier does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3] { "EventIdentifier", obj1.EventIdentifier, obj2.EventIdentifier }
				});
			}
			if (obj1.IsHidden != obj2.IsHidden)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2210, "CObjective IsHidden does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"IsHidden",
						obj1.IsHidden.ToString(),
						obj2.IsHidden.ToString()
					}
				});
			}
			if (obj1.RoundActivatedOn != obj2.RoundActivatedOn)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2211, "CObjective RoundActivatedOn does not match.", new List<string[]>
				{
					new string[3]
					{
						"ObjectiveType",
						obj1.ObjectiveType.ToString(),
						obj2.ObjectiveType.ToString()
					},
					new string[3]
					{
						"RoundActivatedOn",
						obj1.RoundActivatedOn.ToString(),
						obj2.RoundActivatedOn.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2299, "Exception during CObjective compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
