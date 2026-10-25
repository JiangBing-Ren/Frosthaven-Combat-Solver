using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelUIInteractionProfile : ISerializable
{
	[Serializable]
	[KronosEnumPolice(8)]
	public enum EIsolatedControlType
	{
		LevelMessageWindow = -99,
		ResultsScreen = -98,
		EscapeMenu = 0,
		PersistentUI = 1,
		CardSelection = 2,
		CardTopHalfSelection = 3,
		CardBottomHalfSelection = 4,
		BottomBarConfirmButton = 5,
		BottomBarUndoButton = 6,
		BottomBarSkipButton = 7,
		AttackModifierHoverButton = 8,
		TileSelection = 9,
		TakeDamageButton = 10,
		BurnAvailableButton = 11,
		BurnDiscardedButton = 12,
		ShortRestButton = 13,
		ShortRestConfirmButton = 14,
		ShortRestCancelButton = 15,
		ShortRestDialogOption = 16,
		DefaultMoveAbility = 17,
		DefaultAttackAbility = 18,
		CharacterPortrait = 19,
		ConsumeElementOnCardTop = 20,
		ConsumeElementOnCardBottom = 21,
		ItemSlotButton = 22,
		CharacterTabIcon = 23,
		ReduceDamageShieldItemButton = 24,
		BottomBarCameraRoomButton = 25,
		BottomBarWarningConfirmButton = 26,
		StoryBox = 27,
		SpeedUpButton = 28,
		CardSelectionInitiative = 29,
		DialogOptionButton = 30,
		GeneralInfoModule = 31,
		CharacterTab = 32,
		ScenarioObjectivesAndModifiers = 33,
		BottomBarHelpText = 34,
		ElementInfusionBoard = 35,
		CombatLog = 36,
		UseItemSlot = 37
	}

	[Serializable]
	public class CLevelUIInteractionSpecific : ISerializable
	{
		[Serializable]
		[KronosEnumPolice(0)]
		public enum EControlBehaviourType
		{
			HighlightAndLimitInteraction,
			HighlightOnly,
			HighlightAndAllowAnyInteraction,
			LimitInteractionOnly,
			HighlightOnlyObsolete,
			TreatAsInteractableOnlyForVisuals,
			HighlightAndTreatAsInteractableForVisuals
		}

		public static readonly EControlBehaviourType[] ControlBehaviourTypes = (EControlBehaviourType[])Enum.GetValues(typeof(EControlBehaviourType));

		public EIsolatedControlType ControlType { get; set; }

		public string ControlIdentifier { get; set; }

		public int ControlContextTypeInt { get; set; }

		public int ControlIndex { get; set; }

		public string ControlIdentifier2 { get; set; }

		public TileIndex ControlTileIndex { get; set; }

		public EControlBehaviourType ControlBehaviour { get; set; }

		public string HighlightPrefab { get; set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ControlType", ControlType);
			info.AddValue("ControlIdentifier", ControlIdentifier);
			info.AddValue("ControlContextTypeInt", ControlContextTypeInt);
			info.AddValue("ControlIndex", ControlIndex);
			info.AddValue("ControlTileIndex", ControlTileIndex);
			info.AddValue("ControlIdentifier2", ControlIdentifier2);
			info.AddValue("ControlBehaviour", ControlBehaviour);
			info.AddValue("HighlightPrefab", HighlightPrefab);
		}

		public CLevelUIInteractionSpecific(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "ControlType":
						ControlType = (EIsolatedControlType)info.GetValue("ControlType", typeof(EIsolatedControlType));
						break;
					case "ControlIdentifier":
						ControlIdentifier = info.GetString("ControlIdentifier");
						break;
					case "HighlightPrefab":
						HighlightPrefab = info.GetString("HighlightPrefab");
						break;
					case "ControlContextTypeInt":
						ControlContextTypeInt = info.GetInt32("ControlContextTypeInt");
						break;
					case "ControlIndex":
						ControlIndex = info.GetInt32("ControlIndex");
						break;
					case "ControlIdentifier2":
						ControlIdentifier2 = info.GetString("ControlIdentifier2");
						break;
					case "ControlTileIndex":
						ControlTileIndex = (TileIndex)info.GetValue("ControlTileIndex", typeof(TileIndex));
						break;
					case "ControlBehaviour":
						ControlBehaviour = (EControlBehaviourType)info.GetValue("ControlBehaviour", typeof(EControlBehaviourType));
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize CLevelUIInteractionSpecific entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public CLevelUIInteractionSpecific(EIsolatedControlType controlType, EControlBehaviourType behaviourType = EControlBehaviourType.HighlightAndLimitInteraction)
		{
			ControlType = controlType;
			ControlIdentifier = string.Empty;
			ControlContextTypeInt = 0;
			ControlIndex = -1;
			ControlIdentifier2 = string.Empty;
			ControlTileIndex = null;
			ControlBehaviour = behaviourType;
			HighlightPrefab = string.Empty;
		}
	}

	public static readonly EIsolatedControlType[] ControlTypes = (EIsolatedControlType[])Enum.GetValues(typeof(EIsolatedControlType));

	public List<CLevelUIInteractionSpecific> ControlsToAllow { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ControlsToAllow", ControlsToAllow);
	}

	public CLevelUIInteractionProfile(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ControlsToAllow")
				{
					ControlsToAllow = (List<CLevelUIInteractionSpecific>)info.GetValue("ControlsToAllow", typeof(List<CLevelUIInteractionSpecific>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CLevelUIInteractionProfile entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelUIInteractionProfile()
	{
		ControlsToAllow = new List<CLevelUIInteractionSpecific>();
	}
}
