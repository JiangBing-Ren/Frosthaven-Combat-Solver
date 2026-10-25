using FHRL;

namespace FHGUI.DTO.Outpost;

public static class OutpostNotificationUtils
{
	public const string NOTIFICATION_ID_ANY = "*";

	public static bool IsOutpostNotification(OutpostNotificationType notificationType)
	{
		if (notificationType != OutpostNotificationType.NewTraderItems && notificationType != OutpostNotificationType.NewCraftingBlueprints && notificationType != OutpostNotificationType.BuildingWrecked && notificationType != OutpostNotificationType.ConstructionProjectAvailable && notificationType != OutpostNotificationType.EmptyGardenPlot && notificationType != OutpostNotificationType.FullStables)
		{
			return notificationType == OutpostNotificationType.NewJobPosting;
		}
		return true;
	}

	public static bool IsHeroNotification(OutpostNotificationType notificationType)
	{
		return !IsOutpostNotification(notificationType);
	}

	public static bool CanDismiss(OutpostNotificationType notificationType)
	{
		if (notificationType != OutpostNotificationType.HeroLeveledUp && notificationType != OutpostNotificationType.HeroRetired && notificationType != OutpostNotificationType.NewCraftingBlueprints && notificationType != OutpostNotificationType.NewTraderItems)
		{
			return notificationType == OutpostNotificationType.NewJobPosting;
		}
		return true;
	}

	public static string GetNotificationTypeID(OutpostNotificationType notificationType)
	{
		int num = (int)notificationType;
		return num.ToString();
	}

	public static string BuildID(OutpostNotificationType notificationType, object specificID)
	{
		if (specificID != null)
		{
			return $"{GetNotificationTypeID(notificationType)}:{specificID}";
		}
		return GetNotificationTypeID(notificationType);
	}

	public static string BuildID(OutpostEventLogState.EventLogData eventLog)
	{
		if (!(eventLog is OutpostEventLogState.EventLogData.BuildingWrecked buildingWrecked))
		{
			if (!(eventLog is OutpostEventLogState.EventLogData.CharacterLeveledUp characterLeveledUp))
			{
				if (!(eventLog is OutpostEventLogState.EventLogData.CharacterRetired characterRetired))
				{
					if (!(eventLog is OutpostEventLogState.EventLogData.NewBlueprintUnlocked))
					{
						if (!(eventLog is OutpostEventLogState.EventLogData.NewItemsInTrader))
						{
							if (!(eventLog is OutpostEventLogState.EventLogData.TravellingMerchantEnabled))
							{
								if (!(eventLog is OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted characterPersonalQuestCompleted))
								{
									if (!(eventLog is OutpostEventLogState.EventLogData.CharacterPersonalQuestCancelCompleted characterPersonalQuestCancelCompleted))
									{
										if (!(eventLog is OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionTriggered))
										{
											if (eventLog is OutpostEventLogState.EventLogData.NewJobPosting)
											{
												return BuildID(OutpostNotificationType.NewJobPosting, "*");
											}
											return string.Empty;
										}
										return BuildID(OutpostNotificationType.StoryEvents, campaignSectionTriggered.SectionID.ID);
									}
									return BuildID(OutpostNotificationType.HeroReadyForRetirement, characterPersonalQuestCancelCompleted.CharacterID.ID);
								}
								return BuildID(OutpostNotificationType.HeroReadyForRetirement, characterPersonalQuestCompleted.CharacterID.ID);
							}
							return BuildID(OutpostNotificationType.TravellingMerchant, "*");
						}
						return BuildID(OutpostNotificationType.NewTraderItems, "*");
					}
					return BuildID(OutpostNotificationType.NewCraftingBlueprints, "*");
				}
				return BuildID(OutpostNotificationType.HeroRetired, characterRetired.CharacterID.ID);
			}
			return BuildID(OutpostNotificationType.HeroLeveledUp, characterLeveledUp.CharacterID.ID);
		}
		return BuildID(OutpostNotificationType.BuildingWrecked, buildingWrecked.BuildingID.ID);
	}

	public static OutpostNotificationType? GetNotificationTypeFromEventLog(OutpostEventLogState.EventLogData eventLog)
	{
		if (!(eventLog is OutpostEventLogState.EventLogData.BuildingWrecked))
		{
			if (!(eventLog is OutpostEventLogState.EventLogData.CharacterLeveledUp))
			{
				if (!(eventLog is OutpostEventLogState.EventLogData.CharacterRetired))
				{
					if (!(eventLog is OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted))
					{
						if (!(eventLog is OutpostEventLogState.EventLogData.NewBlueprintUnlocked))
						{
							if (!(eventLog is OutpostEventLogState.EventLogData.TravellingMerchantEnabled))
							{
								if (!(eventLog is OutpostEventLogState.EventLogData.NewItemsInTrader))
								{
									if (!(eventLog is OutpostEventLogState.EventLogData.NewJobPosting))
									{
										if (eventLog is OutpostEventLogState.EventLogData.CampaignSectionTriggered)
										{
											return OutpostNotificationType.StoryEvents;
										}
										return null;
									}
									return OutpostNotificationType.NewJobPosting;
								}
								return OutpostNotificationType.NewTraderItems;
							}
							return OutpostNotificationType.TravellingMerchant;
						}
						return OutpostNotificationType.NewCraftingBlueprints;
					}
					return OutpostNotificationType.HeroReadyForRetirement;
				}
				return OutpostNotificationType.HeroRetired;
			}
			return OutpostNotificationType.HeroLeveledUp;
		}
		return OutpostNotificationType.BuildingWrecked;
	}
}
