using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{ClassID}")]
public class ObjectState : EnemyState, ISerializable
{
	public bool TreatAsObstacle;

	public CObjectActor Object => ScenarioManager.Scenario.AllObjects.FirstOrDefault((CObjectActor s) => s.ActorGuid == ActorGuid);

	public bool IsAttachedToProp { get; set; }

	public string PropGuidAttachedTo { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsAttachedToProp", IsAttachedToProp);
		info.AddValue("PropGuidAttachedTo", PropGuidAttachedTo);
		info.AddValue("TreatAsObstacle", TreatAsObstacle);
	}

	public ObjectState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "IsAttachedToProp":
					IsAttachedToProp = info.GetBoolean("IsAttachedToProp");
					break;
				case "PropGuidAttachedTo":
					PropGuidAttachedTo = info.GetString("PropGuidAttachedTo");
					break;
				case "TreatAsObstacle":
					TreatAsObstacle = info.GetBoolean("TreatAsObstacle");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize ObjectState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public override void Save(bool initial, bool forceSave = false)
	{
		if (IsRevealed || forceSave)
		{
			CObjectActor cObjectActor = null;
			cObjectActor = ((!initial) ? Object : ScenarioManager.Scenario.InitialObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == ActorGuid));
			SaveFields(cObjectActor);
		}
	}

	public override void SaveFields(CActor actor)
	{
		base.SaveFields(actor);
		if (actor is CObjectActor cObjectActor)
		{
			TreatAsObstacle = cObjectActor.TreatAsObstacle;
		}
	}

	public override void Load()
	{
		if (Object != null)
		{
			Object.LoadFields(this);
		}
		else
		{
			Log.Warning("Could not find Object");
		}
	}

	public ObjectState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, bool isSummon, string summonerGuid, int augmentSlots, CActor.EType type)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, isSummon, summonerGuid, augmentSlots, type)
	{
	}

	public ObjectState(string classID, int chosenModelIndex, string mapGuid, CActor.EType type)
		: base(classID, chosenModelIndex, mapGuid, type)
	{
	}

	public ObjectState(CObjectActor objectActor, string mapGuid)
		: base(objectActor, mapGuid)
	{
		IsAttachedToProp = objectActor.IsAttachedToProp;
		PropGuidAttachedTo = (objectActor.IsAttachedToProp ? objectActor.AttachedProp.PropGuid : string.Empty);
	}

	public void InitialisePropAttachment(CObjectProp propToAttach, CObjectActor dummyObjectActor)
	{
		PropGuidAttachedTo = propToAttach.PropGuid;
		IsAttachedToProp = true;
		propToAttach.SetActorAttachedAtRuntime(dummyObjectActor);
		dummyObjectActor?.SetAttachedToProp(propToAttach);
	}

	public static List<Tuple<int, string>> Compare(ObjectState state1, ObjectState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(EnemyState.Compare(state1, state2, isMPCompare));
		try
		{
			if (state1.IsAttachedToProp != state2.IsAttachedToProp)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 621, "Object State IsAttachedToProp does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsAttachedToProp",
						state1.IsAttachedToProp.ToString(),
						state2.IsAttachedToProp.ToString()
					}
				});
			}
			if (state1.PropGuidAttachedTo != state2.PropGuidAttachedTo)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 622, "Object State PropGuidAttachedTo does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PropGuidAttachedTo",
						(state1.PropGuidAttachedTo == null) ? "null " : state1.PropGuidAttachedTo.ToString(),
						(state2.PropGuidAttachedTo == null) ? "null " : state2.PropGuidAttachedTo.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(698, "Exception during Object State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
