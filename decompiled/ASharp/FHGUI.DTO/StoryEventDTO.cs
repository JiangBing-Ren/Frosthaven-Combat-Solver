using FHRL;
using GLOOM;

namespace FHGUI.DTO;

public struct StoryEventDTO(CampaignSectionDef def, uint week, uint storyID, bool isNew)
{
	public CampaignSectionID ID = def.ID;

	public uint WeekTriggered = week;

	public string Name = LocalizationManager.GetTranslation($"Event_{def.ID}_Name");

	public bool IsNew = isNew;

	public uint StoryID = storyID;
}
