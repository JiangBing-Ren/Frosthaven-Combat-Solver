using System.Collections.Generic;

namespace FHGUI.DTO;

public class ImportPartyDTO
{
	public string ID;

	public string Name;

	public List<CharacterDataDTO> Members;

	public ImportPartyDTO()
	{
	}

	public ImportPartyDTO(PartyAdventureData adventureData)
	{
		ID = adventureData.PartyNameNoSpaces;
		Name = adventureData.PartyName;
	}
}
