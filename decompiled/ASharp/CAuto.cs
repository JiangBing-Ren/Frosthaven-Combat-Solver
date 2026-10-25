using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

[Serializable]
public class CAuto : ISerializable
{
	[Serializable]
	public enum EAutoType
	{
		None,
		ButtonClick,
		TileClick,
		ButtonHover,
		ChoreographerStep,
		NextRewardClicked,
		AOETileHover,
		AOERotate
	}

	public enum OldChoreographerStateType
	{
		NA,
		WaitingForMoveAnim,
		WaitingForAttackModifierCards,
		WaitingForAttackAnim,
		WaitingForModifierDrawAnim,
		WaitingForDamageAnim,
		WaitingForGeneralAnim,
		WaitingForPlayerIdle,
		WaitingForEndAbilityAnimSync,
		WaitingForEndTurnSync,
		WaitingForProgressChoreographer,
		Play,
		WaitingForCardSelection,
		WaitingForPlayerWaypointSelection,
		WaitingForAreaAttackFocusSelection,
		WaitingForPlayerPushWaypointSelection,
		WaitingForPlayerPullWaypointSelection,
		WaitingForRewardsProcess,
		WaitingInLevelEditor,
		WaitingForExhaustionClear,
		WaitingForAutosave,
		WaitingForTileSelected
	}

	public int ID;

	public EAutoType EventType;

	public Choreographer.ChoreographerStateType ChoreographerState;

	public string ChoreographerStateString;

	public int ChoreographerStepsCompleted;

	public bool ForTutorial;

	public int DisplayedMessageCount;

	public int Version { get; protected set; }

	public DateTime TimeStamp { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Version", Version);
		info.AddValue("ID", ID);
		info.AddValue("EventType", EventType);
		info.AddValue("ChoreographerStateString", ChoreographerState.ToString());
		info.AddValue("ChoreographerStepsCompleted", ChoreographerStepsCompleted);
		info.AddValue("TimeStamp", TimeStamp);
		info.AddValue("ForTutorial", ForTutorial);
		info.AddValue("DisplayedMessageCount", DisplayedMessageCount);
	}

	protected CAuto(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Version":
					Version = info.GetInt32("Version");
					break;
				case "ID":
					ID = info.GetInt32("ID");
					break;
				case "EventType":
					EventType = (EAutoType)info.GetValue("EventType", typeof(EAutoType));
					break;
				case "ChoreographerState":
				{
					int num = (int)info.GetValue("ChoreographerState", typeof(Choreographer.ChoreographerStateType));
					OldChoreographerStateType oldState = (OldChoreographerStateType)num;
					ChoreographerState = Choreographer.ChoreographerStateTypes.Single((Choreographer.ChoreographerStateType s) => s.ToString() == oldState.ToString());
					ChoreographerStateString = ChoreographerState.ToString();
					break;
				}
				case "ChoreographerStateString":
					ChoreographerStateString = info.GetString("ChoreographerStateString");
					ChoreographerState = Choreographer.ChoreographerStateTypes.Single((Choreographer.ChoreographerStateType s) => s.ToString() == ChoreographerStateString);
					break;
				case "ChoreographerStepsCompleted":
					ChoreographerStepsCompleted = info.GetInt32("ChoreographerStepsCompleted");
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				case "ForTutorial":
					ForTutorial = info.GetBoolean("ForTutorial");
					break;
				case "DisplayedMessageCount":
					DisplayedMessageCount = info.GetInt32("DisplayedMessageCount");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize CAuto entry " + item.Name);
				throw ex;
			}
		}
	}

	public CAuto(EAutoType eventType, int id)
	{
		EventType = eventType;
		ID = id;
		if (Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_WaitState != null)
		{
			ChoreographerState = Choreographer.s_Choreographer.m_WaitState.m_State;
		}
		else
		{
			ChoreographerState = Choreographer.ChoreographerStateType.NA;
		}
		ChoreographerStepsCompleted = AutoTestController.s_Instance.ChoreographerMessagesProcessed;
		TimeStamp = DateTime.Now;
		ForTutorial = LevelEventsController.s_EventsControllerActive;
		if (ForTutorial)
		{
			DisplayedMessageCount = Singleton<LevelEventsController>.Instance.NumberOfAlreadyDisplayedMessages;
		}
	}
}
